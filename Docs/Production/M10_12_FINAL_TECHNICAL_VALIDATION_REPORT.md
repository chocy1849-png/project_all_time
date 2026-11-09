# M10-12 — Final Technical Validation Report

**Status: M10-12 FINAL TECHNICAL VALIDATION READY FOR REVIEW**

**Historical scope note:** M10-12 preceded and deferred the user Play Gate and documentation closure. The user-reported M10-13 prototype acceptance is recorded in the [M10-13 report](M10_13_USER_PLAY_GATE_REPORT.md); current M10 status, open risks and release conditions are consolidated in the [M10-14 closure report](M10_14_PROLOGUE_MILESTONE_CLOSURE_REPORT.md). The test and build results below remain historical M10-12 evidence.

**Validation date:** 2026-11-09
**Scope:** Windows release-candidate verification for the integrated S001–S008 production slice. This is not M10-13 user acceptance or M10-14 closure.

## 1. Repository and environment

| Item | Result |
|---|---|
| Repository | C:\Dev\project_all_time |
| Branch | feat/m10-12-final-technical-validation |
| Base / origin/main | 76238a833bda8504f3863936a31eb5bdd7f58578 |
| Final commit | Final branch HEAD containing this report; the exact HEAD SHA is shown by the associated PR and delivery record. |
| Unity Editor | 6000.3.21f1, revision c02631ffc030 |
| Unity CLI | 1.0.0-beta.17 |
| Yarn Spinner | 3.2.7 |
| Unity Pipeline package | 0.7.0-exp.1 |
| Platform | Windows 11 x64, NVIDIA GeForce RTX 3090, D3D12 |
| Build target | StandaloneWindows64 |
| Build scene | VN_Main is enabled in EditorBuildSettings. |
| Serialized Start Node | M2_UI_START remains the technical start. Production validation entered through PROLOGUE_S001_START. |

Input System 1.20.0, URP 17.0.3, and Unity Test Framework 1.4.5 were installed during validation. Project settings were restored to their baseline values after build serialization: default 1920×1080, native resolution, FullScreenWindow, non-resizable, and runInBackground disabled.

All test SaveData, MetaProgress, screenshots, and build outputs were isolated below %LOCALAPPDATA%\Temp\project_all_time_m10_12_43oxvqoa. The live SaveData and MetaProgress roots were kept under the isolated SaveLoad and Continuous subdirectories. Harnesses use the process-scoped PROJECT_ALL_TIME_TEST_ROOT seam. Personal saves and preferences were not read or changed.

## 2. Production story and resume graph

**PASS.** The production graph remained unchanged:

- 103 presentation beats.
- 379 canonical paragraphs with 541 unique explicit Yarn line IDs.
- 367 save-eligible occurrences and 12 restricted occurrences.
- Canonical graph fingerprint: cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9.
- The established compatibility policy continues to accept both historical LF and CRLF manifest fingerprints without rewriting saves.

One uninterrupted run began at PROLOGUE_S001_START and followed the supported S001 → S002 → S003 → S004 → S005 → S006 → S007 → S008 → Part 1 ending path. The production LinePresenter output was captured and compared with the frozen Korean source. The validator accepted all 379 paragraphs in order, with no missing or duplicate paragraph or line ID. Seven profile actions completed; the ending title was reached and the medical diagram was hidden before the end state. No direct mid-scene node jump was used as evidence.

The actual Player still starts at M2_UI_START by design. The standalone launch therefore verifies the technical UI and runtime startup, not the full production story. The full story evidence comes from the controlled production Editor Play run.

