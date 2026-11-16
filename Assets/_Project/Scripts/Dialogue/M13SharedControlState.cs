using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Visual state only; the owning controller decides whether this item is active.</summary>
    public sealed class M13SharedControlState : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private GameObject focusIndicator;
        [SerializeField] private GameObject activeIndicator;
        private bool focused;
        private bool active;
        public void SetActive(bool value) { active = value; Refresh(); }
        public void OnSelect(BaseEventData eventData) { focused = true; Refresh(); }
        public void OnDeselect(BaseEventData eventData) { focused = false; Refresh(); }
        private void OnDisable() { focused = false; Refresh(); }
        private void Refresh()
        {
            var selectable = GetComponent<UnityEngine.UI.Selectable>();
            bool enabledControl = selectable == null || selectable.IsInteractable();
            if (focusIndicator != null) focusIndicator.SetActive(focused && enabledControl);
            if (activeIndicator != null) activeIndicator.SetActive(active);
        }
        public void Configure(GameObject focus, GameObject selected) { focusIndicator = focus; activeIndicator = selected; Refresh(); }
    }
}
