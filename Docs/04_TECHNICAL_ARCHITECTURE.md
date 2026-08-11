# Technical Architecture

## Current baseline

- Unity project root
- `Assets/_Project/`
- `Packages/`
- `ProjectSettings/`
- Universal Render Pipeline (URP) baseline
- Yarn Spinner 3.2.7 runtime through `dev.yarnspinner.unity`
- Official Dialogue System with built-in InMemoryVariableStorage for M1 smoke testing

## M1 temporary presentation boundary

- Official Line Presenter and Options Presenter are temporary smoke UI.
- No custom dialogue UI or custom presenter is implemented.
- M1 does not define persistent variable behavior.

## PLANNED — NOT IMPLEMENTED

Future domains:

- Dialogue
- Presentation
- Save
- Audio
- Tests

Custom VN presentation is deferred to M2.

Persistent variable integration is deferred to M5.
