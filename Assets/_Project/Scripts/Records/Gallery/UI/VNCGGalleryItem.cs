using System;
using ProjectAllTime.VN.Records.Gallery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Gallery.UI
{
    /// <summary>Renders one already-projected Gallery entry without resolving its identity or artwork.</summary>
    [DisallowMultipleComponent]
    public sealed class VNCGGalleryItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private GameObject unlockedRoot;
        [SerializeField] private GameObject lockedRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject lockIndicator;

        private Action<Sprite, string> onUnlockedSelected;
        private Sprite boundSprite;
        private string boundTitle;
        private bool isUnlocked;

        private void OnEnable()
        {
            if (button == null) return;
            button.onClick.RemoveListener(HandleButtonClicked);
            button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(HandleButtonClicked);
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (button == null) return Fail("Gallery item Button is missing.", out diagnostic);
            if (thumbnailImage == null) return Fail("Gallery item thumbnail Image is missing.", out diagnostic);
            if (unlockedRoot == null) return Fail("Gallery item unlocked root is missing.", out diagnostic);
            if (lockedRoot == null) return Fail("Gallery item locked root is missing.", out diagnostic);
            if (titleText == null) return Fail("Gallery item title TMP_Text is missing.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Bind(
            VNCGGalleryEntryProjection projection,
            Action<Sprite, string> unlockedSelected,
            out string diagnostic)
        {
            Unbind();
            if (!TryValidateWiring(out diagnostic)) return false;
            if (projection == null) return Fail("Gallery item projection is missing.", out diagnostic);

            if (projection.IsUnlocked)
            {
                if (projection.Sprite == null)
                    return Fail("Unlocked Gallery projection has no Sprite.", out diagnostic);
                if (unlockedSelected == null)
                    return Fail("Unlocked Gallery item has no viewer action.", out diagnostic);

                isUnlocked = true;
                boundSprite = projection.Sprite;
                boundTitle = projection.DisplayTitle;
                onUnlockedSelected = unlockedSelected;
                unlockedRoot.SetActive(true);
                lockedRoot.SetActive(false);
                thumbnailImage.sprite = boundSprite;
                thumbnailImage.preserveAspect = true;
                thumbnailImage.enabled = true;
                SetTitle(boundTitle);
                button.interactable = true;
                if (lockIndicator != null) lockIndicator.SetActive(false);
                diagnostic = null;
                return true;
            }

            if (projection.Sprite != null)
                return Fail("Locked Gallery projection unexpectedly contains a Sprite.", out diagnostic);

            unlockedRoot.SetActive(false);
            lockedRoot.SetActive(true);
            thumbnailImage.sprite = null;
            thumbnailImage.enabled = false;
            thumbnailImage.preserveAspect = true;
            SetTitle(projection.DisplayTitle);
            button.interactable = false;
            if (lockIndicator != null) lockIndicator.SetActive(true);
            diagnostic = null;
            return true;
        }

        public void Unbind()
        {
            isUnlocked = false;
            boundSprite = null;
            boundTitle = null;
            onUnlockedSelected = null;

            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = null;
                thumbnailImage.enabled = false;
                thumbnailImage.preserveAspect = true;
            }
            if (titleText != null)
            {
                titleText.text = string.Empty;
                titleText.gameObject.SetActive(false);
            }
            if (unlockedRoot != null) unlockedRoot.SetActive(false);
            if (lockedRoot != null) lockedRoot.SetActive(true);
            if (button != null) button.interactable = false;
            if (lockIndicator != null) lockIndicator.SetActive(true);
        }

        private void SetTitle(string title)
        {
            titleText.text = title ?? string.Empty;
            titleText.gameObject.SetActive(title != null);
        }

        private void HandleButtonClicked()
        {
            if (!isUnlocked || boundSprite == null || onUnlockedSelected == null) return;
            onUnlockedSelected(boundSprite, boundTitle);
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}
