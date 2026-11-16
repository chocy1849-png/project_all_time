using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectAllTime.VN.Dialogue;

public static class M13SharedPrefabBuilder
{
    const string Dir = "Assets/_Project/Prefabs/UI/M13/";
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/B_" + name + ".png");
    static Image Child(Transform parent, string name, string sprite, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform; rect.sizeDelta = size;
        var image = go.GetComponent<Image>(); image.sprite = Sprite(sprite); image.type = Image.Type.Sliced; image.raycastTarget = false;
        return image;
    }
    [MenuItem("Tools/M13/Build shared panel and controls")]
    public static void Build()
    {
        Make("SharedPanel", "panel", false, new Vector2(800, 600));
        Make("SharedButton", "button", true, new Vector2(240, 56));
        Make("SharedTab", "tab", true, new Vector2(230, 56));
        AssetDatabase.SaveAssets();
    }
    private static void Make(string name, string spriteName, bool button, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        try
        {
            ((RectTransform)go.transform).sizeDelta = size;
            var image = go.GetComponent<Image>(); image.sprite = Sprite(spriteName); image.type = Image.Type.Sliced;
            Button control = null;
            if (button)
            {
                control = go.AddComponent<Button>(); control.targetGraphic = image;
                var focus = Child(go.transform, "Focus", "focus", size + new Vector2(6, 6));
                var active = Child(go.transform, "Active", "selected", new Vector2(size.x - 20, 6));
                ((RectTransform)active.transform).anchoredPosition = new Vector2(0, -size.y / 2 + 3);
                go.AddComponent<M13SharedControlState>().Configure(focus.gameObject, active.gameObject);
            }
            else
            {
                var header = Child(go.transform, "Header", "secondary", new Vector2(size.x - 40, 80));
                ((RectTransform)header.transform).anchoredPosition = new Vector2(0, size.y / 2 - 60);
                var content = Child(go.transform, "Content", "secondary", new Vector2(size.x - 40, size.y - 140));
                ((RectTransform)content.transform).anchoredPosition = new Vector2(0, -40);
            }
            var binding = go.AddComponent<VNUIThemeBinding>(); var serialized = new SerializedObject(binding);
            serialized.FindProperty("theme").objectReferenceValue = AssetDatabase.LoadAssetAtPath<VNUITheme>("Assets/_Project/Settings/M11_UITheme.asset");
            if (button) serialized.FindProperty("button").objectReferenceValue = control; else serialized.FindProperty("panel").objectReferenceValue = image;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, Dir + "M13_" + name + ".prefab");
        }
        finally { Object.DestroyImmediate(go); }
    }
}
