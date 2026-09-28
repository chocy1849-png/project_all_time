using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records.Timeline;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNTimelineRuntimeTests
    {
        private const string M8ReadLine = "line:m8_meta_read_01";
        private const string M8FirstPassLine = "line:m8_meta_first_pass_01";
        private const string M8CompletionLine = "line:m8_meta_complete_01";

        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private int writes;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M9TimelineRuntimeTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();
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
        public void Constructor_RejectsNullDependencies()
        {
            var catalog = CreateCatalog();
            var projections = new VNTimelineService(catalog, metaProgress);
            Assert.Throws<ArgumentNullException>(() => new VNTimelineRuntime(null, projections, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNTimelineRuntime(catalog, null, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNTimelineRuntime(catalog, projections, null));
        }

        [Test]
        public void ChapterStates_UseM8CompletionMarkersAndKeepZeroEntryChaptersAvailable()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_locked_hidden", "SECRET LOCKED", 0),
                Chapter("chapter_locked_known", "Known chapter", 1, showWhenLocked: true),
                Chapter("chapter_empty", "Empty", 2),
                Chapter("chapter_all_hidden", "Not discovered yet", 3),
                Chapter("m8_smoke_chapter_01", "M8 smoke", 4),
                Entry("hidden_entry", "chapter_all_hidden", "Hidden", "line:hidden_discovery", "line:hidden_completion", 0),
                Entry("m8_entry_a", "m8_smoke_chapter_01", "First pass", M8ReadLine, M8FirstPassLine, 0),
                Entry("m8_entry_b", "m8_smoke_chapter_01", "Complete", M8FirstPassLine, M8CompletionLine, 1));
            var runtime = Runtime(catalog);

            SeedChapter("chapter_empty");
            SeedChapter("chapter_all_hidden");
            SeedChapter("m8_smoke_chapter_01");

            var initial = runtime.BuildSnapshot();
            Assert.That(initial.TryGetChapter("chapter_locked_hidden", out _), Is.False);
            Assert.That(initial.TryGetChapter("chapter_locked_known", out var knownLocked), Is.True);
            Assert.That(knownLocked.State, Is.EqualTo(VNTimelineChapterState.Locked));
            Assert.That(knownLocked.Roots, Is.Empty);
            Assert.That(initial.GetRoots("chapter_locked_known"), Is.Empty);
            Assert.That(initial.TryGetEntry("hidden_entry", out _), Is.False);
            Assert.That(initial.TryGetChapter("chapter_empty", out var empty), Is.True);
            Assert.That(empty.State, Is.EqualTo(VNTimelineChapterState.Available));
            Assert.That(initial.TryGetChapter("chapter_all_hidden", out var undiscovered), Is.True);
            Assert.That(undiscovered.State, Is.EqualTo(VNTimelineChapterState.Available));
            Assert.That(initial.TryGetChapter("m8_smoke_chapter_01", out var smoke), Is.True);
            Assert.That(smoke.State, Is.EqualTo(VNTimelineChapterState.Available));

            SeedLine(M8ReadLine);
            var discovered = runtime.BuildSnapshot();
            Assert.That(discovered.TryGetChapter("m8_smoke_chapter_01", out smoke), Is.True);
            Assert.That(smoke.State, Is.EqualTo(VNTimelineChapterState.InProgress));
            Assert.That(discovered.TryGetEntry("m8_entry_a", out var entryA), Is.True);
            Assert.That(entryA.State, Is.EqualTo(VNTimelineEntryState.Discovered));

            SeedLine(M8FirstPassLine);
            var firstPass = runtime.BuildSnapshot();
            Assert.That(firstPass.TryGetChapter("m8_smoke_chapter_01", out smoke), Is.True);
            Assert.That(smoke.State, Is.EqualTo(VNTimelineChapterState.InProgress));
            Assert.That(firstPass.TryGetEntry("m8_entry_a", out entryA), Is.True);
            Assert.That(entryA.State, Is.EqualTo(VNTimelineEntryState.Completed));
            Assert.That(firstPass.TryGetEntry("m8_entry_b", out var entryB), Is.True);
            Assert.That(entryB.State, Is.EqualTo(VNTimelineEntryState.Discovered));

            SeedLine(M8CompletionLine);
            var completed = runtime.BuildSnapshot();
            Assert.That(completed.TryGetChapter("m8_smoke_chapter_01", out smoke), Is.True);
            Assert.That(smoke.State, Is.EqualTo(VNTimelineChapterState.Completed));
            Assert.That(completed.TryGetEntry("m8_entry_b", out entryB), Is.True);
            Assert.That(entryB.State, Is.EqualTo(VNTimelineEntryState.Completed));
        }

        [Test]
        public void ChapterCompletion_ConsidersHiddenAuthoredEntries_AndCompletionReadCanRevealEntry()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_hidden_completion", "Two events"),
                Entry("entry_a", "chapter_hidden_completion", "Visible completed event", "line:a_discovery", "line:a_complete", 0,
                    relatedCgId: "cg_story_image"),
                Entry("secret_entry_b", "chapter_hidden_completion", "SECRET FUTURE TITLE", "line:b_discovery", "line:b_complete", 1));
            SeedChapter("chapter_hidden_completion");
            SeedLine("line:a_discovery");
            SeedLine("line:a_complete");
            var runtime = Runtime(catalog);

            var partial = runtime.BuildSnapshot();
            Assert.That(partial.TryGetChapter("chapter_hidden_completion", out var chapter), Is.True);
            Assert.That(chapter.State, Is.EqualTo(VNTimelineChapterState.InProgress));
            Assert.That(partial.TryGetEntry("entry_a", out _), Is.True);
            Assert.That(partial.TryGetEntry("secret_entry_b", out _), Is.False);
            Assert.That(chapter.Roots.Select(entry => entry.EntryId), Is.EqualTo(new[] { "entry_a" }));
            Assert.That(chapter.Roots.Any(entry => entry.DisplayTitle == "SECRET FUTURE TITLE"), Is.False);
            Assert.That(chapter.Roots.Single().RelatedCgId, Is.EqualTo("cg_story_image"),
                "Timeline preserves the authored relationship ID without resolving Gallery state.");

            SeedLine("line:b_complete");
            var complete = runtime.BuildSnapshot();
            Assert.That(complete.TryGetChapter("chapter_hidden_completion", out chapter), Is.True);
            Assert.That(chapter.State, Is.EqualTo(VNTimelineChapterState.Completed));
            Assert.That(complete.TryGetEntry("secret_entry_b", out var entryB), Is.True);
            Assert.That(entryB.State, Is.EqualTo(VNTimelineEntryState.Completed));
        }

        [Test]
        public void VisibleTopology_ReparentsChildrenWhenParentBecomesVisibleInFreshSnapshot()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_tree", "Tree"),
                Entry("root_a", "chapter_tree", "Root A", "line:root_a", "line:root_a_done", 0),
                Entry("child_b", "chapter_tree", "Child B", "line:child_b", "line:child_b_done", 0, parent: "root_a"),
                Entry("child_c", "chapter_tree", "Child C", "line:child_c", "line:child_c_done", 1, parent: "root_a"),
                Entry("grandchild_d", "chapter_tree", "Grandchild D", "line:grandchild_d", "line:grandchild_d_done", 0, parent: "child_b"));
            SeedChapter("chapter_tree");
            SeedLine("line:child_c");
            var runtime = Runtime(catalog);

            var onlyC = runtime.BuildSnapshot();
            CollectionAssert.AreEqual(new[] { "child_c" }, onlyC.GetRoots("chapter_tree").Select(entry => entry.EntryId));
            Assert.That(onlyC.TryGetEntry("child_c", out var c), Is.True);
            Assert.That(c.ParentEntryId, Is.Null);
            Assert.That(onlyC.TryGetEntry("root_a", out _), Is.False);

            SeedLine("line:child_b");
            var bAndC = runtime.BuildSnapshot();
            CollectionAssert.AreEqual(new[] { "child_b", "child_c" }, bAndC.GetRoots("chapter_tree").Select(entry => entry.EntryId));
            Assert.That(bAndC.TryGetEntry("child_b", out var b), Is.True);
            Assert.That(b.ParentEntryId, Is.Null, "A visible entry with a hidden parent becomes a root.");

            SeedLine("line:root_a");
            var parentAppears = runtime.BuildSnapshot();
            CollectionAssert.AreEqual(new[] { "root_a" }, parentAppears.GetRoots("chapter_tree").Select(entry => entry.EntryId));
            Assert.That(parentAppears.TryGetEntry("root_a", out var root), Is.True);
            CollectionAssert.AreEqual(new[] { "child_b", "child_c" }, root.Children.Select(entry => entry.EntryId));
            Assert.That(parentAppears.TryGetEntry("child_b", out b), Is.True);
            Assert.That(b.ParentEntryId, Is.EqualTo("root_a"));
            Assert.That(parentAppears.TryGetEntry("grandchild_d", out _), Is.False);

            SeedLine("line:grandchild_d");
            var allVisible = runtime.BuildSnapshot();
            Assert.That(allVisible.TryGetEntry("child_b", out b), Is.True);
            CollectionAssert.AreEqual(new[] { "grandchild_d" }, b.Children.Select(entry => entry.EntryId));
            Assert.That(onlyC.GetRoots("chapter_tree").Single().ParentEntryId, Is.Null,
                "Earlier snapshots remain unchanged when later state reveals an authored parent.");
        }

        [Test]
        public void Ordering_IsSortOrderThenOrdinalIdRegardlessOfAuthoredListOrder()
        {
            var catalog = CreateCatalog(
                Chapter("z_chapter", "Z", 1),
                Chapter("a_chapter", "A", 1),
                Chapter("b_chapter", "B", 0),
                Entry("root_z", "a_chapter", "Z root", "line:root_z", "line:root_z_done", 1),
                Entry("root_b", "a_chapter", "B root", "line:root_b", "line:root_b_done", 1),
                Entry("root_low", "a_chapter", "Low root", "line:root_low", "line:root_low_done", 0),
                Entry("parent", "a_chapter", "Parent", "line:parent", "line:parent_done", 2),
                Entry("child_z", "a_chapter", "Z child", "line:child_z", "line:child_z_done", 1, parent: "parent"),
                Entry("child_a", "a_chapter", "A child", "line:child_a", "line:child_a_done", 1, parent: "parent"),
                Entry("child_b", "a_chapter", "B child", "line:child_b", "line:child_b_done", 1, parent: "parent"));
            SeedChapter("a_chapter");
            SeedChapter("b_chapter");
            SeedChapter("z_chapter");
            foreach (var line in new[]
                     {
                         "line:root_z", "line:root_b", "line:root_low", "line:parent",
                         "line:child_z", "line:child_a", "line:child_b",
                     })
                SeedLine(line);
            var runtime = Runtime(catalog);

            var first = runtime.BuildSnapshot();
            var second = runtime.BuildSnapshot();
            CollectionAssert.AreEqual(new[] { "b_chapter", "a_chapter", "z_chapter" },
                first.Chapters.Select(chapter => chapter.ChapterId));
            CollectionAssert.AreEqual(first.Chapters.Select(chapter => chapter.ChapterId),
                second.Chapters.Select(chapter => chapter.ChapterId));
            CollectionAssert.AreEqual(new[] { "root_low", "root_b", "root_z", "parent" },
                first.GetRoots("a_chapter").Select(entry => entry.EntryId));
            Assert.That(first.TryGetEntry("parent", out var parent), Is.True);
            CollectionAssert.AreEqual(new[] { "child_a", "child_b", "child_z" }, parent.Children.Select(entry => entry.EntryId));
            CollectionAssert.AreEqual(first.GetRoots("a_chapter").Select(entry => entry.EntryId),
                second.GetRoots("a_chapter").Select(entry => entry.EntryId));
        }

        [Test]
        public void LockedChapters_DoNotInferUnlockFromInconsistentReadHistory()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_secret_locked", "SECRET CHAPTER", 0),
                Chapter("chapter_known_locked", "Known locked", 1, showWhenLocked: true),
                Entry("secret_entry", "chapter_secret_locked", "SECRET ENTRY", "line:secret_discovery", "line:secret_complete", 0),
                Entry("known_entry", "chapter_known_locked", "SECRET KNOWN ENTRY", "line:known_discovery", "line:known_complete", 0));
            SeedLine("line:secret_complete");
            SeedLine("line:known_complete");
            var runtime = Runtime(catalog);

            var snapshot = runtime.BuildSnapshot();
            Assert.That(snapshot.TryGetChapter("chapter_secret_locked", out _), Is.False);
            Assert.That(snapshot.TryGetEntry("secret_entry", out _), Is.False);
            Assert.That(snapshot.TryGetChapter("chapter_known_locked", out var locked), Is.True);
            Assert.That(locked.State, Is.EqualTo(VNTimelineChapterState.Locked));
            Assert.That(locked.DisplayTitle, Is.EqualTo("Known locked"));
            Assert.That(locked.Roots, Is.Empty);
            Assert.That(snapshot.TryGetEntry("known_entry", out _), Is.False);
            Assert.That(snapshot.GetChildren("secret_entry"), Is.Empty);
        }

        [Test]
        public void Snapshot_IsImmutableFreshReadOnlyAndIgnoresUnknownMetaProgressIds()
        {
            var catalog = CreateCatalog(
                Chapter("chapter_a", "Chapter A", 0),
                Chapter("chapter_b", "Chapter B", 1),
                Entry("entry_a", "chapter_a", "Entry A", "line:a", "line:a_done", 0),
                Entry("entry_b", "chapter_b", "Entry B", "line:b", "line:b_done", 0));
            SeedChapter("chapter_a");
            SeedLine("line:a");
            SeedLine("line:legacy_unknown");
            SeedChapter("chapter:legacy_unknown");
            var runtime = Runtime(catalog);

            var snapshotA = runtime.BuildSnapshot();
            Assert.That(snapshotA.TryGetChapter("chapter_a", out var chapterA), Is.True);
            Assert.That(snapshotA.TryGetEntry("entry_a", out var entryA), Is.True);
            Assert.That(snapshotA.TryGetChapter("chapter:legacy_unknown", out _), Is.False);
            Assert.That(snapshotA.TryGetEntry("legacy_unknown", out _), Is.False);
            Assert.That(snapshotA.TryGetChapter(null, out _), Is.False);
            Assert.That(snapshotA.TryGetEntry(null, out _), Is.False);
            Assert.That(snapshotA.TryGetChapter("missing", out _), Is.False);
            Assert.That(snapshotA.TryGetEntry("missing", out _), Is.False);
            Assert.That(snapshotA.GetRoots("missing"), Is.Empty);
            Assert.That(snapshotA.GetRoots(null), Is.Empty);
            Assert.That(snapshotA.GetChildren("missing"), Is.Empty);

            var writesBeforeQueries = writes;
            var bytesBeforeQueries = File.ReadAllBytes(repository.CanonicalFilePath);
            var stateBeforeQueries = metaProgress.Current;
            for (var index = 0; index < 3; index++)
            {
                var fresh = runtime.BuildSnapshot();
                Assert.That(fresh.TryGetChapter("chapter_a", out _), Is.True);
                Assert.That(fresh.TryGetEntry("entry_a", out _), Is.True);
                Assert.That(fresh.GetRoots("chapter_a").Single().EntryId, Is.EqualTo("entry_a"));
                Assert.That(fresh.GetChildren("entry_a"), Is.Empty);
            }
            Assert.That(writes, Is.EqualTo(writesBeforeQueries));
            CollectionAssert.AreEqual(bytesBeforeQueries, File.ReadAllBytes(repository.CanonicalFilePath));
            CollectionAssert.AreEqual(stateBeforeQueries.readLineIds, metaProgress.Current.readLineIds);
            CollectionAssert.AreEqual(stateBeforeQueries.unlockedCGs, metaProgress.Current.unlockedCGs);
            CollectionAssert.AreEqual(stateBeforeQueries.unlockedChapters, metaProgress.Current.unlockedChapters);
            CollectionAssert.AreEqual(stateBeforeQueries.unlockedArchiveEntries, metaProgress.Current.unlockedArchiveEntries);
            CollectionAssert.AreEqual(stateBeforeQueries.unlockedAchievements, metaProgress.Current.unlockedAchievements);
            CollectionAssert.AreEqual(stateBeforeQueries.completedEndings, metaProgress.Current.completedEndings);
            Assert.That(metaProgress.IsLineRead("line:legacy_unknown"), Is.True);
            Assert.That(metaProgress.IsChapterUnlocked("chapter:legacy_unknown"), Is.True);

            Assert.Throws<NotSupportedException>(() => ((IList<VNTimelineChapterRuntime>)snapshotA.Chapters).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<VNTimelineEntryRuntime>)chapterA.Roots).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<VNTimelineEntryRuntime>)entryA.Children).Add(null));

            SeedChapter("chapter_b");
            SeedLine("line:b");
            var snapshotB = runtime.BuildSnapshot();
            Assert.That(snapshotA.Chapters.Select(chapter => chapter.ChapterId), Is.EqualTo(new[] { "chapter_a" }));
            Assert.That(snapshotA.TryGetChapter("chapter_b", out _), Is.False);
            Assert.That(snapshotA.TryGetEntry("entry_b", out _), Is.False);
            Assert.That(snapshotB.TryGetChapter("chapter_b", out var chapterB), Is.True);
            Assert.That(chapterB.State, Is.EqualTo(VNTimelineChapterState.InProgress));
            Assert.That(snapshotB.TryGetEntry("entry_b", out var entryB), Is.True);
            Assert.That(entryB.State, Is.EqualTo(VNTimelineEntryState.Discovered));
            Assert.That(snapshotA.TryGetEntry("entry_a", out entryA), Is.True);
            Assert.That(entryA.State, Is.EqualTo(VNTimelineEntryState.Discovered));
            Assert.That(typeof(VNTimelineChapterRuntime).GetProperties().Select(property => property.Name),
                Does.Not.Contain("CompletedCount").And.Not.Contain("TotalCount").And.Not.Contain("ProgressPercent")
                    .And.Not.Contain("MilestonesRead").And.Not.Contain("MilestonesTotal"));
            Assert.That(typeof(VNTimelineEntryRuntime).GetProperties().Select(property => property.Name),
                Does.Not.Contain("ReplayNode").And.Not.Contain("CanReplay").And.Not.Contain("ReplayEnabled")
                    .And.Not.Contain("ProgressPercent").And.Not.Contain("MilestonesRead").And.Not.Contain("MilestonesTotal"));
        }

        private VNTimelineRuntime Runtime(VNTimelineCatalog catalog)
        {
            return new VNTimelineRuntime(catalog, new VNTimelineService(catalog, metaProgress), metaProgress);
        }

        private VNTimelineCatalog CreateCatalog(params object[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNTimelineCatalog>());
            SetPrivateField(catalog, "chapters", definitions.OfType<VNTimelineChapterDefinition>().ToList());
            SetPrivateField(catalog, "entries", definitions.OfType<VNTimelineEntryDefinition>().ToList());
            catalog.GetType().GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(catalog, null);
            return catalog;
        }

        private void SeedLine(string id) => Assert.That(metaProgress.TryRecordReadLine(id), Is.True, "Could not seed line " + id);
        private void SeedChapter(string id) => Assert.That(metaProgress.TryUnlockChapter(id), Is.True, "Could not seed chapter " + id);

        private static VNTimelineChapterDefinition Chapter(string id, string title, int sortOrder = 0, bool showWhenLocked = false)
        {
            return new VNTimelineChapterDefinition(id, title, sortOrder, showWhenLocked);
        }

        private static VNTimelineEntryDefinition Entry(
            string id,
            string chapterId,
            string title,
            string discoveryLine,
            string completionLine,
            int sortOrder,
            string parent = null,
            string relatedCgId = null)
        {
            return new VNTimelineEntryDefinition(
                id, chapterId, title, sortOrder, discoveryLine, completionLine,
                new[] { discoveryLine, completionLine }, parent, "reserved_replay_node", relatedCgId);
        }

        private T Own<T>(T value) where T : UnityEngine.Object
        {
            ownedObjects.Add(value);
            return value;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "Missing private field " + fieldName);
            field.SetValue(instance, value);
        }
    }
}
