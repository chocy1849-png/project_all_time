# Yarn Writing Guide

This is the current project guide for authoring Yarn with the verified Yarn Spinner 3.2.7 runtime. The established M1–M8 conventions below remain in force; M9 adds the Records and Replay authoring rules at the end.

## M1 authoring foundations

- Yarn files are plain `.yarn` text assets.
- All variables must be explicitly declared.
- Node IDs use stable uppercase snake case.
- Production nodes should use a section prefix, for example `PROLOGUE_001_START`.
- Technical test nodes use the `M1_` prefix.
- Variable names use lowercase snake case.
- Boolean story flags use `$flag_...`.
- Numeric relationship values may use `$rel_...`.
- Counters may use `$count_...`.
- Node IDs should be treated as persistent identifiers once Save/Load is introduced.
- M1 does not define production custom commands.
- M1 does not define persistence behavior.

## M3 presentation commands

- Presentation IDs use stable lowercase snake_case.
- `<<vn_bg background_id>>`
- `<<vn_show character_id expression_id slot_id>>` where slot ID is `far_left`, `left`, `center`, `right`, or `far_right`. Use `default` as an expression ID to select the character's default expression.
- `<<vn_expression character_id expression_id>>`, `<<vn_move character_id slot_id>>`, `<<vn_facing character_id left|right>>`, `<<vn_scale character_id scale>>`, and `<<vn_hide character_id>>` affect only visible characters. M3 has no pose command.
- `<<vn_cg cg_id>>` and `<<vn_clear_cg>>` set and clear the CG Image.
- Commands are immediate in M3; no pose, transition, or fade commands are defined. Animation and Live2D-style systems are out of scope.
- `M3_PRESENTATION_SMOKE` is technical non-canon content. Every visual-state command is followed by a manually advanced checkpoint line; timed waits are not used for M3 verification.

## M4 audio and transition commands

- BGM/SFX IDs use stable lowercase snake_case and are authored in `VNAudioCatalog`. Voice does not use this catalog.
- `<<bgm_play bgm_id>>` starts the resolved BGM immediately.
- `<<bgm_crossfade bgm_id duration>>`, `<<bgm_pause duration>>`, `<<bgm_resume duration>>`, and `<<bgm_stop duration>>` wait for their duration-based operation to complete before Yarn continues. Durations are finite seconds greater than or equal to zero.
- `<<sfx_play sfx_id>>` plays one SFX one-shot and does not wait for the clip to end.
- `<<vn_fade_to_black duration>>` and `<<vn_fade_from_black duration>>` wait for the screen fade.
- `<<vn_bg_crossfade background_id duration>>` waits for a background source-to-source crossfade.
- `<<vn_show_fade character_id expression_id slot_id duration>>` and `<<vn_hide_fade character_id duration>>` wait for character CanvasGroup fades. Slot IDs remain `far_left`, `left`, `center`, `right`, and `far_right`.
- `<<vn_cg_fade_in cg_id duration>>` and `<<vn_cg_fade_out duration>>` wait for CG CanvasGroup fades.
- Existing M3 immediate commands remain available and unchanged. Do not use or introduce `vn_pose`.
- Voice is optional per line. Reserve `#line:m4_voice_test` for the M4 technical voice line; its Korean built-in-localization Assets Folder AudioClip must be named exactly `m4_voice_test.wav` for Yarn Spinner's exact line-ID filename match. Lines without an associated voice asset are normal; an associated asset of the wrong type is an error.
- `M4_AUDIO_TRANSITION_SMOKE` is technical non-canon test content. The M4 Play Gate passed; no production story or audio content is established here.

## M5 checkpoint authoring

