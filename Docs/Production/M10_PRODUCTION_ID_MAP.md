# M10-03 — Production ID Map

Status: **PRODUCTION STABLE IDs FROZEN / READY FOR REVIEW**. 2026-10-07 Asia/Seoul; base `e2dec902c3c99ae9c0222cdb3bfbe44197d60f1f`. This freezes lookup identities, not installed catalog entries. Every new resource is **PLANNED**, Current Asset **NONE**. No final art/audio exists; no fixture is silently relabeled. [Inventory](M10_CURRENT_ASSET_INVENTORY.md), [temporary mapping](M10_TEMPORARY_ASSET_MAPPING.md), [master requirements](M10_MASTER_ASSET_GAP_LIST.md) and [manifest](../07_ASSET_MANIFEST.md) form the handoff.

## Identity and ownership rules

Use ASCII `^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$`. Character IDs use semantic romanized names; BG/CG/BGM/SFX use `bg_` / `cg_` / `bgm_` / `sfx_` plus a reusable narrative role. Localized display text, file names and temporary material never define identity. `REQ-*` and presentation beat keys remain documentation references. This is not a freeze of node, line, checkpoint, Records or MetaProgress IDs.

There are **43 unscoped resource identities + 19 character-scoped expression keys = 62 lookup keys**: 9 Character, 19 Expression, 12 BG, 6 CG, 2 BGM, 14 SFX. Expression identity is the pair `(characterId, expressionId)`; `default` is the existing documented default-expression selector and a real planned base state in each new definition. Reusing semantic `default`, `smile`, `crying`, `uneasy`, `alert` across different definitions is allowed; never share/override a definition's key or Sprite authority. No `expr_*` globally addressable IDs or extra manifest rows are manufactured for these child states.

`rope_memory` and `rope` are **two presentation definitions for the same Rope**, because M3 has one fixed Body per definition and no pose/body-swap command. The ordinary in-connection representation owns only `로프·내부`; the actual cyber representation owns only public `로프`. Hide/clear the former across the authored return boundary; show the latter only at S006 L924. They must never coexist as two dogs. No double alias registration. Jihun speech/inner/channel aliases resolve one `jihun` definition, but text-mode/Backlog integration remains M10-04 and off-screen labels do not create visibility.

Character definitions contain fixed Body, optional BackHair, and aligned Head states. Default heads may be part of a visual bundle, not independent files; existing nullable Head semantics do not justify overproducing sprites. Nine requirements for expressions describe semantic sets; nineteen keys include nine base states and ten visual distinctions. `firm` on Jinhee retains tears; it does not imply recovery. Child/dog polish states are deferred rather than line-specific poses.

BG ownership includes occupied morning/sunset tableaux. `bg_family_kitchen_morning` supplies the pan crop; `bg_family_table_morning` supplies the arrival-complete five-person/table/dog view. Do not expose the latter before arrival, duplicate people with foreground sprites, or show empty fifth places. Dedicated ensemble CG is unnecessary; REQ-CG-001 remains traced to the occupied BG strategy. The incidental residential corridor uses prose and a clean cut; no BG identity is frozen for it.

The six focused images chosen for existing full-view CG lookup are old shoe, two beach photographs, returned bag contents, unused new shoe, and matched present table. Each owns one catalog Sprite; the same image is not also entered into backgrounds or an independent insert registry. CG here means runtime presentation lookup, **not automatic Gallery unlock**. Memo, accident paper, two-path diagram, profiles and all structured UI remain Scene/Prefab/component-owned. They receive no persistent/catalog identities.

Both BGM roles are NEW_PRODUCTION_REQUIRED, new eligible paid workflow only. Fourteen SFX roles are intentionally catalog-addressed. Repeated laugh and toe taps use one clip/key twice total. Optional inferred drawer/handle/cloth/paper/wrapping/bag sounds receive no keys. Finite `sfx_connection_rain_handoff` jointly covers REQ-SFX-018 and REQ-AMB-002; it freezes content ownership through VNAudioCatalog.sfx, **not a new Ambient API**. M10-04 must validate a finite authored equipment→rain transition, tail/stop behavior and discrete drop/bus sequencing before it is sourced. If a loop proves necessary, return for an explicit map review; do not silently invent another key or owner.

