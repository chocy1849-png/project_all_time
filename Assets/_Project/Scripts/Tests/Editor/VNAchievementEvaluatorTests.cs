using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNAchievementEvaluatorTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private int writes;
        private FileStream heldFileLock;

        [SetUp]
        public void SetUp()
        {
            writes = 0;
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M9AchievementEvaluatorTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();
        }

        [TearDown]
        public void TearDown()
        {
            heldFileLock?.Dispose();
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                if (ownedObjects[index] != null) UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void Constructor_RejectsNullDependencies()
        {
            var achievements = CreateAchievementCatalog();
            var gallery = CreateGalleryCatalog();
            var archive = CreateArchiveCatalog();
            Assert.Throws<ArgumentNullException>(() => new VNAchievementEvaluator(null, gallery, archive, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNAchievementEvaluator(achievements, null, archive, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNAchievementEvaluator(achievements, gallery, null, metaProgress));
            Assert.Throws<ArgumentNullException>(() => new VNAchievementEvaluator(achievements, gallery, archive, null));
        }

        [Test]
        public void Initialize_CatchesUpDerivedConditionsOnce_SkipsManualAndAlreadyUnlocked_AndDisposeUnsubscribes()
        {
            var catalog = CreateAchievementCatalog(
                Achievement("manual", VNAchievementConditionType.Manual),
                Achievement("ending_award", VNAchievementConditionType.EndingCompleted, target: "ending_1"),
                Achievement("chapter_award", VNAchievementConditionType.ChapterUnlocked, target: "chapter_1"),
                Achievement("cg_award", VNAchievementConditionType.CGCountAtLeast, threshold: 2),
                Achievement("archive_award", VNAchievementConditionType.ArchiveCountAtLeast, threshold: 1),
                Achievement("all_cgs_award", VNAchievementConditionType.AllCGsUnlocked),
                Achievement("all_archive_award", VNAchievementConditionType.AllArchiveEntriesUnlocked),
                Achievement("combined_award", VNAchievementConditionType.AllOfAchievements,
                    prerequisites: new[] { "ending_award", "chapter_award" }),
                Achievement("already_unlocked", VNAchievementConditionType.EndingCompleted, target: "ending_1"),
                Achievement("after_dispose", VNAchievementConditionType.EndingCompleted, target: "ending_after_dispose"));
            var gallery = CreateGalleryCatalog("cg_1", "cg_2", "cg_3");
            var archive = CreateArchiveCatalog("archive_1");

            SeedChapter("chapter_1");
            SeedEnding("ending_1");
            SeedCG("cg_1");
            SeedCG("cg_2");
            SeedCG("cg_3");
            SeedArchive("archive_1");
            Assert.That(metaProgress.TryUnlockAchievement("already_unlocked"), Is.True);
            var writesBeforeEvaluation = writes;

            var evaluator = new VNAchievementEvaluator(catalog, gallery, archive, metaProgress);
            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);
            Assert.That(evaluator.IsInitialized, Is.True);
            var evaluationCountAfterInitialize = GetEvaluationEntryCount(evaluator);
            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);
            Assert.That(GetEvaluationEntryCount(evaluator), Is.EqualTo(evaluationCountAfterInitialize),
                "Initialize is idempotent and does not subscribe or catch up twice.");

            foreach (var id in new[]
                     {
                         "ending_award", "chapter_award", "cg_award", "archive_award",
                         "all_cgs_award", "all_archive_award", "combined_award", "already_unlocked",
                     })
                Assert.That(metaProgress.IsAchievementUnlocked(id), Is.True, id);
            Assert.That(metaProgress.IsAchievementUnlocked("manual"), Is.False);
            Assert.That(metaProgress.IsAchievementUnlocked("after_dispose"), Is.False);
            Assert.That(writes - writesBeforeEvaluation, Is.EqualTo(7),
                "Each newly satisfied derived Achievement is persisted once; an already unlocked ID is skipped.");

            var writesAfterCatchUp = writes;
            Assert.That(evaluator.TryEvaluateAll(), Is.True, evaluator.LastDiagnostic);
            Assert.That(writes, Is.EqualTo(writesAfterCatchUp));
            var callsBeforeDispose = GetEvaluationEntryCount(evaluator);
            evaluator.Dispose();
            evaluator.Dispose();
            Assert.That(evaluator.IsInitialized, Is.False);
            SeedEnding("ending_after_dispose");
            Assert.That(metaProgress.IsAchievementUnlocked("after_dispose"), Is.False,
                "Progress events after disposal must not trigger automatic evaluation.");
            Assert.That(GetEvaluationEntryCount(evaluator), Is.EqualTo(callsBeforeDispose));
            Assert.That(evaluator.TryEvaluateAll(), Is.False);
            Assert.That(evaluator.LastDiagnostic, Does.Contain("after disposal"));
        }

        [Test]
        public void Conditions_UseAuthoredCollectionMembershipAndExplicitPrerequisites()
        {
            var achievements = CreateAchievementCatalog(
                Achievement("manual_gate", VNAchievementConditionType.Manual),
                Achievement("manual_never", VNAchievementConditionType.Manual),
                Achievement("ending_award", VNAchievementConditionType.EndingCompleted, target: "ending_1"),
                Achievement("chapter_award", VNAchievementConditionType.ChapterUnlocked, target: "chapter_1"),
                Achievement("cg_one", VNAchievementConditionType.CGCountAtLeast, threshold: 1),
                Achievement("cg_two", VNAchievementConditionType.CGCountAtLeast, threshold: 2),
                Achievement("cg_three", VNAchievementConditionType.CGCountAtLeast, threshold: 3),
                Achievement("archive_one", VNAchievementConditionType.ArchiveCountAtLeast, threshold: 1),
                Achievement("archive_two", VNAchievementConditionType.ArchiveCountAtLeast, threshold: 2),
                Achievement("archive_three", VNAchievementConditionType.ArchiveCountAtLeast, threshold: 3),
                Achievement("all_cgs", VNAchievementConditionType.AllCGsUnlocked),
                Achievement("all_archive", VNAchievementConditionType.AllArchiveEntriesUnlocked),
                Achievement("explicit_master", VNAchievementConditionType.AllOfAchievements,
                    prerequisites: new[] { "manual_gate" }));
            var gallery = CreateGalleryCatalog("cg_a", "cg_b", "cg_c");
            var archive = CreateArchiveCatalog("archive_a", "archive_b", "archive_c");
            SeedCG("cg_stale_unknown");
            SeedArchive("archive_stale_unknown");
            var evaluator = new VNAchievementEvaluator(achievements, gallery, archive, metaProgress);

            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);
            foreach (var id in new[] { "cg_one", "cg_two", "cg_three", "archive_one", "archive_two", "archive_three", "all_cgs", "all_archive" })
                Assert.That(metaProgress.IsAchievementUnlocked(id), Is.False, "Stale IDs must not count: " + id);
            Assert.That(metaProgress.IsAchievementUnlocked("manual_gate"), Is.False);
            Assert.That(metaProgress.IsAchievementUnlocked("manual_never"), Is.False);

            SeedCG("cg_a");
            Assert.That(metaProgress.IsAchievementUnlocked("cg_one"), Is.True, "One authored CG meets threshold one.");
            Assert.That(metaProgress.IsAchievementUnlocked("cg_two"), Is.False);
            SeedCG("cg_b");
            Assert.That(metaProgress.IsAchievementUnlocked("cg_two"), Is.True, "Two authored CGs meet threshold two.");
            Assert.That(metaProgress.IsAchievementUnlocked("cg_one"), Is.True, "Two unlocked CGs are above threshold one.");
            Assert.That(metaProgress.IsAchievementUnlocked("cg_three"), Is.False);
            Assert.That(metaProgress.IsAchievementUnlocked("all_cgs"), Is.False);
            SeedCG("cg_c");
            Assert.That(metaProgress.IsAchievementUnlocked("cg_three"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("all_cgs"), Is.True);

            SeedArchive("archive_a");
            Assert.That(metaProgress.IsAchievementUnlocked("archive_one"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("archive_two"), Is.False);
            SeedArchive("archive_b");
            Assert.That(metaProgress.IsAchievementUnlocked("archive_two"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("archive_one"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("archive_three"), Is.False);
            Assert.That(metaProgress.IsAchievementUnlocked("all_archive"), Is.False);
            SeedArchive("archive_c");
            Assert.That(metaProgress.IsAchievementUnlocked("archive_three"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("all_archive"), Is.True);

            SeedEnding("ending_1");
            Assert.That(metaProgress.IsAchievementUnlocked("ending_award"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("chapter_award"), Is.False);
            SeedChapter("chapter_1");
            Assert.That(metaProgress.IsAchievementUnlocked("chapter_award"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("explicit_master"), Is.False);

            Assert.That(metaProgress.TryUnlockAchievement("manual_gate"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("explicit_master"), Is.True,
                "AllOfAchievements uses its explicit prerequisite list, even while another Manual ID remains locked.");
            Assert.That(metaProgress.IsAchievementUnlocked("manual_never"), Is.False);
        }

        [Test]
        public void EmptyCatalogs_NeverSatisfyAllConditions()
        {
            var achievements = CreateAchievementCatalog(
                Achievement("empty_cgs", VNAchievementConditionType.AllCGsUnlocked),
                Achievement("empty_archive", VNAchievementConditionType.AllArchiveEntriesUnlocked));
            var evaluator = new VNAchievementEvaluator(achievements, CreateGalleryCatalog(), CreateArchiveCatalog(), metaProgress);

            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);
            Assert.That(metaProgress.IsAchievementUnlocked("empty_cgs"), Is.False);
            Assert.That(metaProgress.IsAchievementUnlocked("empty_archive"), Is.False);
            Assert.That(writes, Is.Zero);
        }

        [Test]
        public void UnsupportedCondition_FailsDiagnosticallyWithoutWritingOrShadowUnlock()
        {
            var achievements = CreateAchievementCatalog(
                Achievement("unsupported", (VNAchievementConditionType)999));
            var evaluator = Evaluator(achievements);

            Assert.That(evaluator.Initialize(), Is.False);
            Assert.That(evaluator.LastDiagnostic, Does.Contain("unsupported condition type"));
            Assert.That(metaProgress.IsAchievementUnlocked("unsupported"), Is.False);
            Assert.That(writes, Is.Zero);
        }

        [Test]
        public void ReadLineNotification_IsIgnored_ButRelevantEndingNotificationEvaluates()
        {
            var achievements = CreateAchievementCatalog(
                Achievement("ending_award", VNAchievementConditionType.EndingCompleted, target: "ending_1"));
            var evaluator = Evaluator(achievements);
            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);
            var evaluationCount = GetEvaluationEntryCount(evaluator);
            var writesBeforeReadLine = writes;

            SeedLine("line:unrelated");
            Assert.That(GetEvaluationEntryCount(evaluator), Is.EqualTo(evaluationCount),
                "ReadLine changes do not run any v1 Achievement condition evaluation.");
            Assert.That(metaProgress.IsAchievementUnlocked("ending_award"), Is.False);
            Assert.That(writes, Is.EqualTo(writesBeforeReadLine + 1), "Only the durable read-line mutation should write.");

            SeedEnding("ending_1");
            Assert.That(GetEvaluationEntryCount(evaluator), Is.EqualTo(evaluationCount + 1));
            Assert.That(metaProgress.IsAchievementUnlocked("ending_award"), Is.True);
            Assert.That(writes, Is.EqualTo(writesBeforeReadLine + 3), "Ending and derived Achievement each persist once.");
        }

        [Test]
        public void ReverseCatalogOrder_ReachesMultiLevelFixedPointWithoutRecursiveEvaluationOrDuplicateWrites()
        {
            var achievements = CreateAchievementCatalog(
                Achievement("achievement_c", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "achievement_b" }, sortOrder: 0),
                Achievement("achievement_b", VNAchievementConditionType.AllOfAchievements, prerequisites: new[] { "achievement_a" }, sortOrder: 1),
                Achievement("achievement_a", VNAchievementConditionType.CGCountAtLeast, threshold: 1, sortOrder: 2));
            var gallery = CreateGalleryCatalog("cg_seed");
            var archive = CreateArchiveCatalog();
            SeedCG("cg_seed");
            var unlockOrder = new List<string>();
            metaProgress.ProgressChanged += change =>
            {
                if (change.Kind == VNMetaProgressChangeKind.Achievement) unlockOrder.Add(change.Id);
            };
            var evaluator = new VNAchievementEvaluator(achievements, gallery, archive, metaProgress);
            var writesBeforeEvaluation = writes;

            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);
            Assert.That(metaProgress.IsAchievementUnlocked("achievement_a"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("achievement_b"), Is.True);
            Assert.That(metaProgress.IsAchievementUnlocked("achievement_c"), Is.True);
            CollectionAssert.AreEqual(new[] { "achievement_a", "achievement_b", "achievement_c" }, unlockOrder);
            Assert.That(writes - writesBeforeEvaluation, Is.EqualTo(3));
            Assert.That(GetEvaluationEntryCount(evaluator), Is.EqualTo(1),
                "Achievement notifications raised during evaluation must not recursively start another pass.");

            var writesAfterCascade = writes;
            Assert.That(evaluator.TryEvaluateAll(), Is.True, evaluator.LastDiagnostic);
            Assert.That(writes, Is.EqualTo(writesAfterCascade), "Already unlocked IDs are not rewritten.");
        }

        [Test]
        public void DerivedWriteFailure_PreservesSourceAndEarlierUnlocks_AndCanRetryLater()
        {
            var partialRoot = Path.Combine(temporaryRoot, "partial_failure");
            var successfulWrites = 0;
            VNMetaProgressRepository partialRepository = null;
            partialRepository = VNMetaProgressRepository.CreateForTesting(partialRoot, () =>
            {
                successfulWrites++;
                writes++;
                if (successfulWrites == 2)
                    heldFileLock = new FileStream(partialRepository.CanonicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            });
            var partialProgress = new VNMetaProgressService(partialRepository);
            partialProgress.Load();
            var achievements = CreateAchievementCatalog(
                Achievement("achievement_a", VNAchievementConditionType.EndingCompleted, target: "ending_partial", sortOrder: 0),
                Achievement("achievement_b", VNAchievementConditionType.EndingCompleted, target: "ending_partial", sortOrder: 1));
            var evaluator = new VNAchievementEvaluator(achievements, CreateGalleryCatalog(), CreateArchiveCatalog(), partialProgress);

            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);
            Assert.That(partialProgress.TryCompleteEnding("ending_partial"), Is.True,
                "An evaluator persistence failure must not make the already committed source mutation appear failed.");
            Assert.That(partialProgress.IsEndingCompleted("ending_partial"), Is.True);
            Assert.That(partialProgress.IsAchievementUnlocked("achievement_a"), Is.True);
            Assert.That(partialProgress.IsAchievementUnlocked("achievement_b"), Is.False);
            Assert.That(successfulWrites, Is.EqualTo(2), "Ending and the first Achievement persist; the blocked write does not.");
            Assert.That(evaluator.LastDiagnostic, Is.Not.Null.And.Not.Empty);

            heldFileLock.Dispose();
            heldFileLock = null;
            var persistedAfterPartialFailure = partialRepository.Read().Data;
            CollectionAssert.AreEqual(new[] { "achievement_a" }, persistedAfterPartialFailure.unlockedAchievements);
            CollectionAssert.AreEqual(new[] { "ending_partial" }, persistedAfterPartialFailure.completedEndings);

            Assert.That(evaluator.TryEvaluateAll(), Is.True, evaluator.LastDiagnostic);
            Assert.That(partialProgress.IsAchievementUnlocked("achievement_a"), Is.True);
            Assert.That(partialProgress.IsAchievementUnlocked("achievement_b"), Is.True);
            Assert.That(successfulWrites, Is.EqualTo(3));
            Assert.That(evaluator.LastDiagnostic, Is.Null);
        }

        [Test]
        public void WriteProtectedMetaProgress_ReportsFailureWithoutShadowUnlockOrException()
        {
            var protectedRoot = Path.Combine(temporaryRoot, "write_protected");
            var protectedRepository = VNMetaProgressRepository.CreateForTesting(protectedRoot);
            var protectedProgress = new VNMetaProgressService(protectedRepository);
            protectedProgress.Load();
            Assert.That(protectedProgress.TryUnlockCG("cg_existing"), Is.True);
            const string futureSchema = "{\"schemaVersion\":2,\"futureField\":\"preserve\"}";
            File.WriteAllText(protectedRepository.CanonicalFilePath, futureSchema);

            var achievements = CreateAchievementCatalog(
                Achievement("derived_award", VNAchievementConditionType.CGCountAtLeast, threshold: 1));
            var evaluator = new VNAchievementEvaluator(
                achievements, CreateGalleryCatalog("cg_existing"), CreateArchiveCatalog(), protectedProgress);

            Assert.That(evaluator.Initialize(), Is.False);
            Assert.That(evaluator.IsInitialized, Is.True);
            Assert.That(protectedProgress.IsWriteProtected, Is.True);
            Assert.That(protectedProgress.IsCGUnlocked("cg_existing"), Is.True);
            Assert.That(protectedProgress.IsAchievementUnlocked("derived_award"), Is.False);
            Assert.That(evaluator.LastDiagnostic, Does.Contain("Could not persist derived Achievement"));
            Assert.That(File.ReadAllText(protectedRepository.CanonicalFilePath), Is.EqualTo(futureSchema));
        }

        private VNAchievementEvaluator Evaluator(VNAchievementCatalog achievements)
        {
            return new VNAchievementEvaluator(achievements, CreateGalleryCatalog(), CreateArchiveCatalog(), metaProgress);
        }

        private VNAchievementCatalog CreateAchievementCatalog(params VNAchievementDefinition[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNAchievementCatalog>());
            SetList(catalog, "achievements", definitions);
            return catalog;
        }

        private VNCGGalleryCatalog CreateGalleryCatalog(params string[] cgIds)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNCGGalleryCatalog>());
            SetList(catalog, "entries", cgIds.Select((id, index) =>
                new VNCGGalleryEntryDefinition(id, "Technical CG " + index, index)));
            return catalog;
        }

        private VNArchiveCatalog CreateArchiveCatalog(params string[] archiveIds)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNArchiveCatalog>());
            SetList(catalog, "categories", new[] { new VNArchiveCategoryDefinition("category_test", "Technical Archive") });
            SetList(catalog, "entries", archiveIds.Select((id, index) =>
                new VNArchiveEntryDefinition(id, "category_test", "Technical Archive " + index, "Summary", "Body", index)));
            return catalog;
        }

        private static VNAchievementDefinition Achievement(
            string id,
            VNAchievementConditionType type,
            string target = null,
            int threshold = 0,
            IEnumerable<string> prerequisites = null,
            int sortOrder = 0)
        {
            return new VNAchievementDefinition(id, "Technical " + id, "Test-only definition", sortOrder, false,
                type, target, threshold, prerequisites);
        }

        private void SeedLine(string id) => Assert.That(metaProgress.TryRecordReadLine(id), Is.True, "Could not seed line " + id);
        private void SeedCG(string id) => Assert.That(metaProgress.TryUnlockCG(id), Is.True, "Could not seed CG " + id);
        private void SeedChapter(string id) => Assert.That(metaProgress.TryUnlockChapter(id), Is.True, "Could not seed chapter " + id);
        private void SeedArchive(string id) => Assert.That(metaProgress.TryUnlockArchiveEntry(id), Is.True, "Could not seed Archive entry " + id);
        private void SeedEnding(string id) => Assert.That(metaProgress.TryCompleteEnding(id), Is.True, "Could not seed ending " + id);

        private int GetEvaluationEntryCount(VNAchievementEvaluator evaluator)
        {
            var field = typeof(VNAchievementEvaluator).GetField("evaluationEntryCountForTests", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Editor test seam missing.");
            return (int)field.GetValue(evaluator);
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
