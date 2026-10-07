# M10-03 — Current Asset Inventory

Status: **CURRENT ASSET INVENTORY READY / READY FOR REVIEW**. 2026-10-07 Asia/Seoul. Repository `C:\Dev\project_all_time`; clean main base `e2dec902c3c99ae9c0222cdb3bfbe44197d60f1f`, merged PR #48 included. [Master requirements](M10_MASTER_ASSET_GAP_LIST.md), [temporary mapping and production briefs](M10_TEMPORARY_ASSET_MAPPING.md), [frozen lookup map](M10_PRODUCTION_ID_MAP.md), [manifest](../07_ASSET_MANIFEST.md).

## Actual inspection method and Unity health

Connected the installed Unity Pipeline MCP stdio server through `unity mcp --project-path C:\Dev\project_all_time` and standard MCP initialize/tools/list/tools/call. Read-only `editor_status`, `console_status`, `eval`, and `open_scene` were used. Main-thread C# evaluated **AssetDatabase.FindAssets** for Sprite, Texture2D, AudioClip, VNCharacterDefinition, VNPresentationCatalog, VNAudioCatalog, Prefab and ScriptableObject over **all Assets**, resolving every GUID through **GUIDToAssetPath**, LoadMainAssetAtPath and Sprite subassets. IsValidFolder and folder-scoped FindAssets checked the six required areas and five production destinations. Supplementary read-only SerializedObject/EditorJsonUtility inspection read scene references, definitions, catalog entries, RectTransforms, prefab components and mixer groups. Unity Search was opened with `dir:Assets/_Project`; the database queries, not filenames or search-result count, determine inventory.

Unity **6000.3.21f1**, actual project `C:/Dev/project_all_time/Assets`. Editor ready, compiling false, updating false, domain reload false, Play Mode stopped; no compile operation blocked inspection. Successful main-thread evaluations, scene opening and Search opening establish no blocking modal during the audit; enumerated Editor windows were normal Search/Mixer/toolbar/Project/Inspector/Hierarchy/Scene/Console/Game windows. VN_Main opened in Edit Mode and remained **dirty=false**. No Play Mode entry or save. Console **0 errors**, compilationFailed=false, 5 nonblocking warnings: legacy Input Manager, administrator startup, AI runtime reference to editor-only Yarn unsafe assembly, existing nullable-context test warning, and AI account API timeout. These are recorded, not fixed in this documentation milestone. Entering **443 passed / 0 failed / 0 skipped** is prior baseline evidence; the full suite was not rerun.

All eight M3 PNG source files were visually inspected. Audio metadata, role and provenance were inspected; clips were **not auditioned**, so no mood/quality claim is made. Fixture identity alone prevents final-story promotion; the two M4 BGMs have no established generation-plan provenance. No reference-game screen/audio evidence or generated visual is substituted for current-project evidence.

## Query results and count basis

| FindAssets filter | Matching asset paths | Interpretation |
| --- | --- | --- |
| t:Sprite | 8 | 8 PNG assets / 8 Sprite subassets |
| t:Texture2D | 15 | 8 fixture PNGs + EmojiOne atlas; other matches are font/SDF/template paths with texture subassets, not 15 story images |
| t:AudioClip | 6 | 2 reference BGM + 2 technical SFX + 2 technical voices |
| t:VNCharacterDefinition | 2 | Existing imported asset paths |
| t:VNPresentationCatalog | 1 | Existing imported asset paths |
| t:VNAudioCatalog | 1 | Existing imported asset paths |
| t:Prefab | 4 | Existing imported asset paths |
| t:ScriptableObject | 23 | 11 _Project SO paths + 12 generic/TMP/URP/input template paths; overlaps prior queries |

Counts below use **distinct relevant file paths**, excluding folders, .meta, scripts, scene geometry and generic template assets. Query totals overlap and must not be added. The focused register contains **32** assets: 10 reusable project framework resources, 18 technical fixtures, 2 reference-only BGM files, 2 Korean font provenance checks. **Existing production-usable M10 story media: 0**. Ten PRODUCTION_USABLE_CONFIRMED entries approve only their implemented framework use; they do not satisfy any new story-media requirement or prove M10 UI behavior. Existing fade is a scene/component capability, not another counted file. Reference BGMs are counted once as REFERENCE_ONLY although also used in technical audio tests.

