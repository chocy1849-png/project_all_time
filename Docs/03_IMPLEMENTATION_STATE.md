# Implementation State

## Phase 0

- Unity project created with Unity 6000.3.21f1.
- 2D project using the Universal Render Pipeline (URP).
- Repository root layout: the Git repository root is the Unity project root.
- Main scene: `Assets/_Project/Scenes/VN_Main.unity`.
- No C# gameplay code is present.
- No save/load implementation is present.
- No custom VN UI is present.

## M1 Narrative Runtime

- Yarn Spinner 3.2.7 is installed through OpenUPM as `dev.yarnspinner.unity`.
- `Assets/_Project/Yarn/GameNarrative.yarnproject` includes the M1 Yarn source files.
- `Assets/_Project/Yarn/M1_RUNTIME_START.yarn` is technical smoke content only and is not story canon.
- Persistent variable integration is deferred to M5.
- Custom VN presentation is deferred to M2.

## USER-VERIFIED

- Windows baseline Build and Run.
- The official Yarn Spinner Dialogue System is configured in `VN_Main.unity`.
- `GameNarrative` is assigned to Dialogue Runner.
- Built-in InMemoryVariableStorage is configured for M1.
- Default Line Presenter and Options Presenter are configured.
- Start Automatically is enabled with `M1_RUNTIME_START` as the start node.
- M1 Play Gate passed: narrator and named-speaker lines, three options, all branch transitions, path-dependent results, variable updates, Play Mode variable reset, and normal end-node completion were verified.
- Console errors: 0.

## M6 Core Convenience UX — COMPLETE

### REPOSITORY-VERIFIED

- M6 supplies session-only Backlog and Read History, Auto/Skip runtime, Next/Hide and M5 Save/Load bridges, QuickControlBar, Backlog modal, Settings shell, and Input System routing.
- Full-display authority resolves the unique active LinePresenter from the delivering DialogueRunner. The VNDialoguePanel callback is primary and its TMP view is the defensive watchdog; both converge on one session transition.
- M6 technical smoke, voice fixture, and checkpoint catalog entry remain non-canon fixtures. The normal `VN_Main` start node is `M2_UI_START`.

### USER-VERIFIED

- USER M6 PLAY GATE: PASS.
- Backlog displays dialogue with speaker/narration handling; Auto progresses; ReadOnly Skip advances the repeated read line and stops at the following unread line.
- QuickLoad no longer flashes a stale/resume line. The authoritative VNDialoguePanel LinePresenter and handler wiring were corrected.

### DEFERRED

- Persistent read IDs, gallery/archive/CG progress, and cloud/Steam saves remain M8/M9 or later work.
- Settings contents/persistence, rebinding UI, final keyboard scheme, and a user-facing Skip All policy are M7 work.
- Backlog voice replay, backlog persistence, choice history, rewind, auto/skip choice selection, Skip transition speed-up, and main menu remain later scope.

## M3 Presentation Runtime

- Project-owned code contracts now define fixed-pose layered character definitions (`BackHair` + `Body` + expression `Head`), the presentation catalog, fixed-slot uGUI presentation, Yarn command registration, and speaker focus.
- Unity-authored M3 ScriptableObject assets, layered slot views, and `VN_Main.unity` references are wired for the M3 presentation runtime.
- `Assets/_Project/Yarn/M3_PRESENTATION_SMOKE.yarn` is technical smoke content only and is not story canon. Its manual checkpoints expose each M3 presentation state for verification.

## M3 USER-VERIFIED PLAY GATE

- Background A/B replacement, CG show/clear, and dialogue UI ordering were verified.
- Both characters moved through FarLeft, Left, Center, Right, and FarRight without stale or duplicate visuals.
- Character A default → smile → default Head-expression switching was verified; Body and optional BackHair behavior remained correct.
- Whole-character facing and scale changes were verified through the shared layered visual root.
- `LocalizedLine.CharacterName` speaker focus was verified for `M3 A` and `M3 B`; narration hides the name container and restores active brightness to all visible characters.
- The M3 smoke reaches normal completion. Console errors: 0.
- Current M3 artwork is temporary test artwork, not production-final art.

## M4 Audio / Voice / Transition Code Contracts

