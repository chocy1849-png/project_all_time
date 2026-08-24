using System;
using UnityEngine;
using UnityEngine.Audio;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>Narrow AudioMixer seam used by M7 settings and its EditMode tests.</summary>
    public interface IVNAudioMixerRuntime
    {
        bool TryGetFloat(string parameterName, out float value);
        bool TrySetFloat(string parameterName, float value);
    }

    /// <summary>
    /// Applies global user attenuation through exposed mixer parameters. M4 owns
    /// AudioSource authored volumes, fades, and BGM source lifecycle separately.
    /// </summary>
    public sealed class VNAudioSettingsController
    {
        public const float MinimumVolumeDb = -80f;
        public const float MaximumVolumeDb = 0f;
        public const string MasterVolumeDbParameter = "MasterVolumeDb";
        public const string BgmVolumeDbParameter = "BgmVolumeDb";
        public const string SfxVolumeDbParameter = "SfxVolumeDb";
        public const string VoiceVolumeDbParameter = "VoiceVolumeDb";

        private readonly VNSettingsService settingsService;
        private readonly IVNAudioMixerRuntime mixerRuntime;

        public VNAudioSettingsController(VNSettingsService settingsService, AudioMixer audioMixer)
            : this(settingsService, new UnityAudioMixerRuntime(audioMixer)) { }

        public VNAudioSettingsController(VNSettingsService settingsService, IVNAudioMixerRuntime mixerRuntime)
        {
            this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.mixerRuntime = mixerRuntime ?? throw new ArgumentNullException(nameof(mixerRuntime));
        }

        public static float NormalizedToDecibels(float normalized)
        {
            if (float.IsNaN(normalized) || normalized <= 0f) return MinimumVolumeDb;
            if (float.IsPositiveInfinity(normalized) || normalized >= 1f) return MaximumVolumeDb;

            return Mathf.Clamp(20f * Mathf.Log10(normalized), MinimumVolumeDb, MaximumVolumeDb);
        }

        /// <summary>Confirms every required exposed attenuation parameter exists.</summary>
        public bool TryValidateMixerContract(out string diagnostic)
        {
            return TryReadMixerValues(out _, out diagnostic);
        }

        /// <summary>
        /// Read-only startup/application seam. Settings loading and Unity
        /// lifecycle composition are deliberately owned outside this controller.
        /// </summary>
        public bool TryApplyCurrentSettings(out string diagnostic)
        {
            if (!TryReadMixerValues(out var previousValues, out diagnostic)) return false;

            var settings = settingsService.Current;
            var requestedValues = new MixerValues(
                NormalizedToDecibels(settings.masterVolumeNormalized),
                NormalizedToDecibels(settings.bgmVolumeNormalized),
                NormalizedToDecibels(settings.sfxVolumeNormalized),
                NormalizedToDecibels(settings.voiceVolumeNormalized));

            return TryApplyAll(requestedValues, previousValues, out diagnostic);
        }

        public bool TrySetMasterVolumeNormalized(float normalized, out string diagnostic)
        {
            return TrySetNormalizedVolume(normalized, VolumeKind.Master, out diagnostic);
        }

        public bool TrySetBgmVolumeNormalized(float normalized, out string diagnostic)
        {
            return TrySetNormalizedVolume(normalized, VolumeKind.Bgm, out diagnostic);
        }

        public bool TrySetSfxVolumeNormalized(float normalized, out string diagnostic)
        {
            return TrySetNormalizedVolume(normalized, VolumeKind.Sfx, out diagnostic);
        }

        public bool TrySetVoiceVolumeNormalized(float normalized, out string diagnostic)
        {
            return TrySetNormalizedVolume(normalized, VolumeKind.Voice, out diagnostic);
        }

        private bool TrySetNormalizedVolume(float normalized, VolumeKind kind, out string diagnostic)
        {
            if (float.IsNaN(normalized) || float.IsInfinity(normalized))
            {
                diagnostic = "Audio volume must be finite.";
                return false;
            }

            if (!TryValidateMixerContract(out diagnostic)) return false;

            var replacement = settingsService.Current;
            var clamped = Mathf.Clamp01(normalized);
            switch (kind)
            {
                case VolumeKind.Master:
                    replacement.masterVolumeNormalized = clamped;
                    break;
                case VolumeKind.Bgm:
                    replacement.bgmVolumeNormalized = clamped;
                    break;
                case VolumeKind.Sfx:
                    replacement.sfxVolumeNormalized = clamped;
                    break;
                case VolumeKind.Voice:
                    replacement.voiceVolumeNormalized = clamped;
                    break;
                default:
                    diagnostic = "Unknown audio volume category.";
                    return false;
            }

            if (!settingsService.TrySave(replacement, out diagnostic)) return false;

            var parameterName = ParameterNameFor(kind);
            if (mixerRuntime.TrySetFloat(parameterName, NormalizedToDecibels(clamped)))
            {
                diagnostic = null;
                return true;
            }

            diagnostic = $"Mixer parameter '{parameterName}' could not be updated after settings were saved.";
            return false;
        }

        private bool TryReadMixerValues(out MixerValues values, out string diagnostic)
        {
            values = default;
            if (!mixerRuntime.TryGetFloat(MasterVolumeDbParameter, out var master))
            {
                diagnostic = $"Required exposed mixer parameter '{MasterVolumeDbParameter}' is missing.";
                return false;
            }

            if (!mixerRuntime.TryGetFloat(BgmVolumeDbParameter, out var bgm))
            {
                diagnostic = $"Required exposed mixer parameter '{BgmVolumeDbParameter}' is missing.";
                return false;
            }

            if (!mixerRuntime.TryGetFloat(SfxVolumeDbParameter, out var sfx))
            {
                diagnostic = $"Required exposed mixer parameter '{SfxVolumeDbParameter}' is missing.";
                return false;
            }

            if (!mixerRuntime.TryGetFloat(VoiceVolumeDbParameter, out var voice))
            {
                diagnostic = $"Required exposed mixer parameter '{VoiceVolumeDbParameter}' is missing.";
                return false;
            }

            values = new MixerValues(master, bgm, sfx, voice);
            diagnostic = null;
            return true;
        }

        private bool TryApplyAll(MixerValues requestedValues, MixerValues previousValues, out string diagnostic)
        {
            var appliedCount = 0;
            if (mixerRuntime.TrySetFloat(MasterVolumeDbParameter, requestedValues.Master)) appliedCount++;
            else return FailApplyAll(MasterVolumeDbParameter, appliedCount, previousValues, out diagnostic);

            if (mixerRuntime.TrySetFloat(BgmVolumeDbParameter, requestedValues.Bgm)) appliedCount++;
            else return FailApplyAll(BgmVolumeDbParameter, appliedCount, previousValues, out diagnostic);

            if (mixerRuntime.TrySetFloat(SfxVolumeDbParameter, requestedValues.Sfx)) appliedCount++;
            else return FailApplyAll(SfxVolumeDbParameter, appliedCount, previousValues, out diagnostic);

            if (mixerRuntime.TrySetFloat(VoiceVolumeDbParameter, requestedValues.Voice)) return Success(out diagnostic);
            return FailApplyAll(VoiceVolumeDbParameter, appliedCount, previousValues, out diagnostic);
        }

        private bool FailApplyAll(string failedParameter, int appliedCount, MixerValues previousValues, out string diagnostic)
        {
            RestoreAppliedValues(appliedCount, previousValues);
            diagnostic = $"Mixer parameter '{failedParameter}' could not be updated; previously changed parameters were restored where possible.";
            return false;
        }

        private void RestoreAppliedValues(int appliedCount, MixerValues previousValues)
        {
            if (appliedCount >= 1) mixerRuntime.TrySetFloat(MasterVolumeDbParameter, previousValues.Master);
            if (appliedCount >= 2) mixerRuntime.TrySetFloat(BgmVolumeDbParameter, previousValues.Bgm);
            if (appliedCount >= 3) mixerRuntime.TrySetFloat(SfxVolumeDbParameter, previousValues.Sfx);
        }

        private static bool Success(out string diagnostic)
        {
            diagnostic = null;
            return true;
        }

        private static string ParameterNameFor(VolumeKind kind)
        {
            switch (kind)
            {
                case VolumeKind.Master: return MasterVolumeDbParameter;
                case VolumeKind.Bgm: return BgmVolumeDbParameter;
                case VolumeKind.Sfx: return SfxVolumeDbParameter;
                case VolumeKind.Voice: return VoiceVolumeDbParameter;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private enum VolumeKind { Master, Bgm, Sfx, Voice }

        private readonly struct MixerValues
        {
            public float Master { get; }
            public float Bgm { get; }
            public float Sfx { get; }
            public float Voice { get; }

            public MixerValues(float master, float bgm, float sfx, float voice)
            {
                Master = master;
                Bgm = bgm;
                Sfx = sfx;
                Voice = voice;
            }
        }

        private sealed class UnityAudioMixerRuntime : IVNAudioMixerRuntime
        {
            private readonly AudioMixer audioMixer;

            public UnityAudioMixerRuntime(AudioMixer audioMixer)
            {
                this.audioMixer = audioMixer ?? throw new ArgumentNullException(nameof(audioMixer));
            }

            public bool TryGetFloat(string parameterName, out float value) => audioMixer.GetFloat(parameterName, out value);
            public bool TrySetFloat(string parameterName, float value) => audioMixer.SetFloat(parameterName, value);
        }
    }
}
