using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectAllTime.VN.Settings
{
    [DisallowMultipleComponent]
    public sealed class VNDisplaySettingsView : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown displayModeDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        private readonly List<VNResolutionOption> resolutionOptions = new();
        private VNSettingsService settingsService;
        private VNDisplaySettingsController controller;
        private Action<string> report;
        private bool initialized;

        private void OnEnable() { RegisterListeners(true); }
        private void OnDisable() { RegisterListeners(false); }

        public void Initialize(VNSettingsService service, VNDisplaySettingsController displayController, Action<string> diagnostic)
        {
            settingsService = service ?? throw new ArgumentNullException(nameof(service));
            controller = displayController ?? throw new ArgumentNullException(nameof(displayController));
            report = diagnostic;
            initialized = true;
            if (displayModeDropdown != null)
            {
                displayModeDropdown.ClearOptions();
                displayModeDropdown.AddOptions(new List<string> { "FullScreen Window", "Windowed" });
            }
            PopulateResolutionOptions();
            Refresh(service.Current, service.CanWrite);
        }

        public void Refresh(VNSettingsData settings, bool canWrite)
        {
            if (!initialized) return;
            PopulateResolutionOptions();
            var isWindowed = settings.displayMode == VNSettingsDefaults.WindowedDisplayMode;
            if (displayModeDropdown != null)
            {
                displayModeDropdown.SetValueWithoutNotify(isWindowed ? 1 : 0);
                displayModeDropdown.interactable = canWrite;
            }
            if (resolutionDropdown != null)
            {
                resolutionDropdown.SetValueWithoutNotify(FindResolutionIndex(controller.GetEffectiveWindowedResolution()));
                resolutionDropdown.interactable = canWrite && isWindowed;
            }
        }

        private void HandleDisplayModeChanged(int value)
        {
            if (!CanMutate()) return;
            if (value != 0 && value != 1) { report?.Invoke("Unsupported display mode selection."); Refresh(settingsService.Current, settingsService.CanWrite); return; }
            var success = value == 0 ? controller.TryUseFullScreenWindow(out var diagnostic) : controller.TryUseWindowed(out diagnostic);
            if (!success || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }

        private void HandleResolutionChanged(int value)
        {
            if (!CanMutate() || value < 0 || value >= resolutionOptions.Count) return;
            var success = controller.TrySetWindowedResolution(resolutionOptions[value], out var diagnostic);
            if (!success || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }

        private void PopulateResolutionOptions()
        {
            if (!initialized) return;
            resolutionOptions.Clear();
            foreach (var option in controller.GetWindowedResolutionOptions()) resolutionOptions.Add(option);
            if (resolutionDropdown == null) return;
            resolutionDropdown.ClearOptions();
            var labels = new List<string>();
            foreach (var option in resolutionOptions) labels.Add($"{option.Width} × {option.Height}");
            resolutionDropdown.AddOptions(labels);
        }

        private int FindResolutionIndex(VNResolutionOption option)
        {
            for (var index = 0; index < resolutionOptions.Count; index++) if (resolutionOptions[index] == option) return index;
            return 0;
        }
        private bool CanMutate() => initialized && settingsService.CanWrite;
        public bool TryValidateWiring(out string diagnostic)
        {
            if (displayModeDropdown == null || resolutionDropdown == null) { diagnostic = "Display Settings UI requires mode and resolution dropdowns."; return false; }
            diagnostic = null; return true;
        }
        private void RegisterListeners(bool add)
        {
            if (displayModeDropdown != null) { if (add) displayModeDropdown.onValueChanged.AddListener(HandleDisplayModeChanged); else displayModeDropdown.onValueChanged.RemoveListener(HandleDisplayModeChanged); }
            if (resolutionDropdown != null) { if (add) resolutionDropdown.onValueChanged.AddListener(HandleResolutionChanged); else resolutionDropdown.onValueChanged.RemoveListener(HandleResolutionChanged); }
        }
    }
}
