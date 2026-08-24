using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    [DisallowMultipleComponent]
    public sealed class VNGameplaySettingsView : MonoBehaviour
    {
        [SerializeField] private Toggle skipUnreadToggle;
        [SerializeField] private Toggle screenShakeToggle;
        private VNSettingsService settingsService;
        private VNGameplaySettingsController controller;
        private Action<string> report;
        private bool initialized;

        private void OnEnable() => RegisterListeners(true);
        private void OnDisable() => RegisterListeners(false);
        public void Initialize(VNSettingsService service, VNGameplaySettingsController gameplayController, Action<string> diagnostic)
        {
            settingsService = service ?? throw new ArgumentNullException(nameof(service));
            controller = gameplayController ?? throw new ArgumentNullException(nameof(gameplayController));
            report = diagnostic;
            initialized = true;
            Refresh(service.Current, service.CanWrite);
        }
        public void Refresh(VNSettingsData settings, bool canWrite)
        {
            if (!initialized) return;
            if (skipUnreadToggle != null) { skipUnreadToggle.SetIsOnWithoutNotify(settings.skipUnread); skipUnreadToggle.interactable = canWrite; }
            if (screenShakeToggle != null) { screenShakeToggle.SetIsOnWithoutNotify(settings.screenShakeEnabled); screenShakeToggle.interactable = canWrite; }
        }
        private void HandleSkipUnread(bool value) => Commit(value, controller.TrySetSkipUnread);
        private void HandleScreenShake(bool value) => Commit(value, controller.TrySetScreenShakeEnabled);
        private delegate bool ToggleMutation(bool value, out string diagnostic);
        public bool TryValidateWiring(out string diagnostic)
        {
            if (skipUnreadToggle == null || screenShakeToggle == null) { diagnostic = "Gameplay Settings UI requires both Toggles."; return false; }
            diagnostic = null; return true;
        }
        private void Commit(bool value, ToggleMutation mutation)
        {
            if (!initialized || !settingsService.CanWrite) return;
            var success = mutation(value, out var diagnostic);
            if (!success || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }
        private void RegisterListeners(bool add)
        {
            if (skipUnreadToggle != null) { if (add) skipUnreadToggle.onValueChanged.AddListener(HandleSkipUnread); else skipUnreadToggle.onValueChanged.RemoveListener(HandleSkipUnread); }
            if (screenShakeToggle != null) { if (add) screenShakeToggle.onValueChanged.AddListener(HandleScreenShake); else screenShakeToggle.onValueChanged.RemoveListener(HandleScreenShake); }
        }
    }
}
