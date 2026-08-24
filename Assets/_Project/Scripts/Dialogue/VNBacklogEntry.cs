namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// One displayed occurrence of dialogue in the current application session.
    /// It deliberately contains text-only data so it can be consumed by a later
    /// backlog UI without retaining Yarn or Unity asset references.
    /// </summary>
    public sealed class VNBacklogEntry
    {
        public string LineId { get; }
        public string SpeakerName { get; }
        public string Text { get; }
        public bool IsNarration { get; }

        public VNBacklogEntry(string lineId, string speakerName, string text)
        {
            LineId = lineId ?? string.Empty;
            SpeakerName = speakerName ?? string.Empty;
            Text = text ?? string.Empty;
            IsNarration = string.IsNullOrWhiteSpace(SpeakerName);
        }
    }
}
