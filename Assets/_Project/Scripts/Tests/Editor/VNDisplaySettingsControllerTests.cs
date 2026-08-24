using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProjectAllTime.VN.Settings;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNDisplaySettingsControllerTests
    {
        private string temporaryRoot;
        private VNSettingsRepository repository;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M7DisplayTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSettingsRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void ResolutionOptions_UseWidthHeightIdentity_DedupeInvalidEntriesAndSort()
        {
            var controller = CreateController(CreateService(), new FakeDisplayRuntime(1920, 1080,
                ResolutionEntry(1920, 1080),
                ResolutionEntry(1280, 720),
                ResolutionEntry(1920, 1080),
                ResolutionEntry(0, 720),
                ResolutionEntry(1280, 0),
                ResolutionEntry(1600, 900)));

            Assert.That(new VNResolutionOption(1920, 1080), Is.EqualTo(new VNResolutionOption(1920, 1080)));
            Assert.That(new VNResolutionOption(1920, 1080), Is.Not.EqualTo(new VNResolutionOption(1920, 1079)));

            var options = controller.GetWindowedResolutionOptions();
            Assert.That(options, Has.Count.EqualTo(3));
            Assert.That(options[0], Is.EqualTo(new VNResolutionOption(1280, 720)));
            Assert.That(options[1], Is.EqualTo(new VNResolutionOption(1600, 900)));
            Assert.That(options[2], Is.EqualTo(new VNResolutionOption(1920, 1080)));
        }

        [Test]
        public void EmptyResolutionSource_ProvidesDefaultWindowedOption()
        {
            var controller = CreateController(CreateService(), new FakeDisplayRuntime(1920, 1080));

            var options = controller.GetWindowedResolutionOptions();

            Assert.That(options, Has.Count.EqualTo(1));
            Assert.That(options[0], Is.EqualTo(VNDisplaySettingsController.DefaultWindowedResolution));
        }

        [Test]
        public void UseFullScreenWindow_PersistsModePreservesRememberedWindowedSizeAndRequestsNative()
        {
            var service = CreateService();
            SaveDisplaySettings(service, VNSettingsDefaults.WindowedDisplayMode, 1600, 900);
            var runtime = new FakeDisplayRuntime(2560, 1440, ResolutionEntry(1600, 900));
            var controller = CreateController(service, runtime);

            Assert.That(controller.TryUseFullScreenWindow(out _), Is.True);

            var current = service.Current;
            Assert.That(current.displayMode, Is.EqualTo(VNSettingsDefaults.FullScreenWindowDisplayMode));
            Assert.That(current.windowedWidth, Is.EqualTo(1600));
            Assert.That(current.windowedHeight, Is.EqualTo(900));
            AssertRequest(runtime, 2560, 1440, FullScreenMode.FullScreenWindow);
        }

        [Test]
        public void UseWindowed_RestoresRememberedAvailableResolution()
        {
            var service = CreateService();
            SaveDisplaySettings(service, VNSettingsDefaults.FullScreenWindowDisplayMode, 1600, 900);
            var runtime = new FakeDisplayRuntime(2560, 1440, ResolutionEntry(1280, 720), ResolutionEntry(1600, 900));
            var controller = CreateController(service, runtime);

            Assert.That(controller.TryUseWindowed(out _), Is.True);

            var current = service.Current;
            Assert.That(current.displayMode, Is.EqualTo(VNSettingsDefaults.WindowedDisplayMode));
            Assert.That(current.windowedWidth, Is.EqualTo(1600));
            Assert.That(current.windowedHeight, Is.EqualTo(900));
            AssertRequest(runtime, 1600, 900, FullScreenMode.Windowed);
        }

        [Test]
        public void SetWindowedResolution_PersistsSelectionAndPreservesUnrelatedSettings()
        {
            var service = CreateService();
            var source = VNSettingsDefaults.CreateDefault();
            source.textSpeedLps = 75;
            source.autoSpeedNormalized = 0.25f;
            source.masterVolumeNormalized = 0.8f;
            source.skipUnread = true;
            source.screenShakeEnabled = false;
            source.inputBindingOverridesJson = "{\"Dialogue/Advance\":\"<Keyboard>/enter\"}";
            Assert.That(service.TrySave(source, out _), Is.True);

            var runtime = new FakeDisplayRuntime(1920, 1080, ResolutionEntry(1280, 720), ResolutionEntry(1920, 1080));
            var controller = CreateController(service, runtime);

            Assert.That(controller.TrySetWindowedResolution(new VNResolutionOption(1280, 720), out _), Is.True);

            var current = service.Current;
            Assert.That(current.displayMode, Is.EqualTo(VNSettingsDefaults.WindowedDisplayMode));
            Assert.That(current.windowedWidth, Is.EqualTo(1280));
            Assert.That(current.windowedHeight, Is.EqualTo(720));
            Assert.That(current.textSpeedLps, Is.EqualTo(75));
            Assert.That(current.autoSpeedNormalized, Is.EqualTo(0.25f));
            Assert.That(current.masterVolumeNormalized, Is.EqualTo(0.8f));
            Assert.That(current.skipUnread, Is.True);
            Assert.That(current.screenShakeEnabled, Is.False);
            Assert.That(current.inputBindingOverridesJson, Is.EqualTo(source.inputBindingOverridesJson));
            AssertRequest(runtime, 1280, 720, FullScreenMode.Windowed);
        }

        [Test]
        public void StaleWindowedResolution_UsesAvailableProjectDefault()
        {
            var service = CreateService();
            SaveDisplaySettings(service, VNSettingsDefaults.WindowedDisplayMode, 1366, 768);
            var runtime = new FakeDisplayRuntime(2560, 1440, ResolutionEntry(1280, 720), ResolutionEntry(1920, 1080), ResolutionEntry(2560, 1440));
            var controller = CreateController(service, runtime);

            Assert.That(controller.TryUseWindowed(out _), Is.True);

            AssertRequest(runtime, 1920, 1080, FullScreenMode.Windowed);
            Assert.That(service.Current.windowedWidth, Is.EqualTo(1920));
            Assert.That(service.Current.windowedHeight, Is.EqualTo(1080));
        }

        [Test]
        public void StaleWindowedResolution_UsesDeterministicNearestWhenDefaultUnavailable()
        {
            var service = CreateService();
            SaveDisplaySettings(service, VNSettingsDefaults.WindowedDisplayMode, 1366, 768);
            var runtime = new FakeDisplayRuntime(2560, 1440, ResolutionEntry(1280, 720), ResolutionEntry(1600, 900), ResolutionEntry(2560, 1440));
            var controller = CreateController(service, runtime);

            Assert.That(controller.TryUseWindowed(out _), Is.True);

            AssertRequest(runtime, 1600, 900, FullScreenMode.Windowed);
        }

        [Test]
        public void InvalidNativeFullScreenSize_FailsBeforePersistenceOrRuntimeRequest()
        {
            var service = CreateService();
            SaveDisplaySettings(service, VNSettingsDefaults.WindowedDisplayMode, 1600, 900);
            var runtime = new FakeDisplayRuntime(0, 1080, ResolutionEntry(1600, 900));
            var controller = CreateController(service, runtime);

            Assert.That(controller.TryUseFullScreenWindow(out _), Is.False);
            Assert.That(service.Current.displayMode, Is.EqualTo(VNSettingsDefaults.WindowedDisplayMode));
            Assert.That(runtime.Requests, Is.Empty);
        }

        [Test]
        public void PersistenceFailure_PreventsUserInitiatedRuntimeRequest()
        {
            var original = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(Path.Combine(temporaryRoot, VNSettingsRepository.CanonicalFileName), original);
            var service = new VNSettingsService(repository);
            service.Load();
            var runtime = new FakeDisplayRuntime(1920, 1080, ResolutionEntry(1280, 720));
            var controller = CreateController(service, runtime);

            Assert.That(controller.TrySetWindowedResolution(new VNResolutionOption(1280, 720), out _), Is.False);
            Assert.That(runtime.Requests, Is.Empty);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));
        }

        [Test]
        public void ApplyCurrentSettings_UsesEffectiveFallbackWhileWriteProtectedWithoutRewriting()
        {
            var original = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(Path.Combine(temporaryRoot, VNSettingsRepository.CanonicalFileName), original);
            var service = new VNSettingsService(repository);
            service.Load();
            var runtime = new FakeDisplayRuntime(2560, 1440, ResolutionEntry(1920, 1080));
            var controller = CreateController(service, runtime);

            Assert.That(service.IsWriteProtected, Is.True);
            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);
            AssertRequest(runtime, 2560, 1440, FullScreenMode.FullScreenWindow);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));
        }

        private VNSettingsService CreateService()
        {
            var service = new VNSettingsService(repository);
            service.Load();
            return service;
        }

        private static VNDisplaySettingsController CreateController(VNSettingsService service, FakeDisplayRuntime runtime)
        {
            return new VNDisplaySettingsController(service, runtime);
        }

        private static void SaveDisplaySettings(VNSettingsService service, string displayMode, int width, int height)
        {
            var replacement = service.Current;
            replacement.displayMode = displayMode;
            replacement.windowedWidth = width;
            replacement.windowedHeight = height;
            Assert.That(service.TrySave(replacement, out _), Is.True);
        }

        private static Resolution ResolutionEntry(int width, int height)
        {
            return new Resolution { width = width, height = height };
        }

        private static void AssertRequest(FakeDisplayRuntime runtime, int width, int height, FullScreenMode mode)
        {
            Assert.That(runtime.Requests, Has.Count.EqualTo(1));
            Assert.That(runtime.Requests[0].Width, Is.EqualTo(width));
            Assert.That(runtime.Requests[0].Height, Is.EqualTo(height));
            Assert.That(runtime.Requests[0].Mode, Is.EqualTo(mode));
        }

        private sealed class FakeDisplayRuntime : IVNDisplayRuntime
        {
            public Resolution[] SupportedResolutions { get; }
            public int NativeWidth { get; }
            public int NativeHeight { get; }
            public List<DisplayRequest> Requests { get; } = new();

            public FakeDisplayRuntime(int nativeWidth, int nativeHeight, params Resolution[] supportedResolutions)
            {
                NativeWidth = nativeWidth;
                NativeHeight = nativeHeight;
                SupportedResolutions = supportedResolutions;
            }

            public void SetResolution(int width, int height, FullScreenMode fullScreenMode)
            {
                Requests.Add(new DisplayRequest(width, height, fullScreenMode));
            }
        }

        private readonly struct DisplayRequest
        {
            public int Width { get; }
            public int Height { get; }
            public FullScreenMode Mode { get; }

            public DisplayRequest(int width, int height, FullScreenMode mode)
            {
                Width = width;
                Height = height;
                Mode = mode;
            }
        }
    }
}
