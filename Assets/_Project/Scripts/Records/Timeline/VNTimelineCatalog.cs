using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ProjectAllTime.VN.Records.Timeline
{
    [Serializable]
    public sealed class VNTimelineChapterDefinition
    {
        [SerializeField] private string chapterId;
        [SerializeField] private string displayTitle;
        [SerializeField] private int sortOrder;
        [SerializeField] private bool showWhenLocked;

        public string ChapterId => chapterId;
        public string DisplayTitle => displayTitle;
        public int SortOrder => sortOrder;
        public bool ShowWhenLocked => showWhenLocked;

        public VNTimelineChapterDefinition() { }

        public VNTimelineChapterDefinition(string chapterId, string displayTitle, int sortOrder = 0, bool showWhenLocked = false)
        {
            this.chapterId = chapterId;
            this.displayTitle = displayTitle;
            this.sortOrder = sortOrder;
            this.showWhenLocked = showWhenLocked;
        }
    }

    [Serializable]
    public sealed class VNTimelineEntryDefinition
    {
        [SerializeField] private string entryId;
        [SerializeField] private string chapterId;
        [SerializeField] private string displayTitle;
        [SerializeField] private int sortOrder;
        [SerializeField] private string discoveryLineId;
        [SerializeField] private string completionLineId;
        [SerializeField] private List<string> milestoneLineIds = new();
        [SerializeField] private string parentEntryId;
        [SerializeField] private string replayNode;
        [SerializeField] private string relatedCgId;

        public string EntryId => entryId;
        public string ChapterId => chapterId;
        public string DisplayTitle => displayTitle;
        public int SortOrder => sortOrder;
        public string DiscoveryLineId => discoveryLineId;
        public string CompletionLineId => completionLineId;
        public IReadOnlyList<string> MilestoneLineIds => milestoneLineIds?.AsReadOnly();
        public string ParentEntryId => parentEntryId;
        public string ReplayNode => replayNode;
        public string RelatedCgId => relatedCgId;

        public VNTimelineEntryDefinition() { }

        public VNTimelineEntryDefinition(
            string entryId,
            string chapterId,
            string displayTitle,
            int sortOrder,
            string discoveryLineId,
            string completionLineId,
            IEnumerable<string> milestoneLineIds,
            string parentEntryId = null,
            string replayNode = null,
            string relatedCgId = null)
        {
            this.entryId = entryId;
            this.chapterId = chapterId;
            this.displayTitle = displayTitle;
            this.sortOrder = sortOrder;
            this.discoveryLineId = discoveryLineId;
            this.completionLineId = completionLineId;
            this.milestoneLineIds = milestoneLineIds == null ? null : new List<string>(milestoneLineIds);
            this.parentEntryId = parentEntryId;
            this.replayNode = replayNode;
            this.relatedCgId = relatedCgId;
        }
    }

    [CreateAssetMenu(menuName = "VN/Records/Timeline Catalog", fileName = "VNTimelineCatalog")]
    public sealed class VNTimelineCatalog : ScriptableObject
    {
        [SerializeField] private List<VNTimelineChapterDefinition> chapters = new();
        [SerializeField] private List<VNTimelineEntryDefinition> entries = new();

        [NonSerialized] private ReadOnlyCollection<VNTimelineChapterDefinition> chaptersView;
        [NonSerialized] private ReadOnlyCollection<VNTimelineEntryDefinition> entriesView;
        [NonSerialized] private Dictionary<string, VNTimelineChapterDefinition> chaptersById;
        [NonSerialized] private Dictionary<string, VNTimelineEntryDefinition> entriesById;
        [NonSerialized] private bool indexesBuilt;
        [NonSerialized] private bool indexesValid;

        public IReadOnlyList<VNTimelineChapterDefinition> Chapters =>
            chaptersView ??= (chapters ?? new List<VNTimelineChapterDefinition>()).AsReadOnly();
        public IReadOnlyList<VNTimelineEntryDefinition> Entries =>
            entriesView ??= (entries ?? new List<VNTimelineEntryDefinition>()).AsReadOnly();

        public bool TryGetChapter(string chapterId, out VNTimelineChapterDefinition chapter)
        {
            chapter = null;
            BuildIndexes();
            return indexesValid && VNRecordsCatalogValidation.IsStableId(chapterId) &&
                   chaptersById.TryGetValue(chapterId, out chapter);
        }

        public bool TryGetEntry(string entryId, out VNTimelineEntryDefinition entry)
        {
            entry = null;
            BuildIndexes();
            return indexesValid && VNRecordsCatalogValidation.IsStableId(entryId) &&
                   entriesById.TryGetValue(entryId, out entry);
        }

        private void OnValidate() => InvalidateIndexes();

        private void InvalidateIndexes()
        {
            chaptersView = null;
            entriesView = null;
            chaptersById = null;
            entriesById = null;
            indexesBuilt = false;
        }

        private void BuildIndexes()
        {
            if (indexesBuilt) return;
            indexesBuilt = true;
            indexesValid = chapters != null && entries != null;
            chaptersById = new Dictionary<string, VNTimelineChapterDefinition>(StringComparer.Ordinal);
            entriesById = new Dictionary<string, VNTimelineEntryDefinition>(StringComparer.Ordinal);

            if (chapters != null)
            {
                foreach (var chapter in chapters)
                {
                    if (chapter == null || !VNRecordsCatalogValidation.IsStableId(chapter.ChapterId) ||
                        !chaptersById.TryAdd(chapter.ChapterId, chapter))
                        indexesValid = false;
                }
            }

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry == null || !VNRecordsCatalogValidation.IsStableId(entry.EntryId) ||
                        !entriesById.TryAdd(entry.EntryId, entry))
                        indexesValid = false;
                }
            }
        }
    }
}
