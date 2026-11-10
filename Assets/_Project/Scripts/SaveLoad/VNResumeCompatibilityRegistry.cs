using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
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
        private const string M10DescriptorSha256 = "126088f2c52fc7fc5c8eaa860c70240e345e937929578566a7967b59f984a8f0";
        private const string M10Lf = "cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9";
        private const string M10CrLf = "9aefad0924fffae848c25a23081b48606a0cfd2c51d637bb6da4386a65883980";
        public static VNResumeCompatibilityDecision Evaluate(string lineId, YarnProject project, string savedVersion, TextAsset manifestAsset = null)
        {
            if (string.IsNullOrWhiteSpace(savedVersion) ||
                !VNProductionLineResume.TryResolve(lineId, project, out var node, out var canonical, out var diagnostic, manifestAsset))
                return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.InvalidMetadata, null,
                    "Exact-line resume metadata is invalid or unavailable.");
            if (string.Equals(savedVersion, canonical, StringComparison.Ordinal))
                return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.ExactMatch, node, null);
            var asset = manifestAsset != null ? manifestAsset : Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            if (asset != null && VNProductionLineResume.TryGetManifestRepresentations(asset.bytes, out _, out var crlf) &&
                VNProductionLineResume.TryComputeGraphVersion(project, crlf, out var crlfVersion) &&
                string.Equals(savedVersion, crlfVersion, StringComparison.Ordinal))
                return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.LegacyNewlineEquivalent, node, null);
            if (savedVersion == M10Lf || savedVersion == M10CrLf)
            {
                if (MatchesM10Contract(project, asset, out var failure))
                    return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.ApprovedBackwardCompatible, node, null);
                return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.MigrationRequired, null, failure);
            }
            return new VNResumeCompatibilityDecision(VNResumeCompatibilityStatus.Incompatible, null,
                "Exact-line resume metadata does not match this narrative graph.");
        }

        // Conservative subset: every original compiled node, original variable default,
        // and original manifest entry must be byte-for-byte equivalent. Extra nodes and
        // variables may be added. A changed dependency anywhere rejects the epoch.
        private static bool MatchesM10Contract(YarnProject project, TextAsset manifestAsset, out string diagnostic)
        {
            diagnostic = "The historical M10 execution contract cannot be verified; migration is required.";
            try
            {
                var evidence = Resources.Load<TextAsset>("M10_15_BASELINE_DESCRIPTOR");
                if (evidence == null || manifestAsset == null ||
                    !VNProductionLineResume.TryGetManifestRepresentations(evidence.bytes, out var canonicalEvidence, out _) ||
                    Hash(canonicalEvidence) != M10DescriptorSha256) return false;
                var baseline = JsonUtility.FromJson<VNResumeCompatibilityDescriptor>(Encoding.UTF8.GetString(canonicalEvidence));
                if (baseline == null || baseline.descriptorVersion != 1 || baseline.epoch != "M10" ||
                    baseline.canonicalGraphFingerprint != M10Lf || baseline.crlfGraphFingerprint != M10CrLf ||
                    baseline.nodes == null || baseline.nodes.Length != 535 ||
                    baseline.variables == null || baseline.variables.Length != 8 ||
                    baseline.lineTexts == null || baseline.lineTexts.Length != 541 || project.baseLocalization == null) return false;
                var oldManifest = JsonUtility.FromJson<VNProductionLineResume.Manifest>(baseline.manifestJson);
                var currentManifest = JsonUtility.FromJson<VNProductionLineResume.Manifest>(manifestAsset.text);
                if (oldManifest?.entries == null || oldManifest.entries.Length != 379 || currentManifest?.entries == null) return false;
                var current = new Dictionary<string, VNProductionLineResume.Entry>(StringComparer.Ordinal);
                foreach (var entry in currentManifest.entries)
                    if (entry == null || string.IsNullOrEmpty(entry.runtimeLineId) || !current.TryAdd(entry.runtimeLineId, entry)) return false;
                foreach (var entry in oldManifest.entries)
                    if (entry == null || !current.TryGetValue(entry.runtimeLineId, out var replacement) ||
                        JsonUtility.ToJson(entry) != JsonUtility.ToJson(replacement)) return false;
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var contract in baseline.nodes)
                    if (contract == null || string.IsNullOrEmpty(contract.name) || !names.Add(contract.name) ||
                        !project.Program.Nodes.TryGetValue(contract.name, out var node) || Hash(node.ToByteArray()) != contract.sha256) return false;
                foreach (var contract in baseline.nodes)
                {
                    foreach (var instruction in project.Program.Nodes[contract.name].Instructions)
                    {
                        // A runtime-selected node could start resolving to a newly added
                        // node, and saliency can change when candidates are added.
                        if (instruction.PeekAndRunNode != null || instruction.PeekAndDetourToNode != null ||
                            instruction.AddSaliencyCandidate != null) return false;
                        if (instruction.RunNode != null && !names.Contains(instruction.RunNode.NodeName)) return false;
                        if (instruction.DetourToNode != null && !names.Contains(instruction.DetourToNode.NodeName)) return false;
                    }
                }
                names.Clear();
                foreach (var contract in baseline.variables)
                    if (contract == null || string.IsNullOrEmpty(contract.name) || !names.Add(contract.name) ||
                        !project.Program.InitialValues.TryGetValue(contract.name, out var value) || Hash(value.ToByteArray()) != contract.sha256) return false;
                names.Clear();
                foreach (var contract in baseline.lineTexts)
                {
                    if (contract == null || string.IsNullOrEmpty(contract.name) || !names.Add(contract.name)) return false;
                    var text = project.baseLocalization.GetLocalizedString(contract.name);
                    if (text == null || Hash(Encoding.UTF8.GetBytes(text)) != contract.sha256) return false;
                }
                diagnostic = null;
                return true;
            }
            catch (Exception) { return false; }
        }

        internal static bool IsOriginalM10Entry(VNProductionLineResume.Entry current)
        {
            return TryGetOriginalM10Entries(out var entries) && entries.TryGetValue(current.runtimeLineId, out var entry) &&
                JsonUtility.ToJson(entry) == JsonUtility.ToJson(current);
        }

        internal static bool AreM10TextsUnchanged(YarnProject project)
        {
            try
            {
                var evidence = Resources.Load<TextAsset>("M10_15_BASELINE_DESCRIPTOR");
                if (project == null || project.baseLocalization == null || evidence == null ||
                    !VNProductionLineResume.TryGetManifestRepresentations(evidence.bytes, out var bytes, out _) ||
                    Hash(bytes) != M10DescriptorSha256) return false;
                var baseline = JsonUtility.FromJson<VNResumeCompatibilityDescriptor>(Encoding.UTF8.GetString(bytes));
                if (baseline?.lineTexts == null || baseline.lineTexts.Length != 541) return false;
                var ids = new HashSet<string>(StringComparer.Ordinal);
                foreach (var contract in baseline.lineTexts)
                {
                    if (contract == null || string.IsNullOrEmpty(contract.name) || !ids.Add(contract.name)) return false;
                    var text = project.baseLocalization.GetLocalizedString(contract.name);
                    if (text == null || Hash(Encoding.UTF8.GetBytes(text)) != contract.sha256) return false;
                }
                return true;
            }
            catch (Exception) { return false; }
        }

        internal static bool TryGetOriginalM10Entries(out Dictionary<string, VNProductionLineResume.Entry> entries)
        {
            entries = null;
            try
            {
                var evidence = Resources.Load<TextAsset>("M10_15_BASELINE_DESCRIPTOR");
                if (evidence == null || !VNProductionLineResume.TryGetManifestRepresentations(evidence.bytes, out var bytes, out _) ||
                    Hash(bytes) != M10DescriptorSha256) return false;
                var baseline = JsonUtility.FromJson<VNResumeCompatibilityDescriptor>(Encoding.UTF8.GetString(bytes));
                var manifest = JsonUtility.FromJson<VNProductionLineResume.Manifest>(baseline.manifestJson);
                if (manifest?.entries == null || manifest.entries.Length != 379) return false;
                entries = new Dictionary<string, VNProductionLineResume.Entry>(StringComparer.Ordinal);
                foreach (var entry in manifest.entries)
                    if (entry == null || string.IsNullOrEmpty(entry.runtimeLineId) || !entries.TryAdd(entry.runtimeLineId, entry))
                    { entries = null; return false; }
                return true;
            }
            catch (Exception) { }
            return false;
        }

        private static string Hash(byte[] bytes)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }
}
