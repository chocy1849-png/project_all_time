# M11 — UI visual validation report

**Status: incomplete; user visual acceptance has not been requested or granted.**

## Evidence and limitations

The reference canvas uses 1920 × 1080. The original technical baseline is [before_m2_technical.png](M11_Evidence/before_m2_technical.png); it is not a representative story-state baseline. An export of base commit `c53adc63701fe840590fa9c39b94ca182ba8e0b8` was prepared separately to capture actual before states, but its Unity Editor is blocked at the administrator-mode startup warning. The six required comparable before story-state screenshots remain outstanding; no reconstructed screenshots are represented as a baseline.

Actual composited Play Game View captures from the integrated production scene, with isolated test storage:

| Story state | After screenshot | Coverage |
| --- | --- | --- |
| S001 | [final_s001.jpg](M11_Evidence/final_s001.jpg) | bright kitchen dialogue |
| S003 | [final_s003.jpg](M11_Evidence/final_s003.jpg) | internal presentation; [Rope internal](M11_Evidence/final_s003_rope.jpg) at 720p |
| S004 | [final_s004.jpg](M11_Evidence/final_s004.jpg) | confrontation; [manual hold](M11_Evidence/final_s004_manual.jpg) at 720p |
| S005 | [final_s005.jpg](M11_Evidence/final_s005.jpg) | sunset dialogue |
| S007 | [final_s007.jpg](M11_Evidence/final_s007.jpg) | clinical information presentation |
| S008 | [final_s008.jpg](M11_Evidence/final_s008.jpg) | post-profile dialogue; [profile browser](M11_Evidence/final_s008_profile.jpg) at 720p |

The captures use actual Game View dimensions: [1280 × 720](M11_Evidence/final_1280.jpg), [1600 × 900](M11_Evidence/final_1600.jpg), and 1920 × 1080 (the story captures above). Sizing and visual appearance still require the user's review, including small controls at 720p and long Korean text. These do not constitute user visual acceptance.

## Automated and connected-Editor checks

- Unity 6000.3.21f1, unfiltered EditMode suite: **528 passed, 0 failed, 0 skipped**, in the final batch rerun after tooltip disable-state cleanup (2026-11-10 06:20–06:21 UTC).
- Focused tests exercise all 14 imported sprites, four prefab assets, theme application, authoritative presenter references, nonblocking decorations, and nine assigned scene buttons. Existing record wiring assertions were updated for the Korean icon tooltip.
- Connected-Editor Play traversed 379 canonical paragraphs in order from `PROLOGUE_S001_START`, exercised seven profile browser actions and observed the ending title with the diagram hidden. SaveData and MetaProgress used an isolated test root. The existing M10 tests cover the protected save compatibility contract; the story content and manifest were not edited.
- Live interaction checks exercised Next, Hide/restore, Backlog, Settings, Records, Save, Load, Skip and Auto, including modal tooltip cleanup and mode indicators. The continue indicator was confirmed visible on a fully displayed S001 line after correcting its component placement.
- Deterministic asset regeneration was checked by repeated SHA-256 comparison. Scalable sprites were imported with slice borders through Unity importer APIs.

## Remaining work

Capture corresponding six before states from the actual baseline, with S003 Rope internal dialogue, S004 manual hold and S008 profile browser explicitly evidenced; review the staged changes, and create the one final PR only after the requested technical criteria are satisfied. User visual approval is a separate pending check.
