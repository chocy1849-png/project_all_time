using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    public enum VNSettingsCategory { Display, Text, Audio, Gameplay, Controls }

    /// <summary>UI coordinator only: service/controller state remains authoritative.</summary>
    [DisallowMultipleComponent]
    public sealed class VNSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Button displayCategoryButton;
        [SerializeField] private Button textCategoryButton;
        [SerializeField] private Button audioCategoryButton;
        [SerializeField] private Button gameplayCategoryButton;
        [SerializeField] private Button controlsCategoryButton;
        [SerializeField] private GameObject displayContent;
        [SerializeField] private GameObject textContent;
        [SerializeField] private GameObject audioContent;
        [SerializeField] private GameObject gameplayContent;
        [SerializeField] private GameObject controlsContent;
        [SerializeField] private VNDisplaySettingsView displayView;
        [SerializeField] private VNTextSettingsView textView;
        [SerializeField] private VNAudioSettingsView audioView;
        [SerializeField] private VNGameplaySettingsView gameplayView;
        [SerializeField] private VNControlsSettingsView controlsView;
        [SerializeField] private TMP_Text statusText;

        private VNSettingsService settingsService;
        private VNSettingsCategory selectedCategory = VNSettingsCategory.Display;
        private bool initialized;

        public bool IsInitialized => initialized;
        public VNSettingsCategory SelectedCategory => selectedCategory;

        private void OnEnable() => RegisterCategoryListeners(true);
        private void OnDisable()
        {
            RegisterCategoryListeners(false);
            PrepareForClose();
        }

        public bool Initialize(VNSettingsService service, VNDisplaySettingsController displayController,
            VNTextAutoSettingsController textAutoController, VNAudioSettingsController audioController,
            VNGameplaySettingsController gameplayController, VNInputRebindService rebindService)
        {
            if (service == null || displayController == null || textAutoController == null || audioController == null || gameplayController == null || rebindService == null)
            {
                ReportDiagnostic("Settings UI requires all M7 runtime dependencies.");
                return false;
            }
            if (!TryValidateWiring(out var wiringDiagnostic))
            {
                ReportDiagnostic(wiringDiagnostic);
                initialized = false;
                return false;
            }
            if (initialized && !ReferenceEquals(settingsService, service))
            {
                ReportDiagnostic("Settings UI is already initialized with a different settings service.");
                return false;
            }

            settingsService = service;
            displayView?.Initialize(service, displayController, ReportDiagnostic);
            textView?.Initialize(service, textAutoController, ReportDiagnostic);
            audioView?.Initialize(service, audioController, ReportDiagnostic);
            gameplayView?.Initialize(service, gameplayController, ReportDiagnostic);
            if (!controlsView.Initialize(service, rebindService, ReportDiagnostic))
            {
                initialized = false;
                ReportDiagnostic("Controls Settings UI initialization failed.");
                return false;
            }
            initialized = true;
            ShowCategory(selectedCategory);
            RefreshFromAuthority();
            return true;
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (displayCategoryButton == null || textCategoryButton == null || audioCategoryButton == null || gameplayCategoryButton == null || controlsCategoryButton == null ||
                displayContent == null || textContent == null || audioContent == null || gameplayContent == null || controlsContent == null ||
                displayView == null || textView == null || audioView == null || gameplayView == null || controlsView == null)
            { diagnostic = "Settings Panel requires all category buttons, content roots, and category views."; return false; }
            if (!displayView.TryValidateWiring(out diagnostic) || !textView.TryValidateWiring(out diagnostic) || !audioView.TryValidateWiring(out diagnostic) || !gameplayView.TryValidateWiring(out diagnostic) || !controlsView.TryValidateWiring(out diagnostic)) return false;
            diagnostic = null; return true;
        }

        public void RefreshFromAuthority()
        {
            if (!initialized) return;
            var settings = settingsService.Current;
            var canWrite = settingsService.CanWrite;
            displayView?.Refresh(settings, canWrite);
            textView?.Refresh(settings, canWrite);
            audioView?.Refresh(settings, canWrite);
            gameplayView?.Refresh(settings, canWrite);
            controlsView?.Refresh(settings, canWrite);
            if (!canWrite) ReportDiagnostic("Settings are read-only to preserve the current file.");
        }

        public void PrepareForClose() => controlsView?.PrepareForClose();

        public void ReportDiagnostic(string diagnostic)
        {
            if (statusText != null) statusText.text = diagnostic ?? string.Empty;
        }

        public void ShowCategory(VNSettingsCategory category)
        {
            if (controlsView != null && controlsView.IsListening && category != VNSettingsCategory.Controls) controlsView.PrepareForClose();
            selectedCategory = category;
            SetActive(displayContent, category == VNSettingsCategory.Display);
            SetActive(textContent, category == VNSettingsCategory.Text);
            SetActive(audioContent, category == VNSettingsCategory.Audio);
            SetActive(gameplayContent, category == VNSettingsCategory.Gameplay);
            SetActive(controlsContent, category == VNSettingsCategory.Controls);
        }

        private void RegisterCategoryListeners(bool add)
        {
            SetListener(displayCategoryButton, HandleDisplayCategory, add);
            SetListener(textCategoryButton, HandleTextCategory, add);
            SetListener(audioCategoryButton, HandleAudioCategory, add);
            SetListener(gameplayCategoryButton, HandleGameplayCategory, add);
            SetListener(controlsCategoryButton, HandleControlsCategory, add);
        }

        private void HandleDisplayCategory() => ShowCategory(VNSettingsCategory.Display);
        private void HandleTextCategory() => ShowCategory(VNSettingsCategory.Text);
        private void HandleAudioCategory() => ShowCategory(VNSettingsCategory.Audio);
        private void HandleGameplayCategory() => ShowCategory(VNSettingsCategory.Gameplay);
        private void HandleControlsCategory() => ShowCategory(VNSettingsCategory.Controls);

        private static void SetListener(Button button, UnityEngine.Events.UnityAction action, bool add)
        {
            if (button == null) return;
            if (add) button.onClick.AddListener(action); else button.onClick.RemoveListener(action);
        }
        private static void SetActive(GameObject target, bool active) { if (target != null) target.SetActive(active); }
    }
}
