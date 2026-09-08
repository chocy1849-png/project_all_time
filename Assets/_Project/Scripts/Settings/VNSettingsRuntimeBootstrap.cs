using System;
using System.Collections.Generic;
using ProjectAllTime.VN.Dialogue;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Yarn.Unity;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>Scene-session composition owner; SettingsService remains the settings authority.</summary>
    // Yarn 3.2.7 builds LinePresenter.Typewriter in Awake and auto-starts the
    // runner in default-order Start. All scene Awake calls finish before this
    // Start; -1 applies persisted text before dialogue and keeps Mixer writes in Start.
    [DefaultExecutionOrder(-1)]
    [DisallowMultipleComponent]
    public sealed class VNSettingsRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNSettingsPanel settingsPanel;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private InputActionAsset inputActions;

        private VNSettingsRepository repository;
        private VNSettingsService settingsService;
        private VNDisplaySettingsController displayController;
        private VNTextAutoSettingsController textAutoController;
        private VNAudioSettingsController audioController;
        private VNGameplaySettingsController gameplayController;
        private VNInputRebindService rebindService;
        private bool initializationAttempted;

        public bool IsInitialized { get; private set; }
        public VNSettingsService SettingsService => settingsService;
        public string LastDiagnostic { get; private set; }
        public IVNScreenShakeGate ScreenShakeGate => gameplayController;

        private void Start()
        {
            Initialize();
            if (!string.IsNullOrEmpty(LastDiagnostic))
                Debug.LogError($"{nameof(VNSettingsRuntimeBootstrap)}: {LastDiagnostic}", this);
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (dialogueRunner == null) { diagnostic = "Dialogue Runner is required."; return false; }
            if (settingsPanel == null) { diagnostic = "Settings Panel is required."; return false; }
            if (audioMixer == null) { diagnostic = "Audio Mixer is required."; return false; }
            if (inputActions == null) { diagnostic = "Input Actions is required."; return false; }
            if (GetComponent<VNConvenienceController>() == null)
            { diagnostic = "A sibling VNConvenienceController is required on the same GameObject."; return false; }
            if (GetComponent<VNConvenienceInputRouter>() == null)
            { diagnostic = "A sibling VNConvenienceInputRouter is required on the same GameObject."; return false; }
            return settingsPanel.TryValidateWiring(out diagnostic);
        }

        // Private so callers cannot accidentally apply AudioMixer settings from Awake/OnEnable.
        private void Initialize()
        {
            if (initializationAttempted) return;
            initializationAttempted = true;
            if (!TryValidateWiring(out var diagnostic)) { LastDiagnostic = diagnostic; return; }

            var diagnostics = new List<string>();
            try
            {
                repository = new VNSettingsRepository();
                var service = new VNSettingsService(repository);
                service.Load();
                if (!string.IsNullOrEmpty(service.LastDiagnostic))
                    diagnostics.Add("Settings load: " + service.LastDiagnostic);
                CreateRuntimeOwners(service);

                Apply("Input", rebindService.TryApplyCurrentSettings, diagnostics);
                Apply("Text / Auto", textAutoController.TryApplyCurrentSettings, diagnostics);
                Apply("Gameplay", gameplayController.TryApplyCurrentSettings, diagnostics);
                Apply("Audio", audioController.TryApplyCurrentSettings, diagnostics);
                Apply("Display", displayController.TryApplyCurrentSettings, diagnostics);

                IsInitialized = settingsPanel.Initialize(settingsService, displayController,
                    textAutoController, audioController, gameplayController, rebindService);
                if (!IsInitialized) diagnostics.Add("Settings Panel initialization failed.");
            }
            catch (Exception exception)
            {
                diagnostics.Add("Settings initialization failed: " + exception.Message);
            }
            LastDiagnostic = diagnostics.Count == 0 ? null : string.Join("\n", diagnostics);
        }

        private void CreateRuntimeOwners(VNSettingsService service)
        {
            if (settingsService != null) return;
            settingsService = service;
            var convenience = GetComponent<VNConvenienceController>();
            rebindService = new VNInputRebindService(service, inputActions, GetComponent<VNConvenienceInputRouter>());
            textAutoController = new VNTextAutoSettingsController(service, dialogueRunner, convenience);
            gameplayController = new VNGameplaySettingsController(service, convenience);
            audioController = new VNAudioSettingsController(service, audioMixer);
            displayController = new VNDisplaySettingsController(service);
        }

        private delegate bool ApplySettings(out string diagnostic);

        private static void Apply(string category, ApplySettings apply, List<string> diagnostics)
        {
            try
            {
                if (!apply(out var diagnostic)) diagnostics.Add(category + ": " + diagnostic);
            }
            catch (Exception exception)
            {
                diagnostics.Add(category + ": " + exception.Message);
            }
        }

        private void OnDestroy()
        {
            rebindService?.Dispose();
            IsInitialized = false;
        }
    }
}
