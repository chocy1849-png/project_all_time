using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Runtime listener owner for the eight M6 QuickControl actions.</summary>
    [DisallowMultipleComponent]
    public sealed class VNQuickControlBar : MonoBehaviour
    {
        [SerializeField] private VNConvenienceController convenienceController;
        [SerializeField] private VNConvenienceModalController modalController;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button hideButton;
        [SerializeField] private Button backlogButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button autoButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject skipSelectedIndicator;
        [SerializeField] private GameObject autoSelectedIndicator;

        private void OnEnable()
        {
            RegisterListeners();
            if (convenienceController != null)
            {
                convenienceController.AutoStateChanged += HandleAutoStateChanged;
                convenienceController.SkipStateChanged += HandleSkipStateChanged;
            }
            RefreshModeIndicators();
        }

        private void OnDisable()
        {
            if (convenienceController != null)
            {
                convenienceController.AutoStateChanged -= HandleAutoStateChanged;
                convenienceController.SkipStateChanged -= HandleSkipStateChanged;
            }
            UnregisterListeners();
        }

        private void RegisterListeners()
        {
            nextButton?.onClick.AddListener(HandleNextClicked);
            hideButton?.onClick.AddListener(HandleHideClicked);
            backlogButton?.onClick.AddListener(HandleBacklogClicked);
            skipButton?.onClick.AddListener(HandleSkipClicked);
            autoButton?.onClick.AddListener(HandleAutoClicked);
            saveButton?.onClick.AddListener(HandleSaveClicked);
            loadButton?.onClick.AddListener(HandleLoadClicked);
            settingsButton?.onClick.AddListener(HandleSettingsClicked);
        }

        private void UnregisterListeners()
        {
            nextButton?.onClick.RemoveListener(HandleNextClicked);
            hideButton?.onClick.RemoveListener(HandleHideClicked);
            backlogButton?.onClick.RemoveListener(HandleBacklogClicked);
            skipButton?.onClick.RemoveListener(HandleSkipClicked);
            autoButton?.onClick.RemoveListener(HandleAutoClicked);
            saveButton?.onClick.RemoveListener(HandleSaveClicked);
            loadButton?.onClick.RemoveListener(HandleLoadClicked);
            settingsButton?.onClick.RemoveListener(HandleSettingsClicked);
        }

        private void HandleNextClicked() => convenienceController?.HandleManualAdvance();
        private void HandleHideClicked() => convenienceController?.ToggleUiVisibility();
        private void HandleBacklogClicked() => modalController?.TryOpenBacklog();
        private void HandleSkipClicked() => convenienceController?.ToggleSkip();
        private void HandleAutoClicked() => convenienceController?.ToggleAuto();
        private void HandleSaveClicked() => convenienceController?.OpenSave();
        private void HandleLoadClicked() => convenienceController?.OpenLoad();
        private void HandleSettingsClicked() => modalController?.TryOpenSettings();

        private void HandleAutoStateChanged(bool enabled) => SetActive(autoSelectedIndicator, enabled);
        private void HandleSkipStateChanged(bool enabled) => SetActive(skipSelectedIndicator, enabled);

        private void RefreshModeIndicators()
        {
            SetActive(autoSelectedIndicator, convenienceController != null && convenienceController.IsAutoEnabled);
            SetActive(skipSelectedIndicator, convenienceController != null && convenienceController.IsSkipEnabled);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null) target.SetActive(active);
        }
    }
}
