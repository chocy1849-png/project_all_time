namespace ProjectAllTime.VN.Settings
{
    public static class VNSettingsDefaults
    {
        public const int CurrentSchemaVersion = 1;
        public const string FullScreenWindowDisplayMode = "full_screen_window";
        public const string WindowedDisplayMode = "windowed";

        public static VNSettingsData CreateDefault()
        {
            return new VNSettingsData
            {
                schemaVersion = CurrentSchemaVersion,
                displayMode = FullScreenWindowDisplayMode,
                windowedWidth = 1920,
                windowedHeight = 1080,
                textSpeedLps = 60,
                autoSpeedNormalized = 0.5f,
                masterVolumeNormalized = 1f,
                bgmVolumeNormalized = 1f,
                sfxVolumeNormalized = 1f,
                voiceVolumeNormalized = 1f,
                skipUnread = false,
                screenShakeEnabled = true,
                inputBindingOverridesJson = string.Empty,
            };
        }
    }
}
