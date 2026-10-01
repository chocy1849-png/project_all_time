# Prologue Vertical Slice

Status: DEFINED / CANON SOURCE IMPORTED / PRESENTATION REQUIREMENTS MAPPED
Story Canon: READY
Presentation Mapping: READY
Master Asset Requirements: READY
Asset Inventory / Stable-ID Freeze: PENDING M10-03
Technical Presentation Spikes: PENDING M10-04
Yarn Implementation: NOT STARTED
Implementation Ready: NO — story authority is ready; production implementation prerequisites remain pending.

## Story authority and source fidelity

The sole M10 prologue narrative authority is [ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt](Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt): 《우리가 만났던 모든 시대》, 1부 — 깨우는 사람.

- Source revision: **비주얼 노벨 공통 루트 대본 · 제1고**.
- Imported repository path: `Docs/Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt`.
- Original authoring path: `C:\Users\UserK\Downloads\message.txt`.
- Import date: 2026-10-01 (Asia/Seoul); M10-01 base: `4ddbdafe8ba7e56af6e18cfbdac8feab55802056`, including merged PR #46.
- Original authoring-source / imported working-copy SHA-256: `8413E847DD66D7D45585847A935B9C384D91FACFE9724EE27084F3D86FF977E9`.

The source is a preservation copy, copied rather than moved. Its initial repository working copy is byte-for-byte identical to the 39,939-byte external source; the Downloads original remains intact. Existing Git `text=auto` filtering normalizes its 1,400 CRLF line endings to LF in the repository blob. The SHA-256 above identifies the original authoring bytes, not the normalized blob; decoded text is identical after accounting only for those line endings. No `.gitattributes` change is required.

Treat this revision as frozen human-authored source. Future Yarn conversion or presentation work must not rewrite it, normalize names or punctuation, translate it, reorder scenes, or insert metadata, commands, or stable IDs into it. Any narrative revision requires a separate, explicitly approved source revision and updated authority record. This document summarizes scope; the source supplies the exact wording and full directions, including its non-player-visible continuity notes. The `.txt` file lives under `Docs/Story/`, outside Unity `Assets`; it is not runtime Yarn or a Unity TextAsset and receives no manually authored `.meta`.

## Canonical scope

M10 covers **S001–S008 inclusive**. It begins at **S001. 다섯 사람이 먹는 아침** and ends with S008's connection sequence and **1부 — 깨우는 사람 / 끝**, immediately before the next era/story sequence begins. Later route content is excluded.

| Scene | Canonical title |
| --- | --- |
| S001 | 다섯 사람이 먹는 아침 |
| S002 | 돌아온 다음의 일이 없는 여행 |
| S003 | 강아지가 부르는 이름 |
| S004 | 새 신발이 있는 방 |
| S005 | 성공한 작별 |
| S006 | 흰 문, 닫힌 진료 |
| S007 | 삶을 묻는 검사 |
| S008 | 가장 편할 것 같은 여자 |

The authored cast is **지훈, 진희, 민석, 나래, 준호, 로프, 이도경, 은정**. Preserve these names; do not restore earlier placeholder names. S008's 선주, 해진, 하나 are authored role-profile information and do not add alternate companion routes to this scope. The source distinguishes role settings from the later assignment and private identity of a human helper.

## Text semantics and textbox boundaries

| Source role | Canonical meaning |
| --- | --- |
| `[제작 지시]` / square-bracket directions | Production directions, not player-visible dialogue. Do not automatically display, speak, or narrate them |
| `서술` | Player-visible non-voice narration; story content |
| `지훈·독백` | Jihun's internal monologue, distinct from spoken `지훈` dialogue |
| `로프·내부` / `지훈·내부` | Private connection exchange heard by Jihun, distinct from spoken dialogue, monologue, and later publicly spoken `로프` |
| Normal named dialogue | Spoken dialogue using the exact authored speaker identity; preserve off-screen qualifiers where authored |
| Authored information blocks | Player-facing information at its directed reveal, not automatically character speech. Presentation mapping is ready in the linked M10-02 plan |

