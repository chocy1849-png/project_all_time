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

        public bool IsBlockingModalActive => saveLoadController != null &&
            (saveLoadController.IsModalOpen || saveLoadController.IsOverwriteConfirmationActive) || convenienceModalActive;
        public bool IsLoadInProgress => saveLoadController != null && saveLoadController.IsLoadInProgress;
        public bool IsUiHidden => isUiHidden;
        public bool IsConvenienceModalActive => convenienceModalActive;
        public bool IsM5ModalOrLoadActive => saveLoadController != null &&
            (saveLoadController.IsModalOpen || saveLoadController.IsOverwriteConfirmationActive || saveLoadController.IsLoadInProgress);
        public bool CanAdvanceStory => sessionState != null && !sessionState.OptionsActive &&
            !IsBlockingModalActive && !IsLoadInProgress && !isUiHidden;
        public bool CanRunAutomation => CanAdvanceStory;
        /// <summary>Save/Load is intentionally permitted while a Yarn choice is visible.</summary>
        public bool CanUseSaveLoad => !IsBlockingModalActive && !IsLoadInProgress && !isUiHidden;
        /// <summary>Hide requires an actual current line so command-only intervals stay visible.</summary>
        public bool CanHideUi => sessionState != null && sessionState.IsLineActive &&
            !sessionState.OptionsActive && !IsBlockingModalActive && !IsLoadInProgress && !isUiHidden;

        /// <summary>Future M6 hide UI calls this without owning any visual implementation here.</summary>
        public void SetUiHidden(bool hidden) => isUiHidden = hidden;

        /// <summary>Owned exclusively by the compact M6 Backlog/Settings coordinator.</summary>
        public void SetConvenienceModalActive(bool active) => convenienceModalActive = active;
    }
}
