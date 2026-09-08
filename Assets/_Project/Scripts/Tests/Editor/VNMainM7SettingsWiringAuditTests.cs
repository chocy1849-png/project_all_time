using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
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
    public sealed class VNMainM7SettingsWiringAuditTests
    {
        internal const string ScenePath = "Assets/_Project/Scenes/VN_Main.unity";
        internal const string MixerPath = "Assets/_Project/Audio/VNAudioMixer.mixer";
        internal const string InputPath = "Assets/_Project/Settings/Input/VNInputActions.inputactions";
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
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }

        [Test]
        public void SettingsGraph_HasFiveDistinctCategoriesAndHiddenActiveModal()
        {
            var panel = Single<VNSettingsPanel>();
            var modal = Single<VNSettingsModal>();
            Assert.That(modal.gameObject.activeInHierarchy, Is.True);
            var group = Ref<CanvasGroup>(modal, "modalCanvasGroup");
            Assert.That(group, Is.SameAs(modal.GetComponent<CanvasGroup>()));
            Assert.That(group.alpha, Is.Zero);
            Assert.That(group.interactable, Is.False);
            Assert.That(group.blocksRaycasts, Is.False);
            Assert.That(Ref<VNSettingsPanel>(modal, "settingsPanel"), Is.SameAs(panel));
            Empty(Ref<Button>(modal, "closeButton"));
            Assert.That(panel.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            var categories = new[] { "display", "text", "audio", "gameplay", "controls" };
            var buttons = categories.Select(c => Ref<Button>(panel, c + "CategoryButton")).ToArray();
            var contents = categories.Select(c => Ref<GameObject>(panel, c + "Content")).ToArray();
            var views = categories.Select(c => Ref<Component>(panel, c + "View")).ToArray();
            Assert.That(buttons.Distinct().Count(), Is.EqualTo(5));
            Assert.That(contents.Distinct().Count(), Is.EqualTo(5));
            Assert.That(views.Distinct().Count(), Is.EqualTo(5));
            for (var i = 0; i < categories.Length; i++)
            {
                Empty(buttons[i]);
                Assert.That(views[i].transform.IsChildOf(contents[i].transform), Is.True);
                Assert.That(contents[i].transform.IsChildOf(panel.transform), Is.True);
            }
            TestContext.WriteLine("Settings Panel Inspector target: " + Hierarchy(panel.transform));
        }

        [Test]
        public void CategoryControls_HaveExactContractsAndNoPersistentMutationEvents()
        {
            var panel = Single<VNSettingsPanel>();
            var display = Ref<VNDisplaySettingsView>(panel, "displayView");
            Assert.That(display.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            foreach (var field in new[] { "displayModeDropdown", "resolutionDropdown" })
                Assert.That(Ref<TMP_Dropdown>(display, field).onValueChanged.GetPersistentEventCount(), Is.Zero);

            var text = Ref<VNTextSettingsView>(panel, "textView");
            Assert.That(text.TryValidateWiring(out diagnostic), Is.True, diagnostic);
            var intended = new List<Slider>
            {
                CheckSlider(text, "textSpeedSlider", "textSpeedCommit", 20, 120, true),
                CheckSlider(text, "autoSpeedSlider", "autoSpeedCommit", 0, 1, false)
            };
            var audio = Ref<VNAudioSettingsView>(panel, "audioView");
            Assert.That(audio.TryValidateWiring(out diagnostic), Is.True, diagnostic);
            foreach (var channel in new[] { "master", "bgm", "sfx", "voice" })
                intended.Add(CheckSlider(audio, channel + "Slider", channel + "Commit", 0, 1, false));
            Assert.That(intended.Distinct().Count(), Is.EqualTo(6));
            CollectionAssert.AreEquivalent(intended, panel.GetComponentsInChildren<Slider>(true));
            var audioProperties = new SerializedObject(audio).GetIterator();
            while (audioProperties.Next(true))
                if (audioProperties.propertyType == SerializedPropertyType.ObjectReference)
                    Assert.That(audioProperties.objectReferenceValue, Is.Not.InstanceOf<AudioSource>());

            var gameplay = Ref<VNGameplaySettingsView>(panel, "gameplayView");
            Assert.That(gameplay.TryValidateWiring(out diagnostic), Is.True, diagnostic);
            var toggles = new[] { Ref<Toggle>(gameplay, "skipUnreadToggle"), Ref<Toggle>(gameplay, "screenShakeToggle") };
            Assert.That(toggles.Distinct().Count(), Is.EqualTo(2));
            CollectionAssert.AreEquivalent(toggles, gameplay.GetComponentsInChildren<Toggle>(true));
            foreach (var toggle in toggles) Assert.That(toggle.onValueChanged.GetPersistentEventCount(), Is.Zero);
        }

        [Test]
        public void Controls_HaveExactlySixUniqueTargetsAndRuntimeOwnedButtons()
        {
            var controls = Ref<VNControlsSettingsView>(Single<VNSettingsPanel>(), "controlsView");
            Assert.That(controls.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            var array = new SerializedObject(controls).FindProperty("rebindItems");
            Assert.That(array.arraySize, Is.EqualTo(6));
            var items = Enumerable.Range(0, array.arraySize)
                .Select(i => array.GetArrayElementAtIndex(i).objectReferenceValue as VNRebindItem).ToArray();
            CollectionAssert.AreEquivalent(Enum.GetValues(typeof(VNRebindTarget)), items.Select(i => i.Target));
            CollectionAssert.AreEquivalent(items, controls.GetComponentsInChildren<VNRebindItem>(true));
            var buttons = new List<Button> { Ref<Button>(controls, "resetAllButton") };
            foreach (var item in items)
            {
                Assert.That(item.TryValidateWiring(out diagnostic), Is.True, diagnostic);
                Ref<TMP_Text>(item, "bindingText");
                buttons.Add(Ref<Button>(item, "rebindButton"));
                buttons.Add(Ref<Button>(item, "resetButton"));
            }
            Assert.That(buttons.Distinct().Count(), Is.EqualTo(13));
            foreach (var button in buttons) Empty(button);
        }

        [Test]
        public void CanvasInputAndDialogue_RetainProductionBaseline()
        {
            var canvas = Single<VNSettingsPanel>().GetComponentInParent<Canvas>();
            Assert.That(canvas.name, Is.EqualTo("VNCanvas"));
            var scaler = canvas.GetComponent<CanvasScaler>();
            Assert.That(scaler, Is.Not.Null);
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920, 1080)));
            Assert.That(scaler.screenMatchMode, Is.EqualTo(CanvasScaler.ScreenMatchMode.MatchWidthOrHeight));
            Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(0.5f));
            var events = Single<EventSystem>();
            var module = Single<InputSystemUIInputModule>();
            Assert.That(module.gameObject, Is.SameAs(events.gameObject));
            Assert.That(module.enabled, Is.True);
            Assert.That(module.actionsAsset, Is.Not.Null);
            var runner = Single<DialogueRunner>();
            Assert.That(new SerializedObject(runner).FindProperty("startNode").stringValue, Is.EqualTo("M2_UI_START"));
            TestContext.WriteLine("Dialogue Runner Inspector target: " + Hierarchy(runner.transform));
            TestContext.WriteLine("Dialogue Runner autoStart: " + new SerializedObject(runner).FindProperty("autoStart").boolValue);
            Assert.That(MonoImporter.GetExecutionOrder(MonoScript.FromMonoBehaviour(runner)), Is.Zero);
            foreach (var root in scene.GetRootGameObjects())
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero, Hierarchy(transform));
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            Assert.That(input, Is.Not.Null);
            var router = Single<VNConvenienceInputRouter>();
            foreach (var field in new[] { "advanceAction", "toggleAutoAction", "skipHoldAction", "toggleHideAction", "quickSaveAction", "quickLoadAction", "cancelAction" })
                Assert.That(Ref<InputActionReference>(router, field).asset, Is.SameAs(input), field);
            using var rebind = new VNInputRebindService(new VNSettingsService(new VNSettingsRepository()), input, router);
            Assert.That(rebind.TryValidateInputContract(out var diagnostic), Is.True, diagnostic);
        }

        [Test]
        public void RealMixer_ResolvesAllGroupsAndExactExposedParameters_ReadOnly()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            Assert.That(mixer, Is.Not.Null);
            foreach (var group in new[] { "Master", "BGM", "SFX", "Voice" })
                Assert.That(mixer.FindMatchingGroups(group).Count(g => g.name == group), Is.EqualTo(1), group);
            foreach (var parameter in new[] { "MasterVolumeDb", "BgmVolumeDb", "SfxVolumeDb", "VoiceVolumeDb" })
                Assert.That(mixer.GetFloat(parameter, out _), Is.True, parameter);
        }

        // Run explicitly after the user's Inspector wiring gate; never skip a missing bootstrap.
        [Test, Category("M7BootstrapSceneGate")]
        public void Bootstrap_IsWiredOnceToAuthoritativeProductionObjects()
        {
            var bootstrap = Single<VNSettingsRuntimeBootstrap>();
            var convenience = Single<VNConvenienceController>();
            Assert.That(bootstrap.gameObject, Is.SameAs(convenience.gameObject));
            Assert.That(bootstrap.name, Is.EqualTo("VNConvenienceRuntime"));
            Assert.That(bootstrap.GetComponent<VNConvenienceInputRouter>(), Is.SameAs(Single<VNConvenienceInputRouter>()));
            Assert.That(Ref<DialogueRunner>(bootstrap, "dialogueRunner"), Is.SameAs(Single<DialogueRunner>()));
            Assert.That(Ref<VNSettingsPanel>(bootstrap, "settingsPanel"), Is.SameAs(Single<VNSettingsPanel>()));
            Assert.That(Ref<AudioMixer>(bootstrap, "audioMixer"), Is.SameAs(AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath)));
            Assert.That(Ref<InputActionAsset>(bootstrap, "inputActions"), Is.SameAs(AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath)));
            Assert.That(bootstrap.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            Assert.That(bootstrap.isActiveAndEnabled, Is.True);
            // MonoImporter reports the Editor setting, not DefaultExecutionOrder.
            // Keep the attribute contract explicit and reject a competing Editor override.
            var order = (DefaultExecutionOrder)Attribute.GetCustomAttribute(typeof(VNSettingsRuntimeBootstrap), typeof(DefaultExecutionOrder));
            Assert.That(order, Is.Not.Null);
            Assert.That(order.order, Is.EqualTo(-1));
            Assert.That(MonoImporter.GetExecutionOrder(MonoScript.FromMonoBehaviour(bootstrap)), Is.Zero);
        }

        private static Slider CheckSlider(Component view, string sliderField, string commitField, float min, float max, bool whole)
        {
            var slider = Ref<Slider>(view, sliderField);
            var commit = Ref<VNSettingsSliderCommit>(view, commitField);
            Assert.That(commit, Is.SameAs(slider.GetComponent<VNSettingsSliderCommit>()));
            Assert.That(commit.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            Assert.That(Ref<Slider>(commit, "slider"), Is.SameAs(slider));
            Assert.That(slider.minValue, Is.EqualTo(min));
            Assert.That(slider.maxValue, Is.EqualTo(max));
            Assert.That(slider.wholeNumbers, Is.EqualTo(whole));
            Assert.That(slider.onValueChanged.GetPersistentEventCount(), Is.Zero);
            return slider;
        }

        private static void Empty(Button button) => Assert.That(button.onClick.GetPersistentEventCount(), Is.Zero, button.name);
        internal static T Ref<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object
        {
            var property = new SerializedObject(owner).FindProperty(field);
            Assert.That(property, Is.Not.Null, field);
            var value = property.objectReferenceValue as T;
            Assert.That(value, Is.Not.Null, owner.name + "." + field);
            return value;
        }
        private T Single<T>() where T : Component
        {
            var components = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();
            Assert.That(components, Has.Length.EqualTo(1), typeof(T).Name);
            return components[0];
        }
        private static string Hierarchy(Transform transform) => transform.parent == null ? transform.name : Hierarchy(transform.parent) + "/" + transform.name;
    }
}
