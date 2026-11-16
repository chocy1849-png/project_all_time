using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectAllTime.VN.Dialogue;

public class M13SharedControlTests
{
    static GameObject Load(string name) => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_Shared" + name + ".prefab");
    [TestCase("Panel"), TestCase("Button"), TestCase("Tab"), TestCase("Card"), TestCase("Scrollbar"), TestCase("Slider"), TestCase("Toggle")]
    public void Prefab_HasValidScriptsAndSprites(string name)
    {
        var prefab = Load(name); Assert.NotNull(prefab);
        foreach (var t in prefab.GetComponentsInChildren<Transform>(true)) Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject), t.name);
        foreach (var image in prefab.GetComponentsInChildren<Image>(true)) Assert.NotNull(image.sprite, image.name);
        foreach (var text in prefab.GetComponentsInChildren<TMP_Text>(true)) Assert.NotNull(text.font, text.name);
    }
    [Test]
    public void Tab_SelectionAndFocusRemainIndependent()
    {
        var go = Object.Instantiate(Load("Tab"));
        try {
            var state = go.GetComponent<M13SharedControlState>(); var focus = go.transform.Find("Focus").gameObject; var active = go.transform.Find("Active").gameObject;
            state.SetActive(true); state.OnSelect(null); Assert.IsTrue(focus.activeSelf); Assert.IsTrue(active.activeSelf);
            state.OnDeselect(null); Assert.IsFalse(focus.activeSelf); Assert.IsTrue(active.activeSelf);
            state.OnSelect(null); go.GetComponent<Button>().interactable = false; typeof(M13SharedControlState).GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(state, null); Assert.IsFalse(focus.activeSelf);
            Assert.NotNull(go.GetComponent<VNUIThemeBinding>().Theme);
        } finally { Object.DestroyImmediate(go); }
    }
    [Test]
    public void Card_ContentAndLockedStateFollowOwner()
    {
        var go=Object.Instantiate(Load("Card"));
        try {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/M13/B_card.png");
            var card=go.GetComponent<M13SharedCard>(); card.SetContent("긴 한글 제목", "기록 정보", sprite); card.SetLocked(true);
            Assert.AreEqual("긴 한글 제목",go.transform.Find("Title").GetComponent<TMP_Text>().text);
            Assert.AreEqual(sprite,go.transform.Find("Thumbnail").GetComponent<Image>().sprite); Assert.IsFalse(go.GetComponent<Button>().interactable);
            var events=new GameObject("Events",typeof(UnityEngine.EventSystems.EventSystem));
            try {
                bool clicked=false; var button=go.GetComponent<Button>(); button.onClick.AddListener(()=>clicked=true);
                var pointer=new UnityEngine.EventSystems.PointerEventData(events.GetComponent<UnityEngine.EventSystems.EventSystem>()); pointer.button=UnityEngine.EventSystems.PointerEventData.InputButton.Left;
                button.OnPointerClick(pointer); Assert.IsFalse(clicked);
                card.SetLocked(false); Assert.IsTrue(button.interactable); button.OnPointerClick(pointer); Assert.IsTrue(clicked);
            } finally { Object.DestroyImmediate(events); }
        } finally { Object.DestroyImmediate(go); }
    }
    [Test]
    public void SliderAndToggle_StandardValuesCanChange()
    {
        var slider=Object.Instantiate(Load("Slider")); var toggle=Object.Instantiate(Load("Toggle"));
        try { var s=slider.GetComponent<Slider>(); s.value=.75f; Assert.AreEqual(.75f,s.value); var t=toggle.GetComponent<Toggle>(); t.isOn=true; Assert.IsTrue(t.isOn); t.isOn=false; Assert.IsFalse(t.isOn); }
        finally { Object.DestroyImmediate(slider); Object.DestroyImmediate(toggle); }
    }
    [Test]
    public void Scrollbar_HasValidHandleAndAcceptsNormalizedValue()
    {
        var go=Object.Instantiate(Load("Scrollbar"));
        try { var bar=go.GetComponent<Scrollbar>(); Assert.NotNull(bar.handleRect); bar.value=.25f; Assert.AreEqual(.25f,bar.value); bar.value=1; Assert.AreEqual(1,bar.value); }
        finally { Object.DestroyImmediate(go); }
    }
    [Test]
    public void KoreanFont_SourceContainsRepresentativeHangul()
    {
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M13_Shared_Korean_SDF.asset");
        Assert.NotNull(font); Assert.AreEqual(AtlasPopulationMode.Dynamic,font.atlasPopulationMode); Assert.NotNull(font.sourceFontFile);
        foreach (var character in "긴한글제목기록정보") Assert.IsTrue(font.sourceFontFile.HasCharacter(character), character.ToString());
    }    [Test]
    public void StandardControls_RespondToProgrammaticPointerAndKeyboardEvents()
    {
        var canvas=new GameObject("Canvas",typeof(Canvas));
        var events=new GameObject("Events",typeof(UnityEngine.EventSystems.EventSystem));
        var button=Object.Instantiate(Load("Button"),canvas.transform);
        var toggle=Object.Instantiate(Load("Toggle"),canvas.transform);
        var slider=Object.Instantiate(Load("Slider"),canvas.transform);
        try {
            bool clicked=false; var b=button.GetComponent<Button>(); b.onClick.AddListener(()=>clicked=true);
            var pointer=new UnityEngine.EventSystems.PointerEventData(events.GetComponent<UnityEngine.EventSystems.EventSystem>()); pointer.button=UnityEngine.EventSystems.PointerEventData.InputButton.Left;
            b.OnPointerClick(pointer); Assert.IsTrue(clicked);
            var t=toggle.GetComponent<Toggle>(); t.isOn=false; t.OnSubmit(new UnityEngine.EventSystems.BaseEventData(events.GetComponent<UnityEngine.EventSystems.EventSystem>())); Assert.IsTrue(t.isOn);
            var v=slider.GetComponent<Slider>(); v.value=.5f;
            var axis=new UnityEngine.EventSystems.AxisEventData(events.GetComponent<UnityEngine.EventSystems.EventSystem>()); axis.moveDir=UnityEngine.EventSystems.MoveDirection.Right; v.OnMove(axis); Assert.Greater(v.value,.5f);
        } finally { Object.DestroyImmediate(canvas); Object.DestroyImmediate(events); }
    }    [Test]
    public void ScrollRect_WheelMovesContentAndSynchronizesScrollbar()
    {
        var canvas=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas));
        var root=new GameObject("Scroll",typeof(RectTransform),typeof(ScrollRect)); root.transform.SetParent(canvas.transform,false);
        var viewport=new GameObject("Viewport",typeof(RectTransform),typeof(RectMask2D)); viewport.transform.SetParent(root.transform,false);
        var content=new GameObject("Content",typeof(RectTransform)); content.transform.SetParent(viewport.transform,false);
        var events=new GameObject("Events",typeof(UnityEngine.EventSystems.EventSystem));
        var bar=Object.Instantiate(Load("Scrollbar"),root.transform).GetComponent<Scrollbar>();
        try {
            var rr=(RectTransform)root.transform; rr.sizeDelta=new Vector2(200,200);
            var vr=(RectTransform)viewport.transform; vr.sizeDelta=new Vector2(160,200);
            var cr=(RectTransform)content.transform; cr.anchorMin=cr.anchorMax=new Vector2(.5f,1); cr.pivot=new Vector2(.5f,1); cr.sizeDelta=new Vector2(160,600);
            var scroll=root.GetComponent<ScrollRect>(); scroll.viewport=vr; scroll.content=cr; scroll.horizontal=false; scroll.verticalScrollbar=bar; scroll.scrollSensitivity=40;
            Canvas.ForceUpdateCanvases(); scroll.verticalNormalizedPosition=1; Canvas.ForceUpdateCanvases();
            var ev=new UnityEngine.EventSystems.PointerEventData(events.GetComponent<UnityEngine.EventSystems.EventSystem>()); ev.scrollDelta=new Vector2(0,-1); scroll.OnScroll(ev);
            Assert.Greater(cr.anchoredPosition.y,0);
            typeof(ScrollRect).GetMethod("LateUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(scroll,null);
            Assert.Less(bar.value,1);
        } finally { Object.DestroyImmediate(canvas); Object.DestroyImmediate(events); }
    }    [Test]
    public void ScrollbarAndSlider_ProgrammaticDragChangesValues()
    {
        var canvas=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas)); canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var events=new GameObject("Events",typeof(UnityEngine.EventSystems.EventSystem));
        var bar=Object.Instantiate(Load("Scrollbar"),canvas.transform).GetComponent<Scrollbar>();
        var slider=Object.Instantiate(Load("Slider"),canvas.transform).GetComponent<Slider>();
        try {
            Canvas.ForceUpdateCanvases(); bar.value=.5f; slider.value=.5f;
            var ev=new UnityEngine.EventSystems.PointerEventData(events.GetComponent<UnityEngine.EventSystems.EventSystem>()); ev.button=UnityEngine.EventSystems.PointerEventData.InputButton.Left;
            ev.position=RectTransformUtility.WorldToScreenPoint(null,bar.handleRect.position); bar.OnBeginDrag(ev); ev.position+=new Vector2(0,60); bar.OnDrag(ev); Assert.AreNotEqual(.5f,bar.value);
            ev.position=RectTransformUtility.WorldToScreenPoint(null,slider.handleRect.position); slider.OnInitializePotentialDrag(ev); slider.OnPointerDown(ev); ev.position+=new Vector2(60,0); slider.OnDrag(ev); Assert.AreNotEqual(.5f,slider.value);
        } finally { Object.DestroyImmediate(canvas); Object.DestroyImmediate(events); }
    }}
