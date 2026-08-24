# Decision Log

## DEC-001 — Unity baseline

Status: ACCEPTED

- Latest installed Unity 6.3 LTS patch in the 6000.3.x line
- Alpha and beta editors prohibited

## DEC-002 — Project layout

Status: ACCEPTED

- Repository root equals Unity project root

## DEC-003 — Rendering baseline

Status: ACCEPTED

- The project uses the Universal Render Pipeline (URP).
- The current verified package baseline is `com.unity.render-pipelines.universal` 17.0.3.
- Existing Unity-generated URP assets and ProjectSettings are canonical.
- Do not migrate rendering pipelines during Phase 0.
- Any future rendering-pipeline migration requires a separate accepted decision.

## DEC-004 — Target platform

Status: ACCEPTED

- Windows PC first
- 1920×1080 reference target

## DEC-005 — Source control

Status: ACCEPTED

- GitHub and Git
- Visible Meta Files
- Force Text
- Package lock tracked

## DEC-006 — Narrative tooling

Status: ACCEPTED

- Yarn Spinner planned
- Not installed in Phase 0

## DEC-007 — Content authority

Status: ACCEPTED

- Story is draft
- No story canon is established in Phase 0

## DEC-008 — Phase 0 scope

Status: ACCEPTED

- No gameplay implementation

## DEC-009 — M1 narrative runtime

Status: ACCEPTED

- Yarn Spinner 3.2.7 is the M1 narrative-runtime package.
- The OpenUPM package identifier is `dev.yarnspinner.unity`.
- The built-in InMemoryVariableStorage is used for M1 only.
- Persistent variable integration is deferred to M5.
- The official Line Presenter and Options Presenter are temporary M1 smoke UI.
- Custom VN presentation is deferred to M2.

## DEC-010 — M2 core VN play screen

Status: ACCEPTED

- The project-owned uGUI VN play screen is the M2 presentation shell.
- Yarn Spinner supplied LinePresenter and OptionsPresenter remain the runtime presenter implementations.
- The existing Yarn LineAdvancer is reused for click/Space hurry-then-advance behavior.
- The existing InputSystemUIInputModule is reused for UI navigation.
- A custom DialoguePresenterBase is not required in M2.
- Background, Character, and CG presentation are deferred to M3.
- Audio and transitions are deferred to M4.
- Persistence is deferred to M5.
- Convenience UX is deferred to M6.
- Settings are deferred to M7.

## DEC-011 — M3 presentation runtime contract

Status: ACCEPTED

- M3 presentation uses project-owned uGUI `Image` references; `SpriteRenderer` is not used.
- The fixed character slots are `far_left`, `left`, `center`, `right`, and `far_right`.
- Characters use one fixed pose assembled from layered `BackHair`, `Body`, and expression `Head` sprites. BackHair is optional; expressions swap only the Head sprite.
- `VNPresentationCatalog` and `VNCharacterDefinition` assets contain authored IDs and sprites; presentation state remains runtime-only.
- Speaker focus uses `LocalizedLine.CharacterName` aliases and Image tinting: active `1,1,1,1`, inactive `0.65,0.65,0.65,1`, over `0.20` seconds.
- M3 commands are immediate; no pose, transition, or fade commands are introduced. Animation and Live2D-style systems are out of scope.
- M3 artwork is temporary test artwork and is not production-final art.
- The M3 presentation smoke is technical, non-canon content used for manual observation of presentation states.

## DEC-012 ??M4 audio, voice, and transition runtime contract

Status: ACCEPTED

- M4 uses a project-owned `VNAudioCatalog` for BGM and SFX only. Voice clips are not catalogued there.
- BGM uses two AudioSources for source-to-source crossfades; pause uses `AudioSource.Pause` and resume uses `AudioSource.UnPause` to preserve playback position.
- SFX uses one AudioSource and `PlayOneShot`, allowing overlapping one-shots.
- Voice assets are optional per line. `VNOptionalVoicePresenter` is the sole Dialogue Runner voice presenter: it silently completes unvoiced lines, reports wrong-type associated assets, and delegates valid playback and lifecycle callbacks to Yarn Spinner 3.2.7's supplied `VoiceOverPresenter`. The delegated presenter uses VoiceSource and is configured not to advance dialogue when voice playback finishes. Voice is associated with `LocalizedLine.TextID` through Yarn's built-in localization Assets Folder path.
- M4 duration-based Yarn commands return `IEnumerator`, which Yarn Spinner 3.2.7 awaits before continuing dialogue.
- Screen, background, character, and CG transitions use independent alpha channels. Character fade is `CanvasGroup.alpha` on each CharacterVisualRoot; M3 speaker focus remains Image RGB tint.
- M3 immediate presentation commands remain unchanged. M4 adds only named fade/crossfade commands; `vn_pose` is not introduced.
- Production voice acting, final audio mastering, advanced voice scheduling, lipsync, white flash, blur, screen shake, advanced transition presets, persistent audio/transition state, and settings UI or per-category user volume controls remain deferred beyond M4.

