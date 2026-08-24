using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace ProjectAllTime.VN.Settings
{
    public enum VNSettingsStorageState
    {
        Missing,
        Valid,
        Corrupted,
        Unsupported,
        IoFailure,
    }

    public sealed class VNSettingsReadResult
    {
        public VNSettingsStorageState State { get; }
        public VNSettingsData Settings { get; }
        public string Diagnostic { get; }
        public bool IsWriteProtected { get; }

        private VNSettingsReadResult(VNSettingsStorageState state, VNSettingsData settings, string diagnostic, bool isWriteProtected)
        {
            State = state;
            Settings = settings;
            Diagnostic = diagnostic;
            IsWriteProtected = isWriteProtected;
        }

        public static VNSettingsReadResult Missing() => new(VNSettingsStorageState.Missing, null, null, false);
        public static VNSettingsReadResult Valid(VNSettingsData settings) => new(VNSettingsStorageState.Valid, settings, null, false);
        public static VNSettingsReadResult Corrupted(string diagnostic) => new(VNSettingsStorageState.Corrupted, null, diagnostic, false);
        public static VNSettingsReadResult Unsupported(string diagnostic) => new(VNSettingsStorageState.Unsupported, null, diagnostic, true);
        public static VNSettingsReadResult IoFailure(string diagnostic) => new(VNSettingsStorageState.IoFailure, null, diagnostic, true);
    }

    public sealed class VNSettingsWriteResult
    {
        public bool Succeeded { get; }
        public VNSettingsStorageState State { get; }
        public string Diagnostic { get; }
        public bool IsWriteProtected { get; }

        private VNSettingsWriteResult(bool succeeded, VNSettingsStorageState state, string diagnostic, bool isWriteProtected)
        {
            Succeeded = succeeded;
            State = state;
            Diagnostic = diagnostic;
            IsWriteProtected = isWriteProtected;
        }

        public static VNSettingsWriteResult Success() => new(true, VNSettingsStorageState.Valid, null, false);
        public static VNSettingsWriteResult Failure(VNSettingsStorageState state, string diagnostic, bool isWriteProtected = false) =>
            new(false, state, diagnostic, isWriteProtected);
    }

    /// <summary>
    /// Owns the one global settings file. It never applies settings to Unity
    /// runtime systems; it only validates and durably preserves JSON data.
    /// </summary>
    public sealed class VNSettingsRepository
    {
        public const string SettingsDirectoryName = "Settings";
        public const string CanonicalFileName = "settings.json";

        private readonly string storageRoot;

        public string StorageRoot => storageRoot;
        public string CanonicalFilePath => Path.Combine(storageRoot, CanonicalFileName);
        public static string ProductionStorageRoot => Path.Combine(Application.persistentDataPath, SettingsDirectoryName);

        public VNSettingsRepository() : this(ProductionStorageRoot) { }

        private VNSettingsRepository(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory)) throw new ArgumentException("A settings storage root is required.", nameof(rootDirectory));
            storageRoot = Path.GetFullPath(rootDirectory);
        }

        /// <summary>
        /// Test-only construction path. Production code uses the parameterless
        /// constructor and therefore Application.persistentDataPath/Settings.
        /// </summary>
        public static VNSettingsRepository CreateForTesting(string isolatedRootDirectory) => new(isolatedRootDirectory);

        public VNSettingsReadResult Read()
        {
            string json;
            try
            {
                json = File.ReadAllText(CanonicalFilePath, Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                return VNSettingsReadResult.Missing();
            }
            catch (DirectoryNotFoundException)
            {
                return VNSettingsReadResult.Missing();
            }
            catch (Exception)
            {
                return VNSettingsReadResult.IoFailure("Settings file could not be read without risking its preservation.");
            }

            var parsed = TryParse(json, out var data, out var state, out var diagnostic);
            if (parsed) return VNSettingsReadResult.Valid(data);
            if (state == VNSettingsStorageState.Unsupported) return VNSettingsReadResult.Unsupported(diagnostic);

            if (TryQuarantineCanonicalFile(out var quarantineDiagnostic))
                return VNSettingsReadResult.Corrupted(diagnostic + " The original file was quarantined as corrupt.");

            return VNSettingsReadResult.IoFailure(diagnostic + " " + quarantineDiagnostic);
        }

        public VNSettingsWriteResult Write(VNSettingsData data)
        {
            if (!VNSettingsValidation.TryValidate(data, out var diagnostic))
                return VNSettingsWriteResult.Failure(VNSettingsStorageState.Corrupted, diagnostic);

            var existingState = InspectExistingCanonicalForWrite(out var existingDiagnostic);
            if (existingState == VNSettingsStorageState.Unsupported)
                return VNSettingsWriteResult.Failure(existingState, existingDiagnostic, true);
            if (existingState == VNSettingsStorageState.Corrupted || existingState == VNSettingsStorageState.IoFailure)
                return VNSettingsWriteResult.Failure(existingState, existingDiagnostic, true);

            string temporaryPath = null;
            try
            {
                Directory.CreateDirectory(storageRoot);
                var json = JsonUtility.ToJson(data, true);
                if (string.IsNullOrEmpty(json))
                    return VNSettingsWriteResult.Failure(VNSettingsStorageState.IoFailure, "Settings serialization produced no JSON.");

                temporaryPath = Path.Combine(storageRoot, CanonicalFileName + "." + Guid.NewGuid().ToString("N") + ".tmp");
                var bytes = new UTF8Encoding(false).GetBytes(json);

                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(CanonicalFilePath)) File.Replace(temporaryPath, CanonicalFilePath, null);
                else File.Move(temporaryPath, CanonicalFilePath);

                return VNSettingsWriteResult.Success();
            }
            catch (Exception)
            {
                return VNSettingsWriteResult.Failure(VNSettingsStorageState.IoFailure, "Settings file could not be written without replacing the authoritative file.");
            }
            finally
            {
                TryDeleteExactTemporaryFile(temporaryPath);
            }
        }

        private VNSettingsStorageState InspectExistingCanonicalForWrite(out string diagnostic)
        {
            diagnostic = null;
            try
            {
                if (!File.Exists(CanonicalFilePath)) return VNSettingsStorageState.Missing;

                var json = File.ReadAllText(CanonicalFilePath, Encoding.UTF8);
                if (TryParse(json, out _, out var state, out diagnostic)) return VNSettingsStorageState.Valid;
                return state;
            }
            catch (Exception)
            {
                diagnostic = "Existing settings file could not be inspected without risking its preservation.";
                return VNSettingsStorageState.IoFailure;
            }
        }

        private static bool TryParse(string json, out VNSettingsData data, out VNSettingsStorageState state, out string diagnostic)
        {
            data = null;
            state = VNSettingsStorageState.Corrupted;
            diagnostic = null;

            try
            {
                var schemaProbe = JsonUtility.FromJson<VNSettingsSchemaProbe>(json);
                if (schemaProbe == null || schemaProbe.schemaVersion <= 0)
                {
                    diagnostic = "Settings JSON is missing a positive schema version.";
                    return false;
                }

                if (schemaProbe.schemaVersion > VNSettingsDefaults.CurrentSchemaVersion)
                {
                    state = VNSettingsStorageState.Unsupported;
                    diagnostic = "Settings JSON uses a future schema version and was preserved unchanged.";
                    return false;
                }

                data = JsonUtility.FromJson<VNSettingsData>(json);
                if (!VNSettingsValidation.TryValidate(data, out diagnostic))
                {
                    data = null;
                    return false;
                }

                state = VNSettingsStorageState.Valid;
                return true;
            }
            catch (Exception)
            {
                data = null;
                state = VNSettingsStorageState.Corrupted;
                diagnostic = "Settings JSON could not be parsed.";
                return false;
            }
        }

        private bool TryQuarantineCanonicalFile(out string diagnostic)
        {
            diagnostic = null;
            try
            {
                var quarantinePath = CreateUniqueQuarantinePath();
                File.Move(CanonicalFilePath, quarantinePath);
                return true;
            }
            catch (Exception)
            {
                diagnostic = "The corrupt settings file could not be quarantined, so writes are blocked.";
                return false;
            }
        }

        private string CreateUniqueQuarantinePath()
        {
            string quarantinePath;
            do
            {
                quarantinePath = Path.Combine(storageRoot, CanonicalFileName + "." + Guid.NewGuid().ToString("N") + ".corrupt");
            }
            while (File.Exists(quarantinePath) || Directory.Exists(quarantinePath));

            return quarantinePath;
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
                // A failed temporary cleanup must not cause canonical-file deletion.
            }
        }

        [Serializable]
        private sealed class VNSettingsSchemaProbe
        {
            public int schemaVersion;
        }
    }
}
