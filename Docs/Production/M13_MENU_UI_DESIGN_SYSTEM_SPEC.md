# M13 Menu UI design system — working specification

**Direction B review candidate — 2026-11-16.** The preview and production ownership/state audit are documented for review. Several production state and input gaps remain open; this is not user visual acceptance or completion of M13-00/01. Base `9380e894f3e5b941e01a8272d85e8cb772eddc84`.

## Production inventory and protected ownership

| Screen | Observed hierarchy / visual owner | Data, input and disposition |
|---|---|---|
| Settings | `VNCanvas/ModalLayer/SettingsModal/Panel`, `VNSettingsPanel`; Display, Text, Audio, Gameplay, Controls views and category buttons | Settings service/controllers own values and persistence; convenience modal arbitration owns opening. **KEEP** ownership; **RESTYLE / RELAYOUT** controls and layout. Five categories captured in isolated live scene. |
| Save / Load | `VNCanvas/ModalLayer/SaveLoadModal`, `VNSaveLoadModal`, `VNSaveSlotItem` prefab; Manual/Auto/Quick tabs, pages, overwrite confirmation | `VNSaveLoadController` owns transactions and modal interaction. **KEEP** validation, async completion, models and paging; **RESTYLE / RELAYOUT** visual shell and slots. Empty categories captured in both modes. |
| Records | `VNCanvas/ModalLayer/RecordsModal`, `VNRecordsModal`; separate Timeline, Gallery, Archive, Achievements roots and indicators | Records projections and MetaProgress own disclosure/unlocks; convenience modal controller arbitrates input; Replay remains isolated. **KEEP** ownership; **RESTYLE / RELAYOUT** shell and items. Four tabs captured with isolated progress. |
| Backlog | `VNCanvas/ModalLayer/BacklogModal`, `VNBacklogModal`, pooled `BacklogItem`, `ScrollRect`, empty text | Session backlog owns data; modal controller owns input. **KEEP** pooling/scroll semantics and previous width/readability correction; **RESTYLE / RELAYOUT** rows. One live state captured. |
| Title / in-game menu | `VNTitleScreen` under `VNCanvas`; `VNTitleFlowController` | **KEEP** state machine, load completion, title flow and Cancel chain; **DEFER** limited consistency updates to later M13 integration. Both captured. |

The [Before index](M13_Evidence/Before/README.md) records isolated production captures and their conditions. The M13-00/01 production audit now inventories every listed screen/category and its observed states, with the remaining state gaps recorded in [audit closeout](M13_AUDIT_WORKING_NOTES.md). Evidence includes populated Manual Save, overwrite confirmation, seeded Records, and long/narrator Backlog fixtures. The audit found small Save metadata, a wrapped page indicator, a weak overwrite panel, and missing production-font glyphs in one long Backlog fixture. Those are integration findings; production screens and data ownership were left unchanged.

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

The user selected B, Textured Matte Composite, as the preferred working material direction. This decision does not approve the whole milestone or every implementation detail. A is preserved as historical comparison. B uses warm matte charcoal, restrained texture, gold focus and teal active accents; avoid increasing grain simply to differentiate it from A. The preview-specific Noto font contains only its known preview characters; it is not a replacement for the production Korean font.

Future M13-02–08 integration must preserve Yarn/stable IDs, SaveData schemas and M10-15 compatibility, Settings persistence, MetaProgress, Records unlock and Replay isolation, dialogue and modal arbitration, input routing, story/audio timing, and M12 title/async-load flow. Reuse existing `VNUITheme` and bindings where justified; no second persistent theme authority. No production screen redesign has been made in this stage.

