# M13 menu UI audit — working notes

**Audit inventory and observed-state review complete; not a visual direction approval or M13-00/01 completion.**

Initial source snapshot: `9380e894f3e5b941e01a8272d85e8cb772eddc84`, Unity 6000.3.21f1, active scene `Assets/_Project/Scenes/VN_Main.unity`. The connected Editor's `SerializedObject` API inspected the loaded scene. Current audit coverage, observations and gaps are closed out below.

| Scene component | Observed hierarchy and serialized references | Initial treatment |
|---|---|---|
| `VNSettingsPanel` | `VNCanvas/ModalLayer/SettingsModal/Panel`; five category buttons, five content roots and corresponding views, status text | KEEP service/controller ownership; RESTYLE and investigate layout |
| `VNSaveLoadModal` | `VNCanvas/ModalLayer/SaveLoadModal`; Manual/Auto/Quick tabs, slot container and `VNSaveSlotItem` prefab, pagination, overwrite panel | KEEP controller and transaction ownership; RESTYLE cards and controls |
| `VNRecordsModal` | `VNCanvas/ModalLayer/RecordsModal`; Timeline/Gallery/Archive/Achievements roots, tabs and separate selected indicators | KEEP projection and unlock ownership; RESTYLE shell and cards |
| `VNBacklogModal` | `VNCanvas/ModalLayer/BacklogModal`; ScrollRect, content, `BacklogItem` prefab, empty text, session-state reference | KEEP pooling, scrolling and session ownership; investigate layout and RESTYLE |
| Title/in-game menu | M12 `VNTitleScreen` prefab under canvas; M12 title flow controller | KEEP title flow and input routing; DEFER limited consistency changes |

The M11 report identifies a corrected near-opaque Backlog surface and text-width fix. Preserve those readability fixes. M12 documents 36 actual Editor Game View captures at 1280×720, 1600×900 and 1920×1080 in `M12_Evidence/FinalMatrix`, including Settings, Load, Records, Title and in-game menu. They are historical M12 captures and have not yet been verified against current live rendered states. They do not cover all categories and states requested for M13.

Protected contracts: existing SaveData schemas and async load completion, settings persistence, MetaProgress and Records unlock rules, Replay isolation, dialogue ownership, modal arbitration and existing input routing. No runtime logic should be changed for material styling.

The initial outstanding list is superseded by the audit closeout below, the current B visual evidence, and the latest test/contrast results in the production report.


## Audit closeout — 2026-11-16

The source/hierarchy inventory and observed-state review for M13-00/01 are complete as an audit deliverable. “Complete” here means known production screens, ownership boundaries, captured states, findings, and unobserved states are all recorded; it does not mean every production state passed visual or interaction validation. No production scene, runtime component, save schema, settings service, unlock service, or global TMP font was changed.

| Production area | Observed coverage and findings | Still unobserved |
|---|---|---|
| Settings | Display, Text, Audio, Gameplay and Controls captured in the isolated `VN_Main` baseline. Existing setting-service ownership and persistence are retained. | Dropdown popups/overflow, all focus traversal, rebind listening/busy/error/cancel and reset confirmation. |
| Save / Load | Manual/Auto/Quick empty states; a Manual populated slot and overwrite prompt at 720p. The populated sample has small metadata and a wrapped page indicator; the overwrite prompt has weak message/action visibility. | Full pages, Auto/Quick populated policies, corrupt/unavailable/error states, async completion and every confirm/cancel route. |
| Records | Timeline/Gallery/Archive/Achievements empty states; a seeded chapter, one unlocked gallery item/viewer, archive entry and achievement. | Complete lock/unlock combinations, long text, all scroll extremes and all spoiler-safe/replay routes. |
| Backlog | Empty, long/named-speaker and narrator fixtures captured. The long production-font fixture exposed missing Korean glyphs; preserve the prior M11 width fix. | Full live session length, every pool/scroll extreme, every speaker style and production-font remediation. |
| Title / in-game menu | Existing Title and in-game menu captured; M12 ownership and flow retained. | Full current-build resolution matrix and standalone Player behavior. |

The current isolated baseline capture methodology, storage identity and image index remain in [Before/README](M13_Evidence/Before/README.md). Production state mutation was limited to the documented isolated identity and was restored. The M13 visual prototype does not claim production integration.

### Preview input/focus interference record

| Focus/hover target | Keyboard and pointer path | Result | Status |
|---|---|---|---|
| Card0 → Card1 | Pointer select Card0, press Right | EventSystem focus advances to Card1; only its gold focus ring is active. | Physical Editor pass. |
| Slider | Pointer drag/click, then Right | Slider value changes; card ScrollRect does not change. | Physical Editor pass. |
| Toggle label | Click label text | Toggle changes state. | Physical Editor pass. |
| Scrollable cards / blank viewport / lane | Wheel; drag narrow thumb | Content and thumb move together, clamp at both ends. | Physical Editor pass; 240 reference units per notch. |
| Settings slider panel | Wheel over the panel | Scroll content and slider value remain unchanged. | Physical Editor pass. |
| Disabled card | Hover/click | No focus ring, selection change or activation. | Physical Editor pass and automated test. |
| Toggle Submit | UI Submit event | Toggle changes. | Automated focused test only; physical Space was not captured. |

This matrix records every input path physically exercised in the preview and marks the rest as gaps. It does not infer production input routing from fixture behavior.

### Quality rubric

Scale: **2** = verified within stated evidence scope; **1** = partial or limited scope; **0** = not verified / unresolved. Scores describe evidence completeness, not a combined M13 approval score.

| Criterion | Score | Evidence / decision |
|---|---:|---|
| B scrollbar geometry and screen-scaled safe bounds | 2 | RectTransform screen bounds at 1920×1080, automated geometry checks, and required top/bottom and resolution captures. |
| Preview scroll and control input separation | 2 | Physical wheel/drag/click checks plus focused Editor UI-event tests. |
| Card focus, selection and disabled blocking | 2 | Physical Card0→Card1 keyboard check, independent selection/focus visuals, disabled hover/click, focused tests. |
| Text and indicator contrast | 1 | Three background and three bright-size text samples plus indicator samples; metadata at 720p is borderline and slider fill is low contrast. |
| B 9-slice sizing | 1 | Four primary surface pairs checked at minimum/maximum at 1080p; no multi-resolution or every-role stress image. |
| Production ownership and state audit | 1 | All five areas inventoried with baseline states/findings; the explicitly unobserved states above remain. |
| Preview camera behavior | 1 | Camera mask 0 and overlay verified in Editor Game View only; no Windows Player validation. |
| Automated regression | 2 | Current 7/7 focused M13 and 548/548 unfiltered EditMode tests pass. |

The Direction B preview is ready for visual review, while M13-00/01 sign-off remains open for the named contrast and production-state gaps. Do not treat this audit as a user design approval.
