using System;

namespace ProjectAllTime.VN.SaveLoad
{
    public enum VNSaveSlotType
    {
        Manual,
        Auto,
        Quick,
    }

    /// <summary>
    /// A validated logical save slot. This key, rather than serialized save
    /// data, is authoritative when resolving a filesystem path.
    /// </summary>
    public readonly struct VNSaveSlotKey : IEquatable<VNSaveSlotKey>
    {
        public const int ManualSlotCount = 12;
        public const int AutoSlotCount = 5;
        public const int QuickSlotCount = 1;

        public VNSaveSlotType SlotType { get; }
        public int SlotIndex { get; }

        public VNSaveSlotKey(VNSaveSlotType slotType, int slotIndex)
        {
            SlotType = slotType;
            SlotIndex = slotIndex;
        }

        public bool IsValid
        {
            get
            {
                switch (SlotType)
                {
                    case VNSaveSlotType.Manual:
                        return SlotIndex >= 0 && SlotIndex < ManualSlotCount;
                    case VNSaveSlotType.Auto:
                        return SlotIndex >= 0 && SlotIndex < AutoSlotCount;
                    case VNSaveSlotType.Quick:
                        return SlotIndex == 0;
                    default:
                        return false;
                }
            }
        }

        public static bool TryCreate(VNSaveSlotType slotType, int slotIndex, out VNSaveSlotKey key)
        {
            key = new VNSaveSlotKey(slotType, slotIndex);
            return key.IsValid;
        }

        public string ToSerializedSlotType()
        {
            switch (SlotType)
            {
                case VNSaveSlotType.Manual: return "manual";
                case VNSaveSlotType.Auto: return "auto";
                case VNSaveSlotType.Quick: return "quick";
                default: return null;
            }
        }

        public bool TryGetCanonicalFileName(out string fileName)
        {
            fileName = null;
            if (!IsValid) return false;

            switch (SlotType)
            {
                case VNSaveSlotType.Manual:
                    fileName = $"manual_{SlotIndex:00}.json";
                    return true;
                case VNSaveSlotType.Auto:
                    fileName = $"auto_{SlotIndex:00}.json";
                    return true;
                case VNSaveSlotType.Quick:
                    fileName = "quick_00.json";
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns the only thumbnail sidecar basename produced by the M5 UI.
        /// The slot key, not caller-provided save metadata, remains the source
        /// of truth for this name.
        /// </summary>
        public bool TryGetCanonicalThumbnailFileName(out string fileName)
        {
            fileName = null;
            if (!TryGetCanonicalFileName(out var jsonFileName)) return false;
            fileName = System.IO.Path.GetFileNameWithoutExtension(jsonFileName) + ".jpg";
            return true;
        }

        public static bool TryParseSerializedSlotType(string value, out VNSaveSlotType slotType)
        {
            switch (value)
            {
                case "manual":
                    slotType = VNSaveSlotType.Manual;
                    return true;
                case "auto":
                    slotType = VNSaveSlotType.Auto;
                    return true;
                case "quick":
                    slotType = VNSaveSlotType.Quick;
                    return true;
                default:
                    slotType = default;
                    return false;
            }
        }

        public bool Equals(VNSaveSlotKey other) => SlotType == other.SlotType && SlotIndex == other.SlotIndex;
        public override bool Equals(object obj) => obj is VNSaveSlotKey other && Equals(other);
        public override int GetHashCode() => ((int)SlotType * 397) ^ SlotIndex;
        public override string ToString() => IsValid ? $"{ToSerializedSlotType()}:{SlotIndex}" : "invalid";
        public static bool operator ==(VNSaveSlotKey left, VNSaveSlotKey right) => left.Equals(right);
        public static bool operator !=(VNSaveSlotKey left, VNSaveSlotKey right) => !left.Equals(right);
    }
}