## DEC-013 ??M5-03 checkpoint and Yarn-state contract

Status: ACCEPTED

- Save eligibility is established only by the exact Yarn command `<<vn_checkpoint checkpoint_id>>`. The command resolves a project-authored `VNCheckpointCatalog` definition, validates the catalog against the assigned Yarn Project's `NodeNames`, and only then replaces the current immutable checkpoint context. Unknown or invalid checkpoint IDs preserve the prior context and emit one project error from the command bridge.
- A checkpoint definition contains a unique lowercase-snake-case `checkpointId`, exact `resumeNode`, lowercase-snake-case `chapterId`, and non-empty `sceneTitle`. Save/load never infers a checkpoint or resume node from `Dialogue.CurrentNode`; that value remains diagnostics-only.
- Every production re-entry checkpoint uses a dedicated Yarn node. Its first instruction is the matching checkpoint command and it contains continuation only. All non-idempotent state changes must happen before jumping to that node. Line/instruction-position saves are prohibited.
- M5-03 snapshots all three Yarn variable kinds with `VariableStorageBehaviour.GetAllVariables()`. Entries are serialized into the M5-02 arrays in ordinal name order. Restore validates all DTO arrays and builds all typed dictionaries before one `SetAllVariables(..., clear: true)` call; malformed data cannot partially mutate storage.
- Technical save composition uses only a valid checkpoint context, current Yarn variables, play time, and a UTC timestamp. Chapter/title derive from the catalog context. Presentation and audio are deliberate neutral placeholders in M5-03, and composition does not write a slot or create autosaves.
- Load validation reads the repository, validates the saved checkpoint ID and exact saved resume node against the catalog and the assigned Yarn Project, and prepares variables before changing dialogue, variables, or checkpoint context. Only then may execution stop an active runner, restore all variables, adopt the validated context, and start the catalog resume node. `Stop()` is never called for an inactive runner. After the stop boundary there is intentionally no rollback claim.

## DEC-014 ??M5-04 full logical presentation and BGM snapshot

Status: ACCEPTED

- M5 saves the authoritative M3 logical presentation only: current background ID, current CG ID, and the exact visible-character set. A character records only character ID, expression ID, fixed slot, facing, and scale. There is no visibility flag for hidden entries, pose, speaker-active state, RGB tint, fade alpha, hierarchy coordinates, or transition progress.
- Saved character order is deterministic: `far_left`, `left`, `center`, `right`, `far_right`, then character ID. Restore validates catalog IDs, expressions, slots, facing, finite positive scale, and unique visible character/slot occupancy before any load mutation.
- M4 transitions are never persisted. Save composition is temporarily unavailable while an M4 presentation transition or BGM operation is active. Successful load cancels M4 operations, clears screen/incoming-background residue and stale SFX, restores M3 state immediately, normalizes CanvasGroup visibility, and resets speaker focus to neutral. M3 speaker focus remains RGB-only and is recalculated by the next Yarn line.
- `AudioState` stores only a logical BGM ID plus approximate seconds. It contains no source-A/source-B role, volume, pause state, fade/crossfade progress, SFX, voice, or mixer settings. Paused BGM capture uses `timeSamples / clip.frequency` when available; restored audio is always playing, not paused.
- A non-empty saved BGM must resolve through `VNAudioCatalog` and have a usable clip. Playback is normalized before mutation: looping clips wrap modulo duration; non-looping clips clamp to the final playable sample. Load restores one canonical BGM source at catalog loop/default volume and leaves its paired source silent and cleared.
- Yarn Spinner 3.2.7's existing guarded `DialogueRunner.Stop()` path remains voice cleanup authority: Dialogue Runner awaits `VNOptionalVoicePresenter.OnDialogueCompleteAsync`, which delegates to `VoiceOverPresenter.OnDialogueCompleteAsync`, and that presenter stops its VoiceSource. M5 adds no competing voice manager.
- M5-04 exposes, but does not wire, `TryWriteCompleteAutoSave()`. It allocates an auto slot and writes the same complete schema-v1 DTO only when checkpoint, presentation, and audio are stable. Scene/event/UI wiring remains deferred.

## DEC-015 — M5-05 Save/Load UI and thumbnail sidecars

Status: ACCEPTED