| Classification | Distinct focused assets | Production meaning |
| --- | --- | --- |
| PRODUCTION_USABLE_CONFIRMED | 10 | Project-owned existing framework only; 0 story-media matches |
| TECHNICAL_FIXTURE_ONLY | 18 | 8 PNG + 2 SFX + 2 voice + 4 M3/M4 definitions/catalogs + M5 checkpoint + current Yarn project |
| REFERENCE_ONLY | 2 | M4 BGM; all known old Free-workflow tracks are also reference-only by policy |
| PROVENANCE_UNCONFIRMED | 2 | Noto font source and derived SDF attribution checks; not READY |
| NEW_PRODUCTION_REQUIRED | 50 semantic requirements | Future resources, not existing inventory files; 43 global resource identities + 19 scoped expression keys |

**Provenance approval blockers: 2 existing typography candidates; PROVENANCE_BLOCKED story requirements: 0.** No otherwise suitable story-media candidate was withheld only for provenance. General package/template items below are supplementary and excluded from the focused denominator. No unknown file is approved for final content use.

## Inspected folders

| Folder | AssetDatabase valid | Result |
| --- | --- | --- |
| Assets/_Project/Art | true | 8 inventoried file paths; remaining entries are folders |
| Assets/_Project/Audio | true | 8 inventoried file paths; remaining entries are folders |
| Assets/_Project/Settings/Presentation | true | 3 inventoried file paths; remaining entries are folders |
| Assets/_Project/Settings/Records | true | 4 inventoried file paths; remaining entries are folders |
| Assets/_Project/Prefabs | true | 4 inventoried file paths; remaining entries are folders |
| Assets/_Project/UI | true | Empty; reuse as later production destination |
| Assets/_Project/Art/Backgrounds | true | Empty; reuse as later production destination |
| Assets/_Project/Art/Characters | true | Empty; reuse as later production destination |
| Assets/_Project/Art/CG | true | Empty; reuse as later production destination |
| Assets/_Project/Audio/BGM | true | Empty; reuse as later production destination |
| Assets/_Project/Audio/SFX | true | Empty; reuse as later production destination |

All five production destinations are valid and empty. `Assets/_Project/UI` is also valid and empty; the current UI prefabs live under Prefabs/UI. Later Backgrounds → Art/Backgrounds, characters → Art/Characters/<characterId>, focused CG/images → Art/CG, BGM → Audio/BGM, SFX → Audio/SFX. No new insert directory is necessary for scene-owned text or the six CG lookup images. No directory is created now.

## Focused existing-asset register

Classification and provenance are separate fields. PROJECT_OWNED confirms the existing framework authority; TECHNICAL_FIXTURE and REFERENCE_ONLY explicitly prevent content promotion. UNKNOWN requires an attribution/source record before final approval. No file is changed or imported by this register.

