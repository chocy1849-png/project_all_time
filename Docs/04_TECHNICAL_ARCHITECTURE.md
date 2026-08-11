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
- Background, Character, and CG presentation are deferred to M3.
- Audio and transitions are deferred to M4.
- Persistence is deferred to M5.
- Convenience UX is deferred to M6.
- Settings are deferred to M7.

## PLANNED — NOT IMPLEMENTED

Future domains:

- Dialogue
- Presentation
- Save
- Audio
- Tests

Persistent variable integration is deferred to M5.
