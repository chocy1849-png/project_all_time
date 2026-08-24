# Technical Architecture

## Current baseline

- Unity project root
- `Assets/_Project/`
- `Packages/`
- `ProjectSettings/`
- Universal Render Pipeline (URP) baseline
- Yarn Spinner 3.2.7 runtime through `dev.yarnspinner.unity`
- Official Dialogue System with built-in InMemoryVariableStorage for M1 smoke testing

## M2 core VN play screen

- Project-owned uGUI objects and prefabs provide the VN play-screen layout.
- Yarn Spinner supplied LinePresenter and OptionsPresenter remain the runtime presenter implementations.
- The existing Yarn LineAdvancer provides click/Space hurry-then-advance behavior.
- The existing InputSystemUIInputModule provides UI navigation.
- A custom DialoguePresenterBase is not required.
- Background, Character, and CG presentation are delivered by the M3 project-owned uGUI runtime.
- Audio and transitions are deferred to M4.
- Persistence is deferred to M5.
- Convenience UX is deferred to M6.
- Settings are deferred to M7.

## PLANNED — NOT IMPLEMENTED

Future domains:

- Dialogue
- Save
- Audio
- Tests

Persistent variable integration is deferred to M5.

## M3 presentation runtime contract

- `VNCharacterDefinition` stores a stable character ID, speaker aliases, default facing and scale, optional BackHair, Body, and expression Head sprites. Characters use a fixed pose; expressions swap only the Head sprite.
- `VNPresentationCatalog` resolves unique character, background, CG, and speaker-alias IDs. Invalid or duplicate IDs invalidate lookup, so commands fail without changing presentation state.
- `VNPresentationController` owns runtime-only character state and drives a background Image, CG Image, and one `VNCharacterSlotView` per fixed character slot. Each view owns a shared visual root plus BackHair, Body, and Head Images.
- `VNYarnPresentationCommands` registers `vn_bg`, `vn_show`, `vn_expression`, `vn_move`, `vn_facing`, `vn_scale`, `vn_hide`, `vn_cg`, and `vn_clear_cg` with Dialogue Runner. M3 has no pose command.
- `VNSpeakerFocusPresenter` observes `LocalizedLine.CharacterName` and returns immediately without rendering text or handling options.
- The M3 technical smoke is non-canon and uses manual dialogue checkpoints to expose presentation changes for Play Gate verification.
- Character entrance/exit animation, movement tweening, background/CG transitions, screen effects, audio, persistence, and final artwork/polish are out of scope for M3.

## M4 audio / voice / transition runtime contract

- `VNAudioCatalog` is a ScriptableObject lookup for unique lowercase-snake-case BGM and SFX IDs. Invalid or duplicate entries invalidate runtime lookup so unknown IDs fail before audio state changes. Voice clips are intentionally excluded.
- `VNAudioController` owns two BGM AudioSource references and one SFX AudioSource reference. It crossfades BGM source-to-source, pauses with `Pause`, resumes with `UnPause`, fade-stops to a reusable source state, and plays SFX through `PlayOneShot`.
- `VNYarnAudioCommands` registers `bgm_play`, `bgm_crossfade`, `bgm_pause`, `bgm_resume`, `bgm_stop`, and `sfx_play`. All duration operations return `IEnumerator` and are awaited by Yarn Spinner 3.2.7.
- Voice assets are optional per Yarn line. `VNOptionalVoicePresenter` is the sole Dialogue Runner voice presenter; it returns immediately for a null `LocalizedLine.Asset`, reports a project-owned error for a non-AudioClip asset, and delegates valid AudioClip playback plus dialogue lifecycle callbacks to the Unity-wired Yarn Spinner 3.2.7 `VoiceOverPresenter`. The delegated presenter uses the assigned Voice AudioSource and retains `endLineWhenVoiceoverComplete = false` for normal manual advancement.
- `VNTransitionController` uses the M3 `VNPresentationController` as the authoritative background, character, and CG runtime state. It requires Unity-wired CanvasGroups for the screen black overlay, current and incoming backgrounds, CG, and each M3 CharacterVisualRoot.
- `VNYarnTransitionCommands` registers `vn_fade_to_black`, `vn_fade_from_black`, `vn_bg_crossfade`, `vn_show_fade`, `vn_hide_fade`, `vn_cg_fade_in`, and `vn_cg_fade_out` as awaited `IEnumerator` commands. Existing M3 immediate commands remain separate and unchanged.
- Deferred beyond M4: production voice acting, final audio mastering, advanced voice scheduling, lipsync, white flash, blur, screen shake, advanced transition presets, persistent audio/transition state, and settings UI or per-category user volume controls.

