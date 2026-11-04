using System.Collections;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Presentation
{
    public sealed class VNYarnTransitionCommands : MonoBehaviour
    {
        private static readonly string[] CommandNames = { "vn_fade_to_black", "vn_fade_from_black", "vn_bg_crossfade", "vn_show_fade", "vn_hide_fade", "vn_cg_fade_in", "vn_cg_fade_out", "vn_local_effect" };

        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNTransitionController transitionController;
        private bool handlersRegistered;

        private void OnEnable()
        {
            UnregisterHandlers();
            RegisterHandlers();
        }

        private void OnDisable() => UnregisterHandlers();

        private void Update()
        {
            if (handlersRegistered && dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
                ClearLocalizedEffect();
        }

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
            dialogueRunner.AddCommandHandler<string, string, string, float>("vn_show_fade", TrackedShowCharacterFade);
            dialogueRunner.AddCommandHandler<string, float>("vn_hide_fade", HideCharacterFade);
            dialogueRunner.AddCommandHandler<string, float>("vn_cg_fade_in", FadeCGIn);
            dialogueRunner.AddCommandHandler<float>("vn_cg_fade_out", FadeCGOut);
            dialogueRunner.AddCommandHandler<string, float>("vn_local_effect", TrackedLocalizedEffect);
            dialogueRunner.onDialogueComplete?.AddListener(ClearLocalizedEffect);
            handlersRegistered = true;
        }

        private void UnregisterHandlers()
        {
            if (!handlersRegistered || dialogueRunner == null) return;
            dialogueRunner.onDialogueComplete?.RemoveListener(ClearLocalizedEffect);
            ClearLocalizedEffect();
            foreach (var commandName in CommandNames) dialogueRunner.RemoveCommandHandler(commandName);
            handlersRegistered = false;
        }

        private IEnumerator FadeToBlack(float duration) => VNAsyncCommandRegistry.Track(dialogueRunner, transitionController.FadeToBlack(duration));
        private IEnumerator FadeFromBlack(float duration) => VNAsyncCommandRegistry.Track(dialogueRunner, transitionController.FadeFromBlack(duration));
        private IEnumerator CrossfadeBackground(string backgroundId, float duration) => VNAsyncCommandRegistry.Track(dialogueRunner, transitionController.CrossfadeBackground(backgroundId, duration));

        private IEnumerator TrackedShowCharacterFade(string characterId, string expressionId, string slotId, float duration)
            => VNAsyncCommandRegistry.Track(dialogueRunner, ShowCharacterFade(characterId, expressionId, slotId, duration));

        private IEnumerator ShowCharacterFade(string characterId, string expressionId, string slotId, float duration)
        {
            if (!VNPresentationController.TryParseSlot(slotId, out var slot))
            {
                Debug.LogError($"Unknown VN character slot '{slotId}'.", this);
                yield break;
            }

            yield return transitionController.FadeCharacterIn(characterId, expressionId, slot, duration);
        }

        private IEnumerator HideCharacterFade(string characterId, float duration) => VNAsyncCommandRegistry.Track(dialogueRunner, transitionController.FadeCharacterOut(characterId, duration));
        private IEnumerator FadeCGIn(string cgId, float duration) => VNAsyncCommandRegistry.Track(dialogueRunner, transitionController.FadeCGIn(cgId, duration));
        private IEnumerator FadeCGOut(float duration) => VNAsyncCommandRegistry.Track(dialogueRunner, transitionController.FadeCGOut(duration));

        private void ClearLocalizedEffect()
        {
            if (transitionController != null && transitionController.IsLocalizedEffectVisible)
                transitionController.NormalizeForLoad();
        }

        private IEnumerator TrackedLocalizedEffect(string state, float duration)
            => VNAsyncCommandRegistry.Track(dialogueRunner, LocalizedEffect(state, duration));

        private IEnumerator LocalizedEffect(string state, float duration)
        {
            if ((state != "on" && state != "off") || float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f)
            {
                Debug.LogError("vn_local_effect requires on/off and a finite non-negative duration.", this);
                yield break;
            }
            var routine = transitionController.FadeLocalizedEffect(state == "on", duration);
            try
            {
                while (isActiveAndEnabled && dialogueRunner != null && dialogueRunner.IsDialogueRunning && routine.MoveNext())
                    yield return routine.Current;
                if (!isActiveAndEnabled || dialogueRunner == null || !dialogueRunner.IsDialogueRunning)
                    transitionController.NormalizeForLoad();
            }
            finally { (routine as System.IDisposable)?.Dispose(); }
        }
    }
}
