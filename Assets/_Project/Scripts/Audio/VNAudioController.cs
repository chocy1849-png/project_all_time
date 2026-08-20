using System;
using System.Collections;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;

namespace ProjectAllTime.VN.Audio
{
    /// <summary>
    /// Validated logical BGM restore input. The A/B source role and transient
    /// fades are intentionally absent because they are M4 implementation detail.
    /// </summary>
    public sealed class VNAudioRestorePlan
    {
        public string BgmId { get; }
        public float PlaybackSeconds { get; }
        internal VNBgmCatalogEntry Entry { get; }
        internal int TimeSamples { get; }

        internal VNAudioRestorePlan(string bgmId, float playbackSeconds, VNBgmCatalogEntry entry, int timeSamples)
        {
            BgmId = bgmId;
            PlaybackSeconds = playbackSeconds;
            Entry = entry;
            TimeSamples = timeSamples;
        }
    }

    public sealed class VNAudioController : MonoBehaviour
    {
        [SerializeField] private VNAudioCatalog catalog;
        [SerializeField] private AudioSource bgmSourceA;
        [SerializeField] private AudioSource bgmSourceB;
        [SerializeField] private AudioSource sfxSource;

        private bool sourceAIsActive = true;
        private bool activeBgmIsPaused;
        private float pausedBgmVolume = 1f;
        private string currentBgmId = string.Empty;
        private int activeBgmTransitionOperations;

        public string CurrentBgmId => currentBgmId ?? string.Empty;
        public bool IsBgmTransitionActive => activeBgmTransitionOperations > 0;

        public bool PlayBgm(string bgmId)
        {
            if (!TryResolveBgm(bgmId, out var entry) || !TryGetBgmSources(out var active, out var inactive)) return false;

            StopAndReset(inactive);
            StopAndReset(active);
            ConfigureBgm(active, entry, entry.DefaultVolume);
            active.Play();
            activeBgmIsPaused = false;
            pausedBgmVolume = entry.DefaultVolume;
            currentBgmId = bgmId;
            return true;
        }

        public IEnumerator CrossfadeBgm(string bgmId, float duration) => TrackBgmTransition(CrossfadeBgmRoutine(bgmId, duration));

        public IEnumerator PauseBgm(float duration) => TrackBgmTransition(PauseBgmRoutine(duration));

        public IEnumerator ResumeBgm(float duration) => TrackBgmTransition(ResumeBgmRoutine(duration));

        public IEnumerator FadeStopBgm(float duration) => TrackBgmTransition(FadeStopBgmRoutine(duration));

        public bool PlaySfx(string sfxId)
        {
            if (catalog == null || !catalog.TryGetSfx(sfxId, out var entry))
            {
                Debug.LogError($"Cannot play unknown SFX '{sfxId}'.", this);
                return false;
            }

            if (sfxSource == null)
            {
                Debug.LogError("VN Audio Controller requires an SFX AudioSource reference.", this);
                return false;
            }

            sfxSource.PlayOneShot(entry.Clip, entry.DefaultVolume);
            return true;
        }

        /// <summary>Validates a logical BGM ID without exposing source roles.</summary>
        public bool TryValidateBgmId(string bgmId, out string diagnostic)
        {
            if (bgmId == null)
            {
                diagnostic = "BGM ID is missing.";
                return false;
            }

            if (bgmId.Length == 0)
            {
                diagnostic = null;
                return true;
            }

            if (catalog != null && catalog.TryGetBgm(bgmId, out var entry) && entry.Clip != null)
            {
                diagnostic = null;
                return true;
            }

            diagnostic = $"BGM '{bgmId}' is not available in the audio catalog.";
            return false;
        }

