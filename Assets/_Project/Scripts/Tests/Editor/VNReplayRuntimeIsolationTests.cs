using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records.Replay;
using ProjectAllTime.VN.SaveLoad;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Yarn;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNReplayRuntimeIsolationTests
    {
        private const string YarnProjectPath = "Assets/_Project/Yarn/GameNarrative.yarnproject";
        private static readonly string[] MetaProgressMutationCommands =
        {
            "vn_unlock_cg",
            "vn_unlock_chapter",
            "vn_unlock_archive",
            "vn_unlock_achievement",
            "vn_complete_ending",
        };

        private string temporaryMetaRoot;
        private string temporarySaveRoot;
        private string metaCanonicalPath;
        private AudioSnapshot productionAudioSnapshot;
        private bool playModeOptionsCaptured;
        private bool previousEnterPlayModeOptionsEnabled;
        private EnterPlayModeOptions previousEnterPlayModeOptions;
        private bool backgroundSettingCaptured;
        private bool previousRunInBackground;

        [TearDown]
        public void TearDown()
        {
            if (backgroundSettingCaptured) Application.runInBackground = previousRunInBackground;
            backgroundSettingCaptured = false;
            RestorePlayModeOptions();
            DeleteTestDirectory(temporaryMetaRoot);
            DeleteTestDirectory(temporarySaveRoot);
            temporaryMetaRoot = null;
            temporarySaveRoot = null;
            metaCanonicalPath = null;
            productionAudioSnapshot = null;
        }

        [Test]
        public void CompiledProgramValidator_AcceptsOnlyExplicitLinearVisualClosure()
        {
            var project = LoadYarnProject();
            Assert.That(project, Is.Not.Null);
            Assert.That(project.Program, Is.Not.Null);

            var replayLineIds = project.GetLineIDsForNodes(project.NodeNames.Where(name => name.StartsWith("M9_REPLAY_", StringComparison.Ordinal))).ToArray();
            CollectionAssert.AreEquivalent(new[]
            {
                "line:m9_replay_safe_01",
                "line:m9_replay_safe_02",
                "line:m9_replay_safe_03",
                "line:m9_replay_safe_04",
                "line:m9_replay_safe_05",
                "line:m9_replay_safe_06",
                "line:m9_replay_prod_01",
                "line:m9_replay_prod_02",
                "line:m9_replay_cancel_01",
                "line:m9_replay_option_prompt",
                "line:m9_replay_option_01",
                "line:m9_replay_unsafe_unknown_01",
                "line:m9_replay_unsafe_target_01",
            }, replayLineIds);
            Assert.That(replayLineIds.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(13), "All thirteen M9-04 fixture lines/options have unique explicit IDs.");

            var safePolicy = CreatePolicy("M9_REPLAY_SAFE_START", "M9_REPLAY_CANCEL");
            var valid = VNReplayContentValidator.Validate(project, "M9_REPLAY_SAFE_START", safePolicy);
            Assert.That(valid.IsValid, Is.True, valid.Diagnostic);
            CollectionAssert.Contains(valid.ReachableNodes, "M9_REPLAY_SAFE_START");

            var invalidRoot = VNReplayContentValidator.Validate(project, "M9_REPLAY_MISSING", safePolicy);
            Assert.That(invalidRoot.IsValid, Is.False);
            StringAssert.Contains("not explicitly approved", invalidRoot.Diagnostic);

            var optionResult = ValidateSingleRoot(project, "M9_REPLAY_UNSAFE_OPTIONS");
            Assert.That(optionResult.IsValid, Is.False);
            StringAssert.Contains("interactive options", optionResult.Diagnostic);

            var forbiddenNodes = new Dictionary<string, string>
            {
                ["M9_REPLAY_UNSAFE_CHECKPOINT"] = "vn_checkpoint",
                ["M9_REPLAY_UNSAFE_UNLOCK_CG"] = "vn_unlock_cg",
                ["M9_REPLAY_UNSAFE_UNLOCK_CHAPTER"] = "vn_unlock_chapter",
                ["M9_REPLAY_UNSAFE_UNLOCK_ARCHIVE"] = "vn_unlock_archive",
                ["M9_REPLAY_UNSAFE_UNLOCK_ACHIEVEMENT"] = "vn_unlock_achievement",
                ["M9_REPLAY_UNSAFE_COMPLETE_ENDING"] = "vn_complete_ending",
                ["M9_REPLAY_UNSAFE_BGM"] = "bgm_play",
                ["M9_REPLAY_UNSAFE_SFX"] = "sfx_play",
                ["M9_REPLAY_UNSAFE_UNKNOWN"] = "m9_replay_unknown_command",
            };

            foreach (var item in forbiddenNodes)
            {
                var result = ValidateSingleRoot(project, item.Key);
                Assert.That(result.IsValid, Is.False, item.Key);
                StringAssert.Contains(item.Value, result.Diagnostic, item.Key);
            }

            var jumpPolicy = CreatePolicy("M9_REPLAY_UNSAFE_JUMP");
            var jumpResult = VNReplayContentValidator.Validate(project, "M9_REPLAY_UNSAFE_JUMP", jumpPolicy);
            Assert.That(jumpResult.IsValid, Is.False);
            StringAssert.Contains("M9_REPLAY_UNSAFE_TARGET", jumpResult.Diagnostic);

            foreach (var command in MetaProgressMutationCommands)
                Assert.That(VNReplayContentPolicy.IsVisualCommandAllowed(command), Is.False, command);
            Assert.That(VNReplayContentPolicy.IsVisualCommandAllowed("vn_bg replay_background"), Is.True);
            Assert.That(VNReplayContentPolicy.IsVisualCommandAllowed("vn_clear_cg"), Is.True);

            TestContext.WriteLine("Compiled Yarn validator: 13 explicit fixture line/option IDs PASS; safe root accepted; option, checkpoint, 5 MetaProgress, 2 audio, unknown command, and unapproved jump rejected.");
        }

        [UnityTest]
        public IEnumerator ConcurrentProductionAndReplayRunnersRemainIsolatedInRealPlayMode()
        {
            EnablePlayModeWithoutDomainReload();
            yield return new EnterPlayMode(expectDomainReload: false);
            previousRunInBackground = Application.runInBackground;
            backgroundSettingCaptured = true;
            Application.runInBackground = true; // Required for awaited Play work during an unfocused MCP test run.

            // Build persistence fixtures only after Unity's Play Mode domain transition so
            // no test-owned service or repository instance crosses a domain reload.
            var metaWriteCounter = new WriteCounter();
            temporaryMetaRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M9ReplayMeta_" + Guid.NewGuid().ToString("N"));
            temporarySaveRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M9ReplaySave_" + Guid.NewGuid().ToString("N"));
            var metaRepository = VNMetaProgressRepository.CreateForTesting(temporaryMetaRoot, () => metaWriteCounter.Value++);
            metaCanonicalPath = metaRepository.CanonicalFilePath;
            var metaProgress = new VNMetaProgressService(metaRepository);
            metaProgress.Load();
            Assert.That(metaProgress.TryUnlockCG("m9_replay_seed_cg"), Is.True);
            Assert.That(metaWriteCounter.Value, Is.EqualTo(1));
            var metaJsonBefore = JsonUtility.ToJson(metaProgress.Current, true);
            var metaBytesBefore = File.ReadAllBytes(metaRepository.CanonicalFilePath);
            var metaHashBefore = Sha256(metaBytesBefore);

            var saveRepository = VNSaveRepository.CreateForTesting(temporarySaveRoot);
            var saveKeys = new[]
            {
                new VNSaveSlotKey(VNSaveSlotType.Auto, 0),
                new VNSaveSlotKey(VNSaveSlotType.Quick, 0),
                new VNSaveSlotKey(VNSaveSlotType.Manual, 0),
            };
            var saveFilePaths = new List<string>();
            foreach (var key in saveKeys)
            {
                Assert.That(saveRepository.Write(key, CreateSaveFixture(key)).Succeeded, Is.True);
                Assert.That(saveRepository.TryGetSlotPath(key, out var path), Is.True);
                saveFilePaths.Add(path);
            }
            var saveBytesBefore = saveFilePaths.Select(File.ReadAllBytes).ToArray();
            var saveHashesBefore = saveBytesBefore.Select(Sha256).ToArray();
            var project = LoadYarnProject();
            Assert.That(project, Is.Not.Null);
            Assert.That(project.Program, Is.Not.Null);

            var fixtureAssets = new List<UnityEngine.Object>();
            var catalog = CreatePresentationCatalog(fixtureAssets);
            var productionRoot = new GameObject("M9-04 Production Runtime Harness");
            productionRoot.SetActive(false);
            var productionStorage = productionRoot.AddComponent<InMemoryVariableStorage>();
            var productionRunner = productionRoot.AddComponent<DialogueRunner>();
            productionRunner.autoStart = false;
            productionRunner.VariableStorage = productionStorage;
            productionRunner.SetProject(project);
            productionStorage.Program = project.Program;
            productionStorage.SetValue("$replay_probe", 10f);

            var productionSession = productionRoot.AddComponent<VNDialogueSessionState>();
            Assert.That(productionSession.ReadHistory.ReplacePersistentBaseline(new[] { "m9_replay_seed_read" }), Is.True);

            var productionPresenterRoot = new GameObject("Production Presenter");
            productionPresenterRoot.transform.SetParent(productionRoot.transform, false);
            productionPresenterRoot.SetActive(false);
            var productionLineTextObject = new GameObject("Production Line Text", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            productionLineTextObject.transform.SetParent(productionPresenterRoot.transform, false);
            var productionLineText = productionLineTextObject.AddComponent<TextMeshProUGUI>();
            productionLineText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            var productionLinePresenter = productionPresenterRoot.AddComponent<LinePresenter>();
            productionLinePresenter.canvasGroup = productionLineTextObject.GetComponent<CanvasGroup>();
            productionLinePresenter.lineText = productionLineText;
            productionLinePresenter.useFadeEffect = false;
            productionLinePresenter.autoAdvance = false;
            SetPrivateField(
                productionLinePresenter,
                "typewriterStyle",
                Enum.Parse(typeof(LinePresenter).GetNestedType("TypewriterType", BindingFlags.NonPublic), "Instant"));
            var productionLifecyclePresenter = productionPresenterRoot.AddComponent<VNLineLifecyclePresenter>();
            SetPrivateField(productionLifecyclePresenter, "sessionState", productionSession);
            SetPrivateField(productionLifecyclePresenter, "linePresenter", productionLinePresenter);
            productionRunner.DialoguePresenters = new DialoguePresenterBase[]
            {
                productionLinePresenter,
                productionLifecyclePresenter,
            };
            productionPresenterRoot.SetActive(true);
            _ = productionRunner.LineProvider;

            var productionPresentation = CreatePresentationController(productionRoot.transform, catalog, "Production Presentation");
            Assert.That(productionPresentation.SetBackground("production_background"), Is.True);
            Assert.That(productionPresentation.CurrentCGId, Is.Null);

            var productionCheckpointService = productionRoot.AddComponent<VNCheckpointService>();
            var checkpointMutations = 0;
            productionCheckpointService.CheckpointEntered += _ => checkpointMutations++;
            var audioSourceA = productionRoot.AddComponent<AudioSource>();
            var audioSourceB = productionRoot.AddComponent<AudioSource>();
            var sfxSource = productionRoot.AddComponent<AudioSource>();
            audioSourceA.playOnAwake = audioSourceB.playOnAwake = sfxSource.playOnAwake = false;
            var productionAudio = productionRoot.AddComponent<VNAudioController>();
            SetPrivateField(productionAudio, "bgmSourceA", audioSourceA);
            SetPrivateField(productionAudio, "bgmSourceB", audioSourceB);
            SetPrivateField(productionAudio, "sfxSource", sfxSource);

            var productionMetaHandlerCalls = 0;
            var productionCheckpointHandlerCalls = 0;
            var productionAudioHandlerCalls = 0;
            productionRunner.AddCommandHandler<string>("vn_unlock_cg", _ => productionMetaHandlerCalls++);
            productionRunner.AddCommandHandler<string>("vn_checkpoint", _ => productionCheckpointHandlerCalls++);
            productionRunner.AddCommandHandler<string>("bgm_play", _ => productionAudioHandlerCalls++);

            productionRoot.SetActive(true);
            Assert.That(productionRunner.autoStart, Is.False, "The controlled harness starts production explicitly; VN_Main remains untouched.");

            productionRunner.StartDialogue("M9_REPLAY_PRODUCTION_WAIT").Forget();
            yield return WaitFor(
                () => productionSession.IsLineActive && productionSession.IsCurrentLineFullyDisplayed && productionSession.Backlog.Count == 1,
                "Production runner did not reach a fully displayed line.");

            Assert.That(productionRunner.IsDialogueRunning, Is.True);
            Assert.That(productionSession.CurrentLineId, Is.EqualTo("line:m9_replay_prod_01"));
            Assert.That(productionLineText.text, Does.Contain("10"));
            Assert.That(productionStorage.TryGetValue<float>("$replay_probe", out var productionProbeBefore), Is.True);
            Assert.That(productionProbeBefore, Is.EqualTo(10f));
            Assert.That(productionPresentation.CurrentBackgroundId, Is.EqualTo("production_background"));
            Assert.That(productionPresentation.CurrentCGId, Is.Null);
            Assert.That(productionAudio.CurrentBgmId, Is.Empty);
            Assert.That(productionAudio.IsBgmTransitionActive, Is.False);
            Assert.That(productionCheckpointService.HasCurrentCheckpoint, Is.False);

            var productionSnapshot = CaptureProductionSnapshot(productionSession);
            productionAudioSnapshot = CaptureAudioSnapshot(productionAudio, audioSourceA, audioSourceB, sfxSource);
            var safeReplayPolicy = CreatePolicy("M9_REPLAY_SAFE_START", "M9_REPLAY_CANCEL");

            var replayA = CreateReplaySession(project, safeReplayPolicy, catalog);
            replayA.VariableStorage.SetValue("$replay_probe", 99f);
            AssertReplayOwnership(productionRunner, productionStorage, replayA);
            Assert.That(replayA.RuntimeHandlersRegistered, Is.True);
            Assert.That(replayA.TryStart("M9_REPLAY_SAFE_START", out var replayStartDiagnostic), Is.True, replayStartDiagnostic);
            var replayPresenterA = (VNReplayTestPresenter)replayA.Presenter;

            yield return WaitFor(() => replayPresenterA.LineIds.Count == 1, "Replay A did not receive its first Yarn line.");
            Assert.That(productionRunner.IsDialogueRunning, Is.True);
            Assert.That(replayA.Runner.IsDialogueRunning, Is.True);
            Assert.That(replayPresenterA.LineIds[0], Is.EqualTo("line:m9_replay_safe_01"));
            StringAssert.Contains("99", replayPresenterA.Texts[0]);
            AssertReplayIsolation(productionRunner, productionStorage, productionSession, productionPresentation, productionAudio,
                audioSourceA, audioSourceB, sfxSource, productionSnapshot, productionProbeBefore,
                productionMetaHandlerCalls, productionCheckpointHandlerCalls, productionAudioHandlerCalls,
                checkpointMutations, metaProgress, metaWriteCounter.Value, metaJsonBefore, metaBytesBefore, saveFilePaths, saveBytesBefore);
            Assert.That(replayA.VariableStorage.TryGetValue<float>("$replay_probe", out var replayProbeDuring), Is.True);
            Assert.That(replayProbeDuring, Is.EqualTo(99f));

            replayA.Runner.RequestNextLine();
            yield return WaitFor(() => replayPresenterA.LineIds.Count == 2, "Replay A did not reach its background presentation line.");
            Assert.That(replayA.PresentationController.CurrentBackgroundId, Is.EqualTo("replay_background"));
            Assert.That(productionPresentation.CurrentBackgroundId, Is.EqualTo("production_background"));
            Assert.That(productionPresentation.CurrentCGId, Is.Null);
            AssertReplayIsolation(productionRunner, productionStorage, productionSession, productionPresentation, productionAudio,
                audioSourceA, audioSourceB, sfxSource, productionSnapshot, productionProbeBefore,
                productionMetaHandlerCalls, productionCheckpointHandlerCalls, productionAudioHandlerCalls,
                checkpointMutations, metaProgress, metaWriteCounter.Value, metaJsonBefore, metaBytesBefore, saveFilePaths, saveBytesBefore);

            replayA.Runner.RequestNextLine();
            yield return WaitFor(() => replayPresenterA.LineIds.Count == 3, "Replay A did not reach its CG presentation line.");
            Assert.That(replayA.PresentationController.CurrentBackgroundId, Is.EqualTo("replay_background"));
            Assert.That(replayA.PresentationController.CurrentCGId, Is.EqualTo("m3_cg"));
            Assert.That(productionPresentation.CurrentBackgroundId, Is.EqualTo("production_background"));
            Assert.That(productionPresentation.CurrentCGId, Is.Null);
            AssertReplayIsolation(productionRunner, productionStorage, productionSession, productionPresentation, productionAudio,
                audioSourceA, audioSourceB, sfxSource, productionSnapshot, productionProbeBefore,
                productionMetaHandlerCalls, productionCheckpointHandlerCalls, productionAudioHandlerCalls,
                checkpointMutations, metaProgress, metaWriteCounter.Value, metaJsonBefore, metaBytesBefore, saveFilePaths, saveBytesBefore);

            replayA.Runner.RequestNextLine();
            yield return WaitFor(() => replayPresenterA.LineIds.Count == 4, "Replay A did not reach its CG-clear line.");
            Assert.That(replayA.PresentationController.CurrentCGId, Is.Null);
            replayA.Runner.RequestNextLine();
            yield return WaitFor(() => replayPresenterA.LineIds.Count == 5, "Replay A did not reach its character presentation line.");
            Assert.That(replayA.PresentationController.VisibleCharacters.TryGetValue("m9_replay_character", out var replayCharacter), Is.True);
            Assert.That(replayCharacter.ExpressionId, Is.EqualTo("replay_expression"));
            Assert.That(replayCharacter.Slot, Is.EqualTo(VNCharacterSlot.Left));
            Assert.That(replayCharacter.Facing, Is.EqualTo(VNCharacterFacing.Left));
            Assert.That(replayCharacter.Scale, Is.EqualTo(1.25f));
            Assert.That(replayA.PresentationController.TryGetVisibleCharacterSlotView("m9_replay_character", out var replayCharacterView), Is.True);
            Assert.That(replayCharacterView.transform.localScale.x, Is.EqualTo(-1.25f));
            Assert.That(productionPresentation.VisibleCharacters, Is.Empty);
            AssertReplayIsolation(productionRunner, productionStorage, productionSession, productionPresentation, productionAudio,
                audioSourceA, audioSourceB, sfxSource, productionSnapshot, productionProbeBefore,
                productionMetaHandlerCalls, productionCheckpointHandlerCalls, productionAudioHandlerCalls,
                checkpointMutations, metaProgress, metaWriteCounter.Value, metaJsonBefore, metaBytesBefore, saveFilePaths, saveBytesBefore);

            replayA.Runner.RequestNextLine();
            yield return WaitFor(() => replayPresenterA.LineIds.Count == 6, "Replay A did not reach its character-clear line.");
            Assert.That(replayA.PresentationController.VisibleCharacters, Is.Empty);
            Assert.That(replayPresenterA.LineIds, Is.EqualTo(new[]
            {
                "line:m9_replay_safe_01", "line:m9_replay_safe_02", "line:m9_replay_safe_03",
                "line:m9_replay_safe_04", "line:m9_replay_safe_05", "line:m9_replay_safe_06",
            }));
            replayA.Runner.RequestNextLine();
            yield return WaitFor(() => !replayA.Runner.IsDialogueRunning && replayA.Runner.DialogueTask.IsCompletedSuccessfully(),
                "Replay A did not complete normally.");
            Assert.That(replayA.Lifecycle, Is.EqualTo(VNReplaySessionLifecycle.Completed));
            Assert.That(replayPresenterA.DialogueCompleted, Is.True);
            Assert.That(replayPresenterA.OptionsReceived, Is.False);
            Assert.That(replayA.VariableStorage.TryGetValue<float>("$replay_probe", out var replayProbeAfter), Is.True);
            Assert.That(replayProbeAfter, Is.EqualTo(99f));
            AssertReplayIsolation(productionRunner, productionStorage, productionSession, productionPresentation, productionAudio,
                audioSourceA, audioSourceB, sfxSource, productionSnapshot, productionProbeBefore,
                productionMetaHandlerCalls, productionCheckpointHandlerCalls, productionAudioHandlerCalls,
                checkpointMutations, metaProgress, metaWriteCounter.Value, metaJsonBefore, metaBytesBefore, saveFilePaths, saveBytesBefore);
            var completedReplayRoot = replayA.Runner.gameObject;
            replayA.DisposeAsync().Forget();
            yield return WaitFor(() => completedReplayRoot == null, "Dispose after Replay completion did not destroy its owned hierarchy.");
            Assert.That(replayA.Lifecycle, Is.EqualTo(VNReplaySessionLifecycle.Disposed));
            Assert.That(replayA.RuntimeHandlersRegistered, Is.False);
            Assert.That(productionStorage.TryGetValue<float>("$replay_probe", out var productionProbeAfterA), Is.True);
            Assert.That(productionProbeAfterA, Is.EqualTo(10f));

            // Replay B proves cancellation and that a newly created storage receives only the Yarn initial value.
            var replayB = CreateReplaySession(project, safeReplayPolicy, catalog);
            Assert.That(replayB.VariableStorage.TryGetValue<float>("$replay_probe", out var freshReplayProbe), Is.True);
            Assert.That(freshReplayProbe, Is.EqualTo(0f));
            Assert.That(replayB.TryStart("M9_REPLAY_CANCEL", out var replayCancelDiagnostic), Is.True, replayCancelDiagnostic);
            var replayPresenterB = (VNReplayTestPresenter)replayB.Presenter;
            yield return WaitFor(() => replayPresenterB.LineIds.Count == 1, "Replay B did not reach its cancellable line.");
            Assert.That(replayB.PresentationController.CurrentBackgroundId, Is.EqualTo("replay_background"));
            Assert.That(replayB.PresentationController.CurrentCGId, Is.EqualTo("m3_cg"));
            replayB.CancelAsync().Forget();
            yield return WaitFor(() => replayB.Lifecycle == VNReplaySessionLifecycle.Cancelled && !replayB.IsDialogueRunning,
                "Replay B cancellation did not complete through DialogueRunner.Stop.");
            Assert.That(replayPresenterB.DialogueCompleted, Is.True);
            Assert.That(replayB.PresentationController.CurrentBackgroundId, Is.Null);
            Assert.That(replayB.PresentationController.CurrentCGId, Is.Null);
            AssertReplayIsolation(productionRunner, productionStorage, productionSession, productionPresentation, productionAudio,
                audioSourceA, audioSourceB, sfxSource, productionSnapshot, productionProbeBefore,
                productionMetaHandlerCalls, productionCheckpointHandlerCalls, productionAudioHandlerCalls,
                checkpointMutations, metaProgress, metaWriteCounter.Value, metaJsonBefore, metaBytesBefore, saveFilePaths, saveBytesBefore);
            var cancelledReplayRoot = replayB.Runner.gameObject;
            replayB.DisposeAsync().Forget();
            yield return WaitFor(() => cancelledReplayRoot == null, "Dispose after Replay cancellation did not destroy its owned hierarchy.");
            Assert.That(replayB.RuntimeHandlersRegistered, Is.False);

            // Replay A again confirms that no handler, presenter, storage, or GameObject from A/B leaked.
            var replayA2 = CreateReplaySession(project, safeReplayPolicy, catalog);
            replayA2.VariableStorage.SetValue("$replay_probe", 99f);
            Assert.That(replayA2.TryStart("M9_REPLAY_SAFE_START", out var replayA2Diagnostic), Is.True, replayA2Diagnostic);
            var replayPresenterA2 = (VNReplayTestPresenter)replayA2.Presenter;
            yield return WaitFor(() => replayPresenterA2.LineIds.Count == 1, "Second Replay A did not start.");
            Assert.That(replayPresenterA2.LineIds[0], Is.EqualTo("line:m9_replay_safe_01"));
            Assert.That(replayPresenterA2.Texts[0], Does.Contain("99"));
            for (var lineIndex = 0; lineIndex < 6; lineIndex++)
            {
                if (lineIndex > 0)
                    yield return WaitFor(() => replayPresenterA2.LineIds.Count == lineIndex + 1, "Second Replay A stalled before its next line.");
                replayA2.Runner.RequestNextLine();
            }
            yield return WaitFor(() => !replayA2.Runner.IsDialogueRunning && replayA2.Runner.DialogueTask.IsCompletedSuccessfully(),
                "Second Replay A did not complete normally.");
            Assert.That(replayA2.Lifecycle, Is.EqualTo(VNReplaySessionLifecycle.Completed));
            Assert.That(replayPresenterA2.DialogueCompleted, Is.True);
            AssertReplayIsolation(productionRunner, productionStorage, productionSession, productionPresentation, productionAudio,
                audioSourceA, audioSourceB, sfxSource, productionSnapshot, productionProbeBefore,
                productionMetaHandlerCalls, productionCheckpointHandlerCalls, productionAudioHandlerCalls,
                checkpointMutations, metaProgress, metaWriteCounter.Value, metaJsonBefore, metaBytesBefore, saveFilePaths, saveBytesBefore);
            var replayA2Root = replayA2.Runner.gameObject;
            replayA2.DisposeAsync().Forget();
            yield return WaitFor(() => replayA2Root == null, "Second Replay A did not clean up its hierarchy.");

            // Invalid roots and unsafe content fail before any Yarn instruction executes.
            var invalidRootSession = CreateReplaySession(project, safeReplayPolicy, catalog);
            Assert.That(invalidRootSession.TryStart("M9_REPLAY_MISSING", out var invalidRootDiagnostic), Is.False);
            StringAssert.Contains("not explicitly approved", invalidRootDiagnostic);
            Assert.That(((VNReplayTestPresenter)invalidRootSession.Presenter).LineIds, Is.Empty);
            Assert.That(invalidRootSession.IsDialogueRunning, Is.False);
            invalidRootSession.DisposeAsync().Forget();
            var invalidRootHierarchy = invalidRootSession.Runner.gameObject;
            yield return WaitFor(() => invalidRootHierarchy == null, "Invalid-root Replay resources were not disposable.");

            var unsafePolicy = CreatePolicy("M9_REPLAY_UNSAFE_CHECKPOINT");
            var unsafeSession = CreateReplaySession(project, unsafePolicy, catalog);
            Assert.That(unsafeSession.TryStart("M9_REPLAY_UNSAFE_CHECKPOINT", out var unsafeDiagnostic), Is.False);
            StringAssert.Contains("vn_checkpoint", unsafeDiagnostic);
            Assert.That(((VNReplayTestPresenter)unsafeSession.Presenter).LineIds, Is.Empty);
            Assert.That(unsafeSession.IsDialogueRunning, Is.False);
            unsafeSession.DisposeAsync().Forget();
            var unsafeHierarchy = unsafeSession.Runner.gameObject;
            yield return WaitFor(() => unsafeHierarchy == null, "Unsafe-content Replay resources were not disposable.");

            DialogueRunner rejectedRunner = null;
            VNCheckpointService rejectedCheckpointOwner = null;
            var forbiddenCapabilityException = Assert.Throws<InvalidOperationException>(() => VNReplaySession.Create(
                project,
                safeReplayPolicy,
                parent =>
                {
                    rejectedRunner = parent.GetComponent<DialogueRunner>();
                    rejectedCheckpointOwner = parent.gameObject.AddComponent<VNCheckpointService>();
                    return CreatePresentationController(parent, catalog, "Rejected Replay Presentation");
                },
                parent =>
                {
                    var presenterObject = new GameObject("Rejected Replay Presenter");
                    presenterObject.transform.SetParent(parent, false);
                    return presenterObject.AddComponent<VNReplayTestPresenter>();
                }));
            StringAssert.Contains(nameof(VNCheckpointService), forbiddenCapabilityException.Message);
            yield return null;
            Assert.That(rejectedRunner == null, Is.True, "Rejected Replay factories must destroy their runner hierarchy.");
            Assert.That(rejectedCheckpointOwner == null, Is.True, "Rejected Replay factories must destroy forbidden production owners.");

            // A caller that bypasses TryStart is still stopped before forbidden or unknown dispatch.
            var rawUnknownSession = CreateReplaySession(project, CreatePolicy("M9_REPLAY_UNSAFE_UNKNOWN"), catalog);
            rawUnknownSession.Runner.StartDialogue("M9_REPLAY_UNSAFE_UNKNOWN").Forget();
            var rawUnknownPresenter = (VNReplayTestPresenter)rawUnknownSession.Presenter;
            yield return WaitFor(() => rawUnknownPresenter.LineIds.Count == 1, "Unknown-command fixture did not reach its pre-command line.");
            Assert.That(rawUnknownSession.PresentationController.CurrentBackgroundId, Is.EqualTo("replay_background"));
            rawUnknownSession.Runner.RequestNextLine();
            yield return WaitFor(() => rawUnknownSession.Lifecycle == VNReplaySessionLifecycle.Failed && !rawUnknownSession.IsDialogueRunning,
                "Runtime guard did not stop a bypassed unknown command.");
            StringAssert.Contains("m9_replay_unknown_command", rawUnknownSession.Diagnostic);
            Assert.That(rawUnknownSession.PresentationController.CurrentBackgroundId, Is.Null);
            rawUnknownSession.DisposeAsync().Forget();
            var rawUnknownHierarchy = rawUnknownSession.Runner.gameObject;
            yield return WaitFor(() => rawUnknownHierarchy == null, "Unknown-command Replay resources were not disposable.");

            var rawCheckpointSession = CreateReplaySession(project, CreatePolicy("M9_REPLAY_UNSAFE_CHECKPOINT"), catalog);
            rawCheckpointSession.Runner.StartDialogue("M9_REPLAY_UNSAFE_CHECKPOINT").Forget();
            yield return WaitFor(() => rawCheckpointSession.Lifecycle == VNReplaySessionLifecycle.Failed && !rawCheckpointSession.IsDialogueRunning,
                "Runtime guard did not stop a bypassed checkpoint command.");
            StringAssert.Contains("vn_checkpoint", rawCheckpointSession.Diagnostic);
            Assert.That(productionCheckpointHandlerCalls, Is.Zero);
            Assert.That(checkpointMutations, Is.Zero);
            rawCheckpointSession.DisposeAsync().Forget();
            var rawCheckpointHierarchy = rawCheckpointSession.Runner.gameObject;
            yield return WaitFor(() => rawCheckpointHierarchy == null, "Checkpoint-command Replay resources were not disposable.");

            var rawMetaSession = CreateReplaySession(project, CreatePolicy("M9_REPLAY_UNSAFE_UNLOCK_CG"), catalog);
            rawMetaSession.Runner.StartDialogue("M9_REPLAY_UNSAFE_UNLOCK_CG").Forget();
            yield return WaitFor(() => rawMetaSession.Lifecycle == VNReplaySessionLifecycle.Failed && !rawMetaSession.IsDialogueRunning,
                "Runtime guard did not stop a bypassed MetaProgress command.");
            StringAssert.Contains("vn_unlock_cg", rawMetaSession.Diagnostic);
            Assert.That(productionMetaHandlerCalls, Is.Zero);
            rawMetaSession.DisposeAsync().Forget();
            var rawMetaHierarchy = rawMetaSession.Runner.gameObject;
            yield return WaitFor(() => rawMetaHierarchy == null, "MetaProgress-command Replay resources were not disposable.");

            var rawAudioSession = CreateReplaySession(project, CreatePolicy("M9_REPLAY_UNSAFE_BGM"), catalog);
            rawAudioSession.Runner.StartDialogue("M9_REPLAY_UNSAFE_BGM").Forget();
            yield return WaitFor(() => rawAudioSession.Lifecycle == VNReplaySessionLifecycle.Failed && !rawAudioSession.IsDialogueRunning,
                "Runtime guard did not stop a bypassed audio command.");
            StringAssert.Contains("bgm_play", rawAudioSession.Diagnostic);
            Assert.That(productionAudioHandlerCalls, Is.Zero);
            rawAudioSession.DisposeAsync().Forget();
            var rawAudioHierarchy = rawAudioSession.Runner.gameObject;
            yield return WaitFor(() => rawAudioHierarchy == null, "Audio-command Replay resources were not disposable.");

            AssertReplayIsolation(productionRunner, productionStorage, productionSession, productionPresentation, productionAudio,
                audioSourceA, audioSourceB, sfxSource, productionSnapshot, productionProbeBefore,
                productionMetaHandlerCalls, productionCheckpointHandlerCalls, productionAudioHandlerCalls,
                checkpointMutations, metaProgress, metaWriteCounter.Value, metaJsonBefore, metaBytesBefore, saveFilePaths, saveBytesBefore);
            Assert.That(productionRunner.IsDialogueRunning, Is.True);
            Assert.That(productionSession.CurrentLineId, Is.EqualTo("line:m9_replay_prod_01"));

            productionRunner.RequestNextLine();
            yield return WaitFor(() => productionSession.IsLineActive && productionSession.CurrentLineId == "line:m9_replay_prod_02" &&
                                          productionSession.IsCurrentLineFullyDisplayed,
                "Production runner could not continue after Replay disposed.");
            Assert.That(productionStorage.TryGetValue<float>("$replay_probe", out var productionProbeAfterReplay), Is.True);
            Assert.That(productionProbeAfterReplay, Is.EqualTo(10f));
            Assert.That(productionPresentation.CurrentBackgroundId, Is.EqualTo("production_background"));
            Assert.That(productionPresentation.CurrentCGId, Is.Null);
            productionRunner.RequestNextLine();
            yield return WaitFor(() => !productionRunner.IsDialogueRunning && productionRunner.DialogueTask.IsCompletedSuccessfully(),
                "Production runner did not complete after its second line.");

            var finalMetaBytes = File.ReadAllBytes(metaRepository.CanonicalFilePath);
            var finalSaveBytes = saveFilePaths.Select(File.ReadAllBytes).ToArray();
            Assert.That(metaWriteCounter.Value, Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(metaProgress.Current, true), Is.EqualTo(metaJsonBefore));
            Assert.That(Sha256(finalMetaBytes), Is.EqualTo(metaHashBefore));
            for (var i = 0; i < finalSaveBytes.Length; i++)
                CollectionAssert.AreEqual(saveBytesBefore[i], finalSaveBytes[i], saveFilePaths[i]);

            TestContext.WriteLine("RUNTIME PROOF: Unity Play Mode, Unity 6000.3.21f1, Yarn Spinner Unity 3.2.7.");
            TestContext.WriteLine("Production runner: active at line:m9_replay_prod_01 during all Replay sessions; advanced to line:m9_replay_prod_02; completed normally.");
            TestContext.WriteLine("Replay A: lines [line:m9_replay_safe_01, line:m9_replay_safe_02, line:m9_replay_safe_03, line:m9_replay_safe_04, line:m9_replay_safe_05, line:m9_replay_safe_06]; variable=99; replay BG/CG isolated; character expression set, moved left, faced left, scaled 1.25, then hidden.");
            TestContext.WriteLine("Variables: production=10 before/during/after; Replay A=99; fresh Replay B=$replay_probe 0.");
            TestContext.WriteLine($"Session: active={productionSnapshot.IsLineActive}, line={productionSnapshot.CurrentLineId}, options={productionSnapshot.OptionsActive}; backlog={productionSnapshot.Backlog.Length}; reads=[{string.Join(",", productionSnapshot.ReadHistory)}]. unchanged during Replay.");
            TestContext.WriteLine($"MetaProgress: writes={metaWriteCounter.Value} (one test seed before baseline); SHA256 before/after={metaHashBefore}/{Sha256(finalMetaBytes)}.");
            TestContext.WriteLine($"SaveData: temp auto/quick/manual file SHA256 before/after={string.Join(",", saveHashesBefore)}/{string.Join(",", finalSaveBytes.Select(Sha256))}; checkpoint events={checkpointMutations}.");
            TestContext.WriteLine($"Production presentation/audio: BG={productionPresentation.CurrentBackgroundId}; CG={productionPresentation.CurrentCGId ?? "none"}; BGM='{productionAudio.CurrentBgmId}'; sourcesPlaying={audioSourceA.isPlaying || audioSourceB.isPlaying || sfxSource.isPlaying}.");
            TestContext.WriteLine("Lifecycle: completion PASS, cancel PASS, invalid root PASS, unsafe validation PASS, forbidden-owner factory rejected and cleaned up PASS, raw unknown/checkpoint/MetaProgress guard PASS, repeated Replay PASS, dispose after completion PASS.");

            UnityEngine.Object.Destroy(productionRoot);
            foreach (var asset in fixtureAssets)
                if (asset != null) UnityEngine.Object.Destroy(asset);
            yield return null;
            yield return new ExitPlayMode();
            RestorePlayModeOptions();

            DeleteTestDirectory(temporaryMetaRoot);
            DeleteTestDirectory(temporarySaveRoot);
            temporaryMetaRoot = null;
            temporarySaveRoot = null;
        }

        private static VNReplaySession CreateReplaySession(YarnProject project, VNReplayContentPolicy policy, VNPresentationCatalog catalog)
        {
            return VNReplaySession.Create(
                project,
                policy,
                parent => CreatePresentationController(parent, catalog, "Replay Presentation"),
                parent =>
                {
                    var presenterObject = new GameObject("Replay Test Presenter");
                    presenterObject.transform.SetParent(parent, false);
                    return presenterObject.AddComponent<VNReplayTestPresenter>();
                });
        }

        private void EnablePlayModeWithoutDomainReload()
        {
            if (!playModeOptionsCaptured)
            {
                previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
                previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
                playModeOptionsCaptured = true;
            }

            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions | EnterPlayModeOptions.DisableDomainReload;
        }

        private void RestorePlayModeOptions()
        {
            if (!playModeOptionsCaptured) return;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = previousEnterPlayModeOptionsEnabled;
            playModeOptionsCaptured = false;
        }

        private static VNPresentationController CreatePresentationController(Transform parent, VNPresentationCatalog catalog, string name)
        {
            var presentationObject = new GameObject(name);
            presentationObject.transform.SetParent(parent, false);
            presentationObject.SetActive(false);

            var backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backgroundObject.transform.SetParent(presentationObject.transform, false);
            var cgObject = new GameObject("CG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cgObject.transform.SetParent(presentationObject.transform, false);
            var characterSlotViews = new List<VNCharacterSlotView>();
            foreach (VNCharacterSlot slot in Enum.GetValues(typeof(VNCharacterSlot)))
                characterSlotViews.Add(CreateSlotView(presentationObject.transform, slot));

            var controller = presentationObject.AddComponent<VNPresentationController>();
            SetPrivateField(controller, "catalog", catalog);
            SetPrivateField(controller, "backgroundImage", backgroundObject.GetComponent<Image>());
            SetPrivateField(controller, "cgImage", cgObject.GetComponent<Image>());
            SetPrivateField(controller, "characterSlotViews", characterSlotViews);
            presentationObject.SetActive(true);
            return controller;
        }

        private static VNPresentationCatalog CreatePresentationCatalog(List<UnityEngine.Object> ownedAssets)
        {
            var catalog = ScriptableObject.CreateInstance<VNPresentationCatalog>();
            catalog.name = "M9 Replay Runtime Test Catalog";
            ownedAssets.Add(catalog);

            var productionBackground = CreateSprite("Production Background", new Color(0.2f, 0.3f, 0.4f, 1f), ownedAssets);
            var replayBackground = CreateSprite("Replay Background", new Color(0.4f, 0.3f, 0.2f, 1f), ownedAssets);
            var cgSprite = CreateSprite("Replay CG", new Color(0.7f, 0.5f, 0.3f, 1f), ownedAssets);
            var characterSprite = CreateSprite("Replay Character", new Color(0.3f, 0.6f, 0.7f, 1f), ownedAssets);
            var character = CreateCharacterDefinition(characterSprite, ownedAssets);
            SetPrivateField(catalog, "characterDefinitions", new List<VNCharacterDefinition> { character });
            SetPrivateField(catalog, "backgrounds", new List<VNSpriteCatalogEntry>
            {
                CreateSpriteEntry("production_background", productionBackground),
                CreateSpriteEntry("replay_background", replayBackground),
            });
            SetPrivateField(catalog, "cgs", new List<VNSpriteCatalogEntry>
            {
                CreateSpriteEntry("m3_cg", cgSprite),
            });
            return catalog;
        }

        private static VNCharacterDefinition CreateCharacterDefinition(Sprite sprite, List<UnityEngine.Object> ownedAssets)
        {
            var definition = ScriptableObject.CreateInstance<VNCharacterDefinition>();
            definition.name = "M9 Replay Test Character";
            ownedAssets.Add(definition);

            var expression = new VNExpressionDefinition();
            SetPrivateField(expression, "expressionId", "default");
            SetPrivateField(expression, "headSprite", sprite);
            var replayExpression = new VNExpressionDefinition();
            SetPrivateField(replayExpression, "expressionId", "replay_expression");
            SetPrivateField(replayExpression, "headSprite", sprite);
            SetPrivateField(definition, "characterId", "m9_replay_character");
            SetPrivateField(definition, "speakerAliases", new List<string>());
            SetPrivateField(definition, "defaultFacing", VNCharacterFacing.Right);
            SetPrivateField(definition, "defaultScale", 1f);
            SetPrivateField(definition, "bodySprite", sprite);
            SetPrivateField(definition, "defaultExpressionId", "default");
            SetPrivateField(definition, "expressions", new List<VNExpressionDefinition> { expression, replayExpression });
            return definition;
        }

        private static Sprite CreateSprite(string name, Color color, List<UnityEngine.Object> ownedAssets)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = name + " Texture" };
            texture.SetPixels(new[] { color, color, color, color });
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            sprite.name = name;
            ownedAssets.Add(texture);
            ownedAssets.Add(sprite);
            return sprite;
        }

        private static VNSpriteCatalogEntry CreateSpriteEntry(string id, Sprite sprite)
        {
            var entry = new VNSpriteCatalogEntry();
            SetPrivateField(entry, "id", id);
            SetPrivateField(entry, "sprite", sprite);
            return entry;
        }

        private static VNCharacterSlotView CreateSlotView(Transform parent, VNCharacterSlot slot)
        {
            var slotObject = new GameObject("Replay Slot " + slot, typeof(RectTransform), typeof(CanvasGroup));
            slotObject.transform.SetParent(parent, false);
            var view = slotObject.AddComponent<VNCharacterSlotView>();
            SetPrivateField(view, "slot", slot);
            SetPrivateField(view, "visualRoot", slotObject.GetComponent<RectTransform>());
            SetPrivateField(view, "backHairImage", CreateSlotLayer(slotObject.transform, "Back Hair"));
            SetPrivateField(view, "bodyImage", CreateSlotLayer(slotObject.transform, "Body"));
            SetPrivateField(view, "headImage", CreateSlotLayer(slotObject.transform, "Head"));
            SetPrivateField(view, "fadeCanvasGroup", slotObject.GetComponent<CanvasGroup>());
            return view;
        }

        private static Image CreateSlotLayer(Transform parent, string name)
        {
            var layer = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            layer.transform.SetParent(parent, false);
            return layer.GetComponent<Image>();
        }

        private static void AssertReplayOwnership(DialogueRunner productionRunner, InMemoryVariableStorage productionStorage, VNReplaySession replay)
        {
            Assert.That(replay.Runner, Is.Not.SameAs(productionRunner));
            Assert.That(replay.VariableStorage, Is.Not.SameAs(productionStorage));
            Assert.That(replay.Runner.VariableStorage, Is.SameAs(replay.VariableStorage));
            Assert.That(replay.Runner.YarnProject, Is.SameAs(productionRunner.YarnProject));
            Assert.That(replay.Runner.autoStart, Is.False);
            Assert.That(replay.Presenter, Is.Not.SameAs(productionRunner.DialoguePresenters.First()));
            Assert.That(replay.Runner.LineProvider, Is.Not.SameAs(productionRunner.LineProvider));

            var forbiddenComponents = new HashSet<Type>
            {
                typeof(VNDialogueSessionState),
                typeof(VNPersistentReadHistoryBridge),
                typeof(VNYarnCheckpointCommands),
                typeof(VNYarnMetaProgressCommands),
                typeof(VNYarnAudioCommands),
                typeof(VNTransitionController),
                typeof(VNAudioController),
                typeof(VNSaveLoadController),
                typeof(VNCheckpointService),
            };
            foreach (var component in replay.Runner.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                Assert.That(forbiddenComponents.Contains(component.GetType()), Is.False, component.GetType().FullName);
            }
            Assert.That(replay.RuntimeHandlersRegistered, Is.True);
        }

        private void AssertReplayIsolation(
            DialogueRunner productionRunner,
            InMemoryVariableStorage productionStorage,
            VNDialogueSessionState productionSession,
            VNPresentationController productionPresentation,
            VNAudioController productionAudio,
            AudioSource audioSourceA,
            AudioSource audioSourceB,
            AudioSource sfxSource,
            ProductionSnapshot expected,
            float expectedProductionProbe,
            int productionMetaHandlerCalls,
            int productionCheckpointHandlerCalls,
            int productionAudioHandlerCalls,
            int checkpointMutations,
            VNMetaProgressService metaProgress,
            int metaWrites,
            string expectedMetaJson,
            byte[] expectedMetaBytes,
            IReadOnlyList<string> savePaths,
            IReadOnlyList<byte[]> expectedSaveBytes)
        {
            Assert.That(productionRunner.IsDialogueRunning, Is.True);
            Assert.That(productionStorage.TryGetValue<float>("$replay_probe", out var currentProbe), Is.True);
            Assert.That(currentProbe, Is.EqualTo(expectedProductionProbe));
            AssertProductionSnapshot(productionSession, expected);
            Assert.That(productionPresentation.CurrentBackgroundId, Is.EqualTo("production_background"));
            Assert.That(productionPresentation.CurrentCGId, Is.Null);
            Assert.That(productionPresentation.VisibleCharacters, Is.Empty);
            Assert.That(productionAudio.CurrentBgmId, Is.Empty);
            Assert.That(productionAudio.IsBgmTransitionActive, Is.False);
            Assert.That(audioSourceA.isPlaying || audioSourceB.isPlaying || sfxSource.isPlaying, Is.False);
            Assert.That(productionAudio.CurrentBgmId, Is.EqualTo(productionAudioSnapshot.CurrentBgmId));
            Assert.That(productionAudio.IsBgmTransitionActive, Is.EqualTo(productionAudioSnapshot.TransitionActive));
            CollectionAssert.AreEqual(productionAudioSnapshot.PlayingSources, new[] { audioSourceA.isPlaying, audioSourceB.isPlaying, sfxSource.isPlaying });
            CollectionAssert.AreEqual(productionAudioSnapshot.Volumes, new[] { audioSourceA.volume, audioSourceB.volume, sfxSource.volume });
            Assert.That(productionMetaHandlerCalls, Is.Zero);
            Assert.That(productionCheckpointHandlerCalls, Is.Zero);
            Assert.That(productionAudioHandlerCalls, Is.Zero);
            Assert.That(checkpointMutations, Is.Zero);

            Assert.That(metaWrites, Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(metaProgress.Current, true), Is.EqualTo(expectedMetaJson));
            CollectionAssert.AreEqual(expectedMetaBytes, File.ReadAllBytes(metaCanonicalPath));
            for (var i = 0; i < savePaths.Count; i++)
                CollectionAssert.AreEqual(expectedSaveBytes[i], File.ReadAllBytes(savePaths[i]), savePaths[i]);
        }

        private static ProductionSnapshot CaptureProductionSnapshot(VNDialogueSessionState session)
        {
            return new ProductionSnapshot
            {
                IsLineActive = session.IsLineActive,
                CurrentLineId = session.CurrentLineId,
                OptionsActive = session.OptionsActive,
                Backlog = SnapshotBacklog(session),
                ReadHistory = session.ReadHistory.Snapshot().ToArray(),
            };
        }

        private static void AssertProductionSnapshot(VNDialogueSessionState session, ProductionSnapshot expected)
        {
            Assert.That(session.IsLineActive, Is.EqualTo(expected.IsLineActive));
            Assert.That(session.CurrentLineId, Is.EqualTo(expected.CurrentLineId));
            Assert.That(session.OptionsActive, Is.EqualTo(expected.OptionsActive));
            CollectionAssert.AreEqual(expected.Backlog, SnapshotBacklog(session));
            CollectionAssert.AreEqual(expected.ReadHistory, session.ReadHistory.Snapshot().ToArray());
        }

        private static string[] SnapshotBacklog(VNDialogueSessionState session) => session.Backlog.Entries
            .Select(entry => $"{entry.LineId}\u001f{entry.SpeakerName}\u001f{entry.Text}")
            .ToArray();

        private static AudioSnapshot CaptureAudioSnapshot(VNAudioController controller, AudioSource bgmA, AudioSource bgmB, AudioSource sfx)
        {
            return new AudioSnapshot
            {
                CurrentBgmId = controller.CurrentBgmId,
                TransitionActive = controller.IsBgmTransitionActive,
                PlayingSources = new[] { bgmA.isPlaying, bgmB.isPlaying, sfx.isPlaying },
                Volumes = new[] { bgmA.volume, bgmB.volume, sfx.volume },
            };
        }

        private static VNReplayContentPolicy CreatePolicy(params string[] approvedNodes) => new(approvedNodes);

        private static VNReplayContentValidationResult ValidateSingleRoot(YarnProject project, string nodeName) =>
            VNReplayContentValidator.Validate(project, nodeName, CreatePolicy(nodeName));

        private static YarnProject LoadYarnProject() => AssetDatabase.LoadAssetAtPath<YarnProject>(YarnProjectPath);

        private static SaveSlotData CreateSaveFixture(VNSaveSlotKey key)
        {
            return new SaveSlotData
            {
                schemaVersion = VNSaveSerializer.CurrentSchemaVersion,
                slotType = key.ToSerializedSlotType(),
                slotIndex = key.SlotIndex,
                checkpointId = "m9_replay_test_checkpoint",
                resumeNode = "M9_REPLAY_PRODUCTION_WAIT",
                yarnVariables = new YarnVariablesData
                {
                    floats = new[] { new FloatVariableEntry { name = "$replay_probe", value = 10f } },
                    strings = Array.Empty<StringVariableEntry>(),
                    bools = Array.Empty<BoolVariableEntry>(),
                },
                presentationState = new PresentationState
                {
                    backgroundId = "production_background",
                    cgId = string.Empty,
                    characters = Array.Empty<CharacterSaveState>(),
                },
                audioState = new AudioState { bgmId = string.Empty, playbackSeconds = 0f },
                chapterId = "m9_replay_test_chapter",
                sceneTitle = "M9 Replay Test",
                playedSeconds = 12f,
                savedAtUtcIso8601 = DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                thumbnailFileName = string.Empty,
            };
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().Name}.{fieldName}");
            field.SetValue(target, value);
        }

        private static IEnumerator WaitFor(Func<bool> condition, string failureMessage, int timeoutFrames = 900)
        {
            for (var frame = 0; frame < timeoutFrames; frame++)
            {
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail(failureMessage);
        }

        private static string Sha256(byte[] bytes)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static void DeleteTestDirectory(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private sealed class ProductionSnapshot
        {
            public bool IsLineActive;
            public string CurrentLineId;
            public bool OptionsActive;
            public string[] Backlog;
            public string[] ReadHistory;
        }

        private sealed class WriteCounter
        {
            public int Value;
        }

        private sealed class AudioSnapshot
        {
            public string CurrentBgmId;
            public bool TransitionActive;
            public bool[] PlayingSources;
            public float[] Volumes;
        }
    }

    public sealed class VNReplayTestPresenter : DialoguePresenterBase
    {
        public readonly List<string> LineIds = new();
        public readonly List<string> Texts = new();
        public bool DialogueCompleted { get; private set; }
        public bool OptionsReceived { get; private set; }

        public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            LineIds.Add(line.TextID);
            Texts.Add(line.Text.Text);
            await YarnTask.WaitUntilCanceled(token.NextContentToken).SuppressCancellationThrow();
        }

        public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken cancellationToken)
        {
            OptionsReceived = true;
            return DialogueRunner.NoOptionSelected;
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            DialogueCompleted = false;
            return YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            DialogueCompleted = true;
            return YarnTask.CompletedTask;
        }
    }
}
