using System;
using System.Collections.Generic;
using ProjectAllTime.VN.Records.Gallery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Gallery.UI
{
    /// <summary>Renders fresh spoiler-safe projections supplied by VNCGGalleryService.</summary>
    [DisallowMultipleComponent]
    public sealed class VNCGGalleryView : MonoBehaviour
    {
        [SerializeField] private ScrollRect galleryScrollRect;
        [SerializeField] private Transform itemContent;
        [SerializeField] private VNCGGalleryItem itemPrefab;
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private GameObject contentStateRoot;
        [SerializeField] private VNCGGalleryViewer fullscreenViewer;
        [SerializeField] private GameObject errorStateRoot;
        [SerializeField] private TMP_Text errorText;

        private readonly List<VNCGGalleryItem> items = new();
        private VNCGGalleryService galleryService;
        private bool initialized;

        public string LastDiagnostic { get; private set; }

        private void Awake()
        {
            SetStateRoots(empty: false, content: false, error: false, diagnostic: null);
            if (fullscreenViewer != null) fullscreenViewer.Close();
        }

        private void OnEnable()
        {
            if (!initialized) return;
            if (fullscreenViewer != null) fullscreenViewer.Close();
            Refresh();
        }

        private void OnDisable()
        {
            if (fullscreenViewer != null) fullscreenViewer.Close();
        }

        public bool Initialize(VNCGGalleryService service)
        {
            if (service == null)
            {
                LastDiagnostic = "A VNCGGalleryService is required.";
                return false;
            }

            if (initialized)
            {
                if (!ReferenceEquals(galleryService, service))
                {
                    LastDiagnostic = "VNCGGalleryView is already initialized with a different service.";
                    return false;
                }

                LastDiagnostic = null;
                return !isActiveAndEnabled || Refresh();
            }

            galleryService = service;
            initialized = true;
            LastDiagnostic = null;
            return !isActiveAndEnabled || Refresh();
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (galleryScrollRect == null) return Fail("Gallery ScrollRect is missing.", out diagnostic);
            if (itemContent == null) return Fail("Gallery item content Transform is missing.", out diagnostic);
            if (itemContent is not RectTransform contentRect)
                return Fail("Gallery item content must have a RectTransform.", out diagnostic);
            if (galleryScrollRect.content != contentRect)
                return Fail("Gallery ScrollRect content must reference the item content RectTransform.", out diagnostic);
            if (itemPrefab == null) return Fail("Gallery item prefab is missing.", out diagnostic);
            if (!itemPrefab.TryValidateWiring(out var itemDiagnostic))
                return Fail("Gallery item prefab: " + itemDiagnostic, out diagnostic);
            if (emptyStateRoot == null) return Fail("Gallery empty state root is missing.", out diagnostic);
            if (contentStateRoot == null) return Fail("Gallery content state root is missing.", out diagnostic);
            if (ReferenceEquals(emptyStateRoot, contentStateRoot))
                return Fail("Gallery empty and content state roots must be different objects.", out diagnostic);
            if (errorStateRoot != null &&
                (ReferenceEquals(errorStateRoot, emptyStateRoot) || ReferenceEquals(errorStateRoot, contentStateRoot)))
                return Fail("Gallery error state root must be separate from the empty and content roots.", out diagnostic);
            if (fullscreenViewer == null) return Fail("Gallery fullscreen viewer is missing.", out diagnostic);
            if (!fullscreenViewer.TryValidateWiring(out var viewerDiagnostic))
                return Fail("Gallery fullscreen viewer: " + viewerDiagnostic, out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Refresh()
        {
            if (!initialized || galleryService == null)
                return FailAndClear("Gallery service has not been initialized.");
            if (!TryValidateWiring(out var wiringDiagnostic))
                return FailAndClear(wiringDiagnostic);

            fullscreenViewer.Close();

            IReadOnlyList<VNCGGalleryEntryProjection> entries;
            string serviceDiagnostic;
            try
            {
                if (!galleryService.TryGetGalleryEntries(out entries, out serviceDiagnostic))
                    return FailAndClear(serviceDiagnostic ?? "Gallery service could not provide entries.");
            }
            catch (Exception exception)
            {
                return FailAndClear("Gallery service query failed: " + exception.Message);
            }

            if (entries == null)
                return FailAndClear("Gallery service returned no projection list.");

            if (entries.Count == 0)
            {
                DeactivateAllItems();
                SetStateRoots(empty: true, content: false, error: false, diagnostic: null);
                ResetScrollToTop();
                LastDiagnostic = null;
                return true;
            }

            if (!EnsurePool(entries.Count))
                return FailAndClear("Gallery item pool could not be created.");

            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];
                if (index >= entries.Count)
                {
                    item.Unbind();
                    if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
                    continue;
                }

                if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
                if (!item.Bind(entries[index], HandleUnlockedItemSelected, out var itemDiagnostic))
                    return FailAndClear("Gallery item could not be bound: " + itemDiagnostic);
            }

            SetStateRoots(empty: false, content: true, error: false, diagnostic: null);
            ResetScrollToTop();
            LastDiagnostic = null;
            return true;
        }

        private bool EnsurePool(int requiredCount)
        {
            while (items.Count < requiredCount)
            {
                var item = Instantiate(itemPrefab, itemContent, false);
                if (item == null) return false;
                items.Add(item);
            }

            return true;
        }

        private bool FailAndClear(string diagnostic)
        {
            DeactivateAllItems();
            if (fullscreenViewer != null) fullscreenViewer.Close();
            SetStateRoots(empty: errorStateRoot == null, content: false, error: errorStateRoot != null,
                diagnostic: diagnostic);
            ResetScrollToTop();
            LastDiagnostic = diagnostic;
            return false;
        }

        private void DeactivateAllItems()
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                item.Unbind();
                if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
            }
        }

        private void HandleUnlockedItemSelected(Sprite sprite, string displayTitle)
        {
            if (fullscreenViewer != null) fullscreenViewer.TryOpen(sprite, displayTitle);
        }

        private void ResetScrollToTop()
        {
            if (itemContent is not RectTransform contentRect ||
                galleryScrollRect == null ||
                galleryScrollRect.content != contentRect ||
                (galleryScrollRect.viewport == null && galleryScrollRect.transform is not RectTransform))
                return;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            galleryScrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
        }

        private void SetStateRoots(bool empty, bool content, bool error, string diagnostic)
        {
            if (emptyStateRoot != null) emptyStateRoot.SetActive(empty);
            if (contentStateRoot != null) contentStateRoot.SetActive(content);
            if (errorStateRoot != null) errorStateRoot.SetActive(error);
            if (errorText != null) errorText.text = diagnostic ?? string.Empty;
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}
