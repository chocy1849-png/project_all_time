using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Replay.UI;
using ProjectAllTime.VN.Records.Timeline;
using ProjectAllTime.VN.Records.Timeline.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNTimelineViewTests
    {
        private const string M8ReadLine = "line:m8_meta_read_01";
        private const string M8FirstPassLine = "line:m8_meta_first_pass_01";
        private const string M8CompletionLine = "line:m8_meta_complete_01";

        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private int writes;
        private VNTimelineView view;
        private Transform chapterContent;
        private RectTransform entryContent;
        private ScrollRect entryScrollRect;
        private TMP_Text selectedChapterTitle;
        private TMP_Text selectedChapterState;
        private GameObject globalEmptyRoot;
        private GameObject lockedRoot;
        private GameObject emptyRoot;
        private GameObject contentRoot;
        private VNTimelineChapterItem chapterItemPrefab;
        private VNTimelineEntryItem entryItemPrefab;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M9TimelineViewTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();
            CreateViewHarness();
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                if (ownedObjects[index] != null) UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void Initialization_RejectsNullIsIdempotentAndRefreshesWhenAlreadyActive()
        {
            Assert.That(view.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            Assert.That(view.Initialize(null), Is.False);
            Assert.That(view.LastDiagnostic, Does.Contain(nameof(VNTimelineRuntime)));

            var runtime = Runtime(CreateCatalog());
            Assert.That(view.Initialize(runtime), Is.True);
            Assert.That(globalEmptyRoot.activeSelf, Is.True, "Initialize on an active view performs an immediate refresh.");
            Assert.That(view.Initialize(runtime), Is.True, "Initializing again with the same runtime is safe.");
            Assert.That(view.LastDiagnostic, Is.Null);

            var otherRuntime = Runtime(CreateCatalog());
            Assert.That(view.Initialize(otherRuntime), Is.False);
            Assert.That(view.LastDiagnostic, Does.Contain("different runtime"));
        }

        [Test]
        public void EmptySnapshot_ShowsGlobalEmptyStateAndClearsAllContent()
        {
            var catalog = CreateCatalog();
            Assert.That(view.Initialize(Runtime(catalog)), Is.True);
            Assert.That(view.Refresh(), Is.True);
            Assert.That(globalEmptyRoot.activeSelf, Is.True);
            AssertStateRoots(globalEmpty: true, locked: false, empty: false, content: false);
            Assert.That(selectedChapterTitle.text, Is.Empty);
            Assert.That(view.SelectedChapterId, Is.Null);
            Assert.That(ActiveChapterItems(), Is.Zero);
            Assert.That(ActiveEntryItems(), Is.Zero);
            Assert.That(view.LastDiagnostic, Is.Null);
        }

        [Test]
        public void InvalidWiring_ReturnsDiagnosticAndClearsRatherThanThrowing()
        {
            var runtime = Runtime(CreateCatalog(Chapter("chapter_a", "A", 0, showWhenLocked: true)));
            var invalidObject = NewObject("Invalid Timeline View");
            var invalidView = invalidObject.AddComponent<VNTimelineView>();
            Assert.That(invalidView.TryValidateWiring(out var diagnostic), Is.False);
            Assert.That(diagnostic, Does.Contain("Chapter content"));
            Assert.That(invalidView.Initialize(runtime), Is.True, "Runtime injection succeeds independently of view wiring.");
            Assert.That(invalidView.LastDiagnostic, Does.Contain("Chapter content"));
            Assert.That(invalidView.Refresh(), Is.False);
            Assert.That(invalidView.LastDiagnostic, Is.Not.Empty);
        }

        [Test]
        public void InitialSelection_SkipsEarlierLockedChapterAndPreservesValidSelection()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_locked", "Known locked", 0, showWhenLocked: true),
                Chapter("chapter_b", "Chapter B", 1, showWhenLocked: true),
                Chapter("chapter_c", "Chapter C", 2, showWhenLocked: true));
            SeedChapter("chapter_b");
            SeedChapter("chapter_c");
            var runtime = Runtime(catalog);

            Assert.That(view.Initialize(runtime), Is.True);
            Assert.That(view.SelectedChapterId, Is.EqualTo("chapter_b"));
            Assert.That(selectedChapterTitle.text, Is.EqualTo("Chapter B"));
            Assert.That(view.TrySelectChapter("chapter_c"), Is.True);
            Assert.That(view.Refresh(), Is.True);
            Assert.That(view.SelectedChapterId, Is.EqualTo("chapter_c"));
            Assert.That(selectedChapterTitle.text, Is.EqualTo("Chapter C"));

            ReplaceCatalogChapters(catalog,
                Chapter("chapter_locked", "Known locked", 0, showWhenLocked: true),
                Chapter("chapter_b", "Chapter B", 1, showWhenLocked: true));
            Assert.That(view.Refresh(), Is.True);
            Assert.That(view.SelectedChapterId, Is.EqualTo("chapter_b"), "A removed selection uses the normal unlocked fallback.");
        }

        [Test]
        public void InitialSelection_UsesFirstLockedWhenEveryVisibleChapterIsLocked()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_first", "First known locked", 0, showWhenLocked: true),
                Chapter("chapter_second", "Second known locked", 1, showWhenLocked: true));
            Assert.That(view.Initialize(Runtime(catalog)), Is.True);
            Assert.That(view.SelectedChapterId, Is.EqualTo("chapter_first"));
            Assert.That(selectedChapterTitle.text, Is.EqualTo("First known locked"));
            AssertStateRoots(globalEmpty: false, locked: true, empty: false, content: false);
        }

        [Test]
        public void ChapterStates_RenderLockedAvailableInProgressAndCompletedWithoutCounts()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_locked", "Known locked", 0, showWhenLocked: true),
                Chapter("chapter_available", "Available", 1),
                Chapter("chapter_progress", "In progress", 2),
                Chapter("chapter_completed", "Completed", 3),
                Entry("entry_available", "chapter_available", "Not yet discovered", "line:available_discovery", "line:available_completion", 0),
                Entry("entry_progress", "chapter_progress", "Discovered event", "line:progress_discovery", "line:progress_completion", 0),
                Entry("entry_completed", "chapter_completed", "Completed event", "line:completed_discovery", "line:completed_completion", 0));
            SeedChapter("chapter_available");
            SeedChapter("chapter_progress");
            SeedChapter("chapter_completed");
            SeedLine("line:progress_discovery");
            SeedLine("line:completed_completion");
            var runtime = Runtime(catalog);

            Assert.That(view.Initialize(runtime), Is.True);
            Assert.That(view.TrySelectChapter("chapter_locked"), Is.True);
            AssertStateRoots(globalEmpty: false, locked: true, empty: false, content: false);
            Assert.That(ActiveEntryItems(), Is.Zero);
            Assert.That(selectedChapterState.text, Is.EqualTo("Locked"));

            Assert.That(view.TrySelectChapter("chapter_available"), Is.True);
            AssertStateRoots(globalEmpty: false, locked: false, empty: true, content: false);
            Assert.That(ActiveEntryItems(), Is.Zero);
            Assert.That(selectedChapterState.text, Is.EqualTo("Available"));

            Assert.That(view.TrySelectChapter("chapter_progress"), Is.True);
            AssertStateRoots(globalEmpty: false, locked: false, empty: false, content: true);
            Assert.That(ActiveEntryItems(), Is.EqualTo(1));
            Assert.That(selectedChapterState.text, Is.EqualTo("In progress"));

            Assert.That(view.TrySelectChapter("chapter_completed"), Is.True);
            AssertStateRoots(globalEmpty: false, locked: false, empty: false, content: true);
            Assert.That(ActiveEntryItems(), Is.EqualTo(1));
            Assert.That(selectedChapterState.text, Is.EqualTo("Completed"));
            Assert.That(AllRenderedText(), Does.Not.Contain("%"));
            Assert.That(AllRenderedText(), Does.Not.Contain("/"));
        }

        [Test]
        public void EntryTree_FlattensDepthFirstInRuntimeChildOrderAndIndentsRows()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_tree", "Tree", 0),
                Entry("a", "chapter_tree", "A", "line:a", "line:a_done", 0),
                Entry("b", "chapter_tree", "B", "line:b", "line:b_done", 0, parent: "a"),
                Entry("d", "chapter_tree", "D", "line:d", "line:d_done", 0, parent: "b"),
                Entry("c", "chapter_tree", "C", "line:c", "line:c_done", 1, parent: "a"));
            SeedChapter("chapter_tree");
            foreach (var id in new[] { "a", "b", "d", "c" }) SeedLine("line:" + id);

            Assert.That(view.Initialize(Runtime(catalog)), Is.True);
            Assert.That(ActiveEntryItems(), Is.EqualTo(4));
            var rows = EntryRows();
            CollectionAssert.AreEqual(new[] { "A", "B", "D", "C" }, rows.Select(row => EntryTitle(row)).ToArray());
            CollectionAssert.AreEqual(new[] { "Discovered", "Discovered", "Discovered", "Discovered" },
                rows.Select(row => EntryState(row)).ToArray());
            CollectionAssert.AreEqual(new[] { 0f, 24f, 48f, 24f }, rows.Select(row => IndentationWidth(row)).ToArray());
        }

        [Test]
        public void Pools_GrowToMaximumReuseInstancesAndDeactivateSurplus()
        {
            var chapters = Enumerable.Range(0, 5)
                .Select(index => Chapter("chapter_" + index, "Chapter " + index, index, showWhenLocked: true)).ToArray();
            var entries = Enumerable.Range(0, 7)
                .Select(index => Entry("entry_" + index, "chapter_0", "Entry " + index,
                    "line:entry_" + index, "line:entry_done_" + index, index)).ToArray();
            var catalog = CreateCatalog(chapters.Cast<object>().Concat(entries).ToArray());
            SeedChapter("chapter_0");
            foreach (var index in Enumerable.Range(0, 7)) SeedLine("line:entry_" + index);
            var runtime = Runtime(catalog);

            Assert.That(view.Initialize(runtime), Is.True);
            AssertPoolCounts(chapters: 5, activeChapters: 5, entries: 7, activeEntries: 7);
            var originalChapterInstances = ChapterRows().Select(item => item.GetInstanceID()).ToArray();
            var originalEntryInstances = EntryRows().Select(item => item.GetInstanceID()).ToArray();

            ReplaceCatalog(catalog,
                new[] { chapters[0], chapters[1] },
                new[] { entries[0] });
            Assert.That(view.Refresh(), Is.True);
            AssertPoolCounts(chapters: 5, activeChapters: 2, entries: 7, activeEntries: 1);

            ReplaceCatalog(catalog,
                new[] { chapters[0], chapters[1], chapters[2], chapters[3] },
                entries.Take(5).ToArray());
            Assert.That(view.Refresh(), Is.True);
            AssertPoolCounts(chapters: 5, activeChapters: 4, entries: 7, activeEntries: 5);
            CollectionAssert.AreEqual(originalChapterInstances, ChapterRows().Select(item => item.GetInstanceID()).ToArray());
            CollectionAssert.AreEqual(originalEntryInstances, EntryRows().Select(item => item.GetInstanceID()).ToArray());
        }

        [Test]
        public void ChapterItem_RebindingAndDisableEnableNeverAccumulatesClickListeners()
        {
            var item = chapterItemPrefab;
            var selectedCount = 0;
            for (var index = 0; index < 10; index++)
                item.Bind("chapter_a", "Chapter A", VNTimelineChapterState.Available, false, _ => selectedCount++);
            GetField<Button>(item, "button").onClick.Invoke();
            Assert.That(selectedCount, Is.EqualTo(1));

            InvokePrivateNoArguments(item, "OnDisable");
            InvokePrivateNoArguments(item, "OnEnable");
            GetField<Button>(item, "button").onClick.Invoke();
            Assert.That(selectedCount, Is.EqualTo(2));

            item.Unbind();
            GetField<Button>(item, "button").onClick.Invoke();
            Assert.That(selectedCount, Is.EqualTo(2));
        }

        [Test]
        public void SelectingDifferentChapterAndRefreshingResetScrollToTop()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_a", "A", 0),
                Chapter("chapter_b", "B", 1),
                Entry("entry_a", "chapter_a", "A event", "line:a", "line:a_done", 0),
                Entry("entry_b", "chapter_b", "B event", "line:b", "line:b_done", 0));
            SeedChapter("chapter_a");
            SeedChapter("chapter_b");
            SeedLine("line:a");
            SeedLine("line:b");
            Assert.That(view.Initialize(Runtime(catalog)), Is.True);

            Canvas.ForceUpdateCanvases();
            entryScrollRect.verticalNormalizedPosition = 0.2f;
            Assert.That(view.TrySelectChapter("chapter_b"), Is.True);
            Assert.That(entryScrollRect.verticalNormalizedPosition, Is.EqualTo(1f).Within(0.001f));

            entryScrollRect.verticalNormalizedPosition = 0.2f;
            Assert.That(view.TrySelectChapter("chapter_b"), Is.True);
            Assert.That(entryScrollRect.verticalNormalizedPosition, Is.EqualTo(0.2f).Within(0.02f),
                "Selecting the already active chapter does not reset its scroll position.");

            Assert.That(view.Refresh(), Is.True);
            Assert.That(entryScrollRect.verticalNormalizedPosition, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void RootReactivationBuildsFreshSnapshotAndPreservesSelection()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_a", "A", 0),
                Chapter("chapter_b", "B", 1),
                Entry("entry_a", "chapter_a", "A event", "line:a", "line:a_done", 0));
            SeedChapter("chapter_a");
            SeedChapter("chapter_b");
            var runtime = Runtime(catalog);
            Assert.That(view.Initialize(runtime), Is.True);
            Assert.That(selectedChapterState.text, Is.EqualTo("Available"));
            Assert.That(view.TrySelectChapter("chapter_b"), Is.True);

            view.gameObject.SetActive(false);
            SeedLine("line:a");
            view.gameObject.SetActive(true);
            InvokePrivateNoArguments(view, "OnEnable");
            Assert.That(selectedChapterState.text, Is.EqualTo("Available"));
            Assert.That(view.SelectedChapterId, Is.EqualTo("chapter_b"));

            Assert.That(view.TrySelectChapter("chapter_a"), Is.True);
            Assert.That(selectedChapterState.text, Is.EqualTo("In progress"),
                "The reactivation refresh used a newly built snapshot rather than stale state.");
        }

        [Test]
        public void HiddenParentAndLockedChapterNeverExposeEntryTopologyOrRelatedCgIds()
        {
            const string rawRelatedCgId = "cg_internal_story_asset_007";
            var catalog = CreateCatalog(
                Chapter("chapter_locked", "Known locked", 0, showWhenLocked: true),
                Chapter("chapter_visible", "Visible chapter", 1),
                Entry("secret_locked_entry", "chapter_locked", "SECRET LOCKED TITLE", "line:secret_locked", "line:secret_locked_done", 0),
                Entry("secret_parent", "chapter_visible", "SECRET HIDDEN PARENT", "line:secret_parent", "line:secret_parent_done", 0),
                Entry("visible_child", "chapter_visible", "Visible child", "line:visible_child", "line:visible_child_done", 1,
                    parent: "secret_parent", relatedCgId: rawRelatedCgId));
            SeedChapter("chapter_visible");
            SeedLine("line:visible_child");
            Assert.That(view.Initialize(Runtime(catalog)), Is.True);

            Assert.That(view.SelectedChapterId, Is.EqualTo("chapter_visible"), "The unlocked chapter is preferred over an earlier Locked row.");
            CollectionAssert.AreEqual(new[] { "Visible child" }, EntryRows().Select(EntryTitle).ToArray());
            Assert.That(IndentationWidth(EntryRows()[0]), Is.Zero, "A hidden parent is not reconstructed in the UI hierarchy.");
            Assert.That(AllRenderedText(), Does.Not.Contain("SECRET"));
            Assert.That(AllRenderedText(), Does.Not.Contain(rawRelatedCgId));
            Assert.That(AllRenderedText(), Does.Not.Contain("%"));

            Assert.That(view.TrySelectChapter("chapter_locked"), Is.True);
            AssertStateRoots(globalEmpty: false, locked: true, empty: false, content: false);
            Assert.That(ActiveEntryItems(), Is.Zero);
            Assert.That(AllRenderedText(), Does.Not.Contain("SECRET LOCKED TITLE"));
        }

        [Test]
        public void ActualM8StableIdsFlowThroughProjectionRuntimeAndUi()
        {
            var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            Assert.That(yarnProject, Is.Not.Null);
            var actualIds = new HashSet<string>(yarnProject.GetLineIDsForNodes(new[] { "M8_META_PROGRESS_START" }), StringComparer.Ordinal);
            CollectionAssert.IsSubsetOf(new[] { M8ReadLine, M8FirstPassLine, M8CompletionLine }, actualIds);

            var catalog = CreateCatalog(
                Chapter("chapter_m8", "M8 smoke chapter", 0),
                Entry("entry_first_pass", "chapter_m8", "First pass", M8ReadLine, M8FirstPassLine, 0,
                    milestones: new[] { M8ReadLine, M8FirstPassLine }),
                Entry("entry_complete", "chapter_m8", "Completion", M8FirstPassLine, M8CompletionLine, 1,
                    parent: "entry_first_pass", milestones: new[] { M8FirstPassLine, M8CompletionLine }));
            SeedChapter("chapter_m8");
            var runtime = Runtime(catalog);
            Assert.That(view.Initialize(runtime), Is.True);
            Assert.That(selectedChapterState.text, Is.EqualTo("Available"));
            Assert.That(ActiveEntryItems(), Is.Zero);

            SeedLine(M8ReadLine);
            Assert.That(view.Refresh(), Is.True);
            Assert.That(selectedChapterState.text, Is.EqualTo("In progress"));
            CollectionAssert.AreEqual(new[] { "First pass" }, EntryRows().Select(EntryTitle).ToArray());
            CollectionAssert.AreEqual(new[] { "Discovered" }, EntryRows().Select(EntryState).ToArray());

            SeedLine(M8FirstPassLine);
            Assert.That(view.Refresh(), Is.True);
            CollectionAssert.AreEqual(new[] { "First pass", "Completion" }, EntryRows().Select(EntryTitle).ToArray());
            CollectionAssert.AreEqual(new[] { "Completed", "Discovered" }, EntryRows().Select(EntryState).ToArray());

            SeedLine(M8CompletionLine);
            Assert.That(view.Refresh(), Is.True);
            Assert.That(selectedChapterState.text, Is.EqualTo("Completed"));
            CollectionAssert.AreEqual(new[] { "Completed", "Completed" }, EntryRows().Select(EntryState).ToArray());
        }

        [Test]
        public void InitializeRefreshSelectionScrollAndRootLifecycleProduceZeroWrites()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_a", "A", 0),
                Chapter("chapter_b", "B", 1),
                Entry("entry_a", "chapter_a", "A event", "line:a", "line:a_done", 0));
            SeedChapter("chapter_a");
            SeedChapter("chapter_b");
            SeedLine("line:a");
            var runtime = Runtime(catalog);
            var writesBeforeView = writes;
            var bytesBeforeView = File.ReadAllBytes(repository.CanonicalFilePath);
            var stateBeforeView = metaProgress.Current;

            Assert.That(view.Initialize(runtime), Is.True);
            Assert.That(view.Initialize(runtime), Is.True);
            Assert.That(view.TrySelectChapter("chapter_b"), Is.True);
            Assert.That(view.TrySelectChapter("chapter_a"), Is.True);
            entryScrollRect.verticalNormalizedPosition = 0.25f;
            Assert.That(view.Refresh(), Is.True);

            view.gameObject.SetActive(false);
            view.gameObject.SetActive(true);
            InvokePrivateNoArguments(view, "OnEnable");
            Assert.That(view.TrySelectChapter("chapter_b"), Is.True);
            Assert.That(writes, Is.EqualTo(writesBeforeView));
            CollectionAssert.AreEqual(bytesBeforeView, File.ReadAllBytes(repository.CanonicalFilePath));
            CollectionAssert.AreEqual(stateBeforeView.readLineIds, metaProgress.Current.readLineIds);
            CollectionAssert.AreEqual(stateBeforeView.unlockedChapters, metaProgress.Current.unlockedChapters);
        }

        [Test]
        public void EntryItem_WithHiddenOrInvalidStateClearsMetadataAndNeverBecomesInteractive()
        {
            var item = entryItemPrefab;
            var title = GetField<TMP_Text>(item, "titleText");
            var state = GetField<TMP_Text>(item, "stateText");
            var childButton = item.GetComponent<Button>();
            var replayButton = GetField<Button>(item, "replayButton");
            var invokedEntryId = string.Empty;
            Assert.That(replayButton.gameObject.activeSelf, Is.False,
                "A pooled Timeline row starts with Replay hidden until an eligible Bind.");
            Assert.That(item.Bind("entry_visible", "Visible", VNTimelineEntryState.Completed, 3, true,
                entryId => invokedEntryId = entryId), Is.True);
            Assert.That(title.text, Is.EqualTo("Visible"));
            Assert.That(state.text, Is.EqualTo("Completed"));
            Assert.That(childButton.interactable, Is.False);
            Assert.That(replayButton.gameObject.activeSelf, Is.True);
            Assert.That(replayButton.interactable, Is.True);
            replayButton.onClick.Invoke();
            Assert.That(invokedEntryId, Is.EqualTo("entry_visible"));

            Assert.That(item.Bind("SECRET", VNTimelineEntryState.Hidden, 0), Is.False);
            Assert.That(title.text, Is.Empty);
            Assert.That(state.text, Is.Empty);
            Assert.That(childButton.interactable, Is.False);
            Assert.That(replayButton.gameObject.activeSelf, Is.False);
            Assert.That(replayButton.interactable, Is.False);
        }

        [Test]
        public void ReplayButton_RequiresCompletedVisibleSafeEntryAndRebindingKeepsOneListener()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_replay", "Replay", 0),
                Entry("entry_safe", "chapter_replay", "Safe completed", "line:safe_discovery", "line:safe_complete", 0,
                    replayNode: "M9_REPLAY_SAFE_START"),
                Entry("entry_discovered", "chapter_replay", "Discovered", "line:discovered", "line:discovered_complete", 1,
                    replayNode: "M9_REPLAY_SAFE_START"),
                Entry("entry_no_node", "chapter_replay", "No node", "line:no_node_discovery", "line:no_node_complete", 2),
                Entry("entry_unsafe", "chapter_replay", "Unsafe", "line:unsafe_discovery", "line:unsafe_complete", 3,
                    replayNode: "M9_REPLAY_UNSAFE_CHECKPOINT"));
            SeedChapter("chapter_replay");
            foreach (var lineId in new[] { "line:safe_complete", "line:no_node_complete", "line:unsafe_complete", "line:discovered" })
                SeedLine(lineId);

            var runtime = Runtime(catalog);
            var replayControllerObject = NewObject("Replay Eligibility Controller");
            var replayController = replayControllerObject.AddComponent<VNReplayController>();
            var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            Assert.That(yarnProject, Is.Not.Null);
            Assert.That(replayController.Initialize(runtime, catalog, yarnProject), Is.True, replayController.LastDiagnostic);
            var writesBeforeBind = writes;
            var bytesBeforeBind = File.ReadAllBytes(repository.CanonicalFilePath);

            Assert.That(view.Initialize(runtime), Is.True);
            Assert.That(view.InitializeReplay(replayController), Is.True, view.LastDiagnostic);
            var rows = EntryRows().ToDictionary(item => GetField<string>(item, "entryId"), StringComparer.Ordinal);
            var safeButton = GetField<Button>(rows["entry_safe"], "replayButton");
            var discoveredButton = GetField<Button>(rows["entry_discovered"], "replayButton");
            var noNodeButton = GetField<Button>(rows["entry_no_node"], "replayButton");
            var unsafeButton = GetField<Button>(rows["entry_unsafe"], "replayButton");

            Assert.That(safeButton.gameObject.activeSelf && safeButton.interactable, Is.True);
            Assert.That(discoveredButton.gameObject.activeSelf, Is.False);
            Assert.That(noNodeButton.gameObject.activeSelf, Is.False);
            Assert.That(unsafeButton.gameObject.activeSelf, Is.False);
            Assert.That(rows["entry_safe"].GetComponent<Button>().interactable, Is.False,
                "Only the dedicated Replay button is interactive; the row stays inert.");
            Assert.That(AllRenderedText(), Does.Not.Contain("M9_REPLAY_SAFE_START"));
            Assert.That(writes, Is.EqualTo(writesBeforeBind));
            CollectionAssert.AreEqual(bytesBeforeBind, File.ReadAllBytes(repository.CanonicalFilePath));

            var clicks = 0;
            var row = entryItemPrefab;
            for (var index = 0; index < 10; index++)
                Assert.That(row.Bind("entry_rebind", "Rebind", VNTimelineEntryState.Completed, 0, true, _ => clicks++), Is.True);
            GetField<Button>(row, "replayButton").onClick.Invoke();
            Assert.That(clicks, Is.EqualTo(1));
        }

        [Test]
        public void UiComponentsExposeNoReplayCatalogMetaProgressOrGalleryDependency()
        {
            var uiTypes = new[] { typeof(VNTimelineView), typeof(VNTimelineChapterItem), typeof(VNTimelineEntryItem) };
            foreach (var type in uiTypes)
            {
                var memberNames = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(member => member.Name);
                Assert.That(memberNames.Any(name => string.Equals(name, "ReplayNode", StringComparison.Ordinal)), Is.False);
                Assert.That(memberNames.Any(name => name.IndexOf("RelatedCgId", StringComparison.OrdinalIgnoreCase) >= 0), Is.False);
            }

            var viewFieldTypes = typeof(VNTimelineView)
                .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(field => field.FieldType).ToArray();
            Assert.That(viewFieldTypes.Any(type => type == typeof(VNTimelineCatalog)), Is.False);
            Assert.That(viewFieldTypes.Any(type => type == typeof(VNReplayController)), Is.True);
            Assert.That(viewFieldTypes.Any(type => type == typeof(VNMetaProgressService)), Is.False);
            Assert.That(viewFieldTypes.Any(type => type == typeof(VNCGGalleryService)), Is.False);
            Assert.That(typeof(VNTimelineChapterItem).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(fieldType => fieldType == typeof(VNTimelineRuntime)), Is.False);
            Assert.That(typeof(VNTimelineEntryItem).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(fieldType => fieldType == typeof(VNTimelineRuntime)), Is.False);
        }

        private void CreateViewHarness()
        {
            var root = NewObject("Timeline UI");
            root.SetActive(false);
            root.AddComponent<Canvas>();
            chapterContent = NewChildObject(root.transform, "Chapter Content").transform;
            chapterContent.gameObject.AddComponent<VerticalLayoutGroup>();

            var scrollObject = NewChildObject(root.transform, "Entry ScrollRect");
            entryScrollRect = scrollObject.AddComponent<ScrollRect>();
            var viewport = NewChildObject(scrollObject.transform, "Viewport").GetComponent<RectTransform>();
            viewport.sizeDelta = new Vector2(480f, 200f);
            entryContent = NewChildObject(viewport.transform, "Entry Content").GetComponent<RectTransform>();
            entryContent.pivot = new Vector2(0.5f, 1f);
            entryContent.anchorMin = new Vector2(0f, 1f);
            entryContent.anchorMax = new Vector2(1f, 1f);
            entryContent.sizeDelta = new Vector2(0f, 1200f);
            entryContent.gameObject.AddComponent<VerticalLayoutGroup>();
            entryScrollRect.viewport = viewport;
            entryScrollRect.content = entryContent;

            selectedChapterTitle = NewText(root.transform, "Selected Chapter Title");
            selectedChapterState = NewText(root.transform, "Selected Chapter State");
            globalEmptyRoot = NewChildObject(root.transform, "Global Empty State");
            lockedRoot = NewChildObject(root.transform, "Locked State");
            emptyRoot = NewChildObject(root.transform, "Chapter Empty State");
            contentRoot = NewChildObject(root.transform, "Chapter Content State");
            scrollObject.transform.SetParent(contentRoot.transform, false);

            chapterItemPrefab = CreateChapterPrefab();
            entryItemPrefab = CreateEntryPrefab();

            view = root.AddComponent<VNTimelineView>();
            SetField(view, "chapterContent", chapterContent);
            SetField(view, "chapterItemPrefab", chapterItemPrefab);
            SetField(view, "entryScrollRect", entryScrollRect);
            SetField(view, "entryContent", entryContent);
            SetField(view, "entryItemPrefab", entryItemPrefab);
            SetField(view, "selectedChapterTitleText", selectedChapterTitle);
            SetField(view, "selectedChapterStateText", selectedChapterState);
            SetField(view, "globalEmptyStateRoot", globalEmptyRoot);
            SetField(view, "chapterLockedStateRoot", lockedRoot);
            SetField(view, "chapterEmptyStateRoot", emptyRoot);
            SetField(view, "chapterContentStateRoot", contentRoot);
            root.SetActive(true);
        }

        private VNTimelineChapterItem CreateChapterPrefab()
        {
            var prefabObject = NewObject("Timeline Chapter Item Prefab");
            prefabObject.SetActive(false);
            var button = prefabObject.AddComponent<Button>();
            var title = NewChildText(prefabObject.transform, "Chapter Title");
            var state = NewChildText(prefabObject.transform, "Chapter State");
            var selected = NewChildObject(prefabObject.transform, "Selected Indicator");
            var locked = NewChildObject(prefabObject.transform, "Locked Indicator");
            var completed = NewChildObject(prefabObject.transform, "Completed Indicator");
            var item = prefabObject.AddComponent<VNTimelineChapterItem>();
            SetField(item, "button", button);
            SetField(item, "titleText", title);
            SetField(item, "stateText", state);
            SetField(item, "selectedIndicator", selected);
            SetField(item, "lockedIndicator", locked);
            SetField(item, "completedIndicator", completed);
            return item;
        }

        private VNTimelineEntryItem CreateEntryPrefab()
        {
            var prefabObject = NewObject("Timeline Entry Item Prefab");
            prefabObject.SetActive(false);
            prefabObject.AddComponent<Button>();
            var title = NewChildText(prefabObject.transform, "Entry Title");
            var state = NewChildText(prefabObject.transform, "Entry State");
            var indentation = NewChildObject(prefabObject.transform, "Indentation Spacer").AddComponent<LayoutElement>();
            var completed = NewChildObject(prefabObject.transform, "Entry Completed Indicator");
            var replayButtonObject = NewChildObject(prefabObject.transform, "Replay Button");
            replayButtonObject.SetActive(false);
            var replayButton = replayButtonObject.AddComponent<Button>();
            var item = prefabObject.AddComponent<VNTimelineEntryItem>();
            SetField(item, "titleText", title);
            SetField(item, "stateText", state);
            SetField(item, "indentationSpacer", indentation);
            SetField(item, "completedIndicator", completed);
            SetField(item, "replayButton", replayButton);
            SetField(item, "indentationPerLevel", 24f);
            return item;
        }

        private VNTimelineRuntime Runtime(VNTimelineCatalog catalog)
        {
            var service = new VNTimelineService(catalog, metaProgress);
            return new VNTimelineRuntime(catalog, service, metaProgress);
        }

        private VNTimelineCatalog CreateCatalog(params object[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNTimelineCatalog>());
            ReplaceCatalog(catalog,
                definitions.OfType<VNTimelineChapterDefinition>().ToArray(),
                definitions.OfType<VNTimelineEntryDefinition>().ToArray());
            return catalog;
        }

        private static void ReplaceCatalogChapters(VNTimelineCatalog catalog, params VNTimelineChapterDefinition[] chapters)
        {
            SetField(catalog, "chapters", new List<VNTimelineChapterDefinition>(chapters));
            InvokePrivateNoArguments(catalog, "OnValidate");
        }

        private static void ReplaceCatalog(VNTimelineCatalog catalog,
            IReadOnlyList<VNTimelineChapterDefinition> chapters,
            IReadOnlyList<VNTimelineEntryDefinition> entries)
        {
            SetField(catalog, "chapters", new List<VNTimelineChapterDefinition>(chapters));
            SetField(catalog, "entries", new List<VNTimelineEntryDefinition>(entries));
            InvokePrivateNoArguments(catalog, "OnValidate");
        }

        private void SeedChapter(string chapterId) => Assert.That(metaProgress.TryUnlockChapter(chapterId), Is.True, chapterId);
        private void SeedLine(string lineId) => Assert.That(metaProgress.TryRecordReadLine(lineId), Is.True, lineId);

        private static VNTimelineChapterDefinition Chapter(string id, string title, int sortOrder, bool showWhenLocked = false)
        {
            return new VNTimelineChapterDefinition(id, title, sortOrder, showWhenLocked);
        }

        private static VNTimelineEntryDefinition Entry(string id, string chapterId, string title,
            string discoveryLine, string completionLine, int sortOrder, string parent = null,
            string relatedCgId = null, IEnumerable<string> milestones = null, string replayNode = null)
        {
            return new VNTimelineEntryDefinition(id, chapterId, title, sortOrder,
                discoveryLine, completionLine, milestones ?? new[] { discoveryLine, completionLine },
                parent, replayNode, relatedCgId);
        }

        private void AssertStateRoots(bool globalEmpty, bool locked, bool empty, bool content)
        {
            Assert.That(globalEmptyRoot.activeSelf, Is.EqualTo(globalEmpty));
            Assert.That(lockedRoot.activeSelf, Is.EqualTo(locked));
            Assert.That(emptyRoot.activeSelf, Is.EqualTo(empty));
            Assert.That(contentRoot.activeSelf, Is.EqualTo(content));
            Assert.That(new[] { globalEmptyRoot, lockedRoot, emptyRoot, contentRoot }.Count(root => root.activeSelf), Is.EqualTo(1));
        }

        private int ActiveChapterItems() => ChapterRows().Count(item => item.gameObject.activeSelf);
        private int ActiveEntryItems() => EntryRows().Count(item => item.gameObject.activeSelf);

        private VNTimelineChapterItem[] ChapterRows() =>
            GetField<List<VNTimelineChapterItem>>(view, "chapterItems").ToArray();

        private VNTimelineEntryItem[] EntryRows() =>
            GetField<List<VNTimelineEntryItem>>(view, "entryItems").ToArray();

        private void AssertPoolCounts(int chapters, int activeChapters, int entries, int activeEntries)
        {
            Assert.That(ChapterRows(), Has.Length.EqualTo(chapters));
            Assert.That(ActiveChapterItems(), Is.EqualTo(activeChapters));
            Assert.That(chapterContent.childCount, Is.EqualTo(chapters));
            Assert.That(EntryRows(), Has.Length.EqualTo(entries));
            Assert.That(ActiveEntryItems(), Is.EqualTo(activeEntries));
            Assert.That(entryContent.childCount, Is.EqualTo(entries));
        }

        private static string EntryTitle(VNTimelineEntryItem item) => GetField<TMP_Text>(item, "titleText").text;
        private static string EntryState(VNTimelineEntryItem item) => GetField<TMP_Text>(item, "stateText").text;
        private static float IndentationWidth(VNTimelineEntryItem item) => GetField<LayoutElement>(item, "indentationSpacer").preferredWidth;

        private string AllRenderedText()
        {
            var rendered = new List<string> { selectedChapterTitle.text, selectedChapterState.text };
            foreach (var item in ChapterRows())
            {
                rendered.Add(GetField<TMP_Text>(item, "titleText").text);
                rendered.Add(GetField<TMP_Text>(item, "stateText").text);
            }
            foreach (var item in EntryRows())
            {
                rendered.Add(EntryTitle(item));
                rendered.Add(EntryState(item));
            }
            return string.Join("\n", rendered);
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

        private TMP_Text NewChildText(Transform parent, string name)
        {
            var gameObject = NewChildObject(parent, name);
            gameObject.AddComponent<CanvasRenderer>();
            var text = gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            return text;
        }

        private TMP_Text NewText(Transform parent, string name) => NewChildText(parent, name);

        private T Own<T>(T value) where T : UnityEngine.Object
        {
            ownedObjects.Add(value);
            return value;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static T GetField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            return (T)field.GetValue(target);
        }

        private static void InvokePrivateNoArguments(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }
    }
}
