using System.Collections;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Presentation
{
    public sealed class VNYarnTransitionCommands : MonoBehaviour
    {
        private static readonly string[] CommandNames = { "vn_fade_to_black", "vn_fade_from_black", "vn_bg_crossfade", "vn_show_fade", "vn_hide_fade", "vn_cg_fade_in", "vn_cg_fade_out" };

        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNTransitionController transitionController;
        private bool handlersRegistered;

        private void OnEnable()
        {
            UnregisterHandlers();
            RegisterHandlers();
        }

        private void OnDisable() => UnregisterHandlers();

        private void RegisterHandlers()
        {
            if (dialogueRunner == null || transitionController == null)
            {
                Debug.LogError("VNYarnTransitionCommands requires Dialogue Runner and Transition Controller references.", this);
                return;
            }

            dialogueRunner.AddCommandHandler<float>("vn_fade_to_black", FadeToBlack);
            dialogueRunner.AddCommandHandler<float>("vn_fade_from_black", FadeFromBlack);
            dialogueRunner.AddCommandHandler<string, float>("vn_bg_crossfade", CrossfadeBackground);
            dialogueRunner.AddCommandHandler<string, string, string, float>("vn_show_fade", ShowCharacterFade);
            dialogueRunner.AddCommandHandler<string, float>("vn_hide_fade", HideCharacterFade);
            dialogueRunner.AddCommandHandler<string, float>("vn_cg_fade_in", FadeCGIn);
            dialogueRunner.AddCommandHandler<float>("vn_cg_fade_out", FadeCGOut);
            handlersRegistered = true;
        }

        private void UnregisterHandlers()
        {
            if (!handlersRegistered || dialogueRunner == null) return;
            foreach (var commandName in CommandNames) dialogueRunner.RemoveCommandHandler(commandName);
            handlersRegistered = false;
        }

        private IEnumerator FadeToBlack(float duration) => transitionController.FadeToBlack(duration);
        private IEnumerator FadeFromBlack(float duration) => transitionController.FadeFromBlack(duration);
        private IEnumerator CrossfadeBackground(string backgroundId, float duration) => transitionController.CrossfadeBackground(backgroundId, duration);

        private IEnumerator ShowCharacterFade(string characterId, string expressionId, string slotId, float duration)
        {
            if (!VNPresentationController.TryParseSlot(slotId, out var slot))
            {
                Debug.LogError($"Unknown VN character slot '{slotId}'.", this);
                yield break;
            }

            yield return transitionController.FadeCharacterIn(characterId, expressionId, slot, duration);
        }

        private IEnumerator HideCharacterFade(string characterId, float duration) => transitionController.FadeCharacterOut(characterId, duration);
        private IEnumerator FadeCGIn(string cgId, float duration) => transitionController.FadeCGIn(cgId, duration);
        private IEnumerator FadeCGOut(float duration) => transitionController.FadeCGOut(duration);
    }
}
