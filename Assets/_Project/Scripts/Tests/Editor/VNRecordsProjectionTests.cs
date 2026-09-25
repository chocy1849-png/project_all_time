using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Timeline;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNRecordsProjectionTests
    {
        private const string PresentationCatalogPath = "Assets/_Project/Settings/Presentation/M3_PresentationCatalog.asset";
        private const string DiscoveryLine = "line:m8_meta_read_01";
        private const string FirstPassLine = "line:m8_meta_first_pass_01";
        private const string CompletionLine = "line:m8_meta_complete_01";
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private VNPresentationCatalog currentPresentationCatalog;
        private Sprite currentM3Sprite;
        private int writes;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M9ProjectionTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();

            currentPresentationCatalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(PresentationCatalogPath);
            Assert.That(currentPresentationCatalog, Is.Not.Null);
            Assert.That(currentPresentationCatalog.TryGetCG("m3_cg", out currentM3Sprite), Is.True);
            Assert.That(currentM3Sprite, Is.Not.Null);
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
        public void ServiceConstructors_RejectNullDependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new VNTimelineService(null, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNTimelineService(CreateTimelineCatalog(), null));
            Assert.Throws<ArgumentNullException>(() => new VNCGGalleryService(null, currentPresentationCatalog, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNCGGalleryService(CreateGalleryCatalog(), null, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNCGGalleryService(CreateGalleryCatalog(), currentPresentationCatalog, null));
            Assert.Throws<ArgumentNullException>(() => new VNArchiveService(null, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNArchiveService(CreateArchiveCatalog(), null));
            Assert.Throws<ArgumentNullException>(() => new VNAchievementService(null, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNAchievementService(CreateAchievementCatalog(), null));
        }

        [Test]
        public void Timeline_HidesLockedChaptersAndProjectsOnlyDurablyDiscoveredEntries()
        {
            SeedChapter("chapter_unlocked");
            SeedReadLine("line:discovered");
            SeedReadLine("line:completed_without_discovery");
            SeedReadLine("line:middle_milestone");
            SeedReadLine("line:visible_parent");
            SeedReadLine("line:visible_child");

            var catalog = CreateTimelineCatalog(
                new VNTimelineChapterDefinition("chapter_locked_visible", "Known locked chapter", 0, true),
                new VNTimelineChapterDefinition("chapter_unlocked", "Unlocked chapter", 1),
                new VNTimelineChapterDefinition("chapter_locked_secret", "SECRET CHAPTER TITLE", 2, false),
                TimelineEntry("entry_hidden_parent", "chapter_unlocked", "HIDDEN PARENT TITLE", "line:hidden_parent", "line:hidden_parent_done", 0, relatedCgId: "cg_hidden"),
                TimelineEntry("entry_discovered", "chapter_unlocked", "Discovered entry", "line:discovered", "line:discovered_done", 1,
                    parentEntryId: "entry_hidden_parent", relatedCgId: "cg_discovered"),
                TimelineEntry("entry_completed", "chapter_unlocked", "Completed entry", "line:completed_discovery_not_read", "line:completed_without_discovery", 2),
                TimelineEntry("entry_milestone_only", "chapter_unlocked", "MILESTONE ONLY TITLE", "line:milestone_discovery", "line:milestone_completion", 3,
                    milestones: new[] { "line:milestone_discovery", "line:middle_milestone", "line:milestone_completion" }),
                TimelineEntry("entry_visible_parent", "chapter_unlocked", "Visible parent", "line:visible_parent", "line:visible_parent_done", 4),
                TimelineEntry("entry_visible_child", "chapter_unlocked", "Visible child", "line:visible_child", "line:visible_child_done", 5,
                    parentEntryId: "entry_visible_parent"),
                TimelineEntry("entry_secret_chapter", "chapter_locked_secret", "SECRET ENTRY TITLE", "line:secret_discovery", "line:secret_completion", 0,
                    relatedCgId: "cg_secret"));

            var service = new VNTimelineService(catalog, metaProgress);
            AssertReadOnly(() =>
            {
                var chapters = service.GetVisibleChapters();
                Assert.That(chapters.Select(chapter => chapter.ChapterId),
                    Is.EqualTo(new[] { "chapter_locked_visible", "chapter_unlocked" }));
                Assert.That(chapters[0].IsUnlocked, Is.False);
                Assert.That(chapters[0].DisplayTitle, Is.EqualTo("Known locked chapter"));
                Assert.That(chapters[0].Entries, Is.Empty);

                var unlocked = chapters[1];
                Assert.That(unlocked.IsUnlocked, Is.True);
                var ids = unlocked.Entries.Select(entry => entry.EntryId).ToArray();
                CollectionAssert.AreEqual(
                    new[] { "entry_discovered", "entry_completed", "entry_visible_parent", "entry_visible_child" }, ids);
                Assert.That(ids, Does.Not.Contain("entry_hidden_parent"));
                Assert.That(ids, Does.Not.Contain("entry_milestone_only"));
                Assert.That(ids, Does.Not.Contain("entry_secret_chapter"));

                var discovered = unlocked.Entries.Single(entry => entry.EntryId == "entry_discovered");
                Assert.That(discovered.State, Is.EqualTo(VNTimelineEntryState.Discovered));
                Assert.That(discovered.DisplayTitle, Is.EqualTo("Discovered entry"));
                Assert.That(discovered.RelatedCgId, Is.EqualTo("cg_discovered"));
                Assert.That(discovered.ParentEntryId, Is.Null, "A hidden parent's ID must not leak through its visible child.");

                var completed = unlocked.Entries.Single(entry => entry.EntryId == "entry_completed");
                Assert.That(completed.State, Is.EqualTo(VNTimelineEntryState.Completed),
                    "Completion is sufficient when its separate discovery line has not been read.");

                var visibleChild = unlocked.Entries.Single(entry => entry.EntryId == "entry_visible_child");
                Assert.That(visibleChild.ParentEntryId, Is.EqualTo("entry_visible_parent"));
                Assert.That(typeof(VNTimelineEntryProjection).GetProperty("MilestoneCount"), Is.Null);
                Assert.That(typeof(VNTimelineEntryProjection).GetProperty("ProgressPercentage"), Is.Null);
                Assert.That(typeof(VNTimelineEntryProjection).GetProperty("CanReplay"), Is.Null);
            });
        }

        [Test]
        public void Timeline_UsesExactM8LineIdsForHiddenDiscoveredCompletedTransitions()
        {
            var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            Assert.That(yarnProject, Is.Not.Null);
            var actualLineIds = new HashSet<string>(yarnProject.GetLineIDsForNodes(new[] { "M8_META_PROGRESS_START" }), StringComparer.Ordinal);
            CollectionAssert.IsSubsetOf(new[] { DiscoveryLine, FirstPassLine, CompletionLine }, actualLineIds);
            SeedChapter("chapter_m8");
            var catalog = CreateTimelineCatalog(
                new VNTimelineChapterDefinition("chapter_m8", "M8 test chapter", 0),
                TimelineEntry("entry_m8", "chapter_m8", "M8 test entry", DiscoveryLine, CompletionLine, 0,
                    milestones: new[] { DiscoveryLine, FirstPassLine, CompletionLine }));
            var service = new VNTimelineService(catalog, metaProgress);

            AssertReadOnly(() =>
                Assert.That(service.GetVisibleChapters().Single().Entries, Is.Empty));

            SeedReadLine(FirstPassLine);
            AssertReadOnly(() =>
                Assert.That(service.GetVisibleChapters().Single().Entries, Is.Empty,
                    "A middle milestone alone keeps the entry hidden."));

            SeedReadLine(DiscoveryLine);
            AssertReadOnly(() =>
                Assert.That(service.GetVisibleChapters().Single().Entries.Single().State,
                    Is.EqualTo(VNTimelineEntryState.Discovered)));

            SeedReadLine(CompletionLine);
            AssertReadOnly(() =>
                Assert.That(service.GetVisibleChapters().Single().Entries.Single().State,
                    Is.EqualTo(VNTimelineEntryState.Completed)));
        }

        [Test]
        public void Timeline_OrdersChaptersAndEntriesBySortThenOrdinalId()
        {
            SeedChapter("chapter_z");
            SeedChapter("chapter_a");
            SeedChapter("chapter_first");
            SeedReadLine("line:entry_z");
            SeedReadLine("line:entry_a");

            var catalog = CreateTimelineCatalog(
                new VNTimelineChapterDefinition("chapter_z", "Z", 1),
                new VNTimelineChapterDefinition("chapter_first", "First", 0),
                new VNTimelineChapterDefinition("chapter_a", "A", 1),
                TimelineEntry("entry_z", "chapter_a", "Z", "line:entry_z", "line:entry_z_done", 0),
                TimelineEntry("entry_a", "chapter_a", "A", "line:entry_a", "line:entry_a_done", 0));

            var service = new VNTimelineService(catalog, metaProgress);
            AssertReadOnly(() =>
            {
                var chapters = service.GetVisibleChapters();
                CollectionAssert.AreEqual(new[] { "chapter_first", "chapter_a", "chapter_z" },
                    chapters.Select(chapter => chapter.ChapterId));
                CollectionAssert.AreEqual(new[] { "entry_a", "entry_z" },
                    chapters.Single(chapter => chapter.ChapterId == "chapter_a").Entries.Select(entry => entry.EntryId));
                Assert.That(((IList<VNTimelineChapterProjection>)chapters).IsReadOnly, Is.True);
                Assert.That(((IList<VNTimelineEntryProjection>)chapters[1].Entries).IsReadOnly, Is.True);
            });
        }

        [Test]
        public void Timeline_CompletionWinsAndMilestonesAloneRemainHidden()
        {
            SeedChapter("chapter_progress");
            SeedReadLine("line:both_discovery");
            SeedReadLine("line:both_completion");
            SeedReadLine("line:just_milestone");

            var catalog = CreateTimelineCatalog(
                new VNTimelineChapterDefinition("chapter_progress", "Progress", 0),
                TimelineEntry("entry_both", "chapter_progress", "Both", "line:both_discovery", "line:both_completion", 0),
                TimelineEntry("entry_milestone", "chapter_progress", "Milestone", "line:other_discovery", "line:other_completion", 1,
                    milestones: new[] { "line:other_discovery", "line:just_milestone", "line:other_completion" }));

            var service = new VNTimelineService(catalog, metaProgress);
            AssertReadOnly(() =>
            {
                var entries = service.GetVisibleChapters().Single().Entries;
                Assert.That(entries, Has.Count.EqualTo(1));
                Assert.That(entries[0].State, Is.EqualTo(VNTimelineEntryState.Completed));
            });
        }

        [Test]
        public void Gallery_LockMetadataProtectsTitleAndArtworkAndFreshQueriesSeeUnlocks()
        {
            var catalog = CreateGalleryCatalog(
                new VNCGGalleryEntryDefinition("cg_z_locked", "Secret Z", 1, false),
                new VNCGGalleryEntryDefinition("cg_a_locked_title", "Known title", 0, true),
                new VNCGGalleryEntryDefinition("cg_unlocked", "Open CG", 0, false));
            var service = new VNCGGalleryService(
                catalog,
                CreatePresentationCatalog("cg_z_locked", "cg_a_locked_title", "cg_unlocked"),
                metaProgress);

            var beforeUnlockWriteCount = writes;
            AssertReadOnly(() =>
            {
                var entries = GetGalleryEntries(service);
                CollectionAssert.AreEqual(new[] { "cg_a_locked_title", "cg_unlocked", "cg_z_locked" },
                    entries.Select(entry => entry.CgId));

                var hiddenTitle = entries.Single(entry => entry.CgId == "cg_z_locked");
                Assert.That(hiddenTitle.IsUnlocked, Is.False);
                Assert.That(hiddenTitle.DisplayTitle, Is.Null);
                Assert.That(hiddenTitle.Sprite, Is.Null);

                var safeTitle = entries.Single(entry => entry.CgId == "cg_a_locked_title");
                Assert.That(safeTitle.IsUnlocked, Is.False);
                Assert.That(safeTitle.DisplayTitle, Is.EqualTo("Known title"));
                Assert.That(safeTitle.Sprite, Is.Null);

                var stillLocked = entries.Single(entry => entry.CgId == "cg_unlocked");
                Assert.That(stillLocked.IsUnlocked, Is.False);
                Assert.That(stillLocked.Sprite, Is.Null);
            });

            SeedCG("cg_unlocked");
            Assert.That(writes, Is.EqualTo(beforeUnlockWriteCount + 1));
            AssertReadOnly(() =>
            {
                var unlocked = GetGalleryEntries(service).Single(entry => entry.CgId == "cg_unlocked");
                Assert.That(unlocked.IsUnlocked, Is.True);
                Assert.That(unlocked.DisplayTitle, Is.EqualTo("Open CG"));
                Assert.That(unlocked.Sprite, Is.SameAs(currentM3Sprite));
            });
        }

        [Test]
        public void Gallery_UsesActualM3SpriteAndIgnoresStaleUnlockIds()
        {
            SeedCG("m3_cg");
            SeedCG("unknown_old_cg");
            var service = new VNCGGalleryService(
                CreateGalleryCatalog(new VNCGGalleryEntryDefinition("m3_cg", "M3 CG", 3)),
                currentPresentationCatalog,
                metaProgress);

            AssertReadOnly(() =>
            {
                var entries = GetGalleryEntries(service);
                Assert.That(entries, Has.Count.EqualTo(1));
                Assert.That(entries[0].CgId, Is.EqualTo("m3_cg"));
                Assert.That(entries[0].Sprite, Is.SameAs(currentM3Sprite));
                Assert.That(entries.Any(entry => entry.CgId == "unknown_old_cg"), Is.False);
            });
        }

        [Test]
        public void Gallery_UnlockedMissingSpriteReturnsDeterministicDiagnostic()
        {
            SeedCG("cg_missing_sprite");
            var service = new VNCGGalleryService(
                CreateGalleryCatalog(new VNCGGalleryEntryDefinition("cg_missing_sprite", "Missing sprite", 0)),
                CreatePresentationCatalog(),
                metaProgress);

            AssertReadOnly(() =>
            {
                Assert.That(service.TryGetGalleryEntries(out var entries, out var diagnostic), Is.False);
                Assert.That(entries, Is.Empty);
                Assert.That(diagnostic, Does.Contain("Unlocked Gallery CG 'cg_missing_sprite'"));
                Assert.That(diagnostic, Does.Contain("VNPresentationCatalog"));
            });
        }

        [Test]
        public void Archive_HidesLockedFieldsCountsOnlyByOptInAndSortsDeterministically()
        {
            SeedArchive("archive_a_unlocked");
            var catalog = CreateArchiveCatalog(
                new VNArchiveCategoryDefinition("category_z", "Z category", 1, false),
                new VNArchiveCategoryDefinition("category_first", "First category", 0, false),
                new VNArchiveCategoryDefinition("category_a", "A category", 1, true),
                new VNArchiveEntryDefinition("archive_z_locked", "category_a", "SECRET Z", "Secret summary", "Secret body", 0, false, currentM3Sprite),
                new VNArchiveEntryDefinition("archive_a_unlocked", "category_a", "Open title", "Open summary", "Open body", 0, false, currentM3Sprite),
                new VNArchiveEntryDefinition("archive_title_visible", "category_a", "Known locked title", "Hidden summary", "Hidden body", 2, true, currentM3Sprite),
                new VNArchiveEntryDefinition("archive_z_category", "category_z", "Z item", "Z summary", "Z body", 0));
            var service = new VNArchiveService(catalog, metaProgress);

            AssertReadOnly(() =>
            {
                var categories = service.GetCategories();
                CollectionAssert.AreEqual(new[] { "category_first", "category_a", "category_z" },
                    categories.Select(category => category.CategoryId));

                var hiddenCounts = categories.Single(category => category.CategoryId == "category_z");
                Assert.That(hiddenCounts.HasCompletionCount, Is.False);
                Assert.That(hiddenCounts.TotalCount, Is.Null);
                Assert.That(hiddenCounts.UnlockedCount, Is.Null);

                var count = categories.Single(category => category.CategoryId == "category_a");
                Assert.That(count.HasCompletionCount, Is.True);
                Assert.That(count.TotalCount, Is.EqualTo(3));
                Assert.That(count.UnlockedCount, Is.EqualTo(1));

                var entries = service.GetEntries("category_a");
                CollectionAssert.AreEqual(new[] { "archive_a_unlocked", "archive_z_locked", "archive_title_visible" },
                    entries.Select(entry => entry.ArchiveId));

                var unlocked = entries[0];
                Assert.That(unlocked.IsUnlocked, Is.True);
                Assert.That(unlocked.DisplayTitle, Is.EqualTo("Open title"));
                Assert.That(unlocked.Summary, Is.EqualTo("Open summary"));
                Assert.That(unlocked.Body, Is.EqualTo("Open body"));
                Assert.That(unlocked.OptionalImage, Is.SameAs(currentM3Sprite));

                var locked = entries[1];
                Assert.That(locked.IsUnlocked, Is.False);
                Assert.That(locked.DisplayTitle, Is.Null);
                Assert.That(locked.Summary, Is.Null);
                Assert.That(locked.Body, Is.Null);
                Assert.That(locked.OptionalImage, Is.Null);

                var titleVisible = entries[2];
                Assert.That(titleVisible.IsUnlocked, Is.False);
                Assert.That(titleVisible.DisplayTitle, Is.EqualTo("Known locked title"));
                Assert.That(titleVisible.Summary, Is.Null);
                Assert.That(titleVisible.Body, Is.Null);
                Assert.That(titleVisible.OptionalImage, Is.Null);
                Assert.That(service.GetEntries("unknown_category"), Is.Empty);
                Assert.That(((IList<VNArchiveCategoryProjection>)categories).IsReadOnly, Is.True);
                Assert.That(((IList<VNArchiveEntryProjection>)entries).IsReadOnly, Is.True);
            });
        }

        [Test]
        public void Archive_StaleUnlockDoesNotCreateAnEntryOrInflateCompletionCount()
        {
            SeedArchive("unknown_old_archive");
            var service = new VNArchiveService(
                CreateArchiveCatalog(
                    new VNArchiveCategoryDefinition("category_lore", "Lore", 0, true),
                    new VNArchiveEntryDefinition("archive_current", "category_lore", "Current", "Summary", "Body", 0)),
                metaProgress);

            AssertReadOnly(() =>
            {
                var category = service.GetCategories().Single();
                Assert.That(category.TotalCount, Is.EqualTo(1));
                Assert.That(category.UnlockedCount, Is.Zero);
                Assert.That(service.GetEntries("category_lore").Select(entry => entry.ArchiveId),
                    Is.EqualTo(new[] { "archive_current" }));
            });
        }

        [Test]
        public void Achievement_UsesPersistedUnlockAndHidesLockedHiddenMetadata()
        {
            var catalog = CreateAchievementCatalog(
                new VNAchievementDefinition("achievement_z_visible", "Visible locked", "Description stays visible", 1, false, VNAchievementConditionType.Manual),
                new VNAchievementDefinition("achievement_a_hidden", "SECRET TITLE", "SECRET DESCRIPTION", 0, true, VNAchievementConditionType.Manual, optionalIcon: currentM3Sprite),
                new VNAchievementDefinition("achievement_b_derived", "Derived but locked", "Its condition is not evaluated", 0, false, VNAchievementConditionType.CGCountAtLeast, threshold: 1, optionalIcon: currentM3Sprite),
                new VNAchievementDefinition("achievement_c_unlocked_hidden", "Unlocked title", "Unlocked description", 0, true, VNAchievementConditionType.AllOfAchievements, prerequisiteAchievementIds: new[] { "achievement_a_hidden" }, optionalIcon: currentM3Sprite));
            SeedCG("cg_that_meets_count");
            SeedAchievement("achievement_c_unlocked_hidden");
            var service = new VNAchievementService(catalog, metaProgress);

            AssertReadOnly(() =>
            {
                var achievements = service.GetAchievements();
                CollectionAssert.AreEqual(
                    new[] { "achievement_a_hidden", "achievement_b_derived", "achievement_c_unlocked_hidden", "achievement_z_visible" },
                    achievements.Select(achievement => achievement.AchievementId));

                var hidden = achievements.Single(achievement => achievement.AchievementId == "achievement_a_hidden");
                Assert.That(hidden.IsUnlocked, Is.False);
                Assert.That(hidden.DisplayTitle, Is.Null);
                Assert.That(hidden.Description, Is.Null);
                Assert.That(hidden.OptionalIcon, Is.Null);

                var visibleLocked = achievements.Single(achievement => achievement.AchievementId == "achievement_z_visible");
                Assert.That(visibleLocked.IsUnlocked, Is.False);
                Assert.That(visibleLocked.DisplayTitle, Is.EqualTo("Visible locked"));
                Assert.That(visibleLocked.Description, Is.EqualTo("Description stays visible"));

                var derived = achievements.Single(achievement => achievement.AchievementId == "achievement_b_derived");
                Assert.That(derived.IsUnlocked, Is.False,
                    "A satisfied-looking derived condition stays locked until MetaProgress says otherwise.");
                Assert.That(derived.DisplayTitle, Is.EqualTo("Derived but locked"));

                var unlockedHidden = achievements.Single(achievement => achievement.AchievementId == "achievement_c_unlocked_hidden");
                Assert.That(unlockedHidden.IsUnlocked, Is.True);
                Assert.That(unlockedHidden.DisplayTitle, Is.EqualTo("Unlocked title"));
                Assert.That(unlockedHidden.Description, Is.EqualTo("Unlocked description"));
                Assert.That(unlockedHidden.OptionalIcon, Is.SameAs(currentM3Sprite));

                var properties = typeof(VNAchievementProjection).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(property => property.Name);
                Assert.That(properties, Does.Not.Contain("ConditionType"));
                Assert.That(properties, Does.Not.Contain("ConditionTargetId"));
                Assert.That(properties, Does.Not.Contain("Threshold"));
                Assert.That(properties, Does.Not.Contain("PrerequisiteAchievementIds"));
                Assert.That(((IList<VNAchievementProjection>)achievements).IsReadOnly, Is.True);
            });
        }

        [Test]
        public void ProjectionCollectionsAreReadOnlySnapshots()
        {
            SeedChapter("chapter_one");
            SeedCG("cg_one");
            SeedArchive("archive_one");
            SeedAchievement("achievement_one");
            SeedReadLine("line:entry_one");

            var timeline = new VNTimelineService(
                CreateTimelineCatalog(new VNTimelineChapterDefinition("chapter_one", "Chapter", 0),
                    TimelineEntry("entry_one", "chapter_one", "Entry", "line:entry_one", "line:entry_done", 0)),
                metaProgress);
            var gallery = new VNCGGalleryService(
                CreateGalleryCatalog(new VNCGGalleryEntryDefinition("cg_one", "CG", 0)),
                CreatePresentationCatalog("cg_one"), metaProgress);
            var archive = new VNArchiveService(
                CreateArchiveCatalog(new VNArchiveCategoryDefinition("category_one", "Category", 0),
                    new VNArchiveEntryDefinition("archive_one", "category_one", "Archive", "Summary", "Body")),
                metaProgress);
            var achievements = new VNAchievementService(
                CreateAchievementCatalog(new VNAchievementDefinition("achievement_one", "Achievement", "Description", 0, false, VNAchievementConditionType.Manual)),
                metaProgress);

            AssertReadOnly(() =>
            {
                var chapter = timeline.GetVisibleChapters().Single();
                Assert.That(((IList<VNTimelineChapterProjection>)timeline.GetVisibleChapters()).IsReadOnly, Is.True);
                Assert.That(((IList<VNTimelineEntryProjection>)chapter.Entries).IsReadOnly, Is.True);
                Assert.That(((IList<VNCGGalleryEntryProjection>)GetGalleryEntries(gallery)).IsReadOnly, Is.True);
                Assert.That(((IList<VNArchiveCategoryProjection>)archive.GetCategories()).IsReadOnly, Is.True);
                Assert.That(((IList<VNArchiveEntryProjection>)archive.GetEntries("category_one")).IsReadOnly, Is.True);
                Assert.That(((IList<VNAchievementProjection>)achievements.GetAchievements()).IsReadOnly, Is.True);
            });
        }

        [Test]
        public void AllDomains_IgnorePreserveUnknownIdsAndRepeatedCallsAreSideEffectFree()
        {
            SeedReadLine("unknown_old_line");
            SeedCG("unknown_old_cg");
            SeedChapter("unknown_old_chapter");
            SeedArchive("unknown_old_archive");
            SeedAchievement("unknown_old_achievement");

            var timeline = new VNTimelineService(
                CreateTimelineCatalog(new VNTimelineChapterDefinition("chapter_current", "Current chapter", 0, false)),
                metaProgress);
            var gallery = new VNCGGalleryService(
                CreateGalleryCatalog(new VNCGGalleryEntryDefinition("cg_current", "Current CG", 0)),
                CreatePresentationCatalog("cg_current"), metaProgress);
            var archive = new VNArchiveService(
                CreateArchiveCatalog(new VNArchiveCategoryDefinition("category_current", "Current", 0),
                    new VNArchiveEntryDefinition("archive_current", "category_current", "Current", "Summary", "Body")),
                metaProgress);
            var achievements = new VNAchievementService(
                CreateAchievementCatalog(new VNAchievementDefinition("achievement_current", "Current", "Description", 0, false, VNAchievementConditionType.Manual)),
                metaProgress);

            AssertReadOnly(() =>
            {
                for (var repeat = 0; repeat < 2; repeat++)
                {
                    Assert.That(timeline.GetVisibleChapters(), Is.Empty);
                    Assert.That(GetGalleryEntries(gallery).Select(entry => entry.CgId), Is.EqualTo(new[] { "cg_current" }));
                    Assert.That(archive.GetEntries("category_current").Select(entry => entry.ArchiveId),
                        Is.EqualTo(new[] { "archive_current" }));
                    Assert.That(achievements.GetAchievements().Select(entry => entry.AchievementId),
                        Is.EqualTo(new[] { "achievement_current" }));
                }

                var persisted = metaProgress.Current;
                Assert.That(persisted.readLineIds, Does.Contain("unknown_old_line"));
                Assert.That(persisted.unlockedCGs, Does.Contain("unknown_old_cg"));
                Assert.That(persisted.unlockedChapters, Does.Contain("unknown_old_chapter"));
                Assert.That(persisted.unlockedArchiveEntries, Does.Contain("unknown_old_archive"));
                Assert.That(persisted.unlockedAchievements, Does.Contain("unknown_old_achievement"));
            });
        }

        private void AssertReadOnly(Action projectionCalls)
        {
            Assert.That(metaProgress.TryUnlockCG("   "), Is.False);
            var diagnostic = metaProgress.LastDiagnostic;
            var writeBaseline = writes;
            var fileExisted = File.Exists(repository.CanonicalFilePath);
            var fileBytes = fileExisted ? File.ReadAllBytes(repository.CanonicalFilePath) : null;

            projectionCalls();

            Assert.That(writes, Is.EqualTo(writeBaseline), "Projection calls must not write MetaProgress.");
            Assert.That(metaProgress.LastDiagnostic, Is.EqualTo(diagnostic), "Projection reads must not mutate MetaProgress diagnostics.");
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.EqualTo(fileExisted));
            if (fileExisted)
                CollectionAssert.AreEqual(fileBytes, File.ReadAllBytes(repository.CanonicalFilePath));
        }

        private void SeedReadLine(string id) => Assert.That(metaProgress.TryRecordReadLine(id), Is.True, "Could not seed read line " + id);
        private void SeedCG(string id) => Assert.That(metaProgress.TryUnlockCG(id), Is.True, "Could not seed CG " + id);
        private void SeedChapter(string id) => Assert.That(metaProgress.TryUnlockChapter(id), Is.True, "Could not seed chapter " + id);
        private void SeedArchive(string id) => Assert.That(metaProgress.TryUnlockArchiveEntry(id), Is.True, "Could not seed Archive entry " + id);
        private void SeedAchievement(string id) => Assert.That(metaProgress.TryUnlockAchievement(id), Is.True, "Could not seed achievement " + id);

        private static VNTimelineEntryDefinition TimelineEntry(
            string id, string chapterId, string title, string discovery, string completion, int sortOrder,
            string parentEntryId = null, string relatedCgId = null, IEnumerable<string> milestones = null)
        {
            return new VNTimelineEntryDefinition(id, chapterId, title, sortOrder, discovery, completion,
                milestones ?? new[] { discovery, completion }, parentEntryId, null, relatedCgId);
        }

        private VNTimelineCatalog CreateTimelineCatalog(params object[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNTimelineCatalog>());
            SetList(catalog, "chapters", definitions.OfType<VNTimelineChapterDefinition>());
            SetList(catalog, "entries", definitions.OfType<VNTimelineEntryDefinition>());
            return catalog;
        }

        private VNCGGalleryCatalog CreateGalleryCatalog(params VNCGGalleryEntryDefinition[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNCGGalleryCatalog>());
            SetList(catalog, "entries", definitions);
            return catalog;
        }

        private VNArchiveCatalog CreateArchiveCatalog(params object[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNArchiveCatalog>());
            SetList(catalog, "categories", definitions.OfType<VNArchiveCategoryDefinition>());
            SetList(catalog, "entries", definitions.OfType<VNArchiveEntryDefinition>());
            return catalog;
        }

        private VNAchievementCatalog CreateAchievementCatalog(params VNAchievementDefinition[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNAchievementCatalog>());
            SetList(catalog, "achievements", definitions);
            return catalog;
        }

        private VNPresentationCatalog CreatePresentationCatalog(params string[] cgIds)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNPresentationCatalog>());
            var entries = new List<VNSpriteCatalogEntry>();
            foreach (var id in cgIds)
            {
                var entry = new VNSpriteCatalogEntry();
                SetPrivateField(entry, "id", id);
                SetPrivateField(entry, "sprite", currentM3Sprite);
                entries.Add(entry);
            }

            SetList(catalog, "cgs", entries);
            return catalog;
        }

        private static IReadOnlyList<VNCGGalleryEntryProjection> GetGalleryEntries(VNCGGalleryService service)
        {
            Assert.That(service.TryGetGalleryEntries(out var entries, out var diagnostic), Is.True, diagnostic);
            return entries;
        }

        private T Own<T>(T value) where T : UnityEngine.Object
        {
            ownedObjects.Add(value);
            return value;
        }

        private static void SetList<T>(object instance, string fieldName, IEnumerable<T> values)
        {
            SetPrivateField(instance, fieldName, new List<T>(values));
            var onValidate = instance.GetType().GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidate?.Invoke(instance, null);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "Missing private field " + fieldName);
            field.SetValue(instance, value);
        }
    }
}
