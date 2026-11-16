using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Preview-only focus affordance; production input ownership remains unchanged.
public sealed class M13PreviewFocus : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject indicator;
    private bool CanFocus => TryGetComponent<Selectable>(out var control) && control.IsInteractable();
    public void Configure(GameObject value) { indicator = value; indicator.SetActive(false); }
    public void OnSelect(BaseEventData eventData) { if (indicator) indicator.SetActive(CanFocus); }
    public void OnDeselect(BaseEventData eventData) { if (indicator) indicator.SetActive(false); }
    public void OnPointerEnter(PointerEventData eventData) { if (indicator) indicator.SetActive(CanFocus); }
    public void OnPointerExit(PointerEventData eventData) { if (indicator && (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != gameObject)) indicator.SetActive(false); }
}
