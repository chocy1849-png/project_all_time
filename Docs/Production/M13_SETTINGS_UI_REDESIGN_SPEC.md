# M13-03 Production Settings UI redesign specification

`Assets/_Project/Scenes/VN_Main.unity` keeps the original `VNCanvas/ModalLayer/SettingsModal/Panel` and its `VNSettingsPanel`, five category view components, close button and status field. The presentation now uses a matte panel, left category navigation and a separate content surface. The existing CanvasScaler, EventSystem and modal controller remain owners of layout context and modal/input arbitration.

The M13 shared Panel and Tab sprites supply the panel and navigation treatments; M13 Slider and Toggle prefabs are nested as visual instances under the original Selectables. Their duplicate Selectable/state components are removed from the instances, while each original Slider remains on the same object as its original `VNSettingsSliderCommit`. Button, focus and active visuals use the shared Direction B assets and `M13SharedControlState`. Text uses `M13_Shared_Korean_SDF`. `M13SettingsSceneBuilder` is an editor-only reproducibility tool; production does not depend on it.

Display retains the original TMP dropdowns and authority-owned option mapping; Korean display names do not change persisted mode IDs. Text and Audio retain preview and commit boundaries. Gameplay retains original toggles and write restrictions. Controls retains the six original `VNRebindItem` objects and `VNInputRebindService`; listening text is teal, other actions are disabled while capture is active, and Escape and lifecycle cancellation remain service-owned. Category active markers are driven by `VNSettingsPanel.ShowCategory` independently of EventSystem focus.

This work does not implement Save/Load, Records or Backlog redesign. User visual acceptance remains pending.
