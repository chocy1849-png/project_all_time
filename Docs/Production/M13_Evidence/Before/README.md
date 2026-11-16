# Current production baseline captures — representative, incomplete

Captured 2026-11-16 from `Assets/_Project/Scenes/VN_Main.unity` in Unity 6000.3.21f1 Editor Play Mode, Pipeline `capture_game_view` source `screen`, actual `Screen.width × Screen.height = 1920×1080`. Storage identity was temporarily set to `M13PreviewIsolated/AllTimeAudit` and verified via `Application.persistentDataPath`; values were restored afterward. UI actions were invoked through the existing Buttons, category APIs and, for the in-game menu, the existing Cancel handler by reflection. These are not physical-input tests. No save was written deliberately. The isolated state has empty slots and limited progress.

- [Title](Before_title.png), [in-game menu](Before_ingame.png), [Backlog](Before_backlog.png).
- Settings: [Display](Before_settings_display.png), [Text](Before_settings_Text.png), [Audio](Before_settings_Audio.png), [Gameplay](Before_settings_Gameplay.png), [Controls](Before_settings_Controls.png).
- Load: [Manual](Before_load_Manual_corrected.png), [Auto](Before_load_Auto_corrected.png), [Quick](Before_load_Quick_corrected.png).
- Save: [Manual](Before_save_Manual.png), [Auto](Before_save_Auto.png), [Quick](Before_save_Quick.png).
- Records: [Timeline](Before_records_Timeline.png), [Gallery](Before_records_Gallery.png), [Archive](Before_records_Archive.png), [Achievements](Before_records_Achievements.png).

These do not cover populated saves, every locked/unlocked state, long/narrator Backlog, 720p/900p live production layouts or every interaction. Continue audit before declaring M13-00 complete.
