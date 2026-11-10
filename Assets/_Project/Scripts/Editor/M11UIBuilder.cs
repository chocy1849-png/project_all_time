using System;
using System.IO;
using System.Linq;
using ProjectAllTime.VN.Dialogue;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.Editor
{
    /// <summary>Builds serialized M11 UI assets and updates production references through Unity APIs.</summary>
    public static class M11UIBuilder
    {
        private const string Root = "Assets/_Project/Prefabs/UI/M11";
        private const string Art = "Assets/_Project/Art/UI/M11/";
        private const string ThemePath = "Assets/_Project/Settings/M11_UITheme.asset";
        private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        private static void Set(UnityEngine.Object obj, string property, UnityEngine.Object value)
        {
            var so = new SerializedObject(obj); so.FindProperty(property).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Text(UnityEngine.Object obj, string property, string value)
        {
            var so = new SerializedObject(obj); so.FindProperty(property).stringValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static GameObject ImageObject(string name, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var image = go.GetComponent<Image>(); image.sprite = sprite; image.type = Image.Type.Sliced; image.raycastTarget = false;
            return go;
        }
        private static void Stretch(RectTransform r)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero; }
        private static GameObject Save(GameObject go, string name)
        {
            var asset = PrefabUtility.SaveAsPrefabAsset(go, Root + "/" + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(go); return asset;
        }
        private static GameObject Instance(GameObject prefab, Transform parent)
        { var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent); return go; }

        [MenuItem("All Time/M11/Apply theme bindings")]
        public static void ApplyThemeBindings()
        {
            var theme = AssetDatabase.LoadAssetAtPath<VNUITheme>(ThemePath);
            if (theme == null) throw new InvalidOperationException("M11 theme missing.");
            var panelPath = "Assets/_Project/Prefabs/UI/VNDialoguePanel.prefab";
            var root = PrefabUtility.LoadPrefabContents(panelPath);
            var panel = root.transform.Find("DialoguePanel");
            var speaker = panel.Find("SpeakerNameContainer/SpeakerName").GetComponent<TMP_Text>();
            speaker.enableAutoSizing = true; speaker.fontSizeMin = 21; speaker.fontSizeMax = theme.speakerSize;
            speaker.textWrappingMode = TextWrappingModes.NoWrap;
            var binding = root.GetComponent<VNUIThemeBinding>() ?? root.AddComponent<VNUIThemeBinding>();
            Set(binding, "theme", theme); Set(binding, "speaker", speaker);
            Set(binding, "dialogue", panel.Find("DialogueText").GetComponent<TMP_Text>());
            Set(binding, "panel", panel.Find("M11_Frame").GetComponent<Image>()); binding.Apply();
            var marker = panel.Find("ContinueIndicator").gameObject;
            var oldIndicator = marker.GetComponent<VNContinueIndicator>();
            if (oldIndicator != null) UnityEngine.Object.DestroyImmediate(oldIndicator);
            var indicator = root.GetComponent<VNContinueIndicator>() ?? root.AddComponent<VNContinueIndicator>();
            Set(indicator, "marker", marker.GetComponent<Image>());
            PrefabUtility.SaveAsPrefabAsset(root, panelPath); PrefabUtility.UnloadPrefabContents(root);

            var buttonPath = Root + "/VNIconButton.prefab";
            var buttonRoot = PrefabUtility.LoadPrefabContents(buttonPath);
            var buttonBinding = buttonRoot.GetComponent<VNUIThemeBinding>() ?? buttonRoot.AddComponent<VNUIThemeBinding>();
            Set(buttonBinding, "theme", theme); Set(buttonBinding, "button", buttonRoot.GetComponent<Button>());
            Set(buttonBinding, "icon", buttonRoot.transform.Find("Icon").GetComponent<Image>());
            Set(buttonBinding, "tooltip", buttonRoot.transform.Find("Tooltip/Label").GetComponent<TMP_Text>());
            var iconTooltip = buttonRoot.GetComponent<VNIconTooltip>();
            Set(iconTooltip, "theme", theme); Set(iconTooltip, "icon", buttonRoot.transform.Find("Icon").GetComponent<Image>());
            buttonBinding.Apply(); PrefabUtility.SaveAsPrefabAsset(buttonRoot, buttonPath); PrefabUtility.UnloadPrefabContents(buttonRoot);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("All Time/M11/Build production UI")]
        public static void Build()
        {
            Directory.CreateDirectory(Root);
            AssetDatabase.Refresh();
            var theme = AssetDatabase.LoadAssetAtPath<VNUITheme>(ThemePath);
            if (theme == null) { theme = ScriptableObject.CreateInstance<VNUITheme>(); AssetDatabase.CreateAsset(theme, ThemePath); }
            var original = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/UI/VNDialoguePanel.prefab");
            if (original.transform.Find("DialoguePanel/M11_Frame") != null)
            {
                PrefabUtility.UnloadPrefabContents(original);
                throw new InvalidOperationException("M11 is already installed; use Apply theme bindings instead of rebuilding scene references.");
            }
            var sourceFont = original.transform.Find("DialoguePanel/SpeakerNameContainer/SpeakerName").GetComponent<TMP_Text>().font;
            if (sourceFont == null || Sprite("dialogue_frame") == null) throw new InvalidOperationException("M11 font or sprites missing.");

            var frameAsset = Save(ImageObject("VNDialogueFrame", Sprite("dialogue_frame")), "VNDialogueFrame");
            var nameAsset = Save(ImageObject("VNNameplate", Sprite("nameplate")), "VNNameplate");
            var tooltip = ImageObject("VNTooltip", Sprite("tooltip_background"));
            tooltip.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 44);
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(tooltip.transform, false); Stretch(labelGo.GetComponent<RectTransform>());
            var label = labelGo.GetComponent<TextMeshProUGUI>(); label.font = sourceFont; label.fontSize = 22; label.color = theme.text;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; tooltip.SetActive(false);
            var tooltipAsset = Save(tooltip, "VNTooltip");

            var buttonGo = ImageObject("VNIconButton", Sprite("button_background"));
            buttonGo.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 44);
            var image = buttonGo.GetComponent<Image>(); image.raycastTarget = true;
            var button = buttonGo.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.normalColor = theme.normal; colors.highlightedColor = theme.hover;
            colors.selectedColor = theme.hover; colors.pressedColor = theme.pressed; colors.disabledColor = theme.disabled; colors.fadeDuration = .12f; button.colors = colors;
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(buttonGo.transform, false); var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(.5f, .5f); iconRect.sizeDelta = Vector2.one * theme.iconSize;
            iconGo.GetComponent<Image>().raycastTarget = false; iconGo.GetComponent<Image>().color = theme.text;
            var tip = Instance(tooltipAsset, buttonGo.transform); tip.name = "Tooltip";
            var tipRect = tip.GetComponent<RectTransform>(); tipRect.anchorMin = tipRect.anchorMax = new Vector2(.5f, 1); tipRect.pivot = new Vector2(.5f, 0); tipRect.anchoredPosition = new Vector2(0, 8);
            var tooltipComponent = buttonGo.AddComponent<VNIconTooltip>(); Set(tooltipComponent, "tooltip", tipRect); Set(tooltipComponent, "tooltipText", tip.GetComponentInChildren<TMP_Text>(true));
            var buttonAsset = Save(buttonGo, "VNIconButton");

            var panel = original.transform.Find("DialoguePanel");
            var oldPanel = panel.GetComponent<Image>(); oldPanel.color = Color.clear; oldPanel.raycastTarget = false;
            var frame = Instance(frameAsset, panel); frame.name = "M11_Frame"; frame.transform.SetAsFirstSibling(); Stretch(frame.GetComponent<RectTransform>());
            var nameplate = panel.Find("SpeakerNameContainer");
            nameplate.GetComponent<Image>().color = Color.clear; nameplate.GetComponent<Image>().raycastTarget = false;
            var plate = Instance(nameAsset, nameplate); plate.name = "M11_Nameplate"; plate.transform.SetAsFirstSibling(); Stretch(plate.GetComponent<RectTransform>());
            var speaker = nameplate.Find("SpeakerName").GetComponent<TMP_Text>(); speaker.fontSize = theme.speakerSize; speaker.color = theme.speaker; speaker.raycastTarget = false;
            var dialogue = panel.Find("DialogueText").GetComponent<TMP_Text>(); dialogue.fontSize = theme.dialogueSize; dialogue.color = theme.text; dialogue.raycastTarget = false;
            var marker = panel.Find("ContinueIndicator").GetComponent<Image>(); marker.sprite = Sprite("continue_marker"); marker.type = Image.Type.Simple; marker.raycastTarget = false;
            PrefabUtility.SaveAsPrefabAsset(original, "Assets/_Project/Prefabs/UI/VNDialoguePanel.prefab"); PrefabUtility.UnloadPrefabContents(original);

            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/VN_Main.unity");
            var bar = UnityEngine.Object.FindFirstObjectByType<VNQuickControlBar>(FindObjectsInactive.Include);
            if (bar == null) throw new InvalidOperationException("Production quick bar missing.");
            var fields = new[] { "nextButton", "hideButton", "backlogButton", "skipButton", "autoButton", "saveButton", "loadButton", "settingsButton", "recordsButton" };
            var ids = new[] { "next", "hide", "backlog", "skip", "auto", "save", "load", "settings", "records" };
            var labels = new[] { "다음", "UI 숨기기", "백로그", "스킵", "자동 진행", "저장", "불러오기", "설정", "기록" };
            var serialized = new SerializedObject(bar);
            for (var i = 0; i < fields.Length; i++)
            {
                var old = serialized.FindProperty(fields[i]).objectReferenceValue as Button;
                if (old == null) throw new InvalidOperationException("Missing button " + fields[i]);
                var sibling = old.transform.GetSiblingIndex();
                var replacement = Instance(buttonAsset, bar.transform); replacement.name = old.name; replacement.transform.SetSiblingIndex(sibling);
                replacement.transform.Find("Icon").GetComponent<Image>().sprite = Sprite("icon_" + ids[i]);
                Text(replacement.GetComponent<VNIconTooltip>(), "label", labels[i]);
                var dot = new GameObject("SelectedIndicator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                dot.transform.SetParent(replacement.transform, false); var dr = dot.GetComponent<RectTransform>();
                dr.anchorMin = dr.anchorMax = new Vector2(.5f, 0); dr.anchoredPosition = new Vector2(0, 3); dr.sizeDelta = new Vector2(16, 3);
                var di = dot.GetComponent<Image>(); di.color = theme.accent; di.raycastTarget = false; dot.SetActive(false);
                serialized.FindProperty(fields[i]).objectReferenceValue = replacement.GetComponent<Button>();
                if (ids[i] == "skip") serialized.FindProperty("skipSelectedIndicator").objectReferenceValue = dot;
                if (ids[i] == "auto") serialized.FindProperty("autoSelectedIndicator").objectReferenceValue = dot;
                UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var layout = bar.GetComponent<HorizontalLayoutGroup>(); layout.spacing = theme.spacing; layout.childControlWidth = false; layout.childControlHeight = false;
            var bg = bar.GetComponent<Image>(); bg.color = Color.clear; bg.raycastTarget = false;
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }
    }
}