| Actual Unity asset path | GUID | Classification | Provenance | Current role | Evidence / compatibility |
| --- | --- | --- | --- | --- | --- |
| Assets/_Project/Art/M3Test/Backgrounds/M3_BG_A.png | 9ebc0d6c087a9594d992c2d1171fd258 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 art smoke only | Viewed: blue fantasy cavern, statue and spell ring; not a domestic kitchen/storage/office. Imported texture 1536×1024; Sprite subasset confirmed. |
| Assets/_Project/Art/M3Test/Backgrounds/M3_BG_B.png | 02ef117bcba07c04eb6bd1697f667e79 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 art smoke only | Viewed: aerial future skyline/island/tower; not the authored pedestrian-level bright city street. Imported texture 1448×1086; Sprite subasset confirmed. |
| Assets/_Project/Art/M3Test/CG/M3_CG.png | 3d054858c48e4dd488e751049ee55541 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 art smoke only | Viewed: elf-eared magic/uniform portrait; not a family/object photograph or present table. Imported texture 1122×1402; Sprite subasset confirmed. |
| Assets/_Project/Art/M3Test/Characters/M3_A/M3_A_Body.png | a9b80d74a58de804c87bdec7fd8f82e3 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 art smoke only | Viewed: school-uniform female fixed body; not any canonical cast design. Imported texture 997×1577; Sprite subasset confirmed. |
| Assets/_Project/Art/M3Test/Characters/M3_A/M3_A_Crying.png | 9f71fd827a5c0b842adb1abe92c5e3de | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 art smoke only | Viewed: brown-haired cat-ear girl crying head; not Jinhee or any M10 person. Imported texture 2048×2048; Sprite subasset confirmed. |
| Assets/_Project/Art/M3Test/Characters/M3_A/M3_A_Default.png | a40824d1d12fd924080034c67804fe47 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 art smoke only | Viewed: brown-haired cat-ear girl neutral head; not any M10 person. Imported texture 2048×2048; Sprite subasset confirmed. |
| Assets/_Project/Art/M3Test/Characters/M3_A/M3_A_Smile.png | b11d5731062c76a4593e3cf34e6bc60a | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 art smoke only | Viewed: brown-haired cat-ear girl smile head; not any M10 person. Imported texture 2048×2048; Sprite subasset confirmed. |
| Assets/_Project/Art/M3Test/Characters/M3_B/M3_B.png | bc9d46fa74c88244ab79535970fd3117 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 art smoke only | Viewed: pink-haired elf/goth girl fixed figure; not any M10 person. Imported texture 864×1821; Sprite subasset confirmed. |
| Assets/_Project/Audio/M4Test/BGM/M4_BGM_A.mp3 | ab77293157f2a9548ad0e55ed8afbe82 | REFERENCE_ONLY | REFERENCE_ONLY | M4 BGM reference/command smoke only | 290.304s, 2 channel(s), 48000Hz. Metadata inspected; not auditioned. Specific Free/paid generation origin is not established; no production reuse. |
| Assets/_Project/Audio/M4Test/BGM/M4_BGM_B.mp3 | 41c27349b77d87a4681b2149ef1ce775 | REFERENCE_ONLY | REFERENCE_ONLY | M4 BGM reference/command smoke only | 219.504s, 2 channel(s), 48000Hz. Metadata inspected; not auditioned. Specific Free/paid generation origin is not established; no production reuse. |
| Assets/_Project/Audio/M4Test/SFX/M4_SFX_DOOR_CLOSE.ogg | b6a9e21138083fa4dbe8e86f54d1aae5 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M4 SFX command smoke only | 0.675s, 2 channel(s), 48000Hz. Metadata inspected; not auditioned. A door-close fixture is not a doorbell, handle turn or child arrival cue; no semantic match. |
| Assets/_Project/Audio/M4Test/SFX/M4_SFX_UI_CONFIRM.ogg | 6d4e9ae738926b549963938755e25bfd | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M4 SFX command smoke only | 0.539s, 1 channel(s), 44100Hz. Metadata inspected; not auditioned. UI confirmation is not an authored M10 household/equipment cue. |
| Assets/_Project/Audio/Voice/ko/m4_voice_test.wav | 79a1ae6c414dbd548a92763830acd8d8 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M4/M6 test voice only | 2.780s, 1 channel(s), 16000Hz. Metadata inspected; not auditioned. Test voice is not a commissioned canonical voice line. |
| Assets/_Project/Audio/Voice/ko/m6_voice_test.wav | abdbe658952ea3b499cb282303c9da59 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M4/M6 test voice only | 2.780s, 1 channel(s), 16000Hz. Metadata inspected; not auditioned. Test voice is not a commissioned canonical voice line. |
| Assets/_Project/Settings/Presentation/M3_CharacterA.asset | 5327b3fe52891294a9b8a85e3b33dc2a | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 definition/catalog | Serialized keys and live references inspected; no M10 production entries. Current Yarn starts at M2_UI_START; no M10 Yarn was authored. |
| Assets/_Project/Settings/Presentation/M3_CharacterB.asset | 3cf48b83b71cf9742ba1af1a9403bebd | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 definition/catalog | Serialized keys and live references inspected; no M10 production entries. Current Yarn starts at M2_UI_START; no M10 Yarn was authored. |
| Assets/_Project/Settings/Presentation/M3_PresentationCatalog.asset | 02467ef5fd9e1214d9400e43b7cbf7cd | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M3 definition/catalog | Serialized keys and live references inspected; no M10 production entries. Current Yarn starts at M2_UI_START; no M10 Yarn was authored. |
| Assets/_Project/Audio/M4_AudioCatalog.asset | 3c335d8c37515c34881b9aff4d5de774 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | M4 audio catalog | Serialized keys and live references inspected; no M10 production entries. Current Yarn starts at M2_UI_START; no M10 Yarn was authored. |
| Assets/_Project/Settings/SaveLoad/M5_CheckpointCatalog.asset | 7c30ba91e5b418a4ba818c2c123b1fcd | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | Technical checkpoint catalog | Serialized keys and live references inspected; no M10 production entries. Current Yarn starts at M2_UI_START; no M10 Yarn was authored. |
| Assets/_Project/Yarn/GameNarrative.yarnproject | c6e164ddc9efe2d4f8761bf2c5e13d48 | TECHNICAL_FIXTURE_ONLY | TECHNICAL_FIXTURE | Current technical Yarn project | Serialized keys and live references inspected; no M10 production entries. Current Yarn starts at M2_UI_START; no M10 Yarn was authored. |
| Assets/_Project/Prefabs/SaveLoad/VNSaveSlotItem.prefab | ea2ffa9bc8cf8b04dbb4f30ce9baa8a7 | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing Save/Load UI | AssetDatabase and prefab component structure confirmed; reusable baseline framework only. No M10 overlay/profile/manual-hold implementation or final cast art approval. |
| Assets/_Project/Prefabs/UI/M6/BacklogItem.prefab | de9405a8cc2a5914bae5b4a2ac398e69 | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing Backlog row | AssetDatabase and prefab component structure confirmed; reusable baseline framework only. No M10 overlay/profile/manual-hold implementation or final cast art approval. |
| Assets/_Project/Prefabs/UI/VNDialoguePanel.prefab | 3a0ef08201a89ac4895c101b5a3ffc2b | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing dialogue presenter | AssetDatabase and prefab component structure confirmed; reusable baseline framework only. No M10 overlay/profile/manual-hold implementation or final cast art approval. |
| Assets/_Project/Prefabs/UI/VNOptionItem.prefab | ffeaba71644a8244d879138d04628cd2 | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing technical option presenter; no M10 branch | AssetDatabase and prefab component structure confirmed; reusable baseline framework only. No M10 overlay/profile/manual-hold implementation or final cast art approval. |
| Assets/_Project/Audio/VNAudioMixer.mixer | 9cc27892a90083744943c3730dda1065 | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing M4 audio routing | Live AudioMixer groups: Master, BGM, SFX, Voice; routing framework only, not an audio clip. |
| Assets/_Project/Settings/Input/VNInputActions.inputactions | 6fac5fd1e55030243aff7c38a614bab8 | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing input authority | Reusable input asset; browsing/manual hold must integrate current ownership, not create a new global map. |
| Assets/_Project/Settings/Records/Production/VNAchievementCatalog.asset | 2e6ac52fc2d74d94d84b09a54c7c2c6a | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing Records catalog framework | All content entries empty (Timeline chapters and Archive categories also empty); no M10 record/achievement/Gallery/Timeline identity approved or generated. |
| Assets/_Project/Settings/Records/Production/VNArchiveCatalog.asset | fa10a73a1e7dd434b822ed8344a362a2 | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing Records catalog framework | All content entries empty (Timeline chapters and Archive categories also empty); no M10 record/achievement/Gallery/Timeline identity approved or generated. |
| Assets/_Project/Settings/Records/Production/VNCGGalleryCatalog.asset | 27f8d1688f46a894abdb38ef671436ad | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing Records catalog framework | All content entries empty (Timeline chapters and Archive categories also empty); no M10 record/achievement/Gallery/Timeline identity approved or generated. |
| Assets/_Project/Settings/Records/Production/VNTimelineCatalog.asset | 655982406f684734fa8385e6d9abe9eb | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing Records catalog framework | All content entries empty (Timeline chapters and Archive categories also empty); no M10 record/achievement/Gallery/Timeline identity approved or generated. |
| Assets/_Project/Fonts/NotoSansKR/NotoSansKR-Regular.ttf | cede0e29dc3f2ff4f91b168da5b92e04 | PROVENANCE_UNCONFIRMED | UNKNOWN | Existing Korean typography candidate | Imported font/font asset exists; repository source/license attribution evidence not found. Resolve source/license record before final typography approval; no new font production request and no claim of infringement. |
| Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Regular SDF.asset | 14df1fc36bd4506429dc2fc598fe1eab | PROVENANCE_UNCONFIRMED | UNKNOWN | Existing Korean typography candidate | Imported font/font asset exists; repository source/license attribution evidence not found. Resolve source/license record before final typography approval; no new font production request and no claim of infringement. |

