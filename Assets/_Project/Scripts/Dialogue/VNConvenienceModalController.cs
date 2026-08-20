using UnityEngine;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Arbitrates only the M6 Backlog and Settings views; M5 owns Save/Load.</summary>
    [DisallowMultipleComponent]
    public sealed class VNConvenienceModalController : MonoBehaviour
    {
        [SerializeField] private VNInteractionGate interactionGate;
        [SerializeField] private VNConvenienceController convenienceController;
        [SerializeField] private VNBacklogModal backlogModal;
        [SerializeField] private VNSettingsModal settingsModal;

        public VNConvenienceModalKind ActiveModal { get; private set; }
        public bool IsConvenienceModalOpen => ActiveModal != VNConvenienceModalKind.None;

        private void OnEnable()
        {
            if (convenienceController != null) convenienceController.SafeManualStateRequested += HandleSafeManualStateRequested;
        }

        private void OnDisable()
        {
            if (convenienceController != null) convenienceController.SafeManualStateRequested -= HandleSafeManualStateRequested;
        }

        public bool TryOpenBacklog()
        {
            if (!CanOpenModal() || backlogModal == null) return false;
            interactionGate.SetConvenienceModalActive(true);
            if (!backlogModal.TryOpen())
            {
                interactionGate.SetConvenienceModalActive(false);
                return false;
            }

            ActiveModal = VNConvenienceModalKind.Backlog;
            return true;
        }

        public bool TryOpenSettings()
        {
            if (!CanOpenModal() || settingsModal == null) return false;
            interactionGate.SetConvenienceModalActive(true);
            if (!settingsModal.TryOpen())
            {
                interactionGate.SetConvenienceModalActive(false);
                return false;
            }

            ActiveModal = VNConvenienceModalKind.Settings;
            return true;
        }

        public bool CloseActiveModal()
        {
            if (!IsConvenienceModalOpen) return false;
            var closed = ActiveModal switch
            {
                VNConvenienceModalKind.Backlog => backlogModal != null && backlogModal.Close(),
                VNConvenienceModalKind.Settings => settingsModal != null && settingsModal.Close(),
                _ => false,
            };
            if (!closed) return false;

            ActiveModal = VNConvenienceModalKind.None;
            interactionGate?.SetConvenienceModalActive(false);
            return true;
        }

        private bool CanOpenModal()
        {
            return interactionGate != null && !IsConvenienceModalOpen &&
                !interactionGate.IsUiHidden && !interactionGate.IsBlockingModalActive && !interactionGate.IsLoadInProgress;
        }

        private void HandleSafeManualStateRequested()
        {
            if (IsConvenienceModalOpen) CloseActiveModal();
        }
    }
}
