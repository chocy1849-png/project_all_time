# Current isolated preview captures

All images are Unity Editor Play Mode Game View `source=screen`; `Screen.width/height` was queried before capture. Current A/B match at 1920×1080, same bright project background, scroll at top and Gallery programmatically focused:

- [A 1080p](A_1080_v5.png) and [B 1080p](B_1080_v5.png).
- A: [900p](A_v5_900.png), [720p](A_v5_720.png), [sunset 720p](A_v5_sunset.png), [provisional title background 720p](A_v5_title.png).

Earlier screenshots in the other evidence directories predate the final metadata/scroll initialization change and are retained as intermediate evidence, not current serialized-state screenshots. The 1080p pair and v5 size/background set are the latest. Programmatic focus does not prove physical navigation. See the [working production report](../../M13_UI_ASSET_PRODUCTION_REPORT.md) for measurements and limitations.

The [v6 720p](A_v6_720.png) is newer than the v5 images and contains a brighter hint label. The v5 1080p A/B pair remains useful for earlier comparison but is no longer the current serialized appearance; new matching captures are required.

Current v7 actual Game View: [A 1080](A_v7_1080.png), [B 1080](B_v7_1080.png), [A 900](A_v7_900.png), [A 720](A_v7_720.png). These supersede v5 in the matching A/B and bright-background resolution comparison. v5 background variants predate the latest label brightness changes.

Current v7 background comparisons at actual 1280×720: [bright](A_v7_720.png), [sunset](A_v7_sunset.png), [provisional title](A_v7_title.png). Each used the same preview and programmatic Gallery focus.

Latest serialized scenes use two card rows. Compare [A v9 1080](A_v9_1080.png) and [B v9 1080](B_v9_1080.png); [720p top](A_v9_top.png) and [720p bottom](A_v9_bottom.png) demonstrate the two scroll extremes. Earlier v7 900p and background captures precede this content-layout change and must be refreshed before final validation.

Current v9 same-version captures: [A 900p](A_v9_900.png), [sunset 720p](A_v9_sunset.png), [title 720p](A_v9_title.png). Together with the existing v9 A/B 1080p and A 720p captures they supersede v7 for the sampled layout.

[Selected + focused tab at 720p](A_selected_focused_720.png): real Game View with EventSystem selection set to the already active tab. Teal surface and underline remain visible inside the gold focus outline. This is programmatic selection; a physical arrow-key navigation check is recorded in the production report.

Latest matching 1080p captures are [A v11](A_v11_1080.png) and [B v11](B_v11_1080.png). They contain selectable/focusable card Buttons and a disabled `잠김` card; all earlier 720p/900p/background captures predate this addition. [Card selection plus focus at 720p](A_v10_cards.png) demonstrates an earlier version before the lock label. These are Unity-rendered fixtures, not production menu redesigns.

## Current B after user feedback

Unity Editor Play Mode screen captures, B only: [720 top / selected and locked](B_feedback_top.png), [720 bottom / focused](B_feedback_bottom.png), [900 top](B_feedback_900.png), [1080 top](B_feedback_1080.png), [sunset 720](B_feedback_sunset.png), [provisional title 720](B_feedback_title.png). The provisional title asset renders black in this comparison. Background swaps and selected Game View sizes were preview-only; these are not production scenes. The 720 bottom was reached by physical wheel input; the selection and focus state shown on other captures is not evidence of a new physical test by itself. Current B screenshots supersede prior B layout evidence; earlier A/B comparisons are historical.