## Frozen lookup records

Temporary Asset **NONE / neutral later** means a future neutral frame or silhouette or silence under separate technical mapping; no substitution is installed now. A missing key must never be sent to today's fixture catalogs as if it resolved. Expression temporary treatment is its character's neutral base, not M3 A's face.

### Character

| Production ID | Character scope | Requirement Ref | Scene(s) | Runtime Authority | Current Asset | Status | Temporary Asset | Display/Speaker Alias | Request timing | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `jihun` | — | [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | S001–S008 | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 지훈 / 지훈·내부 / 지훈·독백 | REQUEST_NOW | Procedural composed human; internal and thought labels are reading modes, never separate people. |
| `jinhee` | — | [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | S001–S006 | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 진희 | REQUEST_NOW | Agency remains hers; tears can continue through firm return response; no promised recovery. |
| `minseok` | — | [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003) | S001–S005 | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 민석 | REQUEST_NOW | Ordinary husband/father; never threatening or grabbing; retain visible household presence. |
| `narae` | — | [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004) | S001–S005 | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 나래 / 나래·화면 밖 | REQUEST_NOW | Ordinary daughter; off-screen alias does not summon a sprite. |
| `junho` | — | [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005) | S001–S005 | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 준호 / 준호·화면 밖 | REQUEST_NOW | Ordinary son; old shoe and new unused shoe must differ. |
| `rope_memory` | — | [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006) | S001–S006 before awakening | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 로프·내부 | REQUEST_NOW | Ordinary small white dog in the connection; no machinery before S006 L924. |
| `rope` | — | [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007) | S006 after L924, S007, S008 | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 로프 | REQUEST_M10_06 | Same Rope identity, actual cyber-dog representation after L924; shoulder/ankle joints only now. |
| `dokyung` | — | [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008) | S007 | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 도경 / 이도경 | REQUEST_M10_07 | Serious attentive clinician; no ridicule or premature cause diagnosis. |
| `eunjung` | — | [REQ-CHAR-009](M10_MASTER_ASSET_GAP_LIST.md#req-char-009) | S008 | VNPresentationCatalog.characterDefinitions → VNCharacterDefinition | NONE | PLANNED | NONE / neutral later | 은정 | REQUEST_M10_07 | Neutral professional guide; no hidden helper identity or flirtatious treatment. |

### Expression

| Production ID | Character scope | Requirement Ref | Scene(s) | Runtime Authority | Current Asset | Status | Temporary Asset | Display/Speaker Alias | Request timing | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `default` | jihun | [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | S001–S008 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Composed attentive base. |
| `uneasy` | jihun | [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-EXP-004](M10_MASTER_ASSET_GAP_LIST.md#req-exp-004) | S002, S003, S006, S007 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Small pain/pause distinction; not emotional collapse. |
| `default` | jinhee | [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | S001–S006 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Ordinary attentive base. |
| `smile` | jinhee | [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-001](M10_MASTER_ASSET_GAP_LIST.md#req-exp-001) | S001, S002, S005 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Warm ordinary smile. |
| `uneasy` | jinhee | [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-002](M10_MASTER_ASSET_GAP_LIST.md#req-exp-002) | S001–S004 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Small recollection/hesitation shift. |
| `crying` | jinhee | [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003) | S004–S006 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Accepted loss; restrained tears. |
| `firm` | jinhee | [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003) | S004–S006 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Self-directed resolve with tears retained, not instant composure. |
| `default` | minseok | [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003) | S001–S005 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Ordinary attentive base. |
| `quiet` | minseok | [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-EXP-005](M10_MASTER_ASSET_GAP_LIST.md#req-exp-005) | S001–S005 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_08_POLISH | Silent restrained attention, no menace. |
| `default` | narae | [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004), [REQ-EXP-006](M10_MASTER_ASSET_GAP_LIST.md#req-exp-006) | S001–S005 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Ordinary matter-of-fact base; no extra expression file quota. |
| `default` | junho | [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005) | S001–S005 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Ordinary base. |
| `pout` | junho | [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-EXP-006](M10_MASTER_ASSET_GAP_LIST.md#req-exp-006) | S001–S005 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_08_POLISH | Small breakfast pout. |
| `eager` | junho | [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-EXP-006](M10_MASTER_ASSET_GAP_LIST.md#req-exp-006) | S001–S005 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_08_POLISH | Shoe anticipation, no creepy caricature. |
| `default` | rope_memory | [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006) | S001–S006 before awakening | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Ordinary dog calm base; paw/rest actions carried by prose/composition. |
| `alert` | rope_memory | [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006), [REQ-EXP-009](M10_MASTER_ASSET_GAP_LIST.md#req-exp-009) | S002, S003, S006 before awakening | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_05_REVIEW | Small ordinary head-attention distinction; no mechanical hints. |
| `default` | rope | [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007) | S006 after L924, S007, S008 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Actual dog calm base; joints visible only after reveal. |
| `alert` | rope | [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007), [REQ-EXP-009](M10_MASTER_ASSET_GAP_LIST.md#req-exp-009) | S006 after L924, S007, S008 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_08_POLISH | Actual dog attentive head; stretch is not a head animation. |
| `default` | dokyung | [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-EXP-007](M10_MASTER_ASSET_GAP_LIST.md#req-exp-007) | S007 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_07 | Serious attentive base; no separate smile needed. |
| `default` | eunjung | [REQ-CHAR-009](M10_MASTER_ASSET_GAP_LIST.md#req-char-009), [REQ-EXP-008](M10_MASTER_ASSET_GAP_LIST.md#req-exp-008) | S008 | VNCharacterDefinition.expressions (character-scoped) | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_07 | Neutral professional base; no portrait requirement. |

### BG

| Production ID | Character scope | Requirement Ref | Scene(s) | Runtime Authority | Current Asset | Status | Temporary Asset | Display/Speaker Alias | Request timing | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `bg_family_kitchen_morning` | — | [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-INS-001](M10_MASTER_ASSET_GAP_LIST.md#req-ins-001) | S001 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Pan-first kitchen/staging plate; five eggs readable in crop, then four household people in wider occupied framing; no visitor or clinical reveal. |
| `bg_family_table_morning` | — | [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-CG-001](M10_MASTER_ASSET_GAP_LIST.md#req-cg-001) | S001 (compatible table-glance reuse in S002/S003 only) | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Occupied tableau only after arrival; exactly Jihun, Jinhee, Minseok, Narae, Junho plus ordinary Rope at Jihun foot; no empty chair. |
| `bg_family_entrance` | — | [REQ-BG-002](M10_MASTER_ASSET_GAP_LIST.md#req-bg-002) | S001, S006 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Ordinary entry; normal exit on screen right and adjacent wall retained for S006 localized outline. |
| `bg_family_living_room` | — | [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003) | S002–S004 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Home photos and visibly linked hall/table; no accident records or clue marks. |
| `bg_family_hall` | — | [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004) | S003, S004 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Narrow domestic hall, readable closed handle, visibly connected living/table; no threatening architecture. |
| `bg_family_storage` | — | [REQ-BG-005](M10_MASTER_ASSET_GAP_LIST.md#req-bg-005) | S004 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Box, returned bag, belongings envelopes; record contents unreadable in wide frame. |
| `bg_family_storage_doorway` | — | [REQ-BG-006](M10_MASTER_ASSET_GAP_LIST.md#req-bg-006) | S005 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Jinhee/Jihun foreground speaker space; Minseok/Narae/Junho visible beyond; maintain ordinary Rope continuity. |
| `bg_family_table_sunset` | — | [REQ-BG-007](M10_MASTER_ASSET_GAP_LIST.md#req-bg-007), [REQ-CG-001](M10_MASTER_ASSET_GAP_LIST.md#req-cg-001) | S005 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Same occupied table geometry and cast, chosen sunset meal; no vanishing people or time-acceleration clock. |
| `bg_jinhee_bedroom_day` | — | [REQ-BG-008](M10_MASTER_ASSET_GAP_LIST.md#req-bg-008) | S006 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Actual home bedroom in future city; opposite bed, restrained equipment and natural daylight, no family projections. |
| `bg_future_city_street_day` | — | [REQ-BG-010](M10_MASTER_ASSET_GAP_LIST.md#req-bg-010) | S006 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Ordinary bright street at pedestrian height, not an aerial skyline or surveillance image. |
| `bg_dokyung_office` | — | [REQ-BG-011](M10_MASTER_ASSET_GAP_LIST.md#req-bg-011) | S007 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_07 | Desk/window, room for two humans and actual Rope below desk; screen secondary. |
| `bg_immersion_preparation` | — | [REQ-BG-012](M10_MASTER_ASSET_GAP_LIST.md#req-bg-012) | S008 | VNPresentationCatalog.backgrounds | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_07 | Screen and bed, two humans and actual Rope beside bed; no next-era scene or helper identity. |

### CG

| Production ID | Character scope | Requirement Ref | Scene(s) | Runtime Authority | Current Asset | Status | Temporary Asset | Display/Speaker Alias | Request timing | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `cg_worn_child_shoe` | — | [REQ-INS-002](M10_MASTER_ASSET_GAP_LIST.md#req-ins-002) | S001, S004 | VNPresentationCatalog.cgs | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Small folded worn heel with child heel outside; reuse S004. No accident clue. |
| `cg_beach_photo_child` | — | [REQ-INS-004](M10_MASTER_ASSET_GAP_LIST.md#req-ins-004) | S002 | VNPresentationCatalog.cgs | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Junho sandy hands holding snack bag in a photograph, not a new flashback. |
| `cg_beach_photo_sea` | — | [REQ-INS-004](M10_MASTER_ASSET_GAP_LIST.md#req-ins-004) | S002 | VNPresentationCatalog.cgs | NONE | PLANNED | NONE / neutral later | — | REQUEST_NOW | Strong sunlight sea photograph turned to by Minseok; no accident aftermath. |
| `cg_returned_bag_contents` | — | [REQ-INS-005](M10_MASTER_ASSET_GAP_LIST.md#req-ins-005) | S004 | VNPresentationCatalog.cgs | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Returned travel bag, sandy towel, partly eaten snack wrapper; no implied returner. |
| `cg_unused_new_shoe` | — | [REQ-INS-007](M10_MASTER_ASSET_GAP_LIST.md#req-ins-007) | S004, S005 | VNPresentationCatalog.cgs | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Unused sole in Jinhee palm; later fitting uses same design in staging; no memory montage. |
| `cg_present_table_glimpse` | — | [REQ-INS-008](M10_MASTER_ASSET_GAP_LIST.md#req-ins-008) | S006 | VNPresentationCatalog.cgs | NONE | PLANNED | NONE / neutral later | — | REQUEST_M10_06 | Brief present table matching family table camera; no inferred owner, wife/daughter, or lingering empty-chair close-up. |

### BGM

| Production ID | Character scope | Requirement Ref | Scene(s) | Runtime Authority | Current Asset | Status | Temporary Asset | Display/Speaker Alias | Request timing | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `bgm_ordinary_morning` | — | [REQ-BGM-001](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-001) | S001–S003 | VNAudioCatalog.bgm | NONE | PLANNED | NONE / silence | — | REQUEST_NOW | New eligible paid workflow. L45 enter; S002 continuous lowering requires M10-04; S003 L350 fade at paw touch; silence S004. |
| `bgm_quiet_farewell` | — | [REQ-BGM-002](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-002) | Late S005–S006 L892 | VNAudioCatalog.bgm | NONE | PLANNED | NONE / silence | — | REQUEST_M10_06 | New eligible paid workflow. Late S005 L784 only; continue exit; S006 L892 immediate stop. |

### SFX

| Production ID | Character scope | Requirement Ref | Scene(s) | Runtime Authority | Current Asset | Status | Temporary Asset | Display/Speaker Alias | Request timing | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_frying_oil` | — | [REQ-SFX-001](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-001) | S001 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_NOW | Finite domestic frying segment under opening black; no loop requirement. |
| `sfx_plate_counter` | — | [REQ-SFX-002](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-002) | S001 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_05_REVIEW | Single plate placed on counter; authored P2 cue still required before final S001 sound acceptance. |
| `sfx_home_doorbell` | — | [REQ-SFX-003](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-003) | S001 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_NOW | Ordinary doorbell at L77; not a suspense sting. |
| `sfx_child_entry_steps` | — | [REQ-SFX-004](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-004) | S001 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_05_REVIEW | Short child footsteps preceding father rising; authored P2 acceptance retained. |
| `sfx_chair_leg_scrape` | — | [REQ-SFX-006](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-006) | S002 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_NOW | Exactly one close chair-leg scrape; no voices/hallucination. |
| `sfx_junho_laugh` | — | [REQ-SFX-007](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-007) | S003 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_NOW | Same diegetic short child laugh twice total, identical pitch/length; no laugh track. |
| `sfx_meal_cutlery` | — | [REQ-SFX-012](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-012) | S005 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_06 | Sparse finite dish/cutlery actions during late meal, leave dialogue clear. |
| `sfx_sink_washing` | — | [REQ-SFX-013](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-013) | S005 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_06 | Finite ordinary flowing-water/plate wash at consent; no independent continuous subsystem. |
| `sfx_slipper_toe_tap` | — | [REQ-SFX-014](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-014) | S006 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_06 | Same quiet tap twice total, not footsteps or ominous knocking. |
| `sfx_low_vibration` | — | [REQ-SFX-015](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-015) | S006 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_06 | Restrained low vibration at localized outline; no cosmic/horror swell. |
| `sfx_equipment_disconnect` | — | [REQ-SFX-016](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-016) | S006 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_06 | Short clinical equipment termination at daylight bedroom reveal; no victory chime. |
| `sfx_connection_rain_handoff` | — | [REQ-SFX-018](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-018), [REQ-AMB-002](M10_MASTER_ASSET_GAP_LIST.md#req-amb-002) | S008; S008 ending | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_07 | Planned finite equipment→rain composite one-shot; includes AMB-002. M10-04 must verify tail, cue order and audio ownership before sourcing; no promised loop API. |
| `sfx_raindrop_awning` | — | [REQ-SFX-019](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-019) | S008 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_07 | Single large drop on awning after handoff; separate onset accent, avoid double counting same drop in composite. |
| `sfx_old_bus_brake` | — | [REQ-SFX-020](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-020) | S008 | VNAudioCatalog.sfx | NONE | PLANNED | NONE / silence | — | REQUEST_M10_07 | Old bus braking after drop; no bus picture or next-era gameplay. |

## Component ownership without content IDs

| Requirements | Planned authority / scope | Later gate |
| --- | --- | --- |
| REQ-UI-001 | Existing dialogue area + shared authoritative reading/focus/Backlog lifecycle; integrate explicit modes | M10-04 → M10-05 |
| REQ-UI-002–012 | M10 Scene/Prefab structured-information view under existing Hide/modal/load ownership; exact authored facts only | M10-04 → corresponding slice |
| REQ-UI-013 | Scene-owned static end-title text and black transition | M10-07 |
| REQ-INS-003, REQ-INS-006 | Scene-owned memo/paper text panels; preserve reveal/cover/reopen, no fabricated date | M10-05 / M10-06 |
| REQ-DIAG-001 | Scene-owned simple text/vector two-path diagram | M10-07 |
| REQ-PROFILE-001/002 | Scene/Prefab transient browse view integrated with current input/reading gate; exact three role profiles | M10-04 → M10-07 |
| REQ-FX-001 | Existing VNTransitionController / ScreenFadeOverlay | Existing capability; slice integration later |
| REQ-FX-002/003 | Later minimal localized effect component using existing presentation and M7 shake gate if movement selected | M10-04 → M10-06 |
| REQ-FX-004 | Linked-room composition + exact canonical prose | M10-05 |

These owner labels are architecture directions, not invented class/API contracts. They add no SaveData, Settings or MetaProgress fields. No scene hierarchy is created here.

## Catalog and destination policy

**M10-04/M10-05 IMPLEMENTATION REQUIREMENT:** create dedicated production VNPresentationCatalog and VNAudioCatalog assets in Unity, then wire all dependent consumers consistently. Current VN_Main uses M3/M4; keep those assets/IDs intact for technical regression. Presentation controller, Save/Load lookup, speaker focus, Records/Gallery and Replay template catalog consumers must be audited together; Replay controller/state isolation and allowed immediate command list remain intact. Production catalogs may use neutral planned-key material only in explicitly temporary builds; neutral material never advances manifest status to READY. Do not clone fixture IDs or repurpose fixture catalogs. No asset is created or rewired now; production catalog file names/IDs are not invented.

Reuse AssetDatabase-confirmed existing empty `Assets/_Project/Art/Backgrounds`, `Art/Characters`, `Art/CG`, `Audio/BGM`, `Audio/SFX`. Character subfolders may be created later under `Art/Characters/<characterId>`; no folder is created in M10-03. Focused CG resources live under Art/CG, including selected inserts, with one Sprite authority. Component-owned inserts need no new directory. Later production definitions/catalogs belong under current Settings/Presentation and Audio policy, respectively; exact file paths await creation. File names may be descriptive but never become identity. Manifest sourceFile/runtimeFile are consistently `—` until real files exist.

## Collision audit

Every proposed token was searched across the entire repository with case-insensitive whole-word `rg -n -i -w -F --hidden`, excluding only Git metadata and ignored generated Library/Temp/Logs/obj/UserSettings. Search output was interpreted against actual serialized catalogs, character expression scopes, Yarn usages, test fixtures and documentation. Raw English prose matches (`Jihun`, `Rope`, `default` etc.) are not serialized ID conflicts. All 43 unscoped keys and all 19 `(character, expression)` pairs are unique in their ownership domains; zero conflicting existing stable IDs, zero case-only serialized ID variants, zero technical content identity reuse. The current scopes `m3_a/default`, `m3_a/smile`, `m3_a/crying`, `m3_b/default` do not conflict with new character scopes. Existing `default` selectors are a shared contract, not fixture identity promotion. No fixture was renamed. The per-token evidence below was collected **before** this document/manifest was written, so these docs cannot self-certify their own absence.

| Proposed token | Pre-write matching files / lines | Disposition |
| --- | --- | --- |
| `alert` | 3 / 3 | Shared expression/selector text; new character scopes unique |
| `bg_dokyung_office` | 0 / 0 | No occurrence; new lookup token |
| `bg_family_entrance` | 0 / 0 | No occurrence; new lookup token |
| `bg_family_hall` | 0 / 0 | No occurrence; new lookup token |
| `bg_family_kitchen_morning` | 0 / 0 | No occurrence; new lookup token |
| `bg_family_living_room` | 0 / 0 | No occurrence; new lookup token |
| `bg_family_storage` | 0 / 0 | No occurrence; new lookup token |
| `bg_family_storage_doorway` | 0 / 0 | No occurrence; new lookup token |
| `bg_family_table_morning` | 0 / 0 | No occurrence; new lookup token |
| `bg_family_table_sunset` | 0 / 0 | No occurrence; new lookup token |
| `bg_future_city_street_day` | 0 / 0 | No occurrence; new lookup token |
| `bg_immersion_preparation` | 0 / 0 | No occurrence; new lookup token |
| `bg_jinhee_bedroom_day` | 0 / 0 | No occurrence; new lookup token |
| `bgm_ordinary_morning` | 0 / 0 | No occurrence; new lookup token |
| `bgm_quiet_farewell` | 0 / 0 | No occurrence; new lookup token |
| `cg_beach_photo_child` | 0 / 0 | No occurrence; new lookup token |
| `cg_beach_photo_sea` | 0 / 0 | No occurrence; new lookup token |
| `cg_present_table_glimpse` | 0 / 0 | No occurrence; new lookup token |
| `cg_returned_bag_contents` | 0 / 0 | No occurrence; new lookup token |
| `cg_unused_new_shoe` | 0 / 0 | No occurrence; new lookup token |
| `cg_worn_child_shoe` | 0 / 0 | No occurrence; new lookup token |
| `crying` | 1 / 1 | Shared expression/selector text; new character scopes unique |
| `default` | 55 / 142 | Shared expression/selector text; new character scopes unique |
| `dokyung` | 0 / 0 | No occurrence; new lookup token |
| `eager` | 1 / 1 | Shared expression/selector text; new character scopes unique |
| `eunjung` | 0 / 0 | No occurrence; new lookup token |
| `firm` | 1 / 2 | Shared expression/selector text; new character scopes unique |
| `jihun` | 4 / 123 | Documentation/prose/contract occurrences only; no conflicting content ID |
| `jinhee` | 2 / 93 | Documentation/prose/contract occurrences only; no conflicting content ID |
| `junho` | 2 / 17 | Documentation/prose/contract occurrences only; no conflicting content ID |
| `minseok` | 4 / 28 | Documentation/prose/contract occurrences only; no conflicting content ID |
| `narae` | 2 / 9 | Documentation/prose/contract occurrences only; no conflicting content ID |
| `pout` | 1 / 1 | Shared expression/selector text; new character scopes unique |
| `quiet` | 4 / 42 | Shared expression/selector text; new character scopes unique |
| `rope` | 4 / 103 | Documentation/prose/contract occurrences only; no conflicting content ID |
| `rope_memory` | 0 / 0 | No occurrence; new lookup token |
| `sfx_chair_leg_scrape` | 0 / 0 | No occurrence; new lookup token |
| `sfx_child_entry_steps` | 0 / 0 | No occurrence; new lookup token |
| `sfx_connection_rain_handoff` | 0 / 0 | No occurrence; new lookup token |
| `sfx_equipment_disconnect` | 0 / 0 | No occurrence; new lookup token |
| `sfx_frying_oil` | 0 / 0 | No occurrence; new lookup token |
| `sfx_home_doorbell` | 0 / 0 | No occurrence; new lookup token |
| `sfx_junho_laugh` | 0 / 0 | No occurrence; new lookup token |
| `sfx_low_vibration` | 0 / 0 | No occurrence; new lookup token |
| `sfx_meal_cutlery` | 0 / 0 | No occurrence; new lookup token |
| `sfx_old_bus_brake` | 0 / 0 | No occurrence; new lookup token |
| `sfx_plate_counter` | 0 / 0 | No occurrence; new lookup token |
| `sfx_raindrop_awning` | 0 / 0 | No occurrence; new lookup token |
| `sfx_sink_washing` | 0 / 0 | No occurrence; new lookup token |
| `sfx_slipper_toe_tap` | 0 / 0 | No occurrence; new lookup token |
| `smile` | 8 / 12 | Shared expression/selector text; new character scopes unique |
| `uneasy` | 0 / 0 | No occurrence; new lookup token |

All keys have requirement traceability above. Case-only display words and C# default keywords are counted in raw hits; no existing conflicting runtime identity was found. Future semantic identity changes require revising this map and its manifest/requirement links before Yarn use.
