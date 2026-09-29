using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Gallery.UI
{
    /// <summary>Displays unlocked Gallery content inside the Records Gallery root.</summary>
    [DisallowMultipleComponent]
    public sealed class VNCGGalleryViewer : MonoBehaviour
    {
        [SerializeField] private CanvasGroup viewerCanvasGroup;
        [SerializeField] private Image fullImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text titleText;

        private void Awake()
        {
            Close();
            if (fullImage != null) fullImage.preserveAspect = true;
        }

        private void OnEnable()
        {
            if (closeButton == null) return;
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(HandleCloseClicked);
            Close();
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (viewerCanvasGroup == null) return Fail("Viewer CanvasGroup is missing.", out diagnostic);
            if (fullImage == null) return Fail("Viewer full Image is missing.", out diagnostic);
            if (closeButton == null) return Fail("Viewer Close button is missing.", out diagnostic);
            if (titleText == null) return Fail("Viewer title TMP_Text is missing.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool TryOpen(Sprite sprite, string displayTitle)
        {
            if (sprite == null || !TryValidateWiring(out _))
            {
                Close();
                return false;
            }

            fullImage.preserveAspect = true;
            fullImage.sprite = sprite;
            fullImage.enabled = true;
            titleText.text = displayTitle ?? string.Empty;
            viewerCanvasGroup.alpha = 1f;
            viewerCanvasGroup.interactable = true;
            viewerCanvasGroup.blocksRaycasts = true;
            return true;
        }

        public void Close()
        {
            if (viewerCanvasGroup != null)
            {
                viewerCanvasGroup.alpha = 0f;
                viewerCanvasGroup.interactable = false;
                viewerCanvasGroup.blocksRaycasts = false;
            }

            if (fullImage != null)
            {
                fullImage.sprite = null;
                fullImage.enabled = false;
                fullImage.preserveAspect = true;
            }

            if (titleText != null) titleText.text = string.Empty;
        }

        private void HandleCloseClicked() => Close();

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}
