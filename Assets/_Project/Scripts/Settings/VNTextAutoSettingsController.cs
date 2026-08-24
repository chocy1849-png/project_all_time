using System;
using ProjectAllTime.VN.Dialogue;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>
    /// Applies persisted text and Auto speed preferences to the existing Yarn
    /// LinePresenter and M6 convenience controller. It owns no scene lifecycle
    /// or settings UI and never loads settings itself.
    /// </summary>
    public sealed class VNTextAutoSettingsController
    {
        public const int MinimumTextSpeedLps = 20;
        public const int DefaultTextSpeedLps = 60;
        public const int MaximumTextSpeedLps = 120;
        public const float SlowestAutoDelayMultiplier = 1.5f;
        public const float FastestAutoDelayMultiplier = 0.5f;

        private readonly VNSettingsService settingsService;
        private readonly DialogueRunner dialogueRunner;
        private readonly VNConvenienceController convenienceController;

        public VNTextAutoSettingsController(
            VNSettingsService settingsService,
            DialogueRunner dialogueRunner,
            VNConvenienceController convenienceController)
        {
            this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.dialogueRunner = dialogueRunner ?? throw new ArgumentNullException(nameof(dialogueRunner));
            this.convenienceController = convenienceController ?? throw new ArgumentNullException(nameof(convenienceController));
        }

        public static int ClampTextSpeedLps(int value) => Mathf.Clamp(value, MinimumTextSpeedLps, MaximumTextSpeedLps);

        public static float ToAutoDelayMultiplier(float normalized)
        {
            return Mathf.Lerp(SlowestAutoDelayMultiplier, FastestAutoDelayMultiplier, normalized);
        }

        /// <summary>
        /// Applies the already-effective settings without writing them. A text
        /// change affects subsequent Yarn lines; it never restarts a current line.
        /// </summary>
        public bool TryApplyCurrentSettings(out string diagnostic)
        {
            if (!TryResolveTextTarget(out var linePresenter, out var letterTypewriter, out diagnostic)) return false;

            var settings = settingsService.Current;
            var effectiveTextSpeed = ClampTextSpeedLps(settings.textSpeedLps);
            var autoMultiplier = ToAutoDelayMultiplier(settings.autoSpeedNormalized);
            ApplyTextSpeed(linePresenter, letterTypewriter, effectiveTextSpeed);
            if (!convenienceController.TrySetAutoDelayMultiplier(autoMultiplier))
            {
                diagnostic = "The Auto delay multiplier could not be applied.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        public bool TrySetTextSpeedLps(int requestedLps, out string diagnostic)
        {
            if (!TryResolveTextTarget(out var linePresenter, out var letterTypewriter, out diagnostic)) return false;

            var replacement = settingsService.Current;
            replacement.textSpeedLps = ClampTextSpeedLps(requestedLps);
            if (!settingsService.TrySave(replacement, out diagnostic)) return false;

            ApplyTextSpeed(linePresenter, letterTypewriter, replacement.textSpeedLps);
            diagnostic = null;
            return true;
        }

        public bool TrySetAutoSpeedNormalized(float normalized, out string diagnostic)
        {
            if (float.IsNaN(normalized) || float.IsInfinity(normalized))
            {
                diagnostic = "Auto speed must be finite.";
                return false;
            }

            var replacement = settingsService.Current;
            replacement.autoSpeedNormalized = Mathf.Clamp01(normalized);
            if (!settingsService.TrySave(replacement, out diagnostic)) return false;

            if (!convenienceController.TrySetAutoDelayMultiplier(ToAutoDelayMultiplier(replacement.autoSpeedNormalized)))
            {
                diagnostic = "The Auto delay multiplier could not be applied.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        private bool TryResolveTextTarget(out LinePresenter linePresenter, out LetterTypewriter letterTypewriter, out string diagnostic)
        {
            linePresenter = null;
            letterTypewriter = null;
            if (!VNAuthoritativeLinePresenterResolver.TryResolve(dialogueRunner, out var resolvedPresenter, out diagnostic)) return false;

            if (!(resolvedPresenter.Typewriter is LetterTypewriter resolvedTypewriter))
            {
                diagnostic = "The authoritative LinePresenter must use an active LetterTypewriter for M7 text speed.";
                return false;
            }

            linePresenter = resolvedPresenter;
            letterTypewriter = resolvedTypewriter;
            return true;
        }

        private static void ApplyTextSpeed(LinePresenter linePresenter, LetterTypewriter letterTypewriter, int effectiveTextSpeed)
        {
            linePresenter.lettersPerSecond = effectiveTextSpeed;
            letterTypewriter.CharactersPerSecond = effectiveTextSpeed;
        }
    }
}
