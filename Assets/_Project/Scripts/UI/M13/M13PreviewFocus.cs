using UnityEngine;
using UnityEngine.EventSystems;

// Preview-only focus affordance; production input ownership remains unchanged.
public sealed class M13PreviewFocus : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject indicator;
    public void Configure(GameObject value) { indicator = value; indicator.SetActive(false); }
    public void OnSelect(BaseEventData eventData) { if (indicator) indicator.SetActive(true); }
    public void OnDeselect(BaseEventData eventData) { if (indicator) indicator.SetActive(false); }
    public void OnPointerEnter(PointerEventData eventData) { if (indicator) indicator.SetActive(true); }
    public void OnPointerExit(PointerEventData eventData) { if (indicator && (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != gameObject)) indicator.SetActive(false); }
}
