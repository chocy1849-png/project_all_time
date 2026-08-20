using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNQuickControlModalTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private VNDialogueSessionState sessionState;
        private VNLineLifecyclePresenter lifecycle;
        private VNInteractionGate gate;
        private VNLineAdvancerInputBridge bridge;
        private VNConvenienceController convenience;
        private VNUIVisibilityController visibility;
        private VNConvenienceModalController modalController;
        private VNBacklogModal backlogModal;
        private VNSettingsModal settingsModal;
        private Button backlogCloseButton;
        private Button settingsCloseButton;
        private TextMeshProUGUI backlogEmptyStateText;
        private int forwardedCount;

        [SetUp]
        public void SetUp()
        {
            forwardedCount = 0;
            var root = NewObject("M6-05 Root");
            root.AddComponent<LineAdvancer>().enabled = false;
            sessionState = root.AddComponent<VNDialogueSessionState>();
            lifecycle = root.AddComponent<VNLineLifecyclePresenter>();
            gate = root.AddComponent<VNInteractionGate>();
            bridge = root.AddComponent<VNLineAdvancerInputBridge>();
            visibility = root.AddComponent<VNUIVisibilityController>();
            convenience = root.AddComponent<VNConvenienceController>();
            modalController = root.AddComponent<VNConvenienceModalController>();
            backlogModal = NewObject("Backlog Modal").AddComponent<VNBacklogModal>();
            settingsModal = NewObject("Settings Modal").AddComponent<VNSettingsModal>();

            var dialogueLayer = NewObject("Dialogue Layer").AddComponent<CanvasGroup>();
            var quickLayer = NewObject("Quick Layer").AddComponent<CanvasGroup>();
            var backlogCanvas = backlogModal.gameObject.AddComponent<CanvasGroup>();
            var settingsCanvas = settingsModal.gameObject.AddComponent<CanvasGroup>();
            var content = NewObject("Backlog Content").transform;
            var itemPrefab = NewObject("Backlog Item Prefab").AddComponent<VNBacklogItem>();
            backlogCloseButton = NewObject("Backlog Close").AddComponent<Button>();
            settingsCloseButton = NewObject("Settings Close").AddComponent<Button>();
            backlogEmptyStateText = NewObject("Backlog Empty State").AddComponent<TextMeshProUGUI>();
            backlogEmptyStateText.text = "Authored empty-state copy";

            SetPrivateField(lifecycle, "sessionState", sessionState);
            SetPrivateField(gate, "sessionState", sessionState);
            SetPrivateField(bridge, "sessionState", sessionState);
            SetPrivateField(bridge, "interactionGate", gate);
            SetPrivateField(visibility, "dialogueLayer", dialogueLayer);
            SetPrivateField(visibility, "quickControlLayer", quickLayer);
            SetPrivateField(visibility, "interactionGate", gate);
            SetPrivateField(convenience, "sessionState", sessionState);
            SetPrivateField(convenience, "advanceBridge", bridge);
            SetPrivateField(convenience, "interactionGate", gate);
            SetPrivateField(convenience, "uiVisibilityController", visibility);
            SetPrivateField(convenience, "convenienceModalController", modalController);
            SetPrivateField(backlogModal, "modalCanvasGroup", backlogCanvas);
            SetPrivateField(backlogModal, "content", content);
            SetPrivateField(backlogModal, "itemPrefab", itemPrefab);
            SetPrivateField(backlogModal, "sessionState", sessionState);
            SetPrivateField(backlogModal, "closeButton", backlogCloseButton);
            SetPrivateField(backlogModal, "emptyStateText", backlogEmptyStateText);
            SetPrivateField(settingsModal, "modalCanvasGroup", settingsCanvas);
            SetPrivateField(settingsModal, "closeButton", settingsCloseButton);
            SetPrivateField(modalController, "interactionGate", gate);
            SetPrivateField(modalController, "convenienceController", convenience);
            SetPrivateField(modalController, "backlogModal", backlogModal);
            SetPrivateField(modalController, "settingsModal", settingsModal);
            InvokePrivateNoArguments(backlogModal, "OnEnable");
            InvokePrivateNoArguments(settingsModal, "OnEnable");
            InvokePrivateNoArguments(modalController, "OnEnable");
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
        public void ModalArbitration_OptionsMayOpenBacklog_AndCloseClearsOnlyConvenienceBlock()
        {
            Present("choice", "Choose.");
            lifecycle.RunOptionsAsync(Array.Empty<DialogueOption>(), Token());
            Assert.That(modalController.TryOpenBacklog(), Is.True);
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.Backlog));
            Assert.That(gate.IsConvenienceModalActive, Is.True);
            Assert.That(backlogModal.IsOpen, Is.True);
            Assert.That(modalController.TryOpenSettings(), Is.False);

            convenience.SetAutoEnabled(true);
            Assert.That(modalController.CloseActiveModal(), Is.True);
            Assert.That(gate.IsConvenienceModalActive, Is.False);
            Assert.That(backlogModal.IsOpen, Is.False);
            Assert.That(convenience.IsAutoEnabled, Is.True);
        }

        [Test]
        public void SettingsShell_UsesCanvasGroupAndLoadSafeStateClosesIt()
        {
            Assert.That(modalController.TryOpenSettings(), Is.True);
            var group = settingsModal.GetComponent<CanvasGroup>();
            Assert.That(group.alpha, Is.EqualTo(1f));
            Assert.That(group.interactable, Is.True);
            Assert.That(group.blocksRaycasts, Is.True);

            InvokePrivate(convenience, "HandleLoadStateChanged", true);
            Assert.That(modalController.IsConvenienceModalOpen, Is.False);
            Assert.That(group.alpha, Is.Zero);
            Assert.That(group.interactable, Is.False);
            Assert.That(group.blocksRaycasts, Is.False);
        }

        [Test]
        public void ModalCloseButtons_DelegateClosureToTheCoordinator()
        {
            Assert.That(modalController.TryOpenBacklog(), Is.True);
            backlogCloseButton.onClick.Invoke();
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.None));
            Assert.That(gate.IsConvenienceModalActive, Is.False);
            Assert.That(backlogModal.IsOpen, Is.False);

            Assert.That(modalController.TryOpenSettings(), Is.True);
            settingsCloseButton.onClick.Invoke();
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.None));
            Assert.That(gate.IsConvenienceModalActive, Is.False);
            Assert.That(settingsModal.IsOpen, Is.False);
        }

        [Test]
        public void Backlog_BindsInsertionOrder_ReusesPool_AndPreservesSessionData()
        {
            MarkRead("repeat", "First.", "Eve");
            MarkRead("repeat", "Second.", null);
            Assert.That(modalController.TryOpenBacklog(), Is.True);
            Assert.That(backlogModal.PooledItemCount, Is.EqualTo(2));
            Assert.That(sessionState.Backlog.Count, Is.EqualTo(2));
            Assert.That(modalController.CloseActiveModal(), Is.True);

            MarkRead("third", "Third.", "Mina");
            Assert.That(modalController.TryOpenBacklog(), Is.True);
            Assert.That(backlogModal.PooledItemCount, Is.EqualTo(3));
            Assert.That(sessionState.Backlog.Entries[0].Text, Is.EqualTo("First."));
            Assert.That(sessionState.Backlog.Entries[1].Text, Is.EqualTo("Second."));
            Assert.That(sessionState.Backlog.Entries[2].Text, Is.EqualTo("Third."));
            Assert.That(sessionState.Backlog.Entries[1].IsNarration, Is.True);
        }

        [Test]
        public void BacklogEmptyState_OnlyChangesVisibilityAndPreservesAuthoredText()
        {
            Assert.That(modalController.TryOpenBacklog(), Is.True);
            Assert.That(backlogEmptyStateText.gameObject.activeSelf, Is.True);
            Assert.That(backlogEmptyStateText.text, Is.EqualTo("Authored empty-state copy"));

            Assert.That(modalController.CloseActiveModal(), Is.True);
            MarkRead("entry", "One entry.", null);
            Assert.That(modalController.TryOpenBacklog(), Is.True);
            Assert.That(backlogEmptyStateText.gameObject.activeSelf, Is.False);
            Assert.That(backlogEmptyStateText.text, Is.EqualTo("Authored empty-state copy"));
        }

        [Test]
        public void QuickControl_DispatchesOnce_AndModeIndicatorsMirrorRuntimeTruth()
        {
            Present("next", "Next.");
            var bar = NewObject("QuickControl Bar").AddComponent<VNQuickControlBar>();
            var next = NewObject("Next").AddComponent<Button>();
            var skip = NewObject("Skip").AddComponent<Button>();
            var auto = NewObject("Auto").AddComponent<Button>();
            var backlog = NewObject("Backlog").AddComponent<Button>();
            var settings = NewObject("Settings").AddComponent<Button>();
            var autoIndicator = NewObject("Auto Indicator");
            var skipIndicator = NewObject("Skip Indicator");
            SetPrivateField(bar, "convenienceController", convenience);
            SetPrivateField(bar, "modalController", modalController);
            SetPrivateField(bar, "nextButton", next);
            SetPrivateField(bar, "skipButton", skip);
            SetPrivateField(bar, "autoButton", auto);
            SetPrivateField(bar, "backlogButton", backlog);
            SetPrivateField(bar, "settingsButton", settings);
            SetPrivateField(bar, "autoSelectedIndicator", autoIndicator);
            SetPrivateField(bar, "skipSelectedIndicator", skipIndicator);
            InvokePrivateNoArguments(bar, "OnEnable");

            next.onClick.Invoke();
            Assert.That(forwardedCount, Is.EqualTo(1));
            auto.onClick.Invoke();
            Assert.That(convenience.IsAutoEnabled, Is.True);
            Assert.That(autoIndicator.activeSelf, Is.True);
            skip.onClick.Invoke();
            Assert.That(convenience.IsSkipEnabled, Is.True);
            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(autoIndicator.activeSelf, Is.False);
            Assert.That(skipIndicator.activeSelf, Is.True);
            backlog.onClick.Invoke();
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.Backlog));
        }

        [Test]
        public void Cancel_ClosesConvenienceModalBeforeRestoringHiddenUi_AndNeverAdvances()
        {
            Present("cancel", "Cancel.");
            Assert.That(modalController.TryOpenSettings(), Is.True);
            Assert.That(convenience.HandleCancel(), Is.True);
            Assert.That(modalController.IsConvenienceModalOpen, Is.False);
            Assert.That(forwardedCount, Is.Zero);

            Assert.That(convenience.TryHideUi(), Is.True);
            Assert.That(convenience.HandleCancel(), Is.True);
            Assert.That(visibility.IsUiHidden, Is.False);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(convenience.HandleCancel(), Is.False);
        }

        private GameObject NewObject(string name)
        {
            var gameObject = new GameObject(name);
            ownedObjects.Add(gameObject);
            return gameObject;
        }

        private void MarkRead(string lineId, string text, string speaker)
        {
            Present(lineId, text, speaker);
            lifecycle.OnLineDisplayComplete();
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.True);
            lifecycle.OnLineWillDismiss();
        }

        private void Present(string lineId, string text, string speaker = null)
        {
            lifecycle.RunLineAsync(CreateLine(lineId, text, speaker), Token());
            lifecycle.OnLineDisplayBegin(default, null);
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
        }

        private static LineCancellationToken Token() => new()
        {
            NextContentToken = System.Threading.CancellationToken.None,
            HurryUpToken = System.Threading.CancellationToken.None,
        };

        private static LocalizedLine CreateLine(string lineId, string text, string speaker)
        {
            var attributes = new List<MarkupAttribute>();
            if (speaker != null)
            {
                var constructor = typeof(MarkupAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new[] { typeof(int), typeof(int), typeof(int), typeof(string), typeof(IEnumerable<MarkupProperty>) }, null);
                var propertyConstructor = typeof(MarkupProperty).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new[] { typeof(string), typeof(string) }, null);
                attributes.Add((MarkupAttribute)constructor.Invoke(new object[]
                {
                    0, 0, 0, "character", new[] { (MarkupProperty)propertyConstructor.Invoke(new object[] { "name", speaker }) },
                }));
            }

            return new LocalizedLine { TextID = lineId, Text = new MarkupParseResult(text, attributes) };
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
