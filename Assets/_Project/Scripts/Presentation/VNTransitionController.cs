using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Presentation
{
    public sealed class VNTransitionController : MonoBehaviour
    {
        [SerializeField] private VNPresentationController presentationController;
        [SerializeField] private CanvasGroup screenFadeCanvasGroup;
        [SerializeField] private CanvasGroup backgroundCurrentCanvasGroup;
        [SerializeField] private Image backgroundIncomingImage;
        [SerializeField] private CanvasGroup backgroundIncomingCanvasGroup;
        [SerializeField] private CanvasGroup cgCanvasGroup;

        private int activeTransitionOperations;

        /// <summary>True while an awaited M4 presentation operation is active.</summary>
        public bool IsTransitionActive => activeTransitionOperations > 0;

        public IEnumerator FadeToBlack(float duration) => TrackTransition(FadeToBlackRoutine(duration));

        public IEnumerator FadeFromBlack(float duration) => TrackTransition(FadeFromBlackRoutine(duration));

        public IEnumerator CrossfadeBackground(string backgroundId, float duration) => TrackTransition(CrossfadeBackgroundRoutine(backgroundId, duration));

        public IEnumerator FadeCharacterIn(string characterId, string expressionId, VNCharacterSlot slot, float duration) => TrackTransition(FadeCharacterInRoutine(characterId, expressionId, slot, duration));

        public IEnumerator FadeCharacterOut(string characterId, float duration) => TrackTransition(FadeCharacterOutRoutine(characterId, duration));

        public IEnumerator FadeCGIn(string cgId, float duration) => TrackTransition(FadeCGInRoutine(cgId, duration));

        public IEnumerator FadeCGOut(float duration) => TrackTransition(FadeCGOutRoutine(duration));

        /// <summary>
        /// Cancels transient M4 operations and restores their visual channels
        /// to a stable baseline. Logical M3 state remains controller-owned and
        /// is reconstructed immediately by the subsequent restore call.
        /// </summary>
        public void NormalizeForLoad()
        {
            StopAllCoroutines();
            activeTransitionOperations = 0;

            if (screenFadeCanvasGroup != null) screenFadeCanvasGroup.alpha = 0f;
            if (backgroundCurrentCanvasGroup != null)
            {
                var current = presentationController == null ? null : presentationController.BackgroundImage;
                backgroundCurrentCanvasGroup.alpha = current != null && current.enabled && current.sprite != null ? 1f : 0f;
            }

            ResetIncomingBackground();
            if (cgCanvasGroup != null) cgCanvasGroup.alpha = 1f;
            presentationController?.NormalizeStableCharacterVisuals();
        }

        /// <summary>Finalizes alpha and buffer state after immediate M3 restore.</summary>
        public void FinalizeStableStateAfterLoad()
        {
            if (screenFadeCanvasGroup != null) screenFadeCanvasGroup.alpha = 0f;
            if (backgroundCurrentCanvasGroup != null)
            {
                var current = presentationController == null ? null : presentationController.BackgroundImage;
                backgroundCurrentCanvasGroup.alpha = current != null && current.enabled && current.sprite != null ? 1f : 0f;
            }

            ResetIncomingBackground();
            if (cgCanvasGroup != null) cgCanvasGroup.alpha = 1f;
            presentationController?.NormalizeStableCharacterVisuals();
        }

        private IEnumerator FadeToBlackRoutine(float duration)
        {
            if (!IsValidDuration(duration, "Screen fade to black")) yield break;
            if (screenFadeCanvasGroup == null) { LogMissingReference("Screen Fade CanvasGroup"); yield break; }
            yield return FadeCanvasGroup(screenFadeCanvasGroup, screenFadeCanvasGroup.alpha, 1f, duration);
        }

        private IEnumerator FadeFromBlackRoutine(float duration)
        {
            if (!IsValidDuration(duration, "Screen fade from black")) yield break;
            if (screenFadeCanvasGroup == null) { LogMissingReference("Screen Fade CanvasGroup"); yield break; }
            yield return FadeCanvasGroup(screenFadeCanvasGroup, screenFadeCanvasGroup.alpha, 0f, duration);
        }

        private IEnumerator CrossfadeBackgroundRoutine(string backgroundId, float duration)
        {
            if (!IsValidDuration(duration, "Background crossfade") || !TryGetBackgroundCrossfadeReferences(out var currentImage)) yield break;
            if (!presentationController.TryGetBackgroundSprite(backgroundId, out var targetSprite))
            {
                Debug.LogError($"Cannot crossfade to unknown background '{backgroundId}'.", this);
                yield break;
            }

            if (duration <= 0f)
            {
                presentationController.SetBackground(backgroundId);
                ResetIncomingBackground();
                backgroundCurrentCanvasGroup.alpha = 1f;
                yield break;
            }

            backgroundIncomingImage.sprite = targetSprite;
            backgroundIncomingImage.enabled = true;
            backgroundIncomingCanvasGroup.alpha = 0f;
            var currentAlpha = currentImage.sprite == null ? 0f : backgroundCurrentCanvasGroup.alpha;

            yield return FadePair(backgroundCurrentCanvasGroup, currentAlpha, 0f, backgroundIncomingCanvasGroup, 0f, 1f, duration);

            // Commit through the M3 controller so the current background ID and primary image remain authoritative.
            presentationController.SetBackground(backgroundId);
            backgroundCurrentCanvasGroup.alpha = 1f;
            ResetIncomingBackground();
        }

        private IEnumerator FadeCharacterInRoutine(string characterId, string expressionId, VNCharacterSlot slot, float duration)
        {
            if (!IsValidDuration(duration, "Character fade in")) yield break;
            if (presentationController == null) { LogMissingReference(nameof(VNPresentationController)); yield break; }
            if (!presentationController.TryGetCharacterSlotView(slot, out var view) || !view.HasFadeCanvasGroup)
            {
                Debug.LogError($"Cannot fade in character because slot '{slot}' has no configured CanvasGroup on its CharacterVisualRoot.", this);
                yield break;
            }

            if (!presentationController.ShowCharacter(characterId, expressionId, slot)) yield break;

            view.SetFadeAlpha(0f);
            yield return FadeCanvasGroup(view.FadeCanvasGroup, 0f, 1f, duration);
        }

        private IEnumerator FadeCharacterOutRoutine(string characterId, float duration)
        {
            if (!IsValidDuration(duration, "Character fade out")) yield break;
            if (presentationController == null) { LogMissingReference(nameof(VNPresentationController)); yield break; }
            if (!presentationController.TryGetVisibleCharacterSlotView(characterId, out var view) || !view.HasFadeCanvasGroup)
            {
                Debug.LogError($"Cannot fade out character '{characterId}' because it is not visible or has no configured CanvasGroup.", this);
                yield break;
            }

            yield return FadeCanvasGroup(view.FadeCanvasGroup, view.FadeCanvasGroup.alpha, 0f, duration);
            presentationController.HideCharacter(characterId);
        }

        private IEnumerator FadeCGInRoutine(string cgId, float duration)
        {
            if (!IsValidDuration(duration, "CG fade in") || !TryGetCGFadeReferences()) yield break;
            if (!presentationController.TryGetCGSprite(cgId, out _))
            {
                Debug.LogError($"Cannot fade in unknown CG '{cgId}'.", this);
                yield break;
            }

            cgCanvasGroup.alpha = 0f;
            if (!presentationController.SetCG(cgId)) yield break;
            yield return FadeCanvasGroup(cgCanvasGroup, 0f, 1f, duration);
        }

        private IEnumerator FadeCGOutRoutine(float duration)
        {
            if (!IsValidDuration(duration, "CG fade out") || !TryGetCGFadeReferences()) yield break;
            if (presentationController.CGImage == null || presentationController.CGImage.sprite == null)
            {
                Debug.LogError("Cannot fade out CG because no CG is currently visible.", this);
                yield break;
            }

            yield return FadeCanvasGroup(cgCanvasGroup, cgCanvasGroup.alpha, 0f, duration);
            presentationController.ClearCG();
            cgCanvasGroup.alpha = 1f;
        }

        private bool TryGetBackgroundCrossfadeReferences(out Image currentImage)
        {
            currentImage = null;
            if (presentationController == null || backgroundCurrentCanvasGroup == null || backgroundIncomingImage == null || backgroundIncomingCanvasGroup == null)
            {
                Debug.LogError("VN Transition Controller requires Presentation Controller, current-background CanvasGroup, incoming Background Image, and incoming-background CanvasGroup references.", this);
                return false;
            }

            currentImage = presentationController.BackgroundImage;
            if (currentImage == null)
            {
                Debug.LogError("VN Transition Controller requires the M3 Background Image reference on VN Presentation Controller.", this);
                return false;
            }

            return true;
        }

        private bool TryGetCGFadeReferences()
        {
            if (presentationController != null && cgCanvasGroup != null) return true;
            Debug.LogError("VN Transition Controller requires Presentation Controller and CG CanvasGroup references.", this);
            return false;
        }

        private void ResetIncomingBackground()
        {
            if (backgroundIncomingCanvasGroup == null || backgroundIncomingImage == null) return;
            backgroundIncomingCanvasGroup.alpha = 0f;
            backgroundIncomingImage.sprite = null;
            backgroundIncomingImage.enabled = false;
        }

        private IEnumerator TrackTransition(IEnumerator operation)
        {
            activeTransitionOperations++;
            try
            {
                while (operation != null && operation.MoveNext()) yield return operation.Current;
            }
            finally
            {
                activeTransitionOperations = Mathf.Max(0, activeTransitionOperations - 1);
            }
        }

        private static IEnumerator FadePair(CanvasGroup first, float firstStart, float firstTarget, CanvasGroup second, float secondStart, float secondTarget, float duration)
        {
            if (duration <= 0f)
            {
                first.alpha = firstTarget;
                second.alpha = secondTarget;
                yield break;
            }

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                var progress = elapsed / duration;
                first.alpha = Mathf.Lerp(firstStart, firstTarget, progress);
                second.alpha = Mathf.Lerp(secondStart, secondTarget, progress);
                yield return null;
            }

            first.alpha = firstTarget;
            second.alpha = secondTarget;
        }

        private static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float target, float duration)
        {
            if (duration <= 0f)
            {
                canvasGroup.alpha = target;
                yield break;
            }

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = target;
        }

        private bool IsValidDuration(float duration, string operation)
        {
            if (!float.IsNaN(duration) && !float.IsInfinity(duration) && duration >= 0f) return true;
            Debug.LogError($"{operation} duration must be a finite value greater than or equal to zero.", this);
            return false;
        }

        private void LogMissingReference(string referenceName) => Debug.LogError($"VN Transition Controller requires a {referenceName} reference.", this);
    }
}