Each authored dialogue paragraph maps conceptually to a separate textbox. Long narration may later be split by sentence for readability, preserving wording, order, and meaning. Dialogue paragraphs must not be arbitrarily rewritten to fit UI. Production/continuity commentary explaining a character's thoughts is not narration and must not be read aloud. No Yarn line IDs are assigned in M10-01; later runtime adaptation must retain these semantic distinctions while following the [Yarn Writing Guide](06_YARN_WRITING_GUIDE.md).

## Narrative presentation invariants

### Ordinary opening and family tone

S001 starts as an ordinary family morning: black screen, no music, household frying/plate sounds, then the authored kitchen reveal and later morning theme. There are five people at the table and no empty chair. Before S003, labels/UI must not reveal deep consciousness, a patient, or a therapist session. Rope initially appears to be an ordinary small white dog; its actual cyber-dog form is first revealed at the authored S006 beat.

Family affection must not become horror. Unease comes from ordinary life that has stopped progressing, not monstrous or threatening family imagery. S002's contradictions remain restrained with music continuity; S003 does not distort Minseok's face into a threat. Preserve the source's sensory cues and exclusions rather than adding hallucinations or suspense effects.

### Common-route choices and profile review

S001–S008 has **no branching narrative choice**. Do not add a choice to tell 진희 the truth immediately, force her return, choose an alternate partner route, or select another profile only to silently receive Hana. The former technical objective to validate one choice does not authorize a story choice; existing technical fixtures remain non-canon.

The only review-order variation authorized in this M10 scope is S008 profile browsing. The canonical result remains **`하나로 하겠습니다.`**, motivated by **`할 일이 명확하네요.`** Three profiles begin with equal visual/story weight; Hana receives no pre-selection destiny or memory effect. Profile review is not a three-route romance choice. Exact Profile UI mechanics, navigation, visit tracking, and confirmation design are **not frozen here**; they belong to M10-02/M10-04/M10-07. The source's broader production note about possible observation-order expansion does not itself authorize additional interaction in this frozen scope.

### 진희's agency and family projection

진희 opens the room herself. 지훈 neither forces the door nor abruptly announces the family's death as a progression trigger. Preserve the S004 direction that the long silence does not auto-advance: the handle moves when the player advances to the next sentence. How to enforce that direction is later presentation mapping, not a new global Auto/Skip policy in M10-01.

The family are not independent post-death consciousnesses. Projection assistance has a visible beginning and ending, stays within the authored treatment scope and existing-reaction range, and preserves intervention records. It does not invent the actual family's intentions or guarantee recovery. After assistance ends, 진희's own response determines return; do not turn that decision into a player morality choice.

The family remain visibly/presentationally present through S005's farewell. Do not dissolve them, replace them with empty chairs, or make them vanish as a horror effect. The sunset table is a memory-scene transition, not a newly introduced time-acceleration rule. Preserve S004's music absence, S005's late low melody after the meal has sufficiently progressed, and the absence of a completion chime.

### White-door spoiler boundary

The white door appears only at the authored S006 beat before return; 진희 sees it as well. Jihun's sensory confusion has already begun during ordinary family life. Preserve the low vibration, brief localized overlap, view disturbance, immediate music stop, disappearance of the outline, and authored exit fade. The cause remains unresolved in M10. Add no stars, spacecraft, wife/daughter imagery, explicit cosmology, explanatory whisper, strong horror/glitch treatment, or definitive causal explanation.

### 지훈 characterization and medical continuity

지훈 is competent and procedure-oriented in patient care while emotionally depleted in his personal life. Do not add taking 진희's hand, emotional collapse, philosophical narration about family love, or extra compassionate monologue absent from the source.

The care system functions; staff incompetence must not be invented to manufacture later drama. Keep acute-symptom assessment, record analysis, work restriction, recovery period, reassessment, and immersion approval distinct. 각성고갈 findings do not resolve the unknown headache/sensory-path cause. Increased examination frequency and stronger immersion stimulation are separate matters. S007 preserves the authored simple two-path diagram rather than adding medical lore or elaborate neural imagery.

