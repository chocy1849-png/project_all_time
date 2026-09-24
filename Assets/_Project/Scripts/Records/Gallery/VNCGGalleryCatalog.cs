using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ProjectAllTime.VN.Records.Gallery
{
    [Serializable]
    public sealed class VNCGGalleryEntryDefinition
    {
        [SerializeField] private string cgId;
        [SerializeField] private string displayTitle;
        [SerializeField] private int sortOrder;
        [SerializeField] private bool showTitleWhenLocked;

        public string CgId => cgId;
        public string DisplayTitle => displayTitle;
        public int SortOrder => sortOrder;
        public bool ShowTitleWhenLocked => showTitleWhenLocked;

        public VNCGGalleryEntryDefinition() { }

        public VNCGGalleryEntryDefinition(string cgId, string displayTitle, int sortOrder = 0, bool showTitleWhenLocked = false)
        {
            this.cgId = cgId;
            this.displayTitle = displayTitle;
            this.sortOrder = sortOrder;
            this.showTitleWhenLocked = showTitleWhenLocked;
        }
    }

    [CreateAssetMenu(menuName = "VN/Records/CG Gallery Catalog", fileName = "VNCGGalleryCatalog")]
    public sealed class VNCGGalleryCatalog : ScriptableObject
    {
        [SerializeField] private List<VNCGGalleryEntryDefinition> entries = new();

        [NonSerialized] private ReadOnlyCollection<VNCGGalleryEntryDefinition> entriesView;
        [NonSerialized] private Dictionary<string, VNCGGalleryEntryDefinition> entriesById;
        [NonSerialized] private bool indexesBuilt;
        [NonSerialized] private bool indexesValid;

        public IReadOnlyList<VNCGGalleryEntryDefinition> Entries =>
            entriesView ??= (entries ?? new List<VNCGGalleryEntryDefinition>()).AsReadOnly();

        public bool TryGetCG(string cgId, out VNCGGalleryEntryDefinition entry)
        {
            entry = null;
            BuildIndex();
            return indexesValid && VNRecordsCatalogValidation.IsStableId(cgId) &&
                   entriesById.TryGetValue(cgId, out entry);
        }

        private void OnValidate()
        {
            entriesView = null;
            entriesById = null;
            indexesBuilt = false;
        }

        private void BuildIndex()
        {
            if (indexesBuilt) return;
            indexesBuilt = true;
            indexesValid = entries != null;
            entriesById = new Dictionary<string, VNCGGalleryEntryDefinition>(StringComparer.Ordinal);
            if (entries == null) return;

            foreach (var entry in entries)
            {
                if (entry == null || !VNRecordsCatalogValidation.IsStableId(entry.CgId) ||
                    !entriesById.TryAdd(entry.CgId, entry))
                    indexesValid = false;
            }
        }
    }
}