The three M3 A head files are source PNGs at 2508×2508 but imported textures at 2048×2048, proving why filesystem size alone is insufficient. M3 B texture is 864×1821, Sprite M3_B_0 rect 703×1820. No import setting was changed. The two recorded font checks concern release provenance, not authorization to change fonts in M10-03.

## Supplementary generic/imported assets

The following additional all-Assets hits were checked rather than omitted. They are not compatible family/cast/prop/profile/medical media. English package fonts do not replace Korean typography; emoji artwork does not replace canonical expressions or Rope.

| AssetDatabase path | GUID | Classification | Provenance | Exclusion from focused M10 count |
| --- | --- | --- | --- | --- |
| Assets/Settings/Lit2DSceneTemplate.scenetemplate | d03ed43fc9d8a4f2e9fa70c1c7916eb9 | TECHNICAL_FIXTURE_ONLY | PROJECT_OWNED | Existing generic framework/template infrastructure only; no M10 content or new feature acceptance. |
| Assets/TextMesh Pro/Fonts/LiberationSans.ttf | e3265ab4bf004d28a9537516768c1c75 | PRODUCTION_USABLE_CONFIRMED | LICENSE_CONFIRMED | Bundled English font infrastructure; local Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt supplies license evidence, not a Korean-font provenance substitute. |
| Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset | 2e498d1c8094910479dc3e1b768306a4 | PRODUCTION_USABLE_CONFIRMED | LICENSE_CONFIRMED | Bundled English font infrastructure; local Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt supplies license evidence, not a Korean-font provenance substitute. |
| Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset | 8f586378b4e144a9851e7b34d9b748ee | PRODUCTION_USABLE_CONFIRMED | LICENSE_CONFIRMED | Bundled English font infrastructure; local Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt supplies license evidence, not a Korean-font provenance substitute. |
| Assets/TextMesh Pro/Sprites/EmojiOne.png | dffef66376be4fa480fb02b19edbe903 | REFERENCE_ONLY | UNKNOWN | Generic emoji atlas/sprite asset; semantic mismatch, provenance not approved for story content. |
| Assets/DefaultVolumeProfile.asset | 3f9215ea0144899419cfbc0957140d3f | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing generic framework/template infrastructure only; no M10 content or new feature acceptance. |
| Assets/InputSystem_Actions.inputactions | 2bcd2660ca9b64942af0de543d8d7100 | TECHNICAL_FIXTURE_ONLY | PROJECT_OWNED | Existing generic framework/template infrastructure only; no M10 content or new feature acceptance. |
| Assets/Settings/Renderer2D.asset | 424799608f7334c24bf367e4bbfa7f9a | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing generic framework/template infrastructure only; no M10 content or new feature acceptance. |
| Assets/Settings/UniversalRP.asset | 681886c5eb7344803b6206f758bf0b1c | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing generic framework/template infrastructure only; no M10 content or new feature acceptance. |
| Assets/TextMesh Pro/Resources/Sprite Assets/EmojiOne.asset | c41005c129ba4d66911b75229fd70b45 | REFERENCE_ONLY | UNKNOWN | Generic emoji atlas/sprite asset; semantic mismatch, provenance not approved for story content. |
| Assets/TextMesh Pro/Resources/Style Sheets/Default Style Sheet.asset | f952c082cb03451daed3ee968ac6c63e | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing generic framework/template infrastructure only; no M10 content or new feature acceptance. |
| Assets/TextMesh Pro/Resources/TMP Settings.asset | 3f5b5dff67a942289a9defa416b206f3 | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing generic framework/template infrastructure only; no M10 content or new feature acceptance. |
| Assets/UniversalRenderPipelineGlobalSettings.asset | 93b439a37f63240aca3dd4e01d978a9f | PRODUCTION_USABLE_CONFIRMED | PROJECT_OWNED | Existing generic framework/template infrastructure only; no M10 content or new feature acceptance. |

