using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.SaveLoad;
using TMPro;
using UnityEngine;

namespace ProjectAllTime.VN.Presentation
{
    /// <summary>Passive scene-session authored facts. Re-entry rebuilds content; no history or save state.</summary>
    [DisallowMultipleComponent]
    public sealed class VNInformationOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private VNUIVisibilityController visibility;
        [SerializeField] private VNSaveLoadController saveLoad;

        public bool IsShown { get; private set; }

        public bool Show(string title, string facts)
        {
            if (!isActiveAndEnabled || root == null || titleText == null || bodyText == null ||
                visibility == null || saveLoad == null || saveLoad.IsLoadInProgress) return false;
            titleText.text = title ?? string.Empty;
            bodyText.text = facts ?? string.Empty;
            IsShown = true;
            ApplyVisibility();
            return true;
        }

        public void Hide()
        {
            IsShown = false;
            if (titleText != null) titleText.text = string.Empty;
            if (bodyText != null) bodyText.text = string.Empty;
            ApplyVisibility();
        }

        private void OnEnable()
        {
            if (visibility != null) visibility.UiVisibilityChanged += HandleVisibility;
            if (saveLoad != null) saveLoad.LoadStateChanged += HandleLoad;
            Hide();
        }

        private void OnDisable()
        {
            if (visibility != null) visibility.UiVisibilityChanged -= HandleVisibility;
            if (saveLoad != null) saveLoad.LoadStateChanged -= HandleLoad;
            Hide();
        }

        private void HandleLoad(bool loading) { if (loading) Hide(); }
        private void HandleVisibility(bool hidden) => ApplyVisibility();

        private void ApplyVisibility()
        {
            if (root == null) return;
            root.alpha = IsShown && visibility != null && !visibility.IsUiHidden ? 1f : 0f;
            root.interactable = false;
            root.blocksRaycasts = false;
        }
    }
}
