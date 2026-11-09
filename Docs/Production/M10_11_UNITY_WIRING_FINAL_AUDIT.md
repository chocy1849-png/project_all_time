# M10-11 — Unity Wiring Final Audit

**Status: M10-11 UNITY WIRING / FINAL AUDIT READY FOR REVIEW**

**Historical status note:** M10-11 deferred technical validation and user acceptance to later milestones. M10-12 completed the technical validation, and M10-13 recorded the user's overall QA prototype pass. See the [M10-12 report](M10_12_FINAL_TECHNICAL_VALIDATION_REPORT.md), [M10-13 acceptance report](M10_13_USER_PLAY_GATE_REPORT.md), and current [M10-14 closure report](M10_14_PROLOGUE_MILESTONE_CLOSURE_REPORT.md).

## 1. Baseline and scope

| Item | Result |
|---|---|
| Base / `origin/main` | `4fbb2c09b2707be67bf9af9e5594773e8e32da2f` |
| Merged prerequisite | PR #58, `[M10-10] Populate production Records and preserve exact-line saves` |
| Branch | `feat/m10-11-unity-final-wiring-audit` |
| Unity Editor | `6000.3.21f1` |
| Yarn Spinner | `dev.yarnspinner.unity 3.2.7` |
| Unity Pipeline package | `0.7.0-exp.1` |
| Unity CLI | `1.0.0-beta.17`, unchanged |
| Pre-change EditMode baseline | `508 passed / 0 failed / 0 skipped` |
| Final unfiltered EditMode | `517 passed / 0 failed / 0 skipped` |

M10-11 adds the consolidated editor audit suite and this report. No production Scene, Prefab, Yarn, catalog, mixer, font, or player preference was changed. No personal SaveData or MetaProgress was read or written. All live save/load checks used an isolated temporary root.

## 2. Scene and serialized-reference audit

**PASS.** Unity opened and inspected the complete `Assets/_Project/Scenes/VN_Main.unity` hierarchy, including inactive descendants. It contains 7 root objects, 523 GameObjects, and 34 inactive GameObjects. The new read-only test found no missing scripts, null components, or read-only Scene dirtiness. Twelve required Scene/catalog/Yarn/audio/input assets resolved through their existing stable GUIDs. Existing domain audits validated the required production Prefabs and catalog assets.

The table lists high-risk authorities rather than incidental Unity fields. All actual assignments below were inspected through Unity `SerializedObject` and checked by the M10-11 suite or the corresponding M6/M7/M9/M10 suite.

