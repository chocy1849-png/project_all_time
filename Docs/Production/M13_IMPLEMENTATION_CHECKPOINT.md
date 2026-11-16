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

## Second preview iteration

The current matching 1920×1080 Game View captures are [A](M13_Evidence/DesignCandidates/Direction_A/current_1920.png) and [B](M13_Evidence/DesignCandidates/Direction_B/current_1920.png). Both used the same scene content, bright production background and programmatic EventSystem selection of the Gallery tab. A preview-only focus component responds to select/deselect and pointer enter/exit; the capture demonstrates selected-state rendering, not physical keyboard navigation. A Toggle was added and the scrollbar moved away from the action buttons. The generator was regenerated independently again; all 20 output hashes matched.

The comparison still fails the desired visual quality: B's stretched grain is visibly blotchy, while A is comparatively flat. Current direction B is warmer and more angular, but requires a non-stretching material treatment. The current captures are evidence of progress, not a recommendation or final candidate. The previously listed 720p/900p images predate this iteration and need recapture. The production baseline audit, complete interaction and asset families, contrast, nine-slice, focused preview tests, and final reports remain outstanding.

Exact next action: replace stretched center grain with a separate subtle tiled or otherwise scale-stable material treatment in the preview and assets; render the same current A/B scenes again at 1920×1080, inspect the surface at 1:1, then proceed with the required background, resolution, interaction and audit validation.

## Third material experiment and regression

Full unfiltered EditMode regression executed after the second preview iteration: **541 passed, 0 failed, 0 skipped, 0 inconclusive**. This precedes the subsequent material-only experiment. The 22 PNGs now generated (including A/B tiled grain overlays) were regenerated into an isolated temporary folder and all SHA-256 hashes matched.

The [third B experiment](M13_Evidence/DesignCandidates/Direction_B/grain_experiment_1920.png) replaces stretched panel-center noise with a tiled overlay, but the result is too noisy and exposes missing Korean glyphs (e.g. in the toggle label and footer). It is not an acceptable candidate. `Application.runInBackground` was restored to its original false value. The existing dynamically modified Noto font asset was restored to the clean baseline; do not commit its incidental glyph-cache changes.

The current serialized A/B scenes now include the experimental overlay and therefore are newer than the matching A/B comparison images above. Neither the latest A nor the latest B is approved or validated. The interaction focus screenshot demonstrates programmatic selection only. Full asset-specific tests and the original task's remaining deliverables have not been completed.

**Next implementation action:** lower or redesign the tiled overlay until the actual Game View has subtle scale-stable grain, and create a dedicated M13 preview Korean TMP font/atlas or another reproducible glyph solution that preserves the existing font asset. Rebuild the scenes, capture both at actual 1080p and inspect glyphs and grain at 1:1. Then resume the outstanding baseline, matrices, states, 9-slice, contrast and documentation work.
