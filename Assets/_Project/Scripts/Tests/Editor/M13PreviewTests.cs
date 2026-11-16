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
    public void FeedbackLayout_BindsScrollbarAndKeepsCardsWithinViewport(string direction)
    {
        var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_StylePreview_"+direction+".prefab"));
        var system=new GameObject("EventSystem",typeof(EventSystem));
        try {
            var scroll=root.GetComponentInChildren<ScrollRect>(true); Canvas.ForceUpdateCanvases();
            Assert.AreEqual(scroll.transform,scroll.verticalScrollbar.transform.parent);
            Assert.AreEqual(240,scroll.scrollSensitivity); Assert.AreEqual(320,scroll.viewport.rect.height);
            var bar=(RectTransform)scroll.verticalScrollbar.transform;
            Assert.Greater(bar.anchoredPosition.x-bar.rect.width/2,scroll.viewport.anchoredPosition.x+scroll.viewport.rect.width/2);
            for(int j=0;j<6;j++) {
                var card=(RectTransform)scroll.content.Find("Card"+j);
                Assert.LessOrEqual(Mathf.Abs(card.anchoredPosition.x)+card.rect.width/2,scroll.viewport.rect.width/2);
                var focus=(RectTransform)card.Find("Focus");
                Assert.LessOrEqual(Mathf.Abs(card.anchoredPosition.x)+focus.rect.width/2,scroll.viewport.rect.width/2);
                Assert.LessOrEqual(Mathf.Abs(card.anchoredPosition.y)+focus.rect.height/2,scroll.content.rect.height/2);
            }
            var slider=root.GetComponentInChildren<Slider>(true); Assert.GreaterOrEqual(((RectTransform)slider.transform).rect.height,48);
            var toggle=root.GetComponentInChildren<Toggle>(true); Assert.IsTrue(toggle.transform.Find("ToggleLabel").GetComponent<TMP_Text>().raycastTarget);
            var locked=scroll.content.Find("Card2"); var was=scroll.content.Find("Card0/SelectedMarker").gameObject.activeSelf;
            locked.GetComponent<Button>().OnPointerClick(new PointerEventData(system.GetComponent<EventSystem>()) {button=PointerEventData.InputButton.Left});
            Assert.AreEqual(was,scroll.content.Find("Card0/SelectedMarker").gameObject.activeSelf);
        } finally { Object.DestroyImmediate(root); Object.DestroyImmediate(system); }
    }
    static Transform Find(Transform parent,string name) { foreach(var t in parent.GetComponentsInChildren<Transform>(true)) if(t.name==name) return t; Assert.Fail("Missing "+name); return null; }
}