- `<<vn_checkpoint checkpoint_id>>` is the only way Yarn establishes a saveable checkpoint. `checkpoint_id` must exactly match a unique lowercase-snake-case entry in the Unity-authored `VNCheckpointCatalog`.
- Every production checkpoint must have a dedicated re-entry node whose first instruction is the matching `<<vn_checkpoint checkpoint_id>>` command. That node contains continuation only; it is safe to enter again after loading.
- Put all non-idempotent state changes before the jump to a checkpoint re-entry node. Do not award items, increment counters, play one-shot effects, or make one-time presentation/audio changes inside the re-entry continuation.
- Do not treat a currently displayed line, instruction position, or `Dialogue.CurrentNode` as save state. M5 resumes only through the catalog's exact dedicated `resumeNode`.
- Production checkpoint IDs and chapter IDs use lowercase snake_case. Resume-node names remain exact Yarn node names and are checked against the assigned Yarn Project during validation.
- M5 automatically requests one complete Auto save after each successful explicit checkpoint entry when the scene controller enables autosave. Do not add JSON/file commands to Yarn. A restored re-entry node's first matching checkpoint command is consumed once to avoid a duplicate Auto save; later genuine checkpoint entries remain eligible.
- A full save is unavailable during an active transition/fade. Yarn must not attempt to persist transition progress, source-A/source-B state, speaker focus, SFX, or voice; the backend restores only stable catalog-backed M3/M4 state.
- The M5 technical smoke (`M5_SAVE_LOAD_START`, `M5_CHECKPOINT_A`, and `M5_CHECKPOINT_B`) is non-canon. It validates the finalized checkpoint/node contract only and must not become the normal `VN_Main` start node.

## M6 convenience authoring

- Visible lines retain stable, unique explicit line IDs. Changing an ID changes its ReadHistory identity, including durable M8 history.
- A repeatedly executed authored line keeps the same stable ID; do not duplicate one explicit ID across separate authored source lines.
- Choices are never Auto-selected or Skip-selected.
- M6 originally kept ReadHistory session-only. M8 adds a persistent baseline with a session overlay; Backlog remains session-only.
- `M6_CONVENIENCE_SMOKE` and its voice/checkpoint fixtures are non-canon technical regression content.

## Stable line IDs for durable Read History

- Every localizable production line and option must have an explicit, unique `#line:<stable_id>` tag. Do not deliberately rely on an implicit compiler-generated ID for durable read history; implicit IDs are prohibited by this project's authoring contract.
- Once an authored ID is committed and used as durable identity, preserve it when visible text, speaker wording, node position, or source file changes. Copying a line to create a new authored line requires a new unique ID. Never reuse one ID for independent lines or options.
- Visible or localized dialogue text is never read-history identity. Runtime identity is Yarn's exact `LocalizedLine.TextID`. For example, author `Hello. #line:prologue_hello_01`; use the exact runtime `TextID` as delivered. Do not manually strip, add, or reconstruct a runtime prefix.
- Preserve existing IDs during script edits. The project compiler regression requires zero implicit localizable IDs and zero duplicate IDs; this project rule does not mean Yarn itself cannot generate implicit IDs.

## M8 MetaProgress commands

Use stable internal IDs, never localized display labels or descriptions:

- `<<vn_unlock_cg stable_id>>`
- `<<vn_unlock_chapter stable_id>>`
- `<<vn_unlock_archive stable_id>>`
- `<<vn_unlock_achievement stable_id>>`
- `<<vn_complete_ending stable_id>>`

Each command is idempotent. Reuse the same stable ID for the same content instead of inventing another ID for duplicate awards. These commands persist only their game-internal ID state; they do not persist display names or descriptions.

### CG unlocks

Displaying a CG does not permanently unlock it. Use `vn_cg` or another presentation command to show it. Use `vn_unlock_cg` only at the authored point where the permanent MetaProgress unlock should occur.

### Ending completion

Place `vn_complete_ending` only at the definitive ending-completion point. Do not mark an ending complete merely because its node begins.

### Achievements

`vn_unlock_achievement` records game-internal MetaProgress only. It does not integrate with Steam, Epic, or another platform achievement service.

### Read History

Authors do not call a Yarn command to mark dialogue read. The authorized line-consume runtime path records the exact `LocalizedLine.TextID`; full display or choice presentation alone does not. There is no `vn_mark_read` command.

## M8 technical smoke

`M8_META_PROGRESS_START` and its M8 IDs are reserved technical/non-canon verification content. The production `VN_Main` Start Node remains `M2_UI_START`. The M8 phase-boundary statement that Gallery, Archive, and Achievement UIs remained future consumers is superseded by M9 below; M8 remains the sole durable MetaProgress authority.

