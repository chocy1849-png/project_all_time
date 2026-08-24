using System;
using System.IO;
using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using ProjectAllTime.VN.Settings;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNSettingsPersistenceTests
    {
        private string temporaryRoot;
        private VNSettingsRepository repository;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M7SettingsTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSettingsRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void Defaults_AreExactAndIndependent()
        {
            var first = VNSettingsDefaults.CreateDefault();
            var second = VNSettingsDefaults.CreateDefault();

            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first.schemaVersion, Is.EqualTo(1));
            Assert.That(first.displayMode, Is.EqualTo("full_screen_window"));
            Assert.That(first.windowedWidth, Is.EqualTo(1920));
            Assert.That(first.windowedHeight, Is.EqualTo(1080));
            Assert.That(first.textSpeedLps, Is.EqualTo(60));
            Assert.That(first.autoSpeedNormalized, Is.EqualTo(0.5f));
            Assert.That(first.masterVolumeNormalized, Is.EqualTo(1f));
            Assert.That(first.bgmVolumeNormalized, Is.EqualTo(1f));
            Assert.That(first.sfxVolumeNormalized, Is.EqualTo(1f));
            Assert.That(first.voiceVolumeNormalized, Is.EqualTo(1f));
            Assert.That(first.skipUnread, Is.False);
            Assert.That(first.screenShakeEnabled, Is.True);
            Assert.That(first.inputBindingOverridesJson, Is.EqualTo(string.Empty));

            first.textSpeedLps = 1;
            Assert.That(second.textSpeedLps, Is.EqualTo(60));
        }

        [Test]
        public void Validation_RejectsEveryRequiredInvalidValue()
        {
            AssertInvalid(null);

            var data = VNSettingsDefaults.CreateDefault();
            data.schemaVersion = 0;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.schemaVersion = 2;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.displayMode = "exclusive_fullscreen";
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.windowedWidth = 0;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.windowedHeight = -1;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.textSpeedLps = 0;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.autoSpeedNormalized = float.NaN;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.masterVolumeNormalized = float.PositiveInfinity;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.bgmVolumeNormalized = -0.01f;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.sfxVolumeNormalized = 1.01f;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.voiceVolumeNormalized = float.NegativeInfinity;
            AssertInvalid(data);
            data = VNSettingsDefaults.CreateDefault();
            data.inputBindingOverridesJson = null;
            AssertInvalid(data);
        }

        [Test]
        public void MissingRead_ReturnsMissingAndDoesNotCreateFiles()
        {
            var result = repository.Read();

            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Missing));
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            Assert.That(Directory.Exists(temporaryRoot), Is.False);
        }

        [Test]
        public void CompleteSettings_RoundTripEveryField()
        {
            var source = CreateValidSettings();

            Assert.That(repository.Write(source).Succeeded, Is.True);
            var result = repository.Read();

            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Valid));
            AssertSettingsEqual(source, result.Settings);
        }

        [Test]
        public void Overwrite_ReplacesCanonicalFileWithoutTemporaryResidue()
        {
            var first = CreateValidSettings();
            Assert.That(repository.Write(first).Succeeded, Is.True);

            var replacement = CreateValidSettings();
            replacement.displayMode = VNSettingsDefaults.FullScreenWindowDisplayMode;
            replacement.textSpeedLps = 77;
            replacement.skipUnread = false;
            Assert.That(repository.Write(replacement).Succeeded, Is.True);

            var result = repository.Read();
            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Valid));
            Assert.That(result.Settings.textSpeedLps, Is.EqualTo(77));
            Assert.That(result.Settings.skipUnread, Is.False);
            Assert.That(Directory.GetFiles(temporaryRoot, "settings.json.*.tmp", SearchOption.TopDirectoryOnly), Is.Empty);
        }

        [Test]
        public void MalformedJson_IsQuarantinedWithExactOriginalBytes()
        {
            var originalBytes = new byte[] { 0x7B, 0x20, 0x6E, 0x6F, 0x74, 0x2D, 0x6A, 0x73, 0x6F, 0x6E };
            WriteCanonicalBytes(originalBytes);

            var result = repository.Read();

            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Corrupted));
            Assert.That(result.IsWriteProtected, Is.False);
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            var quarantined = Directory.GetFiles(temporaryRoot, "settings.json.*.corrupt", SearchOption.TopDirectoryOnly);
            Assert.That(quarantined, Has.Length.EqualTo(1));
            CollectionAssert.AreEqual(originalBytes, File.ReadAllBytes(quarantined[0]));
        }

        [Test]
        public void InvalidSchemaOneJson_IsQuarantined()
        {
            var original = "{\"schemaVersion\":1,\"displayMode\":\"windowed\",\"windowedWidth\":0,\"windowedHeight\":1080,\"textSpeedLps\":60,\"autoSpeedNormalized\":0.5,\"masterVolumeNormalized\":1,\"bgmVolumeNormalized\":1,\"sfxVolumeNormalized\":1,\"voiceVolumeNormalized\":1,\"skipUnread\":false,\"screenShakeEnabled\":true,\"inputBindingOverridesJson\":\"\"}";
            WriteCanonicalText(original);

            var result = repository.Read();

            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Corrupted));
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            var quarantined = Directory.GetFiles(temporaryRoot, "settings.json.*.corrupt", SearchOption.TopDirectoryOnly);
            Assert.That(quarantined, Has.Length.EqualTo(1));
            Assert.That(File.ReadAllText(quarantined[0]), Is.EqualTo(original));
        }

        [Test]
        public void PartialSchemaV1_MissingNormalizedField_IsQuarantined()
        {
            var original = "{\"schemaVersion\":1,\"displayMode\":\"windowed\",\"windowedWidth\":1920,\"windowedHeight\":1080,\"textSpeedLps\":60,\"autoSpeedNormalized\":0.5,\"bgmVolumeNormalized\":1,\"sfxVolumeNormalized\":1,\"voiceVolumeNormalized\":1,\"skipUnread\":false,\"screenShakeEnabled\":true,\"inputBindingOverridesJson\":\"\"}";
            WriteCanonicalText(original);

            var result = repository.Read();

            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Corrupted));
            Assert.That(result.Settings, Is.Null);
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            AssertQuarantinedText(original);

            var service = new VNSettingsService(repository);
            var defaults = service.Load();
            Assert.That(defaults.masterVolumeNormalized, Is.EqualTo(1f));
            Assert.That(service.IsWriteProtected, Is.False);
        }

        [Test]
        public void PartialSchemaV1_MissingBooleanField_IsQuarantined()
        {
            var original = "{\"schemaVersion\":1,\"displayMode\":\"windowed\",\"windowedWidth\":1920,\"windowedHeight\":1080,\"textSpeedLps\":60,\"autoSpeedNormalized\":0.5,\"masterVolumeNormalized\":1,\"bgmVolumeNormalized\":1,\"sfxVolumeNormalized\":1,\"voiceVolumeNormalized\":1,\"skipUnread\":false,\"inputBindingOverridesJson\":\"\"}";
            WriteCanonicalText(original);

            var result = repository.Read();

            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Corrupted));
            Assert.That(result.Settings, Is.Null);
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            AssertQuarantinedText(original);

            var service = new VNSettingsService(repository);
            var defaults = service.Load();
            Assert.That(defaults.screenShakeEnabled, Is.True);
            Assert.That(service.IsWriteProtected, Is.False);
        }

        [Test]
        public void CompleteSchemaV1_ExplicitZeroAndFalseValuesRemainValid()
        {
            var complete = "{\"schemaVersion\":1,\"displayMode\":\"windowed\",\"windowedWidth\":1920,\"windowedHeight\":1080,\"textSpeedLps\":60,\"autoSpeedNormalized\":0,\"masterVolumeNormalized\":0,\"bgmVolumeNormalized\":0,\"sfxVolumeNormalized\":0,\"voiceVolumeNormalized\":0,\"skipUnread\":false,\"screenShakeEnabled\":false,\"inputBindingOverridesJson\":\"\"}";
            WriteCanonicalText(complete);

            var result = repository.Read();

            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Valid));
            Assert.That(result.Settings.autoSpeedNormalized, Is.EqualTo(0f));
            Assert.That(result.Settings.masterVolumeNormalized, Is.EqualTo(0f));
            Assert.That(result.Settings.skipUnread, Is.False);
            Assert.That(result.Settings.screenShakeEnabled, Is.False);
            Assert.That(Directory.GetFiles(temporaryRoot, "settings.json.*.corrupt", SearchOption.TopDirectoryOnly), Is.Empty);
        }

        [Test]
        public void CompleteSchemaV1_ExtraFieldRemainsAccepted()
        {
            var complete = "{\"schemaVersion\":1,\"displayMode\":\"windowed\",\"windowedWidth\":1920,\"windowedHeight\":1080,\"textSpeedLps\":60,\"autoSpeedNormalized\":0.5,\"masterVolumeNormalized\":1,\"bgmVolumeNormalized\":1,\"sfxVolumeNormalized\":1,\"voiceVolumeNormalized\":1,\"skipUnread\":false,\"screenShakeEnabled\":true,\"inputBindingOverridesJson\":\"\",\"nonBreakingExtraField\":\"ignored\"}";
            WriteCanonicalText(complete);

            var result = repository.Read();

            Assert.That(result.State, Is.EqualTo(VNSettingsStorageState.Valid));
            Assert.That(result.Settings.displayMode, Is.EqualTo(VNSettingsDefaults.WindowedDisplayMode));
            Assert.That(result.Settings.textSpeedLps, Is.EqualTo(60));
            Assert.That(Directory.GetFiles(temporaryRoot, "settings.json.*.corrupt", SearchOption.TopDirectoryOnly), Is.Empty);
        }

        [Test]
        public void FutureSchema_IsPreservedAndBlocksRepositoryAndServiceWrites()
        {
            var original = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            WriteCanonicalText(original);

            var read = repository.Read();
            Assert.That(read.State, Is.EqualTo(VNSettingsStorageState.Unsupported));
            Assert.That(read.IsWriteProtected, Is.True);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));
            Assert.That(Directory.GetFiles(temporaryRoot, "settings.json.*.corrupt", SearchOption.TopDirectoryOnly), Is.Empty);

            var directWrite = repository.Write(CreateValidSettings());
            Assert.That(directWrite.Succeeded, Is.False);
            Assert.That(directWrite.State, Is.EqualTo(VNSettingsStorageState.Unsupported));
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));

            var service = new VNSettingsService(repository);
            var sessionDefaults = service.Load();
            Assert.That(sessionDefaults.displayMode, Is.EqualTo(VNSettingsDefaults.FullScreenWindowDisplayMode));
            Assert.That(service.IsWriteProtected, Is.True);
            Assert.That(service.TrySave(CreateValidSettings(), out _), Is.False);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));
        }

        [Test]
        public void FailedQuarantine_PreservesCanonicalBytesAndBlocksServiceWrites()
        {
            var original = "{ invalid json";
            WriteCanonicalText(original);

            using (new FileStream(repository.CanonicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var read = repository.Read();
                Assert.That(read.State, Is.EqualTo(VNSettingsStorageState.IoFailure));
                Assert.That(read.IsWriteProtected, Is.True);
                Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));

                var service = new VNSettingsService(repository);
                service.Load();
                Assert.That(service.IsWriteProtected, Is.True);
                Assert.That(service.TrySave(CreateValidSettings(), out _), Is.False);
            }
        }

        [Test]
        public void ProductionSettingsRoot_IsSeparateFromM5SaveData_AndTestsUseInjectedRoot()
        {
            Assert.That(VNSettingsRepository.ProductionStorageRoot, Is.EqualTo(Path.Combine(Application.persistentDataPath, "Settings")));
            Assert.That(VNSettingsRepository.ProductionStorageRoot, Is.Not.EqualTo(VNSaveRepository.ProductionStorageRoot));
            Assert.That(repository.StorageRoot, Is.EqualTo(Path.GetFullPath(temporaryRoot)));
            Assert.That(repository.StorageRoot, Is.Not.EqualTo(VNSettingsRepository.ProductionStorageRoot));
        }

        private void AssertInvalid(VNSettingsData data)
        {
            Assert.That(VNSettingsValidation.TryValidate(data, out _), Is.False);
        }

        private void WriteCanonicalText(string contents)
        {
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(repository.CanonicalFilePath, contents);
        }

        private void WriteCanonicalBytes(byte[] contents)
        {
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllBytes(repository.CanonicalFilePath, contents);
        }

        private void AssertQuarantinedText(string expected)
        {
            var quarantined = Directory.GetFiles(temporaryRoot, "settings.json.*.corrupt", SearchOption.TopDirectoryOnly);
            Assert.That(quarantined, Has.Length.EqualTo(1));
            Assert.That(File.ReadAllText(quarantined[0]), Is.EqualTo(expected));
        }

        private static VNSettingsData CreateValidSettings()
        {
            return new VNSettingsData
            {
                schemaVersion = 1,
                displayMode = VNSettingsDefaults.WindowedDisplayMode,
                windowedWidth = 1600,
                windowedHeight = 900,
                textSpeedLps = 73,
                autoSpeedNormalized = 0.25f,
                masterVolumeNormalized = 0.9f,
                bgmVolumeNormalized = 0.8f,
                sfxVolumeNormalized = 0.7f,
                voiceVolumeNormalized = 0.6f,
                skipUnread = true,
                screenShakeEnabled = false,
                inputBindingOverridesJson = "{\"Dialogue/Advance\":\"<Keyboard>/enter\"}",
            };
        }

        private static void AssertSettingsEqual(VNSettingsData expected, VNSettingsData actual)
        {
            Assert.That(actual.schemaVersion, Is.EqualTo(expected.schemaVersion));
            Assert.That(actual.displayMode, Is.EqualTo(expected.displayMode));
            Assert.That(actual.windowedWidth, Is.EqualTo(expected.windowedWidth));
            Assert.That(actual.windowedHeight, Is.EqualTo(expected.windowedHeight));
            Assert.That(actual.textSpeedLps, Is.EqualTo(expected.textSpeedLps));
            Assert.That(actual.autoSpeedNormalized, Is.EqualTo(expected.autoSpeedNormalized));
            Assert.That(actual.masterVolumeNormalized, Is.EqualTo(expected.masterVolumeNormalized));
            Assert.That(actual.bgmVolumeNormalized, Is.EqualTo(expected.bgmVolumeNormalized));
            Assert.That(actual.sfxVolumeNormalized, Is.EqualTo(expected.sfxVolumeNormalized));
            Assert.That(actual.voiceVolumeNormalized, Is.EqualTo(expected.voiceVolumeNormalized));
            Assert.That(actual.skipUnread, Is.EqualTo(expected.skipUnread));
            Assert.That(actual.screenShakeEnabled, Is.EqualTo(expected.screenShakeEnabled));
            Assert.That(actual.inputBindingOverridesJson, Is.EqualTo(expected.inputBindingOverridesJson));
        }
    }
}