| Classification | Focused register | Supplementary generic assets | All registered queried files |
| --- | --- | --- | --- |
| PRODUCTION_USABLE_CONFIRMED | 10 | 9 | 19 |
| TECHNICAL_FIXTURE_ONLY | 18 | 2 | 20 |
| REFERENCE_ONLY | 2 | 2 | 4 |
| PROVENANCE_UNCONFIRMED | 2 | 0 | 2 |

Total across both registers: **45 distinct imported/query-visible files** (32 focused + 13 supplementary). Across all registered files, 19 are usable baseline framework/font resources, 20 technical fixtures/templates, 4 reference-only resources (2 BGM + 2 generic emoji files), 2 typography provenance checks. Story-media matches remain zero. VN_Main itself is separately inspected as the project-owned baseline scene, outside these query/file registers; it is not an approved M10 story implementation.

Unknown emoji provenance does not create another production blocker because no reuse is proposed. Template input/scene resources are technical rather than production story authority. Framework labels apply to current render/text configuration only, not new art rights or final M10 UI styling.

## Current catalogs, definitions and Scene consumers

| Current asset / lookup | Actual serialized state | M10 decision |
| --- | --- | --- |
| M3_PresentationCatalog | backgrounds `m3_bg_a`, `m3_bg_b`; cgs `m3_cg`; characterDefinitions M3 A/B | TECHNICAL_FIXTURE_ONLY; no production keys |
| M3_CharacterA | `characterId=m3_a`, alias M3A, fixed Body, default/smile/crying Heads; defaultExpressionId default; Facing Right, scale 1; no BackHair/save icon | Unrelated cast design; never relabel as Jinhee/Jihun |
| M3_CharacterB | `characterId=m3_b`, alias M3B, fixed Body, default expression with null Head; Facing Left, scale 1; no BackHair/save icon | Null Head is valid current contract; never relabel as family |
| M4_AudioCatalog | bgm `m4_bgm_a`, `m4_bgm_b`, loop=true, volume=1; sfx **`ui_confirm`, `door_close`**, volume=1 | Technical/reference keys; no production audio identity reuse |
| Records/Production four catalogs | All entry lists empty; Archive categories and Timeline chapters empty | Reusable project framework, no M10 unlock content |
| GameNarrative Yarn project / VN_Main DialogueSystem | Current startNode **M2_UI_START** | Technical baseline; no production Yarn authored |

