using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class M13PreviewTests
{
    [TestCase("A")]
    [TestCase("B")]
    public void Preview_UsesCompleteLocalFontAndRealControls(string direction)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_StylePreview_"+direction+".prefab");
        Assert.NotNull(prefab);
        foreach(var label in prefab.GetComponentsInChildren<TMP_Text>(true)) {
            Assert.NotNull(label.font, label.name);
            Assert.IsTrue(label.font.HasCharacters(label.text, out var missing, false), label.name+": "+(missing == null ? "" : string.Join(",",missing)));
            Assert.AreEqual("Assets/_Project/Fonts/NotoSansKR/M13_Preview_Korean_SDF.asset",AssetDatabase.GetAssetPath(label.font));
        }
        var scroll=prefab.GetComponentInChildren<ScrollRect>(true);
        Assert.NotNull(scroll); Assert.NotNull(scroll.viewport); Assert.NotNull(scroll.content); Assert.NotNull(scroll.verticalScrollbar);
        for(int row=0;row<2;row++) { var card=Find(scroll.content,"Card"+(row*3)) as RectTransform; var center=card.anchoredPosition.y; var half=card.rect.height/2; var contentHalf=scroll.content.rect.height/2; Assert.LessOrEqual(center+half,contentHalf); Assert.GreaterOrEqual(center-half,-contentHalf); Assert.LessOrEqual(card.rect.height,scroll.viewport.rect.height); }
        Assert.NotNull(prefab.GetComponentInChildren<Slider>(true)); Assert.NotNull(prefab.GetComponentInChildren<Toggle>(true));
        Assert.Greater(prefab.GetComponentsInChildren<M13PreviewFocus>(true).Length, 3);
    }
    [TestCase("A")]
    [TestCase("B")]
    public void ImportedSprites_HaveAppropriateBordersAndFiltering(string direction)
    {
        foreach(var role in new[]{"panel","secondary","card","inset","button","selected","tab","track","handle","focus","grain"}) {
            var path="Assets/_Project/Art/UI/M13/"+direction+"_"+role+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path); Assert.NotNull(importer,path);
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            Assert.AreEqual(TextureImporterType.Sprite,importer.textureType,path); Assert.AreEqual(SpriteMeshType.FullRect,settings.spriteMeshType,path);
            Assert.AreEqual(FilterMode.Bilinear,importer.filterMode,path); Assert.IsFalse(importer.mipmapEnabled,path);
            Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression,path);
            int border=role=="grain"?0:(role=="button"||role=="tab"||role=="selected"||role=="handle"?12:20);
            Assert.AreEqual(new Vector4(border,border,border,border),importer.spriteBorder,path);
        }
    }
    [Test]
    public void FocusAndControls_RespondToUiEvents()
    {
        var system=new GameObject("M13TestEventSystem",typeof(EventSystem));
        var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_StylePreview_A.prefab"));
        try {
            var focus=Find(root.transform,"TabNormal"); var ring=focus.Find("Focus");
            Assert.IsFalse(ring.gameObject.activeSelf); var responder=focus.GetComponent<M13PreviewFocus>();
            responder.OnSelect(new BaseEventData(system.GetComponent<EventSystem>())); Assert.IsTrue(ring.gameObject.activeSelf);
            responder.OnDeselect(new BaseEventData(system.GetComponent<EventSystem>())); Assert.IsFalse(ring.gameObject.activeSelf);
            var disabled=Find(root.transform,"Disabled"); var disabledRing=disabled.Find("Focus"); disabled.GetComponent<M13PreviewFocus>().OnPointerEnter(new PointerEventData(system.GetComponent<EventSystem>())); Assert.IsFalse(disabledRing.gameObject.activeSelf);
            var card0=Find(root.transform,"Card0"); var card1=Find(root.transform,"Card1"); var card2=Find(root.transform,"Card2");
            Assert.IsTrue(card0.Find("SelectedMarker").gameObject.activeSelf); typeof(M13PreviewCard).GetMethod("OnEnable",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(card1.GetComponent<M13PreviewCard>(),null); card1.GetComponent<Button>().onClick.Invoke(); Assert.IsFalse(card0.Find("SelectedMarker").gameObject.activeSelf); Assert.IsTrue(card1.Find("SelectedMarker").gameObject.activeSelf); Assert.IsFalse(card2.GetComponent<Button>().IsInteractable());
            var slider=Find(root.transform,"Slider").GetComponent<Slider>(); var start=slider.value;
            slider.OnMove(new AxisEventData(system.GetComponent<EventSystem>()){moveDir=MoveDirection.Right,moveVector=Vector2.right}); Assert.Greater(slider.value,start);
            var toggle=Find(root.transform,"Toggle").GetComponent<Toggle>(); var previous=toggle.isOn; toggle.OnSubmit(new BaseEventData(system.GetComponent<EventSystem>())); Assert.AreNotEqual(previous,toggle.isOn);
            var scroll=root.GetComponentInChildren<ScrollRect>(true); Canvas.ForceUpdateCanvases(); scroll.verticalNormalizedPosition=1; var before=scroll.content.anchoredPosition.y; scroll.verticalScrollbar.value=0; Assert.Greater(scroll.content.anchoredPosition.y,before);
        } finally {Object.DestroyImmediate(root); Object.DestroyImmediate(system);}
    }
    [TestCase("A")]
    [TestCase("B")]
    public void FeedbackLayout_UsesScreenScaledInsetScrollbarLane(string direction)
    {
        var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_StylePreview_"+direction+".prefab"));
        var system=new GameObject("EventSystem",typeof(EventSystem));
        try {
            var scroll=root.GetComponentInChildren<ScrollRect>(true); Canvas.ForceUpdateCanvases();
            var canvas=root.GetComponent<Canvas>(); var panel=(RectTransform)scroll.transform; var viewport=scroll.viewport;
            var lane=(RectTransform)scroll.verticalScrollbar.transform; var track=lane.Find("TrackVisual") as RectTransform;
            var hitArea=lane.Find("HandleHitArea") as RectTransform; var thumb=lane.Find("HandleHitArea/VisibleThumb") as RectTransform; var scale=canvas.scaleFactor;
            Assert.Greater(scale,0); Assert.AreEqual(scroll.transform,lane.parent);
            Assert.AreEqual(240,scroll.scrollSensitivity); Assert.AreEqual(320,viewport.rect.height);
            Assert.AreEqual(36,lane.rect.width); Assert.AreEqual(288,lane.rect.height);
            Assert.AreEqual(10,track.rect.width); Assert.AreEqual(288,track.rect.height);
            Assert.AreEqual(36,hitArea.rect.width); Assert.AreEqual(18,thumb.rect.width); Assert.AreEqual(hitArea,scroll.verticalScrollbar.handleRect);
            var vp=ScreenBounds(viewport); var hit=ScreenBounds(lane); var rail=ScreenBounds(track);
            var laneImage=lane.GetComponent<Image>(); var hitImage=hitArea.GetComponent<Image>();
            Assert.IsTrue(laneImage.raycastTarget,"Scrollbar lane must receive pointer input.");
            Assert.IsTrue(hitImage.raycastTarget,"Transparent handle area must remain draggable.");
            Assert.IsFalse(track.GetComponent<Image>().raycastTarget,"Visible track must not block pointer routing.");
            Assert.IsFalse(thumb.GetComponent<Image>().raycastTarget,"Narrow visible thumb must use the transparent hit area.");
            Assert.GreaterOrEqual(hit.xMin-vp.xMax,12*scale-1f,"Scrollbar hit lane must remain outside the card mask.");
            Assert.AreEqual(16*scale,vp.yMax-rail.yMax,1.5f*scale,"Track top inset must survive canvas scaling.");
            Assert.AreEqual(16*scale,rail.yMin-vp.yMin,1.5f*scale,"Track bottom inset must survive canvas scaling.");
            Assert.AreEqual(17*scale,(hit.xMin-vp.xMax),1.5f*scale,"Actual screen-space gutter must scale with the Canvas.");
            Assert.False(Overlaps(vp,hit),"Transparent hit area must not intercept card input.");
            Assert.AreEqual(910,viewport.rect.width);
            for(int j=0;j<6;j++) {
                var card=scroll.content.Find("Card"+j) as RectTransform;
                var focus=card.Find("Focus") as RectTransform;
                Assert.LessOrEqual(Mathf.Abs(card.anchoredPosition.x)+focus.rect.width/2,viewport.rect.width/2);
                foreach(var label in card.GetComponentsInChildren<TMP_Text>(true)) Assert.True(Contains(ScreenBounds(card),ScreenBounds((RectTransform)label.transform)),label.name+" should remain inside its card.");
            }
            scroll.verticalScrollbar.value=1; Canvas.ForceUpdateCanvases();
            var panelTop=ScreenBounds(panel); var panelSafeLeft=panelTop.xMin+20*scale; var panelSafeRight=panelTop.xMax-20*scale;
            var panelSafeBottom=panelTop.yMin+20*scale; var panelSafeTop=panelTop.yMax-20*scale;
            Assert.GreaterOrEqual(hit.xMin,panelSafeLeft-1,"Full pointer lane must remain inside the panel border-safe region.");
            Assert.LessOrEqual(hit.xMax,panelSafeRight+1,"Full pointer lane must remain inside the panel border-safe region.");
            Assert.GreaterOrEqual(hit.yMin,panelSafeBottom-1,"Full pointer lane must remain inside the panel border-safe region.");
            Assert.LessOrEqual(hit.yMax,panelSafeTop+1,"Full pointer lane must remain inside the panel border-safe region.");
            var topThumb=ScreenBounds(thumb);
            Assert.LessOrEqual(topThumb.xMax,panelSafeRight+1,"Visible thumb must stay inside the panel border-safe area.");
            Assert.LessOrEqual(topThumb.yMax,panelSafeTop+1,"Top thumb must stay clear of the panel border.");
            for(int j=0;j<3;j++) Assert.True(Contains(vp,ScreenBounds((RectTransform)scroll.content.Find("Card"+j+"/Focus"))),"Top-row focus outline must be visible.");
            scroll.verticalScrollbar.value=0; Canvas.ForceUpdateCanvases();
            var bottomThumb=ScreenBounds(thumb);
            Assert.GreaterOrEqual(bottomThumb.yMin,panelTop.yMin+20*scale-1,"Bottom thumb must stay clear of the panel border.");
            for(int j=3;j<6;j++) Assert.True(Contains(vp,ScreenBounds((RectTransform)scroll.content.Find("Card"+j+"/Focus"))),"Bottom-row focus outline must be visible.");
            var slider=root.GetComponentInChildren<Slider>(true); Assert.GreaterOrEqual(((RectTransform)slider.transform).rect.height,48);
            var toggle=root.GetComponentInChildren<Toggle>(true); Assert.IsTrue(toggle.transform.Find("ToggleLabel").GetComponent<TMP_Text>().raycastTarget);
            var locked=scroll.content.Find("Card2"); var was=scroll.content.Find("Card0/SelectedMarker").gameObject.activeSelf;
            locked.GetComponent<Button>().OnPointerClick(new PointerEventData(system.GetComponent<EventSystem>()) {button=PointerEventData.InputButton.Left});
            Assert.AreEqual(was,scroll.content.Find("Card0/SelectedMarker").gameObject.activeSelf);
        } finally { Object.DestroyImmediate(root); Object.DestroyImmediate(system); }
    }
    struct ScreenRect { public float xMin,xMax,yMin,yMax; }
    static ScreenRect ScreenBounds(RectTransform rect) {
        var corners=new Vector3[4]; rect.GetWorldCorners(corners);
        var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]); var c=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
        return new ScreenRect{xMin=a.x,xMax=c.x,yMin=a.y,yMax=c.y};
    }
    static bool Contains(ScreenRect outer,ScreenRect inner) => inner.xMin>=outer.xMin-1&&inner.xMax<=outer.xMax+1&&inner.yMin>=outer.yMin-1&&inner.yMax<=outer.yMax+1;
    static bool Overlaps(ScreenRect a,ScreenRect b) => a.xMin<b.xMax&&a.xMax>b.xMin&&a.yMin<b.yMax&&a.yMax>b.yMin;
    static Transform Find(Transform parent,string name) { foreach(var t in parent.GetComponentsInChildren<Transform>(true)) if(t.name==name) return t; Assert.Fail("Missing "+name); return null; }
}
