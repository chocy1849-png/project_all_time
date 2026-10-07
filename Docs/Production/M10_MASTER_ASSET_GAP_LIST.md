# M10-03 — Master Asset Gap List / Actual Disposition

Status: **84/84 REQUIREMENTS AUDITED / READY FOR REVIEW**. Actual Unity AssetDatabase and current Scene/catalog inspection are recorded in [Inventory](M10_CURRENT_ASSET_INVENTORY.md). [Temporary Mapping](M10_TEMPORARY_ASSET_MAPPING.md) defines reversible technical treatment; [Production ID Map](M10_PRODUCTION_ID_MAP.md) freezes lookup identities; [Docs/07](../07_ASSET_MANIFEST.md) contains 43 PLANNED resources. The frozen [Presentation Map](M10_PROLOGUE_PRESENTATION_MAP.md) still supplies 103 beats; every original Requirement Ref, meaning and constraint remains traceable. No production media, Yarn or runtime implementation is created.

Date: 2026-10-07 Asia/Seoul. M10-03 base: `e2dec902c3c99ae9c0222cdb3bfbe44197d60f1f`, merged PR #48 included. Canonical source original authoring SHA-256: `8413E847DD66D7D45585847A935B9C384D91FACFE9724EE27084F3D86FF977E9`; source and Presentation Map unchanged. Inventory availability does not override canon.

## Planning policy and request timing

`REQ-*` keys are preserved documentation references only, never final asset IDs, paths, filenames or runtime catalog entries. A row can represent a semantic set / variant or a reusable crop; totals count requirements, **not production file quantities**. For example, child expression states cover both named children; photo/detail/profile sets are not promises of one file per card. M10-03 preserves all 84 refs while selecting concrete resources and shared component/composition ownership. Each row is referenced by at least one beat; the reading-mode and ordinary-transition roles are also shared cross-scene requirements.

P0 = meaning / interaction cannot be fairly validated without it. P1 = major visual/emotional/audio value, still understandable with canonical text. P2 = polish / causal incidental action rendering. OPTIONAL = only if composition demonstrates additional value. P0 neutral placeholders are for technical layout/spikes only; they do not pass meaningful production validation. Text-only memo / readable paper / profiles can be semantically valid when exact content and interaction remain intact. No semantically wrong substitute is allowed.