| Scene | M10-12 evidence |
|---|---|
| S001 | Production opening entered normally; canonical presenter text and ordinary morning BGM state were observed. |
| S002 | Continuous path reached the beach-photo passage; exact-line restoration returned the child-photo CG and morning BGM. |
| S003 | Internal narration and information overlay state were captured; exact-line restore continued to the recorded successor. |
| S004 | Shoe/storage flow was reached; the stable post-hold save round trip continued from S004-041 to S004-042. |
| S005 | Six-character family snapshot and farewell BGM were restored from S005-048; one advance reached S005-049. |
| S006 | Mechanical Rope remained distinct from ordinary Rope; exact-line restoration of S006-017 continued to S006-018. |
| S007 | Information overlay state was restored at S007-008; the medical-diagram state remains represented in the existing production regression coverage. |
| S008 | Post-profile state restored at S008-017; one advance reached S008-018. The full production flow reached the Part 1 title. |

These checks validate production state and authored flow. Subjective presentation quality, pacing, and acoustic judgment remain for M10-13.

## 3. Exact-line Save/Load matrix

**PASS.** The M10-12 live matrix used the production controller, repository, LinePresenter, and isolated JSON files. Each row was reached through production flow, saved, advanced, loaded from disk, and checked for the restored line/state and the authorized successor.

| Scene | Saved line | Restored state checked | Successor |
|---|---|---|---|
| S001 | line:prologue_s001_016 | Kitchen background, ordinary morning BGM | line:prologue_s001_017 |
| S002 | line:prologue_s002_009 | Child beach-photo CG, family characters, ordinary morning BGM | line:prologue_s002_010 |
| S003 | line:prologue_s003_020 | Internal narration, family presentation, information overlay | line:prologue_s003_021 |
| S004 | line:prologue_s004_041 | Storage background and confrontation characters | line:prologue_s004_042 |
| S005 | line:prologue_s005_048 | Six family slots including rope_memory, farewell BGM | line:prologue_s005_049 |
| S006 | line:prologue_s006_017 | Bedroom background and mechanical rope character | line:prologue_s006_018 |
| S007 | line:prologue_s007_008 | Doctor scene and recent-life information overlay | line:prologue_s007_009 |
| S008 | line:prologue_s008_017 | Post-profile treatment preparation state | line:prologue_s008_018 |

The eight round trips were real file writes and reads, not DTO comparisons. The full continuous pass also generated isolated Auto slots. Durable MetaProgress survived loading the earlier S001 position: the read count remained 355, unlock state was unchanged, and a Play restart retained the exact MetaProgress snapshot and a Valid manual slot.

### Save failure handling and restrictions

**PASS — automated regression and prior isolated file evidence.** The fresh full EditMode suite exercised the existing repository, load validation, compatibility, and thumbnail tests. These include malformed and missing-field JSON, future schema, stored-slot identity mismatch, malformed runtime snapshots, fingerprint compatibility/rejection, and stale or missing thumbnail fallback. Existing M10-09/M10-10 isolated-file evidence also records rejection of corrupted and incompatible saves before runtime mutation, protection of a valid slot, and acceptance of both historical LF and CRLF fingerprints. Schema-v1 remains on its separate compatibility path.

The production manifest still explicitly restricts S004-034 manual hold, S006-007 through S006-014 white-door lifetime, S008-009 and S008-010 profile review, and S008-041 terminal audio/title. Existing regression and prior live probes cover the hold, white-door, and profile restrictions.

**KNOWN_LIMITATION.** M10-12 did not collect a new live file-injection result for every requested individual fault variant. In particular, an operating-system write denial was not injected; invalid character/presentation/audio IDs and incomplete sidecar permutations were not each rerun through a live Player transaction. No permissions were changed. Treat these items as regression-covered only where the existing tests assert them, and do not infer a blanket pass for unexercised variants.

## 4. Cross-system checks

