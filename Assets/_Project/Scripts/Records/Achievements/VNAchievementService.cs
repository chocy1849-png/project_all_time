using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectAllTime.VN.MetaProgress;
using UnityEngine;

namespace ProjectAllTime.VN.Records.Achievements
{
    public sealed class VNAchievementProjection
    {
        public string AchievementId { get; }
        public string DisplayTitle { get; }
        public string Description { get; }
        public Sprite OptionalIcon { get; }
        public int SortOrder { get; }
        public bool IsUnlocked { get; }

        internal VNAchievementProjection(
            string achievementId,
            string displayTitle,
            string description,
            Sprite optionalIcon,
            int sortOrder,
            bool isUnlocked)
        {
            AchievementId = achievementId;
            DisplayTitle = displayTitle;
            Description = description;
            OptionalIcon = optionalIcon;
            SortOrder = sortOrder;
            IsUnlocked = isUnlocked;
        }
    }

    /// <summary>Projects only persisted Achievement unlocks; authored conditions are never evaluated here.</summary>
    public sealed class VNAchievementService
    {
        private readonly VNAchievementCatalog catalog;
        private readonly VNMetaProgressService metaProgress;

        public VNAchievementService(VNAchievementCatalog catalog, VNMetaProgressService metaProgress)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.metaProgress = metaProgress ?? throw new ArgumentNullException(nameof(metaProgress));
        }

        public IReadOnlyList<VNAchievementProjection> GetAchievements()
        {
            var achievements = new List<VNAchievementProjection>();
            foreach (var definition in catalog.Achievements
                         .OrderBy(achievement => achievement.SortOrder)
                         .ThenBy(achievement => achievement.AchievementId, StringComparer.Ordinal))
            {
                var isUnlocked = metaProgress.IsAchievementUnlocked(definition.AchievementId);
                var hideMetadata = !isUnlocked && definition.HiddenBeforeUnlock;
                achievements.Add(new VNAchievementProjection(
                    definition.AchievementId,
                    hideMetadata ? null : definition.DisplayTitle,
                    hideMetadata ? null : definition.Description,
                    hideMetadata ? null : definition.OptionalIcon,
                    definition.SortOrder,
                    isUnlocked));
            }

            return Array.AsReadOnly(achievements.ToArray());
        }
    }
}
