# Decision Log

## DEC-001 — Unity baseline

Status: ACCEPTED

- Latest installed Unity 6.3 LTS patch in the 6000.3.x line
- Alpha and beta editors prohibited

## DEC-002 — Project layout

Status: ACCEPTED

- Repository root equals Unity project root

## DEC-003 — Rendering baseline

Status: ACCEPTED

- The project uses the Universal Render Pipeline (URP).
- The current verified package baseline is `com.unity.render-pipelines.universal` 17.0.3.
- Existing Unity-generated URP assets and ProjectSettings are canonical.
- Do not migrate rendering pipelines during Phase 0.
- Any future rendering-pipeline migration requires a separate accepted decision.

## DEC-004 — Target platform

Status: ACCEPTED

- Windows PC first
- 1920×1080 reference target

## DEC-005 — Source control

Status: ACCEPTED

- GitHub and Git
- Visible Meta Files
- Force Text
- Package lock tracked

## DEC-006 — Narrative tooling

Status: ACCEPTED

- Yarn Spinner planned
- Not installed in Phase 0

## DEC-007 — Content authority

Status: ACCEPTED

- Story is draft
- No story canon is established in Phase 0

## DEC-008 — Phase 0 scope

Status: ACCEPTED

- No gameplay implementation