| System | Result |
|---|---|
| Auto save and restart | **PASS.** Continuous progression produced rotating Auto files. Earlier S001 load and Player restart preserved durable M8 read history; the manual slot remained Valid. |
| M6 Auto / Skip / Hide / Backlog / modal gates | **PASS — existing regression and prior production UI evidence.** The integrated suite passed; prior M10-10 live UI checks opened/closed these controls without moving the underlying line. M10-12 did not repeat every toggle combination with physical input. |
| M7 Settings and InputActions | **PASS.** Settings UI changed display mode and resolution through its production controls. Existing tests cover the production InputAction bindings and settings persistence. No saved user settings were touched. |
| M8 MetaProgress | **PASS.** Loading the earlier S001 checkpoint left 355 durable reads and unlock state unchanged; restarting the Play session preserved the exact MetaProgress file. |
| M9 Records | **PASS — regression coverage.** Existing full-suite tests and M10-10 live evidence cover catalog loading, records UI, progress, and modal ownership. |
| M9 Replay isolation | **PASS.** S001 and S002 excerpts each completed and were cancelled. The harness confirmed the production line, MetaProgress, stable presentation, BGM, variables, and manual-save anchor were unchanged. Replay clicks were isolated to its own runner. |
| Physical controls | **DEFERRED_M10_13.** Prior Editor keyboard focus was unavailable; synthetic key state did not trigger the production InputAction callback. Physical Space, click, and Escape acceptance is not claimed. |

## 5. Windows standalone Player

**PASS.** Connected Editor Pipeline builds succeeded for Windows x64 in Development and standard release configurations. The standard release report recorded 347,138,009 bytes, 42.2 seconds, zero errors, and 486 warnings. The Development build recorded 431,735,387 bytes, about 60.0 seconds, and zero errors. A separate isolated-clone smoke build and a resolution-test clone build also completed.

The Pipeline parser rejected the CLI string-array --options form for the Development build; the same build options were applied through the connected Pipeline evaluation API. The resulting BuildReports and executables reported success. The cold clone smoke build also succeeded; licensing-service messages from that separate batch attempt were not Player runtime failures.

Build warnings were classified as:

- 460 ConvGeneric shader variant warnings.
- 25 Hidden/Sentis shader warnings.
- 1 Pipeline-package warning that no RuntimePipelineConfig asset was found and Pipeline’s own Player runtime feature would be disabled.
- 0 build errors.

The isolated Player launched and showed the expected M2 technical UI dialogue. It exited cleanly when closed. The resolution clone used a distinct company/product identity and its own Player data path.

### Player diagnostics

The scoped Player logs contained no NullReferenceException, MissingReferenceException, unhandled project exception, or crash. They did contain:

- D3D12 info-queue query warning: 0x80004002.
- In the 1920×1080 launch, Unity reported broken IDXGISwapChain GetFrameStatistics timestamps and switched delta-time measurement to CPU-side timing.
- On Player shutdown, Unity emitted “GarbageCollector disposing of ComputeBuffer. Please use ComputeBuffer.Release() or .Dispose() to manually release the buffer.”

The last message appeared on the repeated smoke launches. No project C# allocation of ComputeBuffer was found during source inspection, but the native warning has no stack attribution in Player.log; its owner is therefore **KNOWN_LIMITATION**, not proven to be harmless or leak-free. Memory-leak freedom is not claimed. Player logs remain in the isolated temp root and are not committed.

## 6. Resolution and font checks

| Requested mode | Result | Evidence |
|---|---|---|
| 1920×1080 | **PASS** — default native Player mode | player_1920x1080.jpg |
| 1600×900 | **PASS** — selected in the actual Player Settings UI | resolution_1600x900.jpg |
| 1366×768 | **PASS** — selected in the actual Player Settings UI | resolution_1366x768.jpg |
| 1280×720 | **PASS** — selected in the actual Player Settings UI; game view and controls remained on-screen | resolution_1280x720.jpg |
| 2560×1440 | **ENVIRONMENT_BLOCKED** — the Player’s offered mode list ended at 1920×1080 on the attached display. No OS display mode was changed. | resolution_2560x1440_unavailable.jpg |

The tested sizes showed the dialogue panel and controls within the game view. The evidence screenshots include the Windows window frame for windowed modes; the displayed settings value is the game resolution. Per-monitor DPI behavior and broader hardware coverage remain untested.

**PASS — Korean glyph fallback.** The static production Korean font misses three code points in the tested Records corpus. The configured global NotoSansKR dynamic fallback resolves them. The new M10-12 Editor test asserts the font asset, fallback chain, and complete Records-text resolution. No font was regenerated or substituted.

