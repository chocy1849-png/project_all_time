using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class VNPrologueSliceTests
    {
        private const string YarnPath = "Assets/_Project/Yarn/PROLOGUE_S001_S003.yarn";
        private const string CatalogPath = "Assets/_Project/Settings/Presentation/Production/M10_ProductionPresentationCatalog.asset";
        private static string Normalize(string value) => Regex.Replace(value, @"\s+", " ").Trim();

        [Test]
        public void CanonicalParagraphs_ExactOrderedSpeakerAndText_NoDirectionsOrAddedLines()
        {
            var source = File.ReadAllText("Docs/Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt");
            source = source.Substring(source.IndexOf("## S001.", StringComparison.Ordinal));
            source = source.Substring(0, source.IndexOf("## S004.", StringComparison.Ordinal));
            var expected = new List<string>();
            foreach (Match block in Regex.Matches(source, @"\*\*(?<speaker>[^\r\n*]+)\*\*\r?\n(?<text>(?:(?!\r?\n\r?\n).)+)", RegexOptions.Singleline))
            {
                var speaker = block.Groups["speaker"].Value;
                if (speaker == "접속 정보") continue;
                expected.Add((speaker == "서술" ? "" : speaker + ": ") + Normalize(block.Groups["text"].Value));
            }
            var actual = File.ReadAllLines(YarnPath).Where(line => line.Contains("#line:")).Select(line => Normalize(line.Substring(0, line.IndexOf("#line:", StringComparison.Ordinal)))).ToArray();
            CollectionAssert.AreEqual(expected, actual);
            Assert.That(actual.Length, Is.EqualTo(111));
        }

        [Test]
        public void Nodes_CoverAll33Beats_AndLinkToS004()
        {
            var yarn = File.ReadAllText(YarnPath);
            var nodes = Regex.Matches(yarn, @"(?m)^title: (\w+)").Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            Assert.That(nodes.Length, Is.EqualTo(145)); // Start + 33 beats + 111 line entries.
            for (var scene = 1; scene <= 3; scene++)
                Assert.That(nodes.Count(n => n.StartsWith($"PROLOGUE_LINE_S00{scene}_", StringComparison.Ordinal)),
                    Is.EqualTo(Regex.Matches(yarn, $"#line:prologue_s00{scene}_").Count));
            Assert.That(nodes.Count(n => n == "PROLOGUE_LINE_S001_016"), Is.EqualTo(1));
            for (var scene = 1; scene <= 3; scene++)
                for (var beat = 1; beat <= (scene == 1 ? 13 : 10); beat++)
                    Assert.That(nodes.Count(n => n == $"PROLOGUE_S00{scene}_P{beat:00}"), Is.EqualTo(1));
            foreach (Match jump in Regex.Matches(yarn, @"<<jump (\w+)>>"))
                Assert.That(nodes.Contains(jump.Groups[1].Value) || jump.Groups[1].Value == "PROLOGUE_S004_P01", Is.True);
            Assert.That(yarn, Does.Contain("<<jump PROLOGUE_S004_P01>>"));
            Assert.That(Regex.IsMatch(yarn, @"(?m)^\s*->|<<vn_complete_ending|<<vn_unlock_|<<vn_checkpoint"), Is.False);
        }

        [Test]
        public void CueOrder_NoS002RestartOrGain_FirstInformationAfterIdentity_IdenticalLaughTwice()
        {
            var text = File.ReadAllText(YarnPath);
            Assert.That(Regex.Matches(text, "<<bgm_play bgm_ordinary_morning>>").Count, Is.EqualTo(1));
            Assert.That(Regex.Matches(text, "<<sfx_play sfx_junho_laugh>>").Count, Is.EqualTo(2));
            Assert.That(Regex.Matches(text, "<<sfx_play sfx_chair_leg_scrape>>").Count, Is.EqualTo(1));
            Assert.That(text.IndexOf("bgm_stop 0.6", StringComparison.Ordinal), Is.LessThan(text.IndexOf("로프·내부:", StringComparison.Ordinal)));
            Assert.That(text.IndexOf("지훈. 심층의식 치료사.", StringComparison.Ordinal), Is.LessThan(text.IndexOf("vn_info_show \"접속 정보\"", StringComparison.Ordinal)));
            Assert.That(text.IndexOf("vn_info_show \"접속 정보\"", StringComparison.Ordinal), Is.LessThan(text.IndexOf("지훈·독백:", StringComparison.Ordinal)));
        }

        [Test]
        public void ProductionCatalogs_AllAuthoredLookupsResolve_AndHaveRealImportedResources()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(CatalogPath);
            var audio = AssetDatabase.LoadAssetAtPath<VNAudioCatalog>("Assets/_Project/Audio/M10_ProductionAudioCatalog.asset");
            foreach (Match command in Regex.Matches(File.ReadAllText(YarnPath), @"<<(vn_bg_crossfade|vn_cg|bgm_play|sfx_play) (\w+)(?: 0)?>>"))
            {
                var id = command.Groups[2].Value;
                switch (command.Groups[1].Value)
                {
                    case "vn_bg_crossfade": Assert.That(catalog.TryGetBackground(id, out _), Is.True, id); break;
                    case "vn_cg": Assert.That(catalog.TryGetCG(id, out _), Is.True, id); break;
                    case "bgm_play": Assert.That(audio.TryGetBgm(id, out var bgm), Is.True, id); Assert.That(bgm.Clip.samples, Is.GreaterThan(0)); break;
                    case "sfx_play": Assert.That(audio.TryGetSfx(id, out var sfx), Is.True, id); Assert.That(sfx.Clip.samples, Is.GreaterThan(0)); break;
                }
            }
            foreach (var id in new[] { "jihun", "jinhee", "minseok", "narae", "junho", "rope_memory" }) Assert.That(catalog.TryGetCharacter(id, out _), Is.True, id);
            foreach (var alias in new[] { "지훈", "지훈·독백", "지훈·내부" }) { Assert.That(catalog.TryResolveSpeakerAlias(alias, out var id), Is.True); Assert.That(id, Is.EqualTo("jihun")); }
            Assert.That(catalog.TryResolveSpeakerAlias("로프·내부", out var dog), Is.True); Assert.That(dog, Is.EqualTo("rope_memory"));
            Assert.That(catalog.TryResolveSpeakerAlias("로프", out var actualDog), Is.True);
            Assert.That(actualDog, Is.EqualTo("rope"));
            Assert.That(catalog.TryGetBackground("m3_bg_a", out _), Is.False, "Legacy technical lookups fail closed.");
            Assert.That(audio.TryGetBgm("m4_bgm_a", out _), Is.False);
        }

        [Test]
        public void Scene_AllFiveCatalogReferencesMatch_ProductionStartUnchanged()
        {
            var scene = File.ReadAllText("Assets/_Project/Scenes/VN_Main.unity");
            Assert.That(Regex.Matches(scene, AssetDatabase.AssetPathToGUID(CatalogPath)).Count, Is.EqualTo(4));
            Assert.That(Regex.Matches(scene, AssetDatabase.AssetPathToGUID("Assets/_Project/Audio/M10_ProductionAudioCatalog.asset")).Count, Is.EqualTo(1));
            Assert.That(Regex.IsMatch(scene, @"propertyPath: startNode\s+value: M2_UI_START"), Is.True);
        }

        [Test]
        public void InformationCommand_ExactFacts_NoHistory_AndDisableClears()
        {
            var go = new GameObject("Technical command test");
            try
            {
                var root = go.AddComponent<CanvasGroup>();
                var overlay = go.AddComponent<VNInformationOverlay>();
                var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)); title.transform.SetParent(go.transform);
                var body = new GameObject("Facts", typeof(RectTransform), typeof(TextMeshProUGUI)); body.transform.SetParent(go.transform);
                var save = go.AddComponent<VNSaveLoadController>(); var visibility = go.AddComponent<VNUIVisibilityController>();
                Set(overlay, "root", root); Set(overlay, "titleText", title.GetComponent<TMP_Text>()); Set(overlay, "bodyText", body.GetComponent<TMP_Text>()); Set(overlay, "saveLoad", save); Set(overlay, "visibility", visibility);
                var command = go.AddComponent<VNYarnInformationCommands>(); Set(command, "overlay", overlay); Set(command, "saveLoad", save);
                var routine = (IEnumerator)Call(command, "Show", "접속 정보", "대상: 진희", "방문: 3회차", "신원 응답: 지연");
                Assert.That(routine.MoveNext(), Is.False);
                Assert.That(body.GetComponent<TMP_Text>().text, Is.EqualTo("대상: 진희\n방문: 3회차\n신원 응답: 지연"));
                Assert.That(root.blocksRaycasts, Is.False);
                Call(command, "OnDisable"); Assert.That(overlay.IsShown, Is.False);
                LogAssert.Expect(LogType.Error, "vn_info_show requires an overlay, SaveLoad and a nonempty first fact.");
                Assert.That(((IEnumerator)Call(command, "Show", "", "", "", "")).MoveNext(), Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void StaticFont_CoversEveryProductionGlyphWithoutRuntimeAtlasMutation()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M10_Prologue_Static_Korean.asset");
            Assert.That(font.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static));
            foreach (var character in File.ReadAllText(YarnPath).Where(c => !char.IsControl(c)).Distinct())
                Assert.That(font.HasCharacter(character), Is.True, "Missing glyph " + character);
        }

        [Test]
        public void InformationCommand_MissingReferencesDiagnosed()
        {
            var go = new GameObject("Missing command wiring");
            try
            {
                var command = go.AddComponent<VNYarnInformationCommands>();
                LogAssert.Expect(LogType.Error, "VNYarnInformationCommands requires production runner, overlay and SaveLoad references.");
                Call(command, "OnEnable");
                LogAssert.Expect(LogType.Error, "vn_info_show requires an overlay, SaveLoad and a nonempty first fact.");
                Assert.That(((IEnumerator)Call(command, "Show", "Title", "Fact", "", "")).MoveNext(), Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        private static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    }
}
