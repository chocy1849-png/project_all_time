using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProjectAllTime.VN.Settings;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNAudioSettingsControllerTests
    {
        private string temporaryRoot;
        private VNSettingsRepository repository;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M7AudioTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSettingsRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void NormalizedToDecibels_UsesFrozenAttenuationRange()
        {
            Assert.That(VNAudioSettingsController.NormalizedToDecibels(1f), Is.EqualTo(0f));
            Assert.That(VNAudioSettingsController.NormalizedToDecibels(0.5f), Is.EqualTo(-6.0206f).Within(0.001f));
            Assert.That(VNAudioSettingsController.NormalizedToDecibels(0.1f), Is.EqualTo(-20f).Within(0.001f));
            Assert.That(VNAudioSettingsController.NormalizedToDecibels(0.01f), Is.EqualTo(-40f).Within(0.001f));
            Assert.That(VNAudioSettingsController.NormalizedToDecibels(0.0001f), Is.EqualTo(-80f));
            Assert.That(VNAudioSettingsController.NormalizedToDecibels(0f), Is.EqualTo(-80f));
            Assert.That(VNAudioSettingsController.NormalizedToDecibels(0.0000001f), Is.EqualTo(-80f));
        }

        [Test]
        public void DefaultApplication_WritesAllFourParametersWithoutPersistence()
        {
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);

            AssertAllValues(mixer, 0f, 0f, 0f, 0f);
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
        }

        [Test]
        public void StartupApplication_MapsCategoriesIndependentlyWithoutManualCombination()
        {
            var service = CreateService();
            var replacement = service.Current;
            replacement.masterVolumeNormalized = 0.5f;
            replacement.bgmVolumeNormalized = 0.25f;
            replacement.sfxVolumeNormalized = 0.75f;
            replacement.voiceVolumeNormalized = 0.1f;
            Assert.That(service.TrySave(replacement, out _), Is.True);
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);

            Assert.That(mixer.Value(VNAudioSettingsController.MasterVolumeDbParameter), Is.EqualTo(-6.0206f).Within(0.001f));
            Assert.That(mixer.Value(VNAudioSettingsController.BgmVolumeDbParameter), Is.EqualTo(-12.0412f).Within(0.001f));
            Assert.That(mixer.Value(VNAudioSettingsController.SfxVolumeDbParameter), Is.EqualTo(-2.4988f).Within(0.001f));
            Assert.That(mixer.Value(VNAudioSettingsController.VoiceVolumeDbParameter), Is.EqualTo(-20f).Within(0.001f));
        }

        [Test]
        public void MissingParameter_RejectsValidationAndStartupWithoutMutation()
        {
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            mixer.Remove(VNAudioSettingsController.VoiceVolumeDbParameter);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TryValidateMixerContract(out var validationDiagnostic), Is.False);
            Assert.That(validationDiagnostic, Does.Contain(VNAudioSettingsController.VoiceVolumeDbParameter));
            Assert.That(controller.TryApplyCurrentSettings(out var applyDiagnostic), Is.False);
            Assert.That(applyDiagnostic, Does.Contain(VNAudioSettingsController.VoiceVolumeDbParameter));
            Assert.That(mixer.SetCalls, Is.Empty);
        }

        [Test]
        public void UserMasterChange_PersistsOnlyMasterAndWritesOnlyMasterParameter()
        {
            var service = CreateService();
            var original = service.Current;
            original.bgmVolumeNormalized = 0.25f;
            original.sfxVolumeNormalized = 0.75f;
            original.voiceVolumeNormalized = 0.1f;
            original.textSpeedLps = 80;
            Assert.That(service.TrySave(original, out _), Is.True);
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TrySetMasterVolumeNormalized(0.5f, out _), Is.True);

            Assert.That(service.Current.masterVolumeNormalized, Is.EqualTo(0.5f));
            Assert.That(service.Current.bgmVolumeNormalized, Is.EqualTo(0.25f));
            Assert.That(service.Current.sfxVolumeNormalized, Is.EqualTo(0.75f));
            Assert.That(service.Current.voiceVolumeNormalized, Is.EqualTo(0.1f));
            Assert.That(service.Current.textSpeedLps, Is.EqualTo(80));
            Assert.That(mixer.SetCalls, Is.EqualTo(new[] { VNAudioSettingsController.MasterVolumeDbParameter }));
            Assert.That(mixer.Value(VNAudioSettingsController.MasterVolumeDbParameter), Is.EqualTo(-6.0206f).Within(0.001f));
        }

        [Test]
        public void UserCategoryChanges_PersistAndApplyOnlyTheirIndividualFields()
        {
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TrySetBgmVolumeNormalized(0.25f, out _), Is.True);
            Assert.That(mixer.SetCalls, Is.EqualTo(new[] { VNAudioSettingsController.BgmVolumeDbParameter }));
            mixer.SetCalls.Clear();
            Assert.That(controller.TrySetSfxVolumeNormalized(0.75f, out _), Is.True);
            Assert.That(mixer.SetCalls, Is.EqualTo(new[] { VNAudioSettingsController.SfxVolumeDbParameter }));
            mixer.SetCalls.Clear();
            Assert.That(controller.TrySetVoiceVolumeNormalized(0.1f, out _), Is.True);
            Assert.That(mixer.SetCalls, Is.EqualTo(new[] { VNAudioSettingsController.VoiceVolumeDbParameter }));

            Assert.That(service.Current.masterVolumeNormalized, Is.EqualTo(1f));
            Assert.That(service.Current.bgmVolumeNormalized, Is.EqualTo(0.25f));
            Assert.That(service.Current.sfxVolumeNormalized, Is.EqualTo(0.75f));
            Assert.That(service.Current.voiceVolumeNormalized, Is.EqualTo(0.1f));
        }

        [Test]
        public void UserInput_RejectsNonFiniteAndClampsFiniteOutOfRangeValues()
        {
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TrySetMasterVolumeNormalized(float.NaN, out _), Is.False);
            Assert.That(controller.TrySetMasterVolumeNormalized(float.PositiveInfinity, out _), Is.False);
            Assert.That(controller.TrySetMasterVolumeNormalized(float.NegativeInfinity, out _), Is.False);
            Assert.That(mixer.SetCalls, Is.Empty);
            Assert.That(service.Current.masterVolumeNormalized, Is.EqualTo(1f));

            Assert.That(controller.TrySetMasterVolumeNormalized(-1f, out _), Is.True);
            Assert.That(service.Current.masterVolumeNormalized, Is.EqualTo(0f));
            Assert.That(mixer.Value(VNAudioSettingsController.MasterVolumeDbParameter), Is.EqualTo(-80f));
            Assert.That(controller.TrySetMasterVolumeNormalized(2f, out _), Is.True);
            Assert.That(service.Current.masterVolumeNormalized, Is.EqualTo(1f));
            Assert.That(mixer.Value(VNAudioSettingsController.MasterVolumeDbParameter), Is.EqualTo(0f));
        }

        [Test]
        public void WriteProtectedUserChange_DoesNotMutateMixerOrProtectedFile()
        {
            const string futureJson = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            WriteFutureSettings(futureJson);
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TrySetBgmVolumeNormalized(0.25f, out _), Is.False);
            Assert.That(mixer.SetCalls, Is.Empty);
            Assert.That(service.Current.bgmVolumeNormalized, Is.EqualTo(1f));
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(futureJson));
        }

        [Test]
        public void WriteProtectedStartupApplication_UsesDefaultsWithoutWriting()
        {
            const string futureJson = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            WriteFutureSettings(futureJson);
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(service.IsWriteProtected, Is.True);
            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);
            AssertAllValues(mixer, 0f, 0f, 0f, 0f);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(futureJson));
        }

        [Test]
        public void UnexpectedUserSetFailure_KeepsPersistedValueAndReportsFailure()
        {
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            mixer.FailSetFor = VNAudioSettingsController.SfxVolumeDbParameter;
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TrySetSfxVolumeNormalized(0.5f, out var diagnostic), Is.False);
            Assert.That(diagnostic, Does.Contain(VNAudioSettingsController.SfxVolumeDbParameter));
            Assert.That(service.Current.sfxVolumeNormalized, Is.EqualTo(0.5f));
        }

        [Test]
        public void UnexpectedStartupSetFailure_RestoresPreviouslyChangedParametersWherePossible()
        {
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            mixer.FailSetFor = VNAudioSettingsController.VoiceVolumeDbParameter;
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TryApplyCurrentSettings(out var diagnostic), Is.False);
            Assert.That(diagnostic, Does.Contain(VNAudioSettingsController.VoiceVolumeDbParameter));
            AssertAllValues(mixer, -12f, -12f, -12f, -12f);
        }

        [Test]
        public void MixerContract_UsesOnlyTheFourFrozenParameterNames()
        {
            var service = CreateService();
            var mixer = FakeMixer.WithRequiredParameters(-12f);
            var controller = new VNAudioSettingsController(service, mixer);

            Assert.That(controller.TryValidateMixerContract(out _), Is.True);

            Assert.That(mixer.GetCalls, Is.EqualTo(new[]
            {
                VNAudioSettingsController.MasterVolumeDbParameter,
                VNAudioSettingsController.BgmVolumeDbParameter,
                VNAudioSettingsController.SfxVolumeDbParameter,
                VNAudioSettingsController.VoiceVolumeDbParameter,
            }));
        }

        private VNSettingsService CreateService()
        {
            var service = new VNSettingsService(repository);
            service.Load();
            return service;
        }

        private void WriteFutureSettings(string contents)
        {
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(repository.CanonicalFilePath, contents);
        }

        private static void AssertAllValues(FakeMixer mixer, float master, float bgm, float sfx, float voice)
        {
            Assert.That(mixer.Value(VNAudioSettingsController.MasterVolumeDbParameter), Is.EqualTo(master).Within(0.001f));
            Assert.That(mixer.Value(VNAudioSettingsController.BgmVolumeDbParameter), Is.EqualTo(bgm).Within(0.001f));
            Assert.That(mixer.Value(VNAudioSettingsController.SfxVolumeDbParameter), Is.EqualTo(sfx).Within(0.001f));
            Assert.That(mixer.Value(VNAudioSettingsController.VoiceVolumeDbParameter), Is.EqualTo(voice).Within(0.001f));
        }

        private sealed class FakeMixer : IVNAudioMixerRuntime
        {
            private readonly Dictionary<string, float> values = new();
            public List<string> GetCalls { get; } = new();
            public List<string> SetCalls { get; } = new();
            public string FailSetFor { get; set; }

            public static FakeMixer WithRequiredParameters(float initialValue)
            {
                var mixer = new FakeMixer();
                mixer.values.Add(VNAudioSettingsController.MasterVolumeDbParameter, initialValue);
                mixer.values.Add(VNAudioSettingsController.BgmVolumeDbParameter, initialValue);
                mixer.values.Add(VNAudioSettingsController.SfxVolumeDbParameter, initialValue);
                mixer.values.Add(VNAudioSettingsController.VoiceVolumeDbParameter, initialValue);
                return mixer;
            }

            public bool TryGetFloat(string parameterName, out float value)
            {
                GetCalls.Add(parameterName);
                return values.TryGetValue(parameterName, out value);
            }

            public bool TrySetFloat(string parameterName, float value)
            {
                SetCalls.Add(parameterName);
                if (parameterName == FailSetFor || !values.ContainsKey(parameterName)) return false;
                values[parameterName] = value;
                return true;
            }

            public float Value(string parameterName) => values[parameterName];
            public void Remove(string parameterName) => values.Remove(parameterName);
        }
    }
}
