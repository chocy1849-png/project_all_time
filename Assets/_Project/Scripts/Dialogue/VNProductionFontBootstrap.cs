using TMPro;
using UnityEngine;

namespace ProjectAllTime.VN.Presentation
{
    /// <summary>Allows the shared dynamic production font to grow beyond its first atlas.</summary>
    public static class VNProductionFontBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansKR-Regular SDF");
            EnableGrowth(font);
        }
        public static void EnableGrowth(TMP_FontAsset font)
        {
            if (font != null && font.atlasPopulationMode == AtlasPopulationMode.Dynamic && font.sourceFontFile != null)
                font.isMultiAtlasTexturesEnabled = true;
        }
    }
}
