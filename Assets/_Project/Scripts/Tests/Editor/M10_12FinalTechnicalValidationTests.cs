using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Timeline;
using TMPro;
using UnityEditor;

namespace ProjectAllTime.Tests.Editor
{
    /// <summary>
    /// Adds release-focused glyph coverage for persisted Records UI content.
    /// Story glyph coverage is already owned by the S001-S008 canonical tests.
    /// </summary>
    public sealed class M10_12FinalTechnicalValidationTests
    {
        [Test]
        public void ProductionRecordsVisibleText_HasStaticKoreanFontCoverage()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/_Project/Fonts/NotoSansKR/M10_Prologue_Static_Korean.asset");
            Assert.That(font, Is.Not.Null, "The production Korean font asset must be included.");
            Assert.That(font.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static),
                "Release text must not depend on runtime glyph generation.");
            var fallbacks = TMP_Settings.fallbackFontAssets;
            Assert.That(fallbacks, Is.Not.Null.And.Not.Empty, "TMP's default fallback chain must be configured.");
            Assert.That(fallbacks.Exists(fallback => fallback != null &&
                fallback.atlasPopulationMode == AtlasPopulationMode.Dynamic && fallback.sourceFontFile != null), Is.True,
                "The default Korean fallback must be able to populate missing static glyphs at runtime.");

            var corpus = new List<string>();

            var timeline = AssetDatabase.LoadAssetAtPath<VNTimelineCatalog>(
                "Assets/_Project/Settings/Records/Production/VNTimelineCatalog.asset");
            var gallery = AssetDatabase.LoadAssetAtPath<VNCGGalleryCatalog>(
                "Assets/_Project/Settings/Records/Production/VNCGGalleryCatalog.asset");
            var archive = AssetDatabase.LoadAssetAtPath<VNArchiveCatalog>(
                "Assets/_Project/Settings/Records/Production/VNArchiveCatalog.asset");
            var achievements = AssetDatabase.LoadAssetAtPath<VNAchievementCatalog>(
                "Assets/_Project/Settings/Records/Production/VNAchievementCatalog.asset");

            Assert.That(timeline, Is.Not.Null);
            Assert.That(gallery, Is.Not.Null);
            Assert.That(archive, Is.Not.Null);
            Assert.That(achievements, Is.Not.Null);

            foreach (var chapter in timeline.Chapters) corpus.Add(chapter.DisplayTitle);
            foreach (var entry in timeline.Entries) corpus.Add(entry.DisplayTitle);
            foreach (var entry in gallery.Entries) corpus.Add(entry.DisplayTitle);
            foreach (var category in archive.Categories) corpus.Add(category.DisplayTitle);
            foreach (var entry in archive.Entries)
            {
                corpus.Add(entry.DisplayTitle);
                corpus.Add(entry.Summary);
                corpus.Add(entry.Body);
            }
            foreach (var achievement in achievements.Achievements)
            {
                corpus.Add(achievement.DisplayTitle);
                corpus.Add(achievement.Description);
            }

            var unresolved = string.Concat(corpus).Distinct()
                .Where(character => !char.IsControl(character))
                .Where(character => !font.HasCharacter(character) &&
                    TMP_FontAssetUtilities.GetCharacterFromFontAssets((uint)character, font, fallbacks, true,
                        FontStyles.Normal, FontWeight.Regular, out _) == null)
                .Select(character => $"U+{(int)character:X4} ({character})").ToArray();
            Assert.That(unresolved, Is.Empty,
                "Production Records text is unresolved by the static font and its configured fallback chain: " +
                string.Join(", ", unresolved));
        }
    }
}
