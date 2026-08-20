using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// JsonUtility serialization and storage-level validation for schema v1.
    /// Catalog and runtime-system validation intentionally belong to later M5 work.
    /// </summary>
    public static class VNSaveSerializer
    {
        public const int CurrentSchemaVersion = 1;

        public static bool TrySerialize(SaveSlotData saveData, VNSaveSlotKey requestedKey, out string json, out string diagnostic)
        {
            json = null;
            if (!TryValidate(saveData, requestedKey, out diagnostic)) return false;

            try
            {
                json = JsonUtility.ToJson(saveData, true);
                if (string.IsNullOrEmpty(json))
                {
                    diagnostic = "Save serialization produced empty JSON.";
                    return false;
                }

                diagnostic = null;
                return true;
            }
            catch (Exception)
            {
                diagnostic = "Save serialization failed.";
                return false;
            }
        }

        public static VNSaveReadResult Deserialize(string json, VNSaveSlotKey requestedKey)
        {
            if (!requestedKey.IsValid) return VNSaveReadResult.InvalidRequest("The requested save slot key is invalid.");
            if (string.IsNullOrWhiteSpace(json)) return VNSaveReadResult.Corrupted("Save JSON is empty.");

            SaveSlotData saveData;
            try
            {
                saveData = JsonUtility.FromJson<SaveSlotData>(json);
            }
            catch (Exception)
            {
                return VNSaveReadResult.Corrupted("Save JSON could not be parsed.");
            }

            if (saveData == null) return VNSaveReadResult.Corrupted("Save JSON did not contain a root object.");
            if (saveData.schemaVersion > CurrentSchemaVersion)
                return VNSaveReadResult.Unsupported("Save schema is newer than this build supports.");

            if (!TryValidate(saveData, requestedKey, out var diagnostic)) return VNSaveReadResult.Corrupted(diagnostic);
            return VNSaveReadResult.Valid(saveData);
        }

        public static bool TryValidate(SaveSlotData saveData, VNSaveSlotKey requestedKey, out string diagnostic)
        {
            if (!requestedKey.IsValid)
            {
                diagnostic = "The requested save slot key is invalid.";
                return false;
            }

            if (saveData == null)
            {
                diagnostic = "Save data is missing its root object.";
                return false;
            }

            if (saveData.schemaVersion != CurrentSchemaVersion)
            {
                diagnostic = "Save schema version is not supported for this operation.";
                return false;
            }

            if (!VNSaveSlotKey.TryParseSerializedSlotType(saveData.slotType, out var storedSlotType))
            {
                diagnostic = "Save slot type is invalid.";
                return false;
            }

            var storedKey = new VNSaveSlotKey(storedSlotType, saveData.slotIndex);
            if (!storedKey.IsValid || storedKey != requestedKey)
            {
                diagnostic = "Save slot key does not match the requested slot.";
                return false;
            }

            if (!HasText(saveData.checkpointId) || !HasText(saveData.resumeNode))
            {
                diagnostic = "Save checkpoint data is incomplete.";
                return false;
            }

            if (!TryValidateYarnVariables(saveData.yarnVariables, out diagnostic)) return false;

            if (saveData.presentationState == null || saveData.presentationState.backgroundId == null || saveData.presentationState.cgId == null || saveData.presentationState.characters == null)
            {
                diagnostic = "Save presentation state is incomplete.";
                return false;
            }

            if (!ValidateCharacters(saveData.presentationState.characters, out diagnostic)) return false;

            if (saveData.audioState == null || saveData.audioState.bgmId == null || !IsNonNegativeFinite(saveData.audioState.playbackSeconds))
            {
                diagnostic = "Save audio state is invalid.";
                return false;
            }

            if (saveData.chapterId == null || saveData.sceneTitle == null)
            {
                diagnostic = "Save metadata is incomplete.";
                return false;
            }

            if (!IsNonNegativeFinite(saveData.playedSeconds))
            {
                diagnostic = "Saved play time must be finite and non-negative.";
                return false;
            }

            if (!TryParseUtcTimestamp(saveData.savedAtUtcIso8601, out _))
            {
                diagnostic = "Saved UTC timestamp is invalid.";
                return false;
            }

            if (!IsSafeThumbnailFileName(saveData.thumbnailFileName))
            {
                diagnostic = "Thumbnail filename is not a safe basename.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        /// <summary>
        /// Validates the transport form used to snapshot all Yarn variable
        /// kinds. This is intentionally reusable by runtime capture/restore so
        /// a save is checked before any VariableStorage mutation takes place.
        /// </summary>
        public static bool TryValidateYarnVariables(YarnVariablesData yarnVariables, out string diagnostic)
        {
            if (yarnVariables == null || yarnVariables.floats == null || yarnVariables.strings == null || yarnVariables.bools == null)
            {
                diagnostic = "Save variable arrays must all be present.";
                return false;
            }

            if (!ValidateFloatVariables(yarnVariables.floats, out diagnostic) ||
                !ValidateStringVariables(yarnVariables.strings, out diagnostic) ||
                !ValidateBoolVariables(yarnVariables.bools, out diagnostic))
                return false;

            diagnostic = null;
            return true;
        }

        public static string CreateUtcTimestamp() => DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        public static bool TryParseUtcTimestamp(string value, out DateTimeOffset timestamp)
        {
            timestamp = default;
            return !string.IsNullOrEmpty(value)
                   && DateTimeOffset.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp)
                   && timestamp.Offset == TimeSpan.Zero;
        }

        public static bool IsSafeThumbnailFileName(string fileName)
        {
            if (fileName == null) return false;
            if (fileName.Length == 0) return true;
            if (string.IsNullOrWhiteSpace(fileName) || fileName == "." || fileName == "..") return false;
            if (Path.IsPathRooted(fileName) || fileName.IndexOf("..", StringComparison.Ordinal) >= 0) return false;
            if (fileName.IndexOf('/') >= 0 || fileName.IndexOf('\\') >= 0 || fileName.IndexOf(':') >= 0) return false;
            if (fileName.EndsWith(".", StringComparison.Ordinal) || Path.GetFileName(fileName) != fileName) return false;
            return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        private static bool ValidateFloatVariables(FloatVariableEntry[] entries, out string diagnostic)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (entry == null || !HasText(entry.name) || !names.Add(entry.name) || !IsFinite(entry.value))
                {
                    diagnostic = "Float variables must have unique names and finite values.";
                    return false;
                }
            }

            diagnostic = null;
            return true;
        }

        private static bool ValidateStringVariables(StringVariableEntry[] entries, out string diagnostic)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (entry == null || !HasText(entry.name) || entry.value == null || !names.Add(entry.name))
                {
                    diagnostic = "String variables must have unique names and non-null values.";
                    return false;
                }
            }

            diagnostic = null;
            return true;
        }

        private static bool ValidateBoolVariables(BoolVariableEntry[] entries, out string diagnostic)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (entry == null || !HasText(entry.name) || !names.Add(entry.name))
                {
                    diagnostic = "Bool variables must have unique names.";
                    return false;
                }
            }

            diagnostic = null;
            return true;
        }

        private static bool ValidateCharacters(CharacterSaveState[] characters, out string diagnostic)
        {
            foreach (var character in characters)
            {
                if (character == null || !HasText(character.characterId) || !HasText(character.expressionId) ||
                    !IsValidSlot(character.slot) || !IsValidFacing(character.facing) || !IsPositiveFinite(character.scale))
                {
                    diagnostic = "Character save state is invalid.";
                    return false;
                }
            }

            diagnostic = null;
            return true;
        }

        private static bool HasText(string value) => !string.IsNullOrWhiteSpace(value);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool IsNonNegativeFinite(float value) => IsFinite(value) && value >= 0f;
        private static bool IsPositiveFinite(float value) => IsFinite(value) && value > 0f;

        private static bool IsValidSlot(string value)
        {
            return value == "far_left" || value == "left" || value == "center" || value == "right" || value == "far_right";
        }

        private static bool IsValidFacing(string value) => value == "left" || value == "right";
    }
}
