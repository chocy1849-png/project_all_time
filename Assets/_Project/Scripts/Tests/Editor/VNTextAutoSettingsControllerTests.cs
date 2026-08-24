using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Settings;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNTextAutoSettingsControllerTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNSettingsRepository repository;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M7TextAutoTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSettingsRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void DefaultApplication_UpdatesBothYarnTextSpeedFieldsAndDefaultAutoMultiplier()
        {
            var controller = CreateController(out var service, out var linePresenter, out var typewriter, out var convenience);
            linePresenter.lettersPerSecond = 7;
            typewriter.CharactersPerSecond = 7f;

            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);

            Assert.That(service.Current.textSpeedLps, Is.EqualTo(60));
            Assert.That(linePresenter.lettersPerSecond, Is.EqualTo(60));
            Assert.That(typewriter.CharactersPerSecond, Is.EqualTo(60f));
            Assert.That(convenience.AutoDelayMultiplier, Is.EqualTo(1f));
            Assert.That(convenience.IsAutoEnabled, Is.False);
        }

        [Test]
        public void StoredTextSpeedOutsideProductRange_AppliesClampedRuntimeValueWithoutRewriting()
        {
            var controller = CreateController(out var service, out var linePresenter, out var typewriter, out _);
            SaveTextSpeed(service, 1);

            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);
            Assert.That(service.Current.textSpeedLps, Is.EqualTo(1));
            Assert.That(linePresenter.lettersPerSecond, Is.EqualTo(20));
            Assert.That(typewriter.CharactersPerSecond, Is.EqualTo(20f));

            SaveTextSpeed(service, 500);
            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);
            Assert.That(service.Current.textSpeedLps, Is.EqualTo(500));
            Assert.That(linePresenter.lettersPerSecond, Is.EqualTo(120));
            Assert.That(typewriter.CharactersPerSecond, Is.EqualTo(120f));
        }

        [Test]
        public void UserTextSpeedChange_PersistsOnlyTextThenSynchronizesBothYarnFields()
        {
            var controller = CreateController(out var service, out var linePresenter, out var typewriter, out _);
            var source = service.Current;
            source.masterVolumeNormalized = 0.8f;
            source.skipUnread = true;
            source.inputBindingOverridesJson = "{\"Dialogue/Advance\":\"<Keyboard>/enter\"}";
            Assert.That(service.TrySave(source, out _), Is.True);

            Assert.That(controller.TrySetTextSpeedLps(80, out _), Is.True);

            Assert.That(service.Current.textSpeedLps, Is.EqualTo(80));
            Assert.That(service.Current.masterVolumeNormalized, Is.EqualTo(0.8f));
            Assert.That(service.Current.skipUnread, Is.True);
            Assert.That(service.Current.inputBindingOverridesJson, Is.EqualTo(source.inputBindingOverridesJson));
            Assert.That(linePresenter.lettersPerSecond, Is.EqualTo(80));
            Assert.That(typewriter.CharactersPerSecond, Is.EqualTo(80f));
        }

        [Test]
        public void PersistenceFailure_PreventsTextAndAutoRuntimeMutation()
        {
            WriteFutureSettings();
            var controller = CreateController(out var service, out var linePresenter, out var typewriter, out var convenience);
            service.Load();
            linePresenter.lettersPerSecond = 44;
            typewriter.CharactersPerSecond = 44f;

            Assert.That(controller.TrySetTextSpeedLps(80, out _), Is.False);
            Assert.That(controller.TrySetAutoSpeedNormalized(0f, out _), Is.False);
            Assert.That(linePresenter.lettersPerSecond, Is.EqualTo(44));
            Assert.That(typewriter.CharactersPerSecond, Is.EqualTo(44f));
            Assert.That(convenience.AutoDelayMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void AmbiguousOrIncompatiblePresenter_RejectsTextChangeBeforePersistence()
        {
            var controller = CreateController(out var service, out var linePresenter, out _, out _);
            var root = new GameObject("M7 Second LinePresenter");
            ownedObjects.Add(root);
            var second = root.AddComponent<LinePresenter>();
            var runner = GetRunner(controller);
            runner.DialoguePresenters = new DialoguePresenterBase[] { linePresenter, second };

            Assert.That(controller.TrySetTextSpeedLps(80, out _), Is.False);
            Assert.That(service.Current.textSpeedLps, Is.EqualTo(60));

            runner.DialoguePresenters = new DialoguePresenterBase[] { linePresenter };
            linePresenter.Typewriter = new InstantTypewriter();
            Assert.That(controller.TrySetTextSpeedLps(80, out _), Is.False);
            Assert.That(service.Current.textSpeedLps, Is.EqualTo(60));
            Assert.That(linePresenter.Typewriter, Is.TypeOf<InstantTypewriter>());
        }

        [Test]
        public void AutoSpeedMappingAndPersistence_UseFrozenEndpointsAndDefault()
        {
            Assert.That(VNTextAutoSettingsController.ToAutoDelayMultiplier(0f), Is.EqualTo(1.5f));
            Assert.That(VNTextAutoSettingsController.ToAutoDelayMultiplier(0.5f), Is.EqualTo(1f));
            Assert.That(VNTextAutoSettingsController.ToAutoDelayMultiplier(1f), Is.EqualTo(0.5f));

            var controller = CreateController(out var service, out _, out _, out var convenience);
            Assert.That(controller.TrySetAutoSpeedNormalized(0f, out _), Is.True);
            Assert.That(service.Current.autoSpeedNormalized, Is.EqualTo(0f));
            Assert.That(convenience.AutoDelayMultiplier, Is.EqualTo(1.5f));
            Assert.That(controller.TrySetAutoSpeedNormalized(2f, out _), Is.True);
            Assert.That(service.Current.autoSpeedNormalized, Is.EqualTo(1f));
            Assert.That(convenience.AutoDelayMultiplier, Is.EqualTo(0.5f));
        }

        [Test]
        public void ConvenienceAutoDelay_PreservesM6BaseThenAppliesMultiplierAndFinalBounds()
        {
            var root = new GameObject("M7 Auto Delay Test");
            ownedObjects.Add(root);
            var convenience = root.AddComponent<VNConvenienceController>();

            Assert.That(convenience.GetAutoDelaySeconds(string.Empty), Is.EqualTo(0.8f));
            Assert.That(convenience.GetAutoDelaySeconds(new string('x', 1000)), Is.EqualTo(4f));
            Assert.That(convenience.TrySetAutoDelayMultiplier(1.5f), Is.True);
            Assert.That(convenience.GetAutoDelaySeconds(new string('x', 1000)), Is.EqualTo(4f));
            Assert.That(convenience.TrySetAutoDelayMultiplier(0.5f), Is.True);
            Assert.That(convenience.GetAutoDelaySeconds(string.Empty), Is.EqualTo(0.8f));
            Assert.That(convenience.GetAutoDelaySeconds(new string('x', 50)), Is.EqualTo(1.125f));
        }

        [Test]
        public void AutoMultiplierChange_ResetsOnlyAutoScheduling()
        {
            var root = new GameObject("M7 Auto Schedule Test");
            ownedObjects.Add(root);
            var convenience = root.AddComponent<VNConvenienceController>();
            convenience.SetAutoEnabled(true);
            SetPrivateField(convenience, "autoTimerArmed", true);
            SetPrivateField(convenience, "autoDeadline", 123f);

            Assert.That(convenience.TrySetAutoDelayMultiplier(0.5f), Is.True);
            Assert.That(GetPrivateField<bool>(convenience, "autoTimerArmed"), Is.False);
            Assert.That(GetPrivateField<float>(convenience, "autoDeadline"), Is.EqualTo(0f));
            Assert.That(convenience.IsAutoEnabled, Is.True);
            Assert.That(convenience.IsSkipEnabled, Is.False);
        }

        [Test]
        public void ProtectedStartupApplication_AppliesDefaultsWithoutWritingFutureFile()
        {
            WriteFutureSettings();
            var controller = CreateController(out var service, out var linePresenter, out var typewriter, out var convenience);
            service.Load();

            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);
            Assert.That(linePresenter.lettersPerSecond, Is.EqualTo(60));
            Assert.That(typewriter.CharactersPerSecond, Is.EqualTo(60f));
            Assert.That(convenience.AutoDelayMultiplier, Is.EqualTo(1f));
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo("{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}"));
        }

        private VNTextAutoSettingsController CreateController(
            out VNSettingsService service,
            out LinePresenter linePresenter,
            out LetterTypewriter typewriter,
            out VNConvenienceController convenience)
        {
            var root = new GameObject("M7 Text Auto Runtime Test");
            ownedObjects.Add(root);
            var runner = root.AddComponent<DialogueRunner>();
            linePresenter = root.AddComponent<LinePresenter>();
            typewriter = new LetterTypewriter { CharactersPerSecond = 11f };
            linePresenter.Typewriter = typewriter;
            runner.DialoguePresenters = new DialoguePresenterBase[] { linePresenter };
            convenience = root.AddComponent<VNConvenienceController>();
            service = new VNSettingsService(repository);
            service.Load();
            return new VNTextAutoSettingsController(service, runner, convenience);
        }

        private static DialogueRunner GetRunner(VNTextAutoSettingsController controller)
        {
            var field = typeof(VNTextAutoSettingsController).GetField("dialogueRunner", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (DialogueRunner)field.GetValue(controller);
        }

        private static void SaveTextSpeed(VNSettingsService service, int textSpeed)
        {
            var replacement = service.Current;
            replacement.textSpeedLps = textSpeed;
            Assert.That(service.TrySave(replacement, out _), Is.True);
        }

        private void WriteFutureSettings()
        {
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(repository.CanonicalFilePath, "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}");
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            return (T)field.GetValue(target);
        }
    }
}
