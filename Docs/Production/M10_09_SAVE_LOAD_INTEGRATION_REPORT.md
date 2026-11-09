# M10-09 Save/Load Integration — Completed Technical Report

The investigation below is historical. Its final integration section records the merged exact-line production implementation; legacy schema-v1 checkpoints remain supported separately.

The following initial investigation is historical. Option A was rejected; the later Option C implementation and validation sections supersede its status and candidates.

## Baseline

- Base: `e53bae886e94d19587a793371986294e533786f8`.
- Branch: `feat/m10-09-production-save-load-integration`.
- Connected Editor: Unity `6000.3.21f1`.
- Fresh, unfiltered Editor test run through Unity MCP: **495 passed, 0 failed, 0 skipped**. This is the pre-change baseline, not an M10-09 acceptance result.
- The command-line batch test could not run against the open project: `COMMAND_FAILED`, project already open in Editor PID 26152. The connected Editor test runner was used instead.

## Checkpoint design investigation

The following are candidate boundaries only; no production catalog, node, or scene wiring has been added yet. Exact target nodes and presentation normalization must be validated in Play Mode before adoption.

| Candidate | Source position | Relevant state and hazards |
| --- | --- | --- |
| Family morning | S001 P03/P04 | `bgm_ordinary_morning` starts in P03. A re-entry before that command would restart music. Later saves may have different characters and backgrounds, so the captured presentation cannot simply be displayed with earlier text. |
| Storage confrontation | S003 P10 to S004 P01 | P10 hides the information overlay; S004 P01 immediately displays the shoe CG. S004 P07 later arms a manual-only hold. A rewind to this boundary intentionally encounters the hold again. |
| Farewell | S005 P11/P12 | P11 starts `bgm_quiet_farewell`; P12 follows it. S006 P03 subsequently stops the BGM, so a later save mapped to this boundary could capture silence. Six-character staging also changes in S006. |
| Return to reality | S006 P07/P08 | The white-door effect is turned off in P05; P06 contains a fade and disconnect SFX; P07 shows ordinary Rope. Avoid re-entering P06 and its one-shot audio. |
| Treatment preparation | S008 P01/P02 | P01 changes background and characters and displays an information overlay; P03 begins the profile browser. A boundary must be outside the browser and handle the overlay without inventing backlog lines. |

`VNYarnSaveCoordinator.TryComposeCompleteSave` captures the **current** stable presentation and BGM, while the checkpoint stores an earlier Yarn node. Consequently, advancing far past a checkpoint can result in saved presentation/audio that is incompatible with replayed dialogue. A safe implementation must establish deterministic re-entry presentation without replaying earlier one-shot effects, while explicitly resolving the conflict between restoring the saved BGM and the expected music at the rewound narrative point. Simply adding jumps to the nodes above does not satisfy this requirement. The existing transition controller considers a visible localized effect unstable, and load normalization clears it; this has not yet been verified in a production save/load run.

## Remaining work

Implement and validate the production topology and re-entry behavior, create the catalog and wire the scene through the Editor, retain the technical fixture catalog in tests, and exercise real isolated file round trips and live UI scenarios. Run focused and full tests again after implementation. Verify all requested transient-state, Auto/Skip, Backlog, M8, Replay, thumbnail, and continuous-play requirements. Record actual evidence here, then commit and open the single final PR only once the change is reviewable. No save file, thumbnail, or MetaProgress data has been intentionally modified by this investigation.

M10-10 has not been started.

## Option C decision and fixture progress (2026-11-04)

The checkpoint-only production proposal above was rejected. The approved target is exact-current-line persistence (Option C); the candidate checkpoint table is historical investigation and is not the selected production design.

The installed Yarn Spinner Unity package is 3.2.7. Its `DialogueRunner.StartDialogue` sets a program and starts an explicit node with `Dialogue.SetNode`; no public instruction-position restoration was established in the inspected runner API. The project-owned line-entry approach remains the implementation direction.

An isolated Yarn project under `Assets/_Project/Tests/ExactLineFixture` and `M10ExactLineFixtureTests` exercise a real `DialogueRunner`. The focused EditMode test passes: starting the line entry redisplays `line:exact_fixture_a`, does not repeat a preceding variable increment or SFX command, and after an accepted advance reaches `line:exact_fixture_b` and executes the subsequent increment once. The test uses a test presenter and manually restores fixture variables; it does not test an actual production save file, schema v2, real LinePresenter visuals, presentation, BGM, or the complete load transaction.

A synchronous test presenter exposed that `DialogueRunner.Stop()` cancels the current line before the dialogue cancellation token; a presenter completing synchronously can let the old continuation run. The fixture models asynchronous presenter dismissal and waits for both stop and presenter quiescence before starting again. This finding requires validation with the production LinePresenter/load barrier; the fixture is not proof that the production transaction is safe.

