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

## REPOSITORY-VERIFIED

- Unity version is 6000.3.21f1.
- `Assets/_Project/Scenes/VN_Main.unity` and its `.meta` file exist.
- Visible Meta Files is enabled.
- Force Text serialization is enabled.
- `Packages/manifest.json` and `Packages/packages-lock.json` exist.
- The package baseline includes `com.unity.render-pipelines.universal` 17.0.3.
- `Packages/manifest.json` and `Packages/packages-lock.json` specify Yarn Spinner 3.2.7.
- The M1 Yarn project and technical smoke script exist.

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
