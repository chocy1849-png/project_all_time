using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Owns insertion-ordered backlog entries for one application session.</summary>
    public sealed class VNBacklogService
    {
        private readonly List<VNBacklogEntry> entries = new();
        private readonly ReadOnlyCollection<VNBacklogEntry> readOnlyEntries;

        public VNBacklogService()
        {
            readOnlyEntries = entries.AsReadOnly();
        }

        public IReadOnlyList<VNBacklogEntry> Entries => readOnlyEntries;
        public int Count => entries.Count;

        public event Action<VNBacklogEntry> EntryAdded;

        internal void Append(VNBacklogEntry entry)
        {
            if (entry == null) return;
            entries.Add(entry);
            EntryAdded?.Invoke(entry);
        }

        public void ClearSession()
        {
            entries.Clear();
        }
    }
}
