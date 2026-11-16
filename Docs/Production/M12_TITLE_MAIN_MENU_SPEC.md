# M12 — Title and Main Menu Specification

This specification describes the implementation on `feat/m12-title-main-menu`. The user verified physical Escape opens the standalone menu; Editor Play verified menu close and return confirmation/reload. Standalone keyboard navigation and end-to-end failed-load recovery remain pending. See the [validation report](M12_TITLE_MAIN_MENU_VALIDATION_REPORT.md). User visual acceptance remains pending.

## Baseline and observed startup

Audited against `e6f8b7a964661e4da24dc2248a372d26f3f1d7a2`. The live Editor opened `VN_Main` additively for inspection. Its root objects are Main Camera, Dialogue System, VNCanvas, PresentationRuntime, AudioRuntime, SaveLoadRuntime, and VNConvenienceRuntime. The production DialogueRunner uses GameNarrative and InMemoryVariableStorage; its serialized `autoStart` is true and `startNode` is `M2_UI_START`. The production LinePresenter is `VNCanvas/DialogueLayer/LineContainer`; the Dialogue System contains a separate inactive template presenter, and Records contains an isolated replay presenter. The scene contains an EventSystem under Dialogue System.

## Intended composition

Keep one production scene and its existing services. A TitleLayer in the scene should be distinct from gameplay presentation and below the existing modal layer. Its separate background Image must accept an Inspector-assigned Sprite and preserve aspect ratio with cover cropping; a separate editable dark overlay keeps the title and controls independent of artwork. Use the M11 palette and Korean TMP font; the approved title is 《우리가 만났던 모든 시대》 (see `Docs/05_PROLOGUE_VERTICAL_SLICE.md`). Temporary background artwork is provisional and replaceable.

Disable the runner's serialized automatic startup via the Editor API; retain the technical start node for fixtures. Start `PROLOGUE_S001_START` explicitly after a guarded New Game action. A freshly loaded scene is the clean-session boundary for a repeated New Game or recovery after runtime mutation; never delete durable saves, Settings, or MetaProgress. Verify variable defaults, session backlog, presentation, audio, and play-time behaviour at this boundary.

## Dependencies and ownership

SaveLoadRuntime constructs its repository and coordinator in Awake. MetaProgress initializes at execution order -2; Settings and Records initialize at -1, with Records depending on MetaProgress. Check their `IsInitialized` results and diagnostics before enabling affected actions. Existing modal ownership is authoritative: Save/Load owns its input suppression; the convenience modal controller owns Settings, Records, and Backlog. The existing Cancel chain prioritizes overwrite confirmation, Save/Load, story interaction, convenience modal, and hidden UI restoration. Extend that chain rather than adding a competing Escape listener. Title ownership must block gameplay actions without preventing UI navigation or rebinding.

## Continue and load

Inspect all 12 manual, five auto, and one quick slots with the repository. Reject invalid timestamps, unreadable/unsupported data, and any candidate that fails the existing coordinator's complete compatibility preflight. Rank by saved UTC timestamp descending, then deterministically by slot type and index. Disable Continue when no candidate qualifies. Revalidate at execution time.

`VNSaveLoadController.Load()` reports success after validated execution starts, not after restoration. Its `StartLoad` awaits `ExecuteValidatedLoad`, publishes completion status on success, emits an operation failure on error, and clears `IsLoadInProgress` in `finally`. The implemented `LoadExecutionCompleted` signal distinguishes actual success from failure and prevents overlapping transitions. A failure after mutation requires clean-scene recovery; preflight failure must leave the title intact. Reuse the existing Load modal, coordinator, and repository.

## Return, exit, and outstanding validation

Return to title requires confirmation and a guarded scene reload after outstanding load operations finish. Exit requires confirmation and `Application.Quit` in Player; Editor validation must not claim the Editor exits. Reuse existing Settings and Records, including isolated Replay. Modal return focus, exact-line continuity, serialized references, layout and Sprite replacement, EditMode regression, connected Editor Play, and a mouse-driven Windows Player smoke are documented. Standalone keyboard/Escape, in-game-menu/return-to-title, and remaining negative save/load cases are still unverified; therefore technical acceptance is blocked and user visual approval is not claimed.

## Implemented title presentation and replacement procedure

`VNTitleScreen.prefab` is instantiated under `VNCanvas`, before the existing `ModalLayer`. It contains `TitleLayer/Background/BackgroundImage`, a separate `DarkOverlay`, title typography, six menu buttons, an in-game menu, and confirmations. The provisional 512×288 gradient was generated locally by the M12 installer; it is original, non-narrative artwork and is not final art. The prefab uses the M11 palette and the existing Korean TMP font. Buttons provide color-tinted normal, hover, pressed, selected, and disabled states.

To replace the artwork: open `Assets/_Project/Prefabs/UI/M12/VNTitleScreen.prefab`, select `TitleLayer/Background/BackgroundImage`, and assign the new Sprite in the Image component. `VNTitleBackgroundCover` uses the assigned Sprite's dimensions with an Envelope Parent aspect-ratio fitter; no path or title text is baked into the image. Adjust `TitleLayer/DarkOverlay` Image alpha independently for contrast. Save the prefab. The hierarchy and controller require no changes. A square M11 test Sprite was temporarily assigned and the prefab saved and reopened: the Image referenced the test Sprite, the cover ratio was 1, the title and all six menu controls remained separate, and the overlay remained present. The original provisional Sprite was restored.

## Runtime flow and limitations under validation

`VNTitleFlowController` coordinates the existing services. `VNInteractionGate.SetTitleFlowBlocksGameplay` blocks story/convenience actions while title or in-game menu owns the interaction; the router invokes the title's Cancel handler only after the established Cancel chain. Settings and Records continue to use `VNConvenienceModalController`; the existing Save/Load modal remains the owner of its transaction. Returning to title reloads the current production scene. New Game resets the play-time counter, disables Auto/Skip, and explicitly starts S001 from the clean scene. Title startup initializes the line presenter to the quiescent state required by the existing load barrier.

`VNContinueSelection` orders valid UTC timestamps descending; equal timestamps prefer Manual, then Auto, then Quick, and the lower slot index. The controller calls the existing full compatibility preflight before selecting and revalidates on load. `LoadExecutionCompleted` reports the awaited execution outcome after input release, distinct from the immediate accepted-start result. A failed title load returns to a clean scene with a diagnostic. See the validation report for completed checks, limitations, and the current blocked developer gate; user visual acceptance remains pending.
