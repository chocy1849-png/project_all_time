# M13-04 production capture index

All `save_*`, `load_*`, and `overwrite_programmatic_*` captures use the production `VN_Main` scene. New Game and Save were opened through the production title and convenience controllers, then a temporary isolated repository was populated with test fixtures. Save autosaves were disabled and MetaProgress storage was redirected to a temporary directory. `Screen.width` and `Screen.height` were queried after selecting each fixed Game View size. The composited screen capture is programmatic; no physical Player input is claimed. The overwrite confirmation was displayed programmatically using the production modal and a fixture model; actual confirmation and cancellation were separately tested through the production controller.

| View | 1280×720 | 1600×900 | 1920×1080 |
|---|---|---|---|
| Save Manual page 1, empty/valid/corrupt/unsupported, missing thumbnail | [720](save_manual_states_720.png) | [900](save_manual_states_900.png) | [1080](save_manual_states_1080.png) |
| Save Manual page 2 | [720](save_manual_page2_720.png) | [900](save_manual_page2_900.png) | [1080](save_manual_page2_1080.png) |
| Save Auto | [720](save_auto_720.png) | [900](save_auto_900.png) | [1080](save_auto_1080.png) |
| Save Quick | [720](save_quick_720.png) | [900](save_quick_900.png) | [1080](save_quick_1080.png) |
| Load Manual | [720](load_manual_720.png) | [900](load_manual_900.png) | [1080](load_manual_1080.png) |
| Load Quick | [720](load_quick_720.png) | [900](load_quick_900.png) | [1080](load_quick_1080.png) |
| Overwrite confirmation (programmatic presentation) | [720](overwrite_programmatic_720.png) | [900](overwrite_programmatic_900.png) | [1080](overwrite_programmatic_1080.png) |

[Final Load selected + focused, 900p](load_selected_focus_final_900.png). [Final Title Load, 900p](title_load_final_900.png); [long Korean and English titles and JPEG fixture](title_long_thumbnail_900.png); [selected long title](title_long_selected_900.png). Earlier `title_load_720/900/1080` captures predate final navigation styling and are historical, not final evidence.

The focused test and full regression JSON reports are included separately. [Bright kitchen before](gameplay_bright_before_900.png) / [Save](gameplay_bright_save_900.png) and [dimmer storage before](gameplay_storage_before_900.png) / [Load](gameplay_storage_load_900.png) show production backgrounds selected programmatically during gameplay. The storage background is dimmer but is not a separately verified dark scene. Physical input and independent Player smoke remain outstanding.
