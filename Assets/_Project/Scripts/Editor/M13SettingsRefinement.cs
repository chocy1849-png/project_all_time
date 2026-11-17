using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Settings;

/// <summary>Reference-led refinement of the existing production hierarchy; rebuilding is idempotent.</summary>
public static class M13SettingsRefinement
{
    const string FontDir="Assets/_Project/Fonts/Pretendard/";
    static readonly Color32 Text=new Color32(238,234,227,255), Sub=new Color32(184,194,192,255), Teal=new Color32(128,185,177,255), Gold=new Color32(215,185,139,255);
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/B_"+name+".png");
    static TMP_FontAsset Font(string weight)=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontDir+"Pretendard-"+weight+" SDF.asset");
    static void Box(RectTransform r,Vector2 a,Vector2 b,Vector2 lo,Vector2 hi){r.anchorMin=a;r.anchorMax=b;r.offsetMin=lo;r.offsetMax=hi;}
    static void Top(RectTransform r,float top,float height,float left=0,float right=0)=>Box(r,new Vector2(0,1),Vector2.one,new Vector2(left,-top-height),new Vector2(-right,-top));
    static GameObject New(string name,Transform parent,params Type[] components){var old=parent.Find(name);if(old!=null) UnityEngine.Object.DestroyImmediate(old.gameObject);var go=new GameObject(name,components);go.transform.SetParent(parent,false);return go;}
    static Image Panel(string name,Transform parent,Color color)
    {var go=New(name,parent,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));var i=go.GetComponent<Image>();i.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/M13_SwitchPill.png");i.type=Image.Type.Sliced;i.color=color;i.raycastTarget=false;return i;}
    static TMP_Text Label(string name,Transform parent,string value,float size,Color color,string weight="Regular")
    {var go=New(name,parent,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));var t=go.GetComponent<TMP_Text>();t.font=Font(weight);t.fontSize=size;t.color=color;t.text=value;t.raycastTarget=false;t.alignment=TextAlignmentOptions.MidlineLeft;return t;}
    static void Style(TMP_Text t,string weight,float size,Color color){if(t==null)return;t.font=Font(weight);t.fontSize=size;t.color=color;t.fontStyle=FontStyles.Normal;t.raycastTarget=false;}
    static void RestoreControls(Transform controls)
    {
        var bindings=controls.Find("M13_Bindings");
        if(bindings!=null){foreach(var row in bindings.GetComponentsInChildren<VNRebindItem>(true).Select(x=>x.transform).ToArray())row.SetParent(controls,false);var reset=bindings.Find("ResetAllBindings");if(reset!=null)reset.SetParent(controls,false);UnityEngine.Object.DestroyImmediate(bindings.gameObject);}
        foreach(var n in new[]{"M13_Guide","M13_BindTab","M13_GuideTab","M13_Subline","M13_ControlCaption"}){var t=controls.Find(n);if(t!=null)UnityEngine.Object.DestroyImmediate(t.gameObject);}
    }
    static void EnsureRoundSprites()
    {
        MakeSprite("M13_SwitchPill",64,32,true); MakeSprite("M13_SwitchThumb",32,32,false);
    }
    static void MakeSprite(string name,int width,int height,bool pill)
    {
        string path="Assets/_Project/Art/UI/M13/"+name+".png";if(File.Exists(path))return;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);float radius=height*.5f-1;
        for(int y=0;y<height;y++)for(int x=0;x<width;x++){
            float cx=pill?Mathf.Clamp(x,radius,width-1-radius):(width-1)*.5f;float cy=(height-1)*.5f;
            float distance=Vector2.Distance(new Vector2(x,y),new Vector2(cx,cy));float alpha=Mathf.Clamp01(radius+.5f-distance);
            texture.SetPixel(x,y,new Color(1,1,1,alpha));}
        texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;if(pill)importer.spriteBorder=new Vector4(15,15,15,15);importer.SaveAndReimport();
    }
    static void Switch(Toggle toggle)
    {
        var parent=toggle.transform;var old=parent.Find("M13_SwitchTrack");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);old=parent.Find("M13_SwitchLabel");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var visual=parent.Find("M13_Toggle_Visual"); if(visual!=null) visual.gameObject.SetActive(false);
        var previousFocus=parent.Find("M13_SwitchFocus");if(previousFocus!=null)UnityEngine.Object.DestroyImmediate(previousFocus.gameObject);
        var focusSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_SharedToggle.prefab").transform.Find("Focus");
        var focus=UnityEngine.Object.Instantiate(focusSource.gameObject,parent,false);focus.name="M13_SwitchFocus";
        var focusState=toggle.GetComponent<M13SharedControlState>();if(focusState!=null)focusState.Configure(focus,null);
        var trackGo=New("M13_SwitchTrack",parent,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));var track=trackGo.GetComponent<Image>();track.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/M13_SwitchPill.png");track.type=Image.Type.Sliced;track.raycastTarget=false;
        var tr=(RectTransform)trackGo.transform;tr.anchorMin=tr.anchorMax=new Vector2(1,.5f);tr.pivot=new Vector2(1,.5f);tr.anchoredPosition=new Vector2(-8,0);tr.sizeDelta=new Vector2(92,50);
        var thumbGo=New("Thumb",trackGo.transform,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));var thumb=thumbGo.GetComponent<Image>();thumb.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/M13_SwitchThumb.png");thumb.raycastTarget=false;var th=(RectTransform)thumb.transform;th.sizeDelta=new Vector2(36,36);
        var label=Label("M13_SwitchLabel",parent,"꺼짐",22,Sub);Box((RectTransform)label.transform,Vector2.zero,new Vector2(1,1),new Vector2(0,0),new Vector2(-108,0));label.alignment=TextAlignmentOptions.MidlineRight;
        toggle.graphic=null;toggle.targetGraphic=toggle.GetComponent<Image>();toggle.transition=Selectable.Transition.None;var state=toggle.GetComponent<VNSettingsSwitchVisual>()??toggle.gameObject.AddComponent<VNSettingsSwitchVisual>();state.Configure(toggle,track,th,label);
    }
    static Button SubTab(string name,Transform parent,string title,float x)
    {
        var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_SharedTab.prefab");var old=parent.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(p,parent);go.name=name;var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-105);r.sizeDelta=new Vector2(185,50);var t=go.GetComponentInChildren<TMP_Text>(true);t.text=title;Style(t,"Medium",24,Text);return go.GetComponent<Button>();
    }
    static void Nav(Transform nav)
    {
        var layout=nav.GetComponent<VerticalLayoutGroup>();if(layout!=null)layout.enabled=false;
        var section=Label("M13_NavCaption",nav,"PREFERENCES",18,Sub,"SemiBold");Top((RectTransform)section.transform,24,32,20,10);
        for(int i=0;i<5;i++){
            var button=nav.GetChild(i).GetComponent<Button>();Top((RectTransform)button.transform,70+i*76,64,12,12);
            var image=button.GetComponent<Image>();image.color=new Color(0,0,0,0);var colors=button.colors;colors.normalColor=new Color(0,0,0,0);colors.highlightedColor=new Color(.22f,.29f,.29f,.75f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color(.25f,.37f,.35f,.9f);button.colors=colors;
            var surface=Panel("M13_SelectedSurface",button.transform,new Color32(54,85,82,235));Box((RectTransform)surface.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);surface.transform.SetAsFirstSibling();
            var active=(RectTransform)button.transform.Find("M13_Active");if(active!=null)Box(active,Vector2.zero,new Vector2(0,1),new Vector2(0,8),new Vector2(5,-8));
            var state=button.GetComponent<M13SharedControlState>();state.ConfigureSurface(surface.gameObject);
            var label=button.GetComponentInChildren<TMP_Text>(true);Style(label,"Medium",27,Text);label.alignment=TextAlignmentOptions.MidlineLeft;Box((RectTransform)label.transform,Vector2.zero,Vector2.one,new Vector2(24,0),new Vector2(-12,0));
        }
    }
    static void Category(Transform area,string name,string description,string section,string[] hints)
    {
        var content=area.Find(name);var title=content.Find("Title").GetComponent<TMP_Text>();Top((RectTransform)title.transform,20,52,35,28);Style(title,"SemiBold",36,Text);
        var subtitle=Label("M13_Subtitle",content,description,21,Sub);Top((RectTransform)subtitle.transform,74,32,35,28);
        var rows=content.Cast<Transform>().Where(x=>x.name.EndsWith("Row",StringComparison.Ordinal)).ToArray();float rowHeight=name=="AudioContent"?88:105;
        var bg=Panel("M13_Group",content,new Color32(36,45,50,235));Top((RectTransform)bg.transform,125,65+rows.Length*rowHeight,20,20);bg.transform.SetAsFirstSibling();
        var groupTitle=Label("M13_GroupTitle",content,section,24,Text,"SemiBold");Top((RectTransform)groupTitle.transform,140,40,48,48);
        for(int i=0;i<rows.Length;i++){
            var row=rows[i];Top((RectTransform)row,190+i*rowHeight,rowHeight-3,48,48);
            var label=row.Find("Label").GetComponent<TMP_Text>();Box((RectTransform)label.transform,new Vector2(0,.45f),new Vector2(.53f,1),Vector2.zero,Vector2.zero);Style(label,"Medium",26,Text);label.alignment=TextAlignmentOptions.MidlineLeft;
            var desc=row.Find("Description");if(desc==null)desc=Label("Description",row,"",20,Sub).transform;Box((RectTransform)desc,new Vector2(0,0),new Vector2(.53f,.48f),Vector2.zero,Vector2.zero);var text=desc.GetComponent<TMP_Text>();text.text=i<hints.Length?hints[i]:"";Style(text,"Regular",20,Sub);text.alignment=TextAlignmentOptions.MidlineLeft;
            var slider=row.GetComponentInChildren<Slider>(true);if(slider!=null)Box((RectTransform)slider.transform,new Vector2(.60f,.24f),new Vector2(.91f,.76f),Vector2.zero,Vector2.zero);
            var dropdown=row.GetComponentInChildren<TMP_Dropdown>(true);if(dropdown!=null)Box((RectTransform)dropdown.transform,new Vector2(.61f,.18f),new Vector2(.98f,.82f),Vector2.zero,Vector2.zero);
            var toggle=row.GetComponentsInChildren<Toggle>(true).FirstOrDefault(x=>x.transform.parent==row);if(toggle!=null){Box((RectTransform)toggle.transform,new Vector2(.77f,.13f),new Vector2(.98f,.87f),Vector2.zero,Vector2.zero);Switch(toggle);}
            var value=row.Find("Value");if(value!=null){Box((RectTransform)value,new Vector2(.92f,0),Vector2.one,Vector2.zero,Vector2.zero);Style(value.GetComponent<TMP_Text>(),"SemiBold",22,Text);value.GetComponent<TMP_Text>().alignment=TextAlignmentOptions.MidlineRight;}
            if(i<rows.Length-1){var line=Panel("M13_Separator",row,new Color32(82,96,97,105));Box((RectTransform)line.transform,new Vector2(0,0),new Vector2(1,0),Vector2.zero,new Vector2(0,2));}
        }
    }
    static void Controls(Transform controls)
    {
        var title=controls.Find("Title").GetComponent<TMP_Text>();Top((RectTransform)title.transform,20,50,35,28);Style(title,"SemiBold",36,Text);
        var subtitle=Label("M13_Subtitle",controls,"자주 쓰는 조작을 변경하거나 현재 키를 확인합니다.",21,Sub);Top((RectTransform)subtitle.transform,72,32,35,28);
        var bindTab=SubTab("M13_BindTab",controls,"키 설정",35);var guideTab=SubTab("M13_GuideTab",controls,"조작 가이드",225);
        var line=Panel("M13_Subline",controls,new Color32(82,96,97,130));Top((RectTransform)line.transform,160,2,35,35);
        var rows=controls.GetComponentsInChildren<VNRebindItem>(true).OrderBy(x=>(int)x.Target).ToArray();var reset=controls.Find("ResetAllBindings");
        var bindings=New("M13_Bindings",controls,typeof(RectTransform));Box((RectTransform)bindings.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var caption=Label("M13_ControlCaption",bindings.transform,"변경할 수 있는 단축키 · 6개",20,Gold,"SemiBold");Top((RectTransform)caption.transform,175,32,35,35);
        for(int i=0;i<rows.Length;i++){
            var row=rows[i].transform;row.SetParent(bindings.transform,false);int col=i%2, r=i/2;
            Box((RectTransform)row,new Vector2(col*.5f,1),new Vector2((col+1)*.5f,1),new Vector2(col==0?35:8,-(220+r*128)-112),new Vector2(col==0?-8:-35,-(220+r*128)));
            var bg=row.GetComponent<Image>()??row.gameObject.AddComponent<Image>();bg.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/M13_SwitchPill.png");bg.type=Image.Type.Sliced;bg.color=new Color32(34,48,54,255);bg.raycastTarget=false;
            var action=row.Find("ActionLabel").GetComponent<TMP_Text>();Box((RectTransform)action.transform,new Vector2(0,.52f),new Vector2(.55f,1),new Vector2(20,0),Vector2.zero);Style(action,"Medium",23,Text);action.alignment=TextAlignmentOptions.MidlineLeft;
            var binding=row.Find("BindingText").GetComponent<TMP_Text>();Box((RectTransform)binding.transform,new Vector2(0,0),new Vector2(.56f,.52f),new Vector2(28,5),Vector2.zero);Style(binding,"Medium",20,Sub);binding.alignment=TextAlignmentOptions.MidlineLeft;
            var keycap=Panel("M13_Keycap",row,new Color32(52,64,68,255));Box((RectTransform)keycap.transform,new Vector2(0,0),new Vector2(.54f,.5f),new Vector2(16,4),new Vector2(-8,-2));keycap.transform.SetAsFirstSibling();
            var reb=(RectTransform)row.Find("Rebehind");var resetOne=(RectTransform)row.Find("Reset");Box(reb,new Vector2(.59f,.21f),new Vector2(.79f,.79f),Vector2.zero,new Vector2(-5,0));Box(resetOne,new Vector2(.80f,.21f),new Vector2(.98f,.79f),Vector2.zero,Vector2.zero);
            Style(reb.GetComponentInChildren<TMP_Text>(true),"Medium",20,Text);Style(resetOne.GetComponentInChildren<TMP_Text>(true),"Medium",19,Text);
        }
        reset.SetParent(bindings.transform,false);var rr=(RectTransform)reset;rr.anchorMin=rr.anchorMax=new Vector2(1,1);rr.pivot=new Vector2(1,1);rr.anchoredPosition=new Vector2(-35,-620);rr.sizeDelta=new Vector2(270,50);
        var tip=Label("M13_BindTip",bindings.transform,"변경 버튼을 누르면 새로운 키를 지정할 수 있습니다.",19,Sub);Top((RectTransform)tip.transform,620,48,35,320);var resetImage=reset.GetComponent<Image>();resetImage.color=new Color32(135,82,76,255);Style(reset.GetComponentInChildren<TMP_Text>(true),"Medium",22,new Color32(233,181,173,255));
        var ownerSerialized=new SerializedObject(controls.GetComponent<VNControlsSettingsView>()); ownerSerialized.FindProperty("guidanceText").objectReferenceValue=tip; ownerSerialized.ApplyModifiedPropertiesWithoutUndo();
        var guide=New("M13_Guide",controls,typeof(RectTransform));Box((RectTransform)guide.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var guideBg=Panel("M13_GuidePanel",guide.transform,new Color32(36,45,50,235));Top((RectTransform)guideBg.transform,185,400,35,35);
        var heading=Label("M13_GuideHeading",guide.transform,"대화와 플레이",24,Text,"SemiBold");Top((RectTransform)heading.transform,200,45,60,60);
        var labels=new TMP_Text[6];string[] actions={"대화 진행","자동 진행","건너뛰기","대화창 숨기기","빠른 저장","빠른 불러오기"};
        for(int i=0;i<6;i++){
            int col=i%3,row=i/3;var card=Panel("M13_GuideCard"+i,guide.transform,new Color32(34,48,52,255));Box((RectTransform)card.transform,new Vector2(col/3f,1),new Vector2((col+1)/3f,1),new Vector2(50+col*2,-(270+row*115)-95),new Vector2(-50+(2-col)*2,-(270+row*115)));
            var key=Label("Key",card.transform,"—",20,Text,"Medium");Box((RectTransform)key.transform,new Vector2(0,0),new Vector2(.46f,1),new Vector2(15,0),Vector2.zero);var action=Label("Action",card.transform,actions[i],20,Sub);Box((RectTransform)action.transform,new Vector2(.46f,0),Vector2.one,Vector2.zero,new Vector2(-8,0));labels[i]=key;
        }
        var helper=controls.GetComponent<VNControlsGuide>()??controls.gameObject.AddComponent<VNControlsGuide>();helper.Configure(controls.GetComponent<VNControlsSettingsView>(),bindTab,guideTab,bindings,guide,labels);helper.ShowBindings();
    }
    public static void Apply()
    {
        var panel=UnityEngine.Object.FindFirstObjectByType<VNSettingsPanel>(FindObjectsInactive.Include);if(panel==null)throw new Exception("Open VN_Main first");
        var area=panel.transform.Find("Body/ContentArea");RestoreControls(area.Find("ControlsContent"));
        M13PretendardFontBuilder.Build();EnsureRoundSprites();M13SettingsSceneBuilder.ApplyBase();
        foreach(var text in panel.GetComponentsInChildren<TMP_Text>(true))Style(text,"Regular",text.fontSize,Text);
        var header=panel.transform.Find("Header");var brand=Label("M13_Brand",header,"ALL TIME / INTERFACE SYSTEM",17,Gold,"SemiBold");Top((RectTransform)brand.transform,0,28,0,120);
        var headerTitle=header.Find("Title").GetComponent<TMP_Text>();headerTitle.text="환경 설정";Style(headerTitle,"SemiBold",38,Text);Box((RectTransform)headerTitle.transform,Vector2.zero,Vector2.one,new Vector2(0,-10),new Vector2(-110,-28));
        var close=header.Find("Close").GetComponentInChildren<TMP_Text>(true);close.text="×";Style(close,"Medium",31,Text);
        Nav(panel.transform.Find("Body/CategoryNav"));
        Category(area,"DisplayContent","디스플레이 모드와 해상도를 조정합니다.","디스플레이",new[]{"전체 화면 창 또는 창 모드를 선택합니다.","창 모드에서 해상도를 변경할 수 있습니다."});
        Category(area,"TextContent","대화가 표시되는 속도를 원하는 리듬으로 맞춥니다.","진행 속도",new[]{"문장이 한 글자씩 나타나는 속도","다음 대화로 넘어가기 전의 여유"});
        Category(area,"AudioContent","음량을 세부적으로 조절해 장면의 분위기를 맞춥니다.","음량 조절",new[]{"모든 소리의 기준 음량","장면의 음악과 분위기","인터랙션 및 장면 효과","캐릭터 대사 및 음성 재생"});
        Category(area,"GameplayContent","플레이 방식에 영향을 주는 옵션을 관리합니다.","대화와 연출",new[]{"처음 보는 대화도 건너뛸 수 있습니다.","장면 연출의 화면 흔들림 효과를 사용합니다."});
        Controls(area.Find("ControlsContent")); foreach(var text in panel.GetComponentsInChildren<TMP_Text>(true)) if(text.font==null || !text.font.name.StartsWith("Pretendard-",StringComparison.Ordinal)) text.font=Font("Regular"); panel.ShowCategory(VNSettingsCategory.Display);if(!panel.TryValidateWiring(out var error))throw new Exception(error);
        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);EditorSceneManager.SaveScene(panel.gameObject.scene);AssetDatabase.SaveAssets();
    }
}
