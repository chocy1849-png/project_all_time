using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    [DisallowMultipleComponent]
    public sealed class VNAudioSettingsView : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider voiceSlider;
        [SerializeField] private TMP_Text masterValueLabel;
        [SerializeField] private TMP_Text bgmValueLabel;
        [SerializeField] private TMP_Text sfxValueLabel;
        [SerializeField] private TMP_Text voiceValueLabel;
        [SerializeField] private VNSettingsSliderCommit masterCommit;
        [SerializeField] private VNSettingsSliderCommit bgmCommit;
        [SerializeField] private VNSettingsSliderCommit sfxCommit;
        [SerializeField] private VNSettingsSliderCommit voiceCommit;
        private VNSettingsService settingsService;
        private VNAudioSettingsController controller;
        private Action<string> report;
        private bool initialized;

        private void OnEnable() => RegisterListeners(true);
        private void OnDisable() => RegisterListeners(false);

        public void Initialize(VNSettingsService service, VNAudioSettingsController audioController, Action<string> diagnostic)
        {
            settingsService = service ?? throw new ArgumentNullException(nameof(service));
            controller = audioController ?? throw new ArgumentNullException(nameof(audioController));
            report = diagnostic;
            initialized = true;
            ConfigureSlider(masterSlider, masterCommit); ConfigureSlider(bgmSlider, bgmCommit); ConfigureSlider(sfxSlider, sfxCommit); ConfigureSlider(voiceSlider, voiceCommit);
            Refresh(service.Current, service.CanWrite);
        }

        public void Refresh(VNSettingsData settings, bool canWrite)
        {
            if (!initialized) return;
            var mixerAvailable = controller.TryValidateMixerContract(out var diagnostic);
            if (!mixerAvailable) report?.Invoke(diagnostic);
            RefreshSlider(masterSlider, settings.masterVolumeNormalized, canWrite && mixerAvailable); UpdateLabel(masterValueLabel, settings.masterVolumeNormalized);
            RefreshSlider(bgmSlider, settings.bgmVolumeNormalized, canWrite && mixerAvailable); UpdateLabel(bgmValueLabel, settings.bgmVolumeNormalized);
            RefreshSlider(sfxSlider, settings.sfxVolumeNormalized, canWrite && mixerAvailable); UpdateLabel(sfxValueLabel, settings.sfxVolumeNormalized);
            RefreshSlider(voiceSlider, settings.voiceVolumeNormalized, canWrite && mixerAvailable); UpdateLabel(voiceValueLabel, settings.voiceVolumeNormalized);
            masterCommit?.SyncAuthoritativeValue(settings.masterVolumeNormalized); bgmCommit?.SyncAuthoritativeValue(settings.bgmVolumeNormalized); sfxCommit?.SyncAuthoritativeValue(settings.sfxVolumeNormalized); voiceCommit?.SyncAuthoritativeValue(settings.voiceVolumeNormalized);
        }

        private void CommitMaster(float value) => Commit(value, controller.TrySetMasterVolumeNormalized);
        private void CommitBgm(float value) => Commit(value, controller.TrySetBgmVolumeNormalized);
        private void CommitSfx(float value) => Commit(value, controller.TrySetSfxVolumeNormalized);
        private void CommitVoice(float value) => Commit(value, controller.TrySetVoiceVolumeNormalized);
        private delegate bool VolumeMutation(float value, out string diagnostic);
        private void Commit(float value, VolumeMutation mutation)
        {
            if (!initialized || !settingsService.CanWrite) return;
            var success = mutation(value, out var diagnostic);
            if (!success || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }
        private static void ConfigureSlider(Slider slider, VNSettingsSliderCommit commit)
        {
            if (slider == null) return;
            slider.minValue = 0f; slider.maxValue = 1f;
            commit?.Initialize(slider);
        }
        private static void RefreshSlider(Slider slider, float value, bool interactable) { if (slider != null) { slider.SetValueWithoutNotify(value); slider.interactable = interactable; } }
        private static void UpdateLabel(TMP_Text label, float value) { if (label != null) label.text = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%"; }
        private void RegisterListeners(bool add)
        {
            SetListener(masterCommit, CommitMaster, add); SetListener(bgmCommit, CommitBgm, add); SetListener(sfxCommit, CommitSfx, add); SetListener(voiceCommit, CommitVoice, add);
            SetPreviewListener(masterSlider, PreviewMaster, add); SetPreviewListener(bgmSlider, PreviewBgm, add); SetPreviewListener(sfxSlider, PreviewSfx, add); SetPreviewListener(voiceSlider, PreviewVoice, add);
        }
        private void PreviewMaster(float value) => UpdateLabel(masterValueLabel, value);
        private void PreviewBgm(float value) => UpdateLabel(bgmValueLabel, value);
        private void PreviewSfx(float value) => UpdateLabel(sfxValueLabel, value);
        private void PreviewVoice(float value) => UpdateLabel(voiceValueLabel, value);
        private static void SetPreviewListener(Slider slider, UnityEngine.Events.UnityAction<float> callback, bool add)
        {
            if (slider == null) return;
            if (add) slider.onValueChanged.AddListener(callback); else slider.onValueChanged.RemoveListener(callback);
        }
        public bool TryValidateWiring(out string diagnostic)
        {
            if (masterSlider == null || bgmSlider == null || sfxSlider == null || voiceSlider == null || masterCommit == null || bgmCommit == null || sfxCommit == null || voiceCommit == null) { diagnostic = "Audio Settings UI requires four Sliders and commit seams."; return false; }
            if (!masterCommit.TryValidateWiring(out diagnostic) || !bgmCommit.TryValidateWiring(out diagnostic) || !sfxCommit.TryValidateWiring(out diagnostic) || !voiceCommit.TryValidateWiring(out diagnostic)) return false;
            diagnostic = null; return true;
        }
        private static void SetListener(VNSettingsSliderCommit commit, Action<float> callback, bool add)
        {
            if (commit == null) return;
            if (add) commit.CommitRequested += callback; else commit.CommitRequested -= callback;
        }
    }
}
