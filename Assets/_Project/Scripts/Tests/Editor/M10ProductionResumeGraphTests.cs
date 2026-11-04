using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M10ProductionResumeGraphTests
    {
        [Serializable] private sealed class Manifest { public Entry[] entries; }
        [Serializable] private sealed class Entry
        {
            public string runtimeLineId, sourceYarnFile, sourceNode, resumeEntryNode;
            public string scene, presentationBeat, eligibility, restrictionReason, successorNodeOrFlow;
        }

        [Test]
        public void EveryProductionLine_HasUniqueCompiledEntryAndDeclaredEligibility()
        {
            var asset = Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST");
            Assert.That(asset, Is.Not.Null);
            var manifest = JsonUtility.FromJson<Manifest>(asset.text);
            var project = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            Assert.That(project, Is.Not.Null);
            Assert.That(manifest.entries, Has.Length.EqualTo(379));
            Assert.That(manifest.entries.Select(e => e.runtimeLineId).Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(379));
            Assert.That(manifest.entries.Select(e => e.resumeEntryNode).Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(379));
            var actualLines = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in manifest.entries)
            {
                Assert.That(project.Program.Nodes.TryGetValue(entry.resumeEntryNode, out var node), Is.True, entry.runtimeLineId);
                Assert.That(node.Instructions.Count, Is.GreaterThan(0), entry.runtimeLineId);
                Assert.That(node.Instructions[0].RunLine, Is.Not.Null, entry.runtimeLineId);
                Assert.That(node.Instructions[0].RunLine.LineID, Is.EqualTo(entry.runtimeLineId));
                var transfers = node.Instructions.Where(i => i.RunNode != null).ToArray();
                Assert.That(transfers, Has.Length.EqualTo(1), entry.runtimeLineId);
                Assert.That(transfers[0].RunNode.NodeName, Is.EqualTo(entry.successorNodeOrFlow), entry.runtimeLineId);
                Assert.That(project.Program.Nodes.ContainsKey(entry.successorNodeOrFlow), Is.True, entry.runtimeLineId);
                Assert.That(actualLines.Add(node.Instructions[0].RunLine.LineID), Is.True);
                Assert.That(project.Program.Nodes.ContainsKey(entry.sourceNode), Is.True, entry.runtimeLineId);
                var valid = VNProductionLineResume.TryResolve(entry.runtimeLineId, project, out var resolved, out var fingerprint, out var diagnostic);
                if (entry.eligibility == "SAVE_ELIGIBLE")
                {
                    Assert.That(valid, Is.True, entry.runtimeLineId + ": " + diagnostic);
                    Assert.That(resolved, Is.EqualTo(entry.resumeEntryNode));
                    Assert.That(fingerprint, Has.Length.EqualTo(64));
                }
                else
                {
                    Assert.That(entry.eligibility, Is.EqualTo("SAVE_BLOCKED_TRANSIENT"));
                    Assert.That(entry.restrictionReason, Is.Not.Empty);
                    Assert.That(valid, Is.False, entry.runtimeLineId);
                }
            }
            Assert.That(manifest.entries.Count(e => e.eligibility == "SAVE_ELIGIBLE"), Is.EqualTo(367));
            Assert.That(VNProductionLineResume.TryGetPresentationContext("line:prologue_s007_008", out var information), Is.True);
            Assert.That(information.Title, Is.EqualTo("최근 생활 기록"));
            Assert.That(information.Facts, Is.EqualTo("수면\n식사"));
            Assert.That(VNProductionLineResume.TryGetPresentationContext("line:prologue_s007_028", out var diagram), Is.True);
            Assert.That(diagram.Diagram, Is.True);
            Assert.That(VNProductionLineResume.TryGetPresentationContext("line:prologue_s007_038", out var afterDiagram), Is.True);
            Assert.That(afterDiagram.Diagram, Is.False);
            Assert.That(VNProductionLineResume.TryGetPresentationContext("line:prologue_s008_001", out var briefing), Is.True);
            Assert.That(briefing.Title, Is.EqualTo("며칠 뒤"));
            Assert.That(briefing.Facts, Is.EqualTo("재평가 완료\n접속 승인"));
            Assert.That(VNProductionLineResume.TryGetPresentationContext("line:prologue_s008_017", out var afterProfiles), Is.True);
            Assert.That(afterProfiles.Facts, Is.Empty);
        }
    }
}
