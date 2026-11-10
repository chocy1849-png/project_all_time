using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
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
    }
}