| VN_Main consumer | Current catalog/authority | Later required audit |
| --- | --- | --- |
| PresentationRuntime / VNPresentationController | M3_PresentationCatalog | Dedicated production Presentation catalog; visual/speaker lookup |
| AudioRuntime / VNAudioController | M4_AudioCatalog; BGMSourceA/B and SFXSource | Dedicated production Audio catalog; existing M4 mixer/settings ownership |
| SaveLoadRuntime / VNSaveLoadController | presentationCatalog M3 | Restore uses same production presentation lookup, checkpoint contract unchanged |
| VNConvenienceRuntime / VNRecordsRuntimeBootstrap | presentationCatalog M3 + four empty Records/Production catalogs | Speaker/focus/Gallery lookups consistent; MetaProgress remains sole durable meta authority |
| ReplayPresentation / VNPresentationController | M3; isolated, inactive Replay hierarchy | Review production lookup dependency together; keep Replay state isolation and allowed immediate commands |
| PresentationRuntime / VNTransitionController | Existing controller, ScreenFadeOverlay, current/incoming BG and CG groups | Reuse implemented fade; no new global owner |

**M10-04/M10-05 IMPLEMENTATION REQUIREMENT:** create dedicated production VNPresentationCatalog and VNAudioCatalog later, with the frozen IDs, and audit all consumers consistently. Keep M3/M4 fixtures intact. No clone/rename/rewire or production catalog file/path is created here. Current baseline method signatures and fields, not a newly invented API, govern future integration. Fixed Body/Head/optional BackHair per character definition means ordinary Rope and actual cyber Rope need two presentation definitions for one identity; their disjoint aliases/reveal boundary are frozen in the ID Map.

