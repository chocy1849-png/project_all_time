using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Yarn.Unity;
using ProjectAllTime.VN.SaveLoad;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class VNPrologueFinalTests
    {
        private const string YarnPath = "Assets/_Project/Yarn/PROLOGUE_S007_S008.yarn";
        private static string Normalize(string text) => Regex.Replace(text, @"\s+", " ").Trim();

        [Test]
        public void CanonicalParagraphs_MatchTextAndSpeaker()
        {
            var source = File.ReadAllText("Docs/Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt");
            source = source.Substring(source.IndexOf("## S007.", StringComparison.Ordinal));
            source = source.Substring(0, source.IndexOf("**1부 — 깨우는 사람 / 끝**", StringComparison.Ordinal));
            var expected = Regex.Matches(source, @"\*\*(?<speaker>[^\r\n*]+)\*\*\r?\n(?<text>(?:(?!\r?\n\r?\n).)+)", RegexOptions.Singleline)
                .Cast<Match>().Where(m => !m.Groups["speaker"].Value.StartsWith("프로필", StringComparison.Ordinal) && m.Groups["speaker"].Value != "진행 안내")
                .Select(m => (m.Groups["speaker"].Value == "서술" ? "" : m.Groups["speaker"].Value + ": ") + Normalize(m.Groups["text"].Value)).ToArray();
            var actual = File.ReadAllLines(YarnPath).Where(l => l.Contains("#line:")).Select(l => Normalize(l.Substring(0, l.IndexOf("#line:", StringComparison.Ordinal)))).ToArray();
            CollectionAssert.AreEqual(expected, actual);
            Assert.That(actual.Length, Is.EqualTo(96));
        }

        [Test]
        public void BeatsAndEnding_ArePresentWithoutChoicesOrUnlocks()
        {
            var yarn = File.ReadAllText(YarnPath);
            var nodes = Regex.Matches(yarn, @"(?m)^title: (\w+)").Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            for (int scene = 7; scene <= 8; scene++)
                for (int beat = 1; beat <= (scene == 7 ? 11 : 14); beat++)
                    Assert.That(nodes.Count(n => n == $"PROLOGUE_S00{scene}_P{beat:00}"), Is.EqualTo(1));
            foreach (Match jump in Regex.Matches(yarn, @"<<jump (\w+)>>")) Assert.That(nodes, Does.Contain(jump.Groups[1].Value));
            Assert.That(Regex.IsMatch(yarn, @"(?m)^\s*->|<<vn_complete_ending|<<vn_unlock_|<<vn_checkpoint"), Is.False);
            var cues = new[] { "vn_fade_to_black 0.8", "sfx_connection_rain_handoff", "sfx_raindrop_awning", "sfx_old_bus_brake", "vn_part_end_title" };
            var offset = -1;
            foreach (var cue in cues) { var next = yarn.IndexOf(cue, StringComparison.Ordinal); Assert.That(next, Is.GreaterThan(offset), cue); offset = next; }
            foreach (var cue in cues.Skip(1).Take(3)) Assert.That(Regex.Matches(yarn, cue).Count, Is.EqualTo(1));
        }

        [Test]
        public void ProductionCatalogs_ResolveNewIds()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>("Assets/_Project/Settings/Presentation/Production/M10_ProductionPresentationCatalog.asset");
            var audio = AssetDatabase.LoadAssetAtPath<VNAudioCatalog>("Assets/_Project/Audio/M10_ProductionAudioCatalog.asset");
            foreach (var id in new[] { "bg_dokyung_office", "bg_immersion_preparation" }) Assert.That(catalog.TryGetBackground(id, out _), Is.True, id);
            foreach (var id in new[] { "dokyung", "eunjung" }) { Assert.That(catalog.TryGetCharacter(id, out var character), Is.True, id); Assert.That(character.TryGetExpression("default", out _), Is.True); }
            foreach (var id in new[] { "sfx_connection_rain_handoff", "sfx_raindrop_awning", "sfx_old_bus_brake" }) Assert.That(audio.TryGetSfx(id, out _), Is.True, id);
        }

        [Test]
        public void ProductionFont_CoversNewYarn()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M10_Prologue_Static_Korean.asset");
            foreach (var character in File.ReadAllText(YarnPath).Where(c => !char.IsControl(c)).Distinct())
                Assert.That(font.HasCharacter(character), Is.True, "Missing glyph " + character);
        }

        [Test]
        public void Panels_DiagramLoadAndTitleLifecycle_AreTransient()
        {
            var root = new GameObject("Panels test"); root.SetActive(false);
            try
            {
                var runner = root.AddComponent<DialogueRunner>();
                runner.onDialogueStart = new UnityEvent(); runner.onDialogueComplete = new UnityEvent();
                var save = root.AddComponent<VNSaveLoadController>();
                var panels = root.AddComponent<VNProloguePanels>();
                var diagram = new GameObject("diagram").AddComponent<CanvasGroup>(); diagram.transform.SetParent(root.transform);
                var title = new GameObject("title").AddComponent<CanvasGroup>(); title.transform.SetParent(root.transform);
                void Set(string field, object value) => typeof(VNProloguePanels).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(panels, value);
                void Call(string method) => typeof(VNProloguePanels).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(panels, null);
                Set("dialogueRunner", runner); Set("saveLoad", save); Set("diagram", diagram); Set("endTitle", title);
                Call("OnEnable"); Call("ShowDiagram"); Assert.That(diagram.alpha, Is.EqualTo(1));
                typeof(VNSaveLoadController).GetMethod("SetLoadInProgress", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(save, new object[] { true });
                Assert.That(diagram.alpha, Is.Zero);
                Call("ShowTitle"); runner.onDialogueComplete.Invoke(); Assert.That(title.alpha, Is.EqualTo(1));
                runner.onDialogueStart.Invoke(); Assert.That(title.alpha, Is.Zero);
                Call("ShowTitle"); Call("OnDisable"); Assert.That(title.alpha, Is.Zero);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
