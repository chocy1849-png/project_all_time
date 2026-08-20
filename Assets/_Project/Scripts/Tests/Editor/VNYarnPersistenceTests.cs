using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNYarnPersistenceTests
    {
        private const string YarnProjectPath = "Assets/_Project/Yarn/GameNarrative.yarnproject";
        private const string CheckpointId = "m5_checkpoint";
        private const string ResumeNode = "M1_RUNTIME_START";

        private string temporaryRoot;
        private VNSaveRepository repository;
        private readonly List<UnityEngine.Object> ownedObjects = new();

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M5YarnTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSaveRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();

            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void Catalog_ResolvesACompleteDefinition_AndChecksItsYarnNode()
        {
            var catalog = CreateCatalog(ValidDefinitionsJson());
            var yarnProject = LoadYarnProject();

            Assert.That(catalog.TryValidate(yarnProject, out var validationDiagnostic), Is.True, validationDiagnostic);
            Assert.That(catalog.TryResolve(CheckpointId, out var context, out var resolveDiagnostic), Is.True, resolveDiagnostic);
            Assert.That(context.CheckpointId, Is.EqualTo(CheckpointId));
            Assert.That(context.ResumeNode, Is.EqualTo(ResumeNode));
            Assert.That(context.ChapterId, Is.EqualTo("m5_chapter"));
            Assert.That(context.SceneTitle, Is.EqualTo("M5 test checkpoint"));
        }

        [Test]
        public void Catalog_RejectsDuplicatesBadIdsAndMissingNodes()
        {
            var duplicate = CreateCatalog("[{\"checkpointId\":\"m5_checkpoint\",\"resumeNode\":\"M1_RUNTIME_START\",\"chapterId\":\"m5_chapter\",\"sceneTitle\":\"One\"},{\"checkpointId\":\"m5_checkpoint\",\"resumeNode\":\"M2_UI_START\",\"chapterId\":\"m5_chapter\",\"sceneTitle\":\"Two\"}]");
            Assert.That(duplicate.TryResolve(CheckpointId, out _, out _), Is.False);

            var badId = CreateCatalog("[{\"checkpointId\":\"Bad-ID\",\"resumeNode\":\"M1_RUNTIME_START\",\"chapterId\":\"m5_chapter\",\"sceneTitle\":\"Bad\"}]");
            Assert.That(badId.TryResolve("Bad-ID", out _, out _), Is.False);

            var missingNode = CreateCatalog("[{\"checkpointId\":\"missing_node\",\"resumeNode\":\"NO_SUCH_NODE\",\"chapterId\":\"m5_chapter\",\"sceneTitle\":\"Missing\"}]");
            Assert.That(missingNode.TryValidate(LoadYarnProject(), out _), Is.False);
        }

        [Test]
        public void CheckpointService_OnlyAdoptsAValidatedCheckpoint_AndNeverStartsDialogue()
        {
            var catalog = CreateCatalog(ValidDefinitionsJson());
            var runner = CreateRunner();
            var service = CreateService(catalog);

            Assert.That(service.HasCurrentCheckpoint, Is.False);
            Assert.That(service.TryEnterCheckpoint(CheckpointId, runner, out var enterDiagnostic), Is.True, enterDiagnostic);
            Assert.That(service.TryGetCurrentCheckpoint(out var current), Is.True);
            Assert.That(current.ResumeNode, Is.EqualTo(ResumeNode));
            Assert.That(runner.IsDialogueRunning, Is.False);

            Assert.That(service.TryEnterCheckpoint("unknown_checkpoint", runner, out _), Is.False);
            Assert.That(service.TryGetCurrentCheckpoint(out var preserved), Is.True);
            Assert.That(preserved.CheckpointId, Is.EqualTo(CheckpointId));
            Assert.That(runner.IsDialogueRunning, Is.False);
        }

        [Test]
        public void YarnVariableSnapshot_CapturesOrdinalOrder_AndRestoreClearsStaleVariables()
        {
            var storage = CreateStorage();
            storage.SetAllVariables(
                new Dictionary<string, float> { ["$z_float"] = 7f, ["$a_float"] = 1f },
                new Dictionary<string, string> { ["$route"] = "kind" },
                new Dictionary<string, bool> { ["$flag"] = true },
                clear: true);

            Assert.That(VNYarnVariableSnapshot.TryCapture(storage, out var snapshot, out var captureDiagnostic), Is.True, captureDiagnostic);
            Assert.That(snapshot.floats[0].name, Is.EqualTo("$a_float"));
            Assert.That(snapshot.floats[1].name, Is.EqualTo("$z_float"));

            storage.SetAllVariables(
                new Dictionary<string, float> { ["$different"] = 99f },
                new Dictionary<string, string> { ["$stale"] = "remove" },
                new Dictionary<string, bool> { ["$stale_flag"] = false },
                clear: true);

            Assert.That(VNYarnVariableSnapshot.TryRestore(storage, snapshot, out var restoreDiagnostic), Is.True, restoreDiagnostic);
            var restored = storage.GetAllVariables();
            Assert.That(restored.Item1.ContainsKey("$a_float"), Is.True);
            Assert.That(restored.Item1.ContainsKey("$z_float"), Is.True);
            Assert.That(restored.Item1.ContainsKey("$different"), Is.False);
            Assert.That(restored.Item2.ContainsKey("$route"), Is.True);
            Assert.That(restored.Item2.ContainsKey("$stale"), Is.False);
            Assert.That(restored.Item3.ContainsKey("$flag"), Is.True);
            Assert.That(restored.Item3.ContainsKey("$stale_flag"), Is.False);
        }

        [Test]
        public void YarnVariableSnapshot_RejectsMalformedDataBeforeStorageMutation()
        {
            var storage = CreateStorage();
            storage.SetAllVariables(
                new Dictionary<string, float> { ["$before"] = 4f },
                new Dictionary<string, string>(),
                new Dictionary<string, bool>(),
                clear: true);
            var malformed = new YarnVariablesData
            {
                floats = new[]
                {
                    new FloatVariableEntry { name = "$duplicate", value = 1f },
                    new FloatVariableEntry { name = "$duplicate", value = 2f },
                },
                strings = Array.Empty<StringVariableEntry>(),
                bools = Array.Empty<BoolVariableEntry>(),
            };

            Assert.That(VNYarnVariableSnapshot.TryRestore(storage, malformed, out _), Is.False);
            var after = storage.GetAllVariables();
            Assert.That(after.Item1.Count, Is.EqualTo(1));
            Assert.That(after.Item1["$before"], Is.EqualTo(4f));
        }

        [Test]
        public void Coordinator_RequiresFullM3M4DependenciesBeforeComposingACompleteSave()
        {
            var catalog = CreateCatalog(ValidDefinitionsJson());
            var runner = CreateRunner();
            var service = CreateService(catalog);
            var tracker = new VNPlayTimeTracker();
            Assert.That(tracker.TrySetPlayedSeconds(42.5f), Is.True);
            var coordinator = new VNYarnSaveCoordinator(repository, service, runner, tracker);
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);

            Assert.That(coordinator.TryComposeTechnicalSave(key, out _, out _), Is.False, "A save is never composed before a checkpoint command is accepted.");
            Assert.That(service.TryEnterCheckpoint(CheckpointId, runner, out var enterDiagnostic), Is.True, enterDiagnostic);
            runner.VariableStorage.SetAllVariables(
                new Dictionary<string, float> { ["$trust"] = 2f },
                new Dictionary<string, string>(),
                new Dictionary<string, bool>(),
                clear: true);

            Assert.That(coordinator.TryComposeTechnicalSave(key, out _, out var composeDiagnostic), Is.False);
            Assert.That(composeDiagnostic, Does.Contain("Presentation Controller"));
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Empty), "Composition does not write user-facing saves.");
        }

        [Test]
        public void LoadValidation_RejectsUnknownCheckpointWithoutMutatingRuntimeState()
        {
            var coordinator = CreateCoordinator(out var service, out var runner);
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 1);
            var saveData = CreateValidSave(key);
            saveData.checkpointId = "unknown_checkpoint";
            Assert.That(repository.Write(key, saveData).Succeeded, Is.True);

            SeedAndEnterKnownCheckpoint(service, runner);
            var result = coordinator.ValidateLoad(key);

            Assert.That(result.Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidCheckpoint));
            AssertRuntimeStatePreserved(service, runner);
        }

        [Test]
        public void LoadValidation_RejectsResumeMismatchAndMissingCatalogNodeBeforeMutation()
        {
            var coordinator = CreateCoordinator(out var service, out var runner);
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 2);
            var mismatch = CreateValidSave(key);
            mismatch.resumeNode = "M2_UI_START";
            Assert.That(repository.Write(key, mismatch).Succeeded, Is.True);

            SeedAndEnterKnownCheckpoint(service, runner);
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidCheckpoint));
            AssertRuntimeStatePreserved(service, runner);

            ConfigureServiceCatalog(service, CreateCatalog("[{\"checkpointId\":\"missing_node\",\"resumeNode\":\"NO_SUCH_NODE\",\"chapterId\":\"m5_chapter\",\"sceneTitle\":\"Missing\"}]"));
            var missingNode = CreateValidSave(key);
            missingNode.checkpointId = "missing_node";
            missingNode.resumeNode = "NO_SUCH_NODE";
            Assert.That(repository.Write(key, missingNode).Succeeded, Is.True);

            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidCheckpoint));
            AssertRuntimeStatePreserved(service, runner);
        }

        [Test]
        public void LoadValidation_RejectsMalformedAndUnsupportedFilesWithoutMutatingRuntimeState()
        {
            var coordinator = CreateCoordinator(out var service, out var runner);
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 3);
            SeedAndEnterKnownCheckpoint(service, runner);

            WriteRaw(key, "{ not valid json");
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.ReadFailed));
            AssertRuntimeStatePreserved(service, runner);

            WriteRaw(key, "{\"schemaVersion\":2}");
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.ReadFailed));
            AssertRuntimeStatePreserved(service, runner);
        }

        private VNYarnSaveCoordinator CreateCoordinator(out VNCheckpointService service, out DialogueRunner runner)
        {
            runner = CreateRunner();
            service = CreateService(CreateCatalog(ValidDefinitionsJson()));
            return new VNYarnSaveCoordinator(repository, service, runner, new VNPlayTimeTracker());
        }

        private void SeedAndEnterKnownCheckpoint(VNCheckpointService service, DialogueRunner runner)
        {
            runner.VariableStorage.SetAllVariables(
                new Dictionary<string, float> { ["$before"] = 8f },
                new Dictionary<string, string> { ["$before_text"] = "keep" },
                new Dictionary<string, bool> { ["$before_flag"] = true },
                clear: true);
            Assert.That(service.TryEnterCheckpoint(CheckpointId, runner, out var diagnostic), Is.True, diagnostic);
        }

        private static void AssertRuntimeStatePreserved(VNCheckpointService service, DialogueRunner runner)
        {
            Assert.That(service.TryGetCurrentCheckpoint(out var checkpoint), Is.True);
            Assert.That(checkpoint.CheckpointId, Is.EqualTo(CheckpointId));
            var variables = runner.VariableStorage.GetAllVariables();
            Assert.That(variables.FloatVariables["$before"], Is.EqualTo(8f));
            Assert.That(variables.StringVariables["$before_text"], Is.EqualTo("keep"));
            Assert.That(variables.BoolVariables["$before_flag"], Is.True);
            Assert.That(runner.IsDialogueRunning, Is.False);
        }

        private DialogueRunner CreateRunner()
        {
            var gameObject = new GameObject("M5 Test Dialogue Runner");
            ownedObjects.Add(gameObject);
            var runner = gameObject.AddComponent<DialogueRunner>();
            runner.SetProject(LoadYarnProject());
            return runner;
        }

        private InMemoryVariableStorage CreateStorage()
        {
            var gameObject = new GameObject("M5 Test Variable Storage");
            ownedObjects.Add(gameObject);
            return gameObject.AddComponent<InMemoryVariableStorage>();
        }

        private VNCheckpointService CreateService(VNCheckpointCatalog catalog)
        {
            var gameObject = new GameObject("M5 Test Checkpoint Service");
            ownedObjects.Add(gameObject);
            var service = gameObject.AddComponent<VNCheckpointService>();
            ConfigureServiceCatalog(service, catalog);
            return service;
        }

        private VNCheckpointCatalog CreateCatalog(string definitionsJson)
        {
            var catalog = ScriptableObject.CreateInstance<VNCheckpointCatalog>();
            JsonUtility.FromJsonOverwrite("{\"checkpointDefinitions\":" + definitionsJson + "}", catalog);
            ownedObjects.Add(catalog);
            return catalog;
        }

        private static void ConfigureServiceCatalog(VNCheckpointService service, VNCheckpointCatalog catalog)
        {
            var field = typeof(VNCheckpointService).GetField("checkpointCatalog", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(service, catalog);
        }

        private YarnProject LoadYarnProject()
        {
            var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>(YarnProjectPath);
            Assert.That(yarnProject, Is.Not.Null, "The existing M1 Yarn Project is required for M5 checkpoint validation tests.");
            return yarnProject;
        }

        private void WriteRaw(VNSaveSlotKey key, string json)
        {
            Assert.That(repository.TryGetSlotPath(key, out var path), Is.True);
            Directory.CreateDirectory(temporaryRoot);
            File.WriteAllText(path, json);
        }

        private static SaveSlotData CreateValidSave(VNSaveSlotKey key)
        {
            return new SaveSlotData
            {
                schemaVersion = VNSaveSerializer.CurrentSchemaVersion,
                slotType = key.ToSerializedSlotType(),
                slotIndex = key.SlotIndex,
                checkpointId = CheckpointId,
                resumeNode = ResumeNode,
                yarnVariables = new YarnVariablesData
                {
                    floats = new[] { new FloatVariableEntry { name = "$loaded", value = 1f } },
                    strings = new[] { new StringVariableEntry { name = "$loaded_text", value = "load" } },
                    bools = new[] { new BoolVariableEntry { name = "$loaded_flag", value = true } },
                },
                presentationState = new PresentationState
                {
                    backgroundId = string.Empty,
                    cgId = string.Empty,
                    characters = Array.Empty<CharacterSaveState>(),
                },
                audioState = new AudioState { bgmId = string.Empty, playbackSeconds = 0f },
                chapterId = "m5_chapter",
                sceneTitle = "M5 test checkpoint",
                playedSeconds = 1f,
                savedAtUtcIso8601 = "2026-08-19T01:00:00.0000000+00:00",
                thumbnailFileName = string.Empty,
            };
        }

        private static string ValidDefinitionsJson()
        {
            return "[{\"checkpointId\":\"m5_checkpoint\",\"resumeNode\":\"M1_RUNTIME_START\",\"chapterId\":\"m5_chapter\",\"sceneTitle\":\"M5 test checkpoint\"}]";
        }
    }
}
