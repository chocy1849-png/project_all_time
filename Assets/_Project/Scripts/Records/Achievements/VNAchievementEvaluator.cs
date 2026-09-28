using System;
using System.Collections.Generic;
using System.Linq;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;

namespace ProjectAllTime.VN.Records.Achievements
{
    /// <summary>Evaluates authored Achievement conditions and persists unlocks through MetaProgress.</summary>
    public sealed class VNAchievementEvaluator : IDisposable
    {
        private readonly VNAchievementCatalog achievementCatalog;
        private readonly VNCGGalleryCatalog galleryCatalog;
        private readonly VNArchiveCatalog archiveCatalog;
        private readonly VNMetaProgressService metaProgress;
        private bool evaluationInProgress;
        private bool isDisposed;

#if UNITY_EDITOR
        // Private Editor-only seam used to verify that irrelevant ReadLine events do not evaluate conditions.
        private int evaluationEntryCountForTests;
#endif

        public bool IsInitialized { get; private set; }
        public string LastDiagnostic { get; private set; }

        public VNAchievementEvaluator(
            VNAchievementCatalog achievementCatalog,
            VNCGGalleryCatalog galleryCatalog,
            VNArchiveCatalog archiveCatalog,
            VNMetaProgressService metaProgress)
        {
            this.achievementCatalog = achievementCatalog ?? throw new ArgumentNullException(nameof(achievementCatalog));
            this.galleryCatalog = galleryCatalog ?? throw new ArgumentNullException(nameof(galleryCatalog));
            this.archiveCatalog = archiveCatalog ?? throw new ArgumentNullException(nameof(archiveCatalog));
            this.metaProgress = metaProgress ?? throw new ArgumentNullException(nameof(metaProgress));
        }

        /// <summary>Subscribes once and catches derived Achievements up to current durable progress.</summary>
        public bool Initialize()
        {
            if (isDisposed)
            {
                LastDiagnostic = "Achievement evaluation cannot be initialized after disposal.";
                return false;
            }

            if (IsInitialized) return string.IsNullOrEmpty(LastDiagnostic);

            metaProgress.ProgressChanged += OnProgressChanged;
            IsInitialized = true;
            return TryEvaluateAll();
        }