## M5-02 persistent storage kernel

- `SaveSlotData` is a plain JsonUtility-compatible schema-version-1 DTO. It contains logical checkpoint/resume metadata, typed Yarn-variable arrays, presentation data, BGM data, save metadata, play time, and an optional thumbnail basename only; it never contains dictionaries or Unity object references.
- `VNSaveSlotKey` owns canonical filenames and validates Manual `0..11`, Auto `0..4`, and Quick `0`. The requested key is authoritative for the filesystem path, and deserialized `slotType` plus `slotIndex` must match it.
- The parameterless `VNSaveRepository` uses `Path.Combine(Application.persistentDataPath, "SaveData")`. Test code uses the explicit `CreateForTesting` root factory and never writes to the production path.
- Reads return Empty, Valid, Corrupted, Unsupported, or InvalidRequest. JsonUtility parsing is followed by structural validation; missing fields are never accepted by relying on JsonUtility defaults. All three variable arrays are required (an empty array represents no values), and future schemas are Unsupported without attempted reinterpretation.
- Writes serialize only data that passes schema-v1 validation. The repository writes a unique same-directory `.tmp` file in UTF-8, calls `FileStream.Flush(true)`, then performs first-write `File.Move` or overwrite `File.Replace`. It does not fall back to delete-then-copy.
- Auto-slot allocation is deterministic: lowest Empty first; otherwise oldest Valid timestamp parsed from round-trip UTC `"O"` data; equal timestamps choose lowest index. Unsupported and Corrupted slots are not automatic overwrite candidates.
- `VNPlayTimeTracker` is an unwired finite non-negative seconds holder with set/restore and explicit advance operations. Scene timing ownership and all pause policy remain deferred.
- M5-03/M5-04 must still connect the storage DTO to Yarn checkpoints and variables, presentation, BGM, and later UI/thumbnail services. M5-02 does not alter Yarn, scene, prefab, audio, or presentation runtime behaviour.

## M5-03 checkpoint and Yarn-state runtime contract

- `VNCheckpointCatalog` is the project-authored authority for `VNCheckpointDefinition` entries. It rejects incomplete, invalid, or duplicate lowercase-snake-case checkpoint IDs and chapter IDs, requires exact non-empty resume nodes and scene titles, and validates every catalog resume node through the supplied `YarnProject.NodeNames` API.
- `VNCheckpointContext` is an immutable runtime value. `VNCheckpointService` exposes only `HasCurrentCheckpoint` and `TryGetCurrentCheckpoint`; it has no startup behaviour and never returns a mutable catalog definition. The service changes context only after catalog and assigned-runner Yarn-node validation succeeds.
- `VNYarnCheckpointCommands` owns the exact `<<vn_checkpoint checkpoint_id>>` bridge with M3/M4-style add-on-enable/remove-on-disable registration. A rejected command changes no variables, disk state, presentation state, audio state, or current context, and the bridge emits one project error.
- `VNYarnVariableSnapshot` obtains all three typed dictionaries from `DialogueRunner.VariableStorage.GetAllVariables()`, serializes sorted plain-array entries, and validates them with the M5-02 serializer contract. Restore builds complete ordinal dictionaries before one `SetAllVariables(floats, strings, bools, clear: true)` call; no conversion failure can create a progressive restore.
- M5-03 originally introduced `VNYarnSaveCoordinator.TryComposeTechnicalSave`; M5-04 upgrades it to a complete M3/M4 logical DTO without calling user-facing save UI. It derives checkpoint/resume/chapter/title solely from context and uses controller-owned presentation/audio snapshots.
- `ValidateLoad` is a no-mutation phase: it requires a Valid repository read (therefore supported schema), an exact saved checkpoint/resume match with the catalog, current runner Yarn-node validation, and a complete variable restore plan. Failures before the boundary preserve dialogue, variables, checkpoint context, and M3/M4 state.
- `ExecuteValidatedLoad` is the later mutation phase: only if `DialogueRunner.IsDialogueRunning` is true does it await `Stop`; it then performs the single variable restore, adopts the validated context, and awaits `StartDialogue(catalogResumeNode)`. A failure after the stop boundary is reported without rollback. M5-03 never uses `Dialogue.CurrentNode` as persisted state.