- `VNAudioCatalog`, `VNAudioController`, and `VNYarnAudioCommands` define BGM and SFX contracts. Unity-owned M4 audio assets, AudioMixer groups, AudioSources, and catalog entries are wired in `VN_Main`.
- `VNTransitionController` and `VNYarnTransitionCommands` define screen, background, character, and CG transition contracts. Unity-owned CanvasGroups, the second background Image, and TransitionLayer black overlay are wired in `VN_Main`.
- M3 character slot views now expose an optional CanvasGroup alpha channel. M3 Image RGB speaker focus and all immediate M3 commands retain their established behavior.
- M4 voice uses built-in localization asset association with per-line optional voice. `VNOptionalVoicePresenter` filters unvoiced lines and delegates valid AudioClip playback to Yarn Spinner 3.2.7 `VoiceOverPresenter`; the delegated presenter must not be registered directly with Dialogue Runner. Missing voice is normal, while a wrong-type associated asset remains an error.
- `M4_AUDIO_TRANSITION_SMOKE.yarn` is technical, non-canon content. The normal Scene Start Node remains `M2_UI_START`.

## M4 USER-VERIFIED PLAY GATE

- BGM A/B playback, loop, audible crossfade, pause/resume position preservation, and fade-stop were verified.
- `ui_confirm` single and repeated one-shots plus `door_close` resolve and play correctly.
- Unvoiced lines complete silently. `#line:m4_voice_test` resolves to `m4_voice_test.wav`, plays through VoiceSource without duplicate playback or stale replay, and does not auto-advance; normal LineAdvancer input advances after voice completion.
- Screen fade, background crossfade, CharacterVisualRoot CanvasGroup fades, and CG fades were verified. Character alpha remains independent from M3 speaker-focus RGB tint, and underlying presentation survives CG removal.
- Established M3 immediate commands remain immediate, M4 duration commands remain awaited, and the smoke completes normally. Console Error: 0. Unhandled Command: 0.

## M5-02 Storage Kernel

- Project-owned schema-version-1 save DTOs, slot keys, JSON serialization, persistent-file repository, and a narrow play-time tracker are implemented under `Assets/_Project/Scripts/SaveLoad/`.
- The authoritative save JSON is stored under `Application.persistentDataPath/SaveData`: 12 manual slots (`manual_00`–`manual_11`), 5 auto slots (`auto_00`–`auto_04`), and one quick slot (`quick_00`).
- Writes validate the complete plain DTO, write UTF-8 JSON to a same-directory unique temporary file, flush it to disk, and then use `File.Move` for first write or `File.Replace` for overwrite. The known-good authoritative file is never deliberately deleted before a completed temporary write exists.
- Slot inspection explicitly classifies Empty, Valid, Corrupted, Unsupported, and invalid caller requests. Missing fields, malformed JSON, key mismatches, invalid timestamps/numbers, and unsafe thumbnail basenames are Corrupted; future schemas are Unsupported.
- Auto allocation chooses the lowest Empty auto slot first, otherwise the oldest Valid save by parsed UTC timestamp with lowest-index tie breaking. Corrupted and Unsupported auto slots are preserved.
- Runtime checkpoint/Yarn variables, presentation restore, BGM restore, thumbnails, and save/load UI remain deferred to later M5 milestones.

## M5-03 Checkpoint + Yarn State

- `VNCheckpointCatalog`, `VNCheckpointDefinition`, and `VNCheckpointService` establish an explicit, validated checkpoint context from `<<vn_checkpoint checkpoint_id>>`. The catalog contains stable checkpoint/chapter IDs, exact dedicated Yarn re-entry nodes, and scene-title metadata; it validates every entry against the assigned Yarn Project node list.
- `VNYarnCheckpointCommands` follows the existing M3/M4 handler lifecycle and registers only `vn_checkpoint`. It does not start dialogue, write saves, or affect M3/M4 presentation/audio state.
- `VNYarnVariableSnapshot` captures every float, string, and bool from Yarn Variable Storage in deterministic ordinal order. Restore fully validates and converts all arrays before its single clear-and-replace storage call.
- `VNYarnSaveCoordinator` established the no-mutation checkpoint/Yarn-variable load preflight which M5-04 extends to complete presentation/audio plans.
- M5-03 intentionally leaves presentation/audio capture and restore, thumbnails, save/load UI, production autosave policy, scene wiring, and authored checkpoint catalog assets for later work.

## M5-04 Full Presentation / Audio Snapshot

