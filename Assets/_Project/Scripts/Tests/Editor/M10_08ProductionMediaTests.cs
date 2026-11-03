using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Presentation;
using UnityEditor;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M10_08ProductionMediaTests
    {
        private const string PresentationPath = "Assets/_Project/Settings/Presentation/Production/M10_ProductionPresentationCatalog.asset";
        private const string AudioPath = "Assets/_Project/Audio/M10_ProductionAudioCatalog.asset";

        private static readonly string[] ExpectedCharacters =
        {
            "jihun", "jinhee", "minseok", "narae", "junho", "rope_memory", "rope", "dokyung", "eunjung"
        };

        private static readonly string[] ExpectedBackgrounds =
        {
            "bg_family_kitchen_morning", "bg_family_table_morning", "bg_family_entrance", "bg_family_living_room",
            "bg_family_hall", "bg_family_storage", "bg_jinhee_bedroom_day", "bg_future_city_street_day",
            "bg_family_storage_doorway", "bg_family_table_sunset", "bg_dokyung_office", "bg_immersion_preparation"
        };

        private static readonly string[] ExpectedCgs =
        {
            "cg_worn_child_shoe", "cg_beach_photo_child", "cg_beach_photo_sea", "cg_returned_bag_contents",
            "cg_unused_new_shoe", "cg_present_table_glimpse"
        };

        private static readonly string[] ExpectedBgm = { "bgm_ordinary_morning", "bgm_quiet_farewell" };

        private static readonly string[] ExpectedSfx =
        {
            "sfx_frying_oil", "sfx_plate_counter", "sfx_home_doorbell", "sfx_child_entry_steps",
            "sfx_chair_leg_scrape", "sfx_junho_laugh", "sfx_meal_cutlery", "sfx_sink_washing",
            "sfx_slipper_toe_tap", "sfx_low_vibration", "sfx_equipment_disconnect",
            "sfx_connection_rain_handoff", "sfx_raindrop_awning", "sfx_old_bus_brake"
        };

        private static readonly string[] ExpectedExpressionKeys =
        {
            "jihun/default", "jihun/uneasy", "jinhee/default", "jinhee/smile", "jinhee/uneasy", "jinhee/crying", "jinhee/firm",
            "minseok/default", "minseok/quiet", "narae/default", "junho/default", "junho/pout", "junho/eager",
            "rope_memory/default", "rope_memory/alert", "rope/default", "rope/alert", "dokyung/default", "eunjung/default"
        };

        [Test]
        public void ProductionCatalogs_PreserveAllFortyThreeFrozenIds()
        {
            var presentation = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(PresentationPath);
            var audio = AssetDatabase.LoadAssetAtPath<VNAudioCatalog>(AudioPath);
            Assert.That(presentation, Is.Not.Null);
            Assert.That(audio, Is.Not.Null);

            var presentationObject = new SerializedObject(presentation);
            var characterIds = Enumerable.Range(0, presentationObject.FindProperty("characterDefinitions").arraySize)
                .Select(i => (presentationObject.FindProperty("characterDefinitions").GetArrayElementAtIndex(i).objectReferenceValue as VNCharacterDefinition)?.CharacterId)
                .ToArray();
            var backgroundIds = ReadIds(presentationObject.FindProperty("backgrounds"));
            var cgIds = ReadIds(presentationObject.FindProperty("cgs"));
            var audioObject = new SerializedObject(audio);
            var bgmIds = ReadIds(audioObject.FindProperty("bgm"));
            var sfxIds = ReadIds(audioObject.FindProperty("sfx"));

            CollectionAssert.AreEquivalent(ExpectedCharacters, characterIds);
            CollectionAssert.AreEquivalent(ExpectedBackgrounds, backgroundIds);
            CollectionAssert.AreEquivalent(ExpectedCgs, cgIds);
            CollectionAssert.AreEquivalent(ExpectedBgm, bgmIds);
            CollectionAssert.AreEquivalent(ExpectedSfx, sfxIds);
            Assert.That(characterIds.Concat(backgroundIds).Concat(cgIds).Concat(bgmIds).Concat(sfxIds).Distinct().Count(), Is.EqualTo(43));
        }

        [Test]
        public void AcceptedImages_ResolveToCanonicalSpritesWithSpriteImportSettings()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(PresentationPath);
            var presentation = new SerializedObject(catalog);
            var acceptedBackgrounds = new Dictionary<string, string>
            {
                ["bg_family_kitchen_morning"] = "Assets/_Project/Art/Backgrounds/bg_family_kitchen_morning.png",
                ["bg_family_table_morning"] = "Assets/_Project/Art/Backgrounds/bg_family_table_morning.png",
                ["bg_family_entrance"] = "Assets/_Project/Art/Backgrounds/bg_family_entrance.png",
                ["bg_family_living_room"] = "Assets/_Project/Art/Backgrounds/bg_family_living_room.png",
                ["bg_family_hall"] = "Assets/_Project/Art/Backgrounds/bg_family_hall.png",
                ["bg_family_storage"] = "Assets/_Project/Art/Backgrounds/bg_family_storage.png",
                ["bg_family_storage_doorway"] = "Assets/_Project/Art/Backgrounds/bg_family_storage_doorway.png",
                ["bg_family_table_sunset"] = "Assets/_Project/Art/Backgrounds/bg_family_table_sunset.png",
                ["bg_jinhee_bedroom_day"] = "Assets/_Project/Art/Backgrounds/bg_jinhee_bedroom_day.png",
                ["bg_future_city_street_day"] = "Assets/_Project/Art/Backgrounds/bg_future_city_street_day.png",
                ["bg_dokyung_office"] = "Assets/_Project/Art/Backgrounds/bg_dokyung_office.png",
                ["bg_immersion_preparation"] = "Assets/_Project/Art/Backgrounds/bg_immersion_preparation.png",
            };
            var acceptedCgs = new Dictionary<string, string>
            {
                ["cg_worn_child_shoe"] = "Assets/_Project/Art/CG/cg_worn_child_shoe.png",
                ["cg_beach_photo_child"] = "Assets/_Project/Art/CG/cg_beach_photo_child.png",
                ["cg_beach_photo_sea"] = "Assets/_Project/Art/CG/cg_beach_photo_sea.png",
                ["cg_returned_bag_contents"] = "Assets/_Project/Art/CG/cg_returned_bag_contents.png",
                ["cg_unused_new_shoe"] = "Assets/_Project/Art/CG/cg_unused_new_shoe.png",
                ["cg_present_table_glimpse"] = "Assets/_Project/Art/CG/cg_present_table_glimpse.png",
            };
            AssertSpriteEntries(presentation.FindProperty("backgrounds"), acceptedBackgrounds);
            AssertSpriteEntries(presentation.FindProperty("cgs"), acceptedCgs);

            var definitions = presentation.FindProperty("characterDefinitions");
            var bodyPaths = new Dictionary<string, string>
            {
                ["jihun"] = "Assets/_Project/Art/Characters/jihun/jihun_default.png",
                ["jinhee"] = "Assets/_Project/Art/Characters/jinhee/jinhee_default.png",
                ["minseok"] = "Assets/_Project/Art/Characters/minseok/minseok_default.png",
                ["narae"] = "Assets/_Project/Art/Characters/narae/narae_default.png",
                ["junho"] = "Assets/_Project/Art/Characters/junho/junho_default.png",
                ["rope_memory"] = "Assets/_Project/Art/Characters/rope_memory/rope_memory_default.png",
                ["dokyung"] = "Assets/_Project/Art/Characters/dokyung/dokyung_default.png",
                ["eunjung"] = "Assets/_Project/Art/Characters/eunjung/eunjung_default.png",
            };
            for (var i = 0; i < definitions.arraySize; i++)
            {
                var definition = definitions.GetArrayElementAtIndex(i).objectReferenceValue as VNCharacterDefinition;
                if (!bodyPaths.TryGetValue(definition.CharacterId, out var bodyPath)) continue;
                Assert.That(definition.BodySprite, Is.Not.Null, definition.CharacterId);
                Assert.That(AssetDatabase.GetAssetPath(definition.BodySprite), Is.EqualTo(bodyPath), definition.CharacterId);
                AssertSpriteImporter(bodyPath, true);
            }

            foreach (var path in acceptedBackgrounds.Values.Concat(acceptedCgs.Values)) AssertSpriteImporter(path, false);
            Assert.That(catalog.TryResolveSpeakerAlias("로프·내부", out var memory), Is.True);
            Assert.That(memory, Is.EqualTo("rope_memory"));
            Assert.That(catalog.TryResolveSpeakerAlias("로프", out var actual), Is.True);
            Assert.That(actual, Is.EqualTo("rope"));
        }

        [Test]
        public void AcceptedAudio_ResolvesToCanonicalClipsWithPurposeSpecificImportSettings()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VNAudioCatalog>(AudioPath);
            var expectedBgm = new Dictionary<string, string>
            {
                ["bgm_ordinary_morning"] = "Assets/_Project/Audio/BGM/bgm_ordinary_morning.wav",
                ["bgm_quiet_farewell"] = "Assets/_Project/Audio/BGM/bgm_quiet_farewell.wav",
            };
            var expectedSfx = new Dictionary<string, string>
            {
                ["sfx_frying_oil"] = "Assets/_Project/Audio/SFX/sfx_frying_oil.wav",
                ["sfx_home_doorbell"] = "Assets/_Project/Audio/SFX/sfx_home_doorbell.wav",
                ["sfx_chair_leg_scrape"] = "Assets/_Project/Audio/SFX/sfx_chair_leg_scrape.wav",
                ["sfx_junho_laugh"] = "Assets/_Project/Audio/SFX/sfx_junho_laugh.wav",
                ["sfx_meal_cutlery"] = "Assets/_Project/Audio/SFX/sfx_meal_cutlery.wav",
                ["sfx_low_vibration"] = "Assets/_Project/Audio/SFX/sfx_low_vibration.wav",
                ["sfx_equipment_disconnect"] = "Assets/_Project/Audio/SFX/sfx_equipment_disconnect.wav",
                ["sfx_connection_rain_handoff"] = "Assets/_Project/Audio/SFX/sfx_connection_rain_handoff.wav",
                ["sfx_old_bus_brake"] = "Assets/_Project/Audio/SFX/sfx_old_bus_brake.wav",
            };
            var serialized = new SerializedObject(catalog);
            AssertClipEntries(serialized.FindProperty("bgm"), expectedBgm, AudioClipLoadType.Streaming);
            AssertClipEntries(serialized.FindProperty("sfx"), expectedSfx, AudioClipLoadType.DecompressOnLoad);
        }

        [Test]
        public void ExpressionKeys_RemainScopedAndUnfabricated()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(PresentationPath);
            var presentation = new SerializedObject(catalog);
            var definitions = presentation.FindProperty("characterDefinitions");
            var count = 0;
            var actualKeys = new List<string>();
            for (var i = 0; i < definitions.arraySize; i++)
            {
                var character = definitions.GetArrayElementAtIndex(i).objectReferenceValue as VNCharacterDefinition;
                var serializedCharacter = new SerializedObject(character);
                var expressions = serializedCharacter.FindProperty("expressions");
                for (var e = 0; e < expressions.arraySize; e++)
                {
                    var expression = expressions.GetArrayElementAtIndex(e);
                    actualKeys.Add(character.CharacterId + "/" + expression.FindPropertyRelative("expressionId").stringValue);
                    Assert.That(expression.FindPropertyRelative("headSprite").objectReferenceValue, Is.Null,
                        $"{character.CharacterId}/{expression.FindPropertyRelative("expressionId").stringValue} must remain an unillustrated fallback.");
                    count++;
                }
            }
            Assert.That(count, Is.EqualTo(19));
            CollectionAssert.AreEquivalent(ExpectedExpressionKeys, actualKeys);
        }

        [Test]
        public void YarnComposition_StagesHouseholdAroundImportedTableWithoutChangingLineIds()
        {
            var opening = File.ReadAllText("Assets/_Project/Yarn/PROLOGUE_S001_S003.yarn").Replace("\r\n", "\n");
            var continuation = File.ReadAllText("Assets/_Project/Yarn/PROLOGUE_S004_S006.yarn").Replace("\r\n", "\n");
            Assert.That(opening, Does.Contain("<<vn_bg_crossfade bg_family_table_morning 0>>\n<<vn_show minseok default far_left>>\n<<vn_scale minseok 0.60>>\n<<vn_show jinhee default left>>\n<<vn_scale jinhee 0.60>>\n<<vn_show jihun default center>>\n<<vn_scale jihun 0.60>>\n<<vn_show rope_memory default right>>\n<<vn_scale rope_memory 0.28>>\n<<vn_show junho default far_right>>\n<<vn_scale junho 0.60>>\n<<vn_show narae default outer_far_right>>\n<<vn_scale narae 0.60>>"));
            Assert.That(opening, Does.Contain("<<vn_hide minseok>>\n<<vn_hide narae>>\n<<vn_hide junho>>\n<<vn_hide rope_memory>>\n<<vn_bg_crossfade bg_family_living_room 0>>"));
            Assert.That(continuation, Does.Contain("<<vn_bg_crossfade bg_family_storage_doorway 0>>\n<<vn_show minseok default far_left>>\n<<vn_scale minseok 0.60>>\n<<vn_show jinhee default left>>\n<<vn_scale jinhee 0.60>>\n<<vn_show jihun default center>>\n<<vn_scale jihun 0.60>>\n<<vn_show rope_memory default right>>\n<<vn_scale rope_memory 0.28>>\n<<vn_show junho default far_right>>\n<<vn_scale junho 0.60>>\n<<vn_show narae default outer_far_right>>\n<<vn_scale narae 0.60>>"));
            Assert.That(continuation, Does.Contain("<<vn_bg_crossfade bg_family_table_sunset 0>>\n<<sfx_play sfx_meal_cutlery>>"));
            Assert.That(VNPresentationController.TryParseSlot("outer_far_right", out var slot), Is.True);
            Assert.That(slot, Is.EqualTo(VNCharacterSlot.OuterFarRight));
            Assert.That(VNPresentationController.ToSerializedSlot(slot), Is.EqualTo("outer_far_right"));
            Assert.That(Regex.Matches(opening + continuation, @"#line:prologue_s00[1256]_").Count, Is.GreaterThan(0));
        }

        [Test]
        public void OrdinaryRopeDefinition_UsesFootAlignedStageOffset()
        {
            var rope = AssetDatabase.LoadAssetAtPath<VNCharacterDefinition>("Assets/_Project/Settings/Presentation/Production/M10_rope_memory.asset");
            Assert.That(rope, Is.Not.Null);
            Assert.That(rope.StageOffset, Is.EqualTo(new Vector2(0f, -125f)));
        }

        [Test]
        public void PersistentCharacters_MoveBetweenStagingBeatsWithoutOccupiedSlotShowCommands()
        {
            var opening = File.ReadAllText("Assets/_Project/Yarn/PROLOGUE_S001_S003.yarn").Replace("\r\n", "\n");
            var continuation = File.ReadAllText("Assets/_Project/Yarn/PROLOGUE_S004_S006.yarn").Replace("\r\n", "\n");
            var s002 = opening.Substring(opening.IndexOf("title: PROLOGUE_S002_P01", StringComparison.Ordinal));
            s002 = s002.Substring(0, s002.IndexOf("===", StringComparison.Ordinal));
            var s006 = continuation.Substring(continuation.IndexOf("title: PROLOGUE_S006_P01", StringComparison.Ordinal));
            s006 = s006.Substring(0, s006.IndexOf("===", StringComparison.Ordinal));
            Assert.That(s002, Does.Contain("<<vn_move jinhee right>>\n<<vn_move jihun left>>"));
            Assert.That(s006, Does.Contain("<<vn_hide jihun>>\n<<vn_hide jinhee>>\n<<vn_hide rope_memory>>\n<<vn_bg_crossfade bg_family_entrance 0>>\n<<vn_show jihun default left>>\n<<vn_scale jihun 0.60>>\n<<vn_show jinhee firm right>>\n<<vn_scale jinhee 0.60>>\n<<vn_show rope_memory alert center>>\n<<vn_scale rope_memory 0.28>>"));
        }

        private static string[] ReadIds(SerializedProperty entries)
        {
            return Enumerable.Range(0, entries.arraySize)
                .Select(i => entries.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue)
                .ToArray();
        }

        private static void AssertSpriteEntries(SerializedProperty entries, IReadOnlyDictionary<string, string> paths)
        {
            var actual = Enumerable.Range(0, entries.arraySize)
                .ToDictionary(i => entries.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue,
                    i => entries.GetArrayElementAtIndex(i).FindPropertyRelative("sprite").objectReferenceValue as Sprite);
            foreach (var item in paths)
            {
                Assert.That(actual.TryGetValue(item.Key, out var sprite), Is.True, item.Key);
                Assert.That(sprite, Is.Not.Null, item.Key);
                Assert.That(AssetDatabase.GetAssetPath(sprite), Is.EqualTo(item.Value), item.Key);
                AssertSpriteImporter(item.Value, false);
            }
        }

        private static void AssertSpriteImporter(string path, bool isCharacter)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.That(importer, Is.Not.Null, path);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path);
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single), path);
            Assert.That(importer.maxTextureSize, Is.EqualTo(2048), path);
            Assert.That(importer.mipmapEnabled, Is.False, path);
            Assert.That(importer.isReadable, Is.False, path);
            Assert.That(importer.alphaIsTransparency, Is.EqualTo(isCharacter), path);
        }

        private static void AssertClipEntries(SerializedProperty entries, IReadOnlyDictionary<string, string> paths, AudioClipLoadType loadType)
        {
            var actual = Enumerable.Range(0, entries.arraySize)
                .ToDictionary(i => entries.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue,
                    i => entries.GetArrayElementAtIndex(i).FindPropertyRelative("clip").objectReferenceValue as AudioClip);
            foreach (var item in paths)
            {
                Assert.That(actual.TryGetValue(item.Key, out var clip), Is.True, item.Key);
                Assert.That(clip, Is.Not.Null, item.Key);
                Assert.That(clip.samples, Is.GreaterThan(0), item.Key);
                Assert.That(AssetDatabase.GetAssetPath(clip), Is.EqualTo(item.Value), item.Key);
                var importer = AssetImporter.GetAtPath(item.Value) as AudioImporter;
                Assert.That(importer, Is.Not.Null, item.Value);
                Assert.That(importer.forceToMono, Is.False, item.Value);
                Assert.That(importer.loadInBackground, Is.False, item.Value);
                Assert.That(importer.defaultSampleSettings.loadType, Is.EqualTo(loadType), item.Value);
                Assert.That(importer.defaultSampleSettings.preloadAudioData, Is.True, item.Value);
            }
        }
    }
}
