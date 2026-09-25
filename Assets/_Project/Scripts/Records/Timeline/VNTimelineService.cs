using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectAllTime.VN.MetaProgress;

namespace ProjectAllTime.VN.Records.Timeline
{
    public enum VNTimelineEntryState
    {
        Hidden,
        Discovered,
        Completed
    }

    public sealed class VNTimelineEntryProjection
    {
        public string EntryId { get; }
        public string ChapterId { get; }
        public string DisplayTitle { get; }
        public int SortOrder { get; }
        public VNTimelineEntryState State { get; }
        public string ParentEntryId { get; }
        public string RelatedCgId { get; }

        internal VNTimelineEntryProjection(
            string entryId,
            string chapterId,
            string displayTitle,
            int sortOrder,
            VNTimelineEntryState state,
            string parentEntryId,
            string relatedCgId)
        {
            EntryId = entryId;
            ChapterId = chapterId;
            DisplayTitle = displayTitle;
            SortOrder = sortOrder;
            State = state;
            ParentEntryId = parentEntryId;
            RelatedCgId = relatedCgId;
        }
    }

    public sealed class VNTimelineChapterProjection
    {
        private readonly ReadOnlyCollection<VNTimelineEntryProjection> entries;

        public string ChapterId { get; }
        public string DisplayTitle { get; }
        public int SortOrder { get; }
        public bool IsUnlocked { get; }
        public IReadOnlyList<VNTimelineEntryProjection> Entries => entries;

        internal VNTimelineChapterProjection(
            string chapterId,
            string displayTitle,
            int sortOrder,
            bool isUnlocked,
            IEnumerable<VNTimelineEntryProjection> entries)
        {
            ChapterId = chapterId;
            DisplayTitle = displayTitle;
            SortOrder = sortOrder;
            IsUnlocked = isUnlocked;
            this.entries = Array.AsReadOnly(entries.ToArray());
        }
    }

    /// <summary>Builds fresh, spoiler-safe Timeline projections from authored definitions and durable query state.</summary>
    public sealed class VNTimelineService
    {
        private readonly VNTimelineCatalog catalog;
        private readonly VNMetaProgressService metaProgress;

        public VNTimelineService(VNTimelineCatalog catalog, VNMetaProgressService metaProgress)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.metaProgress = metaProgress ?? throw new ArgumentNullException(nameof(metaProgress));
        }

        public IReadOnlyList<VNTimelineChapterProjection> GetVisibleChapters()
        {
            var chapters = new List<VNTimelineChapterProjection>();
            foreach (var chapter in catalog.Chapters
                         .OrderBy(definition => definition.SortOrder)
                         .ThenBy(definition => definition.ChapterId, StringComparer.Ordinal))
            {
                var isUnlocked = metaProgress.IsChapterUnlocked(chapter.ChapterId);
                if (!isUnlocked)
                {
                    if (chapter.ShowWhenLocked)
                        chapters.Add(new VNTimelineChapterProjection(
                            chapter.ChapterId, chapter.DisplayTitle, chapter.SortOrder, false,
                            Array.Empty<VNTimelineEntryProjection>()));
                    continue;
                }

                chapters.Add(BuildUnlockedChapter(chapter));
            }

            return Array.AsReadOnly(chapters.ToArray());
        }

        private VNTimelineChapterProjection BuildUnlockedChapter(VNTimelineChapterDefinition chapter)
        {
            var visibleDefinitions = new List<VNTimelineEntryDefinition>();
            var visibleIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var entry in catalog.Entries)
            {
                if (!string.Equals(entry.ChapterId, chapter.ChapterId, StringComparison.Ordinal)) continue;
                if (ResolveState(entry) == VNTimelineEntryState.Hidden) continue;
                visibleDefinitions.Add(entry);
                visibleIds.Add(entry.EntryId);
            }

            var projections = new List<VNTimelineEntryProjection>(visibleDefinitions.Count);
            foreach (var entry in visibleDefinitions
                         .OrderBy(definition => definition.SortOrder)
                         .ThenBy(definition => definition.EntryId, StringComparer.Ordinal))
            {
                var parentEntryId = !string.IsNullOrEmpty(entry.ParentEntryId) &&
                                    visibleIds.Contains(entry.ParentEntryId)
                    ? entry.ParentEntryId
                    : null;

                projections.Add(new VNTimelineEntryProjection(
                    entry.EntryId,
                    entry.ChapterId,
                    entry.DisplayTitle,
                    entry.SortOrder,
                    ResolveState(entry),
                    parentEntryId,
                    entry.RelatedCgId));
            }

            return new VNTimelineChapterProjection(
                chapter.ChapterId, chapter.DisplayTitle, chapter.SortOrder, true, projections);
        }

        private VNTimelineEntryState ResolveState(VNTimelineEntryDefinition entry)
        {
            if (metaProgress.IsLineRead(entry.CompletionLineId)) return VNTimelineEntryState.Completed;
            if (metaProgress.IsLineRead(entry.DiscoveryLineId)) return VNTimelineEntryState.Discovered;
            return VNTimelineEntryState.Hidden;
        }
    }
}