- `VNPresentationController` now captures and validates the authoritative M3 logical background, CG, and visible-character state, and immediately restores a prepared stable state. Character snapshots are slot-ordered and contain no hidden entries, pose, speaker tint/state, alpha, or transition data.
- `VNTransitionController` tracks active M4 transition operations and normalizes load-time screen, current/incoming background, CG, and CharacterVisualRoot CanvasGroup state without replaying Yarn fade commands. Speaker focus is reset neutral and remains owned by `VNSpeakerFocusPresenter` on the next line.
- `VNAudioController` now tracks current logical BGM ID independently of its internal source A/B implementation, captures sample-based approximate playback seconds (including paused position), validates a BGM restore plan, stops stale SFX, and restores one canonical stable playing source. Voice remains in the existing Dialogue Runner → optional voice → Yarn VoiceOverPresenter lifecycle.
- `VNYarnSaveCoordinator` now composes complete schema-v1 DTOs with real M3/M4 state, refuses composition while presentation/BGM operations are active, prevalidates presentation/audio/play-time before load mutation, and executes normalized restore after guarded dialogue stop. The backend exposes an unwired complete-autosave method using the same DTO/path.
- M5-04 does not create catalog assets, alter `VN_Main`, wire checkpoint events, enable UI saves/loads, create thumbnails, or add a final M5 smoke/play gate.

## M5-05 Save/Load UI Runtime + Thumbnail Contracts

- `VNSaveLoadController` is the scene composition root for a parameterless production `VNSaveRepository`, its controller-owned `VNPlayTimeTracker`, `VNYarnSaveCoordinator`, `VNThumbnailService`, modal interaction, and checkpoint autosave subscription. It exposes `OpenSave`, `OpenLoad`, `Close`, `SaveManual`, `Load`, `QuickSave`, `QuickLoad`, and `RefreshSlots` for later controls/shortcuts.
- `VNSaveLoadModal` owns the small reusable uGUI view contract and creates at most six reusable `VNSaveSlotItem` instances beneath its authored SlotContainer. It changes bindings on tab/page refresh and releases each item-owned runtime thumbnail texture when re-bound or closed. The modal uses Save/Load mode, Manual/Auto/Quick category, and zero-based page state.
- Manual maps physical `0..5` to displayed labels `1..6` and physical `6..11` to `7..12`. Auto keeps all five physical entries but displays valid saves newest-first by parsed UTC timestamp; every non-valid entry follows in physical-index order. Quick exposes only `quick_00`.
- Every Manual, Quick, and application-triggered Auto write uses `TryWriteCompleteSave`, which composes the full M5-04 checkpoint/Yarn/presentation/BGM/playtime DTO and assigns the canonical JPG basename before authoritative JSON replacement. Stable snapshot failure leaves the target JSON unchanged.
- JPG capture is end-of-frame only. Modal visuals use a CanvasGroup alpha hide while retaining raycast blocking, then `ScreenCapture.CaptureScreenshotAsTexture`, a GPU `Graphics.Blit` center crop into `480x270`, `ReadPixels` to RGB24, and `EncodeToJPG(75)`. Capture is not batch/EditMode visual proof and remains a later human Play Gate item.
- Thumbnail sidecars are optional. Successful JSON saves first invalidate any old canonical JPG best-effort, then refresh the image asynchronously. The currently running UI forces a placeholder while replacement is pending or fails, so a deletion failure cannot show a knowingly stale card image. Capture/write failure leaves JSON valid. `RawImage` receives caller-owned runtime textures, which `VNSaveSlotItem` destroys before replacement, on close, and on destroy.
- `VNCharacterDefinition` adds optional `saveIcon`; card icons derive runtime character IDs from saved presentation state through `VNPresentationCatalog`. No Sprite reference is persisted.
- `VNCheckpointService` now publishes a successful checkpoint-entry event after context validation. The controller subscribes automatically when its serialized service reference is enabled. A one-shot matching restored-checkpoint event is consumed to avoid an unintended load-resume autosave; later entries autosave according to the M5-02 rotation policy.
- The existing M2 `Dialogue/Advance` InputActionReference is disabled only while modal/load input is owned, not by stopping DialogueRunner, disabling presenters, or changing time scale. The exact scene InputActionReference still requires user wiring.
- No `VN_Main` Scene, Prefab, ScriptableObject, package, ProjectSettings, Yarn, or final M5 smoke asset is created or changed in M5-05. USER M5 SAVELOAD UI WIRING is required before visual/play verification.

## M5-06 / M5-07 Final Reconciliation

