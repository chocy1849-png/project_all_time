namespace ProjectAllTime.VN.Settings
{
    /// <summary>
    /// Scene-independent session owner for persisted settings. Applying settings
    /// to display, audio, input, or gameplay systems is intentionally deferred.
    /// </summary>
    public sealed class VNSettingsService
    {
        private readonly VNSettingsRepository repository;
        private VNSettingsData current;
        private string lastDiagnostic;
        private bool isWriteProtected;
        private VNSettingsStorageState storageState;

        public VNSettingsService(VNSettingsRepository repository)
        {
            this.repository = repository ?? throw new System.ArgumentNullException(nameof(repository));
            current = VNSettingsDefaults.CreateDefault();
            storageState = VNSettingsStorageState.Missing;
        }

        public VNSettingsData Current => Copy(current);
        public VNSettingsStorageState StorageState => storageState;
        public bool IsWriteProtected => isWriteProtected;
        public bool CanWrite => !isWriteProtected;
        public string LastDiagnostic => lastDiagnostic;

        public VNSettingsData Load()
        {
            var result = repository.Read();
            storageState = result.State;
            lastDiagnostic = result.Diagnostic;
            isWriteProtected = result.IsWriteProtected;
            current = result.State == VNSettingsStorageState.Valid
                ? Copy(result.Settings)
                : VNSettingsDefaults.CreateDefault();
            return Current;
        }

        public bool TrySave(VNSettingsData replacement, out string diagnostic)
        {
            if (isWriteProtected)
            {
                diagnostic = "Settings writes are blocked to preserve an unsupported or unquarantined file.";
                lastDiagnostic = diagnostic;
                return false;
            }

            if (!VNSettingsValidation.TryValidate(replacement, out diagnostic))
            {
                lastDiagnostic = diagnostic;
                return false;
            }

            var result = repository.Write(replacement);
            diagnostic = result.Diagnostic;
            lastDiagnostic = diagnostic;
            storageState = result.State;
            if (!result.Succeeded)
            {
                isWriteProtected = result.IsWriteProtected;
                return false;
            }

            current = Copy(replacement);
            isWriteProtected = false;
            return true;
        }

        private static VNSettingsData Copy(VNSettingsData source)
        {
            return new VNSettingsData
            {
                schemaVersion = source.schemaVersion,
                displayMode = source.displayMode,
                windowedWidth = source.windowedWidth,
                windowedHeight = source.windowedHeight,
                textSpeedLps = source.textSpeedLps,
                autoSpeedNormalized = source.autoSpeedNormalized,
                masterVolumeNormalized = source.masterVolumeNormalized,
                bgmVolumeNormalized = source.bgmVolumeNormalized,
                sfxVolumeNormalized = source.sfxVolumeNormalized,
                voiceVolumeNormalized = source.voiceVolumeNormalized,
                skipUnread = source.skipUnread,
                screenShakeEnabled = source.screenShakeEnabled,
                inputBindingOverridesJson = source.inputBindingOverridesJson,
            };
        }
    }
}