## S001/S005 five-slot composition evidence

VN_Main GUID `15ebc4e3dabe3f441ba137435b41821f`. Production VNCanvas uses CanvasScaler ScaleWithScreenSize, reference **1920×1080**, match=.5. CharacterLayer is 1920×1080. All five slots have valid configured references; initial Edit Mode sprite references are empty.

| Slot | Anchor x / y | Reference-size center x | Rect / pivot | Observed issue |
| --- | --- | --- | --- | --- |
| FarLeftSlot | .08 / 0 | 153.6 | 650×950 / (.5,0) | Extends beyond left edge at rect bounds |
| LeftSlot | .28 / 0 | 537.6 | 650×950 / (.5,0) | Adjacent spacing 384 |
| Centerslot | .50 / 0 | 960 | 650×950 / (.5,0) | Adjacent spacing 422.4 |
| RightSlot | .72 / 0 | 1382.4 | 650×950 / (.5,0) | Adjacent spacing 422.4 |
| FarRightSlot | .92 / 0 | 1766.4 | 650×950 / (.5,0) | Extends beyond right edge at rect bounds |

All root/body/head scales are 1. Root rect 650×950 pivot(.5,.5). BodyImage 650×950, anchored position(50,-150); HeadImage 650×950, position(33,200); BackHair 100×100 position(0,0); preserveAspect=false on Images. Rect-width overlaps are **228–266px** before actual painted silhouettes; outer slot rect bounds alone reach -171.4/2091.4 on a 1920-wide frame. These are layout observations, not a rendered final-art quality test. No five-person visibility proof exists from empty Edit Mode images, and stretching/overlap risk is material. Separate inactive Replay CharacterLayer is 1408×740 with anchors .1/.3/.5/.7/.9 and the same 650×950 slots; it is not the production canvas.

M3 occupies one fixed slot per visible definition; occupied-slot rejection is not a free positioning/automatic packing system. Five humans plus Rope imply six identities, exceeding five simultaneous individual slots. **Frozen decision:** use at most two or three active/relevant foreground sprites in ordinary dialogue staging; other family members remain present through BG/prose. For explicit S001/S005 group views, commission occupied morning/sunset BG tableaux and visible-family storage-doorway framing. All five humans and ordinary Rope must be present at authored family-table beats; clear foreground figures already painted into the tableau to avoid clones. Pan/kitchen plate is separate from arrival-complete occupied table to protect entry order. Technical neutral figure diagrams validate wiring only and never certify final story visibility. No empty fifth place, family erasure, threatening Minseok, disappearing projection or new free-position system. [Temporary scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) and [briefs](M10_TEMPORARY_ASSET_MAPPING.md#first-user-production-requests) carry this forward.

## Provenance and scope boundary

All final M10 BG/cast/focused images and both BGMs lack compatible approved media. Later deliveries must record source, creator/license evidence and integration under PROJECT_OWNED / USER_GENERATED_PRODUCTION / LICENSE_CONFIRMED before READY. Known old Suno Free-workflow music remains **REFERENCE_ONLY**. Existing M4 BGM plan origin is unknown; no unsupported claim that these particular MP3s were Free-generated is made. Their filename/duration does not establish either mood compatibility or rights.

The 84 requirements reconcile to 0 MATCHED_PRODUCTION, 0 TEMP_TECHNICAL, 50 NEW_PRODUCTION_REQUIRED, 10 NO_SEPARATE_ASSET_REQUIRED, 16 TECHNICAL_REVIEW_M10_04, 8 OPTIONAL_DEFER, 0 PROVENANCE_BLOCKED. Neutral technical fallback is separately recorded and does not conceal a final-media gap. Only the six authorized Markdown docs change. Canonical source and Presentation Map are unchanged; no C#, Yarn, Scene/Prefab/SO, imported media, .meta, package, input/persistence schema or project setting is modified. No generation, full Unity suite or M10-04 work begins.
