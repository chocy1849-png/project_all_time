using System;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>
    /// Plain schema-versioned settings data. This DTO intentionally contains no
    /// Unity object references, runtime application behavior, or product defaults.
    /// </summary>
    [Serializable]
    public sealed class VNSettingsData
    {
        public int schemaVersion;
        public string displayMode;
        public int windowedWidth;
        public int windowedHeight;
        public int textSpeedLps;
        public float autoSpeedNormalized;
        public float masterVolumeNormalized;
        public float bgmVolumeNormalized;
        public float sfxVolumeNormalized;
        public float voiceVolumeNormalized;
        public bool skipUnread;
        public bool screenShakeEnabled;
        public string inputBindingOverridesJson;
    }
}
