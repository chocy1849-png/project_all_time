# M13-03 Settings UI validation report

## Scope and results

Unity Editor 6000.3.21f1. `VN_Main.unity` is the production scene; the existing Settings panel, category views, sliders and commit seams, toggles, rebind items and modal controller were retained. M13 Slider and Toggle prefabs are instantiated as visual children; their duplicate Selectable components are removed, so the original control remains the input owner. Panel, tabs, buttons, focus and dropdowns use Direction B assets; the production M13 Korean dynamic font is used throughout.

The focused M13-03 EditMode suite completed **8 passed / 0 failed / 0 skipped / 0 inconclusive**. It checks production-scene wiring, five category roots and active indicators, actual nested prefab links, missing scripts, dropdown template references, and all six same-object slider commit seams. The integration tests instantiate the production panel with the real settings service and controllers, a temporary settings repository, fake display/mixer adapters, and a separate input action asset. They exercise display-mode selection and resolution enabled state; text and auto commit boundaries; independent Master/BGM/SFX/Voice commits and reload; gameplay toggles; read-only behavior; and synthetic-keyboard rebind success, conflict, Escape cancellation, category/close cancellation, one reset and Reset All. EditMode lifecycle handlers are explicitly attached in the test harness. Synthetic events are not physical-input validation.

The most recent unfiltered EditMode suite completed **571 passed / 0 failed / 0 skipped / 0 inconclusive**. It includes the existing controller, rebind, title and modal tests, but does not substitute for an end-to-end Player session.

## Production Game View

In Editor Play Mode the actual title Settings button was invoked programmatically. `VNSettingsModal.IsOpen`, `VNSettingsPanel.IsInitialized` and `TryValidateWiring` were true. The Game View render target was explicitly set to each listed resolution and `Screen.width/height` verified before capture. The following are actual production-scene Game View captures, not isolated mockups:

| Category | 1280×720 | 1920×1080 |
| --- | --- | --- |
| Display | [720](M13_Evidence/M13_03/m1303_Display_verified720.png) | [1080](M13_Evidence/M13_03/m1303_Display_verified1080.png) |
| Text | [720](M13_Evidence/M13_03/m1303_Text_verified720.png) | [1080](M13_Evidence/M13_03/m1303_Text_verified1080.png) |
| Audio | [720](M13_Evidence/M13_03/m1303_Audio_verified720.png) | [1080](M13_Evidence/M13_03/m1303_Audio_verified1080.png) |
| Gameplay | [720](M13_Evidence/M13_03/m1303_Gameplay_verified720.png) | [1080](M13_Evidence/M13_03/m1303_Gameplay_verified1080.png) |
| Controls | [720](M13_Evidence/M13_03/m1303_Controls_verified720.png) | [1080](M13_Evidence/M13_03/m1303_Controls_verified1080.png) |

Additional 1600×900 evidence: [Controls](M13_Evidence/M13_03/m1303_Controls_verified900.png), [open dropdown](M13_Evidence/M13_03/m1303_dropdown_verified900.png), [rebind listening](M13_Evidence/M13_03/m1303_listening_verified900.png), [synthetic Escape cancellation](M13_Evidence/M13_03/m1303_cancel_verified900.png), [active plus focused category](M13_Evidence/M13_03/m1303_focused_verified900.png), [simulated read-only Audio](M13_Evidence/M13_03/m1303_readonly_verified900.png), and [simulated toggle-on state](M13_Evidence/M13_03/m1303_toggle_on_1080.png). The read-only screenshot temporarily changed the in-memory service flag, then restored it; the toggle-on image used SetIsOnWithoutNotify. Neither operation changed the real settings file. Listening and cancellation were initiated by programmatic events. Modal close while listening and subsequent reopening were also exercised in Editor Play Mode via the existing modal controller.

An earlier capture exposed overlapping dropdown items; their template item height was corrected and the linked open-dropdown capture shows the corrected presentation. The viewport shows the representative Korean labels without observed clipping at the captured sizes. This is not exhaustive glyph or localization-length coverage. Approximate rendered pixel samples in the 720p Text image give 8.6:1 for a representative normal text pixel versus its panel, 5.1:1 for a representative secondary-text pixel, 6.2:1 for the teal handle and 3.7:1 for a bright navigation-edge pixel. Anti-aliasing, translucent surfaces and darker edge pixels vary; these spot checks are not a comprehensive contrast audit or accessibility certification.

## Windows Player and remaining gates

The final production Windows x64 build completed successfully with **0 errors and 486 warnings** (including shader warnings), at `Builds/M13_03_Final/ProjectAllTime.exe`. A second build used a temporary alternate company identifier to isolate settings, saves and MetaProgress; the project identifier and incidental preloaded-asset change were restored afterwards. The isolated build also succeeded. Windows Computer Use rejected the attempt to launch the isolated executable because that app was not approved for Computer Use. Therefore no physical standalone Player scenario was executed or passed.

User gate: run the isolated or otherwise safely isolated Player; open Settings from title and gameplay, physically navigate and change a setting, exercise Cancel and rebind, close/reopen, and verify narrative progression. The actual in-game entry route and end-to-end narrative continuation remain unverified in this work; existing unit tests and the programmatic title route are not claimed as substitutes. Physical mouse/keyboard behavior, unavailable-mixer presentation, more extreme Korean text lengths and additional aspect ratios also remain for follow-up. User visual acceptance has not been granted.

Existing global and M13 dynamic TMP font atlas changes and unrelated evidence remain outside the implementation commit. No M13-04 work was started.
