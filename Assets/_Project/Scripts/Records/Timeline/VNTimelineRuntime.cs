using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectAllTime.VN.MetaProgress;

namespace ProjectAllTime.VN.Records.Timeline
{
    public enum VNTimelineChapterState
    {
        Locked,
        Available,
        InProgress,
        Completed,
    }

    /// <summary>A read-only Timeline view built from the current catalog and MetaProgress state.</summary>
    public sealed class VNTimelineSnapshot
    {
        private static readonly IReadOnlyList<VNTimelineEntryRuntime> EmptyEntries =
            Array.AsReadOnly(Array.Empty<VNTimelineEntryRuntime>());
        private readonly ReadOnlyCollection<VNTimelineChapterRuntime> chapters;
        private readonly Dictionary<string, VNTimelineChapterRuntime> chaptersById;
        private readonly Dictionary<string, VNTimelineEntryRuntime> entriesById;

        public IReadOnlyList<VNTimelineChapterRuntime> Chapters => chapters;

        internal VNTimelineSnapshot(IEnumerable<VNTimelineChapterRuntime> chapters)
        {
            var chapterArray = (chapters ?? Enumerable.Empty<VNTimelineChapterRuntime>()).ToArray();
            this.chapters = Array.AsReadOnly(chapterArray);
            chaptersById = new Dictionary<string, VNTimelineChapterRuntime>(StringComparer.Ordinal);
            entriesById = new Dictionary<string, VNTimelineEntryRuntime>(StringComparer.Ordinal);

            foreach (var chapter in chapterArray)
            {
                if (chapter == null || string.IsNullOrEmpty(chapter.ChapterId)) continue;
                chaptersById[chapter.ChapterId] = chapter;
                IndexEntries(chapter.Roots);
            }
        }

        public bool TryGetChapter(string chapterId, out VNTimelineChapterRuntime chapter)
        {
            chapter = null;
            return !string.IsNullOrWhiteSpace(chapterId) && chaptersById.TryGetValue(chapterId, out chapter);
        }

        public bool TryGetEntry(string entryId, out VNTimelineEntryRuntime entry)
        {
            entry = null;
            return !string.IsNullOrWhiteSpace(entryId) && entriesById.TryGetValue(entryId, out entry);
        }

        public IReadOnlyList<VNTimelineEntryRuntime> GetRoots(string chapterId)
        {
            return TryGetChapter(chapterId, out var chapter) ? chapter.Roots : EmptyEntries;
        }

        public IReadOnlyList<VNTimelineEntryRuntime> GetChildren(string entryId)
        {
            return TryGetEntry(entryId, out var entry) ? entry.Children : EmptyEntries;
        }

