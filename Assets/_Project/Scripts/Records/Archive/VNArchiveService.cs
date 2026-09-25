using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectAllTime.VN.MetaProgress;
using UnityEngine;

namespace ProjectAllTime.VN.Records.Archive
{
    public sealed class VNArchiveCategoryProjection
    {
        public string CategoryId { get; }
        public string DisplayTitle { get; }
        public int SortOrder { get; }
        public bool HasCompletionCount { get; }
        public int? UnlockedCount { get; }
        public int? TotalCount { get; }

        internal VNArchiveCategoryProjection(
            string categoryId,
            string displayTitle,
            int sortOrder,
            bool hasCompletionCount,
            int? unlockedCount,
            int? totalCount)
        {
            CategoryId = categoryId;
            DisplayTitle = displayTitle;
            SortOrder = sortOrder;
            HasCompletionCount = hasCompletionCount;
            UnlockedCount = unlockedCount;
            TotalCount = totalCount;
        }
    }

    public sealed class VNArchiveEntryProjection
    {
        public string ArchiveId { get; }
        public string CategoryId { get; }
        public string DisplayTitle { get; }
        public string Summary { get; }
        public string Body { get; }
        public Sprite OptionalImage { get; }
        public int SortOrder { get; }
        public bool IsUnlocked { get; }

        internal VNArchiveEntryProjection(
            string archiveId,
            string categoryId,
            string displayTitle,
            string summary,
            string body,
            Sprite optionalImage,
            int sortOrder,
            bool isUnlocked)
        {
            ArchiveId = archiveId;
            CategoryId = categoryId;
            DisplayTitle = displayTitle;
            Summary = summary;
            Body = body;
            OptionalImage = optionalImage;
            SortOrder = sortOrder;
            IsUnlocked = isUnlocked;
        }
    }

    /// <summary>Builds spoiler-safe Archive category and entry projections from current MetaProgress queries.</summary>
    public sealed class VNArchiveService
    {
        private static readonly IReadOnlyList<VNArchiveEntryProjection> EmptyEntries =
            Array.AsReadOnly(Array.Empty<VNArchiveEntryProjection>());

        private readonly VNArchiveCatalog catalog;
        private readonly VNMetaProgressService metaProgress;

        public VNArchiveService(VNArchiveCatalog catalog, VNMetaProgressService metaProgress)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.metaProgress = metaProgress ?? throw new ArgumentNullException(nameof(metaProgress));
        }

        public IReadOnlyList<VNArchiveCategoryProjection> GetCategories()
        {
            var categories = new List<VNArchiveCategoryProjection>();
            foreach (var definition in catalog.Categories
                         .OrderBy(category => category.SortOrder)
                         .ThenBy(category => category.CategoryId, StringComparer.Ordinal))
            {
                if (!definition.ShowCompletionCount)
                {
                    categories.Add(new VNArchiveCategoryProjection(
                        definition.CategoryId, definition.DisplayTitle, definition.SortOrder, false, null, null));
                    continue;
                }

                var categoryEntries = catalog.Entries
                    .Where(entry => string.Equals(entry.CategoryId, definition.CategoryId, StringComparison.Ordinal))
                    .ToArray();
                var unlockedCount = 0;
                foreach (var entry in categoryEntries)
                    if (metaProgress.IsArchiveEntryUnlocked(entry.ArchiveId)) unlockedCount++;

                categories.Add(new VNArchiveCategoryProjection(
                    definition.CategoryId,
                    definition.DisplayTitle,
                    definition.SortOrder,
                    true,
                    unlockedCount,
                    categoryEntries.Length));
            }

            return Array.AsReadOnly(categories.ToArray());
        }

        public IReadOnlyList<VNArchiveEntryProjection> GetEntries(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId) ||
                !catalog.TryGetCategory(categoryId, out _))
                return EmptyEntries;

            var entries = new List<VNArchiveEntryProjection>();
            foreach (var definition in catalog.Entries
                         .Where(entry => string.Equals(entry.CategoryId, categoryId, StringComparison.Ordinal))
                         .OrderBy(entry => entry.SortOrder)
                         .ThenBy(entry => entry.ArchiveId, StringComparer.Ordinal))
            {
                var isUnlocked = metaProgress.IsArchiveEntryUnlocked(definition.ArchiveId);
                entries.Add(new VNArchiveEntryProjection(
                    definition.ArchiveId,
                    definition.CategoryId,
                    isUnlocked || definition.ShowTitleWhenLocked ? definition.DisplayTitle : null,
                    isUnlocked ? definition.Summary : null,
                    isUnlocked ? definition.Body : null,
                    isUnlocked ? definition.OptionalImage : null,
                    definition.SortOrder,
                    isUnlocked));
            }

            return Array.AsReadOnly(entries.ToArray());
        }
    }
}
