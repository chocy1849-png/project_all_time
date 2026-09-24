using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ProjectAllTime.VN.Records.Archive
{
    [Serializable]
    public sealed class VNArchiveCategoryDefinition
    {
        [SerializeField] private string categoryId;
        [SerializeField] private string displayTitle;
        [SerializeField] private int sortOrder;
        [SerializeField] private bool showCompletionCount;

        public string CategoryId => categoryId;
        public string DisplayTitle => displayTitle;
        public int SortOrder => sortOrder;
        public bool ShowCompletionCount => showCompletionCount;

        public VNArchiveCategoryDefinition() { }

        public VNArchiveCategoryDefinition(string categoryId, string displayTitle, int sortOrder = 0, bool showCompletionCount = false)
        {
            this.categoryId = categoryId;
            this.displayTitle = displayTitle;
            this.sortOrder = sortOrder;
            this.showCompletionCount = showCompletionCount;
        }
    }

    [Serializable]
    public sealed class VNArchiveEntryDefinition
    {
        [SerializeField] private string archiveId;
        [SerializeField] private string categoryId;
        [SerializeField] private string displayTitle;
        [SerializeField] private string summary;
        [SerializeField, TextArea] private string body;
        [SerializeField] private Sprite optionalImage;
        [SerializeField] private int sortOrder;
        [SerializeField] private bool showTitleWhenLocked;

        public string ArchiveId => archiveId;
        public string CategoryId => categoryId;
        public string DisplayTitle => displayTitle;
        public string Summary => summary;
        public string Body => body;
        public Sprite OptionalImage => optionalImage;
        public int SortOrder => sortOrder;
        public bool ShowTitleWhenLocked => showTitleWhenLocked;

        public VNArchiveEntryDefinition() { }

        public VNArchiveEntryDefinition(
            string archiveId,
            string categoryId,
            string displayTitle,
            string summary,
            string body,
            int sortOrder = 0,
            bool showTitleWhenLocked = false,
            Sprite optionalImage = null)
        {
            this.archiveId = archiveId;
            this.categoryId = categoryId;
            this.displayTitle = displayTitle;
            this.summary = summary;
            this.body = body;
            this.sortOrder = sortOrder;
            this.showTitleWhenLocked = showTitleWhenLocked;
            this.optionalImage = optionalImage;
        }
    }

    [CreateAssetMenu(menuName = "VN/Records/Archive Catalog", fileName = "VNArchiveCatalog")]
    public sealed class VNArchiveCatalog : ScriptableObject
    {
        [SerializeField] private List<VNArchiveCategoryDefinition> categories = new();
        [SerializeField] private List<VNArchiveEntryDefinition> entries = new();

        [NonSerialized] private ReadOnlyCollection<VNArchiveCategoryDefinition> categoriesView;
        [NonSerialized] private ReadOnlyCollection<VNArchiveEntryDefinition> entriesView;
        [NonSerialized] private Dictionary<string, VNArchiveCategoryDefinition> categoriesById;
        [NonSerialized] private Dictionary<string, VNArchiveEntryDefinition> entriesById;
        [NonSerialized] private bool indexesBuilt;
        [NonSerialized] private bool indexesValid;

        public IReadOnlyList<VNArchiveCategoryDefinition> Categories =>
            categoriesView ??= (categories ?? new List<VNArchiveCategoryDefinition>()).AsReadOnly();
        public IReadOnlyList<VNArchiveEntryDefinition> Entries =>
            entriesView ??= (entries ?? new List<VNArchiveEntryDefinition>()).AsReadOnly();

        public bool TryGetCategory(string categoryId, out VNArchiveCategoryDefinition category)
        {
            category = null;
            BuildIndexes();
            return indexesValid && VNRecordsCatalogValidation.IsStableId(categoryId) &&
                   categoriesById.TryGetValue(categoryId, out category);
        }

        public bool TryGetEntry(string archiveId, out VNArchiveEntryDefinition entry)
        {
            entry = null;
            BuildIndexes();
            return indexesValid && VNRecordsCatalogValidation.IsStableId(archiveId) &&
                   entriesById.TryGetValue(archiveId, out entry);
        }

        private void OnValidate()
        {
            categoriesView = null;
            entriesView = null;
            categoriesById = null;
            entriesById = null;
            indexesBuilt = false;
        }

        private void BuildIndexes()
        {
            if (indexesBuilt) return;
            indexesBuilt = true;
            indexesValid = categories != null && entries != null;
            categoriesById = new Dictionary<string, VNArchiveCategoryDefinition>(StringComparer.Ordinal);
            entriesById = new Dictionary<string, VNArchiveEntryDefinition>(StringComparer.Ordinal);

            if (categories != null)
            {
                foreach (var category in categories)
                {
                    if (category == null || !VNRecordsCatalogValidation.IsStableId(category.CategoryId) ||
                        !categoriesById.TryAdd(category.CategoryId, category))
                        indexesValid = false;
                }
            }

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry == null || !VNRecordsCatalogValidation.IsStableId(entry.ArchiveId) ||
                        !entriesById.TryAdd(entry.ArchiveId, entry))
                        indexesValid = false;
                }
            }
        }
    }
}
