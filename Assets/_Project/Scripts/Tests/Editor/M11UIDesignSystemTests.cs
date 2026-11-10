using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M11UIDesignSystemTests
    {
        [Test]
        public void SpritesAndReusablePrefabs_AreImportedAndConfigured()
        {
            var names = new[] { "dialogue_frame", "nameplate", "button_background", "tooltip_background", "continue_marker",
                "icon_next", "icon_hide", "icon_backlog", "icon_skip", "icon_auto", "icon_save", "icon_load", "icon_settings", "icon_records" };
            foreach (var name in names)
            {
                var path = "Assets/_Project/Art/UI/M11/" + name + ".png";
                Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>(path), Is.Not.Null, name);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
                if (name == "dialogue_frame" || name == "nameplate" || name == "button_background" || name == "tooltip_background")
                    Assert.That(importer.spriteBorder.x, Is.GreaterThan(0), name);
            }
            foreach (var name in new[] { "VNIconButton", "VNTooltip", "VNNameplate", "VNDialogueFrame" })
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M11/" + name + ".prefab"), Is.Not.Null, name);
            Assert.That(AssetDatabase.LoadAssetAtPath<VNUITheme>("Assets/_Project/Settings/M11_UITheme.asset"), Is.Not.Null);
        }

        [Test]
        public void DialoguePrefab_KeepsPresenterAndNonblockingVisuals()
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/VNDialoguePanel.prefab");
            var presenter = root.GetComponent<LinePresenter>();
            Assert.That(presenter, Is.Not.Null);
            Assert.That(presenter.lineText, Is.Not.Null);
            Assert.That(presenter.lineText.font, Is.Not.Null);
            Assert.That(presenter.characterNameText, Is.Not.Null);
            Assert.That(root.transform.Find("DialoguePanel/M11_Frame").GetComponent<Image>().raycastTarget, Is.False);
            Assert.That(root.transform.Find("DialoguePanel/SpeakerNameContainer/M11_Nameplate").GetComponent<Image>().raycastTarget, Is.False);
            Assert.That(root.GetComponent<VNContinueIndicator>(), Is.Not.Null);
        }

        [Test]
        public void SharedTheme_AppliesPanelSpeakerAndButtonStates()
        {
            var theme = ScriptableObject.CreateInstance<VNUITheme>();
            var root = new GameObject("theme test", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(VNUIThemeBinding));
            var child = new GameObject("speaker", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            child.transform.SetParent(root.transform, false);
            try
            {
                theme.panel = Color.magenta; theme.speaker = Color.yellow; theme.hover = Color.cyan; theme.disabled = Color.gray;
                var binding = root.GetComponent<VNUIThemeBinding>();
                var so = new SerializedObject(binding);
                so.FindProperty("theme").objectReferenceValue = theme;
                so.FindProperty("panel").objectReferenceValue = root.GetComponent<Image>();
                so.FindProperty("button").objectReferenceValue = root.GetComponent<Button>();
                so.FindProperty("speaker").objectReferenceValue = child.GetComponent<TMP_Text>();
                so.ApplyModifiedPropertiesWithoutUndo(); binding.Apply();
                Assert.That(root.GetComponent<Image>().color, Is.EqualTo(Color.magenta));
                Assert.That(child.GetComponent<TMP_Text>().color, Is.EqualTo(Color.yellow));
                Assert.That(root.GetComponent<Button>().colors.highlightedColor, Is.EqualTo(Color.cyan));
                Assert.That(root.GetComponent<Button>().colors.disabledColor, Is.EqualTo(Color.gray));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(theme); }
        }

        [UnityTest]
        public System.Collections.IEnumerator SharedTheme_RefreshesLoadedBindingsAfterAssetEdit()
        {
            var theme = ScriptableObject.CreateInstance<VNUITheme>();
            var root = new GameObject("theme refresh test", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VNUIThemeBinding));
            try
            {
                var binding = new SerializedObject(root.GetComponent<VNUIThemeBinding>());
                binding.FindProperty("theme").objectReferenceValue = theme;
                binding.FindProperty("panel").objectReferenceValue = root.GetComponent<Image>();
                binding.ApplyModifiedPropertiesWithoutUndo();
                var editedTheme = new SerializedObject(theme);
                editedTheme.FindProperty("panel").colorValue = Color.magenta;
                editedTheme.ApplyModifiedPropertiesWithoutUndo();
                var deadline = EditorApplication.timeSinceStartup + 2;
                while (root.GetComponent<Image>().color != Color.magenta && EditorApplication.timeSinceStartup < deadline)
                    yield return null;
                Assert.That(root.GetComponent<Image>().color, Is.EqualTo(Color.magenta));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(theme); }
        }

        [Test]
        public void ProductionScene_HasNineAssignedSharedButtons()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/VN_Main.unity", OpenSceneMode.Additive);
            try
            {
                VNQuickControlBar bar = null;
                foreach (var root in scene.GetRootGameObjects())
                { bar = root.GetComponentInChildren<VNQuickControlBar>(true); if (bar != null) break; }
                Assert.That(bar, Is.Not.Null);
                var so = new SerializedObject(bar);
                foreach (var field in new[] { "nextButton", "hideButton", "backlogButton", "skipButton", "autoButton", "saveButton", "loadButton", "settingsButton", "recordsButton" })
                {
                    var button = so.FindProperty(field).objectReferenceValue as Button;
                    Assert.That(button, Is.Not.Null, field);
                    Assert.That(button.GetComponent<VNIconTooltip>(), Is.Not.Null, field);
                    Assert.That(button.transform.Find("Icon").GetComponent<Image>().sprite, Is.Not.Null, field);
                }
                Assert.That(so.FindProperty("skipSelectedIndicator").objectReferenceValue, Is.Not.Null);
                Assert.That(so.FindProperty("autoSelectedIndicator").objectReferenceValue, Is.Not.Null);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
