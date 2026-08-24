using System;
using UnityEngine;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Owns only the Dialogue and QuickControl CanvasGroup visual state. It
    /// deliberately leaves presentation, transition, and modal layers alone.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VNUIVisibilityController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup dialogueLayer;
        [SerializeField] private CanvasGroup quickControlLayer;
        [SerializeField] private VNInteractionGate interactionGate;

        private bool isUiHidden;
        private CanvasGroupState dialogueBaseline;
        private CanvasGroupState quickControlBaseline;

        public event Action<bool> UiVisibilityChanged;

        public bool IsUiHidden => isUiHidden;

        public bool TryHideUi()
        {
            if (isUiHidden) return true;
            if (!HasRequiredReferences()) return false;
            if (!interactionGate.CanHideUi) return false;

            dialogueBaseline = CanvasGroupState.Capture(dialogueLayer);
            quickControlBaseline = CanvasGroupState.Capture(quickControlLayer);
            ApplyHidden(dialogueLayer);
            ApplyHidden(quickControlLayer);
            isUiHidden = true;
            interactionGate.SetUiHidden(true);
            UiVisibilityChanged?.Invoke(true);
            return true;
        }

        public bool ShowUi()
        {
            if (!isUiHidden)
            {
                // Keep the gate synchronized if an external lifecycle (such as
                // Load normalization) asks for the known visible baseline.
                interactionGate?.SetUiHidden(false);
                return true;
            }
            if (!HasRequiredReferences()) return false;

            dialogueBaseline.Apply(dialogueLayer);
            quickControlBaseline.Apply(quickControlLayer);
            isUiHidden = false;
            interactionGate.SetUiHidden(false);
            UiVisibilityChanged?.Invoke(false);
            return true;
        }

        public bool ToggleUiVisibility() => isUiHidden ? ShowUi() : TryHideUi();

        /// <summary>Allows hidden-context input to consume one press restoring UI only.</summary>
        public bool RestoreUiIfHidden() => isUiHidden && ShowUi();

        private bool HasRequiredReferences()
        {
            if (dialogueLayer != null && quickControlLayer != null && interactionGate != null) return true;
            Debug.LogError(
                $"{nameof(VNUIVisibilityController)} requires DialogueLayer, QuickControlLayer, and {nameof(VNInteractionGate)} references before UI visibility can change.",
                this);
            return false;
        }

        private static void ApplyHidden(CanvasGroup group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private struct CanvasGroupState
        {
            private readonly float alpha;
            private readonly bool interactable;
            private readonly bool blocksRaycasts;

            private CanvasGroupState(float alpha, bool interactable, bool blocksRaycasts)
            {
                this.alpha = alpha;
                this.interactable = interactable;
                this.blocksRaycasts = blocksRaycasts;
            }

            public static CanvasGroupState Capture(CanvasGroup group) =>
                new(group.alpha, group.interactable, group.blocksRaycasts);

            public void Apply(CanvasGroup group)
            {
                group.alpha = alpha;
                group.interactable = interactable;
                group.blocksRaycasts = blocksRaycasts;
            }
        }
    }
}
