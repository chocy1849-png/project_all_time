using UnityEditor;

public static class M13SharedDemoBuilder
{
    [MenuItem("Tools/M13/Open shared component demo" )]
    public static void Open()
    {
if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
var cameraObject = new UnityEngine.GameObject("Demo Camera", typeof(UnityEngine.Camera));
var camera = cameraObject.GetComponent<UnityEngine.Camera>(); camera.clearFlags = UnityEngine.CameraClearFlags.SolidColor; camera.backgroundColor = new UnityEngine.Color(.07f,.07f,.065f); camera.orthographic = true; camera.orthographicSize = 5;
var canvasObject = new UnityEngine.GameObject("M13 Component Demo", typeof(UnityEngine.Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
var canvas = canvasObject.GetComponent<UnityEngine.Canvas>(); canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance=1;
var scaler=canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new UnityEngine.Vector2(1280,720);
System.Func<string,float,float,UnityEngine.GameObject> add = (name,x,y) => { var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/_Project/Prefabs/UI/M13/M13_Shared"+name+".prefab"); var go=(UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab,canvasObject.transform); ((UnityEngine.RectTransform)go.transform).anchoredPosition=new UnityEngine.Vector2(x,y); return go; };
var panel=add("Panel",0,0); ((UnityEngine.RectTransform)panel.transform).sizeDelta=new UnityEngine.Vector2(1160,660); panel.transform.SetAsFirstSibling();
var card=add("Card",-260,100); card.GetComponent<ProjectAllTime.VN.Dialogue.M13SharedCard>().SetContent("기록 제목", "긴 한국어 메타데이터와 날짜", UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>("Assets/_Project/Art/UI/M13/B_card.png"));
var selected=add("Card",-260,-70); selected.GetComponent<ProjectAllTime.VN.Dialogue.M13SharedControlState>().SetActive(true); selected.GetComponent<ProjectAllTime.VN.Dialogue.M13SharedControlState>().OnSelect(null);
var locked=add("Card",-260,-220); locked.GetComponent<ProjectAllTime.VN.Dialogue.M13SharedCard>().SetLocked(true);
add("Button",270,130); var tab=add("Tab",270,50); tab.GetComponent<ProjectAllTime.VN.Dialogue.M13SharedControlState>().SetActive(true); tab.GetComponent<ProjectAllTime.VN.Dialogue.M13SharedControlState>().OnSelect(null);
add("Slider",270,-40); add("Toggle",270,-120);
var scrollRoot=new UnityEngine.GameObject("Scrollable cards",typeof(UnityEngine.RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.ScrollRect)); scrollRoot.transform.SetParent(canvasObject.transform,false);
var sr=(UnityEngine.RectTransform)scrollRoot.transform; sr.sizeDelta=new UnityEngine.Vector2(500,460); sr.anchoredPosition=new UnityEngine.Vector2(-260,-45); scrollRoot.GetComponent<UnityEngine.UI.Image>().color=new UnityEngine.Color(1,1,1,0);
var viewport=new UnityEngine.GameObject("Viewport",typeof(UnityEngine.RectTransform),typeof(UnityEngine.UI.RectMask2D)); viewport.transform.SetParent(scrollRoot.transform,false);
var vr=(UnityEngine.RectTransform)viewport.transform; vr.anchorMin=UnityEngine.Vector2.zero; vr.anchorMax=UnityEngine.Vector2.one; vr.offsetMin=new UnityEngine.Vector2(0,0); vr.offsetMax=new UnityEngine.Vector2(-30,0);
var content=new UnityEngine.GameObject("Content",typeof(UnityEngine.RectTransform)); content.transform.SetParent(viewport.transform,false); var cr=(UnityEngine.RectTransform)content.transform; cr.anchorMin=cr.anchorMax=new UnityEngine.Vector2(.5f,1); cr.pivot=new UnityEngine.Vector2(.5f,1); cr.sizeDelta=new UnityEngine.Vector2(470,600);
var items=new[]{card,selected,locked}; for(int i=0;i<items.Length;i++){items[i].transform.SetParent(content.transform,false); var r=(UnityEngine.RectTransform)items[i].transform; r.anchorMin=r.anchorMax=new UnityEngine.Vector2(.5f,1); r.anchoredPosition=new UnityEngine.Vector2(0,-75-i*165); r.sizeDelta=new UnityEngine.Vector2(450,140);}
var bar=add("Scrollbar",-20,-45).GetComponent<UnityEngine.UI.Scrollbar>(); ((UnityEngine.RectTransform)bar.transform).sizeDelta=new UnityEngine.Vector2(28,440);
var scroll=scrollRoot.GetComponent<UnityEngine.UI.ScrollRect>(); scroll.viewport=vr; scroll.content=cr; scroll.horizontal=false; scroll.verticalScrollbar=bar; scroll.scrollSensitivity=40; scroll.verticalNormalizedPosition=1;
new UnityEngine.GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.EventSystems.StandaloneInputModule));
UnityEngine.Canvas.ForceUpdateCanvases();


    }
}
