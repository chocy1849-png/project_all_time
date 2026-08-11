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

## DEC-009 — M1 narrative runtime

Status: ACCEPTED

- Yarn Spinner 3.2.7 is the M1 narrative-runtime package.
- The OpenUPM package identifier is `dev.yarnspinner.unity`.
- The built-in InMemoryVariableStorage is used for M1 only.
- Persistent variable integration is deferred to M5.
- The official Line Presenter and Options Presenter are temporary M1 smoke UI.
- Custom VN presentation is deferred to M2.

## DEC-010 — M2 core VN play screen

Status: ACCEPTED

- The project-owned uGUI VN play screen is the M2 presentation shell.
- Yarn Spinner supplied LinePresenter and OptionsPresenter remain the runtime presenter implementations.
- The existing Yarn LineAdvancer is reused for click/Space hurry-then-advance behavior.
- The existing InputSystemUIInputModule is reused for UI navigation.
- A custom DialoguePresenterBase is not required in M2.
- Background, Character, and CG presentation are deferred to M3.
- Audio and transitions are deferred to M4.
- Persistence is deferred to M5.
- Convenience UX is deferred to M6.
- Settings are deferred to M7.

## DEC-011 — M3 presentation runtime contract

Status: ACCEPTED

- M3 presentation uses project-owned uGUI `Image` references; `SpriteRenderer` is not used.
- The fixed character slots are `far_left`, `left`, `center`, `right`, and `far_right`.
- Characters use one fixed pose assembled from layered `BackHair`, `Body`, and expression `Head` sprites. BackHair is optional; expressions swap only the Head sprite.
- `VNPresentationCatalog` and `VNCharacterDefinition` assets contain authored IDs and sprites; presentation state remains runtime-only.
- Speaker focus uses `LocalizedLine.CharacterName` aliases and Image tinting: active `1,1,1,1`, inactive `0.65,0.65,0.65,1`, over `0.20` seconds.
- M3 commands are immediate; no pose, transition, or fade commands are introduced. Animation and Live2D-style systems are out of scope.
- M3 artwork is temporary test artwork and is not production-final art.
- The M3 presentation smoke is technical, non-canon content used for manual observation of presentation states.