## M5-04 complete M3/M4 snapshot and restore contract

- `VNPresentationController.TryCaptureStableState` serializes `CurrentBackgroundId`, `CurrentCGId`, and `VisibleCharacters` into the frozen `PresentationState`. Characters are ordered by M3 slot (`far_left` through `far_right`) with ordinal character-ID tie-break. It never reads arbitrary transforms or Image sprites to infer logical state.
- `TryPrepareRestore` validates the background/CG catalog IDs (empty means clear), each exact character/expression, exact M3 slot/facing, finite positive scale, unique character IDs, unique slots, and configured slot views. It returns an immutable-by-convention logical plan without changing visuals. `RestorePreparedState` clears old occupancy, restores/clears background, restores visible characters, restores/clears CG, normalizes CanvasGroup alpha, and neutralizes speaker focus.
- `VNTransitionController` wraps all existing M4 transition enumerators to report `IsTransitionActive`. `NormalizeForLoad` cancels operations, clears screen fade and incoming-background buffer, and removes alpha residue; `FinalizeStableStateAfterLoad` restores stable current/background/CG/character alpha after immediate M3 reconstruction. No M4 fade command is replayed during load.
- `VNAudioController` owns `CurrentBgmId` and `IsBgmTransitionActive`. `bgm_play` commits requested ID immediately; completed crossfade commits target ID; pause/resume retain ID; completed stop clears it. Source A/B identity remains private.
- BGM capture emits empty ID/zero seconds for silence. For a logical BGM with usable sample metadata, capture uses `AudioSource.timeSamples / AudioClip.frequency`, preserving a paused source position where `AudioSource.time` may be zero. Saved positions are finite/non-negative. Restore planning wraps looped BGM by clip duration and clamps non-looped BGM to its last playable sample before any mutation.
- Load calls `VNAudioController.NormalizeTransientForLoad` to cancel M4 audio operations, stop both BGM sources, clear logical state, and stop stale SFX. `RestorePreparedState` then uses canonical source A with catalog clip/loop/default volume and normalized sample position, starts it playing, and leaves source B cleared. Voice remains exclusively in the guarded Yarn presenter lifecycle.
- `VNYarnSaveCoordinator.ValidateLoad` now prepares, in order, repository/checkpoint/resume, Yarn variables, presentation, BGM, and play time. Only after all plans exist does execution guard `Stop`, normalize M4 presentation/audio, restore variables, presentation, BGM, and play time, adopt checkpoint context, and start the exact catalog node. There is no rollback after the stop boundary.
- `TryWriteCompleteAutoSave` is a backend-only capability: it calls `AllocateNextAutoSlot`, requires the same complete stable composition, and writes the normal schema-v1 DTO. It has no checkpoint-event subscription or Unity Scene reference.

## M5-05 Save/Load application and thumbnail contract

