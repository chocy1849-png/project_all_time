# M1 Yarn Writing Guide

This guide records only the M1 technical rules established with Yarn Spinner 3.2.7.

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
