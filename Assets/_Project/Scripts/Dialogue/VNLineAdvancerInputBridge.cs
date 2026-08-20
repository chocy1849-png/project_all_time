using System;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Project-owned external input seam for Yarn's existing LineAdvancer. It is
    /// designed to live beside LineAdvancer when M6-06 changes it to External.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VNLineAdvancerInputBridge : MonoBehaviour, ILineAdvancerInput
    {
        [SerializeField] private VNDialogueSessionState sessionState;
        [SerializeField] private VNInteractionGate interactionGate;

        private LineAdvancer lineAdvancer;

        public event Action<VNAdvanceSource> AdvanceForwarded;

        public LineAdvancer LineAdvancer
        {
            get => ResolveLineAdvancer();
            set => lineAdvancer = value;
        }

        public void OnDialogueStarted() => ResolveLineAdvancer();

        public void OnDialogueComplete() { }

        /// <summary>
        /// Requests existing Yarn hurry-then-next semantics. Options are never
        /// forwarded through this path.
        /// </summary>
        public bool TryAdvance(VNAdvanceSource source)
        {
            return TryAdvance(source, Time.frameCount);
        }

        /// <summary>
        /// Mirrors Yarn's same-frame content guard before authorizing any read
        /// state, so a rejected input cannot fabricate a consumed line.
        /// </summary>
        internal bool TryAdvance(VNAdvanceSource source, int frameCount)
        {
            var resolvedLineAdvancer = ResolveLineAdvancer();
            if (resolvedLineAdvancer == null)
            {
                Debug.LogError($"{nameof(VNLineAdvancerInputBridge)} requires a sibling {nameof(LineAdvancer)}.", this);
                return false;
            }

            if (sessionState == null || interactionGate == null || !interactionGate.CanAdvanceStory || sessionState.OptionsActive)
                return false;

            if (!sessionState.IsLineActive) return false;

            if (frameCount == sessionState.CurrentPresentationStartedFrame) return false;

            if (sessionState.IsCurrentLineFullyDisplayed && !sessionState.TryAuthorizeCurrentLineConsume())
                return false;

            resolvedLineAdvancer.OnInputHurryUpLines();
            AdvanceForwarded?.Invoke(source);
            return true;
        }

        private LineAdvancer ResolveLineAdvancer()
        {
            if (lineAdvancer == null) lineAdvancer = GetComponent<LineAdvancer>();
            return lineAdvancer;
        }
    }
}