Still required: complete the fixture coverage specified in the work order; implement v2 serialization and explicit v1 compatibility; implement and validate the resume manifest and graph fingerprint; capture line snapshots; integrate Manual, Quick and Auto with the existing coordinator and two-phase load; transform and verify all 379 production lines; implement transient and overlay handling; conduct real-file and actual UI/Play validation; and update this report with results. No v2 persistence has been implemented yet.

After adding the fixture, the unfiltered EditMode suite passed **496/496**, with 0 failures and 0 skipped. This validates the existing suite plus the narrow fixture only; it is not M10-09 acceptance. Unity recompilation reported zero compiler errors. The installed Pipeline package emitted a warning that it uses older command names.

## Implementation progress (continued 2026-11-04)

This section supersedes the earlier statement that no v2 persistence had been implemented. Storage now accepts legacy checkpoint schema v1 and a distinct schema v2 with `saveMode`, `resumeLineId`, `resumeEntryNode`, and a SHA-256 fingerprint of the compiled Yarn program. The existing technical writer remains v1; the limited production path selects v2. Future-version tests now use schema 3 instead of 2 because 2 is now a recognized schema. The v1 technical coordinator path remains separate; no migration is attempted. V2 validation verifies the approved line/node pair, the entry node's first compiled instruction, and the program fingerprint before crossing the Stop boundary.

One provisional production mapping exists: `line:prologue_s001_016` -> `PROLOGUE_LINE_S001_016`. The original paragraph appears only once and the next paragraph is still `line:prologue_s001_017`. Other active production paragraphs are currently refused by the line mapping; this is not the required 379-line integration. The VN_Main session and interaction-gate references were wired using the Unity Editor. The serializer also accepts `outer_far_right`, fixing a six-slot file-validation gap. A consumed-line flag prevents the supported line from being saved after an authorized consume while the prior presentation is still active.

The repository tests wrote and reread v2 JSON under an isolated temporary root and verified malformed identity data does not replace a valid v1 file. A coordinator test composed a v2 save for the production ID, reread it, and rejected a mismatched node before mutation; that test sets session state in a harness and is not a live production-line load. A separate compiled-program inspection showed the entry's first line and successor are `line:prologue_s001_016` and `line:prologue_s001_017`. The unfiltered EditMode suite passed **499/499** after updating the S001 node-count assertion to account for the one extra entry node; there were no skipped tests or compiler errors. The old future-version test inputs were updated from 2 to 3; other assertions remain intact.

Not yet validated: actual VN_Main production Play save/load, LinePresenter stop/quiescence with this v2 route, thumbnail, audio and visual restoration through a real file, input normalization, and the remainder of the requested M6/M8/interaction cases. The first end-to-end production line and the other 378 lines remain outstanding. Do not present this report or this branch as ready for review.

## Final integration and verification (supersedes the historical entries above)

M10-09 remains a single change on `feat/m10-09-production-save-load-integration`, based on `e53bae886e94d19587a793371986294e533786f8`. This section records the completed implementation and the scope of the checks; it does not claim the separate M10-13 user Play Gate or completion of M10 as a whole.

### Persistence and graph

Schema v2 stores the current unconsumed production line, its approved resume entry, `saveMode`, and `resumeGraphVersion`, alongside the validated Yarn variables, presentation, audio and playtime snapshot. The version is a SHA-256 fingerprint of the compiled Yarn program and the resume manifest, calculated from the final graph rather than a hard-coded prototype hash. Version 1 remains on the separate legacy checkpoint path; there is no automatic migration. Corrupted, unknown-version and incompatible slots are protected from automatic and manual overwrites.

The deterministic `AgentScripts/m10_09_build_resume_graph.py` transformation preserves the 103 original presentation beats and 379 canonical paragraphs, each with its existing explicit line ID. The manifest records source beat, scene, resume node, successor flow, presentation context, eligibility and reason. Automated compiled-program checks inspect the first RunLine and successor of all 379 entries, and existing explicit-ID tests cover all 541 IDs with no implicit or duplicate IDs. There are 367 eligible paragraphs and 12 restricted paragraphs: S004-034 (manual hold), S006-007 through S006-014 (white-door lifetime), S008-009 and S008-010 (profile review), and S008-041 (terminal audio/title). Dynamic transition, active effect, asynchronous command, interaction and load ownership checks also reject unstable occurrences.

The authoritative session records consumption per presentation occurrence, so a consumed or stale occurrence cannot be captured. Load validates the slot, graph, state and required scene dependencies before stopping the runner. The existing presenter barrier now waits for both presenter and tracked command quiescence and settled frames. Project asynchronous profile, information, transition, audio and Yarn wait commands are tracked and cancelled on Stop. This addresses an observed Yarn 3.2.7 race in which an old async continuation could otherwise advance the newly started runner. A live reproduction loading S004-001 while the S008 profile browser was active stayed on S004-001 after the fix, and one manual advance reached S004-002. A separate terminal wait probe also remained on the restored line after the old wait would have expired.