## 7. Regression, lifecycle, and diagnostics

| Check | Result |
|---|---|
| Focused M10 EditMode suite | 22 passed, 0 failed, 0 skipped |
| Full unfiltered EditMode suite | 518 passed, 0 failed, 0 skipped |
| Compilation/recompile | Completed; no compiler errors |
| PlayMode test discovery | 0 PlayMode tests found; the project has no PlayMode suite to run. Production behavior was exercised through the controlled live Editor Play harnesses instead. |
| Production Yarn run | No compilation errors, unresolved commands, missing line IDs, or unintended branch observed in the controlled run. |
| Scoped story Console interval | 0 new production-path errors. Historical Editor and Pipeline tool entries were kept separate from this interval. |

The baseline snapshot included retained Editor/tooling history (3 errors and 1 warning at initial capture). Those counts are not the M10-12 production-path result. The final story run was scoped from its own cursor. Expected negative-test diagnostics and Pipeline command/parser errors were not classified as Player or production-story failures.

The eight disk round trips, two Replay excerpts, MetaProgress restart, repeated Player launches, and full story are meaningful lifecycle observations. No profiler capture was available for frame-time, resident-memory, allocation, or long-duration soak measurements. A clean exit and absence of an exception do not prove absence of a resource leak.

## 8. Media completeness and provenance

**PASS — technical references.** Required catalog IDs resolved; no broken GUID or unresolved loading path was found. The following are intentional placeholders, not broken runtime references:

| Resource | Classification | Release status |
|---|---|---|
| rope | Valid technical placeholder: temporary cyber-dog marker | **KNOWN_LIMITATION** — final mechanical Rope art and expressive head art are needed. |
| sfx_plate_counter | Valid technical placeholder: silent fallback | **KNOWN_LIMITATION** — intended sound is absent. |
| sfx_child_entry_steps | Valid technical placeholder: silent fallback | **KNOWN_LIMITATION** — intended sound is absent. |
| sfx_sink_washing | Valid technical placeholder: silent fallback | **KNOWN_LIMITATION** — intended sound is absent. |
| sfx_slipper_toe_tap | Valid technical placeholder: silent fallback | **KNOWN_LIMITATION** — intended sound is absent. |
| sfx_raindrop_awning | Valid technical placeholder: silent fallback | **KNOWN_LIMITATION** — no isolated drop source was approved; the supplied long recording was near-duplicate rain and was not substituted. |
| 19 scoped expression keys | Valid body-art fallback where separate head sprites are absent | **DEFERRED_M10_13** — expressive head assets and visual acceptance remain open. |

Existing user-supplied art, BGM, and SFX remain candidate media; per-file creator/source history and commercial-use release evidence are incomplete. The Mixkit-labeled rain component lacks an item-specific download receipt tying the local file to the cited license. The bundled Korean font’s file/style identity remains unreconciled (the imported filename says Regular while the recorded PostScript name says NotoSansCJKkr-Thin); the prior M10-08 audit records the relevant Noto license but does not close project-level provenance. No distribution clearance is asserted.

Before public release, retain per-file rights and attribution evidence, reconcile the font identity/license record, and replace or approve the six temporary resource IDs and missing expression heads. The existing M10-08 hardening report remains the detailed source/hash ledger.

## 9. Findings and status register

| Status | Finding |
|---|---|
| PASS | Full supported production flow, exact-line eight-scene disk round trips, Replay isolation, durable M8 progress, Windows builds, standalone launch, and 518-test EditMode regression passed. |
| PASS | No reproducible production-code defect was found in the M10-12 validation scope, so no production runtime change was needed. The new glyph coverage test passed against the existing configured fallback. |
| KNOWN_LIMITATION | Nonfatal ComputeBuffer disposal warning appears on Player shutdown; native ownership is not attributed. |
| KNOWN_LIMITATION | Individual write-denial and every negative file permutation were not newly exercised in M10-12; rely only on explicit existing regression assertions and cited prior file evidence. |
| ENVIRONMENT_BLOCKED | 2560×1440 could not be selected because the connected display exposed modes only through 1920×1080. |
| DEFERRED_M10_13 | Physical input, subjective visual and emotional pacing review, final sound audition, and user acceptance. |
| DEFERRED_M10_14 | Documentation and milestone closure after M10-13 and media/provenance decisions. |

