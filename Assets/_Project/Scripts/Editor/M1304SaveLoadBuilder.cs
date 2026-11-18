using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectAllTime.VN.SaveLoad;
using ProjectAllTime.VN.Dialogue;

public static class M1304SaveLoadBuilder
{
    const string Scene="Assets/_Project/Scenes/VN_Main.unity";
    const string Card="Assets/_Project/Prefabs/SaveLoad/VNSaveSlotItem.prefab";
    static Sprite Sprite(string n)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/B_"+n+".png");
    static TMP_FontAsset Font(string n)=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Pretendard/Pretendard-"+n+" SDF.asset");
    static void Rect(RectTransform r,Vector2 a,Vector2 b,Vector2 lo,Vector2 hi){r.anchorMin=a;r.anchorMax=b;r.offsetMin=lo;r.offsetMax=hi;}
    static void Top(RectTransform r,float top,float h,float left,float right)=>Rect(r,new Vector2(0,1),Vector2.one,new Vector2(left,-top-h),new Vector2(-right,-top));
    static void Style(TMP_Text t,float size,string weight="Regular",string value=null){if(t==null)return;t.font=Font(weight);t.fontSize=size;t.color=new Color32(236,234,230,255);t.raycastTarget=false;t.enableAutoSizing=false;if(value!=null)t.text=value;}
    static void ImageStyle(Image i,string sprite,Color color){i.sprite=Sprite(sprite);i.type=Image.Type.Sliced;i.color=color;}
    static TMP_Text Label(string name,Transform parent,string text,float size,string weight="Regular")
    {var old=parent.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);var t=go.GetComponent<TMP_Text>();Style(t,size,weight,text);return t;}
    static Button Button(string name,Transform parent,string text)
    {var old=parent.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_SharedButton.prefab");var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name=name;Style(go.GetComponentInChildren<TMP_Text>(true),25,"Medium",text);return go.GetComponent<Button>();}
    static void ExistingButton(Button b,string label)
    {ImageStyle(b.GetComponent<Image>(),"button",Color.white);b.targetGraphic=b.GetComponent<Image>();Style(b.GetComponentInChildren<TMP_Text>(true),24,"Medium",label);var old=b.transform.Find("M13_Focus");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_SharedButton.prefab");var focus=UnityEngine.Object.Instantiate(source.transform.Find("Focus").gameObject,b.transform,false);focus.name="M13_Focus";var state=b.GetComponent<M13SharedControlState>()??b.gameObject.AddComponent<M13SharedControlState>();state.Configure(focus,null);}
    static void NavigationState(Button button)
    {
        var old=button.transform.Find("M13_SelectedSurface");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var surface=new GameObject("M13_SelectedSurface",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        surface.transform.SetParent(button.transform,false);surface.transform.SetAsFirstSibling();
        Rect((RectTransform)surface.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        ImageStyle(surface.GetComponent<Image>(),"selected",Color.white);surface.GetComponent<Image>().raycastTarget=false;
        var state=button.GetComponent<M13SharedControlState>()??button.gameObject.AddComponent<M13SharedControlState>();
        state.ConfigureSurface(surface);
    }
    static void BuildCard()
    {
        var root=PrefabUtility.LoadPrefabContents(Card);
        try{
            var r=(RectTransform)root.transform;r.sizeDelta=new Vector2(800,185);ImageStyle(root.GetComponent<Image>(),"card",Color.white);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_SharedCard.prefab");
            foreach(var n in new[]{"Focus","Active"}){var old=root.transform.Find("M13_"+n);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var go=UnityEngine.Object.Instantiate(source.transform.Find(n).gameObject,root.transform,false);go.name="M13_"+n;}
            var state=root.GetComponent<M13SharedControlState>()??root.AddComponent<M13SharedControlState>();state.Configure(root.transform.Find("M13_Focus").gameObject,root.transform.Find("M13_Active").gameObject);
            var oldSurface=root.transform.Find("M13_SelectedSurface");if(oldSurface!=null)UnityEngine.Object.DestroyImmediate(oldSurface.gameObject);var surface=new GameObject("M13_SelectedSurface",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));surface.transform.SetParent(root.transform,false);surface.transform.SetAsFirstSibling();Rect((RectTransform)surface.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);ImageStyle(surface.GetComponent<Image>(),"selected",Color.white);surface.GetComponent<Image>().raycastTarget=false;state.ConfigureSurface(surface);
            var thumb=root.transform.Find("ThumbnailArea");Rect((RectTransform)thumb,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(18,-72),new Vector2(274,72));
            var ph=thumb.Find("ThumbnailPlaceholder").GetComponent<Image>();ImageStyle(ph,"inset",new Color32(90,103,102,255));ph.raycastTarget=false; Style(ph.GetComponentInChildren<TMP_Text>(true),18,"Regular","기록 없음");
            var image=thumb.Find("Thumbnail").GetComponent<RawImage>();image.raycastTarget=false;var fit=image.GetComponent<AspectRatioFitter>()??image.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=16f/9f;
            var slot=root.transform.Find("SlotLabel").GetComponent<TMP_Text>();Rect((RectTransform)slot.transform,new Vector2(0,1),Vector2.one,new Vector2(300,-45),new Vector2(-15,-10));Style(slot,22,"SemiBold");slot.color=new Color32(215,185,139,255);slot.alignment=TextAlignmentOptions.MidlineLeft;
            var meta=root.transform.Find("NormalMetadataRoot");Rect((RectTransform)meta,new Vector2(0,0),Vector2.one,new Vector2(300,12),new Vector2(-20,-48));
            string[] names={"SceneTitleText","ChapterText","SavedAtText","PlayedTimeText"};float[] tops={0,33,68,94};for(int i=0;i<names.Length;i++){var t=meta.Find(names[i]).GetComponent<TMP_Text>();Top((RectTransform)t.transform,tops[i],30,0,0);Style(t,i==0?25:18,i==0?"Medium":"Regular");t.alignment=TextAlignmentOptions.MidlineLeft;t.overflowMode=TextOverflowModes.Ellipsis;}
            foreach(var n in new[]{"EmptyStateRoot","CorruptedStateRoot","UnsupportedStateRoot"}){var t=root.transform.Find(n);Rect((RectTransform)t,new Vector2(0,0),Vector2.one,new Vector2(300,15),new Vector2(-20,-50));var label=t.GetComponentInChildren<TMP_Text>(true);Style(label,24,"Medium");label.alignment=TextAlignmentOptions.MidlineLeft;}
            var icons=(RectTransform)meta.Find("CharacterIcons");if(icons!=null)Rect(icons,new Vector2(.7f,0),Vector2.one,Vector2.zero,Vector2.zero);
            PrefabUtility.SaveAsPrefabAsset(root,Card);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    [MenuItem("Tools/M13/Apply Save Load redesign")]
    public static void Apply()
    {
        var scene=EditorSceneManager.GetActiveScene();if(scene.path!=Scene)throw new Exception("Open VN_Main first");BuildCard();
        var modal=UnityEngine.Object.FindFirstObjectByType<VNSaveLoadModal>(FindObjectsInactive.Include);if(modal==null)throw new Exception("SaveLoadModal missing");var root=modal.transform;var bg=root.GetComponent<Image>();bg.sprite=null;bg.color=new Color(0.02f,0.035f,0.04f,.97f);bg.raycastTarget=true;
        var main=root.Find("M13_Main");if(main==null){var go=new GameObject("M13_Main",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(CanvasGroup));go.transform.SetParent(root,false);main=go.transform;foreach(var n in new[]{"Header","CategoryTabs","SlotContainer","Footer"})root.Find(n).SetParent(main,false);}
        Rect((RectTransform)main,new Vector2(.045f,.045f),new Vector2(.955f,.955f),Vector2.zero,Vector2.zero);ImageStyle(main.GetComponent<Image>(),"panel",Color.white);main.GetComponent<Image>().raycastTarget=false;
        var header=main.Find("Header");Top((RectTransform)header,24,115,42,42);var title=header.Find("TitleText").GetComponent<TMP_Text>();Top((RectTransform)title.transform,25,54,0,500);Style(title,42,"SemiBold","기록 보관함");title.alignment=TextAlignmentOptions.MidlineLeft;
        var brand=Label("M13_Brand",header,"ALL TIME / INTERFACE SYSTEM",17,"SemiBold");Top((RectTransform)brand.transform,0,28,0,200);brand.color=new Color32(215,185,139,255);
        var description=Label("M13_Description",header,"이야기의 순간을 저장하고 다시 이어가세요.",20);Top((RectTransform)description.transform,82,26,0,500);description.color=new Color32(185,196,194,255);
        var close=header.Find("CloseButton").GetComponent<Button>();ExistingButton(close,"×");var cr=(RectTransform)close.transform;cr.pivot=new Vector2(.5f,.5f);cr.anchorMin=cr.anchorMax=new Vector2(1,.5f);cr.sizeDelta=new Vector2(70,50);cr.anchoredPosition=new Vector2(-35,0);
        var save=Button("M13_SaveMode",header,"저장");var load=Button("M13_LoadMode",header,"불러오기");NavigationState(save);NavigationState(load);var sr=(RectTransform)save.transform;sr.anchorMin=sr.anchorMax=new Vector2(1,.5f);sr.sizeDelta=new Vector2(150,52);sr.anchoredPosition=new Vector2(-300,0);var lr=(RectTransform)load.transform;lr.anchorMin=lr.anchorMax=new Vector2(1,.5f);lr.sizeDelta=new Vector2(150,52);lr.anchoredPosition=new Vector2(-145,0);
        var tabs=main.Find("CategoryTabs");Top((RectTransform)tabs,145,64,42,42);var hg=tabs.GetComponent<HorizontalLayoutGroup>();hg.childControlWidth=true;hg.childForceExpandWidth=true;hg.spacing=14;string[] tabnames={"수동 저장","자동 저장","퀵 저장"};for(int i=0;i<3;i++){var tab=tabs.GetChild(i).GetComponent<Button>();ExistingButton(tab,tabnames[i]);NavigationState(tab);}
        var grid=main.Find("SlotContainer");Rect((RectTransform)grid,Vector2.zero,Vector2.one,new Vector2(42,135),new Vector2(-42,-235));var layout=grid.GetComponent<GridLayoutGroup>();layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=2;layout.cellSize=new Vector2(805,185);layout.spacing=new Vector2(20,18);layout.childAlignment=TextAnchor.UpperCenter;
        var footer=main.Find("Footer");Rect((RectTransform)footer,Vector2.zero,new Vector2(1,0),new Vector2(42,32),new Vector2(-42,105));
        var prev=footer.Find("PrevPageButton").GetComponent<Button>();var next=footer.Find("NextPageButton").GetComponent<Button>();ExistingButton(prev,"이전");ExistingButton(next,"다음");var pr=(RectTransform)prev.transform;pr.anchorMin=pr.anchorMax=new Vector2(.62f,.5f);pr.sizeDelta=new Vector2(100,50);pr.anchoredPosition=new Vector2(-95,0);var nr=(RectTransform)next.transform;nr.anchorMin=nr.anchorMax=new Vector2(.62f,.5f);nr.sizeDelta=new Vector2(100,50);nr.anchoredPosition=new Vector2(95,0);
        var page=footer.Find("PageIndicatorText").GetComponent<TMP_Text>();var pgr=(RectTransform)page.transform;pgr.anchorMin=pgr.anchorMax=new Vector2(.62f,.5f);pgr.sizeDelta=new Vector2(100,50);pgr.anchoredPosition=Vector2.zero;Style(page,22,"Medium","01 / 02");page.alignment=TextAlignmentOptions.Center;page.enableWordWrapping=false;
        var summary=Label("M13_SelectedSummary",footer,"선택된 기록 없음",24,"Medium");Rect((RectTransform)summary.transform,new Vector2(0,0),new Vector2(.48f,1),Vector2.zero,Vector2.zero);
        var footerClose=Button("M13_FooterClose",footer,"닫기");var fcr=(RectTransform)footerClose.transform;fcr.anchorMin=fcr.anchorMax=new Vector2(1,.5f);fcr.pivot=new Vector2(1,.5f);fcr.sizeDelta=new Vector2(110,58);fcr.anchoredPosition=new Vector2(-285,0);
        var action=Button("M13_PrimaryAction",footer,"저장할 수 없음");var ar=(RectTransform)action.transform;ar.anchorMin=ar.anchorMax=new Vector2(1,.5f);ar.pivot=new Vector2(1,.5f);ar.sizeDelta=new Vector2(270,58);ar.anchoredPosition=Vector2.zero;
        var statusTransform=main.Find("StatusText")??main.Find("Footer/StatusText");statusTransform.SetParent(main,false);var status=statusTransform.GetComponent<TMP_Text>();Rect((RectTransform)status.transform,new Vector2(0,0),new Vector2(1,0),new Vector2(42,3),new Vector2(-42,32));Style(status,18);status.alignment=TextAlignmentOptions.MidlineLeft;
        var confirm=root.Find("OverwriteConfirmPanel");confirm.SetAsLastSibling();var shield=confirm.GetComponent<Image>();shield.sprite=null;shield.color=new Color(0,0,0,.8f);shield.raycastTarget=true;Rect((RectTransform)confirm,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var box=confirm.Find("ConfirmBox");var br=(RectTransform)box;br.anchorMin=br.anchorMax=new Vector2(.5f,.5f);br.sizeDelta=new Vector2(800,380);br.anchoredPosition=Vector2.zero;ImageStyle(box.GetComponent<Image>(),"panel",Color.white);
        var message=box.Find("MessageText").GetComponent<TMP_Text>();Rect((RectTransform)message.transform,new Vector2(0,.38f),Vector2.one,new Vector2(45,0),new Vector2(-45,-30));Style(message,30,"Medium");message.alignment=TextAlignmentOptions.Center;
        var yes=box.Find("ConfirmButton").GetComponent<Button>();var no=box.Find("CancelButton").GetComponent<Button>();ExistingButton(yes,"덮어쓰기");ExistingButton(no,"취소");Rect((RectTransform)no.transform,new Vector2(.08f,.1f),new Vector2(.48f,.3f),Vector2.zero,Vector2.zero);Rect((RectTransform)yes.transform,new Vector2(.52f,.1f),new Vector2(.92f,.3f),Vector2.zero,Vector2.zero);
        var so=new SerializedObject(modal);so.FindProperty("footerCloseButton").objectReferenceValue=footerClose;so.FindProperty("primaryActionButton").objectReferenceValue=action;so.FindProperty("primaryActionText").objectReferenceValue=action.GetComponentInChildren<TMP_Text>(true);so.FindProperty("selectedSummaryText").objectReferenceValue=summary;so.FindProperty("saveModeButton").objectReferenceValue=save;so.FindProperty("loadModeButton").objectReferenceValue=load;so.FindProperty("mainContentGroup").objectReferenceValue=main.GetComponent<CanvasGroup>();so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    }
}
