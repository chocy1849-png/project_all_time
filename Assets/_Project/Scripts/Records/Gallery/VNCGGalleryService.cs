using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using UnityEngine;

namespace ProjectAllTime.VN.Records.Gallery
{
    public sealed class VNCGGalleryEntryProjection
    {
        public string CgId { get; }
        public string DisplayTitle { get; }
        public int SortOrder { get; }
        public bool IsUnlocked { get; }
        public Sprite Sprite { get; }

        internal VNCGGalleryEntryProjection(
            string cgId,
            string displayTitle,
            int sortOrder,
            bool isUnlocked,
            Sprite sprite)
        {
            CgId = cgId;
            DisplayTitle = displayTitle;
            SortOrder = sortOrder;
            IsUnlocked = isUnlocked;
            Sprite = sprite;
        }
    }

    /// <summary>Builds fresh Gallery projections while resolving unlocked artwork only through VNPresentationCatalog.</summary>
    public sealed class VNCGGalleryService
    {
        private static readonly IReadOnlyList<VNCGGalleryEntryProjection> EmptyEntries =
            Array.AsReadOnly(Array.Empty<VNCGGalleryEntryProjection>());

        private readonly VNCGGalleryCatalog catalog;
        private readonly VNPresentationCatalog presentationCatalog;
        private readonly VNMetaProgressService metaProgress;

        public VNCGGalleryService(
            VNCGGalleryCatalog catalog,
            VNPresentationCatalog presentationCatalog,
            VNMetaProgressService metaProgress)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.presentationCatalog = presentationCatalog ?? throw new ArgumentNullException(nameof(presentationCatalog));
            this.metaProgress = metaProgress ?? throw new ArgumentNullException(nameof(metaProgress));
        }

        public bool TryGetGalleryEntries(
            out IReadOnlyList<VNCGGalleryEntryProjection> entries,
            out string diagnostic)
        {
            var projections = new List<VNCGGalleryEntryProjection>();
            foreach (var definition in catalog.Entries
                         .OrderBy(entry => entry.SortOrder)
                         .ThenBy(entry => entry.CgId, StringComparer.Ordinal))
            {
                var isUnlocked = metaProgress.IsCGUnlocked(definition.CgId);
                if (!isUnlocked)
                {
                    projections.Add(new VNCGGalleryEntryProjection(
                        definition.CgId,
                        definition.ShowTitleWhenLocked ? definition.DisplayTitle : null,
                        definition.SortOrder,
                        false,
                        null));
                    continue;
                }

                if (!presentationCatalog.TryGetCG(definition.CgId, out var sprite) || sprite == null)
                {
                    entries = EmptyEntries;
                    diagnostic = $"Unlocked Gallery CG '{definition.CgId}' could not resolve a Sprite through VNPresentationCatalog.";
                    return false;
                }

                projections.Add(new VNCGGalleryEntryProjection(
                    definition.CgId,
                    definition.DisplayTitle,
                    definition.SortOrder,
                    true,
                    sprite));
            }

            entries = Array.AsReadOnly(projections.ToArray());
            diagnostic = null;
            return true;
        }
    }
}
