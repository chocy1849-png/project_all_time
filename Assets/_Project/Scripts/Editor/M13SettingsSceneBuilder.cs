using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectAllTime.VN.Settings;
using ProjectAllTime.VN.Dialogue;

/// <summary>Applies M13 styling to the existing Settings hierarchy, retaining runtime components and references.</summary>
public static class M13SettingsSceneBuilder
{
    const string Scene = "Assets/_Project/Scenes/VN_Main.unity";
    const string Prefabs = "Assets/_Project/Prefabs/UI/M13/M13_Shared";
    static GameObject Prefab(string name) => AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab");
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/B_" + name + ".png");
    static void Rect(RectTransform r, Vector2 a, Vector2 b, Vector2 lo, Vector2 hi) { r.anchorMin=a; r.anchorMax=b; r.offsetMin=lo; r.offsetMax=hi; }
    static void Top(RectTransform r,float top,float height,float left=0,float right=0) => Rect(r,new Vector2(0,1),Vector2.one,new Vector2(left,-top-height),new Vector2(-right,-top));
    static void ImageStyle(Image image, string sprite) { if (image == null) return; image.sprite=Sprite(sprite); image.type=Image.Type.Sliced; image.color=Color.white; }
    static void TextStyle(TMP_Text text, string value=null, float size=30)
    {
        if (text==null) return; text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M13_Shared_Korean_SDF.asset");
        text.color=new Color32(247,244,239,255); text.fontSize=size; text.raycastTarget=false; if(value!=null) text.text=value;
    }
    static GameObject CloneChild(GameObject prefab,string child,Transform parent)
    {
        var source=prefab.transform.Find(child); if(source==null) throw new Exception("Missing shared visual: "+child);
        var old=parent.Find("M13_"+child); if(old!=null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var go=UnityEngine.Object.Instantiate(source.gameObject,parent,false); go.name="M13_"+child; return go;
    }
    static void AddVisualState(GameObject go, string prefabName, bool selected)
    {
        var prefab=Prefab(prefabName); var focus=CloneChild(prefab,"Focus",go.transform);
        var active=selected?CloneChild(prefab,"Active",go.transform):null;
        var state=go.GetComponent<M13SharedControlState>() ?? go.AddComponent<M13SharedControlState>(); state.Configure(focus,active);
    }
    static void ButtonStyle(Button button, string prefabName="Button", bool selected=false)
    {
        ImageStyle(button.GetComponent<Image>(),prefabName=="Tab"?"tab":"button");
        button.targetGraphic=button.GetComponent<Image>(); AddVisualState(button.gameObject,prefabName,selected);
        var c=button.colors; c.normalColor=Color.white; c.highlightedColor=new Color(.9f,.96f,.94f); c.pressedColor=new Color(.55f,.75f,.72f); c.selectedColor=c.highlightedColor; c.disabledColor=new Color(.45f,.46f,.45f,.7f); button.colors=c;
    }
    static GameObject SharedVisual(string name, Transform parent)
    {
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(Prefab(name),parent);
        visual.name="M13_"+name+"_Visual";
        Rect((RectTransform)visual.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var selectable=visual.GetComponent<Selectable>(); if(selectable!=null) UnityEngine.Object.DestroyImmediate(selectable);
        var state=visual.GetComponent<M13SharedControlState>(); if(state!=null) UnityEngine.Object.DestroyImmediate(state);
        var image=visual.GetComponent<Image>(); if(image!=null) { image.raycastTarget=false; image.color=Prefab(name).GetComponent<Image>().color; }
        return visual;
    }
    static void SliderStyle(Slider slider)
    {
        // Preserve the Slider and VNSettingsSliderCommit on their original object; the nested prefab supplies visuals.
        if(slider.GetComponent<VNSettingsSliderCommit>()==null) throw new Exception("Slider missing commit: "+slider.name);
        while (slider.transform.childCount > 0) UnityEngine.Object.DestroyImmediate(slider.transform.GetChild(0).gameObject);
        var visual=SharedVisual("Slider",slider.transform);
        var image=slider.GetComponent<Image>() ?? slider.gameObject.AddComponent<Image>(); image.sprite=Prefab("Slider").GetComponent<Image>().sprite; image.type=Image.Type.Sliced; image.color=new Color(1,1,1,0); image.raycastTarget=true;
        slider.fillRect=(RectTransform)visual.transform.Find("Fill Area/Fill"); slider.handleRect=(RectTransform)visual.transform.Find("Handle Slide Area/Handle"); slider.targetGraphic=slider.handleRect.GetComponent<Image>();
        var state=slider.GetComponent<M13SharedControlState>() ?? slider.gameObject.AddComponent<M13SharedControlState>(); state.Configure(visual.transform.Find("Focus").gameObject,null);
    }
    static void ToggleStyle(Toggle toggle)
    {
        while (toggle.transform.childCount > 0) UnityEngine.Object.DestroyImmediate(toggle.transform.GetChild(0).gameObject);
        var visual=SharedVisual("Toggle",toggle.transform); visual.transform.Find("Label").gameObject.SetActive(false);
        var image=toggle.GetComponent<Image>() ?? toggle.gameObject.AddComponent<Image>(); image.color=new Color(1,1,1,0); image.raycastTarget=true;
        toggle.targetGraphic=visual.GetComponent<Image>(); toggle.graphic=visual.transform.Find("Box/Check").GetComponent<Image>();
        var state=toggle.GetComponent<M13SharedControlState>() ?? toggle.gameObject.AddComponent<M13SharedControlState>(); state.Configure(visual.transform.Find("Focus").gameObject,null);
    }
    static void DropdownStyle(TMP_Dropdown dropdown)
    {
        ImageStyle(dropdown.GetComponent<Image>(),"button"); dropdown.targetGraphic=dropdown.GetComponent<Image>();
        foreach(var image in dropdown.GetComponentsInChildren<Image>(true))
        {
            if(image.name=="Template") ImageStyle(image,"panel");
            else if(image.name=="Item Background") ImageStyle(image,"button");
            else if(image.name=="Item Checkmark") ImageStyle(image,"selected");
            else if(image.name=="Handle") ImageStyle(image,"handle");
        }
        var template=dropdown.template; template.sizeDelta=new Vector2(template.sizeDelta.x,240);
        var content=(RectTransform)template.Find("Viewport/Content"); content.sizeDelta=new Vector2(content.sizeDelta.x,54);
        var item=(RectTransform)content.Find("Item"); item.sizeDelta=new Vector2(item.sizeDelta.x,54);
        foreach(var text in dropdown.GetComponentsInChildren<TMP_Text>(true)) TextStyle(text,null,25);
    }
    static void Row(Transform row, float y, string label)
    {
        var layout=row.GetComponent<HorizontalLayoutGroup>(); if(layout!=null) layout.enabled=false;
        Top((RectTransform)row,y,70,28,28); var l=row.Find("Label"); if(l!=null) {Rect((RectTransform)l,new Vector2(0,0),new Vector2(.38f,1),new Vector2(0,0),new Vector2(-10,0)); TextStyle(l.GetComponent<TMP_Text>(),label,29); l.GetComponent<TMP_Text>().alignment=TextAlignmentOptions.MidlineLeft;}
        var slider=row.GetComponentInChildren<Slider>(true); if(slider!=null) { SliderStyle(slider); Rect((RectTransform)slider.transform,new Vector2(.39f,.15f),new Vector2(.85f,.85f),Vector2.zero,Vector2.zero); }
        var toggle=row.GetComponentsInChildren<Toggle>(true).FirstOrDefault(t => t.transform.parent == row); if(toggle!=null) { ToggleStyle(toggle); Rect((RectTransform)toggle.transform,new Vector2(.72f,.1f),new Vector2(.95f,.9f),Vector2.zero,Vector2.zero); }
        var drop=row.GetComponentInChildren<TMP_Dropdown>(true); if(drop!=null) { DropdownStyle(drop); Rect((RectTransform)drop.transform,new Vector2(.48f,.1f),new Vector2(.95f,.9f),Vector2.zero,Vector2.zero); }
        var value=row.Find("Value"); if(value!=null) {Rect((RectTransform)value,new Vector2(.86f,0),Vector2.one,Vector2.zero,Vector2.zero); TextStyle(value.GetComponent<TMP_Text>(),null,26);}
    }
    static void Description(Transform row, string text)
    {
        var label=row.Find("Label"); Rect((RectTransform)label,new Vector2(0,.45f),new Vector2(.46f,1),Vector2.zero,Vector2.zero);
        var d=row.Find("Description");
        if(d==null) { var go=new GameObject("Description",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI)); go.transform.SetParent(row,false); d=go.transform; }
        Rect((RectTransform)d,new Vector2(0,0),new Vector2(.47f,.46f),Vector2.zero,Vector2.zero);
        var textComponent=d.GetComponent<TMP_Text>(); TextStyle(textComponent,text,19); textComponent.color=new Color32(205,204,192,255); textComponent.alignment=TextAlignmentOptions.MidlineLeft;
    }
    static void Category(Transform area,string name,string title,string[] labels)
    {
        var content=area.Find(name); var layout=content.GetComponent<LayoutGroup>(); if(layout!=null) layout.enabled=false;
        Rect((RectTransform)content,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var titleText=content.Find("Title"); Top((RectTransform)titleText,20,55,28,28); TextStyle(titleText.GetComponent<TMP_Text>(),title,36); titleText.GetComponent<TMP_Text>().alignment=TextAlignmentOptions.MidlineLeft;
        int i=0; foreach(Transform row in content) if(row.name.EndsWith("Row",StringComparison.Ordinal) && i<labels.Length) {Row(row,110+i*95,labels[i]); i++;}
    }
    [MenuItem("Tools/M13/Apply Settings production styling")]
    public static void Apply()
    {
        var scene=EditorSceneManager.GetActiveScene(); if(scene.path!=Scene) throw new Exception("Open VN_Main before applying Settings styling.");
        var panel=UnityEngine.Object.FindFirstObjectByType<VNSettingsPanel>(FindObjectsInactive.Include); if(panel==null) throw new Exception("Settings panel missing"); if(!panel.TryValidateWiring(out var error)) throw new Exception("Settings wiring: "+error);
        var root=(RectTransform)panel.transform; Rect(root,new Vector2(.05f,.06f),new Vector2(.95f,.94f),Vector2.zero,Vector2.zero); ImageStyle(panel.GetComponent<Image>(),"panel");
        foreach(var text in panel.GetComponentsInChildren<TMP_Text>(true)) TextStyle(text);
        var header=panel.transform.Find("Header"); Top((RectTransform)header,24,70,40,40); TextStyle(header.Find("Title").GetComponent<TMP_Text>(),"설정",38); var close=header.Find("Close").GetComponent<Button>(); ButtonStyle(close); TextStyle(close.GetComponentInChildren<TMP_Text>(true),"닫기",25);
        var body=(RectTransform)panel.transform.Find("Body"); Rect(body,Vector2.zero,Vector2.one,new Vector2(40,100),new Vector2(-40,-115));
        var nav=body.Find("CategoryNav"); var navImage=nav.GetComponent<Image>() ?? nav.gameObject.AddComponent<Image>(); ImageStyle(navImage,"secondary"); navImage.color=new Color(1,1,1,.48f); navImage.raycastTarget=false; var navrect=(RectTransform)nav; Rect(navrect,Vector2.zero,new Vector2(0,1),Vector2.zero,new Vector2(245,0));
        var navLayout=nav.GetComponent<VerticalLayoutGroup>(); navLayout.childAlignment=TextAnchor.UpperLeft; navLayout.childControlWidth=true; navLayout.childControlHeight=true; navLayout.childForceExpandHeight=false; navLayout.spacing=12; navLayout.padding=new RectOffset(0,0,12,0);
        string[] navNames={"화면","텍스트","오디오","게임플레이","조작"}; for(int i=0;i<5;i++){var b=nav.GetChild(i).GetComponent<Button>(); ButtonStyle(b,"Tab",true); TextStyle(b.GetComponentInChildren<TMP_Text>(true),navNames[i],28); var le=b.GetComponent<LayoutElement>(); le.preferredHeight=64;}
        var area=body.Find("ContentArea"); var areaImage=area.GetComponent<Image>() ?? area.gameObject.AddComponent<Image>(); ImageStyle(areaImage,"secondary"); areaImage.color=new Color(1,1,1,.25f); areaImage.raycastTarget=false; Rect((RectTransform)area,Vector2.zero,Vector2.one,new Vector2(275,0),Vector2.zero);
        Category(area,"DisplayContent","화면",new[]{"화면 모드","해상도"}); Category(area,"TextContent","텍스트",new[]{"텍스트 속도","자동 진행 속도"});
        Category(area,"AudioContent","오디오",new[]{"전체 음량","배경 음악","효과음","음성"}); Category(area,"GameplayContent","게임플레이",new[]{"읽지 않은 대화 건너뛰기","화면 흔들림"});
        Description(area.Find("TextContent/TextSpeedRow"),"대화가 표시되는 속도를 조절합니다.");
        Description(area.Find("TextContent/AutoSpeedRow"),"자동으로 다음 대화에 넘어가는 속도입니다.");
        Description(area.Find("GameplayContent/SkipUnreadRow"),"읽지 않은 대화도 건너뜁니다.");
        Description(area.Find("GameplayContent/ScreenShakeRow"),"화면 흔들림 효과를 사용합니다.");
        var controls=area.Find("ControlsContent"); var vertical=controls.GetComponent<VerticalLayoutGroup>(); vertical.enabled=false; Rect((RectTransform)controls,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero); var title=controls.Find("Title"); Top((RectTransform)title,16,55,28,28); TextStyle(title.GetComponent<TMP_Text>(),"조작",36); title.GetComponent<TMP_Text>().alignment=TextAlignmentOptions.MidlineLeft;
        int n=0; foreach(Transform row in controls) if(row.name.EndsWith("Row",StringComparison.Ordinal)) {var group=row.GetComponent<HorizontalLayoutGroup>(); if(group!=null) group.enabled=false; Top((RectTransform)row,80+n*78,64,28,28); var action=row.Find("ActionLabel"); var binding=row.Find("BindingText"); var itemSerialized=new SerializedObject(row.GetComponent<VNRebindItem>()); itemSerialized.FindProperty("actionNameText").objectReferenceValue=action.GetComponent<TMP_Text>(); itemSerialized.ApplyModifiedPropertiesWithoutUndo(); Rect((RectTransform)action,Vector2.zero,new Vector2(.30f,1),Vector2.zero,Vector2.zero); Rect((RectTransform)binding,new Vector2(.31f,0),new Vector2(.59f,1),Vector2.zero,Vector2.zero); TextStyle(action.GetComponent<TMP_Text>(),null,24); TextStyle(binding.GetComponent<TMP_Text>(),null,22); var reb=row.Find("Rebehind").GetComponent<Button>(); var reset=row.Find("Reset").GetComponent<Button>(); Rect((RectTransform)reb.transform,new Vector2(.60f,.12f),new Vector2(.80f,.88f),Vector2.zero,new Vector2(-8,0)); Rect((RectTransform)reset.transform,new Vector2(.81f,.12f),new Vector2(1,.88f),Vector2.zero,Vector2.zero); ButtonStyle(reb); ButtonStyle(reset); TextStyle(reb.GetComponentInChildren<TMP_Text>(true),"변경",23); TextStyle(reset.GetComponentInChildren<TMP_Text>(true),"초기화",23); n++;}
        var all=controls.Find("ResetAllBindings").GetComponent<Button>(); Top((RectTransform)all.transform,560,52,28,28); ButtonStyle(all); TextStyle(all.GetComponentInChildren<TMP_Text>(true),"모든 키 설정 초기화",24);
        var status=(RectTransform)panel.transform.Find("StatusText"); Rect(status,Vector2.zero,new Vector2(1,0),new Vector2(45,25),new Vector2(-45,82)); TextStyle(status.GetComponent<TMP_Text>(),null,22);
        panel.ShowCategory(VNSettingsCategory.Display); if(!panel.TryValidateWiring(out error)) throw new Exception(error);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); Debug.Log("M13 Settings styling applied to production scene.");
    }
}
