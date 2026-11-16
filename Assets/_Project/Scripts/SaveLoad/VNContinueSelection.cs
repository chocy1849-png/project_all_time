using System;
using System.Collections.Generic;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>Ranks repository inspections without changing storage or runtime state.</summary>
    public static class VNContinueSelection
    {
        public static bool TrySelect(IEnumerable<VNSaveSlotInspection> slots, Func<VNSaveSlotKey, bool> isCompatible, out VNSaveSlotKey selected)
        {
            selected = default;
            if (slots == null || isCompatible == null) return false;
            bool found = false;
            DateTimeOffset latest = default;
            foreach (var inspection in slots)
            {
                if (inspection == null || inspection.ReadResult == null || !inspection.SlotKey.IsValid) continue;
                var read = inspection.ReadResult;
                if (read.State != VNSaveSlotState.Valid || read.SaveData == null ||
                    !VNSaveSerializer.TryParseUtcTimestamp(read.SaveData.savedAtUtcIso8601, out var timestamp) ||
                    (found && (timestamp < latest || timestamp == latest && !Precedes(inspection.SlotKey, selected))) ||
                    !isCompatible(inspection.SlotKey)) continue;
                found = true; latest = timestamp; selected = inspection.SlotKey;
            }
            return found;
        }

        private static bool Precedes(VNSaveSlotKey left, VNSaveSlotKey right) =>
            (int)left.SlotType < (int)right.SlotType || left.SlotType == right.SlotType && left.SlotIndex < right.SlotIndex;
    }
}
