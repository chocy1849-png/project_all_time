using System;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>Versioned evidence captured from a specific compiled narrative graph.</summary>
    [Serializable]
    public sealed class VNResumeCompatibilityDescriptor
    {
        public int descriptorVersion;
        public string epoch;
        public string compiledProgramSha256;
        public string manifestSha256;
        public string canonicalGraphFingerprint;
        public string crlfGraphFingerprint;
        public string manifestJson;
        public VNResumeContractHash[] nodes;
        public VNResumeContractHash[] variables;
    }

    [Serializable]
    public sealed class VNResumeContractHash
    {
        public string name;
        public string sha256;
    }
}
