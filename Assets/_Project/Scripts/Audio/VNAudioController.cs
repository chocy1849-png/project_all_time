using System.Collections;
using UnityEngine;

namespace ProjectAllTime.VN.Audio
{
    public sealed class VNAudioController : MonoBehaviour
    {
        [SerializeField] private VNAudioCatalog catalog;
        [SerializeField] private AudioSource bgmSourceA;
        [SerializeField] private AudioSource bgmSourceB;
        [SerializeField] private AudioSource sfxSource;

        private bool sourceAIsActive = true;
        private bool activeBgmIsPaused;
        private float pausedBgmVolume = 1f;

        public bool PlayBgm(string bgmId)
        {
            if (!TryResolveBgm(bgmId, out var entry) || !TryGetBgmSources(out var active, out var inactive)) return false;

            StopAndReset(inactive);
            StopAndReset(active);
            ConfigureBgm(active, entry, entry.DefaultVolume);
            active.Play();
            activeBgmIsPaused = false;
            pausedBgmVolume = entry.DefaultVolume;
            return true;
        }

        public IEnumerator CrossfadeBgm(string bgmId, float duration)
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
        }

        public IEnumerator PauseBgm(float duration)
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

        public IEnumerator ResumeBgm(float duration)
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

        public IEnumerator FadeStopBgm(float duration)
        {
            if (!IsValidDuration(duration, "BGM stop") || !TryGetBgmSources(out var active, out _)) yield break;
            if (active.clip == null)
            {
                Debug.LogError("Cannot stop BGM because no active BGM is configured.", this);
                yield break;
            }

            if (!activeBgmIsPaused)
                yield return Fade(active, active.volume, 0f, duration);

            StopAndReset(active);
            activeBgmIsPaused = false;
            pausedBgmVolume = 1f;
        }

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
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
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
            if (!float.IsNaN(duration) && !float.IsInfinity(duration) && duration >= 0f) return true;
            Debug.LogError($"{operation} duration must be a finite value greater than or equal to zero.", this);
            return false;
        }
    }
}