        /// <summary>
        /// Captures the logical BGM only when no M4 audio operation is between
        /// stable source states. Paused position uses timeSamples/frequency
        /// whenever usable, because AudioSource.time may report zero paused.
        /// </summary>
        public bool TryCaptureStableState(out AudioState audioState, out string diagnostic)
        {
            audioState = new AudioState { bgmId = string.Empty, playbackSeconds = 0f };
            if (IsBgmTransitionActive)
            {
                diagnostic = "BGM state is temporarily unavailable while an audio transition is active.";
                return false;
            }

            if (string.IsNullOrEmpty(CurrentBgmId))
            {
                diagnostic = null;
                return true;
            }

            if (!TryGetBgmSources(out var active, out _) || active.clip == null || !TryGetPlaybackSeconds(active, out var playbackSeconds))
            {
                diagnostic = "Current logical BGM cannot be represented safely.";
                return false;
            }

            audioState.bgmId = CurrentBgmId;
            audioState.playbackSeconds = playbackSeconds;
            diagnostic = null;
            return true;
        }

        /// <summary>
        /// Performs complete non-mutating BGM validation and deterministic
        /// playback normalization for a later immediate load restore.
        /// </summary>
        public bool TryPrepareRestore(AudioState audioState, out VNAudioRestorePlan restorePlan, out string diagnostic)
        {
            restorePlan = null;
            if (audioState == null || audioState.bgmId == null || !IsNonNegativeFinite(audioState.playbackSeconds))
            {
                diagnostic = "Saved BGM state is invalid.";
                return false;
            }

            if (!HasConfiguredBgmSources())
            {
                diagnostic = "VN Audio Controller requires two distinct BGM AudioSource references for restore.";
                return false;
            }

            if (string.IsNullOrEmpty(audioState.bgmId))
            {
                restorePlan = new VNAudioRestorePlan(string.Empty, 0f, null, 0);
                diagnostic = null;
                return true;
            }

            if (!TryValidateBgmId(audioState.bgmId, out _) || catalog == null || !catalog.TryGetBgm(audioState.bgmId, out var entry) || !TryNormalizePlayback(entry, audioState.playbackSeconds, out var normalizedSeconds, out var timeSamples))
            {
                diagnostic = $"Saved BGM '{audioState.bgmId}' cannot be restored with the current audio catalog.";
                return false;
            }

            restorePlan = new VNAudioRestorePlan(audioState.bgmId, normalizedSeconds, entry, timeSamples);
            diagnostic = null;
            return true;
        }

        /// <summary>
        /// Cancels M4 fades, stops stale one-shot SFX, and restores one
        /// canonical BGM source. Voice is intentionally owned by Yarn's
        /// presenter lifecycle and is not touched here.
        /// </summary>
        public bool RestorePreparedState(VNAudioRestorePlan restorePlan, out string diagnostic)
        {
            if (restorePlan == null)
            {
                diagnostic = "A validated BGM restore plan is required.";
                return false;
            }

            if (string.IsNullOrEmpty(restorePlan.BgmId))
            {
                diagnostic = null;
                return true;
            }

            if (restorePlan.Entry == null || bgmSourceA == null)
            {
                diagnostic = "Validated BGM restore dependencies are no longer available.";
                return false;
            }

            ConfigureBgm(bgmSourceA, restorePlan.Entry, restorePlan.Entry.DefaultVolume);
            if (bgmSourceA.clip.samples > 0) bgmSourceA.timeSamples = Mathf.Clamp(restorePlan.TimeSamples, 0, bgmSourceA.clip.samples - 1);
            bgmSourceA.Play();
            sourceAIsActive = true;
            activeBgmIsPaused = false;
            pausedBgmVolume = restorePlan.Entry.DefaultVolume;
            currentBgmId = restorePlan.BgmId;
            diagnostic = null;
            return true;
        }

        /// <summary>Stops transient SFX and both BGM sources without touching voice.</summary>
        public void NormalizeTransientForLoad()
        {
            StopAllCoroutines();
            activeBgmTransitionOperations = 0;
            StopAndReset(bgmSourceA);
            StopAndReset(bgmSourceB);
            if (sfxSource != null) sfxSource.Stop();
            sourceAIsActive = true;
            activeBgmIsPaused = false;
            pausedBgmVolume = 1f;
            currentBgmId = string.Empty;
        }

