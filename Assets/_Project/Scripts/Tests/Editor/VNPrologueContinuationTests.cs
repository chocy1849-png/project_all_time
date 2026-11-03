using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Yarn.Unity;
using TMPro;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class VNPrologueContinuationTests
    {
        private const string Path = "Assets/_Project/Yarn/PROLOGUE_S004_S006.yarn";
        private static string Normalize(string value) => Regex.Replace(value, @"\s+", " ").Trim();

        [Test]
        public void CanonicalParagraphs_KeepExactOrderTextAndSpeaker()
        {
            var source = File.ReadAllText("Docs/Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt");
            source = source.Substring(source.IndexOf("## S004.", StringComparison.Ordinal));
            source = source.Substring(0, source.IndexOf("## S007.", StringComparison.Ordinal));
            var expected = Regex.Matches(source, @"\*\*(?<speaker>[^\r\n*]+)\*\*\r?\n(?<text>(?:(?!\r?\n\r?\n).)+)", RegexOptions.Singleline)
                .Cast<Match>().Select(m => (m.Groups["speaker"].Value == "서술" ? "" : m.Groups["speaker"].Value + ": ") + Normalize(m.Groups["text"].Value)).ToArray();
            var actual = File.ReadAllLines(Path).Where(line => line.Contains("#line:")).Select(line => Normalize(line.Substring(0, line.IndexOf("#line:", StringComparison.Ordinal)))).ToArray();
            CollectionAssert.AreEqual(expected, actual); Assert.That(actual.Length, Is.EqualTo(172));
        }

        [Test]
        public void NodesAndCues_KeepManualAgencyMusicAndRopeRevealOrder()
        {
            var yarn = File.ReadAllText(Path).Replace("\r\n", "\n");
            var nodes = Regex.Matches(yarn, @"(?m)^title: (\w+)").Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            Assert.That(nodes.Length, Is.EqualTo(45));
            for (var scene = 4; scene <= 6; scene++)
                for (var beat = 1; beat <= (scene == 4 ? 16 : scene == 5 ? 15 : 14); beat++)
                    Assert.That(nodes, Does.Contain($"PROLOGUE_S00{scene}_P{beat:00}"));
            foreach (Match jump in Regex.Matches(yarn, @"<<jump (\w+)>>"))
                Assert.That(nodes.Contains(jump.Groups[1].Value) || jump.Groups[1].Value == "PROLOGUE_S007_P01", Is.True);
            Assert.That(Regex.IsMatch(yarn, @"(?m)^\s*->|<<vn_complete_ending|<<vn_unlock_|<<vn_checkpoint"), Is.False);
            Assert.That(yarn, Does.Contain("<<vn_manual_hold>>\n지훈: 제가 대신"));
            Assert.That(yarn.IndexOf("<<vn_manual_hold>>", StringComparison.Ordinal), Is.LessThan(yarn.IndexOf("그녀가 문을 열었다", StringComparison.Ordinal)));
            Assert.That(yarn.IndexOf("bgm_play bgm_quiet_farewell", StringComparison.Ordinal), Is.GreaterThan(yarn.IndexOf("PROLOGUE_S005_P11\n---", StringComparison.Ordinal)));
            Assert.That(yarn.IndexOf("vn_local_effect on", StringComparison.Ordinal), Is.LessThan(yarn.IndexOf("bgm_stop 0", StringComparison.Ordinal)));
            Assert.That(yarn.IndexOf("vn_local_effect off", StringComparison.Ordinal), Is.LessThan(yarn.IndexOf("bg_jinhee_bedroom_day", StringComparison.Ordinal)));
            Assert.That(yarn.IndexOf("vn_show rope default", StringComparison.Ordinal), Is.GreaterThan(yarn.IndexOf("PROLOGUE_S006_P07\n---", StringComparison.Ordinal)));
            var assist = yarn.IndexOf("vn_info_show \"\" \"투영 반응 보조\"", StringComparison.Ordinal);
            var reaction = yarn.IndexOf("#line:prologue_s005_024", StringComparison.Ordinal);
            Assert.That(assist, Is.GreaterThanOrEqualTo(0));
            Assert.That(reaction, Is.GreaterThan(assist));
            Assert.That(yarn.IndexOf("<<vn_info_hide>>", assist, StringComparison.Ordinal), Is.GreaterThan(reaction), "Assistance disclosure remains visible for the first adjusted response.");
            Assert.That(Regex.Matches(yarn, "sfx_play sfx_slipper_toe_tap").Count, Is.EqualTo(2));
            Assert.That(yarn, Does.Contain("<<jump PROLOGUE_S007_P01>>"));
        }

        [Test]
        public void Catalogs_ResolveAllContinuationResourcesAndDistinctRopeAliases()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>("Assets/_Project/Settings/Presentation/Production/M10_ProductionPresentationCatalog.asset");
            var audio = AssetDatabase.LoadAssetAtPath<VNAudioCatalog>("Assets/_Project/Audio/M10_ProductionAudioCatalog.asset");
            foreach (var id in new[]{"bg_family_storage", "bg_family_storage_doorway", "bg_family_table_sunset", "bg_jinhee_bedroom_day", "bg_future_city_street_day"}) Assert.That(catalog.TryGetBackground(id, out _), Is.True, id);
            foreach (var id in new[]{"cg_returned_bag_contents", "cg_unused_new_shoe", "cg_present_table_glimpse"}) Assert.That(catalog.TryGetCG(id, out _), Is.True, id);
            Assert.That(catalog.TryResolveSpeakerAlias("로프·내부", out var memory), Is.True); Assert.That(memory, Is.EqualTo("rope_memory"));
            Assert.That(catalog.TryResolveSpeakerAlias("로프", out var actual), Is.True); Assert.That(actual, Is.EqualTo("rope"));
            Assert.That(audio.TryGetBgm("bgm_quiet_farewell", out var bgm), Is.True); Assert.That(bgm.Clip.samples, Is.GreaterThan(0));
            foreach(var id in new[]{"sfx_meal_cutlery", "sfx_sink_washing", "sfx_slipper_toe_tap", "sfx_low_vibration", "sfx_equipment_disconnect"}) Assert.That(audio.TryGetSfx(id, out _), Is.True, id);
        }

        [Test]
        public void ProductionFont_CoversContinuationText()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M10_Prologue_Static_Korean.asset");
            Assert.That(font.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static));
            foreach (var character in File.ReadAllText(Path).Where(c => !char.IsControl(c)).Distinct())
                Assert.That(font.HasCharacter(character), Is.True, "Missing glyph " + character);
        }

        [Test]
        public void ManualHoldAdapter_ArmsSessionAndDisableClearsTransientHold()
        {
            var go = new GameObject("Technical hold command");
            try
            {
                var session = go.AddComponent<VNDialogueSessionState>(); var runner = go.AddComponent<DialogueRunner>(); var command = go.AddComponent<VNYarnManualHoldCommands>();
                Set(command, "dialogueRunner", runner); Set(command, "sessionState", session);
                Call(command, "OnEnable"); Call(command, "Hold"); Assert.That(session.IsManualAdvanceRequired, Is.True);
                Call(command, "OnDisable"); Assert.That(session.IsManualAdvanceRequired, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void LocalEffectAdapter_InvalidInputReportsDiagnostic()
        {
            var go = new GameObject("Invalid technical effect command");
            try
            {
                var command = go.AddComponent<VNYarnTransitionCommands>();
                LogAssert.Expect(LogType.Error, "vn_local_effect requires on/off and a finite non-negative duration.");
                Assert.That(((IEnumerator)Call(command, "LocalizedEffect", "bad", 1f)).MoveNext(), Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        private static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        private static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    }
}