- M5 is complete. `VN_Main` contains the production `SaveLoadRuntime`, `SaveLoadModal` under `ModalLayer`, the reusable `VNSaveSlotItem` prefab, and the `M5_CheckpointCatalog` asset. The normal DialogueRunner Start Node remains `M2_UI_START`; `M5_SAVE_LOAD_START` is technical smoke content only.
- The catalog has the two validated definitions `m5_checkpoint_a` → `M5_CHECKPOINT_A` and `m5_checkpoint_b` → `M5_CHECKPOINT_B`. Each smoke re-entry node begins with its matching checkpoint command and continues with re-entry-safe content.
- Save JSON remains authoritative and thumbnails remain optional JPG sidecars. Load prevalidates repository/schema, checkpoint and exact resume node, Yarn variables, presentation, audio, and play time before runtime mutation. A valid load restores the prepared state and starts the catalog-associated node; it does not resume arbitrary Yarn lines.
- The final UI uses two Manual pages of six cards, one newest-first Auto page of five cards, and one Quick card. Missing thumbnails render as a fallback without disabling a valid load. The normal M2 `Dialogue/Advance` input is suppressed only while modal/load interaction owns it.
- User Play Gate results are recorded below as USER-VERIFIED. They are user observations, not an additional Codex runtime reproduction.

## DEFERRED BEYOND M5

- Cloud/cross-device save, encryption, compression, migrations beyond the v1 extension point, unlimited pages, save search/filter, advanced save UI animation, final keyboard Quick Save/Load UX, settings persistence, M6 convenience UX, M7 settings/input, M8 meta-progress, and M9 gallery/archive/achievement persistence.

## REPOSITORY-VERIFIED

- Unity version is 6000.3.21f1.
- `Assets/_Project/Scenes/VN_Main.unity` and its `.meta` file exist.
- Visible Meta Files is enabled.
- Force Text serialization is enabled.
- `Packages/manifest.json` and `Packages/packages-lock.json` exist.
- The package baseline includes `com.unity.render-pipelines.universal` 17.0.3.
- `Packages/manifest.json` and `Packages/packages-lock.json` specify Yarn Spinner 3.2.7.
- The M1 Yarn project and technical smoke script exist.
- M5-03 editor tests cover catalog validity/duplicates/IDs/nodes, checkpoint preservation, deterministic Yarn capture, clear restore and malformed rejection, technical composition, and no-mutation load-validation failures.
- M5-04 editor tests cover M3 capture/validation/restore normalization, M4 BGM capture/normalization/restore and SFX cleanup, full composition stability rejection, and pre-mutation full-load presentation/audio failures.
- The final focused EditMode suite passed 39/39. Serialized checks confirm the M5 catalog mappings, smoke-node import, Noto Save/Load TMP references, modal/prefab wiring, and normal `M2_UI_START` baseline. `git diff --check` passes.

## M5 USER-VERIFIED PLAY GATE

- Manual save/load, empty-slot saves, overwrite confirmation, slot metadata/time/chapter rendering, and Manual pagination passed.
- Yarn float/string/bool state, checkpoint/resume-node restore, re-entry safety, presentation/background/CG/character state, speaker-focus recalculation, logical BGM with approximate position, and transient SFX/Voice/crossfade cleanup passed.
- Thumbnail generation/overwrite/modal exclusion/missing-thumbnail fallback, five-slot Auto rotation/newest-first ordering, Quick overwrite/load, persistence across Play Mode restart and fresh load, and malformed/unsupported/invalid-checkpoint failure handling passed.
- The failed-load no-mutation invariant and one-shot load-resume autosave suppression (followed by normal later autosave) passed. Console Error and Unhandled Command were both zero; basic M2/M3/M4 runtime behavior remained intact.

## M2 Core VN Play Screen

- A project-owned uGUI VN play screen is configured in `VN_Main.unity`.
- Yarn Spinner supplied LinePresenter and OptionsPresenter remain the runtime presenter implementations.
- The existing Yarn LineAdvancer is reused for click/Space hurry-then-advance behavior.
- The existing InputSystemUIInputModule is reused for UI navigation.
- No custom DialoguePresenterBase is present.
- `Assets/_Project/Yarn/M2_UI_SMOKE.yarn` is technical smoke content only and is not story canon.

## Deferred after M2

- Background, Character, and CG presentation: M3.
- Audio and transitions: M4.
- Persistence: M5.
- Convenience UX: M6.
- Settings: M7.

## M2 USER-VERIFIED PLAY GATE

- Narrator mode hides the speaker-name container; named-speaker mode shows the speaker name separately.
- Dialogue text, typewriter behavior, and click/Space hurry-then-advance behavior were verified.
- Two-, three-, and four-option layouts work with mouse and keyboard navigation/selection.
- Normal advance input does not bypass active options.
- No duplicate M1 presenter UI appears.
- Layout was verified at 1280×720, 1600×900, 1920×1080, and 2560×1440.
- Dialogue reaches `M2_UI_END` normally.
- Console errors: 0.
