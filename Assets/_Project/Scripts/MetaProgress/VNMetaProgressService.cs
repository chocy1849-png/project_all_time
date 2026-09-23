using System;
using System.Collections.Generic;

namespace ProjectAllTime.VN.MetaProgress
{
    /// <summary>
    /// Scene-independent authoritative session owner. Mutations are persisted
    /// before their candidate state becomes the in-memory authority.
    /// </summary>
    public sealed class VNMetaProgressService
    {
        private readonly VNMetaProgressRepository repository;
        private VNMetaProgressData current;
        private VNMetaProgressStorageState storageState;
        private bool isWriteProtected;
        private string lastDiagnostic;

        public VNMetaProgressService(VNMetaProgressRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            current = VNMetaProgressDefaults.CreateDefault();
            storageState = VNMetaProgressStorageState.Missing;
        }

        public VNMetaProgressData Current => current.Copy();
        public VNMetaProgressStorageState StorageState => storageState;
        public bool IsWriteProtected => isWriteProtected;
        public bool CanWrite => !isWriteProtected;
        public string LastDiagnostic => lastDiagnostic;

        public VNMetaProgressData Load()
        {
            var result = repository.Read();
            storageState = result.State;
            isWriteProtected = result.IsWriteProtected;
            lastDiagnostic = result.Diagnostic;
            current = result.State == VNMetaProgressStorageState.Valid
                ? result.Data.Copy()
                : VNMetaProgressDefaults.CreateDefault();
            return Current;
        }

        public bool TryRecordReadLine(string id) => TryAdd(id, Collection.ReadLineIds);
        public bool TryUnlockCG(string id) => TryAdd(id, Collection.UnlockedCGs);
        public bool TryUnlockChapter(string id) => TryAdd(id, Collection.UnlockedChapters);
        public bool TryUnlockArchiveEntry(string id) => TryAdd(id, Collection.UnlockedArchiveEntries);
        public bool TryUnlockAchievement(string id) => TryAdd(id, Collection.UnlockedAchievements);
        public bool TryCompleteEnding(string id) => TryAdd(id, Collection.CompletedEndings);

        public bool IsLineRead(string id) => ContainsCurrent(Collection.ReadLineIds, id);
        public bool IsCGUnlocked(string id) => ContainsCurrent(Collection.UnlockedCGs, id);
        public bool IsChapterUnlocked(string id) => ContainsCurrent(Collection.UnlockedChapters, id);
        public bool IsArchiveEntryUnlocked(string id) => ContainsCurrent(Collection.UnlockedArchiveEntries, id);
        public bool IsAchievementUnlocked(string id) => ContainsCurrent(Collection.UnlockedAchievements, id);
        public bool IsEndingCompleted(string id) => ContainsCurrent(Collection.CompletedEndings, id);

        private bool TryAdd(string id, Collection collection)
        {
            if (isWriteProtected)
            {
                lastDiagnostic = "MetaProgress writes are blocked to preserve an unsupported or unquarantined file.";
                return false;
            }

            if (!VNMetaProgressValidation.IsValidId(id))
            {
                lastDiagnostic = "MetaProgress IDs must not be null, empty, or whitespace-only.";
                return false;
            }

            var existing = GetCollection(current, collection);
            if (ContainsOrdinal(existing, id))
            {
                lastDiagnostic = null;
                return true;
            }

            var candidate = current.Copy();
            SetCollection(candidate, collection, Add(existing, id));
            if (!VNMetaProgressValidation.TryCreateCanonical(candidate, out var canonical, out var validationDiagnostic))
            {
                lastDiagnostic = validationDiagnostic;
                return false;
            }

            var write = repository.Write(canonical);
            storageState = write.State;
            lastDiagnostic = write.Diagnostic;
            if (!write.Succeeded)
            {
                isWriteProtected = write.IsWriteProtected;
                return false;
            }

            current = canonical;
            isWriteProtected = false;
            return true;
        }

        private static bool ContainsOrdinal(string[] values, string value)
        {
            foreach (var candidate in values)
            {
                if (string.Equals(candidate, value, StringComparison.Ordinal)) return true;
            }

            return false;
        }

        private bool ContainsCurrent(Collection collection, string id)
        {
            return VNMetaProgressValidation.IsValidId(id) && ContainsOrdinal(GetCollection(current, collection), id);
        }

        private static string[] Add(string[] values, string value)
        {
            var copy = new List<string>(values) { value };
            return copy.ToArray();
        }

        private static string[] GetCollection(VNMetaProgressData data, Collection collection)
        {
            switch (collection)
            {
                case Collection.ReadLineIds: return data.readLineIds;
                case Collection.UnlockedCGs: return data.unlockedCGs;
                case Collection.UnlockedChapters: return data.unlockedChapters;
                case Collection.UnlockedArchiveEntries: return data.unlockedArchiveEntries;
                case Collection.UnlockedAchievements: return data.unlockedAchievements;
                case Collection.CompletedEndings: return data.completedEndings;
                default: throw new ArgumentOutOfRangeException(nameof(collection));
            }
        }

        private static void SetCollection(VNMetaProgressData data, Collection collection, string[] values)
        {
            switch (collection)
            {
                case Collection.ReadLineIds: data.readLineIds = values; break;
                case Collection.UnlockedCGs: data.unlockedCGs = values; break;
                case Collection.UnlockedChapters: data.unlockedChapters = values; break;
                case Collection.UnlockedArchiveEntries: data.unlockedArchiveEntries = values; break;
                case Collection.UnlockedAchievements: data.unlockedAchievements = values; break;
                case Collection.CompletedEndings: data.completedEndings = values; break;
                default: throw new ArgumentOutOfRangeException(nameof(collection));
            }
        }

        private enum Collection
        {
            ReadLineIds,
            UnlockedCGs,
            UnlockedChapters,
            UnlockedArchiveEntries,
            UnlockedAchievements,
            CompletedEndings,
        }
    }
}
