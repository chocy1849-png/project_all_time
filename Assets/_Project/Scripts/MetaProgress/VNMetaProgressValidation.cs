using System;
using System.Collections.Generic;

namespace ProjectAllTime.VN.MetaProgress
{
    /// <summary>
    /// Pure schema-v1 validation and canonicalization. This class never changes
    /// caller-owned DTO collections.
    /// </summary>
    public static class VNMetaProgressValidation
    {
        public static bool TryValidate(VNMetaProgressData data, out string diagnostic)
        {
            if (data == null)
            {
                diagnostic = "MetaProgress data is missing.";
                return false;
            }

            if (data.schemaVersion != VNMetaProgressDefaults.CurrentSchemaVersion)
            {
                diagnostic = "MetaProgress data is not schema version 1.";
                return false;
            }

            return TryValidateCollection(data.readLineIds, "readLineIds", out diagnostic) &&
                   TryValidateCollection(data.unlockedCGs, "unlockedCGs", out diagnostic) &&
                   TryValidateCollection(data.unlockedChapters, "unlockedChapters", out diagnostic) &&
                   TryValidateCollection(data.unlockedArchiveEntries, "unlockedArchiveEntries", out diagnostic) &&
                   TryValidateCollection(data.unlockedAchievements, "unlockedAchievements", out diagnostic) &&
                   TryValidateCollection(data.completedEndings, "completedEndings", out diagnostic);
        }

        public static bool TryCreateCanonical(VNMetaProgressData source, out VNMetaProgressData canonical, out string diagnostic)
        {
            canonical = null;
            if (!TryValidate(source, out diagnostic)) return false;

            canonical = new VNMetaProgressData
            {
                schemaVersion = VNMetaProgressDefaults.CurrentSchemaVersion,
                readLineIds = Canonicalize(source.readLineIds),
                unlockedCGs = Canonicalize(source.unlockedCGs),
                unlockedChapters = Canonicalize(source.unlockedChapters),
                unlockedArchiveEntries = Canonicalize(source.unlockedArchiveEntries),
                unlockedAchievements = Canonicalize(source.unlockedAchievements),
                completedEndings = Canonicalize(source.completedEndings),
            };
            diagnostic = null;
            return true;
        }

        public static bool IsValidId(string id) => !string.IsNullOrWhiteSpace(id);

        private static bool TryValidateCollection(string[] ids, string fieldName, out string diagnostic)
        {
            if (ids == null)
            {
                diagnostic = "MetaProgress collection '" + fieldName + "' is missing.";
                return false;
            }

            foreach (var id in ids)
            {
                if (IsValidId(id)) continue;
                diagnostic = "MetaProgress collection '" + fieldName + "' contains an invalid ID.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        private static string[] Canonicalize(string[] source)
        {
            var ids = new HashSet<string>(source, StringComparer.Ordinal);
            var canonical = new List<string>(ids);
            canonical.Sort(StringComparer.Ordinal);
            return canonical.ToArray();
        }
    }
}
