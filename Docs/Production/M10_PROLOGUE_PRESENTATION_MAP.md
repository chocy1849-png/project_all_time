# M10-02 — Prologue Presentation Map

Status: **PRESENTATION MAPPING READY FOR REVIEW**. Planning only, 2026-10-01 (Asia/Seoul), base `512750786a15a7ca2f2b747814fce36727a8f83c` including PR #47. Story authority remains [the frozen Korean S001–S008 source](../Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt), original authoring SHA-256 `8413E847DD66D7D45585847A935B9C384D91FACFE9724EE27084F3D86FF977E9`. Working-copy hash still matches. Approximate source line ranges below identify this revision; exact wording stays in the source.

Authority: source → [Docs/05 canon/invariants](../05_PROLOGUE_VERTICAL_SLICE.md) → [M1–M9 architecture](../04_TECHNICAL_ARCHITECTURE.md) and [writing contracts](../06_YARN_WRITING_GUIDE.md) → verified [M10-00 evidence](../Research/M10_LOCAL_VN_REFERENCE_AUDIT.md) → convention. M10-00 local visual/audio claims remain USER_CAPTURE_NEEDED; they do not block this map and no candidate numerical timing is adopted.

Companion: [Master Asset Gap List](M10_MASTER_ASSET_GAP_LIST.md). Every beat and requirement reference is a mutable documentation key, **not** a node, line, checkpoint, persistent key, final asset ID or filename. No Unity hierarchy, class/API/command, slot assignment, duration/curve, BPM/instrument specification, pixel/color value or generation prompt is frozen. Source, Yarn, C#, runtime assets, catalogs and schemas remain unchanged. No production generation or user asset request is made. Actual repository inventory has NOT yet been compared; that is M10-03.

## Frozen presentation vocabulary

| Mode | Player-facing communication | Reading / focus / history contract |
| --- | --- | --- |
| Normal Dialogue | Spoken character words, speaker identity visible; off-screen qualifier retained where authored | Existing dialogue area, authoritative advance lifecycle; visible-speaker focus compatible with M3. No inferred focus on an off-screen person |
| Narration | Canonical 서술, prose with no character nameplate | Same primary area; non-voice. Each authored paragraph retained; only permitted long narration sentence splits later. No fullscreen narration system assumed |
| Jihun Inner Monologue | Explicit subtle thought identity (지훈 · 독백), distinguishable without color/italics alone | Same lifecycle and understandable Backlog label; not spoken or heard by others. No added thought or emotional interpretation |
| Internal Channel | Explicit 로프 · 내부 / 지훈 · 내부 identity; restrained channel marker distinct from both speech and thought | Same authoritative reading lifecycle. Channel label must not expose machinery before L924; later public 로프 is normal dialogue. Alias / focus / history integration is a spike |
| Structured Information Overlay | Non-dialogue, non-speaker, compact secondary facts; noninteractive unless expressly authored | No independent Advance consumption; no fake Backlog dialogue from raw values. Hide hides it with VN presentation. Save/Load/Settings/Records modal opening/closing does not mutate logical overlay state. Load clears stale transient UI and reconstructs required facts through authored entry flow; no SaveData or MetaProgress fields |

Source bracket instructions are never displayed or voiced verbatim. Profile data and title are informational presentation, not character speech; their Text Mode is None. Mixed-mode beat rows list modes in order of first occurrence; the block ledger preserves their full source sequence. This is **not** a license to merge paragraphs into one textbox. Voice is optional for spoken lines; narration is non-voice and this milestone creates no voice assets. No global Backlog, voice, Auto, Skip or ReadHistory redesign is implied.

## Frozen interaction contracts

**S004 authored manual hold:** `AUTHORED_MANUAL_ADVANCE_HOLD` is a planning term only. At L537 the handle and next action remain still. Auto stops/pauses; neither ReadOnly nor All Skip, including held Skip, may cross automatically. An explicit manual Advance, through existing hurry-then-advance ownership after any pending text is fully displayed, releases this one hold and permits L539–542. Returning from a modal, restoring Hide, finishing voice, elapsed time, or a queued automated advance never releases it. Modal arbitration continues to work; a checkpoint revisit must reconstruct the authored hold where applicable rather than auto-open the door. Later implementation decides the smallest compatible mechanism and automation reactivation policy. Global M6 semantics remain intact elsewhere. The source's S007 silence remains normal reading rhythm, not another manual-only hold.

**S008 profile review:** initial 선주 / 해진 / 하나 cards have equal visual weight. Inspection order is free: overview → detail → Back → overview. Reviewed feedback means reviewed, never preferred. All three exact profiles and the corresponding authored immediate Sunju/Haejin thoughts must be delivered once before neutral `브리핑으로 돌아가기` continuation becomes available. A detail may be revisited without repeating its thought; all three remain accessible. No portrait is mandated by source. **Hana has no immediate profile monologue in the source**: preserve the later spoken `하나로 하겠습니다.` and task rationale, and the separate reread/monologue at L1364–1370. Do not move that thought into browsing or invent another.

While browsing, profile controls own pointer/keyboard navigation within existing arbitration; Advance, Auto and Skip cannot close/select cards or leak into normal story. Each associated authored thought still uses the shared reading lifecycle under deliberately integrated ownership; displaying a profile is not itself a durable read-history event. Back/Escape in detail returns to overview; modal Cancel keeps its existing priority and must not select/continue profiles. Continuation explicitly releases browsing ownership to ordinary story. Hana confirmation appears only after her canonical spoken selection; other profiles are then stored as written. There is no route choice, affection score, hidden romance selection, prehighlight or alternate ending. Visits/order/detail position are transient; checkpoint reload may deterministically restart/reconstruct the segment without preserving click order. No new persistence or independent global input owner.

## Current architecture compatibility boundary

M3 fixed poses / Head expression swaps, catalog-backed BG/CG and five slots are constraints, not a promise that every described gesture can animate. Prefer canonical prose plus coherent static framing; review body actions, dog placement and prop occlusion. Five-person composition needs M10-03 review, with BG + sprites, optional ensemble CG, or hybrid remaining open.

M4 has two BGM sources and one-shot SFX ownership; no separate looping Ambient service is established. Existing black / background / CG transitions can supply ordinary scene changes; cropped informational layers, localized door effects and camera disturbance are not assumed supported. M7 Screen Shake is a preference/gate without a consumer. No new Mixer category or audio owner is planned here.

Read-only [audio bridge](../../Assets/_Project/Scripts/Audio/VNYarnAudioCommands.cs) and [audio controller](../../Assets/_Project/Scripts/Audio/VNAudioController.cs) inspection confirms catalog default volume and play/crossfade/pause/resume/stop capability, but no dedicated authored level-lowering operation. S002's lowered continuous morning music therefore needs M10-04 review: retain audible continuity, user mixer settings and deterministic checkpoint reconstruction without a new save field. This freezes intent, not a volume command, second track role or implementation solution. Equipment-to-rain replacement likewise requires reviewed sequencing rather than assuming one-shot gain fading already exists.

M5 reloads stable logical state at dedicated checkpoint entry, not an arbitrary displayed line; transient effects and one-shot cues must not be replayed in re-entry continuation. Reconstruct overlays / profile entry deterministically through a later compatible authored flow without adding serialized fields or smuggling non-idempotent cues into re-entry. M6 full display and authorized consume remain Backlog/read authorities. M8 remains sole durable meta authority; no overlay/profile progress or clinical data becomes MetaProgress. M9 Records / isolated Replay remain intact; this map does not assert that production overlay/browse/audio/fade content is eligible for Replay v1. Production start, Yarn version and existing contracts remain frozen in linked architecture docs.

## Mode introduction sequence

| First appearance | Source cue | Beat | Knowledge boundary |
| --- | --- | --- | --- |
| Normal Dialogue | L19–20 준호 on black | S001-P01 | Ordinary breakfast only |
| Narration | L47–49 mushroom transfer | S001-P03 | Household prose, no clinical meaning |
| Rope Internal | L352–353 신원 확인 | S003-P04 | First private connection communication; ordinary dog image remains |
| Jihun Internal | L355–356 …왜 | S003-P05 | Reply shares private channel, not monologue |
| System Overlay | L364–369 target / visit / response | S003-P06 | First connection facts, after therapist identity is spoken internally |
| Jihun Monologue | L398–400 prior visits | S003-P09 | First explicit thought mode, then already-known family death L402–404 |
| Rope public speech | L926–927 환자 연결 해제 | S006-P07 | Mechanical form first confirmed at L924, then ordinary speaker treatment |

## Beat map reading key

Each scene uses four tables joined by **Beat Ref**. Together they supply all 22 requested fields for every beat. Canon Source includes scene, source span and exact opening cue. Player Knowledge Before is cumulative earlier knowledge with the immediately relevant latest fact; S008 review groups explicitly allow varying order. Visible Characters specify required story presence / composition, not a slot for every actor on every line. An insert can occlude sprites without inventing an exit; restore logical composition afterward. Off-screen presence never substitutes for the explicitly visible five-person table or S005 family.

Implementation Risk: NONE = fits conceptual existing capability; REVIEW = later asset / composition / fixed-pose representation review; SPIKE = integration unproven, queued for M10-04. Blank audio is written as None / silence, not a forgotten cue. Optional household bed is never a prerequisite for clinical or emotional understanding.


## S001 — 다섯 사람이 먹는 아침 (13 beats)

### Meaning and reading

| Beat Ref | Canon Source | Narrative Purpose | Player Knowledge Before | New Information | Text Mode | Speaker |
| --- | --- | --- | --- | --- | --- | --- |
| S001-P01 | S001 L17–24: 화면: 검정. 음악 없음. 프라이팬에 기름이 튀는 소리. 접시가 조리대에 놓인다. | Hear children before seeing household | No treatment or dream context disclosed | Children argue over breakfast | Dialogue | 준호, 나래 |
| S001-P02 | S001 L25–44: 배경: 아침 빛이 드는 주방. 인물보다 프라이팬을 먼저 보여준다. 후라이 다섯 개. | Reveal deliberate breakfast setting before faces | Cumulative earlier beats; latest: Children argue over breakfast | Five eggs and ordinary family teasing | Dialogue | 진희, 준호, 민석, 나래 |
| S001-P03 | S001 L45–68: 음악: 가벼운 아침 테마. 웃음을 강조하는 효과음은 넣지 않는다. | Let family rhythm establish normality | Cumulative earlier beats; latest: Five eggs and ordinary family teasing | Morning music begins; mushroom transfer is narration | Narration → Dialogue | 민석, 진희, 준호 |
| S001-P04 | S001 L69–76: 민석이 접시를 내려다본다. 남은 버섯 하나가 이미 나래 접시로 옮겨져 있다. | Complete small household joke | Cumulative earlier beats; latest: Morning music begins; mushroom transfer is narration | Mushroom has moved to Narae plate | Dialogue | 나래, 준호 |
| S001-P05 | S001 L77–87: 초인종. 진희가 뜨거운 프라이팬을 내려놓는다. | Interrupt breakfast with familiar arrival | Cumulative earlier beats; latest: Mushroom has moved to Narae plate | Doorbell; Jinhee sets hot pan; own meal deferred | Dialogue | 진희, 민석 |
| S001-P06 | S001 L88–91: 서술 | Move child toward visitor and prepare place | Cumulative earlier beats; latest: Doorbell; Jinhee sets hot pan; own meal deferred | Junho feet precede father rising; extra cutlery | Narration | None |
| S001-P07 | S001 L92–117: 배경: 현관. 지훈과 작은 흰 강아지. 강아지는 평범한 동물처럼 보인다. | Introduce visitor and ordinary companion | Cumulative earlier beats; latest: Junho feet precede father rising; extra cutlery | Jihun and white dog arrive; Narae reply off-screen | Dialogue | 준호, 지훈, 나래·화면 밖 |
| S001-P08 | S001 L118–130: 서술 | Return visitor to familiar breakfast | Cumulative earlier beats; latest: Jihun and white dog arrive; Narae reply off-screen | Shoes aligned left; dog under table; familiar coffee | Narration → Dialogue | 진희, 지훈 |
| S001-P09 | S001 L131–141: 인서트: 뒤축이 접힌 작은 운동화. 준호의 뒤꿈치가 밖으로 밀려 있다. | Establish old shoe condition | Cumulative earlier beats; latest: Shoes aligned left; dog under table; familiar coffee | Folded heel; too small and no alternate shoe | Dialogue | 준호, 진희 |
| S001-P10 | S001 L142–150: 진희가 대답하려다 식탁으로 시선을 돌린다. | Let mother sidestep question and visitor notice memo | Cumulative earlier beats; latest: Folded heel; too small and no alternate shoe | Meal redirect; previous handwriting remains | Dialogue → Narration | 진희 |
| S001-P11 | S001 L151–152: 인서트: ‘다음 방문 — 아침 식사 후’. 작성자나 병원 표식은 보이지 않는다. | Show visit continuity without explaining it | Cumulative earlier beats; latest: Meal redirect; previous handwriting remains | 다음 방문 — 아침 식사 후 | None | None |
| S001-P12 | S001 L153–164: 진희 | Resume ordinary guest / family exchange | Cumulative earlier beats; latest: 다음 방문 — 아침 식사 후 | Egg preference joke continues | Dialogue | 진희, 지훈, 민석 |
| S001-P13 | S001 L165–166: 화면: 다섯 사람의 식탁. 비어 있는 의자 없음. 강아지가 지훈의 발 옆에 눕는다. | Fix occupied family / visitor composition | Cumulative earlier beats; latest: Egg preference joke continues | All five at table; no empty chair; Rope at Jihun feet | None | None |

### Staging

| Beat Ref | Background Intent | Visible Characters | Off-screen Characters | Expression/State | Insert/CG Intent | Information Overlay |
| --- | --- | --- | --- | --- | --- | --- |
| S001-P01 | Black | None | 준호, 나래 heard before image | Ordinary banter, no spectacle | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S001-P02 | Morning kitchen / table | 진희, 민석, 나래, 준호 after pan-first framing; no Jihun / dog yet | None authored | Ordinary warm household; fixed-pose composition | Pan-first close framing with five eggs | None; no clinical/context UI before S003 |
| S001-P03 | Morning kitchen / table | 진희, 민석, 나래, 준호 after pan-first framing; no Jihun / dog yet | None authored | Junho pouts; familiar affection | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S001-P04 | Morning kitchen / table | 진희, 민석, 나래, 준호 after pan-first framing; no Jihun / dog yet | None authored | Minseok looks down; Narae matter-of-fact | Wide prop detail sufficient; no mandatory extra insert | None; no clinical/context UI before S003 |
| S001-P05 | Morning kitchen / table | 진희, 민석, 나래, 준호 after pan-first framing; no Jihun / dog yet | None authored | Ordinary warm household; fixed-pose composition | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S001-P06 | Morning kitchen / table | Family; Junho begins moving toward entry | None authored | Ordinary warm household; fixed-pose composition | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S001-P07 | Family-home entrance | 지훈, 준호, ordinary white dog | 나래·화면 밖; 진희 / 민석 remain kitchen | Ordinary warm household; fixed-pose composition | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S001-P08 | Entry shoes then kitchen/table | 지훈, 진희; family at table; dog under table | None authored | Ordinary warm household; fixed-pose composition | Wide scene sufficient for aligned shoes; no new insert | None; no clinical/context UI before S003 |
| S001-P09 | Morning kitchen / table | 진희, 민석, 나래, 준호; 지훈 joins after arrival; ordinary Rope under table after arrival | None authored | Child practical complaint; mother hesitant | Authored old-shoe insert; heel outside visible | None; no clinical/context UI before S003 |
| S001-P10 | Morning kitchen / table | 진희, 민석, 나래, 준호; 지훈 joins after arrival; ordinary Rope under table after arrival | None authored | Jinhee begins answer then looks to table | Memo not yet replaced with a clinical clue | None; no clinical/context UI before S003 |
| S001-P11 | Morning kitchen / table | Prop focus; established people remain in scene | None authored | Ordinary warm household; fixed-pose composition | Exact authored memo close-up | None; no clinical/context UI before S003 |
| S001-P12 | Morning kitchen / table | 진희, 민석, 나래, 준호; 지훈 joins after arrival; ordinary Rope under table after arrival | None authored | Ordinary warm household; fixed-pose composition | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S001-P13 | Morning kitchen / table | 진희, 민석, 나래, 준호, 지훈 plus ordinary Rope | None authored | Ordinary warm household; fixed-pose composition | BG/sprites/ensemble CG/hybrid unresolved; evaluate clear five-person view | None; no clinical/context UI before S003 |

### Sound and transition

| Beat Ref | BGM Intent | Ambient Intent | SFX Cue | Transition Intent |
| --- | --- | --- | --- | --- |
| S001-P01 | Silence | No continuous bed required; frying / plate cues supply opening life | Frying oil; plate on counter | Black opening; no reveal yet |
| S001-P02 | Silence | No continuous bed required; frying / plate cues supply opening life | None added | Black to Scene; pan before character focus |
| S001-P03 | Start light morning theme only at L45 | Optional ordinary home bed (technical review); authored action sounds take precedence | No laugh-emphasis effect | Presentation State Change / same composition |
| S001-P04 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | None added | Presentation State Change / same composition |
| S001-P05 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | Doorbell at L77; no extra surprise hit | Presentation State Change / same composition |
| S001-P06 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | Child footsteps; optional drawer / cutlery sound | Audio-led Transition toward entrance |
| S001-P07 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | None added | Cut to entrance |
| S001-P08 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | None added | Cut back to table after entry actions |
| S001-P09 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | None added | Cut to insert; retain logical household composition |
| S001-P10 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | None added | Return from shoe insert; look toward memo |
| S001-P11 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | None added | Cut to readable insert; no clue effect |
| S001-P12 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | None added | Return from memo to table |
| S001-P13 | Continue light morning theme | Optional ordinary home bed (technical review); authored action sounds take precedence | None added | Presentation State Change / same composition |

### Interaction, resources and constraints

