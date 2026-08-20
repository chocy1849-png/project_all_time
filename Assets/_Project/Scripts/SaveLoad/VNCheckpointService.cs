using System;
using Yarn.Unity;
using UnityEngine;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Holds only the current validated checkpoint context. It does not start
    /// dialogue, infer a current Yarn node, or initiate saving.
    /// </summary>
    public sealed class VNCheckpointService : MonoBehaviour
    {
        [SerializeField] private VNCheckpointCatalog checkpointCatalog;

        private bool hasCurrentCheckpoint;
        private VNCheckpointContext currentCheckpoint;

        public bool HasCurrentCheckpoint => hasCurrentCheckpoint;

        /// <summary>
        /// Raised only after an explicit Yarn checkpoint command has been
        /// catalog-validated and adopted. Disk I/O remains application-layer
        /// work; this event merely describes successful checkpoint entry.
        /// </summary>
        public event Action<VNCheckpointContext> CheckpointEntered;

        public bool TryGetCurrentCheckpoint(out VNCheckpointContext context)
        {
            context = currentCheckpoint;
            return hasCurrentCheckpoint;
        }

        public bool TryEnterCheckpoint(string checkpointId, DialogueRunner dialogueRunner, out string diagnostic)
        {
            if (!TryResolveForRunner(checkpointId, dialogueRunner, out var context, out diagnostic)) return false;
            currentCheckpoint = context;
            hasCurrentCheckpoint = true;
            CheckpointEntered?.Invoke(context);
            return true;
        }

        /// <summary>
        /// Performs all catalog and exact-resume checks required before a save
        /// load crosses its dialogue-stop mutation boundary.
        /// </summary>
        public bool TryValidateSavedCheckpoint(SaveSlotData saveData, DialogueRunner dialogueRunner, out VNCheckpointContext context, out string diagnostic)
        {
            context = default;
            if (saveData == null)
            {
                diagnostic = "Save data is missing.";
                return false;
            }

            if (!TryResolveForRunner(saveData.checkpointId, dialogueRunner, out context, out diagnostic)) return false;
            if (saveData.resumeNode != context.ResumeNode)
            {
                diagnostic = $"Saved resume node '{saveData.resumeNode}' does not match checkpoint '{context.CheckpointId}'.";
                context = default;
                return false;
            }

            diagnostic = null;
            return true;
        }

        internal void AdoptValidatedContext(VNCheckpointContext context)
        {
            currentCheckpoint = context;
            hasCurrentCheckpoint = true;
        }

        private bool TryResolveForRunner(string checkpointId, DialogueRunner dialogueRunner, out VNCheckpointContext context, out string diagnostic)
        {
            context = default;
            if (checkpointCatalog == null)
            {
                diagnostic = "VNCheckpointService requires a checkpoint catalog.";
                return false;
            }

            if (dialogueRunner == null)
            {
                diagnostic = "VNCheckpointService requires a Dialogue Runner.";
                return false;
            }

            if (!checkpointCatalog.TryValidate(dialogueRunner.YarnProject, out diagnostic)) return false;
            if (!checkpointCatalog.TryResolve(checkpointId, out context, out diagnostic)) return false;

            // TryValidate checks every catalog entry. Keep the context-specific
            // check explicit so this call remains correct if catalog policy is
            // later narrowed to lazy validation.
            return checkpointCatalog.TryValidateContextResumeNode(context, dialogueRunner.YarnProject, out diagnostic);
        }
    }
}
