# M11 — VN UI Design System Specification

**Status: work in progress.** The production scene has not yet been redesigned or visually accepted.

## Direction and baseline

The approved direction is a restrained translucent dark dialogue panel, ivory text, a compact speaker label, warm-gold and teal accents, and secondary icon controls. The intended reference is 1920×1080, with dialogue occupying roughly 22–27% of screen height subject to visual review.

Inspection of `VN_Main` found its production LinePresenter on `DialogueLayer/LineContainer`, with `DialoguePanel`, `SpeakerNameContainer`, `DialogueText`, and `ContinueIndicator` descendants. The CanvasScaler uses a 1920×1080 reference. The existing `VNQuickControlBar` owns nine Button listeners; its current buttons are approximately 95–100×36 reference units. The separate Replay and other scene presenters must remain distinct.

Initial palette: panel `#252B32`, primary text `#F7F4EF`, speaker `#E5CAA2`, secondary accent `#81BBB7`, muted UI `#AAB5BD`. Final layout, typography, contrast, 9-slice appearance and active/disabled states require rendered review.

## Original sprite source

`python AgentScripts/generate_m11_ui_assets.py` requires Pillow and produces 14 original RGBA PNGs in `Assets/_Project/Art/UI/M11`: dialogue frame, nameplate, button and tooltip backgrounds, continue marker, and nine action icons. A repeated run produced identical SHA-256 hashes. The assets were imported using Unity's Sprite importer, bilinear filtering and Full Rect mesh; the four scalable backgrounds use 16-pixel (frame) or 14-pixel borders. The intended Image type for those backgrounds is Sliced.

## Remaining work

Create and apply a shared theme and the reusable icon-button, tooltip, nameplate and dialogue-frame prefabs. Integrate them in the actual scene while preserving the LinePresenter, quick-control listener ownership, modal/input arbitration and SaveData contracts. Capture comparable story-state screenshots at the requested resolutions, complete interaction and regression testing, and prepare the final visual validation report and PR. User visual acceptance remains pending.
