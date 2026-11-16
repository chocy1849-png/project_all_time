# M13 Menu UI design system — working specification

**Work in progress.** This is a proposed design and implemented preview, not user visual acceptance or completion of M13-00/01. The live capture set covers representative states; the complete production-state audit and interaction validation remain open. Base `9380e894f3e5b941e01a8272d85e8cb772eddc84`.

## Production inventory and protected ownership

| Screen | Observed hierarchy / visual owner | Data, input and disposition |
|---|---|---|
| Settings | `VNCanvas/ModalLayer/SettingsModal/Panel`, `VNSettingsPanel`; Display, Text, Audio, Gameplay, Controls views and category buttons | Settings service/controllers own values and persistence; convenience modal arbitration owns opening. **KEEP** ownership; **RESTYLE / RELAYOUT** controls and layout. Five categories captured in isolated live scene. |
| Save / Load | `VNCanvas/ModalLayer/SaveLoadModal`, `VNSaveLoadModal`, `VNSaveSlotItem` prefab; Manual/Auto/Quick tabs, pages, overwrite confirmation | `VNSaveLoadController` owns transactions and modal interaction. **KEEP** validation, async completion, models and paging; **RESTYLE / RELAYOUT** visual shell and slots. Empty categories captured in both modes. |
| Records | `VNCanvas/ModalLayer/RecordsModal`, `VNRecordsModal`; separate Timeline, Gallery, Archive, Achievements roots and indicators | Records projections and MetaProgress own disclosure/unlocks; convenience modal controller arbitrates input; Replay remains isolated. **KEEP** ownership; **RESTYLE / RELAYOUT** shell and items. Four tabs captured with isolated progress. |
| Backlog | `VNCanvas/ModalLayer/BacklogModal`, `VNBacklogModal`, pooled `BacklogItem`, `ScrollRect`, empty text | Session backlog owns data; modal controller owns input. **KEEP** pooling/scroll semantics and previous width/readability correction; **RESTYLE / RELAYOUT** rows. One live state captured. |
| Title / in-game menu | `VNTitleScreen` under `VNCanvas`; `VNTitleFlowController` | **KEEP** state machine, load completion, title flow and Cancel chain; **DEFER** limited consistency updates to later M13 integration. Both captured. |

The 1080p [Before index](M13_Evidence/Before/README.md) records capture conditions. The isolated run did not exercise populated saves, all locked/unlocked Records variations, long/narrator Backlog, hover, or all scroll extremes. Existing M12 evidence is historical. Those omissions prevent calling the production audit complete. Current captures show white default controls, large empty areas and a wrapped page indicator in Save/Load; retain behavior while correcting presentation later.

## Proposed interaction language

| Component | Normal / hover / pressed | Selection / focus | Disabled / exceptional |
|---|---|---|---|
| Button / confirmation | neutral surface; hover lifts luminance; press darkens | gold focus outline plus luminance; active uses teal | muted legible label; destructive uses restrained red surface plus explicit wording |
| Tab | neutral; hover lifts surface | teal surface + underline for selection; gold outline for focus, both can coexist | muted label and no action |
| Save / gallery card | raised surface; hover border | teal selection accent and gold focus outline | disabled or locked label/icon and no activation; preserve spoiler policy |
| Achievement / records row | neutral row, clear hover | marker/edge plus focus outline | lock icon and disclosure-safe text |
| Slider / scrollbar | visible track and distinct handle; press feedback | focus ring or clear handle outline | muted track and handle, no interaction |
| Toggle / dropdown / rebinding | visible control and text; hover/press feedback | check/indicator shape for value, outline for focus | explanatory disabled label; rebind busy state must be explicit |
| Busy / loading | readable status and progress or activity cue | focus retained or intentionally transferred by existing owner | controls blocked as existing contract dictates |

The preview currently demonstrates a selected tab, focused tab, disabled button, destructive button, slider, scrollbar and toggle. It does **not** yet demonstrate all states, pointer/keyboard paths, or production event ownership. Do not treat this matrix as validation of those states.

## Working tokens at 1920×1080 reference

M11 anchors: charcoal `#252B32`, ivory `#F7F4EF`, gold `#E5CAA2`, teal `#81BBB7`, secondary `#AAB5BD`. Current panel source `(31,39,47,254)`; secondary `(39,48,56,250)`; card `(48,58,65,250)`; inset `(23,30,36,252)`; selected `(60,91,90,255)`. Source colors are not guaranteed rendered composite colors. Prototype dimensions: modal 1590×880, tab 230×56, cards 292×305, action 240×56, title 42, body 20–24, metadata 17 reference units. These are candidate values, not production-wide approved tokens. Material uses a clean sliced structure and separate 128px tiled grain at Image alpha 0.16; appearance and repeat must be evaluated on final screens.

## Candidate direction and boundaries

A is the working recommendation: cooler smoked surface and restrained gray edges align with M11. B uses warmer undertones, squarer corners and muted-gold edges. Both retain the same layout and source content; neither is user-approved. Their [matching 1080p captures](M13_Evidence/Current/README.md) support comparison, but further material quality, focus and B-state testing remains. The preview-specific Noto font contains only its known preview characters; it is not a replacement for the production Korean font.

Future M13-02–08 integration must preserve Yarn/stable IDs, SaveData schemas and M10-15 compatibility, Settings persistence, MetaProgress, Records unlock and Replay isolation, dialogue and modal arbitration, input routing, story/audio timing, and M12 title/async-load flow. Reuse existing `VNUITheme` and bindings where justified; no second persistent theme authority. No production screen redesign has been made in this stage.

Public project references are used for broad presentation context only: [13 Sentinels official site](https://atlus.com/13sentinels/index.html) and [PARANORMASIGHT official site](https://paranormasight.square-enix-games.com/en-gb/). No reference-game textures or art are included. The requested Saniyang screenshots were not present in the supplied attachment, so no analysis of them is claimed.

Additional baseline finding: seeded isolated Gallery shows locked placeholders and one project-owned unlocked thumbnail. The long in-memory Backlog fixture wraps but has missing glyphs in the production font, so a later production integration must test real Korean coverage independently of the limited preview atlas. See the Before index for the fixture methodology.
