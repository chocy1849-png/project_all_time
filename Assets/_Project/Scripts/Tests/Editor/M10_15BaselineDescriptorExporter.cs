using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    /// <summary>One-time export from the frozen M10 graph; never overwrites existing evidence.</summary>
    public static class M10_15BaselineDescriptorExporter
    {
        public static void Export()
        {
            var output = Environment.GetEnvironmentVariable("M10_15_DESCRIPTOR_OUTPUT");
            if (string.IsNullOrWhiteSpace(output) || File.Exists(output))
                throw new InvalidOperationException("M10_15_DESCRIPTOR_OUTPUT must name a new file.");
            var project = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            var asset = Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            if (project == null || asset == null) throw new InvalidOperationException("M10 baseline assets are missing.");
            var program = project.compiledYarnProgram;
            var utf8 = new UTF8Encoding(false, true);
            var manifest = utf8.GetString(asset.bytes).Replace("\r\n", "\n");
            var canonical = utf8.GetBytes(manifest);
            var crlf = utf8.GetBytes(manifest.Replace("\n", "\r\n"));
            var programHash = Hash(program);
            var lfFingerprint = Hash(program.Concat(canonical).ToArray());
            var crlfFingerprint = Hash(program.Concat(crlf).ToArray());
            if (programHash != "f084b94f29cedfbe2897857158fd717d3dedb0ae9afe9d0025dbe9b861ae75d5" ||
                lfFingerprint != "cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9" ||
                crlfFingerprint != "9aefad0924fffae848c25a23081b48606a0cfd2c51d637bb6da4386a65883980")
                throw new InvalidOperationException("The compiled graph is not the approved M10 baseline.");
            var descriptor = new VNResumeCompatibilityDescriptor
            {
                descriptorVersion = 1,
                epoch = "M10",
                compiledProgramSha256 = programHash,
                manifestSha256 = Hash(canonical),
                canonicalGraphFingerprint = lfFingerprint,
                crlfGraphFingerprint = crlfFingerprint,
                manifestJson = manifest,
                nodes = project.Program.Nodes.OrderBy(p => p.Key, StringComparer.Ordinal)
                    .Select(p => new VNResumeContractHash { name = p.Key, sha256 = Hash(p.Value.ToByteArray()) }).ToArray(),
                variables = project.Program.InitialValues.OrderBy(p => p.Key, StringComparer.Ordinal)
                    .Select(p => new VNResumeContractHash { name = p.Key, sha256 = Hash(p.Value.ToByteArray()) }).ToArray(),
            };
            File.WriteAllText(output, JsonUtility.ToJson(descriptor, true) + "\n", utf8);
        }

        private static string Hash(byte[] bytes)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }
}