Production Status now records the final M10-03 disposition: MATCHED_PRODUCTION, TEMP_TECHNICAL, NEW_PRODUCTION_REQUIRED, NO_SEPARATE_ASSET_REQUIRED, TECHNICAL_REVIEW_M10_04, OPTIONAL_DEFER, PROVENANCE_BLOCKED. A neutral temporary mapping does not change a confirmed production gap into TEMP_TECHNICAL. Both final BGM rows remain NEW_PRODUCTION_REQUIRED; all old Free-workflow tracks remain REFERENCE_ONLY. Current M4 BGM files are reference-only for production and fixture-only for command smoke, with no inferred generation-plan history. The first prioritized user briefs are in [Temporary Mapping](M10_TEMPORARY_ASSET_MAPPING.md#first-user-production-requests); no generation occurs.

Continuous Ambient is distinguished from M4 one-shot SFX. Any continuous realization is `TECHNICAL_REVIEW_M10_04`; M4 has no independent Ambient-loop subsystem. Optional home bed can be omitted; authored rain handoff must remain audible, with a finite cue realization acceptable only after review. No decorative sound quota, clue stinger, laugh track or completion fanfare. Fixed-pose sprites / Head expression swaps constrain action staging; narration and coherent composition may carry body actions without a new pose system.

Sound priority does not cancel canon: a P2 authored sound may be absent only in temporary technical work, not final authored-cue production review. Optional inferred action renderings are identified in Notes and can be omitted where prose / visual staging suffices. S002 authored music lowering also needs M10-04 review; current source-level gain is catalog/fade-owned, not a dedicated story operation. User audio settings and the save schema must stay unchanged.

| Phase | When requests become actionable |
| --- | --- |
| M10-02 | Semantic requirements known; no user production request |
| M10-03 | Actual inventory compared; first real P0 requests can be formulated with confirmed gaps and IDs |
| M10-04 | Technical prototypes refine UI / effect / continuous-audio feasibility, without rewriting canon |
| M10-05 | S001–S003 playable; refine first-slice production needs |
| M10-06 | S004–S006 playable; refine storage / farewell / return needs |
| M10-07 | S007–S008 playable; refine clinical / profile / connection needs |
| M10-08 | Remaining P1/P2 production and polish after meaningful play review |

Needed By is a later readiness target, not a request sent now. Reversible technical placeholders must preserve semantic relationships; unknown availability never authorizes a futuristic lab for a kitchen or a funeral track for breakfast.

## Totals

**84 requirement rows**, all final dispositions resolved against actual inventory. **103 presentation beats** reference them. Priorities: P0 23, P1 42, P2 17, OPTIONAL 2.


| Type | Requirement rows |
| --- | --- |
| BG | 12 |
| Character Sprite | 9 |
| Expression | 9 |
| Insert Image | 9 |
| CG | 1 |
| BGM | 2 |
| Ambient | 2 |
| SFX | 20 |
| UI Overlay | 13 |
| Diagram | 1 |
| Profile UI | 2 |
| Transition Effect | 4 |


## Requirement records

For each category, three keyed tables preserve the original definition/constraint fields and now include actual disposition, lookup identity, provenance, temporary treatment and request timing. Requirement Ref joins the definition, disposition and constraint rows. Type remains explicit. Anchors allow beat links to jump to the relevant row. Runtime lookup IDs are linked below; file names, final palette and production pixel dimensions remain unfrozen.


### BG

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-bg-001"></a>REQ-BG-001 | S001 | BG | Morning family kitchen / occupied table | Ordinary home and deliberate fifth place | Pan-first reveal, later five-person table | P0 |
| <a id="req-bg-002"></a>REQ-BG-002 | S001, S006 | BG | Family-home entrance and adjacent wall | Arrival and normal departure share a recognizable place | Wide entry; right-side normal exit and wall overlap | P0 |
| <a id="req-bg-003"></a>REQ-BG-003 | S002–S004 | BG | Family living room, photos and access to hall | Domestic conversation and memory association | Stable wide conversation area; retreat from hall | P1 |
| <a id="req-bg-004"></a>REQ-BG-004 | S003, S004 | BG | Narrow hall with closed storage-room door | Avoidance and patient-controlled approach | Hall linked visibly to living room / table | P0 |
| <a id="req-bg-005"></a>REQ-BG-005 | S004 | BG | Storage room with shoe box, returned bag, belongings envelopes | Concrete avoided memory | Wide reveal before selected close-ups | P0 |
| <a id="req-bg-006"></a>REQ-BG-006 | S005 | BG | Storage doorway looking into occupied living room | Family presence survives fact acceptance | Foreground treatment exchange with visible family beyond | P0 |
| <a id="req-bg-007"></a>REQ-BG-007 | S005 | BG | Same family table at sunset | Patient chooses a last meal | Memory-scene change, family-inclusive meal and farewell | P1 |
| <a id="req-bg-008"></a>REQ-BG-008 | S006 | BG | Jinhee bedroom in future city, daytime | Ground actual awakening and functioning care | Bed opposite Jihun, equipment and calm daylight | P0 |
| <a id="req-bg-009"></a>REQ-BG-009 | S006 | BG | Residential-area corridor | Procedural departure continuity | Brief route from bedroom toward street | P2 |
| <a id="req-bg-010"></a>REQ-BG-010 | S006 | BG | Bright future-city street | Care and reassignment continue in ordinary daylight | Public conversation after home exit | P1 |
| <a id="req-bg-011"></a>REQ-BG-011 | S007 | BG | Dokyeong office with desk and window | Life evidence precedes explanation | Two-person consultation with Rope below desk | P1 |
| <a id="req-bg-012"></a>REQ-BG-012 | S008 | BG | Immersion preparation room with screen and bed | Approved preparation and connection | Eunjeong, Jihun, Rope; profile review then patch setup | P0 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-BG-001 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_family_kitchen_morning; bg_family_table_morning | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-BG-002 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_family_entrance | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 / M10-06 | — |
| REQ-BG-003 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_family_living_room | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-BG-004 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_family_hall | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 / M10-06 | — |
| REQ-BG-005 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_family_storage | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-BG-006 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_family_storage_doorway | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-BG-007 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_family_table_sunset | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-BG-008 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_jinhee_bedroom_day | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-BG-009 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-06 | — |
| REQ-BG-010 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_future_city_street_day | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-BG-011 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_dokyung_office | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |
| REQ-BG-012 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bg_immersion_preparation | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-BG-001 | No patient, dream or hospital identifiers | Five eggs and five occupied places; room for dog, faces and dialogue | S005 layout foundation | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-002 | No future machinery before S006 exterior reveal | Retain ordinary entrance while white outline is localized beside it | Arrival / departure | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-003 | No clue markers or visible accident papers | Seated Jihun can follow hall gaze without getting up | S002 / S003 retreat / S004 | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-004 | No threatening architecture or face distortion | Closed handle readable; Minseok can stand without forcibly holding anyone | Two approaches | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-005 | Document names/date unreadable until approach | Objects coherent; paper slips from beneath box at its cue, not an early death display | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-006 | No vanished family or empty places | Allow Jinhee kneeling / child hand contact through composition or prose; faces stay clear | May reuse storage / living components | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-007 | No clock or global time acceleration | Same table geometry; family stays present, with room sound audible | Kitchen composition variant | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-008 | Do not make home a threatening lab or guarantee recovery | Patient orientation and equipment checks legible; natural light | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-009 | No surveillance motif | Ordinary occupied residential setting; do not prolong lonely-chair imagery | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-010 | No mysterious monitoring screen or hostile system voice | Readable daylight, no anomaly recurring here | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-011 | No grand neural laboratory spectacle | Record screen and simple diagram secondary; window gaze possible | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-BG-012 | No next-era scene or helper identity reveal | Profiles readable; bed-side preparation unobstructed | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |


### Character Sprite

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-char-001"></a>REQ-CHAR-001 | S001–S008 | Character Sprite | 지훈 | Procedural care and personal depletion | Spoken focus; restrained pauses / pain | P0 |
| <a id="req-char-002"></a>REQ-CHAR-002 | S001–S006 | Character Sprite | 진희, household and awake-care states | Agency and grief remain her own | Warm breakfast, uncertain recall, grief, consent, orientation | P0 |
| <a id="req-char-003"></a>REQ-CHAR-003 | S001–S005 | Character Sprite | 민석 | Affectionate family projection and avoidance | Conversation, hall presence, table / sink context | P1 |
| <a id="req-char-004"></a>REQ-CHAR-004 | S001–S005 | Character Sprite | 나래 | Family presence and ordinary banter | Table / living room, authored off-screen replies, coat handoff | P1 |
| <a id="req-char-005"></a>REQ-CHAR-005 | S001–S005 | Character Sprite | 준호 | Old/new shoe motive and repeating tomorrow | Family conversation, shoes, independent fitting | P0 |
| <a id="req-char-006"></a>REQ-CHAR-006 | S001–S006 before awakening | Character Sprite | Small ordinary white dog / Rope inside connection | Ground familiar dog before channel reveal | Under table, head lift, ankle touch, departure | P0 |
| <a id="req-char-007"></a>REQ-CHAR-007 | S006 after L924, S007, S008 | Character Sprite | Actual cyber-dog Rope | First physical mechanical reveal after awakening | Metal joints at stretch; desk / bedside presence; public speech | P0 |
| <a id="req-char-008"></a>REQ-CHAR-008 | S007 | Character Sprite | 이도경 / spoken label 도경 | Attentive functioning clinician | Questions, record comparison, prescription | P1 |
| <a id="req-char-009"></a>REQ-CHAR-009 | S008 | Character Sprite | 은정 | Competent consent and preparation guide | Briefing, profile screen, patch placement | P1 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-CHAR-001 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | jihun; jihun / default; jihun / uneasy | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-CHAR-002 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | jinhee; jinhee / default; jinhee / smile; jinhee / uneasy; jinhee / crying; jinhee / firm | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-CHAR-003 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | minseok; minseok / default; minseok / quiet | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-CHAR-004 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | narae; narae / default | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-CHAR-005 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | junho; junho / default; junho / pout; junho / eager | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-CHAR-006 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | rope_memory; rope_memory / default; rope_memory / alert | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-CHAR-007 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | rope; rope / default; rope / alert | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-CHAR-008 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | dokyung; dokyung / default | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |
| REQ-CHAR-009 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | eunjung; eunjung / default | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-CHAR-001 | No invented sentimental collapse | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-CHAR-002 | No horror face, guaranteed recovery or disappearance | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-CHAR-003 | No villain transformation or physical coercion | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-CHAR-004 | No ghost or memory-flash treatment | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-CHAR-005 | No hallucinated extra voice | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-CHAR-006 | No metal, circuitry, machine labeling or cyber silhouette before L924 | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-CHAR-007 | No early substitution for ordinary dog | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-CHAR-008 | No ridicule, incompetence or false headache diagnosis | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-CHAR-009 | No hidden coercion / helper identity | Fixed-pose M3 base; meaningful actions may use framing / narration rather than a new pose command | Character across listed scenes | M10-03 audited: see disposition and frozen lookup map above. |


### Expression

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-exp-001"></a>REQ-EXP-001 | S001, S002, S005 | Expression | Jinhee warm ordinary expression | Affection without menace | Meaningful Head expression/state changes | P1 |
| <a id="req-exp-002"></a>REQ-EXP-002 | S001–S004 | Expression | Jinhee small hesitation / recollection | Contradiction carried by performance | Meaningful Head expression/state changes | P1 |
| <a id="req-exp-003"></a>REQ-EXP-003 | S004–S006 | Expression | Jinhee grief / tears / resolve | Accepted fact differs from return willingness | Meaningful Head expression/state changes | P1 |
| <a id="req-exp-004"></a>REQ-EXP-004 | S002, S003, S006, S007 | Expression | Jihun pause / discomfort / composed attention | Symptoms precede white door | Meaningful Head expression/state changes | P1 |
| <a id="req-exp-005"></a>REQ-EXP-005 | S001–S005 | Expression | Minseok ordinary attentive / quiet state | Family protection without villainy | Meaningful Head expression/state changes | P2 |
| <a id="req-exp-006"></a>REQ-EXP-006 | S001–S005 | Expression | Narae ordinary and Junho pout / eager states | Household banter and shoe anticipation | Meaningful Head expression/state changes | P2 |
| <a id="req-exp-007"></a>REQ-EXP-007 | S007 | Expression | Dokyeong serious attentive state | Gives time to answer without ridicule | Meaningful Head expression/state changes | P2 |
| <a id="req-exp-008"></a>REQ-EXP-008 | S008 | Expression | Eunjeong neutral professional state | Cooperation briefing is ordinary care | Meaningful Head expression/state changes | P2 |
| <a id="req-exp-009"></a>REQ-EXP-009 | S002, S003, S006–S008 | Expression | Rope alert / resting head states | Notice symptom and maintain presence | Meaningful Head expression/state changes | P2 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-EXP-001 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | jinhee / smile | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-EXP-002 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | jinhee / uneasy | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-EXP-003 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | jinhee / crying; jinhee / firm | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-EXP-004 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | jihun / uneasy | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-EXP-005 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | minseok / quiet | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_08_POLISH | M10-05 | — |
| REQ-EXP-006 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | narae / default; junho / pout; junho / eager | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_08_POLISH | M10-05 | — |
| REQ-EXP-007 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | dokyung / default | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |
| REQ-EXP-008 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | eunjung / default | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |
| REQ-EXP-009 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | rope_memory / alert; rope / alert | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_05_REVIEW | M10-05 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-EXP-001 | No added horror, romance or early machinery | Smile / head lowered are restrained; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-EXP-002 | No added horror, romance or early machinery | Small shift; no shock flash; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-EXP-003 | No added horror, romance or early machinery | No instant composure or recovery guarantee; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-EXP-004 | No added horror, romance or early machinery | Do not imply an additional memory or spoken thought; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-EXP-005 | No added horror, romance or early machinery | No threatening face distortion; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-EXP-006 | No added horror, romance or early machinery | Distinct children, no creepy identical faces; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-EXP-007 | No added horror, romance or early machinery | No smile at failed taste response; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-EXP-008 | No added horror, romance or early machinery | No flirtatious framing; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-EXP-009 | No added horror, romance or early machinery | Dog paw touch / stretch are composition review, not invented Head-only body animation; fixed-pose Head swap limits apply | Listed character states across scenes | M10-03 audited: see disposition and frozen lookup map above. |


### Insert Image

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-ins-001"></a>REQ-INS-001 | S001 | Insert Image | Frying pan with five fried eggs | Establish planned fifth place before faces | First kitchen reveal crop | P0 |
| <a id="req-ins-002"></a>REQ-INS-002 | S001, S004 | Insert Image | Small shoe with folded worn heel and child heel outside | Size and wear drive recollection | Authored close-up then reusable prop detail | P1 |
| <a id="req-ins-003"></a>REQ-INS-003 | S001 | Insert Image | Visit memo: 다음 방문 — 아침 식사 후 | Previous visits are implied before explanation | Brief readable memo insert | P0 |
| <a id="req-ins-004"></a>REQ-INS-004 | S002 | Insert Image | Beach photos: Junho with sandy hands / snack; strong sunlight sea | Travel detail and Minseok returning to earlier photo | Optional focused photo view when wide view insufficient | P1 |
| <a id="req-ins-005"></a>REQ-INS-005 | S004 | Insert Image | Returned bag, sandy towel and folded snack wrapper | Returned-life contradiction becomes tangible | Focused contents if wide scene cannot show them | P1 |
| <a id="req-ins-006"></a>REQ-INS-006 | S004 | Insert Image | Previously read accident paper, three family names and same accident date | Physical recognition of known fact | Readable close-up only after approach and slipping envelope | P0 |
| <a id="req-ins-007"></a>REQ-INS-007 | S004, S005 | Insert Image | Unused new shoe and sole in Jinhee palm | Future hoped for but not reached; later shoe fitting | Condition-focused close-up then coherent wide prop | P1 |
| <a id="req-ins-008"></a>REQ-INS-008 | S006 | Insert Image | Present-day table matching family-house composition | Brief contrast after actual-world exit | Short matched composition glimpse within corridor / street transition | P1 |
| <a id="req-ins-009"></a>REQ-INS-009 | S005 | Insert Image | Blue cup mistaken for Junho cup | Ordinary household mistake | Only if wide props cannot communicate the action | OPTIONAL |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-INS-001 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | bg_family_kitchen_morning | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — (included in kitchen REQUEST_NOW) | M10-05 | — |
| REQ-INS-002 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | cg_worn_child_shoe | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-INS-003 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-05 | — |
| REQ-INS-004 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | cg_beach_photo_child; cg_beach_photo_sea | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-INS-005 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | cg_returned_bag_contents | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-INS-006 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-06 | — |
| REQ-INS-007 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | cg_unused_new_shoe | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-INS-008 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | cg_present_table_glimpse | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-INS-009 | OPTIONAL_DEFER | NONE; no suitable production media | — (optional; no identity) | Neutral frame/silhouette or no audio; technical-only; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | OPTIONAL_DEFER | M10-06 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-INS-001 | No explanatory treatment label | Pan before characters; five unambiguous eggs | Scene-local | Can be a crop / composition of kitchen BG rather than a separate file; M10-03 decides. |
| REQ-INS-002 | No document / accident reveal with shoe | Old small shoe distinct from new unused shoe | S004 worn-heel comparison | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-INS-003 | No writer name, hospital insignia or patient label | Only authored wording; ordinary household board | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-INS-004 | No accident aftermath or new memory scene | Two authored contents; no mystery zoom or metadata clue | Scene-local | Wide scene can suffice; M10-03 chooses two focused CG photographs; see ID map. |
| REQ-INS-005 | No invented returner identity | Bag is returned, not laundry; associated belongings coherent | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-INS-006 | Not readable in opening storage wide shot | Do not invent exact calendar date, agency seal or extra lore; cover / reopen readable states | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-INS-007 | No sentimental added memory montage | Unworn sole; differs from old folded heel; wrapping removal at cue | Farewell fitting | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-INS-008 | No wife / daughter hint or prolonged empty-chair zoom | Brief only; no new explanatory scene or inferred owner | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-INS-009 | No clue label | Subordinate to family faces and dialogue | Scene-local | M10-03 audited: see disposition and frozen lookup map above. |


### CG

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-cg-001"></a>REQ-CG-001 | S001, S005 | CG | Five-person table / family-inclusive meal composition, lighting variants | Occupied fifth place and continuing family presence | Optional ensemble or hybrid BG/sprites solution | OPTIONAL |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-CG-001 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | bg_family_table_morning; bg_family_table_sunset | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | REQUEST_NOW | M10-05 / M10-06 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-CG-001 | No empty fifth chair, fading family or sentimental Jihun addition | Five people and Rope at S001; relevant family remains visibly present through S005 | Morning / sunset composition | M10-03 composition resolved in Temporary Mapping; alternate solution, not an extra required CG quota. |


### BGM

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-bgm-001"></a>REQ-BGM-001 | S001–S003 | BGM | Light ordinary morning theme | Support ordinary life; contradiction stays within it | Delayed entrance; lowered continuity; fade at dog touch | P1 |
| <a id="req-bgm-002"></a>REQ-BGM-002 | Late S005–S006 L892 | BGM | Low single-note melody | Quiet farewell after meal has progressed | Late entrance; continue normal exit; immediate stop at disturbance | P1 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-BGM-001 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bgm_ordinary_morning | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-04 level-continuity review; M10-05 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-BGM-002 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | bgm_quiet_farewell | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-BGM-001 | No sadness / suspense / dream signal | No laugh accent, stinger or ominous escalation | S001–S003 continuity | M10-04 review: authored lowering while preserving musical continuity is not an existing dedicated operation; keep user settings and save schema unchanged. |
| REQ-BGM-002 | No victory, disappearance or cosmic implication | Restrained low single-note role; utensils remain audible | Meal / departure continuity | M10-03 audited: see disposition and frozen lookup map above. |


### Ambient

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-amb-001"></a>REQ-AMB-001 | S001–S005 | Ambient | Optional continuous ordinary household room bed | Life remains audible under lowered music and silence | Low environmental bed only if discrete cues leave an unnatural gap | P2 |
| <a id="req-amb-002"></a>REQ-AMB-002 | S008 ending | Ambient | Rain replacing connection sound | Audio-led entry boundary before next era | Gradual rain handoff under fade / black | P1 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-AMB-001 | OPTIONAL_DEFER | NONE; no suitable production media | — (optional; no identity) | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | OPTIONAL_DEFER | M10-04 review; M10-08 polish | — |
| REQ-AMB-002 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | sfx_connection_rain_handoff | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-04 review; M10-07 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-AMB-001 | No ghost drones or laugh track | No separate looping Ambient subsystem assumed | Home interior | TECHNICAL_REVIEW_M10_04; optional continuous realization, not permission to create a subsystem. |
| REQ-AMB-002 | No whisper or suggestive memory audio | Must hand off from equipment smoothly without new global audio owner | Scene-local | TECHNICAL_REVIEW_M10_04; continuous rain behavior is unimplemented; onset / bus remain separate SFX roles. |


### SFX

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-sfx-001"></a>REQ-SFX-001 | S001 | SFX | Oil frying | Household sound before image | Black opening | P1 |
| <a id="req-sfx-002"></a>REQ-SFX-002 | S001 | SFX | Plate placed on counter | Ground kitchen before image | Black opening | P2 |
| <a id="req-sfx-003"></a>REQ-SFX-003 | S001 | SFX | Doorbell | Arrival interrupts breakfast | L77 before entry cut | P1 |
| <a id="req-sfx-004"></a>REQ-SFX-004 | S001 | SFX | Junho footsteps toward entrance | Child moves before father rises | L89 narration | P2 |
| <a id="req-sfx-005"></a>REQ-SFX-005 | S001 | SFX | Extra cutlery taken from drawer | Fifth guest place is familiar | L90 | P2 |
| <a id="req-sfx-006"></a>REQ-SFX-006 | S002 | SFX | One close chair-leg scrape | Jihun personal hesitation / symptom | L294 once | P1 |
| <a id="req-sfx-007"></a>REQ-SFX-007 | S003 | SFX | Junho laugh repeated identically once | Ordinary activity repeats | L338 from living room, then identical replay | P1 |
| <a id="req-sfx-008"></a>REQ-SFX-008 | S004 | SFX | Handle / ordinary door opening | Only patient opens after manual hold | After explicit release, L539–544 | P2 |
| <a id="req-sfx-009"></a>REQ-SFX-009 | S004 | SFX | Bag / towel movement | Objects read as handled evidence | L549–551 | P2 |
| <a id="req-sfx-010"></a>REQ-SFX-010 | S004 | SFX | Envelope / paper slip, cover and reopen | Readable document follows handling | L556–565 | P2 |
| <a id="req-sfx-011"></a>REQ-SFX-011 | S004 | SFX | Shoe wrapping removed | Unused sole becomes visible | L590–592 | P2 |
| <a id="req-sfx-012"></a>REQ-SFX-012 | S005 | SFX | Meal dish / cutlery sounds | Room sound persists between dialogue | Late meal L779–784 and farewell | P1 |
| <a id="req-sfx-013"></a>REQ-SFX-013 | S005 | SFX | Running water / plates being washed | Minseok remains present during consent | L840–842 | P2 |
| <a id="req-sfx-014"></a>REQ-SFX-014 | S006 | SFX | Two slipper toe taps | Ordinary departure baseline | L883–885 exactly twice | P2 |
| <a id="req-sfx-015"></a>REQ-SFX-015 | S006 | SFX | Very low vibration | Localized sensory anomaly | L887 | P1 |
| <a id="req-sfx-016"></a>REQ-SFX-016 | S006 | SFX | Short equipment termination sound | Actual connection ends | L918 on daylight reveal | P1 |
| <a id="req-sfx-017"></a>REQ-SFX-017 | S006 | SFX | Equipment bag close | Procedural completion and departure | L984–986 | P2 |
| <a id="req-sfx-018"></a>REQ-SFX-018 | S008 | SFX | Equipment connection sound | Connection leads into rain | L1375 after spoken connection | P1 |
| <a id="req-sfx-019"></a>REQ-SFX-019 | S008 | SFX | Large raindrop on awning | First sensory rain cue | L1377–1378 under fade / black | P1 |
| <a id="req-sfx-020"></a>REQ-SFX-020 | S008 | SFX | Old bus brake | Ending audio situates next boundary | L1379 after rain onset | P1 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-SFX-001 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_frying_oil | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-SFX-002 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_plate_counter | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_05_REVIEW | M10-05 | — |
| REQ-SFX-003 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_home_doorbell | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-SFX-004 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_child_entry_steps | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_05_REVIEW | M10-05 | — |
| REQ-SFX-005 | OPTIONAL_DEFER | NONE; no suitable production media | — (optional; no identity) | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | OPTIONAL_DEFER | M10-05 | — |
| REQ-SFX-006 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_chair_leg_scrape | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-SFX-007 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_junho_laugh | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_NOW | M10-05 | — |
| REQ-SFX-008 | OPTIONAL_DEFER | NONE; no suitable production media | — (optional; no identity) | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | OPTIONAL_DEFER | M10-06 | — |
| REQ-SFX-009 | OPTIONAL_DEFER | NONE; no suitable production media | — (optional; no identity) | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | OPTIONAL_DEFER | M10-06 | — |
| REQ-SFX-010 | OPTIONAL_DEFER | NONE; no suitable production media | — (optional; no identity) | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | OPTIONAL_DEFER | M10-06 | — |
| REQ-SFX-011 | OPTIONAL_DEFER | NONE; no suitable production media | — (optional; no identity) | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | OPTIONAL_DEFER | M10-06 | — |
| REQ-SFX-012 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_meal_cutlery | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-SFX-013 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_sink_washing | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-SFX-014 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_slipper_toe_tap | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-SFX-015 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_low_vibration | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-SFX-016 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_equipment_disconnect | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_06 | M10-06 | — |
| REQ-SFX-017 | OPTIONAL_DEFER | NONE; no suitable production media | — (optional; no identity) | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | OPTIONAL_DEFER | M10-06 | — |
| REQ-SFX-018 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_connection_rain_handoff | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |
| REQ-SFX-019 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_raindrop_awning | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |
| REQ-SFX-020 | NEW_PRODUCTION_REQUIRED | NONE; no suitable production media | sfx_old_bus_brake | Silence in temporary technical build only; authored production cue preserved; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | UNKNOWN (no final file) | REQUEST_M10_07 | M10-07 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-SFX-001 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored L17; finite opening sound, no required looping subsystem |
| REQ-SFX-002 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored L17; single placement |
| REQ-SFX-003 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored; no surprise stinger |
| REQ-SFX-004 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored narration; short causal steps |
| REQ-SFX-005 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Necessary action cue if audible staging used; prose remains sufficient |
| REQ-SFX-006 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored single occurrence; no child/father hallucination |
| REQ-SFX-007 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored exact same length and pitch twice total; diegetic, not laugh track |
| REQ-SFX-008 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Necessary action cue; no creak horror; must not play before release |
| REQ-SFX-009 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Optional causal rendering of authored action; no audio fallback acceptable |
| REQ-SFX-010 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Optional causal rendering; no clue chime |
| REQ-SFX-011 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Optional causal rendering; gentle one-shot |
| REQ-SFX-012 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored L784; sparse one-shots at physical action, not a metronome |
| REQ-SFX-013 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored narration; finite segment if used, no new water-loop system |
| REQ-SFX-014 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored narration; mundane and quiet |
| REQ-SFX-015 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored; restrained; no cosmic / horror bass swell |
| REQ-SFX-016 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored short cue; not success fanfare |
| REQ-SFX-017 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Optional causal rendering of narration; no machinery reveal sound before L924 |
| REQ-SFX-018 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored gradually replaced by rain; handoff technical review |
| REQ-SFX-019 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored narration; distinguish onset from environmental bed |
| REQ-SFX-020 | No decorative clue, completion or horror effect | Sparse causal cue at authored action; one-shot ownership stays M4 | Compatible physical actions only | Authored; no bus image / era scene required |


### UI Overlay

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-ui-001"></a>REQ-UI-001 | S001–S008 | UI Overlay | Reading-mode identity treatment | Speech, prose, thought and internal channel are distinguishable | Same authoritative reading area and Backlog identity | P0 |
| <a id="req-ui-002"></a>REQ-UI-002 | S003 | UI Overlay | First compact connection information | Know Jinhee, third visit and delayed identity response | L364–369 translucent secondary panel | P0 |
| <a id="req-ui-003"></a>REQ-UI-003 | S004 | UI Overlay | Jihun static status window | Clinical observation beside shoe-driven recall | L481, static and unobtrusive | P1 |
| <a id="req-ui-004"></a>REQ-UI-004 | S005 | UI Overlay | Return willingness / external remaining information | Fact acceptance differs from willingness | Edge panel: 미형성 and 21분 at L683 | P1 |
| <a id="req-ui-005"></a>REQ-UI-005 | S005 | UI Overlay | Projection assistance disclosure and ending / waiting state | Player knows intervention scope and cessation | L700 disclosure then minimized; L829 waiting; L835 end accompanying internal line | P0 |
| <a id="req-ui-006"></a>REQ-UI-006 | S006 | UI Overlay | Brief awakening / follow-up / reconnect restriction | Functioning post-care is visible | L960 three compact medical facts | P1 |
| <a id="req-ui-007"></a>REQ-UI-007 | S007 | UI Overlay | Sleep / meal record view | Observed life evidence precedes diagnosis | Doctor changes screen L1056 | P1 |
| <a id="req-ui-008"></a>REQ-UI-008 | S007, S008 | UI Overlay | Prescription / reassessment and approval context | Recovery and approval are distinct steps | L1212 승인 전 재평가; L1218 elapsed-time / cleared reassessment | P0 |
| <a id="req-ui-009"></a>REQ-UI-009 | S008 | UI Overlay | Reconstructed early-2000s Korea context heading | Introduce era immersion setting at briefing | L1229 secondary heading above equal cards | P1 |
| <a id="req-ui-010"></a>REQ-UI-010 | S008 | UI Overlay | Participation / recording / information protection and signature | Approved, private helper participation | L1246 brief consent screen | P1 |
| <a id="req-ui-011"></a>REQ-UI-011 | S008 | UI Overlay | Three qualitative normal-completion conditions | Cooperation and voluntary interest are not romance score | L1313–1318 secondary three-line guide | P0 |
| <a id="req-ui-012"></a>REQ-UI-012 | S006 | UI Overlay | Appointment replacement display | Doctor reassignment proceeds before next connection | L1017–1019 one slot clears then examination replaces it | P1 |
| <a id="req-ui-013"></a>REQ-UI-013 | S008 | UI Overlay | Black-screen part end title | Stop before next era content | L1381–1383 exact title | P1 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-UI-001 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-002 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-05 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-003 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-06 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-004 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-06 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-005 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-06 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-006 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-06 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-007 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-07 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-008 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-07 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-009 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-07 | — |
| REQ-UI-010 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-07 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-011 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-07 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-012 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-06 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-UI-013 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-07 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-UI-001 | No patient labels before S003; no early cyber disclosure | Labels convey mode independent of color / italics | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-002 | First reveal here only | Exact three authored facts; no tutorial / score animation | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-003 | No invented numeric readings | Source supplies no numeric fields here; preserve concise established status context only | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-004 | Do not imply UI decides consent | No live countdown; never cover faces | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-005 | Not real family will, new event or guaranteed recovery | Existing reactions only; original/intervention records distinct; no completion sound | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-006 | Stable response not full recovery guarantee | Short display; no duplicate explanatory narrator | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-007 | No invented lab values or diagnosis of white door | Record labels and comparison context; dialogue explains significance | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-008 | No immediate approval at diagnosis | Do not invent test numbers, elapsed day count or strengthened stimulus | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-009 | Role setting not actual helper identity | Only authored era/context words; no next-era image | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-010 | No personal helper name, actual age or hidden romance consent | Authored scope labels only; no invented interactive agreement choice | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-011 | No numeric race or mind-reading meter | Exact three authored items; clinical judgement and safety abort remain dialogue | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-012 | No threat notification | Not a new calendar feature or player appointment task | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |
| REQ-UI-013 | No teaser memory / next-scene reveal | Legible static title, ordinary black transition | Shared visual vocabulary, scene-authored content | Conceptual presentation role; no new persistent state, UI hierarchy or API frozen. |


### Diagram

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-diag-001"></a>REQ-DIAG-001 | S007 | Diagram | Simple two starting paths converging at terminal stage | Separate origin from shared irreversible late failure | L1121, beside explanation not a new glossary | P1 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-DIAG-001 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-07 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-DIAG-001 | No new taxonomy, 영면병 definition or white-door cause | No neural animation; dialogue owns definitions and reversibility distinction | Scene-local medical explanation | M10-03 audited: see disposition and frozen lookup map above. |


### Profile UI

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-profile-001"></a>REQ-PROFILE-001 | S008 | Profile UI | Equal three-profile overview, Back and reviewed feedback | Inspection order differs from route choice | Overview → detail → Back; all-reviewed neutral continuation | P0 |
| <a id="req-profile-002"></a>REQ-PROFILE-002 | S008 | Profile UI | Sunju / Haejin / Hana role detail content | Player receives every authored profile | Readable detail plus associated authored reading-area response | P0 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-PROFILE-001 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-07 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-PROFILE-002 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-07 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-PROFILE-001 | No Hana prehighlight, hidden score or helper identity | All three accessible; reviewed ≠ preferred; no click leaks to story advance | Scene-local | Transient visits only; deterministic checkpoint restart/reconstruction; no persistence field. |
| REQ-PROFILE-002 | No invented Hana thought, real-person identity or route promise | Sunju/Haejin thoughts once; Hana spoken decision and later thought retain original positions | Scene-local | Revisits reread details without duplicate thoughts; accessible Back to overview. |


### Transition Effect

| Requirement Ref | Scene(s) | Type | Description | Narrative Purpose | Presentation Use | Requirement Level |
| --- | --- | --- | --- | --- | --- | --- |
| <a id="req-fx-001"></a>REQ-FX-001 | S001, S006, S008 | Transition Effect | Ordinary black reveal / exit fade | Scene boundaries remain comprehensible | Existing black / BG / CG capabilities conceptually reused | P1 |
| <a id="req-fx-002"></a>REQ-FX-002 | S006 | Transition Effect | Brief white-door outline localized beside normal exit | Both Jinhee and Jihun encounter unresolved anomaly | L887 appear; L916 vanish before fade | P0 |
| <a id="req-fx-003"></a>REQ-FX-003 | S006 | Transition Effect | Restrained perceptual disturbance | Jihun pain and view wobble at exact music stop | L892 minimal disturbance over normal entry | P1 |
| <a id="req-fx-004"></a>REQ-FX-004 | S003 | Transition Effect | Subtle hall-distance impression / return to normal | Perceptual problem precedes white door | L345–379 framing and prose, localized only if needed | P1 |


| Requirement Ref | Production Status / Final Disposition | Existing Asset Match | Frozen Production ID(s) / scope | Temporary Mapping | Provenance | Request timing | Needed By | M10-04 technical review |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| REQ-FX-001 | NO_SEPARATE_ASSET_REQUIRED | Existing M4 black-fade wiring; content owner has no standalone file | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-05 / M10-06 / M10-07 | — |
| REQ-FX-002 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-06 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-FX-003 | TECHNICAL_REVIEW_M10_04 | NONE; M10 capability not implemented | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 / M10-06 | [pending spike](M10_TEMPORARY_ASSET_MAPPING.md#m10-04-carry-forward) |
| REQ-FX-004 | NO_SEPARATE_ASSET_REQUIRED | NONE; component-owned content planned | — (component/composition owned) | Exact text/vector under planned owner; no fixture content; [scene policy](M10_TEMPORARY_ASSET_MAPPING.md#scene-level-temporary-mapping) | PROJECT_OWNED (planned ownership only; file not READY) | — | M10-04 review / M10-05 | — |


| Requirement Ref | Spoiler Constraints | Composition/Audio Constraints | Reuse Potential | Notes |
| --- | --- | --- | --- | --- |
| REQ-FX-001 | No dream, memory or cosmic wash | Qualitative timing; no curve / duration / hierarchy frozen | Cross-scene transition vocabulary | M10-03 audited: see disposition and frozen lookup map above. |
| REQ-FX-002 | No flames, stars, spacecraft, wife/daughter or cosmology | Normal room remains recognizable; only brief single overlap | Scene-local | M10-04 SPIKE CANDIDATE; not assumed supported by full-view CG / BG transitions. |
| REQ-FX-003 | No heavy glitch, monster or repeating anomaly | Honor effective Screen Shake preference if a movement consumer is selected; no consumer assumed | Scene-local | M10-04 SPIKE CANDIDATE; minimum compatible mechanism pending. |
| REQ-FX-004 | No Minseok deformation or visual explanation | No mandatory shader; logical hall stays the same | Scene-local | Review composition-first representation; no unimplemented camera capability assumed. |


## Beat-reference validation

Every requirement below has at least one concrete presentation beat. This preserves beat traceability; inventory dispositions are recorded in the category tables above. The two Ambient roles and two BGM roles have their detailed lifecycle in the Presentation Map audio section.

| Requirement Ref | Referenced presentation beats |
| --- | --- |
| REQ-BG-001 | S001-P02, S001-P03, S001-P04, S001-P05, S001-P06, S001-P08, S001-P10, S001-P12, S001-P13 |
| REQ-BG-002 | S001-P07, S001-P08, S006-P01, S006-P02 |
| REQ-BG-003 | S002-P01, S002-P02, S002-P03, S002-P07, S003-P08, S003-P09, S003-P10, S004-P01, S004-P06 |
| REQ-BG-004 | S002-P07, S003-P01, S003-P02, S003-P03, S003-P08, S004-P06, S004-P07, S004-P08, S004-P09 |
| REQ-BG-005 | S004-P10, S004-P11, S004-P12 |
| REQ-BG-006 | S005-P01, S005-P02, S005-P03, S005-P06, S005-P15 |
| REQ-BG-007 | S005-P08, S005-P09, S005-P10, S005-P11 |
| REQ-BG-008 | S006-P06, S006-P08, S006-P10 |
| REQ-BG-009 | S006-P11, S006-P12 |
| REQ-BG-010 | S006-P12, S006-P13, S006-P14 |
| REQ-BG-011 | S007-P01, S007-P07 |
| REQ-BG-012 | S008-P01, S008-P11, S008-P12 |
| REQ-CHAR-001 | S001-P07, S001-P08, S001-P10, S001-P12, S001-P13, S002-P01, S002-P08, S003-P01, S003-P05, S003-P07, S003-P09, S003-P10, S004-P01, S004-P07, S005-P01, S005-P07, S005-P15, S006-P01, S006-P04, S006-P06, S006-P08, S006-P10, S006-P11, S007-P01, S007-P03, S007-P05, S007-P07, S007-P10, S008-P01, S008-P07 |
| REQ-CHAR-002 | S001-P02, S001-P05, S001-P08, S001-P09, S001-P12, S001-P13, S002-P01, S002-P06, S003-P01, S003-P07, S003-P10, S004-P01, S004-P03, S004-P05, S004-P06, S004-P07, S004-P09, S004-P14, S004-P15, S005-P01, S005-P07, S005-P08, S005-P10, S005-P12, S005-P15, S006-P01, S006-P04, S006-P06, S006-P08, S006-P10, S006-P11 |
| REQ-CHAR-003 | S001-P02, S001-P05, S001-P12, S001-P13, S002-P01, S002-P04, S003-P01, S004-P05, S005-P01, S005-P06, S005-P08, S005-P11, S005-P14 |
| REQ-CHAR-004 | S001-P02, S001-P07, S001-P13, S002-P01, S005-P01, S005-P06, S005-P08, S005-P11 |
| REQ-CHAR-005 | S001-P02, S001-P07, S001-P09, S001-P13, S002-P01, S002-P06, S004-P03, S004-P15, S005-P01, S005-P02, S005-P06, S005-P08, S005-P12 |
| REQ-CHAR-006 | S001-P07, S001-P08, S001-P13, S002-P10, S003-P04, S005-P01, S006-P01 |
| REQ-CHAR-007 | S006-P07, S006-P12, S007-P01, S008-P01, S008-P10, S008-P12 |
| REQ-CHAR-008 | S007-P01, S007-P02, S007-P05, S007-P06, S007-P07, S007-P08, S007-P09, S007-P10 |
| REQ-CHAR-009 | S008-P01, S008-P02, S008-P07, S008-P11, S008-P12 |
| REQ-EXP-001 | S001-P02, S001-P03, S001-P12, S002-P02, S005-P09 |
| REQ-EXP-002 | S001-P10, S002-P03, S002-P04, S002-P07, S004-P02, S004-P03, S004-P05 |
| REQ-EXP-003 | S004-P13, S004-P14, S004-P15, S005-P02, S005-P03, S005-P06, S005-P09, S005-P10, S005-P12, S005-P14 |
| REQ-EXP-004 | S002-P08, S002-P09, S002-P10, S003-P03, S006-P03, S007-P03, S007-P07 |
| REQ-EXP-005 | S001-P04, S002-P05, S003-P01, S004-P06, S005-P09 |
| REQ-EXP-006 | S001-P02, S001-P04, S002-P06, S004-P03 |
| REQ-EXP-007 | S007-P01, S007-P03, S007-P04, S007-P10 |
| REQ-EXP-008 | S008-P01 |
| REQ-EXP-009 | S002-P10, S003-P04, S006-P07 |
| REQ-INS-001 | S001-P02 |
| REQ-INS-002 | S001-P09, S004-P01, S004-P09 |
| REQ-INS-003 | S001-P11 |
| REQ-INS-004 | S002-P01, S002-P02, S002-P05 |
| REQ-INS-005 | S004-P10, S004-P11 |
| REQ-INS-006 | S004-P10, S004-P12, S004-P13 |
| REQ-INS-007 | S004-P02, S004-P10, S004-P14, S004-P15, S005-P02, S005-P12 |
| REQ-INS-008 | S006-P12 |
| REQ-INS-009 | S005-P09 |
| REQ-CG-001 | S001-P13, S005-P08 |
| REQ-BGM-001 | S001-P03, S002-P01, S002-P04, S003-P04 |
| REQ-BGM-002 | S005-P11, S006-P03 |
| REQ-AMB-001 | S001-P03 |
| REQ-AMB-002 | S008-P13 |
| REQ-SFX-001 | S001-P01 |
| REQ-SFX-002 | S001-P01 |
| REQ-SFX-003 | S001-P05 |
| REQ-SFX-004 | S001-P06 |
| REQ-SFX-005 | S001-P06 |
| REQ-SFX-006 | S002-P09 |
| REQ-SFX-007 | S003-P02 |
| REQ-SFX-008 | S004-P09 |
| REQ-SFX-009 | S004-P11 |
| REQ-SFX-010 | S004-P12, S004-P13 |
| REQ-SFX-011 | S004-P14 |
| REQ-SFX-012 | S005-P11 |
| REQ-SFX-013 | S005-P14 |
| REQ-SFX-014 | S006-P01 |
| REQ-SFX-015 | S006-P02 |
| REQ-SFX-016 | S006-P06 |
| REQ-SFX-017 | S006-P11 |
| REQ-SFX-018 | S008-P13 |
| REQ-SFX-019 | S008-P13 |
| REQ-SFX-020 | S008-P13 |
| REQ-UI-001 | S001-P01, S001-P03, S002-P09, S003-P04, S003-P05, S003-P06, S003-P07, S003-P08, S003-P09, S003-P10, S004-P04, S004-P13, S004-P16, S005-P03, S005-P04, S005-P05, S005-P07, S005-P13, S005-P15, S006-P03, S006-P04, S006-P07, S006-P09, S006-P12, S006-P13, S007-P02, S007-P04, S007-P05, S007-P06, S007-P08, S007-P09, S008-P02, S008-P04, S008-P05, S008-P07, S008-P08, S008-P09, S008-P10, S008-P11, S008-P12, S008-P13 |
| REQ-UI-002 | S003-P06, S003-P08 |
| REQ-UI-003 | S004-P04, S004-P16 |
| REQ-UI-004 | S005-P04 |
| REQ-UI-005 | S005-P05, S005-P13, S005-P14 |
| REQ-UI-006 | S006-P09 |
| REQ-UI-007 | S007-P02, S007-P04 |
| REQ-UI-008 | S007-P08, S007-P09, S007-P11, S008-P01 |
| REQ-UI-009 | S008-P02 |
| REQ-UI-010 | S008-P03 |
| REQ-UI-011 | S008-P09, S008-P10 |
| REQ-UI-012 | S006-P13 |
| REQ-UI-013 | S008-P14 |
| REQ-DIAG-001 | S007-P05, S007-P06 |
| REQ-PROFILE-001 | S008-P02, S008-P03, S008-P04, S008-P05, S008-P06, S008-P07, S008-P08 |
| REQ-PROFILE-002 | S008-P04, S008-P05, S008-P06, S008-P07, S008-P08, S008-P12 |
| REQ-FX-001 | S001-P01, S004-P08, S006-P05, S006-P06, S008-P13, S008-P14 |
| REQ-FX-002 | S006-P02, S006-P04, S006-P05 |
| REQ-FX-003 | S006-P03 |
| REQ-FX-004 | S003-P03, S003-P07 |


## Reconciliation decisions and counts

| Final disposition | Requirements |
| --- | --- |
| MATCHED_PRODUCTION | 0 |
| TEMP_TECHNICAL | 0 |
| NEW_PRODUCTION_REQUIRED | 50 |
| NO_SEPARATE_ASSET_REQUIRED | 10 |
| TECHNICAL_REVIEW_M10_04 | 16 |
| OPTIONAL_DEFER | 8 |
| PROVENANCE_BLOCKED | 0 |

| Requirement Ref | Decision / no independent file explanation |
| --- | --- |
| REQ-BG-009 | Brief corridor carried by canonical prose and clean room→street transition; retain physical route, no standalone corridor plate. |
| REQ-INS-001 | Crop the newly planned kitchen BG with exactly five eggs before faces; no duplicate CG entry. |
| REQ-INS-003 | Scene-owned ordinary memo text block with exact 다음 방문 — 아침 식사 후; no image or catalog identity. |
| REQ-INS-006 | Scene-owned readable paper panel; three exact family names, same unspecified accident date; cover/reopen only after approach. No fabricated calendar date or agency seal. |
| REQ-INS-009 | Cup remains a subordinate wide-view prop, no focused file. |
| REQ-CG-001 | Occupied BG tableau variants satisfy ensemble; optional separate ensemble CG is unnecessary under frozen composition, not a lost five-person beat. |
| REQ-AMB-001 | Optional home bed omitted; no hum/drone added. |
| REQ-AMB-002 | Rain handoff absent; shared finite SFX plan is a production gap, playback feasibility remains M10-04. |
| REQ-SFX-005 | No separate action sound chosen; canonical prose/action remains; reassess only if staging needs it. |
| REQ-SFX-008 | No separate action sound chosen; canonical prose/action remains; reassess only if staging needs it. |
| REQ-SFX-009 | No separate action sound chosen; canonical prose/action remains; reassess only if staging needs it. |
| REQ-SFX-010 | No separate action sound chosen; canonical prose/action remains; reassess only if staging needs it. |
| REQ-SFX-011 | No separate action sound chosen; canonical prose/action remains; reassess only if staging needs it. |
| REQ-SFX-017 | No separate action sound chosen; canonical prose/action remains; reassess only if staging needs it. |
| REQ-UI-001 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-002 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-003 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-004 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-005 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-006 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-007 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-008 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-009 | Scene-owned authored era heading, no new graphic/ID. |
| REQ-UI-010 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-011 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-012 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-UI-013 | Scene-owned exact static title on black, no separate image/ID. |
| REQ-DIAG-001 | Scene-owned text/vector two-path diagram; dialogue owns meaning, no medical raster quota. |
| REQ-PROFILE-001 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-PROFILE-002 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-FX-001 | Reuse existing project-owned VNTransitionController and ScreenFadeOverlay; no independent content ID. |
| REQ-FX-002 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-FX-003 | Current scene/prefabs provide the baseline only; M10-specific lifecycle/input/effect is unimplemented. |
| REQ-FX-004 | Canonical hall-distance prose and unchanged linked-room framing suffice; no shader or persistent effect ID. |

P0 new-media gaps have explicit request timing: kitchen/table, entry, hall, Jihun/Jinhee/Junho/ordinary dog REQUEST_NOW; storage/doorway/day bedroom/actual Rope REQUEST_M10_06; preparation room REQUEST_M10_07. Pan/memo/paper remain P0 semantics without independent files; overlay/profile/white-door P0 remain component spikes, never declared implemented. Matched production media is zero. The two unknown font provenance records in Inventory are infrastructure checks, not a compatible story-media match; PROVENANCE_BLOCKED requirements is zero.

## Production request timing

Counts below cover **50 NEW_PRODUCTION_REQUIRED semantic requirement rows**, not image files, expression layers or all 84 roles. Other requirements retain a build-owner gate or optional disposition. REQ-AMB-002 shares the later handoff request with REQ-SFX-018 and is not counted again as a new-media row. Expression sets/defaults share their character production bundle; do not commission separate duplicate bases.

| Timing | New requirement rows |
| --- | --- |
| REQUEST_NOW | 20 |
| REQUEST_M10_05_REVIEW | 3 |
| REQUEST_M10_06 | 16 |
| REQUEST_M10_07 | 9 |
| REQUEST_M10_08_POLISH | 2 |
| OPTIONAL_DEFER | 0 |

Wave A (M10-04): **0 final production assets requested**; use neutral technical material. Wave B (M10-05): REQUEST_NOW is the P0/high-P1 start; P2 plate placement and child entry steps wait until M10-05 review, but remain required for final authored-cue acceptance. First-wave detailed image/BGM/SFX briefs are [here](M10_TEMPORARY_ASSET_MAPPING.md#first-user-production-requests). Later counts do not authorize automatic generation. Optional inferred action sounds, room bed and focused blue cup are not catalog quotas.

## Review boundary

All 84 original refs and 103 beat links remain. No source, Presentation Map, C#, Yarn, Scene/Prefab/SO, media, import setting, .meta, package, ProjectSettings, input action or persistence schema change is included. M10-04 technical reviews remain pending; Production Implementation Ready is NO. Full entering 443/0/0 Unity baseline is prior evidence only; no full suite was rerun for these docs. No M10-04 or asset generation begins automatically.
