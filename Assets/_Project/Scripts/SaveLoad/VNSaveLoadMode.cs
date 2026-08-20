using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProjectAllTime.VN.SaveLoad
{
    public enum VNSaveLoadMode
    {
        Save,
        Load,
    }

    public enum VNSaveLoadCategory
    {
        Manual,
        Auto,
        Quick,
    }

    public enum VNSaveSlotInteraction
    {
        Disabled,
        WriteManual,
        ConfirmManualOverwrite,
        WriteQuick,
        Load,
    }

    /// <summary>Pure Save/Load card interaction policy shared by UI and tests.</summary>
    public static class VNSaveLoadInteractionPolicy
    {
        public static VNSaveSlotInteraction GetInteraction(VNSaveLoadMode mode, VNSaveLoadCategory category, VNSaveSlotState state)
        {
            if (mode == VNSaveLoadMode.Load)
                return state == VNSaveSlotState.Valid ? VNSaveSlotInteraction.Load : VNSaveSlotInteraction.Disabled;

            switch (category)
            {
                case VNSaveLoadCategory.Manual:
                    if (state == VNSaveSlotState.Empty) return VNSaveSlotInteraction.WriteManual;
                    return state == VNSaveSlotState.Valid ? VNSaveSlotInteraction.ConfirmManualOverwrite : VNSaveSlotInteraction.Disabled;
                case VNSaveLoadCategory.Quick:
                    // QuickSave intentionally overwrites quick_00, including a
                    // malformed prior quick sidecar/JSON state.
                    return VNSaveSlotInteraction.WriteQuick;
                default:
                    return VNSaveSlotInteraction.Disabled;
            }
        }
    }

    /// <summary>
    /// One-shot application policy for the checkpoint command immediately
    /// re-entered by a restored save. It intentionally matches by stable ID.
    /// </summary>
    public sealed class VNCheckpointAutosaveGuard
    {
        private string expectedCheckpointId;

        public void ExpectLoadedCheckpoint(string checkpointId) => expectedCheckpointId = checkpointId;

        public bool ConsumeIfExpected(string checkpointId)
        {
            if (string.IsNullOrEmpty(expectedCheckpointId) || checkpointId != expectedCheckpointId) return false;
            expectedCheckpointId = null;
            return true;
        }

        public void Clear() => expectedCheckpointId = null;
    }

    /// <summary>Small UI-ready projection of one authoritative slot inspection.</summary>
    public sealed class VNSaveSlotViewModel
    {
        public VNSaveSlotKey SlotKey { get; }
        public VNSaveSlotState State { get; }
        public string SlotLabel { get; }
        public string ChapterText { get; }
        public string SceneTitleText { get; }
        public string SavedAtText { get; }
        public string PlayedTimeText { get; }
        public string ThumbnailFileName { get; internal set; }
        public IReadOnlyList<string> VisibleCharacterIds { get; }

        public bool HasMetadata => State == VNSaveSlotState.Valid;
        public bool IsWritableManualSave => SlotKey.SlotType == VNSaveSlotType.Manual && (State == VNSaveSlotState.Empty || State == VNSaveSlotState.Valid);
        public bool RequiresManualOverwriteConfirmation => SlotKey.SlotType == VNSaveSlotType.Manual && State == VNSaveSlotState.Valid;
        public bool IsLoadable => State == VNSaveSlotState.Valid;

        internal VNSaveSlotViewModel(
            VNSaveSlotKey slotKey,
            VNSaveSlotState state,
            string slotLabel,
            string chapterText,
            string sceneTitleText,
            string savedAtText,
            string playedTimeText,
            string thumbnailFileName,
            List<string> visibleCharacterIds)
        {
            SlotKey = slotKey;
            State = state;
            SlotLabel = slotLabel;
            ChapterText = chapterText;
            SceneTitleText = sceneTitleText;
            SavedAtText = savedAtText;
            PlayedTimeText = playedTimeText;
            ThumbnailFileName = thumbnailFileName;
            VisibleCharacterIds = visibleCharacterIds;
        }
    }

    /// <summary>
    /// Pure pagination, slot ordering, and display-format policy. It knows no
    /// Unity UI objects and never reads save files itself.
    /// </summary>
    public static class VNSaveLoadSlotModelBuilder
    {
        public const int ManualSlotsPerPage = 6;

        public static int GetPageCount(VNSaveLoadCategory category)
        {
            switch (category)
            {
                case VNSaveLoadCategory.Manual: return 2;
                case VNSaveLoadCategory.Auto:
                case VNSaveLoadCategory.Quick: return 1;
                default: return 1;
            }
        }

        public static int ClampPage(VNSaveLoadCategory category, int requestedPage)
            => Math.Max(0, Math.Min(GetPageCount(category) - 1, requestedPage));

        public static IReadOnlyList<VNSaveSlotViewModel> Build(
            IReadOnlyList<VNSaveSlotInspection> inspections,
            VNSaveLoadCategory category,
            int requestedPage,
            TimeZoneInfo displayTimeZone = null)
        {
            var models = new List<VNSaveSlotViewModel>();
            if (inspections == null) return models;

            var page = ClampPage(category, requestedPage);
            switch (category)
            {
                case VNSaveLoadCategory.Manual:
                    var firstManualIndex = page * ManualSlotsPerPage;
                    for (var index = firstManualIndex; index < firstManualIndex + ManualSlotsPerPage; index++)
                        AddModelForKey(models, inspections, new VNSaveSlotKey(VNSaveSlotType.Manual, index), displayTimeZone);
                    break;

                case VNSaveLoadCategory.Auto:
                    var autoInspections = new List<VNSaveSlotInspection>();
                    foreach (var inspection in inspections)
                        if (inspection != null && inspection.SlotKey.SlotType == VNSaveSlotType.Auto) autoInspections.Add(inspection);
                    autoInspections.Sort(CompareAutoInspectionsNewestFirst);
                    foreach (var inspection in autoInspections)
                        models.Add(CreateModel(inspection, displayTimeZone));
                    break;

                case VNSaveLoadCategory.Quick:
                    AddModelForKey(models, inspections, new VNSaveSlotKey(VNSaveSlotType.Quick, 0), displayTimeZone);
                    break;
            }

            return models;
        }

        public static string FormatSavedAtLocal(string utcIso8601, TimeZoneInfo displayTimeZone = null)
        {
            if (!VNSaveSerializer.TryParseUtcTimestamp(utcIso8601, out var timestamp)) return "—";
            var timeZone = displayTimeZone ?? TimeZoneInfo.Local;
            var local = TimeZoneInfo.ConvertTime(timestamp, timeZone);
            return local.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string FormatPlayedTime(float playedSeconds)
        {
            if (!VNPlayTimeTracker.IsValidPlayedSeconds(playedSeconds)) return "00:00:00";
            var totalSeconds = (long)Math.Floor(playedSeconds);
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }

        private static void AddModelForKey(List<VNSaveSlotViewModel> models, IReadOnlyList<VNSaveSlotInspection> inspections, VNSaveSlotKey key, TimeZoneInfo displayTimeZone)
        {
            foreach (var inspection in inspections)
            {
                if (inspection != null && inspection.SlotKey == key)
                {
                    models.Add(CreateModel(inspection, displayTimeZone));
                    return;
                }
            }

            // InspectAllSlots normally supplies every physical key. This keeps a
            // UI contract deterministic if an incomplete injected list is used.
            models.Add(CreateModel(new VNSaveSlotInspection(key, VNSaveReadResult.Empty()), displayTimeZone));
        }

        private static VNSaveSlotViewModel CreateModel(VNSaveSlotInspection inspection, TimeZoneInfo displayTimeZone)
        {
            var key = inspection.SlotKey;
            var read = inspection.ReadResult ?? VNSaveReadResult.InvalidRequest("Slot inspection is incomplete.");
            var save = read.State == VNSaveSlotState.Valid ? read.SaveData : null;
            var characters = new List<string>();
            if (save?.presentationState?.characters != null)
            {
                foreach (var character in save.presentationState.characters)
                    if (character != null && !string.IsNullOrEmpty(character.characterId)) characters.Add(character.characterId);
            }

            var slotLabel = key.SlotType == VNSaveSlotType.Manual
                ? (key.SlotIndex + 1).ToString(CultureInfo.InvariantCulture)
                : key.SlotType == VNSaveSlotType.Auto
                    ? "Auto " + (key.SlotIndex + 1).ToString(CultureInfo.InvariantCulture)
                    : "Quick";
            return new VNSaveSlotViewModel(
                key,
                read.State,
                slotLabel,
                string.IsNullOrWhiteSpace(save?.chapterId) ? "—" : save.chapterId,
                string.IsNullOrWhiteSpace(save?.sceneTitle) ? "—" : save.sceneTitle,
                save == null ? "—" : FormatSavedAtLocal(save.savedAtUtcIso8601, displayTimeZone),
                save == null ? "00:00:00" : FormatPlayedTime(save.playedSeconds),
                save?.thumbnailFileName ?? string.Empty,
                characters);
        }

        private static int CompareAutoInspectionsNewestFirst(VNSaveSlotInspection left, VNSaveSlotInspection right)
        {
            var leftTime = default(DateTimeOffset);
            var rightTime = default(DateTimeOffset);
            var leftIsValid = left?.ReadResult?.State == VNSaveSlotState.Valid && VNSaveSerializer.TryParseUtcTimestamp(left.ReadResult.SaveData.savedAtUtcIso8601, out leftTime);
            var rightIsValid = right?.ReadResult?.State == VNSaveSlotState.Valid && VNSaveSerializer.TryParseUtcTimestamp(right.ReadResult.SaveData.savedAtUtcIso8601, out rightTime);
            if (leftIsValid && rightIsValid)
            {
                var timestampOrder = rightTime.CompareTo(leftTime);
                if (timestampOrder != 0) return timestampOrder;
            }
            else if (leftIsValid) return -1;
            else if (rightIsValid) return 1;

            // Non-valid states intentionally stay after valid saves in physical
            // index order; their state is never reinterpreted as empty.
            return left.SlotKey.SlotIndex.CompareTo(right.SlotKey.SlotIndex);
        }
    }
}