        /// <summary>Evaluates all derived conditions to a bounded fixed point.</summary>
        public bool TryEvaluateAll()
        {
            if (isDisposed)
            {
                LastDiagnostic = "Achievement evaluation cannot run after disposal.";
                return false;
            }

            if (evaluationInProgress) return true;
            evaluationInProgress = true;
#if UNITY_EDITOR
            evaluationEntryCountForTests++;
#endif
            try
            {
                LastDiagnostic = null;
                var orderedAchievements = (achievementCatalog.Achievements ?? Array.Empty<VNAchievementDefinition>())
                    .Where(achievement => achievement != null)
                    .OrderBy(achievement => achievement.SortOrder)
                    .ThenBy(achievement => achievement.AchievementId, StringComparer.Ordinal)
                    .ToArray();

                // At most one new durable unlock per authored achievement is possible. The extra scan
                // allows the final productive pass to be followed by a no-change fixed-point check.
                for (var scan = 0; scan <= orderedAchievements.Length; scan++)
                {
                    var unlockedThisPass = false;
                    foreach (var achievement in orderedAchievements)
                    {
                        if (metaProgress.IsAchievementUnlocked(achievement.AchievementId)) continue;
                        if (achievement.ConditionType == VNAchievementConditionType.Manual) continue;

                        if (!TryEvaluateCondition(achievement, out var isSatisfied, out var conditionDiagnostic))
                        {
                            LastDiagnostic = conditionDiagnostic;
                            return false;
                        }

                        if (!isSatisfied) continue;
                        if (!metaProgress.TryUnlockAchievement(achievement.AchievementId))
                        {
                            var cause = metaProgress.LastDiagnostic;
                            LastDiagnostic = string.IsNullOrWhiteSpace(cause)
                                ? "MetaProgress rejected derived Achievement unlock '" + achievement.AchievementId + "'."
                                : "Could not persist derived Achievement '" + achievement.AchievementId + "': " + cause;
                            return false;
                        }

                        unlockedThisPass = true;
                    }

                    if (!unlockedThisPass) return true;
                }

                LastDiagnostic = "Achievement evaluation did not reach a fixed point within the authored Achievement bound.";
                return false;
            }
            catch (Exception exception)
            {
                LastDiagnostic = "Achievement evaluation failed: " + exception.Message;
                return false;
            }
            finally
            {
                evaluationInProgress = false;
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            if (IsInitialized)
            {
                metaProgress.ProgressChanged -= OnProgressChanged;
                IsInitialized = false;
            }

            isDisposed = true;
        }

        private void OnProgressChanged(VNMetaProgressChange change)
        {
            if (!IsInitialized || isDisposed || change == null || change.Kind == VNMetaProgressChangeKind.ReadLine)
                return;

            // The current fixed-point scan accounts for evaluator-authored Achievement changes.
            if (evaluationInProgress) return;
            TryEvaluateAll();
        }

        private bool TryEvaluateCondition(
            VNAchievementDefinition achievement,
            out bool isSatisfied,
            out string diagnostic)
        {
            isSatisfied = false;
            diagnostic = null;
            switch (achievement.ConditionType)
            {
                case VNAchievementConditionType.Manual:
                    return true;

                case VNAchievementConditionType.EndingCompleted:
                    isSatisfied = metaProgress.IsEndingCompleted(achievement.ConditionTargetId);
                    return true;

                case VNAchievementConditionType.ChapterUnlocked:
                    isSatisfied = metaProgress.IsChapterUnlocked(achievement.ConditionTargetId);
                    return true;

                case VNAchievementConditionType.CGCountAtLeast:
                    if (achievement.Threshold < 1)
                    {
                        diagnostic = "Achievement '" + achievement.AchievementId + "' has an invalid CGCountAtLeast threshold.";
                        return false;
                    }

                    isSatisfied = CountUnlockedCGs() >= achievement.Threshold;
                    return true;

                case VNAchievementConditionType.ArchiveCountAtLeast:
                    if (achievement.Threshold < 1)
                    {
                        diagnostic = "Achievement '" + achievement.AchievementId + "' has an invalid ArchiveCountAtLeast threshold.";
                        return false;
                    }

                    isSatisfied = CountUnlockedArchiveEntries() >= achievement.Threshold;
                    return true;

                case VNAchievementConditionType.AllCGsUnlocked:
                    isSatisfied = AreAllAuthoredCGsUnlocked();
                    return true;

                case VNAchievementConditionType.AllArchiveEntriesUnlocked:
                    isSatisfied = AreAllAuthoredArchiveEntriesUnlocked();
                    return true;

                case VNAchievementConditionType.AllOfAchievements:
                    isSatisfied = AreExplicitPrerequisitesUnlocked(achievement.PrerequisiteAchievementIds);
                    return true;

                default:
                    diagnostic = "Achievement '" + achievement.AchievementId + "' has an unsupported condition type '" +
                                 achievement.ConditionType + "'.";
                    return false;
            }
        }

        private int CountUnlockedCGs()
        {
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            var count = 0;
            foreach (var entry in galleryCatalog.Entries ?? Array.Empty<VNCGGalleryEntryDefinition>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.CgId) || !seenIds.Add(entry.CgId)) continue;
                if (metaProgress.IsCGUnlocked(entry.CgId)) count++;
            }

            return count;
        }

        private int CountUnlockedArchiveEntries()
        {
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            var count = 0;
            foreach (var entry in archiveCatalog.Entries ?? Array.Empty<VNArchiveEntryDefinition>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ArchiveId) || !seenIds.Add(entry.ArchiveId)) continue;
                if (metaProgress.IsArchiveEntryUnlocked(entry.ArchiveId)) count++;
            }

            return count;
        }

        private bool AreAllAuthoredCGsUnlocked()
        {
            var authoredCount = 0;
            foreach (var entry in galleryCatalog.Entries ?? Array.Empty<VNCGGalleryEntryDefinition>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.CgId)) return false;
                authoredCount++;
                if (!metaProgress.IsCGUnlocked(entry.CgId)) return false;
            }

            return authoredCount > 0;
        }

        private bool AreAllAuthoredArchiveEntriesUnlocked()
        {
            var authoredCount = 0;
            foreach (var entry in archiveCatalog.Entries ?? Array.Empty<VNArchiveEntryDefinition>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ArchiveId)) return false;
                authoredCount++;
                if (!metaProgress.IsArchiveEntryUnlocked(entry.ArchiveId)) return false;
            }

            return authoredCount > 0;
        }

        private bool AreExplicitPrerequisitesUnlocked(IReadOnlyList<string> prerequisiteIds)
        {
            if (prerequisiteIds == null || prerequisiteIds.Count == 0) return false;
            foreach (var prerequisiteId in prerequisiteIds)
            {
                if (!metaProgress.IsAchievementUnlocked(prerequisiteId)) return false;
            }

            return true;
        }
    }
}
