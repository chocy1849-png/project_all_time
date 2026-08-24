using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Future Save/Load button PointerDown relay; M5 owns suppression implementation.</summary>
    [DisallowMultipleComponent]
    public sealed class VNSaveLoadPointerDownSuppressor : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private VNConvenienceController convenienceController;

        public void OnPointerDown(PointerEventData eventData)
        {
            convenienceController?.BeginSaveLoadOpenerInputSuppression();
        }
    }
}
