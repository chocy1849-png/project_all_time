using System;
using System.Collections.Generic;
using TMPro;
using ProjectAllTime.VN.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Concrete, deliberately small uGUI composition contract for M5. Its
    /// controller supplies all models and owns every save/load operation.
    /// </summary>
    public sealed class VNSaveLoadModal : MonoBehaviour
    {
        [SerializeField] private CanvasGroup modalCanvasGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button manualTabButton;
        [SerializeField] private Button autoTabButton;
        [SerializeField] private Button quickTabButton;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private VNSaveSlotItem slotItemPrefab;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text pageIndicatorText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject overwriteConfirmPanel;
        [SerializeField] private TMP_Text overwriteConfirmMessageText;
        [SerializeField] private Button overwriteConfirmButton;
        [SerializeField] private Button overwriteCancelButton;

        private readonly List<VNSaveSlotItem> slotItems = new();
        private VNSaveLoadController controller;
        private VNSaveRepository repository;
        private VNThumbnailService thumbnailService;
        private VNPresentationCatalog presentationCatalog;
        private bool isOpen;
        private bool thumbnailSuppressionActive;
        private float savedThumbnailAlpha;
        private bool savedThumbnailInteractable;
        private bool savedThumbnailBlocksRaycasts;

        public bool IsOpen => isOpen;
        public VNSaveLoadMode Mode { get; private set; }
        public VNSaveLoadCategory Category { get; private set; }
        public int Page { get; private set; }

        private void Awake()
        {
            closeButton?.onClick.AddListener(() => controller?.Close());
            manualTabButton?.onClick.AddListener(() => controller?.SetCategory(VNSaveLoadCategory.Manual));
            autoTabButton?.onClick.AddListener(() => controller?.SetCategory(VNSaveLoadCategory.Auto));
            quickTabButton?.onClick.AddListener(() => controller?.SetCategory(VNSaveLoadCategory.Quick));
            previousPageButton?.onClick.AddListener(() => controller?.ChangePage(-1));
            nextPageButton?.onClick.AddListener(() => controller?.ChangePage(1));
            overwriteConfirmButton?.onClick.AddListener(() => controller?.ConfirmManualOverwrite());
            overwriteCancelButton?.onClick.AddListener(() => controller?.CancelManualOverwrite());
            SetVisible(false);
            SetActive(overwriteConfirmPanel, false);
        }

        private void OnDestroy()
        {
            ReleaseRuntimeThumbnailTextures();
        }

        internal void Initialize(VNSaveLoadController saveLoadController, VNSaveRepository saveRepository, VNThumbnailService service, VNPresentationCatalog catalog)
        {
            controller = saveLoadController;
            repository = saveRepository;
            thumbnailService = service;
            presentationCatalog = catalog;
        }

        internal void Show(VNSaveLoadMode mode, VNSaveLoadCategory category, int page)
        {
            isOpen = true;
            Mode = mode;
            Category = category;
            Page = VNSaveLoadSlotModelBuilder.ClampPage(category, page);
            SetActive(overwriteConfirmPanel, false);
            SetVisible(true);
            SetText(titleText, mode == VNSaveLoadMode.Save ? "Save" : "Load");
            SetStatus(string.Empty);
        }

        internal void Hide()
        {
            isOpen = false;
            thumbnailSuppressionActive = false;
            SetActive(overwriteConfirmPanel, false);
            ReleaseRuntimeThumbnailTextures();
            SetVisible(false);
        }

        internal void SetNavigation(VNSaveLoadCategory category, int page)
        {
            Category = category;
            Page = VNSaveLoadSlotModelBuilder.ClampPage(category, page);
            SetActive(overwriteConfirmPanel, false);
        }

        internal void BindSlots(IReadOnlyList<VNSaveSlotViewModel> models)
        {
            var count = models?.Count ?? 0;
            EnsureSlotPool(Math.Max(VNSaveLoadSlotModelBuilder.ManualSlotsPerPage, count));
            for (var index = 0; index < slotItems.Count; index++)
            {
                var active = index < count;
                slotItems[index].gameObject.SetActive(active);
                if (!active)
                {
                    slotItems[index].ReleaseRuntimeThumbnail();
                    continue;
                }

                var model = models[index];
                Action<VNSaveSlotViewModel> clickHandler = controller == null ? null : controller.HandleSlotSelected;
                slotItems[index].Bind(model, controller != null && controller.IsSlotInteractive(model), clickHandler, presentationCatalog, thumbnailService, repository);
            }

            var pageCount = VNSaveLoadSlotModelBuilder.GetPageCount(Category);
            SetText(pageIndicatorText, "Page " + (Page + 1) + " / " + pageCount);
            if (previousPageButton != null) previousPageButton.interactable = Page > 0;
            if (nextPageButton != null) nextPageButton.interactable = Page < pageCount - 1;
        }

        internal void ShowOverwriteConfirmation(VNSaveSlotViewModel slot)
        {
            if (slot == null) return;
            SetText(overwriteConfirmMessageText, "Overwrite manual save " + slot.SlotLabel + "?");
            SetActive(overwriteConfirmPanel, true);
        }

        internal void HideOverwriteConfirmation() => SetActive(overwriteConfirmPanel, false);

        internal void SetStatus(string status) => SetText(statusText, status);

        /// <summary>
        /// CanvasGroup alpha hides modal visuals without deactivating the
        /// controller/coroutine. Keeping blocksRaycasts true retains a modal
        /// input shield while the invisible capture is pending.
        /// </summary>
        internal bool BeginThumbnailVisualSuppression()
        {
            if (!isOpen || modalCanvasGroup == null || thumbnailSuppressionActive) return false;
            thumbnailSuppressionActive = true;
            savedThumbnailAlpha = modalCanvasGroup.alpha;
            savedThumbnailInteractable = modalCanvasGroup.interactable;
            savedThumbnailBlocksRaycasts = modalCanvasGroup.blocksRaycasts;
            modalCanvasGroup.alpha = 0f;
            modalCanvasGroup.interactable = false;
            modalCanvasGroup.blocksRaycasts = true;
            return true;
        }

        internal void EndThumbnailVisualSuppression()
        {
            if (!thumbnailSuppressionActive || modalCanvasGroup == null) return;
            modalCanvasGroup.alpha = savedThumbnailAlpha;
            modalCanvasGroup.interactable = savedThumbnailInteractable;
            modalCanvasGroup.blocksRaycasts = savedThumbnailBlocksRaycasts;
            thumbnailSuppressionActive = false;
        }

        internal void ReleaseRuntimeThumbnailTextures()
        {
            foreach (var item in slotItems)
                if (item != null) item.ReleaseRuntimeThumbnail();
        }

        private void EnsureSlotPool(int requiredCount)
        {
            if (slotContainer == null || slotItemPrefab == null) return;
            while (slotItems.Count < requiredCount)
            {
                var item = Instantiate(slotItemPrefab, slotContainer);
                item.gameObject.SetActive(false);
                slotItems.Add(item);
            }
        }

        private void SetVisible(bool visible)
        {
            if (modalCanvasGroup == null) return;
            modalCanvasGroup.alpha = visible ? 1f : 0f;
            modalCanvasGroup.interactable = visible;
            modalCanvasGroup.blocksRaycasts = visible;
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null) target.text = value ?? string.Empty;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null) target.SetActive(active);
        }
    }
}
