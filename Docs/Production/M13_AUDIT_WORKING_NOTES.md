# M13 menu UI audit — working notes

**Incomplete; not a visual direction approval or M13-00/01 completion.**

Baseline: `9380e894f3e5b941e01a8272d85e8cb772eddc84`, Unity 6000.3.21f1, active scene `Assets/_Project/Scenes/VN_Main.unity`. On 2026-11-16 the connected Editor's `SerializedObject` API was used to inspect the loaded scene. The following inventory describes observed references, not a complete visual or interaction audit.

| Scene component | Observed hierarchy and serialized references | Initial treatment |
|---|---|---|
| `VNSettingsPanel` | `VNCanvas/ModalLayer/SettingsModal/Panel`; five category buttons, five content roots and corresponding views, status text | KEEP service/controller ownership; RESTYLE and investigate layout |
| `VNSaveLoadModal` | `VNCanvas/ModalLayer/SaveLoadModal`; Manual/Auto/Quick tabs, slot container and `VNSaveSlotItem` prefab, pagination, overwrite panel | KEEP controller and transaction ownership; RESTYLE cards and controls |
| `VNRecordsModal` | `VNCanvas/ModalLayer/RecordsModal`; Timeline/Gallery/Archive/Achievements roots, tabs and separate selected indicators | KEEP projection and unlock ownership; RESTYLE shell and cards |
| `VNBacklogModal` | `VNCanvas/ModalLayer/BacklogModal`; ScrollRect, content, `BacklogItem` prefab, empty text, session-state reference | KEEP pooling, scrolling and session ownership; investigate layout and RESTYLE |
| Title/in-game menu | M12 `VNTitleScreen` prefab under canvas; M12 title flow controller | KEEP title flow and input routing; DEFER limited consistency changes |

The M11 report identifies a corrected near-opaque Backlog surface and text-width fix. Preserve those readability fixes. M12 documents 36 actual Editor Game View captures at 1280×720, 1600×900 and 1920×1080 in `M12_Evidence/FinalMatrix`, including Settings, Load, Records, Title and in-game menu. They are historical M12 captures and have not yet been verified against current live rendered states. They do not cover all categories and states requested for M13.

Protected contracts: existing SaveData schemas and async load completion, settings persistence, MetaProgress and Records unlock rules, Replay isolation, dialogue ownership, modal arbitration and existing input routing. No runtime logic should be changed for material styling.

Outstanding: full screen/state and layout audit; live baseline evidence; interaction matrix; A/B Unity previews; original asset family and import validation; multi-background and resolution testing; contrast measurements; refinement; tests and production reports. No M13 design tokens or visual assets have yet been validated.