- `VNSaveLoadController` is the single MonoBehaviour application boundary. Serialized references are the M5 modal, Dialogue Runner, Checkpoint Service, M3 Presentation Controller/Catalog, M4 Transition Controller, M4 Audio Controller, and the existing `VNInputActions` `Dialogue/Advance` `InputActionReference`; `autosaveOnSuccessfulCheckpoint` controls automatic subscriptions. In `Awake` it constructs the parameterless production repository, its `VNPlayTimeTracker`, complete coordinator, and thumbnail service. Its `Update` advances the tracker by finite `Time.unscaledDeltaTime`.
- `VNSaveLoadModal` is a uGUI-only view with one CanvasGroup, category/navigation/confirmation buttons, TMP fields, one SlotContainer, and one reusable `VNSaveSlotItem` prefab. Runtime listeners are registered in code; no per-slot persistent `Button.onClick` wiring is required. It pools six cards and rebinds/hides them instead of creating a new page each time.
- `VNSaveLoadSlotModelBuilder` turns `InspectAllSlots` output into card models. Manual is page 0 physical `0..5` / labels `1..6`, page 1 physical `6..11` / labels `7..12`; Auto is one page containing Valid saves sorted timestamp-descending then every other state in physical index order; Quick is one `quick_00` model. Timestamp display converts storage UTC to local time in invariant `yyyy-MM-dd HH:mm:ss`; play time is invariant `HH:MM:SS` with unwrapped hours.
- `VNSaveLoadInteractionPolicy` makes state handling explicit: Save/Manual Empty writes, Save/Manual Valid requests confirmation, Save/Auto is disabled, Save/Quick intentionally writes `quick_00`, and Load accepts only Valid. Corrupted and Unsupported are not silently treated as Empty. `VNSaveLoadOperationResult` separates no-checkpoint/unstable composition, repository, invalid/unsupported load, and non-fatal thumbnail-warning outcomes.
- `VNYarnSaveCoordinator.TryComposeCompleteSave` is the production complete-composition name. The retained `TryComposeTechnicalSave` method is a compatibility alias only. Complete composition now assigns `VNSaveSlotKey`'s canonical JPG basename before `TryWriteCompleteSave` performs the repository write.
- `VNThumbnailService` accepts only the canonical safe basename for the requested slot, loads bytes into an item-owned `Texture2D`, and returns a placeholder result for missing/corrupt image data. Its sidecar write is a same-directory temporary-file + flush + `File.Move`/`File.Replace` operation. It never mutates, invalidates, or rolls back JSON. Save replacement best-effort removes the old canonical JPG after JSON success and before the end-of-frame replacement capture; the controller suppresses that card's thumbnail until replacement succeeds, preventing a knowingly stale live card image even if file deletion fails.
- Screenshot capture is intentionally runtime-only: `WaitForEndOfFrame`, `ScreenCapture.CaptureScreenshotAsTexture`, deterministic center-crop UV scale/offset to a temporary `480x270` RenderTexture, `ReadPixels` into RGB24, `EncodeToJPG(75)`, and full temporary texture/RenderTexture release. During capture the modal CanvasGroup has alpha zero and is non-interactable but still blocks raycasts; it is restored afterward. Correct screenshot contents, crop, and Game View timing need user Play Gate verification.
- `VNCheckpointService.CheckpointEntered` occurs only after explicit `vn_checkpoint` validation/adoption. `VNSaveLoadController` subscribes in `OnEnable` and writes a complete auto save using normal repository allocation. A `VNCheckpointAutosaveGuard` consumes exactly one ID match marked by a prepared load before `ExecuteValidatedLoad` resumes its node. It is cleared when load start returns so an unrelated future checkpoint is never suppressed.
- Opening a modal suppresses only the serialized Dialogue/Advance action; it does not stop DialogueRunner, disable Yarn presenters, reset a line, or change time scale. Closing restores the action's prior enabled state. Load keeps it suppressed through the transaction and restores it afterward. The existing EventSystem/InputSystemUIInputModule remains active for modal controls.
- `VNCharacterDefinition.saveIcon` is optional and lookup-only. Up to five card Image slots resolve saved character IDs through `VNPresentationCatalog`; no Unity object reference is serialized into save JSON.