At the M9 boundary, the Records shell, Timeline, CG Gallery, Archive, Achievements UI, and isolated Replay v1 were implemented while production catalogs were intentionally empty. M10-10 later populated the production catalogs and authored two Replay excerpts; see the [M10-10 report](Production/M10_10_PRODUCTION_RECORDS_REPORT.md). Chapter Select as gameplay navigation, Ending Gallery, a general completion-percentage UI, platform achievements, cloud sync, Meta reset, and New Game / Save Delete UI where still unimplemented remain future work.

## M9 Records / Replay authoring

### Timeline catalogs

- Use stable lowercase ASCII snake_case IDs for Timeline chapter and entry IDs. Use the same M8 stable line identity for discovery and completion; do not derive identity from localized or display text.
- Author discovery and completion references as exact compiled Yarn line IDs. Every discovery, completion, and milestone line ID must exist in the assigned YarnProject.
- Include both the entry's discovery and completion line IDs in its milestone list. Milestones do not replace those identity references.
- A parent entry ID must resolve to an existing entry in the same chapter. Keep parent relationships acyclic.
- An optional Replay node must be an exact node name in the compiled YarnProject.
- An optional related CG must resolve to an entry in the CG Gallery catalog; Gallery artwork still resolves through VNPresentationCatalog.
- Do not expect a Timeline entry to appear before its authored discovery line has been recorded as read. A read completion line makes it Completed; discovery/completion identity is never inferred from display text.

### Replay v1 content

Treat Replay as isolated visual/history playback, not another production story path. It has its own DialogueRunner, fresh variable storage, presenter, and presentation hierarchy. It does not read or mutate production variables, presentation state, Backlog, Read History, SaveData, or MetaProgress.

Replay v1 permits only these immediate visual commands:

- `<<vn_bg background_id>>`
- `<<vn_show character_id expression_id slot_id>>`
- `<<vn_expression character_id expression_id>>`
- `<<vn_move character_id slot_id>>`
- `<<vn_facing character_id left|right>>`
- `<<vn_scale character_id scale>>`
- `<<vn_hide character_id>>`
- `<<vn_cg cg_id>>`
- `<<vn_clear_cg>>`

Replay content must not depend on:

- Interactive options or branching Replay gameplay.
- Checkpoint commands, MetaProgress commands, or save/load mutation.
- BGM, SFX, Voice, or transition commands.
- Production Backlog or Read History side effects.
- Unsupported Yarn instructions or commands, backward/invalid control flow, or any reachable node outside the explicit compiled-node approval policy.

The content validator checks the compiled root and its reachable node closure and fails closed. The production controller currently approves the catalog Replay root for an entry; any reachable node jump outside the approval is rejected. Do not assume arbitrary Yarn content is Replay-safe. Replay eligibility is rechecked at start and requires a visible Completed Timeline entry, an authored Replay node, and successful validation.

### Technical content and future content

M9 technical Replay fixtures are non-canon. Keep production story start at `M2_UI_START`; do not change it to an M9 smoke or Replay node.

At the M9 milestone boundary, the Records framework was complete while production catalogs were intentionally empty. M10-10 later populated the current Timeline, Gallery, Archive and Achievement catalogs and authored Replay excerpts; the [M10-14 closure report](Production/M10_14_PROLOGUE_MILESTONE_CLOSURE_REPORT.md) records their current counts and validation history. Chapter Select as gameplay navigation, Ending Gallery, a general completion-percentage UI, platform achievements, cloud sync, Meta reset, and New Game / Save Delete UI where still unimplemented remain future work. Re-run relevant human UX checks if those separate future systems are added.

## M10-05 production facts and black opening

