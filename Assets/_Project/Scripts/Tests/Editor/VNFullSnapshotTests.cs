using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
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
            audio.SfxSource.clip = audio.ClipA;
            Assert.That(audio.Controller.TryPrepareRestore(new AudioState { bgmId = "bgm_a", playbackSeconds = 0.5f }, out var plan, out var diagnostic), Is.True, diagnostic);
            audio.Controller.NormalizeTransientForLoad();
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

        private PresentationHarness CreatePresentationHarness()
        {
            var spriteA = CreateSprite("A");
            var spriteB = CreateSprite("B");
            var catalog = ScriptableObject.CreateInstance<VNPresentationCatalog>();
            ownedObjects.Add(catalog);
            var characterA = CreateCharacterDefinition("char_a", spriteA);
            var characterB = CreateCharacterDefinition("char_b", spriteB);
            SetPrivate(catalog, "characterDefinitions", new List<VNCharacterDefinition> { characterA, characterB });
            SetPrivate(catalog, "backgrounds", new List<VNSpriteCatalogEntry> { CreateSpriteEntry("bg_a", spriteA), CreateSpriteEntry("bg_b", spriteB) });
            SetPrivate(catalog, "cgs", new List<VNSpriteCatalogEntry> { CreateSpriteEntry("cg_a", spriteA), CreateSpriteEntry("cg_b", spriteB) });

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

        private AudioHarness CreateAudioHarness()
        {
            var clipA = AudioClip.Create("Bgm A", 1000, 1, 1000, false);
            var clipB = AudioClip.Create("Bgm B", 1000, 1, 1000, false);
            ownedObjects.Add(clipA);
            ownedObjects.Add(clipB);
            var catalog = ScriptableObject.CreateInstance<VNAudioCatalog>();
            ownedObjects.Add(catalog);
            SetPrivate(catalog, "bgm", new List<VNBgmCatalogEntry> { CreateBgmEntry("bgm_a", clipA, 0.7f, true), CreateBgmEntry("bgm_b", clipB, 0.4f, false) });
            SetPrivate(catalog, "sfx", new List<VNSfxCatalogEntry>());

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
