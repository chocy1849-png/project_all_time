using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNMetaProgressPersistenceTests
    {
        private static readonly string[] RequiredFields =
        {
            "schemaVersion", "readLineIds", "unlockedCGs", "unlockedChapters", "unlockedArchiveEntries", "unlockedAchievements", "completedEndings",
        };

        private string temporaryRoot;
        private VNMetaProgressRepository repository;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M8MetaProgressTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void Defaults_AreSchemaOneEmptyAndIndependent_AndMissingLoadCreatesNothing()
        {
            var first = VNMetaProgressDefaults.CreateDefault();
            var second = VNMetaProgressDefaults.CreateDefault();
            Assert.That(first.schemaVersion, Is.EqualTo(1));
            AssertAllEmpty(first);
            first.readLineIds = new[] { "changed" };
            Assert.That(second.readLineIds, Is.Empty);

            var service = new VNMetaProgressService(repository);
            AssertAllEmpty(service.Load());
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            Assert.That(Directory.Exists(temporaryRoot), Is.False);
        }

        [Test]
        public void AllCategories_RoundTripAndPreserveOrdinalCaseSensitivity()
        {
            var service = new VNMetaProgressService(repository);
            service.Load();
            Assert.That(service.TryRecordReadLine("line_a"), Is.True);
            Assert.That(service.TryRecordReadLine("LINE_A"), Is.True);
            Assert.That(service.TryUnlockCG("cg_1"), Is.True);
            Assert.That(service.TryUnlockChapter("chapter_1"), Is.True);
            Assert.That(service.TryUnlockArchiveEntry("archive_1"), Is.True);
            Assert.That(service.TryUnlockAchievement("achievement_1"), Is.True);
            Assert.That(service.TryCompleteEnding("ending_1"), Is.True);

            var reloaded = new VNMetaProgressService(repository).Load();
            CollectionAssert.AreEqual(new[] { "LINE_A", "line_a" }, reloaded.readLineIds);
            CollectionAssert.AreEqual(new[] { "cg_1" }, reloaded.unlockedCGs);
            CollectionAssert.AreEqual(new[] { "chapter_1" }, reloaded.unlockedChapters);
            CollectionAssert.AreEqual(new[] { "archive_1" }, reloaded.unlockedArchiveEntries);
            CollectionAssert.AreEqual(new[] { "achievement_1" }, reloaded.unlockedAchievements);
            CollectionAssert.AreEqual(new[] { "ending_1" }, reloaded.completedEndings);
        }

        [Test]
        public void Write_CanonicalizesDedupesAndSerializesDeterministically()
        {
            var unsorted = VNMetaProgressDefaults.CreateDefault();
            unsorted.readLineIds = new[] { "b", "a", "b", "A" };
            unsorted.unlockedCGs = new[] { "z", "A", "z" };
            Assert.That(repository.Write(unsorted).Succeeded, Is.True);
            var firstBytes = File.ReadAllBytes(repository.CanonicalFilePath);
            var loaded = repository.Read().Data;
            CollectionAssert.AreEqual(new[] { "A", "a", "b" }, loaded.readLineIds);
            CollectionAssert.AreEqual(new[] { "A", "z" }, loaded.unlockedCGs);

            var equivalent = VNMetaProgressDefaults.CreateDefault();
            equivalent.readLineIds = new[] { "a", "A", "b" };
            equivalent.unlockedCGs = new[] { "z", "A" };
            Assert.That(repository.Write(equivalent).Succeeded, Is.True);
            CollectionAssert.AreEqual(firstBytes, File.ReadAllBytes(repository.CanonicalFilePath));
        }

        [Test]
        public void RepeatedMutation_IsSuccessfulNoOpWithoutSecondWrite()
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = new VNMetaProgressService(repository);
            service.Load();
            Assert.That(service.TryUnlockCG("cg_a"), Is.True);
            Assert.That(service.TryUnlockCG("cg_a"), Is.True);
            Assert.That(writes, Is.EqualTo(1));
            CollectionAssert.AreEqual(new[] { "cg_a" }, service.Current.unlockedCGs);
        }

        [Test]
        public void AtomicFirstWriteAndReplace_LeaveNoOwnedTemporaryFiles()
        {
            var first = VNMetaProgressDefaults.CreateDefault();
            first.readLineIds = new[] { "first" };
            Assert.That(repository.Write(first).Succeeded, Is.True);
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.True);
            AssertNoOwnedTemporaryFiles();

            var second = VNMetaProgressDefaults.CreateDefault();
            second.readLineIds = new[] { "second" };
            Assert.That(repository.Write(second).Succeeded, Is.True);
            CollectionAssert.AreEqual(new[] { "second" }, repository.Read().Data.readLineIds);
            AssertNoOwnedTemporaryFiles();
        }

        [Test]
        public void FailedWrite_CleansOnlyOwnedTempAndDoesNotCommitServiceCandidate()
        {
            Directory.CreateDirectory(temporaryRoot);
            Directory.CreateDirectory(repository.CanonicalFilePath);
            var unrelated = Path.Combine(temporaryRoot, "unrelated.keep");
            File.WriteAllText(unrelated, "keep");
            var service = new VNMetaProgressService(repository);

            Assert.That(service.TryUnlockCG("will_not_commit"), Is.False);
            Assert.That(service.Current.unlockedCGs, Is.Empty);
            Assert.That(File.Exists(unrelated), Is.True);
            AssertNoOwnedTemporaryFiles();
        }

        [Test]
        public void MalformedJson_IsQuarantinedWithExactOriginalBytes()
        {
            var original = new byte[] { 0x7B, 0x20, 0x6E, 0x6F, 0x74, 0x2D, 0x6A, 0x73, 0x6F, 0x6E };
            WriteCanonicalBytes(original);
            var result = repository.Read();
            Assert.That(result.State, Is.EqualTo(VNMetaProgressStorageState.Corrupted));
            Assert.That(result.IsWriteProtected, Is.False);
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            CollectionAssert.AreEqual(original, File.ReadAllBytes(SingleCorruptFile()));
        }

        [Test]
        public void NullCollectionsAndBlankIds_AreRejectedAndQuarantined()
        {
            WriteCanonicalText(BuildJsonWithOverride("unlockedCGs", "null"));
            Assert.That(repository.Read().State, Is.EqualTo(VNMetaProgressStorageState.Corrupted));
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);

            WriteCanonicalText(BuildJsonWithOverride("completedEndings", "[\"   \"]"));
            Assert.That(repository.Read().State, Is.EqualTo(VNMetaProgressStorageState.Corrupted));
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            Assert.That(Directory.GetFiles(temporaryRoot, "meta_progress.json.*.corrupt"), Has.Length.EqualTo(2));
        }

        [TestCaseSource(nameof(RequiredFields))]
        public void Validation_RejectsNullEmptyAndWhitespaceIdsForEveryCollection(string fieldName)
        {
            if (fieldName == "schemaVersion") return;
            foreach (var invalidId in new[] { null, string.Empty, "   " })
            {
                var data = VNMetaProgressDefaults.CreateDefault();
                SetCollection(data, fieldName, new[] { invalidId });
                Assert.That(VNMetaProgressValidation.TryValidate(data, out _), Is.False, fieldName + " must reject invalid IDs.");
            }

            var nullCollection = VNMetaProgressDefaults.CreateDefault();
            SetCollection(nullCollection, fieldName, null);
            Assert.That(VNMetaProgressValidation.TryValidate(nullCollection, out _), Is.False, fieldName + " must reject null collections.");
        }

        [TestCaseSource(nameof(RequiredFields))]
        public void EveryRequiredSchemaV1RootField_MustBePresent(string missingField)
        {
            var original = BuildJsonWithout(missingField);
            WriteCanonicalText(original);
            var result = repository.Read();
            Assert.That(result.State, Is.EqualTo(VNMetaProgressStorageState.Corrupted));
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
            Assert.That(File.ReadAllText(SingleCorruptFile()), Is.EqualTo(original));
        }

        [Test]
        public void FutureSchema_IsPreservedAndBlocksWrites()
        {
            var original = "{\"schemaVersion\":2,\"futureField\":\"preserve-me\"}";
            WriteCanonicalText(original);
            var service = new VNMetaProgressService(repository);
            AssertAllEmpty(service.Load());
            Assert.That(service.IsWriteProtected, Is.True);
            Assert.That(service.TryUnlockCG("cg_a"), Is.False);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));
            Assert.That(Directory.GetFiles(temporaryRoot, "meta_progress.json.*.corrupt"), Is.Empty);
        }

        [Test]
        public void FailedQuarantine_PreservesAuthoritativeFileAndBlocksWrites()
        {
            const string original = "{ malformed";
            WriteCanonicalText(original);
            using (new FileStream(repository.CanonicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var service = new VNMetaProgressService(repository);
                service.Load();
                Assert.That(service.IsWriteProtected, Is.True);
                Assert.That(service.TryUnlockCG("cg_a"), Is.False);
                Assert.That(File.ReadAllText(repository.CanonicalFilePath), Is.EqualTo(original));
            }
        }

        [Test]
        public void ExtraSchemaV1RootFields_AreAccepted()
        {
            WriteCanonicalText(BuildJsonWithExtra("\"futureField\":\"ignored\""));
            var result = repository.Read();
            Assert.That(result.State, Is.EqualTo(VNMetaProgressStorageState.Valid));
            Assert.That(result.Data.schemaVersion, Is.EqualTo(1));
            Assert.That(Directory.GetFiles(temporaryRoot, "meta_progress.json.*.corrupt"), Is.Empty);

            var service = new VNMetaProgressService(repository);
            service.Load();
            Assert.That(service.TryUnlockCG("cg_after_load"), Is.True);
            Assert.That(File.ReadAllText(repository.CanonicalFilePath), Does.Not.Contain("futureField"));
        }

        [Test]
        public void InvalidIds_AreRejectedForEveryMutationWithoutWriteOrStateChange()
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = new VNMetaProgressService(repository);
            service.Load();
            var mutations = new Action<string>[]
            {
                id => service.TryRecordReadLine(id), id => service.TryUnlockCG(id), id => service.TryUnlockChapter(id),
                id => service.TryUnlockArchiveEntry(id), id => service.TryUnlockAchievement(id), id => service.TryCompleteEnding(id),
            };
            foreach (var mutation in mutations)
            {
                mutation(null);
                mutation(string.Empty);
                mutation("   ");
            }

            Assert.That(writes, Is.EqualTo(0));
            AssertAllEmpty(service.Current);
        }

        [Test]
        public void SnapshotsAndInputData_DoNotRetainAuthoritativeCollectionReferences()
        {
            var source = VNMetaProgressDefaults.CreateDefault();
            source.readLineIds = new[] { "source" };
            Assert.That(repository.Write(source).Succeeded, Is.True);
            source.readLineIds[0] = "mutated-input";

            var service = new VNMetaProgressService(repository);
            var snapshot = service.Load();
            snapshot.readLineIds[0] = "mutated-snapshot";
            Assert.That(service.Current.readLineIds[0], Is.EqualTo("source"));
            Assert.That(repository.Read().Data.readLineIds[0], Is.EqualTo("source"));
        }

        [Test]
        public void ProductionRoot_IsDedicatedToMetaProgress()
        {
            Assert.That(VNMetaProgressRepository.ProductionStorageRoot, Is.EqualTo(Path.Combine(Application.persistentDataPath, "MetaProgress")));
            Assert.That(repository.StorageRoot, Is.EqualTo(Path.GetFullPath(temporaryRoot)));
            Assert.That(repository.StorageRoot, Is.Not.EqualTo(VNMetaProgressRepository.ProductionStorageRoot));
        }

        private static void AssertAllEmpty(VNMetaProgressData data)
        {
            Assert.That(data.readLineIds, Is.Empty);
            Assert.That(data.unlockedCGs, Is.Empty);
            Assert.That(data.unlockedChapters, Is.Empty);
            Assert.That(data.unlockedArchiveEntries, Is.Empty);
            Assert.That(data.unlockedAchievements, Is.Empty);
            Assert.That(data.completedEndings, Is.Empty);
        }

        private void WriteCanonicalText(string contents)
        {
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(repository.CanonicalFilePath, contents, new System.Text.UTF8Encoding(false));
        }

        private void WriteCanonicalBytes(byte[] contents)
        {
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllBytes(repository.CanonicalFilePath, contents);
        }

        private string SingleCorruptFile()
        {
            var files = Directory.GetFiles(temporaryRoot, "meta_progress.json.*.corrupt");
            Assert.That(files, Has.Length.EqualTo(1));
            return files[0];
        }

        private void AssertNoOwnedTemporaryFiles()
        {
            Assert.That(Directory.GetFiles(temporaryRoot, "meta_progress.json.*.tmp"), Is.Empty);
        }

        private static string BuildJsonWithout(string omittedField)
        {
            var properties = new List<string>();
            foreach (var field in RequiredFields)
            {
                if (field == omittedField) continue;
                properties.Add(field == "schemaVersion" ? "\"schemaVersion\":1" : "\"" + field + "\":[]");
            }

            return "{" + string.Join(",", properties) + "}";
        }

        private static string BuildJsonWithOverride(string fieldName, string value)
        {
            var properties = RequiredFields.Select(field => field == "schemaVersion"
                ? "\"schemaVersion\":1"
                : "\"" + field + "\":" + (field == fieldName ? value : "[]"));
            return "{" + string.Join(",", properties) + "}";
        }

        private static string BuildJsonWithExtra(string extraProperty)
        {
            return BuildJsonWithout(null).TrimEnd('}') + "," + extraProperty + "}";
        }

        private static void SetCollection(VNMetaProgressData data, string fieldName, string[] values)
        {
            switch (fieldName)
            {
                case "readLineIds": data.readLineIds = values; break;
                case "unlockedCGs": data.unlockedCGs = values; break;
                case "unlockedChapters": data.unlockedChapters = values; break;
                case "unlockedArchiveEntries": data.unlockedArchiveEntries = values; break;
                case "unlockedAchievements": data.unlockedAchievements = values; break;
                case "completedEndings": data.completedEndings = values; break;
                default: throw new ArgumentOutOfRangeException(nameof(fieldName));
            }
        }
    }
}
