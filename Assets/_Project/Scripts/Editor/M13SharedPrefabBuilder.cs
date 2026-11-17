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
    static void Stretch(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax; }
    static TMP_FontAsset ProductionFont()
    {
        const string path = "Assets/_Project/Fonts/NotoSansKR/M13_Shared_Korean_SDF.asset";
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (font != null) return font;
        var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/NotoSansKR/NotoSansKR-Regular.ttf");
        font = TMP_FontAsset.CreateFontAsset(source, 48, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        AssetDatabase.CreateAsset(font, path);
        AssetDatabase.AddObjectToAsset(font.material, font);
        AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
        AssetDatabase.SaveAssets();
        return font;
    }
    static TextMeshProUGUI Label(Transform parent, string name, string text, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).sizeDelta = size;
        var label = go.GetComponent<TextMeshProUGUI>(); label.text = text;
        label.font = ProductionFont();
        label.color = new Color(.94f,.91f,.83f); label.fontSize = 24; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        return label;
    }
    static GameObject Root(string name, string sprite, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ((RectTransform)go.transform).sizeDelta = size;
        var image = go.GetComponent<Image>(); image.sprite = Sprite(sprite); image.type = Image.Type.Sliced;
        return go;
    }
    static void Save(GameObject go, string name)
    { PrefabUtility.SaveAsPrefabAsset(go, Dir + "M13_" + name + ".prefab"); Object.DestroyImmediate(go); }
    static void State(GameObject go, bool active)
    {
        var focus = Child(go.transform, "Focus", "focus", Vector2.zero);
        Stretch((RectTransform)focus.transform, Vector2.zero, Vector2.one, new Vector2(-3,-3), new Vector2(3,3));
        GameObject selected = null;
        if (active) { var mark = Child(go.transform, "Active", "selected", Vector2.zero); Stretch((RectTransform)mark.transform, new Vector2(0,0), new Vector2(1,0), new Vector2(10,0), new Vector2(-10,6)); selected = mark.gameObject; }
        go.AddComponent<M13SharedControlState>().Configure(focus.gameObject, selected);
    }
    static void Theme(GameObject go, Button button = null)
    {
        var binding = go.AddComponent<VNUIThemeBinding>(); var s = new SerializedObject(binding);
        s.FindProperty("theme").objectReferenceValue = AssetDatabase.LoadAssetAtPath<VNUITheme>("Assets/_Project/Settings/M11_UITheme.asset");
        if (button != null) s.FindProperty("button").objectReferenceValue = button;
        else s.FindProperty("panel").objectReferenceValue = go.GetComponent<Image>();
        s.ApplyModifiedPropertiesWithoutUndo();
    }
    [MenuItem("Tools/M13/Build shared panel and controls")]
    public static void Build()
    {
        MakeCard(); MakeScrollbar(); MakeSlider(); MakeToggle();
        Make("SharedPanel", "panel", false, new Vector2(800, 600));
        Make("SharedButton", "button", true, new Vector2(240, 56));
        Make("SharedTab", "tab", true, new Vector2(230, 56));
        AssetDatabase.SaveAssets();
    }
    static void MakeCard()
    {
        var go = Root("SharedCard", "card", new Vector2(480,140));
        var button = go.AddComponent<Button>(); button.targetGraphic = go.GetComponent<Image>();
        var thumb = Child(go.transform, "Thumbnail", "inset", new Vector2(112,112));
        var tr = (RectTransform)thumb.transform; tr.anchorMin = tr.anchorMax = new Vector2(0,.5f); tr.anchoredPosition = new Vector2(70,0); thumb.type = Image.Type.Simple; thumb.preserveAspect = true;
        var title = Label(go.transform, "Title", "제목", Vector2.zero); title.alignment = TextAlignmentOptions.MidlineLeft;
        Stretch((RectTransform)title.transform, new Vector2(0, .5f), Vector2.one, new Vector2(144,0), new Vector2(-18,-12));
        var meta = Label(go.transform, "Metadata", "기록 정보", Vector2.zero); meta.alignment = TextAlignmentOptions.TopLeft; meta.fontSize = 18;
        Stretch((RectTransform)meta.transform, Vector2.zero, new Vector2(1,.5f), new Vector2(144,12), new Vector2(-18,0));
        go.AddComponent<M13SharedCard>().Configure(thumb,title,meta,button); State(go,true); Theme(go,button); Save(go,"SharedCard");
    }
    static void MakeScrollbar()
    {
        var go = Root("SharedScrollbar", "track", new Vector2(28,300));
        var bg = go.GetComponent<Image>(); bg.color = new Color(1,1,1,0); // wide raycast gutter
        var track = Child(go.transform,"Track","track",Vector2.zero);
        Stretch((RectTransform)track.transform,new Vector2(.5f,0),new Vector2(.5f,1),new Vector2(-4,4),new Vector2(4,-4));
        var area = new GameObject("Sliding Area",typeof(RectTransform)); area.transform.SetParent(go.transform,false);
        Stretch((RectTransform)area.transform,Vector2.zero,Vector2.one,new Vector2(0,8),new Vector2(0,-8));
        var handle = Child(area.transform,"Handle Area","handle",Vector2.zero); handle.color = new Color(1,1,1,0);
        var hr = (RectTransform)handle.transform; Stretch(hr,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var thumb = Child(handle.transform,"Thumb","handle",Vector2.zero);
        Stretch((RectTransform)thumb.transform,new Vector2(.5f,0),new Vector2(.5f,1),new Vector2(-5,0),new Vector2(5,0));
        var bar = go.AddComponent<Scrollbar>(); bar.targetGraphic = thumb; bar.handleRect = hr; bar.direction = Scrollbar.Direction.BottomToTop; bar.size = .25f; bar.value = 1;
        Save(go,"SharedScrollbar");
    }
    static void MakeSlider()
    {
        var go = Root("SharedSlider","track",new Vector2(300,40)); var bg=go.GetComponent<Image>(); bg.color=new Color(1,1,1,0);
        var track=Child(go.transform,"Track","track",Vector2.zero); Stretch((RectTransform)track.transform,new Vector2(0,.5f),new Vector2(1,.5f),new Vector2(12,-5),new Vector2(-12,5));
        var fillArea=new GameObject("Fill Area",typeof(RectTransform)); fillArea.transform.SetParent(go.transform,false); Stretch((RectTransform)fillArea.transform,new Vector2(0,.5f),new Vector2(1,.5f),new Vector2(12,-5),new Vector2(-12,5));
        var fill=Child(fillArea.transform,"Fill","selected",Vector2.zero); Stretch((RectTransform)fill.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var handleArea=new GameObject("Handle Slide Area",typeof(RectTransform)); handleArea.transform.SetParent(go.transform,false); Stretch((RectTransform)handleArea.transform,Vector2.zero,Vector2.one,new Vector2(12,0),new Vector2(-12,0));
        var handle=Child(handleArea.transform,"Handle","handle",new Vector2(24,30));
        var slider=go.AddComponent<Slider>(); slider.targetGraphic=handle; slider.fillRect=(RectTransform)fill.transform; slider.handleRect=(RectTransform)handle.transform; slider.value=.5f;
        State(go,false); Save(go,"SharedSlider");
    }
    static void MakeToggle()
    {
        var go=Root("SharedToggle","button",new Vector2(240,48));
        var box=Child(go.transform,"Box","inset",new Vector2(30,30)); var br=(RectTransform)box.transform; br.anchorMin=br.anchorMax=new Vector2(0,.5f); br.anchoredPosition=new Vector2(24,0);
        var check=Child(box.transform,"Check","selected",new Vector2(20,20));
        var label=Label(go.transform,"Label","설정",Vector2.zero); label.alignment=TextAlignmentOptions.MidlineLeft; Stretch((RectTransform)label.transform,Vector2.zero,Vector2.one,new Vector2(48,4),new Vector2(-8,-4));
        var toggle=go.AddComponent<Toggle>(); toggle.targetGraphic=go.GetComponent<Image>(); toggle.graphic=check; State(go,false); Save(go,"SharedToggle");
    }
    static void Make(string name, string spriteName, bool button, Vector2 size)
    {
        var go = Root(name,spriteName,size); var image=go.GetComponent<Image>();
        if (button)
        {
            var control=go.AddComponent<Button>(); control.targetGraphic=image;
            var label=Label(go.transform,"Label",name=="SharedTab"?"탭":"확인",Vector2.zero); Stretch((RectTransform)label.transform,Vector2.zero,Vector2.one,new Vector2(10,4),new Vector2(-10,-4));
            State(go,true); Theme(go,control);
        }
        else
        {
            var header=Child(go.transform,"Header","secondary",Vector2.zero); Stretch((RectTransform)header.transform,new Vector2(0,1),Vector2.one,new Vector2(20,-100),new Vector2(-20,-20));
            var title=Label(header.transform,"Title","제목",Vector2.zero); Stretch((RectTransform)title.transform,Vector2.zero,Vector2.one,new Vector2(20,8),new Vector2(-20,-8));
            var content=Child(go.transform,"Content","secondary",Vector2.zero); Stretch((RectTransform)content.transform,Vector2.zero,Vector2.one,new Vector2(20,20),new Vector2(-20,-120)); Theme(go);
        }
        Save(go,name);
    }
}
