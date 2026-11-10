using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Visual-only tooltip; action ownership stays with VNQuickControlBar.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class VNIconTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private string label;
        [SerializeField] private RectTransform tooltip;
        [SerializeField] private TMP_Text tooltipText;
        [SerializeField] private Image icon;
        [SerializeField] private VNUITheme theme;
        private Button button;
        private VNInteractionGate gate;
        private readonly Vector3[] corners = new Vector3[4];
        private bool hovered;
        private bool selected;

        private void Awake() { button = GetComponent<Button>(); gate = FindFirstObjectByType<VNInteractionGate>(); Hide(); }
        private void OnDisable() { hovered = false; selected = false; Hide(); }
        private void Update()
        {
            if (icon != null && theme != null)
                icon.color = !button.interactable ? theme.disabled : (hovered || selected ? theme.hover : theme.text);
            if (tooltip != null && tooltip.gameObject.activeSelf &&
                (!button.interactable || !button.gameObject.activeInHierarchy ||
                 (gate != null && (gate.IsBlockingModalActive || gate.IsLoadInProgress || gate.IsUiHidden)))) Hide();
        }
        public void OnPointerEnter(PointerEventData eventData) { hovered = true; Show(); }
        public void OnPointerExit(PointerEventData eventData) { hovered = false; Hide(); }
        public void OnSelect(BaseEventData eventData) { selected = true; Show(); }
        public void OnDeselect(BaseEventData eventData) { selected = false; Hide(); }
        private void Show()
        {
            if (tooltip == null || tooltipText == null || button == null || !button.interactable ||
                (gate != null && (gate.IsBlockingModalActive || gate.IsLoadInProgress || gate.IsUiHidden))) return;
            tooltipText.text = label;
            tooltip.anchoredPosition = new Vector2(0, 8);
            tooltip.gameObject.SetActive(true);
            var canvas = tooltip.GetComponentInParent<Canvas>();
            if (canvas == null) return;
            tooltip.GetWorldCorners(corners);
            var rect = canvas.transform as RectTransform;
            if (rect == null) return;
            var low = rect.InverseTransformPoint(corners[0]);
            var high = rect.InverseTransformPoint(corners[2]);
            var shift = Vector3.zero;
            if (low.x < rect.rect.xMin) shift.x = rect.rect.xMin - low.x;
            if (high.x > rect.rect.xMax) shift.x = rect.rect.xMax - high.x;
            if (high.y > rect.rect.yMax) shift.y = rect.rect.yMax - high.y;
            if (low.y < rect.rect.yMin) shift.y = rect.rect.yMin - low.y;
            tooltip.position += rect.TransformVector(shift);
        }
        private void Hide() { if (tooltip != null) tooltip.gameObject.SetActive(false); }
    }
}
