# Technical Architecture

## Current baseline

- Unity project root
- `Assets/_Project/`
- `Packages/`
- `ProjectSettings/`
- Universal Render Pipeline (URP) baseline
- Yarn Spinner 3.2.7 runtime through `dev.yarnspinner.unity`
- Official Dialogue System with built-in InMemoryVariableStorage for M1 smoke testing

## M2 core VN play screen

- Project-owned uGUI objects and prefabs provide the VN play-screen layout.
- Yarn Spinner supplied LinePresenter and OptionsPresenter remain the runtime presenter implementations.
- The existing Yarn LineAdvancer provides click/Space hurry-then-advance behavior.
- The existing InputSystemUIInputModule provides UI navigation.
- A custom DialoguePresenterBase is not required.
- Background, Character, and CG presentation are delivered by the M3 project-owned uGUI runtime.
- Audio and transitions are deferred to M4.
- Persistence is deferred to M5.
- Convenience UX is deferred to M6.
- Settings are deferred to M7.

## PLANNED — NOT IMPLEMENTED

Future domains:

- Dialogue
- Save
- Audio
- Tests

Persistent variable integration is deferred to M5.

## M3 presentation runtime contract

- `VNCharacterDefinition` stores a stable character ID, speaker aliases, default facing and scale, optional BackHair, Body, and expression Head sprites. Characters use a fixed pose; expressions swap only the Head sprite.
- `VNPresentationCatalog` resolves unique character, background, CG, and speaker-alias IDs. Invalid or duplicate IDs invalidate lookup, so commands fail without changing presentation state.
- `VNPresentationController` owns runtime-only character state and drives a background Image, CG Image, and one `VNCharacterSlotView` per fixed character slot. Each view owns a shared visual root plus BackHair, Body, and Head Images.
- `VNYarnPresentationCommands` registers `vn_bg`, `vn_show`, `vn_expression`, `vn_move`, `vn_facing`, `vn_scale`, `vn_hide`, `vn_cg`, and `vn_clear_cg` with Dialogue Runner. M3 has no pose command.
- `VNSpeakerFocusPresenter` observes `LocalizedLine.CharacterName` and returns immediately without rendering text or handling options.
- The M3 technical smoke is non-canon and uses manual dialogue checkpoints to expose presentation changes for Play Gate verification.
- Character entrance/exit animation, movement tweening, background/CG transitions, screen effects, audio, persistence, and final artwork/polish are out of scope for M3.

## M4 audio / voice / transition runtime contract

- `VNAudioCatalog` is a ScriptableObject lookup for unique lowercase-snake-case BGM and SFX IDs. Invalid or duplicate entries invalidate runtime lookup so unknown IDs fail before audio state changes. Voice clips are intentionally excluded.
- `VNAudioController` owns two BGM AudioSource references and one SFX AudioSource reference. It crossfades BGM source-to-source, pauses with `Pause`, resumes with `UnPause`, fade-stops to a reusable source state, and plays SFX through `PlayOneShot`.
- `VNYarnAudioCommands` registers `bgm_play`, `bgm_crossfade`, `bgm_pause`, `bgm_resume`, `bgm_stop`, and `sfx_play`. All duration operations return `IEnumerator` and are awaited by Yarn Spinner 3.2.7.
- Voice assets are optional per Yarn line. `VNOptionalVoicePresenter` is the sole Dialogue Runner voice presenter; it returns immediately for a null `LocalizedLine.Asset`, reports a project-owned error for a non-AudioClip asset, and delegates valid AudioClip playback plus dialogue lifecycle callbacks to the Unity-wired Yarn Spinner 3.2.7 `VoiceOverPresenter`. The delegated presenter uses the assigned Voice AudioSource and retains `endLineWhenVoiceoverComplete = false` for normal manual advancement.
- `VNTransitionController` uses the M3 `VNPresentationController` as the authoritative background, character, and CG runtime state. It requires Unity-wired CanvasGroups for the screen black overlay, current and incoming backgrounds, CG, and each M3 CharacterVisualRoot.
- `VNYarnTransitionCommands` registers `vn_fade_to_black`, `vn_fade_from_black`, `vn_bg_crossfade`, `vn_show_fade`, `vn_hide_fade`, `vn_cg_fade_in`, and `vn_cg_fade_out` as awaited `IEnumerator` commands. Existing M3 immediate commands remain separate and unchanged.
- Deferred beyond M4: production voice acting, final audio mastering, advanced voice scheduling, lipsync, white flash, blur, screen shake, advanced transition presets, persistent audio/transition state, and settings UI or per-category user volume controls.
