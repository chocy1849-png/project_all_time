using System;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Owns the compact, occurrence-safe Auto and Skip runtime policy.</summary>
    [DisallowMultipleComponent]
    public sealed class VNConvenienceController : MonoBehaviour
    {
        [Header("M6 Runtime Dependencies")]
        [SerializeField] private VNDialogueSessionState sessionState;
        [SerializeField] private VNLineAdvancerInputBridge advanceBridge;
        [SerializeField] private VNInteractionGate interactionGate;
        [SerializeField] private VNUIVisibilityController uiVisibilityController;
        [SerializeField] private VNConvenienceModalController convenienceModalController;
        [SerializeField] private VNOptionalVoicePresenter optionalVoicePresenter;
        [SerializeField] private VNSaveLoadController saveLoadController;

        [Header("Auto Timing (Unscaled Seconds)")]
        [SerializeField] private float baseDelaySeconds = 0.50f;
        [SerializeField] private float secondsPerCharacter = 0.035f;
        [SerializeField] private float minimumDelaySeconds = 0.80f;
        [SerializeField] private float maximumDelaySeconds = 4.00f;
        [SerializeField] private float skipAdvanceIntervalSeconds = 0.05f;

        private bool isAutoEnabled;
        private bool isSkipEnabled;
        private VNSkipPolicy skipPolicy = VNSkipPolicy.ReadOnly;
        private long observedOccurrence = long.MinValue;
        private bool autoTimerArmed;
        private float autoDeadline;
        private long autoRequestedOccurrence = long.MinValue;
        private long skipConsumedOccurrence = long.MinValue;
        private float lastSkipRequestTime = float.NegativeInfinity;
        private int lastSkipRequestFrame = -1;

        public event Action<bool> AutoStateChanged;
        public event Action<bool> SkipStateChanged;
        public event Action<VNSkipPolicy> SkipPolicyChanged;
        /// <summary>Future Backlog/Settings modal owners close in response to a real M5 Load start.</summary>
        public event Action SafeManualStateRequested;

        public bool IsAutoEnabled => isAutoEnabled;
        public bool IsSkipEnabled => isSkipEnabled;
        public VNSkipPolicy SkipPolicy => skipPolicy;

        private void OnEnable()
        {
            if (saveLoadController != null) saveLoadController.LoadStateChanged += HandleLoadStateChanged;
            if (saveLoadController != null && saveLoadController.IsLoadInProgress) HandleLoadStateChanged(true);
        }

        private void OnDisable()
        {
            if (saveLoadController != null) saveLoadController.LoadStateChanged -= HandleLoadStateChanged;
        }

        private void Update() => Tick(Time.unscaledTime, Time.frameCount);

        public void SetAutoEnabled(bool enabled)
        {
            if (isAutoEnabled == enabled) return;
            isAutoEnabled = enabled;
            if (enabled) SetSkipEnabled(false);
            ResetAutoScheduling();
            AutoStateChanged?.Invoke(enabled);
        }

        public void ToggleAuto() => SetAutoEnabled(!isAutoEnabled);

        public void SetSkipEnabled(bool enabled)
        {
            if (isSkipEnabled == enabled) return;
            isSkipEnabled = enabled;
            if (enabled) SetAutoEnabled(false);
            ResetSkipScheduling();
            SkipStateChanged?.Invoke(enabled);
        }

        public void ToggleSkip() => SetSkipEnabled(!isSkipEnabled);

        /// <summary>
        /// The only future manual Next path. A hidden UI consumes this request
        /// by restoring visuals; visible input delegates to the shared bridge.
        /// </summary>
        public bool HandleManualAdvance()
        {
            if (uiVisibilityController != null && uiVisibilityController.IsUiHidden)
                return uiVisibilityController.RestoreUiIfHidden();
            if (interactionGate != null && interactionGate.IsUiHidden) return false;
            return advanceBridge != null && advanceBridge.TryAdvance(VNAdvanceSource.Manual);
        }

        public bool TryHideUi() => uiVisibilityController != null && uiVisibilityController.TryHideUi();

        public bool ShowUi() => uiVisibilityController != null && uiVisibilityController.ShowUi();

        public bool ToggleUiVisibility() => uiVisibilityController != null && uiVisibilityController.ToggleUiVisibility();

        /// <summary>Future Esc routing with M5 ownership first and no dialogue action.</summary>
        public bool HandleCancel()
        {
            if (saveLoadController != null && saveLoadController.IsOverwriteConfirmationActive)
            {
                saveLoadController.CancelManualOverwrite();
                return true;
            }

            if (saveLoadController != null && saveLoadController.IsModalOpen)
            {
                saveLoadController.Close();
                return true;
            }

            if (convenienceModalController != null && convenienceModalController.IsConvenienceModalOpen)
                return convenienceModalController.CloseActiveModal();

            if (uiVisibilityController != null && uiVisibilityController.IsUiHidden)
                return uiVisibilityController.ShowUi();

            return false;
        }

        /// <summary>Delegates M5's modal opening; a Yarn option is not a Save/Load blocker.</summary>
        public bool OpenSave()
        {
            if (!CanUseSaveLoad()) return false;
            saveLoadController.OpenSave();
            return true;
        }

        public bool OpenLoad()
        {
            if (!CanUseSaveLoad()) return false;
            saveLoadController.OpenLoad();
            return true;
        }

        public VNSaveLoadOperationResult QuickSave()
        {
            return CanUseSaveLoad()
                ? saveLoadController.QuickSave()
                : VNSaveLoadOperationResult.Create(VNSaveLoadOperationStatus.Busy, "Save/Load is currently unavailable.");
        }

        public VNSaveLoadOperationResult QuickLoad()
        {
            return CanUseSaveLoad()
                ? saveLoadController.QuickLoad()
                : VNSaveLoadOperationResult.Create(VNSaveLoadOperationStatus.Busy, "Save/Load is currently unavailable.");
        }

        /// <summary>Future Save/Load button PointerDown seam; M5 retains all suppression ownership.</summary>
        public bool BeginSaveLoadOpenerInputSuppression()
        {
            if (!CanUseSaveLoad()) return false;
            saveLoadController.BeginModalInputSuppression();
            return true;
        }

        public void SetSkipPolicy(VNSkipPolicy policy)
        {
            if (skipPolicy == policy) return;
            skipPolicy = policy;
            ResetSkipScheduling();
            SkipPolicyChanged?.Invoke(policy);
        }

        /// <summary>Exposed for deterministic EditMode tests; production calls this from Update.</summary>
        internal void Tick(float unscaledTime, int frameCount)
        {
            if (sessionState == null || advanceBridge == null || interactionGate == null) return;

            var occurrence = sessionState.CurrentPresentationOccurrence;
            if (occurrence != observedOccurrence)
            {
                observedOccurrence = occurrence;
                ResetAutoScheduling();
                ResetSkipScheduling();
                // Always defer automation to a later frame than a new current presentation.
                return;
            }

            if (!interactionGate.CanRunAutomation)
            {
                ResetAutoScheduling();
                ResetSkipThrottle();
                return;
            }

            if (isAutoEnabled) TickAuto(occurrence, unscaledTime);
            if (isSkipEnabled) TickSkip(occurrence, unscaledTime, frameCount);
        }

        /// <summary>Returns the provisional M6 text delay for a displayed string.</summary>
        public float GetAutoDelaySeconds(string displayedText)
        {
            var length = displayedText?.Length ?? 0;
            return Mathf.Clamp(baseDelaySeconds + length * secondsPerCharacter, minimumDelaySeconds, maximumDelaySeconds);
        }

        private void TickAuto(long occurrence, float unscaledTime)
        {
            if (!sessionState.IsLineActive || !sessionState.IsCurrentLineFullyDisplayed || autoRequestedOccurrence == occurrence)
            {
                if (!sessionState.IsCurrentLineFullyDisplayed) ResetAutoScheduling();
                return;
            }

            if (!autoTimerArmed)
            {
                autoTimerArmed = true;
                autoDeadline = unscaledTime + GetAutoDelaySeconds(sessionState.CurrentText);
            }

            if (unscaledTime < autoDeadline || !IsCurrentVoiceComplete()) return;
            if (advanceBridge.TryAdvance(VNAdvanceSource.Auto)) autoRequestedOccurrence = occurrence;
        }

        private void TickSkip(long occurrence, float unscaledTime, int frameCount)
        {
            if (!sessionState.IsLineActive) return;
            if (skipPolicy == VNSkipPolicy.ReadOnly && !sessionState.ReadHistory.IsRead(sessionState.CurrentLineId))
            {
                SetSkipEnabled(false);
                return;
            }

            if (sessionState.IsCurrentLineFullyDisplayed && skipConsumedOccurrence == occurrence) return;
            if (frameCount == lastSkipRequestFrame || unscaledTime - lastSkipRequestTime < Mathf.Max(0f, skipAdvanceIntervalSeconds)) return;

            if (advanceBridge.TryAdvance(VNAdvanceSource.Skip))
            {
                lastSkipRequestTime = unscaledTime;
                lastSkipRequestFrame = frameCount;
                if (sessionState.IsCurrentLineFullyDisplayed) skipConsumedOccurrence = occurrence;
            }
        }

        private bool IsCurrentVoiceComplete()
        {
            return optionalVoicePresenter == null || !optionalVoicePresenter.CurrentLineHasVoice || optionalVoicePresenter.IsCurrentVoiceComplete;
        }

        private void HandleLoadStateChanged(bool isLoading)
        {
            if (!isLoading) return;
            SetAutoEnabled(false);
            SetSkipEnabled(false);
            ResetAutoScheduling();
            ResetSkipScheduling();
            sessionState?.InvalidateTransientPresentation();
            uiVisibilityController?.ShowUi();
            SafeManualStateRequested?.Invoke();
        }

        private bool CanUseSaveLoad()
        {
            return saveLoadController != null && interactionGate != null && interactionGate.CanUseSaveLoad;
        }

        private void ResetAutoScheduling()
        {
            autoTimerArmed = false;
            autoDeadline = 0f;
            autoRequestedOccurrence = long.MinValue;
        }

        private void ResetSkipScheduling()
        {
            skipConsumedOccurrence = long.MinValue;
            ResetSkipThrottle();
        }

        private void ResetSkipThrottle()
        {
            lastSkipRequestTime = float.NegativeInfinity;
            lastSkipRequestFrame = -1;
        }
    }
}