| Hierarchy path | Component / serialized field | Expected dependency | Actual assigned dependency | Validation | Required correction |
|---|---|---|---|---|---|
| `/Dialogue System` | `DialogueRunner.yarnProject` | Production YarnProject | `Assets/_Project/Yarn/GameNarrative.yarnproject` | PASS | None |
| `/Dialogue System` | `DialogueRunner.lineProvider` | Yarn localized provider | Serialized fallback is null in Edit Mode; Yarn 3.2.7 supplies `BuiltinLocalisedLineProvider` at runtime | PASS; live line rendered | None |
| `/VNConvenienceRuntime` | `VNLineLifecyclePresenter.linePresenter` | Production line view | `/VNCanvas/DialogueLayer/LineContainer` | PASS | None |
| `/VNConvenienceRuntime` | `VNLineLifecyclePresenter.sessionState` | Session state authority | `/VNConvenienceRuntime` | PASS | None |
| `/Dialogue System/Line Advancer` | `VNLineAdvancerInputBridge.sessionState`, `.interactionGate` | Shared session and input gate | `/VNConvenienceRuntime` | PASS | None |
| `/SaveLoadRuntime` | `VNSaveLoadController.dialogueRunner`, `.interactionGate` | Production runner and shared gate | `/Dialogue System`, `/VNConvenienceRuntime` | PASS | None |
| `/SaveLoadRuntime` | `presentationCatalog`, `informationOverlay`, `prologuePanels` | Production presentation authorities | `M10_ProductionPresentationCatalog`; `/VNCanvas/DialogueLayer/AuthoredInformation`; `/PresentationRuntime` | PASS | None |
| `/PresentationRuntime` | `VNPresentationController.catalog` | M3 production catalog | `M10_ProductionPresentationCatalog.asset` | PASS | None |
| `/AudioRuntime` | `VNAudioController.catalog`, `bgmSourceA/B`, `sfxSource` | M4 production catalog and distinct sources | `M10_ProductionAudioCatalog.asset`; `/AudioRuntime/BGMSourceA`, `BGMSourceB`, `SFXSource` | PASS; BGM/SFX mixer routes validated | None |
| `/VNConvenienceRuntime` | `VNSettingsRuntimeBootstrap` runner, panel, mixer, actions | Existing M6/M7 authorities | `/Dialogue System`; Settings panel; `VNAudioMixer.mixer`; `VNInputActions.inputactions` | PASS | None |
| `/VNConvenienceRuntime` | `VNMetaProgressRuntimeBootstrap.dialogueRunner` | Production runner | `/Dialogue System` | PASS | None |
| `/VNConvenienceRuntime` | `VNRecordsRuntimeBootstrap` MetaProgress, Yarn, catalogs, mapping, presentation | M8 authority and production assets | `/VNConvenienceRuntime`; `GameNarrative`; production presentation, Timeline, Gallery, Archive, Achievement, and unlock-mapping assets | PASS | None |
| `/VNCanvas/ModalLayer/RecordsModal` | `VNReplayController.recordsModal`, `replaySessionHost`, `replayViewTemplate` | Records-owned Replay hierarchy | Records modal; `/Panel/ReplayHost`; `/Panel/ReplayHost/ReplayViewTemplate` | PASS | None |
| `/VNCanvas/ModalLayer/RecordsModal/Panel/ReplayHost/ReplayViewTemplate` | `VNReplayView.presentationController`, `.linePresenter` | Replay-local presentation and presenter | `/ReplayPresentation`; `/LinePresenter` | PASS; separate from production owner | None |
| `/VNCanvas/M1007ProfileBrowser` | `interactionGate`, `saveLoad` | Shared gate and M5 owner | `/VNConvenienceRuntime`, `/SaveLoadRuntime` | PASS | None |
| `/VNCanvas/DialogueLayer/AuthoredInformation` | `titleText`, `bodyText`, `visibility`, `saveLoad` | Owned TMP fields, M6 visibility, M5 load suppression | `Title`, `Facts`, `/VNConvenienceRuntime`, `/SaveLoadRuntime` | PASS | None |
| `/PresentationRuntime` | `VNProloguePanels.dialogueRunner`, `.saveLoad`, `.diagram`, `.endTitle` | Production runner, Save/Load and both panels | `/Dialogue System`, `/SaveLoadRuntime`, `/VNCanvas/M1007MedicalDiagram`, `/VNCanvas/M1007EndTitle` | PASS | None |
| `/VNConvenienceRuntime` | All seven `VNConvenienceInputRouter` action references | VN gameplay InputAction asset | `VNInputActions.inputactions` (`Dialogue/Advance`, Auto, Skip, Hide, Quick Save/Load, Cancel) | PASS | None |
| `/EventSystem` | `InputSystemUIInputModule` | One UI EventSystem and valid UI action map | One EventSystem; Unity `DefaultInputActions` UI map (`Point`, `Click`, `ScrollWheel`, `Navigate`, `Submit`, `Cancel`) | PASS; correctly separate from VN gameplay actions | None |

`VNInputActions` binds Space to Advance and Escape to Cancel. Settings rebinding uses the same asset. Mixer groups and exposed settings parameters are validated by the M7 audit. No duplicate EventSystem was found. Optional fields are not treated as defects; in particular the runtime localized provider fallback is expected and verified in Play.

## 3. Dialogue ownership, initialization, and commands

**PASS.** `VN_Main` contains exactly one production `DialogueRunner` and one enabled production `LinePresenter` registered with it. Replay’s inactive template and session-local presenter are isolated and are not registered on the production runner. The serialized technical Start Node remains `M2_UI_START`; the approved production entry `PROLOGUE_S001_START` remains compiled and unchanged.

Initialization order was checked from execution-order attributes and scene references: MetaProgress `-2`; Settings and Records `-1`; DialogueRunner default order `0`. Records references the initialized M8 bootstrap and production YarnProject. Live Play showed one production runner and initialized Save/Load, presentation, audio, Settings, MetaProgress, and Records authorities. Existing tests cover cleanup and listener unsubscription; no duplicate registrations or orphan session remained at exit.

The compiled S001–S008 command audit found all 27 production commands mapped to one intended handler on the authoritative runner. No unknown/test-only command was present in the production story set, and no missing handler or collision was found.

| Command group | Commands | Runtime handler |
|---|---|---|
| Audio | `bgm_play`, `bgm_stop`, `sfx_play`, `sfx_wait` | `VNYarnAudioCommands` |
| Presentation | `vn_cg`, `vn_clear_bg`, `vn_clear_cg`, `vn_expression`, `vn_hide`, `vn_move`, `vn_scale`, `vn_show` | `VNYarnPresentationCommands` |
| Transitions | `vn_bg_crossfade`, `vn_fade_from_black`, `vn_fade_to_black`, `vn_local_effect` | `VNYarnTransitionCommands` |
| Manual hold | `vn_manual_hold` | `VNYarnManualHoldCommands` |
| Information | `vn_info_hide`, `vn_info_show` | `VNYarnInformationCommands` |
| Prologue panels | `vn_medical_diagram_hide`, `vn_medical_diagram_show`, `vn_part_end_title` | `VNProloguePanels` |
| Profile browser | `vn_profile_begin`, `vn_profile_review_complete`, `vn_profile_silent_complete`, `vn_profile_wait` | `VNYarnProfileCommands` |
| Tracked wait | `wait` | `VNYarnTrackedWaitCommands` |