### S008 preparation and endpoint

Preserve reassessment before approval, anonymous helper participation, the distinction between a role profile and actual helper identity, and the authored consent/information-protection checks. Progress estimates concern trust/cooperation and treatment response, not literal mind-reading or compelled romance. Normal completion and safety interruption remain distinct; return does not require the companion's approval.

The connection sequence fades out and gradually replaces equipment connection sound with rain, followed by the authored bus-brake sound and end title. Do not add a woman's whisper or suggestive memory imagery. This endpoint does not begin the subsequent era content.

## Research relationship and project contracts

Authority order is **Story Source > Project Architecture > Verified Reference Evidence > General Convention**. The [M10-00 Local VN Reference Audit](Research/M10_LOCAL_VN_REFERENCE_AUDIT.md) is secondary research input and cannot override the source. Local reference-game visual/audio evidence remains incomplete: no game screen or audio was successfully observed. Its unverified findings and candidate timings/styles must not be promoted to canon or treated as measured reference behavior. `USER_CAPTURE_NEEDED` C1–C7 are optional later evidence and do not block this story canon freeze; M10-01 does not attempt those captures. M10-02 may refine presentation recommendations against this source and current contracts.

Preserve the current [Technical Architecture](04_TECHNICAL_ARCHITECTURE.md) and [Yarn Writing Guide](06_YARN_WRITING_GUIDE.md): Yarn Spinner 3.2.7, uGUI, production Start Node `M2_UI_START`, checkpoint-based Save/Load, existing Backlog/Auto/Skip and input ownership, M8 MetaProgress as sole durable meta authority, M9 Records, and isolated Replay. Canon is authoritative for narrative content; this hierarchy does not authorize a technical contract change during M10-01.

Consistency review covered `Docs/00_PROJECT_BRIEF.md`, `Docs/01_CONCEPT_DRAFT.md`, this document, `Docs/06_YARN_WRITING_GUIDE.md`, and the reference audit against the source. No material conflicting story decision was found. Older high-level/draft placeholders are not M10 narrative authority; omissions do not authorize extra content. This document replaces its obsolete undefined/one-choice slice placeholder; unrelated documents remain unchanged.

## Milestone boundary and validation

M10-01 established story authority. M10-02 completes the [S001–S008 Presentation Map](Production/M10_PROLOGUE_PRESENTATION_MAP.md) and [Master Asset Gap List / Requirements](Production/M10_MASTER_ASSET_GAP_LIST.md). Asset requirements are mapped, but actual repository inventory has not been compared; inventory comparison and stable-ID freeze await M10-03. Technical presentation spikes await M10-04. No production BGM/SFX/BG/character/expression/CG/overlay IDs or user asset requests are created. All final M10 BGM remains new-production work; previous Suno Free-workflow tracks are reference-only. Yarn and presentation implementation have not started; Implementation Ready remains NO.

M10-02 changes only this readiness/state record and the two linked planning documents. The preserved canonical story source remains unchanged. No C#, tests, Unity Scene/Prefab/ScriptableObject, Yarn, catalogs, assets, packages, ProjectSettings, input actions, configuration, or persistence schema changes are authorized. Unity is not launched for this documentation task. The entering-M10 443 passed / 0 failed / 0 skipped baseline remains prior technical evidence, not a new test run.

M10-01 import validation (historical): external hash matched the expected M10-00 fingerprint; the copied working-tree file matched byte-for-byte; Git's normalized blob differed only in CRLF-to-LF representation; full two-file diff and whitespace checks passed with the exact two-path allowlist.

M10-02 readiness validation: 103 presentation beats cover S001–S008; all 59 bracketed scene directions are mapped, with all 385 player-content blocks accounted for and production/continuity notes explicitly not player-visible. All 84 requirement rows have presentation references; every existing-asset match remains pending M10-03. Source fingerprint remains unchanged. Full three-document diff, cross-references, field coverage and `git diff --check` are reviewed before commit; commit/PR identities and final worktree status are recorded in the review handoff.
