using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNInputRebindServiceTests
    {
        private const string InputAssetPath = "Assets/_Project/Settings/Input/VNInputActions.inputactions";
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private readonly List<InputDevice> ownedDevices = new();
        private string temporaryRoot;
        private VNSettingsRepository repository;
        private InputSettings.BackgroundBehavior previousBackgroundBehavior;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M7RebindTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSettingsRepository.CreateForTesting(temporaryRoot);
            previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedDevices.Count - 1; index >= 0; index--)
                if (ownedDevices[index] != null && ownedDevices[index].added) InputSystem.RemoveDevice(ownedDevices[index]);
            ownedDevices.Clear();
            InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void StableInventoryAndDefaultDisplays_UseFrozenIdsAndFixedControlsStayOutsideTargets()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out _);

            Assert.That(rebind.TryValidateInputContract(out _), Is.True);
            AssertDisplay(rebind, VNRebindTarget.Advance, "<Keyboard>/space", "Space", true);
            AssertDisplay(rebind, VNRebindTarget.ToggleAuto, "<Keyboard>/a", "A", true);
            AssertDisplay(rebind, VNRebindTarget.SkipHold, "<Keyboard>/leftCtrl | <Keyboard>/rightCtrl", "Left Control / Right Control", true);
            AssertDisplay(rebind, VNRebindTarget.ToggleHide, "<Keyboard>/h", "H", true);
            AssertDisplay(rebind, VNRebindTarget.QuickSave, "<Keyboard>/f1", "F1", true);
            AssertDisplay(rebind, VNRebindTarget.QuickLoad, "<Keyboard>/f2", "F2", true);
            Assert.That(Enum.GetValues(typeof(VNRebindTarget)), Has.Length.EqualTo(6));
        }

        [Test]
        public void EmptyStartupSettings_RemoveStaleRuntimeOverridesWithoutWriting()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset);
            Override(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");

            Assert.That(rebind.TryApplyCurrentSettings(out _), Is.True);
            AssertEffective(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/a");
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
        }

        [Test]
        public void OfficialOverrideJsonRoundTrip_RestoresEffectivePath()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset);
            Override(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");
            var replacement = service.Current;
            replacement.inputBindingOverridesJson = asset.SaveBindingOverridesAsJson();
            Assert.That(service.TrySave(replacement, out _), Is.True);
            asset.RemoveAllBindingOverrides();

            Assert.That(rebind.TryApplyCurrentSettings(out _), Is.True);
            AssertEffective(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");
        }

        [Test]
        public void SkipHoldCustomReset_RestoresBothCtrlsAndPreservesOtherOverride()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset);
            Override(asset, "12575a0b-46d0-45af-98a6-4ae535125107", "2078a088-1d2d-4bb1-abbd-7dbcd1f86a45", "<Keyboard>/k");
            Override(asset, "12575a0b-46d0-45af-98a6-4ae535125107", "e92a13d9-0445-4866-b4ff-8cf1c84d84ca", string.Empty);
            Override(asset, "b61e0edd-7c5b-4700-8ff0-e0d9e2c35999", "95a6ab90-446d-400f-9629-27666fc1a288", "<Keyboard>/j");
            SaveOverrides(service, asset);

            Assert.That(rebind.TryResetBinding(VNRebindTarget.SkipHold, out _), Is.True);
            AssertEffective(asset, "12575a0b-46d0-45af-98a6-4ae535125107", "2078a088-1d2d-4bb1-abbd-7dbcd1f86a45", "<Keyboard>/leftCtrl");
            AssertEffective(asset, "12575a0b-46d0-45af-98a6-4ae535125107", "e92a13d9-0445-4866-b4ff-8cf1c84d84ca", "<Keyboard>/rightCtrl");
            AssertEffective(asset, "b61e0edd-7c5b-4700-8ff0-e0d9e2c35999", "95a6ab90-446d-400f-9629-27666fc1a288", "<Keyboard>/j");
        }

        [Test]
        public void ResetAll_RestoresDefaultsAndPersistsCanonicalEmptyString()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset);
            Override(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");
            Override(asset, "b61e0edd-7c5b-4700-8ff0-e0d9e2c35999", "95a6ab90-446d-400f-9629-27666fc1a288", "<Keyboard>/j");
            SaveOverrides(service, asset);

            Assert.That(rebind.TryResetAllBindings(out _), Is.True);
            Assert.That(service.Current.inputBindingOverridesJson, Is.Empty);
            AssertEffective(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/a");
            AssertEffective(asset, "7c75d042-0409-418a-bf92-a84220ce2099", "7e3a486e-8b78-41c4-b91c-91bb167f735e", "<Mouse>/leftButton");
        }

        [Test]
        public void MalformedNestedJson_RollsBackRuntimeWithoutTouchingSettingsFile()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset);
            Override(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");
            var replacement = service.Current;
            replacement.inputBindingOverridesJson = "not-input-system-json";
            Assert.That(service.TrySave(replacement, out _), Is.True);
            var original = File.ReadAllText(repository.CanonicalFilePath);

            Assert.That(rebind.TryApplyCurrentSettings(out var diagnostic), Is.False);
            Assert.That(diagnostic, Is.Not.Empty);
            AssertEffective(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));
        }

        [Test]
        public void FixedBindingAndDuplicatePayloads_AreRejectedAndRolledBack()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset);
            Override(asset, "7c75d042-0409-418a-bf92-a84220ce2099", "7e3a486e-8b78-41c4-b91c-91bb167f735e", "<Keyboard>/q");
            SaveOverrides(service, asset);
            asset.RemoveAllBindingOverrides();

            Assert.That(rebind.TryApplyCurrentSettings(out _), Is.False);
            AssertEffective(asset, "7c75d042-0409-418a-bf92-a84220ce2099", "7e3a486e-8b78-41c4-b91c-91bb167f735e", "<Mouse>/leftButton");

            Override(asset, "7c75d042-0409-418a-bf92-a84220ce2099", "10ff2c09-1c83-4da5-aefa-e2673f2cd6ba", "<Keyboard>/k");
            Override(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");
            SaveOverrides(service, asset);
            asset.RemoveAllBindingOverrides();
            Assert.That(rebind.TryApplyCurrentSettings(out _), Is.False);
        }

        [Test]
        public void ProcessorAndInteractionTampering_IsRejectedAndRolledBackForFrozenBindings()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset);
            OverrideProcessors(asset, "7c75d042-0409-418a-bf92-a84220ce2099", "7e3a486e-8b78-41c4-b91c-91bb167f735e", "scale(factor=2)");
            SaveOverrides(service, asset);
            asset.RemoveAllBindingOverrides();
            Assert.That(rebind.TryApplyCurrentSettings(out _), Is.False);
            AssertEffective(asset, "7c75d042-0409-418a-bf92-a84220ce2099", "7e3a486e-8b78-41c4-b91c-91bb167f735e", "<Mouse>/leftButton");

            OverrideInteractions(asset, "b889a0b7-f81d-4fd3-a1b8-f29f540ade64", "6ef85e6d-940b-4612-b1ce-4986893c4e63", "press");
            SaveOverrides(service, asset);
            asset.RemoveAllBindingOverrides();
            Assert.That(rebind.TryApplyCurrentSettings(out _), Is.False);
            AssertEffective(asset, "b889a0b7-f81d-4fd3-a1b8-f29f540ade64", "6ef85e6d-940b-4612-b1ce-4986893c4e63", "<Keyboard>/escape");

            OverrideProcessors(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "scale(factor=2)");
            Assert.That(rebind.TryGetBindingDisplay(VNRebindTarget.ToggleAuto, out var display, out _), Is.True);
            Assert.That(display.IsDefault, Is.False);
            SaveOverrides(service, asset);
            asset.RemoveAllBindingOverrides();
            Assert.That(rebind.TryApplyCurrentSettings(out _), Is.False);
            AssertEffective(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/a");
        }

        [Test]
        public void PersistenceFailure_ResetRollsRuntimeOverridesBack()
        {
            const string futureJson = "{\"schemaVersion\":999,\"futureField\":\"preserve-me\"}";
            WriteFutureSettings(futureJson);
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset);
            Override(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");

            Assert.That(rebind.TryResetBinding(VNRebindTarget.ToggleAuto, out _), Is.False);
            AssertEffective(asset, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50", "<Keyboard>/k");
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(futureJson));
        }

        [Test]
        public void InteractiveKeyboardCapture_PersistsOverrideAndRestoresRouterAndActionState()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset, out var router);
            var action = asset.FindAction(new Guid("b61e0edd-7c5b-4700-8ff0-e0d9e2c35999"));
            action.Enable();
            VNRebindResult? result = null;

            Assert.That(rebind.BeginRebind(VNRebindTarget.ToggleHide, (state, _) => result = state, out _), Is.True);
            Assert.That(router.IsRebindCaptureSuspended, Is.True);
            Assert.That(action.enabled, Is.False);
            Press(AddTestKeyboard(), Key.K);

            Assert.That(result, Is.EqualTo(VNRebindResult.Succeeded));
            Assert.That(router.IsRebindCaptureSuspended, Is.False);
            Assert.That(action.enabled, Is.True);
            Assert.That(rebind.IsRebinding, Is.False);
            AssertEffective(asset, "b61e0edd-7c5b-4700-8ff0-e0d9e2c35999", "95a6ab90-446d-400f-9629-27666fc1a288", "<Keyboard>/k");
            Assert.That(service.Current.inputBindingOverridesJson, Is.Not.Empty);
        }

        [Test]
        public void EscapeAndDuplicateCandidate_LeaveBindingsUnchangedAndDoNotRouteM6Shortcut()
        {
            var service = CreateService();
            var rebind = CreateServiceUnderTest(service, out var asset, out var router);
            var controller = ConfigureRouterForInputCallbacks(router, asset);
            var autoTransitions = 0;
            controller.AutoStateChanged += _ => autoTransitions++;
            var originalJson = service.Current.inputBindingOverridesJson;
            VNRebindResult? duplicateResult = null;

            Assert.That(rebind.BeginRebind(VNRebindTarget.ToggleHide, (state, _) => duplicateResult = state, out _), Is.True);
            Press(AddTestKeyboard(), Key.A);
            Assert.That(duplicateResult, Is.EqualTo(VNRebindResult.Duplicate));
            Assert.That(controller.IsAutoEnabled, Is.False);
            Assert.That(autoTransitions, Is.Zero);
            Assert.That(router.IsRebindCaptureSuspended, Is.False);
            AssertEffective(asset, "b61e0edd-7c5b-4700-8ff0-e0d9e2c35999", "95a6ab90-446d-400f-9629-27666fc1a288", "<Keyboard>/h");
            Assert.That(service.Current.inputBindingOverridesJson, Is.EqualTo(originalJson));

            VNRebindResult? cancelResult = null;
            Assert.That(rebind.BeginRebind(VNRebindTarget.ToggleHide, (state, _) => cancelResult = state, out _), Is.True);
            Press(AddTestKeyboard(), Key.Escape);
            Assert.That(cancelResult, Is.EqualTo(VNRebindResult.Canceled));
            Assert.That(router.IsRebindCaptureSuspended, Is.False);
            Assert.That(service.Current.inputBindingOverridesJson, Is.EqualTo(originalJson));
        }

        [Test]
        public void BeginRebind_ClosesActiveCtrlSkipHoldBeforeSuspendingRouter()
        {
            var rebind = CreateServiceUnderTest(CreateService(), out var asset, out var router);
            var controller = ConfigureRouterForInputCallbacks(router, asset);

            InvokePrivate(router, "BeginSkipHold");
            Assert.That(controller.IsSkipEnabled, Is.True);
            Assert.That(rebind.BeginRebind(VNRebindTarget.ToggleHide, null, out _), Is.True);
            Assert.That(router.IsRebindCaptureSuspended, Is.True);
            Assert.That(controller.IsSkipEnabled, Is.False);

            InvokePrivate(router, "EndSkipHold");
            Assert.That(controller.IsSkipEnabled, Is.False);
            rebind.CancelActiveRebind();
            Assert.That(router.IsRebindCaptureSuspended, Is.False);
        }

        [Test]
        public void ServiceDispose_CleansActiveCapture()
        {
            var rebind = CreateServiceUnderTest(CreateService(), out _, out var router);
            Assert.That(rebind.BeginRebind(VNRebindTarget.ToggleHide, null, out _), Is.True);
            rebind.Dispose();
            Assert.That(rebind.IsRebinding, Is.False);
            Assert.That(router.IsRebindCaptureSuspended, Is.False);
        }

        private VNSettingsService CreateService()
        {
            var service = new VNSettingsService(repository);
            service.Load();
            return service;
        }

        private VNInputRebindService CreateServiceUnderTest(VNSettingsService service, out InputActionAsset asset)
        {
            return CreateServiceUnderTest(service, out asset, out _);
        }

        private VNInputRebindService CreateServiceUnderTest(VNSettingsService service, out InputActionAsset asset, out VNConvenienceInputRouter router)
        {
            asset = InputActionAsset.FromJson(File.ReadAllText(Path.Combine(Application.dataPath, "_Project/Settings/Input/VNInputActions.inputactions")));
            ownedObjects.Add(asset);
            var root = new GameObject("M7 Input Rebind Test");
            ownedObjects.Add(root);
            router = root.AddComponent<VNConvenienceInputRouter>();
            return new VNInputRebindService(service, asset, router);
        }

        private VNConvenienceController ConfigureRouterForInputCallbacks(VNConvenienceInputRouter router, InputActionAsset asset)
        {
            var root = router.gameObject;
            var gate = root.AddComponent<VNInteractionGate>();
            var controller = root.AddComponent<VNConvenienceController>();
            SetPrivate(controller, "interactionGate", gate);
            SetPrivate(router, "convenienceController", controller);
            SetPrivate(router, "interactionGate", gate);
            SetPrivate(router, "toggleAutoAction", Track(InputActionReference.Create(asset.FindAction(new Guid("40c4fd51-8e22-48d7-91dd-90a40c664f55")))));
            SetPrivate(router, "skipHoldAction", Track(InputActionReference.Create(asset.FindAction(new Guid("12575a0b-46d0-45af-98a6-4ae535125107")))));
            InvokePrivate(router, "OnDisable");
            InvokePrivate(router, "OnEnable");
            return controller;
        }

        private Keyboard AddTestKeyboard()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            ownedDevices.Add(keyboard);
            return keyboard;
        }

        private static void Press(Keyboard keyboard, Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
        }

        private T Track<T>(T ownedObject) where T : UnityEngine.Object
        {
            ownedObjects.Add(ownedObject);
            return ownedObject;
        }

        private static void SetPrivate(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing private field " + fieldName);
            field.SetValue(instance, value);
        }

        private static void InvokePrivate(object instance, string methodName)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Missing private method " + methodName);
            method.Invoke(instance, null);
        }

        private void WriteFutureSettings(string contents)
        {
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(repository.CanonicalFilePath, contents);
        }

        private static void SaveOverrides(VNSettingsService service, InputActionAsset asset)
        {
            var replacement = service.Current;
            replacement.inputBindingOverridesJson = asset.SaveBindingOverridesAsJson();
            Assert.That(service.TrySave(replacement, out _), Is.True);
        }

        private static void Override(InputActionAsset asset, string actionId, string bindingId, string path)
        {
            var action = asset.FindAction(new Guid(actionId));
            Assert.That(action, Is.Not.Null);
            var index = BindingIndex(action, bindingId);
            action.ApplyBindingOverride(index, path);
        }

        private static void OverrideProcessors(InputActionAsset asset, string actionId, string bindingId, string processors)
        {
            var action = asset.FindAction(new Guid(actionId));
            action.ApplyBindingOverride(BindingIndex(action, bindingId), new InputBinding { overrideProcessors = processors });
        }

        private static void OverrideInteractions(InputActionAsset asset, string actionId, string bindingId, string interactions)
        {
            var action = asset.FindAction(new Guid(actionId));
            action.ApplyBindingOverride(BindingIndex(action, bindingId), new InputBinding { overrideInteractions = interactions });
        }

        private static void AssertEffective(InputActionAsset asset, string actionId, string bindingId, string expectedPath)
        {
            var action = asset.FindAction(new Guid(actionId));
            Assert.That(action.bindings[BindingIndex(action, bindingId)].effectivePath, Is.EqualTo(expectedPath));
        }

        private static void AssertDisplay(VNInputRebindService service, VNRebindTarget target, string expectedPath, string expectedDisplay, bool expectedDefault)
        {
            Assert.That(service.TryGetBindingDisplay(target, out var display, out _), Is.True);
            Assert.That(display.EffectivePath, Is.EqualTo(expectedPath));
            Assert.That(display.DisplayString, Is.EqualTo(expectedDisplay));
            Assert.That(display.IsDefault, Is.EqualTo(expectedDefault));
        }

        private static int BindingIndex(InputAction action, string bindingId)
        {
            var id = new Guid(bindingId);
            for (var index = 0; index < action.bindings.Count; index++)
                if (action.bindings[index].id == id) return index;
            Assert.Fail("Missing binding " + bindingId);
            return -1;
        }
    }
}
