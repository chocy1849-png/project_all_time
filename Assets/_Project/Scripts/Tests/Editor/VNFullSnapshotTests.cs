using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using Yarn.Compiler;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNFullSnapshotTests
    {
        private const string CheckpointId = "m5_checkpoint";
        private const string ResumeNode = "M1_RUNTIME_START";
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNSaveRepository repository;
        private static bool integrationCompleted;
        private static Exception integrationFailure;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M5FullSnapshotTests_" + Guid.NewGuid().ToString("N"));
            repository = VNSaveRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--) UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void ProductionLine_ComposesV2ToDiskAndRejectsMismatchedResumeBeforeMutation()
        {
            var runner = CreateRunner();
            var service = CreateCheckpointService();
            var presentation = CreatePresentationHarness();
            var audio = CreateAudioHarness();
            var sessionObject = new GameObject("Exact line session");
            ownedObjects.Add(sessionObject);
            var session = sessionObject.AddComponent<VNDialogueSessionState>();
            var gate = sessionObject.AddComponent<VNInteractionGate>();
            var linePresenter = sessionObject.AddComponent<LinePresenter>();
            linePresenter.canvasGroup = sessionObject.AddComponent<CanvasGroup>();
            var textObject = new GameObject("Exact line text", typeof(RectTransform), typeof(CanvasRenderer));
            ownedObjects.Add(textObject);
            linePresenter.lineText = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            runner.DialoguePresenters = new DialoguePresenterBase[] { linePresenter };
            var coordinator = new VNYarnSaveCoordinator(repository, service, runner, new VNPlayTimeTracker(),
                presentation.Controller, presentation.Transition, audio.Controller, session, gate);
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);
            Assert.That(service.TryEnterCheckpoint(CheckpointId, runner, out var checkpointDiagnostic), Is.True, checkpointDiagnostic);
            Assert.That(coordinator.TryComposeCompleteSave(key, out _, out var noLine), Is.False);
            Assert.That(noLine, Does.Contain("active dialogue paragraph"));
            SetPrivate(session, "isLineActive", true);
            SetPrivate(session, "currentLineId", "line:prologue_s001_016");
            Assert.That(coordinator.TryWriteCompleteSave(key, out var save).Succeeded, Is.True);
            Assert.That(save.schemaVersion, Is.EqualTo(2));
            Assert.That(save.resumeLineId, Is.EqualTo("line:prologue_s001_016"));
            Assert.That(save.resumeEntryNode, Is.EqualTo("PROLOGUE_LINE_S001_016"));
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Valid));
            var canvasGroup = linePresenter.canvasGroup;
            linePresenter.canvasGroup = null;
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidDependencies));
            Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s001_016"));
            linePresenter.canvasGroup = canvasGroup;
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.Valid));
            Assert.That(save.resumeGraphVersion, Is.EqualTo("cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9"));
            // Persist both historical newline representations through the real repository.
            foreach (var fingerprint in new[] {
                "cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9",
                "9aefad0924fffae848c25a23081b48606a0cfd2c51d637bb6da4386a65883980" })
            {
                save.resumeGraphVersion = fingerprint;
                Assert.That(repository.Write(key, save).Succeeded, Is.True);
                Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.Valid), fingerprint);
                Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s001_016"));
            }
            var controllerObject = new GameObject("M10-15 manual overwrite controller");
            ownedObjects.Add(controllerObject);
            var controller = controllerObject.AddComponent<VNSaveLoadController>();
            SetPrivate(controller, "repository", repository);
            SetPrivate(controller, "saveCoordinator", coordinator);
            SetPrivate(controller, "sessionState", session);
            Assert.That(repository.TryGetSlotPath(key, out var manualPath), Is.True);
            var originalBytes = File.ReadAllBytes(manualPath);
            Assert.That(controller.SaveManual(0).Status, Is.EqualTo(VNSaveLoadOperationStatus.ConfirmationRequired));
            controller.CancelManualOverwrite();
            CollectionAssert.AreEqual(originalBytes, File.ReadAllBytes(manualPath));
            Assert.That(controller.SaveManual(0).Status, Is.EqualTo(VNSaveLoadOperationStatus.ConfirmationRequired));
            Assert.That(controller.ConfirmManualOverwrite().Status, Is.EqualTo(VNSaveLoadOperationStatus.Succeeded));
            Assert.That(repository.Read(key).SaveData.resumeGraphVersion,
                Is.EqualTo("cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9"));
            var program = (byte[])runner.YarnProject.compiledYarnProgram.Clone();
            var manifest = Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST").bytes;
            string Hash(byte[] p, byte[] m)
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                using var stream = new MemoryStream();
                stream.Write(p, 0, p.Length); stream.Write(m, 0, m.Length);
                return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", "").ToLowerInvariant();
            }
            var changedProgram = (byte[])program.Clone(); changedProgram[0] ^= 1;
            var changedManifest = (byte[])manifest.Clone(); changedManifest[changedManifest.Length / 2] ^= 1;
            foreach (var fingerprint in new[] { Hash(changedProgram, manifest), Hash(program, changedManifest), new string('a', 64) })
            {
                save.resumeGraphVersion = fingerprint;
                Assert.That(repository.Write(key, save).Succeeded, Is.True);
                Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidCheckpoint));
                Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s001_016"));
            }
            save.resumeGraphVersion = "cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9";
            save.presentationState.cgId = "missing_cg";
            Assert.That(repository.Write(key, save).Succeeded, Is.True);
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidPresentation));
            Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s001_016"));
            save.presentationState.cgId = string.Empty;
            save.audioState.bgmId = "missing_bgm";
            Assert.That(repository.Write(key, save).Succeeded, Is.True);
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidAudio));
            Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s001_016"));
            save.audioState.bgmId = string.Empty;
            save.yarnVariables.floats = new[] { new FloatVariableEntry { name = "$not_declared", value = 1f } };
            Assert.That(repository.Write(key, save).Succeeded, Is.True);
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidVariables));
            Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s001_016"));
            save.yarnVariables.floats = Array.Empty<FloatVariableEntry>();
            save.resumeEntryNode = "PROLOGUE_S001_P01";
            Assert.That(repository.Write(key, save).Succeeded, Is.True);
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidCheckpoint));
            Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s001_016"));
            Assert.That(repository.TryGetSlotPath(key, out var path), Is.True);
            File.WriteAllText(path, "{\"schemaVersion\":2, broken json");
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.ReadFailed));
            Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s001_016"));
        }

        [UnityTest]
        public IEnumerator ExpandedGraph_ValidatesAndRestoresReconstructedS005SaveThroughCoordinator()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            var fixture = new VNFullSnapshotTests();
            fixture.SetUp();
            var host = new GameObject("M10-15 test coroutine").AddComponent<VNInteractionGate>();
            integrationCompleted = false;
            integrationFailure = null;
            host.StartCoroutine(RunM1015Fixture(fixture, SetM1015Result));
            yield return WaitForM1015(IsM1015Finished, "integration coroutine");
            UnityEngine.Object.Destroy(host.gameObject);
            if (integrationFailure != null) Assert.Fail(integrationFailure.ToString());
            yield return new ExitPlayMode();
        }

        private static void SetM1015Result(Exception failure) { integrationFailure = failure; integrationCompleted = true; }
        private static bool IsM1015Finished() => integrationCompleted;

        private static IEnumerator RunM1015Fixture(VNFullSnapshotTests fixture, Action<Exception> finish)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(fixture.RunExpandedGraphIntegration());
            Exception failure = null;
            while (stack.Count > 0)
            {
                object current = null;
                var moved = false;
                try { moved = stack.Peek().MoveNext(); if (moved) current = stack.Peek().Current; }
                catch (Exception exception) { failure = exception; break; }
                if (!moved) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
                if (current is IEnumerator nested) { stack.Push(nested); continue; }
                yield return current;
            }
            try { fixture.TearDown(); } catch (Exception exception) { failure ??= exception; }
            finish(failure);
        }

        public IEnumerator RunExpandedGraphIntegration()
        {
            Directory.CreateDirectory(temporaryRoot);
            var source = Path.Combine(temporaryRoot, "m1015_fixture.yarn");
            File.WriteAllText(source, "title: M10_15_INTEGRATION_SOURCE\n---\n<<jump M10_15_INTEGRATION_LINE>>\n===\n\n" +
                "title: M10_15_INTEGRATION_LINE\n---\nFixture #line:m1015_integration\n<<jump M10_15_INTEGRATION_END>>\n===\n\n" +
                "title: M10_15_INTEGRATION_END\n---\nEnd #line:m1015_integration_end\n===\n");
            var projectFile = Path.GetFullPath("Assets/_Project/Yarn/GameNarrative.yarnproject");
            var sourceProject = Yarn.Compiler.Project.LoadFromFile(projectFile, Directory.GetCurrentDirectory());
            var job = CompilationJob.CreateFromFiles(sourceProject.SourceFiles.Concat(new[] { source }).ToArray());
            job.LanguageVersion = sourceProject.FileVersion;
            var compiled = Yarn.Compiler.Compiler.Compile(job);
            Assert.That(compiled.Diagnostics.Where(d => d.Severity == Diagnostic.DiagnosticSeverity.Error), Is.Empty);
            var original = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            var expanded = ScriptableObject.CreateInstance<YarnProject>();
            ownedObjects.Add(expanded);
            expanded.compiledYarnProgram = compiled.Program.ToByteArray();
            expanded.baseLocalization = UnityEngine.Object.Instantiate(original.baseLocalization);
            ownedObjects.Add(expanded.baseLocalization);
            expanded.baseLocalization.AddLocalizedString("line:m1015_integration", "Fixture");
            expanded.baseLocalization.AddLocalizedString("line:m1015_integration_end", "End");
            expanded.localizations = new SerializableDictionary<string, Localization>();
            foreach (var locale in original.localizations) expanded.localizations.Add(locale.Key, expanded.baseLocalization);
            expanded.lineMetadata = original.lineMetadata;
            Assert.That(expanded.compiledYarnProgram, Is.Not.EqualTo(original.compiledYarnProgram));
            var manifestText = Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST").text;
            var added = ", {\"runtimeLineId\":\"line:m1015_integration\",\"sourceYarnFile\":\"fixture.yarn\"," +
                "\"sourceNode\":\"M10_15_INTEGRATION_SOURCE\",\"resumeEntryNode\":\"M10_15_INTEGRATION_LINE\"," +
                "\"scene\":\"TEST\",\"presentationBeat\":\"M10_15_INTEGRATION_SOURCE\",\"eligibility\":\"SAVE_ELIGIBLE\"," +
                "\"restrictionReason\":\"\",\"successorNodeOrFlow\":\"M10_15_INTEGRATION_END\"," +
                "\"chapterId\":\"test\",\"sceneTitle\":\"Test\"}";
            var manifest = new TextAsset(manifestText.Insert(manifestText.LastIndexOf(']'), added));
            ownedObjects.Add(manifest);

            var runner = CreateRunner();
            runner.autoStart = false;
            var productionPresentation = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>("Assets/_Project/Settings/Presentation/Production/M10_ProductionPresentationCatalog.asset");
            Assert.That(productionPresentation, Is.Not.Null);
            var presentation = CreatePresentationHarness(productionPresentation);
            var productionAudio = AssetDatabase.LoadAssetAtPath<VNAudioCatalog>("Assets/_Project/Audio/M10_ProductionAudioCatalog.asset");
            Assert.That(productionAudio, Is.Not.Null);
            var audio = CreateAudioHarness(productionAudio);
            var root = new GameObject("M10-15 integration session", typeof(VNDialogueSessionState), typeof(VNInteractionGate), typeof(CanvasGroup), typeof(AudioListener));
            root.SetActive(false);
            ownedObjects.Add(root);
            var session = root.GetComponent<VNDialogueSessionState>();
            var gate = root.GetComponent<VNInteractionGate>();
            var presenter = root.AddComponent<LinePresenter>();
            presenter.canvasGroup = root.GetComponent<CanvasGroup>();
            presenter.canvasGroup.alpha = 0;
            presenter.useFadeEffect = false;
            var canvasObject = new GameObject("M10-15 canvas", typeof(RectTransform), typeof(Canvas));
            ownedObjects.Add(canvasObject);
            var textObject = new GameObject("M10-15 line", typeof(RectTransform), typeof(CanvasRenderer));
            textObject.SetActive(false);
            textObject.transform.SetParent(canvasObject.transform, false);
            ownedObjects.Add(textObject);
            presenter.lineText = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            presenter.lineText.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Regular SDF.asset");
            Assert.That(presenter.lineText.font, Is.Not.Null);
            presenter.lineText.maxVisibleCharacters = 0;
            textObject.SetActive(true);
            var lifecycle = root.AddComponent<VNLineLifecyclePresenter>();
            SetPrivate(lifecycle, "sessionState", session);
            SetPrivate(lifecycle, "linePresenter", presenter);
            runner.DialoguePresenters = new DialoguePresenterBase[] { presenter, lifecycle };
            root.SetActive(true);
            var tracker = new VNPlayTimeTracker();
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);
            var checkpoint = CreateCheckpointService();
            var metaRepository = VNMetaProgressRepository.CreateForTesting(Path.Combine(temporaryRoot, "meta"));
            var metaProgress = new VNMetaProgressService(metaRepository);
            metaProgress.Load();
            Assert.That(metaProgress.TryRecordReadLine("line:prologue_s001_001"), Is.True);
            var metaBytes = File.ReadAllBytes(metaRepository.CanonicalFilePath);
            var historyBridge = new VNPersistentReadHistoryBridge(session, metaProgress);
            Assert.That(historyBridge.Initialize(), Is.True, historyBridge.LastDiagnostic);
            var oldCoordinator = new VNYarnSaveCoordinator(repository, checkpoint, runner, tracker,
                presentation.Controller, presentation.Transition, audio.Controller, session, gate);
            SetPrivate(session, "isLineActive", true);
            SetPrivate(session, "currentLineId", "line:prologue_s005_048");
            Assert.That(audio.Controller.PlayBgm("bgm_quiet_farewell"), Is.True);
            Assert.That(oldCoordinator.TryWriteCompleteSave(key, out var oldSave).Succeeded, Is.True);
            Assert.That(oldSave.resumeGraphVersion, Is.EqualTo("cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9"));
            Assert.That(repository.TryGetSlotPath(key, out var savedPath), Is.True);
            var savedBytes = File.ReadAllBytes(savedPath);
            Assert.That(repository.TryGetThumbnailSidecarPath(key, oldSave.thumbnailFileName, out var oldThumbnail), Is.True);
            File.WriteAllBytes(oldThumbnail, new byte[] { 1, 2, 3 });

            runner.SetProject(expanded);
            var upgraded = new VNYarnSaveCoordinator(repository, checkpoint, runner, tracker,
                presentation.Controller, presentation.Transition, audio.Controller, session, gate, resumeManifest: manifest);
            var validation = upgraded.ValidateLoad(key);
            Assert.That(validation.Status, Is.EqualTo(VNYarnLoadValidationStatus.Valid), validation.Diagnostic);
            Assert.That(upgraded.TryWriteCompleteSave(key, out _).Succeeded, Is.False);
            CollectionAssert.AreEqual(savedBytes, File.ReadAllBytes(savedPath));
            var uiRoot = new GameObject("M10-15 slot protection controller");
            ownedObjects.Add(uiRoot);
            var ui = uiRoot.AddComponent<VNSaveLoadController>();
            SetPrivate(ui, "repository", repository);
            SetPrivate(ui, "saveCoordinator", upgraded);
            SetPrivate(ui, "sessionState", session);
            uiRoot.SetActive(false);
            uiRoot.SetActive(true);
            Assert.That(ui.SaveManual(0).Status, Is.EqualTo(VNSaveLoadOperationStatus.ConfirmationRequired));
            ui.CancelManualOverwrite();
            CollectionAssert.AreEqual(savedBytes, File.ReadAllBytes(savedPath));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, File.ReadAllBytes(oldThumbnail));
            oldSave.resumeGraphVersion = new string('a', 64);
            Assert.That(repository.Write(key, oldSave).Succeeded, Is.True);
            var unknownBytes = File.ReadAllBytes(savedPath);
            Assert.That(upgraded.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidCheckpoint));
            Assert.That(session.CurrentLineId, Is.EqualTo("line:prologue_s005_048"));
            Assert.That(audio.Controller.CurrentBgmId, Is.EqualTo("bgm_quiet_farewell"));
            CollectionAssert.AreEqual(metaBytes, File.ReadAllBytes(metaRepository.CanonicalFilePath));
            Assert.That(ui.SaveManual(0).Status, Is.EqualTo(VNSaveLoadOperationStatus.UnsupportedSave));
            CollectionAssert.AreEqual(unknownBytes, File.ReadAllBytes(savedPath));
            File.WriteAllBytes(savedPath, savedBytes);
            var quickKey = new VNSaveSlotKey(VNSaveSlotType.Quick, 0);
            Assert.That(repository.TryGetSlotPath(quickKey, out var quickPath), Is.True);
            foreach (var protectedContent in new[] { "{ broken", "{\"schemaVersion\":3}" })
            {
                File.WriteAllText(quickPath, protectedContent);
                var quickBytes = File.ReadAllBytes(quickPath);
                Assert.That(ui.QuickSave().Status, Is.EqualTo(VNSaveLoadOperationStatus.UnsupportedSave));
                CollectionAssert.AreEqual(quickBytes, File.ReadAllBytes(quickPath));
            }
            var autoBytes = new Dictionary<string, byte[]>();
            for (var index = 0; index < 5; index++)
            {
                var autoKey = new VNSaveSlotKey(VNSaveSlotType.Auto, index);
                Assert.That(repository.TryGetSlotPath(autoKey, out var autoPath), Is.True);
                File.WriteAllText(autoPath, index % 2 == 0 ? "{ broken" : "{\"schemaVersion\":3}");
                autoBytes.Add(autoPath, File.ReadAllBytes(autoPath));
            }
            Assert.That(upgraded.TryWriteCompleteAutoSave().Succeeded, Is.False);
            foreach (var entry in autoBytes) CollectionAssert.AreEqual(entry.Value, File.ReadAllBytes(entry.Key));
            Assert.That(repository.TryGetSlotPath(new VNSaveSlotKey(VNSaveSlotType.Auto, 0), out var emptyAutoPath), Is.True);
            File.Delete(emptyAutoPath);
            SetPrivate(session, "isLineActive", false);
            SetPrivate(session, "currentLineId", string.Empty);
            SetPrivate(ui, "loadInProgress", true);
            var execution = upgraded.ExecuteValidatedLoad(validation);
            yield return WaitForM1015(() => execution.IsCompleted(), "validated load");
            var executionResult = execution.GetAwaiter().GetResult();
            Assert.That(executionResult.Succeeded, Is.True, executionResult.Diagnostic);
            yield return WaitForM1015(() => session.CurrentLineId == "line:prologue_s005_048", "restored line");
            Assert.That(audio.Controller.CurrentBgmId, Is.EqualTo("bgm_quiet_farewell"));
            Assert.That(audio.SfxSource.isPlaying, Is.False, "Resume must not replay an earlier one-shot.");
            CollectionAssert.AreEqual(savedBytes, File.ReadAllBytes(savedPath));
            Assert.That(File.Exists(emptyAutoPath), Is.False, "Load must not trigger a duplicate autosave.");
            SetPrivate(ui, "loadInProgress", false);
            runner.RequestNextLine();
            yield return WaitForM1015(() => session.CurrentLineId == "line:prologue_s005_049", "successor line");
            CollectionAssert.AreEqual(metaBytes, File.ReadAllBytes(metaRepository.CanonicalFilePath));
            Assert.That(session.ReadHistory.IsRead("line:prologue_s001_001"), Is.True);
            Assert.That(session.ReadHistory.IsRead("line:prologue_s005_048"), Is.False);
            Assert.That(ui.SaveManual(0).Status, Is.EqualTo(VNSaveLoadOperationStatus.ConfirmationRequired));
            Assert.That(ui.ConfirmManualOverwrite().Status, Is.EqualTo(VNSaveLoadOperationStatus.Succeeded));
            Assert.That(File.Exists(oldThumbnail), Is.False, "The previous thumbnail must be removed before replacement.");
            Assert.That(VNProductionLineResume.TryResolve("line:prologue_s005_049", expanded,
                out _, out var expandedFingerprint, out var fingerprintDiagnostic, manifest), Is.True, fingerprintDiagnostic);
            Assert.That(repository.Read(key).SaveData.resumeGraphVersion, Is.EqualTo(expandedFingerprint));
            var stop = runner.Stop();
            yield return WaitForM1015(() => stop.IsCompletedSuccessfully(), "dialogue stop");
            SetPrivate(session, "isLineActive", true);
            SetPrivate(session, "currentLineId", "line:m1015_integration");
            var futureKey = new VNSaveSlotKey(VNSaveSlotType.Manual, 1);
            Assert.That(upgraded.TryWriteCompleteSave(futureKey, out var futureSave).Succeeded, Is.True);
            Assert.That(futureSave.chapterId, Is.EqualTo("test"));
            Assert.That(futureSave.sceneTitle, Is.EqualTo("Test"));
            Assert.That(futureSave.resumeGraphVersion, Is.EqualTo(expandedFingerprint));
            var futureValidation = upgraded.ValidateLoad(futureKey);
            Assert.That(futureValidation.Status, Is.EqualTo(VNYarnLoadValidationStatus.Valid), futureValidation.Diagnostic);
            SetPrivate(session, "isLineActive", false);
            SetPrivate(session, "currentLineId", string.Empty);
            var futureExecution = upgraded.ExecuteValidatedLoad(futureValidation);
            yield return WaitForM1015(() => futureExecution.IsCompleted(), "new line load");
            var futureResult = futureExecution.GetAwaiter().GetResult();
            Assert.That(futureResult.Succeeded, Is.True, futureResult.Diagnostic);
            yield return WaitForM1015(() => session.CurrentLineId == "line:m1015_integration", "new line restored");
            runner.RequestNextLine();
            yield return WaitForM1015(() => session.CurrentLineId == "line:m1015_integration_end", "new line successor");
            CollectionAssert.AreEqual(metaBytes, File.ReadAllBytes(metaRepository.CanonicalFilePath));
            stop = runner.Stop();
            yield return WaitForM1015(() => stop.IsCompletedSuccessfully(), "new dialogue stop");

            var visibility = uiRoot.AddComponent<VNUIVisibilityController>();
            var overlayObject = new GameObject("M10-15 information overlay", typeof(RectTransform), typeof(CanvasGroup));
            overlayObject.SetActive(false);
            overlayObject.transform.SetParent(canvasObject.transform, false);
            ownedObjects.Add(overlayObject);
            var overlay = overlayObject.AddComponent<VNInformationOverlay>();
            var infoTitleObject = new GameObject("Information title", typeof(RectTransform), typeof(CanvasRenderer));
            infoTitleObject.transform.SetParent(overlayObject.transform, false);
            ownedObjects.Add(infoTitleObject);
            var infoTitle = infoTitleObject.AddComponent<TMPro.TextMeshProUGUI>();
            infoTitle.font = presenter.lineText.font;
            var infoBodyObject = new GameObject("Information body", typeof(RectTransform), typeof(CanvasRenderer));
            infoBodyObject.transform.SetParent(overlayObject.transform, false);
            ownedObjects.Add(infoBodyObject);
            var infoBody = infoBodyObject.AddComponent<TMPro.TextMeshProUGUI>();
            infoBody.font = presenter.lineText.font;
            SetPrivate(overlay, "root", overlayObject.GetComponent<CanvasGroup>());
            SetPrivate(overlay, "titleText", infoTitle);
            SetPrivate(overlay, "bodyText", infoBody);
            SetPrivate(overlay, "visibility", visibility);
            SetPrivate(overlay, "saveLoad", ui);
            overlayObject.SetActive(true);
            var cases = new[]
            {
                ("s001_016", "s001_017", "PROLOGUE_LINE_S001_016", "S001", "bg_family_kitchen_morning", "", ""),
                ("s002_009", "s002_010", "PROLOGUE_LINE_S002_009", "S002", "", "cg_beach_photo_child", ""),
                ("s005_048", "s005_049", "PROLOGUE_LINE_S005_048", "S005", "", "", "bgm_quiet_farewell"),
                ("s007_008", "s007_009", "PROLOGUE_LINE_S007_008", "S007", "", "", ""),
                ("s008_017", "s008_018", "PROLOGUE_LINE_S008_017", "S008", "", "", ""),
            };
            for (var index = 0; index < cases.Length; index++)
            {
                var (line, next, node, scene, background, cg, bgm) = cases[index];
                var fixtureKey = new VNSaveSlotKey(VNSaveSlotType.Manual, index + 2);
                var reconstructed = JsonUtility.FromJson<SaveSlotData>(JsonUtility.ToJson(oldSave));
                reconstructed.slotIndex = fixtureKey.SlotIndex;
                reconstructed.resumeLineId = "line:prologue_" + line;
                reconstructed.resumeEntryNode = node;
                reconstructed.resumeGraphVersion = "cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9";
                reconstructed.sceneTitle = "프롤로그 " + scene;
                reconstructed.thumbnailFileName = string.Empty;
                reconstructed.presentationState.backgroundId = background;
                reconstructed.presentationState.cgId = cg;
                reconstructed.presentationState.characters = line == "s001_016"
                    ? new[] { Character("jinhee", "default", "left", "right", 1f) }
                    : Array.Empty<CharacterSaveState>();
                reconstructed.audioState.bgmId = bgm;
                reconstructed.audioState.playbackSeconds = 0;
                Assert.That(repository.Write(fixtureKey, reconstructed).Succeeded, Is.True, line);
                Assert.That(repository.TryGetSlotPath(fixtureKey, out var fixturePath), Is.True);
                var fixtureBytes = File.ReadAllBytes(fixturePath);
                for (var expandedMode = 0; expandedMode < 2; expandedMode++)
                {
                    runner.SetProject(expandedMode == 0 ? original : expanded);
                    var withOverlay = new VNYarnSaveCoordinator(repository, checkpoint, runner, tracker,
                        presentation.Controller, presentation.Transition, audio.Controller, session, gate, overlay,
                        resumeManifest: expandedMode == 0 ? null : manifest);
                    var plan = withOverlay.ValidateLoad(fixtureKey);
                    Assert.That(plan.Status, Is.EqualTo(VNYarnLoadValidationStatus.Valid), line + ": " + plan.Diagnostic);
                    SetPrivate(ui, "loadInProgress", true);
                    var load = withOverlay.ExecuteValidatedLoad(plan);
                    yield return WaitForM1015(() => load.IsCompleted(), "load " + line);
                    var loadResult = load.GetAwaiter().GetResult();
                    Assert.That(loadResult.Succeeded, Is.True, line + ": " + loadResult.Diagnostic);
                    yield return WaitForM1015(() => session.CurrentLineId == "line:prologue_" + line, "line " + line);
                    Assert.That(presentation.Controller.CurrentBackgroundId ?? string.Empty, Is.EqualTo(background));
                    Assert.That(presentation.Controller.CurrentCGId ?? string.Empty, Is.EqualTo(cg));
                    Assert.That(audio.Controller.CurrentBgmId, Is.EqualTo(bgm));
                    if (line == "s001_016") Assert.That(presentation.Controller.VisibleCharacters.ContainsKey("jinhee"), Is.True);
                    if (line == "s007_008")
                    {
                        Assert.That(overlay.IsShown, Is.True);
                        Assert.That(infoTitle.text, Is.EqualTo("최근 생활 기록"));
                        Assert.That(infoBody.text, Is.EqualTo("수면\n식사"));
                    }
                    CollectionAssert.AreEqual(fixtureBytes, File.ReadAllBytes(fixturePath));
                    SetPrivate(ui, "loadInProgress", false);
                    runner.RequestNextLine();
                    yield return WaitForM1015(() => session.CurrentLineId == "line:prologue_" + next, "successor " + line);
                    stop = runner.Stop();
                    yield return WaitForM1015(() => stop.IsCompletedSuccessfully(), "stop " + line);
                    overlay.Hide();
                }
            }
            runner.SetProject(original);
            var legacyKey = new VNSaveSlotKey(VNSaveSlotType.Manual, 8);
            var legacy = CreateSave(legacyKey, new PresentationState(), new AudioState());
            Assert.That(repository.Write(legacyKey, legacy).Succeeded, Is.True);
            Assert.That(repository.TryGetSlotPath(legacyKey, out var legacyPath), Is.True);
            var legacyBytes = File.ReadAllBytes(legacyPath);
            var legacyCoordinator = new VNYarnSaveCoordinator(repository, checkpoint, runner, tracker,
                presentation.Controller, presentation.Transition, audio.Controller, session, gate, overlay);
            var legacyPlan = legacyCoordinator.ValidateLoad(legacyKey);
            Assert.That(legacyPlan.Status, Is.EqualTo(VNYarnLoadValidationStatus.Valid), legacyPlan.Diagnostic);
            SetPrivate(ui, "loadInProgress", true);
            var legacyLoad = legacyCoordinator.ExecuteValidatedLoad(legacyPlan);
            yield return WaitForM1015(() => legacyLoad.IsCompleted(), "legacy checkpoint load");
            var legacyResult = legacyLoad.GetAwaiter().GetResult();
            Assert.That(legacyResult.Succeeded, Is.True, legacyResult.Diagnostic);
            yield return WaitForM1015(() => session.CurrentLineId == "line:0230259", "legacy checkpoint line");
            Assert.That(checkpoint.HasCurrentCheckpoint, Is.True);
            CollectionAssert.AreEqual(legacyBytes, File.ReadAllBytes(legacyPath));
            SetPrivate(ui, "loadInProgress", false);
            stop = runner.Stop();
            yield return WaitForM1015(() => stop.IsCompletedSuccessfully(), "legacy checkpoint stop");
            CollectionAssert.AreEqual(metaBytes, File.ReadAllBytes(metaRepository.CanonicalFilePath));
            Assert.That(File.Exists(emptyAutoPath), Is.False, "Loading historical lines must not allocate an autosave.");

            // Exercise M12's asynchronous load-completion contract through the
            // existing M5 controller. The slot validates before the load starts;
            // remove its catalog dependency from the LoadStateChanged callback so
            // prepared Yarn state is restored before presentation restore fails.
            var failingSave = JsonUtility.FromJson<SaveSlotData>(JsonUtility.ToJson(legacy));
            failingSave.presentationState.backgroundId = "bg_family_kitchen_morning";
            Assert.That(repository.Write(legacyKey, failingSave).Succeeded, Is.True);
            Assert.That(legacyCoordinator.ValidateLoad(legacyKey).Status, Is.EqualTo(VNYarnLoadValidationStatus.Valid));
            SetPrivate(ui, "saveCoordinator", legacyCoordinator);
            var originalCatalog = productionPresentation;
            VNSaveLoadOperationResult failedCompletion = null;
            VNSaveLoadOperationResult repeatedRequest = null;
            var ownershipReleasedBeforeCompletion = false;
            Action<bool> loadOwnership = loading =>
            {
                if (!loading)
                {
                    ownershipReleasedBeforeCompletion = failedCompletion == null;
                    return;
                }

                repeatedRequest = ui.Load(legacyKey);
                SetPrivate(presentation.Controller, "catalog", null);
            };
            ui.LoadStateChanged += loadOwnership;
            ui.LoadExecutionCompleted += result => failedCompletion = result;
            LogAssert.Expect(LogType.Error, "Cannot set unknown background 'bg_family_kitchen_morning'.");
            var accepted = ui.Load(legacyKey);
            Assert.That(accepted.Succeeded, Is.True, accepted.Message);
            Assert.That(repeatedRequest.Status, Is.EqualTo(VNSaveLoadOperationStatus.Busy));
            yield return WaitForM1015(() => failedCompletion != null, "asynchronous controller failure completion");
            Assert.That(failedCompletion.Succeeded, Is.False);
            Assert.That(failedCompletion.Status, Is.EqualTo(VNSaveLoadOperationStatus.LoadValidationFailed));
            Assert.That(ownershipReleasedBeforeCompletion, Is.True,
                "Load ownership must be released before the final completion event.");
            Assert.That(ui.IsLoadInProgress, Is.False);
            SetPrivate(presentation.Controller, "catalog", originalCatalog);
            ui.LoadStateChanged -= loadOwnership;
            CollectionAssert.AreEqual(metaBytes, File.ReadAllBytes(metaRepository.CanonicalFilePath));
            historyBridge.Dispose();
            Debug.Log("M10-15 integration: completed S005 restore/successor and M12 async load-failure recovery signal");
        }

        private static IEnumerator WaitForM1015(Func<bool> condition, string stage)
        {
            var timeout = System.Diagnostics.Stopwatch.StartNew();
            while (timeout.Elapsed < TimeSpan.FromSeconds(30))
            {
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail("M10-15 integration did not reach: " + stage);
        }

        [Test]
        public void PresentationCapture_UsesLogicalIdsAndDeterministicVisibleSlotOrder()
        {
            var presentation = CreatePresentationHarness();
            Assert.That(presentation.Controller.TryCaptureStableState(out var empty, out var emptyDiagnostic), Is.True, emptyDiagnostic);
            Assert.That(empty.backgroundId, Is.Empty);
            Assert.That(empty.cgId, Is.Empty);
            Assert.That(empty.characters, Is.Empty);

            Assert.That(presentation.Controller.SetBackground("bg_a"), Is.True);
            Assert.That(presentation.Controller.SetCG("cg_a"), Is.True);
            Assert.That(presentation.Controller.ShowCharacter("char_b", "default", VNCharacterSlot.Right), Is.True);
            Assert.That(presentation.Controller.ShowCharacter("char_a", "default", VNCharacterSlot.Left), Is.True);
            Assert.That(presentation.Controller.SetFacing("char_a", VNCharacterFacing.Left), Is.True);
            Assert.That(presentation.Controller.SetScale("char_a", 1.25f), Is.True);

            Assert.That(presentation.Controller.TryCaptureStableState(out var saved, out var diagnostic), Is.True, diagnostic);
            Assert.That(saved.backgroundId, Is.EqualTo("bg_a"));
            Assert.That(saved.cgId, Is.EqualTo("cg_a"));
            Assert.That(saved.characters, Has.Length.EqualTo(2));
            Assert.That(saved.characters[0].characterId, Is.EqualTo("char_a"));
            Assert.That(saved.characters[0].slot, Is.EqualTo("left"));
            Assert.That(saved.characters[0].facing, Is.EqualTo("left"));
            Assert.That(saved.characters[0].scale, Is.EqualTo(1.25f));
            Assert.That(saved.characters[1].characterId, Is.EqualTo("char_b"));
            Assert.That(saved.characters[1].slot, Is.EqualTo("right"));
        }

        [Test]
        public void PresentationValidation_RejectsUnknownAndDuplicateLogicalStateBeforeRestore()
        {
            var presentation = CreatePresentationHarness();
            var valid = CreatePresentationState("bg_a", "cg_a", Character("char_a", "default", "left", "right", 1f));
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out var validDiagnostic), Is.True, validDiagnostic);

            valid.backgroundId = "missing_bg";
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out _), Is.False);
            valid.backgroundId = "bg_a";
            valid.cgId = "missing_cg";
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out _), Is.False);
            valid.cgId = "cg_a";
            valid.characters[0].characterId = "missing_character";
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out _), Is.False);
            valid.characters[0].characterId = "char_a";
            valid.characters[0].expressionId = "missing_expression";
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out _), Is.False);
            valid.characters[0].expressionId = "default";
            valid.characters = new[] { Character("char_a", "default", "left", "right", 1f), Character("char_a", "default", "right", "right", 1f) };
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out _), Is.False);
            valid.characters = new[] { Character("char_a", "default", "left", "right", 1f), Character("char_b", "default", "left", "right", 1f) };
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out _), Is.False);
            valid.characters = new[] { Character("char_a", "default", "not_a_slot", "right", 1f) };
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out _), Is.False);
            valid.characters = new[] { Character("char_a", "default", "left", "up", 1f) };
            Assert.That(presentation.Controller.TryPrepareRestore(valid, out _, out _), Is.False);
        }

        [Test]
        public void PresentationRestore_ClearsUnsavedStateAndNormalizesM4VisualChannels()
        {
            var presentation = CreatePresentationHarness();
            Assert.That(presentation.Controller.SetBackground("bg_b"), Is.True);
            Assert.That(presentation.Controller.SetCG("cg_b"), Is.True);
            Assert.That(presentation.Controller.ShowCharacter("char_b", "default", VNCharacterSlot.FarLeft), Is.True);
            presentation.ScreenFade.alpha = 1f;
            presentation.IncomingImage.sprite = presentation.SpriteA;
            presentation.IncomingImage.enabled = true;
            presentation.IncomingFade.alpha = 0.5f;

            var saved = CreatePresentationState("bg_a", "cg_a", Character("char_a", "default", "center", "left", 1.5f));
            Assert.That(presentation.Controller.TryPrepareRestore(saved, out var plan, out var validationDiagnostic), Is.True, validationDiagnostic);
            presentation.Transition.NormalizeForLoad();
            Assert.That(presentation.Controller.RestorePreparedState(plan, out var restoreDiagnostic), Is.True, restoreDiagnostic);
            presentation.Transition.FinalizeStableStateAfterLoad();

            Assert.That(presentation.Controller.CurrentBackgroundId, Is.EqualTo("bg_a"));
            Assert.That(presentation.Controller.CurrentCGId, Is.EqualTo("cg_a"));
            Assert.That(presentation.Controller.VisibleCharacters.ContainsKey("char_a"), Is.True);
            Assert.That(presentation.Controller.VisibleCharacters.ContainsKey("char_b"), Is.False);
            Assert.That(presentation.Controller.VisibleCharacters["char_a"].Facing, Is.EqualTo(VNCharacterFacing.Left));
            Assert.That(presentation.Controller.VisibleCharacters["char_a"].Scale, Is.EqualTo(1.5f));
            Assert.That(presentation.Views[VNCharacterSlot.Center].FadeCanvasGroup.alpha, Is.EqualTo(1f));
            Assert.That(presentation.ScreenFade.alpha, Is.EqualTo(0f));
            Assert.That(presentation.IncomingImage.sprite, Is.Null);
            Assert.That(presentation.IncomingImage.enabled, Is.False);
            Assert.That(presentation.IncomingFade.alpha, Is.EqualTo(0f));
            Assert.That(presentation.Views[VNCharacterSlot.Center].Tint, Is.EqualTo(Color.white));

            var clear = CreatePresentationState(string.Empty, string.Empty);
            Assert.That(presentation.Controller.TryPrepareRestore(clear, out var clearPlan, out _), Is.True);
            Assert.That(presentation.Controller.RestorePreparedState(clearPlan, out _), Is.True);
            Assert.That(presentation.Controller.CurrentBackgroundId, Is.Null);
            Assert.That(presentation.Controller.CurrentCGId, Is.Null);
            Assert.That(presentation.Controller.VisibleCharacters, Is.Empty);
        }

        [TestCase("rope_memory")]
        [TestCase("rope")]
        public void ProductionMorningSixCharacterSnapshot_RoundTripsAndLegacyFiveSlotStateRemainsCompatible(string ropeId)
        {
            const string catalogPath = "Assets/_Project/Settings/Presentation/Production/M10_ProductionPresentationCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(catalogPath);
            Assert.That(catalog, Is.Not.Null);
            var presentation = CreatePresentationHarness(catalog);
            var characterIds = new[] { "minseok", "jinhee", "jihun", ropeId, "junho", "narae" };
            var slots = new[]
            {
                VNCharacterSlot.FarLeft, VNCharacterSlot.Left, VNCharacterSlot.Center,
                VNCharacterSlot.Right, VNCharacterSlot.FarRight, VNCharacterSlot.OuterFarRight,
            };
            var scales = new[] { 0.60f, 0.60f, 0.60f, 0.28f, 0.60f, 0.60f };
            var definitions = new VNCharacterDefinition[characterIds.Length];
            var uniqueBodySprites = new HashSet<Sprite>();

            Assert.That(presentation.Controller.SetBackground("bg_family_table_morning"), Is.True);
            for (var index = 0; index < characterIds.Length; index++)
            {
                Assert.That(catalog.TryGetCharacter(characterIds[index], out definitions[index]), Is.True, characterIds[index]);
                Assert.That(presentation.Controller.ShowCharacter(characterIds[index], "default", slots[index]), Is.True, characterIds[index]);
                Assert.That(presentation.Controller.SetFacing(characterIds[index], definitions[index].DefaultFacing), Is.True, characterIds[index]);
                Assert.That(presentation.Controller.SetScale(characterIds[index], scales[index]), Is.True, characterIds[index]);
                Assert.That(definitions[index].BodySprite, Is.Not.Null, characterIds[index]);
                Assert.That(uniqueBodySprites.Add(definitions[index].BodySprite), Is.True, $"{characterIds[index]} must retain its own default body sprite.");
            }

            Assert.That(presentation.Controller.VisibleCharacters, Has.Count.EqualTo(6));
            Assert.That(presentation.Controller.TryCaptureStableState(out var captured, out var captureDiagnostic), Is.True, captureDiagnostic);
            Assert.That(captured.characters, Has.Length.EqualTo(6));
            var diskKey = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);
            Assert.That(repository.Write(diskKey, CreateSave(diskKey, captured, new AudioState())).Succeeded, Is.True);
            var diskRead = repository.Read(diskKey);
            Assert.That(diskRead.State, Is.EqualTo(VNSaveSlotState.Valid));
            captured = diskRead.SaveData.presentationState;
            var capturedIds = new HashSet<string>();
            var capturedSlots = new HashSet<string>();
            for (var index = 0; index < captured.characters.Length; index++)
            {
                var state = captured.characters[index];
                Assert.That(capturedIds.Add(state.characterId), Is.True, state.characterId);
                Assert.That(capturedSlots.Add(state.slot), Is.True, state.slot);
                Assert.That(state.characterId, Is.EqualTo(characterIds[index]));
                Assert.That(state.slot, Is.EqualTo(VNPresentationController.ToSerializedSlot(slots[index])));
                Assert.That(state.expressionId, Is.EqualTo(definitions[index].DefaultExpressionId));
                Assert.That(state.facing, Is.EqualTo(VNPresentationController.ToSerializedFacing(definitions[index].DefaultFacing)));
                Assert.That(state.scale, Is.EqualTo(scales[index]));
            }

            Assert.That(VNPresentationController.TryParseSlot("outer_far_right", out var sixthSlot), Is.True);
            Assert.That(sixthSlot, Is.EqualTo(VNCharacterSlot.OuterFarRight));
            Assert.That(VNPresentationController.ToSerializedSlot(sixthSlot), Is.EqualTo("outer_far_right"));

            Assert.That(presentation.Controller.TryPrepareRestore(captured, out var fullRestorePlan, out var preparationDiagnostic), Is.True, preparationDiagnostic);
            Assert.That(presentation.Controller.VisibleCharacters, Has.Count.EqualTo(6), "Preparing a snapshot must not mutate the live presentation.");
            Assert.That(presentation.Controller.CurrentBackgroundId, Is.EqualTo("bg_family_table_morning"));

            var invalidSlotState = ClonePresentationState(captured);
            invalidSlotState.characters[5].slot = "unrecognized_slot";
            Assert.That(presentation.Controller.TryPrepareRestore(invalidSlotState, out _, out _), Is.False);

            var duplicateSlotState = ClonePresentationState(captured);
            duplicateSlotState.characters[5].slot = "far_right";
            Assert.That(presentation.Controller.TryPrepareRestore(duplicateSlotState, out _, out _), Is.False);
            Assert.That(presentation.Controller.VisibleCharacters, Has.Count.EqualTo(6), "Invalid preparation must leave all six visible characters untouched.");

            var clearState = CreatePresentationState(string.Empty, string.Empty);
            Assert.That(presentation.Controller.TryPrepareRestore(clearState, out var clearPlan, out _), Is.True);
            Assert.That(presentation.Controller.RestorePreparedState(clearPlan, out var clearDiagnostic), Is.True, clearDiagnostic);
            Assert.That(presentation.Controller.VisibleCharacters, Is.Empty);
            Assert.That(presentation.Controller.RestorePreparedState(fullRestorePlan, out var restoreDiagnostic), Is.True, restoreDiagnostic);
            Assert.That(presentation.Controller.VisibleCharacters, Has.Count.EqualTo(6));

            for (var index = 0; index < characterIds.Length; index++)
            {
                var state = presentation.Controller.VisibleCharacters[characterIds[index]];
                Assert.That(state.Slot, Is.EqualTo(slots[index]));
                Assert.That(state.Facing, Is.EqualTo(definitions[index].DefaultFacing));
                Assert.That(state.Scale, Is.EqualTo(scales[index]));
                Assert.That(presentation.Views[slots[index]].gameObject.activeSelf, Is.True, characterIds[index]);
                var bodyImage = (Image)typeof(VNCharacterSlotView).GetField("bodyImage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(presentation.Views[slots[index]]);
                Assert.That(bodyImage.enabled, Is.True, characterIds[index]);
                Assert.That(bodyImage.sprite, Is.SameAs(definitions[index].BodySprite), characterIds[index]);
            }

            Assert.That(catalog.TryGetCharacter("jinhee", out var jinhee), Is.True);
            Assert.That(jinhee.TryGetExpression("crying", out var cryingExpression), Is.True);
            Assert.That(presentation.Controller.SetExpression("jinhee", "crying"), Is.True);
            var jinheeBody = (Image)typeof(VNCharacterSlotView).GetField("bodyImage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(presentation.Views[VNCharacterSlot.Left]);
            var jinheeHead = (Image)typeof(VNCharacterSlotView).GetField("headImage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(presentation.Views[VNCharacterSlot.Left]);
            Assert.That(jinheeBody.enabled, Is.True);
            Assert.That(jinheeBody.sprite, Is.SameAs(jinhee.BodySprite));
            if (cryingExpression.HeadSprite != null)
                Assert.That(ReferenceEquals(jinheeHead.sprite, cryingExpression.HeadSprite), Is.True);
            else
                Assert.That(jinheeHead.sprite == null, Is.True, "Unity object null semantics should keep the optional head image hidden when no expression art is assigned.");
            Assert.That(jinheeHead.enabled, Is.EqualTo(cryingExpression.HeadSprite != null), "Optional expression art must be displayed when supplied and safely hidden while the default body remains visible when absent.");

            var legacyFiveCharacters = new CharacterSaveState[5];
            Array.Copy(captured.characters, legacyFiveCharacters, legacyFiveCharacters.Length);
            var legacyFiveState = CreatePresentationState(captured.backgroundId, captured.cgId, legacyFiveCharacters);
            Assert.That(presentation.Controller.TryPrepareRestore(legacyFiveState, out var legacyFivePlan, out var legacyDiagnostic), Is.True, legacyDiagnostic);
            Assert.That(presentation.Controller.RestorePreparedState(clearPlan, out _), Is.True);
            Assert.That(presentation.Controller.RestorePreparedState(legacyFivePlan, out var legacyRestoreDiagnostic), Is.True, legacyRestoreDiagnostic);
            Assert.That(presentation.Controller.VisibleCharacters, Has.Count.EqualTo(5));
            Assert.That(presentation.Controller.VisibleCharacters.ContainsKey("narae"), Is.False);
            for (var index = 0; index < legacyFiveCharacters.Length; index++)
                Assert.That(presentation.Controller.VisibleCharacters[legacyFiveCharacters[index].characterId].Slot, Is.EqualTo(slots[index]));
        }

        [Test]
        public void AudioCapture_UsesLogicalIdAndSamplePositionAndSilenceHasNoSourceIdentity()
        {
            var audio = CreateAudioHarness();
            Assert.That(audio.Controller.TryCaptureStableState(out var silence, out var silenceDiagnostic), Is.True, silenceDiagnostic);
            Assert.That(silence.bgmId, Is.Empty);
            Assert.That(silence.playbackSeconds, Is.EqualTo(0f));

            audio.SourceA.clip = audio.ClipA;
            audio.SourceA.loop = true;
            audio.SourceA.timeSamples = 500;
            SetPrivate(audio.Controller, "currentBgmId", "bgm_a");
            SetPrivate(audio.Controller, "sourceAIsActive", true);
            Assert.That(audio.Controller.TryCaptureStableState(out var captured, out var captureDiagnostic), Is.True, captureDiagnostic);
            Assert.That(captured.bgmId, Is.EqualTo("bgm_a"));
            Assert.That(captured.playbackSeconds, Is.EqualTo(0.5f).Within(0.01f));
        }

        [Test]
        public void AudioValidation_NormalizesLoopAndNonLoopPlaybackAndRejectsInvalidBgm()
        {
            var audio = CreateAudioHarness();
            Assert.That(audio.Controller.TryPrepareRestore(new AudioState { bgmId = "bgm_a", playbackSeconds = 2.25f }, out var loopPlan, out var loopDiagnostic), Is.True, loopDiagnostic);
            Assert.That(loopPlan.PlaybackSeconds, Is.EqualTo(0.25f).Within(0.01f));
            Assert.That(audio.Controller.TryPrepareRestore(new AudioState { bgmId = "bgm_b", playbackSeconds = 2.25f }, out var nonLoopPlan, out var nonLoopDiagnostic), Is.True, nonLoopDiagnostic);
            Assert.That(nonLoopPlan.PlaybackSeconds, Is.EqualTo(0.99f).Within(0.01f));
            Assert.That(audio.Controller.TryPrepareRestore(new AudioState { bgmId = "missing_bgm", playbackSeconds = 0f }, out _, out _), Is.False);
            Assert.That(audio.Controller.TryPrepareRestore(new AudioState { bgmId = "bgm_a", playbackSeconds = float.NaN }, out _, out _), Is.False);
        }

        [Test]
        public void AudioRestore_NormalizesToCanonicalSourceAndStopsStaleSfx()
        {
            var audio = CreateAudioHarness();
            audio.SourceB.clip = audio.ClipB;
            audio.SourceB.volume = 0.25f;
            audio.SfxSource.mute = true;
            Assert.That(audio.Controller.PlaySfx("sfx_test"), Is.True);
            Assert.That(audio.SfxSource.isPlaying, Is.True, "PlayOneShot should be visible through the controller's SFX AudioSource.");
            SetPrivate(audio.Controller, "pendingSfxEndDspTime", AudioSettings.dspTime - 1d);
            Assert.That(audio.Controller.IsSfxPlaybackPending, Is.True);
            Assert.That(audio.SfxSource.isPlaying, Is.True, "The live AudioSource state keeps the wait pending even after the estimated clip deadline.");
            var sfxWait = audio.Controller.WaitForSfxPlayback();
            Assert.That(sfxWait.MoveNext(), Is.True, "The wait must yield while the one-shot is pending.");
            Assert.That(audio.Controller.TryPrepareRestore(new AudioState { bgmId = "bgm_a", playbackSeconds = 0.5f }, out var plan, out var diagnostic), Is.True, diagnostic);
            audio.Controller.NormalizeTransientForLoad();
            Assert.That(audio.Controller.IsSfxPlaybackPending, Is.False);
            Assert.That(sfxWait.MoveNext(), Is.False, "Load normalization must release a pending SFX wait after stopping the transient source.");
            Assert.That(audio.Controller.RestorePreparedState(plan, out var restoreDiagnostic), Is.True, restoreDiagnostic);
            Assert.That(audio.Controller.CurrentBgmId, Is.EqualTo("bgm_a"));
            Assert.That(audio.SourceA.clip, Is.EqualTo(audio.ClipA));
            Assert.That(audio.SourceA.loop, Is.True);
            Assert.That(audio.SourceA.volume, Is.EqualTo(0.7f));
            Assert.That(audio.SourceB.clip, Is.Null);
            Assert.That(audio.SfxSource.isPlaying, Is.False);

            Assert.That(audio.Controller.TryPrepareRestore(new AudioState { bgmId = string.Empty, playbackSeconds = 0f }, out var silencePlan, out _), Is.True);
            audio.Controller.NormalizeTransientForLoad();
            Assert.That(audio.Controller.RestorePreparedState(silencePlan, out _), Is.True);
            Assert.That(audio.Controller.CurrentBgmId, Is.Empty);
            Assert.That(audio.SourceA.clip, Is.Null);
            Assert.That(audio.SourceB.clip, Is.Null);
        }

        [Test]
        public void FullComposition_UsesRealM3M4StateAndRejectsUnstableOperations()
        {
            var presentation = CreatePresentationHarness();
            var audio = CreateAudioHarness();
            var runner = CreateRunner();
            var service = CreateCheckpointService();
            var tracker = new VNPlayTimeTracker();
            var coordinator = new VNYarnSaveCoordinator(repository, service, runner, tracker, presentation.Controller, presentation.Transition, audio.Controller);
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);
            Assert.That(service.TryEnterCheckpoint(CheckpointId, runner, out var enterDiagnostic), Is.True, enterDiagnostic);
            Assert.That(presentation.Controller.SetBackground("bg_a"), Is.True);
            Assert.That(presentation.Controller.ShowCharacter("char_a", "default", VNCharacterSlot.Left), Is.True);
            audio.SourceA.clip = audio.ClipA;
            audio.SourceA.loop = true;
            SetPrivate(audio.Controller, "currentBgmId", "bgm_a");
            runner.VariableStorage.SetAllVariables(new Dictionary<string, float> { ["$saved"] = 2f }, new Dictionary<string, string>(), new Dictionary<string, bool>(), true);

            Assert.That(coordinator.TryComposeCompleteSave(key, out var saveData, out var composeDiagnostic), Is.True, composeDiagnostic);
            Assert.That(saveData.presentationState.backgroundId, Is.EqualTo("bg_a"));
            Assert.That(saveData.presentationState.characters, Has.Length.EqualTo(1));
            Assert.That(saveData.audioState.bgmId, Is.EqualTo("bgm_a"));
            Assert.That(saveData.thumbnailFileName, Is.EqualTo("manual_00.jpg"));
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Empty));

            Assert.That(coordinator.TryWriteCompleteSave(key, out var writtenData).Succeeded, Is.True);
            Assert.That(writtenData.thumbnailFileName, Is.EqualTo("manual_00.jpg"));
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Valid));

            SetPrivate(presentation.Transition, "activeTransitionOperations", 1);
            Assert.That(coordinator.TryComposeTechnicalSave(key, out _, out _), Is.False);
            Assert.That(coordinator.TryWriteCompleteSave(key, out _).Succeeded, Is.False);
            Assert.That(repository.Read(key).State, Is.EqualTo(VNSaveSlotState.Valid), "An unstable complete snapshot must not replace the authoritative JSON.");
            SetPrivate(presentation.Transition, "activeTransitionOperations", 0);
            var localEffect = CreateCanvasGroup("Technical localized door");
            localEffect.alpha = 1f;
            SetPrivate(presentation.Transition, "localizedEffectCanvasGroup", localEffect);
            Assert.That(coordinator.TryComposeCompleteSave(key, out _, out var localDiagnostic), Is.False, localDiagnostic);
            presentation.Transition.NormalizeForLoad();
            Assert.That(localEffect.alpha, Is.Zero);
            Assert.That(coordinator.TryComposeCompleteSave(key, out _, out _), Is.True);
            SetPrivate(audio.Controller, "activeBgmTransitionOperations", 1);
            Assert.That(coordinator.TryComposeTechnicalSave(key, out _, out _), Is.False);
        }

        [Test]
        public void FullLoadValidation_RejectsPresentationAndAudioBeforeRuntimeMutation()
        {
            var presentation = CreatePresentationHarness();
            var audio = CreateAudioHarness();
            var runner = CreateRunner();
            var service = CreateCheckpointService();
            var tracker = new VNPlayTimeTracker();
            var coordinator = new VNYarnSaveCoordinator(repository, service, runner, tracker, presentation.Controller, presentation.Transition, audio.Controller);
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 1);
            Assert.That(service.TryEnterCheckpoint(CheckpointId, runner, out _), Is.True);
            runner.VariableStorage.SetAllVariables(new Dictionary<string, float> { ["$before"] = 4f }, new Dictionary<string, string>(), new Dictionary<string, bool>(), true);
            Assert.That(presentation.Controller.SetBackground("bg_b"), Is.True);
            audio.SourceA.clip = audio.ClipA;
            SetPrivate(audio.Controller, "currentBgmId", "bgm_a");

            var invalidPresentation = CreateSave(key, CreatePresentationState("missing_bg", string.Empty), new AudioState { bgmId = string.Empty, playbackSeconds = 0f });
            Assert.That(repository.Write(key, invalidPresentation).Succeeded, Is.True);
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidPresentation));
            AssertPreMutationState(service, runner, presentation, audio);

            var invalidAudio = CreateSave(key, CreatePresentationState("bg_a", string.Empty), new AudioState { bgmId = "missing_bgm", playbackSeconds = 0f });
            Assert.That(repository.Write(key, invalidAudio).Succeeded, Is.True);
            Assert.That(coordinator.ValidateLoad(key).Status, Is.EqualTo(VNYarnLoadValidationStatus.InvalidAudio));
            AssertPreMutationState(service, runner, presentation, audio);
        }

        private void AssertPreMutationState(VNCheckpointService service, DialogueRunner runner, PresentationHarness presentation, AudioHarness audio)
        {
            Assert.That(service.TryGetCurrentCheckpoint(out var context), Is.True);
            Assert.That(context.CheckpointId, Is.EqualTo(CheckpointId));
            Assert.That(runner.VariableStorage.GetAllVariables().FloatVariables["$before"], Is.EqualTo(4f));
            Assert.That(presentation.Controller.CurrentBackgroundId, Is.EqualTo("bg_b"));
            Assert.That(audio.Controller.CurrentBgmId, Is.EqualTo("bgm_a"));
            Assert.That(runner.IsDialogueRunning, Is.False);
        }

        [Test]
        public void SemanticSpeakerAliases_SharePhysicalIdentity_AndThoughtsNeverShowHiddenCharacters()
        {
            var presentation = CreatePresentationHarness();
            var catalog = (VNPresentationCatalog)typeof(VNPresentationController)
                .GetField("catalog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(presentation.Controller);
            var definitions = (List<VNCharacterDefinition>)typeof(VNPresentationCatalog)
                .GetField("characterDefinitions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(catalog);
            SetPrivate(definitions[0], "speakerAliases", new List<string> { "지훈", "지훈·독백", "지훈·내부" });
            SetPrivate(definitions[1], "speakerAliases", new List<string> { "로프·내부" });
            foreach (var label in new[] { "지훈", "지훈·독백", "지훈·내부" })
            {
                Assert.That(catalog.TryResolveSpeakerAlias(label, out var id), Is.True);
                Assert.That(id, Is.EqualTo("char_a"));
            }
            Assert.That(presentation.Controller.ShowCharacter("char_a", "default", VNCharacterSlot.Left), Is.True);
            SetPrivate(presentation.Controller, "speakerHighlightDuration", 0f);
            foreach (var label in new[] { "지훈·독백", "지훈·내부", "로프·내부", "지훈·화면 밖" })
            {
                presentation.Controller.FocusSpeaker(VNSpeakerFocusPresenter.ShouldFocusSpeaker(label) ? label : string.Empty);
                Assert.That(presentation.Controller.VisibleCharacters.Count, Is.EqualTo(1));
                Assert.That(presentation.Controller.VisibleCharacters.ContainsKey("char_b"), Is.False);
                Assert.That(presentation.Controller.VisibleCharacters["char_a"].SpeakerActive, Is.True);
            }
            Assert.That(presentation.Controller.ShowCharacter("char_b", "default", VNCharacterSlot.Right), Is.True);
            presentation.Controller.FocusSpeaker("지훈");
            Assert.That(presentation.Controller.VisibleCharacters["char_a"].SpeakerActive, Is.True);
            Assert.That(presentation.Controller.VisibleCharacters["char_b"].SpeakerActive, Is.False);
        }

        private PresentationHarness CreatePresentationHarness(VNPresentationCatalog existingCatalog = null)
        {
            var spriteA = CreateSprite("A");
            var spriteB = CreateSprite("B");
            var catalog = existingCatalog;
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<VNPresentationCatalog>();
                ownedObjects.Add(catalog);
                var characterA = CreateCharacterDefinition("char_a", spriteA);
                var characterB = CreateCharacterDefinition("char_b", spriteB);
                SetPrivate(catalog, "characterDefinitions", new List<VNCharacterDefinition> { characterA, characterB });
                SetPrivate(catalog, "backgrounds", new List<VNSpriteCatalogEntry> { CreateSpriteEntry("bg_a", spriteA), CreateSpriteEntry("bg_b", spriteB) });
                SetPrivate(catalog, "cgs", new List<VNSpriteCatalogEntry> { CreateSpriteEntry("cg_a", spriteA), CreateSpriteEntry("cg_b", spriteB) });
            }

            var background = CreateImage("Background", out var backgroundFade);
            var cg = CreateImage("CG", out var cgFade);
            var incoming = CreateImage("Incoming", out var incomingFade);
            var screen = CreateCanvasGroup("Screen Fade");
            var views = new List<VNCharacterSlotView>();
            var viewsBySlot = new Dictionary<VNCharacterSlot, VNCharacterSlotView>();
            foreach (VNCharacterSlot slot in Enum.GetValues(typeof(VNCharacterSlot)))
            {
                var view = CreateSlotView(slot);
                views.Add(view);
                viewsBySlot.Add(slot, view);
            }

            var controllerObject = new GameObject("Presentation Controller");
            ownedObjects.Add(controllerObject);
            var controller = controllerObject.AddComponent<VNPresentationController>();
            SetPrivate(controller, "catalog", catalog);
            SetPrivate(controller, "backgroundImage", background);
            SetPrivate(controller, "cgImage", cg);
            SetPrivate(controller, "characterSlotViews", views);
            SetPrivate(controller, "speakerHighlightDuration", 0f);
            InvokePrivate(controller, "BuildSlotIndex");

            var transitionObject = new GameObject("Transition Controller");
            ownedObjects.Add(transitionObject);
            var transition = transitionObject.AddComponent<VNTransitionController>();
            SetPrivate(transition, "presentationController", controller);
            SetPrivate(transition, "screenFadeCanvasGroup", screen);
            SetPrivate(transition, "backgroundCurrentCanvasGroup", backgroundFade);
            SetPrivate(transition, "backgroundIncomingImage", incoming);
            SetPrivate(transition, "backgroundIncomingCanvasGroup", incomingFade);
            SetPrivate(transition, "cgCanvasGroup", cgFade);
            return new PresentationHarness(controller, transition, viewsBySlot, screen, incoming, incomingFade, spriteA);
        }

        private AudioHarness CreateAudioHarness(VNAudioCatalog existingCatalog = null)
        {
            var clipA = AudioClip.Create("Bgm A", 1000, 1, 1000, false);
            var clipB = AudioClip.Create("Bgm B", 1000, 1, 1000, false);
            ownedObjects.Add(clipA);
            ownedObjects.Add(clipB);
            var catalog = existingCatalog;
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<VNAudioCatalog>();
                ownedObjects.Add(catalog);
                SetPrivate(catalog, "bgm", new List<VNBgmCatalogEntry> { CreateBgmEntry("bgm_a", clipA, 0.7f, true), CreateBgmEntry("bgm_b", clipB, 0.4f, false) });
                SetPrivate(catalog, "sfx", new List<VNSfxCatalogEntry> { CreateSfxEntry("sfx_test", clipA) });
            }

            var root = new GameObject("Audio Controller");
            ownedObjects.Add(root);
            var sourceA = root.AddComponent<AudioSource>();
            var sourceB = root.AddComponent<AudioSource>();
            var sfx = root.AddComponent<AudioSource>();
            var controller = root.AddComponent<VNAudioController>();
            SetPrivate(controller, "catalog", catalog);
            SetPrivate(controller, "bgmSourceA", sourceA);
            SetPrivate(controller, "bgmSourceB", sourceB);
            SetPrivate(controller, "sfxSource", sfx);
            return new AudioHarness(controller, sourceA, sourceB, sfx, clipA, clipB);
        }

        private DialogueRunner CreateRunner()
        {
            var gameObject = new GameObject("M5 Full Snapshot Runner");
            ownedObjects.Add(gameObject);
            var runner = gameObject.AddComponent<DialogueRunner>();
            var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            Assert.That(yarnProject, Is.Not.Null);
            runner.SetProject(yarnProject);
            return runner;
        }

        private VNCheckpointService CreateCheckpointService()
        {
            var catalog = ScriptableObject.CreateInstance<VNCheckpointCatalog>();
            ownedObjects.Add(catalog);
            JsonUtility.FromJsonOverwrite("{\"checkpointDefinitions\":[{\"checkpointId\":\"m5_checkpoint\",\"resumeNode\":\"M1_RUNTIME_START\",\"chapterId\":\"m5_chapter\",\"sceneTitle\":\"M5 test checkpoint\"}]}", catalog);
            var gameObject = new GameObject("M5 Full Snapshot Checkpoint Service");
            ownedObjects.Add(gameObject);
            var service = gameObject.AddComponent<VNCheckpointService>();
            SetPrivate(service, "checkpointCatalog", catalog);
            return service;
        }

        private SaveSlotData CreateSave(VNSaveSlotKey key, PresentationState presentationState, AudioState audioState)
        {
            return new SaveSlotData
            {
                schemaVersion = 1,
                slotType = key.ToSerializedSlotType(),
                slotIndex = key.SlotIndex,
                checkpointId = CheckpointId,
                resumeNode = ResumeNode,
                yarnVariables = new YarnVariablesData { floats = Array.Empty<FloatVariableEntry>(), strings = Array.Empty<StringVariableEntry>(), bools = Array.Empty<BoolVariableEntry>() },
                presentationState = presentationState,
                audioState = audioState,
                chapterId = "m5_chapter",
                sceneTitle = "M5 test checkpoint",
                playedSeconds = 4f,
                savedAtUtcIso8601 = "2026-08-19T01:00:00.0000000+00:00",
                thumbnailFileName = string.Empty,
            };
        }

        private VNCharacterDefinition CreateCharacterDefinition(string id, Sprite sprite)
        {
            var character = ScriptableObject.CreateInstance<VNCharacterDefinition>();
            ownedObjects.Add(character);
            var expression = new VNExpressionDefinition();
            SetPrivate(expression, "expressionId", "default");
            SetPrivate(expression, "headSprite", sprite);
            SetPrivate(character, "characterId", id);
            SetPrivate(character, "speakerAliases", new List<string>());
            SetPrivate(character, "defaultFacing", VNCharacterFacing.Right);
            SetPrivate(character, "defaultScale", 1f);
            SetPrivate(character, "bodySprite", sprite);
            SetPrivate(character, "defaultExpressionId", "default");
            SetPrivate(character, "expressions", new List<VNExpressionDefinition> { expression });
            return character;
        }

        private static VNSpriteCatalogEntry CreateSpriteEntry(string id, Sprite sprite)
        {
            var entry = new VNSpriteCatalogEntry();
            SetPrivate(entry, "id", id);
            SetPrivate(entry, "sprite", sprite);
            return entry;
        }

        private static VNBgmCatalogEntry CreateBgmEntry(string id, AudioClip clip, float volume, bool loop)
        {
            var entry = new VNBgmCatalogEntry();
            SetPrivate(entry, "id", id);
            SetPrivate(entry, "clip", clip);
            SetPrivate(entry, "defaultVolume", volume);
            SetPrivate(entry, "loop", loop);
            return entry;
        }

        private static VNSfxCatalogEntry CreateSfxEntry(string id, AudioClip clip)
        {
            var entry = new VNSfxCatalogEntry();
            SetPrivate(entry, "id", id);
            SetPrivate(entry, "clip", clip);
            SetPrivate(entry, "defaultVolume", 1f);
            return entry;
        }

        private Sprite CreateSprite(string name)
        {
            var texture = new Texture2D(2, 2) { name = name + " Texture" };
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
            ownedObjects.Add(sprite);
            ownedObjects.Add(texture);
            return sprite;
        }

        private Image CreateImage(string name, out CanvasGroup canvasGroup)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            ownedObjects.Add(gameObject);
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            return gameObject.AddComponent<Image>();
        }

        private CanvasGroup CreateCanvasGroup(string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            ownedObjects.Add(gameObject);
            return gameObject.GetComponent<CanvasGroup>();
        }

        private VNCharacterSlotView CreateSlotView(VNCharacterSlot slot)
        {
            var gameObject = new GameObject("Slot " + slot, typeof(RectTransform), typeof(CanvasGroup));
            ownedObjects.Add(gameObject);
            var view = gameObject.AddComponent<VNCharacterSlotView>();
            SetPrivate(view, "slot", slot);
            SetPrivate(view, "visualRoot", gameObject.GetComponent<RectTransform>());
            SetPrivate(view, "backHairImage", CreateSlotLayer(gameObject.transform, "Back Hair"));
            SetPrivate(view, "bodyImage", CreateSlotLayer(gameObject.transform, "Body"));
            SetPrivate(view, "headImage", CreateSlotLayer(gameObject.transform, "Head"));
            SetPrivate(view, "fadeCanvasGroup", gameObject.GetComponent<CanvasGroup>());
            return view;
        }

        private Image CreateSlotLayer(Transform parent, string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            ownedObjects.Add(gameObject);
            gameObject.transform.SetParent(parent, false);
            return gameObject.AddComponent<Image>();
        }

        private static PresentationState CreatePresentationState(string backgroundId, string cgId, params CharacterSaveState[] characters)
        {
            return new PresentationState { backgroundId = backgroundId, cgId = cgId, characters = characters };
        }

        private static PresentationState ClonePresentationState(PresentationState state)
        {
            var characters = new CharacterSaveState[state.characters.Length];
            for (var index = 0; index < state.characters.Length; index++)
            {
                var character = state.characters[index];
                characters[index] = Character(character.characterId, character.expressionId, character.slot, character.facing, character.scale);
            }
            return CreatePresentationState(state.backgroundId, state.cgId, characters);
        }

        private static CharacterSaveState Character(string id, string expression, string slot, string facing, float scale)
        {
            return new CharacterSaveState { characterId = id, expressionId = expression, slot = slot, facing = facing, scale = scale };
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing private field {target.GetType().Name}.{fieldName}");
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing private method {target.GetType().Name}.{methodName}");
            method.Invoke(target, null);
        }

        private sealed class PresentationHarness
        {
            public VNPresentationController Controller { get; }
            public VNTransitionController Transition { get; }
            public IReadOnlyDictionary<VNCharacterSlot, VNCharacterSlotView> Views { get; }
            public CanvasGroup ScreenFade { get; }
            public Image IncomingImage { get; }
            public CanvasGroup IncomingFade { get; }
            public Sprite SpriteA { get; }

            public PresentationHarness(VNPresentationController controller, VNTransitionController transition, IReadOnlyDictionary<VNCharacterSlot, VNCharacterSlotView> views, CanvasGroup screenFade, Image incomingImage, CanvasGroup incomingFade, Sprite spriteA)
            {
                Controller = controller;
                Transition = transition;
                Views = views;
                ScreenFade = screenFade;
                IncomingImage = incomingImage;
                IncomingFade = incomingFade;
                SpriteA = spriteA;
            }
        }

        private sealed class AudioHarness
        {
            public VNAudioController Controller { get; }
            public AudioSource SourceA { get; }
            public AudioSource SourceB { get; }
            public AudioSource SfxSource { get; }
            public AudioClip ClipA { get; }
            public AudioClip ClipB { get; }

            public AudioHarness(VNAudioController controller, AudioSource sourceA, AudioSource sourceB, AudioSource sfxSource, AudioClip clipA, AudioClip clipB)
            {
                Controller = controller;
                SourceA = sourceA;
                SourceB = sourceB;
                SfxSource = sfxSource;
                ClipA = clipA;
                ClipB = clipB;
            }
        }
    }
}
