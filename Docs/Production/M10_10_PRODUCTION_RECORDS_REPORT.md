# M10-10 Production Records — investigation in progress

This report is still in progress; the remaining validation listed below must be completed before review readiness is claimed.

## Execution checklist

- [x] Populate and wire the four existing production catalogs and M8 unlock mapping.
- [x] Compile and validate the separate Replay project and wire two excerpts.
- [x] Run a connected-Editor full EditMode regression after the Editor-only isolation seam was added: 508 passed, 0 failed, 0 skipped.
- [x] Run isolated live S001–S008 progression, partial Records, Replay cancellation, Gallery viewer, and LF/CRLF fingerprint loads.
- [x] Inspect clean-progress Records UI in a fresh isolated root.
- [ ] Complete historical pre-change fixture provenance and remaining UI/physical-input and load-regression coverage before acceptance.

## Baseline and compatibility finding

The verified base is `a95569b3c13e47a5765b6ff8b761bb43d4b5798f` (merged M10-09). Before changing production content, the connected Unity 6000.3.21f1 Editor reported compiled Yarn program SHA-256 `f084b94f29cedfbe2897857158fd717d3dedb0ae9afe9d0025dbe9b861ae75d5`.

The initially cached manifest bytes had SHA-256 `79e93aeebbbeee0d6fafd194fa03af9393885cd90dcbbe4fc670192006494ab9` and yielded graph fingerprint `9aefad0924fffae848c25a23081b48606a0cfd2c51d637bb6da4386a65883980`. After the Editor refreshed its imported assets, the program hash was unchanged, but the manifest bytes had SHA-256 `d034212dfd07d9b4027734581f902bc56f2f7f76bfe7bb54ffa55e7979cd8f75` and yielded fingerprint `cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9`. The latter matches the LF file on disk. Converting that file's LF characters to CRLF produces the initial manifest hash. No tracked Yarn or manifest change occurred during the investigation.

This shows that a save created with the cached CRLF manifest fingerprint would fail the original exact fingerprint comparison after the LF import, even though the program bytes and manifest text are equivalent. The investigation has not established whether any particular user save has that fingerprint.

The implementation now decodes the current manifest as strict UTF-8, normalizes CRLF to LF and writes the fingerprint over the program bytes and canonical LF manifest bytes. Load validation first validates the current manifest and line/node pair, then accepts only a fingerprint computed from the current program and either the canonical LF representation or its CRLF representation. Unknown fingerprints and fingerprints derived from altered program or manifest bytes are rejected. Saves are not rewritten. The canonical LF fingerprint remains `cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9`.

The isolated coordinator/repository test composes a production-line v2 save and validates copies with both recorded historical fingerprints. It also checks altered-program, altered-manifest and unknown fingerprints, malformed save JSON and an incorrect resume node are rejected before the session line changes. This is a coordinator validation harness, not a live save restoration or a pre-change save captured from gameplay; presentation, BGM and successor restoration still require the requested live compatibility tests. The full `VNFullSnapshotTests` fixture passed 12/12.

`M10ProductionResumeGraphTests` passed after the fresh import: one test checked the 379 mappings, 367 eligible entries and the freshly imported LF fingerprint. Unity reported zero compilation errors. The production scene now references the authored mapping, and a production-asset test checks partial progress and persistence.

## Authored content and triggers

The existing catalog GUIDs remain in use. The Timeline contains chapter `prologue_part_01` and `prologue_s001` through `prologue_s008`, in canonical scene order. Each uses its first and final canonical paragraph as discovery and completion milestones. The live run consumed `line:prologue_s008_041` before the part title; this is a scene milestone, not a game ending.

