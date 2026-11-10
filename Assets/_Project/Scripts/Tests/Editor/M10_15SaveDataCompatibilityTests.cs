using System;
using System.IO;
using System.Linq;
using Google.Protobuf;
using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using Yarn.Compiler;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M10_15SaveDataCompatibilityTests
    {
        private const string Line = "line:prologue_s005_048";
        private const string Canonical = "cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9";
        private const string CrLf = "9aefad0924fffae848c25a23081b48606a0cfd2c51d637bb6da4386a65883980";

        [TestCase(Canonical, VNResumeCompatibilityStatus.ExactMatch, true)]
        [TestCase(CrLf, VNResumeCompatibilityStatus.LegacyNewlineEquivalent, true)]
        [TestCase("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff", VNResumeCompatibilityStatus.Incompatible, false)]
        public void GraphIdentity_IsClassifiedWithoutWhitelistingUnknownGraphs(string fingerprint,
            VNResumeCompatibilityStatus expected, bool compatible)
        {
            var project = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            var result = VNResumeCompatibilityRegistry.Evaluate(Line, project, fingerprint);
            Assert.That(result.Status, Is.EqualTo(expected));
            Assert.That(result.IsCompatible, Is.EqualTo(compatible));
            if (compatible) Assert.That(result.ResumeNode, Is.EqualTo("PROLOGUE_LINE_S005_048"));
            else Assert.That(result.Diagnostic, Is.Not.Empty);
        }

        [Test]
        public void RecognizedFingerprint_DoesNotAuthorizeUnknownLine()
        {
            var project = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            var result = VNResumeCompatibilityRegistry.Evaluate("line:unknown", project, Canonical);
            Assert.That(result.Status, Is.EqualTo(VNResumeCompatibilityStatus.InvalidMetadata));
            Assert.That(result.IsCompatible, Is.False);
        }

        [Test]
        public void AddedCompiledNode_IsAcceptedOnlyWhileOriginalContractsRemainUnchanged()
        {
            var source = Path.Combine(Path.GetTempPath(), "m10_15_" + Guid.NewGuid().ToString("N") + ".yarn");
            YarnProject expandedProject = null;
            YarnProject alteredProject = null;
            TextAsset expandedManifest = null;
            TextAsset alteredManifest = null;
            GameObject storageObject = null;
            Localization expandedLocalization = null;
            Localization changedLocalization = null;
            try
            {
                File.WriteAllText(source, "title: M10_15_SOURCE\n---\n<<declare $m1015_new = 42>>\n<<jump M10_15_RESUME>>\n===\n\n" +
                    "title: M10_15_RESUME\n---\nTest fixture. #line:m1015_test_only\n<<jump M10_15_END>>\n===\n\n" +
                    "title: M10_15_END\n---\nTest end. #line:m1015_test_end\n===\n");
                var projectPath = Path.GetFullPath("Assets/_Project/Yarn/GameNarrative.yarnproject");
                var sourceProject = Yarn.Compiler.Project.LoadFromFile(projectPath, Directory.GetCurrentDirectory());
                var job = CompilationJob.CreateFromFiles(sourceProject.SourceFiles.Concat(new[] { source }).ToArray());
                job.LanguageVersion = sourceProject.FileVersion;
                var result = Yarn.Compiler.Compiler.Compile(job);
                Assert.That(result.Diagnostics.Where(d => d.Severity == Diagnostic.DiagnosticSeverity.Error), Is.Empty);
                var original = AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
                var program = result.Program;
                expandedProject = ScriptableObject.CreateInstance<YarnProject>();
                expandedProject.compiledYarnProgram = program.ToByteArray();
                expandedLocalization = UnityEngine.Object.Instantiate(original.baseLocalization);
                expandedLocalization.AddLocalizedString("line:m1015_test_only", "Test fixture.");
                expandedLocalization.AddLocalizedString("line:m1015_test_end", "Test end.");
                expandedProject.baseLocalization = expandedLocalization;
                var originalManifest = Resources.Load<TextAsset>("M10_09_LINE_RESUME_MANIFEST").text;
                var end = originalManifest.LastIndexOf(']');
                Assert.That(end, Is.GreaterThan(0));
                expandedManifest = new TextAsset(originalManifest.Insert(end,
                    ", {\"runtimeLineId\":\"line:m1015_test_only\",\"sourceYarnFile\":\"fixture.yarn\"," +
                    "\"sourceNode\":\"M10_15_SOURCE\",\"resumeEntryNode\":\"M10_15_RESUME\"," +
                    "\"scene\":\"TEST_SCENE\",\"presentationBeat\":\"M10_15_SOURCE\"," +
                    "\"eligibility\":\"SAVE_ELIGIBLE\",\"restrictionReason\":\"\"," +
                    "\"successorNodeOrFlow\":\"M10_15_END\",\"chapterId\":\"test_chapter\",\"sceneTitle\":\"Test scene\"}"));
                Assert.That(expandedProject.compiledYarnProgram, Is.Not.EqualTo(original.compiledYarnProgram));
                var accepted = VNResumeCompatibilityRegistry.Evaluate(Line, expandedProject, Canonical, expandedManifest);
                Assert.That(accepted.Status, Is.EqualTo(VNResumeCompatibilityStatus.ApprovedBackwardCompatible), accepted.Diagnostic);
                Assert.That(VNProductionLineResume.TryResolve("line:m1015_test_only", expandedProject,
                    out var newNode, out var newFingerprint, out var diagnostic, expandedManifest), Is.True, diagnostic);
                Assert.That(newNode, Is.EqualTo("M10_15_RESUME"));
                Assert.That(newFingerprint, Is.Not.EqualTo(Canonical));
                Assert.That(VNProductionLineResume.TryGetSaveMetadata("line:m1015_test_only", expandedProject,
                    out var chapter, out var title, out diagnostic, expandedManifest), Is.True, diagnostic);
                Assert.That(chapter, Is.EqualTo("test_chapter"));
                Assert.That(title, Is.EqualTo("Test scene"));
                Assert.That(VNResumeCompatibilityRegistry.Evaluate("line:m1015_test_only", expandedProject,
                    newFingerprint, expandedManifest).Status, Is.EqualTo(VNResumeCompatibilityStatus.ExactMatch));

                var snapshot = new YarnVariablesData
                {
                    floats = new[] { new FloatVariableEntry { name = "$m1_trust", value = 3f } },
                    strings = new[] { new StringVariableEntry { name = "$m1_selected_path", value = "kind" } },
                    bools = new[] { new BoolVariableEntry { name = "$m1_memory_seen", value = true } },
                };
                Assert.That(VNYarnVariableSnapshot.TryValidateAgainstProgram(snapshot, expandedProject, out diagnostic), Is.True, diagnostic);
                storageObject = new GameObject("M10-15 variable fixture");
                var storage = storageObject.AddComponent<InMemoryVariableStorage>();
                storage.Program = expandedProject.Program;
                for (var attempt = 0; attempt < 2; attempt++)
                {
                    storage.SetValue("$m1015_new", 99f);
                    Assert.That(VNYarnVariableSnapshot.TryRestore(storage, snapshot, out diagnostic), Is.True, diagnostic);
                    Assert.That(storage.TryGetValue<float>("$m1015_new", out var defaultValue), Is.True);
                    Assert.That(defaultValue, Is.EqualTo(42f));
                    Assert.That(storage.TryGetValue<float>("$m1_trust", out var trust), Is.True);
                    Assert.That(trust, Is.EqualTo(3f));
                }
                snapshot.floats[0].name = "$m1_selected_path";
                Assert.That(VNYarnVariableSnapshot.TryValidateAgainstProgram(snapshot, expandedProject, out _), Is.False);
                snapshot.floats[0].name = "$unknown_variable";
                Assert.That(VNYarnVariableSnapshot.TryValidateAgainstProgram(snapshot, expandedProject, out _), Is.False);

                program.Nodes["PROLOGUE_LINE_S005_049"].Instructions[0].RunLine.LineID = "line:changed_test_contract";
                alteredProject = ScriptableObject.CreateInstance<YarnProject>();
                alteredProject.compiledYarnProgram = program.ToByteArray();
                alteredProject.baseLocalization = expandedLocalization;
                var rejected = VNResumeCompatibilityRegistry.Evaluate(Line, alteredProject, Canonical, expandedManifest);
                Assert.That(rejected.IsCompatible, Is.False);
                Assert.That(rejected.Diagnostic, Is.Not.Empty);

                UnityEngine.Object.DestroyImmediate(alteredProject);
                program = Yarn.Program.Parser.ParseFrom(expandedProject.compiledYarnProgram);
                program.Nodes["PROLOGUE_S005_P01"].Instructions.Clear();
                alteredProject = ScriptableObject.CreateInstance<YarnProject>();
                alteredProject.compiledYarnProgram = program.ToByteArray();
                alteredProject.baseLocalization = expandedLocalization;
                rejected = VNResumeCompatibilityRegistry.Evaluate(Line, alteredProject, Canonical, expandedManifest);
                Assert.That(rejected.Status, Is.EqualTo(VNResumeCompatibilityStatus.MigrationRequired));

                UnityEngine.Object.DestroyImmediate(alteredProject);
                program = Yarn.Program.Parser.ParseFrom(expandedProject.compiledYarnProgram);
                var command = program.Nodes["PROLOGUE_S005_P01"].Instructions.First(i => i.RunCommand != null).RunCommand;
                command.CommandText += " altered";
                alteredProject = ScriptableObject.CreateInstance<YarnProject>();
                alteredProject.compiledYarnProgram = program.ToByteArray();
                alteredProject.baseLocalization = expandedLocalization;
                rejected = VNResumeCompatibilityRegistry.Evaluate(Line, alteredProject, Canonical, expandedManifest);
                Assert.That(rejected.Status, Is.EqualTo(VNResumeCompatibilityStatus.MigrationRequired));

                UnityEngine.Object.DestroyImmediate(alteredProject);
                program = Yarn.Program.Parser.ParseFrom(expandedProject.compiledYarnProgram);
                program.Nodes["PROLOGUE_LINE_S005_048"].Instructions.First(i => i.RunNode != null).RunNode.NodeName = "PROLOGUE_LINE_S005_050";
                alteredProject = ScriptableObject.CreateInstance<YarnProject>();
                alteredProject.compiledYarnProgram = program.ToByteArray();
                alteredProject.baseLocalization = expandedLocalization;
                rejected = VNResumeCompatibilityRegistry.Evaluate(Line, alteredProject, Canonical, expandedManifest);
                Assert.That(rejected.IsCompatible, Is.False);

                UnityEngine.Object.DestroyImmediate(alteredProject);
                program = Yarn.Program.Parser.ParseFrom(expandedProject.compiledYarnProgram);
                program.InitialValues["$m1_trust"].StringValue = "changed type";
                alteredProject = ScriptableObject.CreateInstance<YarnProject>();
                alteredProject.compiledYarnProgram = program.ToByteArray();
                alteredProject.baseLocalization = expandedLocalization;
                rejected = VNResumeCompatibilityRegistry.Evaluate(Line, alteredProject, Canonical, expandedManifest);
                Assert.That(rejected.Status, Is.EqualTo(VNResumeCompatibilityStatus.MigrationRequired));

                alteredManifest = new TextAsset(expandedManifest.text.Replace(
                    "\"restrictionReason\": \"profile browser review thought\"",
                    "\"restrictionReason\": \"changed restriction\""));
                Assert.That(alteredManifest.text, Is.Not.EqualTo(expandedManifest.text));
                rejected = VNResumeCompatibilityRegistry.Evaluate(Line, expandedProject, Canonical, alteredManifest);
                Assert.That(rejected.IsCompatible, Is.False);
                UnityEngine.Object.DestroyImmediate(alteredManifest);
                alteredManifest = new TextAsset(expandedManifest.text.Replace("최근 생활 기록", "changed information"));
                Assert.That(alteredManifest.text, Is.Not.EqualTo(expandedManifest.text));
                rejected = VNResumeCompatibilityRegistry.Evaluate(Line, expandedProject, Canonical, alteredManifest);
                Assert.That(rejected.IsCompatible, Is.False);

                UnityEngine.Object.DestroyImmediate(alteredProject);
                changedLocalization = UnityEngine.Object.Instantiate(original.baseLocalization);
                changedLocalization.AddLocalizedString("line:m1015_test_only", "Test fixture.");
                changedLocalization.AddLocalizedString("line:m1015_test_end", "Test end.");
                changedLocalization.AddLocalizedString(Line, "Changed historical dialogue.");
                alteredProject = ScriptableObject.CreateInstance<YarnProject>();
                alteredProject.compiledYarnProgram = expandedProject.compiledYarnProgram;
                alteredProject.baseLocalization = changedLocalization;
                rejected = VNResumeCompatibilityRegistry.Evaluate(Line, alteredProject, Canonical, expandedManifest);
                Assert.That(rejected.Status, Is.EqualTo(VNResumeCompatibilityStatus.MigrationRequired));
                rejected = VNResumeCompatibilityRegistry.Evaluate("line:m1015_test_only", alteredProject, newFingerprint, expandedManifest);
                Assert.That(rejected.IsCompatible, Is.False, "A text rewrite must change the current graph identity.");
            }
            finally
            {
                if (expandedProject != null) UnityEngine.Object.DestroyImmediate(expandedProject);
                if (alteredProject != null) UnityEngine.Object.DestroyImmediate(alteredProject);
                if (expandedManifest != null) UnityEngine.Object.DestroyImmediate(expandedManifest);
                if (alteredManifest != null) UnityEngine.Object.DestroyImmediate(alteredManifest);
                if (expandedLocalization != null) UnityEngine.Object.DestroyImmediate(expandedLocalization);
                if (changedLocalization != null) UnityEngine.Object.DestroyImmediate(changedLocalization);
                if (storageObject != null) UnityEngine.Object.DestroyImmediate(storageObject);
                if (File.Exists(source)) File.Delete(source);
            }
        }
    }
}
