using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.SaveLoad;

public class M1303FontIntegrityTests
{
    const string Noto="Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Regular SDF.asset";
    const string Corpus="Empty slot\nCorrupted save\nUnsupported save\nSave Load Page 1 / 2 Manual / Auto / Quick\n빈 저장 슬롯 수동 저장 자동 저장 빠른 저장 불러오기 덮어쓰기 저장 기록 설정 대화 기록 기록 보관함 건너뛰기 이어하기 환경 설정 조작 가이드\nm2_technical M2_Technical_Checkpoint S001 S003 2026-11-17 15:30 1920 × 1080 Ctrl / Right Control Shift + Enter Esc\n0123456789 ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz\n가나다라마바사아자차카타파하 ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎ";
    [Test] public void SavePrefab_UsesProductionDynamicNotoAndSourceCanGrow()
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/SaveLoad/VNSaveSlotItem.prefab"); Assert.NotNull(prefab);
        var item=prefab.GetComponent<VNSaveSlotItem>(); var text=(TMP_Text)new SerializedObject(item).FindProperty("emptyStateText").objectReferenceValue;
        var asset=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Noto); Assert.AreSame(asset,text.font); Assert.NotNull(asset.sourceFontFile); Assert.AreEqual(AtlasPopulationMode.Dynamic,asset.atlasPopulationMode); Assert.AreSame(asset.atlasTexture,asset.material.mainTexture);
        foreach(char c in Corpus.Where(x=>!char.IsWhiteSpace(x)).Distinct()) Assert.IsTrue(asset.sourceFontFile.HasCharacter(c),"source missing "+c);
    }
    [TestCase("NotoSansKR-Regular.ttf",false)]
    [TestCase("Pretendard-Regular.otf",true)]
    [TestCase("Pretendard-Medium.otf",true)]
    [TestCase("Pretendard-SemiBold.otf",true)]
    public void FreshDynamicFont_RendersCorpusWithoutReplacementAndCanGrow(string sourceName,bool pretendard)
    {
        var path=pretendard?"Assets/_Project/Fonts/Pretendard/"+sourceName:"Assets/_Project/Fonts/NotoSansKR/"+sourceName;
        var source=AssetDatabase.LoadAssetAtPath<Font>(path); Assert.NotNull(source);
        var font=TMP_FontAsset.CreateFontAsset(source,48,5,GlyphRenderMode.SDFAA,256,256,AtlasPopulationMode.Dynamic,false); Assert.NotNull(font);
        var root=new GameObject("Font test",typeof(RectTransform),typeof(Canvas)); var go=new GameObject("Text",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI)); go.transform.SetParent(root.transform,false);
        try {
            VNProductionFontBootstrap.EnableGrowth(font); Assert.IsTrue(font.isMultiAtlasTexturesEnabled);
            Assert.IsTrue(font.TryAddCharacters(Corpus,out string missing),"missing: "+missing); Assert.Greater(font.atlasTextureCount,1);
            var label=go.GetComponent<TextMeshProUGUI>(); label.font=font; label.fontSize=24; label.text=Corpus; ((RectTransform)go.transform).sizeDelta=new Vector2(2000,2000);
            label.ForceMeshUpdate(true,true); var info=label.textInfo;
            Assert.Greater(info.characterCount,0); for(int i=0;i<info.characterCount;i++) Assert.AreNotEqual('\u25A1',info.characterInfo[i].character,"replacement at "+i);
            Assert.NotNull(label.fontSharedMaterial); Assert.NotNull(label.fontSharedMaterial.mainTexture);
        } finally { Object.DestroyImmediate(root); Object.DestroyImmediate(font.material); foreach(var atlas in font.atlasTextures) if(atlas!=null) Object.DestroyImmediate(atlas); Object.DestroyImmediate(font); }
    }
    [Test] public void Fallback_ResolvesKoreanThroughActualTmpText()
    {
        var latin=AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
        var korean=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/NotoSansKR/NotoSansKR-Regular.ttf");
        var primary=TMP_FontAsset.CreateFontAsset(latin,48,5,GlyphRenderMode.SDFAA,256,256,AtlasPopulationMode.Dynamic,true);
        var fallback=TMP_FontAsset.CreateFontAsset(korean,48,5,GlyphRenderMode.SDFAA,256,256,AtlasPopulationMode.Dynamic,true);
        primary.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>{fallback};
        var root=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas)); var go=new GameObject("Text",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));go.transform.SetParent(root.transform,false);
        try { var t=go.GetComponent<TextMeshProUGUI>(); t.font=primary;t.text="Save 설정";((RectTransform)go.transform).sizeDelta=new Vector2(600,100);t.ForceMeshUpdate(true,true);
            var hangul=t.textInfo.characterInfo.Take(t.textInfo.characterCount).Where(x=>x.character=='설'||x.character=='정').ToArray(); Assert.AreEqual(2,hangul.Length);Assert.IsTrue(hangul.All(x=>x.fontAsset==fallback)); }
        finally {Object.DestroyImmediate(root); Object.DestroyImmediate(primary.material); foreach(var a in primary.atlasTextures) if(a!=null) Object.DestroyImmediate(a);Object.DestroyImmediate(primary);Object.DestroyImmediate(fallback.material);foreach(var a in fallback.atlasTextures) if(a!=null) Object.DestroyImmediate(a);Object.DestroyImmediate(fallback);}
    }
}
