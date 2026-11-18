using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class M13PretendardFontBuilder
{
    const string Dir="Assets/_Project/Fonts/Pretendard/";
    public static void Build()
    {
        foreach(var weight in new[]{"Regular","Medium","SemiBold"})
        {
            var path=Dir+"Pretendard-"+weight+" SDF.asset";
            if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path)!=null) continue;
            var source=AssetDatabase.LoadAssetAtPath<Font>(Dir+"Pretendard-"+weight+".otf");
            if(source==null) throw new System.Exception("Missing official Pretendard source: "+weight);
            var font=TMP_FontAsset.CreateFontAsset(source,48,5,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            typeof(TMP_FontAsset).GetProperty("clearDynamicDataOnBuild", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.SetValue(font, true);
            font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            font.fallbackFontAssetTable.Add(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M13_Shared_Korean_SDF.asset"));
            AssetDatabase.CreateAsset(font,path); AssetDatabase.AddObjectToAsset(font.material,font); AssetDatabase.AddObjectToAsset(font.atlasTexture,font);
        }
        AssetDatabase.SaveAssets();
    }
}
