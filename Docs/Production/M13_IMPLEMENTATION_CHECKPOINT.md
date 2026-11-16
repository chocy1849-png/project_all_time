# M13 implementation checkpoint — incomplete

This is a working checkpoint, not user visual approval or M13-00/01 completion. Branch `feat/m13-menu-ui-redesign`; base `9380e894f3e5b941e01a8272d85e8cb772eddc84`.

## Implemented

- `AgentScripts/generate_m13_ui_assets.py`: 20 original 128×128 RGBA assets, 10 each for A and B. An isolated regeneration produced matching SHA-256 hashes for all 20 files.
- `M13StylePreviewBuilder.cs` generates isolated A/B prefabs and scenes via Unity APIs. They contain uGUI Images, TMP text, Buttons, Slider and Scrollbar with a 1920×1080 CanvasScaler. They do not replace production menus.
- Unity importer inspection reported 20 Sprite assets with Full Rect, border (20,20,20,20), bilinear filtering, no mipmaps and uncompressed textures. This is settings inspection, not proof that every size renders correctly.
- M11 shared-theme focused EditMode tests: 6 passed, 0 failed, 0 skipped. No full regression was run.
- Game View screenshots: [A at 1920×1080](M13_Evidence/DraftA/actual_1920.png), [B at 1920×1080](M13_Evidence/DesignCandidates/Direction_B/actual_1920.png), [A at 1600×900](M13_Evidence/DraftA/actual_1600.png), [A at 1280×720 before track correction](M13_Evidence/DraftA/actual_1280.png), [A at 1280×720 after track correction](M13_Evidence/DraftA/refined_1280.png), [sunset](M13_Evidence/DraftA/sunset_1280.png), [provisional title background](M13_Evidence/DraftA/title_1280.png). Pipeline `capture_game_view` used `source=screen`, after checking `Screen.width/height` in Play Mode. A and B 1080 images precede the latest track correction; recapture both current prefabs before a final comparison.
- Earlier `DesignCandidates/*/preview.png` and `DraftA/refined_1920.png` were output at 1920×1080 from a 1280×720 Game View; they are resampled preliminary images, not 1080p layout tests.

## Known defects and remaining work

- A and B differ mainly in tone and edge treatment; they are not yet sufficiently distinct as material directions. Surface refinement made A flatter. Both need art direction work, not just tint adjustments.
- The scrollbar competes with adjacent buttons. The preview lacks a dynamic focus ring, content thumbnails, Toggle, complete destructive/disabled states, and functional scrolling content. Slider/Scrollbar input and keyboard focus have not been exercised.
- PNGs use a fixed 20px border even for small controls; nine-slice edge quality and varied sizes need dedicated inspection. The track was changed to a plain uGUI Image for legibility; it is not a validated production sprite solution.
- Contrast has not been measured against rendered composites. Korean readability needs inspection at actual sizes, especially metadata. Bright/dark tests were captured but not fully assessed.
- Production UI baseline captures and detailed audit and interaction matrices remain incomplete. Existing M12 evidence is historical and not a replacement for current-state verification.
- Complete asset families, direction comparison/refinement, validation report, quality rubric, review package, relevant automated asset and prefab tests, and user review gate remain incomplete. No visual acceptance is claimed.

## Exact next action

Rework A/B generator so A has fine non-stretched surface detail and B has a distinctly warmer matte treatment; rebuild with `Tools > M13 > Build style previews`, reposition the scrollbar, add a focus indicator driven by selection and a Toggle, then capture both current prefabs at actual 1920×1080 against the same bright background. Inspect those images before selecting a candidate. Continue the original M13-00/01 instructions afterward.
