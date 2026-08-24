using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNSettingsUiRuntimeTests
    {
        private readonly List<UnityEngine.Object> owned = new();
        private string temporaryRoot;

        [SetUp] public void SetUp() => temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M7Ui_" + Guid.NewGuid().ToString("N"));
        [TearDown]
        public void TearDown()
        {
            for (var index = owned.Count - 1; index >= 0; index--) UnityEngine.Object.DestroyImmediate(owned[index]);
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void PanelRefresh_IsAnAuthorityInspectionAndDoesNotCreateSettingsFile()
        {
            var repository = VNSettingsRepository.CreateForTesting(temporaryRoot);
            var service = new VNSettingsService(repository);
            service.Load();
            var root = Track(new GameObject("M7 UI Test"));
            var convenience = root.AddComponent<VNConvenienceController>();
            var runner = root.AddComponent<DialogueRunner>();
            var router = root.AddComponent<VNConvenienceInputRouter>();
            var asset = Track(InputActionAsset.FromJson(File.ReadAllText(Path.Combine(Application.dataPath, "_Project/Settings/Input/VNInputActions.inputactions"))));
            var panel = root.AddComponent<VNSettingsPanel>();
            var initialized = panel.Initialize(service, new VNDisplaySettingsController(service, new FakeDisplay()),
                new VNTextAutoSettingsController(service, runner, convenience), new VNAudioSettingsController(service, new FakeMixer()),
                new VNGameplaySettingsController(service, convenience), new VNInputRebindService(service, asset, router));

            Assert.That(initialized, Is.True);
            panel.RefreshFromAuthority();
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
        }

        [Test]
        public void SliderCommit_CoalescesPreviewChangesIntoOneExplicitMutation()
        {
            var root = Track(new GameObject("Slider Commit Test"));
            var slider = root.AddComponent<Slider>();
            var commit = root.AddComponent<VNSettingsSliderCommit>();
            commit.Initialize(slider);
            var count = 0;
            var value = 0f;
            commit.CommitRequested += candidate => { count++; value = candidate; };

            slider.value = 0.2f;
            slider.value = 0.5f;
            slider.value = 0.8f;
            Assert.That(count, Is.Zero);
            commit.Commit();
            Assert.That(count, Is.EqualTo(1));
            Assert.That(value, Is.EqualTo(0.8f));
        }

        private T Track<T>(T value) where T : UnityEngine.Object { owned.Add(value); return value; }

        private sealed class FakeDisplay : IVNDisplayRuntime
        {
            public Resolution[] SupportedResolutions => Array.Empty<Resolution>();
            public int NativeWidth => 1920;
            public int NativeHeight => 1080;
            public void SetResolution(int width, int height, FullScreenMode fullScreenMode) { }
        }
        private sealed class FakeMixer : IVNAudioMixerRuntime
        {
            public bool TryGetFloat(string parameterName, out float value) { value = 0f; return true; }
            public bool TrySetFloat(string parameterName, float value) => true;
        }
    }
}
