using System;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>
    /// Pure schema-v1 validation. Callers decide whether invalid persisted data
    /// is quarantined; this class never clamps or changes values.
    /// </summary>
    public static class VNSettingsValidation
    {
        public static bool TryValidate(VNSettingsData data, out string diagnostic)
        {
            if (data == null)
            {
                diagnostic = "Settings data is missing.";
                return false;
            }

            if (data.schemaVersion <= 0)
            {
                diagnostic = "Settings schema version must be positive.";
                return false;
            }

            if (data.schemaVersion != VNSettingsDefaults.CurrentSchemaVersion)
            {
                diagnostic = "Settings data is not schema version 1.";
                return false;
            }

            if (!IsKnownDisplayMode(data.displayMode))
            {
                diagnostic = "Settings display mode is not recognized.";
                return false;
            }

            if (data.windowedWidth <= 0 || data.windowedHeight <= 0)
            {
                diagnostic = "Settings windowed dimensions must be positive.";
                return false;
            }

            if (data.textSpeedLps <= 0)
            {
                diagnostic = "Settings text speed must be positive.";
                return false;
            }

            if (!IsNormalized(data.autoSpeedNormalized) ||
                !IsNormalized(data.masterVolumeNormalized) ||
                !IsNormalized(data.bgmVolumeNormalized) ||
                !IsNormalized(data.sfxVolumeNormalized) ||
                !IsNormalized(data.voiceVolumeNormalized))
            {
                diagnostic = "Settings normalized values must be finite and between zero and one.";
                return false;
            }

            if (data.inputBindingOverridesJson == null)
            {
                diagnostic = "Settings input binding overrides must not be null.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        public static bool IsKnownDisplayMode(string displayMode)
        {
            return displayMode == VNSettingsDefaults.FullScreenWindowDisplayMode ||
                   displayMode == VNSettingsDefaults.WindowedDisplayMode;
        }

        private static bool IsNormalized(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value <= 1f;
        }
    }
}