No unexplained production-path exception was observed. The shutdown warning and unmeasured long-run memory behavior remain visible technical follow-ups. “Ready for review” does not mean public-release clearance.

## 10. M10-13 User Play Gate checklist

Do these checks with the user and record acceptance; none is marked accepted by M10-12.

### Physical controls

- [ ] Space advances dialogue and respects the S004 manual hold.
- [ ] Mouse click advances dialogue and does not double-advance.
- [ ] Escape closes/backtracks through Settings, Save/Load, Records, and Replay as authored.
- [ ] Auto and ReadOnly Skip start, stop, and respect holds and modal gates.
- [ ] Manual, Quick, and Auto Save/Load controls are understandable and usable.
- [ ] Records opens/closes without advancing the story; Replay completes and cancels cleanly.
- [ ] S008 profile cards browse and select using physical pointer/keyboard input.

### Visuals

- [ ] Sprite scale, slot alignment, family composition, and expression changes.
- [ ] Beach CG, shoe CG, medical diagram, and ending-title framing.
- [ ] Scene transitions, localized white-door effect, and absence of unintended shake.
- [ ] Korean text readability and line wrapping at intended display sizes.

### Sound

- [ ] Morning and farewell BGM timing, fades, and dialogue/SFX balance.
- [ ] Equipment/rain handoff, awning-drop timing, and separate bus-brake cue.
- [ ] Decide on the five silent SFX fallbacks and temporary cyber-dog marker.
- [ ] Audition the final mix on the intended playback device.

### Story experience

- [ ] Narrative pacing and manual-hold timing.
- [ ] Farewell emotional impact and white-door presentation.
- [ ] S006 identity reveal and S008 profile interaction.
- [ ] Part 1 ending presentation and emotional landing.

## 11. M10-14 documentation and closure handoff

At the time of M10-12, M10-14 was not started. PR #62 has since merged, and this documentation closure is recorded in the [M10-14 report](M10_14_PROLOGUE_MILESTONE_CLOSURE_REPORT.md). Remaining media/provenance follow-up is listed there; this technical report retains its historical validation scope.

## 12. Evidence files and reproduction assets

Intentional screenshot evidence is committed under Docs/Production/Evidence/M10_12:

- [1920×1080 Player launch](Evidence/M10_12/player_1920x1080.jpg)
- [1600×900 Player launch](Evidence/M10_12/player_1600x900.jpg)
- [1366×768 Player launch](Evidence/M10_12/player_1366x768.jpg)
- [1280×720 Settings selection](Evidence/M10_12/resolution_1280x720.jpg)
- [1366×768 Settings selection](Evidence/M10_12/resolution_1366x768.jpg)
- [1600×900 Settings selection](Evidence/M10_12/resolution_1600x900.jpg)
- [2560×1440 unavailable mode list](Evidence/M10_12/resolution_2560x1440_unavailable.jpg)

The temporary validation root retains the full Player logs, build reports, isolated save JSON, thumbnails, story capture, and matrix output. Generated executables, Player logs, save files, MetaProgress data, and temporary test JSON are excluded from Git.

Harnesses added for this milestone:

- AgentScripts/m10_12_live_full_story.cs and AgentScripts/m10_12_validate_story_capture.py
- AgentScripts/m10_12_live_save_load_matrix.cs
- AgentScripts/m10_12_live_meta_load_check.cs and AgentScripts/m10_12_meta_restart_probe.cs
- AgentScripts/m10_12_live_replay_isolation.cs
- AgentScripts/m10_12_set_isolated_root.cs
- Assets/_Project/Scripts/Tests/Editor/M10_12FinalTechnicalValidationTests.cs
