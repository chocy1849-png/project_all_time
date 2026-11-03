using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Scene-owned binding for an authored, transient manual-only line.</summary>
    [DisallowMultipleComponent]
    public sealed class VNYarnManualHoldCommands : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNDialogueSessionState sessionState;
        private bool registered;

        private void OnEnable()
        {
            if (registered) return;
            if (dialogueRunner == null || sessionState == null)
            {
                Debug.LogError("VNYarnManualHoldCommands requires a production runner and dialogue session.", this);
                return;
            }
            dialogueRunner.AddCommandHandler("vn_manual_hold", Hold);
            registered = true;
        }

        private void OnDisable()
        {
            if (registered && dialogueRunner != null) dialogueRunner.RemoveCommandHandler("vn_manual_hold");
            registered = false;
            if (sessionState != null) sessionState.AcceptManualConsume();
        }

        private void Hold()
        {
            if (sessionState == null) { Debug.LogError("vn_manual_hold requires a dialogue session.", this); return; }
            sessionState.RequireManualAdvance();
        }
    }
}
