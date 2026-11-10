using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Shows the marker only when the current line can be advanced.</summary>
    public sealed class VNContinueIndicator : MonoBehaviour
    {
        [SerializeField] private Image marker;
        private VNDialogueSessionState session;
        private VNInteractionGate gate;
        private void Awake()
        {
            session = FindFirstObjectByType<VNDialogueSessionState>();
            gate = FindFirstObjectByType<VNInteractionGate>();
        }
        private void LateUpdate()
        {
            if (marker == null) return;
            var visible = session != null && gate != null && session.IsLineActive &&
                session.IsCurrentLineFullyDisplayed && !session.IsCurrentLineConsumed && !session.OptionsActive &&
                gate.CanAdvanceStory;
            if (marker.gameObject.activeSelf != visible) marker.gameObject.SetActive(visible);
            marker.enabled = visible;
        }
    }
}
