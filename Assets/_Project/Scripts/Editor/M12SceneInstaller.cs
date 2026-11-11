using System.IO;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records;
using ProjectAllTime.VN.SaveLoad;
using ProjectAllTime.VN.Settings;
using ProjectAllTime.VN.Title;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public static class M12SceneInstaller
{
    private const string Folder = "Assets/_Project/Prefabs/UI/M12";
    private static TMP_FontAsset font;
    private static VNUITheme theme;
    private static GameObject Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return go;
    }
    private static RectTransform Rect(GameObject go, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        var r = go.GetComponent<RectTransform>(); r.anchorMin=min; r.anchorMax=max; r.offsetMin=offsetMin; r.offsetMax=offsetMax; return r;
    }
    private static Image Panel(string name, Transform parent, Color color)
    {
        var go=Node(name,parent); var image=go.AddComponent<Image>(); image.color=color; return image;
    }
    private static TMP_Text Text(string name, Transform parent, string text, float size, Color color)
    {
        var go=Node(name,parent); var t=go.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=text; t.fontSize=size; t.color=color; t.raycastTarget=false; t.alignment=TextAlignmentOptions.MidlineLeft; return t;
    }
    private static Button Button(string name, Transform parent, string label)
    {
        var image=Panel(name,parent,Color.white); var b=image.gameObject.AddComponent<Button>();
        var colors=b.colors; colors.normalColor=new Color32(37,43,50,235); colors.highlightedColor=new Color32(65,91,92,255); colors.pressedColor=new Color32(103,139,135,255); colors.selectedColor=new Color32(65,103,100,255); colors.disabledColor=new Color32(27,32,37,160); b.colors=colors;
        var text=Text("Label",image.transform,label,30,theme.text); Rect(text.gameObject,Vector2.zero,Vector2.one,new Vector2(28,0),new Vector2(-20,0));
        return b;
    }
    private static void Set(SerializedObject so, string name, Object value) { var p=so.FindProperty(name); if(p==null) throw new System.Exception("Missing field "+name); p.objectReferenceValue=value; }
    public static string Install()
    {
        if (File.Exists(Folder+"/VNTitleScreen.prefab")) throw new System.Exception("M12 prefab already exists; refusing to overwrite.");
        Directory.CreateDirectory(Folder);
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M10_Prologue_Static_Korean.asset");
        theme=AssetDatabase.LoadAssetAtPath<VNUITheme>("Assets/_Project/Settings/M11_UITheme.asset");
        if(font==null || theme==null) throw new System.Exception("M11 font or theme missing.");
        var texture=new Texture2D(512,288,TextureFormat.RGB24,false);
        for(int y=0;y<288;y++) for(int x=0;x<512;x++) { float u=x/511f, v=y/287f; float glow=Mathf.Exp(-((u-.28f)*(u-.28f)*8+(v-.6f)*(v-.6f)*4)); texture.SetPixel(x,y,new Color(.045f+glow*.10f,.065f+glow*.10f,.085f+glow*.10f)); }
        texture.Apply(); var art=Folder+"/M12_ProvisionalBackground.png"; File.WriteAllBytes(art,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(art); var importer=(TextureImporter)AssetImporter.GetAtPath(art); importer.textureType=TextureImporterType.Sprite; importer.SaveAndReimport();
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(art);
        var root=Node("M12TitleUI",null); Rect(root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var title=Node("TitleLayer",root.transform); Rect(title,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        title.AddComponent<CanvasGroup>();
        var background=Node("Background",title.transform); Rect(background,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero); background.AddComponent<RectMask2D>();
        var bg=Panel("BackgroundImage",background.transform,Color.white); bg.sprite=sprite; bg.raycastTarget=false; Rect(bg.gameObject,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var fitter=bg.gameObject.AddComponent<AspectRatioFitter>(); fitter.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent; fitter.aspectRatio=16f/9f;
        bg.gameObject.AddComponent<VNTitleBackgroundCover>();
        var overlay=Panel("DarkOverlay",title.transform,new Color(0,0,0,.28f)); overlay.raycastTarget=true; Rect(overlay.gameObject,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var titleText=Text("KoreanTitle",title.transform,"우리가 만났던\n모든 시대",72,theme.text); titleText.fontStyle=FontStyles.Bold; Rect(titleText.gameObject,new Vector2(.10f,.40f),new Vector2(.60f,.68f),Vector2.zero,Vector2.zero);
        var caption=Text("Caption",title.transform,"ALL TIME",21,theme.speaker); Rect(caption.gameObject,new Vector2(.105f,.36f),new Vector2(.5f,.40f),Vector2.zero,Vector2.zero);
        var menu=Node("MainMenu",title.transform); Rect(menu,new Vector2(.72f,.20f),new Vector2(.92f,.80f),Vector2.zero,Vector2.zero);
        var layout=menu.AddComponent<VerticalLayoutGroup>(); layout.spacing=12; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandHeight=true;
        var n=Button("NewGameButton",menu.transform,"새 게임"); var c=Button("ContinueButton",menu.transform,"이어하기"); var l=Button("LoadGameButton",menu.transform,"불러오기"); var r=Button("RecordsButton",menu.transform,"기록"); var s=Button("SettingsButton",menu.transform,"설정"); var q=Button("QuitButton",menu.transform,"종료");
        var status=Text("Status",title.transform,"",22,theme.text); Rect(status.gameObject,new Vector2(.70f,.12f),new Vector2(.95f,.18f),Vector2.zero,Vector2.zero);
        var game=Panel("InGameMenu",root.transform,new Color(0,0,0,.80f)); Rect(game.gameObject,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var gm=Node("Actions",game.transform); Rect(gm,new Vector2(.37f,.29f),new Vector2(.63f,.71f),Vector2.zero,Vector2.zero); var gl=gm.AddComponent<VerticalLayoutGroup>(); gl.spacing=14; gl.childControlWidth=true; gl.childControlHeight=true; gl.childForceExpandHeight=true;
        var resume=Button("ResumeButton",gm.transform,"계속하기"); var back=Button("TitleButton",gm.transform,"타이틀로"); var gq=Button("QuitButton",gm.transform,"종료");
        var confirm=Panel("ConfirmationLayer",root.transform,new Color(0,0,0,.78f)); Rect(confirm.gameObject,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var box=Panel("Dialog",confirm.transform,new Color32(37,43,50,255)); Rect(box.gameObject,new Vector2(.30f,.35f),new Vector2(.70f,.65f),Vector2.zero,Vector2.zero);
        var message=Text("Message",box.transform,"",28,theme.text); message.alignment=TextAlignmentOptions.Center; message.enableWordWrapping=true; Rect(message.gameObject,new Vector2(.08f,.43f),new Vector2(.92f,.90f),Vector2.zero,Vector2.zero);
        var yes=Button("ConfirmButton",box.transform,"확인"); Rect(yes.gameObject,new Vector2(.10f,.10f),new Vector2(.46f,.35f),Vector2.zero,Vector2.zero);
        var no=Button("CancelButton",box.transform,"취소"); Rect(no.gameObject,new Vector2(.54f,.10f),new Vector2(.90f,.35f),Vector2.zero,Vector2.zero);
        game.gameObject.SetActive(false); confirm.gameObject.SetActive(false);
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/VNTitleScreen.prefab"); Object.DestroyImmediate(root);
        var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/VN_Main.unity",OpenSceneMode.Additive);
        try {
            var roots=scene.GetRootGameObjects(); GameObject Find(string name) { foreach(var x in roots) if(x.name==name) return x; throw new System.Exception("Missing "+name); }
            var canvas=Find("VNCanvas"); var dialogue=Find("Dialogue System"); var runtime=Find("VNConvenienceRuntime"); var save=Find("SaveLoadRuntime");
            var runner=dialogue.GetComponent<DialogueRunner>(); var runnerSo=new SerializedObject(runner); runnerSo.FindProperty("autoStart").boolValue=false; runnerSo.ApplyModifiedPropertiesWithoutUndo();
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene); instance.transform.SetParent(canvas.transform,false); instance.transform.SetSiblingIndex(canvas.transform.Find("ModalLayer").GetSiblingIndex());
            var flow=runtime.AddComponent<VNTitleFlowController>(); var so=new SerializedObject(flow);
            Set(so,"runner",runner); Set(so,"gate",runtime.GetComponent<VNInteractionGate>()); Set(so,"convenience",runtime.GetComponent<VNConvenienceController>()); Set(so,"router",runtime.GetComponent<VNConvenienceInputRouter>()); Set(so,"modals",runtime.GetComponent<VNConvenienceModalController>()); Set(so,"saves",save.GetComponent<VNSaveLoadController>()); Set(so,"settings",runtime.GetComponent<VNSettingsRuntimeBootstrap>()); Set(so,"meta",runtime.GetComponent<VNMetaProgressRuntimeBootstrap>()); Set(so,"records",runtime.GetComponent<VNRecordsRuntimeBootstrap>());
            Transform FindChild(string path) { var t=instance.transform.Find(path); if(t==null) throw new System.Exception("Missing UI "+path); return t; }
            Set(so,"titleLayer",FindChild("TitleLayer").gameObject); Set(so,"gameMenu",FindChild("InGameMenu").gameObject); Set(so,"confirmation",FindChild("ConfirmationLayer").gameObject); Set(so,"confirmationText",FindChild("ConfirmationLayer/Dialog/Message").GetComponent<TMP_Text>()); Set(so,"statusText",FindChild("TitleLayer/Status").GetComponent<TMP_Text>());
            string menuPath="TitleLayer/MainMenu/"; Set(so,"newGame",FindChild(menuPath+"NewGameButton").GetComponent<Button>()); Set(so,"continueGame",FindChild(menuPath+"ContinueButton").GetComponent<Button>()); Set(so,"loadGame",FindChild(menuPath+"LoadGameButton").GetComponent<Button>()); Set(so,"recordsButton",FindChild(menuPath+"RecordsButton").GetComponent<Button>()); Set(so,"settingsButton",FindChild(menuPath+"SettingsButton").GetComponent<Button>()); Set(so,"quitButton",FindChild(menuPath+"QuitButton").GetComponent<Button>());
            Set(so,"resumeButton",FindChild("InGameMenu/Actions/ResumeButton").GetComponent<Button>()); Set(so,"titleButton",FindChild("InGameMenu/Actions/TitleButton").GetComponent<Button>()); Set(so,"gameQuitButton",FindChild("InGameMenu/Actions/QuitButton").GetComponent<Button>()); Set(so,"confirmButton",FindChild("ConfirmationLayer/Dialog/ConfirmButton").GetComponent<Button>()); Set(so,"cancelButton",FindChild("ConfirmationLayer/Dialog/CancelButton").GetComponent<Button>());
            Set(so,"dialogueLayer",canvas.transform.Find("DialogueLayer").GetComponent<CanvasGroup>()); Set(so,"quickControls",canvas.transform.Find("QuickControlLayer").GetComponent<CanvasGroup>()); so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene); if(!EditorSceneManager.SaveScene(scene)) throw new System.Exception("Scene save failed"); AssetDatabase.SaveAssets();
        } finally { EditorSceneManager.CloseScene(scene,true); }
        return "M12 UI installed";
    }
}