        private IEnumerator CrossfadeBgmRoutine(string bgmId, float duration)
        {
            if (!IsValidDuration(duration, "BGM crossfade") || !TryResolveBgm(bgmId, out var entry) || !TryGetBgmSources(out var current, out var target)) yield break;
            if (activeBgmIsPaused)
            {
                Debug.LogError("Cannot crossfade BGM while the active BGM is paused. Resume or fade-stop it first.", this);
                yield break;
            }

            if (current.clip == null || !current.isPlaying || duration <= 0f)
            {
                StopAndReset(current);
                StopAndReset(target);
                ConfigureBgm(target, entry, entry.DefaultVolume);
                target.Play();
                SwapActiveSource();
                activeBgmIsPaused = false;
                pausedBgmVolume = entry.DefaultVolume;
                currentBgmId = bgmId;
                yield break;
            }

            StopAndReset(target);
            var currentVolume = current.volume;
            ConfigureBgm(target, entry, 0f);
            target.Play();
            yield return FadePair(current, currentVolume, 0f, target, 0f, entry.DefaultVolume, duration);

            StopAndReset(current);
            target.volume = entry.DefaultVolume;
            SwapActiveSource();
            activeBgmIsPaused = false;
            pausedBgmVolume = entry.DefaultVolume;
            currentBgmId = bgmId;
        }

        private IEnumerator PauseBgmRoutine(float duration)
        {
            if (!IsValidDuration(duration, "BGM pause") || !TryGetActiveBgm(out var active)) yield break;
            if (activeBgmIsPaused)
            {
                Debug.LogError("Cannot pause BGM because it is already paused.", this);
                yield break;
            }

            pausedBgmVolume = active.volume;
            yield return Fade(active, pausedBgmVolume, 0f, duration);
            active.Pause();
            activeBgmIsPaused = true;
        }

        private IEnumerator ResumeBgmRoutine(float duration)
        {
            if (!IsValidDuration(duration, "BGM resume") || !TryGetBgmSources(out var active, out _)) yield break;
            if (!activeBgmIsPaused || active.clip == null)
            {
                Debug.LogError("Cannot resume BGM because no active BGM is paused.", this);
                yield break;
            }

            active.UnPause();
            yield return Fade(active, active.volume, pausedBgmVolume, duration);
            active.volume = pausedBgmVolume;
            activeBgmIsPaused = false;
        }

        private IEnumerator FadeStopBgmRoutine(float duration)
        {
            if (!IsValidDuration(duration, "BGM stop") || !TryGetBgmSources(out var active, out _)) yield break;
            if (active.clip == null)
            {
                Debug.LogError("Cannot stop BGM because no active BGM is configured.", this);
                yield break;
            }

            if (!activeBgmIsPaused) yield return Fade(active, active.volume, 0f, duration);

            StopAndReset(active);
            activeBgmIsPaused = false;
            pausedBgmVolume = 1f;
            currentBgmId = string.Empty;
        }

        private IEnumerator TrackBgmTransition(IEnumerator operation)
        {
            activeBgmTransitionOperations++;
            try
            {
                while (operation != null && operation.MoveNext()) yield return operation.Current;
            }
            finally
            {
                activeBgmTransitionOperations = Mathf.Max(0, activeBgmTransitionOperations - 1);
            }
        }

        private bool TryResolveBgm(string bgmId, out VNBgmCatalogEntry entry)
        {
            entry = null;
            if (catalog != null && catalog.TryGetBgm(bgmId, out entry)) return true;
            Debug.LogError($"Cannot resolve unknown BGM '{bgmId}'.", this);
            return false;
        }

