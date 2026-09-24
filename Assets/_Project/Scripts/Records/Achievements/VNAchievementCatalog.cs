using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ProjectAllTime.VN.Records.Achievements
{
    public enum VNAchievementConditionType
    {
        Manual,
        EndingCompleted,
        ChapterUnlocked,
        CGCountAtLeast,
        ArchiveCountAtLeast,
        AllCGsUnlocked,
        AllArchiveEntriesUnlocked,
        AllOfAchievements
    }

    [Serializable]
    public sealed class VNAchievementDefinition
    {
        [SerializeField] private string achievementId;
        [SerializeField] private string displayTitle;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite optionalIcon;
        [SerializeField] private int sortOrder;
        [SerializeField] private bool hiddenBeforeUnlock;
        [SerializeField] private VNAchievementConditionType conditionType;
        [SerializeField] private string conditionTargetId;
        [SerializeField] private int threshold;
        [SerializeField] private List<string> prerequisiteAchievementIds = new();

        public string AchievementId => achievementId;
        public string DisplayTitle => displayTitle;
        public string Description => description;
        public Sprite OptionalIcon => optionalIcon;
        public int SortOrder => sortOrder;
        public bool HiddenBeforeUnlock => hiddenBeforeUnlock;
        public VNAchievementConditionType ConditionType => conditionType;
        public string ConditionTargetId => conditionTargetId;
        public int Threshold => threshold;
        public IReadOnlyList<string> PrerequisiteAchievementIds => prerequisiteAchievementIds?.AsReadOnly();

        public VNAchievementDefinition() { }

        public VNAchievementDefinition(
            string achievementId,
            string displayTitle,
            string description,
            int sortOrder,
            bool hiddenBeforeUnlock,
            VNAchievementConditionType conditionType,
            string conditionTargetId = null,
            int threshold = 0,
            IEnumerable<string> prerequisiteAchievementIds = null,
            Sprite optionalIcon = null)
        {
            this.achievementId = achievementId;
            this.displayTitle = displayTitle;
            this.description = description;
            this.sortOrder = sortOrder;
            this.hiddenBeforeUnlock = hiddenBeforeUnlock;
            this.conditionType = conditionType;
            this.conditionTargetId = conditionTargetId;
            this.threshold = threshold;
            this.prerequisiteAchievementIds = prerequisiteAchievementIds == null
                ? new List<string>()
                : new List<string>(prerequisiteAchievementIds);
            this.optionalIcon = optionalIcon;
        }
    }

    [CreateAssetMenu(menuName = "VN/Records/Achievement Catalog", fileName = "VNAchievementCatalog")]
    public sealed class VNAchievementCatalog : ScriptableObject
    {
        [SerializeField] private List<VNAchievementDefinition> achievements = new();

        [NonSerialized] private ReadOnlyCollection<VNAchievementDefinition> achievementsView;
        [NonSerialized] private Dictionary<string, VNAchievementDefinition> achievementsById;
        [NonSerialized] private bool indexesBuilt;
        [NonSerialized] private bool indexesValid;

        public IReadOnlyList<VNAchievementDefinition> Achievements =>
            achievementsView ??= (achievements ?? new List<VNAchievementDefinition>()).AsReadOnly();

        public bool TryGetAchievement(string achievementId, out VNAchievementDefinition achievement)
        {
            achievement = null;
            BuildIndex();
            return indexesValid && VNRecordsCatalogValidation.IsStableId(achievementId) &&
                   achievementsById.TryGetValue(achievementId, out achievement);
        }

        private void OnValidate()
        {
            achievementsView = null;
            achievementsById = null;
            indexesBuilt = false;
        }

        private void BuildIndex()
        {
            if (indexesBuilt) return;
            indexesBuilt = true;
            indexesValid = achievements != null;
            achievementsById = new Dictionary<string, VNAchievementDefinition>(StringComparer.Ordinal);
            if (achievements == null) return;

            foreach (var achievement in achievements)
            {
                if (achievement == null || !VNRecordsCatalogValidation.IsStableId(achievement.AchievementId) ||
                    !achievementsById.TryAdd(achievement.AchievementId, achievement))
                    indexesValid = false;
            }
        }
    }
}
