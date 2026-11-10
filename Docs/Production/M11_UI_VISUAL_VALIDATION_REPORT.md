# M11 — UI visual validation report

**Status: integrated visual review evidence; user visual acceptance remains pending.**

## Evidence and limitations

The reference canvas uses 1920 × 1080. Baseline screenshots below were captured in Play from an isolated export of base commit `c53adc63701fe840590fa9c39b94ca182ba8e0b8`, using controlled progression through the production story and isolated save and meta-progress storage. Its complete run observed 379 canonical paragraphs in order, seven profile actions, and the ending title. The older [technical baseline](M11_Evidence/before_m2_technical.png) is retained separately.

Actual composited Play Game View captures from the integrated production scene, with isolated test storage:

| Story state | Before | After screenshot | Coverage |
| --- | --- | --- | --- |
| S001 | [baseline](M11_Evidence/before_s001.jpg) | [final_s001.jpg](M11_Evidence/final_s001.jpg) | bright kitchen dialogue |
| S003 | [baseline](M11_Evidence/before_s003.jpg) | [final_s003.jpg](M11_Evidence/final_s003.jpg) | internal presentation; Rope internal [before](M11_Evidence/before_s003_rope.jpg) at 1080p and [after](M11_Evidence/final_s003_rope.jpg) at 720p |
| S004 | [baseline](M11_Evidence/before_s004.jpg) | [final_s004.jpg](M11_Evidence/final_s004.jpg) | confrontation; manual hold [before](M11_Evidence/before_s004_manual.jpg) at 1080p and [after](M11_Evidence/final_s004_manual.jpg) at 720p |
| S005 | [baseline](M11_Evidence/before_s005.jpg) | [final_s005.jpg](M11_Evidence/final_s005.jpg) | sunset dialogue |
| S007 | [baseline](M11_Evidence/before_s007.jpg) | [final_s007.jpg](M11_Evidence/final_s007.jpg) | clinical information presentation |
| S008 | [baseline](M11_Evidence/before_s008.jpg) | [final_s008.jpg](M11_Evidence/final_s008.jpg) | post-profile dialogue; profile browser [before](M11_Evidence/before_s008_profile.jpg) at 1080p and [after](M11_Evidence/final_s008_profile.jpg) at 720p |

The captures use actual Game View dimensions: [1280 × 720](M11_Evidence/final_1280.jpg), [1600 × 900](M11_Evidence/final_1600.jpg), and 1920 × 1080 (the story captures above). Sizing and visual appearance still require the user's review, including small controls at 720p and long Korean text. These do not constitute user visual acceptance.

## Baseline audit

The original 1920 × 1080 CanvasScaler used a broad translucent gray lower panel with a dark rectangular speaker strip and white Korean text. Nine pale, text-labeled controls occupied the lower right, approximately 95–100 × 36 reference units each. The dialogue panel overlapped the lower bodies while leaving faces visible in the sampled scenes; information and profile panels were separate overlays. The original control visuals relied on standard Button color states and the existing Skip/Auto mode indicators. The baseline and redesigned captures let reviewers assess contrast and character occlusion against the same story states.

## Automated and connected-Editor checks

- Unity 6000.3.21f1, unfiltered EditMode suite: **529 passed, 0 failed, 0 skipped**, in the final unfiltered batch run (2026-11-10 06:28 UTC), including automatic theme refresh.
- Focused tests exercise all 14 imported sprites, four prefab assets, theme application, authoritative presenter references, nonblocking decorations, and nine assigned scene buttons. Existing record wiring assertions were updated for the Korean icon tooltip.
- Connected-Editor Play traversed 379 canonical paragraphs in order from `PROLOGUE_S001_START`, exercised seven profile browser actions and observed the ending title with the diagram hidden. SaveData and MetaProgress used an isolated test root. The existing M10 tests cover the protected save compatibility contract; the story content and manifest were not edited.
- Live interaction checks exercised Next, Hide/restore, Backlog, Settings, Records, Save, Load, Skip and Auto, including modal tooltip cleanup and mode indicators. The continue indicator was confirmed visible on a fully displayed S001 line after correcting its component placement.
- Deterministic asset regeneration was checked by repeated SHA-256 comparison. Scalable sprites were imported with slice borders through Unity importer APIs.

## Remaining work

User visual review and explicit acceptance remain outstanding. The application has not been declared visually accepted.
