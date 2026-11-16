using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Replay.UI;
using ProjectAllTime.VN.Records.UI;
using ProjectAllTime.VN.Records.Timeline;
using ProjectAllTime.VN.SaveLoad;
using ProjectAllTime.VN.Settings;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    /// <summary>
    /// Cross-system M10-11 invariants. Existing M6, M7, M9, M10-08, M10-09,
    /// and M10-10 suites remain the detailed authority for their own domains.
    /// These checks join their references and guard the complete VN_Main Scene.
    /// </summary>
    public sealed class M10_11UnityFinalWiringAuditTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/VN_Main.unity";
        private const string ProductionYarnPath = "Assets/_Project/Yarn/GameNarrative.yarnproject";
        private const string ReplayYarnPath = "Assets/_Project/RecordsReplay/Yarn/PrologueReplay.yarnproject";
        private const string PresentationCatalogPath = "Assets/_Project/Settings/Presentation/Production/M10_ProductionPresentationCatalog.asset";
        private const string AudioCatalogPath = "Assets/_Project/Audio/M10_ProductionAudioCatalog.asset";
        private const string MixerPath = "Assets/_Project/Audio/VNAudioMixer.mixer";
        private const string InputPath = "Assets/_Project/Settings/Input/VNInputActions.inputactions";
        private const string RecordsPath = "Assets/_Project/Settings/Records/Production/";

        private static readonly Dictionary<string, string> StableAssetGuids = new(StringComparer.Ordinal)
        {
            [ScenePath] = "15ebc4e3dabe3f441ba137435b41821f",
            [ProductionYarnPath] = "c6e164ddc9efe2d4f8761bf2c5e13d48",
            [ReplayYarnPath] = "e26ba57c09322a64ab5c9da62966f033",
            [PresentationCatalogPath] = "8cc24f7387355284cbe3e06af3067684",
            [AudioCatalogPath] = "ddd9e821dd823324eaa16cdb5ab1c55c",
            [MixerPath] = "9cc27892a90083744943c3730dda1065",
            [InputPath] = "6fac5fd1e55030243aff7c38a614bab8",
            [RecordsPath + "VNTimelineCatalog.asset"] = "655982406f684734fa8385e6d9abe9eb",
            [RecordsPath + "VNCGGalleryCatalog.asset"] = "27f8d1688f46a894abdb38ef671436ad",
            [RecordsPath + "VNArchiveCatalog.asset"] = "fa10a73a1e7dd434b822ed8344a362a2",
            [RecordsPath + "VNAchievementCatalog.asset"] = "2e6ac52fc2d74d94d84b09a54c7c2c6a",
            [RecordsPath + "VNProductionRecordsUnlockMapping.asset"] = "185ab3d7cfe756841a02db9ecd60add4",
        };

        private Scene scene;
        private bool opened;

        [SetUp]
        public void SetUp()
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            if (opened && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }

        [Test]
        public void ProductionScene_OpensReadOnlyAndHasNoMissingScriptsAcrossInactiveHierarchy()
        {
            Assert.That(scene.IsValid(), Is.True);
            Assert.That(scene.path, Is.EqualTo(ScenePath));
            var dirtyBefore = scene.isDirty;
            var visited = 0;
            var inactive = 0;

            foreach (var root in scene.GetRootGameObjects())
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                visited++;
                if (!transform.gameObject.activeSelf) inactive++;
                Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject),
                    Is.Zero, Hierarchy(transform) + " has a missing MonoBehaviour script.");
                foreach (var component in transform.GetComponents<Component>())
                    Assert.That(component, Is.Not.Null, Hierarchy(transform) + " has a broken component reference.");
            }

            Assert.That(visited, Is.GreaterThan(0));
            Assert.That(inactive, Is.GreaterThan(0), "The audit must include inactive modals and templates.");
            Assert.That(Single<VNReplayView>().gameObject.activeSelf, Is.False);
            TestContext.WriteLine($"VN_Main read-only hierarchy: {visited} GameObjects, {inactive} inactive; no missing scripts.");
            Assert.That(scene.isDirty, Is.EqualTo(dirtyBefore), "Read-only audit marked VN_Main dirty.");
        }

        [Test]
        public void RequiredAssets_ResolveThroughTheirMergedStableGuids()
        {
            foreach (var item in StableAssetGuids)
            {
                var actualGuid = AssetDatabase.AssetPathToGUID(item.Key);
                Assert.That(actualGuid, Is.EqualTo(item.Value), item.Key + " GUID changed.");
                Assert.That(AssetDatabase.GUIDToAssetPath(actualGuid), Is.EqualTo(item.Key), item.Key);
                Assert.That(AssetDatabase.LoadMainAssetAtPath(item.Key), Is.Not.Null, item.Key);
            }

            foreach (var component in RequiredProjectComponents())
            {
                var script = MonoScript.FromMonoBehaviour(component);
                Assert.That(script, Is.Not.Null, Hierarchy(component.transform));
                var scriptPath = AssetDatabase.GetAssetPath(script);
                var guid = AssetDatabase.AssetPathToGUID(scriptPath);
                Assert.That(scriptPath, Does.StartWith("Assets/_Project/Scripts/"));
                Assert.That(guid, Is.Not.Empty, scriptPath);
                Assert.That(AssetDatabase.GUIDToAssetPath(guid), Is.EqualTo(scriptPath), scriptPath);
            }
        }

        [Test]
        public void DialogueAuthority_UsesOneProductionRunnerAndTheApprovedStartContract()
        {
            var runner = Single<DialogueRunner>();
            Assert.That(runner.isActiveAndEnabled, Is.True);
            var yarnProject = Ref<YarnProject>(runner, "yarnProject");
            Assert.That(AssetDatabase.GetAssetPath(yarnProject), Is.EqualTo(ProductionYarnPath));
            var serialized = new SerializedObject(runner);
            Assert.That(serialized.FindProperty("autoStart").boolValue, Is.False, "M12 starts from the title screen.");
            Assert.That(serialized.FindProperty("startNode").stringValue, Is.EqualTo("M2_UI_START"));
            Assert.That(yarnProject.Program.Nodes.ContainsKey("M2_UI_START"), Is.True);
            Assert.That(yarnProject.Program.Nodes.ContainsKey("PROLOGUE_S001_START"), Is.True);

            var linePresenters = runner.DialoguePresenters.OfType<LinePresenter>()
                .Where(p => p != null && p.isActiveAndEnabled).Distinct().ToArray();
            Assert.That(linePresenters, Has.Length.EqualTo(1),
                "DialogueRunner must register one enabled production LinePresenter.");
            var mainLinePresenter = linePresenters[0];
            var lifecycle = Single<VNLineLifecyclePresenter>();
            var lineAdvancer = Single<LineAdvancer>();
            AssertRef(lifecycle, "sessionState", Single<VNDialogueSessionState>());
            AssertRef(lifecycle, "linePresenter", mainLinePresenter);
            AssertRef(lineAdvancer, "presenter", mainLinePresenter);
            Assert.That(Ref<TMP_Text>(mainLinePresenter, "lineText"), Is.Not.Null);
            Assert.That(Ref<TMP_Text>(mainLinePresenter, "characterNameText"), Is.Not.Null);
            Assert.That(Single<VNLineAdvancerInputBridge>().isActiveAndEnabled, Is.True);
            Assert.That(All<DialogueRunner>(), Has.Length.EqualTo(1),
                "The Replay template is local presentation and must not add a production Runner.");

            var lineProviderProperty = serialized.FindProperty("lineProvider");
            Assert.That(lineProviderProperty, Is.Not.Null);
            TestContext.WriteLine(lineProviderProperty.objectReferenceValue == null
                ? "Serialized lineProvider is null; live Play verified Yarn's BuiltinLocalisedLineProvider fallback."
                : "Serialized lineProvider: " + lineProviderProperty.objectReferenceValue.GetType().FullName);
            TestContext.WriteLine("DialogueRunner: /Dialogue System; authoritative LinePresenter: " + Hierarchy(mainLinePresenter.transform));
        }

        [Test]
        public void CompiledPrologueCommands_MapToOneProductionBridgeOnTheAuthoritativeRunner()
        {
            var runner = Single<DialogueRunner>();
            var project = Ref<YarnProject>(runner, "yarnProject");
            var expectedOwners = ProductionCommandOwners();
            var actual = new HashSet<string>(StringComparer.Ordinal);

            foreach (var pair in project.Program.Nodes)
            {
                if (!IsCanonicalPrologueNode(pair.Key)) continue;
                foreach (var instruction in pair.Value.Instructions)
                {
                    if (instruction.RunCommand == null) continue;
                    var parts = instruction.RunCommand.CommandText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    Assert.That(parts, Is.Not.Empty, pair.Key);
                    actual.Add(parts[0]);
                }
            }

            CollectionAssert.AreEquivalent(expectedOwners.Keys, actual,
                "Compiled S001-S008 commands changed; re-audit bridge ownership before changing this matrix.");
            foreach (var item in expectedOwners)
            {
                var bridge = Single(item.Value);
                Assert.That(bridge, Is.InstanceOf<Behaviour>(), item.Key);
                Assert.That(((Behaviour)bridge).isActiveAndEnabled, Is.True, item.Key);
                AssertRef(bridge, "dialogueRunner", runner);
                TestContext.WriteLine($"{item.Key} | {item.Value.Name} | {Hierarchy(bridge.transform)} | PASS");
            }

            // The deliberately unknown M9 spike and other technical smoke nodes are
            // compiled into GameNarrative for historical tests, but are not story owners.
            Assert.That(actual.Contains("m9_replay_unknown_command"), Is.False);
            Assert.That(actual.Contains("exact_fixture_sfx"), Is.False);
            Assert.That(actual.Contains("m104_hold"), Is.False);
        }

        [Test]
        public void PresentationConsumersShareOneCatalogAndBothSixSlotCompositionsAreValid()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(PresentationCatalogPath);
            Assert.That(catalog, Is.Not.Null);
            var presentationConsumers = All<VNPresentationController>();
            Assert.That(presentationConsumers, Has.Length.EqualTo(2),
                "Expected the production presentation owner and the intentionally isolated Replay-local owner.");
            foreach (var consumer in presentationConsumers)
                AssertRef(consumer, "catalog", catalog);

            var saveLoad = Single<VNSaveLoadController>();
            AssertRef(saveLoad, "presentationCatalog", catalog);
            var records = Single<VNRecordsRuntimeBootstrap>();
            AssertRef(records, "presentationCatalog", catalog);

            var production = presentationConsumers.Single(p => p.name == "PresentationRuntime");
            var mainSlots = RefList<VNCharacterSlotView>(production, "characterSlotViews");
            AssertSlotSet(mainSlots, "production");
            var replayView = Single<VNReplayView>();
            Assert.That(replayView.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            AssertSlotSet(replayView.GetComponentsInChildren<VNCharacterSlotView>(true).ToList(), "Replay");
            Assert.That(Ref<VNPresentationController>(replayView, "presentationController"),
                Is.SameAs(presentationConsumers.Single(p => p != production)));

            var project = Ref<YarnProject>(Single<DialogueRunner>(), "yarnProject");
            var names = project.NodeNames;
            Assert.That(names, Does.Contain("PROLOGUE_S001_START"));
            Assert.That(names, Does.Contain("PROLOGUE_S008_PROFILE_WAIT"));
        }

        [Test]
        public void AudioCatalog_HasOneControllerAndPurposeRoutedDistinctSources()
        {
            var controller = Single<VNAudioController>();
            var catalog = Ref<VNAudioCatalog>(controller, "catalog");
            Assert.That(AssetDatabase.GetAssetPath(catalog), Is.EqualTo(AudioCatalogPath));
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            Assert.That(mixer, Is.Not.Null);
            var groups = new Dictionary<string, AudioMixerGroup>(StringComparer.Ordinal)
            {
                ["bgmSourceA"] = mixer.FindMatchingGroups("BGM").Single(g => g.name == "BGM"),
                ["bgmSourceB"] = mixer.FindMatchingGroups("BGM").Single(g => g.name == "BGM"),
                ["sfxSource"] = mixer.FindMatchingGroups("SFX").Single(g => g.name == "SFX"),
            };
            var sources = new[]
            {
                Ref<AudioSource>(controller, "bgmSourceA"),
                Ref<AudioSource>(controller, "bgmSourceB"),
                Ref<AudioSource>(controller, "sfxSource"),
            };
            Assert.That(sources.Distinct().Count(), Is.EqualTo(3));
            for (var i = 0; i < sources.Length; i++)
            {
                var field = i == 0 ? "bgmSourceA" : i == 1 ? "bgmSourceB" : "sfxSource";
                Assert.That(sources[i].outputAudioMixerGroup, Is.SameAs(groups[field]), field);
                TestContext.WriteLine($"AudioRuntime/{sources[i].name}.outputAudioMixerGroup = {sources[i].outputAudioMixerGroup.name}");
            }

            var voiceSource = GameObjectAt("AudioRuntime/VoiceSource").GetComponent<AudioSource>();
            Assert.That(voiceSource, Is.Not.Null);
            Assert.That(voiceSource.outputAudioMixerGroup,
                Is.SameAs(mixer.FindMatchingGroups("Voice").Single(g => g.name == "Voice")));
            var voicePresenter = Single<VoiceOverPresenter>();
            Assert.That(ReferencedObjects<AudioSource>(voicePresenter), Is.EquivalentTo(new[] { voiceSource }));
            var optionalVoice = Single<VNOptionalVoicePresenter>();
            AssertRef(optionalVoice, "voiceOverPresenter", voicePresenter);
            AssertRef(Single<VNYarnAudioCommands>(), "audioController", controller);
        }

        [Test]
        public void SaveLoadConvenienceAndInputShareTheAuthoritativeAdvanceAndGate()
        {
            var runner = Single<DialogueRunner>();
            var session = Single<VNDialogueSessionState>();
            var gate = Single<VNInteractionGate>();
            var saveLoad = Single<VNSaveLoadController>();
            var convenience = Single<VNConvenienceController>();
            var modalController = Single<VNConvenienceModalController>();
            var router = Single<VNConvenienceInputRouter>();
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);

            Assert.That(input, Is.Not.Null);
            AssertRef(saveLoad, "dialogueRunner", runner);
            AssertRef(saveLoad, "sessionState", session);
            AssertRef(saveLoad, "interactionGate", gate);
            AssertRef(gate, "sessionState", session);
            AssertRef(gate, "saveLoadController", saveLoad);
            AssertRef(convenience, "sessionState", session);
            AssertRef(convenience, "interactionGate", gate);
            AssertRef(convenience, "advanceBridge", Single<VNLineAdvancerInputBridge>());
            AssertRef(convenience, "saveLoadController", saveLoad);
            AssertRef(modalController, "interactionGate", gate);
            AssertRef(modalController, "convenienceController", convenience);
            AssertRef(modalController, "backlogModal", Single<VNBacklogModal>());
            AssertRef(modalController, "settingsModal", Single<VNSettingsModal>());
            AssertRef(modalController, "recordsModal", Single<VNRecordsModal>());

            foreach (var field in new[] { "advanceAction", "toggleAutoAction", "skipHoldAction", "toggleHideAction", "quickSaveAction", "quickLoadAction", "cancelAction" })
            {
                var action = Ref<InputActionReference>(router, field);
                Assert.That(action.asset, Is.SameAs(input), field);
            }
            var advance = Ref<InputActionReference>(router, "advanceAction").action;
            var cancel = Ref<InputActionReference>(router, "cancelAction").action;
            Assert.That(advance, Is.Not.Null);
            Assert.That(cancel, Is.Not.Null);
            Assert.That(advance.bindings.Any(b => b.effectivePath == "<Keyboard>/space"), Is.True);
            Assert.That(cancel.bindings.Any(b => b.effectivePath == "<Keyboard>/escape"), Is.True);
            var saveAdvance = Ref<InputActionReference>(saveLoad, "dialogueAdvanceAction");
            Assert.That(saveAdvance.action.id, Is.EqualTo(advance.id));
            var eventSystem = Single<EventSystem>();
            var uiModule = Single<InputSystemUIInputModule>();
            Assert.That(uiModule.gameObject, Is.SameAs(eventSystem.gameObject));
            var uiActions = uiModule.actionsAsset;
            Assert.That(uiActions, Is.Not.Null, "EventSystem UI actions asset is required.");
            var uiMap = uiActions.FindActionMap("UI", false);
            Assert.That(uiMap, Is.Not.Null, "EventSystem UI action map is required.");
            AssertUiAction(uiModule.point, uiActions, uiMap, "Point");
            AssertUiAction(uiModule.leftClick, uiActions, uiMap, "Click");
            AssertUiAction(uiModule.scrollWheel, uiActions, uiMap, "ScrollWheel");
            AssertUiAction(uiModule.move, uiActions, uiMap, "Navigate");
            AssertUiAction(uiModule.submit, uiActions, uiMap, "Submit");
            AssertUiAction(uiModule.cancel, uiActions, uiMap, "Cancel");
            Assert.That(All<EventSystem>(), Has.Length.EqualTo(1));
        }

        [Test]
        public void SettingsMetaProgressAndRecordsShareRuntimeOwnerAndDependencyOrder()
        {
            var runner = Single<DialogueRunner>();
            var settings = Single<VNSettingsRuntimeBootstrap>();
            var meta = Single<VNMetaProgressRuntimeBootstrap>();
            var records = Single<VNRecordsRuntimeBootstrap>();
            var session = Single<VNDialogueSessionState>();
            Assert.That(settings.gameObject, Is.SameAs(meta.gameObject));
            Assert.That(meta.gameObject, Is.SameAs(records.gameObject));
            Assert.That(meta.gameObject, Is.SameAs(session.gameObject));
            Assert.That(settings.isActiveAndEnabled && meta.isActiveAndEnabled && records.isActiveAndEnabled, Is.True);
            AssertRef(settings, "dialogueRunner", runner);
            AssertRef(meta, "dialogueRunner", runner);
            AssertRef(records, "metaProgressBootstrap", meta);
            AssertRef(records, "yarnProject", Ref<YarnProject>(runner, "yarnProject"));

            Assert.That(ExecutionOrder(typeof(VNMetaProgressRuntimeBootstrap)), Is.EqualTo(-2));
            Assert.That(ExecutionOrder(typeof(VNSettingsRuntimeBootstrap)), Is.EqualTo(-1));
            Assert.That(ExecutionOrder(typeof(VNRecordsRuntimeBootstrap)), Is.EqualTo(-1));
            Assert.That(MonoImporter.GetExecutionOrder(MonoScript.FromMonoBehaviour(runner)), Is.Zero);

            var timeline = Ref<VNTimelineCatalog>(records, "timelineCatalog");
            var gallery = Ref<VNCGGalleryCatalog>(records, "galleryCatalog");
            var archive = Ref<VNArchiveCatalog>(records, "archiveCatalog");
            var achievements = Ref<VNAchievementCatalog>(records, "achievementCatalog");
            var mapping = Ref<VNRecordsUnlockMapping>(records, "unlockMapping");
            Assert.That(timeline.Entries.Count, Is.EqualTo(8));
            Assert.That(gallery.Entries.Count, Is.EqualTo(6));
            Assert.That(archive.Entries.Count, Is.EqualTo(5));
            Assert.That(achievements.Achievements.Count, Is.EqualTo(4));
            Assert.That(mapping.Rules.Count, Is.EqualTo(12));
            Assert.That(records.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            TestContext.WriteLine("Start order: MetaProgress -2; Settings and Records -1; DialogueRunner 0.");
        }

        [Test]
        public void ReplayProfileAndProloguePanels_AreWiredToTheirOwnApprovedAuthorities()
        {
            var runner = Single<DialogueRunner>();
            var productionProject = Ref<YarnProject>(runner, "yarnProject");
            var records = Single<VNRecordsRuntimeBootstrap>();
            var replayProject = Ref<YarnProject>(records, "replayYarnProject");
            Assert.That(AssetDatabase.GetAssetPath(replayProject), Is.EqualTo(ReplayYarnPath));
            Assert.That(AssetDatabase.AssetPathToGUID(ReplayYarnPath), Is.Not.EqualTo(AssetDatabase.AssetPathToGUID(ProductionYarnPath)));
            Assert.That(replayProject.NodeNames, Does.Contain("REPLAY_S001_MORNING"));
            Assert.That(replayProject.NodeNames, Does.Contain("REPLAY_S002_PHOTO"));
            Assert.That(productionProject.NodeNames, Does.Not.Contain("REPLAY_S001_MORNING"));
            Assert.That(productionProject.NodeNames, Does.Not.Contain("REPLAY_S002_PHOTO"));

            var browser = Single<VNProfileBrowser>();
            AssertRef(browser, "root", browser.GetComponent<CanvasGroup>());
            AssertRef(browser, "interactionGate", Single<VNInteractionGate>());
            AssertRef(browser, "saveLoad", Single<VNSaveLoadController>());
            Assert.That(Ref<GameObject>(browser, "overview").transform.IsChildOf(browser.transform), Is.True);
            Assert.That(Ref<GameObject>(browser, "detail").transform.IsChildOf(browser.transform), Is.True);
            Assert.That(RefArray<Button>(browser, "profileButtons"), Has.Length.EqualTo(3));
            Assert.That(RefArray<TMP_Text>(browser, "profileNames"), Has.Length.EqualTo(3));
            Assert.That(RefArray<TMP_Text>(browser, "reviewedLabels"), Has.Length.EqualTo(3));

            var information = Single<VNInformationOverlay>();
            AssertRef(information, "root", information.GetComponent<CanvasGroup>());
            AssertOwnedText(information, Ref<TMP_Text>(information, "titleText"), "titleText");
            AssertOwnedText(information, Ref<TMP_Text>(information, "bodyText"), "bodyText");
            AssertRef(information, "visibility", Single<VNUIVisibilityController>());
            AssertRef(information, "saveLoad", Single<VNSaveLoadController>());

            var panels = Single<VNProloguePanels>();
            AssertRef(panels, "dialogueRunner", runner);
            AssertRef(panels, "saveLoad", Single<VNSaveLoadController>());
            AssertRef(panels, "diagram", GroupAt("VNCanvas/M1007MedicalDiagram"));
            AssertRef(panels, "endTitle", GroupAt("VNCanvas/M1007EndTitle"));
            Assert.That(GroupAt("VNCanvas/M1007MedicalDiagram").alpha, Is.Zero);
            Assert.That(GroupAt("VNCanvas/M1007EndTitle").alpha, Is.Zero);
        }

        private IEnumerable<MonoBehaviour> RequiredProjectComponents()
        {
            return new MonoBehaviour[]
            {
                Single<VNLineLifecyclePresenter>(), Single<VNLineAdvancerInputBridge>(),
                Single<VNConvenienceController>(), Single<VNConvenienceInputRouter>(),
                Single<VNInteractionGate>(), Single<VNSaveLoadController>(),
                All<VNPresentationController>().Single(p => p.name == "PresentationRuntime"),
                Single<VNAudioController>(),
                Single<VNSettingsRuntimeBootstrap>(), Single<VNMetaProgressRuntimeBootstrap>(),
                Single<VNRecordsRuntimeBootstrap>(), Single<VNProloguePanels>(),
                Single<VNProfileBrowser>(), Single<VNInformationOverlay>(),
            };
        }

        private static Dictionary<string, Type> ProductionCommandOwners() => new(StringComparer.Ordinal)
        {
            ["bgm_play"] = typeof(VNYarnAudioCommands), ["bgm_stop"] = typeof(VNYarnAudioCommands),
            ["sfx_play"] = typeof(VNYarnAudioCommands), ["sfx_wait"] = typeof(VNYarnAudioCommands),
            ["vn_bg_crossfade"] = typeof(VNYarnTransitionCommands), ["vn_cg"] = typeof(VNYarnPresentationCommands),
            ["vn_clear_bg"] = typeof(VNYarnPresentationCommands), ["vn_clear_cg"] = typeof(VNYarnPresentationCommands),
            ["vn_expression"] = typeof(VNYarnPresentationCommands), ["vn_hide"] = typeof(VNYarnPresentationCommands),
            ["vn_move"] = typeof(VNYarnPresentationCommands), ["vn_scale"] = typeof(VNYarnPresentationCommands),
            ["vn_show"] = typeof(VNYarnPresentationCommands), ["vn_fade_from_black"] = typeof(VNYarnTransitionCommands),
            ["vn_fade_to_black"] = typeof(VNYarnTransitionCommands), ["vn_local_effect"] = typeof(VNYarnTransitionCommands),
            ["vn_manual_hold"] = typeof(VNYarnManualHoldCommands), ["vn_info_hide"] = typeof(VNYarnInformationCommands),
            ["vn_info_show"] = typeof(VNYarnInformationCommands), ["vn_medical_diagram_hide"] = typeof(VNProloguePanels),
            ["vn_medical_diagram_show"] = typeof(VNProloguePanels), ["vn_part_end_title"] = typeof(VNProloguePanels),
            ["vn_profile_begin"] = typeof(VNYarnProfileCommands), ["vn_profile_review_complete"] = typeof(VNYarnProfileCommands),
            ["vn_profile_silent_complete"] = typeof(VNYarnProfileCommands), ["vn_profile_wait"] = typeof(VNYarnProfileCommands),
            ["wait"] = typeof(VNYarnTrackedWaitCommands),
        };

        private static bool IsCanonicalPrologueNode(string nodeName) =>
            Regex.IsMatch(nodeName, "^PROLOGUE_(?:S00[1-8]|LINE_S00[1-8])");

        private void AssertSlotSet(IReadOnlyList<VNCharacterSlotView> slots, string owner)
        {
            Assert.That(slots, Has.Count.EqualTo(6), owner);
            Assert.That(slots.Select(s => s.Slot).Distinct().Count(), Is.EqualTo(6), owner);
            CollectionAssert.AreEquivalent(Enum.GetValues(typeof(VNCharacterSlot)), slots.Select(s => s.Slot), owner);
            Assert.That(slots.All(s => s.IsConfigured), Is.True, owner);
            Assert.That(slots.Select(s => Ref<Transform>(s, "visualRoot")).Distinct().Count(), Is.EqualTo(6), owner);
        }

        private T[] All<T>() where T : Component => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();

        private Component Single(Type type)
        {
            var values = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Component>(true))
                .Where(component => component != null && type.IsInstanceOfType(component)).ToArray();
            Assert.That(values, Has.Length.EqualTo(1), type.Name + " in VN_Main");
            return values[0];
        }

        private T Single<T>() where T : Component
        {
            var values = All<T>();
            Assert.That(values, Has.Length.EqualTo(1), typeof(T).Name + " in VN_Main");
            return values[0];
        }

        private static T Ref<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object
        {
            var property = new SerializedObject(owner).FindProperty(field);
            Assert.That(property, Is.Not.Null, owner.name + "." + field);
            Assert.That(property.propertyType, Is.EqualTo(SerializedPropertyType.ObjectReference), owner.name + "." + field);
            var value = property.objectReferenceValue as T;
            Assert.That(value, Is.Not.Null, owner.name + "." + field);
            return value;
        }

        private static T[] RefArray<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object
        {
            var property = new SerializedObject(owner).FindProperty(field);
            Assert.That(property, Is.Not.Null, owner.name + "." + field);
            Assert.That(property.isArray, Is.True, owner.name + "." + field);
            var values = new T[property.arraySize];
            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                Assert.That(item.propertyType, Is.EqualTo(SerializedPropertyType.ObjectReference), owner.name + "." + field + "[" + i + "]");
                values[i] = item.objectReferenceValue as T;
                Assert.That(values[i], Is.Not.Null, owner.name + "." + field + "[" + i + "]");
            }
            return values;
        }

        private static List<T> RefList<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object =>
            RefArray<T>(owner, field).ToList();

        private static void AssertRef(UnityEngine.Object owner, string field, UnityEngine.Object expected)
        {
            var property = new SerializedObject(owner).FindProperty(field);
            Assert.That(property, Is.Not.Null, owner.name + "." + field);
            var actual = property.objectReferenceValue;
            Assert.That(actual, Is.Not.Null, owner.name + "." + field);
            Assert.That(actual, Is.SameAs(expected), owner.name + "." + field);
            TestContext.WriteLine($"{Hierarchy(owner)} | {owner.GetType().Name}.{field} | {expected.GetType().Name} {Hierarchy(expected)} | PASS");
        }

        private static IEnumerable<T> ReferencedObjects<T>(UnityEngine.Object owner) where T : UnityEngine.Object
        {
            var serialized = new SerializedObject(owner);
            var property = serialized.GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is T value)
                    yield return value;
        }

        private GameObject GameObjectAt(string relativePath) =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject).Single(go => Hierarchy(go.transform) == "/" + relativePath);

        private CanvasGroup GroupAt(string relativePath)
        {
            var group = GameObjectAt(relativePath).GetComponent<CanvasGroup>();
            Assert.That(group, Is.Not.Null, relativePath);
            return group;
        }

        private static void AssertOwnedText(Component owner, TMP_Text text, string field)
        {
            Assert.That(text.transform == owner.transform || text.transform.IsChildOf(owner.transform), Is.True,
                owner.name + "." + field + " must be owned by the information overlay hierarchy.");
        }

        private static void AssertUiAction(
            InputActionReference reference,
            InputActionAsset asset,
            InputActionMap map,
            string actionName)
        {
            Assert.That(reference, Is.Not.Null, "EventSystem UI " + actionName + " action reference.");
            Assert.That(reference.asset, Is.SameAs(asset), "EventSystem UI " + actionName + " action asset.");
            Assert.That(reference.action, Is.SameAs(map.FindAction(actionName, false)),
                "EventSystem UI " + actionName + " action mapping.");
        }

        private static int ExecutionOrder(Type type)
        {
            var attribute = (DefaultExecutionOrder)Attribute.GetCustomAttribute(type, typeof(DefaultExecutionOrder));
            Assert.That(attribute, Is.Not.Null, type.Name);
            return attribute.order;
        }

        private static string Hierarchy(Component component) => Hierarchy(component.transform);

        private static string Hierarchy(Transform transform) => transform.parent == null
            ? "/" + transform.name
            : Hierarchy(transform.parent) + "/" + transform.name;

        private static string Hierarchy(UnityEngine.Object value) => value is Component component
            ? Hierarchy(component)
            : value is GameObject gameObject ? Hierarchy(gameObject.transform) : AssetDatabase.GetAssetPath(value);
    }
}