## 4. Presentation, audio, and character slots

**PASS.** Production Presentation Catalog references resolve for the main presentation controller, Save/Load, Records bootstrap, and Replay-local presentation controller. Catalog and media tests validate the frozen production IDs, scoped aliases/expressions, and actual Sprite references: 9 characters, 12 backgrounds, and 6 CGs. Production and Replay each have six distinct configured character slots and visual roots; six-slot Save/Load and legacy five-slot restore compatibility passed.

The morning family composition uses five humans and ordinary `rope_memory`. Mechanical `rope` remains a separate identity and is never assigned ordinary dog art. The temporary cyber-dog marker and missing expression heads remain documented media placeholders, not broken catalog references. Background and CG validation includes import settings and representative Game View; Save/Load restored the child photo CG. The Records Gallery displayed six resolved, unlocked production Sprites in Play, and its fullscreen viewer preserved aspect and cleared its Sprite on close.

**PASS.** The sole `VNAudioController` uses `M10_ProductionAudioCatalog.asset` with 2 BGM IDs and 14 SFX IDs. The two BGM sources route to the BGM mixer group, SFX routes to SFX, and the voice presenter routes to Voice. Stable BGM capture/restore and finite SFX completion remain under the existing M4/M10 tests; no Ambient manager or duplicate audio owner exists. The live route executed the authored S001 morning cue, S005 farewell, S006 stop/door sequence, and S008 equipment-to-rain, awning-drop, and bus-brake ordering.

Five SFX IDs resolve to the documented temporary silent fallback: `sfx_plate_counter`, `sfx_child_entry_steps`, `sfx_sink_washing`, `sfx_slipper_toe_tap`, and `sfx_raindrop_awning`. The last remains inaudible; the tested sequencing verifies timing and distinct cues, not acoustic acceptance. `rope` still uses the documented temporary cyber-dog marker. These six media gaps are `KNOWN_LIMITATION` items for the later content/user gates; all catalog references resolve.

## 5. Save/Load, convenience, and Settings

**PASS.** The M10-09 schema-v2 exact-line system and separate schema-v1 path remain intact. The production manifest still has 379 mapped paragraphs, 367 eligible lines, 12 explicit restrictions, and 541 unique compiled explicit IDs; the canonical story remains 103 presentation beats. The graph fingerprint remains `cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9`.

The `VNFullSnapshotTests` focused run passed 12/12, including acceptance of both historical LF and CRLF fingerprints and rejection of altered/unknown fingerprints before mutation. The M10 graph audit and all exact-line Save/Load tests passed. A fresh isolated Play session loaded the S007 exact-line v2 save, restored `line:prologue_s007_008`, the `최근 생활 기록` overlay (`수면` / `식사`), and the expected presenter; the original successor flow continued. The M10-10 isolated matrix also restored representative S001, S002, S005, S006, S007, and post-profile S008 saves and checked presentation, Yarn variables, BGM, overlay, read count, and successor. The S002 photo, six-character S005 family including `rope_memory`, S006 `rope`, S007 information overlay, and S008 closed-browser state resolved correctly. SaveData and MetaProgress roots were under the task’s temporary test directory.

**PASS.** Manual Advance used the production bridge. In the live UI smoke, Hide/Restore, Auto on/off, Skip on/off, Backlog open/close, Settings open/close, and Save/Load modal open/close all preserved the current story line. A manual bridge request while Records was open was rejected without advancing the story. M8 durable ReadHistory remains the persistence owner; Backlog remains session-only.

M7 Settings bootstrap, category controls, InputActions, Mixer, and references passed. UI uses its own valid `DefaultInputActions` map; it is intentionally not the VN gameplay asset. No persisted Settings or Mixer preference was changed.

## 6. MetaProgress, Records, and Replay

**PASS.** Exactly one M8 MetaProgress authority is assigned to the M9 Records bootstrap. Catalog counts remain Timeline 8, Gallery 6, Archive 5, Achievements 4, with 12 valid unlock rules. The M9 validator passed and Records initialized with the production M8 service. Timeline, Gallery, Archive, and Achievements tabs opened in the production UI; the modal gate blocked underlying story advance and released on close.