| Timeline ID | Canonical title | Discovery → completion |
| --- | --- | --- |
| `prologue_s001` | 다섯 사람이 먹는 아침 | `line:prologue_s001_001` → `line:prologue_s001_042` |
| `prologue_s002` | 돌아온 다음의 일이 없는 여행 | `line:prologue_s002_001` → `line:prologue_s002_042` |
| `prologue_s003` | 강아지가 부르는 이름 | `line:prologue_s003_001` → `line:prologue_s003_027` |
| `prologue_s004` | 새 신발이 있는 방 | `line:prologue_s004_001` → `line:prologue_s004_057` |
| `prologue_s005` | 성공한 작별 | `line:prologue_s005_001` → `line:prologue_s005_069` |
| `prologue_s006` | 흰 문, 닫힌 진료 | `line:prologue_s006_001` → `line:prologue_s006_046` |
| `prologue_s007` | 삶을 묻는 검사 | `line:prologue_s007_001` → `line:prologue_s007_055` |
| `prologue_s008` | 가장 편할 것 같은 여자 | `line:prologue_s008_001` → `line:prologue_s008_041` |

The six Gallery entries and their committed-read triggers are: `cg_worn_child_shoe` → S001-034; `cg_beach_photo_child` → S002-009; `cg_beach_photo_sea` → S002-025; `cg_returned_bag_contents` → S004-037; `cg_unused_new_shoe` → S004-049; `cg_present_table_glimpse` → S006-035. The last trigger follows the brief visual insert. Chapter `prologue_part_01` unlocks on S001-001. Archive entries `rope`, `awakening_exhaustion`, `era_immersion`, `companions`, and `session_conditions` unlock on S006-016, S007-028, S007-040, S008-006, and S008-028 respectively. The mapping asset records the scene and justification for each rule.

The four evaluator-owned achievements are `first_steps` (chapter unlocked), `first_memory` (one CG), `notes_gathered` (three Archive entries), and secret `all_memories` (all CGs). No ending ID is awarded. Reconciliation subscribes to committed M8 read changes and catches up only from persisted read IDs. Failed unlock writes are surfaced; duplicate reads and catch-up are idempotent.

`PrologueReplay.yarnproject` is outside the production Yarn source scope. `REPLAY_S001_MORNING` and `REPLAY_S002_PHOTO` reproduce canonical short passages with separate line IDs and approved immediate visual commands. Production lines and Replay nodes are validated against their respective projects; M9 fixtures retain the single-project default.

## Live observations and remaining evidence

Unity 6000.3.21f1 ran `VN_Main` with Editor-only temporary SaveData and MetaProgress roots. Production input-bridge events advanced all 379 paragraphs; profile cards used button events. At S001-035, the Timeline showed the first scene discovered, the shoe CG was unlocked, and five later Gallery entries were locked. Replay S001 started, advanced, cancelled, and left the production line and read count unchanged; a later replay completed without an orphaned session. The Gallery fullscreen viewer opened a resolved production sprite and closed. Archive and Achievement tabs displayed their earned data. After S008, all six CGs, five Archive entries and four achievements were durable, with zero completed endings; a Play restart reloaded those counts.

Two isolated files derived from the live S005-048 autosave were assigned the recorded LF and CRLF historical hashes. Both loaded through `VNSaveLoadController` to S005-048 with `bgm_quiet_farewell`, and the LF load advanced to S005-049. Variables and presentation matched the CRLF fixture after load, and later M8 progress remained intact. These files were constructed after integration, not preserved from an earlier M10-09 gameplay session. This is not a substitute for all requested pre-change historical fixture scenarios. No physical keyboard or mouse acceptance is claimed.

In a further isolated restart, current-graph autosaves also loaded to S007-001 and S008-017 through the production controller; the latter accepted a production input-bridge advance. These were not the requested S007 information-overlay fixture and do not establish historical-file provenance.

A separate fresh isolated run showed an empty Timeline, six locked Gallery slots, hidden Archive bodies, and a secret achievement displayed as `???`; no CG, Archive or achievement unlocks were present. The scoped live run produced no new captured runtime errors; existing console history includes expected negative-test logs and Unity CLI/Pipeline command-name errors. The first full suite had one failure because the historical M9 audit required empty catalogs; the audit was updated to require the populated assets and their separated Replay project, and its seven tests passed. Remaining gaps include the full matrix of S001/S007/S008 historical save restorations and complete modal arbitration and physical-input checks. No M10-13 acceptance is claimed.
