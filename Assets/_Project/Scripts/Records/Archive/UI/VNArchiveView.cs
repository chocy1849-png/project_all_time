using System;
using System.Collections.Generic;
using ProjectAllTime.VN.Records.Archive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Archive.UI
{
    /// <summary>Renders fresh, spoiler-safe Archive projections supplied by VNArchiveService.</summary>
    [DisallowMultipleComponent]
    public sealed class VNArchiveView : MonoBehaviour
    {
        [SerializeField] private Transform categoryContent;
        [SerializeField] private VNArchiveCategoryItem categoryItemPrefab;
        [SerializeField] private ScrollRect categoryScrollRect;
        [SerializeField] private Transform entryContent;
        [SerializeField] private VNArchiveEntryItem entryItemPrefab;
        [SerializeField] private ScrollRect entryScrollRect;
        [SerializeField] private VNArchiveDetailView detailView;
        [SerializeField] private GameObject globalEmptyStateRoot;
        [SerializeField] private GameObject categoryEmptyStateRoot;
        [SerializeField] private GameObject contentStateRoot;
        [SerializeField] private GameObject errorStateRoot;
        [SerializeField] private TMP_Text errorText;

        private readonly List<VNArchiveCategoryItem> categoryItems = new();
        private readonly List<VNArchiveEntryItem> entryItems = new();
        private VNArchiveService archiveService;
        private IReadOnlyList<VNArchiveCategoryProjection> currentCategories;
        private IReadOnlyList<VNArchiveEntryProjection> currentEntries;
        private string selectedCategoryId;
        private string selectedArchiveId;
        private bool initialized;

        public string SelectedCategoryId => selectedCategoryId;
        public string SelectedArchiveId => selectedArchiveId;
        public string LastDiagnostic { get; private set; }
        public int CategoryPoolCapacity => categoryItems.Count;
        public int EntryPoolCapacity => entryItems.Count;

        private void Awake()
        {
            SetStateRoots(false, false, false, false, null);
            if (detailView != null) detailView.Clear();
        }

        private void OnEnable()
        {
            if (initialized) Refresh();
        }

        private void OnDisable()
        {
            if (detailView != null) detailView.Clear();
        }

        public bool Initialize(VNArchiveService service)
        {
            if (service == null)
            {
                LastDiagnostic = "A VNArchiveService is required.";
                return false;
            }

            if (initialized)
            {
                if (!ReferenceEquals(archiveService, service))
                {
                    LastDiagnostic = "VNArchiveView is already initialized with a different service.";
                    return false;
                }

                LastDiagnostic = null;
                return !isActiveAndEnabled || Refresh();
            }

            archiveService = service;
            initialized = true;
            LastDiagnostic = null;
            return !isActiveAndEnabled || Refresh();
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (categoryContent == null) return Fail("Archive category content Transform is missing.", out diagnostic);
            if (categoryContent is not RectTransform categoryRect)
                return Fail("Archive category content must have a RectTransform.", out diagnostic);
            if (categoryItemPrefab == null) return Fail("Archive category item prefab is missing.", out diagnostic);
            if (!categoryItemPrefab.TryValidateWiring(out var categoryDiagnostic))
                return Fail("Archive category item prefab: " + categoryDiagnostic, out diagnostic);
            if (categoryScrollRect != null && categoryScrollRect.content != categoryRect)
                return Fail("Archive category ScrollRect content must reference category content.", out diagnostic);

            if (entryContent == null) return Fail("Archive entry content Transform is missing.", out diagnostic);
            if (entryContent is not RectTransform entryRect)
                return Fail("Archive entry content must have a RectTransform.", out diagnostic);
            if (entryItemPrefab == null) return Fail("Archive entry item prefab is missing.", out diagnostic);
            if (!entryItemPrefab.TryValidateWiring(out var entryDiagnostic))
                return Fail("Archive entry item prefab: " + entryDiagnostic, out diagnostic);
            if (entryScrollRect != null && entryScrollRect.content != entryRect)
                return Fail("Archive entry ScrollRect content must reference entry content.", out diagnostic);

            if (detailView == null) return Fail("Archive detail view is missing.", out diagnostic);
            if (!detailView.TryValidateWiring(out var detailDiagnostic))
                return Fail("Archive detail view: " + detailDiagnostic, out diagnostic);
            if (globalEmptyStateRoot == null) return Fail("Archive global empty state root is missing.", out diagnostic);
            if (categoryEmptyStateRoot == null) return Fail("Archive category empty state root is missing.", out diagnostic);
            if (contentStateRoot == null) return Fail("Archive content state root is missing.", out diagnostic);
            if (ReferenceEquals(globalEmptyStateRoot, categoryEmptyStateRoot) ||
                ReferenceEquals(globalEmptyStateRoot, contentStateRoot) ||
                ReferenceEquals(categoryEmptyStateRoot, contentStateRoot))
                return Fail("Archive empty and content state roots must be different objects.", out diagnostic);
            if (errorStateRoot != null &&
                (ReferenceEquals(errorStateRoot, globalEmptyStateRoot) ||
                 ReferenceEquals(errorStateRoot, categoryEmptyStateRoot) ||
                 ReferenceEquals(errorStateRoot, contentStateRoot)))
                return Fail("Archive error state root must be separate from other state roots.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Refresh()
        {
            if (!initialized || archiveService == null)
                return FailAndClear("Archive service has not been initialized.");
            if (!TryValidateWiring(out var wiringDiagnostic))
                return FailAndClear(wiringDiagnostic);

            IReadOnlyList<VNArchiveCategoryProjection> categories;
            try { categories = archiveService.GetCategories(); }
            catch (Exception exception)
            {
                return FailAndClear("Archive categories could not be queried: " + exception.Message);
            }

            if (categories == null)
                return FailAndClear("Archive service returned no category projections.");

            currentCategories = categories;
            if (categories.Count == 0)
            {
                selectedCategoryId = null;
                selectedArchiveId = null;
                currentEntries = null;
                DeactivateCategoryItems();
                DeactivateEntryItems();
                detailView.Clear();
                SetStateRoots(true, false, false, false, null);
                ResetCategoryScrollToTop();
                ResetEntryScrollToTop();
                LastDiagnostic = null;
                return true;
            }

            var selectedCategory = FindCategory(categories, selectedCategoryId) ?? categories[0];
            if (selectedCategory == null)
                return FailAndClear("Archive service returned a null category projection.");
            selectedCategoryId = selectedCategory.CategoryId;
            if (selectedCategoryId == null)
                return FailAndClear("Archive category projection has no category ID.");
            if (!BindCategoryItems(categories, selectedCategoryId))
                return FailAndClear(LastDiagnostic ?? "Archive categories could not be rendered.");
            if (!LoadSelectedCategoryEntries(true)) return false;

            ResetCategoryScrollToTop();
            ResetEntryScrollToTop();
            LastDiagnostic = null;
            return true;
        }
        public bool TrySelectCategory(string categoryId)
        {
            if (!initialized || currentCategories == null || string.IsNullOrWhiteSpace(categoryId)) return false;
            if (FindCategory(currentCategories, categoryId) == null) return false;
            if (string.Equals(selectedCategoryId, categoryId, StringComparison.Ordinal)) return true;

            selectedCategoryId = categoryId;
            selectedArchiveId = null;
            if (!BindCategoryItems(currentCategories, selectedCategoryId))
                return FailAndClear(LastDiagnostic ?? "Archive categories could not be rendered.");
            if (!LoadSelectedCategoryEntries(false)) return false;

            ResetEntryScrollToTop();
            LastDiagnostic = null;
            return true;
        }

        public bool TrySelectEntry(string archiveId)
        {
            if (!initialized || currentEntries == null || string.IsNullOrWhiteSpace(archiveId)) return false;
            var projection = FindEntry(currentEntries, archiveId);
            if (projection == null) return false;
            selectedArchiveId = projection.ArchiveId;

            if (!BindEntryItems(currentEntries, selectedArchiveId))
                return FailAndClear(LastDiagnostic ?? "Archive entries could not be rendered.");
            if (!detailView.Bind(projection, out var detailDiagnostic))
                return FailAndClear("Archive detail could not be rendered: " + detailDiagnostic);

            SetStateRoots(false, false, true, false, null);
            LastDiagnostic = null;
            return true;
        }

        private bool LoadSelectedCategoryEntries(bool preserveSelection)
        {
            var previousArchiveId = preserveSelection ? selectedArchiveId : null;
            IReadOnlyList<VNArchiveEntryProjection> entries;
            try { entries = archiveService.GetEntries(selectedCategoryId); }
            catch (Exception exception)
            {
                return FailAndClear("Archive entries could not be queried: " + exception.Message);
            }

            if (entries == null) return FailAndClear("Archive service returned no entry projections.");
            currentEntries = entries;
            if (entries.Count == 0)
            {
                selectedArchiveId = null;
                DeactivateEntryItems();
                detailView.Clear();
                SetStateRoots(false, true, false, false, null);
                LastDiagnostic = null;
                return true;
            }

            var selectedEntry = FindEntry(entries, previousArchiveId) ?? FirstUnlockedEntry(entries) ?? entries[0];
            if (selectedEntry == null || selectedEntry.ArchiveId == null)
                return FailAndClear("Archive service returned a null or unidentified entry projection.");
            selectedArchiveId = selectedEntry.ArchiveId;
            if (!BindEntryItems(entries, selectedArchiveId))
                return FailAndClear(LastDiagnostic ?? "Archive entries could not be rendered.");
            if (!detailView.Bind(selectedEntry, out var detailDiagnostic))
                return FailAndClear("Archive detail could not be rendered: " + detailDiagnostic);

            SetStateRoots(false, false, true, false, null);
            LastDiagnostic = null;
            return true;
        }

        private bool BindCategoryItems(IReadOnlyList<VNArchiveCategoryProjection> categories, string selectedId)
        {
            if (!EnsureCategoryPool(categories.Count))
                return Fail(LastDiagnostic ?? "Archive category item pool could not be created.");
            for (var index = 0; index < categoryItems.Count; index++)
            {
                var item = categoryItems[index];
                if (index >= categories.Count)
                {
                    item.Unbind();
                    if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
                    continue;
                }

                if (categories[index] == null) return Fail("Archive service returned a null category projection.");
                if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
                if (!item.Bind(categories[index],
                        string.Equals(categories[index].CategoryId, selectedId, StringComparison.Ordinal),
                        HandleCategorySelected, out var itemDiagnostic))
                    return Fail("Archive category item could not be bound: " + itemDiagnostic);
            }
            return true;
        }

        private bool BindEntryItems(IReadOnlyList<VNArchiveEntryProjection> entries, string selectedId)
        {
            if (!EnsureEntryPool(entries.Count))
                return Fail(LastDiagnostic ?? "Archive entry item pool could not be created.");
            for (var index = 0; index < entryItems.Count; index++)
            {
                var item = entryItems[index];
                if (index >= entries.Count)
                {
                    item.Unbind();
                    if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
                    continue;
                }

                if (entries[index] == null) return Fail("Archive service returned a null entry projection.");
                if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
                if (!item.Bind(entries[index],
                        string.Equals(entries[index].ArchiveId, selectedId, StringComparison.Ordinal),
                        HandleEntrySelected, out var itemDiagnostic))
                    return Fail("Archive entry item could not be bound: " + itemDiagnostic);
            }
            return true;
        }

        private bool EnsureCategoryPool(int count)
        {
            try
            {
                while (categoryItems.Count < count)
                {
                    var item = Instantiate(categoryItemPrefab, categoryContent, false);
                    if (item == null) return false;
                    item.gameObject.SetActive(false);
                    categoryItems.Add(item);
                }
                return true;
            }
            catch (Exception exception)
            {
                LastDiagnostic = "Archive category item pool failed: " + exception.Message;
                return false;
            }
        }

        private bool EnsureEntryPool(int count)
        {
            try
            {
                while (entryItems.Count < count)
                {
                    var item = Instantiate(entryItemPrefab, entryContent, false);
                    if (item == null) return false;
                    item.gameObject.SetActive(false);
                    entryItems.Add(item);
                }
                return true;
            }
            catch (Exception exception)
            {
                LastDiagnostic = "Archive entry item pool failed: " + exception.Message;
                return false;
            }
        }

        private void HandleCategorySelected(string id) => TrySelectCategory(id);
        private void HandleEntrySelected(string id) => TrySelectEntry(id);

        private bool FailAndClear(string diagnostic)
        {
            DeactivateCategoryItems();
            DeactivateEntryItems();
            currentCategories = null;
            currentEntries = null;
            if (detailView != null) detailView.Clear();
            SetStateRoots(errorStateRoot == null, false, false, errorStateRoot != null, diagnostic);
            ResetCategoryScrollToTop();
            ResetEntryScrollToTop();
            LastDiagnostic = diagnostic;
            return false;
        }

        private void DeactivateCategoryItems()
        {
            foreach (var item in categoryItems)
            {
                if (item == null) continue;
                item.Unbind();
                if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
            }
        }

        private void DeactivateEntryItems()
        {
            foreach (var item in entryItems)
            {
                if (item == null) continue;
                item.Unbind();
                if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
            }
        }

        private void ResetCategoryScrollToTop()
        {
            if (categoryScrollRect == null || categoryContent is not RectTransform rect ||
                categoryScrollRect.content != rect) return;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            categoryScrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
        }

        private void ResetEntryScrollToTop()
        {
            if (entryScrollRect == null || entryContent is not RectTransform rect ||
                entryScrollRect.content != rect) return;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            entryScrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
        }

        private void SetStateRoots(bool globalEmpty, bool categoryEmpty, bool content, bool error, string diagnostic)
        {
            if (globalEmptyStateRoot != null) globalEmptyStateRoot.SetActive(globalEmpty);
            if (categoryEmptyStateRoot != null) categoryEmptyStateRoot.SetActive(categoryEmpty);
            if (contentStateRoot != null) contentStateRoot.SetActive(content);
            if (errorStateRoot != null) errorStateRoot.SetActive(error);
            if (errorText != null) errorText.text = diagnostic ?? string.Empty;
        }

        private static VNArchiveCategoryProjection FindCategory(
            IReadOnlyList<VNArchiveCategoryProjection> values, string id)
        {
            if (values == null || string.IsNullOrEmpty(id)) return null;
            for (var index = 0; index < values.Count; index++)
                if (values[index] != null && string.Equals(values[index].CategoryId, id, StringComparison.Ordinal))
                    return values[index];
            return null;
        }

        private static VNArchiveEntryProjection FindEntry(IReadOnlyList<VNArchiveEntryProjection> values, string id)
        {
            if (values == null || string.IsNullOrEmpty(id)) return null;
            for (var index = 0; index < values.Count; index++)
                if (values[index] != null && string.Equals(values[index].ArchiveId, id, StringComparison.Ordinal))
                    return values[index];
            return null;
        }

        private static VNArchiveEntryProjection FirstUnlockedEntry(IReadOnlyList<VNArchiveEntryProjection> values)
        {
            for (var index = 0; index < values.Count; index++)
                if (values[index] != null && values[index].IsUnlocked) return values[index];
            return null;
        }

        private bool Fail(string message)
        {
            LastDiagnostic = message;
            return false;
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}