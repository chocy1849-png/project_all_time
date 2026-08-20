using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>CanvasGroup settings shell reserved for M7 controls.</summary>
    [DisallowMultipleComponent]
    public sealed class VNSettingsModal : MonoBehaviour
    {
        [SerializeField] private CanvasGroup modalCanvasGroup;
        [SerializeField] private Button closeButton;

        private bool isOpen;
        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (modalCanvasGroup != null) SetVisible(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public bool TryOpen()
        {
            if (modalCanvasGroup == null)
            {
                Debug.LogError($"{nameof(VNSettingsModal)} requires a CanvasGroup reference.", this);
                return false;
            }

            SetVisible(true);
            isOpen = true;
            return true;
        }

        public bool Close()
        {
            if (!isOpen) return true;
            if (modalCanvasGroup == null) return false;
            SetVisible(false);
            isOpen = false;
            return true;
        }

        private void SetVisible(bool visible)
        {
            modalCanvasGroup.alpha = visible ? 1f : 0f;
            modalCanvasGroup.interactable = visible;
            modalCanvasGroup.blocksRaycasts = visible;
        }

        private void HandleCloseClicked() => Close();
    }
}