- `<<vn_clear_bg>>` clears the existing presentation owner's background without hiding dialogue. It is registered by the production `VNYarnPresentationCommands` and removed on disable; Replay v1's allowlist is unchanged.
- `<<vn_info_show "title" "first fact" "second fact" "third fact">>` replaces the scene-session passive information panel. The first fact must be nonblank; empty second/third facts are omitted, otherwise joined with newlines. Use exact authored facts only. All four quoted arguments are required.
- `<<vn_info_hide>>` clears that panel. It is not dialogue, a narrator, history or persisted state.
- `VNYarnInformationCommands` registers only on its configured production runner. Missing wiring and blank facts log a diagnostic. Disable unregisters and hides; load invalidates stale operations. Re-entry waits until loading ends and honors cancellation/disable. Do not expose these facts commands to isolated Replay.
- M10 backgrounds use awaited `<<vn_bg_crossfade background_id 0>>` for stable cuts after normalized clears. A full-screen black fade covers text too; use the cleared black background for the opening child dialogue.
- S001–S003 are 111 canonical paragraphs under 33 beat nodes plus PROLOGUE_S001_START. Source physical line wrapping becomes spaces within each retained paragraph. The S003 boundary links directly to S004 without a visible completion or full-prologue unlock.
- The controlled MCP entry and temporary-media/asset boundaries are documented in [M10-05 report](Production/M10_05_S001_S003_PLAYABLE_REPORT.md). Start Node remains M2_UI_START. S002 steady restrained BGM is the request's provisional fallback; final deviation acceptance remains tracked.

## M10-06 manual hold and localized effect

- `<<vn_manual_hold>>` arms the scene-session manual-only hold for the **next authored line**. Place it immediately before the line that must remain visible until explicit Manual advance. It does not change Auto/Skip state or add a timer. Hurry retains the hold; accepted full manual consume releases it. Load, dialogue completion and adapter disable clear stale state.
- `<<vn_local_effect on 0.2>>` and `<<vn_local_effect off 0.2>>` are awaited transitions on the existing localized effect group. Use `on` or `off` and a finite non-negative duration. The visible interval blocks stable Save capture; Load and stop clear the transient effect. These production commands are not part of the isolated Replay allowlist.
- S004-P07 arms the hold on its last spoken line; P08 is the authored silent boundary and P09 is Jinhee opening the door. S005 assistance is disclosed before the first adjusted response and terminates before her own return choice. S006 hides `rope_memory` before revealing the separate actual `rope` definition. See [M10-06 report](Production/M10_06_S004_S006_PLAYABLE_REPORT.md) for cues and temporary limits.

## M10-07 profiles, diagram and ending

- `vn_profile_begin` starts the transient browser; `vn_profile_wait` yields until one browser event, then returns to Yarn. `vn_profile_result()` routes first Sunju/Haejin reviews to their original ordinary Yarn lines and Hana to the silent P06 beat. After the line is consumed, `vn_profile_review_complete` acknowledges its index; `vn_profile_silent_complete` acknowledges Hana. Continue is available only after all three are reviewed and returns to P07. Do not block a runner command while waiting for that same runner to present a thought or add profile variables to saves.
- `vn_medical_diagram_show` and `vn_medical_diagram_hide` control a transient two-path scene insert; `vn_part_end_title` shows the component-owned title over black. Load and disable clear the panels; a new dialogue clears the previous title. These commands are production-only, not Replay commands.
- The ending uses the existing finite SFX owner: equipment/rain handoff, one separate awning drop, and one bus brake. Clear the staged background and characters under the opaque fade before revealing the black background for the final narration; the full-screen fade would otherwise cover the dialogue. The current clips are silent technical placeholders. See [M10-07 report](Production/M10_07_S007_S008_PLAYABLE_REPORT.md).

## M10-09 production line entries

The M5 checkpoint rules above remain the legacy technical-v1 contract. Production Manual/Quick/Auto saves use v2 exact-line entries instead. Each production paragraph retains its original single `#line:` tag in a `PROLOGUE_LINE_Sxxx_nnn` node; normal flow reaches that node after prerequisite commands. Do not move a pre-line SFX, variable mutation, transition, or BGM command into its line node. After changing production Yarn, run `python AgentScripts/m10_09_build_resume_graph.py` from the repository root and review the generated `Assets/_Project/Resources/M10_09_LINE_RESUME_MANIFEST.json`; the tool is idempotent and records each line, originating beat, continuation, eligibility and authored passive panel state. The program and manifest fingerprint deliberately invalidates incompatible v2 saves when the resume graph changes.

Do not mark the S004 manual hold, visible S006 white door, S008 browser review thoughts or ending one-shot narration as ordinarily saveable. Information panels and the medical diagram are reconstructed from their authored manifest context by the existing scene components, not inserted into Backlog. Keep the profile browser's three-review gate and the existing `M2_UI_START` technical scene start. Yarn `wait` remains scaled-time but is tracked so a stopped dialogue cannot continue into a newly loaded one.