- M5 presents 12 Manual saves in two six-slot pages, five Auto saves in one newest-first inspection page, and one Quick slot. Physical slot identity is never changed by visual sorting.
- In Save mode, only Manual Empty slots write immediately; Manual Valid slots require an explicit overwrite confirmation; Manual Corrupted/Unsupported slots remain preserved and disabled. Auto is read-only. Quick Save deliberately overwrites `quick_00` without redirecting to Manual.
- In Load mode only Valid slots are interactive. Empty, Corrupted, and Unsupported saves remain visibly distinct and disabled. A missing/bad thumbnail never changes JSON validity or loadability.
- M5 thumbnail sidecars are optional canonical JPG files: `<slot-stem>.jpg`, `480x270`, 16:9 center crop, quality 75. Authoritative JSON stores only the canonical basename. JSON success is not rolled back for capture, decode, or sidecar-write failure.
- The project-owned controller owns a `VNPlayTimeTracker`, repository, complete M5 coordinator, checkpoint autosave subscription, and narrow Dialogue/Advance input suppression. `VNCheckpointService` emits a successful-checkpoint event but remains free of disk I/O.
- One load-resume guard consumes only the first successful checkpoint entry whose stable ID matches the restored checkpoint. Later normal checkpoint entries autosave normally.
- Scene, modal, prefab, catalog, input-action, and checkpoint-command wiring remain user-owned Unity work. M5-05 creates no Scene/Prefab/ScriptableObject assets and no final M5 smoke.

## DEC-016 — M5 final checkpoint-load reconciliation

Status: ACCEPTED

- M5 is finalized around schema-v1 JSON at `Application.persistentDataPath/SaveData`; JSON is authoritative and canonical JPG thumbnails are optional sidecars. Manual has 12 slots in two six-slot pages, Auto has five slots displayed newest-first, and Quick has one slot.
- The only persisted Yarn location is a validated checkpoint ID plus its exact catalog resume node. Load validates all persisted plans before mutation and resumes with `StartDialogue(resumeNode)`; arbitrary Yarn line/instruction resume is not supported.
- Presentation persistence remains logical M3 state only (background, CG, and visible character ID/expression/slot/facing/scale). Audio persistence remains logical BGM ID plus approximate playback seconds. Pose, speaker focus, RGB/alpha/transitional state, BGM source role, SFX, Voice, and mixer settings are not persisted.
- The one-shot matching checkpoint event after a load is intentionally consumed to suppress duplicate autosave. A later genuine checkpoint entry returns to normal autosave allocation.
- `VN_Main` keeps `M2_UI_START` as its normal start node. M5 smoke nodes are technical-only. Repository checks and the completed user Play Gate are recorded as distinct evidence classes in implementation state and the M5 Draft PR.

## DEC-017 — M6 core convenience UX

Status: ACCEPTED

- Backlog and Read History are application-session services only. Backlog records full-display occurrences; Read History records stable Yarn `TextID` values only after an authorized normal consume.
- ReadOnly Skip is the default policy. Skip All remains runtime-supported, but its user-facing policy control is deferred to M7. Auto and Skip are mutually exclusive and both use the shared LineAdvancer bridge.
- `VNConvenienceInputRouter` owns M6 Input System routing. Hide affects only DialogueLayer and QuickControlLayer; Backlog and Settings are M6 modal owners, while M5 retains Save/Load and overwrite ownership.
- M2's VNDialoguePanel LinePresenter is authoritative. M6 resolves it from the delivering DialogueRunner's unique enabled LinePresenter rather than trusting duplicate scene presenter components. The official ActionMarkupHandler callback is primary; TMP visual observation is an idempotent defensive watchdog.
- QuickLoad retains the M5 stop → LinePresenter visual-quiescence barrier → restore → StartDialogue ordering.
- M6 technical smoke, checkpoint, and voice assets are non-canon regression fixtures. The normal start node remains `M2_UI_START`.

## DEC-018 — M7 settings persistence kernel

Status: ACCEPTED

- Global settings are a project-owned schema-v1 JSON contract at `Application.persistentDataPath/Settings/settings.json`, deliberately independent from M5 save data at `Application.persistentDataPath/SaveData`.
- The first persisted contract contains only stable user preferences: display mode string, windowed dimensions, text/auto speed, normalized audio categories, skip and screen-shake flags, and Input System binding override JSON. Applying those values to runtime systems and exposing settings/rebinding UI are later M7 work.
- Valid writes use same-directory temporary UTF-8 files, a flush-to-disk boundary, and `File.Move`/`File.Replace`; delete-and-copy replacement is prohibited.
- Schema-v1 settings JSON must explicitly contain every required top-level contract field. Incomplete schema-v1 files are corrupt and follow the same preservation-first quarantine rule; explicit valid zero and false values remain valid.
- Malformed, missing/invalid-schema, and invalid schema-v1 JSON is moved intact to a unique sibling `.corrupt` file before defaults are used. If preservation cannot be completed, writes are blocked.
- Future schema files are preserved byte-for-byte, never quarantined, downgraded, parsed as v1, or overwritten by ordinary saves. They yield session defaults under explicit write protection until a future migration owns them.

## DEC-019 — M7 display runtime

