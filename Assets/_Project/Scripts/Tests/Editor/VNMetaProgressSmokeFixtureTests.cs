using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Yarn.Compiler;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNMetaProgressSmokeFixtureTests
    {
        private const string SmokeAssetPath = "Assets/_Project/Yarn/M8_META_PROGRESS_SMOKE.yarn";
        private const string YarnProjectAssetPath = "Assets/_Project/Yarn/GameNarrative.yarnproject";
        private const string SmokeNodeName = "M8_META_PROGRESS_START";

        private static readonly string[] ExpectedLineIds =
        {
            "m8_meta_read_01",
            "m8_meta_first_pass_01",
            "m8_meta_complete_01",
        };

        private static readonly (string Command, string Id)[] ExpectedCommands =
        {
            ("vn_unlock_cg", "m8_smoke_cg_01"),
            ("vn_unlock_chapter", "m8_smoke_chapter_01"),
            ("vn_unlock_archive", "m8_smoke_archive_01"),
            ("vn_unlock_achievement", "m8_smoke_achievement_01"),
            ("vn_complete_ending", "m8_smoke_ending_01"),
        };

        [Test]
        public void GameNarrative_CompilesWithOnlyExplicitUniqueLineIds()
        {
            var result = CompileGameNarrative();
            var errors = result.Diagnostics.Where(d => d.Severity == Diagnostic.DiagnosticSeverity.Error).ToArray();

            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors.Select(e => e.ToString())));
            Assert.That(result.Program, Is.Not.Null, "Yarn Spinner must produce a complete compiled program.");
            Assert.That(result.ContainsImplicitStringTags, Is.False, "Every line and option must have an explicit stable #line ID.");

            var entries = result.StringTable.ToArray();
            Assert.That(entries.Count(entry => entry.Value.isImplicitTag), Is.Zero, "String table entries must not use implicit IDs.");
            Assert.That(entries.Select(entry => entry.Key).Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(entries.Length), "Compiled line IDs must be unique.");
            Assert.That(entries.Length, Is.EqualTo(138), "The M8-02 inventory of 135 plus three smoke lines should total 138.");
        }

        [Test]
        public void SmokeFixture_CompilesAndMatchesTheTechnicalContract()
        {
            var result = CompileGameNarrative();
            var errors = result.Diagnostics.Where(d => d.Severity == Diagnostic.DiagnosticSeverity.Error).ToArray();
            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors.Select(e => e.ToString())));

            Assert.That(result.Program.Nodes.ContainsKey(SmokeNodeName), Is.True, "The full compiled GameNarrative program must contain the smoke node.");

            var source = File.ReadAllText(Path.GetFullPath(SmokeAssetPath));
            Assert.That(Regex.Matches(source, @"(?m)^\s*title:\s*" + SmokeNodeName + @"\s*$").Count, Is.EqualTo(1));
            foreach (var (command, id) in ExpectedCommands)
            {
                Assert.That(Regex.Matches(source, @"<<\s*" + Regex.Escape(command) + @"\s+" + Regex.Escape(id) + @"\s*>>").Count,
                    Is.EqualTo(2), $"{command} must submit {id} in both passes.");
            }

            var allCommandNames = Regex.Matches(source, @"<<\s*([A-Za-z_][A-Za-z0-9_]*)\b")
                .Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .ToArray();
            CollectionAssert.AreEquivalent(ExpectedCommands.SelectMany(command => Enumerable.Repeat(command.Command, 2)), allCommandNames);

            foreach (var lineId in ExpectedLineIds)
                Assert.That(Regex.Matches(source, @"#line:" + Regex.Escape(lineId) + @"\b").Count, Is.EqualTo(1), $"Expected exactly one {lineId} line tag.");

            Assert.That(Regex.Matches(source, @"#line:[A-Za-z0-9_]+\b").Count, Is.EqualTo(ExpectedLineIds.Length), "The fixture must contain only its three explicit line IDs.");
            Assert.That(Regex.Matches(source, @"(?m)^\s*->").Count, Is.Zero, "The fixture must not introduce options.");
            Assert.That(Regex.IsMatch(source, @"<<\s*(?:vn_cg|vn_bg|bgm_[A-Za-z0-9_]*|sfx_[A-Za-z0-9_]*|vn_checkpoint)\b"), Is.False,
                "The fixture must not issue presentation, audio, or checkpoint commands.");
            Assert.That(source, Does.Contain("TECHNICAL / NON-CANON"));
        }

        private static CompilationResult CompileGameNarrative()
        {
            var projectPath = Path.GetFullPath(YarnProjectAssetPath);
            var project = Yarn.Compiler.Project.LoadFromFile(projectPath, Directory.GetCurrentDirectory());
            var job = CompilationJob.CreateFromFiles(project.SourceFiles);
            job.LanguageVersion = project.FileVersion;
            return Yarn.Compiler.Compiler.Compile(job);
        }
    }
}