| Beat Ref | Interaction State | Canon Constraints | Asset Requirement Refs | Implementation Risk | Notes |
| --- | --- | --- | --- | --- | --- |
| S001-P01 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-FX-001](M10_MASTER_ASSET_GAP_LIST.md#req-fx-001), [REQ-SFX-001](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-001), [REQ-SFX-002](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-002), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | NONE | Preserve each authored paragraph boundary. |
| S001-P02 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-INS-001](M10_MASTER_ASSET_GAP_LIST.md#req-ins-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-EXP-001](M10_MASTER_ASSET_GAP_LIST.md#req-exp-001), [REQ-EXP-006](M10_MASTER_ASSET_GAP_LIST.md#req-exp-006) | REVIEW | M10-03 review: BG crop versus dedicated insert. |
| S001-P03 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-BGM-001](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-001), [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-EXP-001](M10_MASTER_ASSET_GAP_LIST.md#req-exp-001), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-AMB-001](M10_MASTER_ASSET_GAP_LIST.md#req-amb-001) | NONE | Preserve each authored paragraph boundary. |
| S001-P04 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-EXP-005](M10_MASTER_ASSET_GAP_LIST.md#req-exp-005), [REQ-EXP-006](M10_MASTER_ASSET_GAP_LIST.md#req-exp-006) | NONE | Preserve each authored paragraph boundary. |
| S001-P05 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-SFX-003](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-003), [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003) | NONE | Preserve each authored paragraph boundary. |
| S001-P06 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-SFX-004](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-004), [REQ-SFX-005](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-005), [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001) | NONE | Preserve each authored paragraph boundary. |
| S001-P07 | Normal authored paragraph advances | Dog reads as normal animal; no internal channel / patient label | [REQ-BG-002](M10_MASTER_ASSET_GAP_LIST.md#req-bg-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004) | NONE | Preserve each authored paragraph boundary. |
| S001-P08 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-BG-002](M10_MASTER_ASSET_GAP_LIST.md#req-bg-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | NONE | Preserve each authored paragraph boundary. |
| S001-P09 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-INS-002](M10_MASTER_ASSET_GAP_LIST.md#req-ins-002), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | NONE | Preserve each authored paragraph boundary. |
| S001-P10 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-EXP-002](M10_MASTER_ASSET_GAP_LIST.md#req-exp-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | NONE | Preserve each authored paragraph boundary. |
| S001-P11 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-INS-003](M10_MASTER_ASSET_GAP_LIST.md#req-ins-003) | NONE | Preserve each authored paragraph boundary. |
| S001-P12 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-EXP-001](M10_MASTER_ASSET_GAP_LIST.md#req-exp-001) | NONE | Preserve each authored paragraph boundary. |
| S001-P13 | Normal authored paragraph advances | No deep-consciousness/patient/therapy clue; dog ordinary; five places intentional | [REQ-BG-001](M10_MASTER_ASSET_GAP_LIST.md#req-bg-001), [REQ-CG-001](M10_MASTER_ASSET_GAP_LIST.md#req-cg-001), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006) | REVIEW | M10-03 COMPOSITION REVIEW; five slots do not imply five full-size sprites plus dog fit. |


## S002 — 돌아온 다음의 일이 없는 여행 (10 beats)

### Meaning and reading

| Beat Ref | Canon Source | Narrative Purpose | Player Knowledge Before | New Information | Text Mode | Speaker |
| --- | --- | --- | --- | --- | --- | --- |
| S002-P01 | S002 L171–196: 배경: 거실. 아침 식사가 끝난 직후. 음악은 생활 소리 뒤로 낮춘다. | Carry breakfast into living conversation | Five-person breakfast and familiar visits; no clinical explanation | Photos prepared; beach trip recollected | Dialogue | 진희, 지훈, 나래, 준호, 민석 |
| S002-P02 | S002 L197–215: 서술 | Establish travel photo and mother absence | Cumulative earlier beats; latest: Photos prepared; beach trip recollected | Junho sandy snack photo; Jinhee ankle injury / laundry | Narration → Dialogue | 지훈, 진희, 민석, 나래 |
| S002-P03 | S002 L216–227: 지훈 | Probe what happened after return | Cumulative earlier beats; latest: Junho sandy snack photo; Jinhee ankle injury / laundry | Packed spare clothes; laundry answer trails off | Dialogue | 지훈, 진희 |
| S002-P04 | S002 L228–247: 진희의 표정 변화는 작게. 음악을 끊지 않는다. | Leave contradiction in ordinary performance | Cumulative earlier beats; latest: Packed spare clothes; laundry answer trails off | Wet bag zipper; uncertain returner, husband deflects | Dialogue | 진희, 지훈, 민석 |
| S002-P05 | S002 L248–250: 서술 | Show return to earlier photograph | Cumulative earlier beats; latest: Wet bag zipper; uncertain returner, husband deflects | Minseok turns photo forward to sunny sea | Narration | None |
| S002-P06 | S002 L251–263: 준호 | Expose tomorrow that has not arrived | Cumulative earlier beats; latest: Minseok turns photo forward to sunny sea | New shoe promised for tomorrow again | Dialogue | 준호, 진희 |
| S002-P07 | S002 L264–289: 진희의 시선이 복도 끝으로 간다. 지훈은 그 시선을 따라가되 자리에서 바로 일어나지 않는다. | Connect closed room and new shoe purchase | Cumulative earlier beats; latest: New shoe promised for tomorrow again | Jinhee hall gaze; size / laces / father next-time offer | Dialogue | 지훈, 진희, 준호, 민석, 나래 |
| S002-P08 | S002 L290–293: 서술 | Begin Jihun personal pause without added thought | Cumulative earlier beats; latest: Jinhee hall gaze; size / laces / father next-time offer | Cup finger stops during children planning | Narration | None |
| S002-P09 | S002 L294–299: 효과음: 의자 다리가 바닥을 스치는 소리를 한 번만 가까이 들려준다. 별도 아이 목소리나 ‘아빠’라는 환청은 넣지 않는다. | Let one close sound carry disturbance | Cumulative earlier beats; latest: Cup finger stops during children planning | Chair scrape; prose of table, cup and hands | Narration | None |
| S002-P10 | S002 L300–309: 진희 | Carry symptom forward and dog notices | Cumulative earlier beats; latest: Chair scrape; prose of table, cup and hands | Pain remains after cup set down; dog raises head | Dialogue → Narration | 진희, 지훈 |

### Staging

| Beat Ref | Background Intent | Visible Characters | Off-screen Characters | Expression/State | Insert/CG Intent | Information Overlay |
| --- | --- | --- | --- | --- | --- | --- |
| S002-P01 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Small restrained expressions; Jihun remains seated until later hall approach | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S002-P02 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Small restrained expressions; Jihun remains seated until later hall approach | Photo visible in wide view or focused crop if needed | None; no clinical/context UI before S003 |
| S002-P03 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Recall falters quietly | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S002-P04 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Small expression shift at L228 | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S002-P05 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Small restrained expressions; Jihun remains seated until later hall approach | Photo change only if readability warrants; no new memory scene | None; no clinical/context UI before S003 |
| S002-P06 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Ordinary family exchange; restrained hesitation | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S002-P07 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Jihun follows gaze while staying seated | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S002-P08 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Finger holds coffee handle; restrained pause | Wide or close framing sufficient; no mandatory cup insert | None; no clinical/context UI before S003 |
| S002-P09 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Small restrained expressions; Jihun remains seated until later hall approach | None; wide composition sufficient | None; no clinical/context UI before S003 |
| S002-P10 | Living room after breakfast | 진희, 지훈, 민석, 나래, 준호; ordinary Rope nearby | None required; family remains in shared living context | Jihun composed; ordinary dog alert | None; wide composition sufficient | None; no clinical/context UI before S003 |

### Sound and transition

| Beat Ref | BGM Intent | Ambient Intent | SFX Cue | Transition Intent |
| --- | --- | --- | --- | --- |
| S002-P01 | Continue morning theme, lower behind life sounds | Optional home bed, technical review; no suspense drone | None added | Cut: living room immediately after breakfast |
| S002-P02 | Morning theme lowered behind life sounds; never stop for contradictions | Optional home bed, technical review; no suspense drone | None added | Presentation State Change / same composition |
| S002-P03 | Morning theme lowered behind life sounds; never stop for contradictions | Optional home bed, technical review; no suspense drone | None added | Presentation State Change / same composition |
| S002-P04 | Continue lowered music without stop | Optional home bed, technical review; no suspense drone | None added | Presentation State Change / same composition |
| S002-P05 | Morning theme lowered behind life sounds; never stop for contradictions | Optional home bed, technical review; no suspense drone | None added | Presentation State Change / same composition |
| S002-P06 | Morning theme lowered behind life sounds; never stop for contradictions | Optional home bed, technical review; no suspense drone | None added | Presentation State Change / same composition |
| S002-P07 | Morning theme lowered behind life sounds; never stop for contradictions | Optional home bed, technical review; no suspense drone | None added | Gaze within same composition, not a location jump |
| S002-P08 | Morning theme lowered behind life sounds; never stop for contradictions | Optional home bed, technical review; no suspense drone | None added | Presentation State Change / same composition |
| S002-P09 | Morning theme lowered behind life sounds; never stop for contradictions | Optional home bed, technical review; no suspense drone | Exactly one close chair-leg scrape at L294 | Presentation State Change / same composition |
| S002-P10 | Lowered morning music continues; no sting | Optional home bed, technical review; no suspense drone | None added | Presentation State Change / same composition |

### Interaction, resources and constraints

| Beat Ref | Interaction State | Canon Constraints | Asset Requirement Refs | Implementation Risk | Notes |
| --- | --- | --- | --- | --- | --- |
| S002-P01 | Normal authored paragraph advances | Contradictions carry scene; no stinger/glitch/clue popup or invented hallucination | [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003), [REQ-INS-004](M10_MASTER_ASSET_GAP_LIST.md#req-ins-004), [REQ-BGM-001](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-001), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005) | SPIKE | M10-04 authored level-continuity review; no BGM restart or user-volume mutation assumed. |
| S002-P02 | Normal authored paragraph advances | Contradictions carry scene; no stinger/glitch/clue popup or invented hallucination | [REQ-INS-004](M10_MASTER_ASSET_GAP_LIST.md#req-ins-004), [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003), [REQ-EXP-001](M10_MASTER_ASSET_GAP_LIST.md#req-exp-001) | NONE | Preserve each authored paragraph boundary. |
| S002-P03 | Normal authored paragraph advances | Contradictions carry scene; no stinger/glitch/clue popup or invented hallucination | [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003), [REQ-EXP-002](M10_MASTER_ASSET_GAP_LIST.md#req-exp-002) | NONE | Preserve each authored paragraph boundary. |
| S002-P04 | Normal authored paragraph advances | No clue popup, sting or glitch at hesitation | [REQ-EXP-002](M10_MASTER_ASSET_GAP_LIST.md#req-exp-002), [REQ-BGM-001](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-001), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003) | NONE | Preserve each authored paragraph boundary. |
| S002-P05 | Normal authored paragraph advances | Contradictions carry scene; no stinger/glitch/clue popup or invented hallucination | [REQ-INS-004](M10_MASTER_ASSET_GAP_LIST.md#req-ins-004), [REQ-EXP-005](M10_MASTER_ASSET_GAP_LIST.md#req-exp-005) | NONE | Preserve each authored paragraph boundary. |
| S002-P06 | Normal authored paragraph advances | Contradictions carry scene; no stinger/glitch/clue popup or invented hallucination | [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-006](M10_MASTER_ASSET_GAP_LIST.md#req-exp-006) | NONE | Preserve each authored paragraph boundary. |
| S002-P07 | Normal authored paragraph advances | Contradictions carry scene; no stinger/glitch/clue popup or invented hallucination | [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003), [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004), [REQ-EXP-002](M10_MASTER_ASSET_GAP_LIST.md#req-exp-002) | NONE | Preserve each authored paragraph boundary. |
| S002-P08 | Normal authored paragraph advances | Contradictions carry scene; no stinger/glitch/clue popup or invented hallucination | [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-EXP-004](M10_MASTER_ASSET_GAP_LIST.md#req-exp-004) | NONE | Preserve each authored paragraph boundary. |
| S002-P09 | Normal authored paragraph advances | No separate child voice or 아빠 hallucination; no invented inner memory | [REQ-SFX-006](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-006), [REQ-EXP-004](M10_MASTER_ASSET_GAP_LIST.md#req-exp-004), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | NONE | Preserve each authored paragraph boundary. |
| S002-P10 | Normal authored paragraph advances | Contradictions carry scene; no stinger/glitch/clue popup or invented hallucination | [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006), [REQ-EXP-009](M10_MASTER_ASSET_GAP_LIST.md#req-exp-009), [REQ-EXP-004](M10_MASTER_ASSET_GAP_LIST.md#req-exp-004) | NONE | Preserve each authored paragraph boundary. |


## S003 — 강아지가 부르는 이름 (10 beats)

### Meaning and reading

| Beat Ref | Canon Source | Narrative Purpose | Player Knowledge Before | New Information | Text Mode | Speaker |
| --- | --- | --- | --- | --- | --- | --- |
| S003-P01 | S003 L314–337: 배경: 거실에서 이어지는 복도. 닫힌 방문. | Approach avoided door without force | Return-life contradiction and Jihun pain are visible; family nature not yet explained | Closed room; Minseok will not yield narrow passage | Dialogue → Narration | 지훈, 진희, 민석 |
| S003-P02 | S003 L338–342: 효과음: 거실에서 준호가 웃는 소리. 잠시 뒤 똑같은 길이와 높이로 한 번 반복된다. | Repeat familiar life sound precisely | Cumulative earlier beats; latest: Closed room; Minseok will not yield narrow passage | Junho laugh repeats and off-screen reply follows | Dialogue | 준호·화면 밖 |
| S003-P03 | S003 L343–349: 지훈의 시선이 손잡이에서 식탁으로 흔들린다. 남편의 얼굴을 위협적으로 변형하지 않는다. | Place perception change before channel explanation | Cumulative earlier beats; latest: Junho laugh repeats and off-screen reply follows | Gaze handle→table; hall feels distant; own plate remains | Narration | None |
| S003-P04 | S003 L350–353: 흰 강아지가 지훈의 발목에 앞발을 댄다. 음악 페이드아웃. | Introduce first private Rope channel after touch | Cumulative earlier beats; latest: Gaze handle→table; hall feels distant; own plate remains | Ordinary dog ankle touch; Rope asks identity | Internal | 로프·내부 |
| S003-P05 | S003 L354–363: 지훈·내부 | Introduce Jihun internal reply and occupation | Cumulative earlier beats; latest: Ordinary dog ankle touch; Rope asks identity | …왜; identity sequence; deep-consciousness therapist | Internal | 지훈·내부, 로프·내부 |
| S003-P06 | S003 L364–376: 첫 접속 정보 표시. 간결한 반투명 인터페이스. | Reveal first connection data and place | Cumulative earlier beats; latest: …왜; identity sequence; deep-consciousness therapist | Target Jinhee; third visit; delayed response; deep generated realm | None (structured information) → Internal | 지훈·내부, 로프·내부 |
| S003-P07 | S003 L377–386: 서술 | Restore orientation and distinguish public reply | Cumulative earlier beats; latest: Target Jinhee; third visit; delayed response; deep generated realm | Hand lowered; passage normal; equipment check spoken to Jinhee | Narration → Dialogue | 진희, 지훈 |
| S003-P08 | S003 L387–397: 민석이 조금 비켜선다. 지훈은 거실 쪽으로 먼저 물러난다. | Withdraw rather than force access; record symptoms | Cumulative earlier beats; latest: Hand lowered; passage normal; equipment check spoken to Jinhee | Minseok yields slightly; Jihun retreats; 34-minute estimate | Internal | 로프·내부, 지훈·내부 |
| S003-P09 | S003 L398–405: 지훈·독백 | Introduce Jihun monologue as known clinical history | Cumulative earlier beats; latest: Minseok yields slightly; Jihun retreats; 34-minute estimate | First two visits; family already dead; return memory unresolved | Monologue | 지훈·독백 |
| S003-P10 | S003 L406–417: 로프·내부 | Keep approach procedural and return to shoe memory | Cumulative earlier beats; latest: First two visits; family already dead; return memory unresolved | Defense response noted; pause and retry, door untouched | Internal → Dialogue | 로프·내부, 지훈·내부, 진희, 지훈 |

### Staging

| Beat Ref | Background Intent | Visible Characters | Off-screen Characters | Expression/State | Insert/CG Intent | Information Overlay |
| --- | --- | --- | --- | --- | --- | --- |
| S003-P01 | Hall linked to living room / table; closed door | 지훈, 진희, 민석; ordinary Rope beside ankle | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Minseok rises and does not move aside; never grabs Jihun | None; wide composition sufficient | None; first connection overlay not yet revealed |
| S003-P02 | Hall linked to living room / table; closed door | 지훈, 진희, 민석; ordinary Rope beside ankle | 준호·화면 밖 from living room; Narae there | Quiet obstruction without physical restraint; dog visually ordinary | None; wide composition sufficient | None; first connection overlay not yet revealed |
| S003-P03 | Hall linked to living room / table; closed door | 지훈, 진희, 민석; ordinary Rope beside ankle | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Jihun wavering gaze; Minseok face unaltered | None; wide composition sufficient | None; first connection overlay not yet revealed |
| S003-P04 | Hall linked to living room / table; closed door | 지훈, 진희, 민석; ordinary Rope beside ankle | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Dog paw at ankle; ordinary appearance unchanged | None; wide composition sufficient | None; first connection overlay not yet revealed |
| S003-P05 | Hall linked to living room / table; closed door | 지훈, 진희, 민석; ordinary Rope beside ankle | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Quiet obstruction without physical restraint; dog visually ordinary | None; wide composition sufficient | None; first connection overlay not yet revealed |
| S003-P06 | Hall linked to living room / table; closed door | 지훈, 진희, 민석; ordinary Rope beside ankle | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Quiet obstruction without physical restraint; dog visually ordinary | None; wide composition sufficient | First compact translucent panel: 대상 진희 / 방문 3회차 / 신원 응답 지연 |
| S003-P07 | Hall linked to living room / table; closed door | 지훈, 진희, 민석; ordinary Rope beside ankle | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Hand down, ordinary corridor restored | None; wide composition sufficient | None added; established state remains secondary if retained |
| S003-P08 | Hall toward living room | 지훈, 진희; 민석 / children remain living-room family context; ordinary Rope | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Quiet obstruction without physical restraint; dog visually ordinary | None; wide composition sufficient | Retain first compact facts; 34분 remains authored internal speech, not invented countdown |
| S003-P09 | Living-room side after Jihun withdrawal | 지훈, 진희; 민석 / children remain living-room family context; ordinary Rope | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Quiet obstruction without physical restraint; dog visually ordinary | None; wide composition sufficient | None added; established state remains secondary if retained |
| S003-P10 | Living-room approach resumed | 지훈, 진희; 민석 / children remain living-room family context; ordinary Rope | 나래 / 준호 in living context; 준호·화면 밖 only as authored | Quiet obstruction without physical restraint; dog visually ordinary | None; wide composition sufficient | None added; established state remains secondary if retained |

### Sound and transition

| Beat Ref | BGM Intent | Ambient Intent | SFX Cue | Transition Intent |
| --- | --- | --- | --- | --- |
| S003-P01 | Lowered morning theme until L350 fade; silence thereafter | Optional home bed; repeat laugh is discrete SFX | None added | Cut to hall linked to living room |
| S003-P02 | Lowered morning theme until L350 fade; silence thereafter | Optional home bed; repeat laugh is discrete SFX | Same laugh once, then identical length/pitch once again | Presentation State Change / same composition |
| S003-P03 | Lowered morning theme until L350 fade; silence thereafter | Optional home bed; repeat laugh is discrete SFX | None added | Presentation State Change; subtle framing / prose |
| S003-P04 | Fade out morning theme at L350 | Optional home bed; repeat laugh is discrete SFX | None added | Presentation State Change to explicitly labeled internal channel |
| S003-P05 | Silence after authored dog-touch fade | Optional home bed; repeat laugh is discrete SFX | None added | Presentation State Change / same composition |
| S003-P06 | Silence after authored dog-touch fade | Optional home bed; repeat laugh is discrete SFX | None added | Presentation State Change / same composition |
| S003-P07 | Silence after authored dog-touch fade | Optional home bed; repeat laugh is discrete SFX | None added | Presentation State Change to stable hall / public dialogue |
| S003-P08 | Silence after authored dog-touch fade | Optional home bed; repeat laugh is discrete SFX | None added | Cut / composition return to living room |
| S003-P09 | Silence after authored dog-touch fade | Optional home bed; repeat laugh is discrete SFX | None added | Presentation State Change / same composition |
| S003-P10 | Silence after authored dog-touch fade | Optional home bed; repeat laugh is discrete SFX | None added | Presentation State Change / same composition |

### Interaction, resources and constraints

| Beat Ref | Interaction State | Canon Constraints | Asset Requirement Refs | Implementation Risk | Notes |
| --- | --- | --- | --- | --- | --- |
| S003-P01 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-EXP-005](M10_MASTER_ASSET_GAP_LIST.md#req-exp-005) | NONE | Preserve each authored paragraph boundary. |
| S003-P02 | Normal authored paragraph advances | Diegetic repetition, not comic laugh track or new haunting voice | [REQ-SFX-007](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-007), [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004) | NONE | Preserve each authored paragraph boundary. |
| S003-P03 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-FX-004](M10_MASTER_ASSET_GAP_LIST.md#req-fx-004), [REQ-EXP-004](M10_MASTER_ASSET_GAP_LIST.md#req-exp-004), [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004) | REVIEW | Framing and narration may suffice; no camera / shader assumed. |
| S003-P04 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006), [REQ-EXP-009](M10_MASTER_ASSET_GAP_LIST.md#req-exp-009), [REQ-BGM-001](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-001), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | First Rope Internal L352–353; mode/focus/lifecycle spike. |
| S003-P05 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | SPIKE | First Jihun Internal L355–356; first explicit profession only here. |
| S003-P06 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-UI-002](M10_MASTER_ASSET_GAP_LIST.md#req-ui-002), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | System Overlay first L364–369; no fake Backlog data lines. |
| S003-P07 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-FX-004](M10_MASTER_ASSET_GAP_LIST.md#req-fx-004), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | REVIEW | Preserve each authored paragraph boundary. |
| S003-P08 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003), [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-UI-002](M10_MASTER_ASSET_GAP_LIST.md#req-ui-002) | SPIKE | Withdrawn posture/actions must fit fixed-pose or prose. |
| S003-P09 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003) | SPIKE | First Monologue L398–400; cumulative knowledge shift, not surprise clue UI. |
| S003-P10 | Normal authored paragraph advances | No threatening Minseok transformation or premature cyber reveal; first clinical/channel reveal only here | [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | SPIKE | No additional player choice or forced door action. |


## S004 — 새 신발이 있는 방 (16 beats)

### Meaning and reading

| Beat Ref | Canon Source | Narrative Purpose | Player Knowledge Before | New Information | Text Mode | Speaker |
| --- | --- | --- | --- | --- | --- | --- |
| S004-P01 | S004 L422–435: 배경: 거실. 지훈은 준호의 작은 신발을 집어 진희에게 건넨다. | Use old shoe rather than announce death | Third connection known; family death already known to Jihun; door untouched | Worn heel contrasted with new firm heel | Dialogue | 지훈, 진희 |
| S004-P02 | S004 L436–458: 진희가 기억을 더듬듯 두 손으로 상자의 크기를 만든다. | Follow physical memory association in order | Cumulative earlier beats; latest: Worn heel contrasted with new firm heel | Hands shape box; Friday receipt before Saturday sea trip | Dialogue | 진희, 지훈 |
| S004-P03 | S004 L459–480: 서술 | Let repeating tomorrow be noticed by patient | Cumulative earlier beats; latest: Hands shape box; Friday receipt before Saturday sea trip | Child pulls socks; still never tried new shoe; tomorrow repeats | Narration → Dialogue | 진희, 준호, 민석 |
| S004-P04 | S004 L481–486: 지훈의 상태창은 표시하되 수치 상승 애니메이션은 넣지 않는다. | Separate measured state from inner reasoning | Cumulative earlier beats; latest: Child pulls socks; still never tried new shoe; tomorrow repeats | Static status; shoe embodies wished-for walking day | Monologue | 지훈·독백 |
| S004-P05 | S004 L487–501: 진희 | Let patient question postponed access | Cumulative earlier beats; latest: Static status; shoe embodies wished-for walking day | Box in room; husband later; ankle already healed | Dialogue | 진희, 민석 |
| S004-P06 | S004 L502–522: 진희가 일어난다. 민석은 이번에도 복도 쪽에 서지만, 진희의 진로를 강제로 막지 않는다. | Move toward door and resolve returner contradiction | Cumulative earlier beats; latest: Box in room; husband later; ankle already healed | Jinhee rises; returned bag was not brought by husband | Dialogue → Narration | 진희, 지훈 |
| S004-P07 | S004 L523–536: 진희가 닫힌 문 앞에 선다. 지훈은 한 걸음 뒤에 머문다. | Keep decision hers, Jihun a step behind | Cumulative earlier beats; latest: Jinhee rises; returned bag was not brought by husband | Opening may restore reason; Jihun cannot bypass avoidance | Dialogue | 진희, 지훈 |
| S004-P08 | S004 L537–537: 긴 침묵은 자동 진행시키지 않는다. 플레이어가 다음 문장으로 넘길 때 손잡이를 움직인다. | Hold long silence for deliberate manual progression | Cumulative earlier beats; latest: Opening may restore reason; Jihun cannot bypass avoidance | Door remains still until player explicitly advances | None | None |
| S004-P09 | S004 L538–543: 서술 | Release into Jinhee personally opening | Cumulative earlier beats; latest: Door remains still until player explicitly advances | Old shoe on floor; Jinhee opens room herself | Narration | None |
| S004-P10 | S004 L544–548: 배경: 보관방. 신발 상자, 돌려받은 여행 가방, 소지품 봉투. 서류는 가까이 다가간 뒤에만 읽히게 한다. 음악 없음. | Reveal avoided objects before document legibility | Cumulative earlier beats; latest: Old shoe on floor; Jinhee opens room herself | Box, returned bag, belongings envelopes; not laundry | Dialogue | 진희 |
| S004-P11 | S004 L549–555: 서술 | Expose physical contents of returned bag | Cumulative earlier beats; latest: Box, returned bag, belongings envelopes; not laundry | Sandy towel and half-eaten snack wrapper | Narration → Dialogue | 진희 |
| S004-P12 | S004 L556–563: 서술 | Make previously read record readable at proximity | Cumulative earlier beats; latest: Sandy towel and half-eaten snack wrapper | Envelope slips; three names share accident date | Narration → Dialogue | 진희 |
| S004-P13 | S004 L564–589: 진희가 서류를 덮는다. 손을 치웠다가 다시 펼친다. | Let acknowledgment remain patient-led | Cumulative earlier beats; latest: Envelope slips; three names share accident date | Paper covered/reopened; all three failed to return; external records known | Dialogue | 진희, 지훈 |
| S004-P14 | S004 L590–605: 서술 | Show unused future alongside selective forgetting | Cumulative earlier beats; latest: Paper covered/reopened; all three failed to return; external records known | Unused sole; removed memories did not remove milk habit | Narration → Dialogue | 진희 |
| S004-P15 | S004 L606–615: 서술 | Retain child outside room despite acknowledgment | Cumulative earlier beats; latest: Unused sole; removed memories did not remove milk habit | Junho calls; Jinhee cannot put shoe down | Narration → Dialogue | 준호·화면 밖, 진희 |
| S004-P16 | S004 L616–619: 지훈·독백 | Separate fact acceptance from return response | Cumulative earlier beats; latest: Junho calls; Jinhee cannot put shoe down | Fact accepted; no return reaction yet | Monologue | 지훈·독백 |

### Staging

| Beat Ref | Background Intent | Visible Characters | Off-screen Characters | Expression/State | Insert/CG Intent | Information Overlay |
| --- | --- | --- | --- | --- | --- | --- |
| S004-P01 | Living room with old shoe / closed hall in background | 진희, 지훈 foreground; 준호 / 민석 / 나래 in living-room context | None specifically authored | Jihun inspects worn heel; Jinhee ordinary recollection / small hesitation | Reuse old-shoe detail if wear unclear; wide handoff otherwise | No new overlay cue; previous connection layer may already have receded |
| S004-P02 | Living room with old shoe / closed hall in background | 진희, 지훈 foreground; 준호 / 민석 / 나래 in living-room context | None specifically authored | Jinhee shapes remembered box with hands | Wide gesture/prose sufficient; new shoe not shown as already used | No new overlay cue; previous connection layer may already have receded |
| S004-P03 | Living room with old shoe / closed hall in background | 진희, 지훈 foreground; 준호 / 민석 / 나래 in living-room context | None specifically authored | Jinhee notices repetition herself | None; wide composition sufficient | No new overlay cue; previous connection layer may already have receded |
| S004-P04 | Living room with old shoe / closed hall in background | 진희, 지훈 foreground; 준호 / 민석 / 나래 in living-room context | None specifically authored | Jinhee uncertain recall; Jihun clinical status and authored private reasoning | None; wide composition sufficient | Static Jihun status window; no new numeric field invented |
| S004-P05 | Living room with old shoe / closed hall in background | 진희, 지훈 foreground; 준호 / 민석 / 나래 in living-room context | None specifically authored | Jinhee questions deferment; father remains ordinary | None; wide composition sufficient | Static status secondary if retained; no extra fields or animated values |
| S004-P06 | Living room toward hall | Jinhee, Jihun, Minseok; children in living context | Children remain living-room context; no authored off-screen line here | Minseok stands but does not forcibly block; Jinhee looks to silent husband | None; wide composition sufficient | Static status secondary if retained; no extra fields or animated values |
| S004-P07 | Closed-door hall | 진희 at closed door, 지훈 a step behind; 민석 nearby without force | Children remain living-room context | Patient deciding, no player morality / yes-no choice | None; wide composition sufficient | Static status secondary if retained; no extra fields or animated values |
| S004-P08 | Closed-door hall | 진희 at closed door, 지훈 a step behind; 민석 nearby without force | Children remain living-room context | Stillness; handle must not move yet | None; wide composition sufficient | Static status secondary if retained; no extra fields or animated values |
| S004-P09 | Doorway as Jinhee opens | 진희 opens door, 지훈 behind; 민석 / children remain living side | Children remain living-room context | Jinhee handles door; Jihun remains behind | None; wide composition sufficient | Static status secondary if retained; no extra fields or animated values |
| S004-P10 | Storage room; living family remains outside | 진희 and 지훈 in storage room; family remains outside | 민석, 나래, 준호 remain outside storage; 준호 speaks off-screen at L610 | Jinhee handles avoided objects; recognition develops from bag to paper | Wide props sufficient here; paper deliberately unreadable | Static status secondary if retained; no extra fields or animated values |
| S004-P11 | Storage room; living family remains outside | 진희 and 지훈 in storage room; family remains outside | 민석, 나래, 준호 remain outside storage; 준호 speaks off-screen at L610 | Jinhee handles avoided objects; recognition develops from bag to paper | Likely focused bag contents if scale requires | Static status secondary if retained; no extra fields or animated values |
| S004-P12 | Storage room; living family remains outside | 진희 and 지훈 in storage room; family remains outside | 민석, 나래, 준호 remain outside storage; 준호 speaks off-screen at L610 | Jinhee handles avoided objects; recognition develops from bag to paper | Readable accident-paper close-up now; exact names, same date without invented value | Static status secondary if retained; no extra fields or animated values |
| S004-P13 | Storage room; living family remains outside | 진희 and 지훈 in storage room; family remains outside | 민석, 나래, 준호 remain outside storage; 준호 speaks off-screen at L610 | Jinhee covers paper, removes hand, reopens; grief; Jihun factual | Cover / reopen same document detail; return to faces as dialogue needs | Static status secondary if retained; no extra fields or animated values |
| S004-P14 | Storage room; living family remains outside | 진희 and 지훈 in storage room; family remains outside | 민석, 나래, 준호 remain outside storage; 준호 speaks off-screen at L610 | Jinhee grieves with new shoe; Jihun remains composed / procedural | Focused unused new sole in palm justified by condition | Static status secondary if retained; no extra fields or animated values |
| S004-P15 | Storage room; living family remains outside | Jinhee, Jihun; child remains outside room | 준호·화면 밖: 엄마. 찾았어? | Jinhee turns with shoe still held | None; wide composition sufficient | Static status secondary if retained; no extra fields or animated values |
| S004-P16 | Storage room; living family remains outside | 진희 and 지훈 in storage room; family remains outside | 민석, 나래, 준호 remain outside storage; 준호 speaks off-screen at L610 | Jinhee grieves with new shoe; Jihun remains composed / procedural | None; wide composition sufficient | Do not fabricate a success meter; status secondary only |

### Sound and transition

| Beat Ref | BGM Intent | Ambient Intent | SFX Cue | Transition Intent |
| --- | --- | --- | --- | --- |
| S004-P01 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Cut to living room; shoe handoff |
| S004-P02 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Presentation State Change / same composition |
| S004-P03 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Presentation State Change / same composition |
| S004-P04 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Presentation State Change / same composition |
| S004-P05 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Presentation State Change / same composition |
| S004-P06 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Cut toward hall |
| S004-P07 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Presentation State Change / same composition |
| S004-P08 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Presentation State Change / same composition |
| S004-P09 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | Optional ordinary handle / opening only after release | Presentation State Change: closed→open door |
| S004-P10 | Silence; no music | Optional quiet home bed; action sounds sparse | None added | Cut to storage room |
| S004-P11 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | Optional cloth / bag one-shot; no clue effect | Presentation State Change / same composition |
| S004-P12 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | Optional envelope / paper movement | Cut to document insert at approach |
| S004-P13 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | Optional paper handling, no stinger | Presentation State Change within readable insert / room |
| S004-P14 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | Optional wrapping removal only | Cut to shoe detail then room |
| S004-P15 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Presentation State Change / same composition |
| S004-P16 | Silence throughout; no BGM in storage room | Optional quiet home bed; action sounds sparse | None added | Presentation State Change / same composition |

### Interaction, resources and constraints

| Beat Ref | Interaction State | Canon Constraints | Asset Requirement Refs | Implementation Risk | Notes |
| --- | --- | --- | --- | --- | --- |
| S004-P01 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003), [REQ-INS-002](M10_MASTER_ASSET_GAP_LIST.md#req-ins-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | NONE | Preserve each authored paragraph boundary. |
| S004-P02 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-EXP-002](M10_MASTER_ASSET_GAP_LIST.md#req-exp-002), [REQ-INS-007](M10_MASTER_ASSET_GAP_LIST.md#req-ins-007) | REVIEW | Fixed-pose gesture representation reviewed; no fabricated flashback. |
| S004-P03 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-006](M10_MASTER_ASSET_GAP_LIST.md#req-exp-006), [REQ-EXP-002](M10_MASTER_ASSET_GAP_LIST.md#req-exp-002) | NONE | Preserve each authored paragraph boundary. |
| S004-P04 | Normal authored paragraph advances | No rising-value animation; no family-love philosophical monologue | [REQ-UI-003](M10_MASTER_ASSET_GAP_LIST.md#req-ui-003), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S004-P05 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-EXP-002](M10_MASTER_ASSET_GAP_LIST.md#req-exp-002) | NONE | Preserve each authored paragraph boundary. |
| S004-P06 | Normal authored paragraph advances | No forceful blockage or death announcement by Jihun | [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004), [REQ-BG-003](M10_MASTER_ASSET_GAP_LIST.md#req-bg-003), [REQ-EXP-005](M10_MASTER_ASSET_GAP_LIST.md#req-exp-005), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | NONE | Preserve each authored paragraph boundary. |
| S004-P07 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | NONE | Preserve each authored paragraph boundary. |
| S004-P08 | AUTHORED_MANUAL_ADVANCE_HOLD; Auto paused/stopped; Skip blocked; explicit manual Advance releases | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004), [REQ-FX-001](M10_MASTER_ASSET_GAP_LIST.md#req-fx-001) | SPIKE | Narrow authoring contract only; modal close / Hide restore never counts as release. |
| S004-P09 | Normal after explicit manual release; no automatic release | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-BG-004](M10_MASTER_ASSET_GAP_LIST.md#req-bg-004), [REQ-SFX-008](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-008), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-INS-002](M10_MASTER_ASSET_GAP_LIST.md#req-ins-002) | REVIEW | No timed wait or automated handle motion before manual input. |
| S004-P10 | Normal authored paragraph advances | Readable accident record withheld until approach | [REQ-BG-005](M10_MASTER_ASSET_GAP_LIST.md#req-bg-005), [REQ-INS-005](M10_MASTER_ASSET_GAP_LIST.md#req-ins-005), [REQ-INS-007](M10_MASTER_ASSET_GAP_LIST.md#req-ins-007), [REQ-INS-006](M10_MASTER_ASSET_GAP_LIST.md#req-ins-006) | NONE | Preserve each authored paragraph boundary. |
| S004-P11 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-INS-005](M10_MASTER_ASSET_GAP_LIST.md#req-ins-005), [REQ-SFX-009](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-009), [REQ-BG-005](M10_MASTER_ASSET_GAP_LIST.md#req-bg-005) | NONE | Preserve each authored paragraph boundary. |
| S004-P12 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-INS-006](M10_MASTER_ASSET_GAP_LIST.md#req-ins-006), [REQ-SFX-010](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-010), [REQ-BG-005](M10_MASTER_ASSET_GAP_LIST.md#req-bg-005) | NONE | Preserve each authored paragraph boundary. |
| S004-P13 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-INS-006](M10_MASTER_ASSET_GAP_LIST.md#req-ins-006), [REQ-SFX-010](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-010), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | NONE | Preserve each authored paragraph boundary. |
| S004-P14 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-INS-007](M10_MASTER_ASSET_GAP_LIST.md#req-ins-007), [REQ-SFX-011](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-011), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | NONE | Preserve each authored paragraph boundary. |
| S004-P15 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003), [REQ-INS-007](M10_MASTER_ASSET_GAP_LIST.md#req-ins-007) | NONE | Preserve each authored paragraph boundary. |
| S004-P16 | Normal authored paragraph advances | Jinhee chooses and opens; papers unreadable until approach; no rising status numbers | [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-UI-003](M10_MASTER_ASSET_GAP_LIST.md#req-ui-003) | SPIKE | Preserve each authored paragraph boundary. |


## S005 — 성공한 작별 (15 beats)

### Meaning and reading

| Beat Ref | Canon Source | Narrative Purpose | Player Knowledge Before | New Information | Text Mode | Speaker |
| --- | --- | --- | --- | --- | --- | --- |
| S005-P01 | S005 L624–640: 배경: 보관방 문 앞. 거실의 가족은 그대로 보인다. 빈자리로 바꾸지 않는다. | Keep family visible while explaining projection | Fact accepted, but return reaction not yet formed; unused shoe found | Not currently alive outside; their image remains inside Jinhee | Dialogue | 진희, 지훈 |
| S005-P02 | S005 L641–654: 서술 | Let mother fit shoe and hold child without rushing | Cumulative earlier beats; latest: Not currently alive outside; their image remains inside Jinhee | New shoe large; boy will grow; Jihun waits for hand release | Narration → Dialogue | 진희, 준호 |
| S005-P03 | S005 L655–682: 지훈 | Explain irreversible risk and hear refusal motive | Cumulative earlier beats; latest: New shoe large; boy will grow; Jihun waits for hand release | Outside response can be lost; alone means abandoning family | Dialogue → Narration | 지훈, 진희 |
| S005-P04 | S005 L683–699: 지훈의 시야 가장자리: 귀환 의향 — 미형성. 외부 접속 잔여 — 21분. 인물 얼굴을 가리지 않는다. | Display clinical facts separately from emotion | Cumulative earlier beats; latest: Outside response can be lost; alone means abandoning family | Return willingness unformed; 21 external minutes; assistance has limited scope | Internal | 지훈·내부, 로프·내부 |
| S005-P05 | S005 L700–701: 접속 표시: ‘투영 반응 보조 / 기존 반응 범위 / 개입 기록 보존’. 이후 상태창은 최소화한다. 이것이 조정된 반응임을 플레이어에게 숨기지 않는다. | Disclose assistance before reaction changes | Cumulative earlier beats; latest: Return willingness unformed; 21 external minutes; assistance has limited scope | Existing reaction assistance; intervention records preserved | None | None |
| S005-P06 | S005 L702–730: 민석 | Let familiar care invite meal and child release hand | Cumulative earlier beats; latest: Existing reaction assistance; intervention records preserved | Father collects plates / asks own meal; family invites; child runs; empty hand remains | Dialogue → Narration | 민석, 진희, 나래, 준호 |
| S005-P07 | S005 L731–736: 진희 | Honor her chosen farewell time | Cumulative earlier beats; latest: Father collects plates / asks own meal; family invites; child runs; empty hand remains | Jinhee asks dinner first; Jihun will return after | Dialogue | 진희, 지훈 |
| S005-P08 | S005 L737–760: 배경 전환: 같은 식탁, 노을. 시간이 흐르는 시계 연출 없이 진희가 선택한 저녁 장면으로 변한다. 외부 접속은 그대로 이어진다. | Change memory scene to chosen sunset meal | Cumulative earlier beats; latest: Jinhee asks dinner first; Jihun will return after | Same table at sunset; familiar dishes and family preparation | Narration → Dialogue | 진희, 나래, 민석 |
| S005-P09 | S005 L761–778: 민석이 찬장을 연다. 잠깐 망설인 뒤 파란 컵을 꺼낸다. | Preserve ordinary mistake beside grief | Cumulative earlier beats; latest: Same table at sunset; familiar dishes and family preparation | Father picks blue cup; child corrects; Jinhee laughs then bows head | Dialogue → Narration | 준호, 민석, 나래, 진희 |
| S005-P10 | S005 L779–783: 서술 | Give meal its own reading space | Cumulative earlier beats; latest: Father picks blue cup; child corrects; Jinhee laughs then bows head | Jinhee chews slowly and finishes while children rise | Narration | None |
| S005-P11 | S005 L784–810: 음악: 식사가 충분히 진행된 뒤에만 낮은 단음 선율을 시작한다. 대사 사이의 식기 소리를 살린다. | Start low music late and keep utensils / farewell ordinary | Cumulative earlier beats; latest: Jinhee chews slowly and finishes while children rise | Late melody; husband offers wash; Narae hands coat | Dialogue → Narration | 민석, 진희, 나래 |
| S005-P12 | S005 L811–828: 서술 | Let child put on new shoe himself | Cumulative earlier beats; latest: Late melody; husband offers wash; Narae hands coat | Loose heel refitted; mother stops helping and touches head once | Narration → Dialogue | 준호, 진희 |
| S005-P13 | S005 L829–836: 로프의 표시: 귀환 선택 대기. 완료음 없음. | End assistance before final choice | Cumulative earlier beats; latest: Loose heel refitted; mother stops helping and touches head once | Return choice waiting; intervention ends; own response needed | Internal | 지훈·내부, 로프·내부 |
| S005-P14 | S005 L837–852: 지훈 | Ask consent while family remains present | Cumulative earlier beats; latest: Return choice waiting; intervention ends; own response needed | No memory deletion; Jinhee chooses to leave | Dialogue → Narration | 지훈, 진희 |
| S005-P15 | S005 L853–859: 서술 | Record answer and wait through practical departure | Cumulative earlier beats; latest: No memory deletion; Jinhee chooses to leave | Consent recorded; coat closed; exit opens; connection maintained | Narration → Monologue | 지훈·독백 |

### Staging

| Beat Ref | Background Intent | Visible Characters | Off-screen Characters | Expression/State | Insert/CG Intent | Information Overlay |
| --- | --- | --- | --- | --- | --- | --- |
| S005-P01 | Storage doorway with visible living family | Jinhee, Jihun foreground; all three family members visibly beyond | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Jinhee emotional; Jihun measures and waits; familiar family affection | None; wide composition sufficient | No new overlay cue; S004 static status secondary if retained |
| S005-P02 | Storage doorway with visible living family | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Jinhee kneels / places shoe; child hand held; Jihun waits | Wide bodily relationship sufficient; no mandatory emotional CG | No new overlay cue; S004 static status secondary if retained |
| S005-P03 | Storage doorway with visible living family | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Jinhee tightens grip again; Jihun procedural | None; wide composition sufficient | No new overlay cue; S004 static status secondary if retained |
| S005-P04 | Storage doorway with visible living family | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Jinhee emotional; Jihun measures and waits; familiar family affection | None; wide composition sufficient | Edge: 귀환 의향 — 미형성 / 외부 접속 잔여 — 21분 |
| S005-P05 | Storage doorway with visible living family | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Jinhee emotional; Jihun measures and waits; familiar family affection | None; wide composition sufficient | 투영 반응 보조 / 기존 반응 범위 / 개입 기록 보존, then minimized |
| S005-P06 | Storage doorway with visible living family | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Family ordinary; Jinhee hand briefly empty | Wide scene sufficient for plates / hand; no disappearance | Disclosed assistance status minimized; do not cover family |
| S005-P07 | Storage doorway with visible living family | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Patient choice spoken, not player branch | None; wide composition sufficient | Disclosed assistance status minimized; do not cover family |
| S005-P08 | Same family table at sunset, through meal and farewell | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Jinhee emotional; Jihun measures and waits; familiar family affection | None; wide composition sufficient | Disclosed assistance status minimized; do not cover family |
| S005-P09 | Same family table at sunset, through meal and farewell | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Small hesitation then ordinary correction; Jinhee smile / head down | Blue cup in wide scene sufficient; optional insert only if needed | Disclosed assistance status minimized; do not cover family |
| S005-P10 | Same family table at sunset, through meal and farewell | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Meal has materially progressed; no exact timer added | None; wide composition sufficient | Disclosed assistance status minimized; do not cover family |
| S005-P11 | Same family table at sunset, through meal and farewell | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Ordinary coat handoff; family remains | None; wide composition sufficient | Disclosed assistance status minimized; do not cover family |
| S005-P12 | Same family table at sunset, through meal and farewell | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Junho fits independently; mother single head touch | Wide shoe fitting sufficient; no extra insert requirement | Disclosed assistance status minimized; do not cover family |
| S005-P13 | Same family table at sunset, through meal and farewell | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Jinhee emotional; Jihun measures and waits; familiar family affection | None; wide composition sufficient | 귀환 선택 대기; disclose assistance ended beside exact internal 종료 |
| S005-P14 | Same family table at sunset, through meal and farewell | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Jinhee looks at table; Minseok washes under running water | None; wide composition sufficient | Waiting state secondary; no automatic success fireworks |
| S005-P15 | Same family table at sunset, through meal and farewell | 진희, 지훈; 민석, 나래, 준호 remain visibly present in family composition; ordinary Rope | None required; 민석, 나래, 준호 remain visibly present in family composition, even when not speaking | Tears continue; Jihun waits, opens exit without sentimental addition | None; wide composition sufficient | None added; established state remains secondary if retained |

### Sound and transition

| Beat Ref | BGM Intent | Ambient Intent | SFX Cue | Transition Intent |
| --- | --- | --- | --- | --- |
| S005-P01 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Cut to doorway view retaining family |
| S005-P02 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change / same composition |
| S005-P03 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change / same composition |
| S005-P04 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change / same composition |
| S005-P05 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change / same composition |
| S005-P06 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Same occupied living/table composition |
| S005-P07 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change / same composition |
| S005-P08 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Crossfade / Cut: memory-scene change, continuing external connection |
| S005-P09 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change / same composition |
| S005-P10 | Silence; melody not yet permitted | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change / same composition |
| S005-P11 | Start low single-note melody at L784 only | Sparse household sounds, optional continuous bed under technical review | Sparse utensils between lines, not completion chime | Presentation State Change / same composition |
| S005-P12 | Continue low single-note farewell melody; utensils / household sound unobscured | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change / same composition |
| S005-P13 | Continue low single-note farewell melody; utensils / household sound unobscured | Sparse household sounds, optional continuous bed under technical review | No completion sound | Presentation State Change / same composition |
| S005-P14 | Continue low single-note farewell melody; utensils / household sound unobscured | Sparse household sounds, optional continuous bed under technical review | Optional finite running water / dish sound | Presentation State Change / same composition |
| S005-P15 | Continue low single-note farewell melody; utensils / household sound unobscured | Sparse household sounds, optional continuous bed under technical review | None added | Presentation State Change toward prepared departure |

### Interaction, resources and constraints

| Beat Ref | Interaction State | Canon Constraints | Asset Requirement Refs | Implementation Risk | Notes |
| --- | --- | --- | --- | --- | --- |
| S005-P01 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-BG-006](M10_MASTER_ASSET_GAP_LIST.md#req-bg-006), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006) | REVIEW | Composition must communicate presence; no empty-place substitute. |
| S005-P02 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-BG-006](M10_MASTER_ASSET_GAP_LIST.md#req-bg-006), [REQ-INS-007](M10_MASTER_ASSET_GAP_LIST.md#req-ins-007), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005) | REVIEW | Actions through composition/prose, not new fixed-pose API; later renewed tighter grasp is preserved. |
| S005-P03 | Normal authored paragraph advances | No invented recovery guarantee or emotional Jihun monologue | [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-BG-006](M10_MASTER_ASSET_GAP_LIST.md#req-bg-006) | NONE | Preserve each authored paragraph boundary. |
| S005-P04 | Normal authored paragraph advances | Faces unobscured; authored snapshot not real-time countdown; no new family will | [REQ-UI-004](M10_MASTER_ASSET_GAP_LIST.md#req-ui-004), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S005-P05 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-UI-005](M10_MASTER_ASSET_GAP_LIST.md#req-ui-005), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Disclose once visibly before next response; no fake speaker / Backlog values. |
| S005-P06 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-BG-006](M10_MASTER_ASSET_GAP_LIST.md#req-bg-006), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003) | NONE | Preserve each authored paragraph boundary. |
| S005-P07 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | NONE | Preserve each authored paragraph boundary. |
| S005-P08 | Normal authored paragraph advances | No clock montage or universal time acceleration | [REQ-BG-007](M10_MASTER_ASSET_GAP_LIST.md#req-bg-007), [REQ-CG-001](M10_MASTER_ASSET_GAP_LIST.md#req-cg-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005) | REVIEW | M10-03 occupied family / meal composition review. |
| S005-P09 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-BG-007](M10_MASTER_ASSET_GAP_LIST.md#req-bg-007), [REQ-INS-009](M10_MASTER_ASSET_GAP_LIST.md#req-ins-009), [REQ-EXP-001](M10_MASTER_ASSET_GAP_LIST.md#req-exp-001), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003), [REQ-EXP-005](M10_MASTER_ASSET_GAP_LIST.md#req-exp-005) | NONE | Preserve each authored paragraph boundary. |
| S005-P10 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-BG-007](M10_MASTER_ASSET_GAP_LIST.md#req-bg-007), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003) | NONE | Preserve each authored paragraph boundary. |
| S005-P11 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-BGM-002](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-002), [REQ-SFX-012](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-012), [REQ-BG-007](M10_MASTER_ASSET_GAP_LIST.md#req-bg-007), [REQ-CHAR-004](M10_MASTER_ASSET_GAP_LIST.md#req-char-004), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003) | REVIEW | Physical handoff can remain canonical narration under wide framing. |
| S005-P12 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-INS-007](M10_MASTER_ASSET_GAP_LIST.md#req-ins-007), [REQ-CHAR-005](M10_MASTER_ASSET_GAP_LIST.md#req-char-005), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003) | REVIEW | Preserve each authored paragraph boundary. |
| S005-P13 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-UI-005](M10_MASTER_ASSET_GAP_LIST.md#req-ui-005), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | After end, no further altered reactions. |
| S005-P14 | Normal authored paragraph advances | No player consent choice; family not erased; memory deletion absent | [REQ-UI-005](M10_MASTER_ASSET_GAP_LIST.md#req-ui-005), [REQ-EXP-003](M10_MASTER_ASSET_GAP_LIST.md#req-exp-003), [REQ-CHAR-003](M10_MASTER_ASSET_GAP_LIST.md#req-char-003), [REQ-SFX-013](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-013) | NONE | Preserve each authored paragraph boundary. |
| S005-P15 | Normal authored paragraph advances | Family remains; intervention disclosed; patient confirms return after assistance ends; no guaranteed recovery | [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-BG-006](M10_MASTER_ASSET_GAP_LIST.md#req-bg-006) | SPIKE | Preserve each authored paragraph boundary. |


## S006 — 흰 문, 닫힌 진료 (14 beats)

### Meaning and reading

| Beat Ref | Canon Source | Narrative Purpose | Player Knowledge Before | New Information | Text Mode | Speaker |
| --- | --- | --- | --- | --- | --- | --- |
| S006-P01 | S006 L865–885: 배경: 현관. 평범한 출입문 너머로 준비된 귀환 통로가 연결된다. 가족은 뒤에서 따라 나오지 않는다. | Keep departure ordinary before anomaly | Consent recorded; family farewell intact; Jihun headache began before white door | Slippers misunderstanding and exactly two toe taps | Dialogue → Narration | 진희, 지훈 |
| S006-P02 | S006 L886–891: 효과음: 아주 낮은 진동. 문 옆 벽에 흰 문이 잠깐 겹친다. 불꽃·별·우주선 형태는 보이지 않는다. | Show one localized unexplained overlap, recognized by patient | Cumulative earlier beats; latest: Slippers misunderstanding and exactly two toe taps | Low vibration; white door beside wall; Jinhee notices | Dialogue | 진희 |
| S006-P03 | S006 L892–896: 지훈의 시야가 흔들린다. 음악 즉시 중단. | Disturb Jihun view at immediate silence | Cumulative earlier beats; latest: Low vibration; white door beside wall; Jinhee notices | View wavers, music stops; pain and vibration in monologue | Monologue | 지훈·독백 |
| S006-P04 | S006 L897–915: 로프·내부 | Maintain patient return instead of following anomaly | Cumulative earlier beats; latest: View wavers, music stops; pain and vibration in monologue | Unknown path blocked; normal exit right; Jinhee recognizes house door | Internal → Dialogue | 로프·내부, 지훈·내부, 지훈, 진희 |
| S006-P05 | S006 L916–917: 흰 문의 윤곽이 사라진다. 지훈이 진희보다 반걸음 뒤에서 출구를 지난다. 페이드아웃. | Remove outline before authored exit fade | Cumulative earlier beats; latest: Unknown path blocked; normal exit right; Jinhee recognizes house door | White outline vanishes; Jihun half-step behind patient exits | None | None |
| S006-P06 | S006 L918–923: 배경: 미래도시의 진희 침실. 주간. 차분한 자연광. 치료 장비의 짧은 종료음. | Reveal actual calm daylight care environment | Cumulative earlier beats; latest: White outline vanishes; Jihun half-step behind patient exits | Jihun awake; Jinhee eyes closed opposite; equipment ends | Narration | None |
| S006-P07 | S006 L924–927: 흰 강아지가 기지개를 켠다. 어깨와 발목에서 금속 관절이 드러난다. 처음으로 실제 사이버 도그의 모습이 확인된다. | Reveal actual cyber dog for first time, now public speaker | Cumulative earlier beats; latest: Jihun awake; Jinhee eyes closed opposite; equipment ends | Stretch exposes shoulder / ankle joints; Rope publicly speaks | Dialogue | 로프 |
| S006-P08 | S006 L928–959: 지훈 | Perform minimum orientation and safety checks | Cumulative earlier beats; latest: Stretch exposes shoulder / ankle joints; Rope publicly speaks | Voice, place, Jihun name, two fingers, breathing / nausea checked | Dialogue → Narration | 지훈, 진희 |
| S006-P09 | S006 L960–970: 화면 표시: 각성 반응 안정 / 후속 관리 알림 전송 / 재접속 제한 적용. 의료 자료는 짧게 보여준다. | Make functioning follow-up explicit and compact | Cumulative earlier beats; latest: Voice, place, Jihun name, two fingers, breathing / nausea checked | Stable awakening; team notified; reconnect restricted; same-day check / bedside call | Dialogue | 로프, 지훈 |
| S006-P10 | S006 L971–983: 서술 | Detach patch without ignoring care | Cumulative earlier beats; latest: Stable awakening; team notified; reconnect restricted; same-day check / bedside call | Patch stored; Jinhee asks sleep duration; instructed not to rise suddenly | Narration → Dialogue | 진희, 지훈 |
| S006-P11 | S006 L984–987: 서술 | Show efficient departure before patient looks back | Cumulative earlier beats; latest: Patch stored; Jinhee asks sleep duration; instructed not to rise suddenly | Bag closed; Jihun already outside | Narration | None |
| S006-P12 | S006 L988–1006: 배경: 주거 구역 복도 → 거리. 가족의 집과 같은 구도로 현재의 식탁을 잠깐 보여준다. 빈 의자를 오래 확대하지 않는다. | Briefly contrast table and keep anomaly unresolved | Cumulative earlier beats; latest: Bag closed; Jihun already outside | Matched current table glimpse; reduced headache; path absent from patient record; delay predates door | Dialogue | 로프, 지훈 |
| S006-P13 | S006 L1007–1026: 로프 | Show reassignment and earlier examination | Cumulative earlier beats; latest: Matched current table glimpse; reduced headache; path absent from patient record; delay predates door | Raw records forwarded; appointment slot replaced; emergency check if pain returns | Dialogue → Narration | 로프, 지훈 |
| S006-P14 | S006 L1027–1028: 배경: 밝은 도시 거리. 위협적인 시스템 음성이나 정체불명의 감시 화면을 추가하지 않는다. | End on ordinary city baseline | Cumulative earlier beats; latest: Raw records forwarded; appointment slot replaced; emergency check if pain returns | Bright street remains without hostile systems | None | None |

### Staging

| Beat Ref | Background Intent | Visible Characters | Off-screen Characters | Expression/State | Insert/CG Intent | Information Overlay |
| --- | --- | --- | --- | --- | --- | --- |
| S006-P01 | Normal home entrance; ordinary exit right, adjacent wall localized anomaly only at its cue | 지훈, 진희, ordinary white Rope; family stays behind and does not follow | 민석, 나래, 준호 remain behind in household; do not follow | Everyday practical exchange | None; wide composition sufficient | No added diagnostic overlay; white outline is a localized visual cue only |
| S006-P02 | Normal home entrance; ordinary exit right, adjacent wall localized anomaly only at its cue | 지훈, 진희, ordinary white Rope; family stays behind and does not follow | 민석, 나래, 준호 remain behind in household; do not follow | Jinhee recognition, not monster terror | None; wide composition sufficient | No added diagnostic overlay; white outline is a localized visual cue only |
| S006-P03 | Normal home entrance; ordinary exit right, adjacent wall localized anomaly only at its cue | 지훈, 진희, ordinary white Rope; family stays behind and does not follow | 민석, 나래, 준호 remain behind in household; do not follow | Jihun restrained view disturbance / pain; Jinhee has just recognized outline | None; wide composition sufficient | No added diagnostic overlay; white outline is a localized visual cue only |
| S006-P04 | Normal home entrance; ordinary exit right, adjacent wall localized anomaly only at its cue | 지훈, 진희, ordinary white Rope; family stays behind and does not follow | 민석, 나래, 준호 remain behind in household; do not follow | Jihun maintains return by voice while in pain; Jinhee also recognizes unexpected door | None; wide composition sufficient | No added unknown-path diagnostic UI |
| S006-P05 | Normal home entrance; ordinary exit right, adjacent wall localized anomaly only at its cue | 지훈, 진희, ordinary white Rope; family stays behind and does not follow | 민석, 나래, 준호 remain behind in household; do not follow | Ordinary exit retained; no family trailing behind | None; wide composition sufficient | No added diagnostic overlay; white outline is a localized visual cue only |
| S006-P06 | Jinhee future-city bedroom, daytime | Jihun, Jinhee eyes closed; Rope nearby not yet emphasized mechanically | No family projection physically present in bedroom | Jihun awakens; Jinhee eyes still closed; calm actual-room orientation | None; wide composition sufficient | None added; established state remains secondary if retained |
| S006-P07 | Actual future-city bedroom in calm daylight | Actual cyber Rope; Jihun / Jinhee in bedroom | No family projection physically present in bedroom | Ordinary stretch reveals metal joints, no transformation spectacle | None; wide composition sufficient | None added; established state remains secondary if retained |
| S006-P08 | Actual future-city bedroom in calm daylight | 진희, 지훈, actual cyber Rope | Family not physically present outside connection | Responsive Jinhee; procedural checking eyes and device | Wide / brief hand framing sufficient; no obligatory insert | None added; established state remains secondary if retained |
| S006-P09 | Actual future-city bedroom in calm daylight | 진희, 지훈, actual cyber Rope | Family not physically present outside connection | Normal departure before brief anomaly; competent orientation and ordinary care after | None; wide composition sufficient | 각성 반응 안정 / 후속 관리 알림 전송 / 재접속 제한 적용, briefly |
| S006-P10 | Actual future-city bedroom in calm daylight | 진희, 지훈, actual cyber Rope | Family not physically present outside connection | Procedural equipment handling; patient looks at opposite door | Wide equipment sufficient; no invented duration figure | Medical check display recedes after brief reveal; no extra facts |
| S006-P11 | Bedroom exit / residential corridor | 지훈 leaving; 진희 remains in room, actual Rope accompanies | Jinhee remains bedroom after departure | Normal departure before brief anomaly; competent orientation and ordinary care after | None; wide composition sufficient | Medical check display recedes after brief reveal; no extra facts |
| S006-P12 | Residential corridor → street, brief matched-table glimpse within transition | 지훈 and actual cyber Rope | Jinhee remains bedroom; family no longer physical scene participants | Normal departure before brief anomaly; competent orientation and ordinary care after | Brief matched current table, no prolonged empty-chair zoom | Medical check display recedes after brief reveal; no extra facts |
| S006-P13 | Bright city street | 지훈 and actual cyber Rope | Jinhee remains bedroom; family no longer physical scene participants | Normal departure before brief anomaly; competent orientation and ordinary care after | None; wide composition sufficient | One appointment slot blank then examination replaces it; no interactive calendar |
| S006-P14 | Bright city street | 지훈 and actual cyber Rope | Jinhee remains bedroom; family no longer physical scene participants | Normal departure before brief anomaly; competent orientation and ordinary care after | None; wide composition sufficient | Medical check display recedes after brief reveal; no extra facts |

### Sound and transition

| Beat Ref | BGM Intent | Ambient Intent | SFX Cue | Transition Intent |
| --- | --- | --- | --- | --- |
| S006-P01 | Continue farewell melody until authored view disturbance | No new continuous city / equipment bed required; silence plus authored cues | Two quiet slipper toe taps | Cut to entrance; same ordinary home |
| S006-P02 | Continue farewell melody until authored view disturbance | No new continuous city / equipment bed required; silence plus authored cues | Very low vibration at L887 | Localized Overlay beside normal exit |
| S006-P03 | Stop farewell BGM immediately at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Localized Overlay / restrained perceptual state change |
| S006-P04 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Presentation State Change / same composition |
| S006-P05 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Localized Overlay removed → Fade to Black |
| S006-P06 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | Short equipment termination sound | Black to Scene in calm natural light |
| S006-P07 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Presentation State Change: actual physical form / public speech |
| S006-P08 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Presentation State Change / same composition |
| S006-P09 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Presentation State Change / same composition |
| S006-P10 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Presentation State Change / same composition |
| S006-P11 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | Optional equipment bag close | Cut / composition change from room to corridor |
| S006-P12 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Cut corridor→matched table glimpse→street |
| S006-P13 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Presentation State Change / same composition |
| S006-P14 | Silence after immediate stop at L892 | No new continuous city / equipment bed required; silence plus authored cues | None added | Stable daylight scene exit |

### Interaction, resources and constraints

| Beat Ref | Interaction State | Canon Constraints | Asset Requirement Refs | Implementation Risk | Notes |
| --- | --- | --- | --- | --- | --- |
| S006-P01 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-BG-002](M10_MASTER_ASSET_GAP_LIST.md#req-bg-002), [REQ-SFX-014](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-014), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-006](M10_MASTER_ASSET_GAP_LIST.md#req-char-006) | NONE | Preserve each authored paragraph boundary. |
| S006-P02 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-FX-002](M10_MASTER_ASSET_GAP_LIST.md#req-fx-002), [REQ-SFX-015](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-015), [REQ-BG-002](M10_MASTER_ASSET_GAP_LIST.md#req-bg-002) | SPIKE | M10-04 SPIKE CANDIDATE; normal right-side exit stays readable. |
| S006-P03 | Normal authored paragraph advances | No heavy glitch, stars, spacecraft, flame or explanatory whisper | [REQ-FX-003](M10_MASTER_ASSET_GAP_LIST.md#req-fx-003), [REQ-EXP-004](M10_MASTER_ASSET_GAP_LIST.md#req-exp-004), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-BGM-002](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-002) | SPIKE | Preserve each authored paragraph boundary. |
| S006-P04 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-FX-002](M10_MASTER_ASSET_GAP_LIST.md#req-fx-002), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | SPIKE | Preserve each authored paragraph boundary. |
| S006-P05 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-FX-002](M10_MASTER_ASSET_GAP_LIST.md#req-fx-002), [REQ-FX-001](M10_MASTER_ASSET_GAP_LIST.md#req-fx-001) | SPIKE | Preserve each authored paragraph boundary. |
| S006-P06 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-BG-008](M10_MASTER_ASSET_GAP_LIST.md#req-bg-008), [REQ-SFX-016](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-016), [REQ-FX-001](M10_MASTER_ASSET_GAP_LIST.md#req-fx-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | NONE | Preserve each authored paragraph boundary. |
| S006-P07 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007), [REQ-EXP-009](M10_MASTER_ASSET_GAP_LIST.md#req-exp-009), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | REVIEW | First real cyber reveal L924; fixed-pose stretch / joint staging composition review. |
| S006-P08 | Normal authored paragraph advances | No instant complete recovery claim | [REQ-BG-008](M10_MASTER_ASSET_GAP_LIST.md#req-bg-008), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | NONE | Preserve each authored paragraph boundary. |
| S006-P09 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-UI-006](M10_MASTER_ASSET_GAP_LIST.md#req-ui-006), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S006-P10 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-BG-008](M10_MASTER_ASSET_GAP_LIST.md#req-bg-008), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | NONE | Preserve each authored paragraph boundary. |
| S006-P11 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-SFX-017](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-017), [REQ-BG-009](M10_MASTER_ASSET_GAP_LIST.md#req-bg-009), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-002](M10_MASTER_ASSET_GAP_LIST.md#req-char-002) | NONE | Preserve each authored paragraph boundary. |
| S006-P12 | Normal authored paragraph advances | No owner / wife / daughter inference; cause unresolved | [REQ-BG-009](M10_MASTER_ASSET_GAP_LIST.md#req-bg-009), [REQ-BG-010](M10_MASTER_ASSET_GAP_LIST.md#req-bg-010), [REQ-INS-008](M10_MASTER_ASSET_GAP_LIST.md#req-ins-008), [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | NONE | Preserve each authored paragraph boundary. |
| S006-P13 | Normal authored paragraph advances | White door once, unexplained, both see it; no cosmic/horror/whisper; care system works | [REQ-BG-010](M10_MASTER_ASSET_GAP_LIST.md#req-bg-010), [REQ-UI-012](M10_MASTER_ASSET_GAP_LIST.md#req-ui-012), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve sequence; does not change into a patient task / systemic threat. |
| S006-P14 | Normal authored paragraph advances | No threatening system voice / unknown surveillance screen | [REQ-BG-010](M10_MASTER_ASSET_GAP_LIST.md#req-bg-010) | NONE | Preserve each authored paragraph boundary. |


## S007 — 삶을 묻는 검사 (11 beats)

### Meaning and reading

| Beat Ref | Canon Source | Narrative Purpose | Player Knowledge Before | New Information | Text Mode | Speaker |
| --- | --- | --- | --- | --- | --- | --- |
| S007-P01 | S007 L1033–1055: 배경: 이도경의 진료실. 다음 날 오전. 책상 아래 로프. | Check acute symptom and restrictions first | Awakening checks complete; headache reduced; appointment moved before next connection | No current pain, only one view event; cause analyzed; no repeat connection yet | Dialogue | 도경, 지훈 |
| S007-P02 | S007 L1056–1084: 도경이 화면을 바꾼다. 수면과 식사 기록. | Ask about ordinary pleasure using life records | Cumulative earlier beats; latest: No current pain, only one view event; cause analyzed; no repeat connection yet | Sleep / meals shown; work success differs from enjoyment; no recalled outside pleasure | Dialogue | 도경, 지훈 |
| S007-P03 | S007 L1085–1089: 침묵. 지훈이 답하려다 멈춘다. | Let failed recollection sit in ordinary silence | Cumulative earlier beats; latest: Sleep / meals shown; work success differs from enjoyment; no recalled outside pleasure | Jihun starts to answer, stops, admits none | Dialogue | 지훈 |
| S007-P04 | S007 L1090–1120: 도경 | Use taste question before clinical explanation | Cumulative earlier beats; latest: Jihun starts to answer, stops, admits none | Product name not taste; doctor gives time; response range narrowed beyond sleep | Dialogue → Narration | 도경, 지훈, 로프 |
| S007-P05 | S007 L1121–1136: 화면: 서로 다른 두 경로가 말기 구간으로 이어지는 단순 도식. 긴 의학 설명이나 신경 영상 애니메이션은 사용하지 않는다. | Introduce early reversible condition with simple diagram | Cumulative earlier beats; latest: Product name not taste; doctor gives time; response range narrowed beyond sleep | 각성고갈증 early findings; common does not mean safe | Dialogue | 도경, 지훈 |
| S007-P06 | S007 L1137–1152: 지훈 | Distinguish starting paths and shared terminal limit | Cumulative earlier beats; latest: 각성고갈증 early findings; common does not mean safe | Retreat from pain / memories differs from response depletion; late self-function loss irreversible | Dialogue | 지훈, 도경 |
| S007-P07 | S007 L1153–1160: 지훈이 도식을 더 보지 않고 창밖을 본다. | Return attention to person and distinguish bodily maintenance | Cumulative earlier beats; latest: Retreat from pain / memories differs from response depletion; late self-function loss irreversible | Jihun looks outside; regression-care body stability does not reverse response decline | Dialogue | 도경, 지훈 |
| S007-P08 | S007 L1161–1185: 도경 | Prescribe recovery before light immersion, task needs another person | Cumulative earlier beats; latest: Jihun looks outside; regression-care body stability does not reverse response decline | Sensory-path check / recovery first; three era sessions; profile cannot tell wants | Dialogue → Narration | 도경, 지훈 |
| S007-P09 | S007 L1186–1194: 도경 | Separate test frequency from stimulus strength | Cumulative earlier beats; latest: Sensory-path check / recovery first; three era sessions; profile cannot tell wants | Quarterly→monthly tests; symptoms can delay / stop immersion | Dialogue | 도경, 지훈 |
| S007-P10 | S007 L1195–1211: 지훈 | Ask what rest actually means and permit it separately | Cumulative earlier beats; latest: Quarterly→monthly tests; symptoms can delay / stop immersion | Jihun reads patient records off-duty; no-record rest alongside immersion | Dialogue | 지훈, 도경 |
| S007-P11 | S007 L1212–1213: 도경이 처방을 확정한다. 화면에 ‘승인 전 재평가’가 함께 표시된다. | Confirm prescription with approval prerequisite visible | Cumulative earlier beats; latest: Jihun reads patient records off-duty; no-record rest alongside immersion | 승인 전 재평가 remains required | None | None |

### Staging

| Beat Ref | Background Intent | Visible Characters | Off-screen Characters | Expression/State | Insert/CG Intent | Information Overlay |
| --- | --- | --- | --- | --- | --- | --- |
| S007-P01 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Attentive serious doctor; Jihun procedural, pauses at ordinary-life questions | None; wide composition sufficient | None; acute findings and restriction are spoken, not a new diagnostic panel |
| S007-P02 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Attentive serious doctor; Jihun procedural, pauses at ordinary-life questions | None; wide composition sufficient | Doctor record screen: sleep and meals, no invented numbers |
| S007-P03 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Jihun answer stalls; attentive silence | None; wide composition sufficient | Sleep / meal records remain secondary |
| S007-P04 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Doctor never laughs; Rope public correction; Jihun factual | None; wide composition sufficient | Records secondary; evidence significance remains dialogue |
| S007-P05 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Attentive serious doctor; Jihun procedural, pauses at ordinary-life questions | Diagram, not neural imagery | Simple two-path diagram begins L1121; no duplicate medical overlay paragraph |
| S007-P06 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Attentive serious doctor; Jihun procedural, pauses at ordinary-life questions | None; wide composition sufficient | Same two paths, shared terminal region; dialogue defines distinctions |
| S007-P07 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Jihun stops watching diagram and looks out window | None; wide composition sufficient | Diagram can recede; no new narrated medical panel |
| S007-P08 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Attentive serious doctor; Jihun procedural, pauses at ordinary-life questions | None; wide composition sufficient | Prescription can show scope, without approval yet |
| S007-P09 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Attentive serious doctor; Jihun procedural, pauses at ordinary-life questions | None; wide composition sufficient | Prescription context secondary; frequency distinction in dialogue |
| S007-P10 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Doctor looks at Jihun; procedural answer falters; no ridicule | None; wide composition sufficient | Prescription context secondary; no new fact overlay |
| S007-P11 | Dokyeong office next morning | 도경, 지훈; actual Rope under desk | None required | Attentive serious doctor; Jihun procedural, pauses at ordinary-life questions | None; wide composition sufficient | Exact 승인 전 재평가 beside confirmed prescription |

### Sound and transition

| Beat Ref | BGM Intent | Ambient Intent | SFX Cue | Transition Intent |
| --- | --- | --- | --- | --- |
| S007-P01 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Cut / Short Fade to office, next morning |
| S007-P02 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change / same composition |
| S007-P03 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | No punctuating effect | Presentation State Change / same composition |
| S007-P04 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change / same composition |
| S007-P05 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change: record screen→simple diagram |
| S007-P06 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change / same composition |
| S007-P07 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change: diagram focus→people/window |
| S007-P08 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change / same composition |
| S007-P09 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change / same composition |
| S007-P10 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change / same composition |
| S007-P11 | No authored music role; plan silence, not a new obligatory music cue | No continuous bed required; quiet consultation | None added | Presentation State Change: prescription confirmed |

### Interaction, resources and constraints

| Beat Ref | Interaction State | Canon Constraints | Asset Requirement Refs | Implementation Risk | Notes |
| --- | --- | --- | --- | --- | --- |
| S007-P01 | Normal authored paragraph advances | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-BG-011](M10_MASTER_ASSET_GAP_LIST.md#req-bg-011), [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007), [REQ-EXP-007](M10_MASTER_ASSET_GAP_LIST.md#req-exp-007) | NONE | Preserve each authored paragraph boundary. |
| S007-P02 | Normal authored paragraph advances | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-UI-007](M10_MASTER_ASSET_GAP_LIST.md#req-ui-007), [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S007-P03 | Normal reading/advance pause; not S004 manual-hold contract | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-EXP-004](M10_MASTER_ASSET_GAP_LIST.md#req-exp-004), [REQ-EXP-007](M10_MASTER_ASSET_GAP_LIST.md#req-exp-007), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | NONE | Preserve each authored paragraph boundary. |
| S007-P04 | Normal authored paragraph advances | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-EXP-007](M10_MASTER_ASSET_GAP_LIST.md#req-exp-007), [REQ-UI-007](M10_MASTER_ASSET_GAP_LIST.md#req-ui-007), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S007-P05 | Normal authored paragraph advances | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-DIAG-001](M10_MASTER_ASSET_GAP_LIST.md#req-diag-001), [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | NONE | Preserve each authored paragraph boundary. |
| S007-P06 | Normal authored paragraph advances | No extra term definition or new taxonomy; patient history not changed | [REQ-DIAG-001](M10_MASTER_ASSET_GAP_LIST.md#req-diag-001), [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | NONE | Preserve each authored paragraph boundary. |
| S007-P07 | Normal authored paragraph advances | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-BG-011](M10_MASTER_ASSET_GAP_LIST.md#req-bg-011), [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-EXP-004](M10_MASTER_ASSET_GAP_LIST.md#req-exp-004) | NONE | Preserve each authored paragraph boundary. |
| S007-P08 | Normal authored paragraph advances | No immediate connection or known helper identity | [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-UI-008](M10_MASTER_ASSET_GAP_LIST.md#req-ui-008) | SPIKE | Preserve each authored paragraph boundary. |
| S007-P09 | Normal authored paragraph advances | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-UI-008](M10_MASTER_ASSET_GAP_LIST.md#req-ui-008), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S007-P10 | Normal authored paragraph advances | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-EXP-007](M10_MASTER_ASSET_GAP_LIST.md#req-exp-007), [REQ-CHAR-008](M10_MASTER_ASSET_GAP_LIST.md#req-char-008), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | NONE | Preserve each authored paragraph boundary. |
| S007-P11 | Normal authored paragraph advances | Question → life/record evidence → explanation; diagnosis does not resolve unknown sensory cause | [REQ-UI-008](M10_MASTER_ASSET_GAP_LIST.md#req-ui-008) | SPIKE | Preserve each authored paragraph boundary. |


## S008 — 가장 편할 것 같은 여자 (14 beats)

### Meaning and reading

| Beat Ref | Canon Source | Narrative Purpose | Player Knowledge Before | New Information | Text Mode | Speaker |
| --- | --- | --- | --- | --- | --- | --- |
| S008-P01 | S008 L1218–1228: 배경: 몰입 치료 준비실. 며칠 뒤. 자막은 경과 시간만 표시한다. 통증과 감각 혼선이 재발하지 않았음을 확인한 후 접속을 승인한다. | Show elapsed recovery and reassessment before approval | Recovery / reassessment required; immersion not yet approved in prior scene | Days later, no symptom recurrence, approved; new companion method explained | Dialogue | 은정, 지훈 |
| S008-P02 | S008 L1229–1245: 화면: ‘시대 몰입 / 재구성 한국 도시 / 2000년대 초의 생활 환경’. 세 프로필 카드. 선택 전 카드는 동일한 밝기로 제시한다. | Introduce equal role cards, context and anonymous participation | Cumulative earlier beats; latest: Days later, no symptom recurrence, approved; new companion method explained | Reconstructed Korean early-2000s setting; helper assigned after role choice; relay contact by mutual consent | Dialogue | 은정, 지훈 |
| S008-P03 | S008 L1246–1247: 지훈이 참여 방식과 녹화·정보 보호 범위를 확인한다. 서명 화면을 짧게 보여준다. | Check participation and information protection before review | Cumulative earlier beats; latest: Reconstructed Korean early-2000s setting; helper assigned after role choice; relay contact by mutual consent | Participation, recording / information scope checked; signature briefly shown | None | None |
| S008-P04 | S008 L1248–1256: 프로필 1 — 선주 | Review Sunju and its one authored thought | Consent cleared; any other profiles may already be reviewed | Photo club, sociable / spontaneous, neighborhood event; Jihun notes crowd | None (structured information) → Monologue | 지훈·독백 |
| S008-P05 | S008 L1257–1265: 프로필 2 — 해진 | Review Haejin and its one authored thought | Consent cleared; any other profiles may already be reviewed | Small-performance trainee, candid / direct, venue prep; plan may change | None (structured information) → Monologue | 지훈·독백 |
| S008-P06 | S008 L1266–1271: 프로필 3 — 하나 | Review Hana without fabricating an extra thought | Consent cleared; any other profiles may already be reviewed | May Theater projectionist, calm / practical, concrete help, last screening | None (structured information) | None |
| S008-P07 | S008 L1272–1289: 지훈 | Return control to canonical story choice and rationale | All three profiles reviewed; both immediate authored thoughts delivered | 하나로 하겠습니다; task clear; person not known yet | Dialogue | 지훈, 은정 |
| S008-P08 | S008 L1290–1312: 화면: 하나 선택 확정. 나머지 프로필은 보관됨. 하나의 얼굴을 특별한 기억 효과로 강조하지 않는다. | Confirm Hana only now and explain limited progress meaning | Cumulative earlier beats; latest: 하나로 하겠습니다; task clear; person not known yet | Other profiles stored; cooperation/trust estimate ≠ love; behavior and refusal matter | Dialogue | 은정, 지훈 |
| S008-P09 | S008 L1313–1331: 화면: 정상 종료 확인 항목 세 줄. 숫자로 경쟁시키지 않는다. | Present qualitative conditions without triplicate explanation | Cumulative earlier beats; latest: Other profiles stored; cooperation/trust estimate ≠ love; behavior and refusal matter | Joint task, trust/cooperation, voluntary interest; clinical judgement remains; Jihun counts tasks | None (structured information) → Dialogue → Monologue | 은정, 지훈, 지훈·독백 |
| S008-P10 | S008 L1332–1345: 은정 | Keep safe interruption independent of completion | Cumulative earlier beats; latest: Joint task, trust/cooperation, voluntary interest; clinical judgement remains; Jihun counts tasks | Can return if unwell without conditions or companion approval; Rope responsible | Dialogue | 은정, 지훈, 로프 |
| S008-P11 | S008 L1346–1361: 은정 | Brief role and separate inside deadline from external limit | Cumulative earlier beats; latest: Can return if unwell without conditions or companion approval; Rope responsible | Temporary screening assistant; Hana expects him; tonight screening deadline; abort allowed | Dialogue | 은정, 지훈 |
| S008-P12 | S008 L1362–1374: 은정이 패치를 붙인다. 로프가 침상 옆에 앉는다. | Prepare patch and preserve later Hana reread / monologue | Cumulative earlier beats; latest: Temporary screening assistant; Hana expects him; tonight screening deadline; abort allowed | Patch attached; Rope beside bed; Hana profile reread; efficient-task expectation; connect spoken | Narration → Monologue → Dialogue | 지훈·독백, 은정 |
| S008-P13 | S008 L1375–1380: 페이드아웃. 장비의 연결음을 서서히 빗소리로 교체한다. 연결 후 별도의 여성 속삭임이나 의미심장한 기억 영상은 넣지 않는다. | Leave room through equipment-to-rain audio transition | Cumulative earlier beats; latest: Patch attached; Rope beside bed; Hana profile reread; efficient-task expectation; connect spoken | Fade; connection sound becomes rain; awning drop then old bus brake | Narration | None |
| S008-P14 | S008 L1381–1384: 검정 화면에 부 제목. | Finish part before next era starts | Cumulative earlier beats; latest: Fade; connection sound becomes rain; awning drop then old bus brake | 1부 — 깨우는 사람 / 끝 | None (structured information) | None |

### Staging

| Beat Ref | Background Intent | Visible Characters | Off-screen Characters | Expression/State | Insert/CG Intent | Information Overlay |
| --- | --- | --- | --- | --- | --- | --- |
| S008-P01 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Elapsed-time caption only; reassessment/approval context concise |
| S008-P02 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Authored era heading plus equally bright three-profile overview |
| S008-P03 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Brief signing/scope screen then equal overview restored |
| S008-P04 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Sunju exact four profile lines in detail; shared reading-area Monologue at L1254–1255 |
| S008-P05 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Haejin exact four profile lines in detail; Monologue L1263–1264 |
| S008-P06 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Hana exact four profile lines; no immediate Hana monologue authored |
| S008-P07 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | No chosen highlight until authored decision |
| S008-P08 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Hana confirmed after spoken line, others stored; no special face memory effect |
| S008-P09 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Exact three authored guide lines; no competitive numbers |
| S008-P10 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | Three guide lines secondary / may recede; no added permission meter |
| S008-P11 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Professional preparation; procedural task-oriented Jihun | None; wide composition sufficient | No invented timer or task checklist beyond authored guide |
| S008-P12 | Immersion preparation room, days after clinical recovery / reassessment | 은정, 지훈, actual Rope; profiles are role information, not people physically entering room | No actual helper physically revealed | Jihun on bed / preparing; Eunjeong patch; Rope bedside | None; wide composition sufficient | Hana detail reread only as authored, not a fresh browse / thought loop |
| S008-P13 | Preparation room fading to black; no next-era BG required | Room / figures fade out; black at endpoint | No actual helper physically revealed | Prepared room / people fade; equipment sound gives way to rain without memory effect | None; wide composition sufficient | Hide preparation / profile panels with departing presentation |
| S008-P14 | Black | None | None | Static legible part-end title | None; wide composition sufficient | Exact static part-end title |

### Sound and transition

| Beat Ref | BGM Intent | Ambient Intent | SFX Cue | Transition Intent |
| --- | --- | --- | --- | --- |
| S008-P01 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Cut / Short Fade to preparation room |
| S008-P02 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P03 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change: scope/signature→overview |
| S008-P04 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P05 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P06 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P07 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P08 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P09 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P10 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P11 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P12 | No authored music role; keep silence | No new continuous room bed required; rain not yet begun | None added | Presentation State Change / same composition |
| S008-P13 | No authored music role; keep silence | Rain replaces connection gradually; continuous / finite sequencing reviewed | Connection→rain handoff, one awning drop, then old bus brake | Fade to Black plus Audio-led Transition |
| S008-P14 | No authored music role; keep silence | Rain tail may remain under title; no additional ambient role / next-era scene | No added completion fanfare | Presentation State Change: black→end title |

### Interaction, resources and constraints

| Beat Ref | Interaction State | Canon Constraints | Asset Requirement Refs | Implementation Risk | Notes |
| --- | --- | --- | --- | --- | --- |
| S008-P01 | Normal authored paragraph advances | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-BG-012](M10_MASTER_ASSET_GAP_LIST.md#req-bg-012), [REQ-UI-008](M10_MASTER_ASSET_GAP_LIST.md#req-ui-008), [REQ-CHAR-009](M10_MASTER_ASSET_GAP_LIST.md#req-char-009), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001), [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007), [REQ-EXP-008](M10_MASTER_ASSET_GAP_LIST.md#req-exp-008) | SPIKE | No invented exact day count or symptom test numbers. |
| S008-P02 | Normal briefing; cards visible but browse opens after consent check | No Hana prehighlight / real helper identity / actual-person age | [REQ-PROFILE-001](M10_MASTER_ASSET_GAP_LIST.md#req-profile-001), [REQ-UI-009](M10_MASTER_ASSET_GAP_LIST.md#req-ui-009), [REQ-CHAR-009](M10_MASTER_ASSET_GAP_LIST.md#req-char-009), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S008-P03 | Normal authored paragraph advances | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-UI-010](M10_MASTER_ASSET_GAP_LIST.md#req-ui-010), [REQ-PROFILE-001](M10_MASTER_ASSET_GAP_LIST.md#req-profile-001) | SPIKE | No invented interactive legal-choice branch. |
| S008-P04 | Profile Browse: overview→Sunju detail→thought once→Back→overview/reviewed | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-PROFILE-001](M10_MASTER_ASSET_GAP_LIST.md#req-profile-001), [REQ-PROFILE-002](M10_MASTER_ASSET_GAP_LIST.md#req-profile-002), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Order among three detail groups free; revisit detail does not duplicate thought. |
| S008-P05 | Profile Browse: overview→Haejin detail→thought once→Back→overview/reviewed | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-PROFILE-001](M10_MASTER_ASSET_GAP_LIST.md#req-profile-001), [REQ-PROFILE-002](M10_MASTER_ASSET_GAP_LIST.md#req-profile-002), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Reviewed means inspected, never preferred. |
| S008-P06 | Profile Browse: overview→Hana detail→Back→overview/reviewed; after all three enable neutral 브리핑으로 돌아가기 | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-PROFILE-001](M10_MASTER_ASSET_GAP_LIST.md#req-profile-001), [REQ-PROFILE-002](M10_MASTER_ASSET_GAP_LIST.md#req-profile-002) | SPIKE | All three details + Sunju/Haejin thoughts delivered once before continuation; navigation labels are planning UI, not extra dialogue. |
| S008-P07 | Neutral return releases browse ownership; normal paragraph advances | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-PROFILE-001](M10_MASTER_ASSET_GAP_LIST.md#req-profile-001), [REQ-PROFILE-002](M10_MASTER_ASSET_GAP_LIST.md#req-profile-002), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-CHAR-009](M10_MASTER_ASSET_GAP_LIST.md#req-char-009), [REQ-CHAR-001](M10_MASTER_ASSET_GAP_LIST.md#req-char-001) | SPIKE | Canonical Hana response is spoken; no alternate route or fabricated selection result. |
| S008-P08 | Normal briefing; no profile route selection | No new romance score or actual mind-reading UI | [REQ-PROFILE-001](M10_MASTER_ASSET_GAP_LIST.md#req-profile-001), [REQ-PROFILE-002](M10_MASTER_ASSET_GAP_LIST.md#req-profile-002), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S008-P09 | Normal authored paragraph advances | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-UI-011](M10_MASTER_ASSET_GAP_LIST.md#req-ui-011), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Dialogue supplies assessment meaning; Monologue remains authored task-count thought. |
| S008-P10 | Normal authored paragraph advances | No trapped-until-liked condition | [REQ-UI-011](M10_MASTER_ASSET_GAP_LIST.md#req-ui-011), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007) | SPIKE | Preserve each authored paragraph boundary. |
| S008-P11 | Normal authored paragraph advances | No era content played yet; no externally accelerated-time rule | [REQ-CHAR-009](M10_MASTER_ASSET_GAP_LIST.md#req-char-009), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001), [REQ-BG-012](M10_MASTER_ASSET_GAP_LIST.md#req-bg-012) | NONE | Preserve each authored paragraph boundary. |
| S008-P12 | Normal paragraphs; canonical later Monologue at L1368–1370 retained | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-BG-012](M10_MASTER_ASSET_GAP_LIST.md#req-bg-012), [REQ-PROFILE-002](M10_MASTER_ASSET_GAP_LIST.md#req-profile-002), [REQ-CHAR-009](M10_MASTER_ASSET_GAP_LIST.md#req-char-009), [REQ-CHAR-007](M10_MASTER_ASSET_GAP_LIST.md#req-char-007), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | REVIEW | Patch gesture composition review; no extra immediate Hana thought. |
| S008-P13 | Normal authored paragraph advances | No female whisper / meaningful memory film / next-era gameplay | [REQ-FX-001](M10_MASTER_ASSET_GAP_LIST.md#req-fx-001), [REQ-SFX-018](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-018), [REQ-AMB-002](M10_MASTER_ASSET_GAP_LIST.md#req-amb-002), [REQ-SFX-019](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-019), [REQ-SFX-020](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-020), [REQ-UI-001](M10_MASTER_ASSET_GAP_LIST.md#req-ui-001) | SPIKE | Preserve each authored paragraph boundary. |
| S008-P14 | End title / end of mapped scope; no additional narrative choice | Anonymous helper assigned after role choice; browse order free, result Hana; safety abort remains available | [REQ-FX-001](M10_MASTER_ASSET_GAP_LIST.md#req-fx-001), [REQ-UI-013](M10_MASTER_ASSET_GAP_LIST.md#req-ui-013) | NONE | Scope ends here; next-era content excluded. |


## Dedicated transition map

Qualitative classes only; no final durations. A slash is a later composition choice, not two mandatory effects. Prop returns and informational state changes are included so no location or overlay change silently disappears.

| Beat relationship | From | To | Relationship | Recommended transition class | Reason |
| --- | --- | --- | --- | --- | --- |
| S001-P01 → P02 | Black / children heard | Morning pan then family kitchen | Opening reveal | Black to Scene | Sound and eggs precede faces / music |
| S001-P02 → P03 | Kitchen without BGM | Light morning theme under ordinary family prose | Delayed authored music entrance | Presentation State Change | Do not begin music at first visual reveal |
| S001-P05 → P07 | Kitchen after bell | Entrance | Continuous household movement | Audio-led Transition / Cut | Junho footsteps bridge familiar arrival |
| S001-P07 → P08 | Entrance | Table via aligned shoes | Same visit | Cut | Dog under table and familiar coffee |
| S001-P09 → P10 | Table | Old-shoe insert then table | Prop condition / return | Cut / Presentation State Change | Folded heel readable; mother redirects |
| S001-P11 → P12 | Table / memo gaze | Readable note then table | Prop readability / return | Cut / Presentation State Change | No hospital identifiers |
| S001-P13 | Table exchange | Occupied five-person tableau | Composition confirmation | Presentation State Change | No empty chair; dog at Jihun foot |
| S001 → S002 | Morning table | Living room after breakfast | Immediate continuity | Cut | Morning BGM continues lowered |
| S002 photo changes | Living room with photo | Sandy snack photo → sunny sea photo | Object review within location | Presentation State Change | No new travel flashback or clue popup |
| S002 → S003 | Living room | Connected hall / closed door | Continuous approach | Cut | Mother gaze was followed without earlier standing |
| S003-P03 → P04 | Stable hall | Perceived distance / dog touch / private channel | Perception and communication change | Presentation State Change | No menace; morning theme fades at touch |
| S003-P05 → P06 | Identity exchange without panel | First compact connection facts | First structured information reveal | Presentation State Change | After therapist identity; not a speaker / second narrator |
| S003-P06 → P07 | Internal exchange / data | Public reassurance / normal hall | Orientation restoration | Presentation State Change | Lowered hand restores ordinary passage |
| S003-P08 → S004 | Hall retreat | Living room / shoe handoff | Same connection, pause and retry | Cut | No forced access; BGM remains absent |
| S004-P03 → P04 | Shoe / repeating-tomorrow exchange | Static Jihun status beside monologue | Clinical layer appearance | Presentation State Change | No new numeric values or rise animation |
| S004-P06 → P07 | Living room | Closed-door hall | Patient-led approach | Cut | Jinhee stands; husband does not forcibly block |
| S004-P07 → P08 | Door decision, normal reading | Still closed door / manual-only hold | Narrow authored input state | Presentation State Change | Auto / Skip cannot cause handle motion |
| S004-P08 → P09 | Manual hold / closed door | Jinhee opens | Explicit player Advance permits authored action | Presentation State Change | No timed / automatic handle motion |
| S004-P09 → P10 | Open doorway | Storage wide | Immediate reveal | Cut | Paper not yet readable; no BGM |
| S004-P11 → P12 → P13 | Storage objects | Bag contents → slipped readable paper / cover-reopen | Authored proximity / object handling | Cut / Presentation State Change | No early document reveal or sudden death announcement |
| S004-P14 → P15 | New sole detail | Room / off-screen child response | Prop-to-human continuity | Cut | Mother still holds shoe; family remains outside |
| S004 → S005 | Storage room | Doorway with visible living family | Same connection | Cut | No empty places or disappearing family |
| S005-P03 → P04 | Patient refusal / visible family | Edge willingness / external remaining snapshot | Clinical information introduction | Presentation State Change | Faces unobscured; 21 minutes not a live timer |
| S005-P05 | Status disclosure | Minimized assistance display | Clinical layer state change | Presentation State Change | Intervention not hidden or dominant |
| S005-P07 → P08 | Doorway / morning table context | Same table sunset meal | Chosen memory-scene change; external connection continues | Crossfade / Cut | No clock / universal acceleration rule |
| S005-P10 → P11 | Meal sufficiently progressed, no music | Low single-note melody / audible utensils | Late authored audio entrance | Presentation State Change | No melody before mother completes meal span |
| S005-P11 → P12 | Meal / coats | Child independently fitting shoe | Continuous farewell | Presentation State Change | No mandatory extra CG |
| S005-P13 → P14 | Assistance end / waiting | Patient consent while father washing | Clinical state change; family remains | Presentation State Change | No completion chime; consent after intervention ends |
| S005 → S006 | Farewell / prepared exit | Normal home entrance | Continuous departure | Cut | Family does not follow; melody continues initially |
| S006-P02 → P03 | Normal wall / exit | White localized outline and view disturbance | Single unresolved sensory event | Localized Overlay | Low vibration; Jinhee sees; immediate music stop at view disturbance |
| S006-P05 → P06 | Outline removed / exit | Black → actual daylight bedroom | Authored return / awakening | Fade to Black / Black to Scene | Jihun half-step behind; termination cue at real-room reveal |
| S006-P07 | Ordinary white silhouette nearby | Cyber joints / public Rope | First physical true-form reveal | Presentation State Change | Only now confirmed; no cosmic transformation |
| S006-P08 → P09 | Orientation / safety check | Brief stable awakening / care / reconnect facts | Medical layer appearance | Presentation State Change | Short secondary facts, not full recovery guarantee |
| S006-P11 → P12 | Bedroom | Residential corridor → matched present table glimpse → street | Physical departure with brief composition contrast | Cut | No prolonged empty-chair zoom or family lore |
| S006-P13 | Public street conversation | Appointment replaced with exam | Procedural schedule change | Presentation State Change | No surveillance threat / interactive calendar |
| S006 → S007 | Bright street | Doctor office next morning | Next-day appointment | Short Fade / Cut | No new time count or symptom recurrence added |
| S007-P01 → P02 | Consultation baseline / acute check | Sleep / meal record view | Evidence before explanation | Presentation State Change | Do not invent record numbers or new diagnosis |
| S007-P02 → P05 → P07 | Life record screen | Two-path diagram → people / window | Evidence → explanation → personal focus | Presentation State Change | No duplicate glossary or neural animation |
| S007-P10 → P11 | Rest / prescription discussion | Prescription confirmed with 승인 전 재평가 | Conditional clinical plan visible | Presentation State Change | Reassessment still required before approval |
| S007-P11 → S008-P01 | Prescription pending reassessment | Approved preparation days later | Recovery / reassessment elapsed | Short Fade / Cut | Elapsed-time caption only; approval not instantaneous |
| S008-P02 → P03 | Equal profiles behind briefing | Consent/signature then equal overview | Information-protection check before review | Presentation State Change | No new interactive consent branch |
| S008-P04 / P05 / P06 | Overview | Any profile detail / authored response → Back / overview | Transient browsing, not route selection | Presentation State Change | Reviewed feedback; no Hana preference; all content before neutral continuation |
| S008-P07 → P08 | Neutral browse return | Normal story / Hana confirmed then others stored | Canonical choice only after full review | Presentation State Change | No fake alternate route or memory-face effect |
| S008-P08 → P09 | Limited cooperation briefing | Three qualitative completion guide lines | Authored informational layer appearance | Presentation State Change | No race numbers or absolute mind-reading |
| S008-P11 → P12 | Briefing | Bed / patch / later Hana reread | Preparation within room | Presentation State Change | Later thought stays here |
| S008-P12 → P13 | Preparation room / connection speech | Fade / black with equipment→rain→bus | Connection boundary led by sound | Fade to Black / Audio-led Transition | No era BG/gameplay, woman whisper or memory film |
| S008-P13 → P14 | Black with ending sounds | Black-screen part title / scope end | Part endpoint | Presentation State Change | No added fanfare or next-era content |


## Audio cue map

All **two production BGM roles are NEW_PRODUCTION_REQUIRED**. Previous Suno Free-workflow tracks may be REFERENCE_ONLY for mood comparison; no track is claimed as available or eligible final production, and no individual old-track inventory is inferred. No prompts or generation requests in M10-02. Silence in S004 is mandatory; S007/S008 have no authored music, so this plan adds none. Silence does not erase an authored causal sound.


| BGM requirement / coverage | Narrative role / emotion | Begin | Continue / lower | Fade / stop | Loop intent | Avoid | Priority |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [REQ-BGM-001](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-001); S001–S003 | Ordinary warmth; life carries contradiction | S001-P03 L45, after teasing / before first narration | S001 onward; S002-P01 lowered behind life, remain through contradictions / hall approach | S003-P04 L350 dog touch fade to silence; absent S004 | Likely loop for variable reading span; seam reviewed later | Suspense, sadness, joke accents, dream cue | P1 / M10-05 |
| [REQ-BGM-002](M10_MASTER_ASSET_GAP_LIST.md#req-bgm-002); late S005–S006 | Low restrained farewell, not victory | S005-P11 L784 only after meal has sufficiently progressed | Meal farewell / consent / normal entry departure | S006-P03 L892 immediate stop; do not smooth anomaly silence | Likely loop for variable reading / consent span; restrained ending | Swelling grief from scene opening, fanfare, cosmic motif | P1 / M10-06 |


### Ambient versus one-shot

| Requirement | Scenes / cues | Role | Playback / boundary | Technical disposition |
| --- | --- | --- | --- | --- |
| [REQ-AMB-001](M10_MASTER_ASSET_GAP_LIST.md#req-amb-001) | S001–S005 | Optional continuous ordinary room life | Low behind text / BGM; no must-play hum | TECHNICAL_REVIEW_M10_03_OR_04; omit bed if discrete authored cues suffice |
| [REQ-AMB-002](M10_MASTER_ASSET_GAP_LIST.md#req-amb-002) | S008-P13 L1375–1379 | Rain replacing connection sound | Gradual handoff under fade / black; duration not frozen | TECHNICAL_REVIEW_M10_03_OR_04; finite one-shot sequencing possible only after review |


### SFX and necessary action roles

A temporary technical slice may omit P2 audio; final authored sound cues must be retained even when their priority is P2. Only inferred optional physical-action sound renderings may be omitted when prose / staging already communicates them. Authored perceptual and ending cues retain semantic validation priority; no decorative quota or extra laugh / clue / completion sound. The repeated laugh uses the same diegetic recording twice total, not two distinct laugh assets.

| Requirement | Scene / presentation cue | Role / timing | Authorship / constraint | Priority |
| --- | --- | --- | --- | --- |
| [REQ-SFX-001](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-001) | S001; Black opening | Household sound before image | Authored L17; finite opening sound, no required looping subsystem | P1 |
| [REQ-SFX-002](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-002) | S001; Black opening | Ground kitchen before image | Authored L17; single placement | P2 |
| [REQ-SFX-003](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-003) | S001; L77 before entry cut | Arrival interrupts breakfast | Authored; no surprise stinger | P1 |
| [REQ-SFX-004](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-004) | S001; L89 narration | Child moves before father rises | Authored narration; short causal steps | P2 |
| [REQ-SFX-005](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-005) | S001; L90 | Fifth guest place is familiar | Necessary action cue if audible staging used; prose remains sufficient | P2 |
| [REQ-SFX-006](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-006) | S002; L294 once | Jihun personal hesitation / symptom | Authored single occurrence; no child/father hallucination | P1 |
| [REQ-SFX-007](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-007) | S003; L338 from living room, then identical replay | Ordinary activity repeats | Authored exact same length and pitch twice total; diegetic, not laugh track | P1 |
| [REQ-SFX-008](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-008) | S004; After explicit release, L539–544 | Only patient opens after manual hold | Necessary action cue; no creak horror; must not play before release | P2 |
| [REQ-SFX-009](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-009) | S004; L549–551 | Objects read as handled evidence | Optional causal rendering of authored action; no audio fallback acceptable | P2 |
| [REQ-SFX-010](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-010) | S004; L556–565 | Readable document follows handling | Optional causal rendering; no clue chime | P2 |
| [REQ-SFX-011](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-011) | S004; L590–592 | Unused sole becomes visible | Optional causal rendering; gentle one-shot | P2 |
| [REQ-SFX-012](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-012) | S005; Late meal L779–784 and farewell | Room sound persists between dialogue | Authored L784; sparse one-shots at physical action, not a metronome | P1 |
| [REQ-SFX-013](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-013) | S005; L840–842 | Minseok remains present during consent | Authored narration; finite segment if used, no new water-loop system | P2 |
| [REQ-SFX-014](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-014) | S006; L883–885 exactly twice | Ordinary departure baseline | Authored narration; mundane and quiet | P2 |
| [REQ-SFX-015](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-015) | S006; L887 | Localized sensory anomaly | Authored; restrained; no cosmic / horror bass swell | P1 |
| [REQ-SFX-016](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-016) | S006; L918 on daylight reveal | Actual connection ends | Authored short cue; not success fanfare | P1 |
| [REQ-SFX-017](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-017) | S006; L984–986 | Procedural completion and departure | Optional causal rendering of narration; no machinery reveal sound before L924 | P2 |
| [REQ-SFX-018](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-018) | S008; L1375 after spoken connection | Connection leads into rain | Authored gradually replaced by rain; handoff technical review | P1 |
| [REQ-SFX-019](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-019) | S008; L1377–1378 under fade / black | First sensory rain cue | Authored narration; distinguish onset from environmental bed | P1 |
| [REQ-SFX-020](M10_MASTER_ASSET_GAP_LIST.md#req-sfx-020) | S008; L1379 after rain onset | Ending audio situates next boundary | Authored; no bus image / era scene required | P1 |


## Canon fidelity audit — second direct-source pass

The full 1,400-line source and all six required documentation inputs were reviewed. **59 / 59 scene bracket directions MAPPED; 0 DEFERRED_WITH_REASON; 0 silently omitted.** The direction text below is an audit cue only and must not become visible dialogue. Global notation / tone guidance (L7–11) and all 12 continuity notes (L1389–1400) are EXPLICITLY_NOT_PLAYER_VISIBLE; their constraints are retained. Coverage spans include every authored player block, not just production directions.


| Source line / scene | Bracketed canonical direction | Disposition | Presentation beat |
| --- | --- | --- | --- |
| L17 / S001 | [화면: 검정. 음악 없음. 프라이팬에 기름이 튀는 소리. 접시가 조리대에 놓인다.] | MAPPED | S001-P01 |
| L25 / S001 | [배경: 아침 빛이 드는 주방. 인물보다 프라이팬을 먼저 보여준다. 후라이 다섯 개.] | MAPPED | S001-P02 |
| L45 / S001 | [음악: 가벼운 아침 테마. 웃음을 강조하는 효과음은 넣지 않는다.] | MAPPED | S001-P03 |
| L69 / S001 | [민석이 접시를 내려다본다. 남은 버섯 하나가 이미 나래 접시로 옮겨져 있다.] | MAPPED | S001-P04 |
| L77 / S001 | [초인종. 진희가 뜨거운 프라이팬을 내려놓는다.] | MAPPED | S001-P05 |
| L92 / S001 | [배경: 현관. 지훈과 작은 흰 강아지. 강아지는 평범한 동물처럼 보인다.] | MAPPED | S001-P07 |
| L131 / S001 | [인서트: 뒤축이 접힌 작은 운동화. 준호의 뒤꿈치가 밖으로 밀려 있다.] | MAPPED | S001-P09 |
| L142 / S001 | [진희가 대답하려다 식탁으로 시선을 돌린다.] | MAPPED | S001-P10 |
| L151 / S001 | [인서트: ‘다음 방문 — 아침 식사 후’. 작성자나 병원 표식은 보이지 않는다.] | MAPPED | S001-P11 |
| L165 / S001 | [화면: 다섯 사람의 식탁. 비어 있는 의자 없음. 강아지가 지훈의 발 옆에 눕는다.] | MAPPED | S001-P13 |
| L171 / S002 | [배경: 거실. 아침 식사가 끝난 직후. 음악은 생활 소리 뒤로 낮춘다.] | MAPPED | S002-P01 |
| L228 / S002 | [진희의 표정 변화는 작게. 음악을 끊지 않는다.] | MAPPED | S002-P04 |
| L264 / S002 | [진희의 시선이 복도 끝으로 간다. 지훈은 그 시선을 따라가되 자리에서 바로 일어나지 않는다.] | MAPPED | S002-P07 |
| L294 / S002 | [효과음: 의자 다리가 바닥을 스치는 소리를 한 번만 가까이 들려준다. 별도 아이 목소리나 ‘아빠’라는 환청은 넣지 않는다.] | MAPPED | S002-P09 |
| L314 / S003 | [배경: 거실에서 이어지는 복도. 닫힌 방문.] | MAPPED | S003-P01 |
| L338 / S003 | [효과음: 거실에서 준호가 웃는 소리. 잠시 뒤 똑같은 길이와 높이로 한 번 반복된다.] | MAPPED | S003-P02 |
| L343 / S003 | [지훈의 시선이 손잡이에서 식탁으로 흔들린다. 남편의 얼굴을 위협적으로 변형하지 않는다.] | MAPPED | S003-P03 |
| L350 / S003 | [흰 강아지가 지훈의 발목에 앞발을 댄다. 음악 페이드아웃.] | MAPPED | S003-P04 |
| L364 / S003 | [첫 접속 정보 표시. 간결한 반투명 인터페이스.] | MAPPED | S003-P06 |
| L387 / S003 | [민석이 조금 비켜선다. 지훈은 거실 쪽으로 먼저 물러난다.] | MAPPED | S003-P08 |
| L422 / S004 | [배경: 거실. 지훈은 준호의 작은 신발을 집어 진희에게 건넨다.] | MAPPED | S004-P01 |
| L436 / S004 | [진희가 기억을 더듬듯 두 손으로 상자의 크기를 만든다.] | MAPPED | S004-P02 |
| L481 / S004 | [지훈의 상태창은 표시하되 수치 상승 애니메이션은 넣지 않는다.] | MAPPED | S004-P04 |
| L502 / S004 | [진희가 일어난다. 민석은 이번에도 복도 쪽에 서지만, 진희의 진로를 강제로 막지 않는다.] | MAPPED | S004-P06 |
| L523 / S004 | [진희가 닫힌 문 앞에 선다. 지훈은 한 걸음 뒤에 머문다.] | MAPPED | S004-P07 |
| L537 / S004 | [긴 침묵은 자동 진행시키지 않는다. 플레이어가 다음 문장으로 넘길 때 손잡이를 움직인다.] | MAPPED | S004-P08 |
| L544 / S004 | [배경: 보관방. 신발 상자, 돌려받은 여행 가방, 소지품 봉투. 서류는 가까이 다가간 뒤에만 읽히게 한다. 음악 없음.] | MAPPED | S004-P10 |
| L564 / S004 | [진희가 서류를 덮는다. 손을 치웠다가 다시 펼친다.] | MAPPED | S004-P13 |
| L624 / S005 | [배경: 보관방 문 앞. 거실의 가족은 그대로 보인다. 빈자리로 바꾸지 않는다.] | MAPPED | S005-P01 |
| L683 / S005 | [지훈의 시야 가장자리: 귀환 의향 — 미형성. 외부 접속 잔여 — 21분. 인물 얼굴을 가리지 않는다.] | MAPPED | S005-P04 |
| L700 / S005 | [접속 표시: ‘투영 반응 보조 / 기존 반응 범위 / 개입 기록 보존’. 이후 상태창은 최소화한다. 이것이 조정된 반응임을 플레이어에게 숨기지 않는다.] | MAPPED | S005-P05 |
| L737 / S005 | [배경 전환: 같은 식탁, 노을. 시간이 흐르는 시계 연출 없이 진희가 선택한 저녁 장면으로 변한다. 외부 접속은 그대로 이어진다.] | MAPPED | S005-P08 |
| L761 / S005 | [민석이 찬장을 연다. 잠깐 망설인 뒤 파란 컵을 꺼낸다.] | MAPPED | S005-P09 |
| L784 / S005 | [음악: 식사가 충분히 진행된 뒤에만 낮은 단음 선율을 시작한다. 대사 사이의 식기 소리를 살린다.] | MAPPED | S005-P11 |
| L829 / S005 | [로프의 표시: 귀환 선택 대기. 완료음 없음.] | MAPPED | S005-P13 |
| L865 / S006 | [배경: 현관. 평범한 출입문 너머로 준비된 귀환 통로가 연결된다. 가족은 뒤에서 따라 나오지 않는다.] | MAPPED | S006-P01 |
| L887 / S006 | [효과음: 아주 낮은 진동. 문 옆 벽에 흰 문이 잠깐 겹친다. 불꽃·별·우주선 형태는 보이지 않는다.] | MAPPED | S006-P02 |
| L892 / S006 | [지훈의 시야가 흔들린다. 음악 즉시 중단.] | MAPPED | S006-P03 |
| L916 / S006 | [흰 문의 윤곽이 사라진다. 지훈이 진희보다 반걸음 뒤에서 출구를 지난다. 페이드아웃.] | MAPPED | S006-P05 |
| L918 / S006 | [배경: 미래도시의 진희 침실. 주간. 차분한 자연광. 치료 장비의 짧은 종료음.] | MAPPED | S006-P06 |
| L924 / S006 | [흰 강아지가 기지개를 켠다. 어깨와 발목에서 금속 관절이 드러난다. 처음으로 실제 사이버 도그의 모습이 확인된다.] | MAPPED | S006-P07 |
| L960 / S006 | [화면 표시: 각성 반응 안정 / 후속 관리 알림 전송 / 재접속 제한 적용. 의료 자료는 짧게 보여준다.] | MAPPED | S006-P09 |
| L988 / S006 | [배경: 주거 구역 복도 → 거리. 가족의 집과 같은 구도로 현재의 식탁을 잠깐 보여준다. 빈 의자를 오래 확대하지 않는다.] | MAPPED | S006-P12 |
| L1027 / S006 | [배경: 밝은 도시 거리. 위협적인 시스템 음성이나 정체불명의 감시 화면을 추가하지 않는다.] | MAPPED | S006-P14 |
| L1033 / S007 | [배경: 이도경의 진료실. 다음 날 오전. 책상 아래 로프.] | MAPPED | S007-P01 |
| L1056 / S007 | [도경이 화면을 바꾼다. 수면과 식사 기록.] | MAPPED | S007-P02 |
| L1085 / S007 | [침묵. 지훈이 답하려다 멈춘다.] | MAPPED | S007-P03 |
| L1121 / S007 | [화면: 서로 다른 두 경로가 말기 구간으로 이어지는 단순 도식. 긴 의학 설명이나 신경 영상 애니메이션은 사용하지 않는다.] | MAPPED | S007-P05 |
| L1153 / S007 | [지훈이 도식을 더 보지 않고 창밖을 본다.] | MAPPED | S007-P07 |
| L1204 / S007 | [도경이 지훈을 본다.] | MAPPED | S007-P10 |
| L1212 / S007 | [도경이 처방을 확정한다. 화면에 ‘승인 전 재평가’가 함께 표시된다.] | MAPPED | S007-P11 |
| L1218 / S008 | [배경: 몰입 치료 준비실. 며칠 뒤. 자막은 경과 시간만 표시한다. 통증과 감각 혼선이 재발하지 않았음을 확인한 후 접속을 승인한다.] | MAPPED | S008-P01 |
| L1229 / S008 | [화면: ‘시대 몰입 / 재구성 한국 도시 / 2000년대 초의 생활 환경’. 세 프로필 카드. 선택 전 카드는 동일한 밝기로 제시한다.] | MAPPED | S008-P02 |
| L1246 / S008 | [지훈이 참여 방식과 녹화·정보 보호 범위를 확인한다. 서명 화면을 짧게 보여준다.] | MAPPED | S008-P03 |
| L1290 / S008 | [화면: 하나 선택 확정. 나머지 프로필은 보관됨. 하나의 얼굴을 특별한 기억 효과로 강조하지 않는다.] | MAPPED | S008-P08 |
| L1313 / S008 | [화면: 정상 종료 확인 항목 세 줄. 숫자로 경쟁시키지 않는다.] | MAPPED | S008-P09 |
| L1362 / S008 | [은정이 패치를 붙인다. 로프가 침상 옆에 앉는다.] | MAPPED | S008-P12 |
| L1375 / S008 | [페이드아웃. 장비의 연결음을 서서히 빗소리로 교체한다. 연결 후 별도의 여성 속삭임이나 의미심장한 기억 영상은 넣지 않는다.] | MAPPED | S008-P13 |
| L1381 / S008 | [검정 화면에 부 제목.] | MAPPED | S008-P14 |


### Player-content block ledger

Each bold source block is listed once with its original source range and beat. Dialogue, narration, monologue and internal paragraphs remain individual advances; information/profile/title blocks remain non-speaker presentation. This ledger plus the direction ledger checks BGM, SFX, silence, BG, insert/CG implications, information, channels, prose, transitions and interaction notes.

| Source range | Canonical role | Presentation beat |
| --- | --- | --- |
| L19–20 | 준호 | S001-P01 |
| L22–23 | 나래 | S001-P01 |
| L27–28 | 진희 | S001-P02 |
| L30–31 | 준호 | S001-P02 |
| L33–34 | 민석 | S001-P02 |
| L36–37 | 나래 | S001-P02 |
| L39–40 | 민석 | S001-P02 |
| L42–43 | 진희 | S001-P02 |
| L47–49 | 서술 | S001-P03 |
| L51–52 | 민석 | S001-P03 |
| L54–55 | 진희 | S001-P03 |
| L57–58 | 준호 | S001-P03 |
| L60–61 | 진희 | S001-P03 |
| L63–64 | 민석 | S001-P03 |
| L66–67 | 준호 | S001-P03 |
| L71–72 | 나래 | S001-P04 |
| L74–75 | 준호 | S001-P04 |
| L79–80 | 진희 | S001-P05 |
| L82–83 | 민석 | S001-P05 |
| L85–86 | 진희 | S001-P05 |
| L88–90 | 서술 | S001-P06 |
| L94–95 | 준호 | S001-P07 |
| L97–98 | 지훈 | S001-P07 |
| L100–101 | 준호 | S001-P07 |
| L103–104 | 지훈 | S001-P07 |
| L106–107 | 준호 | S001-P07 |
| L109–110 | 지훈 | S001-P07 |
| L112–113 | 준호 | S001-P07 |
| L115–116 | 나래·화면 밖 | S001-P07 |
| L118–120 | 서술 | S001-P08 |
| L122–123 | 진희 | S001-P08 |
| L125–126 | 지훈 | S001-P08 |
| L128–129 | 진희 | S001-P08 |
| L133–134 | 준호 | S001-P09 |
| L136–137 | 진희 | S001-P09 |
| L139–140 | 준호 | S001-P09 |
| L144–145 | 진희 | S001-P10 |
| L147–149 | 서술 | S001-P10 |
| L153–154 | 진희 | S001-P12 |
| L156–157 | 지훈 | S001-P12 |
| L159–160 | 민석 | S001-P12 |
| L162–163 | 진희 | S001-P12 |
| L173–174 | 진희 | S002-P01 |
| L176–177 | 지훈 | S002-P01 |
| L179–180 | 진희 | S002-P01 |
| L182–183 | 지훈 | S002-P01 |
| L185–186 | 진희 | S002-P01 |
| L188–189 | 나래 | S002-P01 |
| L191–192 | 준호 | S002-P01 |
| L194–195 | 민석 | S002-P01 |
| L197–199 | 서술 | S002-P02 |
| L201–202 | 지훈 | S002-P02 |
| L204–205 | 진희 | S002-P02 |
| L207–208 | 민석 | S002-P02 |
| L210–211 | 진희 | S002-P02 |
| L213–214 | 나래 | S002-P02 |
| L216–217 | 지훈 | S002-P03 |
| L219–220 | 진희 | S002-P03 |
| L222–223 | 지훈 | S002-P03 |
| L225–226 | 진희 | S002-P03 |
| L230–231 | 진희 | S002-P04 |
| L233–234 | 지훈 | S002-P04 |
| L236–237 | 진희 | S002-P04 |
| L239–240 | 지훈 | S002-P04 |
| L242–243 | 진희 | S002-P04 |
| L245–246 | 민석 | S002-P04 |
| L248–250 | 서술 | S002-P05 |
| L252–253 | 준호 | S002-P06 |
| L255–256 | 진희 | S002-P06 |
| L258–259 | 준호 | S002-P06 |
| L261–262 | 진희 | S002-P06 |
| L266–267 | 지훈 | S002-P07 |
| L269–270 | 진희 | S002-P07 |
| L272–273 | 지훈 | S002-P07 |
| L275–276 | 진희 | S002-P07 |
| L278–279 | 준호 | S002-P07 |
| L281–282 | 진희 | S002-P07 |
| L284–285 | 민석 | S002-P07 |
| L287–288 | 나래 | S002-P07 |
| L290–292 | 서술 | S002-P08 |
| L296–298 | 서술 | S002-P09 |
| L300–301 | 진희 | S002-P10 |
| L303–304 | 지훈 | S002-P10 |
| L306–308 | 서술 | S002-P10 |
| L316–317 | 지훈 | S003-P01 |
| L319–320 | 진희 | S003-P01 |
| L322–323 | 민석 | S003-P01 |
| L325–327 | 서술 | S003-P01 |
| L329–330 | 민석 | S003-P01 |
| L332–333 | 지훈 | S003-P01 |
| L335–336 | 진희 | S003-P01 |
| L340–341 | 준호·화면 밖 | S003-P02 |
| L345–348 | 서술 | S003-P03 |
| L352–353 | 로프·내부 | S003-P04 |
| L355–356 | 지훈·내부 | S003-P05 |
| L358–359 | 로프·내부 | S003-P05 |
| L361–362 | 지훈·내부 | S003-P05 |
| L366–369 | 접속 정보 | S003-P06 |
| L371–372 | 지훈·내부 | S003-P06 |
| L374–375 | 로프·내부 | S003-P06 |
| L377–379 | 서술 | S003-P07 |
| L381–382 | 진희 | S003-P07 |
| L384–385 | 지훈 | S003-P07 |
| L389–390 | 로프·내부 | S003-P08 |
| L392–393 | 지훈·내부 | S003-P08 |
| L395–396 | 로프·내부 | S003-P08 |
| L398–400 | 지훈·독백 | S003-P09 |
| L402–404 | 지훈·독백 | S003-P09 |
| L406–407 | 로프·내부 | S003-P10 |
| L409–410 | 지훈·내부 | S003-P10 |
| L412–413 | 진희 | S003-P10 |
| L415–416 | 지훈 | S003-P10 |
| L424–425 | 지훈 | S004-P01 |
| L427–428 | 진희 | S004-P01 |
| L430–431 | 지훈 | S004-P01 |
| L433–434 | 진희 | S004-P01 |
| L438–439 | 진희 | S004-P02 |
| L441–442 | 지훈 | S004-P02 |
| L444–445 | 진희 | S004-P02 |
| L447–448 | 지훈 | S004-P02 |
| L450–451 | 진희 | S004-P02 |
| L453–454 | 지훈 | S004-P02 |
| L456–457 | 진희 | S004-P02 |
| L459–461 | 서술 | S004-P03 |
| L463–464 | 진희 | S004-P03 |
| L466–467 | 준호 | S004-P03 |
| L469–470 | 민석 | S004-P03 |
| L472–473 | 진희 | S004-P03 |
| L475–476 | 준호 | S004-P03 |
| L478–479 | 진희 | S004-P03 |
| L483–485 | 지훈·독백 | S004-P04 |
| L487–488 | 진희 | S004-P05 |
| L490–491 | 민석 | S004-P05 |
| L493–494 | 진희 | S004-P05 |
| L496–497 | 민석 | S004-P05 |
| L499–500 | 진희 | S004-P05 |
| L504–505 | 진희 | S004-P06 |
| L507–508 | 지훈 | S004-P06 |
| L510–511 | 진희 | S004-P06 |
| L513–514 | 지훈 | S004-P06 |
| L516–518 | 서술 | S004-P06 |
| L520–521 | 진희 | S004-P06 |
| L525–526 | 진희 | S004-P07 |
| L528–529 | 지훈 | S004-P07 |
| L531–532 | 진희 | S004-P07 |
| L534–535 | 지훈 | S004-P07 |
| L539–542 | 서술 | S004-P09 |
| L546–547 | 진희 | S004-P10 |
| L549–551 | 서술 | S004-P11 |
| L553–554 | 진희 | S004-P11 |
| L556–559 | 서술 | S004-P12 |
| L561–562 | 진희 | S004-P12 |
| L566–567 | 진희 | S004-P13 |
| L569–570 | 지훈 | S004-P13 |
| L572–573 | 진희 | S004-P13 |
| L575–576 | 지훈 | S004-P13 |
| L578–579 | 진희 | S004-P13 |
| L581–582 | 지훈 | S004-P13 |
| L584–585 | 진희 | S004-P13 |
| L587–588 | 지훈 | S004-P13 |
| L590–592 | 서술 | S004-P14 |
| L594–595 | 진희 | S004-P14 |
| L597–598 | 진희 | S004-P14 |
| L600–601 | 진희 | S004-P14 |
| L603–604 | 진희 | S004-P14 |
| L606–608 | 서술 | S004-P15 |
| L610–611 | 준호·화면 밖 | S004-P15 |
| L613–614 | 진희 | S004-P15 |
| L616–618 | 지훈·독백 | S004-P16 |
| L626–627 | 진희 | S005-P01 |
| L629–630 | 지훈 | S005-P01 |
| L632–633 | 진희 | S005-P01 |
| L635–636 | 지훈 | S005-P01 |
| L638–639 | 진희 | S005-P01 |
| L641–643 | 서술 | S005-P02 |
| L645–646 | 진희 | S005-P02 |
| L648–649 | 준호 | S005-P02 |
| L651–653 | 서술 | S005-P02 |
| L655–656 | 지훈 | S005-P03 |
| L658–659 | 진희 | S005-P03 |
| L661–662 | 지훈 | S005-P03 |
| L664–665 | 진희 | S005-P03 |
| L667–668 | 지훈 | S005-P03 |
| L670–671 | 진희 | S005-P03 |
| L673–675 | 서술 | S005-P03 |
| L677–678 | 진희 | S005-P03 |
| L680–681 | 진희 | S005-P03 |
| L685–686 | 지훈·내부 | S005-P04 |
| L688–689 | 로프·내부 | S005-P04 |
| L691–692 | 지훈·내부 | S005-P04 |
| L694–695 | 로프·내부 | S005-P04 |
| L697–698 | 지훈·내부 | S005-P04 |
| L702–703 | 민석 | S005-P06 |
| L705–707 | 서술 | S005-P06 |
| L709–710 | 민석 | S005-P06 |
| L712–713 | 진희 | S005-P06 |
| L715–716 | 민석 | S005-P06 |
| L718–719 | 나래 | S005-P06 |
| L721–722 | 준호 | S005-P06 |
| L724–725 | 진희 | S005-P06 |
| L727–729 | 서술 | S005-P06 |
| L731–732 | 진희 | S005-P07 |
| L734–735 | 지훈 | S005-P07 |
| L739–741 | 서술 | S005-P08 |
| L743–744 | 진희 | S005-P08 |
| L746–747 | 나래 | S005-P08 |
| L749–750 | 민석 | S005-P08 |
| L752–753 | 진희 | S005-P08 |
| L755–756 | 민석 | S005-P08 |
| L758–759 | 진희 | S005-P08 |
| L763–764 | 준호 | S005-P09 |
| L766–767 | 민석 | S005-P09 |
| L769–771 | 서술 | S005-P09 |
| L773–774 | 나래 | S005-P09 |
| L776–777 | 진희 | S005-P09 |
| L779–782 | 서술 | S005-P10 |
| L786–787 | 민석 | S005-P11 |
| L789–790 | 진희 | S005-P11 |
| L792–793 | 민석 | S005-P11 |
| L795–796 | 진희 | S005-P11 |
| L798–800 | 서술 | S005-P11 |
| L802–803 | 나래 | S005-P11 |
| L805–806 | 진희 | S005-P11 |
| L808–809 | 나래 | S005-P11 |
| L811–814 | 서술 | S005-P12 |
| L816–817 | 준호 | S005-P12 |
| L819–820 | 진희 | S005-P12 |
| L822–824 | 서술 | S005-P12 |
| L826–827 | 진희 | S005-P12 |
| L831–832 | 지훈·내부 | S005-P13 |
| L834–835 | 로프·내부 | S005-P13 |
| L837–838 | 지훈 | S005-P14 |
| L840–842 | 서술 | S005-P14 |
| L844–845 | 진희 | S005-P14 |
| L847–848 | 지훈 | S005-P14 |
| L850–851 | 진희 | S005-P14 |
| L853–855 | 서술 | S005-P15 |
| L857–859 | 지훈·독백 | S005-P15 |
| L867–868 | 진희 | S006-P01 |
| L870–871 | 지훈 | S006-P01 |
| L873–874 | 진희 | S006-P01 |
| L876–878 | 서술 | S006-P01 |
| L880–881 | 지훈 | S006-P01 |
| L883–885 | 서술 | S006-P01 |
| L889–890 | 진희 | S006-P02 |
| L894–896 | 지훈·독백 | S006-P03 |
| L898–899 | 로프·내부 | S006-P04 |
| L901–902 | 지훈·내부 | S006-P04 |
| L904–905 | 로프·내부 | S006-P04 |
| L907–908 | 지훈 | S006-P04 |
| L910–911 | 진희 | S006-P04 |
| L913–914 | 지훈 | S006-P04 |
| L920–922 | 서술 | S006-P06 |
| L926–927 | 로프 | S006-P07 |
| L929–930 | 지훈 | S006-P08 |
| L932–933 | 진희 | S006-P08 |
| L935–936 | 지훈 | S006-P08 |
| L938–939 | 진희 | S006-P08 |
| L941–942 | 지훈 | S006-P08 |
| L944–945 | 진희 | S006-P08 |
| L947–948 | 지훈 | S006-P08 |
| L950–952 | 서술 | S006-P08 |
| L954–955 | 지훈 | S006-P08 |
| L957–958 | 진희 | S006-P08 |
| L962–963 | 로프 | S006-P09 |
| L965–966 | 지훈 | S006-P09 |
| L968–969 | 로프 | S006-P09 |
| L971–973 | 서술 | S006-P10 |
| L975–976 | 진희 | S006-P10 |
| L978–979 | 지훈 | S006-P10 |
| L981–982 | 진희 | S006-P10 |
| L984–986 | 서술 | S006-P11 |
| L990–991 | 로프 | S006-P12 |
| L993–994 | 지훈 | S006-P12 |
| L996–997 | 로프 | S006-P12 |
| L999–1000 | 지훈 | S006-P12 |
| L1002–1003 | 로프 | S006-P12 |
| L1005–1006 | 지훈 | S006-P12 |
| L1008–1009 | 로프 | S006-P13 |
| L1011–1012 | 지훈 | S006-P13 |
| L1014–1015 | 로프 | S006-P13 |
| L1017–1019 | 서술 | S006-P13 |
| L1021–1022 | 지훈 | S006-P13 |
| L1024–1025 | 로프 | S006-P13 |
| L1035–1036 | 도경 | S007-P01 |
| L1038–1039 | 지훈 | S007-P01 |
| L1041–1042 | 도경 | S007-P01 |
| L1044–1045 | 지훈 | S007-P01 |
| L1047–1048 | 도경 | S007-P01 |
| L1050–1051 | 지훈 | S007-P01 |
| L1053–1054 | 도경 | S007-P01 |
| L1058–1059 | 도경 | S007-P02 |
| L1061–1062 | 지훈 | S007-P02 |
| L1064–1065 | 도경 | S007-P02 |
| L1067–1068 | 지훈 | S007-P02 |
| L1070–1071 | 도경 | S007-P02 |
| L1073–1074 | 지훈 | S007-P02 |
| L1076–1077 | 도경 | S007-P02 |
| L1079–1080 | 지훈 | S007-P02 |
| L1082–1083 | 도경 | S007-P02 |
| L1087–1088 | 지훈 | S007-P03 |
| L1090–1091 | 도경 | S007-P04 |
| L1093–1094 | 지훈 | S007-P04 |
| L1096–1097 | 도경 | S007-P04 |
| L1099–1100 | 지훈 | S007-P04 |
| L1102–1103 | 도경 | S007-P04 |
| L1105–1106 | 지훈 | S007-P04 |
| L1108–1109 | 로프 | S007-P04 |
| L1111–1113 | 서술 | S007-P04 |
| L1115–1116 | 지훈 | S007-P04 |
| L1118–1119 | 도경 | S007-P04 |
| L1123–1124 | 도경 | S007-P05 |
| L1126–1127 | 지훈 | S007-P05 |
| L1129–1130 | 도경 | S007-P05 |
| L1132–1133 | 지훈 | S007-P05 |
| L1135–1136 | 도경 | S007-P05 |
| L1138–1139 | 지훈 | S007-P06 |
| L1141–1142 | 도경 | S007-P06 |
| L1144–1145 | 도경 | S007-P06 |
| L1147–1148 | 지훈 | S007-P06 |
| L1150–1151 | 도경 | S007-P06 |
| L1155–1156 | 도경 | S007-P07 |
| L1158–1159 | 지훈 | S007-P07 |
| L1161–1162 | 도경 | S007-P08 |
| L1164–1165 | 지훈 | S007-P08 |
| L1167–1168 | 도경 | S007-P08 |
| L1170–1171 | 지훈 | S007-P08 |
| L1173–1174 | 도경 | S007-P08 |
| L1176–1177 | 지훈 | S007-P08 |
| L1179–1180 | 도경 | S007-P08 |
| L1182–1184 | 서술 | S007-P08 |
| L1186–1187 | 도경 | S007-P09 |
| L1189–1190 | 지훈 | S007-P09 |
| L1192–1193 | 도경 | S007-P09 |
| L1195–1196 | 지훈 | S007-P10 |
| L1198–1199 | 도경 | S007-P10 |
| L1201–1202 | 지훈 | S007-P10 |
| L1206–1207 | 지훈 | S007-P10 |
| L1209–1210 | 도경 | S007-P10 |
| L1220–1221 | 은정 | S008-P01 |
| L1223–1224 | 지훈 | S008-P01 |
| L1226–1227 | 은정 | S008-P01 |
| L1231–1232 | 은정 | S008-P02 |
| L1234–1235 | 지훈 | S008-P02 |
| L1237–1238 | 은정 | S008-P02 |
| L1240–1241 | 지훈 | S008-P02 |
| L1243–1244 | 은정 | S008-P02 |
| L1248–1252 | 프로필 1 — 선주 | S008-P04 |
| L1254–1255 | 지훈·독백 | S008-P04 |
| L1257–1261 | 프로필 2 — 해진 | S008-P05 |
| L1263–1264 | 지훈·독백 | S008-P05 |
| L1266–1270 | 프로필 3 — 하나 | S008-P06 |
| L1272–1273 | 지훈 | S008-P07 |
| L1275–1276 | 은정 | S008-P07 |
| L1278–1279 | 지훈 | S008-P07 |
| L1281–1282 | 은정 | S008-P07 |
| L1284–1285 | 지훈 | S008-P07 |
| L1287–1288 | 은정 | S008-P07 |
| L1292–1293 | 은정 | S008-P08 |
| L1295–1296 | 지훈 | S008-P08 |
| L1298–1299 | 은정 | S008-P08 |
| L1301–1302 | 지훈 | S008-P08 |
| L1304–1305 | 은정 | S008-P08 |
| L1307–1308 | 지훈 | S008-P08 |
| L1310–1311 | 은정 | S008-P08 |
| L1315–1318 | 진행 안내 | S008-P09 |
| L1320–1321 | 은정 | S008-P09 |
| L1323–1324 | 지훈 | S008-P09 |
| L1326–1327 | 은정 | S008-P09 |
| L1329–1330 | 지훈·독백 | S008-P09 |
| L1332–1333 | 은정 | S008-P10 |
| L1335–1336 | 지훈 | S008-P10 |
| L1338–1339 | 은정 | S008-P10 |
| L1341–1342 | 로프 | S008-P10 |
| L1344–1345 | 지훈 | S008-P10 |
| L1347–1348 | 은정 | S008-P11 |
| L1350–1351 | 지훈 | S008-P11 |
| L1353–1354 | 은정 | S008-P11 |
| L1356–1357 | 지훈 | S008-P11 |
| L1359–1360 | 은정 | S008-P11 |
| L1364–1366 | 서술 | S008-P12 |
| L1368–1370 | 지훈·독백 | S008-P12 |
| L1372–1373 | 은정 | S008-P12 |
| L1377–1379 | 서술 | S008-P13 |
| L1383–1383 | 1부 — 깨우는 사람 / 끝 | S008-P14 |


### Non-player-visible source constraints

| Source | Disposition | Application |
| --- | --- | --- |
| L7, L9, L11 | EXPLICITLY_NOT_PLAYER_VISIBLE | Notation / cast / ordinary opening; vocabulary and scene maps enforce it |
| L1389 note 1 | EXPLICITLY_NOT_PLAYER_VISIBLE | Five-place intent via S001-P02/P11/P13, no explanatory extra dialogue |
| L1390 note 2 | EXPLICITLY_NOT_PLAYER_VISIBLE | Old/new shoe and returned bag sequence in S004; paper appears only after proximity |
| L1391 note 3 | EXPLICITLY_NOT_PLAYER_VISIBLE | Jinhee agency and manual hold; no forced opening / abrupt death assertion |
| L1392 note 4 | EXPLICITLY_NOT_PLAYER_VISIBLE | S005 assistance disclosure / end / consent, not real family will or recovery promise |
| L1393 note 5 | EXPLICITLY_NOT_PLAYER_VISIBLE | No new Jihun sentimental prose or touching Jinhee hand |
| L1394 note 6 | EXPLICITLY_NOT_PLAYER_VISIBLE | S006 functioning follow-up and minimum safety checks |
| L1395 note 7 | EXPLICITLY_NOT_PLAYER_VISIBLE | S002/S003 symptoms precede single S006 door; unresolved cause / spoiler exclusions |
| L1396 note 8 | EXPLICITLY_NOT_PLAYER_VISIBLE | S005 sunset memory transition; external connection separate |
| L1397 note 9 | EXPLICITLY_NOT_PLAYER_VISIBLE | S007 acute evidence / restriction / recovery; S008 reassessment / approval |
| L1398 note 10 | EXPLICITLY_NOT_PLAYER_VISIBLE | S008 role choice precedes helper assignment; no personal identity / actual age |
| L1399 note 11 | EXPLICITLY_NOT_PLAYER_VISIBLE | Cooperation qualitative; clinical judgement and safety abort remain |
| L1400 note 12 | EXPLICITLY_NOT_PLAYER_VISIBLE | Only profile order free in frozen scope; no extra observation / morality / alternate route |


## Presentation risk register and handoff

| Area | Question / acceptance intent | Status / later gate | Requirements |
| --- | --- | --- | --- |
| S001 ensemble, reused in S005 | Five people plus Rope clear, fifth chair occupied, family continues visibly without overcrowding? Compare BG/sprites/ensemble/hybrid | M10-03 COMPOSITION REVIEW | Kitchen / ensemble / cast |
| S003 text modes | Shared authoritative lifecycle keeps speaker focus, off-screen identity, explicit thought/channel Backlog and exact ReadHistory consume semantics? | M10-04 TECHNICAL SPIKE | Reading-mode treatment |
| Structured overlays S003–S008 | Hide, modal arbitration, Save/Load and deterministic checkpoint reconstruction clear stale UI without new persistence or fake Backlog entries? | M10-04 TECHNICAL SPIKE | Connection, status, assistance, care, records, approval, conditions, consent, schedule |
| S004 manual hold | Smallest scoped gate prevents Auto / Skip / queued advance from opening door; manual Advance alone releases? | M10-04 TECHNICAL SPIKE | Existing advance ownership; no asset/API invented |
| S006 white door / view disturbance | Single minimal wall-localized outline and view wobble at immediate music stop, then removal / fade; preference respected if movement used? | M10-04 TECHNICAL SPIKE / SPIKE CANDIDATE | White outline, disturbance, low vibration |
| S008 profile browsing | Equal overview, arbitrary inspection, Back / reviewed continuity, thoughts once, all-reviewed return; controls claim/release through existing gate with no persistence? | M10-04 TECHNICAL SPIKE | Overview / detail UI |
| Ambient / audio handoff | Can finite cue sequencing convey household life and equipment→rain handoff, or is minimal continuous support needed? No subsystem assumed | TECHNICAL_REVIEW_M10_03_OR_04 | Home bed, rain, connection / end cues |
| S002 authored music balance | Lower existing morning music behind life sound without restarting musical continuity, changing user volume, or adding saved gain fields; entry/load reconstruct correct intent? | M10-04 TECHNICAL REVIEW / SPIKE | Morning BGM role; current bridge lacks dedicated authored lowering |
| Static action staging | Shoes, paper handling, child hand / head touch, dog paw / stretch, coat and patch placement readable with fixed poses, prose and inserts? | M10-03 COMPOSITION REVIEW; unresolved mechanism to M10-04 | Cast / expressions / key props; no pose command |

No blocking narrative ambiguity was found. Deliberate unspecified details remain unspecified: S004 status has no authored new numeric fields; accident date has no literal calendar value; current-table glimpse has no inferred owner; S007/S008 have no new BGM requirement. The Hana immediate-thought issue is resolved by fidelity to exact source positions, not rewriting. White-door cause remains intentionally unresolved.

M10-02 completes player-facing intent and requirements only. M10-03 will compare actual assets and freeze IDs; M10-04 resolves technical spikes; M10-05/06/07 apply the three scene groups; M10-08 refines remaining P1/P2 production. No implementation, asset production or request has started. Source hash / full source diff, documentation cross-reference / coverage / field checks, exact three-path diff and whitespace review are recorded in the PR handoff. Entering 443 passed / 0 failed / 0 skipped remains prior evidence; no Unity test rerun is required for this documentation-only milestone.
