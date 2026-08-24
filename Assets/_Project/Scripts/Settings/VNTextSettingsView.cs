using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    [DisallowMultipleComponent]
    public sealed class VNTextSettingsView : MonoBehaviour
    {
        [SerializeField] private Slider textSpeedSlider;
        [SerializeField] private TMP_Text textSpeedValueLabel;
        [SerializeField] private Slider autoSpeedSlider;
        [SerializeField] private TMP_Text autoSpeedValueLabel;
        [SerializeField] private VNSettingsSliderCommit textSpeedCommit;
        [SerializeField] private VNSettingsSliderCommit autoSpeedCommit;
        private VNSettingsService settingsService;
        private VNTextAutoSettingsController controller;
        private Action<string> report;
        private bool initialized;

        private void OnEnable() => RegisterListeners(true);
        private void OnDisable() => RegisterListeners(false);

        public void Initialize(VNSettingsService service, VNTextAutoSettingsController textController, Action<string> diagnostic)
        {
            settingsService = service ?? throw new ArgumentNullException(nameof(service));
            controller = textController ?? throw new ArgumentNullException(nameof(textController));
            report = diagnostic;
            initialized = true;
            if (textSpeedSlider != null) { textSpeedSlider.minValue = VNTextAutoSettingsController.MinimumTextSpeedLps; textSpeedSlider.maxValue = VNTextAutoSettingsController.MaximumTextSpeedLps; textSpeedSlider.wholeNumbers = true; }
            if (autoSpeedSlider != null) { autoSpeedSlider.minValue = 0f; autoSpeedSlider.maxValue = 1f; }
            textSpeedCommit?.Initialize(textSpeedSlider);
            autoSpeedCommit?.Initialize(autoSpeedSlider);
            Refresh(service.Current, service.CanWrite);
        }

        public void Refresh(VNSettingsData settings, bool canWrite)
        {
            if (!initialized) return;
            if (textSpeedSlider != null) { textSpeedSlider.SetValueWithoutNotify(settings.textSpeedLps); textSpeedSlider.interactable = canWrite; }
            if (autoSpeedSlider != null) { autoSpeedSlider.SetValueWithoutNotify(settings.autoSpeedNormalized); autoSpeedSlider.interactable = canWrite; }
            UpdateTextLabel(settings.textSpeedLps);
            UpdateAutoLabel(settings.autoSpeedNormalized);
        }

        private void PreviewText(float value) => UpdateTextLabel(Mathf.RoundToInt(value));
        private void PreviewAuto(float value) => UpdateAutoLabel(value);
        private void CommitText(float value)
        {
            if (!CanMutate()) return;
            var success = controller.TrySetTextSpeedLps(Mathf.RoundToInt(value), out var diagnostic);
            if (!success || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }
        private void CommitAuto(float value)
        {
            if (!CanMutate()) return;
            var success = controller.TrySetAutoSpeedNormalized(value, out var diagnostic);
            if (!success || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }
        private void UpdateTextLabel(int value) { if (textSpeedValueLabel != null) textSpeedValueLabel.text = value.ToString(); }
        private void UpdateAutoLabel(float value) { if (autoSpeedValueLabel != null) autoSpeedValueLabel.text = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%"; }
        private bool CanMutate() => initialized && settingsService.CanWrite;
        private void RegisterListeners(bool add)
        {
            if (textSpeedSlider != null) { if (add) textSpeedSlider.onValueChanged.AddListener(PreviewText); else textSpeedSlider.onValueChanged.RemoveListener(PreviewText); }
            if (autoSpeedSlider != null) { if (add) autoSpeedSlider.onValueChanged.AddListener(PreviewAuto); else autoSpeedSlider.onValueChanged.RemoveListener(PreviewAuto); }
            if (textSpeedCommit != null) { if (add) textSpeedCommit.CommitRequested += CommitText; else textSpeedCommit.CommitRequested -= CommitText; }
            if (autoSpeedCommit != null) { if (add) autoSpeedCommit.CommitRequested += CommitAuto; else autoSpeedCommit.CommitRequested -= CommitAuto; }
        }
    }
}
