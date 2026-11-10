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
        [Serializable] internal sealed class Manifest { public Entry[] entries; }
        [Serializable] internal sealed class Entry
        {
            public string runtimeLineId;
            public string sourceYarnFile;
            public string sourceNode;
            public string resumeEntryNode;
            public string scene;
            public string presentationBeat;
            public string eligibility;
            public string restrictionReason;
            public string successorNodeOrFlow;
            public string chapterId;
            public string sceneTitle;
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

        public static bool ContainsLine(string lineId, TextAsset manifestAsset = null)
        {
            var asset = manifestAsset != null ? manifestAsset : Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            try
            {
                var manifest = asset == null ? null : JsonUtility.FromJson<Manifest>(asset.text);
                if (manifest?.entries == null) return false;
                foreach (var entry in manifest.entries)
                    if (entry != null && entry.runtimeLineId == lineId) return true;
            }
            catch (Exception) { }
            return false;
        }

        public static bool TryGetSaveMetadata(string lineId, YarnProject project,
            out string chapter, out string sceneTitle, out string diagnostic, TextAsset manifestAsset = null)
        {
            chapter = null;
            sceneTitle = null;
            if (!TryResolve(lineId, project, out _, out _, out diagnostic, manifestAsset)) return false;
            var asset = manifestAsset != null ? manifestAsset : Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            try
            {
                var manifest = JsonUtility.FromJson<Manifest>(asset.text);
                foreach (var entry in manifest.entries)
                {
                    if (entry.runtimeLineId != lineId) continue;
                    if (!string.IsNullOrWhiteSpace(entry.chapterId) && !string.IsNullOrWhiteSpace(entry.sceneTitle))
                    {
                        chapter = entry.chapterId;
                        sceneTitle = entry.sceneTitle;
                        return true;
                    }
                    if (string.IsNullOrEmpty(entry.chapterId) && string.IsNullOrEmpty(entry.sceneTitle) &&
                        VNResumeCompatibilityRegistry.IsOriginalM10Entry(entry))
                    {
                        chapter = "prologue";
                        sceneTitle = "프롤로그 " + entry.scene;
                        return true;
                    }
                    break;
                }
            }
            catch (Exception) { }
            diagnostic = "The production line is missing validated chapter or scene metadata.";
            return false;
        }

        public static bool TryGetPresentationContext(string lineId, out PresentationContext context, TextAsset manifestAsset = null)
        {
            context = default;
            var asset = manifestAsset != null ? manifestAsset : Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
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

        public static bool TryResolve(string lineId, YarnProject project, out string node, out string graphVersion, out string diagnostic, TextAsset manifestAsset = null)
        {
            try { return TryResolveCore(lineId, project, out node, out graphVersion, out diagnostic, manifestAsset); }
            catch (Exception)
            {
                node = null;
                graphVersion = null;
                diagnostic = "The production narrative graph could not be validated.";
                return false;
            }
        }

        private static bool TryResolveCore(string lineId, YarnProject project, out string node, out string graphVersion, out string diagnostic, TextAsset manifestAsset)
        {
            node = null;
            graphVersion = null;
            var asset = manifestAsset != null ? manifestAsset : Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            Manifest manifest;
            try { manifest = asset == null ? null : JsonUtility.FromJson<Manifest>(asset.text); }
            catch (Exception) { manifest = null; }
            if (manifest?.entries == null || manifest.entries.Length == 0 || project == null ||
                project.compiledYarnProgram == null || project.compiledYarnProgram.Length == 0)
            {
                diagnostic = "Production line-resume manifest is missing or incomplete.";
                return false;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var nodes = new HashSet<string>(StringComparer.Ordinal);
            var compiledLines = new HashSet<string>(StringComparer.Ordinal);
            foreach (var compiledNode in project.Program.Nodes.Values)
                foreach (var instruction in compiledNode.Instructions)
                {
                    var compiledId = instruction.RunLine != null ? instruction.RunLine.LineID : instruction.AddOption?.LineID;
                    if (compiledId != null && (string.IsNullOrWhiteSpace(compiledId) || !compiledLines.Add(compiledId)))
                    {
                        diagnostic = "The compiled narrative contains duplicate or invalid runtime line IDs.";
                        return false;
                    }
                }
            Dictionary<string, Entry> originalEntries = null;
            Entry match = null;
            foreach (var item in manifest.entries)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.runtimeLineId) ||
                    string.IsNullOrWhiteSpace(item.resumeEntryNode) || string.IsNullOrWhiteSpace(item.sourceNode) ||
                    string.IsNullOrWhiteSpace(item.sourceYarnFile) ||
                    string.IsNullOrWhiteSpace(item.scene) || string.IsNullOrWhiteSpace(item.presentationBeat) ||
                    string.IsNullOrWhiteSpace(item.successorNodeOrFlow) ||
                    !ids.Add(item.runtimeLineId) || !nodes.Add(item.resumeEntryNode) ||
                    (item.eligibility != "SAVE_ELIGIBLE" && item.eligibility != "SAVE_BLOCKED_TRANSIENT") ||
                    (item.eligibility == "SAVE_BLOCKED_TRANSIENT" && string.IsNullOrWhiteSpace(item.restrictionReason)) ||
                    (item.eligibility == "SAVE_ELIGIBLE" && !string.IsNullOrWhiteSpace(item.restrictionReason)) ||
                    !project.Program.Nodes.ContainsKey(item.sourceNode) ||
                    !project.Program.Nodes.ContainsKey(item.presentationBeat) ||
                    !project.Program.Nodes.ContainsKey(item.successorNodeOrFlow) ||
                    !project.Program.Nodes.TryGetValue(item.resumeEntryNode, out var compiledEntry) ||
                    compiledEntry.Instructions.Count == 0 || compiledEntry.Instructions[0].RunLine == null ||
                    compiledEntry.Instructions[0].RunLine.LineID != item.runtimeLineId)
                {
                    diagnostic = "Production line-resume manifest contains invalid or ambiguous mappings.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(item.chapterId) || string.IsNullOrWhiteSpace(item.sceneTitle))
                {
                    if (!string.IsNullOrEmpty(item.chapterId) || !string.IsNullOrEmpty(item.sceneTitle) ||
                        (originalEntries == null && !VNResumeCompatibilityRegistry.TryGetOriginalM10Entries(out originalEntries)) ||
                        !originalEntries.TryGetValue(item.runtimeLineId, out var original) ||
                        JsonUtility.ToJson(original) != JsonUtility.ToJson(item))
                    {
                        diagnostic = "Production line-resume manifest has invalid chapter or scene metadata.";
                        return false;
                    }
                }
                var transfers = 0;
                foreach (var instruction in compiledEntry.Instructions)
                    if (instruction.RunNode != null)
                    {
                        transfers++;
                        if (instruction.RunNode.NodeName != item.successorNodeOrFlow)
                        {
                            diagnostic = "Production line-resume manifest has an invalid successor.";
                            return false;
                        }
                    }
                if (transfers != 1)
                {
                    diagnostic = "Production line-resume manifest has an ambiguous successor.";
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
            if (!TryComputeGraphVersion(project, canonical, out graphVersion))
            {
                node = null;
                diagnostic = "Narrative text identity could not be validated.";
                return false;
            }
            diagnostic = null;
            return true;
        }

        /// <summary>Accept only the current graph's canonical LF or historical CRLF fingerprint.</summary>
        public static bool IsCompatibleGraphVersion(string lineId, YarnProject project, string savedVersion,
            out string node, out string diagnostic, TextAsset manifestAsset = null)
        {
            var result = VNResumeCompatibilityRegistry.Evaluate(lineId, project, savedVersion, manifestAsset);
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

        // Preserve the original M10 identity. New graphs also bind all line and
        // option text, so editing localized narrative cannot retain an old hash.
        internal static bool TryComputeGraphVersion(YarnProject project, byte[] manifest, out string version)
        {
            version = null;
            try
            {
                var raw = Fingerprint(project.compiledYarnProgram, manifest);
                if (raw == "cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9" ||
                    raw == "9aefad0924fffae848c25a23081b48606a0cfd2c51d637bb6da4386a65883980")
                {
                    if (!VNResumeCompatibilityRegistry.AreM10TextsUnchanged(project)) return false;
                    version = raw;
                    return true;
                }
                if (project.baseLocalization == null) return false;
                var ids = new SortedSet<string>(StringComparer.Ordinal);
                foreach (var node in project.Program.Nodes.Values)
                    foreach (var instruction in node.Instructions)
                    {
                        var id = instruction.RunLine != null ? instruction.RunLine.LineID : instruction.AddOption?.LineID;
                        if (id != null && (string.IsNullOrWhiteSpace(id) || !ids.Add(id))) return false;
                    }
                using var content = new MemoryStream();
                using (var writer = new BinaryWriter(content, new UTF8Encoding(false, true), true))
                {
                    writer.Write("ProjectAllTime narrative graph with text v1");
                    writer.Write(project.compiledYarnProgram.Length);
                    writer.Write(project.compiledYarnProgram);
                    writer.Write(manifest.Length);
                    writer.Write(manifest);
                    writer.Write(ids.Count);
                    foreach (var id in ids)
                    {
                        var text = project.baseLocalization.GetLocalizedString(id);
                        if (text == null) return false;
                        writer.Write(id);
                        writer.Write(text);
                    }
                }
                using var sha = SHA256.Create();
                content.Position = 0;
                version = BitConverter.ToString(sha.ComputeHash(content)).Replace("-", "").ToLowerInvariant();
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
