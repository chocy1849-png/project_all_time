# M13-02 Shared UI Components — implementation notes

Status: work in progress; not yet approved for integration.

The builder `M13SharedPrefabBuilder` produces seven prefabs under `Assets/_Project/Prefabs/UI/M13`: SharedPanel, SharedButton, SharedTab, SharedCard, SharedScrollbar, SharedSlider and SharedToggle. Build through the Unity menu Tools > M13 > Build shared panel and controls. Rebuilding overwrites the generated prefab contents; modify the builder, not the generated instances. Existing prefab GUIDs are preserved by Unity's PrefabUtility.

Panel uses sliced B_panel and B_secondary images, with anchored header and content. Button, tab and card use standard uGUI Buttons; the owner invokes `M13SharedControlState.SetActive` for selected state. Focus is an independent visual, suppressed while the Selectable is not interactable. `VNUIThemeBinding.SetDestructive` selects the button's destructive color variant. `M13SharedCard.SetContent` supplies text and optional thumbnail, and `SetLocked` controls interaction; it does not read or write save data. Slider, Toggle and Scrollbar use their standard uGUI controls and retain their normal input ownership. The scrollbar is intended for a dedicated gutter and must be assigned to a ScrollRect by its owner.

Sprites are the `B_` Direction B assets. Labels use the dedicated dynamic `M13_Shared_Korean_SDF` and its tracked NotoSansKR source. Panel and Button/Tab/Card use the existing M11 theme binding; Slider, Toggle and Scrollbar currently use their sprite colors and native Selectable color transitions. Controllers integrating M13-03 through M13-06 must provide content, selected state, layout and navigation, attach the scrollbar to their own ScrollRect, and retain existing persistence and modal ownership.

Limitations: input interaction, ScrollRect integration, full Korean rendering and responsive visual acceptance remain subject to the validation report. The generated controls are not a replacement for integration and screen-specific accessibility review.
