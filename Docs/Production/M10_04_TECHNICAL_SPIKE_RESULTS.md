# M10-04 — Presentation Technical Spike Results

Status: technical evidence and review handoff; production Yarn/media/wiring remain M10-05+.
Authority/base: `ae5604792bdf234ec09eb37cdc95496bd3410faf` (merged PR #49), branch `feat/m10-04-presentation-technical-spikes`.

| Spike | Result |
| --- | --- |
| A Reading modes | GO — semantic labels; visual polish later |
| B Information overlay | GO |
| C Manual-only hold | GO |
| D Local white door | GO |
| E View disturbance | ADAPT — no screen motion, as expressly permitted by the task |
| F Profile browser | GO — transient browse and shared manual review-line seam |
| G Home Ambient | NO-GO FOR NEW AMBIENT SUBSYSTEM / OPTIONAL_DEFER; successful non-blocking result |
| H Equipment → rain | GO — finite composite on existing one-shot owner |
| I Same-track lowering | NO-GO for persistent ducking under current snapshot; user decision pending |
| J Catalog consistency | GO — five-reference coordinated wiring plan; no catalog rewired |

## A — Reading modes

- Requirement: narration, spoken Jihun, Jihun monologue, Rope internal and Jihun internal retain distinguishable identity.
- Existing authority: Yarn `LocalizedLine.CharacterName`, one enabled authoritative `LinePresenter`, `VNLineLifecyclePresenter`, session Backlog/read services and passive `VNSpeakerFocusPresenter`.
- Prototype/implementation tested: six labels including off-screen speech, exact Backlog text/name/TextID, full-display-once and authorized read consume; catalog aliases resolve Jihun labels to one physical ID. Existing occurrence/typewriter/optional Voice/Auto/Skip suites remain in regression.
- Result: **GO — semantic mode contract; visual polish later.**
- Code impact: passive focus policy suppresses active physical focus for suffixes `·독백`, `·내부`, `·화면 밖`; ordinary visible speech retains catalog focus. Suppression resets visible characters to neutral; it never shows/hides an actor.
- Persistence impact: none; same stable Yarn TextID and existing services.
- Save/Load impact: existing line barrier/lifecycle unchanged.
- Input impact: same hurry-then-next bridge; no new presenter or history authority.
- Authoring impact: speakerless narration; exact visible `지훈`, `지훈·독백`, `로프·내부`, `지훈·내부`. Typography alone is insufficient identity. Register aliases on their existing physical definitions; Rope memory and later actual Rope remain the distinct frozen presentation definitions specified by M10-03.
- M10-05 contract: one enabled authoritative LinePresenter, explicit unique #line IDs, existing name container/TMP. Do not add early Rope visibility or reveal.
- Remaining risk: final font/style/pacing review `USER_REVIEW_LATER_M10-05+`.

## B — Structured information overlay

- Requirement: show/update/hide exact authored non-dialogue facts, compatible with Hide UI, convenience modals and load/re-entry.
- Existing authority: scene session visuals, `VNUIVisibilityController`, M5 `LoadStateChanged`.
- Prototype/implementation tested: `VNInformationOverlay.Show(title, facts)` / `Hide()`, exact replacement text, Hide/Show UI, modal open/close, real load event clear and deterministic reconstruction; no Backlog/read/Advance events.
- Result: **GO**.
- Code impact: one passive component in Presentation, root CanvasGroup and two TMP fields, explicit visibility/load references.
- Persistence impact: none; content and shown state are transient.
- Save/Load impact: real load start clears facts; disable clears and unsubscribes. Re-entry rebuilds facts from authored content after M5 loading ends; no arbitrary interpolation/content snapshot.
- Input impact: non-interactable, no raycast blocking; modals retain their existing ownership.
- Authoring impact: exact source facts only, no invented numeric values; repeated Show updates the content.
- M10-05 contract: wire under DialogueLayer as a sibling of LineContainer so typewriter panel fades do not own it. The root is a visual group, not the controller GameObject to deactivate. A re-entry command must defer until `IsLoadInProgress == false` before Show; honor the runner cancellation token. No production command name is frozen here.
- Remaining risk: final card density/font and deterministic authored re-entry placement need M10-05 review.

## C — Manual-only authored advance hold

- Requirement: the S004 silence cannot be crossed by Auto or either Skip policy, but Manual can hurry and then consume.
- Existing authority: `VNConvenienceController` → `VNLineAdvancerInputBridge` with `VNAdvanceSource`; occurrence-safe session state.
- Prototype/implementation tested: Auto/ReadOnly Skip/Skip All blocked with flags preserved; manual hurry retains hold, full manual consume clears it; hidden first input only restores UI; modal suspension retains hold; actual load normalization clears and re-entry may rearm; next occurrence resumes the selected automation policy.
- Result: **GO**.
- Code impact: `VNDialogueSessionState.RequireManualAdvance()`, session-only flag, automation gate and source check in existing bridge. Release only after an accepted full Manual request.
- Persistence impact: none.
- Save/Load impact: `InvalidateTransientPresentation()` clears stale hold; source re-entry reapplies it only where authored. M5 still turns convenience modes off on real load, as before.
- Input impact: no bypass or mode toggle; Auto timer/throttle resets while blocked. ReadOnly Skip does not see the held unread line and disable itself.
- Authoring impact: arm immediately before the intended ordinary Yarn line, not as a timer or global Skip policy.
- M10-05 contract: bind the proven C# seam through a scene-owned authoring adapter; no production Yarn command name introduced. Do not arm on unrelated option/command-only content or substitute an empty UI click for the source silence.
- Remaining risk: exact S004 insertion and final pacing are later source-authoring checks.

## D — Localized white-door effect

- Requirement: brief white outline beside the normal exit with both people still possible; remove before black, no baked background/reveal.
- Existing authority: `VNTransitionController` and M5 transition stability checks.
- Prototype/implementation tested: neutral three-bar door outline and unchanged normal exit, appear/hide, active/stable state, real complete-snapshot rejection while visible, load clear, stale externally-held enumerator cannot resurrect effect or decrement a newer operation.
- Result: **GO**.
- Code impact: optional `localizedEffectCanvasGroup`, awaited `FadeLocalizedEffect(visible, duration)`, generation guard and disposal/accounting in the existing transition wrapper.
- Persistence impact: none; interpolation/effect presence absent from SaveData.
- Save/Load impact: **entire visible interval**, including stable visible alpha, is treated as transient/unsaveable; stricter than merely blocking interpolation and avoids restoring an unrepresented door. Load cancels/reset-clears it.
- Input impact: effect itself neither interacts nor blocks raycasts; ordinary modal/input rules remain.
- Authoring impact: show only at S006 L887, retain normal entrance/exit, existing BGM stop around L892, hide before L916 and later black.
- M10-05 contract: optional effect root under presentation visuals, initially alpha 0, local wall-sized shape, awaited appear/hide calls. Never replace `bg_family_entrance` or expose cosmology.
- Remaining risk: final composition and ambiguous restraint `USER_REVIEW_LATER_M10-05+`; existing unrelated nested transition routines retain their established owner, not a new cinematic framework.

## E — Restrained disturbance / Screen Shake

- Requirement: restrained S006 disturbance; if movement exists, M7 Screen Shake Off means exactly zero movement and dialogue/controls/modals must stay static.
- Existing authority: M7 `IVNScreenShakeGate`; M3/M4 visual hierarchy.
- Prototype/implementation tested: MCP hierarchy has separate BackgroundLayer, CharacterLayer and CGLayer directly under VNCanvas. Their only current common root also owns DialogueLayer, QuickControlLayer, TransitionLayer and ModalLayer. Localized effect leaves all transform positions unchanged.
- Result: **ADAPT — localized white door + low vibration SFX + existing BGM stop; no screen motion**, explicitly allowed by this task.
- Code impact: no movement consumer, root reparenting or new effect framework.
- Persistence impact: none; M7 setting/schema untouched.
- Save/Load impact: localized effect uses D's existing transition stability.
- Input impact: dialogue, controls and modal UI remain static.
- Authoring impact: preserve perceptual disturbance through the authored local visual/audio sequence; no large shake, glitch or shader treatment.
- M10-05 contract: zero transform displacement with either preference, including Off. Do not move VNCanvas. A later request for movement needs a separately proven presentation-only root and actual `IVNScreenShakeGate` consumption.
- Remaining risk: final restraint is a later human pacing check, not an M10-04 blocker.

## F — Transient three-profile browse

- Requirement: equal overview cards, arbitrary order, exact detail facts, Back, reviewed feedback, all-three gate and one neutral Continue; no hidden Hana selection.
- Existing authority: existing `VNInteractionGate`, convenience Cancel router, M5 load events, one ordinary LinePresenter for review thoughts.
- Prototype/implementation tested: two different browse orders, initially equal unreviewed cards, repeat inspection, first review callbacks once each for Seonju/Haejin, no immediate Hana thought, manual review line through the shared bridge, all-reviewed Continue once, input/convenience/save gating, real load event and disable cleanup, rejection of a competing owner.
- Result: **GO** for the transient browser and owned ordinary review-line window.
- Code impact: `VNProfileContent` / `VNProfileBrowser`; owner-aware story interaction in the existing gate, no independent modal manager. Public BeginBrowse/Inspect/Back/CompleteReview/TryContinue/CancelBrowse and review/completion events are production UI seams.
- Persistence impact: none; three reviewed flags and current profile are component/session state. No route/romance/meta/save fields.
- Save/Load impact: profile UI rejects new browse while loading and clears on real load/disable; re-entry starts the segment afresh after loading. No checkpoint inside an unfinished browse.
- Input impact: overview/detail blocks underlying story, Auto/Skip, Hide, Backlog, Settings, Records and convenience Save/Load. Esc is browser Back; overview/pending review consumes Esc without closing or advancing. For a first-review thought only, the next **new occurrence** accepts Manual hurry/consume; Auto/Skip remain blocked and the window closes on accepted full consume. The previous underlying occurrence can never leak through.
- Authoring impact: BeginBrowse order is Seonju/Haejin/Hana, equal visual weight, first two `RequiresFirstReviewLine=true`, Hana false. ReviewRequested must hand off to the existing runner's ordinary source thought, then CompleteReview only after it completes; never hand-present an extra competing line. No production Yarn routing syntax is frozen in this spike.
- M10-05 contract: scene-owned adapter must yield for UI events and return to ordinary Yarn for each first-review thought, then resume browse. Do not block an entire browse command while simultaneously waiting for that same runner to present a thought. The technical smoke uses separate NON-CANON nodes through the same runner/presenter to prove the read window. Return only completion; later canonical `하나로 하겠습니다.` and the later Hana monologue stay in their original positions. Repeated card inspection emits no thought callback.
- Remaining risk: production routing adapter, exact source facts/lines and final equal-card layout belong to M10-05+ integration; no final UI aesthetic acceptance claimed.

## G — Optional home ambient ownership

- Requirement: determine whether S001–S003 needs a separate continuous home bed.
- Existing authority: current BGM sources, finite SFX `PlayOneShot`, authored silence; no Ambient owner.
- Prototype/implementation tested: source/gap review against finite frying, doorbell, chair scrape, laugh and ordinary BGM roles; existing one-shot layering proof H. No narrative requirement demands a separately controllable indefinite home bed.
- Result: **NO-GO FOR NEW AMBIENT SUBSYSTEM / OPTIONAL_DEFER**, successful and non-blocking.
- Code impact: zero.
- Persistence impact: zero.
- Save/Load impact: existing audio normalization suffices for finite cues.
- Input impact: none.
- Authoring impact: use existing BGM, finite diegetic SFX and silence; optional ambient stays deferred.
- M10-05 contract: do not add ambient loops/catalogs/sources for a single optional semantic row.
- Remaining risk: only revisit for a concrete independently controlled infinite-bed requirement; that would need explicit architecture review.

## H — Equipment → rain finite handoff

- Requirement: finite equipment → rain, then separate large awning drop and old-bus brake.
- Existing authority: `VNAudioCatalog.sfx`, `VNAudioController.PlaySfx`, one existing SFX `AudioSource.PlayOneShot`.
- Prototype/implementation tested: generated finite clip, separate following one-shot calls, no loop/source replacement, unchanged controller ownership of two BGM sources and one SFX source (VN_Main also retains its separate optional Voice source) and load Stop; no new capture fields.
- Result: **GO**.
- Code impact: zero audio runtime changes.
- Persistence impact: zero; SFX remains transient.
- Save/Load impact: `NormalizeTransientForLoad()` stops stale one-shots; timing reconstructs from authored re-entry.
- Input impact: none.
- Authoring impact: final `sfx_connection_rain_handoff` includes equipment/rain only, finite tail; exclude the separately authored large drop/braking onset. Cue those once afterward on existing source.
- M10-05 contract: preserve order equipment → rain → awning drop → bus → black/end title; no implied duplicate rain/drop or ambient loop API. Final timing is later, not frozen to generated clip seconds.
- Remaining risk: if later timing truly requires indefinitely looping rain, this GO no longer applies; report `NO-GO — finite handoff insufficient` before adding an audio subsystem.

## I — Same-track BGM lowering

- Requirement: same morning track/position, quieter S002, existing S003 paw-contact fade/stop, no duplicate track or user mixer mutation.
- Existing authority: `VNAudioController`, catalog DefaultVolume, M7 mixer preference, M5 `AudioState` with only bgmId/playbackSeconds.
- Prototype/implementation tested: play technical BGM at catalog 0.6, pause at sample 11025, lower active source to 0.3 without restart/ID/position change, capture/prepare/normalize/restore. Restore keeps ID/sample but returns volume to 0.6; gain is absent from the snapshot.
- Result: **NO-GO for persistent ducking under the current Save/Load contract; M10-04 DECISION REQUIRED**.
- Code impact: no unsafe authored-gain API shipped; characterization test only. No M7 AudioMixer.SetFloat call for story gain.
- Persistence impact: no schema changes. Persisting a story multiplier would require explicit new authorization beyond this task.
- Save/Load impact: current representation cannot restore quieter stable audio, so source-only lowering is not production-safe. No silent reset/re-entry workaround is presented as GO.
- Input impact: none.
- Authoring impact: proposed smallest adaptation is an already-low morning BGM kept steady through S001–S002, with existing S003 paw-contact fade/stop. This changes the lowering cue, so it remains for user approval; source text stays untouched.
- M10-05 contract: **do not author persistent S002 ducking until the user accepts the documented adaptation or separately authorizes persistence review**. No extra music resource request, duplicate quiet track, or mixer preference overwrite.
- Remaining risk: blocks S001–S003 implementation readiness while the user's decision is pending.

## J — Catalog consumers and future coordinated wiring

- Requirement: dedicated production catalogs without technical IDs becoming story authority or splitting M5/M9 lookup identity.
- Existing authority: controller-injected immutable catalogs, M5 snapshot preparation, M9 catalog validation/Gallery service and isolated Replay-owned hierarchy.
- Prototype/implementation tested: repo reference scan, MCP serialized-object audit including inactive Replay, existing complete-snapshot, Gallery projection, Records wiring and Replay isolation tests. No empty catalog proof was needed.
- Result: **GO — coordinated M10-05 wiring plan**, no wiring/catalog population in M10-04.
- Code impact: no catalog/Records/Replay runtime changes.
- Persistence impact: none; logical IDs must resolve through the assigned production catalogs before restore.
- Save/Load impact: coordinator validates/restores through the presentation/audio controllers, not a separate duplicate catalog. Save slot character icons receive the same presentation catalog from SaveLoadController → SaveLoadModal.Initialize → VNSaveSlotItem.BindCharacterIcons.
- Input impact: none; isolated Replay retains its own runner/presenter/visual state.
- Authoring impact: keep M3/M4 fixture catalogs and IDs separate and unchanged. Existing technical tests keep in-memory or explicitly technical catalog dependencies.
- M10-05 contract: prepare compatible dedicated production catalogs with the frozen M10 IDs, then atomically rewire the five Scene references below via Unity MCP and validate before story authoring/restore acceptance. Gallery and Replay may share read-only production Sprite definitions, never controller/session state. Empty production Records catalogs remain empty until their own authored content work.
- Remaining risk: legacy technical snapshots whose IDs are absent from production catalogs must fail validation before mutation; do not silently map technical IDs to canon. Do not execute M3/M4 technical nodes against the production catalogs; use technical harness wiring.

### Exact future Scene references

| VN_Main consumer | Field | Current | Future |
| --- | --- | --- | --- |
| PresentationRuntime / VNPresentationController | catalog | M3_PresentationCatalog | dedicated production Presentation catalog |
| AudioRuntime / VNAudioController | catalog | M4_AudioCatalog | dedicated production Audio catalog |
| SaveLoadRuntime / VNSaveLoadController | presentationCatalog | M3_PresentationCatalog | same production Presentation catalog |
| VNConvenienceRuntime / VNRecordsRuntimeBootstrap | presentationCatalog | M3_PresentationCatalog | same production Presentation catalog |
| `VNCanvas/ModalLayer/RecordsModal/Panel/ReplayHost/ReplayViewTemplate/ReplayPresentation` / VNPresentationController | catalog | M3_PresentationCatalog | same production Presentation catalog, separate runtime hierarchy |

Yarn visual/audio/transition command components reference controllers rather than separate catalogs. M5 validation/restore uses those controllers; RecordsBootstrap constructs `VNRecordsCatalogValidationContext` and `VNCGGalleryService` from its injected catalog. Replay clones its own view/controller from the inactive template; it has no audio catalog/production save/history owner. No bootstrap/service locator rewrite is needed.

## Validation and boundaries

Validation date: 2026-10-07 (Asia/Seoul). Unity MCP reported Unity `6000.3.21f1`, project `C:\Dev\project_all_time`, VN_Main, ready/not compiling. Play exited normally; final Scene dirty=false and all five catalog references retain their original M3/M4 assets. Final Console ground truth: **0 errors**, compiling=false, compilationFailed=false; one warning remains (no warning entry exposed by the MCP stream).

| Automated check | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| Focused VNDialogueSessionStateTests (semantic modes/lifecycle) | 15 | 0 | 0 |
| Focused VNConvenienceBridgeTests (hold/overlay/profile) | 15 | 0 | 0 |
| Focused VNLocalizedEffectTests | 2 | 0 | 0 |
| Focused VNPresentationAudioSpikeTests | 2 | 0 | 0 |
| Focused VNFullSnapshotTests | 9 | 0 | 0 |
| Focused VNRecordsProjectionTests (Gallery) | 13 | 0 | 0 |
| Focused VNReplay tests | 6 | 0 | 0 |
| **Final unfiltered EditMode** | **462** | **0** | **0** |

Final full suite also has 0 inconclusive results; entering baseline was 443. Earlier validation exposed the expected Yarn inventory increase (151 → 162) and two intermittent Replay test waits while the unfocused Editor paused its player loop. The inventory assertion now accounts for the eleven explicit technical IDs; those two test harnesses temporarily enable `Application.runInBackground` and restore the captured value in TearDown. No Replay runtime or serialized PlayerSettings change. The final unfiltered run passes all 462.

**Unity MCP Play technical smoke: PASS for GO features.** Normal M2 startup and exactly one enabled authoritative LinePresenter were confirmed. The technical fixture exercised all five semantic identities with the existing LetterTypewriter, exact Backlog entries and stable TextIDs. Auto and already-read ReadOnly Skip reached and stopped at the authored hold without toggling their selected state. Manual crossed only after full display; Hide UI's first press restored only. Backlog/Settings/Records modal open/close retained the hold and exact passive overlay facts. Updating facts created no history entry.

The local white three-bar outline appeared alongside an unchanged normal exit and two neutral person rectangles, then disappeared before restoring the ordinary technical entrance. Dialogue/Quick Controls/modal positions stayed unchanged with the actual Screen Shake preference Off. The equal three-profile overview was inspected in the Game View; Seonju and Haejin first-review thoughts ran once through the same runner/presenter, Hana had no immediate thought, Esc returned to overview, all-three gated Continue emitted one completion, and normal dialogue resumed. Automated tests cover two arbitrary browse orders. Generated finite equipment/rain audio finished on the existing SFX source before separate drop/brake calls; load normalization stopped stale one-shots.

Technical load-event proof invoked the actual M5 `LoadStateChanged` event and existing transition/audio normalization: overlay, hold, browser lease/review flags, white effect and SFX cleared; authored facts reconstructed after loading ended; browser restarted fresh and disable released ownership. This is a transient re-entry proof, not a claim of end-to-end file Save/Load in the technical Play session; complete snapshot/restore contracts are covered by the automated M5 suites.

Temporary neutral UI/audio and command handlers existed only in Play and were discarded on normal exit. The durable read-history bridge was disposed for the non-canon probe; the production MetaProgress file's SHA-256 before/after was identical (`603002D27EA3D55157E669E089027932AA69A77860D3BD4093B098C1D2A611E5`). No technical Records unlocks. Game-view captures were visually inspected via Unity ScreenCapture because the MCP camera screenshot omits ScreenSpaceOverlay UI. One infrastructure eval timeout and an unguarded Stop on an already-completed technical node were harness issues; the corrected guard uses the existing M5 line-presenter quiescence barrier and the completed profile flow was rerun successfully.

Full changed-file diff reviewed; `git diff --check` passes. Final commit/PR identity and clean Git status are recorded in the review handoff.

Canonical source, M10-02 Presentation Map and M10-03 Production ID Map remain byte-for-byte unchanged. Source SHA-256: `8413E847DD66D7D45585847A935B9C384D91FACFE9724EE27084F3D86FF977E9`.

Zero final production media added. No production Yarn, catalog asset population/rewiring, Scene/Prefab YAML edit, manual meta GUID, SaveData/MetaProgress/Settings schema change, extra authoritative LinePresenter, Ambient/cinematic subsystem, or M10-05 work. Unity generates all new meta files. Production Start Node stays `M2_UI_START`.

`M10_PRESENTATION_SPIKE.yarn` has eleven explicit unique technical/non-canon line IDs. Its `m104_*` command handlers exist only in temporary MCP Play wiring; these are not production authoring syntax. Docs/06 remains unchanged because no new production Yarn syntax was finalized. The proven C# seams and semantic labels above are the M10-05 handoff.

M10-04 DECISION REQUIRED: approve the already-low steady S001–S002 morning BGM adaptation, retaining S003 fade/stop, or keep ducking blocked for separately authorized persistence review. No canon text is rewritten. Do not set S001–S003 implementation READY before this decision.
