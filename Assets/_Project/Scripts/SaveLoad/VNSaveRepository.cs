using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ProjectAllTime.VN.SaveLoad
{
    public enum VNSaveSlotState
    {
        Empty,
        Valid,
        Corrupted,
        Unsupported,
        InvalidRequest,
    }

    public enum VNAutoSlotAllocationStatus
    {
        Allocated,
        NoSafeCandidate,
    }

    public sealed class VNSaveReadResult
    {
        public VNSaveSlotState State { get; }
        public SaveSlotData SaveData { get; }
        public string Diagnostic { get; }

        private VNSaveReadResult(VNSaveSlotState state, SaveSlotData saveData, string diagnostic)
        {
            State = state;
            SaveData = saveData;
            Diagnostic = diagnostic;
        }

        public static VNSaveReadResult Empty() => new(VNSaveSlotState.Empty, null, null);
        public static VNSaveReadResult Valid(SaveSlotData saveData) => new(VNSaveSlotState.Valid, saveData, null);
        public static VNSaveReadResult Corrupted(string diagnostic) => new(VNSaveSlotState.Corrupted, null, diagnostic);
        public static VNSaveReadResult Unsupported(string diagnostic) => new(VNSaveSlotState.Unsupported, null, diagnostic);
        public static VNSaveReadResult InvalidRequest(string diagnostic) => new(VNSaveSlotState.InvalidRequest, null, diagnostic);
    }

    public sealed class VNSaveOperationResult
    {
        public bool Succeeded { get; }
        public string Diagnostic { get; }

        private VNSaveOperationResult(bool succeeded, string diagnostic)
        {
            Succeeded = succeeded;
            Diagnostic = diagnostic;
        }

        public static VNSaveOperationResult Success() => new(true, null);
        public static VNSaveOperationResult Failure(string diagnostic) => new(false, diagnostic);
    }

    public sealed class VNSaveSlotInspection
    {
        public VNSaveSlotKey SlotKey { get; }
        public VNSaveReadResult ReadResult { get; }

        public VNSaveSlotInspection(VNSaveSlotKey slotKey, VNSaveReadResult readResult)
        {
            SlotKey = slotKey;
            ReadResult = readResult;
        }
    }

    public sealed class VNAutoSlotAllocationResult
    {
        public VNAutoSlotAllocationStatus Status { get; }
        public VNSaveSlotKey? SlotKey { get; }
        public string Diagnostic { get; }

        private VNAutoSlotAllocationResult(VNAutoSlotAllocationStatus status, VNSaveSlotKey? slotKey, string diagnostic)
        {
            Status = status;
            SlotKey = slotKey;
            Diagnostic = diagnostic;
        }

        public static VNAutoSlotAllocationResult Allocated(VNSaveSlotKey slotKey) => new(VNAutoSlotAllocationStatus.Allocated, slotKey, null);
        public static VNAutoSlotAllocationResult NoSafeCandidate(string diagnostic) => new(VNAutoSlotAllocationStatus.NoSafeCandidate, null, diagnostic);
    }

    /// <summary>
    /// Owns canonical slot paths and durable JSON replacement. It has no
    /// knowledge of Yarn, scene objects, UI, audio, or presentation catalogs.
    /// </summary>
    public sealed class VNSaveRepository
    {
        public const string SaveDirectoryName = "SaveData";

        private readonly string storageRoot;

        public string StorageRoot => storageRoot;
        public static string ProductionStorageRoot => Path.Combine(Application.persistentDataPath, SaveDirectoryName);

        public VNSaveRepository() : this(ProductionStorageRoot) { }

        private VNSaveRepository(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory)) throw new ArgumentException("A storage root is required.", nameof(rootDirectory));
            storageRoot = Path.GetFullPath(rootDirectory);
        }

        /// <summary>
        /// Test-only construction path. Production code uses the parameterless
        /// constructor and therefore Application.persistentDataPath/SaveData.
        /// </summary>
        public static VNSaveRepository CreateForTesting(string isolatedRootDirectory) => new(isolatedRootDirectory);

        public bool TryGetSlotPath(VNSaveSlotKey slotKey, out string path)
        {
            path = null;
            if (!slotKey.TryGetCanonicalFileName(out var fileName)) return false;
            path = Path.Combine(storageRoot, fileName);
            return true;
        }

        public bool TryGetThumbnailSidecarPath(VNSaveSlotKey slotKey, string thumbnailFileName, out string path)
        {
            path = null;
            if (!slotKey.IsValid || string.IsNullOrEmpty(thumbnailFileName) || !VNSaveSerializer.IsSafeThumbnailFileName(thumbnailFileName)) return false;
            path = Path.Combine(storageRoot, thumbnailFileName);
            return true;
        }

        public VNSaveOperationResult Write(VNSaveSlotKey slotKey, SaveSlotData saveData)
        {
            if (!slotKey.IsValid) return VNSaveOperationResult.Failure("The requested save slot key is invalid.");
            if (!VNSaveSerializer.TrySerialize(saveData, slotKey, out var json, out var diagnostic)) return VNSaveOperationResult.Failure(diagnostic);
            if (!TryGetSlotPath(slotKey, out var destinationPath)) return VNSaveOperationResult.Failure("The requested save slot key is invalid.");
            return TryWriteDefensively(destinationPath, json);
        }

        public VNSaveReadResult Read(VNSaveSlotKey slotKey)
        {
            if (!slotKey.IsValid) return VNSaveReadResult.InvalidRequest("The requested save slot key is invalid.");
            if (!TryGetSlotPath(slotKey, out var path)) return VNSaveReadResult.InvalidRequest("The requested save slot key is invalid.");

            try
            {
                return VNSaveSerializer.Deserialize(File.ReadAllText(path, Encoding.UTF8), slotKey);
            }
            catch (FileNotFoundException)
            {
                return VNSaveReadResult.Empty();
            }
            catch (DirectoryNotFoundException)
            {
                return VNSaveReadResult.Empty();
            }
            catch (Exception)
            {
                return VNSaveReadResult.Corrupted("Save file could not be read.");
            }
        }

        public VNSaveOperationResult Delete(VNSaveSlotKey slotKey)
        {
            if (!slotKey.IsValid) return VNSaveOperationResult.Failure("The requested save slot key is invalid.");
            if (!TryGetSlotPath(slotKey, out var path)) return VNSaveOperationResult.Failure("The requested save slot key is invalid.");

            try
            {
                if (File.Exists(path)) File.Delete(path);
                DeleteOwnedTemporaryFiles(slotKey);
                return VNSaveOperationResult.Success();
            }
            catch (Exception)
            {
                return VNSaveOperationResult.Failure("Save slot could not be deleted.");
            }
        }

        public IReadOnlyList<VNSaveSlotInspection> InspectAllSlots()
        {
            var inspections = new List<VNSaveSlotInspection>(VNSaveSlotKey.ManualSlotCount + VNSaveSlotKey.AutoSlotCount + VNSaveSlotKey.QuickSlotCount);
            AddInspections(inspections, VNSaveSlotType.Manual, VNSaveSlotKey.ManualSlotCount);
            AddInspections(inspections, VNSaveSlotType.Auto, VNSaveSlotKey.AutoSlotCount);
            AddInspections(inspections, VNSaveSlotType.Quick, VNSaveSlotKey.QuickSlotCount);
            return inspections;
        }

        public VNAutoSlotAllocationResult AllocateNextAutoSlot()
        {
            VNSaveSlotKey? oldestValidKey = null;
            DateTimeOffset oldestValidTimestamp = default;

            for (var index = 0; index < VNSaveSlotKey.AutoSlotCount; index++)
            {
                var slotKey = new VNSaveSlotKey(VNSaveSlotType.Auto, index);
                var result = Read(slotKey);
                if (result.State == VNSaveSlotState.Empty) return VNAutoSlotAllocationResult.Allocated(slotKey);

                if (result.State != VNSaveSlotState.Valid) continue;
                if (!VNSaveSerializer.TryParseUtcTimestamp(result.SaveData.savedAtUtcIso8601, out var timestamp))
                    continue;

                if (!oldestValidKey.HasValue || timestamp < oldestValidTimestamp || (timestamp == oldestValidTimestamp && index < oldestValidKey.Value.SlotIndex))
                {
                    oldestValidKey = slotKey;
                    oldestValidTimestamp = timestamp;
                }
            }

            return oldestValidKey.HasValue
                ? VNAutoSlotAllocationResult.Allocated(oldestValidKey.Value)
                : VNAutoSlotAllocationResult.NoSafeCandidate("No empty or valid auto-save slot can be safely selected. Corrupted and unsupported slots are preserved.");
        }

        private VNSaveOperationResult TryWriteDefensively(string destinationPath, string json)
        {
            string temporaryPath = null;
            try
            {
                Directory.CreateDirectory(storageRoot);
                temporaryPath = Path.Combine(storageRoot, Path.GetFileName(destinationPath) + "." + Guid.NewGuid().ToString("N") + ".tmp");
                var bytes = new UTF8Encoding(false).GetBytes(json);

                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(destinationPath)) File.Replace(temporaryPath, destinationPath, null);
                else File.Move(temporaryPath, destinationPath);

                return VNSaveOperationResult.Success();
            }
            catch (Exception)
            {
                TryDeleteExactTemporaryFile(temporaryPath);
                return VNSaveOperationResult.Failure("Save file could not be written without replacing the authoritative slot.");
            }
        }

        private void AddInspections(List<VNSaveSlotInspection> inspections, VNSaveSlotType slotType, int count)
        {
            for (var index = 0; index < count; index++)
            {
                var key = new VNSaveSlotKey(slotType, index);
                inspections.Add(new VNSaveSlotInspection(key, Read(key)));
            }
        }

        private void DeleteOwnedTemporaryFiles(VNSaveSlotKey slotKey)
        {
            if (!slotKey.TryGetCanonicalFileName(out var fileName) || !Directory.Exists(storageRoot)) return;

            foreach (var candidatePath in Directory.EnumerateFiles(storageRoot, fileName + ".*.tmp", SearchOption.TopDirectoryOnly))
            {
                var candidateName = Path.GetFileName(candidatePath);
                var prefix = fileName + ".";
                if (!candidateName.StartsWith(prefix, StringComparison.Ordinal) || !candidateName.EndsWith(".tmp", StringComparison.Ordinal)) continue;

                var guidText = candidateName.Substring(prefix.Length, candidateName.Length - prefix.Length - ".tmp".Length);
                if (Guid.TryParseExact(guidText, "N", out _)) File.Delete(candidatePath);
            }
        }

        private static void TryDeleteExactTemporaryFile(string temporaryPath)
        {
            if (string.IsNullOrEmpty(temporaryPath)) return;
            try
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
            catch (Exception)
            {
                // A failed cleanup must not cause an authoritative save deletion.
            }
        }
    }
}