Public project references are used for broad presentation context only: [13 Sentinels official site](https://atlus.com/13sentinels/index.html) and [PARANORMASIGHT official site](https://paranormasight.square-enix-games.com/en-gb/). No reference-game textures or art are included. The requested Saniyang screenshots were not present in the supplied attachment, so no analysis of them is claimed.

Additional baseline finding: seeded isolated Gallery shows locked placeholders and one project-owned unlocked thumbnail. The long in-memory Backlog fixture wraps but has missing glyphs in the production font, so a later production integration must test real Korean coverage independently of the limited preview atlas. See the Before index for the fixture methodology.

The isolated 720p populated-save capture shows small metadata and a wrapped page indicator; the overwrite-confirmation capture shows a white confirmation surface with poor message/action visibility. **RELAYOUT / REPLACE VISUAL COMPONENT** in later authorized integration; **KEEP** transaction and confirmation ownership. Empty Backlog was captured after clearing only the isolated session.

## Detailed component audit (source and representative live evidence)

| Screen/category | Current visual and binding | State, scroll, type and disposition |
|---|---|---|
| Display | `DisplayContent` / `VNDisplaySettingsView`; mode and resolution TMP dropdowns; view delegates to display controller and Settings service | Disabled resolution is visible in baseline. Dropdown popup/focus and long option overflow remain unobserved. **RESTYLE** dropdown and tabs; **KEEP** application/persistence. |
| Text | `TextContent` / `VNTextSettingsView`; text speed and auto speed sliders, value labels and commit components | Commit boundaries are behavior contracts; TMP labels and slider states need 720p pointer/focus review. **RESTYLE** slider and labels, **KEEP** commits. |
| Audio | `AudioContent` / `VNAudioSettingsView`; Master/BGM/SFX/Voice sliders, labels and commit components | Preserve audio controller and commit timing. Current capture uses plain controls. Overflow/focus not exhaustively examined. **RESTYLE**, **KEEP** behavior. |
| Gameplay | `GameplayContent` / `VNGameplaySettingsView`; skip unread and screen shake toggles | View refreshes without notify and blocks changes when `canWrite` is false. Preserve this contract. **RESTYLE** toggles and label hierarchy. |
| Controls | `ControlsContent` / `VNControlsSettingsView`; rebind items and reset button | `VNInputRebindService` owns listening/cancel/reset; preserve focus and input suppression. Rebind busy/error states need live audit. **RESTYLE / RELAYOUT**, **KEEP** event ownership. |
| Save / Load Manual | Modal header, tabs, `SlotContainer`, six `VNSaveSlotItem` instances and pagination; `VNSaveLoadController` supplies model | Empty and populated captures show dark flat rows, white thumbnail fallback, small metadata and wrapped page text. Overwrite confirmation in 720p capture has poor visible hierarchy. **RELAYOUT / REPLACE VISUAL COMPONENT**, **KEEP** model, confirmation, validation and page events. |
| Save / Load Auto and Quick | Same modal and slot item; Auto newest-first and Quick single slot per current policy | Empty live captures exist. Slot button interactivity is model-controlled; Save Auto/Quick restrictions must not be inferred from appearance. **RESTYLE / RELAYOUT**, **KEEP** policies. |
| Records Timeline | `VNRecordsModal` tab root and Timeline views consume projected discovery/completion; Replay has separate presenter | Captured isolated and seeded chapter. Selected/completed details and overflow still need targeted observation. **RESTYLE / RELAYOUT**, **KEEP** projection and replay isolation. |
| Records Gallery | Gallery `ScrollRect` and bound items; unlocked sprite resolved by presentation catalog, viewer separately owns presentation | Captures show gray locked tiles and one unlocked image plus viewer. `VNCGGalleryView` rebuilds content and scrolls to top; no locked Sprite disclosure. **RESTYLE / RELAYOUT**, **KEEP** aspect and spoiler policy. |
| Records Archive | Archive categories/entries/detail and scroll roots, with selected indicators | Seeded and empty captures; item binding respects unlock projection. Long body and scroll extremes remain unverified. **RESTYLE / RELAYOUT**, **KEEP** disclosure. |
| Records Achievements | `VNAchievementsView`, ScrollRect, item and detail binding selected ID | Rebuilds layout, starts at top; secret locked content must remain concealed. Seeded and locked captures exist; long titles/keyboard behavior need further review. **RESTYLE / RELAYOUT**, **KEEP** evaluator/service. |
| Backlog | pooled `VNBacklogItem` and TMP speaker/body in `ScrollRect`; session-only list | Empty, named, narrator and long fixture captured. Existing test text exposes missing production font glyphs; previous M11 width fix must be preserved. Start-at-latest and pool semantics remain. **RELAYOUT / RESTYLE**, **KEEP** pooling/data, **DEFER** production font remediation to later integration. |
| Title / game menu | `VNTitleScreen` prefab, title and in-game menu coordinated by title controller | Existing M12 captures and fresh Title/game-menu samples; focus and standalone behaviors retain M12 limitations. **KEEP** title flow and existing input routing; **DEFER** consistency updates. |

This inventory combines code ownership and actual captures. It is not a complete visual-state sign-off: hover/pressed, keyboard traversal, 900p production layouts, long Records fields, and all save compatibility/error states remain open. Existing Korean TMP fonts are retained by production; M13 preview atlas is fixture-limited. `VNUITheme` remains the shared authority for bindings where applicable.

## User feedback layout revision

The isolated A/B preview keeps the 270×305 cards in three columns at 300-unit centers, with two 320-unit rows in 930×640 content. In the corrected B layout the masked viewport is 910×320 at `(-30, 30)`; a separate 36×288 `ScrollbarLane` at `(460, 30)` sits beside it under `Content`. Its visible track is 10×288 and its transparent drag area is 36 units wide; the visible thumb is 18 units wide and 144 units tall at 50% scroll size. The viewport-to-lane gap is 17 reference units and the track is inset 16 units from each viewport end. The CanvasScaler keeps the three-column composition at 720p, 900p and 1080p without reducing the Korean text size. Wheel sensitivity remains 240, with clamped movement. The slider still has a 14-unit visual track inside a 48-unit hit area; the toggle label remains a clickable child. These are preview-only examples, not production integration.
