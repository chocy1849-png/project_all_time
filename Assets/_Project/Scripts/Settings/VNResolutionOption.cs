using System;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>
    /// A display resolution identified only by width and height. Refresh rate is
    /// intentionally not part of the M7 settings or selection contract.
    /// </summary>
    public readonly struct VNResolutionOption : IEquatable<VNResolutionOption>
    {
        public int Width { get; }
        public int Height { get; }
        public bool IsValid => Width > 0 && Height > 0;
        public string DisplayLabel => Width + " × " + Height;

        public VNResolutionOption(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width;
            Height = height;
        }

        public static bool TryCreate(int width, int height, out VNResolutionOption option)
        {
            option = default;
            if (width <= 0 || height <= 0) return false;
            option = new VNResolutionOption(width, height);
            return true;
        }

        public bool Equals(VNResolutionOption other) => Width == other.Width && Height == other.Height;
        public override bool Equals(object obj) => obj is VNResolutionOption other && Equals(other);
        public override int GetHashCode() => (Width * 397) ^ Height;
        public override string ToString() => IsValid ? DisplayLabel : "invalid";
        public static bool operator ==(VNResolutionOption left, VNResolutionOption right) => left.Equals(right);
        public static bool operator !=(VNResolutionOption left, VNResolutionOption right) => !left.Equals(right);
    }
}
