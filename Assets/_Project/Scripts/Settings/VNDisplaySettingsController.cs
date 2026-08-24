using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>
    /// Narrow runtime seam for display queries and requests. Tests inject a fake
    /// implementation so they never affect the Editor Game View or desktop.
    /// </summary>
    public interface IVNDisplayRuntime
    {
        Resolution[] SupportedResolutions { get; }
        int NativeWidth { get; }
        int NativeHeight { get; }
        void SetResolution(int width, int height, FullScreenMode fullScreenMode);
    }

    /// <summary>
    /// Applies the persisted M7 display preference. It owns no scene lifecycle,
    /// UI, monitor selection, refresh-rate selection, or startup bootstrap.
    /// </summary>
    public sealed class VNDisplaySettingsController
    {
        public static readonly VNResolutionOption DefaultWindowedResolution = new(1920, 1080);

        private readonly VNSettingsService settingsService;
        private readonly IVNDisplayRuntime displayRuntime;

        public VNDisplaySettingsController(VNSettingsService settingsService)
            : this(settingsService, new VNUnityDisplayRuntime()) { }

        public VNDisplaySettingsController(VNSettingsService settingsService, IVNDisplayRuntime displayRuntime)
        {
            this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.displayRuntime = displayRuntime ?? throw new ArgumentNullException(nameof(displayRuntime));
        }

        public IReadOnlyList<VNResolutionOption> GetWindowedResolutionOptions()
        {
            var options = new HashSet<VNResolutionOption>();
            var sourceResolutions = displayRuntime.SupportedResolutions;
            if (sourceResolutions != null)
            {
                foreach (var resolution in sourceResolutions)
                {
                    if (VNResolutionOption.TryCreate(resolution.width, resolution.height, out var option))
                        options.Add(option);
                }
            }

            if (options.Count == 0) options.Add(DefaultWindowedResolution);

            var ordered = new List<VNResolutionOption>(options);
            ordered.Sort(CompareResolutionOptions);
            return ordered;
        }

        /// <summary>Returns the controller-owned effective Windowed selection without requesting a display change.</summary>
        public VNResolutionOption GetEffectiveWindowedResolution()
        {
            var settings = settingsService.Current;
            return ResolveWindowedResolution(settings.windowedWidth, settings.windowedHeight);
        }

        /// <summary>
        /// Startup/application seam. It does not load settings and does not need
        /// to rewrite them merely to request the effective display state.
        /// </summary>
        public bool TryApplyCurrentSettings(out string diagnostic)
        {
            var settings = settingsService.Current;
            if (settings.displayMode == VNSettingsDefaults.FullScreenWindowDisplayMode)
                return TryRequestNativeFullScreenWindow(out diagnostic);

            if (settings.displayMode == VNSettingsDefaults.WindowedDisplayMode)
            {
                var resolution = ResolveWindowedResolution(settings.windowedWidth, settings.windowedHeight);
                return TryRequestResolution(resolution, FullScreenMode.Windowed, out diagnostic);
            }

            diagnostic = "Current settings contain an unsupported display mode.";
            return false;
        }

        public bool TryUseFullScreenWindow(out string diagnostic)
        {
            if (!TryGetNativeFullScreenResolution(out var nativeResolution, out diagnostic)) return false;

            var replacement = settingsService.Current;
            replacement.displayMode = VNSettingsDefaults.FullScreenWindowDisplayMode;
            if (!settingsService.TrySave(replacement, out diagnostic)) return false;

            return TryRequestResolution(nativeResolution, FullScreenMode.FullScreenWindow, out diagnostic);
        }

        public bool TryUseWindowed(out string diagnostic)
        {
            var replacement = settingsService.Current;
            var resolution = ResolveWindowedResolution(replacement.windowedWidth, replacement.windowedHeight);
            replacement.displayMode = VNSettingsDefaults.WindowedDisplayMode;
            replacement.windowedWidth = resolution.Width;
            replacement.windowedHeight = resolution.Height;
            if (!settingsService.TrySave(replacement, out diagnostic)) return false;

            return TryRequestResolution(resolution, FullScreenMode.Windowed, out diagnostic);
        }

        public bool TrySetWindowedResolution(VNResolutionOption option, out string diagnostic)
        {
            if (!option.IsValid)
            {
                diagnostic = "The requested windowed resolution is invalid.";
                return false;
            }

            if (!ContainsWindowedOption(option))
            {
                diagnostic = "The requested windowed resolution is not currently available.";
                return false;
            }

            var replacement = settingsService.Current;
            replacement.displayMode = VNSettingsDefaults.WindowedDisplayMode;
            replacement.windowedWidth = option.Width;
            replacement.windowedHeight = option.Height;
            if (!settingsService.TrySave(replacement, out diagnostic)) return false;

            return TryRequestResolution(option, FullScreenMode.Windowed, out diagnostic);
        }

        private VNResolutionOption ResolveWindowedResolution(int storedWidth, int storedHeight)
        {
            var options = GetWindowedResolutionOptions();
            if (VNResolutionOption.TryCreate(storedWidth, storedHeight, out var storedResolution) && ContainsOption(options, storedResolution))
                return storedResolution;

            if (ContainsOption(options, DefaultWindowedResolution)) return DefaultWindowedResolution;

            var nearest = options[0];
            for (var index = 1; index < options.Count; index++)
            {
                if (CompareFallbackCandidate(options[index], nearest) < 0) nearest = options[index];
            }

            return nearest;
        }

        private bool TryRequestNativeFullScreenWindow(out string diagnostic)
        {
            if (!TryGetNativeFullScreenResolution(out var nativeResolution, out diagnostic)) return false;
            return TryRequestResolution(nativeResolution, FullScreenMode.FullScreenWindow, out diagnostic);
        }

        private bool TryGetNativeFullScreenResolution(out VNResolutionOption nativeResolution, out string diagnostic)
        {
            if (!VNResolutionOption.TryCreate(displayRuntime.NativeWidth, displayRuntime.NativeHeight, out nativeResolution))
            {
                diagnostic = "Primary display native dimensions are invalid.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        private bool TryRequestResolution(VNResolutionOption resolution, FullScreenMode fullScreenMode, out string diagnostic)
        {
            try
            {
                displayRuntime.SetResolution(resolution.Width, resolution.Height, fullScreenMode);
                diagnostic = null;
                return true;
            }
            catch (Exception)
            {
                diagnostic = "Display resolution request could not be issued.";
                return false;
            }
        }

        private bool ContainsWindowedOption(VNResolutionOption option)
        {
            return ContainsOption(GetWindowedResolutionOptions(), option);
        }

        private static bool ContainsOption(IReadOnlyList<VNResolutionOption> options, VNResolutionOption candidate)
        {
            for (var index = 0; index < options.Count; index++)
            {
                if (options[index] == candidate) return true;
            }

            return false;
        }

        private static int CompareResolutionOptions(VNResolutionOption left, VNResolutionOption right)
        {
            var widthComparison = left.Width.CompareTo(right.Width);
            return widthComparison != 0 ? widthComparison : left.Height.CompareTo(right.Height);
        }

        private static int CompareFallbackCandidate(VNResolutionOption left, VNResolutionOption right)
        {
            var leftDistance = SquaredDistanceFromDefault(left);
            var rightDistance = SquaredDistanceFromDefault(right);
            var distanceComparison = leftDistance.CompareTo(rightDistance);
            if (distanceComparison != 0) return distanceComparison;

            var leftAreaDifference = AbsoluteAreaDifferenceFromDefault(left);
            var rightAreaDifference = AbsoluteAreaDifferenceFromDefault(right);
            var areaComparison = leftAreaDifference.CompareTo(rightAreaDifference);
            if (areaComparison != 0) return areaComparison;

            return CompareResolutionOptions(left, right);
        }

        private static long SquaredDistanceFromDefault(VNResolutionOption option)
        {
            var widthDifference = (long)option.Width - DefaultWindowedResolution.Width;
            var heightDifference = (long)option.Height - DefaultWindowedResolution.Height;
            return (widthDifference * widthDifference) + (heightDifference * heightDifference);
        }

        private static long AbsoluteAreaDifferenceFromDefault(VNResolutionOption option)
        {
            var areaDifference = ((long)option.Width * option.Height) - ((long)DefaultWindowedResolution.Width * DefaultWindowedResolution.Height);
            return areaDifference < 0 ? -areaDifference : areaDifference;
        }

        private sealed class VNUnityDisplayRuntime : IVNDisplayRuntime
        {
            public Resolution[] SupportedResolutions => Screen.resolutions;

            public int NativeWidth
            {
                get
                {
                    var display = Display.main;
                    return display == null ? 0 : display.systemWidth;
                }
            }

            public int NativeHeight
            {
                get
                {
                    var display = Display.main;
                    return display == null ? 0 : display.systemHeight;
                }
            }

            public void SetResolution(int width, int height, FullScreenMode fullScreenMode)
            {
                Screen.SetResolution(width, height, fullScreenMode);
            }
        }
    }
}