### Required user Unity wiring after M5-05

- Create a `SaveLoadRuntime` GameObject with `VNSaveLoadController`, and assign its modal, DialogueRunner, CheckpointService, PresentationController, TransitionController, AudioController, PresentationCatalog, and `VNInputActions/Dialogue/Advance` action reference. Leave `autosaveOnSuccessfulCheckpoint` enabled unless intentionally disabling auto saves.
- Create `VNCanvas/ModalLayer/SaveLoadModal` with `VNSaveLoadModal`, a CanvasGroup, full-screen raycast-blocking visual root, header/title/close button, three category buttons, SlotContainer, footer navigation buttons/page TMP text/status TMP text, and inactive overwrite panel/message/confirm/cancel. Assign each serialized field.
- Create exactly one `VNSaveSlotItem` prefab with Button, RawImage, placeholder object, metadata TMP fields, Empty/Corrupted/Unsupported state roots/text, normal metadata root, optional icon root, and up to five Image icon fields. Assign it to the modal; do not hand-create 12 cards.
- Create `VNCheckpointCatalog` and configure its definitions. Put `VNCheckpointService` and `VNYarnCheckpointCommands` on the same or separate active runtime GameObject; assign the catalog to the service and assign both DialogueRunner and that service to the command component. The controller's `OnEnable` subscription then needs no UnityEvent wiring.

## M5 final reconciliation

- The required Unity wiring is complete: `SaveLoadRuntime` owns the controller, checkpoint service, and Yarn checkpoint bridge; `VNCanvas/ModalLayer/SaveLoadModal` owns the modal; `VNSaveSlotItem` is the single reusable card prefab; and `M5_CheckpointCatalog` supplies the two imported smoke mappings. No Play-Gate-only opener objects are serialized.
- The production scene baseline remains `M2_UI_START`. `M5_SAVE_LOAD_START` and `M5_CHECKPOINT_A`/`M5_CHECKPOINT_B` are imported technical smoke nodes, not the normal start node.
- Load is a two-phase transaction: validate the complete JSON/checkpoint/Yarn/presentation/audio/play-time plans without mutation, then stop an active runner if needed, normalize transient M4 state, restore the prepared logical state, adopt the checkpoint context, and `StartDialogue` at the catalog resume node. The one matching checkpoint event on that re-entry is consumed by the autosave guard; later checkpoint entries autosave normally.
- Repository verification covers serialized wiring and EditMode contracts. Visual/runtime behavior is separately USER-VERIFIED by the completed M5 Play Gate.

## M6 convenience runtime

- Manual input flows `InputAction → VNConvenienceInputRouter → VNConvenienceController → VNLineAdvancerInputBridge → Yarn LineAdvancer`.
- For each line, `VNLineLifecyclePresenter` resolves the delivering DialogueRunner's unique enabled LinePresenter. The active VNDialoguePanel `VNLineLifecycleMarkupHandler` callback marks full display; an authoritative TMP visual observer is an idempotent watchdog. `VNDialogueSessionState` is the sole Backlog/full-display/read authority.
- Auto waits for full display, text-length delay, and optional Voice completion before using the shared advance bridge. Skip applies ReadOnly/All policy through that same bridge and never selects choices.
- Interaction gates arbitrate hidden UI, M6 Backlog/Settings modals, and M5 Save/Load ownership. Hide changes only DialogueLayer and QuickControlLayer CanvasGroups.
- M5 Load validates, signals loading, stops dialogue, awaits LinePresenter quiescence, restores, adopts the checkpoint, and starts the resume node. This barrier prevents a stale presenter flash.
