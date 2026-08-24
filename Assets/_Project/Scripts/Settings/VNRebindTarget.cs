namespace ProjectAllTime.VN.Settings
{
    public enum VNRebindTarget
    {
        Advance,
        ToggleAuto,
        SkipHold,
        ToggleHide,
        QuickSave,
        QuickLoad,
    }

    public enum VNRebindResult
    {
        Succeeded,
        Canceled,
        Duplicate,
        PersistenceFailed,
        ContractInvalid,
    }
}
