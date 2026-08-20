using System;
using System.IO;
using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNSaveLoadUiTests
    {
        private string temporaryRoot;
        private VNSaveRepository repository;
        private readonly VNThumbnailService thumbnails = new();

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M5UiTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSaveRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void ManualPages_MapPhysicalIndicesAndDisplayOneBasedLabels()
        {
            var inspections = repository.InspectAllSlots();
            var first = VNSaveLoadSlotModelBuilder.Build(inspections, VNSaveLoadCategory.Manual, 0);
            var second = VNSaveLoadSlotModelBuilder.Build(inspections, VNSaveLoadCategory.Manual, 1);

            Assert.That(first, Has.Count.EqualTo(6));
            Assert.That(first[0].SlotKey, Is.EqualTo(new VNSaveSlotKey(VNSaveSlotType.Manual, 0)));
            Assert.That(first[5].SlotKey, Is.EqualTo(new VNSaveSlotKey(VNSaveSlotType.Manual, 5)));
            Assert.That(first[0].SlotLabel, Is.EqualTo("1"));
            Assert.That(first[5].SlotLabel, Is.EqualTo("6"));
            Assert.That(second, Has.Count.EqualTo(6));
            Assert.That(second[0].SlotKey, Is.EqualTo(new VNSaveSlotKey(VNSaveSlotType.Manual, 6)));
            Assert.That(second[5].SlotKey, Is.EqualTo(new VNSaveSlotKey(VNSaveSlotType.Manual, 11)));
            Assert.That(second[0].SlotLabel, Is.EqualTo("7"));
            Assert.That(second[5].SlotLabel, Is.EqualTo("12"));
            Assert.That(VNSaveLoadSlotModelBuilder.ClampPage(VNSaveLoadCategory.Manual, -5), Is.EqualTo(0));
            Assert.That(VNSaveLoadSlotModelBuilder.ClampPage(VNSaveLoadCategory.Manual, 99), Is.EqualTo(1));
        }

        [Test]
        public void AutoModels_AreNewestFirstThenAllNonValidPhysicalSlots()
        {
            WriteValid(new VNSaveSlotKey(VNSaveSlotType.Auto, 1), "2026-08-19T01:00:00.0000000+00:00");
            WriteValid(new VNSaveSlotKey(VNSaveSlotType.Auto, 3), "2026-08-19T03:00:00.0000000+00:00");
            WriteRaw(new VNSaveSlotKey(VNSaveSlotType.Auto, 2), "{ broken json");
            WriteRaw(new VNSaveSlotKey(VNSaveSlotType.Auto, 4), "{\"schemaVersion\":2}");

            var models = VNSaveLoadSlotModelBuilder.Build(repository.InspectAllSlots(), VNSaveLoadCategory.Auto, 0, TimeZoneInfo.Utc);
            Assert.That(models, Has.Count.EqualTo(5));
            Assert.That(models[0].SlotKey.SlotIndex, Is.EqualTo(3));
            Assert.That(models[1].SlotKey.SlotIndex, Is.EqualTo(1));
            Assert.That(models[2].SlotKey.SlotIndex, Is.EqualTo(0));
            Assert.That(models[3].SlotKey.SlotIndex, Is.EqualTo(2));
            Assert.That(models[3].State, Is.EqualTo(VNSaveSlotState.Corrupted));
            Assert.That(models[4].SlotKey.SlotIndex, Is.EqualTo(4));
            Assert.That(models[4].State, Is.EqualTo(VNSaveSlotState.Unsupported));
        }

        [Test]
        public void QuickModelsAndInteractionPolicy_RespectManualAutoQuickRules()
        {
            var quick = VNSaveLoadSlotModelBuilder.Build(repository.InspectAllSlots(), VNSaveLoadCategory.Quick, 99);
            Assert.That(quick, Has.Count.EqualTo(1));
            Assert.That(quick[0].SlotKey, Is.EqualTo(new VNSaveSlotKey(VNSaveSlotType.Quick, 0)));
            Assert.That(VNSaveLoadSlotModelBuilder.ClampPage(VNSaveLoadCategory.Auto, 3), Is.EqualTo(0));
            Assert.That(VNSaveLoadSlotModelBuilder.ClampPage(VNSaveLoadCategory.Quick, -2), Is.EqualTo(0));

            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Save, VNSaveLoadCategory.Manual, VNSaveSlotState.Empty), Is.EqualTo(VNSaveSlotInteraction.WriteManual));
            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Save, VNSaveLoadCategory.Manual, VNSaveSlotState.Valid), Is.EqualTo(VNSaveSlotInteraction.ConfirmManualOverwrite));
            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Save, VNSaveLoadCategory.Manual, VNSaveSlotState.Corrupted), Is.EqualTo(VNSaveSlotInteraction.Disabled));
            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Save, VNSaveLoadCategory.Auto, VNSaveSlotState.Empty), Is.EqualTo(VNSaveSlotInteraction.Disabled));
            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Save, VNSaveLoadCategory.Quick, VNSaveSlotState.Unsupported), Is.EqualTo(VNSaveSlotInteraction.WriteQuick));
            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Load, VNSaveLoadCategory.Manual, VNSaveSlotState.Valid), Is.EqualTo(VNSaveSlotInteraction.Load));
            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Load, VNSaveLoadCategory.Manual, VNSaveSlotState.Empty), Is.EqualTo(VNSaveSlotInteraction.Disabled));
            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Load, VNSaveLoadCategory.Manual, VNSaveSlotState.Corrupted), Is.EqualTo(VNSaveSlotInteraction.Disabled));
            Assert.That(VNSaveLoadInteractionPolicy.GetInteraction(VNSaveLoadMode.Load, VNSaveLoadCategory.Manual, VNSaveSlotState.Unsupported), Is.EqualTo(VNSaveSlotInteraction.Disabled));
        }

        [Test]
        public void MetadataFormat_UsesExplicitTimezoneAndUnwrappedHours()
        {
            Assert.That(VNSaveLoadSlotModelBuilder.FormatSavedAtLocal("2026-08-19T01:02:03.0000000+00:00", TimeZoneInfo.Utc), Is.EqualTo("2026-08-19 01:02:03"));
            Assert.That(VNSaveLoadSlotModelBuilder.FormatSavedAtLocal("invalid", TimeZoneInfo.Utc), Is.EqualTo("—"));
            Assert.That(VNSaveLoadSlotModelBuilder.FormatPlayedTime((27 * 3600) + (15 * 60) + 4.9f), Is.EqualTo("27:15:04"));
            Assert.That(VNSaveLoadSlotModelBuilder.FormatPlayedTime(float.NaN), Is.EqualTo("00:00:00"));
        }

        [Test]
        public void ThumbnailJpgSidecar_IsCanonicalOptionalAndDoesNotInvalidateJson()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 3);
            Assert.That(thumbnails.TryGetCanonicalFileName(key, out var filename), Is.True);
            Assert.That(filename, Is.EqualTo("manual_03.jpg"));
            Assert.That(repository.TryGetThumbnailSidecarPath(key, "../manual_03.jpg", out _), Is.False);

            var data = CreateValidData(key);
            data.thumbnailFileName = filename;
            Assert.That(repository.Write(key, data).Succeeded, Is.True);
            Assert.That(thumbnails.LoadThumbnail(repository, key, filename).Status, Is.EqualTo(VNThumbnailLoadStatus.Placeholder));
            Assert.That(thumbnails.WriteJpgSidecar(repository, key, Array.Empty<byte>()).Succeeded, Is.False);
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Valid), "A decorative thumbnail failure must not invalidate authoritative JSON.");

            var source = new Texture2D(4, 4, TextureFormat.RGB24, false);
            source.SetPixel(0, 0, Color.red);
            source.Apply();
            var jpg = source.EncodeToJPG(VNThumbnailService.JpegQuality);
            UnityEngine.Object.DestroyImmediate(source);
            Assert.That(thumbnails.WriteJpgSidecar(repository, key, jpg).Succeeded, Is.True);
            var loaded = thumbnails.LoadThumbnail(repository, key, filename);
            Assert.That(loaded.Status, Is.EqualTo(VNThumbnailLoadStatus.Loaded));
            Assert.That(loaded.Texture, Is.Not.Null);
            UnityEngine.Object.DestroyImmediate(loaded.Texture);

            Assert.That(repository.TryGetThumbnailSidecarPath(key, filename, out var path), Is.True);
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
            Assert.That(thumbnails.LoadThumbnail(repository, key, filename).Status, Is.EqualTo(VNThumbnailLoadStatus.Placeholder));
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Valid));
            Assert.That(thumbnails.TryRemoveJpgSidecar(repository, key), Is.True);
            Assert.That(File.Exists(path), Is.False);
        }

        [Test]
        public void ThumbnailCrop_PreservesAspectByCenterCropping()
        {
            VNThumbnailService.GetCenterCropScaleAndOffset(1920, 1080, out var widescale, out var wideOffset);
            Assert.That(widescale, Is.EqualTo(Vector2.one));
            Assert.That(wideOffset, Is.EqualTo(Vector2.zero));

            VNThumbnailService.GetCenterCropScaleAndOffset(1600, 900, out var exactScale, out var exactOffset);
            Assert.That(exactScale, Is.EqualTo(Vector2.one));
            Assert.That(exactOffset, Is.EqualTo(Vector2.zero));

            VNThumbnailService.GetCenterCropScaleAndOffset(1600, 1200, out var tallScale, out var tallOffset);
            Assert.That(tallScale.x, Is.EqualTo(1f));
            Assert.That(tallScale.y, Is.LessThan(1f));
            Assert.That(tallOffset.y, Is.GreaterThan(0f));
        }

        [Test]
        public void AutosaveGuard_ConsumesOnlyOneMatchingLoadResumeCheckpoint()
        {
            var guard = new VNCheckpointAutosaveGuard();
            guard.ExpectLoadedCheckpoint("m5_checkpoint");
            Assert.That(guard.ConsumeIfExpected("different_checkpoint"), Is.False);
            Assert.That(guard.ConsumeIfExpected("m5_checkpoint"), Is.True);
            Assert.That(guard.ConsumeIfExpected("m5_checkpoint"), Is.False, "A later genuine checkpoint must autosave normally.");
            guard.ExpectLoadedCheckpoint("m5_checkpoint");
            guard.Clear();
            Assert.That(guard.ConsumeIfExpected("m5_checkpoint"), Is.False);
        }

        private void WriteValid(VNSaveSlotKey key, string timestamp)
        {
            Assert.That(repository.Write(key, CreateValidData(key, timestamp)).Succeeded, Is.True);
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
                checkpointId = "m5_checkpoint",
                resumeNode = "M1_RUNTIME_START",
                yarnVariables = new YarnVariablesData { floats = Array.Empty<FloatVariableEntry>(), strings = Array.Empty<StringVariableEntry>(), bools = Array.Empty<BoolVariableEntry>() },
                presentationState = new PresentationState { backgroundId = string.Empty, cgId = string.Empty, characters = Array.Empty<CharacterSaveState>() },
                audioState = new AudioState { bgmId = string.Empty, playbackSeconds = 0f },
                chapterId = "m5_chapter",
                sceneTitle = "M5 UI test",
                playedSeconds = 4f,
                savedAtUtcIso8601 = timestamp,
                thumbnailFileName = string.Empty,
            };
        }
    }
}
