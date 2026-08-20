using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;
using UnityEngine.TestTools;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNConvenienceBridgeTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private VNDialogueSessionState sessionState;
        private VNLineLifecyclePresenter lifecycle;
        private VNInteractionGate gate;
        private VNLineAdvancerInputBridge bridge;
        private VNConvenienceController convenience;
        private VNUIVisibilityController visibility;
        private CanvasGroup dialogueLayer;
        private CanvasGroup quickControlLayer;
        private int forwardedCount;

        [SetUp]
        public void SetUp()
        {
            forwardedCount = 0;
            var root = new GameObject("M6-04 Convenience Bridge Test");
            ownedObjects.Add(root);
            var lineAdvancer = root.AddComponent<LineAdvancer>();
            lineAdvancer.enabled = false;
            sessionState = root.AddComponent<VNDialogueSessionState>();
            lifecycle = root.AddComponent<VNLineLifecyclePresenter>();
            gate = root.AddComponent<VNInteractionGate>();
            bridge = root.AddComponent<VNLineAdvancerInputBridge>();
            visibility = root.AddComponent<VNUIVisibilityController>();
            convenience = root.AddComponent<VNConvenienceController>();

            dialogueLayer = CreateCanvasGroup("Dialogue Layer", 0.65f, false, true);
            quickControlLayer = CreateCanvasGroup("QuickControl Layer", 0.9f, true, false);
            SetPrivateField(lifecycle, "sessionState", sessionState);
            SetPrivateField(gate, "sessionState", sessionState);
            SetPrivateField(bridge, "sessionState", sessionState);
            SetPrivateField(bridge, "interactionGate", gate);
            SetPrivateField(visibility, "dialogueLayer", dialogueLayer);
            SetPrivateField(visibility, "quickControlLayer", quickControlLayer);
            SetPrivateField(visibility, "interactionGate", gate);
            SetPrivateField(convenience, "sessionState", sessionState);
            SetPrivateField(convenience, "advanceBridge", bridge);
            SetPrivateField(convenience, "interactionGate", gate);
            SetPrivateField(convenience, "uiVisibilityController", visibility);
            bridge.AdvanceForwarded += _ => forwardedCount++;
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
        }

        [Test]
        public void ManualNext_UsesSharedBridge_WhenVisible_AndRejectsBlockedOrOptions()
        {
            Present("manual", "Manual bridge line.");
            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(sessionState.ReadHistory.IsRead("manual"), Is.False);

            gate.SetUiHidden(true);
            Assert.That(convenience.HandleManualAdvance(), Is.False);
            Assert.That(forwardedCount, Is.EqualTo(1));
            gate.SetUiHidden(false);

            lifecycle.RunOptionsAsync(Array.Empty<DialogueOption>(), NewLineToken());
            Assert.That(convenience.HandleManualAdvance(), Is.False);
            Assert.That(forwardedCount, Is.EqualTo(1));
        }

        [Test]
        public void Hide_Show_CapturesCanvasGroups_AndDoesNotDeactivateLayers()
        {
            Present("hide", "Hide while typewriting is allowed.");
            Assert.That(visibility.TryHideUi(), Is.True);
            AssertHidden(dialogueLayer);
            AssertHidden(quickControlLayer);
            Assert.That(dialogueLayer.gameObject.activeSelf, Is.True);
            Assert.That(quickControlLayer.gameObject.activeSelf, Is.True);
            Assert.That(gate.IsUiHidden, Is.True);
            Assert.That(visibility.TryHideUi(), Is.True, "Hide is idempotent.");

            Assert.That(visibility.ShowUi(), Is.True);
            Assert.That(dialogueLayer.alpha, Is.EqualTo(0.65f));
            Assert.That(dialogueLayer.interactable, Is.False);
            Assert.That(dialogueLayer.blocksRaycasts, Is.True);
            Assert.That(quickControlLayer.alpha, Is.EqualTo(0.9f));
            Assert.That(quickControlLayer.interactable, Is.True);
            Assert.That(quickControlLayer.blocksRaycasts, Is.False);
            Assert.That(gate.IsUiHidden, Is.False);
            Assert.That(visibility.ShowUi(), Is.True, "Show is idempotent.");
        }

        [Test]
        public void Hide_IsRejectedForMissingGroup_Options_AndInactivePresentation()
        {
            Assert.That(visibility.TryHideUi(), Is.False, "A command-only/no-line interval cannot hide UI.");
            Present("options", "Choose.");
            lifecycle.RunOptionsAsync(Array.Empty<DialogueOption>(), NewLineToken());
            Assert.That(visibility.TryHideUi(), Is.False);

            lifecycle.OnDialogueStartedAsync();
            Present("missing", "Missing reference is safe.");
            SetPrivateField(visibility, "quickControlLayer", null);
            LogAssert.Expect(LogType.Error, new Regex("VNUIVisibilityController requires"));
            Assert.That(visibility.TryHideUi(), Is.False);
            Assert.That(dialogueLayer.alpha, Is.EqualTo(0.65f), "No partial hide is applied.");
            Assert.That(gate.IsUiHidden, Is.False);
        }

        [Test]
        public void HiddenManualAdvance_RestoresOnly_ThenALaterAdvanceMayConsumeRead()
        {
            Present("hidden-read", "A completed line.");
            CompleteDisplay();
            Assert.That(convenience.TryHideUi(), Is.True);

            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(visibility.IsUiHidden, Is.False);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(sessionState.ReadHistory.IsRead("hidden-read"), Is.False);

            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(sessionState.ReadHistory.IsRead("hidden-read"), Is.True);
        }

        [Test]
        public void Hide_SuspendsAutoAndSkipWithoutDisablingTheirLogicalState()
        {
            Present("auto", "Automation remains selected.");
            convenience.SetAutoEnabled(true);
            Assert.That(convenience.TryHideUi(), Is.True);
            Assert.That(convenience.IsAutoEnabled, Is.True);
            Assert.That(convenience.IsSkipEnabled, Is.False);
            Assert.That(gate.CanRunAutomation, Is.False);
            Assert.That(convenience.ShowUi(), Is.True);
            Assert.That(convenience.IsAutoEnabled, Is.True);

            convenience.SetSkipEnabled(true);
            Assert.That(convenience.TryHideUi(), Is.True);
            Assert.That(convenience.IsSkipEnabled, Is.True);
            Assert.That(gate.CanRunAutomation, Is.False);
        }

        [Test]
        public void SaveLoadActions_AreGatedButRemainAvailableDuringOptions_AndPointerSeamDelegates()
        {
            var saveLoad = CreateSaveLoadController();
            var statuses = new List<string>();
            saveLoad.StatusChanged += statuses.Add;
            SetPrivateField(gate, "saveLoadController", saveLoad);
            SetPrivateField(convenience, "saveLoadController", saveLoad);

            lifecycle.RunOptionsAsync(Array.Empty<DialogueOption>(), NewLineToken());
            Assert.That(convenience.OpenSave(), Is.True);
            Assert.That(convenience.OpenLoad(), Is.True);
            Assert.That(convenience.BeginSaveLoadOpenerInputSuppression(), Is.True);
            Assert.That(statuses.Count, Is.GreaterThanOrEqualTo(3));

            gate.SetUiHidden(true);
            Assert.That(convenience.OpenSave(), Is.False);
            Assert.That(convenience.QuickSave().Status, Is.EqualTo(VNSaveLoadOperationStatus.Busy));
            Assert.That(convenience.QuickLoad().Status, Is.EqualTo(VNSaveLoadOperationStatus.Busy));
        }

        [Test]
        public void FailedQuickLoad_DoesNotNormalizeModes_ButAuthoritativeLoadStartDoes()
        {
            var saveLoad = CreateSaveLoadController();
            SetPrivateField(gate, "saveLoadController", saveLoad);
            SetPrivateField(convenience, "saveLoadController", saveLoad);
            MarkRead("session", "Keep session services.");
            Present("transient", "Invalidate only this line.");
            convenience.SetAutoEnabled(true);
            Assert.That(convenience.QuickLoad().Succeeded, Is.False);
            Assert.That(convenience.IsAutoEnabled, Is.True);

            Assert.That(convenience.TryHideUi(), Is.True);
            var requested = 0;
            convenience.SafeManualStateRequested += () => requested++;
            InvokePrivate(convenience, "HandleLoadStateChanged", true);

            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(convenience.IsSkipEnabled, Is.False);
            Assert.That(visibility.IsUiHidden, Is.False);
            Assert.That(sessionState.IsLineActive, Is.False);
            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));
            Assert.That(sessionState.ReadHistory.IsRead("session"), Is.True);
            Assert.That(requested, Is.EqualTo(1));

            InvokePrivate(convenience, "HandleLoadStateChanged", false);
            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(convenience.IsSkipEnabled, Is.False);
        }

        private VNSaveLoadController CreateSaveLoadController()
        {
            var gameObject = new GameObject("M6-04 SaveLoad Test");
            ownedObjects.Add(gameObject);
            var controller = gameObject.AddComponent<VNSaveLoadController>();
            InvokePrivateNoArguments(controller, "Awake");
            return controller;
        }

        private CanvasGroup CreateCanvasGroup(string name, float alpha, bool interactable, bool blocksRaycasts)
        {
            var gameObject = new GameObject(name);
            ownedObjects.Add(gameObject);
            var group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = alpha;
            group.interactable = interactable;
            group.blocksRaycasts = blocksRaycasts;
            return group;
        }

        private void MarkRead(string lineId, string text)
        {
            Present(lineId, text);
            CompleteDisplay();
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.True);
            lifecycle.OnLineWillDismiss();
        }

        private void Present(string lineId, string text)
        {
            lifecycle.RunLineAsync(CreateLine(lineId, text), NewLineToken());
            lifecycle.OnLineDisplayBegin(default, null);
        }

        private void CompleteDisplay() => lifecycle.OnLineDisplayComplete();

        private static LineCancellationToken NewLineToken() => new()
        {
            NextContentToken = System.Threading.CancellationToken.None,
            HurryUpToken = System.Threading.CancellationToken.None,
        };

        private static LocalizedLine CreateLine(string lineId, string text) => new()
        {
            TextID = lineId,
            Text = new MarkupParseResult(text, new List<MarkupAttribute>()),
        };

        private static void AssertHidden(CanvasGroup group)
        {
            Assert.That(group.alpha, Is.Zero);
            Assert.That(group.interactable, Is.False);
            Assert.That(group.blocksRaycasts, Is.False);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName, bool value)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, new object[] { value });
        }

        private static void InvokePrivateNoArguments(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }
    }
}
