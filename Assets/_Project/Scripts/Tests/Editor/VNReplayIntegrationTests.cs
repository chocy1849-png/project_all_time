using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records.Replay;
using ProjectAllTime.VN.Records.Replay.UI;
using ProjectAllTime.VN.Records.Timeline;
using ProjectAllTime.VN.Records.Timeline.UI;
using ProjectAllTime.VN.Records.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNReplayIntegrationTests
    {
        private const string YarnProjectPath = "Assets/_Project/Yarn/GameNarrative.yarnproject";
        private readonly List<GameObject> ownedObjects = new();
        private readonly List<UnityEngine.Object> ownedAssets = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private int metaWrites;
        private VNTimelineCatalog timelineCatalog;
        private VNTimelineRuntime timelineRuntime;
        private YarnProject yarnProject;
        private VNPresentationCatalog presentationCatalog;
        private VNRecordsModal recordsModal;
        private VNReplayController replayController;
        private Transform replaySessionHost;
        private VNReplayView replayViewTemplate;
        private VNTimelineView timelineView;
        private GameObject recordsRoot;
        private GameObject timelineRoot;
        private CanvasGroup recordsCanvasGroup;
        private bool playModeOptionsCaptured;
        private bool previousEnterPlayModeOptionsEnabled;
        private EnterPlayModeOptions previousEnterPlayModeOptions;
        private bool backgroundSettingCaptured;
        private bool previousRunInBackground;

        [TearDown]
        public void TearDown()
        {
            if (backgroundSettingCaptured) Application.runInBackground = previousRunInBackground;
            backgroundSettingCaptured = false;
            RestorePlayModeOptions();
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                if (ownedObjects[index] != null) UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            for (var index = ownedAssets.Count - 1; index >= 0; index--)
                if (ownedAssets[index] != null) UnityEngine.Object.DestroyImmediate(ownedAssets[index]);
            ownedAssets.Clear();
            if (!string.IsNullOrWhiteSpace(temporaryRoot) && Directory.Exists(temporaryRoot))
                Directory.Delete(temporaryRoot, true);
            temporaryRoot = null;
        }

        [Test]
        public void ControllerInitializationAndEligibility_AreFreshFailClosedAndWriteFree()
        {
            CreateHarness();

            var uninitializedObject = NewObject("Uninitialized Replay Controller");
            var uninitialized = uninitializedObject.AddComponent<VNReplayController>();
            Assert.That(uninitialized.Initialize(null, timelineCatalog, yarnProject), Is.False);
            Assert.That(uninitialized.Initialize(timelineRuntime, null, yarnProject), Is.False);
            Assert.That(uninitialized.Initialize(timelineRuntime, timelineCatalog, null), Is.False);

            Assert.That(replayController.Initialize(timelineRuntime, timelineCatalog, yarnProject), Is.True);
            Assert.That(replayController.Initialize(timelineRuntime, timelineCatalog, yarnProject), Is.True,
                "The same injected authorities initialize idempotently.");
            var alternateCatalog = CreateTimelineCatalog(Array.Empty<VNTimelineEntryDefinition>());
            var alternateRuntime = new VNTimelineRuntime(alternateCatalog, new VNTimelineService(alternateCatalog, metaProgress), metaProgress);
            Assert.That(replayController.Initialize(alternateRuntime, alternateCatalog, yarnProject), Is.False);
            StringAssert.Contains("different dependencies", replayController.LastDiagnostic);

            var writesBefore = metaWrites;
            var bytesBefore = File.ReadAllBytes(repository.CanonicalFilePath);
            Assert.That(replayController.CanReplay("entry_safe"), Is.True, replayController.LastDiagnostic);
            Assert.That(replayController.CanReplay("entry_discovered"), Is.False);
            StringAssert.Contains("not completed", replayController.LastDiagnostic);
            Assert.That(replayController.CanReplay("entry_no_node"), Is.False);
            StringAssert.Contains("no Replay node", replayController.LastDiagnostic);
            Assert.That(replayController.CanReplay("entry_unsafe"), Is.False);
            StringAssert.Contains("not safe", replayController.LastDiagnostic);
            Assert.That(replayController.CanReplay("entry_hidden"), Is.False);
            StringAssert.Contains("not visible", replayController.LastDiagnostic);
            Assert.That(replayController.CanReplay("missing_entry"), Is.False);
            Assert.That(replayController.CanReplay("  "), Is.False);
            Assert.That(replayController.TryStartReplay("entry_discovered"), Is.False);
            Assert.That(replayController.TryStartReplay("entry_no_node"), Is.False);
            Assert.That(replayController.TryStartReplay("entry_unsafe"), Is.False);
            Assert.That(replayController.HasActiveSession, Is.False);
            Assert.That(replaySessionHost.childCount, Is.EqualTo(1), "Ineligible entries never create a Replay session root.");
            Assert.That(metaWrites, Is.EqualTo(writesBefore));
            CollectionAssert.AreEqual(bytesBefore, File.ReadAllBytes(repository.CanonicalFilePath));
        }

        [Test]
        public void StaleTimelineRow_RechecksFreshSnapshotBeforeCreatingRunner()
        {
            CreateHarness();
            var staleRow = FindTimelineRow("entry_safe");
            var replayButton = GetField<Button>(staleRow, "replayButton");
            Assert.That(replayButton.gameObject.activeSelf && replayButton.interactable, Is.True);

            ReplaceTimelineEntries(timelineCatalog,
                CreateEntry("entry_discovered", "Discovered", "line:integration_discovered", "line:integration_discovered_done", "M9_REPLAY_SAFE_START"),
                CreateEntry("entry_no_node", "No Replay node", "line:integration_no_node", "line:integration_no_node_done", null),
                CreateEntry("entry_unsafe", "Unsafe", "line:integration_unsafe", "line:integration_unsafe_done", "M9_REPLAY_UNSAFE_CHECKPOINT"),
                CreateEntry("entry_hidden", "Hidden", "line:integration_hidden", "line:integration_hidden_done", "M9_REPLAY_SAFE_START"));

            replayButton.onClick.Invoke();

            Assert.That(replayController.HasActiveSession, Is.False);
            Assert.That(replaySessionHost.childCount, Is.EqualTo(1));
            StringAssert.Contains("not visible", replayController.LastDiagnostic);
        }

        [Test]
        public void ReplayView_ValidatesOwnedWiringAndButtonRebindingDoesNotAccumulateListeners()
        {
            CreateHarness();
            var view = replayViewTemplate;
            Assert.That(view.TryValidateWiring(out var validDiagnostic), Is.True, validDiagnostic);

            var originalCanvasGroup = GetField<CanvasGroup>(view, "overlayCanvasGroup");
            var originalPresentation = GetField<VNPresentationController>(view, "presentationController");
            var originalPresenter = GetField<LinePresenter>(view, "linePresenter");
            var originalNext = GetField<Button>(view, "nextButton");
            var originalClose = GetField<Button>(view, "closeButton");

            AssertMissingWiring(view, "overlayCanvasGroup", "CanvasGroup");
            AssertMissingWiring(view, "presentationController", "VNPresentationController");
            AssertMissingWiring(view, "linePresenter", "LinePresenter");
            AssertMissingWiring(view, "nextButton", "Next button");
            AssertMissingWiring(view, "closeButton", "Close button");
            SetField(view, "overlayCanvasGroup", originalCanvasGroup);
            SetField(view, "presentationController", originalPresentation);
            SetField(view, "linePresenter", originalPresenter);
            SetField(view, "nextButton", originalNext);
            SetField(view, "closeButton", originalClose);

            var externalPresentationObject = NewObject("External Replay Presentation");
            var externalPresentation = externalPresentationObject.AddComponent<VNPresentationController>();
            SetField(view, "presentationController", externalPresentation);
            Assert.That(view.TryValidateWiring(out var presentationDiagnostic), Is.False);
            StringAssert.Contains("must belong", presentationDiagnostic);
            SetField(view, "presentationController", originalPresentation);

            var externalPresenterObject = NewObject("External Replay LinePresenter");
            externalPresenterObject.SetActive(false);
            var externalPresenter = externalPresenterObject.AddComponent<LinePresenter>();
            SetField(view, "linePresenter", externalPresenter);
            Assert.That(view.TryValidateWiring(out var presenterDiagnostic), Is.False);
            StringAssert.Contains("must belong", presenterDiagnostic);
            SetField(view, "linePresenter", originalPresenter);

            var runnerObject = NewObject("Replay Button Test Runner");
            runnerObject.SetActive(false);
            var runner = runnerObject.AddComponent<DialogueRunner>();
            var closeRequests = 0;
            view.CloseRequested += () => closeRequests++;
            originalPresenter.autoAdvance = true;
            Assert.That(view.Bind(runner, out var bindDiagnostic), Is.True, bindDiagnostic);
            Assert.That(originalPresenter.autoAdvance, Is.False);
            originalNext.onClick.Invoke();
            originalClose.onClick.Invoke();
            Assert.That(closeRequests, Is.EqualTo(1));
            Assert.That(view.Bind(runner, out bindDiagnostic), Is.True, bindDiagnostic);
            originalClose.onClick.Invoke();
            Assert.That(closeRequests, Is.EqualTo(2), "Rebinding retains exactly one Close callback.");
            view.Unbind();
            originalNext.onClick.Invoke();
            originalClose.onClick.Invoke();
            Assert.That(closeRequests, Is.EqualTo(2));
            Assert.That(view.IsBlockingInput, Is.False);
        }

        [UnityTest]
        public IEnumerator TimelineReplayFlow_UsesManualNextAndCleansUpOnCompletionCloseTabAndRecordsClose()
        {
            EnablePlayModeWithoutDomainReload();
            yield return new EnterPlayMode(expectDomainReload: false);
            previousRunInBackground = Application.runInBackground;
            backgroundSettingCaptured = true;
            Application.runInBackground = true; // MCP may run with the Editor unfocused; keep the awaited player loop alive.
            CreateHarness();

            Assert.That(recordsModal.IsOpen, Is.True);
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
            Assert.That(timelineView.TryValidateWiring(out var timelineDiagnostic), Is.True, timelineDiagnostic);
            Assert.That(timelineView.SelectedChapterId, Is.EqualTo("chapter_replay"));
            var writesBeforeReplay = metaWrites;
            var metaBytesBeforeReplay = File.ReadAllBytes(repository.CanonicalFilePath);

            var originalTimelineRoot = timelineRoot;
            var originalModal = recordsModal;
            var originalHost = replaySessionHost;
            var originalController = replayController;
            var originalTimelineView = timelineView;

            // First Replay completes through the Timeline row, Replay LinePresenter, and its own Next button.
            StartFromTimelineRow("entry_safe");
            var firstSession = ActiveSession();
            var firstView = ActiveView();
            AssertRuntimeSessionOwnership(firstSession, firstView, originalHost, originalModal, originalTimelineRoot);
            yield return WaitFor(() => firstView.LinePresenter.lineText.text.Contains("Replay probe is"),
                "Replay LinePresenter did not receive the first Yarn line.");
            var firstLineText = firstView.LinePresenter.lineText.text;
            for (var frame = 0; frame < 3; frame++) yield return null;
            Assert.That(firstView.LinePresenter.lineText.text, Is.EqualTo(firstLineText),
                "Replay remains paused on its current line until the Replay Next button is used.");
            Assert.That(firstView.LinePresenter.autoAdvance, Is.False);

            yield return AdvanceToLine(firstView, "Replay background is set.");
            Assert.That(firstSession.PresentationController.CurrentBackgroundId, Is.EqualTo("replay_background"));
            yield return AdvanceToLine(firstView, "Replay CG is set.");
            Assert.That(firstSession.PresentationController.CurrentCGId, Is.EqualTo("m3_cg"));
            yield return AdvanceToLine(firstView, "Replay CG is clear.");
            Assert.That(firstSession.PresentationController.CurrentCGId, Is.Null);
            yield return AdvanceToLine(firstView, "Replay character presentation is set.");
            Assert.That(firstSession.PresentationController.VisibleCharacters.TryGetValue("m9_replay_character", out var character), Is.True);
            Assert.That(character.ExpressionId, Is.EqualTo("replay_expression"));
            Assert.That(character.Slot, Is.EqualTo(VNCharacterSlot.Left));
            Assert.That(character.Facing, Is.EqualTo(VNCharacterFacing.Left));
            Assert.That(character.Scale, Is.EqualTo(1.25f));
            yield return AdvanceToLine(firstView, "Replay character presentation is clear.");
            Assert.That(firstSession.PresentationController.VisibleCharacters, Is.Empty);
            firstView.NextButton.onClick.Invoke();
            yield return WaitFor(() => !originalController.HasActiveSession && originalHost.childCount == 1,
                "Normal Replay completion did not dispose its session view.");
            Assert.That(originalModal.IsOpen, Is.True);
            Assert.That(originalModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
            Assert.That(originalTimelineRoot.activeSelf, Is.True);
            Assert.That(originalTimelineView.SelectedChapterId, Is.EqualTo("chapter_replay"));

            // Complete -> cancel -> complete on the same Timeline entry proves UI rebinds stay usable.
            StartFromTimelineRow("entry_safe");
            var cancelledSession = ActiveSession();
            var cancelledView = ActiveView();
            yield return WaitFor(() => cancelledView.LinePresenter.lineText.text.Contains("Replay probe is"),
                "Second Replay did not start.");
            cancelledView.CloseButton.onClick.Invoke();
            yield return WaitFor(() => !originalController.HasActiveSession && originalHost.childCount == 1,
                "Replay Close did not cancel and dispose the session.");
            Assert.That(originalModal.IsOpen, Is.True);
            Assert.That(originalModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
            Assert.That(cancelledSession.Lifecycle, Is.EqualTo(VNReplaySessionLifecycle.Disposed));

            StartFromTimelineRow("entry_safe");
            var repeatedView = ActiveView();
            yield return WaitFor(() => repeatedView.LinePresenter.lineText.text.Contains("Replay probe is"),
                "Third Replay did not start.");
            Assert.That(repeatedView.LinePresenter.lineText.text, Does.Contain("0"),
                "Each integration Replay receives fresh initialized Yarn storage.");
            repeatedView.CloseButton.onClick.Invoke();
            yield return WaitFor(() => !originalController.HasActiveSession && originalHost.childCount == 1,
                "Repeated Replay cleanup left a session root behind.");

            // Programmatic tab changes and closing Records also own Replay lifetime.
            StartFromTimelineRow("entry_safe");
            var tabSession = ActiveSession();
            Assert.That(originalModal.TrySelectTab(VNRecordsTab.Gallery), Is.True);
            yield return WaitFor(() => !originalController.HasActiveSession && originalHost.childCount == 1,
                "Leaving Timeline did not cancel Replay.");
            Assert.That(originalModal.IsOpen, Is.True);
            Assert.That(originalModal.ActiveTab, Is.EqualTo(VNRecordsTab.Gallery));
            Assert.That(tabSession.Lifecycle, Is.EqualTo(VNReplaySessionLifecycle.Disposed));

            Assert.That(originalModal.TrySelectTab(VNRecordsTab.Timeline), Is.True);
            StartFromTimelineRow("entry_safe");
            var closedSession = ActiveSession();
            Assert.That(originalModal.Close(), Is.True);
            yield return WaitFor(() => !originalController.HasActiveSession && originalHost.childCount == 1,
                "Closing Records did not cancel Replay.");
            Assert.That(originalModal.IsOpen, Is.False);
            Assert.That(closedSession.Lifecycle, Is.EqualTo(VNReplaySessionLifecycle.Disposed));
            Assert.That(originalModal != null, Is.True, "Replay disposal must leave its RecordsModal alive.");
            Assert.That(originalTimelineRoot != null, Is.True, "Replay disposal must leave Timeline UI alive.");
            Assert.That(metaWrites, Is.EqualTo(writesBeforeReplay));
            CollectionAssert.AreEqual(metaBytesBeforeReplay, File.ReadAllBytes(repository.CanonicalFilePath));

            TestContext.WriteLine("M9-09 runtime: Timeline EntryId -> fresh Completed/safe check -> Replay session under ReplayHost -> isolated LinePresenter/manual Next -> visual commands -> completion/cancel/modal/tab cleanup PASS.");

            DestroyRuntimeHarnessObjects();
            yield return null;
            yield return new ExitPlayMode();
            RestorePlayModeOptions();
        }

        private void CreateHarness()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M9ReplayIntegration_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => metaWrites++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();
            timelineCatalog = CreateTimelineCatalog(
                CreateEntry("entry_safe", "Safe completed", "line:integration_safe", "line:integration_safe_done", "M9_REPLAY_SAFE_START"),
                CreateEntry("entry_discovered", "Discovered", "line:integration_discovered", "line:integration_discovered_done", "M9_REPLAY_SAFE_START"),
                CreateEntry("entry_no_node", "No Replay node", "line:integration_no_node", "line:integration_no_node_done", null),
                CreateEntry("entry_unsafe", "Unsafe", "line:integration_unsafe", "line:integration_unsafe_done", "M9_REPLAY_UNSAFE_CHECKPOINT"),
                CreateEntry("entry_hidden", "Hidden", "line:integration_hidden", "line:integration_hidden_done", "M9_REPLAY_SAFE_START"));
            Assert.That(metaProgress.TryUnlockChapter("chapter_replay"), Is.True);
            foreach (var lineId in new[] { "line:integration_safe_done", "line:integration_no_node_done", "line:integration_unsafe_done" })
                Assert.That(metaProgress.TryRecordReadLine(lineId), Is.True);
            Assert.That(metaProgress.TryRecordReadLine("line:integration_discovered"), Is.True);
            timelineRuntime = new VNTimelineRuntime(timelineCatalog, new VNTimelineService(timelineCatalog, metaProgress), metaProgress);
            yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>(YarnProjectPath);
            Assert.That(yarnProject, Is.Not.Null);
            Assert.That(yarnProject.Program, Is.Not.Null);

            presentationCatalog = CreatePresentationCatalog();
            recordsRoot = NewObject("Records Modal");
            recordsRoot.SetActive(false);
            recordsRoot.AddComponent<Canvas>();
            recordsRoot.AddComponent<GraphicRaycaster>();
            recordsCanvasGroup = recordsRoot.AddComponent<CanvasGroup>();
            var closeButton = CreateButton(recordsRoot.transform, "Records Close");
            var tabs = new[]
            {
                CreateButton(recordsRoot.transform, "Timeline Tab"),
                CreateButton(recordsRoot.transform, "Gallery Tab"),
                CreateButton(recordsRoot.transform, "Archive Tab"),
                CreateButton(recordsRoot.transform, "Achievements Tab"),
            };
            var tabRoots = new[]
            {
                NewChildObject(recordsRoot.transform, "Timeline Root"),
                NewChildObject(recordsRoot.transform, "Gallery Root"),
                NewChildObject(recordsRoot.transform, "Archive Root"),
                NewChildObject(recordsRoot.transform, "Achievements Root"),
            };
            var indicators = new[]
            {
                NewChildObject(recordsRoot.transform, "Timeline Selected"),
                NewChildObject(recordsRoot.transform, "Gallery Selected"),
                NewChildObject(recordsRoot.transform, "Archive Selected"),
                NewChildObject(recordsRoot.transform, "Achievements Selected"),
            };
            recordsModal = recordsRoot.AddComponent<VNRecordsModal>();
            SetField(recordsModal, "modalCanvasGroup", recordsCanvasGroup);
            SetField(recordsModal, "closeButton", closeButton);
            SetField(recordsModal, "timelineTabButton", tabs[0]);
            SetField(recordsModal, "galleryTabButton", tabs[1]);
            SetField(recordsModal, "archiveTabButton", tabs[2]);
            SetField(recordsModal, "achievementsTabButton", tabs[3]);
            SetField(recordsModal, "timelineRoot", tabRoots[0]);
            SetField(recordsModal, "galleryRoot", tabRoots[1]);
            SetField(recordsModal, "archiveRoot", tabRoots[2]);
            SetField(recordsModal, "achievementsRoot", tabRoots[3]);
            SetField(recordsModal, "timelineSelectedIndicator", indicators[0]);
            SetField(recordsModal, "gallerySelectedIndicator", indicators[1]);
            SetField(recordsModal, "archiveSelectedIndicator", indicators[2]);
            SetField(recordsModal, "achievementsSelectedIndicator", indicators[3]);

            replaySessionHost = NewChildObject(recordsRoot.transform, "ReplayHost").transform;
            replayViewTemplate = CreateReplayViewTemplate(replaySessionHost);
            var controllerObject = NewChildObject(recordsRoot.transform, "ReplayController");
            replayController = controllerObject.AddComponent<VNReplayController>();
            SetField(replayController, "recordsModal", recordsModal);
            SetField(replayController, "replaySessionHost", replaySessionHost);
            SetField(replayController, "replayViewTemplate", replayViewTemplate);

            timelineRoot = tabRoots[0];
            timelineView = CreateTimelineView(timelineRoot.transform);
            recordsRoot.SetActive(true);
            Assert.That(recordsModal.TryOpen(), Is.True);
            Assert.That(replayController.Initialize(timelineRuntime, timelineCatalog, yarnProject), Is.True, replayController.LastDiagnostic);
            Assert.That(timelineView.Initialize(timelineRuntime), Is.True, timelineView.LastDiagnostic);
            Assert.That(timelineView.InitializeReplay(replayController), Is.True, timelineView.LastDiagnostic);
        }

        private void StartFromTimelineRow(string entryId)
        {
            Assert.That(recordsModal.IsOpen, Is.True);
            Assert.That(recordsModal.ActiveTab, Is.EqualTo(VNRecordsTab.Timeline));
            var row = FindTimelineRow(entryId);
            var replayButton = GetField<Button>(row, "replayButton");
            Assert.That(replayButton.gameObject.activeSelf && replayButton.interactable, Is.True, entryId);
            replayButton.onClick.Invoke();
        }

        private VNTimelineEntryItem FindTimelineRow(string entryId) =>
            GetField<List<VNTimelineEntryItem>>(timelineView, "entryItems")
                .Single(item => item != null && GetField<string>(item, "entryId") == entryId);

        private VNReplaySession ActiveSession() => GetField<VNReplaySession>(replayController, "activeSession");

        private VNReplayView ActiveView() => GetField<VNReplayView>(replayController, "activeView");

        private void AssertRuntimeSessionOwnership(
            VNReplaySession session,
            VNReplayView view,
            Transform host,
            VNRecordsModal modal,
            GameObject expectedTimelineRoot)
        {
            Assert.That(session, Is.Not.Null);
            Assert.That(session.Runner, Is.Not.Null);
            Assert.That(session.Runner.autoStart, Is.False);
            var presenters = session.Runner.DialoguePresenters.ToArray();
            Assert.That(presenters, Has.Length.EqualTo(1));
            Assert.That(presenters[0], Is.SameAs(view.LinePresenter));
            Assert.That(view.LinePresenter.autoAdvance, Is.False);
            Assert.That(view.IsBlockingInput, Is.True);
            Assert.That(GetField<CanvasGroup>(view, "overlayCanvasGroup").blocksRaycasts, Is.True);
            Assert.That(view.GetComponent<Image>().raycastTarget, Is.True);
            Assert.That(session.Runner.transform.parent, Is.SameAs(host));
            Assert.That(host.gameObject, Is.Not.Null);
            Assert.That(modal, Is.Not.Null);
            Assert.That(expectedTimelineRoot, Is.Not.Null);
            Assert.That(replaySessionHost.childCount, Is.EqualTo(2), "ReplayHost contains the inactive template and exactly one disposable session.");
            Assert.That(session.Presenter, Is.SameAs(view.LinePresenter));
            Assert.That(session.PresentationController, Is.SameAs(view.PresentationController));
            Assert.That(view.PresentationController.transform.IsChildOf(session.Runner.transform), Is.True);
            Assert.That(view.LinePresenter.transform.IsChildOf(session.Runner.transform), Is.True);
        }

        private IEnumerator AdvanceToLine(VNReplayView view, string expectedText)
        {
            view.NextButton.onClick.Invoke();
            for (var frame = 0; frame < 900; frame++)
            {
                if (view.LinePresenter.lineText.text.Contains(expectedText)) yield break;
                yield return null;
            }
            Assert.Fail("Replay did not present expected line text: " + expectedText);
        }

        private static IEnumerator WaitFor(Func<bool> condition, string message, int timeoutFrames = 900)
        {
            for (var frame = 0; frame < timeoutFrames; frame++)
            {
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail(message);
        }

        private void AssertMissingWiring(VNReplayView view, string fieldName, string diagnosticPart)
        {
            var original = GetField<UnityEngine.Object>(view, fieldName);
            SetField(view, fieldName, null);
            Assert.That(view.TryValidateWiring(out var diagnostic), Is.False);
            StringAssert.Contains(diagnosticPart, diagnostic);
            SetField(view, fieldName, original);
        }

        private VNTimelineCatalog CreateTimelineCatalog(params VNTimelineEntryDefinition[] entries)
        {
            var catalog = ScriptableObject.CreateInstance<VNTimelineCatalog>();
            catalog.name = "M9-09 Replay Integration Timeline Catalog";
            ownedAssets.Add(catalog);
            timelineChapter = new VNTimelineChapterDefinition("chapter_replay", "Replay chapter", 0, showWhenLocked: true);
            SetField(catalog, "chapters", new List<VNTimelineChapterDefinition> { timelineChapter });
            SetField(catalog, "entries", new List<VNTimelineEntryDefinition>(entries ?? Array.Empty<VNTimelineEntryDefinition>()));
            InvokePrivateNoArguments(catalog, "OnValidate");
            return catalog;
        }

        private VNTimelineChapterDefinition timelineChapter;

        private static VNTimelineEntryDefinition CreateEntry(
            string entryId,
            string title,
            string discoveryLineId,
            string completionLineId,
            string replayNode)
        {
            return new VNTimelineEntryDefinition(
                entryId,
                "chapter_replay",
                title,
                0,
                discoveryLineId,
                completionLineId,
                new[] { discoveryLineId, completionLineId },
                parentEntryId: null,
                replayNode: replayNode,
                relatedCgId: null);
        }

        private void ReplaceTimelineEntries(VNTimelineCatalog catalog, params VNTimelineEntryDefinition[] entries)
        {
            SetField(catalog, "chapters", new List<VNTimelineChapterDefinition> { timelineChapter });
            SetField(catalog, "entries", new List<VNTimelineEntryDefinition>(entries));
            InvokePrivateNoArguments(catalog, "OnValidate");
        }

        private VNReplayView CreateReplayViewTemplate(Transform parent)
        {
            var root = NewObject("Replay View Template");
            root.transform.SetParent(parent, false);
            root.SetActive(false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var overlayGroup = root.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 0f;
            overlayGroup.interactable = false;
            overlayGroup.blocksRaycasts = false;
            var overlayGraphic = root.AddComponent<Image>();
            overlayGraphic.color = new Color(0f, 0f, 0f, 0f);
            overlayGraphic.raycastTarget = true;

            var view = root.AddComponent<VNReplayView>();
            var presentation = CreatePresentationController(root.transform, presentationCatalog);
            var linePanel = NewChildObject(root.transform, "Replay Line Panel");
            var linePanelGroup = linePanel.AddComponent<CanvasGroup>();
            var lineText = CreateText(linePanel.transform, "Replay Line Text");
            var linePresenter = linePanel.AddComponent<LinePresenter>();
            linePresenter.canvasGroup = linePanelGroup;
            linePresenter.lineText = lineText;
            linePresenter.autoAdvance = false;
            linePresenter.useFadeEffect = false;
            linePresenter.fadeUpDuration = 0f;
            linePresenter.fadeDownDuration = 0f;
            var typewriterStyle = typeof(LinePresenter).GetField("typewriterStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(typewriterStyle, Is.Not.Null);
            typewriterStyle.SetValue(linePresenter, Enum.ToObject(typewriterStyle.FieldType, 0));

            var nextButton = CreateButton(root.transform, "Replay Next Button");
            var closeButton = CreateButton(root.transform, "Replay Close Button");
            SetField(view, "overlayCanvasGroup", overlayGroup);
            SetField(view, "presentationController", presentation);
            SetField(view, "linePresenter", linePresenter);
            SetField(view, "nextButton", nextButton);
            SetField(view, "closeButton", closeButton);
            return view;
        }

        private VNPresentationController CreatePresentationController(Transform parent, VNPresentationCatalog catalog)
        {
            var root = NewChildObject(parent, "Replay Presentation");
            root.SetActive(false);
            var background = CreateImage(root.transform, "Background");
            var cg = CreateImage(root.transform, "CG");
            var slots = new List<VNCharacterSlotView>();
            foreach (VNCharacterSlot slot in Enum.GetValues(typeof(VNCharacterSlot)))
                slots.Add(CreateSlotView(root.transform, slot));
            var controller = root.AddComponent<VNPresentationController>();
            SetField(controller, "catalog", catalog);
            SetField(controller, "backgroundImage", background);
            SetField(controller, "cgImage", cg);
            SetField(controller, "characterSlotViews", slots);
            root.SetActive(true);
            return controller;
        }

        private VNPresentationCatalog CreatePresentationCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<VNPresentationCatalog>();
            catalog.name = "M9-09 Replay Presentation Catalog";
            ownedAssets.Add(catalog);
            var backgroundSprite = CreateSprite("Replay Background", new Color(0.25f, 0.2f, 0.35f, 1f));
            var cgSprite = CreateSprite("Replay CG", new Color(0.65f, 0.45f, 0.2f, 1f));
            var characterSprite = CreateSprite("Replay Character", new Color(0.3f, 0.6f, 0.7f, 1f));
            var character = CreateCharacterDefinition(characterSprite);
            SetField(catalog, "characterDefinitions", new List<VNCharacterDefinition> { character });
            SetField(catalog, "backgrounds", new List<VNSpriteCatalogEntry> { CreateSpriteEntry("replay_background", backgroundSprite) });
            SetField(catalog, "cgs", new List<VNSpriteCatalogEntry> { CreateSpriteEntry("m3_cg", cgSprite) });
            return catalog;
        }

        private VNCharacterDefinition CreateCharacterDefinition(Sprite sprite)
        {
            var definition = ScriptableObject.CreateInstance<VNCharacterDefinition>();
            definition.name = "M9-09 Replay Character";
            ownedAssets.Add(definition);
            var defaultExpression = new VNExpressionDefinition();
            SetField(defaultExpression, "expressionId", "default");
            SetField(defaultExpression, "headSprite", sprite);
            var replayExpression = new VNExpressionDefinition();
            SetField(replayExpression, "expressionId", "replay_expression");
            SetField(replayExpression, "headSprite", sprite);
            SetField(definition, "characterId", "m9_replay_character");
            SetField(definition, "speakerAliases", new List<string>());
            SetField(definition, "defaultFacing", VNCharacterFacing.Right);
            SetField(definition, "defaultScale", 1f);
            SetField(definition, "bodySprite", sprite);
            SetField(definition, "defaultExpressionId", "default");
            SetField(definition, "expressions", new List<VNExpressionDefinition> { defaultExpression, replayExpression });
            return definition;
        }

        private Sprite CreateSprite(string name, Color color)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = name + " Texture" };
            texture.SetPixels(new[] { color, color, color, color });
            texture.Apply();
            ownedAssets.Add(texture);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            sprite.name = name;
            ownedAssets.Add(sprite);
            return sprite;
        }

        private static VNSpriteCatalogEntry CreateSpriteEntry(string id, Sprite sprite)
        {
            var entry = new VNSpriteCatalogEntry();
            SetField(entry, "id", id);
            SetField(entry, "sprite", sprite);
            return entry;
        }

        private VNCharacterSlotView CreateSlotView(Transform parent, VNCharacterSlot slot)
        {
            var root = NewChildObject(parent, "Replay Slot " + slot);
            root.AddComponent<CanvasGroup>();
            var view = root.AddComponent<VNCharacterSlotView>();
            SetField(view, "slot", slot);
            SetField(view, "visualRoot", root.GetComponent<RectTransform>());
            SetField(view, "backHairImage", CreateImage(root.transform, "Back Hair"));
            SetField(view, "bodyImage", CreateImage(root.transform, "Body"));
            SetField(view, "headImage", CreateImage(root.transform, "Head"));
            SetField(view, "fadeCanvasGroup", root.GetComponent<CanvasGroup>());
            return view;
        }

        private VNTimelineView CreateTimelineView(Transform parent)
        {
            var view = parent.gameObject.AddComponent<VNTimelineView>();
            var chapterContent = NewChildObject(parent, "Chapter Content").transform;
            var entryScroll = NewChildObject(parent, "Entry Scroll").AddComponent<ScrollRect>();
            var entryContentObject = NewChildObject(parent, "Entry Content");
            var entryContent = entryContentObject.GetComponent<RectTransform>();
            entryScroll.content = entryContent;
            var chapterItemPrefab = CreateChapterItemPrefab();
            var entryItemPrefab = CreateEntryItemPrefab();
            var selectedTitle = CreateText(parent, "Selected Chapter Title");
            var selectedState = CreateText(parent, "Selected Chapter State");
            var globalEmpty = NewChildObject(parent, "Global Empty");
            var locked = NewChildObject(parent, "Locked State");
            var empty = NewChildObject(parent, "Chapter Empty");
            var content = NewChildObject(parent, "Chapter Content State");
            SetField(view, "chapterContent", chapterContent);
            SetField(view, "chapterItemPrefab", chapterItemPrefab);
            SetField(view, "entryScrollRect", entryScroll);
            SetField(view, "entryContent", entryContent);
            SetField(view, "entryItemPrefab", entryItemPrefab);
            SetField(view, "selectedChapterTitleText", selectedTitle);
            SetField(view, "selectedChapterStateText", selectedState);
            SetField(view, "globalEmptyStateRoot", globalEmpty);
            SetField(view, "chapterLockedStateRoot", locked);
            SetField(view, "chapterEmptyStateRoot", empty);
            SetField(view, "chapterContentStateRoot", content);
            return view;
        }

        private VNTimelineChapterItem CreateChapterItemPrefab()
        {
            var root = NewObject("Timeline Chapter Item Template");
            root.SetActive(false);
            var button = root.AddComponent<Button>();
            var title = CreateText(root.transform, "Chapter Title");
            var state = CreateText(root.transform, "Chapter State");
            var selected = NewChildObject(root.transform, "Selected Indicator");
            var locked = NewChildObject(root.transform, "Locked Indicator");
            var completed = NewChildObject(root.transform, "Completed Indicator");
            var item = root.AddComponent<VNTimelineChapterItem>();
            SetField(item, "button", button);
            SetField(item, "titleText", title);
            SetField(item, "stateText", state);
            SetField(item, "selectedIndicator", selected);
            SetField(item, "lockedIndicator", locked);
            SetField(item, "completedIndicator", completed);
            return item;
        }

        private VNTimelineEntryItem CreateEntryItemPrefab()
        {
            var root = NewObject("Timeline Entry Item Template");
            root.SetActive(false);
            root.AddComponent<Button>(); // The row's root remains non-interactive.
            var title = CreateText(root.transform, "Entry Title");
            var state = CreateText(root.transform, "Entry State");
            var indentation = NewChildObject(root.transform, "Indentation").AddComponent<LayoutElement>();
            var completed = NewChildObject(root.transform, "Completed Indicator");
            var replayButton = CreateButton(root.transform, "Replay Button");
            replayButton.gameObject.SetActive(false);
            var item = root.AddComponent<VNTimelineEntryItem>();
            SetField(item, "titleText", title);
            SetField(item, "stateText", state);
            SetField(item, "indentationSpacer", indentation);
            SetField(item, "completedIndicator", completed);
            SetField(item, "replayButton", replayButton);
            SetField(item, "indentationPerLevel", 24f);
            return item;
        }

        private static Image CreateImage(Transform parent, string name)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private TMP_Text CreateText(Transform parent, string name)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            return textObject.GetComponent<TextMeshProUGUI>();
        }

        private Button CreateButton(Transform parent, string name)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 48f);
            var image = buttonObject.GetComponent<Image>();
            image.raycastTarget = true;
            return buttonObject.GetComponent<Button>();
        }

        private GameObject NewObject(string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            ownedObjects.Add(gameObject);
            return gameObject;
        }

        private GameObject NewChildObject(Transform parent, string name)
        {
            var gameObject = NewObject(name);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = FindField(target.GetType(), fieldName);
            Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().Name}.{fieldName}");
            field.SetValue(target, value);
        }

        private static T GetField<T>(object target, string fieldName)
        {
            var field = FindField(target.GetType(), fieldName);
            Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().Name}.{fieldName}");
            return (T)field.GetValue(target);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                type = type.BaseType;
            }
            return null;
        }

        private static void InvokePrivateNoArguments(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing method {target.GetType().Name}.{methodName}");
            method.Invoke(target, null);
        }

        private void EnablePlayModeWithoutDomainReload()
        {
            if (playModeOptionsCaptured) return;
            previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            playModeOptionsCaptured = true;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions | EnterPlayModeOptions.DisableDomainReload;
        }

        private void RestorePlayModeOptions()
        {
            if (!playModeOptionsCaptured) return;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = previousEnterPlayModeOptionsEnabled;
            playModeOptionsCaptured = false;
        }

        private void DestroyRuntimeHarnessObjects()
        {
            if (recordsRoot != null) UnityEngine.Object.Destroy(recordsRoot);
            foreach (var asset in ownedAssets)
                if (asset != null) UnityEngine.Object.Destroy(asset);
        }
    }
}
