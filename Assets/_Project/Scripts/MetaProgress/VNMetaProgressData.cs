using System;

namespace ProjectAllTime.VN.MetaProgress
{
    /// <summary>
    /// Unity-serializable schema DTO. Arrays deliberately keep serialized state
    /// separate from the ordinal set semantics used by the service.
    /// </summary>
    [Serializable]
    public sealed class VNMetaProgressData
    {
        public int schemaVersion;
        public string[] readLineIds;
        public string[] unlockedCGs;
        public string[] unlockedChapters;
        public string[] unlockedArchiveEntries;
        public string[] unlockedAchievements;
        public string[] completedEndings;

        public VNMetaProgressData Copy()
        {
            return new VNMetaProgressData
            {
                schemaVersion = schemaVersion,
                readLineIds = CopyArray(readLineIds),
                unlockedCGs = CopyArray(unlockedCGs),
                unlockedChapters = CopyArray(unlockedChapters),
                unlockedArchiveEntries = CopyArray(unlockedArchiveEntries),
                unlockedAchievements = CopyArray(unlockedAchievements),
                completedEndings = CopyArray(completedEndings),
            };
        }

        internal static string[] CopyArray(string[] source)
        {
            if (source == null) return null;
            var copy = new string[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }
    }
}
