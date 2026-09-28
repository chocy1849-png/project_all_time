using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNMetaProgressChangeTests
    {
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private int writes;

        [SetUp]
        public void SetUp()
        {
            writes = 0;
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M9MetaProgressChangeTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void NewMutations_PublishImmutableCategoriesAfterCommit_AndDuplicatesOrLoadDoNotPublish()
        {
            var changes = new List<VNMetaProgressChange>();
            var committedStateWasVisible = new List<bool>();
            metaProgress.ProgressChanged += change =>
            {
                changes.Add(change);
                committedStateWasVisible.Add(IsChangeCommitted(change));
            };

            Assert.That(metaProgress.TryRecordReadLine("line:event"), Is.True);
            Assert.That(metaProgress.TryUnlockCG("cg:event"), Is.True);
            Assert.That(metaProgress.TryUnlockChapter("chapter:event"), Is.True);
            Assert.That(metaProgress.TryUnlockArchiveEntry("archive:event"), Is.True);
            Assert.That(metaProgress.TryUnlockAchievement("achievement:event"), Is.True);
            Assert.That(metaProgress.TryCompleteEnding("ending:event"), Is.True);

            CollectionAssert.AreEqual(
                new[]
                {
                    VNMetaProgressChangeKind.ReadLine,
                    VNMetaProgressChangeKind.CG,
                    VNMetaProgressChangeKind.Chapter,
                    VNMetaProgressChangeKind.ArchiveEntry,
                    VNMetaProgressChangeKind.Achievement,
                    VNMetaProgressChangeKind.Ending,
                },
                changes.Select(change => change.Kind));
            CollectionAssert.AreEqual(
                new[] { "line:event", "cg:event", "chapter:event", "archive:event", "achievement:event", "ending:event" },
                changes.Select(change => change.Id));
            CollectionAssert.AreEqual(Enumerable.Repeat(true, 6), committedStateWasVisible,
                "Subscribers must observe the new state after the service commits it.");

            Assert.That(changes.All(change => change != null), Is.True);
            Assert.That(typeof(VNMetaProgressChange).IsSealed, Is.True);
            Assert.That(typeof(VNMetaProgressChange).GetProperty(nameof(VNMetaProgressChange.Kind)).CanWrite, Is.False);
            Assert.That(typeof(VNMetaProgressChange).GetProperty(nameof(VNMetaProgressChange.Id)).CanWrite, Is.False);
            Assert.That(typeof(VNMetaProgressChange).GetFields(BindingFlags.Instance | BindingFlags.Public), Is.Empty);

            var eventCountAfterNewMutations = changes.Count;
            var writesAfterNewMutations = writes;
            Assert.That(metaProgress.TryRecordReadLine("line:event"), Is.True);
            Assert.That(metaProgress.TryUnlockCG("cg:event"), Is.True);
            Assert.That(metaProgress.TryUnlockChapter("chapter:event"), Is.True);
            Assert.That(metaProgress.TryUnlockArchiveEntry("archive:event"), Is.True);
            Assert.That(metaProgress.TryUnlockAchievement("achievement:event"), Is.True);
            Assert.That(metaProgress.TryCompleteEnding("ending:event"), Is.True);
            Assert.That(metaProgress.TryUnlockCG("  "), Is.False);
            Assert.That(metaProgress.TryCompleteEnding(null), Is.False);
            metaProgress.Load();

            Assert.That(changes.Count, Is.EqualTo(eventCountAfterNewMutations));
            Assert.That(writes, Is.EqualTo(writesAfterNewMutations));
        }

        [Test]
        public void InvalidCurrentCandidate_FailsCanonicalValidationWithoutWritingOrPublishing()
        {
            var eventCount = 0;
            metaProgress.ProgressChanged += _ => eventCount++;
            var currentField = typeof(VNMetaProgressService).GetField("current", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(currentField, Is.Not.Null);
            var current = (VNMetaProgressData)currentField.GetValue(metaProgress);
            current.readLineIds = null;
            var writesBeforeMutation = writes;

            Assert.That(metaProgress.TryUnlockCG("cg:canonical_failure"), Is.False);
            Assert.That(metaProgress.IsCGUnlocked("cg:canonical_failure"), Is.False);
            Assert.That(writes, Is.EqualTo(writesBeforeMutation));
            Assert.That(eventCount, Is.Zero);
        }

        [Test]
        public void FailedAndWriteProtectedMutations_DoNotPublishOrCommit()
        {
            var eventCount = 0;
            metaProgress.ProgressChanged += _ => eventCount++;
            Directory.CreateDirectory(repository.CanonicalFilePath);

            Assert.That(metaProgress.TryUnlockCG("cg:io_failure"), Is.False);
            Assert.That(metaProgress.IsCGUnlocked("cg:io_failure"), Is.False);
            Assert.That(eventCount, Is.Zero);

            var protectedRoot = Path.Combine(temporaryRoot, "future_schema");
            Directory.CreateDirectory(protectedRoot);
            var protectedRepository = VNMetaProgressRepository.CreateForTesting(protectedRoot);
            File.WriteAllText(protectedRepository.CanonicalFilePath, "{\"schemaVersion\":2,\"futureField\":\"keep\"}");
            var protectedService = new VNMetaProgressService(protectedRepository);
            var protectedEventCount = 0;
            protectedService.ProgressChanged += _ => protectedEventCount++;
            protectedService.Load();

            Assert.That(protectedService.IsWriteProtected, Is.True);
            Assert.That(protectedService.TryUnlockAchievement("achievement:blocked"), Is.False);
            Assert.That(protectedService.IsAchievementUnlocked("achievement:blocked"), Is.False);
            Assert.That(protectedEventCount, Is.Zero);
            Assert.That(File.ReadAllText(protectedRepository.CanonicalFilePath),
                Is.EqualTo("{\"schemaVersion\":2,\"futureField\":\"keep\"}"));
        }

        [Test]
        public void ObserverException_DoesNotChangeSuccessfulMutationResultOrStopOtherObservers()
        {
            var laterObserverCalls = 0;
            metaProgress.ProgressChanged += _ => throw new InvalidOperationException("observer test failure");
            metaProgress.ProgressChanged += _ => laterObserverCalls++;

            Assert.That(metaProgress.TryUnlockCG("cg:observer_failure"), Is.True);
            Assert.That(metaProgress.IsCGUnlocked("cg:observer_failure"), Is.True);
            Assert.That(laterObserverCalls, Is.EqualTo(1));
            Assert.That(metaProgress.LastDiagnostic, Does.Contain("observer failed after the committed mutation"));
        }

        private bool IsChangeCommitted(VNMetaProgressChange change)
        {
            switch (change.Kind)
            {
                case VNMetaProgressChangeKind.ReadLine: return metaProgress.IsLineRead(change.Id);
                case VNMetaProgressChangeKind.CG: return metaProgress.IsCGUnlocked(change.Id);
                case VNMetaProgressChangeKind.Chapter: return metaProgress.IsChapterUnlocked(change.Id);
                case VNMetaProgressChangeKind.ArchiveEntry: return metaProgress.IsArchiveEntryUnlocked(change.Id);
                case VNMetaProgressChangeKind.Achievement: return metaProgress.IsAchievementUnlocked(change.Id);
                case VNMetaProgressChangeKind.Ending: return metaProgress.IsEndingCompleted(change.Id);
                default: return false;
            }
        }
    }
}
