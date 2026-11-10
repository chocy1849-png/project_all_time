# M11 — VN UI Design System Specification

**Status: production integration implemented; technical regression passed and user visual acceptance remains pending.**

## Direction and baseline

The approved direction is a restrained translucent dark dialogue panel, ivory text, a compact speaker label, warm-gold and teal accents, and secondary icon controls. The intended reference is 1920×1080, with dialogue occupying roughly 22–27% of screen height subject to visual review.

Inspection of `VN_Main` found its production LinePresenter on `DialogueLayer/LineContainer`, with `DialoguePanel`, `SpeakerNameContainer`, `DialogueText`, and `ContinueIndicator` descendants. The CanvasScaler uses a 1920×1080 reference. The existing `VNQuickControlBar` owns nine Button listeners; its current buttons are approximately 95–100×36 reference units. The separate Replay and other scene presenters must remain distinct.

Initial palette: panel `#252B32`, primary text `#F7F4EF`, speaker `#E5CAA2`, secondary accent `#81BBB7`, muted UI `#AAB5BD`. Final layout, typography, contrast, 9-slice appearance and active/disabled states require rendered review.

## Original sprite source

`python AgentScripts/generate_m11_ui_assets.py` requires Pillow and produces 14 original RGBA PNGs in `Assets/_Project/Art/UI/M11`: dialogue frame, nameplate, button and tooltip backgrounds, continue marker, and nine action icons. A repeated run produced identical SHA-256 hashes. The assets were imported using Unity's Sprite importer, bilinear filtering and Full Rect mesh; the four scalable backgrounds use 16-pixel (frame) or 14-pixel borders. The intended Image type for those backgrounds is Sliced.

## Production implementation

The shared `M11_UITheme` asset controls panel tint, text, speaker accent, button state colors and text sizes through `VNUIThemeBinding`. Runtime icon hover and disabled colors use the same asset. Editing the theme refreshes loaded bindings through a deferred Editor callback; runtime bindings also apply on enable. The theme also records icon sizing and spacing tokens. The existing Korean TextMesh Pro font remains in use.

The serialized `M11/VNDialogueFrame`, `VNNameplate`, `VNIconButton`, and `VNTooltip` prefabs are nested into the authoritative dialogue prefab or the production control bar. Frame and nameplate use sliced images and do not receive raycasts. The existing LinePresenter and dialogue text are retained. The continue marker is driven from the active line and interaction gate, without owning input.

The nine icon controls are Next, Hide, Backlog, Skip, Auto, Save, Load, Settings, and Records. Their existing `VNQuickControlBar` owns actions and derives Skip/Auto indicators from the existing controllers. Buttons use ColorBlock normal, highlighted/focused, pressed and disabled colors; the Korean tooltip is nonblocking and hidden when the control is disabled, the interface is hidden, or a modal blocks interaction.

## Review

The scene captures and actual validation results are documented in `M11_UI_VISUAL_VALIDATION_REPORT.md`. The report includes before and after story-state captures. User visual approval remains outstanding; no visual acceptance is implied.

## Visual review adjustment

The nine quick controls align to the existing bar’s right edge, approximately 25 reference pixels inside the dialogue frame. The continue marker sits above the row to avoid overlapping Records. Backlog readability corrections and the deferred M13 design notes are recorded in the visual validation report.
