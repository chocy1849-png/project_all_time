using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>Validated project-owned mapping from runtime line IDs to executable entries.</summary>
    public static class VNProductionLineResume
    {
        [Serializable] private sealed class Manifest { public Entry[] entries; }
        [Serializable] private sealed class Entry
        {
            public string runtimeLineId;
            public string sourceYarnFile;
            public string sourceNode;
            public string resumeEntryNode;
            public string scene;
            public string presentationBeat;
            public string eligibility;
            public string restrictionReason;
            public string informationTitle;
            public string informationFacts;
            public bool medicalDiagram;
        }

        public readonly struct PresentationContext
        {
            public readonly string Title, Facts;
            public readonly bool Diagram;
            public PresentationContext(string title, string facts, bool diagram)
            { Title = title; Facts = facts; Diagram = diagram; }
        }

        public static bool TryGetPresentationContext(string lineId, out PresentationContext context)
        {
            context = default;
            var asset = Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            if (asset == null) return false;
            try
            {
                var manifest = JsonUtility.FromJson<Manifest>(asset.text);
                if (manifest?.entries == null) return false;
                foreach (var item in manifest.entries)
                {
                    if (item != null && item.runtimeLineId == lineId)
                    {
                        context = new PresentationContext(item.informationTitle ?? string.Empty,
                            item.informationFacts ?? string.Empty, item.medicalDiagram);
                        return true;
                    }
                }
            }
            catch (Exception) { return false; }
            return false;
        }

        public static bool TryResolve(string lineId, YarnProject project, out string node, out string graphVersion, out string diagnostic)
        {
            node = null;
            graphVersion = null;
            var asset = Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            Manifest manifest;
            try { manifest = asset == null ? null : JsonUtility.FromJson<Manifest>(asset.text); }
            catch (Exception) { manifest = null; }
            if (manifest?.entries == null || manifest.entries.Length != 379)
            {
                diagnostic = "Production line-resume manifest is missing or incomplete.";
                return false;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var nodes = new HashSet<string>(StringComparer.Ordinal);
            Entry match = null;
            foreach (var item in manifest.entries)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.runtimeLineId) ||
                    string.IsNullOrWhiteSpace(item.resumeEntryNode) || string.IsNullOrWhiteSpace(item.sourceNode) ||
                    !ids.Add(item.runtimeLineId) || !nodes.Add(item.resumeEntryNode) ||
                    (item.eligibility != "SAVE_ELIGIBLE" && item.eligibility != "SAVE_BLOCKED_TRANSIENT"))
                {
                    diagnostic = "Production line-resume manifest contains invalid or ambiguous mappings.";
                    return false;
                }
                if (string.Equals(item.runtimeLineId, lineId, StringComparison.Ordinal)) match = item;
            }
            if (match == null)
            {
                diagnostic = "Saving is unavailable for this line.";
                return false;
            }
            if (match.eligibility != "SAVE_ELIGIBLE")
            {
                diagnostic = "Saving is unavailable during " + match.restrictionReason + ".";
                return false;
            }
            if (project == null || project.compiledYarnProgram == null || project.compiledYarnProgram.Length == 0 ||
                !project.Program.Nodes.TryGetValue(match.resumeEntryNode, out var entry) || entry.Instructions.Count == 0 ||
                entry.Instructions[0].RunLine == null || entry.Instructions[0].RunLine.LineID != lineId)
            {
                diagnostic = "The approved exact-line resume entry does not match the compiled program.";
                return false;
            }
            if (!TryGetManifestRepresentations(asset.bytes, out var canonical, out _))
            {
                diagnostic = "Production line-resume manifest is not valid UTF-8.";
                return false;
            }
            node = match.resumeEntryNode;
            graphVersion = Fingerprint(project.compiledYarnProgram, canonical);
            diagnostic = null;
            return true;
        }

        /// <summary>Accept only the current graph's canonical LF or historical CRLF fingerprint.</summary>
        public static bool IsCompatibleGraphVersion(string lineId, YarnProject project, string savedVersion,
            out string node, out string diagnostic)
        {
            var result = VNResumeCompatibilityRegistry.Evaluate(lineId, project, savedVersion);
            node = result.ResumeNode;
            diagnostic = result.Diagnostic;
            return result.IsCompatible;
        }

        internal static bool TryGetManifestRepresentations(byte[] bytes, out byte[] canonical, out byte[] crlf)
        {
            canonical = null;
            crlf = null;
            try
            {
                var utf8 = new UTF8Encoding(false, true);
                var text = utf8.GetString(bytes).Replace("\r\n", "\n");
                canonical = utf8.GetBytes(text);
                crlf = utf8.GetBytes(text.Replace("\n", "\r\n"));
                return true;
            }
            catch (DecoderFallbackException) { return false; }
        }

        internal static string Fingerprint(byte[] program, byte[] manifest)
        {
            using var sha = SHA256.Create();
            using var content = new MemoryStream();
            content.Write(program, 0, program.Length);
            content.Write(manifest, 0, manifest.Length);
            content.Position = 0;
            var hash = sha.ComputeHash(content);
            var result = new StringBuilder(hash.Length * 2);
            foreach (var value in hash) result.Append(value.ToString("x2"));
            return result.ToString();
        }
    }
}
