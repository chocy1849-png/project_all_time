using System;
using TMPro;
using ProjectAllTime.VN.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Reusable uGUI slot-card view. It receives already inspected models and
    /// never resolves storage paths or reads a save file itself.
    /// </summary>
    public sealed class VNSaveSlotItem : MonoBehaviour
    {
        [SerializeField] private Button rootButton;
        [SerializeField] private RawImage thumbnailRawImage;
        [SerializeField] private GameObject thumbnailPlaceholderVisual;
        [SerializeField] private TMP_Text slotLabelText;
        [SerializeField] private GameObject normalMetadataRoot;
        [SerializeField] private TMP_Text chapterText;
        [SerializeField] private TMP_Text sceneTitleText;
        [SerializeField] private TMP_Text savedAtText;
        [SerializeField] private TMP_Text playedTimeText;
        [SerializeField] private GameObject characterIconsRoot;
        [SerializeField] private Image[] characterIconImages = new Image[5];
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private TMP_Text emptyStateText;
        [SerializeField] private GameObject corruptedStateRoot;
        [SerializeField] private TMP_Text corruptedStateText;
        [SerializeField] private GameObject unsupportedStateRoot;
        [SerializeField] private TMP_Text unsupportedStateText;

        private Texture2D ownedRuntimeThumbnail;
        private VNSaveSlotViewModel boundModel;
        private Action<VNSaveSlotViewModel> clickCallback;

        public VNSaveSlotViewModel BoundModel => boundModel;

        private void Awake()
        {
            if (rootButton != null) rootButton.onClick.AddListener(InvokeClick);
        }

        private void OnDestroy()
        {
            if (rootButton != null) rootButton.onClick.RemoveListener(InvokeClick);
            ReleaseRuntimeThumbnail();
        }

        public void Bind(
            VNSaveSlotViewModel model,
            bool isInteractive,
            Action<VNSaveSlotViewModel> onClick,
            VNPresentationCatalog presentationCatalog,
            VNThumbnailService thumbnailService,
            VNSaveRepository repository)
        {
            ReleaseRuntimeThumbnail();
            boundModel = model;
            clickCallback = onClick;
            if (model == null)
            {
                if (rootButton != null) rootButton.interactable = false;
                return;
            }

            if (rootButton != null) rootButton.interactable = isInteractive;
            SetText(slotLabelText, model.SlotLabel);
            SetText(chapterText, model.ChapterText);
            SetText(sceneTitleText, model.SceneTitleText);
            SetText(savedAtText, model.SavedAtText);
            SetText(playedTimeText, model.PlayedTimeText);

            var isValid = model.State == VNSaveSlotState.Valid;
            SetActive(normalMetadataRoot, isValid);
            SetActive(emptyStateRoot, model.State == VNSaveSlotState.Empty);
            SetActive(corruptedStateRoot, model.State == VNSaveSlotState.Corrupted || model.State == VNSaveSlotState.InvalidRequest);
            SetActive(unsupportedStateRoot, model.State == VNSaveSlotState.Unsupported);
            SetText(emptyStateText, "Empty slot");
            SetText(corruptedStateText, "Corrupted save");
            SetText(unsupportedStateText, "Unsupported save");
            BindCharacterIcons(model, presentationCatalog);
            BindThumbnail(model, thumbnailService, repository);
        }

        public void ReleaseRuntimeThumbnail()
        {
            VNThumbnailService.ReleaseRuntimeTexture(ownedRuntimeThumbnail);
            ownedRuntimeThumbnail = null;
            if (thumbnailRawImage != null) thumbnailRawImage.texture = null;
            SetActive(thumbnailPlaceholderVisual, true);
        }

        private void BindThumbnail(VNSaveSlotViewModel model, VNThumbnailService thumbnailService, VNSaveRepository repository)
        {
            if (model.State != VNSaveSlotState.Valid || thumbnailService == null || repository == null) return;
            var thumbnail = thumbnailService.LoadThumbnail(repository, model.SlotKey, model.ThumbnailFileName);
            if (thumbnail.Status != VNThumbnailLoadStatus.Loaded || thumbnail.Texture == null) return;

            ownedRuntimeThumbnail = thumbnail.Texture;
            if (thumbnailRawImage != null) thumbnailRawImage.texture = ownedRuntimeThumbnail;
            SetActive(thumbnailPlaceholderVisual, false);
        }

        private void BindCharacterIcons(VNSaveSlotViewModel model, VNPresentationCatalog presentationCatalog)
        {
            if (characterIconImages == null)
            {
                SetActive(characterIconsRoot, false);
                return;
            }

            var hasIcon = false;
            for (var index = 0; index < characterIconImages.Length; index++)
            {
                var iconImage = characterIconImages[index];
                if (iconImage == null) continue;
                Sprite icon = null;
                if (model != null && index < model.VisibleCharacterIds.Count && presentationCatalog != null &&
                    presentationCatalog.TryGetCharacter(model.VisibleCharacterIds[index], out var character))
                    icon = character.SaveIcon;

                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(icon != null);
                hasIcon |= icon != null;
            }

            SetActive(characterIconsRoot, hasIcon);
        }

        private void InvokeClick()
        {
            if (boundModel != null) clickCallback?.Invoke(boundModel);
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null) text.text = value ?? string.Empty;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null) target.SetActive(active);
        }
    }
}
