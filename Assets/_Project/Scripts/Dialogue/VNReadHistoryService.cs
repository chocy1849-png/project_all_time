using System;
using System.Collections.Generic;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Effective stable Yarn line-ID history. Durable MetaProgress reads are a
    /// baseline; newly authorized reads remain a session overlay until durable
    /// persistence confirms their promotion.
    /// </summary>
    public sealed class VNReadHistoryService
    {
        private readonly HashSet<string> persistentReadLineIds = new(StringComparer.Ordinal);
        private readonly HashSet<string> sessionReadLineIds = new(StringComparer.Ordinal);

        public int Count
        {
            get
            {
                var effective = new HashSet<string>(persistentReadLineIds, StringComparer.Ordinal);
                effective.UnionWith(sessionReadLineIds);
                return effective.Count;
            }
        }

        public bool IsRead(string lineId) =>
            !string.IsNullOrWhiteSpace(lineId) &&
            (persistentReadLineIds.Contains(lineId) || sessionReadLineIds.Contains(lineId));

        public IReadOnlyCollection<string> Snapshot()
        {
            var effective = new HashSet<string>(persistentReadLineIds, StringComparer.Ordinal);
            effective.UnionWith(sessionReadLineIds);
            var snapshot = new List<string>(effective);
            snapshot.Sort(StringComparer.Ordinal);
            return snapshot.AsReadOnly();
        }

        /// <summary>
        /// Replaces only the durable baseline. The supplied collection is fully
        /// validated before it replaces anything, and session reads are retained.
        /// </summary>
        public bool ReplacePersistentBaseline(IEnumerable<string> lineIds)
        {
            if (lineIds == null) return false;

            var replacement = new HashSet<string>(StringComparer.Ordinal);
            foreach (var lineId in lineIds)
            {
                if (string.IsNullOrWhiteSpace(lineId)) return false;
                replacement.Add(lineId);
            }

            persistentReadLineIds.Clear();
            persistentReadLineIds.UnionWith(replacement);
            return true;
        }

        /// <summary>Promotes a successfully persisted session read to the durable baseline.</summary>
        internal bool PromoteToPersistent(string lineId)
        {
            if (string.IsNullOrWhiteSpace(lineId)) return false;
            if (persistentReadLineIds.Contains(lineId)) return true;
            persistentReadLineIds.Add(lineId);
            sessionReadLineIds.Remove(lineId);
            return true;
        }

        internal bool RecordAuthorizedConsume(string lineId)
        {
            return !string.IsNullOrWhiteSpace(lineId) && !IsRead(lineId) && sessionReadLineIds.Add(lineId);
        }

        public void ClearSession()
        {
            sessionReadLineIds.Clear();
        }
    }
}
