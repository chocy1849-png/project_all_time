using System.Collections;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>Tracks Yarn's standard scaled-time wait so Load can cancel its continuation safely.</summary>
    public sealed class VNYarnTrackedWaitCommands : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;
        private bool registered;

        private void OnEnable()
        {
            if (dialogueRunner == null) { Debug.LogError("Tracked wait requires a DialogueRunner.", this); return; }
            dialogueRunner.RemoveCommandHandler("wait");
            dialogueRunner.AddCommandHandler<float>("wait", Wait);
            registered = true;
        }

        private void OnDisable()
        {
            if (!registered || dialogueRunner == null) return;
            dialogueRunner.RemoveCommandHandler("wait");
            dialogueRunner.AddCommandHandler<float>("wait", DefaultWait);
            registered = false;
        }

        private IEnumerator Wait(float seconds) => VNAsyncCommandRegistry.Track(dialogueRunner, WaitRoutine(seconds));
        private static IEnumerator WaitRoutine(float seconds)
        {
            // WaitForSeconds starts at the end of the issuing frame, even for
            // zero. Poll scaled time so a stopped runner can unwind promptly.
            yield return null;
            var deadline = Time.time + Mathf.Max(0f, seconds);
            while (Time.time < deadline) yield return null;
        }
        private static IEnumerator DefaultWait(float seconds) { yield return new WaitForSeconds(seconds); }
    }
}
