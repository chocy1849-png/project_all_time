using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records;
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
    public sealed class VNRecordsCatalogTests
    {
        private const string YarnProjectPath = "Assets/_Project/Yarn/GameNarrative.yarnproject";
        private const string PresentationCatalogPath = "Assets/_Project/Settings/Presentation/M3_PresentationCatalog.asset";
        private const string ChapterId = "m8_smoke_chapter_01";
        private const string EntryId = "m8_smoke_timeline_entry_01";
        private const string DiscoveryLineId = "line:m8_meta_read_01";
        private const string FirstPassLineId = "line:m8_meta_first_pass_01";
        private const string CompletionLineId = "line:m8_meta_complete_01";
        private const string SmokeNode = "M8_META_PROGRESS_START";
        private const string EndingId = "m8_smoke_ending_01";

        private readonly List<UnityEngine.Object> ownedObjects = new();
        private YarnProject yarnProject;
        private VNPresentationCatalog presentationCatalog;
        private VNRecordsCatalogValidationContext context;

        [SetUp]
        public void SetUp()
        {
            yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>(YarnProjectPath);
            presentationCatalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(PresentationCatalogPath);
            Assert.That(yarnProject, Is.Not.Null, "Current GameNarrative Yarn Project must be imported.");
            Assert.That(presentationCatalog, Is.Not.Null, "Current M3 Presentation Catalog must be imported.");
            context = new VNRecordsCatalogValidationContext(yarnProject, presentationCatalog, new[] { EndingId });
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var owned in ownedObjects)
                if (owned != null) UnityEngine.Object.DestroyImmediate(owned);
            ownedObjects.Clear();
        }

        [TestCase("chapter_prologue", true)]
        [TestCase("chapter9", true)]
        [TestCase("a1_b2", true)]
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("   ", false)]
        [TestCase("Chapter_prologue", false)]
        [TestCase("chapter prologue", false)]
        [TestCase("_chapter", false)]
        [TestCase("chapter_", false)]
        [TestCase("chapter__one", false)]
        [TestCase("chapter-1", false)]
        public void StableIds_UseLowercaseAsciiSnakeCase(string value, bool expected)
        {
            Assert.That(VNRecordsCatalogValidation.IsStableId(value), Is.EqualTo(expected));
        }

        [Test]
        public void EmptyCatalogs_AreValidAndAllLookupApisReturnFalseForUnknownIds()
        {
            var candidate = CreateCandidate(
                chapters: Array.Empty<VNTimelineChapterDefinition>(),
                timelineEntries: Array.Empty<VNTimelineEntryDefinition>(),
                galleryEntries: Array.Empty<VNCGGalleryEntryDefinition>(),
                archiveCategories: Array.Empty<VNArchiveCategoryDefinition>(),
                archiveEntries: Array.Empty<VNArchiveEntryDefinition>(),
                achievements: Array.Empty<VNAchievementDefinition>());

            AssertValid(candidate);
            Assert.That(candidate.Timeline.TryGetChapter("missing_chapter", out _), Is.False);
            Assert.That(candidate.Timeline.TryGetChapter(" ", out _), Is.False);
            Assert.That(candidate.Timeline.TryGetEntry("missing_entry", out _), Is.False);
            Assert.That(candidate.Gallery.TryGetCG("missing_cg", out _), Is.False);
            Assert.That(candidate.Archive.TryGetCategory("missing_category", out _), Is.False);
            Assert.That(candidate.Archive.TryGetEntry("missing_archive", out _), Is.False);
            Assert.That(candidate.Achievements.TryGetAchievement("missing_achievement", out _), Is.False);
            Assert.That(candidate.Achievements.TryGetAchievement(null, out _), Is.False);
        }

        [Test]
        public void ActualYarnAuthority_ContainsExactM8NodeAndRuntimeLineIds()
        {
            var nodes = yarnProject.NodeNames;
            Assert.That(nodes, Does.Contain(SmokeNode));

            var lines = new HashSet<string>(
                yarnProject.GetLineIDsForNodes(new[] { SmokeNode }),
                StringComparer.Ordinal);

            Assert.That(lines, Does.Contain(DiscoveryLineId));
            Assert.That(lines, Does.Contain(FirstPassLineId));
            Assert.That(lines, Does.Contain(CompletionLineId));
            Assert.That(lines, Does.Not.Contain("m8_meta_read_01"),
                "Records references keep Yarn Spinner's exact runtime ID, including the line: prefix.");
        }

        [Test]
        public void CurrentProjectAuthorities_ValidateCompleteInMemoryTechnicalCatalogs()
        {
            var candidate = CreateValidCandidate();
            AssertValid(candidate);

            Assert.That(candidate.Timeline.TryGetChapter(ChapterId, out _), Is.True);
            Assert.That(candidate.Timeline.TryGetEntry(EntryId, out _), Is.True);
            Assert.That(candidate.Gallery.TryGetCG("m3_cg", out var cg), Is.True);
            Assert.That(cg.DisplayTitle, Is.EqualTo("Technical M3 CG"));
            Assert.That(presentationCatalog.TryGetCG("m3_cg", out var sprite), Is.True);
            Assert.That(sprite, Is.Not.Null);
        }

        [Test]
        public void Timeline_RejectsDuplicateChapterIds()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "chapters",
                new VNTimelineChapterDefinition(ChapterId, "Chapter", 0),
                new VNTimelineChapterDefinition(ChapterId, "Duplicate", 1));

            AssertInvalid(candidate, "duplicate chapter ID");
        }

        [Test]
        public void Timeline_RejectsDuplicateEntryIdsUsingOrdinalIdentity()
        {
            var candidate = CreateValidCandidate();
            var entry = ValidTimelineEntry();
            SetList(candidate.Timeline, "entries", entry, ValidTimelineEntry());

            AssertInvalid(candidate, "duplicate entry ID");
            Assert.That(candidate.Timeline.TryGetEntry(EntryId, out _), Is.False,
                "An ambiguous duplicate catalog must not return an arbitrary definition.");
        }

        [Test]
        public void Timeline_RejectsUnknownChapterReference()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "entries", ValidTimelineEntry(chapterId: "missing_chapter"));

            AssertInvalid(candidate, "missing or invalid chapter");
        }

        [Test]
        public void Timeline_RejectsMissingDiscoveryAndCompletionLines()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(discoveryLineId: "line:missing_discovery", milestones: new[] { "line:missing_discovery", CompletionLineId }));

            AssertInvalid(candidate, "missing Yarn discovery line");

            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(completionLineId: "line:missing_completion", milestones: new[] { DiscoveryLineId, "line:missing_completion" }));

            AssertInvalid(candidate, "missing Yarn completion line");
        }

        [Test]
        public void Timeline_RejectsDuplicateMilestoneLine()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(milestones: new[] { DiscoveryLineId, DiscoveryLineId, CompletionLineId }));

            AssertInvalid(candidate, "duplicate milestone line ID");
        }

        [Test]
        public void Timeline_RequiresDiscoveryAndCompletionInMilestones()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(milestones: new[] { FirstPassLineId, CompletionLineId }));

            AssertInvalid(candidate, "milestones omit discovery");

            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(milestones: new[] { DiscoveryLineId, FirstPassLineId }));

            AssertInvalid(candidate, "milestones omit completion");
        }

        [Test]
        public void Timeline_RejectsNullBlankAndMissingMilestonesAndInvalidMetadata()
        {
            var candidate = CreateValidCandidate();
            var entry = ValidTimelineEntry();
            SetPrivateField(entry, "milestoneLineIds", null);
            SetList(candidate.Timeline, "entries", entry);
            AssertInvalid(candidate, "non-null milestone line list");

            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(milestones: new[] { DiscoveryLineId, "  ", CompletionLineId }));
            AssertInvalid(candidate, "blank milestone");

            SetList(candidate.Timeline, "chapters", new VNTimelineChapterDefinition("Chapter_Invalid", "Title"));
            AssertInvalid(candidate, "lowercase snake_case");

            SetList(candidate.Timeline, "chapters", new VNTimelineChapterDefinition(ChapterId, "Title"));
            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(entryId: "timeline entry"));
            AssertInvalid(candidate, "lowercase snake_case");

            SetList(candidate.Timeline, "entries", new VNTimelineEntryDefinition(
                EntryId, ChapterId, "Title", -1, DiscoveryLineId, CompletionLineId,
                new[] { DiscoveryLineId, CompletionLineId }));
            AssertInvalid(candidate, "negative sort order");
        }

        [Test]
        public void Timeline_AllowsSingleStepDiscoveryAndCompletion()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(
                    discoveryLineId: DiscoveryLineId,
                    completionLineId: DiscoveryLineId,
                    milestones: new[] { DiscoveryLineId }));

            AssertValid(candidate);
        }

        [Test]
        public void Timeline_RejectsMissingSelfAndCrossChapterParents()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "entries", ValidTimelineEntry(parentEntryId: "missing_parent"));
            AssertInvalid(candidate, "missing parent entry");

            SetList(candidate.Timeline, "entries", ValidTimelineEntry(parentEntryId: EntryId));
            AssertInvalid(candidate, "cannot be its own parent");

            SetList(candidate.Timeline, "chapters",
                new VNTimelineChapterDefinition(ChapterId, "Chapter", 0),
                new VNTimelineChapterDefinition("m8_smoke_chapter_02", "Second chapter", 1));
            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(entryId: "parent_entry"),
                ValidTimelineEntry(entryId: EntryId, chapterId: "m8_smoke_chapter_02", parentEntryId: "parent_entry"));
            AssertInvalid(candidate, "same chapter");
        }

        [Test]
        public void Timeline_RejectsParentCycles()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "entries",
                ValidTimelineEntry(entryId: "entry_one", parentEntryId: "entry_two"),
                ValidTimelineEntry(entryId: "entry_two", parentEntryId: "entry_one"));

            AssertInvalid(candidate, "parent relationships contain a cycle");
        }

        [Test]
        public void Timeline_RejectsUnknownReplayNodeAndRelatedCG()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Timeline, "entries", ValidTimelineEntry(replayNode: "MISSING_REPLAY_NODE"));
            AssertInvalid(candidate, "missing replay Yarn node");

            SetList(candidate.Timeline, "entries", ValidTimelineEntry(relatedCgId: "missing_cg"));
            AssertInvalid(candidate, "missing Gallery CG");
        }

        [Test]
        public void Gallery_UsesPresentationCatalogForSpriteAuthorityAndAllowsLockedTitleMetadata()
        {
            var candidate = CreateValidCandidate();
            SetList(candidate.Gallery, "entries", new VNCGGalleryEntryDefinition("m3_cg", "Hidden title", 0, true));

            AssertValid(candidate);
            Assert.That(typeof(VNCGGalleryEntryDefinition).GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic), Is.Null);
            Assert.That(typeof(VNCGGalleryEntryDefinition).GetProperty("Sprite", BindingFlags.Instance | BindingFlags.Public), Is.Null);
        }

        [Test]
        public void Gallery_RejectsDuplicateAndUnresolvedPresentationCGs()
        {
            var candidate = CreateValidCandidate();
            var cg = new VNCGGalleryEntryDefinition("m3_cg", "CG", 0);
            SetList(candidate.Gallery, "entries", cg, cg);
            AssertInvalid(candidate, "duplicate CG ID");

            SetList(candidate.Gallery, "entries", new VNCGGalleryEntryDefinition("missing_cg", "Unknown", 0));
            AssertInvalid(candidate, "does not resolve to a Sprite");
        }

        [Test]
        public void Archive_EmptyAndOptionalImageAndLockDisclosurePoliciesAreValid()
        {
            var empty = CreateCandidate(
                archiveCategories: Array.Empty<VNArchiveCategoryDefinition>(),
                archiveEntries: Array.Empty<VNArchiveEntryDefinition>());
            AssertValid(empty);

            var withEntry = CreateValidCandidate();
            SetList(withEntry.Archive, "categories",
                new VNArchiveCategoryDefinition("lore_category", "Lore", 4, true));
            SetList(withEntry.Archive, "entries",
                new VNArchiveEntryDefinition("archive_record", "lore_category", "Secret title", "Summary", "Body", 2, true));
            AssertValid(withEntry);
            Assert.That(withEntry.Archive.Entries[0].OptionalImage, Is.Null);
            Assert.That(withEntry.Archive.Categories[0].ShowCompletionCount, Is.True);
            Assert.That(withEntry.Archive.Entries[0].ShowTitleWhenLocked, Is.True);
        }

        [Test]
        public void Archive_RejectsDuplicateCategoryAndEntryIdsAndUnknownCategory()
        {
            var candidate = CreateValidCandidate();
            var category = new VNArchiveCategoryDefinition("lore_category", "Lore");
            SetList(candidate.Archive, "categories", category, category);
            AssertInvalid(candidate, "duplicate category ID");

            SetList(candidate.Archive, "categories", category);
            var entry = new VNArchiveEntryDefinition("archive_record", "lore_category", "Title", "Summary", "Body");
            SetList(candidate.Archive, "entries", entry, entry);
            AssertInvalid(candidate, "duplicate entry ID");

            SetList(candidate.Archive, "entries",
                new VNArchiveEntryDefinition("archive_record", "missing_category", "Title", "Summary", "Body"));
            AssertInvalid(candidate, "missing or invalid category");
        }

        [TestCase("DisplayTitle")]
        [TestCase("Summary")]
        [TestCase("Body")]
        public void Archive_RejectsBlankRequiredText(string fieldName)
        {
            var candidate = CreateValidCandidate();
            var entry = new VNArchiveEntryDefinition("archive_record", "lore_category", "Title", "Summary", "Body");
            SetPrivateField(entry, fieldName switch
            {
                "DisplayTitle" => "displayTitle",
                "Summary" => "summary",
                "Body" => "body",
                _ => throw new ArgumentOutOfRangeException(nameof(fieldName))
            }, "  ");
            SetList(candidate.Archive, "entries", entry);

            AssertInvalid(candidate, fieldName switch
            {
                "DisplayTitle" => "requires a display title",
                "Summary" => "requires a summary",
                "Body" => "requires a body",
                _ => throw new ArgumentOutOfRangeException(nameof(fieldName))
            });
        }

        [Test]
        public void Achievement_RejectsDuplicateStableIds()
        {
            var candidate = CreateCandidate(achievements: new[]
            {
                ManualAchievement("duplicate_award"),
                ManualAchievement("duplicate_award")
            });
            AssertInvalid(candidate, "duplicate achievement ID");
        }

        [Test]
        public void Achievement_ManualAndKnownEndingDefinitionsAreValid()
        {
            var candidate = CreateCandidate(achievements: new[] { ManualAchievement(), Achievement("ending_seen", VNAchievementConditionType.EndingCompleted, target: EndingId) });
            AssertValid(candidate);

            SetList(candidate.Achievements, "achievements",
                ManualAchievement(),
                Achievement("ending_seen", VNAchievementConditionType.EndingCompleted, target: "m8_unknown_ending"));
            AssertInvalid(candidate, "unknown ending ID");

            var noEndingAuthority = new VNRecordsCatalogValidationContext(yarnProject, presentationCatalog);
            Assert.That(Validate(candidate, noEndingAuthority), Is.True,
                "EndingCompleted is intrinsically valid when no known-ending set was supplied.");
        }

        [Test]
        public void Achievement_ChapterUnlockedMustResolveTimelineChapter()
        {
            var candidate = CreateCandidate(achievements: new[] { Achievement("chapter_award", VNAchievementConditionType.ChapterUnlocked, target: ChapterId) });
            AssertValid(candidate);

            SetList(candidate.Achievements, "achievements",
                Achievement("chapter_award", VNAchievementConditionType.ChapterUnlocked, target: "missing_chapter"));
            AssertInvalid(candidate, "missing or invalid Timeline chapter");
        }

        [Test]
        public void Achievement_CGAndArchiveThresholdsMustFitAuthoredTotals()
        {
            var candidate = CreateCandidate(achievements: new[] { Achievement("cg_award", VNAchievementConditionType.CGCountAtLeast, threshold: 1), Achievement("archive_award", VNAchievementConditionType.ArchiveCountAtLeast, threshold: 1) });
            AssertValid(candidate);

            SetList(candidate.Achievements, "achievements",
                Achievement("cg_award", VNAchievementConditionType.CGCountAtLeast, threshold: 0));
            AssertInvalid(candidate, "CGCountAtLeast threshold must be at least 1");

            SetList(candidate.Achievements, "achievements",
                Achievement("cg_award", VNAchievementConditionType.CGCountAtLeast, threshold: 2));
            AssertInvalid(candidate, "exceeds Gallery total");

            SetList(candidate.Achievements, "achievements",
                Achievement("archive_award", VNAchievementConditionType.ArchiveCountAtLeast, threshold: 0));
            AssertInvalid(candidate, "ArchiveCountAtLeast threshold must be at least 1");

            SetList(candidate.Achievements, "achievements",
                Achievement("archive_award", VNAchievementConditionType.ArchiveCountAtLeast, threshold: 2));
            AssertInvalid(candidate, "exceeds Archive total");
        }

        [Test]
        public void Achievement_AllConditionsRejectEmptyReferencedCatalogs()
        {
            var candidate = CreateValidCandidate(Achievement("all_cgs", VNAchievementConditionType.AllCGsUnlocked));
            SetList(candidate.Timeline, "entries", ValidTimelineEntry(relatedCgId: null));
            SetList(candidate.Gallery, "entries", Array.Empty<VNCGGalleryEntryDefinition>());
            AssertInvalid(candidate, "empty Gallery catalog");

            SetList(candidate.Achievements, "achievements",
                Achievement("all_archive", VNAchievementConditionType.AllArchiveEntriesUnlocked));
            SetList(candidate.Archive, "entries", Array.Empty<VNArchiveEntryDefinition>());
            AssertInvalid(candidate, "empty Archive catalog");
        }

        [Test]
        public void Achievement_AllOfRequiresExistingUniqueNonSelfPrerequisites()
        {
            var candidate = CreateCandidate(achievements: new[] { ManualAchievement("manual_parent"), Achievement("derived_child", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "manual_parent" }) });
            AssertValid(candidate);

            SetList(candidate.Achievements, "achievements",
                Achievement("derived_child", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "missing_parent" }));
            AssertInvalid(candidate, "missing prerequisite");

            SetList(candidate.Achievements, "achievements",
                ManualAchievement("manual_parent"),
                Achievement("derived_child", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "manual_parent", "manual_parent" }));
            AssertInvalid(candidate, "duplicate prerequisite");

            SetList(candidate.Achievements, "achievements",
                Achievement("self_child", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "self_child" }));
            AssertInvalid(candidate, "cannot depend on itself");
        }

        [Test]
        public void Achievement_RejectsTwoNodeAndMultiNodeCycles()
        {
            var candidate = CreateCandidate(achievements: new[] { Achievement("achievement_a", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "achievement_b" }), Achievement("achievement_b", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "achievement_a" }) });
            AssertInvalid(candidate, "dependency graph contains a cycle");

            SetList(candidate.Achievements, "achievements",
                Achievement("achievement_a", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "achievement_b" }),
                Achievement("achievement_b", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "achievement_c" }),
                Achievement("achievement_c", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "achievement_a" }));
            AssertInvalid(candidate, "dependency graph contains a cycle");
        }

        [TestCase(VNAchievementConditionType.Manual)]
        [TestCase(VNAchievementConditionType.EndingCompleted)]
        [TestCase(VNAchievementConditionType.ChapterUnlocked)]
        [TestCase(VNAchievementConditionType.CGCountAtLeast)]
        [TestCase(VNAchievementConditionType.ArchiveCountAtLeast)]
        [TestCase(VNAchievementConditionType.AllCGsUnlocked)]
        [TestCase(VNAchievementConditionType.AllArchiveEntriesUnlocked)]
        [TestCase(VNAchievementConditionType.AllOfAchievements)]
        public void Achievement_RejectsIrrelevantOrConflictingConditionFields(VNAchievementConditionType type)
        {
            var target = type switch
            {
                VNAchievementConditionType.EndingCompleted => EndingId,
                VNAchievementConditionType.ChapterUnlocked => ChapterId,
                _ => "unexpected_target"
            };
            var threshold = type == VNAchievementConditionType.CGCountAtLeast ||
                            type == VNAchievementConditionType.ArchiveCountAtLeast
                ? 1
                : 7;
            var prerequisites = type == VNAchievementConditionType.AllOfAchievements
                ? new[] { "nonexistent_prerequisite" }
                : new[] { "unexpected_prerequisite" };
            var candidate = CreateCandidate(achievements: new[] { Achievement("condition_check", type, target, threshold, prerequisites) });

            AssertInvalid(candidate, type.ToString());
        }

        [Test]
        public void CatalogLookups_AreOrdinalAndDefinitionCollectionsAreReadOnly()
        {
            var candidate = CreateValidCandidate();
            Assert.That(candidate.Timeline.TryGetChapter(ChapterId, out _), Is.True);
            Assert.That(candidate.Timeline.TryGetChapter(ChapterId.ToUpperInvariant(), out _), Is.False);
            Assert.That(candidate.Timeline.TryGetEntry(EntryId, out _), Is.True);
            Assert.That(candidate.Gallery.TryGetCG("M3_CG", out _), Is.False);
            Assert.That(candidate.Archive.TryGetCategory("lore_category", out _), Is.True);
            Assert.That(candidate.Archive.TryGetEntry("m8_smoke_archive_01", out _), Is.True);
            Assert.That(candidate.Achievements.TryGetAchievement("m8_smoke_achievement_01", out _), Is.True);
            Assert.That(candidate.Achievements.TryGetAchievement("M8_SMOKE_ACHIEVEMENT_01", out _), Is.False);
            Assert.That(candidate.Timeline.Chapters, Is.InstanceOf<IReadOnlyList<VNTimelineChapterDefinition>>());
            Assert.That(((IList<VNTimelineChapterDefinition>)candidate.Timeline.Chapters).IsReadOnly, Is.True);
            Assert.That(((IList<VNCGGalleryEntryDefinition>)candidate.Gallery.Entries).IsReadOnly, Is.True);
        }

        private Candidate CreateValidCandidate(params VNAchievementDefinition[] achievements)
        {
            if (achievements == null || achievements.Length == 0)
            {
                achievements = new[]
                {
                    ManualAchievement(),
                    Achievement("ending_seen", VNAchievementConditionType.EndingCompleted, target: EndingId),
                    Achievement("chapter_seen", VNAchievementConditionType.ChapterUnlocked, target: ChapterId),
                    Achievement("cg_count", VNAchievementConditionType.CGCountAtLeast, threshold: 1),
                    Achievement("archive_count", VNAchievementConditionType.ArchiveCountAtLeast, threshold: 1),
                    Achievement("all_cgs", VNAchievementConditionType.AllCGsUnlocked),
                    Achievement("all_archive", VNAchievementConditionType.AllArchiveEntriesUnlocked),
                    Achievement("achievement_set", VNAchievementConditionType.AllOfAchievements,
                        prerequisites: new[] { "m8_smoke_achievement_01" })
                };
            }

            return CreateCandidate(achievements: achievements);
        }

        private Candidate CreateCandidate(
            VNTimelineChapterDefinition[] chapters = null,
            VNTimelineEntryDefinition[] timelineEntries = null,
            VNCGGalleryEntryDefinition[] galleryEntries = null,
            VNArchiveCategoryDefinition[] archiveCategories = null,
            VNArchiveEntryDefinition[] archiveEntries = null,
            VNAchievementDefinition[] achievements = null)
        {
            var timeline = Own(ScriptableObject.CreateInstance<VNTimelineCatalog>());
            var gallery = Own(ScriptableObject.CreateInstance<VNCGGalleryCatalog>());
            var archive = Own(ScriptableObject.CreateInstance<VNArchiveCatalog>());
            var achievementCatalog = Own(ScriptableObject.CreateInstance<VNAchievementCatalog>());

            SetList(timeline, "chapters", chapters ?? new[] { new VNTimelineChapterDefinition(ChapterId, "Technical chapter") });
            SetList(timeline, "entries", timelineEntries ?? new[] { ValidTimelineEntry() });
            SetList(gallery, "entries", galleryEntries ?? new[] { new VNCGGalleryEntryDefinition("m3_cg", "Technical M3 CG") });
            SetList(archive, "categories", archiveCategories ?? new[] { new VNArchiveCategoryDefinition("lore_category", "Lore") });
            SetList(archive, "entries", archiveEntries ?? new[]
            {
                new VNArchiveEntryDefinition("m8_smoke_archive_01", "lore_category", "Technical archive", "A test summary.", "A test body.")
            });
            SetList(achievementCatalog, "achievements", achievements ?? new[] { ManualAchievement() });

            return new Candidate(timeline, gallery, archive, achievementCatalog);
        }

        private static VNTimelineEntryDefinition ValidTimelineEntry(
            string entryId = EntryId,
            string chapterId = ChapterId,
            string discoveryLineId = DiscoveryLineId,
            string completionLineId = CompletionLineId,
            IEnumerable<string> milestones = null,
            string parentEntryId = null,
            string replayNode = SmokeNode,
            string relatedCgId = "m3_cg")
        {
            return new VNTimelineEntryDefinition(
                entryId,
                chapterId,
                "Technical timeline entry",
                0,
                discoveryLineId,
                completionLineId,
                milestones ?? new[] { DiscoveryLineId, FirstPassLineId, CompletionLineId },
                parentEntryId,
                replayNode,
                relatedCgId);
        }

        private static VNAchievementDefinition ManualAchievement(string achievementId = "m8_smoke_achievement_01")
        {
            return Achievement(achievementId, VNAchievementConditionType.Manual);
        }

        private static VNAchievementDefinition Achievement(
            string id,
            VNAchievementConditionType type,
            string target = null,
            int threshold = 0,
            IEnumerable<string> prerequisites = null)
        {
            return new VNAchievementDefinition(id, "Technical achievement", "Test-only definition", 0, false, type,
                target, threshold, prerequisites);
        }

        private bool Validate(Candidate candidate, VNRecordsCatalogValidationContext validationContext)
        {
            return VNRecordsCatalogValidator.TryValidate(candidate.Timeline, candidate.Gallery,
                candidate.Archive, candidate.Achievements, validationContext, out _);
        }

        private void AssertValid(Candidate candidate)
        {
            Assert.That(VNRecordsCatalogValidator.TryValidate(candidate.Timeline, candidate.Gallery,
                candidate.Archive, candidate.Achievements, context, out var diagnostic), Is.True, diagnostic);
            Assert.That(diagnostic, Is.Null);
        }

        private void AssertInvalid(Candidate candidate, string expectedDiagnostic)
        {
            var valid = VNRecordsCatalogValidator.TryValidate(candidate.Timeline, candidate.Gallery,
                candidate.Archive, candidate.Achievements, context, out var diagnostic);
            Assert.That(valid, Is.False);
            Assert.That(diagnostic, Does.Contain(expectedDiagnostic));
        }

        private T Own<T>(T instance) where T : UnityEngine.Object
        {
            ownedObjects.Add(instance);
            return instance;
        }

        private static void SetList<T>(object catalog, string fieldName, params T[] definitions)
        {
            SetPrivateField(catalog, fieldName, new List<T>(definitions)); var onValidate = catalog.GetType().GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic); onValidate?.Invoke(catalog, null);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected private serialized field '{fieldName}'.");
            field.SetValue(instance, value);
        }

        private sealed class Candidate
        {
            public VNTimelineCatalog Timeline { get; }
            public VNCGGalleryCatalog Gallery { get; }
            public VNArchiveCatalog Archive { get; }
            public VNAchievementCatalog Achievements { get; }

            public Candidate(
                VNTimelineCatalog timeline,
                VNCGGalleryCatalog gallery,
                VNArchiveCatalog archive,
                VNAchievementCatalog achievements)
            {
                Timeline = timeline;
                Gallery = gallery;
                Archive = archive;
                Achievements = achievements;
            }
        }
    }
}