**PASS.** `PrologueReplay.yarnproject` is distinct from `GameNarrative.yarnproject`. The two approved excerpts `REPLAY_S001_MORNING` and `REPLAY_S002_PHOTO` are absent from the production program. Replay started from an actual Timeline Replay button, and both excerpts passed normal completion and cancellation through the Replay Next/Close UI. Production line, Backlog, durable MetaProgress, stable presentation, BGM, Yarn variables, SaveData anchor bytes, and load state matched the pre-Replay snapshot after each session. No orphan Replay session or stale input/modal lease remained.

## 7. S008 forced-entry classification

**KNOWN_LIMITATION — harness-only unsupported entry.** The M10-10 `Character 'dokyung' is not visible` report came from a harness that directly started the S008 node without establishing its preceding S007 presentation. The supported S007 → S008 flow passed in the exact-line matrix and again in the fresh live run. A supported S007 exact-line load restored `dokyung` and the information overlay. The approved S008 post-profile exact-line load restored without reopening the profile browser. No production Scene initialization or supported resume defect was reproduced, and no direct-node production state mutation was introduced.

## 8. Test results and Editor health

Focused EditMode runs (overlapping filters are intentional):

| Focus | Result |
|---|---:|
| `M10_11UnityFinalWiringAuditTests` | 9 passed / 0 failed / 0 skipped |
| `M10_08ProductionMediaTests` | 8 / 0 / 0 |
| `VNPresentationAudioSpikeTests` | 2 / 0 / 0 |
| `VNAudio` | 12 / 0 / 0 |
| `VNSave` | 26 / 0 / 0 |
| `VNFullSnapshotTests` (LF/CRLF compatibility) | 12 / 0 / 0 |
| `VNMainM6WiringAuditTests` | 1 / 0 / 0 |
| `VNConvenience` | 38 / 0 / 0 |
| `VNMainM7SettingsWiringAuditTests` | 7 / 0 / 0 |
| `VNSettings` | 27 / 0 / 0 |
| `VNMetaProgress` | 61 / 0 / 0 |
| `VNMainM9RecordsWiringAuditTests` | 7 / 0 / 0 |
| `VNReplay` | 6 / 0 / 0 |
| `M10` exact-line, resume graph, M10-10, and M10-11 | 22 / 0 / 0 |
| **Full unfiltered EditMode suite** | **517 passed / 0 failed / 0 skipped** |

Final `recompile_status`: completed, failed `false`, no compiler errors. The end-title Play capture closed at Console cursor 563; the final Play interval had no new errors or warnings. Later focused EditMode tests emitted the expected missing-reference diagnostic from `VNConvenienceBridgeTests.Hide_IsRejectedForMissingGroup_Options_AndInactivePresentation` while asserting rejection. Earlier Console history includes the fixed test-source compile errors, other expected negative-path diagnostics, obsolete Pipeline `editor status` calls, and a Pipeline request timeout; none was a production-path runtime error. The test-source compile issues were corrected before the final focused/full runs.

Play exited normally with `Application.runInBackground` restored to `false`, session Auto-tick disabled, isolated root override cleared, Records closed, no active Replay, no profile browser, and no interaction lease. Read-only Scene tests left `VN_Main` clean.

## 9. Findings, corrections, and handoff

| Classification | Finding / action |
|---|---|
| PASS | 0 verified production wiring defects; 0 Scene/Prefab/catalog/Yarn corrections required. |
| FIXED_AND_RETESTED | Corrected M10-11 test implementation issues before acceptance: component enabled-state type, Records UI namespace, incidental TMP child-name assumption, and incorrect assumption that the EventSystem UI map should share the VN gameplay asset. The final 9-test suite and full 517-test run pass. |
| KNOWN_LIMITATION | The required Editor keyboard focus was unavailable (`Application.isFocused == false`); a synthetic H key state changed but did not invoke the gameplay InputAction callback. Hardware keyboard/mouse acceptance is not claimed. |
| DEFERRED_M10_12 | Broader release-quality technical validation remains for M10-12. M10-11 did not begin or claim that milestone. |
| DEFERRED_M10_13 | `PHYSICAL_INPUT_ACCEPTANCE — DEFERRED_M10_13`; visual quality/emotional pacing and audible mix approval; final cyber-dog artwork; five silent SFX; and user Play Gate remain for M10-13. |

**M10-12 technical handoff:** use the verified 517-test baseline and unchanged graph fingerprint; extend the release-quality technical matrix without changing the established production Yarn graph or schema-v2 compatibility.

**M10-13 user acceptance handoff:** conduct focused physical keyboard/mouse testing, visual/art review, pacing review, and acoustic audition. In particular review the temporary cyber-dog marker, silent fallback cues, CG framing, and final mix. No M10-13 user Play Gate is claimed complete.
