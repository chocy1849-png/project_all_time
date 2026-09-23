# Yarn Writing Guide

This is the current project guide for authoring Yarn with the verified Yarn Spinner 3.2.7 runtime. The established M1–M7 conventions below remain in force; the M8 stable-identity and MetaProgress rules extend them.

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

`M8_META_PROGRESS_START` and its M8 IDs are reserved technical/non-canon verification content. The production `VN_Main` Start Node remains `M2_UI_START`. M8 provides the persistence foundation; Gallery, Chapter Select, Archive, Achievement, Ending, and completion-percentage UIs, platform achievements, cloud sync, Meta reset, New Game, and Save Delete UIs remain future consumers.
