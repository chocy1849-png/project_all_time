# M10-00 — Local VN Reference Audit

## 1. Audit Scope

- **Date:** 2026-10-01, Asia/Seoul.
- **Repository:** `C:\Dev\project_all_time`.
- **Base:** clean `main`, `2236ea0dffe9b46bcef32758b84bb2cbb250e05b`; fetched `origin/main` matches. This is the merge commit of PR #45. The initial checkout was the clean M9 fix branch at `a11c28e`; main was fast-forwarded before creating the research branch.
- **Branch:** `research/m10-00-local-vn-reference-audit`.
- **Baseline tests:** 443 passed / 0 failed / 0 skipped is the supplied entering-M10 baseline, not a test run performed by this audit. Documentation-only work does not require a Unity run.
- **Story target:** user-authored `C:\Users\UserK\Downloads\message.txt`, 《우리가 만났던 모든 시대》, 1부 — 깨우는 사람, S001–S008. Read in place; not imported, moved, or rewritten. SHA-256: `8413E847DD66D7D45585847A935B9C384D91FACFE9724EE27084F3D86FF977E9`.

### Local installations

All three exact supplied directories and the executables listed below exist. Literal-path operations were used throughout.

| Reference | Exact install path | Executables / launcher |
| --- | --- | --- |
| Sani Yang's Laboratory | `D:\[한글 무설치]사니양연구실 V251110\Sani Yangs Laboratory [v20251110]\Sani Yang's Laboratory [v20251110]` | `sanyPlus.exe` (666,624 bytes); no separate game launcher seen |
| STEINS;GATE | `D:\[한글무설치] 슈타인즈 게이트 v23012018\STEINS GATE v23.02.2018\STEINS GATE v23.02.2018` | `Game.exe` (10,343,936 bytes); `Launcher.exe` (4,129,792 bytes) |
| WHITE ALBUM2 Extended Edition | `D:\[한글무설치] 화이트 앨범 2 Extended Edition 전개방 세이브 포함\화이트 앨범 2 Extended Edition` | `WA2.exe` (1,220,608 bytes); `WA2_KR.exe` (75,012,608 bytes). `Uninstall.exe` is not a game launcher |

### Evidence boundary and coverage

**This is a reviewable audit with incomplete player-facing evidence. No local game screen, gameplay interval, or audio was successfully observed.** Recommendations below are grounded in the script and current project contracts, with a small set of public feature confirmations. They must not be cited later as experimentally validated reference-game direction.

The Computer Use skill was initialized for normal local observation. A normal launch of `sanyPlus.exe` returned “launched app did not expose a targetable window.” The following window-refresh call reported that the user stopped Computer Use with the physical Escape key. Desktop interaction ceased immediately; no other title was launched by this audit. STEINS;GATE's requested 30–60-minute early sample and WHITE ALBUM2's emotional-scene sample were not obtained. Process/window state after the interruption was not verified.

Read-only evidence comprises top-level and shallow directory listings, file names/sizes/timestamps, plainly readable configuration files, script context, and project documentation/source. No executable or DLL was decompiled; `Assembly-CSharp.dll` was not inspected; proprietary archives, scenario files, saves, image/audio/font assets, and code-registration metadata were not opened or unpacked. No reference screenshot or asset is included in this repository. Ordinary launch can write its own local configuration/logs: Sani's `init.tss` and log timestamps were current during inventory; their earlier contents were not captured, so the exact launch-induced change is UNKNOWN. No save, setting edit, or overwrite was deliberately performed.

Evidence labels apply to **every reference finding**, including table cells:

| Label | Meaning |
| --- | --- |
| OBSERVED | Plainly visible local files/settings, or normal gameplay actually seen. This pass contains file/settings evidence only |
| INFERRED | A limited conclusion from an identified observation; not an internal implementation fact |
| PUBLIC | Primary developer/publisher/store documentation; edition applicability is stated |
| UNKNOWN | Unverified; absence of observation is not evidence that a feature is absent |
| USER_CAPTURE_NEEDED | UNKNOWN requiring the targeted evidence in section 9 |

`SCRIPT` identifies the user's source, `PROJECT` identifies inspected project contracts, and `RECOMMENDATION` identifies our design proposal. These are provenance markers for M10 decisions, not additional classes of reference-game evidence. An ADOPT/ADAPT/REJECT decision expresses suitability, not evidence strength.

Authority remains: **user script → project architecture/contracts → verified reference findings → general convention**. M10-01/M10-02 must reconcile and freeze later contracts. This document implements nothing and does not begin M10-01.

## 2. Executive Findings

**Sani Yang contributes a system-integration question, not a verified local layout.** PUBLIC product information confirms a story/simulation combination and laboratory management. This makes it a useful reference for returning from information or management UI to narrative context. Its local panel behavior, Records equivalents, masking, and navigation remain UNKNOWN.

**STEINS;GATE contributes a verified public principle of story-world interaction.** PUBLIC documentation confirms that everyday phone responses can affect the plot. M10 should ADAPT the contextual introduction of interaction, without implementing a phone or borrowing branching outcomes. Local first-person text distinctions, exposition pacing, TIPS, and anomaly/audio timing remain UNKNOWN.

**WHITE ALBUM2 contributes the emotional-direction study target and public convenience concepts.** An official console-edition page documents a message window, prior text/choice indicators, Auto, and Skip. That page does not verify their implementation in this Korean PC Extended Edition installation. Local rhythm, framing, CG timing, silence, and music persistence remain UNKNOWN.

The strongest M10 conclusions already follow from SCRIPT and PROJECT:

- **ADOPT:** ordinary household opening; SFX on black before BG/BGM; restrained contradictions with music continuity; a shared readable text area; script-directed silence; family presence through S005; existing convenience and checkpoint semantics.
- **ADAPT:** explicit text-channel hierarchy, compact information overlays, purposeful prop inserts, and profile browsing presented as reviewing information. Use script identity and story purpose rather than another title's typography, layout, or mechanics.
- **REJECT:** early dream/medical labels, horror filters, frequent stingers, family erasure, unearned CG emphasis, white-door cosmology hints, new exposition/lore, romance scoring as mind-reading, and false alternate-route choices.

Default framing is two or three active conversation sprites, with an intentional ensemble composition for the five-person breakfast and the still-present family. Default expressions change on meaningful authored beats. Neither default has been validated against the local references. Silence removes BGM, not automatically all household sound. S005's melody begins only after the meal has progressed; S006's anomaly requires an immediate BGM stop. S008 permits varied browse order but always returns to Jihun's authored Hana decision. No global Auto/Skip, Save/Load, Records, input, or persistence redesign is justified by the evidence gathered.

## 3. Sani Yang's Laboratory

**Role:** modern VN plus simulation/system integration and meta/progression UX.

### OBSERVED

- **SY-O1 — Installation/runtime clues:** `sanyPlus.exe`, `UnityPlayer.dll`, `UnityCrashHandler64.exe`, `MonoBleedingEdge`, and `sanyPlus_Data` are present. Executable timestamp: 2025-11-22. `sanyPlus_Data/app.info` plainly names `talesshop` and `sanyPlus`. These are visible Unity runtime clues, not a code audit or proof of an exact engine version.
- **SY-O2 — Configuration:** `sanyPlus_Data/boot.config` is readable; it contains runtime boot switches, including `hdr-display-enabled=0`, but no observed resolution setting. No game-facing resolution control was seen. `SteamData/user_stats.ini` exists (480 bytes); its contents were not inspected.
- **SY-O3 — Save/config candidate:** `C:\Users\UserK\AppData\LocalLow\talesshop\sanyPlus` exists with numbered `.tss` files and similarly numbered `.png` files, plus `data.tss`, `init.tss`, `time.tss`, and player logs. Examples: `0.tss` 7,410 bytes and `0.png` 123,333 bytes. Names/sizes only were inspected; no save/thumbnail content was read.
- **SY-O4 — Version/language clues:** directory labels state v20251110 and a Korean distribution. Actual in-game version/language was not verified.

### INFERRED

- **SY-I1:** SY-O1 plus the matching local application directory in SY-O3 makes that directory a likely save/config location. Numbered PNG/TSS pairs may be slot data and thumbnails; slot meanings, formats, and active ownership are UNKNOWN.
- **SY-I2:** the installation appears consistent with the requested title. The `sanyPlus` name is not proof that any particular DLC is enabled.

### PUBLIC

- **SY-P1:** the official Steam product description combines story/romance simulation with laboratory management and researcher assignments; it also lists Steam achievements. This confirms product domains, not the local transition, achievement-screen layout, or persistence implementation. [Official Steam product page](https://store.steampowered.com/app/3464370/Sani_Yangs_Laboratory/).

### UNKNOWN / USER_CAPTURE_NEEDED

| Target | Unverified detail | Capture |
| --- | --- | --- |
| Initial story | first BG, sprite placement, nameplate, textbox size/density, narration/thought state | C1 |
| Story → system → story | overlay versus replacement, progress/status panels, input ownership, return continuity | C2 |
| Meta/Records | existence and names of Timeline/history, Archive, collections, in-game achievements; locks, masking, navigation | C6 |
| Convenience | actual Auto/Skip/Backlog/Save/Load/Settings/Hide controls, visibility, behavior | C5 |
| Direction/audio | entrances/exits, expression rate, transitions, BGM changes, silence, SFX | C7 |

### ADOPT / ADAPT / REJECT

| Candidate technique | Reference basis | Decision and M10 reason |
| --- | --- | --- |
| Story and system information coexist in one product | PUBLIC SY-P1 | **ADAPT:** introduce M10 treatment information only when the script introduces it; reveal a clear return to dialogue. Specific overlay design comes from SCRIPT, not a locally observed Sani panel |
| Progress presentation has a consistent home | PUBLIC SY-P1 plus PROJECT M9 | **ADOPT the existing project authority:** keep Records and M8 durable unlock ownership. Sani's own Records organization remains UNKNOWN |
| Simulation loops, recruitment, or resource progression in the prologue | PUBLIC SY-P1 | **REJECT:** outside S001–S008; no management loop or new persistence schema |
| A particular textbox, overlay, quickbar, or animation copied from Sani | UNKNOWN | **REJECT copying; defer comparison:** no local visual evidence and no asset/layout reuse |

## 4. STEINS;GATE

**Role:** SF exposition, first-person internal narration, ordinary-life/anomaly contrast, and diegetic interaction. Target sample was the opening/early section; no full-game claim is made.

### OBSERVED

- **SG-O1 — Installation:** `Game.exe` and `Launcher.exe` exist with March 2018 timestamps. `bink2w32.dll` and `steam_api.dll` are visible runtime dependencies. These do not establish the proprietary engine architecture, which remains UNKNOWN.
- **SG-O2 — Plain configuration:** `ds.ini` identifies `AppName=STEINS;GATE` and `AppId=412830`. It has two `UserDataFolder` assignments: `.` and `%SystemDrive%\Users\%USERNAME%\AppData\Roaming\Steam\DARKSiDERs\412830`, plus `RemoteStorageFolder=Saves`. The latter directory does not exist under this user. Duplicate-key interpretation was not investigated.
- **SG-O3 — Language/version:** the directory labels claim a Korean v23.02.2018 distribution; `ds.ini` says `Language=japanese`. Neither proves the language actually rendered by the patched game. Launcher resolution/config UI was not observed.
- **SG-O4 — Archive boundary:** `USRDIR` includes named `.mpk` resources and `manual.mpk`; nothing was unpacked. Presence of `manual.mpk` does not make its contents publicly accessible.

### INFERRED

- **SG-I1:** SG-O2 is consistent with the requested title and supplies a possible save-root hint. Active save/config location is UNKNOWN because the candidate is absent and configuration precedence was not tested.

### PUBLIC

- **SG-P1:** the official Steam page describes Phone Trigger interaction through calls/messages, including responding or ignoring them, with consequences for story outcomes. [Official Steam product page](https://store.steampowered.com/app/412830/STEINSGATE/).
- The Steam-linked manual was discoverable, but page-image retrieval failed. It provides no verified control or layout finding in this audit.

### UNKNOWN / USER_CAPTURE_NEEDED

| Target | Unverified detail | Capture |
| --- | --- | --- |
| Text layers | spoken protagonist versus internal thought, narration, other speakers, nameplate and typography | C1 |
| SF exposition | unfamiliar-term introduction, box length, context before definition, TIPS behavior | C3 |
| Early anomaly | baseline, first abnormal event, effect order, black/cut, CG, BGM drop and SFX | C7 |
| Interaction | first local phone introduction, story interruption, diegetic cue, post-action feedback | C2 |
| Convenience | actual local control placement and behaviors | C5 |

### ADOPT / ADAPT / REJECT

| Candidate technique | Reference basis | Decision and M10 reason |
| --- | --- | --- |
| Introduce interaction through a recognizable story-world task | PUBLIC SG-P1 | **ADAPT:** Rope connection information and preparation-room profiles have authored reasons to appear; provide comprehensible feedback and return |
| Phone responses create plot branches | PUBLIC SG-P1 | **REJECT for M10:** no phone clone, hidden route branching, timed response puzzle, or additional choice outcomes |
| Preserve distinctions between thought, spoken dialogue, and channel communication | UNKNOWN local text comparison; SCRIPT distinction | **ADAPT:** candidate hierarchy in section 8 is script-driven; no claim that STEINS;GATE uses that hierarchy |
| A specific SF glossary, anomaly effect, or opening pacing | UNKNOWN | **REJECT importing an unverified convention:** use the script's simple diagram and minimal white-door treatment; local comparison remains pending |

## 5. WHITE ALBUM2

**Role:** dialogue rhythm, emotional scene direction, character framing, CG emphasis, and music/silence.

### OBSERVED

- **WA-O1 — Installation:** `WA2.exe` timestamp 2011-12-27, `WA2_KR.exe` timestamp 2021-10-09; `IC`, `patch_KR`, and `WHITE ALBUM2 Special Contents` directories are visible. Filename/timestamp evidence is consistent with a Korean-patched PC installation; patch provenance and exact build revision are UNKNOWN.
- **WA-O2 — Config/save location:** `C:\Users\UserK\Documents\Leaf\WA2_KR` contains `save_00.sav` (348,112 bytes), `Sys.sav` (2,528,384 bytes), `D3D.ini`, and `SYSTEM.ini`, timestamped 2026-08-19. Saves were not opened.
- **WA-O3 — Plain display settings:** `D3D.ini` has `window_mode=1`, `WIN_SIZE_W=1280`, and `WIN_SIZE_H=720`. These are stored values, not proof of a currently rendered 1280×720 window or the meaning of mode 1.
- **WA-O4 — Plain audio/text settings:** `SYSTEM.ini` has separate `bgm`, `se`, and `voice` keys and corresponding volume keys; it also contains `wait`, `msg_wait`, `auto_max`, window-alpha, and window-position keys. Units, slider mappings, ranges, menu locations, and live behavior are UNKNOWN. Do not derive pacing seconds from these values.
- **WA-O5 — Language/edition clues:** `patch_KR` contains named `.dat` files; the root contains a full-unlock save ZIP and proprietary `.pak` files. None was opened/imported. The save ZIP's presence does not prove active unlocks or normal spoiler-masking behavior. Exact Extended Edition content activation is UNKNOWN.

### INFERRED

- **WA-I1:** matching app-name directory, save files, and editable INIs in WA-O2 make it a likely active Korean-build save/config location; runtime ownership was not tested.
- **WA-I2:** WA-O4 suggests separate configurable sound categories. It does not verify mixer routing, UI scales, or emotional BGM direction.

### PUBLIC

- **WA-P1:** AQUAPLUS's official screen explanation describes the message window, an indicator for prior text/selected choices, Auto, and Skip. **Edition limitation:** this is the console title *WHITE ALBUM2 幸せの向こう側*, not a verified manual for this PC Korean Extended Edition. [Official screen explanation](https://aquaplus.jp/wa2/system02.html), [official PS3 product information](https://aquaplus.jp/wa2/product.html).
- Do not transfer that edition's Motion Portrait feature, controls, or assets to the local PC build.

### UNKNOWN / USER_CAPTURE_NEEDED

| Target | Unverified detail | Capture |
| --- | --- | --- |
| Text rhythm | typical box length, narration/monologue frequency, emotional thought splitting, advance spacing | C1, C4 |
| Character direction | two/three-person composition, entrances/exits, expression changes, movement | C4 |
| Insert/CG | trigger, relation to key line, sprites during CG, hold duration and exit | C4 |
| Audio | track starts/persistence/changes, silence, SFX density and ambient sound | C4 |
| Convenience | PC Auto/Skip indicators, Backlog/choice history, Save/Load and other controls | C5 |

### ADOPT / ADAPT / REJECT

| Candidate technique | Reference basis | Decision and M10 reason |
| --- | --- | --- |
| Make current Auto/Skip mode legible | PUBLIC WA-P1, with edition limitation; PROJECT existing indicators | **ADOPT retaining project indicators:** already supported; no console-style transplant |
| Emotional dialogue can be evaluated over several advances | UNKNOWN local rhythm; SCRIPT paragraph structure | **ADAPT as a script-driven rule:** one authored dialogue paragraph per box; optional sentence splits only for long narration |
| Music/silence expresses emotional beats | UNKNOWN local execution; SCRIPT S004/S005 | **ADOPT authored cue order:** silence through the storage-room reveal and delayed low melody in the meal; benchmark duration claims remain deferred |
| Animated portraits, copied framing, choice history/rewind, constant dramatic cuts | PUBLIC console portrait context / UNKNOWN local behavior | **REJECT as current scope:** fixed-pose project sprites, existing session Backlog, no new history/rewind architecture |

## 6. Cross-title Comparison

### Presentation comparison

Each UNKNOWN below means no local screen/audio evidence; section 9 identifies captures. Public concepts do not fill local-observation gaps.

| Dimension | Sani Yang | STEINS;GATE | WHITE ALBUM2 PC | M10 recommendation and basis |
| --- | --- | --- | --- | --- |
| Narration | UNKNOWN | UNKNOWN | UNKNOWN | Same text area, blank nameplate; SCRIPT/PROJECT |
| Inner monologue | UNKNOWN | UNKNOWN | UNKNOWN | Small explicit mode distinction; SCRIPT |
| System text | UNKNOWN | UNKNOWN | UNKNOWN | Separate compact information overlay; SCRIPT |
| Textbox | UNKNOWN | UNKNOWN | UNKNOWN; PUBLIC console message-window concept | Preserve current readable box and text-speed behavior; PROJECT |
| Character layout | UNKNOWN | UNKNOWN | UNKNOWN | Normally 2–3 active sprites; ensemble exception for family; RECOMMENDATION |
| Expression changes | UNKNOWN | UNKNOWN | UNKNOWN | Authored meaningful beats, restrained at contradictions; SCRIPT |
| BG transitions | UNKNOWN | UNKNOWN | UNKNOWN | Existing cut/fade/crossfade according to authored scene changes; PROJECT/SCRIPT |
| CG | UNKNOWN; PUBLIC product includes CG | UNKNOWN | UNKNOWN | Necessary relationship/composition emphasis, no quota; RECOMMENDATION |
| BGM | UNKNOWN | UNKNOWN | OBSERVED stored BGM keys; direction UNKNOWN | Scene continuity and exact authored start/stop order; SCRIPT |
| Silence | UNKNOWN | UNKNOWN | UNKNOWN | Musical silence is intentional, not a missing asset; SCRIPT |
| SFX | UNKNOWN | UNKNOWN | OBSERVED stored SE keys; density UNKNOWN | Authored household/safety cues, sparse additions; SCRIPT |
| Choices | UNKNOWN; PUBLIC multiple outcomes | UNKNOWN locally; PUBLIC Phone Trigger outcomes | UNKNOWN; PUBLIC console prior-choice indicator | No route choices; profile browsing only; SCRIPT |
| Auto | UNKNOWN | UNKNOWN | UNKNOWN behavior; OBSERVED `auto_max` key; PUBLIC console concept | Keep M6/M7 Auto, voice gate and mode feedback; PROJECT |
| Skip | UNKNOWN | UNKNOWN | UNKNOWN locally; PUBLIC console concept | ReadOnly default, existing configurable All; PROJECT |
| Backlog | UNKNOWN | UNKNOWN | UNKNOWN; console prior-text indicator is not proof of PC Backlog behavior | Keep session-only text Backlog; PROJECT |
| Save/Load | OBSERVED candidate files; UX UNKNOWN | UNKNOWN active storage/UX | OBSERVED save files; UX UNKNOWN | Keep checkpoint-based Manual/Auto/Quick and overwrite flow; PROJECT |
| Info overlays | UNKNOWN | UNKNOWN | UNKNOWN | Contextual noncompeting script information; SCRIPT |

### Convenience UX matrix: local references

`U` is a complete unverified tuple: **location UNKNOWN; visibility UNKNOWN; activation UNKNOWN; modality UNKNOWN; obviousness UNKNOWN; story interruption UNKNOWN**. It does not mean “not supported.” Applying U to each cell avoids inventing coordinates, shortcuts, or behaviors. M10 relevance is explicit in the last column. C5 requests the evidence for all eleven features in each title.

| Feature | Sani Yang local | STEINS;GATE local | WHITE ALBUM2 local | M10 relevance |
| --- | --- | --- | --- | --- |
| Advance | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Reliable reveal/advance and no card-click leakage |
| Auto | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Preserve speed/voice handling; explicit scripted-hold issue |
| Skip | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Protect unread content and interactive screens |
| Backlog | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Preserve voice/channel identity without rewind |
| Save | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Clear checkpoint expectations |
| Load | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Safe restoration without stale text or overlay |
| Settings | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Keep current readable/speed/audio/input options |
| Quick controls | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Retain discoverable access and active-mode indicators |
| Hide UI | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Inspect art without advancing story |
| Choice presentation | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Browse versus outcome must be clear |
| Read/unread feedback | UNKNOWN — U | UNKNOWN — U | UNKNOWN — U | Existing Skip policy; no new completion/spoiler signals |

PUBLIC supplements: SG-P1 confirms interaction through phone actions, not placement/modality; WA-P1 confirms console Auto/Skip and history indicators, not the local PC tuple; SY-P1 confirms achievements, not local records UX.

### Comparison against the current project

These are PROJECT facts from read-only source/docs, not a fresh Unity play test. Defaults below come from the authored input asset; M7 user rebindings can override the six allowed keyboard targets. Visual obviousness still needs production-content play review.

| Feature | Location / visibility | Activation | Modal? | Obviousness / interruption | M10 conclusion |
| --- | --- | --- | --- | --- | --- |
| Advance | DialogueLayer and Next in QuickControlLayer; suppressed when hidden/blocked | Click/Space hurry then advance; Next bridge | No | Explicit Next; in-story. Pointer over interactive UI is suppressed | ADOPT unchanged |
| Auto | Quickbar Auto with selected indicator | Button; default A | No | Visible active mode; full-display delay and optional voice gate | ADOPT unchanged globally; S004 authored hold needs later scoped design |
| Skip | Quickbar Skip with selected indicator | Button; default Left/Right Ctrl hold | No | ReadOnly default; unread stopping; All through settings | ADOPT unchanged; no transition acceleration |
| Backlog | ModalLayer; opened on demand | Quickbar Backlog; Close/Cancel | Yes | Dedicated view interrupts story input; speaker/narration-aware text | ADOPT unchanged; no rewind/voice replay/choice-history addition |
| Save | ModalLayer; on demand; Manual/Auto/Quick categories | Quickbar Save, slot selection; manual overwrite confirmation; F1 QuickSave | Yes for regular Save | Deliberate interruption; checkpoint metadata and optional thumbnail | ADOPT 12 Manual slots / 2 pages, 5 Auto, 1 Quick; no arbitrary-line saving |
| Load | Same modal categories | Quickbar Load, slot selection; F2 QuickLoad | Yes for regular Load | Restores validated checkpoint and existing load barrier | ADOPT unchanged; do not promise profile browse-order restoration |
| Settings | ModalLayer; on demand; five categories | Quickbar Settings, category controls; Escape closes/cancels | Yes | Dedicated panel; suppresses story input while open | ADOPT current display/text/Auto/audio/gameplay/rebind settings |
| Quick controls | QuickControlLayer; normally available, hidden by Hide | Next, Hide, Backlog, Skip, Auto, Save, Load, Settings, Records buttons | Bar no; some destinations yes | Explicit access, current mode indicators; pointer routing protects story | ADOPT existing placement; no evidence to redesign |
| Hide UI | DialogueLayer and QuickControlLayer CanvasGroups | Hide button; default H; existing restore behavior | No | Deliberately removes reading/controls and gates progression; not available over options | ADOPT current scope; overlay Hide behavior is an unresolved future integration detail |
| Choice presentation | Built-in Yarn OptionsPresenter and UI navigation | Select an option; Auto/Skip do not choose | Options block story advance; not a Records modal | Distinct interactive state; Save/Load allowed by existing gate | Keep real-choice behavior; do not represent S008 cards as alternate outcomes |
| Read/unread feedback | Read-history service and Skip policy; M8 durable baseline plus session overlay | Authorized full-line consume records a read | No | No inspected promise of a dedicated inline read-color indicator | Keep authority; no new indicator mandated |

Read sources: `Docs/04_TECHNICAL_ARCHITECTURE.md`, `Docs/03_IMPLEMENTATION_STATE.md`, `Docs/02_DECISION_LOG.md`; `Assets/_Project/Scripts/Dialogue/{VNQuickControlBar,VNConvenienceController,VNConvenienceInputRouter,VNConvenienceModalController,VNInteractionGate,VNUIVisibilityController,VNBacklogEntry,VNBacklogItem}.cs`; `Assets/_Project/Settings/Input/VNInputActions.inputactions`; and M5 Save/Load sources. References have supplied no concrete usability defect requiring M1–M9 redesign.

### Presentation timing matrix

No live temporal sample was collected. Stored values and executable timestamps cannot measure transitions. Numerical ranges in the final column are **RECOMMENDATION starting ranges for later M10 review**, never measured reference timings or changes to M6/M7 Auto.

| Event | Sani Yang | STEINS;GATE | WHITE ALBUM2 | Candidate M10 timing / reason |
| --- | --- | --- | --- | --- |
| Character entrance/exit | UNKNOWN | UNKNOWN | UNKNOWN | Cut or short 0.15–0.35 s fade where scene direction warrants it; do not animate every line |
| Expression change | UNKNOWN | UNKNOWN | UNKNOWN | Immediate head swap on authored beat; preserve fixed-pose M3 contract |
| BG change | UNKNOWN | UNKNOWN | UNKNOWN | Cut for direct change; short 0.25–0.60 s crossfade for continuity; authored fade to black remains distinct |
| BGM start/change | UNKNOWN | UNKNOWN | UNKNOWN | Gentle 0.5–1.5 s entrance/change as a review range; S001 entrance remains after the exchange, not at first reveal |
| BGM silence | UNKNOWN | UNKNOWN | UNKNOWN | S003 fade-out can be reviewed over 0.5–1.5 s; S004 none; S006 stop is immediate (0 s), not smoothed |
| Important pause | UNKNOWN | UNKNOWN | UNKNOWN | Usually player reading/advance; S004 door hold explicitly requires manual progression. Do not substitute a fixed timer |
| Insert/CG entrance | UNKNOWN | UNKNOWN | UNKNOWN | Cut or 0.15–0.35 s fade; hold through the relevant text; no reference-derived duration claim |
| Scene exit/black | UNKNOWN | UNKNOWN | UNKNOWN | Authored fade, review 0.4–0.8 s for legibility; S008 audio handoff follows script |

These ranges are optional evaluation bands. Do not add mandatory delays where the script specifies none, replace user text/Auto speed, or copy proprietary timings. Actual reference distributions remain USER_CAPTURE_NEEDED C4/C7.

### Asset implication audit (categories only)

| Scene | Likely categories | Scope implication |
| --- | --- | --- |
| S001 | BG, Character Sprite, Expression, Insert Image, CG, BGM, Ambient, SFX | Kitchen/entry/table coverage, ordinary dog, eggs/shoe/note; intentional five-person table composition; CG is optional if BG/composition suffices |
| S002 | BG, Character Sprite, Expression, Insert Image, BGM, Ambient, SFX | Living-room continuity, travel photo if needed; restrained expression and the one close chair scrape |
| S003 | BG, Character Sprite, Expression, Ambient, SFX, UI Overlay, Transition Effect | Hallway continuity, repeated laugh, first connection panel, restrained perspective cue; no threatening face art |
| S004 | BG, Character Sprite, Expression, Insert Image, Ambient, SFX, UI Overlay | Storage room plus purposeful object views; likely more benefit from insert art than extra BG variants |
| S005 | BG, Character Sprite, Expression, CG, BGM, Ambient, SFX, UI Overlay, Transition Effect | Doorway/table and sunset continuity; family stays visible; silence followed by one low melody matters more than extra SFX |
| S006 | BG, Character Sprite, Expression, Insert Image, Ambient, SFX, UI Overlay, Transition Effect | Temporary white-door outline, exterior room/city, real cyber-dog reveal; no glitch/space/horror asset package |
| S007 | BG, Character Sprite, Expression, UI Overlay, Diagram | Clinic, sleep/meal display and simple two-path diagram; no detailed medical animation |
| S008 | BG, Character Sprite, Expression, UI Overlay, Profile UI, Ambient, SFX, Transition Effect | Preparation room, equally weighted cards, consent/progress information; connection sound → rain → bus |

No final IDs, quantities, production assets, catalogs, or Asset Gap Tracker are created. Optional CG/insert needs require M10-03 asset review rather than assuming every category is mandatory. Continuous looping Ambient playback is not established by the inspected M4 one-shot SFX contract.

## 7. S001–S008 Application

### S001 — 다섯 사람이 먹는 아침

- **Script requirement:** black, no music, frying and plate sounds; children's conversation; kitchen/pan before character reveal; five eggs; light morning music only at the authored later cue. Five people at the table, no empty chair; dog reads as ordinary. Shoe and visit-note inserts must not reveal a hospital.
- **Relevant reference finding:** WHITE ALBUM2 ordinary-scene music/rhythm UNKNOWN; Sani initial-screen framing UNKNOWN (C1/C4/C7). No benchmark overrides the explicit opening sequence.
- **Recommendation:** reveal household activity before explanatory context. Preserve one dialogue paragraph per advance; let short narration describe action without interrupting it with a second UI. Use 2–3 sprites for active exchanges, but an ensemble table composition must communicate all five people and the dog when requested. Off-screen speech remains possible where authored.
- **Decision:** **ADOPT** SFX-before-BG/BGM exactly; **ADAPT** framing to existing slots; **REJECT** horror sound beds, comedic laugh effects, early treatment labels, and an empty-chair implication.
- **Technical implication:** later use existing black/BG/CG and M4 SFX/BGM commands in authored order. M3 supports five named slots; that capacity does not mean five full-size sprites must remain onscreen throughout. No new presenter or audio system is implied.
- **Asset implication:** BG, Character Sprite, Expression, Insert Image, optional CG for table composition, BGM, Ambient, SFX. Mundane inserts establish props, not mystery zooms.

### S002 — 돌아온 다음의 일이 없는 여행

- **Script requirement:** after-breakfast living room, music lowered behind life sounds; small expression at the laundry contradiction without music stopping; one close chair scrape; no hallucinated child/father voice.
- **Relevant reference finding:** STEINS;GATE normal/anomaly timing UNKNOWN; WHITE ALBUM2 expression/audio restraint UNKNOWN. The comparison is a future capture question, not evidence for effects.
- **Recommendation:** let repeated ordinary objects and incomplete answers carry contradiction. Preserve BGM continuity; use the existing player advance around hesitation. Do not add “clue found” labels or mechanical highlighting.
- **Decision:** **ADOPT** subtlety, continuous music and the one authored close SFX; **ADAPT** small head-expression swaps; **REJECT** glitches, suspense cuts, stingers, hallucinations, or a new mandatory dramatic pause.
- **Technical implication:** later author music levels/cues using M4 ownership and current settings; do not create a global ducking feature in this phase. Avoid using visual or sound commands to mark every contradiction.
- **Asset implication:** reused BG/sprites/expressions, optional Insert Image for photographs, BGM, Ambient, SFX. More effects are not the primary need.

### S003 — 강아지가 부르는 이름

- **Script requirement:** hallway, ordinary-looking dog; repeated laugh; no threatening distortion of Minseok. Music fades out at the dog's touch. Rope first addresses Jihun privately; the first compact connection panel appears here. Jihun internal replies and private monologue are separately authored.
- **Relevant reference finding:** SG internal text hierarchy UNKNOWN; SY panel behavior UNKNOWN. PUBLIC SG-P1 supports contextual interaction in the story world, not a particular text style.
- **Recommendation:** spoken lines use normal speaker identity; narration shares the box with no nameplate; monologue uses a subtle explicit Jihun thought label; internal conversation uses explicit channel labels for both Rope and Jihun, distinct from thought; connection data occupies a separate compact overlay. Do not label Rope as a machine before the script reveals it.
- **Decision:** **ADAPT** the hierarchy; **ADOPT** first information reveal timing and authored music fade; **REJECT** a full-screen HUD tutorial, early patient labels, extra voice cues, or horror faces.
- **Technical implication:** later mode styling must continue through the same authoritative LinePresenter, read IDs, Backlog and advance lifecycle. Speaker aliases need review because an internal Jihun/Rope label may not imply focus on a visible sprite. Mode/overlay support is a future extension question, not an existing feature assumed here.
- **Asset implication:** BG, Character Sprite, Expression, UI Overlay, Ambient, SFX, optional restrained Transition Effect. Keep the connection values readable without a meter animation.

### S004 — 새 신발이 있는 방

- **Script requirement:** old shoe prompts the memory chain; a status window has no rising-number animation. Jinhui chooses to open the door; the long silence must not auto-advance. Storage room has no music; document names/date become readable only after approaching. Returned bag and unused new shoe are concrete evidence.
- **Relevant reference finding:** WA insert/CG entry, sprite handling and emotional pause behavior UNKNOWN (C4). No evidence justifies a CG for every prop.
- **Recommendation:** give an insert when scale/readability or physical condition matters: old shoe wear, unused new sole, wet/sandy returned bag, then document detail at the authored proximity. Hold text over an unobstructed crop; return to the room's unchanged logical composition. Do not reveal the document in an earlier wide shot. Avoid unnecessary sprite exits; an insert can temporarily occlude them while preserving scene continuity.
- **Decision:** **ADOPT** reveal order, silence and manual door hold; **ADAPT** economical inserts; **REJECT** automatic door motion, full-screen death data before approach, and animated distress scores.
- **Technical implication:** existing CG Image and transitions may suffice for full-view inserts, subject to later composition/checkpoint review; cropped overlays are not assumed implemented. PROJECT automation gating presently knows options/modal/load/hidden UI, not an explicit authored manual-only pause. That concrete script/architecture gap must be solved later without a global Auto/Skip redesign (section 10).
- **Asset implication:** Insert Image likely adds more value than another BG. BG, Character Sprite, Expression, Ambient, SFX, UI Overlay; no BGM needed for the room.

### S005 — 성공한 작별

- **Script requirement:** family remains visible from the storage-room doorway; Jinhui's emotion is shown alongside Jihun's clinical measurements. Projection assistance start/end is disclosed, then minimized; consent is checked after assistance ends. Sunset meal is a memory-scene change. Low single-note melody starts only after the meal has progressed; utensils remain audible; no completion chime.
- **Relevant reference finding:** WA long-dialogue rhythm, expression rate, CG persistence and silence are UNKNOWN (C4). The desired reference role is not proof of any specific execution.
- **Recommendation:** preserve authored paragraph advances, stillness and meaningful expressions. Keep room sounds and silence until the later melody cue, then allow one low sustained musical phrase/track through the relevant span. Retain family figures or a composition that includes them throughout; avoid camera cuts for every exchange. Use a compact edge panel without faces being covered, then reduce it as directed.
- **Decision:** **ADOPT** family presence, late music, disclosed intervention, and patient-led farewell; **ADAPT** sustained composition to slots/BG/optional CG; **REJECT** disappearing family, celebratory completion UI, swelling music from the scene start, or added sentimental Jihun monologue.
- **Technical implication:** BG crossfade can express the same table at sunset without a clock/time-system mechanic. Timed status values are authored information, not a new real-time countdown. Existing expression/head-swap contract remains. A family-inclusive CG must not replace them with an empty room.
- **Asset implication:** BG variation, Character Sprite, Expression, optional CG, one BGM, Ambient, sparse SFX, UI Overlay, Transition Effect. Restraint matters more than SFX quantity.

### S006 — 흰 문, 닫힌 진료

- **Script requirement:** normal exit; very low vibration and brief white-door overlap on the adjacent wall. Jinhui sees it too. Jihun's view wavers and music stops immediately. Outline disappears; authored fade-out to ordinary daylight and short equipment termination sound. Rope's actual cyber-dog form is revealed now, not earlier.
- **Relevant reference finding:** SG opening anomaly/effect/audio ordering UNKNOWN (C7); PUBLIC phone interaction does not support an anomaly style.
- **Recommendation:** retain the normal doorway composition, add one localized outline and the one low vibration, hard-stop BGM at the written beat, then remove the outline and fade at the written exit. Keep view disturbance small and respect the existing Screen Shake preference if a later consumer is designed. Do not replace the overlap with an early hard cut that prevents recognizing its location or Jinhui's response.
- **Decision:** **ADOPT** minimum authored audiovisual change and immediate silence; **ADAPT** the localized door cue; **REJECT** stars, spacecraft, flames, strong glitch, horror distortion, whispers, or lore certainty.
- **Technical implication:** M4 stop supports a zero-duration stop; existing black fade supplies scene exit. Localized door overlay and view disturbance are not established M3/M4 features. M7's shake flag is a future-consumer gate, not a camera effect already implemented. Review a restrained consumer later; no new input/persistence is authorized here.
- **Asset implication:** BG, optional Insert Image for door, restrained Transition Effect, low SFX, Character Sprite/Expression for real Rope, UI Overlay. No space/horror asset class is justified.

### S007 — 삶을 묻는 검사

- **Script requirement:** doctor asks ordinary-life questions before explanation; sleep/meal records; simple diagram with two distinct paths converging at a late stage; no long medical lecture or animated neural imagery. Acute symptom assessment, record analysis, work restriction, recovery/reassessment/approval remain distinct. Terms stay within authored lore.
- **Relevant reference finding:** SG context/definition/box-length and TIPS UNKNOWN; SY info-panel design UNKNOWN (C3/C2). Neither supplies a verified exposition length rule.
- **Recommendation:** preserve question → observed life response → explanation. Use the script's separate records panel and one simple diagram, with the dialogue supplying meaning. Keep 각성고갈증, 영면병, 회귀 관리, 시대 몰입 at their authored mentions; do not add a glossary definition where the source does not provide one. Use the same font/readability level as dialogue and no competitive numbers. The white-door cause remains unresolved.
- **Decision:** **ADOPT** the authored diagnostic sequence and diagram; **ADAPT** legible layout; **REJECT** new lore, mandatory TIPS interruptions, an expanded pathology taxonomy, or treating increased tests as increased immersion intensity.
- **Technical implication:** later noninteractive overlays must not compete for advance input or become a second narrator. Exact script text remains one authored paragraph per box; do not split spoken paragraphs merely to mimic a foreign-language average. Narrow-screen layout review may resolve fit with typography, not rewriting.
- **Asset implication:** BG, Character Sprite, Expression, UI Overlay, Diagram. One clear conceptual diagram is more useful than repeated spectacle.

### S008 — 가장 편할 것 같은 여자

- **Script requirement:** preparation room after days and reassessment approval; three equally bright role-profile cards. Browsing order can vary. Jihun then chooses Hana in authored dialogue; no alternate companion route. Consent/protection information precedes profiles. Normal-completion conditions are three qualitative items, not romance coercion or literal mind-reading. End audio moves from connection sound to rain and bus brake under fade/black/title.
- **Relevant reference finding:** SY card/profile/navigation behavior UNKNOWN; PUBLIC SG-P1 supplies the contextual-action principle. Neither reference validates a fake romance choice.
- **Recommendation:** label the interaction as profile review. Click or keyboard focus opens a detail; Back returns to the equal-weight overview; neutral visited markers mean “reviewed,” not “preferred.” Preserve access to all three. Present every authored profile and its corresponding thought once, in the player's browse order; revisits may reread details without replaying the thought. Enable a neutral “Return to briefing” after all three have been reviewed, then resume the authored Hana decision. Show Hana confirmed only after that line; other profiles are retained/archived as written. No “Choose this route” action on 선주/해진 that silently resolves to Hana. Browse order changes presentation order, never the result or whether authored content is included.
- **Decision:** **ADAPT** browsing feedback and return continuity; **ADOPT** equal initial weight, authored Hana confirmation, anonymity and qualitative conditions; **REJECT** false route buttons, prehighlighting Hana, affection fireworks, new attraction scores, or anonymous-helper identity spoilers.
- **Technical implication:** later integrate Profile UI with existing input/modal ownership, including pointer suppression and keyboard Space/Auto/Skip blocking while browsing. Back/Escape should return from detail to overview, with explicit exit to briefing. Visited state is transient presentation state, not MetaProgress unlock data; do not add persistence fields. With checkpoint Load, reopen a deterministic browsing entry if needed rather than promise exact detail/order restoration. Profile-as-Yarn-lines is not automatically sufficient because the current gate only covers recognized options/modals. Later resolve integration explicitly.
- **Asset implication:** Profile UI, UI Overlay, BG, Character Sprite, Expression, Ambient, SFX, Transition Effect. No alternate-route CGs or romance-selection effects.

## 8. Proposed M10 Presentation Rules

**RECOMMENDATION — candidate contract only.** These rules answer all fourteen priority questions but are not a frozen contract or benchmark-verification claim.

| Mode/domain | Candidate rule | Decision / later implication |
| --- | --- | --- |
| Normal Dialogue | Keep the authoritative shared dialogue area and speaker name. One authored dialogue paragraph per box; screen directions never rendered. Visible-speaker focus stays M3-owned | ADOPT; current LinePresenter/LineAdvancer lifecycle |
| Narration | Same box/font/readability, blank nameplate; no separate full-screen narration UI by default. Long narration may split by sentence as permitted by script; non-voice | ADOPT shared box; modified speaker state only |
| Jihun Inner Monologue | Same area, explicit small `지훈 · 독백` mode label; at most a subtle accent/border. Do not rely on italics, low contrast, or color alone. No new thought before S003 | ADAPT; label must remain intelligible in Backlog; not audible to other characters |
| Rope Internal Channel | Same reading area with explicit `로프 · 내부` label and a modest channel marker. Jihun replies use `지훈 · 내부`, distinguishable from `지훈 · 독백`. Rope's later public speech is normal `로프`. Do not make the early label reveal machinery | ADAPT; review aliases/speaker focus and existing Backlog string identity later |
| System Information | Separate compact edge overlay for structured status/facts, never speaker dialogue. Reveal at S003, minimize at S005, short medical displays at S006–S008. Preserve faces and textbox; no score-rise spectacle | ADAPT; one dialogue advance owner; overlay dismissal/Hide/load behavior needs later review |
| Character framing | Usually 2–3 active sprites; story presence can be established in the BG/ensemble composition/off-screen voice as authored. Five-person breakfast and S005 family continuity override the default. Expressions change on meaningful beats | ADAPT to five M3 slots; no crowding quota or family erasure |
| Insert Images | Use when physical condition, scale or legibility matters to an authored beat. First establish the ordinary prop; close-up only at its cue. Old/new shoes, returned bag and accident paper justify selective emphasis | ADAPT; logical composition survives entry/exit; no mystery labels |
| CG | Reserve for a composition/relationship that sprites plus BG cannot clearly carry. Dialogue can continue in the same text area. No automatic CG for every emotional sentence | ADAPT; optional family/table composition must retain required people |
| BGM | Follow authored entrance and continuity: S001 delayed morning theme; S002 lowered/continuous; S003 fade; S004 no music; S005 late low melody; S006 immediate stop. No invented global “music every scene” rule | ADOPT; M4 ownership and existing M7 user volumes |
| Silence | Treat BGM absence as authored state. Preserve permitted room sound. S004 door hold requires explicit manual advance; ordinary pauses use player reading, not arbitrary timers | ADOPT; scoped hold capability is future work, not assumed |
| SFX | Sparse causal cues: cooking/plate/doorbell/footsteps, single close chair scrape, scripted repeated laugh, utensils, low vibration, equipment and final rain/bus sounds. No laugh track, clue stinger, victory chime, or extra hallucination | ADOPT authored cues; new incidental cues only when they clarify action |
| Transitions | Cut/fade/crossfade communicates actual scene relation. No repeated glitch or dream wash. S005 sunset is memory-scene change; S006 localized anomaly then authored fade; S008 connection sound gradually gives way to rain | ADAPT using M4; continuous Ambient/camera/overlay support is not assumed |
| Profile Browse UI | Equal-weight overview, inspectable details, explicit Back; after all three authored profiles/thoughts are presented, return to briefing. Visited means reviewed. Hana confirmed after Jihun's canonical line. No alternative-route affordance | ADAPT; transient visits, deterministic checkpoint re-entry, existing modal/input arbitration |
| Convenience/Records | Keep Next/Hide/Auto/Skip/Backlog/Save/Load/Settings/Records, rebinding, active-mode indicators, ReadOnly policy, checkpoint categories, M8 authority, M9 projections and isolated Replay | ADOPT unchanged; no evidence warrants redesign |

Hierarchy is based on **communicative role**, not additional windows for every speaker: normal dialogue/narration/thought/internal exchange share the reading location; system data is a separate informational layer. Monologue difference is small, internal-channel difference is explicit but restrained, and system information is structurally distinct. This answers text-mode priority questions 1–4 without copying reference styling.

Keep Yarn Spinner **3.2.7**, uGUI, production Start Node **M2_UI_START**, checkpoint Save/Load, existing M6/M7 input and convenience ownership, M8 MetaProgress as sole durable meta authority, M9 Records architecture, and isolated Replay. No new schema, runtime code, catalog, Scene, Prefab, Yarn content, input action, package, or asset is part of M10-00. Presentation candidates must not be smuggled into Replay: its existing command/content policy remains authoritative.

## 9. User Capture Requests

These are the remaining evidence requests, in priority order. Supply private/local screenshots or short recordings from the exact installed builds; do not extract archive assets or scenario text. Include title/build or launcher-language indication if visible, approximate scene position, and whether full-unlock saves are active. Screenshots establish static appearance; **only an audio-enabled clip can verify BGM/silence/SFX**, and only consecutive observed states can verify timing/return behavior. Assets/captures stay outside this repository.

| ID / priority | Game and exact need | Purpose | Requested evidence |
| --- | --- | --- | --- |
| C1 / 1 | STEINS;GATE early protagonist spoken line, internal/narrative line, and other-character speech; WHITE ALBUM2 normal narration and a two-character exchange; Sani first story screen and narration | Decide text hierarchy, nameplate/box density and framing without inventing local behavior | SG 3 screenshots; WA 2; Sani 2. Same scene/box where possible; no long dialogue transcription |
| C2 / 2 | Sani story immediately before, during and after one system/info interaction; SG first phone introduction/action/return | Overlay versus replacement, feedback, interruption and narrative continuity | One 30–60 s clip per game; include action and return. A panel being absent in this sample is not proof it never exists |
| C3 / 3 | SG early unfamiliar term and its next explanation, plus TIPS if naturally encountered; Sani one story information panel | Context/definition order and readability for S007 | SG 2–3 screenshots plus one TIPS screen only if encountered; Sani 1–2 panel/dialogue screenshots |
| C4 / 4 | WHITE ALBUM2 one emotional exchange spanning 8–12 manual advances, ideally with two/three people and an insert/CG transition | Text rhythm, expressions, hold duration, sprite/CG continuity, BGM/silence/SFX | One 2–4 min audio-enabled clip at normal speed; annotate scene position. If no CG occurs, a separate 30–60 s CG entry/exit clip |
| C5 / 5 | Each game's normal story controls, Auto on/off, Skip stopping policy, Backlog, Save, Load, Settings, Hide, and first genuine choice/read indicator where encountered | Fill every U dimension of the eleven-row convenience matrix | Per title: normal screen + Auto + Skip + Backlog + Save + Load + Settings screenshots, and one 30–60 s navigation/Hide/return clip. Note how each was activated. Inspect empty/occupied slots without overwriting existing saves; provide normal Load behavior only if comfortable doing so |
| C6 / 6 | Sani available history/Timeline-equivalent, Archive, achievements/collections, and one locked/unlocked detail with return | Establish actual names, masking and navigation for the meta role | Overview + locked + unlocked + returned-story screenshots where these exist. If absent, record where looked; do not infer feature absence from one menu. Full-unlock saves cannot establish pristine locking |
| C7 / 7 | SG earliest ordinary→abnormal beat; Sani one BG/character/audio change; optionally WA ordinary-scene entrance if C4 lacks it | Qualitative timing and audio ordering | SG 1–2 min audio-enabled clip around the beat; Sani 30–60 s clip; optional WA 30–60 s. No frame-perfect measurement needed |

USER_CAPTURE_NEEDED does not block this documentation PR. It does block describing the remaining local presentation claims as OBSERVED. When supplied, update this same audit with evidence IDs, approximate timings, classification and any changed recommendation; do not import copyrighted captures.

## 10. Open Questions

1. **Local presentation evidence:** how do the installed builds actually separate text layers, present convenience controls, return from panels, and time emotion/anomaly/audio? C1–C7 resolve this. No amount of filesystem metadata answers it.
2. **S004 manual-only hold:** what narrow later authoring/presentation contract prevents Auto from opening the door, and what should explicit user Skip do at that authored hold? The script forbids automatic progression; current inspected convenience gating has no authored-hold state. Decide in M10-01/M10-02 without changing global M6 semantics in this audit.
3. **M10 informational overlay integration:** how will noninteractive overlays behave under Hide, Backlog, Save/Load and checkpoint resume, and how will S008 transient browsing claim/release input? Existing M5–M9 ownership must remain intact. Reconstruct authored information from a checkpoint rather than adding serialized overlay/browse state.
4. **Production composition fit:** which five-person/doorway/table views need ensemble art, and which props need cropped versus full-view inserts at the project's existing 1920×1080 reference canvas? This is M10-03 asset/layout review; no final IDs or asset counts are frozen here.

### Review and validation record

- Intended change: only `Docs/Research/M10_LOCAL_VN_REFERENCE_AUDIT.md`.
- Review requirement: full staged diff, exact changed-path allowlist, and `git diff --check` before commit.
- No full Unity suite: documentation-only; baseline test numbers are reported, not rerun.
- Source script remains outside the repository. No reference saves/configs/screenshots/assets are staged.
- Commit and PR identities are recorded in the final handoff, not embedded as self-referential document metadata.
- Review status: **M10-00 LOCAL VN REFERENCE AUDIT READY FOR REVIEW — local visual/audio evidence remains USER_CAPTURE_NEEDED.** Do not begin M10-01 automatically.
