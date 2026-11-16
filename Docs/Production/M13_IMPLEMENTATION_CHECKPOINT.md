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

## Fourth iteration — 2026-11-16, still incomplete

- Replaced the preview's dependency on the mutable production TMP atlas with a fixture-only static Noto SDF populated from the checked-in font source. After Editor restart and forced reimport, A labels had no missing glyphs. B's current labels were also checked before restart. This covers fixture strings, not all Korean.
- Reduced the separately tiled grain Image to alpha 0.16. Current matching actual 1080p A/B captures and A 720p/900p/background captures are indexed in `M13_Evidence/Current/README.md`. A is the working recommendation, not user approval.
- Added a project-image thumbnail with preserved aspect ratio, linked the Scrollbar to actual ScrollRect content, a preview-only initial scroll position, a destructive sample, and brighter metadata. Programmatic UI events changed Slider, Toggle and scroll state; physical pointer and keyboard behavior remain untested.
- Current A bright-background screenshot pixel sampling: title 7.40:1, metadata 5.14:1, hint 4.58:1 and disabled label 5.32:1. This is an approximate sample, not complete contrast certification.
- A representative 9-slice resizing image is indexed in the asset report; B and full dimension range remain unverified.
- Generator regeneration matched 22/22 hashes. Focused M13 tests passed 5/5. Full unfiltered EditMode after current implementation changes: 546 passed, 0 failed, 0 skipped, 0 inconclusive.
- Current production 1080p baseline captures are indexed in `M13_Evidence/Before/README.md`; isolated PlayerSettings identity was verified and restored. Save/Load empty states, Settings five categories, Records four tabs, Backlog, Title and game menu were captured. Populated/locked/unlocked/long states and physical input are outstanding.

**Next executable action:** exercise the preview with actual pointer and keyboard input, complete B and minimum/maximum 9-slice visual coverage, then capture production long/populated/locked states under isolated storage and finish the audit/interaction matrices. Measure indicator and text contrast over all required backgrounds and resolutions. The visual direction and both reports remain working drafts; do not mark ready for user review yet.

## Physical input pass and correction

In the connected Editor Game View, Windows input automation dragged the Slider from 0.65 to 0.3395, clicked the Toggle from true to false, and dragged the Scrollbar to normalized 0 with content offset 280. Clicking Gallery then pressing Right selected `TabNormal2`. A first hover/click over the disabled button incorrectly showed its focus ring; `M13PreviewFocus` was corrected to require `Selectable.IsInteractable`, and a focused test now covers that case. On the subsequent live pass, while the pointer was over Disabled, the Button reported `interactable=False` and the ring `activeSelf=False`. This verifies these specific interactions in the Editor at the observed Game View scale, not every input path or a Windows Player.

The full unfiltered EditMode suite was rerun after this correction: **546 passed, 0 failed, 0 skipped, 0 inconclusive**. Production baseline and visual validation gaps listed above still remain. Next: inspect B at multiple sliced sizes, complete populated/unlocked and long Backlog production state evidence, then finish the audit and interaction matrices and contrast measurements across backgrounds/resolutions before claiming readiness.

A matching B 9-slice size capture was added at `M13_Evidence/NineSliceTests/B_sizes.png`; the examined sample pairs appeared continuous at 1080p. Full dimension/control and multi-resolution coverage remains open.

A second isolated capture pass added Records seeded locked/unlocked states, gallery viewer, and named/narrator long Backlog fixtures; see the Before index. The long fixture exposes missing glyphs in the existing production TMP configuration for some test characters; preserve as an M13 integration issue, without silently altering the production font in M13-00/01. These test values did not touch normal player storage. Audit coverage and contrast/state matrices remain incomplete.

## Contrast adjustment — current working version

The prior 720p screenshot sample estimated the hint at 4.24:1 on each of the three tested backgrounds. The preview hint color was brightened and the rebuilt [actual 1280×720 capture](M13_Evidence/Current/A_v6_720.png) yielded approximate sampled ratios: hint 4.92:1, metadata 4.64:1, disabled label 4.49:1. Method: sRGB relative luminance of the 25th-percentile background and 99th-percentile pixel in each manually selected text box. Anti-aliasing and small sample selection limit precision; the disabled value is borderline and should be improved or remeasured rather than treated as passing. Full EditMode after the change: 546 passed, zero failed/skipped/inconclusive. The earlier 1080p A/B and 900p captures predate the hint-color change; capture matching current scenes at all resolutions/backgrounds before final review.

## v7 contrast refinement

The preview disabled label was brightened after the borderline v6 sample. The current actual 720p A capture produced approximate 25th-percentile background / 99th-percentile text luminance ratios of metadata 4.84:1, hint 5.15:1 and disabled label 5.87:1. This remains a limited screenshot sample, not full accessibility verification. The matching current 1080p comparison uses [A](M13_Evidence/Current/A_v7_1080.png) and [B](M13_Evidence/Current/B_v7_1080.png); A is also rendered at [900p](M13_Evidence/Current/A_v7_900.png) and [720p](M13_Evidence/Current/A_v7_720.png). Game View dimensions were checked before each capture. Background tests still use the previous v5 version and need recapture to represent current labels.

Full unfiltered EditMode after v7 label adjustment: **546 passed, 0 failed, 0 skipped, 0 inconclusive**. Remaining: current-version background recaptures, all-state contrast and indicator measurements, production populated save and empty Backlog baselines, a complete per-screen hierarchy/scroll/font/state audit, and final quality review. Do not report ready until those are complete.