Status: ACCEPTED

- M7 display runtime supports only `FullScreenMode.FullScreenWindow` and `FullScreenMode.Windowed`. Fullscreen requests use `Display.main.systemWidth` and `systemHeight`; no fullscreen resolution, monitor, refresh-rate, exclusive-fullscreen, or maximized-window choice is exposed.
- Windowed choices originate from `Screen.resolutions`, discard invalid entries, deduplicate by width and height only, ignore refresh rate, and order width then height ascending. Empty runtime lists retain the 1920×1080 project fallback.
- Persisted `windowedWidth` and `windowedHeight` always mean the last valid Windowed size. Entering fullscreen preserves them; returning to Windowed restores an exact available match, then 1920×1080 if available, otherwise the deterministic nearest option.
- User-initiated display changes persist through `VNSettingsService` before `Screen.SetResolution` is requested. Startup-style application can request an effective fallback without rewriting write-protected settings. Display requests are end-of-frame Unity requests, not synchronous confirmation that the OS window already changed.
- Settings UI, startup/scene wiring, resolution confirmation/revert UX, OS/monitor change reconciliation, and all non-display settings application remain deferred.

## DEC-020 — M7 text and Auto speed runtime

Status: ACCEPTED

- M7 applies text speed only through the unique enabled `LinePresenter` owned by the authoritative `DialogueRunner`. Yarn Spinner 3.2.7 copies `lettersPerSecond` into its ByLetter `LetterTypewriter` during Awake, so runtime application updates both `LinePresenter.lettersPerSecond` and the active `LetterTypewriter.CharactersPerSecond`.
- Text speed is a product/runtime clamp of 20–120 LPS with default 60. Storage validation remains only positive-value validation; applying a stored positive value outside the product range does not rewrite or corrupt the settings. A speed change never restarts, cancels, hurries, or otherwise changes the current line/occurrence lifecycle.
- `autoSpeedNormalized` maps linearly from 0..1 to a 1.5..0.5 M6 Auto delay multiplier; default 0.5 maps to 1.0. M6 first performs its existing clamped text-delay calculation, M7 multiplies that result, and M6's existing 0.80..4.00 final bounds apply afterward.
- Changing the Auto factor resets only its pending timer so the next eligible Auto tick re-arms with the new delay. It does not toggle Auto/Skip, change Skip policy, bypass voice completion, authorize full display, or consume choices.
- User changes persist through `VNSettingsService` before runtime mutation. Startup-style application consumes the effective snapshot without requiring a write, including when future-schema protection has made settings write-protected. UI and startup scene wiring remain deferred.

## DEC-021 — M7 audio settings runtime

Status: ACCEPTED

- M7 maps persisted normalized Master, BGM, SFX, and Voice preferences to Mixer attenuation only: zero is -80 dB, one is 0 dB, and intermediate values use `20 * log10(normalized)` clamped to -80..0 dB. M7 never produces positive user gain.
- The exact exposed Mixer parameter contract is `MasterVolumeDb`, `BgmVolumeDb`, `SfxVolumeDb`, and `VoiceVolumeDb`. Master and child attenuation are not manually combined in C#; the Mixer hierarchy composes them.
- M4 continues to own authored `AudioSource.volume`, BGM crossfades, pause/resume/fade-stop, restore normalization, and SFX `PlayOneShot` scales. Voice playback remains Yarn-owned. M7 has no source-volume, playback, or voice-lifecycle fallback.
- Runtime application validates all four exposed parameters before mutation, captures prior values for best-effort startup rollback, and persists user changes before applying one matching Mixer parameter. Future-schema write protection does not prevent read-only startup application.
- `AudioMixer.SetFloat` composition remains a future Start-or-later startup concern. Settings UI, scene wiring, and exposing Mixer attenuation parameters through the Unity Mixer UI remain deferred user/integration work.

## DEC-022 — M7 gameplay settings runtime

Status: ACCEPTED

- `skipUnread` maps exactly to existing M6 policy: false selects `VNSkipPolicy.ReadOnly`; true selects `VNSkipPolicy.All`. M7 changes only that policy after persistence and does not redesign Skip eligibility, scheduling, throttle, full-display authorization, read history, choices, voice behavior, or Auto/Skip mutual exclusion.
- M6 policy changes reset M6 Skip scheduling and publish its existing policy event, but do not synthesize an advance. The next normal M6 Tick evaluates the current line under the selected policy.
- `screenShakeEnabled` remains a schema-v1 persisted preference with default true. M7 exposes only a read-only future-consumer gate backed by `VNSettingsService`; it introduces no screen-shake consumer, camera behavior, event system, or package.
- Startup-style application reads effective settings without a write, including under future-schema protection. Settings UI and startup/scene wiring remain deferred. M7-05 Mixer parameter exposure remains a separate Unity user integration gate.
