namespace ProjectAllTime.VN.MetaProgress
{
    public static class VNMetaProgressDefaults
    {
        public const int CurrentSchemaVersion = 1;

        public static VNMetaProgressData CreateDefault()
        {
            return new VNMetaProgressData
            {
                schemaVersion = CurrentSchemaVersion,
                readLineIds = new string[0],
                unlockedCGs = new string[0],
                unlockedChapters = new string[0],
                unlockedArchiveEntries = new string[0],
                unlockedAchievements = new string[0],
                completedEndings = new string[0],
            };
        }
    }
}
