using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using UnityEngine.TextCore.LowLevel;

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
        Label("Label",i.transform,text,Vector2.zero,size- new Vector2(20,8),24,interactable ? new Color(.97f,.96f,.94f) : new Color(.72f,.75f,.76f),TextAlignmentOptions.Center); var ring=Img("Focus",i.transform,"focus",Vector2.zero,size+new Vector2(6,6)); ring.raycastTarget=false; i.gameObject.AddComponent<M13PreviewFocus>().Configure(ring.gameObject); return b;
    }
    const string PreviewCharacters = "시각 테스트기록 보관함지나온 순간들을 다시 살펴봅니다×저장갤러리업적삭제잠김첫 번째 이야기억의 조각화면 설정밝불오기옵뒤로사용할 수 없음표방향데이터선택한 긴 한국어 문장도 편안하게 읽을 수 있어야 합니다. 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz·:!?—“”()[]/%+-";
    static TMP_FontAsset PreviewFont() {
        const string path="Assets/_Project/Fonts/NotoSansKR/M13_Preview_Korean_SDF.asset";
        var f=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if(f != null) { if(!f.HasCharacters(PreviewCharacters, out var missing, false)) { f.atlasPopulationMode=AtlasPopulationMode.Dynamic; if(!f.TryAddCharacters(PreviewCharacters,out var extraMissing)) throw new System.Exception("Preview glyph generation failed: "+extraMissing); f.atlasPopulationMode=AtlasPopulationMode.Static; EditorUtility.SetDirty(f); EditorUtility.SetDirty(f.atlasTexture); AssetDatabase.SaveAssets(); } return f; }
        var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/NotoSansKR/NotoSansKR-Regular.ttf");
        f=TMP_FontAsset.CreateFontAsset(source,64,7,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,false);
        if(f==null) throw new System.Exception("Could not create M13 preview font");
        f.name="M13 Preview Korean SDF"; AssetDatabase.CreateAsset(f,path);
        f.material.name="M13 Preview Korean Material"; AssetDatabase.AddObjectToAsset(f.material,f);
        f.atlasTexture.name="M13 Preview Korean Atlas"; AssetDatabase.AddObjectToAsset(f.atlasTexture,f);
        if(!f.TryAddCharacters(PreviewCharacters,out var missingChars)) throw new System.Exception("Preview glyph generation failed: "+missingChars);
        f.atlasPopulationMode=AtlasPopulationMode.Static; EditorUtility.SetDirty(f); EditorUtility.SetDirty(f.atlasTexture); AssetDatabase.SaveAssets(); return f;
    }
    [MenuItem("Tools/M13/Build style previews")]
    public static void Build() {
        AssetDatabase.Refresh();
        foreach(var p in Directory.GetFiles(Art,"*.png")) { var path=p.Replace('\\','/'); var ti=(TextureImporter)AssetImporter.GetAtPath(path); ti.textureType=TextureImporterType.Sprite; ti.spriteImportMode=SpriteImportMode.Single; var settings=new TextureImporterSettings(); ti.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect; ti.SetTextureSettings(settings); int border=path.EndsWith("_grain.png") ? 0 : (path.EndsWith("_button.png") || path.EndsWith("_tab.png") || path.EndsWith("_selected.png") || path.EndsWith("_handle.png") ? 12 : 20); ti.spriteBorder=new Vector4(border,border,border,border); ti.mipmapEnabled=false; ti.filterMode=FilterMode.Bilinear; ti.wrapMode=TextureWrapMode.Clamp; ti.textureCompression=TextureImporterCompression.Uncompressed; ti.alphaIsTransparency=true; ti.SaveAndReimport(); }
        font=PreviewFont();
        Directory.CreateDirectory("Assets/_Project/Prefabs/UI/M13"); Directory.CreateDirectory("Assets/_Project/Scenes/M13");
        foreach(var d in new[]{"A","B"}) { dir=d; var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var canvas=new GameObject("M13_UI_StylePreview_"+d,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay; var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=.5f;
            var bg=Node("ProductionBackground",canvas.transform,Vector2.zero,new Vector2(1920,1080)).AddComponent<Image>(); bg.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_family_table_morning.png"); bg.preserveAspect=false;
            var shade=Node("BackgroundScrim",canvas.transform,Vector2.zero,new Vector2(1920,1080)).AddComponent<Image>(); shade.color=new Color(0,0,0,.30f);
            var panel=Img("MainModal",canvas.transform,"panel",Vector2.zero,new Vector2(1590,880));
            var grain=Img("SurfaceGrain",panel.transform,"grain",Vector2.zero,new Vector2(1550,840)); grain.type=Image.Type.Tiled; grain.color=new Color(1,1,1,.16f); grain.raycastTarget=false;
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
            var viewport=Node("Viewport",content.transform,new Vector2(-30,30),new Vector2(910,320)); viewport.AddComponent<RectMask2D>();
            var scrollContent=Node("ScrollableCards",viewport.transform,Vector2.zero,new Vector2(930,640)); var scrollRect=(RectTransform)scrollContent.transform; scrollRect.anchorMin=scrollRect.anchorMax=new Vector2(.5f,1f); scrollRect.pivot=new Vector2(.5f,1f); scrollRect.anchoredPosition=Vector2.zero;
            var list=content.gameObject.AddComponent<ScrollRect>(); list.viewport=(RectTransform)viewport.transform; list.content=scrollRect; list.horizontal=false; list.scrollSensitivity=240; list.movementType=ScrollRect.MovementType.Clamped; content.gameObject.AddComponent<M13PreviewScrollStart>();
            for(int j=0;j<6;j++){var card=Img("Card"+j,scrollContent.transform,"card",new Vector2((j%3-1)*300,162-(j/3)*320),new Vector2(270,305));
                var thumb=Img("Thumbnail",card.transform,"inset",new Vector2(0,55),new Vector2(254,138)); if(j==0) { var art=Node("StoryImage",thumb.transform,Vector2.zero,new Vector2(246,130)).AddComponent<Image>(); art.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_family_table_morning.png"); art.preserveAspect=true; art.raycastTarget=false; }
                Label("CardTitle",card.transform,j==0?"첫 번째 이야기":"기억의 조각 0"+(j+1),new Vector2(0,-48),new Vector2(250,36),23,ivory);
                Label("Metadata",card.transform,"2026. 11. 16   ·   12:30",new Vector2(0,-90),new Vector2(250,28),17,new Color(.87f,.89f,.90f));
                var cardButton=card.gameObject.AddComponent<Button>(); cardButton.targetGraphic=card; cardButton.interactable=j!=2;
                var marker=Img("SelectedMarker",card.transform,"selected",new Vector2(0,-148),new Vector2(230,6)); marker.type=Image.Type.Simple; marker.sprite=null; marker.color=new Color(.5f,.73f,.71f); marker.raycastTarget=false;
                card.gameObject.AddComponent<M13PreviewCard>().Configure(marker.gameObject,j==0);
                var cardFocus=Img("Focus",card.transform,"focus",Vector2.zero,new Vector2(276,311)); cardFocus.raycastTarget=false; card.gameObject.AddComponent<M13PreviewFocus>().Configure(cardFocus.gameObject);
                if(j==2) { card.color=new Color(.60f,.64f,.65f,1); Label("DisabledState",card.transform,"잠김",new Vector2(0,55),new Vector2(200,38),22,new Color(.75f,.78f,.78f),TextAlignmentOptions.Center); }
            }
            Label("Hint",content.transform,"선택한 기록을 불러옵니다. 긴 한국어 문장도 편안하게 읽을 수 있어야 합니다.",new Vector2(0,-180),new Vector2(900,45),20,new Color(.75f,.79f,.80f));
            var side=Img("SidePanel",panel.transform,"inset",new Vector2(540,-78),new Vector2(350,450));
            Label("SideTitle",side.transform,"화면 설정",new Vector2(0,168),new Vector2(280,38),26,ivory);
            Label("SliderLabel",side.transform,"밝기",new Vector2(0,112),new Vector2(280,30),20,muted);
            var track=Img("Slider",side.transform,"track",new Vector2(0,65),new Vector2(270,48)); track.type=Image.Type.Simple; track.sprite=null; track.color=Color.clear; var trackVisual=Img("TrackVisual",track.transform,"track",Vector2.zero,new Vector2(270,14)); trackVisual.type=Image.Type.Simple; trackVisual.sprite=null; trackVisual.color=new Color(.37f,.48f,.48f,1f); trackVisual.raycastTarget=false; var slider=track.gameObject.AddComponent<Slider>(); var handle=Img("Handle",track.transform,"handle",Vector2.zero,new Vector2(25,25)); slider.handleRect=handle.rectTransform; slider.targetGraphic=handle; slider.value=.65f;
            var scroll=Img("ScrollbarLane",content.transform,"track",new Vector2(460,30),new Vector2(36,288)); scroll.type=Image.Type.Simple; scroll.sprite=null; scroll.color=Color.clear; scroll.raycastTarget=true; var scrollTrackVisual=Img("TrackVisual",scroll.transform,"track",Vector2.zero,new Vector2(10,288)); scrollTrackVisual.type=Image.Type.Simple; scrollTrackVisual.sprite=null; scrollTrackVisual.color=new Color(.25f,.34f,.35f,.90f); scrollTrackVisual.raycastTarget=false; var hit=Img("HandleHitArea",scroll.transform,"track",Vector2.zero,new Vector2(0,0)); hit.sprite=null; hit.color=Color.clear; hit.raycastTarget=true; hit.rectTransform.anchorMin=Vector2.zero; hit.rectTransform.anchorMax=Vector2.one; var sh=Img("VisibleThumb",hit.transform,"handle",Vector2.zero,new Vector2(18,0)); sh.rectTransform.anchorMin=new Vector2(.5f,0); sh.rectTransform.anchorMax=new Vector2(.5f,1); sh.rectTransform.anchoredPosition=Vector2.zero; sh.rectTransform.sizeDelta=new Vector2(18,0); sh.color=Color.white; sh.raycastTarget=false; var sb=scroll.gameObject.AddComponent<Scrollbar>(); sb.handleRect=hit.rectTransform; sb.targetGraphic=sh; sb.direction=Scrollbar.Direction.BottomToTop; sb.size=.5f; list.verticalScrollbar=sb; list.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.Permanent; list.verticalNormalizedPosition=1;
            Btn("Primary",side.transform,"불러오기","selected",new Vector2(0,-42),new Vector2(240,56));
            Btn("Secondary",side.transform,"뒤로","button",new Vector2(0,-112),new Vector2(240,56));
            Btn("Disabled",side.transform,"사용할 수 없음","button",new Vector2(0,-182),new Vector2(240,48),false);
            var danger=Btn("Destructive",panel.transform,"삭제","button",new Vector2(200,-345),new Vector2(160,48)); danger.image.color=new Color(.58f,.34f,.34f,1);
            var toggleBg=Img("Toggle",panel.transform,"button",new Vector2(520,-345),new Vector2(32,32)); var toggle=toggleBg.gameObject.AddComponent<Toggle>(); var check=Img("Check",toggleBg.transform,"selected",Vector2.zero,new Vector2(18,18)); toggle.graphic=check; toggle.targetGraphic=toggleBg; toggle.isOn=true; var toggleLabel=Label("ToggleLabel",toggleBg.transform,"표시 설정",new Vector2(120,0),new Vector2(190,32),20,ivory); toggleLabel.raycastTarget=true;
            Label("Footer",panel.transform,"방향 " + d + "  ·  테스트 데이터",new Vector2(-550,-405),new Vector2(460,24),16,muted);
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var previewCamera=new GameObject("M13_PreviewCamera").AddComponent<Camera>(); previewCamera.clearFlags=CameraClearFlags.SolidColor; previewCamera.backgroundColor=Color.black; previewCamera.cullingMask=0; previewCamera.orthographic=true;
            PrefabUtility.SaveAsPrefabAsset(canvas,"Assets/_Project/Prefabs/UI/M13/M13_StylePreview_"+d+".prefab");
            EditorSceneManager.SaveScene(scene,"Assets/_Project/Scenes/M13/M13_StylePreview_"+d+".unity");
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
    }
}
