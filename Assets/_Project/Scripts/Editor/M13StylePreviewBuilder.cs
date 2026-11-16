using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

// Isolated visual fixture; no production scene or runtime ownership is modified.
public static class M13StylePreviewBuilder
{
    const string Art = "Assets/_Project/Art/UI/M13/";
    static TMP_FontAsset font;
    static string dir;
    static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + dir + "_" + name + ".png");
    static GameObject Node(string name, Transform parent, Vector2 pos, Vector2 size) {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var r = (RectTransform)go.transform; r.anchorMin=r.anchorMax=new Vector2(.5f,.5f); r.sizeDelta=size; r.anchoredPosition=pos; return go;
    }
    static Image Img(string name, Transform parent, string sprite, Vector2 pos, Vector2 size) {
        var go=Node(name,parent,pos,size); var i=go.AddComponent<Image>(); i.sprite=S(sprite); i.type=Image.Type.Sliced; return i;
    }
    static TMP_Text Label(string name, Transform parent, string value, Vector2 pos, Vector2 size, float point, Color color, TextAlignmentOptions align=TextAlignmentOptions.Left) {
        var go=Node(name,parent,pos,size); var t=go.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=value; t.fontSize=point; t.color=color; t.alignment=align; t.raycastTarget=false; t.enableWordWrapping=true; return t;
    }
    static Button Btn(string name, Transform parent, string text, string sprite, Vector2 pos, Vector2 size, bool interactable=true) {
        var i=Img(name,parent,sprite,pos,size); var b=i.gameObject.AddComponent<Button>(); b.interactable=interactable;
        var colors=b.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(1.12f,1.12f,1.12f); colors.pressedColor=new Color(.78f,.84f,.83f); colors.selectedColor=new Color(1.12f,1.12f,1.12f); colors.disabledColor=new Color(.52f,.54f,.54f,.65f); b.colors=colors;
        Label("Label",i.transform,text,Vector2.zero,size- new Vector2(20,8),24,new Color(.97f,.96f,.94f),TextAlignmentOptions.Center); var ring=Img("Focus",i.transform,"focus",Vector2.zero,size+new Vector2(6,6)); ring.raycastTarget=false; i.gameObject.AddComponent<M13PreviewFocus>().Configure(ring.gameObject); return b;
    }
    [MenuItem("Tools/M13/Build style previews")]
    public static void Build() {
        AssetDatabase.Refresh();
        foreach(var p in Directory.GetFiles(Art,"*.png")) { var path=p.Replace('\\','/'); var ti=(TextureImporter)AssetImporter.GetAtPath(path); ti.textureType=TextureImporterType.Sprite; ti.spriteImportMode=SpriteImportMode.Single; var settings=new TextureImporterSettings(); ti.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect; ti.SetTextureSettings(settings); ti.spriteBorder=path.EndsWith("_grain.png") ? Vector4.zero : new Vector4(20,20,20,20); ti.mipmapEnabled=false; ti.filterMode=FilterMode.Bilinear; ti.wrapMode=TextureWrapMode.Clamp; ti.textureCompression=TextureImporterCompression.Uncompressed; ti.alphaIsTransparency=true; ti.SaveAndReimport(); }
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M12_Title_Korean_SDF.asset");
        Directory.CreateDirectory("Assets/_Project/Prefabs/UI/M13"); Directory.CreateDirectory("Assets/_Project/Scenes/M13");
        foreach(var d in new[]{"A","B"}) { dir=d; var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var canvas=new GameObject("M13_UI_StylePreview_"+d,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay; var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=.5f;
            var bg=Node("ProductionBackground",canvas.transform,Vector2.zero,new Vector2(1920,1080)).AddComponent<Image>(); bg.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_family_table_morning.png"); bg.preserveAspect=false;
            var shade=Node("BackgroundScrim",canvas.transform,Vector2.zero,new Vector2(1920,1080)).AddComponent<Image>(); shade.color=new Color(0,0,0,.30f);
            var panel=Img("MainModal",canvas.transform,"panel",Vector2.zero,new Vector2(1590,880));
            var grain=Img("SurfaceGrain",panel.transform,"grain",Vector2.zero,new Vector2(1550,840)); grain.type=Image.Type.Tiled; grain.raycastTarget=false;
            var ivory=new Color(.969f,.957f,.937f); var muted=new Color(.667f,.71f,.74f); var gold=new Color(.898f,.792f,.635f);
            Label("Eyebrow",panel.transform,"시각 테스트",new Vector2(-555,380),new Vector2(400,28),17,gold);
            Label("Title",panel.transform,"기록 보관함",new Vector2(-470,325),new Vector2(570,64),42,ivory);
            Label("Subtitle",panel.transform,"지나온 순간들을 다시 살펴봅니다",new Vector2(-470,275),new Vector2(570,34),22,muted);
            Btn("Close",panel.transform,"×","button",new Vector2(720,350),new Vector2(58,58));
            var tab1=Btn("TabSelected",panel.transform,"저장 기록","selected",new Vector2(-570,202),new Vector2(230,56));
            Img("SelectionLine",tab1.transform,"focus",new Vector2(0,-25),new Vector2(200,6));
            Btn("TabNormal",panel.transform,"갤러리","tab",new Vector2(-320,202),new Vector2(230,56));
            Btn("TabNormal2",panel.transform,"업적","tab",new Vector2(-70,202),new Vector2(230,56));
            var content=Img("Content",panel.transform,"secondary",new Vector2(-210,-78),new Vector2(1010,450));
            for(int j=0;j<3;j++){var card=Img("Card"+j,content.transform,"card",new Vector2((j-1)*316,30),new Vector2(292,305));
                var thumb=Img("Thumbnail",card.transform,"inset",new Vector2(0,55),new Vector2(254,138));
                Label("CardTitle",card.transform,j==0?"첫 번째 이야기":"기억의 조각 0"+(j+1),new Vector2(0,-48),new Vector2(250,36),23,ivory);
                Label("Metadata",card.transform,"2026. 11. 16   ·   12:30",new Vector2(0,-90),new Vector2(250,28),17,muted);
                if(j==0) Img("FocusRing",card.transform,"focus",Vector2.zero,new Vector2(294,307)).raycastTarget=false;
            }
            Label("Hint",content.transform,"선택한 기록을 불러옵니다. 긴 한국어 문장도 편안하게 읽을 수 있어야 합니다.",new Vector2(0,-180),new Vector2(900,45),20,muted);
            var side=Img("SidePanel",panel.transform,"inset",new Vector2(540,-78),new Vector2(350,450));
            Label("SideTitle",side.transform,"화면 설정",new Vector2(0,168),new Vector2(280,38),26,ivory);
            Label("SliderLabel",side.transform,"밝기",new Vector2(0,112),new Vector2(280,30),20,muted);
            var track=Img("Slider",side.transform,"track",new Vector2(0,65),new Vector2(270,14)); track.type=Image.Type.Simple; track.sprite=null; track.color=new Color(.37f,.48f,.48f,1f); var slider=track.gameObject.AddComponent<Slider>(); var handle=Img("Handle",track.transform,"handle",Vector2.zero,new Vector2(25,25)); slider.handleRect=handle.rectTransform; slider.targetGraphic=handle; slider.value=.65f;
            var scroll=Img("Scrollbar",side.transform,"track",new Vector2(151,-55),new Vector2(10,160)); scroll.type=Image.Type.Simple; scroll.sprite=null; scroll.color=new Color(.37f,.48f,.48f,1f); var sb=scroll.gameObject.AddComponent<Scrollbar>(); var sh=Img("Handle",scroll.transform,"handle",Vector2.zero,new Vector2(14,65)); sb.handleRect=sh.rectTransform; sb.targetGraphic=sh; sb.direction=Scrollbar.Direction.BottomToTop; sb.size=.35f;
            Btn("Primary",side.transform,"불러오기","selected",new Vector2(0,-42),new Vector2(240,56));
            Btn("Secondary",side.transform,"뒤로","button",new Vector2(0,-112),new Vector2(240,56));
            Btn("Disabled",side.transform,"사용할 수 없음","button",new Vector2(0,-182),new Vector2(240,48),false);
            var toggleBg=Img("Toggle",panel.transform,"button",new Vector2(520,-345),new Vector2(32,32)); var toggle=toggleBg.gameObject.AddComponent<Toggle>(); var check=Img("Check",toggleBg.transform,"selected",Vector2.zero,new Vector2(18,18)); toggle.graphic=check; toggle.targetGraphic=toggleBg; toggle.isOn=true; Label("ToggleLabel",panel.transform,"표시 설정",new Vector2(640,-345),new Vector2(190,32),20,ivory);
            Label("Footer",panel.transform,"방향 " + d + "  ·  테스트 데이터",new Vector2(-550,-405),new Vector2(460,24),16,muted);
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            PrefabUtility.SaveAsPrefabAsset(canvas,"Assets/_Project/Prefabs/UI/M13/M13_StylePreview_"+d+".prefab");
            EditorSceneManager.SaveScene(scene,"Assets/_Project/Scenes/M13/M13_StylePreview_"+d+".unity");
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
    }
}
