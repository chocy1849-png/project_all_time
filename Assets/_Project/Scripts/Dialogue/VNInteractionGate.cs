using ProjectAllTime.VN.SaveLoad;
using UnityEngine;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Small synchronous policy gate for story input and convenience automation.</summary>
    [DisallowMultipleComponent]
    public sealed class VNInteractionGate : MonoBehaviour
    {
        [SerializeField] private VNDialogueSessionState sessionState;
        [SerializeField] private VNSaveLoadController saveLoadController;

        private bool isUiHidden;
        private bool convenienceModalActive;
        private MonoBehaviour storyInteractionOwner;
        private System.Func<bool> storyCancel;
        private bool storyReadingEnabled;
        private long storyReadingAfterOccurrence;

        public bool IsBlockingModalActive => saveLoadController != null &&
            (saveLoadController.IsModalOpen || saveLoadController.IsOverwriteConfirmationActive) || convenienceModalActive;
        public bool IsLoadInProgress => saveLoadController != null && saveLoadController.IsLoadInProgress;
        public bool IsUiHidden => isUiHidden;
        public bool IsConvenienceModalActive => convenienceModalActive;
        public bool IsStoryInteractionActive => storyInteractionOwner != null;
        public bool IsM5ModalOrLoadActive => saveLoadController != null &&
            (saveLoadController.IsModalOpen || saveLoadController.IsOverwriteConfirmationActive || saveLoadController.IsLoadInProgress);
        public bool CanAdvanceStory => sessionState != null && !sessionState.OptionsActive &&
            !IsBlockingModalActive && !IsLoadInProgress && !isUiHidden &&
            (!IsStoryInteractionActive || storyReadingEnabled && sessionState.IsLineActive &&
                sessionState.CurrentPresentationOccurrence > storyReadingAfterOccurrence);
        public bool CanAdvanceStoryFrom(VNAdvanceSource source) => CanAdvanceStory &&
            (!IsStoryInteractionActive || source == VNAdvanceSource.Manual);
        public bool CanRunAutomation => CanAdvanceStory && !IsStoryInteractionActive && !sessionState.IsManualAdvanceRequired;
        /// <summary>Options may remain visible; modal/load/hidden ownership blocks user mode changes.</summary>
        public bool CanChangeConvenienceMode => !IsBlockingModalActive && !IsLoadInProgress && !isUiHidden && !IsStoryInteractionActive;
        /// <summary>Save/Load is intentionally permitted while a Yarn choice is visible.</summary>
        public bool CanUseSaveLoad => !IsBlockingModalActive && !IsLoadInProgress && !isUiHidden && !IsStoryInteractionActive;
        /// <summary>Hide requires an actual current line so command-only intervals stay visible.</summary>
        public bool CanHideUi => sessionState != null && sessionState.IsLineActive &&
            !sessionState.OptionsActive && !IsBlockingModalActive && !IsLoadInProgress && !isUiHidden && !IsStoryInteractionActive;

        /// <summary>One scene story interaction, arbitrated by the same gate as convenience input.</summary>
        public bool TryAcquireStoryInteraction(MonoBehaviour owner, System.Func<bool> cancel)
        {
            if (owner == null || cancel == null || IsStoryInteractionActive || !CanChangeConvenienceMode ||
                sessionState == null || sessionState.OptionsActive) return false;
            storyInteractionOwner = owner; storyCancel = cancel; storyReadingEnabled = false;
            return true;
        }

        internal void SetStoryReadingEnabled(MonoBehaviour owner, bool enabled)
        {
            if (owner != null && storyInteractionOwner == owner)
            {
                storyReadingEnabled = enabled;
                storyReadingAfterOccurrence = sessionState == null ? long.MaxValue : sessionState.CurrentPresentationOccurrence;
            }
        }

        internal void AcceptStoryManualConsume() { if (IsStoryInteractionActive) storyReadingEnabled = false; }

        public void ReleaseStoryInteraction(MonoBehaviour owner)
        {
            if (storyInteractionOwner != owner) return;
            storyInteractionOwner = null; storyCancel = null; storyReadingEnabled = false;
        }

        public bool TryCancelStoryInteraction() => IsStoryInteractionActive && storyCancel != null && storyCancel();

        /// <summary>Future M6 hide UI calls this without owning any visual implementation here.</summary>
        public void SetUiHidden(bool hidden) => isUiHidden = hidden;

        /// <summary>Owned exclusively by the convenience modal coordinator.</summary>
        public void SetConvenienceModalActive(bool active) => convenienceModalActive = active;
    }
}
