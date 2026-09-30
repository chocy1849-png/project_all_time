using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Records.UI;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNRecordsModalTests
    {
        private readonly System.Collections.Generic.List<GameObject> ownedObjects = new();
        private VNDialogueSessionState sessionState;
        private VNInteractionGate gate;
        private VNConvenienceController convenience;
        private VNConvenienceModalController modalController;
        private VNRecordsModal recordsModal;
        private CanvasGroup recordsCanvasGroup;
        private Button recordsCloseButton;
        private Button[] tabButtons;
        private GameObject[] roots;
        private GameObject[] indicators;

        [SetUp]
        public void SetUp()
        {
            var authority = NewObject("Records Test Authority");
            authority.SetActive(false);
            sessionState = authority.AddComponent<VNDialogueSessionState>();
            gate = authority.AddComponent<VNInteractionGate>();
            convenience = authority.AddComponent<VNConvenienceController>();
            SetField(gate, "sessionState", sessionState);
            SetField(convenience, "sessionState", sessionState);
            SetField(convenience, "interactionGate", gate);
            authority.SetActive(true);

            var recordsObject = NewObject("Records Modal");
            recordsObject.SetActive(false);
            recordsCanvasGroup = recordsObject.AddComponent<CanvasGroup>();
            recordsCloseButton = NewObject("Records Close").AddComponent<Button>();
            tabButtons = new[]
            {
                NewObject("Timeline Tab").AddComponent<Button>(),
                NewObject("Gallery Tab").AddComponent<Button>(),
                NewObject("Archive Tab").AddComponent<Button>(),
                NewObject("Achievements Tab").AddComponent<Button>(),
            };
            roots = new[]
            {
                NewObject("Timeline Root"),
                NewObject("Gallery Root"),
                NewObject("Archive Root"),
                NewObject("Achievements Root"),
            };
            indicators = new[]
            {
                NewObject("Timeline Selected"),
                NewObject("Gallery Selected"),
                NewObject("Archive Selected"),
                NewObject("Achievements Selected"),
            };
            recordsModal = recordsObject.AddComponent<VNRecordsModal>();
            SetField(recordsModal, "modalCanvasGroup", recordsCanvasGroup);
            SetField(recordsModal, "closeButton", recordsCloseButton);
            SetField(recordsModal, "timelineTabButton", tabButtons[0]);
            SetField(recordsModal, "galleryTabButton", tabButtons[1]);
            SetField(recordsModal, "archiveTabButton", tabButtons[2]);
            SetField(recordsModal, "achievementsTabButton", tabButtons[3]);
            SetField(recordsModal, "timelineRoot", roots[0]);
            SetField(recordsModal, "galleryRoot", roots[1]);
            SetField(recordsModal, "archiveRoot", roots[2]);
            SetField(recordsModal, "achievementsRoot", roots[3]);
            SetField(recordsModal, "timelineSelectedIndicator", indicators[0]);
            SetField(recordsModal, "gallerySelectedIndicator", indicators[1]);
            SetField(recordsModal, "archiveSelectedIndicator", indicators[2]);
            SetField(recordsModal, "achievementsSelectedIndicator", indicators[3]);
            recordsObject.SetActive(true);
            InvokePrivateNoArguments(recordsModal, "Awake");
            InvokePrivateNoArguments(recordsModal, "OnEnable");

            var settingsObject = NewObject("Settings Modal");
            settingsObject.SetActive(false);
            var settingsModal = settingsObject.AddComponent<VNSettingsModal>();
            SetField(settingsModal, "modalCanvasGroup", settingsObject.AddComponent<CanvasGroup>());
            SetField(settingsModal, "closeButton", NewObject("Settings Close").AddComponent<Button>());
            settingsObject.SetActive(true);

            var backlogObject = NewObject("Backlog Modal");
            backlogObject.SetActive(false);
            var backlogModal = backlogObject.AddComponent<VNBacklogModal>();
            SetField(backlogModal, "modalCanvasGroup", backlogObject.AddComponent<CanvasGroup>());
            SetField(backlogModal, "closeButton", NewObject("Backlog Close").AddComponent<Button>());
            SetField(backlogModal, "content", NewObject("Backlog Content").transform);
            SetField(backlogModal, "itemPrefab", NewObject("Backlog Item Prefab").AddComponent<VNBacklogItem>());
            SetField(backlogModal, "sessionState", sessionState);
            backlogObject.SetActive(true);

            var controllerObject = NewObject("Convenience Modal Controller");
            controllerObject.SetActive(false);
            modalController = controllerObject.AddComponent<VNConvenienceModalController>();
            SetField(modalController, "interactionGate", gate);
            SetField(modalController, "convenienceController", convenience);
            SetField(modalController, "backlogModal", backlogModal);
            SetField(modalController, "settingsModal", settingsModal);
            SetField(modalController, "recordsModal", recordsModal);
            SetField(convenience, "convenienceModalController", modalController);
            controllerObject.SetActive(true);
            InvokePrivateNoArguments(modalController, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
        }

        [Test]
        public void InitialState_IsClosedAndAllDomainRootsAreInactive()
        {
            Assert.That(recordsModal.IsOpen, Is.False);
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
            Assert.That(recordsCanvasGroup.alpha, Is.Zero);
            Assert.That(recordsCanvasGroup.interactable, Is.False);
            Assert.That(recordsCanvasGroup.blocksRaycasts, Is.False);
            Assert.That(recordsModal.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            AssertRoots(null);
        }

        [Test]
        public void OpenAndTabSwitch_UpdateShellBeforeEventsAndSelectOneRoot()
        {
            var openedCount = 0;
            var changed = 0;
            recordsModal.Opened += () =>
            {
                openedCount++;
                Assert.That(recordsModal.IsOpen, Is.True);
                Assert.That(recordsCanvasGroup.alpha, Is.EqualTo(1f));
                Assert.That(recordsCanvasGroup.interactable, Is.True);
                Assert.That(recordsCanvasGroup.blocksRaycasts, Is.True);
                Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
                AssertRoots(VNRecordsTab.Timeline);
            };
            recordsModal.ActiveTabChanged += tab =>
            {
                changed++;
                Assert.That(recordsModal.ActiveTab, Is.EqualTo(tab));
                AssertRoots(tab);
                AssertIndicators(tab);
            };

            Assert.That(recordsModal.TryOpen(), Is.True);
            Assert.That(openedCount, Is.EqualTo(1));
            Assert.That(changed, Is.Zero, "Implicit initial Timeline selection does not emit a tab-change event.");
            AssertIndicators(VNRecordsTab.Timeline);

            var tabs = new[] { VNRecordsTab.Gallery, VNRecordsTab.Archive, VNRecordsTab.Achievements, VNRecordsTab.Timeline };
            foreach (var tab in tabs)
            {
                Assert.That(recordsModal.TrySelectTab(tab), Is.True);
                Assert.That(changed, Is.EqualTo(Array.IndexOf(tabs, tab) + 1));
            }

            Assert.That(recordsModal.TrySelectTab(VNRecordsTab.Timeline), Is.True);
            Assert.That(changed, Is.EqualTo(tabs.Length), "Selecting the current tab does not emit a duplicate event.");
            Assert.That(recordsModal.TrySelectTab((VNRecordsTab)99), Is.False);
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
        }

        [Test]
        public void ButtonCallbacks_MapToTabsAndAreRemovedAndReRegisteredOnce()
        {
            Assert.That(recordsModal.TryOpen(), Is.True);
            var changed = 0;
            recordsModal.ActiveTabChanged += _ => changed++;
            tabButtons[0].onClick.Invoke();
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
            Assert.That(changed, Is.Zero);

            tabButtons[3].onClick.Invoke();
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Achievements));
            Assert.That(changed, Is.EqualTo(1));
            tabButtons[0].onClick.Invoke();
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
            Assert.That(changed, Is.EqualTo(2));

            recordsModal.gameObject.SetActive(false);
            InvokePrivateNoArguments(recordsModal, "OnDisable");
            recordsModal.gameObject.SetActive(true);
            InvokePrivateNoArguments(recordsModal, "OnEnable");
            tabButtons[1].onClick.Invoke();
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Gallery));
            Assert.That(changed, Is.EqualTo(3));

            recordsModal.gameObject.SetActive(false);
            InvokePrivateNoArguments(recordsModal, "OnDisable");
            recordsModal.gameObject.SetActive(true);
            InvokePrivateNoArguments(recordsModal, "OnEnable");
            tabButtons[2].onClick.Invoke();
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Archive));
            Assert.That(changed, Is.EqualTo(4));
        }

        [Test]
        public void Close_IsIdempotentAndClosedEventObservesHiddenShell()
        {
            var closedCount = 0;
            recordsModal.Closed += () =>
            {
                closedCount++;
                Assert.That(recordsModal.IsOpen, Is.False);
                Assert.That(recordsCanvasGroup.alpha, Is.Zero);
                Assert.That(recordsCanvasGroup.interactable, Is.False);
                Assert.That(recordsCanvasGroup.blocksRaycasts, Is.False);
                AssertRoots(null);
            };

            Assert.That(recordsModal.Close(), Is.True);
            Assert.That(closedCount, Is.Zero);
            Assert.That(recordsModal.TryOpen(), Is.True);
            Assert.That(recordsModal.TrySelectTab(VNRecordsTab.Archive), Is.True);
            Assert.That(recordsModal.Close(), Is.True);
            Assert.That(closedCount, Is.EqualTo(1));
            Assert.That(recordsModal.Close(), Is.True);
            Assert.That(closedCount, Is.EqualTo(1));
            Assert.That(recordsModal.TryOpen(), Is.True);
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
        }

        [Test]
        public void WiringValidation_ReportsMissingReferencesAndDuplicateRoots()
        {
            var incompleteObject = NewObject("Incomplete Records Modal");
            var incomplete = incompleteObject.AddComponent<VNRecordsModal>();
            Assert.That(incomplete.TryValidateWiring(out var missingDiagnostic), Is.False);
            Assert.That(missingDiagnostic, Does.Contain("CanvasGroup"));
            Assert.That(incomplete.TryOpen(), Is.False);

            SetField(recordsModal, "galleryRoot", roots[0]);
            Assert.That(recordsModal.TryValidateWiring(out var duplicateDiagnostic), Is.False);
            Assert.That(duplicateDiagnostic, Does.Contain("distinct root"));
            Assert.That(recordsModal.TryOpen(), Is.False);
        }

        [TestCase("timelineSelectedIndicator", "Timeline")]
        [TestCase("gallerySelectedIndicator", "Gallery")]
        [TestCase("archiveSelectedIndicator", "Archive")]
        [TestCase("achievementsSelectedIndicator", "Achievements")]
        public void WiringValidation_RejectsEachMissingSelectedIndicator(string fieldName, string tabName)
        {
            SetField(recordsModal, fieldName, null);

            Assert.That(recordsModal.TryValidateWiring(out var diagnostic), Is.False);
            Assert.That(diagnostic, Is.EqualTo(tabName + " selected indicator reference is missing."));
            Assert.That(recordsModal.TryOpen(), Is.False);
        }

        [TestCase(0, 1)]
        [TestCase(0, 2)]
        [TestCase(0, 3)]
        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(2, 3)]
        public void WiringValidation_RejectsEveryDuplicateSelectedIndicatorPair(int first, int second)
        {
            var fieldNames = new[]
            {
                "timelineSelectedIndicator",
                "gallerySelectedIndicator",
                "archiveSelectedIndicator",
                "achievementsSelectedIndicator",
            };
            SetField(recordsModal, fieldNames[second], indicators[first]);

            Assert.That(recordsModal.TryValidateWiring(out var diagnostic), Is.False);
            Assert.That(diagnostic, Is.EqualTo("Each Records tab must have a distinct selected indicator GameObject."));
            Assert.That(recordsModal.TryOpen(), Is.False);
        }

        [Test]
        public void Controller_ArbitratesRecordsWithBacklogAndSettingsAndUsesTheExistingGate()
        {
            var m5Object = NewObject("M5 for Gate Policy");
            m5Object.SetActive(false);
            var saveLoadModal = m5Object.AddComponent<VNSaveLoadModal>();
            var saveLoadController = m5Object.AddComponent<VNSaveLoadController>();
            SetField(saveLoadController, "saveLoadModal", saveLoadModal);
            m5Object.SetActive(true);
            SetField(gate, "saveLoadController", saveLoadController);
            SetField(convenience, "saveLoadController", saveLoadController);

            Assert.That(gate.CanAdvanceStory, Is.True);
            Assert.That(modalController.TryOpenRecords(), Is.True);
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.Records));
            Assert.That(gate.IsConvenienceModalActive, Is.True);
            Assert.That(gate.IsBlockingModalActive, Is.True);
            Assert.That(gate.CanAdvanceStory, Is.False);
            Assert.That(gate.CanRunAutomation, Is.False);
            Assert.That(gate.CanUseSaveLoad, Is.False);
            Assert.That(gate.CanChangeConvenienceMode, Is.False);
            Assert.That(convenience.OpenSave(), Is.False);
            Assert.That(convenience.OpenLoad(), Is.False);
            Assert.That(convenience.QuickSave().Status, Is.EqualTo(VNSaveLoadOperationStatus.Busy));
            Assert.That(convenience.QuickLoad().Status, Is.EqualTo(VNSaveLoadOperationStatus.Busy));
            Assert.That(modalController.TryOpenBacklog(), Is.False);
            Assert.That(modalController.TryOpenSettings(), Is.False);

            Assert.That(modalController.CloseActiveModal(), Is.True);
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.None));
            Assert.That(gate.IsConvenienceModalActive, Is.False);
            Assert.That(gate.CanAdvanceStory, Is.True);
            Assert.That(gate.CanUseSaveLoad, Is.True);

            Assert.That(modalController.TryOpenBacklog(), Is.True);
            Assert.That(modalController.TryOpenRecords(), Is.False);
            Assert.That(modalController.CloseActiveModal(), Is.True);
            Assert.That(modalController.TryOpenSettings(), Is.True);
            Assert.That(modalController.TryOpenRecords(), Is.False);
            Assert.That(modalController.CloseActiveModal(), Is.True);
        }

        [Test]
        public void Controller_RejectsRecordsForM5ModalLoadAndHiddenUi()
        {
            var m5Object = NewObject("M5 Save Load");
            m5Object.SetActive(false);
            var saveLoadModal = m5Object.AddComponent<VNSaveLoadModal>();
            var saveLoadController = m5Object.AddComponent<VNSaveLoadController>();
            SetField(saveLoadController, "saveLoadModal", saveLoadModal);
            m5Object.SetActive(true);
            SetField(gate, "saveLoadController", saveLoadController);

            SetField(saveLoadModal, "isOpen", true);
            Assert.That(modalController.TryOpenRecords(), Is.False, "M5 modal ownership blocks Records.");
            SetField(saveLoadModal, "isOpen", false);

            SetField(saveLoadController, "loadInProgress", true);
            Assert.That(modalController.TryOpenRecords(), Is.False, "An active Load transaction blocks Records.");
            SetField(saveLoadController, "loadInProgress", false);

            gate.SetUiHidden(true);
            Assert.That(modalController.TryOpenRecords(), Is.False, "Hidden UI blocks Records.");
            gate.SetUiHidden(false);
            Assert.That(modalController.TryOpenRecords(), Is.True);
            Assert.That(gate.CanUseSaveLoad, Is.False);
        }

        [Test]
        public void Controller_RollsBackGateWhenRecordsWiringIsInvalid()
        {
            SetField(recordsModal, "modalCanvasGroup", null);
            Assert.That(modalController.TryOpenRecords(), Is.False);
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.None));
            Assert.That(gate.IsConvenienceModalActive, Is.False);
        }

        [Test]
        public void CloseButtonCancelAndLoadSafeStateUseCoordinatorOwnership()
        {
            Assert.That(modalController.TryOpenRecords(), Is.True);
            recordsCloseButton.onClick.Invoke();
            Assert.That(recordsModal.IsOpen, Is.False);
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.None));
            Assert.That(gate.IsConvenienceModalActive, Is.False);

            Assert.That(modalController.TryOpenRecords(), Is.True);
            Assert.That(convenience.HandleCancel(), Is.True);
            Assert.That(recordsModal.IsOpen, Is.False);
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.None));
            Assert.That(gate.IsConvenienceModalActive, Is.False);

            Assert.That(modalController.TryOpenRecords(), Is.True);
            InvokePrivate(convenience, "HandleLoadStateChanged", true);
            Assert.That(recordsModal.IsOpen, Is.False);
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.None));
            Assert.That(gate.IsConvenienceModalActive, Is.False);
        }

        [Test]
        public void QuickControlRecordsListenerIsSingleAndSurvivesDisableEnable()
        {
            var barObject = NewObject("QuickControl Bar");
            barObject.SetActive(false);
            var bar = barObject.AddComponent<VNQuickControlBar>();
            var recordsButton = NewObject("QuickControl Records").AddComponent<Button>();
            SetField(bar, "modalController", modalController);
            SetField(bar, "recordsButton", recordsButton);
            var opened = 0;
            recordsModal.Opened += () => opened++;
            barObject.SetActive(true);
            InvokePrivateNoArguments(bar, "OnEnable");
            Assert.That(RuntimeListenerCount(recordsButton), Is.EqualTo(1));

            recordsButton.onClick.Invoke();
            Assert.That(opened, Is.EqualTo(1));
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.Records));
            Assert.That(modalController.CloseActiveModal(), Is.True);

            barObject.SetActive(false);
            InvokePrivateNoArguments(bar, "OnDisable");
            Assert.That(RuntimeListenerCount(recordsButton), Is.Zero);
            barObject.SetActive(true);
            InvokePrivateNoArguments(bar, "OnEnable");
            Assert.That(RuntimeListenerCount(recordsButton), Is.EqualTo(1));
            recordsButton.onClick.Invoke();
            Assert.That(opened, Is.EqualTo(2));
            Assert.That(modalController.ActiveModal, Is.EqualTo(VNConvenienceModalKind.Records));
        }

        private void AssertRoots(VNRecordsTab? selected)
        {
            var activeCount = 0;
            for (var index = 0; index < roots.Length; index++)
            {
                var expectedActive = selected.HasValue && (int)selected.Value == index;
                Assert.That(roots[index].activeSelf, Is.EqualTo(expectedActive));
                if (roots[index].activeSelf) activeCount++;
            }
            Assert.That(activeCount, Is.EqualTo(selected.HasValue ? 1 : 0));
        }

        private void AssertIndicators(VNRecordsTab selected)
        {
            for (var index = 0; index < indicators.Length; index++)
                Assert.That(indicators[index].activeSelf, Is.EqualTo((int)selected == index));
        }

        private GameObject NewObject(string name)
        {
            var gameObject = new GameObject(name);
            ownedObjects.Add(gameObject);
            return gameObject;
        }

        private static void SetField(object target, string fieldName, object value)
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

        private static int RuntimeListenerCount(Button button)
        {
            var callsField = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(callsField, Is.Not.Null, "UnityEvent runtime call-list field");
            var calls = callsField.GetValue(button.onClick);
            var runtimeCallsField = calls.GetType().GetField("m_RuntimeCalls", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(runtimeCallsField, Is.Not.Null, "UnityEvent runtime listener field");
            return ((ICollection)runtimeCallsField.GetValue(calls)).Count;
        }
    }
}
