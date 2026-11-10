using System;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.SaveLoad
{
    public enum VNResumeCompatibilityStatus
    {
        ExactMatch,
        LegacyNewlineEquivalent,
        ApprovedBackwardCompatible,
        MigrationRequired,
        Incompatible,
        InvalidMetadata,
    }

    public readonly struct VNResumeCompatibilityDecision
    {
        public VNResumeCompatibilityStatus Status { get; }
        public string ResumeNode { get; }
        public string Diagnostic { get; }
        public bool IsCompatible => Status == VNResumeCompatibilityStatus.ExactMatch ||
            Status == VNResumeCompatibilityStatus.LegacyNewlineEquivalent ||
            Status == VNResumeCompatibilityStatus.ApprovedBackwardCompatible;

        public VNResumeCompatibilityDecision(VNResumeCompatibilityStatus status, string node, string diagnostic)
        { Status = status; ResumeNode = node; Diagnostic = diagnostic; }
    }

    /// <summary>Side-effect-free graph identity decision. Unknown epochs fail closed.</summary>
    public static class VNResumeCompatibilityRegistry
    {
        public static VNResumeCompatibilityDecision Evaluate(string lineId, YarnProject project, string savedVersion)
        {
            if (string.IsNullOrWhiteSpace(savedVersion) ||
                !VNProductionLineResume.TryResolve(lineId, project, out var node, out var canonical, out var diagnostic))
                return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.InvalidMetadata, null,
                    "Exact-line resume metadata is invalid or unavailable.");
            if (string.Equals(savedVersion, canonical, StringComparison.Ordinal))
                return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.ExactMatch, node, null);
            var asset = Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            if (asset != null && VNProductionLineResume.TryGetManifestRepresentations(asset.bytes, out _, out var crlf) &&
                string.Equals(savedVersion, VNProductionLineResume.Fingerprint(project.compiledYarnProgram, crlf), StringComparison.Ordinal))
                return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.LegacyNewlineEquivalent, node, null);
            return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.Incompatible, null,
                "Exact-line resume metadata does not match this narrative graph.");
        }
    }
}
