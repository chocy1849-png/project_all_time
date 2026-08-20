using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNSaveRepositoryTests
    {
        private string temporaryRoot;
        private VNSaveRepository repository;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M5StorageTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSaveRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void ManualSlotPaths_MapBoundaryIndices_AndRejectInvalidIndices()
        {
            AssertPath(new VNSaveSlotKey(VNSaveSlotType.Manual, 0), "manual_00.json");
            AssertPath(new VNSaveSlotKey(VNSaveSlotType.Manual, 11), "manual_11.json");

            Assert.False(repository.TryGetSlotPath(new VNSaveSlotKey(VNSaveSlotType.Manual, -1), out _));
            Assert.False(repository.TryGetSlotPath(new VNSaveSlotKey(VNSaveSlotType.Manual, 12), out _));
            Assert.That(repository.Read(new VNSaveSlotKey(VNSaveSlotType.Manual, -1)).State, Is.EqualTo(VNSaveSlotState.InvalidRequest));
        }

        [Test]
        public void AutoAndQuickSlotPaths_RespectTheirFixedRanges()
        {
            AssertPath(new VNSaveSlotKey(VNSaveSlotType.Auto, 0), "auto_00.json");
            AssertPath(new VNSaveSlotKey(VNSaveSlotType.Auto, 4), "auto_04.json");
            Assert.False(repository.TryGetSlotPath(new VNSaveSlotKey(VNSaveSlotType.Auto, 5), out _));

            AssertPath(new VNSaveSlotKey(VNSaveSlotType.Quick, 0), "quick_00.json");
            Assert.False(repository.TryGetSlotPath(new VNSaveSlotKey(VNSaveSlotType.Quick, 1), out _));
        }

        [Test]
        public void FullSaveData_RoundTripsEveryM5StorageField()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 3);
            var source = CreateValidData(key, "2026-08-19T01:02:03.0000000+00:00");
            source.thumbnailFileName = "manual_03_preview.webp";

            Assert.That(repository.Write(key, source).Succeeded, Is.True);
            var result = repository.Read(key);

            Assert.That(result.State, Is.EqualTo(VNSaveSlotState.Valid));
            Assert.That(result.SaveData.schemaVersion, Is.EqualTo(1));
            Assert.That(result.SaveData.slotType, Is.EqualTo("manual"));
            Assert.That(result.SaveData.slotIndex, Is.EqualTo(3));
            Assert.That(result.SaveData.checkpointId, Is.EqualTo("chapter_one_checkpoint"));
            Assert.That(result.SaveData.resumeNode, Is.EqualTo("chapter_one_resume"));
            Assert.That(result.SaveData.yarnVariables.floats[0].name, Is.EqualTo("$trust"));
            Assert.That(result.SaveData.yarnVariables.floats[0].value, Is.EqualTo(2.5f));
            Assert.That(result.SaveData.yarnVariables.strings[0].name, Is.EqualTo("$route"));
            Assert.That(result.SaveData.yarnVariables.strings[0].value, Is.EqualTo("kind"));
            Assert.That(result.SaveData.yarnVariables.bools[0].name, Is.EqualTo("$met_before"));
            Assert.That(result.SaveData.yarnVariables.bools[0].value, Is.True);
            Assert.That(result.SaveData.presentationState.backgroundId, Is.EqualTo("library_day"));
            Assert.That(result.SaveData.presentationState.cgId, Is.EqualTo("chapter_one_cg"));
            Assert.That(result.SaveData.presentationState.characters, Has.Length.EqualTo(1));
            Assert.That(result.SaveData.presentationState.characters[0].characterId, Is.EqualTo("heroine"));
            Assert.That(result.SaveData.presentationState.characters[0].expressionId, Is.EqualTo("smile"));
            Assert.That(result.SaveData.presentationState.characters[0].slot, Is.EqualTo("left"));
            Assert.That(result.SaveData.presentationState.characters[0].facing, Is.EqualTo("right"));
            Assert.That(result.SaveData.presentationState.characters[0].scale, Is.EqualTo(1.1f));
            Assert.That(result.SaveData.audioState.bgmId, Is.EqualTo("day_theme"));
            Assert.That(result.SaveData.audioState.playbackSeconds, Is.EqualTo(42.25f));
            Assert.That(result.SaveData.chapterId, Is.EqualTo("chapter_one"));
            Assert.That(result.SaveData.sceneTitle, Is.EqualTo("A Quiet Beginning"));
            Assert.That(result.SaveData.playedSeconds, Is.EqualTo(123.5f));
            Assert.That(result.SaveData.savedAtUtcIso8601, Is.EqualTo("2026-08-19T01:02:03.0000000+00:00"));
            Assert.That(result.SaveData.thumbnailFileName, Is.EqualTo("manual_03_preview.webp"));
        }

        [Test]
        public void FirstWrite_CreatesAuthoritativeFile_AndOverwriteReplacesIt()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);
            var first = CreateValidData(key, "2026-08-19T01:00:00.0000000+00:00");
            Assert.That(repository.Write(key, first).Succeeded, Is.True);
            Assert.That(repository.TryGetSlotPath(key, out var path), Is.True);
            Assert.That(File.Exists(path), Is.True);

            var replacement = CreateValidData(key, "2026-08-19T02:00:00.0000000+00:00");
            replacement.chapterId = "replacement_chapter";
            replacement.sceneTitle = "Replacement";
            Assert.That(repository.Write(key, replacement).Succeeded, Is.True);

            var result = repository.Read(key);
            Assert.That(result.State, Is.EqualTo(VNSaveSlotState.Valid));
            Assert.That(result.SaveData.chapterId, Is.EqualTo("replacement_chapter"));
            Assert.That(result.SaveData.sceneTitle, Is.EqualTo("Replacement"));
        }

        [Test]
        public void RejectedOverwrite_LeavesKnownGoodAuthoritativeSaveReadable()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 1);
            var valid = CreateValidData(key, "2026-08-19T01:00:00.0000000+00:00");
            Assert.That(repository.Write(key, valid).Succeeded, Is.True);

            var invalid = CreateValidData(key, "2026-08-19T02:00:00.0000000+00:00");
            invalid.playedSeconds = float.NaN;
            Assert.That(repository.Write(key, invalid).Succeeded, Is.False);

            var result = repository.Read(key);
            Assert.That(result.State, Is.EqualTo(VNSaveSlotState.Valid));
            Assert.That(result.SaveData.savedAtUtcIso8601, Is.EqualTo(valid.savedAtUtcIso8601));
        }

        [Test]
        public void Delete_MakesOccupiedSlotEmpty()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 2);
            Assert.That(repository.Write(key, CreateValidData(key)).Succeeded, Is.True);
            Assert.That(repository.Delete(key).Succeeded, Is.True);
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Empty));
        }

        [Test]
        public void MalformedAndMissingFieldJson_AreCorrupted_WithoutAffectingOtherSlots()
        {
            var malformedKey = new VNSaveSlotKey(VNSaveSlotType.Manual, 4);
            var missingFieldsKey = new VNSaveSlotKey(VNSaveSlotType.Manual, 5);
            var validKey = new VNSaveSlotKey(VNSaveSlotType.Manual, 6);
            WriteRaw(malformedKey, "{ not valid json");
            WriteRaw(missingFieldsKey, "{\"schemaVersion\":1,\"slotType\":\"manual\",\"slotIndex\":5}");
            Assert.That(repository.Write(validKey, CreateValidData(validKey)).Succeeded, Is.True);

            Assert.That(repository.Read(malformedKey).State, Is.EqualTo(VNSaveSlotState.Corrupted));
            Assert.That(repository.Read(missingFieldsKey).State, Is.EqualTo(VNSaveSlotState.Corrupted));
            Assert.That(repository.Read(validKey).State, Is.EqualTo(VNSaveSlotState.Valid));
        }

        [Test]
        public void FutureSchema_IsUnsupported_AndStoredKeyMismatchIsCorrupted()
        {
            var unsupportedKey = new VNSaveSlotKey(VNSaveSlotType.Manual, 7);
            WriteRaw(unsupportedKey, "{\"schemaVersion\":2}");
            Assert.That(repository.Read(unsupportedKey).State, Is.EqualTo(VNSaveSlotState.Unsupported));

            var requestedKey = new VNSaveSlotKey(VNSaveSlotType.Manual, 8);
            var mismatchedData = CreateValidData(new VNSaveSlotKey(VNSaveSlotType.Manual, 9));
            WriteRaw(requestedKey, JsonUtility.ToJson(mismatchedData));
            Assert.That(repository.Read(requestedKey).State, Is.EqualTo(VNSaveSlotState.Corrupted));
        }

        [Test]
        public void InvalidNumbersTimestampAndUnsafeThumbnail_AreRejected()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 10);

            var invalidFloat = CreateValidData(key);
            invalidFloat.yarnVariables.floats[0].value = float.PositiveInfinity;
            Assert.That(repository.Write(key, invalidFloat).Succeeded, Is.False);

            var invalidScale = CreateValidData(key);
            invalidScale.presentationState.characters[0].scale = 0f;
            Assert.That(repository.Write(key, invalidScale).Succeeded, Is.False);

            var invalidAudioTime = CreateValidData(key);
            invalidAudioTime.audioState.playbackSeconds = float.NaN;
            Assert.That(repository.Write(key, invalidAudioTime).Succeeded, Is.False);

            var invalidTimestamp = CreateValidData(key);
            invalidTimestamp.savedAtUtcIso8601 = "2026-08-19";
            Assert.That(repository.Write(key, invalidTimestamp).Succeeded, Is.False);

            var unsafeThumbnail = CreateValidData(key);
            unsafeThumbnail.thumbnailFileName = "../outside.png";
            Assert.That(repository.Write(key, unsafeThumbnail).Succeeded, Is.False);
        }

        [Test]
        public void EmptyThumbnail_IsValid_AndNeverRequiresAFile()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 11);
            var data = CreateValidData(key);
            data.thumbnailFileName = string.Empty;

            Assert.That(repository.Write(key, data).Succeeded, Is.True);
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Valid));
        }

        [Test]
        public void AutoAllocation_UsesLowestEmptyThenOldestValidAndPreservesUnsupportedSlots()
        {
            var first = repository.AllocateNextAutoSlot();
            Assert.That(first.Status, Is.EqualTo(VNAutoSlotAllocationStatus.Allocated));
            Assert.That(first.SlotKey.Value, Is.EqualTo(new VNSaveSlotKey(VNSaveSlotType.Auto, 0)));

            for (var index = 0; index < VNSaveSlotKey.AutoSlotCount; index++)
            {
                var key = new VNSaveSlotKey(VNSaveSlotType.Auto, index);
                var timestamp = index == 3
                    ? "2026-08-19T01:00:00.0000000+00:00"
                    : "2026-08-19T0" + (index + 2).ToString(CultureInfo.InvariantCulture) + ":00:00.0000000+00:00";
                Assert.That(repository.Write(key, CreateValidData(key, timestamp)).Succeeded, Is.True);
            }

            var oldest = repository.AllocateNextAutoSlot();
            Assert.That(oldest.Status, Is.EqualTo(VNAutoSlotAllocationStatus.Allocated));
            Assert.That(oldest.SlotKey.Value, Is.EqualTo(new VNSaveSlotKey(VNSaveSlotType.Auto, 3)));

            var unsupportedKey = new VNSaveSlotKey(VNSaveSlotType.Auto, 0);
            WriteRaw(unsupportedKey, "{\"schemaVersion\":2}");
            var candidate = repository.AllocateNextAutoSlot();
            Assert.That(candidate.Status, Is.EqualTo(VNAutoSlotAllocationStatus.Allocated));
            Assert.That(candidate.SlotKey.Value, Is.Not.EqualTo(unsupportedKey));
        }

        [Test]
        public void AutoAllocation_BreaksEqualTimestampTiesByLowestIndex_AndFailsWhenNoSafeCandidateExists()
        {
            const string sharedTimestamp = "2026-08-19T01:00:00.0000000+00:00";
            for (var index = 0; index < VNSaveSlotKey.AutoSlotCount; index++)
            {
                var key = new VNSaveSlotKey(VNSaveSlotType.Auto, index);
                Assert.That(repository.Write(key, CreateValidData(key, sharedTimestamp)).Succeeded, Is.True);
            }

            var tied = repository.AllocateNextAutoSlot();
            Assert.That(tied.Status, Is.EqualTo(VNAutoSlotAllocationStatus.Allocated));
            Assert.That(tied.SlotKey.Value, Is.EqualTo(new VNSaveSlotKey(VNSaveSlotType.Auto, 0)));

            for (var index = 0; index < VNSaveSlotKey.AutoSlotCount; index++)
                WriteRaw(new VNSaveSlotKey(VNSaveSlotType.Auto, index), "{\"schemaVersion\":2}");

            var unavailable = repository.AllocateNextAutoSlot();
            Assert.That(unavailable.Status, Is.EqualTo(VNAutoSlotAllocationStatus.NoSafeCandidate));
            Assert.That(unavailable.SlotKey.HasValue, Is.False);
        }

        [Test]
        public void QuickSlot_AlwaysOverwritesQuickZero()
        {
            var quickKey = new VNSaveSlotKey(VNSaveSlotType.Quick, 0);
            var first = CreateValidData(quickKey, "2026-08-19T01:00:00.0000000+00:00");
            first.chapterId = "first";
            var second = CreateValidData(quickKey, "2026-08-19T02:00:00.0000000+00:00");
            second.chapterId = "second";

            Assert.That(repository.Write(quickKey, first).Succeeded, Is.True);
            Assert.That(repository.Write(quickKey, second).Succeeded, Is.True);
            Assert.That(repository.Read(quickKey).SaveData.chapterId, Is.EqualTo("second"));
        }

        [Test]
        public void StaleOwnedTemporaryFile_IsIgnoredAndRemovedByDelete()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);
            Assert.That(repository.Write(key, CreateValidData(key)).Succeeded, Is.True);
            Assert.That(repository.TryGetSlotPath(key, out var authoritativePath), Is.True);
            var stalePath = authoritativePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(stalePath, "broken temporary content");

            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Valid));
            Assert.That(repository.Delete(key).Succeeded, Is.True);
            Assert.That(File.Exists(stalePath), Is.False);
        }

        [Test]
        public void PlayTimeTracker_RestoresAndAdvancesOnlyFiniteNonNegativeValues()
        {
            var tracker = new VNPlayTimeTracker();
            Assert.That(tracker.TrySetPlayedSeconds(5f), Is.True);
            Assert.That(tracker.TryAdvance(2.5f), Is.True);
            Assert.That(tracker.PlayedSeconds, Is.EqualTo(7.5f));
            Assert.That(tracker.TrySetPlayedSeconds(-1f), Is.False);
            Assert.That(tracker.TryAdvance(float.PositiveInfinity), Is.False);
            Assert.That(tracker.PlayedSeconds, Is.EqualTo(7.5f));
        }

        private void AssertPath(VNSaveSlotKey key, string expectedFileName)
        {
            Assert.That(repository.TryGetSlotPath(key, out var path), Is.True);
            Assert.That(Path.GetFileName(path), Is.EqualTo(expectedFileName));
            Assert.That(Path.GetDirectoryName(path), Is.EqualTo(Path.GetFullPath(temporaryRoot)));
        }

        private void WriteRaw(VNSaveSlotKey key, string json)
        {
            Assert.That(repository.TryGetSlotPath(key, out var path), Is.True);
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(path, json);
        }

        private static SaveSlotData CreateValidData(VNSaveSlotKey key, string timestamp = "2026-08-19T01:00:00.0000000+00:00")
        {
            return new SaveSlotData
            {
                schemaVersion = VNSaveSerializer.CurrentSchemaVersion,
                slotType = key.ToSerializedSlotType(),
                slotIndex = key.SlotIndex,
                checkpointId = "chapter_one_checkpoint",
                resumeNode = "chapter_one_resume",
                yarnVariables = new YarnVariablesData
                {
                    floats = new[] { new FloatVariableEntry { name = "$trust", value = 2.5f } },
                    strings = new[] { new StringVariableEntry { name = "$route", value = "kind" } },
                    bools = new[] { new BoolVariableEntry { name = "$met_before", value = true } },
                },
                presentationState = new PresentationState
                {
                    backgroundId = "library_day",
                    cgId = "chapter_one_cg",
                    characters = new[]
                    {
                        new CharacterSaveState
                        {
                            characterId = "heroine",
                            expressionId = "smile",
                            slot = "left",
                            facing = "right",
                            scale = 1.1f,
                        },
                    },
                },
                audioState = new AudioState { bgmId = "day_theme", playbackSeconds = 42.25f },
                chapterId = "chapter_one",
                sceneTitle = "A Quiet Beginning",
                playedSeconds = 123.5f,
                savedAtUtcIso8601 = timestamp,
                thumbnailFileName = string.Empty,
            };
        }
    }
}
