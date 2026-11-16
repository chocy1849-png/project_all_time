using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectAllTime.VN.Dialogue;

public static class M13SharedPrefabBuilder
{
    [MenuItem("Tools/M13/Build shared panel and controls")]
    public static void Build()
    {
        const string dir = "Assets/_Project/Prefabs/UI/M13";
        Make("SharedPanel", "panel", false, new Vector2(800, 600), dir);
        Make("SharedButton", "button", true, new Vector2(240, 56), dir);
        Make("SharedTab", "tab", true, new Vector2(230, 56), dir);
        AssetDatabase.SaveAssets();
    }
    private static void Make(string name, string spriteName, bool button, Vector2 size, string dir)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        try
        {
            ((RectTransform)go.transform).sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/B_" + spriteName + ".png");
            image.type = Image.Type.Sliced;
            if (button) { var control = go.AddComponent<Button>(); control.targetGraphic = image; }
            go.AddComponent<VNUIThemeBinding>();
            PrefabUtility.SaveAsPrefabAsset(go, dir + "/M13_" + name + ".prefab");
        }
        finally { Object.DestroyImmediate(go); }
    }
}
