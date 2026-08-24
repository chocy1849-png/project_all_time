using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Settings;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNGameplaySettingsControllerTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNSettingsRepository repository;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M7GameplayTests_" + Guid.NewGuid().ToString("N"));
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
        public void DefaultStartupApply_UsesReadOnlyAndDefaultScreenShakeWithoutWriting()
        {
            var service = CreateService();
            var controller = CreateController(service, out var convenience);

            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);
            Assert.That(convenience.SkipPolicy, Is.EqualTo(VNSkipPolicy.ReadOnly));
            Assert.That(controller.IsScreenShakeEnabled, Is.True);
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
        }

        [Test]
        public void StoredSkipUnreadEnabled_StartupMapsToAllWithoutAdditionalWrite()
        {
            var service = CreateService();
            var replacement = service.Current;
            replacement.skipUnread = true;
            Assert.That(service.TrySave(replacement, out _), Is.True);
            var before = File.ReadAllText(repository.CanonicalFilePath);
            var controller = CreateController(service, out var convenience);

            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);
            Assert.That(convenience.SkipPolicy, Is.EqualTo(VNSkipPolicy.All));
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(before));
        }

        [Test]
        public void UserSkipUnreadChanges_PersistOnlyThatFieldThenApplyPolicy()
        {
            var service = CreateService();
            var source = service.Current;
            source.textSpeedLps = 80;
            source.autoSpeedNormalized = 0.25f;
            source.masterVolumeNormalized = 0.5f;
            source.screenShakeEnabled = false;
            source.inputBindingOverridesJson = "{\"Dialogue/Advance\":\"<Keyboard>/enter\"}";
            Assert.That(service.TrySave(source, out _), Is.True);
            var controller = CreateController(service, out var convenience);

            Assert.That(controller.TrySetSkipUnread(true, out _), Is.True);
            Assert.That(service.Current.skipUnread, Is.True);
            Assert.That(convenience.SkipPolicy, Is.EqualTo(VNSkipPolicy.All));
            Assert.That(service.Current.textSpeedLps, Is.EqualTo(80));
            Assert.That(service.Current.autoSpeedNormalized, Is.EqualTo(0.25f));
            Assert.That(service.Current.masterVolumeNormalized, Is.EqualTo(0.5f));
            Assert.That(service.Current.screenShakeEnabled, Is.False);
            Assert.That(service.Current.inputBindingOverridesJson, Is.EqualTo(source.inputBindingOverridesJson));

            Assert.That(controller.TrySetSkipUnread(false, out _), Is.True);
            Assert.That(service.Current.skipUnread, Is.False);
            Assert.That(convenience.SkipPolicy, Is.EqualTo(VNSkipPolicy.ReadOnly));
        }

        [Test]
        public void WriteProtectedSkipChange_LeavesSettingAndPolicyUnchanged()
        {
            const string futureJson = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            WriteFutureSettings(futureJson);
            var service = CreateService();
            var controller = CreateController(service, out var convenience);

            Assert.That(controller.TrySetSkipUnread(true, out _), Is.False);
            Assert.That(service.Current.skipUnread, Is.False);
            Assert.That(convenience.SkipPolicy, Is.EqualTo(VNSkipPolicy.ReadOnly));
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(futureJson));
        }

        [Test]
        public void PolicyChanges_DoNotToggleExistingAutoOrSkipModes()
        {
            var controller = CreateController(CreateService(), out var convenience);
            convenience.SetAutoEnabled(true);

            Assert.That(controller.TrySetSkipUnread(true, out _), Is.True);
            Assert.That(convenience.IsAutoEnabled, Is.True);
            Assert.That(convenience.IsSkipEnabled, Is.False);

            convenience.SetAutoEnabled(false);
            convenience.SetSkipEnabled(true);
            Assert.That(controller.TrySetSkipUnread(false, out _), Is.True);
            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(convenience.IsSkipEnabled, Is.True);
        }

        [Test]
        public void M7PolicyMapping_DelegatesToExistingM6SkipAndDoesNotFabricateReadHistory()
        {
            var service = CreateService();
            var runtime = CreateM6Runtime();
            var controller = new VNGameplaySettingsController(service, runtime.Convenience);

            runtime.Present("unread", "Unread.");
            runtime.Convenience.SetSkipEnabled(true);
            runtime.Tick(0f, 1);
            runtime.Tick(0f, 2);
            Assert.That(runtime.Convenience.IsSkipEnabled, Is.False);
            Assert.That(runtime.ForwardedCount, Is.Zero);
            Assert.That(runtime.SessionState.ReadHistory.IsRead("unread"), Is.False);

            Assert.That(controller.TrySetSkipUnread(true, out _), Is.True);
            runtime.Convenience.SetSkipEnabled(true);
            runtime.Tick(1f, 3);
            runtime.Tick(1f, 4);
            Assert.That(runtime.Convenience.SkipPolicy, Is.EqualTo(VNSkipPolicy.All));
            Assert.That(runtime.ForwardedCount, Is.EqualTo(1));
            Assert.That(runtime.SessionState.ReadHistory.IsRead("unread"), Is.False);
        }

        [Test]
        public void ScreenShakeGate_TracksPersistedPreferenceWithoutConsumer()
        {
            var service = CreateService();
            var controller = CreateController(service, out _);

            Assert.That(controller.IsScreenShakeEnabled, Is.True);
            Assert.That(controller.TrySetScreenShakeEnabled(false, out _), Is.True);
            Assert.That(service.Current.screenShakeEnabled, Is.False);
            Assert.That(controller.IsScreenShakeEnabled, Is.False);
            Assert.That(controller.TrySetScreenShakeEnabled(true, out _), Is.True);
            Assert.That(controller.IsScreenShakeEnabled, Is.True);
        }

        [Test]
        public void WriteProtectedScreenShakeChange_LeavesGateAndFileUnchanged()
        {
            const string futureJson = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            WriteFutureSettings(futureJson);
            var controller = CreateController(CreateService(), out _);

            Assert.That(controller.TrySetScreenShakeEnabled(false, out _), Is.False);
            Assert.That(controller.IsScreenShakeEnabled, Is.True);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(futureJson));
        }

        [Test]
        public void WriteProtectedStartupApply_UsesEffectiveDefaultsWithoutWriting()
        {
            const string futureJson = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            WriteFutureSettings(futureJson);
            var controller = CreateController(CreateService(), out var convenience);

            Assert.That(controller.TryApplyCurrentSettings(out _), Is.True);
            Assert.That(convenience.SkipPolicy, Is.EqualTo(VNSkipPolicy.ReadOnly));
            Assert.That(controller.IsScreenShakeEnabled, Is.True);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(futureJson));
        }

        [Test]
        public void FrozenMapping_UsesOnlyExistingM6Policies()
        {
            Assert.That(VNGameplaySettingsController.ToSkipPolicy(false), Is.EqualTo(VNSkipPolicy.ReadOnly));
            Assert.That(VNGameplaySettingsController.ToSkipPolicy(true), Is.EqualTo(VNSkipPolicy.All));
        }

        private VNSettingsService CreateService()
        {
            var service = new VNSettingsService(repository);
            service.Load();
            return service;
        }

        private VNGameplaySettingsController CreateController(VNSettingsService service, out VNConvenienceController convenience)
        {
            var root = new GameObject("M7 Gameplay Settings Test");
            ownedObjects.Add(root);
            convenience = root.AddComponent<VNConvenienceController>();
            return new VNGameplaySettingsController(service, convenience);
        }

        private M6Runtime CreateM6Runtime()
        {
            var root = new GameObject("M7 Gameplay M6 Runtime Test");
            ownedObjects.Add(root);
            var lineAdvancer = root.AddComponent<LineAdvancer>();
            lineAdvancer.enabled = false;
            var sessionState = root.AddComponent<VNDialogueSessionState>();
            var lifecycle = root.AddComponent<VNLineLifecyclePresenter>();
            var markupHandler = root.AddComponent<VNLineLifecycleMarkupHandler>();
            var linePresenter = root.AddComponent<LinePresenter>();
            var dialogueRunner = root.AddComponent<DialogueRunner>();
            var gate = root.AddComponent<VNInteractionGate>();
            var bridge = root.AddComponent<VNLineAdvancerInputBridge>();
            var convenience = root.AddComponent<VNConvenienceController>();
            var textRoot = new GameObject("M7 Gameplay M6 Text");
            ownedObjects.Add(textRoot);
            textRoot.AddComponent<Canvas>();
            var lineText = textRoot.AddComponent<TextMeshProUGUI>();
            lineText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            linePresenter.lineText = lineText;
            linePresenter.characterNameText = lineText;
            dialogueRunner.DialoguePresenters = new DialoguePresenterBase[] { linePresenter };

            SetPrivateField(lifecycle, "sessionState", sessionState);
            SetPrivateField(lifecycle, "linePresenter", linePresenter);
            SetPrivateField(markupHandler, "lifecyclePresenter", lifecycle);
            SetPrivateField(gate, "sessionState", sessionState);
            SetPrivateField(bridge, "sessionState", sessionState);
            SetPrivateField(bridge, "interactionGate", gate);
            SetPrivateField(convenience, "sessionState", sessionState);
            SetPrivateField(convenience, "advanceBridge", bridge);
            SetPrivateField(convenience, "interactionGate", gate);

            return new M6Runtime(sessionState, lifecycle, markupHandler, dialogueRunner, convenience, bridge);
        }

        private void WriteFutureSettings(string contents)
        {
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(repository.CanonicalFilePath, contents);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private sealed class M6Runtime
        {
            private readonly VNLineLifecyclePresenter lifecycle;
            private readonly VNLineLifecycleMarkupHandler markupHandler;
            private readonly DialogueRunner dialogueRunner;
            private readonly VNLineAdvancerInputBridge bridge;

            public VNDialogueSessionState SessionState { get; }
            public VNConvenienceController Convenience { get; }
            public int ForwardedCount { get; private set; }

            public M6Runtime(VNDialogueSessionState sessionState, VNLineLifecyclePresenter lifecycle, VNLineLifecycleMarkupHandler markupHandler, DialogueRunner dialogueRunner, VNConvenienceController convenience, VNLineAdvancerInputBridge bridge)
            {
                SessionState = sessionState;
                this.lifecycle = lifecycle;
                this.markupHandler = markupHandler;
                this.dialogueRunner = dialogueRunner;
                Convenience = convenience;
                this.bridge = bridge;
                bridge.AdvanceForwarded += _ => ForwardedCount++;
            }

            public void Present(string lineId, string text)
            {
                var line = new LocalizedLine
                {
                    TextID = lineId,
                    Text = new MarkupParseResult(text, new List<MarkupAttribute>()),
                    Source = dialogueRunner,
                };
                lifecycle.RunLineAsync(line, default);
                SetPrivateField(SessionState, "currentPresentationStartedFrame", -1);
            }

            public void Tick(float time, int frame)
            {
                var method = typeof(VNConvenienceController).GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null);
                method.Invoke(Convenience, new object[] { time, frame });
            }
        }
    }
}