The existing modal, repository and controller remain the owners. Manual and Quick saves use v2 for eligible production lines. Eight selected production line boundaries request v2 autosaves through the existing rotating allocator; load re-entry does not create another autosave. Thumbnail requests are tied to slot revision and line occurrence, and a sidecar older than its JSON is treated as a placeholder. Stable information overlays and the medical diagram are reconstructed through their existing scene authorities, without inserting narrative or history entries. Backlog stays session scoped and durable ReadHistory remains owned by M8; neither history is serialized into a v2 slot.

### Real Unity Play and disk observations

The following used the actual `VN_Main` runner and LinePresenter, production save/load services and isolated `Application.persistentDataPath` roots. Inputs were driven through the production input bridge and UI `Button.onClick` events, not physical pointer/keyboard input. User SaveData and MetaProgress were not used. The serialized technical start remains `M2_UI_START`; the continuous run used the controlled `PROLOGUE_S001_START` entry.

- S001: the modal saved `manual_00` on `line:prologue_s001_016`, including `bgm_ordinary_morning` and a JPEG. After advancing to 019, the modal loaded the JSON and the real presenter displayed 016; one manual advance reached 017.
- S002: `manual_01` saved 009 with `cg_beach_photo_child` and morning BGM; after 017, loading restored 009 and both assets, then advance reached 010.
- S003: `manual_02` saved and restored 010 with the internal-channel speaker and text after advancing to 015.
- S004: a saved 001 restored `cg_worn_child_shoe` after advancement. The 034 manual hold blocked automated advance and saving, and manual consumption released it. Existing regression tests cover the remaining modal and hide-input hold policies.
- S005/S006: S005-048 saved all six characters including `rope_memory`, `outer_far_right` and `bgm_quiet_farewell` at about 14.66 seconds. After advancing to S006-008, where the BGM was stopped and white door active, saving was rejected. Loading restored S005-048, the six-character presentation, playing farewell BGM and cleared the effect; a later observation was approximately 26.26 seconds, so this is evidence of resumed playback, not a sample-accurate timing measurement. Separately, S006-017 saved and restored the bedroom scene with real `rope`, `jihun` and `jinhee` after advancing to 030.
- S007: Quick Save at 008 restored the `최근 생활 기록` information overlay after advancing. A manual save at 028 restored the medical diagram and the same line after advancing to 038.
- S008: a persisted Auto slot restored 017 after a Play restart, without reopening the browser; one manual advance reached 018, and durable read status remained present. The active browser rejected Manual and Quick Save and manual advance. ReadOnly Skip and Auto were also exercised after restoration. Profile interaction completed in a continuous run through the ending.
- The continuous S001–S008 run recorded 380 backlog entries: 379 production paragraphs plus the initial technical line. Five retained Auto slots covered S004-001, S005-048, S006-017, S007-001 and S008-017. Loading S008-017 did not change the Auto-slot timestamps. Modal overwrite cancellation preserved the JSON hash; confirmed overwrite changed it. Auto slots were not writable in the modal.
- A real isolated v2 file with an invalid graph fingerprint returned `LoadValidationFailed`, and `SaveManual` returned `UnsupportedSave`; the current S001-017 line and file SHA-256 remained unchanged. A corrupted isolated file was likewise rejected. Legacy-v1 compatibility is covered by the existing automated checkpoint tests rather than a claim that an old user save was modified.

### Regression and diagnostics

The connected Unity 6000.3.21f1 Editor compiled with zero compiler errors. The final full unfiltered EditMode run passed **505, failed 0, skipped 0**. This includes serializer and legacy-v1 behavior, graph and successor audits, coordinator and presenter barriers, six-character and five-slot compatibility, input and transient state, M3–M10 regression and protected slot/thumbnail tests. The scene audit found one DialogueRunner, the expected technical start node, no missing script components and no missing inspected dependencies. Play mode was exited and the original company, product and `runInBackground` settings restored.

The final continuous-run Console query, scoped to its starting cursor, contained no new error or warning entries. The wider Console was not globally clean: one error and one warning predated that scoped run, and earlier artificial direct-scene probes generated character-visibility errors; Unity CLI also reported its existing older Pipeline command-name warning. These are not represented as a clean global Console. S006 effect appearance/disappearance phases and some manual-hold hide/modal combinations were covered by automated tests rather than individually observed with physical user input. The S008 live restore sampled the ordinary line after browser completion. The existing temporary media/final art and licensing work remains outside this milestone.
