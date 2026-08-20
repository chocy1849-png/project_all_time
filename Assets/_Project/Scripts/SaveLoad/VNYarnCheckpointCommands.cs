using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Registers the single Yarn command that advances save eligibility:
    /// &lt;&lt;vn_checkpoint checkpoint_id&gt;&gt;.
    /// </summary>
    public sealed class VNYarnCheckpointCommands : MonoBehaviour
    {
        private static readonly string[] CommandNames = { "vn_checkpoint" };

        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNCheckpointService checkpointService;
        private bool handlersRegistered;

        private void OnEnable()
        {
            UnregisterHandlers();
            RegisterHandlers();
        }

        private void OnDisable() => UnregisterHandlers();

        private void RegisterHandlers()
        {
            if (dialogueRunner == null || checkpointService == null)
            {
                Debug.LogError("VNYarnCheckpointCommands requires Dialogue Runner and Checkpoint Service references.", this);
                return;
            }

            dialogueRunner.AddCommandHandler<string>("vn_checkpoint", Checkpoint);
            handlersRegistered = true;
        }

        private void UnregisterHandlers()
        {
            if (!handlersRegistered || dialogueRunner == null) return;
            foreach (var commandName in CommandNames) dialogueRunner.RemoveCommandHandler(commandName);
            handlersRegistered = false;
        }

        private void Checkpoint(string checkpointId)
        {
            if (!checkpointService.TryEnterCheckpoint(checkpointId, dialogueRunner, out var diagnostic))
                Debug.LogError($"VN checkpoint '{checkpointId}' was rejected: {diagnostic}", this);
        }
    }
}
