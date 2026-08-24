using UnityEngine;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Opt-in, low-volume M6 technical smoke diagnostics.</summary>
    public static class VNConvenienceDiagnostics
    {
        public static bool Enabled { get; private set; }

        public static void SetEnabled(bool enabled) => Enabled = enabled;

        public static void Log(string message)
        {
            if (Enabled) Debug.Log(message);
        }
    }
}
