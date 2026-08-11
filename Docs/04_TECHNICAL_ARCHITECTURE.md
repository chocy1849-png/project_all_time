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
