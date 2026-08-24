using System;
using System.Collections.Generic;
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

        private static readonly string[] RequiredSchemaV1Fields =
        {
            "schemaVersion",
            "displayMode",
            "windowedWidth",
            "windowedHeight",
            "textSpeedLps",
            "autoSpeedNormalized",
            "masterVolumeNormalized",
            "bgmVolumeNormalized",
            "sfxVolumeNormalized",
            "voiceVolumeNormalized",
            "skipUnread",
            "screenShakeEnabled",
            "inputBindingOverridesJson",
        };

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

                if (!TryGetTopLevelPropertyNames(json, out var propertyNames))
                {
                    diagnostic = "Settings JSON root object could not be parsed.";
                    return false;
                }

                foreach (var requiredField in RequiredSchemaV1Fields)
                {
                    if (propertyNames.Contains(requiredField)) continue;

                    diagnostic = "Settings JSON is missing required schema-version-1 field '" + requiredField + "'.";
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

        /// <summary>
        /// Reads only names declared by the root JSON object. JsonUtility supplies
        /// default values for missing fields, so schema-v1 completeness must be
        /// established from raw structure before the DTO is accepted.
        /// </summary>
        private static bool TryGetTopLevelPropertyNames(string json, out HashSet<string> propertyNames)
        {
            propertyNames = new HashSet<string>(StringComparer.Ordinal);
            if (json == null) return false;

            var index = 0;
            SkipWhitespace(json, ref index);
            if (!TryConsume(json, ref index, '{')) return false;

            SkipWhitespace(json, ref index);
            if (TryConsume(json, ref index, '}'))
            {
                SkipWhitespace(json, ref index);
                return index == json.Length;
            }

            while (true)
            {
                if (!TryReadJsonString(json, ref index, out var propertyName)) return false;
                propertyNames.Add(propertyName);

                SkipWhitespace(json, ref index);
                if (!TryConsume(json, ref index, ':')) return false;
                if (!TrySkipJsonValue(json, ref index, 0)) return false;

                SkipWhitespace(json, ref index);
                if (TryConsume(json, ref index, '}'))
                {
                    SkipWhitespace(json, ref index);
                    return index == json.Length;
                }

                if (!TryConsume(json, ref index, ',')) return false;
                SkipWhitespace(json, ref index);
            }
        }

        private static bool TrySkipJsonValue(string json, ref int index, int depth)
        {
            if (depth > 64) return false;
            SkipWhitespace(json, ref index);
            if (index >= json.Length) return false;

            switch (json[index])
            {
                case '{':
                    return TrySkipJsonObject(json, ref index, depth + 1);
                case '[':
                    return TrySkipJsonArray(json, ref index, depth + 1);
                case '"':
                    return TryReadJsonString(json, ref index, out _);
                case 't':
                    return TryConsumeLiteral(json, ref index, "true");
                case 'f':
                    return TryConsumeLiteral(json, ref index, "false");
                case 'n':
                    return TryConsumeLiteral(json, ref index, "null");
                default:
                    return TrySkipJsonNumber(json, ref index);
            }
        }

        private static bool TrySkipJsonObject(string json, ref int index, int depth)
        {
            if (!TryConsume(json, ref index, '{')) return false;
            SkipWhitespace(json, ref index);
            if (TryConsume(json, ref index, '}')) return true;

            while (true)
            {
                if (!TryReadJsonString(json, ref index, out _)) return false;
                SkipWhitespace(json, ref index);
                if (!TryConsume(json, ref index, ':')) return false;
                if (!TrySkipJsonValue(json, ref index, depth)) return false;

                SkipWhitespace(json, ref index);
                if (TryConsume(json, ref index, '}')) return true;
                if (!TryConsume(json, ref index, ',')) return false;
                SkipWhitespace(json, ref index);
            }
        }

        private static bool TrySkipJsonArray(string json, ref int index, int depth)
        {
            if (!TryConsume(json, ref index, '[')) return false;
            SkipWhitespace(json, ref index);
            if (TryConsume(json, ref index, ']')) return true;

            while (true)
            {
                if (!TrySkipJsonValue(json, ref index, depth)) return false;
                SkipWhitespace(json, ref index);
                if (TryConsume(json, ref index, ']')) return true;
                if (!TryConsume(json, ref index, ',')) return false;
                SkipWhitespace(json, ref index);
            }
        }

        private static bool TryReadJsonString(string json, ref int index, out string value)
        {
            value = null;
            if (!TryConsume(json, ref index, '"')) return false;

            var builder = new StringBuilder();
            while (index < json.Length)
            {
                var character = json[index++];
                if (character == '"')
                {
                    value = builder.ToString();
                    return true;
                }

                if (character < 0x20) return false;
                if (character != '\\')
                {
                    builder.Append(character);
                    continue;
                }

                if (index >= json.Length) return false;
                var escaped = json[index++];
                switch (escaped)
                {
                    case '"': builder.Append('"'); break;
                    case '\\': builder.Append('\\'); break;
                    case '/': builder.Append('/'); break;
                    case 'b': builder.Append('\b'); break;
                    case 'f': builder.Append('\f'); break;
                    case 'n': builder.Append('\n'); break;
                    case 'r': builder.Append('\r'); break;
                    case 't': builder.Append('\t'); break;
                    case 'u':
                        if (!TryReadUnicodeEscape(json, ref index, out var unicodeCharacter)) return false;
                        builder.Append(unicodeCharacter);
                        break;
                    default:
                        return false;
                }
            }

            return false;
        }

        private static bool TryReadUnicodeEscape(string json, ref int index, out char character)
        {
            character = default;
            if (index + 4 > json.Length) return false;

            var value = 0;
            for (var offset = 0; offset < 4; offset++)
            {
                var digit = HexValue(json[index++]);
                if (digit < 0) return false;
                value = (value << 4) | digit;
            }

            character = (char)value;
            return true;
        }

        private static bool TrySkipJsonNumber(string json, ref int index)
        {
            var start = index;
            if (TryConsume(json, ref index, '-')) { }

            if (index >= json.Length) return false;
            if (json[index] == '0') index++;
            else if (json[index] >= '1' && json[index] <= '9')
            {
                index++;
                while (index < json.Length && json[index] >= '0' && json[index] <= '9') index++;
            }
            else return false;

            if (TryConsume(json, ref index, '.'))
            {
                var fractionStart = index;
                while (index < json.Length && json[index] >= '0' && json[index] <= '9') index++;
                if (index == fractionStart) return false;
            }

            if (index < json.Length && (json[index] == 'e' || json[index] == 'E'))
            {
                index++;
                if (index < json.Length && (json[index] == '+' || json[index] == '-')) index++;
                var exponentStart = index;
                while (index < json.Length && json[index] >= '0' && json[index] <= '9') index++;
                if (index == exponentStart) return false;
            }

            return index > start;
        }

        private static bool TryConsumeLiteral(string json, ref int index, string literal)
        {
            if (index + literal.Length > json.Length) return false;
            for (var offset = 0; offset < literal.Length; offset++)
            {
                if (json[index + offset] != literal[offset]) return false;
            }

            index += literal.Length;
            return true;
        }

        private static bool TryConsume(string json, ref int index, char expected)
        {
            if (index >= json.Length || json[index] != expected) return false;
            index++;
            return true;
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length)
            {
                var character = json[index];
                if (character != ' ' && character != '\t' && character != '\r' && character != '\n') return;
                index++;
            }
        }

        private static int HexValue(char character)
        {
            if (character >= '0' && character <= '9') return character - '0';
            if (character >= 'a' && character <= 'f') return character - 'a' + 10;
            if (character >= 'A' && character <= 'F') return character - 'A' + 10;
            return -1;
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