        private bool TryGetBgmSources(out AudioSource active, out AudioSource inactive)
        {
            active = sourceAIsActive ? bgmSourceA : bgmSourceB;
            inactive = sourceAIsActive ? bgmSourceB : bgmSourceA;
            if (active != null && inactive != null && active != inactive) return true;

            Debug.LogError("VN Audio Controller requires two distinct BGM AudioSource references.", this);
            return false;
        }

        private bool HasConfiguredBgmSources() => bgmSourceA != null && bgmSourceB != null && bgmSourceA != bgmSourceB;

        private bool TryGetActiveBgm(out AudioSource active)
        {
            active = sourceAIsActive ? bgmSourceA : bgmSourceB;
            if (active != null && active.clip != null && active.isPlaying) return true;

            Debug.LogError("Cannot control BGM because no active BGM is playing.", this);
            return false;
        }

        private void SwapActiveSource() => sourceAIsActive = !sourceAIsActive;

        private static void ConfigureBgm(AudioSource source, VNBgmCatalogEntry entry, float volume)
        {
            source.clip = entry.Clip;
            source.loop = entry.Loop;
            source.volume = volume;
        }

        private static void StopAndReset(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
        }

        private static bool TryGetPlaybackSeconds(AudioSource source, out float playbackSeconds)
        {
            playbackSeconds = 0f;
            var clip = source == null ? null : source.clip;
            if (clip == null) return false;

            if (clip.samples > 0 && clip.frequency > 0)
            {
                var sample = Mathf.Clamp(source.timeSamples, 0, clip.samples - 1);
                playbackSeconds = sample / (float)clip.frequency;
                return IsNonNegativeFinite(playbackSeconds);
            }

            playbackSeconds = source.time;
            return IsNonNegativeFinite(playbackSeconds);
        }

        private static bool TryNormalizePlayback(VNBgmCatalogEntry entry, float requestedSeconds, out float normalizedSeconds, out int timeSamples)
        {
            normalizedSeconds = 0f;
            timeSamples = 0;
            var clip = entry == null ? null : entry.Clip;
            if (clip == null || clip.samples <= 0 || clip.frequency <= 0 || !IsNonNegativeFinite(requestedSeconds)) return false;

            var duration = clip.samples / (float)clip.frequency;
            if (!IsNonNegativeFinite(duration) || duration <= 0f) return false;

            if (entry.Loop)
            {
                normalizedSeconds = requestedSeconds % duration;
                if (!IsNonNegativeFinite(normalizedSeconds) || normalizedSeconds >= duration) normalizedSeconds = 0f;
            }
            else
            {
                var lastPlayableSecond = (clip.samples - 1) / (float)clip.frequency;
                normalizedSeconds = Mathf.Clamp(requestedSeconds, 0f, Mathf.Max(0f, lastPlayableSecond));
            }

            timeSamples = Mathf.Clamp(Mathf.FloorToInt(normalizedSeconds * clip.frequency), 0, clip.samples - 1);
            normalizedSeconds = timeSamples / (float)clip.frequency;
            return true;
        }

        private static IEnumerator FadePair(AudioSource first, float firstStart, float firstTarget, AudioSource second, float secondStart, float secondTarget, float duration)
        {
            if (duration <= 0f)
            {
                first.volume = firstTarget;
                second.volume = secondTarget;
                yield break;
            }

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                var progress = elapsed / duration;
                first.volume = Mathf.Lerp(firstStart, firstTarget, progress);
                second.volume = Mathf.Lerp(secondStart, secondTarget, progress);
                yield return null;
            }

            first.volume = firstTarget;
            second.volume = secondTarget;
        }

        private static IEnumerator Fade(AudioSource source, float start, float target, float duration)
        {
            if (duration <= 0f)
            {
                source.volume = target;
                yield break;
            }

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            source.volume = target;
        }

        private bool IsValidDuration(float duration, string operation)
        {
            if (IsNonNegativeFinite(duration)) return true;
            Debug.LogError($"{operation} duration must be a finite value greater than or equal to zero.", this);
            return false;
        }

        private static bool IsNonNegativeFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }
}