        private void IndexEntries(IReadOnlyList<VNTimelineEntryRuntime> entries)
        {
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.EntryId)) continue;
                entriesById[entry.EntryId] = entry;
                IndexEntries(entry.Children);
            }
        }
    }

    public sealed class VNTimelineChapterRuntime
    {
        private readonly ReadOnlyCollection<VNTimelineEntryRuntime> roots;

        public string ChapterId { get; }
        public string DisplayTitle { get; }
        public int SortOrder { get; }
        public VNTimelineChapterState State { get; }
        public IReadOnlyList<VNTimelineEntryRuntime> Roots => roots;

        internal VNTimelineChapterRuntime(
            string chapterId,
            string displayTitle,
            int sortOrder,
            VNTimelineChapterState state,
            IEnumerable<VNTimelineEntryRuntime> roots)
        {
            ChapterId = chapterId;
            DisplayTitle = displayTitle;
            SortOrder = sortOrder;
            State = state;
            this.roots = Array.AsReadOnly((roots ?? Enumerable.Empty<VNTimelineEntryRuntime>()).ToArray());
        }
    }

    public sealed class VNTimelineEntryRuntime
    {
        private readonly ReadOnlyCollection<VNTimelineEntryRuntime> children;

        public string EntryId { get; }
        public string ChapterId { get; }
        public string DisplayTitle { get; }
        public int SortOrder { get; }
        public VNTimelineEntryState State { get; }
        public string ParentEntryId { get; }
        public string RelatedCgId { get; }
        public IReadOnlyList<VNTimelineEntryRuntime> Children => children;

        internal VNTimelineEntryRuntime(
            string entryId,
            string chapterId,
            string displayTitle,
            int sortOrder,
            VNTimelineEntryState state,
            string parentEntryId,
            string relatedCgId,
            IEnumerable<VNTimelineEntryRuntime> children)
        {
            EntryId = entryId;
            ChapterId = chapterId;
            DisplayTitle = displayTitle;
            SortOrder = sortOrder;
            State = state;
            ParentEntryId = parentEntryId;
            RelatedCgId = relatedCgId;
            this.children = Array.AsReadOnly((children ?? Enumerable.Empty<VNTimelineEntryRuntime>()).ToArray());
        }
    }

    /// <summary>Composes spoiler-safe M9-03 projections with chapter completion semantics.</summary>
    public sealed class VNTimelineRuntime
    {
        private readonly VNTimelineCatalog catalog;
        private readonly VNTimelineService timelineService;
        private readonly VNMetaProgressService metaProgress;

        public VNTimelineRuntime(
            VNTimelineCatalog catalog,
            VNTimelineService timelineService,
            VNMetaProgressService metaProgress)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.timelineService = timelineService ?? throw new ArgumentNullException(nameof(timelineService));
            this.metaProgress = metaProgress ?? throw new ArgumentNullException(nameof(metaProgress));
        }

        public VNTimelineSnapshot BuildSnapshot()
        {
            var authoredEntriesByChapter = BuildAuthoredEntryIndex();
            var projectedChapters = timelineService.GetVisibleChapters() ?? Array.Empty<VNTimelineChapterProjection>();
            var runtimeChapters = new List<VNTimelineChapterRuntime>(projectedChapters.Count);

            foreach (var projection in projectedChapters
                         .Where(chapter => chapter != null)
                         .OrderBy(chapter => chapter.SortOrder)
                         .ThenBy(chapter => chapter.ChapterId, StringComparer.Ordinal))
            {
                if (!projection.IsUnlocked)
                {
                    runtimeChapters.Add(new VNTimelineChapterRuntime(
                        projection.ChapterId,
                        projection.DisplayTitle,
                        projection.SortOrder,
                        VNTimelineChapterState.Locked,
                        Array.Empty<VNTimelineEntryRuntime>()));
                    continue;
                }

                IReadOnlyList<VNTimelineEntryDefinition> authoredEntries =
                    authoredEntriesByChapter.TryGetValue(projection.ChapterId, out var indexedEntries)
                    ? indexedEntries
                    : Array.Empty<VNTimelineEntryDefinition>();
                var visibleEntries = (projection.Entries ?? Array.Empty<VNTimelineEntryProjection>())
                    .Where(entry => entry != null && IsVisibleState(entry.State))
                    .ToArray();

                var state = ResolveUnlockedChapterState(authoredEntries, visibleEntries.Length > 0);
                var roots = BuildVisibleTree(projection.ChapterId, visibleEntries);
                runtimeChapters.Add(new VNTimelineChapterRuntime(
                    projection.ChapterId,
                    projection.DisplayTitle,
                    projection.SortOrder,
                    state,
                    roots));
            }

            return new VNTimelineSnapshot(runtimeChapters);
        }

        private Dictionary<string, List<VNTimelineEntryDefinition>> BuildAuthoredEntryIndex()
        {
            var byChapter = new Dictionary<string, List<VNTimelineEntryDefinition>>(StringComparer.Ordinal);
            foreach (var entry in catalog.Entries ?? Array.Empty<VNTimelineEntryDefinition>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ChapterId)) continue;
                if (!byChapter.TryGetValue(entry.ChapterId, out var entries))
                {
                    entries = new List<VNTimelineEntryDefinition>();
                    byChapter.Add(entry.ChapterId, entries);
                }

                entries.Add(entry);
            }

            return byChapter;
        }

        private VNTimelineChapterState ResolveUnlockedChapterState(
            IReadOnlyList<VNTimelineEntryDefinition> authoredEntries,
            bool hasVisibleEntry)
        {
            if (authoredEntries.Count == 0) return VNTimelineChapterState.Available;

            var allCompletionMarkersRead = true;
            foreach (var entry in authoredEntries)
            {
                if (metaProgress.IsLineRead(entry.CompletionLineId)) continue;
                allCompletionMarkersRead = false;
                break;
            }

            if (allCompletionMarkersRead) return VNTimelineChapterState.Completed;
            return hasVisibleEntry ? VNTimelineChapterState.InProgress : VNTimelineChapterState.Available;
        }

        private static IReadOnlyList<VNTimelineEntryRuntime> BuildVisibleTree(
            string chapterId,
            IReadOnlyList<VNTimelineEntryProjection> projections)
        {
            var buildersById = new Dictionary<string, EntryBuilder>(StringComparer.Ordinal);
            foreach (var projection in projections)
            {
                if (!string.Equals(projection.ChapterId, chapterId, StringComparison.Ordinal)) continue;
                if (string.IsNullOrWhiteSpace(projection.EntryId) || buildersById.ContainsKey(projection.EntryId)) continue;
                buildersById.Add(projection.EntryId, new EntryBuilder(projection));
            }

            var rootBuilders = new List<EntryBuilder>();
            foreach (var builder in buildersById.Values)
            {
                if (!string.IsNullOrWhiteSpace(builder.AuthoredParentEntryId) &&
                    buildersById.TryGetValue(builder.AuthoredParentEntryId, out var parent) &&
                    !ReferenceEquals(parent, builder))
                {
                    builder.Parent = parent;
                    parent.Children.Add(builder);
                }
                else
                {
                    rootBuilders.Add(builder);
                }
            }

            rootBuilders.Sort(EntryBuilderComparer.Instance);
            var roots = new List<VNTimelineEntryRuntime>(rootBuilders.Count);
            foreach (var root in rootBuilders)
                roots.Add(Freeze(root));

            return Array.AsReadOnly(roots.ToArray());
        }

        private static VNTimelineEntryRuntime Freeze(EntryBuilder builder)
        {
            builder.Children.Sort(EntryBuilderComparer.Instance);
            var children = new List<VNTimelineEntryRuntime>(builder.Children.Count);
            foreach (var child in builder.Children)
                children.Add(Freeze(child));

            var runtimeEntry = new VNTimelineEntryRuntime(
                builder.EntryId,
                builder.ChapterId,
                builder.DisplayTitle,
                builder.SortOrder,
                builder.State,
                builder.Parent?.EntryId,
                builder.RelatedCgId,
                children);
            return runtimeEntry;
        }

        private static bool IsVisibleState(VNTimelineEntryState state)
        {
            return state == VNTimelineEntryState.Discovered || state == VNTimelineEntryState.Completed;
        }

        private sealed class EntryBuilder
        {
            public readonly string EntryId;
            public readonly string ChapterId;
            public readonly string DisplayTitle;
            public readonly int SortOrder;
            public readonly VNTimelineEntryState State;
            public readonly string AuthoredParentEntryId;
            public readonly string RelatedCgId;
            public readonly List<EntryBuilder> Children = new();
            public EntryBuilder Parent;

            public EntryBuilder(VNTimelineEntryProjection projection)
            {
                EntryId = projection.EntryId;
                ChapterId = projection.ChapterId;
                DisplayTitle = projection.DisplayTitle;
                SortOrder = projection.SortOrder;
                State = projection.State;
                AuthoredParentEntryId = projection.ParentEntryId;
                RelatedCgId = projection.RelatedCgId;
            }
        }

        private sealed class EntryBuilderComparer : IComparer<EntryBuilder>
        {
            public static readonly EntryBuilderComparer Instance = new();

            public int Compare(EntryBuilder left, EntryBuilder right)
            {
                var sortOrderComparison = left.SortOrder.CompareTo(right.SortOrder);
                return sortOrderComparison != 0
                    ? sortOrderComparison
                    : StringComparer.Ordinal.Compare(left.EntryId, right.EntryId);
            }
        }
    }
}
