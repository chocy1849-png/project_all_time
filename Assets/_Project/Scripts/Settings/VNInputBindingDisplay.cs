namespace ProjectAllTime.VN.Settings
{
    public sealed class VNInputBindingDisplay
    {
        public VNRebindTarget Target { get; }
        public string EffectivePath { get; }
        public string DisplayString { get; }
        public bool IsDefault { get; }

        public VNInputBindingDisplay(VNRebindTarget target, string effectivePath, string displayString, bool isDefault)
        {
            Target = target;
            EffectivePath = effectivePath ?? string.Empty;
            DisplayString = displayString ?? string.Empty;
            IsDefault = isDefault;
        }
    }
}
