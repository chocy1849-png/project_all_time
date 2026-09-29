using UnityEngine;
using Yarn.Unity;
using ProjectAllTime.VN.Presentation;

namespace ProjectAllTime.VN.Records.Replay
{
    /// <summary>Replay-only visual command owner bound to one disposable presentation controller.</summary>
    public sealed class VNReplayPresentationCommands : MonoBehaviour
    {
        private static readonly string[] CommandNames =
        {
            "vn_bg", "vn_show", "vn_expression", "vn_move", "vn_facing", "vn_scale", "vn_hide", "vn_cg", "vn_clear_cg",
        };

        private DialogueRunner dialogueRunner;
        private VNPresentationController presentationController;
        private bool handlersRegistered;

        public DialogueRunner RegisteredRunner => handlersRegistered ? dialogueRunner : null;

        public void Bind(DialogueRunner runner, VNPresentationController controller)
        {
            if (isActiveAndEnabled) throw new System.InvalidOperationException("Replay presentation commands must be bound before activation.");
            dialogueRunner = runner;
            presentationController = controller;
        }

        private void OnEnable()
        {
            UnregisterHandlers();
            if (dialogueRunner == null || presentationController == null)
            {
                Debug.LogError("VNReplayPresentationCommands requires its Replay runner and presentation controller.", this);
                return;
            }

            dialogueRunner.AddCommandHandler<string>("vn_bg", Background);
            dialogueRunner.AddCommandHandler<string, string, string>("vn_show", Show);
            dialogueRunner.AddCommandHandler<string, string>("vn_expression", Expression);
            dialogueRunner.AddCommandHandler<string, string>("vn_move", Move);
            dialogueRunner.AddCommandHandler<string, string>("vn_facing", Facing);
            dialogueRunner.AddCommandHandler<string, float>("vn_scale", Scale);
            dialogueRunner.AddCommandHandler<string>("vn_hide", Hide);
            dialogueRunner.AddCommandHandler<string>("vn_cg", CG);
            dialogueRunner.AddCommandHandler("vn_clear_cg", presentationController.ClearCG);
            handlersRegistered = true;
        }

        private void OnDisable() => UnregisterHandlers();

        private void UnregisterHandlers()
        {
            if (!handlersRegistered || dialogueRunner == null) return;
            foreach (var commandName in CommandNames) dialogueRunner.RemoveCommandHandler(commandName);
            handlersRegistered = false;
        }

        private void Background(string backgroundId) => presentationController.SetBackground(backgroundId);

        private void Show(string characterId, string expressionId, string slotId)
        {
            if (VNPresentationController.TryParseSlot(slotId, out var slot)) presentationController.ShowCharacter(characterId, expressionId, slot);
            else Debug.LogError($"Unknown VN character slot '{slotId}'.", this);
        }

        private void Expression(string characterId, string expressionId) => presentationController.SetExpression(characterId, expressionId);

        private void Move(string characterId, string slotId)
        {
            if (VNPresentationController.TryParseSlot(slotId, out var slot)) presentationController.MoveCharacter(characterId, slot);
            else Debug.LogError($"Unknown VN character slot '{slotId}'.", this);
        }

        private void Facing(string characterId, string facingId)
        {
            if (VNPresentationController.TryParseFacing(facingId, out var facing)) presentationController.SetFacing(characterId, facing);
            else Debug.LogError($"Unknown VN character facing '{facingId}'.", this);
        }

        private void Scale(string characterId, float scale) => presentationController.SetScale(characterId, scale);
        private void Hide(string characterId) => presentationController.HideCharacter(characterId);
        private void CG(string cgId) => presentationController.SetCG(cgId);
    }
}
