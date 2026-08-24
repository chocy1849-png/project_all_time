using System;
using System.Collections.Generic;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Session-only stable Yarn line-ID history for future Skip Read.</summary>
    public sealed class VNReadHistoryService
    {
        private readonly HashSet<string> readLineIds = new(StringComparer.Ordinal);

        public int Count => readLineIds.Count;

        public bool IsRead(string lineId) =>
            !string.IsNullOrWhiteSpace(lineId) && readLineIds.Contains(lineId);

        public IReadOnlyCollection<string> Snapshot() => new List<string>(readLineIds).AsReadOnly();

        internal bool RecordAuthorizedConsume(string lineId)
        {
            return !string.IsNullOrWhiteSpace(lineId) && readLineIds.Add(lineId);
        }

        public void ClearSession()
        {
            readLineIds.Clear();
        }
    }
}
