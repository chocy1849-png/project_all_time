using System;
using ProjectAllTime.VN.Dialogue;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>Read-only preference gate for a future screen-shake consumer.</summary>
    public interface IVNScreenShakeGate
    {
        bool IsScreenShakeEnabled { get; }
    }

    /// <summary>
    /// Applies the persisted gameplay preferences without duplicating M6 Skip
    /// logic or inventing a screen-shake runtime consumer.
    /// </summary>
    public sealed class VNGameplaySettingsController : IVNScreenShakeGate
    {
        private readonly VNSettingsService settingsService;
        private readonly VNConvenienceController convenienceController;

        public VNGameplaySettingsController(VNSettingsService settingsService, VNConvenienceController convenienceController)
        {
            this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.convenienceController = convenienceController ?? throw new ArgumentNullException(nameof(convenienceController));
        }

        public bool IsScreenShakeEnabled => settingsService.Current.screenShakeEnabled;

        public static VNSkipPolicy ToSkipPolicy(bool skipUnread)
        {
            return skipUnread ? VNSkipPolicy.All : VNSkipPolicy.ReadOnly;
        }

        /// <summary>Applies the current effective preference snapshot without writing it.</summary>
        public bool TryApplyCurrentSettings(out string diagnostic)
        {
            convenienceController.SetSkipPolicy(ToSkipPolicy(settingsService.Current.skipUnread));
            diagnostic = null;
            return true;
        }

        public bool TrySetSkipUnread(bool enabled, out string diagnostic)
        {
            var replacement = settingsService.Current;
            replacement.skipUnread = enabled;
            if (!settingsService.TrySave(replacement, out diagnostic)) return false;

            convenienceController.SetSkipPolicy(ToSkipPolicy(enabled));
            diagnostic = null;
            return true;
        }

        public bool TrySetScreenShakeEnabled(bool enabled, out string diagnostic)
        {
            var replacement = settingsService.Current;
            replacement.screenShakeEnabled = enabled;
            return settingsService.TrySave(replacement, out diagnostic);
        }
    }
}
