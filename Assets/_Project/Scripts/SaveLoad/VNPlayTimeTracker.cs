using System;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Unwired play-time value holder for save metadata. Scene ownership and
    /// pause policy are intentionally deferred to later M5 work.
    /// </summary>
    public sealed class VNPlayTimeTracker
    {
        private float playedSeconds;

        public float PlayedSeconds => playedSeconds;

        public bool TrySetPlayedSeconds(float value)
        {
            if (!IsValidPlayedSeconds(value)) return false;
            playedSeconds = value;
            return true;
        }

        public bool TryAdvance(float deltaSeconds)
        {
            if (!IsValidPlayedSeconds(deltaSeconds)) return false;

            var nextValue = playedSeconds + deltaSeconds;
            if (!IsValidPlayedSeconds(nextValue)) return false;
            playedSeconds = nextValue;
            return true;
        }

        public static bool IsValidPlayedSeconds(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }
}
