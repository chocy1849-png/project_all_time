namespace ProjectAllTime.VN.MetaProgress
{
    public enum VNMetaProgressChangeKind
    {
        ReadLine,
        CG,
        Chapter,
        ArchiveEntry,
        Achievement,
        Ending,
    }

    /// <summary>An immutable description of one newly committed MetaProgress ID.</summary>
    public sealed class VNMetaProgressChange
    {
        public VNMetaProgressChangeKind Kind { get; }
        public string Id { get; }

        internal VNMetaProgressChange(VNMetaProgressChangeKind kind, string id)
        {
            Kind = kind;
            Id = id;
        }
    }
}
