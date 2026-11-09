# M10-14 — Final Documentation Reconciliation and Prologue Milestone Closure

**Status: M10-14 DOCUMENTATION CLOSURE READY FOR REVIEW**

**Disposition: M10 PROLOGUE VERTICAL SLICE — IMPLEMENTATION AND USER QA COMPLETE**

**Commercial release: `COMMERCIAL_RELEASE_CLEARANCE_PENDING`**

This is a documentation and handoff milestone. It records the merged implementation and the user-reported prototype gate; it does not claim a new gameplay test run, final media approval, public-release clearance, or completion of later story chapters.

## 1. Verified baseline and M10-14 scope

| Git reference | Value |
| --- | --- |
| Repository | `chocy1849-png/project_all_time` |
| M10-14 branch | `feat/m10-14-prologue-milestone-closure` |
| Base | `origin/main` at `6f130c33a257d0dd83a0b0657528d900de34e73c` |
| Merged prerequisite | [PR #62 — Complete user prologue Play Gate](https://github.com/chocy1849-png/project_all_time/pull/62) |
| PR #62 merge commit | `6f130c33a257d0dd83a0b0657528d900de34e73c` |
| M10-13 acceptance date | `2026-11-09` |

M10-14 was branched from that verified merge commit. The changes in this milestone update documentation only. No canonical story or production Yarn, resume manifest, runtime schema, catalog, Scene, serialized asset, user setting, SaveData, or MetaProgress file is changed.

## 2. Milestone disposition

| Milestone | Final disposition | Report / merged PR |
| --- | --- | --- |
| M10-01 | Story Authority Complete | [Frozen story source](../Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt), [PR #47](https://github.com/chocy1849-png/project_all_time/pull/47) |
| M10-02 | Presentation Mapping Complete | [Presentation map](M10_PROLOGUE_PRESENTATION_MAP.md), [PR #48](https://github.com/chocy1849-png/project_all_time/pull/48) |
| M10-03 | Asset Inventory / ID Freeze Complete | [Current inventory](M10_CURRENT_ASSET_INVENTORY.md), [production ID map](M10_PRODUCTION_ID_MAP.md), [PR #49](https://github.com/chocy1849-png/project_all_time/pull/49) |
| M10-04 | Technical Spikes Complete | [Spike results](M10_04_TECHNICAL_SPIKE_RESULTS.md), [PR #50](https://github.com/chocy1849-png/project_all_time/pull/50) |
| M10-05 | S001–S003 Implementation Complete | [Playable report](M10_05_S001_S003_PLAYABLE_REPORT.md), [PR #51](https://github.com/chocy1849-png/project_all_time/pull/51) |
| M10-06 | S004–S006 Implementation Complete | [Playable report](M10_06_S004_S006_PLAYABLE_REPORT.md), [initial PR #52](https://github.com/chocy1849-png/project_all_time/pull/52) and [completion PR #53](https://github.com/chocy1849-png/project_all_time/pull/53) |
| M10-07 | S007–S008 Implementation Complete | [Playable report](M10_07_S007_S008_PLAYABLE_REPORT.md), [PR #54](https://github.com/chocy1849-png/project_all_time/pull/54) |
| M10-08 | Production Media Integration Complete; specific placeholders and evidence gaps remain | [Media integration](M10_08_FINAL_MEDIA_INTEGRATION_REPORT.md), [post-merge hardening](M10_08_POST_MERGE_HARDENING_REPORT.md), [PRs #55](https://github.com/chocy1849-png/project_all_time/pull/55) and [#56](https://github.com/chocy1849-png/project_all_time/pull/56) |
| M10-09 | VN-style Exact-Line Save/Load Complete | [Save/Load report](M10_09_SAVE_LOAD_INTEGRATION_REPORT.md), [PR #57](https://github.com/chocy1849-png/project_all_time/pull/57) |
| M10-10 | Production Records Integration Complete | [Records report](M10_10_PRODUCTION_RECORDS_REPORT.md), [PR #58](https://github.com/chocy1849-png/project_all_time/pull/58) |
| M10-11 | Unity Wiring Audit Complete | [Wiring audit](M10_11_UNITY_WIRING_FINAL_AUDIT.md), [PRs #59](https://github.com/chocy1849-png/project_all_time/pull/59) and [#60](https://github.com/chocy1849-png/project_all_time/pull/60) |
| M10-12 | Final Technical Validation Complete | [Technical validation](M10_12_FINAL_TECHNICAL_VALIDATION_REPORT.md), [PR #61](https://github.com/chocy1849-png/project_all_time/pull/61) |
| M10-13 | User QA Play Gate Passed | [User Play Gate report](M10_13_USER_PLAY_GATE_REPORT.md), [PR #62](https://github.com/chocy1849-png/project_all_time/pull/62) |
| M10-14 | Documentation Closure Ready for Review; not merged | This report; final PR is the M10-14 review vehicle |

The older M10-01–M10-13 reports remain execution records. Their handoff and `PENDING` statements describe what was known at that milestone; later completion reports linked here supersede those earlier current-state summaries without rewriting the historical evidence.

## 3. Canonical story and Yarn integrity

The frozen narrative authority is [`Docs/Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt`](../Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt). Its recorded SHA-256 is `8413e847dd66d7d45585847a935b9c384d91facfe9724ee27084f3d86ff977e9`. Its Git blob matches the M10-01 imported source; the working tree also matches the merged M10-13 baseline. No narrative source or production Yarn file is changed in M10-14.

The production Yarn files and [`M10_09_LINE_RESUME_MANIFEST.json`](../../Assets/_Project/Resources/M10_09_LINE_RESUME_MANIFEST.json) were checked against the merged baseline. Static reconciliation found 379 production `#line:` tags, 379 unique line IDs, 379 unique manifest entries, and an exact match between the source line-ID set and manifest runtime line-ID set. The manifest has 367 `SAVE_ELIGIBLE` entries and 12 `SAVE_BLOCKED_TRANSIENT` entries: one manual-only hold, eight localized white-door occurrences, two profile-review thoughts, and one terminal one-shot audio sequence.

The presentation map contains 103 beats across S001–S008: 13, 10, 10, 16, 15, 14, 11, and 14 beats respectively. The merged M10-12 Yarn compilation result records 541 unique explicit compiled IDs, zero implicit IDs, and zero duplicates. The M10-12 Editor test `GameNarrative_CompilesWithOnlyExplicitUniqueLineIds` asserts the 541 count, no implicit tags, and uniqueness. M10-14 did not rerun Unity compilation or the full suite.

Production Replay remains outside the production Yarn compilation in [`PrologueReplay.yarnproject`](../../Assets/_Project/RecordsReplay/Yarn/PrologueReplay.yarnproject). It does not add resume lines to the production graph.

## 4. Implemented production inventory

| Area | Merged implementation |
| --- | --- |
| Prologue | S001–S008 and their 103 mapped presentation beats are implemented in the canonical order. |
| Production story lines | 379 canonical paragraphs retain stable explicit IDs and executable resume entries. |
| Exact-line persistence | Schema-v2 Manual, Quick, and selected Auto saves use exact-line resume; legacy schema-v1 checkpoints use a separate compatibility path. |
| Presentation/media | Production catalogs contain 43 frozen global resource IDs: 9 character bundles, 12 backgrounds, 6 CGs, 2 BGM and 14 SFX. 37 have user-supplied or derived runtime media; 6 resolve to temporary resources. |
| Character expressions | 19 scoped `(characterId, expressionId)` keys remain within the nine character definitions; they are not additional global resource IDs. |
| Records | Existing catalogs contain 8 Timeline scene entries, 6 Gallery CG entries, 5 Archive entries, 4 Achievements and 12 unlock rules. |
| Replay | Two isolated prologue excerpts are authored in their separate Replay Yarn project. |
| Scene composition | Production supports six fixed character slots and retains the serialized technical start at `M2_UI_START`. |

The authoritative production ID and per-file media records remain in the [production ID map](M10_PRODUCTION_ID_MAP.md), [asset manifest](../07_ASSET_MANIFEST.md), and [M10-08 source/hash ledger](M10_08_FINAL_MEDIA_INTEGRATION_REPORT.md). The [M10-10 report](M10_10_PRODUCTION_RECORDS_REPORT.md) records catalog contents and unlock triggers.

## 5. Technical verification history

These are milestone results, not new M10-14 test outcomes.

| Milestone | Historical verification evidence |
| --- | --- |
| M10-04 | Focused interaction/audio/presentation spikes; full EditMode suite 462 passed / 0 failed / 0 skipped. |
| M10-05 | S001–S003 integration; 470 passed / 0 failed / 0 skipped. |
| M10-06 | S004–S006 integration; 476 passed / 0 failed / 0 skipped. |
| M10-08 | Media catalog checks; full EditMode suite 493 passed / 0 failed / 0 skipped. |
| M10-09 | Exact-line integration and isolated production Save/Load observations; 505 passed / 0 failed / 0 skipped. |
| M10-10 | Populated Records, unlock reconciliation and Replay isolation; 508 passed / 0 failed / 0 skipped. |
| M10-11 | Unity wiring audit; 517 passed / 0 failed / 0 skipped. |
| M10-12 | Final technical validation; 518 passed / 0 failed / 0 skipped; Windows Development and Release builds succeeded with zero build errors. |

The M10-12 Release Player recorded 486 build warnings: 460 ConvGeneric shader-variant warnings, 25 Hidden/Sentis shader warnings, and one Pipeline-package warning. Its standalone shutdown also logged an unattributed ComputeBuffer disposal warning. The Player and project-path observations are detailed in the [M10-12 report](M10_12_FINAL_TECHNICAL_VALIDATION_REPORT.md); the unresolved warning and profiling work remain listed below.

## 6. M10-13 acceptance and Windows QA Player

**`PROTOTYPE_PLAY_GATE_PASS` — accepted 2026-11-09.** The user reported completing the S001–S008 QA prologue, checking Save/Load and restart persistence, general physical interaction, and the overall visual/audio experience, with no reported problems. The report does not provide scene-specific ratings or establish that every key combination, Auto/Skip combination, Replay cancellation, or negative Save/Load case was individually tested. No commercial distribution approval was given.

The isolated Windows QA executable is recorded at `C:/Dev/project_all_time_playtest/M10_13/Build/AllTime_M10_13_PlayGate.exe`, SHA-256 `48EFAB523AA684C653BD1254A6962D3410127B5C02DC1310F6F16F4810666556`. Its M10-13 BuildReport succeeded with zero errors and 486 warnings. Its SaveData and MetaProgress identity was isolated under `ProjectAllTimeQA/AllTime_M10_13_PlayGate`; no personal save data is part of the repository or this change.

The acceptance boundary is the QA prototype with the current media. It does not mean the placeholder Rope, silent effects, expression fallbacks, every imported candidate file, or source rights were individually approved as final.

## 7. Exact-line Save/Load contract

- Production saves use schema v2 and `saveMode=exact_line`, storing the current unconsumed runtime `line:` ID, its approved `PROLOGUE_LINE_Sxxx_nnn` resume node, compiled-program/resume-manifest SHA-256 fingerprint, validated Yarn variables, presentation, audio, slot metadata, and play time.
- The generated manifest maps all 379 production paragraphs to the original authored beat and successor. The source's pre-line commands remain in their original flow and are not replayed by entering a resume node.
- New graph fingerprints use canonical LF manifest bytes. The current compatibility path recognizes the validated LF and CRLF fingerprints recorded by M10-10; incompatible fingerprints, invalid ID/node pairs, and unsupported states are rejected before mutating the runtime. There is no automatic schema-v1 to v2 migration.
- Load owns the interaction while it stops the old runner, waits for the authoritative `LinePresenter` and tracked Yarn/presentation/audio continuations to quiesce, restores stable state, and starts the approved exact line. The next normal advance follows the original graph successor.
- Stable presentation and BGM state restore from validated snapshots. Scene-owned passive information and medical-diagram state are reconstructed from the resume manifest. Transient effects and interaction leases are normalized. The 12 intentionally restricted occurrences remain unsaveable while their authored transient condition is active.
- M8 durable `ReadHistory` remains independent of production save slots. Backlog remains session-only.

For future graph changes, maintain the stable line IDs, run `python AgentScripts/m10_09_build_resume_graph.py`, review the generated manifest, update compiled fingerprint compatibility deliberately, and add migration only as a separately specified schema change. Preserve the separate legacy-v1 path.

## 8. Records and Replay contract

M8 `ReadHistory` is the durable progress source. The M10-10 Records unlock reconciler projects committed read IDs into the existing Timeline, Gallery and Archive sets; Achievement conditions use the existing evaluator. The catalogs contain 8 Timeline entries, 6 Gallery entries, 5 Archive entries, 4 Achievements and 12 unlock rules. No separate persistent Records store is introduced.

Replay compiles in the separate Replay Yarn project and uses its own runner, presenter and presentation hierarchy. It contains the two authored excerpts `REPLAY_S001_MORNING` and `REPLAY_S002_PHOTO`. It is a short reading experience, not a production save/resume route; its isolation preserves production variables, presentation, Backlog, SaveData and MetaProgress.

See the [M10-10 report](M10_10_PRODUCTION_RECORDS_REPORT.md) and current [technical architecture](../04_TECHNICAL_ARCHITECTURE.md) for catalog ownership and the Replay allowlist.

## 9. Asset identity and current media status

The global identity counts remain 9 Characters, 12 BG, 6 CG, 2 BGM and 14 SFX (43 total). The 19 expression keys are scoped children of character definitions. M10-08 technically assigned candidate media to 37 global IDs, with 6 still on technical placeholders. Keep the statuses distinct: `IDENTITY_FROZEN` means a stable lookup key; `USABLE_RUNTIME_MEDIA` means a catalog reference resolves; `TECHNICAL_PLACEHOLDER` means a temporary marker or silence fallback; `FINAL_USER_APPROVED_MEDIA` requires item-level approval evidence; and verified distribution rights require source-specific evidence. No item-level final approval or blanket rights clearance is claimed. “Integrated” describes a runtime reference only.

### Outstanding global resources and composition review

The action owner is stated as a role because no individual delivery owner is identified in the source ledger. `PROTOTYPE_ACCEPTED` means the overall QA build passed; it does not mean item-level final media approval.

| Stable ID | Category | Current runtime asset | Scene / beat | Gap and required delivery | Priority | Integration owner | Source / provenance | QA prototype acceptance | Commercial release |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `rope` | Character body | `Assets/_Project/Art/TemporaryM10/TEMPORARY_cyber_dog_marker.png` — `TECHNICAL_PLACEHOLDER` | S006-P07, P12; S007-P01; S008-P01, P10, P12 | Distinct mechanical Rope body; calm default and alert representation, visible joints only after reveal. Preserve the same character identity and keep it distinct from `rope_memory`. | P0 | Content producer → Unity asset integrator | `PROVENANCE_UNVERIFIED`; no final source delivered. | `PROTOTYPE_ACCEPTED`; this marker was not separately approved. | `FINAL_MEDIA_REQUIRED` |
| `sfx_plate_counter` | SFX | `Assets/_Project/Audio/TEMPORARY_M10_Silence.wav` — `TECHNICAL_PLACEHOLDER` | S001-P01 | Finite, single plate-on-counter cue matching the authored action. | P2 | Audio producer → Unity audio integrator | `PROVENANCE_UNVERIFIED`; no suitable source found in the reviewed inventory. | `PROTOTYPE_ACCEPTED`; silence was not approved as final. | `FINAL_MEDIA_REQUIRED` |
| `sfx_child_entry_steps` | SFX | `Assets/_Project/Audio/TEMPORARY_M10_Silence.wav` — `TECHNICAL_PLACEHOLDER` | S001-P06 | Short child footsteps preceding the father rising; not a generic loop. | P2 | Audio producer → Unity audio integrator | `PROVENANCE_UNVERIFIED`; no suitable source found in the reviewed inventory. | `PROTOTYPE_ACCEPTED`; silence was not approved as final. | `FINAL_MEDIA_REQUIRED` |
| `sfx_sink_washing` | SFX | `Assets/_Project/Audio/TEMPORARY_M10_Silence.wav` — `TECHNICAL_PLACEHOLDER` | S005-P14 | Finite ordinary sink/washing action, timed to the authored consent beat. | P2 | Audio producer → Unity audio integrator | `PROVENANCE_UNVERIFIED`; no suitable source found in the reviewed inventory. | `PROTOTYPE_ACCEPTED`; silence was not approved as final. | `FINAL_MEDIA_REQUIRED` |
| `sfx_slipper_toe_tap` | SFX | `Assets/_Project/Audio/TEMPORARY_M10_Silence.wav` — `TECHNICAL_PLACEHOLDER` | S006-P01, exactly twice | Two quiet slipper toe taps, not footsteps or ominous knocks. | P2 | Audio producer → Unity audio integrator | `PROVENANCE_UNVERIFIED`; no suitable source found in the reviewed inventory. | `PROTOTYPE_ACCEPTED`; silence was not approved as final. | `FINAL_MEDIA_REQUIRED` |
| `sfx_raindrop_awning` | SFX | `Assets/_Project/Audio/TEMPORARY_M10_Silence.wav` — `TECHNICAL_PLACEHOLDER` | S008-P12 | One isolated awning drop. The supplied 12.714-second candidate nearly duplicates a rain-splash recording and is not a verified drop; do not substitute it as-is. | P1 | Audio producer → Unity audio integrator | `PROVENANCE_UNVERIFIED`; candidate exists, but exact cue/source provenance and suitability are not established. | `PROTOTYPE_ACCEPTED`; silence was not approved as final. | `FINAL_MEDIA_REQUIRED` |
| `cg_present_table_glimpse` | Composition review (runtime CG exists) | `Assets/_Project/Art/CG/cg_present_table_glimpse.png` — usable runtime candidate | S006 brief present-table glimpse | Existing runtime reference works and the one-second wide empty dining/kitchen view does not isolate a chair. Its camera differs from the authored same-camera brief; review framing as a future refinement, not a broken reference or reported user rejection. | P1 | Content/art lead → Unity asset integrator | `PROVENANCE_UNVERIFIED`; existing user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no individual composition approval recorded. | `FINAL_APPROVAL_PENDING` |

### Expression delivery register — 19 scoped keys

All listed definitions currently have `headSprite = null`; the renderer falls back to that character's current body Sprite. The nine default keys do not create a requirement for nine additional default-head files. Ten non-default expressive keys lack separate expressive artwork. Rope's body replacement is the separate `rope` gap above. Delivery should preserve transparency, align the Head layer/eye line, and match current character scale. Priorities follow the frozen expression requirements; `BASE` means the existing body is the default fallback, not a requested separate head.

| Character / scoped key | Current body Sprite and fallback | Narrative use; actual missing artwork | Priority | Integration owner | Source / provenance | QA prototype acceptance | Commercial release |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `jihun/default` | `Assets/_Project/Art/Characters/jihun/jihun_default.png` | S001–S008; body fallback is the intended default; no separate default head required. | BASE | Content/source owner → Unity asset integrator; release owner verifies rights. | `PROVENANCE_UNVERIFIED`; user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no item approval recorded. | `FINAL_APPROVAL_PENDING` |
| `jihun/uneasy` | `Assets/_Project/Art/Characters/jihun/jihun_default.png` | Requirement-linked beats S002-P08/P09/P10, S003-P03, S006-P03, S007-P03/P07; aligned unsettled eyes/brows. | P1 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `jinhee/default` | `Assets/_Project/Art/Characters/jinhee/jinhee_default.png` | S001–S006; body fallback is the intended default; no separate default head required. | BASE | Content/source owner → Unity asset integrator; release owner verifies rights. | `PROVENANCE_UNVERIFIED`; user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no item approval recorded. | `FINAL_APPROVAL_PENDING` |
| `jinhee/smile` | `Assets/_Project/Art/Characters/jinhee/jinhee_default.png` | Requirement-linked beats S001-P02/P03/P12, S002-P02, S005-P09; warm smile preserving her existing identity. | P1 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `jinhee/uneasy` | `Assets/_Project/Art/Characters/jinhee/jinhee_default.png` | Requirement-linked beats S001-P10, S002-P03/P04/P07, S004-P02/P03/P05; dawning unease in eyes/brows. | P1 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `jinhee/crying` | `Assets/_Project/Art/Characters/jinhee/jinhee_default.png` | Requirement-linked beats S004-P13/P14/P15, S005-P02/P03/P06/P09/P10/P12/P14; restrained tears. | P1 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `jinhee/firm` | `Assets/_Project/Art/Characters/jinhee/jinhee_default.png` | Requirement-linked beats S004-P13/P14/P15, S005-P02/P03/P06/P09/P10/P12/P14; self-directed resolve while retaining tears; do not imply recovery. | P1 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `minseok/default` | `Assets/_Project/Art/Characters/minseok/minseok_default.png` | S001–S005; body fallback is the intended default; no separate default head required. | BASE | Content/source owner → Unity asset integrator; release owner verifies rights. | `PROVENANCE_UNVERIFIED`; user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no item approval recorded. | `FINAL_APPROVAL_PENDING` |
| `minseok/quiet` | `Assets/_Project/Art/Characters/minseok/minseok_default.png` | Requirement-linked beats S001-P04, S002-P05, S003-P01, S004-P06, S005-P09; restrained, attentive expression without menace. | P2 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `narae/default` | `Assets/_Project/Art/Characters/narae/narae_default.png` | S001-P02/P07/P13, S002-P01, S005-P01/P06/P08/P11; current body is the only scoped key and default fallback; no separate default head required. | P2 / base integrated | Content/source owner → Unity asset integrator; release owner verifies rights. | `PROVENANCE_UNVERIFIED`; user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no item approval recorded. | `FINAL_APPROVAL_PENDING` |
| `junho/default` | `Assets/_Project/Art/Characters/junho/junho_default.png` | S001–S005; body fallback is the intended default; no separate default head required. | BASE | Content/source owner → Unity asset integrator; release owner verifies rights. | `PROVENANCE_UNVERIFIED`; user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no item approval recorded. | `FINAL_APPROVAL_PENDING` |
| `junho/pout` | `Assets/_Project/Art/Characters/junho/junho_default.png` | Requirement-linked beats S001-P02/P04, S002-P06, S004-P03; small breakfast pout. | P2 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `junho/eager` | `Assets/_Project/Art/Characters/junho/junho_default.png` | Requirement-linked beats S001-P02/P04, S002-P06, S004-P03; restrained shoe/arrival anticipation. | P2 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `rope_memory/default` | `Assets/_Project/Art/Characters/rope_memory/rope_memory_default.png` | S001–S006 before awakening; ordinary dog body fallback; no separate default head required. | BASE | Content/source owner → Unity asset integrator; release owner verifies rights. | `PROVENANCE_UNVERIFIED`; user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no item approval recorded. | `FINAL_APPROVAL_PENDING` |
| `rope_memory/alert` | `Assets/_Project/Art/Characters/rope_memory/rope_memory_default.png` | Frozen REQ-EXP-009 references S002-P10, S003-P04 and S006-P07. Authored Yarn enters `alert` at S003-P01 and re-stages it at the S006 entrance; create an attentive ordinary-dog head with no mechanical hints. | P2 | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no expression art delivered. | `PROTOTYPE_ACCEPTED`; fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `rope/default` | `Assets/_Project/Art/TemporaryM10/TEMPORARY_cyber_dog_marker.png` | S006 after L924, S007–S008; no separate default head required, but the marker body must be replaced by the mechanical Rope delivery above. | P0 body / base | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no final body or expression source delivered. | `PROTOTYPE_ACCEPTED`; marker not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `rope/alert` | `Assets/_Project/Art/TemporaryM10/TEMPORARY_cyber_dog_marker.png` | S006-P07, S007-P01/P05/P07, S008-P01/P10/P12; after the actual Rope reveal, deliver the mechanical body and aligned expressive alert head. | P0 body / P2 expression | Character-art supplier → Unity asset integrator. | `PROVENANCE_UNVERIFIED`; no final body or expression source delivered. | `PROTOTYPE_ACCEPTED`; marker fallback not individually approved. | `FINAL_MEDIA_REQUIRED` |
| `dokyung/default` | `Assets/_Project/Art/Characters/dokyung/dokyung_default.png` | S007-P01/P02/P05/P06/P07/P08/P09/P10; current neutral body is the default fallback; no separate default head required. Final clinician-body styling remains a visual review item in the manifest. | P2 / body review | Content/source owner → Unity asset integrator; release owner verifies rights. | `PROVENANCE_UNVERIFIED`; user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no item-level art approval recorded. | `FINAL_APPROVAL_PENDING` |
| `eunjung/default` | `Assets/_Project/Art/Characters/eunjung/eunjung_default.png` | S008-P01/P02/P07/P11/P12; current neutral body is the default fallback; no separate default head required. Final guide-body styling remains a visual review item in the manifest. | P2 / body review | Content/source owner → Unity asset integrator; release owner verifies rights. | `PROVENANCE_UNVERIFIED`; user-supplied candidate. | `PROTOTYPE_ACCEPTED`; no item-level art approval recorded. | `FINAL_APPROVAL_PENDING` |

The M10-08 [expression-art delivery register](M10_08_POST_MERGE_HARDENING_REPORT.md) is the detailed prior evidence. This reconciliation does not turn the nine default body fallbacks into separate head-file requests.

## 10. Provenance and rights readiness register

M10-08 preserves per-file source/import hashes and the processing record for the derived clip. Hash equality and possession establish file identity, not the right to distribute the files. The open evidence below is an unresolved status, not a finding that a license was violated.

| Material | Evidence present | Missing evidence / required action | Release impact | Responsible follow-up |
| --- | --- | --- | --- | --- |
| User-supplied artwork (26 PNG source files and imported copies) | M10-08 source/import paths and SHA-256 ledger; direct copies were hash-matched. | Identify the human creator or generation service for each final file; retain original/export history and written commercial-use, modification and game-distribution rights; collect any required credit text. | Do not include an item in a public release without sufficient provenance and rights evidence. | Content/source owner supplies records; release owner records review and attribution. |
| User-created BGM candidates | Source/import hashes, formats, lengths, catalog assignment and technical playback ownership are documented. | Record creator and generation service/account/export history, terms applicable at generation, permission for commercial game distribution and required credits. Human audition and final mix approval are separate. | Candidate status only until rights and content acceptance are documented. | Music creator/source owner and release owner. |
| User-supplied direct SFX candidates | Source/import hashes and technical catalog assignments are recorded in M10-08. | Record per-file creator/source, applicable service terms, commercial distribution and modification rights, and required attribution; perform final acoustic approval. | Candidate status only until evidence and approval are complete. | Audio source owner and release owner. |
| Derived `sfx_connection_rain_handoff` | Source hashes, output hash and exact processing parameters are recorded in [M10-08](M10_08_POST_MERGE_HARDENING_REPORT.md). | Establish commercial and derivative-use permission for both components, including the exact rain source; retain source receipts and any required attribution. | Derived output inherits unresolved rights from source components. | Audio integrator gathers evidence; release owner reviews both components. |
| Mixkit-labeled rain component | M10-08 records the candidate path/hash and the published Mixkit sound-effects license findings, including permitted incorporation and restrictions. | Obtain a source-specific receipt or stable item record tying the exact local WAV to the listed work and license class. Filename/category similarity alone is not proof of origin. | Keep source provenance unresolved; do not claim this exact local file is cleared. | Audio source owner retrieves source record; release owner verifies license scope and attribution. |
| Korean font `Assets/_Project/Fonts/NotoSansKR/NotoSansKR-Regular.ttf` | SHA-256 `7715af52f5fe77153ce5678546258993982d2da61abea8d25fb89eb5aaec5ca6`; internal name/version and Noto CJK SIL OFL 1.1 references are documented. Technical Korean glyph fallback passed M10-12 checks. | Reconcile filename/import label `Regular` with internal PostScript name `NotoSansCJKkr-Thin`; confirm exact file identity and applicable release/license notice, reserved-name conditions, and bundled attribution. | Technical glyph rendering does not close font provenance or release packaging. | Font/source owner and release owner. |
| Attribution package | Existing reports identify where source-specific credit may be required. | Prepare final credits/third-party notices from verified licenses; no missing receipts, creator histories or licenses should be reconstructed by assumption. | Public distribution remains pending until notices are accurate. | Release owner after source owners deliver evidence. |

## 11. Technical risk register

| Risk | Evidence and current status | Classification | Follow-up |
| --- | --- | --- | --- |
| 486 Windows Release build warnings | M10-12 grouped 460 ConvGeneric shader-variant, 25 Hidden/Sentis shader, and 1 Pipeline-package warning. The build succeeded with zero errors. This does not classify all warnings as application defects. | `KNOWN_NONBLOCKING` | Reassess only if a target pipeline/platform change or user-visible defect makes them relevant. |
| ComputeBuffer disposal warning at Player shutdown | Repeated in M10-12 Player logs; native owner has no stack attribution. No project C# ComputeBuffer allocation was found, but harmlessness and leak-freedom were not proven. | `REQUIRES_INVESTIGATION` | Attribute the owner on a suitable Unity/graphics diagnostic run before making a safety claim. No shader rewrite is part of M10-14. |
| Long-duration memory profiling | No soak, resident-memory, allocation or frame-time profiler capture was available in M10-12. Clean exit is not evidence of leak-free operation. | `FUTURE_VALIDATION` | Profile a representative release candidate on a chosen target configuration. |
| 2560×1440 resolution | The M10-12 connected display exposed modes only through 1920×1080; the higher mode could not be selected. | `FUTURE_VALIDATION` | Recheck when a suitable display or test environment is available; no result is inferred for this mode. |
| Unexercised negative Save/Load cases | Existing automated regression and earlier isolated-file evidence cover specific malformed/incompatible cases. M10-12 did not newly inject OS write denial, every invalid catalog ID, or every sidecar permutation through a live Player transaction. | `FUTURE_VALIDATION` | Add targeted cases if release requirements demand them; do not infer blanket coverage. |
| Specific physical-control combinations | The user accepted general physical interaction and the complete play experience. The report does not enumerate every key combination, Auto/Skip combination, or Replay cancellation. | `FUTURE_VALIDATION` | Use the existing playtest guide if those specific interactions require independent release evidence. |
| Static-font glyph gaps in tested Records corpus | M10-12 found three missing code points in the static font; configured NotoSansKR dynamic fallback resolved the tested corpus and its Editor test passed. This closes the tested rendering case only. | `CLOSED` | Keep the separate font identity/license follow-up in the provenance register. |

## 12. Documentation reconciliation and integrity checks

Current authoritative summaries are updated in [implementation state](../03_IMPLEMENTATION_STATE.md), [technical architecture](../04_TECHNICAL_ARCHITECTURE.md), [prologue vertical slice](../05_PROLOGUE_VERTICAL_SLICE.md), and [asset manifest](../07_ASSET_MANIFEST.md). The M9 architecture text now labels the formerly empty production catalogs as an M8/M9 boundary superseded by M10-10. The M10-08 through M10-13 reports keep their original milestone evidence and link to later completion/closure records where earlier statuses have been superseded.

M10-14 integrity review:

- The production source Yarn line-ID set and all 379 manifest entries match; the canonical story, production Yarn, and resume manifest match the merged baseline.
- The asset manifest contains 43 global IDs in the expected 9/12/6/2/14 categories, with 37 `INTEGRATED` and 6 `IN_PROGRESS` placeholders. Each listed runtime asset path exists in the repository.
- Frozen story source, production Yarn, resume manifest, catalogs, Scene wiring, settings and user data are not changed by this documentation milestone.
- The M10-12 `518 / 0 / 0` EditMode result, Windows builds and player diagnostics are cited as historical evidence. No new full test run or Windows build is claimed.
- No item-level media approval, commercial license, source receipt or attribution is invented.

The documentation diff passes `git diff --check`; the local Markdown link scan reports no broken targets; and the branch change list contains only the 17 intended Markdown files. The final commit and review PR head are recorded below after publication. No Unity suite or Player build was rerun because this milestone changes documentation only; M10-12 results remain historical evidence.

## 13. Future engineering and content handoff

### Story and Yarn

- Treat [`ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt`](../Story/ALL_TIME_PROLOGUE_S001_S008_SOURCE_KO.txt) as the frozen M10 narrative source. Review any future canonical story change against that source before changing Yarn.
- Preserve explicit stable `#line:` IDs for existing content. New scenes need an approved scene node and beat mapping in [`M10_PROLOGUE_PRESENTATION_MAP.md`](M10_PROLOGUE_PRESENTATION_MAP.md), then ordered production Yarn nodes. Keep narrative, stage directions, and non-player-visible notes distinct.
- Do not renumber IDs to fix presentation, tests, or read-history counts. Add future scene nodes after canonical content review; do not begin them as part of M10 closure.

### Exact-line Save/Load

- Regenerate and review `Assets/_Project/Resources/M10_09_LINE_RESUME_MANIFEST.json` with `AgentScripts/m10_09_build_resume_graph.py` after production Yarn graph edits. Verify each resume line and successor, eligibility, and passive panel context.
- Any compiled-program or manifest change can alter the v2 fingerprint and invalidate existing v2 saves. Define and test compatibility or migration explicitly; there is no automatic v1-to-v2 migration. Preserve the separate legacy schema-v1 checkpoint path and protected-file behavior.
- Keep `M2_UI_START` as the serialized technical scene start unless a separately reviewed implementation milestone changes it.

### Presentation assets and audio

- Keep all 43 global resource IDs frozen. Supply art/audio against existing catalog IDs and record source/import hashes, import settings, author, source and rights evidence before marking a delivery ready.
- Expression IDs remain scoped to a character. The default body is the fallback; request separate Head artwork for actual expressive distinctions, not for every default key. Preserve `rope_memory` / `rope` reveal ownership and six-slot staging.
- Reuse the existing presentation/audio catalogs, `VNAudioController`, mixer and Yarn command contracts. Await BGM transitions and authored durations. Keep finite one-shot SFX distinct from continuous BGM; do not add an ambient subsystem for the current finite handoff.

### Records and Replay

- Keep M8 durable `ReadHistory` as the source for Records unlock reconciliation. Maintain catalog entries and unlock line IDs as authored content changes; avoid a second progress store.
- Keep Replay content in its separate Yarn project and use the Replay-owned runtime. Validate only explicitly approved roots and the existing immediate visual-command allowlist.

### Validation and handoff

- Re-run the focused Editor audits after Scene, catalog, Yarn or serialized-reference changes; use the full EditMode suite for implementation regressions.
- Validate production behavior through the wired `VN_Main` scene and actual Player builds. Use isolated `Application.persistentDataPath` identities for QA SaveData/MetaProgress and never include player logs, builds, or save files in source commits.
- Use the existing Korean playtest guide for future user acceptance. Record exactly what the user reports; do not infer detailed control combinations from an overall pass.

## 14. Final M10 disposition and public-release conditions

**M10 PROLOGUE VERTICAL SLICE — IMPLEMENTATION AND USER QA COMPLETE**

**`PROTOTYPE_PLAY_GATE_PASS`**

**`COMMERCIAL_RELEASE_CLEARANCE_PENDING`**

The prototype milestone can close with a passed overall user Play Gate while retaining known media and engineering follow-up. Before public distribution, complete or explicitly approve final media substitutions, decide the table-glimpse composition, verify per-file provenance and applicable commercial-use rights, resolve the font identity and notice, and review the technical follow-ups against the target platform. No claim is made that the game or future story chapters are complete.

M10-14 changes are limited to implementation-state and architecture summaries, the current vertical-slice status, asset manifest status, point-in-time report annotations, and this closure report. The final M10-14 branch/commit and PR URL are recorded in the review delivery metadata; this report is not a merge declaration.
