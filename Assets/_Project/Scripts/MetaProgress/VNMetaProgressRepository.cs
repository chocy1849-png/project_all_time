using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ProjectAllTime.VN.MetaProgress
{
    public enum VNMetaProgressStorageState
    {
        Missing,
        Valid,
        Corrupted,
        Unsupported,
        IoFailure,
    }

    public sealed class VNMetaProgressReadResult
    {
        public VNMetaProgressStorageState State { get; }
        public VNMetaProgressData Data { get; }
        public string Diagnostic { get; }
        public bool IsWriteProtected { get; }

        private VNMetaProgressReadResult(VNMetaProgressStorageState state, VNMetaProgressData data, string diagnostic, bool isWriteProtected)
        {
            State = state;
            Data = data;
            Diagnostic = diagnostic;
            IsWriteProtected = isWriteProtected;
        }

        public static VNMetaProgressReadResult Missing() => new VNMetaProgressReadResult(VNMetaProgressStorageState.Missing, null, null, false);
        public static VNMetaProgressReadResult Valid(VNMetaProgressData data) => new VNMetaProgressReadResult(VNMetaProgressStorageState.Valid, data.Copy(), null, false);
        public static VNMetaProgressReadResult Corrupted(string diagnostic) => new VNMetaProgressReadResult(VNMetaProgressStorageState.Corrupted, null, diagnostic, false);
        public static VNMetaProgressReadResult Unsupported(string diagnostic) => new VNMetaProgressReadResult(VNMetaProgressStorageState.Unsupported, null, diagnostic, true);
        public static VNMetaProgressReadResult IoFailure(string diagnostic) => new VNMetaProgressReadResult(VNMetaProgressStorageState.IoFailure, null, diagnostic, true);
    }

    public sealed class VNMetaProgressWriteResult
    {
        public bool Succeeded { get; }
        public VNMetaProgressStorageState State { get; }
        public string Diagnostic { get; }
        public bool IsWriteProtected { get; }

        private VNMetaProgressWriteResult(bool succeeded, VNMetaProgressStorageState state, string diagnostic, bool isWriteProtected)
        {
            Succeeded = succeeded;
            State = state;
            Diagnostic = diagnostic;
            IsWriteProtected = isWriteProtected;
        }

        public static VNMetaProgressWriteResult Success() => new VNMetaProgressWriteResult(true, VNMetaProgressStorageState.Valid, null, false);
        public static VNMetaProgressWriteResult Failure(VNMetaProgressStorageState state, string diagnostic, bool isWriteProtected = false) =>
            new VNMetaProgressWriteResult(false, state, diagnostic, isWriteProtected);
    }

    /// <summary>
    /// Owns schema-v1 MetaProgress disk persistence only. It does not connect
    /// dialogue, SaveData, Settings, or scene runtime behavior.
    /// </summary>
    public sealed class VNMetaProgressRepository
    {
        public const string MetaProgressDirectoryName = "MetaProgress";
        public const string CanonicalFileName = "meta_progress.json";

        private static readonly string[] RequiredSchemaV1Fields =
        {
            "schemaVersion",
            "readLineIds",
            "unlockedCGs",
            "unlockedChapters",
            "unlockedArchiveEntries",
            "unlockedAchievements",
            "completedEndings",
        };

        private readonly string storageRoot;
        private readonly Action successfulWriteObserver;

        public string StorageRoot => storageRoot;
        public string CanonicalFilePath => Path.Combine(storageRoot, CanonicalFileName);
        public static string ProductionStorageRoot => Path.Combine(Application.persistentDataPath, MetaProgressDirectoryName);

        public VNMetaProgressRepository() : this(ProductionStorageRoot, null) { }

        private VNMetaProgressRepository(string rootDirectory, Action successfulWriteObserver)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory)) throw new ArgumentException("A MetaProgress storage root is required.", nameof(rootDirectory));
            storageRoot = Path.GetFullPath(rootDirectory);
            this.successfulWriteObserver = successfulWriteObserver;
        }

        /// <summary>Test-only path that keeps persistence away from user data.</summary>
        public static VNMetaProgressRepository CreateForTesting(string isolatedRootDirectory, Action successfulWriteObserver = null)
        {
            return new VNMetaProgressRepository(isolatedRootDirectory, successfulWriteObserver);
        }

        public VNMetaProgressReadResult Read()
        {
            string json;
            try
            {
                json = File.ReadAllText(CanonicalFilePath, Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                return VNMetaProgressReadResult.Missing();
            }
            catch (DirectoryNotFoundException)
            {
                return VNMetaProgressReadResult.Missing();
            }
            catch (Exception)
            {
                return VNMetaProgressReadResult.IoFailure("MetaProgress file could not be read without risking its preservation.");
            }

            if (TryParse(json, out var data, out var state, out var diagnostic))
                return VNMetaProgressReadResult.Valid(data);
            if (state == VNMetaProgressStorageState.Unsupported)
                return VNMetaProgressReadResult.Unsupported(diagnostic);

            if (TryQuarantineCanonicalFile(out var quarantineDiagnostic))
                return VNMetaProgressReadResult.Corrupted(diagnostic + " The original file was quarantined as corrupt.");

            return VNMetaProgressReadResult.IoFailure(diagnostic + " " + quarantineDiagnostic);
        }

        public VNMetaProgressWriteResult Write(VNMetaProgressData data)
        {
            if (!VNMetaProgressValidation.TryCreateCanonical(data, out var canonical, out var diagnostic))
                return VNMetaProgressWriteResult.Failure(VNMetaProgressStorageState.Corrupted, diagnostic);

            var existingState = InspectExistingCanonicalForWrite(out var existingDiagnostic);
            if (existingState == VNMetaProgressStorageState.Unsupported ||
                existingState == VNMetaProgressStorageState.Corrupted ||
                existingState == VNMetaProgressStorageState.IoFailure)
            {
                return VNMetaProgressWriteResult.Failure(existingState, existingDiagnostic, true);
            }

            string temporaryPath = null;
            try
            {
                Directory.CreateDirectory(storageRoot);
                var json = JsonUtility.ToJson(canonical, true);
                if (string.IsNullOrEmpty(json))
                    return VNMetaProgressWriteResult.Failure(VNMetaProgressStorageState.IoFailure, "MetaProgress serialization produced no JSON.");

                temporaryPath = Path.Combine(storageRoot, CanonicalFileName + "." + Guid.NewGuid().ToString("N") + ".tmp");
                var bytes = new UTF8Encoding(false).GetBytes(json);
                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(CanonicalFilePath)) File.Replace(temporaryPath, CanonicalFilePath, null);
                else File.Move(temporaryPath, CanonicalFilePath);

                successfulWriteObserver?.Invoke();
                return VNMetaProgressWriteResult.Success();
            }
            catch (Exception)
            {
                return VNMetaProgressWriteResult.Failure(VNMetaProgressStorageState.IoFailure, "MetaProgress file could not be written without replacing the authoritative file.");
            }
            finally
            {
                TryDeleteExactTemporaryFile(temporaryPath);
            }
        }

        private VNMetaProgressStorageState InspectExistingCanonicalForWrite(out string diagnostic)
        {
            diagnostic = null;
            try
            {
                if (!File.Exists(CanonicalFilePath)) return VNMetaProgressStorageState.Missing;
                var json = File.ReadAllText(CanonicalFilePath, Encoding.UTF8);
                if (TryParse(json, out _, out var state, out diagnostic)) return VNMetaProgressStorageState.Valid;
                return state;
            }
            catch (Exception)
            {
                diagnostic = "Existing MetaProgress file could not be inspected without risking its preservation.";
                return VNMetaProgressStorageState.IoFailure;
            }
        }

        private static bool TryParse(string json, out VNMetaProgressData data, out VNMetaProgressStorageState state, out string diagnostic)
        {
            data = null;
            state = VNMetaProgressStorageState.Corrupted;
            diagnostic = null;
            try
            {
                var schemaProbe = JsonUtility.FromJson<VNMetaProgressSchemaProbe>(json);
                if (schemaProbe == null || schemaProbe.schemaVersion <= 0)
                {
                    diagnostic = "MetaProgress JSON is missing a positive schema version.";
                    return false;
                }

                if (schemaProbe.schemaVersion > VNMetaProgressDefaults.CurrentSchemaVersion)
                {
                    state = VNMetaProgressStorageState.Unsupported;
                    diagnostic = "MetaProgress JSON uses a future schema version and was preserved unchanged.";
                    return false;
                }

                if (!TryGetTopLevelPropertyNames(json, out var propertyNames, out var nullPropertyNames))
                {
                    diagnostic = "MetaProgress JSON root object could not be parsed.";
                    return false;
                }

                foreach (var requiredField in RequiredSchemaV1Fields)
                {
                    if (propertyNames.Contains(requiredField)) continue;
                    diagnostic = "MetaProgress JSON is missing required schema-version-1 field '" + requiredField + "'.";
                    return false;
                }

                foreach (var requiredCollection in RequiredSchemaV1Fields)
                {
                    if (requiredCollection == "schemaVersion" || !nullPropertyNames.Contains(requiredCollection)) continue;
                    diagnostic = "MetaProgress JSON collection '" + requiredCollection + "' must not be null.";
                    return false;
                }

                var materialized = JsonUtility.FromJson<VNMetaProgressData>(json);
                if (!VNMetaProgressValidation.TryCreateCanonical(materialized, out data, out diagnostic))
                {
                    data = null;
                    return false;
                }

                state = VNMetaProgressStorageState.Valid;
                return true;
            }
            catch (Exception)
            {
                data = null;
                state = VNMetaProgressStorageState.Corrupted;
                diagnostic = "MetaProgress JSON could not be parsed.";
                return false;
            }
        }

        /// <summary>
        /// JsonUtility fills defaults for missing fields, so root property names
        /// are parsed from raw JSON before accepting schema-v1 materialization.
        /// </summary>
        private static bool TryGetTopLevelPropertyNames(string json, out HashSet<string> propertyNames, out HashSet<string> nullPropertyNames)
        {
            propertyNames = new HashSet<string>(StringComparer.Ordinal);
            nullPropertyNames = new HashSet<string>(StringComparer.Ordinal);
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
                SkipWhitespace(json, ref index);
                var isNull = StartsWithLiteral(json, index, "null");
                if (!TrySkipJsonValue(json, ref index, 0)) return false;
                if (isNull) nullPropertyNames.Add(propertyName);
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
                case '{': return TrySkipJsonObject(json, ref index, depth + 1);
                case '[': return TrySkipJsonArray(json, ref index, depth + 1);
                case '"': return TryReadJsonString(json, ref index, out _);
                case 't': return TryConsumeLiteral(json, ref index, "true");
                case 'f': return TryConsumeLiteral(json, ref index, "false");
                case 'n': return TryConsumeLiteral(json, ref index, "null");
                default: return TrySkipJsonNumber(json, ref index);
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
                if (!TryConsume(json, ref index, ':') || !TrySkipJsonValue(json, ref index, depth)) return false;
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
                    default: return false;
                }
            }

            return false;
        }

        private static bool TryReadUnicodeEscape(string json, ref int index, out char character)
        {
            character = default(char);
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

        private static bool StartsWithLiteral(string json, int index, string literal)
        {
            if (index + literal.Length > json.Length) return false;
            for (var offset = 0; offset < literal.Length; offset++)
            {
                if (json[index + offset] != literal[offset]) return false;
            }

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
                File.Move(CanonicalFilePath, CreateUniqueQuarantinePath());
                return true;
            }
            catch (Exception)
            {
                diagnostic = "The corrupt MetaProgress file could not be quarantined, so writes are blocked.";
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
                // Never risk the authoritative file while cleaning our own temp.
            }
        }

        [Serializable]
        private sealed class VNMetaProgressSchemaProbe
        {
            public int schemaVersion;
        }
    }
}
