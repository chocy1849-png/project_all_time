# M12 Title and Main Menu — Final Validation Report

> **M12 TITLE & MAIN MENU — READY FOR USER PLAY GATE.** Developer-side validation is complete. User Visual Acceptance remains pending. The historical run notes below are retained as-is; their earlier outstanding-state summaries are superseded by this final snapshot.

## Delivery snapshot — 2026-11-11

- Repository: `C:\Dev\project_all_time`
- Branch: `feat/m12-title-main-menu`
- Base: `e6f8b7a964661e4da24dc2248a372d26f3f1d7a2` (merged M11 / current `main` base)
- Source under validation: `217af8266efeb9456c817648e636f1577f4832e4`
- Final evidence and current-source smoke record: committed on `feat/m12-title-main-menu`; the exact final SHA is the PR head and is included in the delivery response.
- PR: [#66 — Complete title screen and main menu experience](https://github.com/chocy1849-png/project_all_time/pull/66), open against `main`, mergeable, not merged.

## Regression and Editor evidence

- Full unfiltered EditMode suite: **541 passed, 0 failed, 0 skipped, 0 inconclusive**. The run was completed before this evidence-only update; result artifact: `%TEMP%\M12-final-editmode-results.xml`.
- Focused M12 suite: **11/11 passed**. No test was rerun during this delivery pass because production code did not change.
- The asynchronous Title recovery integration test passed. It loads the production scene under isolated Settings, SaveData, and MetaProgress roots, starts an accepted asynchronous restore, induces a controlled presentation-restore failure, observes `LoadExecutionCompleted`, exercises the real Title controller's scene-recovery path, verifies Title/message/loading/modal/input recovery, and starts New Game again.
- Connected Unity Editor Play validation and the M12 test runner reported completion. Earlier physical keyboard evidence is retained below and explicitly attributed to the user and the earlier Player build.

## Final-source Windows Player build and smoke status

- Current-source Windows x64 build: `C:\Users\UserK\AppData\Local\Temp\M12FinalPlayerCurrent\M12.exe`
- Source commit: `217af8266efeb9456c817648e636f1577f4832e`
- Unity: `6000.3.21f1`; build result: **Succeeded, 0 errors, 486 warnings**, 353,033,744 bytes reported by Unity. Executable SHA-256: `48EFAB523AA684C653BD1254A6962D3410127B5C02DC1310F6F16F4810666556`.
- This build uses the isolated Player identity `M12Validation20261111 / AllTime_M12_QA`, with data under `%USERPROFILE%\AppData\LocalLow\M12Validation20261111\AllTime_M12_QA`; the target was absent before launch. It does not open or mutate the normal `DefaultCompany\My project` SaveData or Settings.
- The current-source Player launched, initialized Unity 6000.3.21f1 and the Input System, and remained responsive. Its log records `VNSettingsRuntimeBootstrap: Display: Primary display native dimensions are invalid.` during first-run display initialization; no fatal exception was reported. The user subsequently performed the current-source UI smoke listed below and reported every step passing. This smoke confirms Settings opens and closes; it does not claim testing a display-mode change.
- The requested `M12FinalPlayer` output path was occupied by an older running Player, so the current-source build used the distinct `M12FinalPlayerCurrent` path above. Its isolated storage identity remains `M12Validation20261111 / AllTime_M12_QA`.

### Current-source standalone Player interaction smoke

**PASS — user-performed physical/UI verification**, reported after running the build at `C:\Users\UserK\AppData\Local\Temp\M12FinalPlayerCurrent\M12.exe`:

1. Title displayed and New Game entered S001.
2. A Quick save was created; Escape opened the in-game menu; Continue resumed gameplay.
3. Return-to-Title cancellation restored the in-game menu; confirmation returned to Title and Continue was available.
4. Continue restored the saved line; Load opened and restored the Quick save.
5. Settings and Records opened from Title and returned correctly.
6. Quit cancellation returned to Title; confirming Quit exited the Player.

This result is user-reported physical/UI evidence for the current-source build. It is separate from Codex's Unity Editor screenshots and does not by itself constitute User Visual Acceptance.

## UI-state screenshot matrix

The 36 PNGs in [`M12_Evidence/FinalMatrix`](M12_Evidence/FinalMatrix) were captured from the actual production `VN_Main` scene in Unity Editor Play Mode with Unity Pipeline Game View capture, not fabricated or synthesized. The Game View was set to each required size before each capture. Every PNG header was read back and matched its filename dimensions exactly.

| Required state | 1280×720 | 1600×900 | 1920×1080 |
|---|---|---|---|
| Initial Title | Captured | Captured | Captured |
| Focused menu item | Captured | Captured | Captured |
| Continue disabled | Captured | Captured | Captured |
| Continue enabled | Captured | Captured | Captured |
| Load | Captured | Captured | Captured |
| Settings | Captured | Captured | Captured |
| Records | Captured | Captured | Captured |
| Quit confirmation | Captured | Captured | Captured |
| In-game menu | Captured | Captured | Captured |
| Return confirmation | Captured | Captured | Captured |
| S001 after New Game | Captured | Captured | Captured |
| Restored gameplay after Continue | Captured | Captured | Captured |

Visual review of the matrix found the title and six menu actions readable, selected state visible, title content within the viewport, and no button overlap or clipping at all three sizes. The 1280×720 in-game menu, 1920×1080 return confirmation, and 1600×900 restored dialogue were inspected individually; contact sheets for all three sizes were inspected as well. Load/Settings/Records use the existing M7/M9 screens; their full visual redesign remains deferred to M13. Capture metadata and isolated test-save details are in `FinalMatrix/README.md`.

The screenshot run used the isolated temporary Editor data root `%TEMP%\M12_final_matrix_590a04a680dd4a6d81f08f870220383e`. The temporary capture imports were copied into `Docs/Production/M12_Evidence/FinalMatrix` and removed from `Assets`; no runtime source or project settings were changed by this evidence pass.

## Original M12 acceptance checklist

| Acceptance area | Result | Evidence / note |
|---|---|---|
| Existing branch/base preserved; standard-user Unity Editor connected | Pass | Branch remains `feat/m12-title-main-menu`; base is M11 merge. User resolved the Unity administrator warning. |
| Architecture audit and production-scene wiring | Pass | M12 specification and production `VN_Main` integration; one authoritative dialogue/save/settings/records runtime composition. |
| Normal launch enters Title; no automatic `M2_UI_START` | Pass in Editor/test coverage | Production runner automatic start disabled; New Game explicitly requests `PROLOGUE_S001_START`. Current-source Player's rendered Title still needs manual confirmation. |
| Six Korean actions, M11 styling, replaceable Sprite and overlay | Pass in implementation and Editor capture | Sprite is serialized independently from text/menu. Screenshot matrix shows Title states. User visual approval remains pending. |
| Responsive UI, keyboard focus, disabled states and modal ownership | Pass in Editor/test coverage; current-source Player smoke reported pass | 36 actual captures at required dimensions; focused/disabled states included. User reports current-source navigation and modal flows passed. |
| New Game freshness and S001 entry | Pass in Editor/test coverage | Recovery integration returns to Title then starts S001; standalone current-source confirmation pending. |
| Continue candidate selection, compatibility, async completion and recovery | Pass in tests/Editor integration | Latest compatible candidate policy, actual completion event, controlled failure recovery and exact-line integration covered. |
| Load Game reuse, invalid-save handling, Settings and Records integration | Pass in tests/Editor integration | Existing services and modal ownership retained; screenshot states captured. Current-source Player interactions pending. |
| In-game menu, Escape routing, return confirmation and repeat cycle | Pass in Editor/test coverage and user-performed current-source Player smoke | User reports Escape, Continue, return cancellation, and confirmed return to Title passed. |
| Quit confirmation and application exit | Editor/test coverage; current Player not yet exercised | Current-source Player must complete the manual exit step above. |
| Protected story, Yarn, SaveData, MetaProgress, Replay and M11 contracts | Pass | Protected asset review and 541-test regression recorded in prior run. |
| Focused M12 and full EditMode regression | Pass | 11/11 focused; 541/541 full. Not rerun without code changes. |
| Current-source Windows x64 build | Pass | Build succeeded with 0 errors; 486 warnings are recorded above. |
| Current-source standalone interaction smoke | **Pass — user-performed** | User reports all six current-source Player smoke steps passed. |
| User visual/gameplay acceptance | **Pending user approval** | Not inferred from technical screenshots. |
| Final PR | **Open, not merged** | [PR #66](https://github.com/chocy1849-png/project_all_time/pull/66) targets `main`. |

## M13 deferrals

The complete visual redesign of Settings, Save/Load, Records, and Backlog remains deferred to M13. M12 only integrates their existing interfaces with Title and preserves correct navigation/input ownership. No M13 work has started.
## Historical Validation Ledger (retained without rewriting prior observations)

# M12 Title and Main Menu — Validation Report (developer gate blocked)

## Final regression continuation — 2026-11-11

- Branch `feat/m12-title-main-menu`; candidate implementation commit `a26429b5380a2feeb8dcf407588449cc06518812`. The current continuation adds test cleanup and this evidence.
- Full unfiltered EditMode: **541 passed, 0 failed, 0 skipped, 0 inconclusive**. Both Unity NUnit and Pipeline reported completion. The count increased from the historical 538 by three M12 tests. Result artifact: `%TEMP%/M12-final-editmode-results.xml` on the validation machine. Focused M12: **11/11 passed**. PlayMode discovery found **0 tests**; the M12 EditMode integration test enters actual Editor Play Mode.
- An earlier full run failed because the M10-15 fixture's deliberate presentation failure emitted an unregistered error log. The fixture now expects that exact error. Its focused rerun and the final full suite passed.
- The M12 production-scene test uses isolated Editor-only Settings, SaveData and MetaProgress roots, triggers an accepted load, removes a presentation reference after validation, observes actual asynchronous failure, reloads Title through its real controller, verifies recovery message, loading/modal cleanup and UI input, and starts S001 again. Normal Player Settings paths remain unchanged by the Editor-only override.
- Final Windows x64 build at `C:\Users\UserK\AppData\Local\Temp\M12FinalPlayer\M12.exe`: **Succeeded, 0 errors, 486 warnings**. The test-generated background-execution setting and temporary Input System preload were restored before the final build. The fresh build has not received a complete interactive standalone smoke; the earlier Player interaction and user physical keyboard evidence below refer to the prior build. The final code changes after the build affect Editor tests only.
- The user reports physical standalone Escape, arrow navigation, Enter Continue, return confirmation, keyboard cancellation, and confirmed return to Title as **PASS — User-performed physical Windows standalone Player verification**. This is not Codex-performed input and does not constitute overall visual acceptance.
- Title captures exist at 1280×720, 1600×900 and 1920×1080; the report's other state captures are not available at every required resolution. Mandatory remaining evidence includes the full requested state/resolution screenshot matrix and an interactive smoke of the final rebuilt Player. The developer gate remains blocked pending those checks; User Visual Acceptance remains pending. No final PR is claimed.

## Latest continuation — 2026-11-11

- The final full unfiltered EditMode run after the asynchronous-load test change completed **538 passed, 0 failed, 0 skipped, 0 inconclusive**. The M12 focused suite passed 8/8; the updated M10-15 integration case passed independently and in the full run. PlayMode discovery found zero tests; no PlayMode suite is claimed.
- The integration case validates a repeated load request returns Busy, induces a failure after prepared restore begins, and observes failure completion after load ownership is released. Earlier repository and compatibility tests cover missing, corrupted, unsupported, invalid timestamp and incompatible saves; the exact-line integration restores a line and successor. Title recovery after an induced asynchronous failure has not been observed end to end.
- Connected Editor Play verified Resume closes the menu, Return opens confirmation, cancellation restores the menu, confirmation reloads Title, and New Game starts S001 again. Explicit navigation links and EventSystem Move events were tested. These do not establish physical standalone keyboard navigation or cancellation.
- The user independently verified physical Escape opens the standalone menu in the earlier Player build. The most recent Windows x64 build succeeded at `C:\Users\UserK\AppData\Local\Temp\M12FinalPlayer\M12.exe` with 0 errors and 486 warnings. Its interactive smoke remains outstanding; previous Player interaction observations below refer to the earlier build.
- `ProjectSettings.asset` acquired a temporary project-wide Input Actions preload during build. The installed Input System's build provider adds and removes this entry around builds; the Editor's current preloaded list was empty. The incidental file change was restored; no project setting change is included.
- The title uses a static Korean font asset; a separate static three-glyph fallback supplements existing Records text coverage. Gameplay canvas interaction is disabled while the in-game menu or confirmation owns input and restored on Resume.
- Mandatory remaining checks: physical standalone keyboard navigation, selection and cancellation; standalone return-to-title confirmation and return; and end-to-end title recovery following a failed asynchronous restore. User visual acceptance remains pending. No final PR is claimed.

Status: **developer-side acceptance is blocked; not ready for user Play Gate**. This report records only checks actually performed; user visual acceptance remains a separate pending decision.

- Base: `e6f8b7a964661e4da24dc2248a372d26f3f1d7a2`; implementation commit: `c9f83c1`; preserved continuation commit: `b319dfc`. A final delivery commit and PR are intentionally not available while the developer-side gate is blocked.
- Scene integration: `VN_Main` disables the production runner's serialized automatic start and instantiates `VNTitleScreen.prefab`; its controller references existing scene services. The technical start node remains unchanged.
- Assets: `VNTitleScreen.prefab` and locally generated `M12_ProvisionalBackground.png`; the temporary gradient contains no story imagery. The existing Korean TMP font and M11 palette are reused. Artwork replacement procedure and square-Sprite serialization check are in the specification.
- Focused tests: five M12 EditMode tests passed in the most recent focused run (scene wiring, prefab structure, gameplay gate, ordering/ties, and rejection of invalid/incompatible candidates).
- Full unfiltered EditMode: latest run after the title-flow changes reported 535 passed, 0 failed, 0 skipped. An earlier run reported one failure; the subsequent full rerun passed.
- Connected Editor Play: title appeared with runner idle; New Game reached `line:prologue_s001_001` after a double invocation; isolated Quick Save, return to title, Continue, actual load completion, and successor `line:prologue_s001_002` were observed. Existing Settings and Records opened with production dialogue idle. These checks do not cover every failure or input scenario.
- Game View captures: `Docs/Production/M12_Evidence/M12_title_1280.png`, `M12_title_1600.png`, `M12_title_1920.png`, `M12_title_load_final.png`, `M12_title_settings.png`, `M12_title_records.png`, `M12_title_quit.png`, `M12_ingame_menu_final.png`, `M12_return_confirm.png`, `M12_S001.png`, and `M12_continue.png`. The three title captures were rendered using corresponding Game View target sizes. Subjective user approval remains pending.
- Windows Player: the final isolated x64 build at `C:\Users\UserK\AppData\Local\Temp\M12FinalPlayer\M12.exe` succeeded with 0 errors and 486 warnings. The first post-build Computer Use session reported `GetCursorPos failed: access denied (0x80070005)`; this was later recovered by resetting the Computer Use session and binding a fresh visible Player window. See the Windows diagnostic and continuation results below. Mouse-based Player checks cover Title, New Game, Load, Settings, Records, and Quit. The user has now independently confirmed physical Escape opens the three-option in-game menu. Remaining menu, keyboard-navigation, return-to-title, and save/load failure scenarios are tracked below.
- Outstanding: complete in-game menu Resume/Return confirmation and cancellation/success cycles, applicable keyboard selection/cancel checks, and remaining save/load negative cases; fix any reproducible defect and repeat affected gates before the final commit/push/PR. Full M13 redesign of Settings, Save/Load, Records, and Backlog remains deferred.

## Continuation verification

- 2026-11-11: full unfiltered EditMode rerun completed: 535 passed, 0 failed, 0 skipped, 0 inconclusive. PlayMode discovery completed with **0 tests**, so no PlayMode suite is claimed as passing.
- Final isolated Windows x64 build: succeeded, 0 errors, 486 warnings. Build-report grouping: 460 `ConvGeneric`, 9 `Hidden/Sentis/ScatterND`, 6 `Hidden/Sentis/ConvTranspose`, 4 `Hidden/Sentis/SliceSet`, 3 `Hidden/Sentis/GridSample`, 2 `Hidden/Sentis/ScatterElements`, 1 `Hidden/Sentis/Pad` shader warnings, and 1 warning that optional Pipeline runtime configuration is absent. These are package shader/runtime-configuration warnings; no pre-M12 comparison build was run, so they cannot be classified as pre-existing by measurement.
- That earlier observation is superseded for mouse input by the fresh-window session below. No security policy, desktop ACL, UAC, or process privilege change was made.
- The Editor Play evidence above remains valid but does not substitute for standalone keyboard acceptance. M12 is not ready for the user Play Gate; no final PR has been opened.

## Windows desktop/input diagnosis and additional Player smoke

Environment diagnostics for the failed `GetCursorPos` call found an active `userk` Remote Desktop session (session 1), an accessible `WinSta0\Default` input desktop, and a successful read-only `GetCursorPos`/`OpenInputDesktop` probe from the current standard-user PowerShell process. Codex/Computer Use host, Unity Editor, and the diagnostic Player were observed in session 1 under the same user at medium integrity with token elevation disabled. No locked desktop, secure UAC desktop, elevated-process mismatch, or UIPI evidence was found. This does not support a Windows security-context cause for the original denial.

The Computer Use JavaScript session was reset, then a newly enumerated and freshly bound Player window accepted mouse clicks. Actual state changes followed those clicks, including advancing the dialogue and opening/closing UI. The most likely cause of the original `GetCursorPos` denial is stale Computer Use session/window-binding state; this is an inference from recovery after a fresh binding, not a direct inspection of the helper internals. No bypass, elevation, desktop permission change, or Windows security-policy change was used.

On 2026-11-11, the isolated Player at `C:\Users\UserK\AppData\Local\Temp\M12FinalPlayer\M12.exe` used the isolated data root `C:\Users\UserK\AppData\LocalLow\M12Validation\M12FinalSmoke`. The following were observed in its actual 1920×1080 runtime:

- Title Load opened the existing Load modal; selecting the persisted Quick slot restored the saved S001 line (`민석 / 아빠는 아직 아무 말도 안 했는데.`). Earlier mouse advance confirmed the successor line (`나래 / 지난번에 했잖아.`).
- Title Settings opened the existing Settings screen and its visible close control returned to Title.
- Title Records opened the existing Records screen and its visible Close control returned to Title.
- Quit opened the Korean confirmation. Cancel returned to Title. Confirm removed the Player window from the live Windows window inventory; a subsequent Player bind reported no owning process. This verifies application exit through the in-app confirmation flow, not a forced process stop.
- Earlier in this isolated run, New Game displayed the first S001 line and mouse advance progressed dialogue; Save/Load UI, a Quick save, and the exact-line restore/successor flow were observed. These operations touched only the isolated test data root above.
- Computer Use key actions had produced no visible state change; this is superseded by the user's independent physical-keyboard test below and is not treated as evidence of an application binding defect. No controller was available for a legitimate device test.
- The final Player logs contain no managed exception or crash marker. They include Unity's non-fatal `d3d12: failed to query info queue interface (0x80004002)` startup message. This smoke does not establish the absence of all runtime errors.

Computer Use screenshots from this Player pass were displayed inline during the task; they were not exported as screenshot files. The named PNGs above remain Unity Editor Game View evidence, and are not represented as Windows Player captures.

## Test discovery and current gate

The project currently has Editor tests under `Assets/_Project/Scripts/Tests/Editor`, no `.asmdef` files, and no PlayMode test assembly. Connected Unity Pipeline discovery returned 0 PlayMode tests. This matches the current test layout; it does not establish PlayMode coverage. The five focused M12 EditMode tests and the full 535-test EditMode run are the previously executed results recorded above. No implementation code changed after those runs during this continuation; this continuation changed only this report.

The Unity Editor remains connected to `C:\Dev\project_all_time` on Unity `6000.3.21f1`, Play Mode stopped, compilation clean, and the current Editor Console ground truth reports 0 errors and 3 warnings. The 486 build warnings are the package Sentis shader warnings and the optional Pipeline runtime-configuration warning listed earlier; there is no measured pre-M12 comparison build to classify them as pre-existing.

## User-performed physical keyboard evidence

The user subsequently reports the following physical Windows standalone Player checks as **PASS — User-performed physical Windows standalone Player verification**: S001 Escape menu, arrow navigation through all three options, Enter on Continue, return-to-title confirmation, keyboard cancellation, and confirmed return to Title. This is user-reported evidence from the earlier Player build; it is not a fresh test of a later rebuild. Overall visual acceptance remains pending.

The user subsequently reports completing the requested standalone keyboard-navigation checks and that arrow-key navigation works. This is user-reported physical input evidence. Individual confirmation, cancellation, and return substeps were not separately described in that report; their Editor Play observations are recorded above.

On 2026-11-11, the user launched `C:\Users\UserK\AppData\Local\Temp\M12FinalPlayer\M12.exe`, started New Game, entered S001, and pressed Escape on a physical keyboard. The user reports **PASS**: the in-game menu opened over the active dialogue and displayed `계속하기`, `타이틀로`, and `종료`. The user also reports a screenshot is available as visual evidence. This is explicitly user-performed physical Windows Player input; it is separate from Computer Use and simulated-input evidence. It confirms Escape-to-open only; it does not establish menu close, keyboard navigation, return confirmation, or return-to-title behavior.

**Developer-side gate: BLOCKED.** The remaining gate includes in-game-menu Resume/Return flows, keyboard selection/cancellation behavior independent of Escape-to-open, and the outstanding missing/corrupt/incompatible/failed-load scenarios. User visual acceptance and M13 deferred redesign work remain separate.
