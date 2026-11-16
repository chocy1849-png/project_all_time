using UnityEngine;
using UnityEngine.UI;

// Selection is confined to the disposable visual fixture; no save or records action is invoked.
[RequireComponent(typeof(Button))]
public sealed class M13PreviewCard : MonoBehaviour
{
    [SerializeField] private GameObject selectedMarker;
    public void Configure(GameObject marker, bool selected)
    {
        selectedMarker = marker;
        SetSelected(selected);
    }
    private void OnEnable() => GetComponent<Button>().onClick.AddListener(Select);
    private void OnDisable() => GetComponent<Button>().onClick.RemoveListener(Select);
    private void Select()
    {
        foreach (var card in transform.parent.GetComponentsInChildren<M13PreviewCard>(true)) card.SetSelected(card == this);
    }
    private void SetSelected(bool selected) { if (selectedMarker) selectedMarker.SetActive(selected); }
}
