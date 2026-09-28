using System;
using System.Collections.Generic;
using ProjectAllTime.VN.Records.Timeline;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Timeline.UI
{
    /// <summary>Renders fresh, spoiler-safe snapshots supplied by VNTimelineRuntime.</summary>
    [DisallowMultipleComponent]
    public sealed class VNTimelineView : MonoBehaviour
    {
        [SerializeField] private Transform chapterContent;
        [SerializeField] private VNTimelineChapterItem chapterItemPrefab;
        [SerializeField] private ScrollRect entryScrollRect;
        [SerializeField] private Transform entryContent;
        [SerializeField] private VNTimelineEntryItem entryItemPrefab;
        [SerializeField] private TMP_Text selectedChapterTitleText;
        [SerializeField] private GameObject globalEmptyStateRoot;
        [SerializeField] private GameObject chapterLockedStateRoot;
        [SerializeField] private GameObject chapterEmptyStateRoot;
        [SerializeField] private GameObject chapterContentStateRoot;
        [SerializeField] private TMP_Text selectedChapterStateText;

        private readonly List<VNTimelineChapterItem> chapterItems = new();
        private readonly List<VNTimelineEntryItem> entryItems = new();
        private readonly List<FlattenedEntry> flattenedEntries = new();
        private VNTimelineRuntime runtime;
        private VNTimelineSnapshot currentSnapshot;
        private string selectedChapterId;
        private bool initialized;

        public string SelectedChapterId => selectedChapterId;
        public string LastDiagnostic { get; private set; }

        private void Awake()
        {
            SetStateRoots(globalEmpty: false, locked: false, empty: false, content: false);
            ClearSelectedChapterText();
        }

        private void OnEnable()
        {
            if (initialized) Refresh();
        }

        public bool Initialize(VNTimelineRuntime timelineRuntime)
        {
            if (timelineRuntime == null)
            {
                LastDiagnostic = "A VNTimelineRuntime is required.";
                return false;
            }

            if (initialized)
            {
                if (!ReferenceEquals(runtime, timelineRuntime))
                {
                    LastDiagnostic = "VNTimelineView is already initialized with a different runtime.";
                    return false;
                }

                LastDiagnostic = null;
                if (isActiveAndEnabled) Refresh();
                return true;
            }

            runtime = timelineRuntime;
            initialized = true;
            LastDiagnostic = null;
            if (isActiveAndEnabled) Refresh();
            return true;
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (chapterContent == null) return Fail("Chapter content Transform is missing.", out diagnostic);
            if (chapterItemPrefab == null) return Fail("Chapter item prefab is missing.", out diagnostic);
            if (!chapterItemPrefab.TryValidateWiring(out var chapterDiagnostic))
                return Fail("Chapter item prefab: " + chapterDiagnostic, out diagnostic);
            if (entryScrollRect == null) return Fail("Entry ScrollRect is missing.", out diagnostic);
            if (entryContent == null) return Fail("Entry content Transform is missing.", out diagnostic);
            if (entryContent is not RectTransform entryContentRect || entryScrollRect.content != entryContentRect)
                return Fail("Entry ScrollRect content must reference the Entry Content RectTransform.", out diagnostic);
            if (entryItemPrefab == null) return Fail("Entry item prefab is missing.", out diagnostic);
            if (!entryItemPrefab.TryValidateWiring(out var entryDiagnostic))
                return Fail("Entry item prefab: " + entryDiagnostic, out diagnostic);
            if (selectedChapterTitleText == null) return Fail("Selected chapter title TMP_Text is missing.", out diagnostic);
            if (globalEmptyStateRoot == null) return Fail("Global empty state root is missing.", out diagnostic);
            if (chapterLockedStateRoot == null) return Fail("Locked chapter state root is missing.", out diagnostic);
            if (chapterEmptyStateRoot == null) return Fail("Empty chapter state root is missing.", out diagnostic);
            if (chapterContentStateRoot == null) return Fail("Chapter content state root is missing.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Refresh()
        {
            if (!initialized || runtime == null)
                return FailAndClear("Timeline runtime has not been initialized.");
            if (!TryValidateWiring(out var wiringDiagnostic))
                return FailAndClear(wiringDiagnostic);

            VNTimelineSnapshot snapshot;
            try
            {
                snapshot = runtime.BuildSnapshot();
            }
            catch (Exception exception)
            {
                return FailAndClear("Timeline snapshot could not be built: " + exception.Message);
            }

            if (snapshot == null || snapshot.Chapters == null)
                return FailAndClear("Timeline runtime returned no snapshot.");

            currentSnapshot = snapshot;
            var previousChapterId = selectedChapterId;
            var selectedChapter = ResolveSelection(snapshot.Chapters, previousChapterId);
            selectedChapterId = selectedChapter?.ChapterId;

            BindChapterItems(snapshot.Chapters, selectedChapterId);
            if (snapshot.Chapters.Count == 0)
            {
                SetStateRoots(globalEmpty: true, locked: false, empty: false, content: false);
                ClearSelectedChapterText();
                DeactivateEntryItems();
                ResetScrollToTop();
                LastDiagnostic = null;
                return true;
            }

            if (selectedChapter == null)
                return FailAndClear("Timeline snapshot contains chapters but no selectable chapter.");

            if (!BindSelectedChapter(selectedChapter))
                return FailAndClear(LastDiagnostic ?? "Timeline chapter content could not be rendered.");

            ResetScrollToTop();
            LastDiagnostic = null;
            return true;
        }

        public bool TrySelectChapter(string chapterId)
        {
            if (!initialized || currentSnapshot == null || string.IsNullOrWhiteSpace(chapterId)) return false;
            if (!currentSnapshot.TryGetChapter(chapterId, out var chapter)) return false;
            if (string.Equals(selectedChapterId, chapterId, StringComparison.Ordinal)) return true;

            selectedChapterId = chapterId;
            BindChapterItems(currentSnapshot.Chapters, selectedChapterId);
            if (!BindSelectedChapter(chapter))
            {
                FailAndClear(LastDiagnostic ?? "Timeline chapter content could not be rendered.");
                return false;
            }

            ResetScrollToTop();
            LastDiagnostic = null;
            return true;
        }

        private bool BindSelectedChapter(VNTimelineChapterRuntime chapter)
        {
            SetStateRoots(globalEmpty: false, locked: false, empty: false, content: false);
            if (selectedChapterTitleText != null) selectedChapterTitleText.text = chapter?.DisplayTitle ?? string.Empty;
            if (selectedChapterStateText != null) selectedChapterStateText.text = chapter == null ? string.Empty : FormatChapterState(chapter.State);
            DeactivateEntryItems();

            if (chapter == null)
            {
                SetStateRoots(globalEmpty: true, locked: false, empty: false, content: false);
                return true;
            }

            switch (chapter.State)
            {
                case VNTimelineChapterState.Locked:
                    SetStateRoots(globalEmpty: false, locked: true, empty: false, content: false);
                    return true;
                case VNTimelineChapterState.Available:
                    SetStateRoots(globalEmpty: false, locked: false, empty: true, content: false);
                    return true;
                case VNTimelineChapterState.InProgress:
                case VNTimelineChapterState.Completed:
                    if (chapter.Roots == null || chapter.Roots.Count == 0)
                    {
                        SetStateRoots(globalEmpty: false, locked: false, empty: true, content: false);
                        return true;
                    }
                    if (!TryFlatten(chapter.Roots, out var diagnostic))
                    {
                        LastDiagnostic = diagnostic;
                        return false;
                    }
                    SetStateRoots(globalEmpty: false, locked: false, empty: false, content: true);
                    return BindEntryItems();
                default:
                    LastDiagnostic = "Timeline chapter has an unsupported state.";
                    return false;
            }
        }

        private void BindChapterItems(IReadOnlyList<VNTimelineChapterRuntime> chapters, string selectedId)
        {
            EnsureChapterPool(chapters.Count);
            for (var index = 0; index < chapterItems.Count; index++)
            {
                var item = chapterItems[index];
                if (index < chapters.Count && chapters[index] != null)
                {
                    var chapter = chapters[index];
                    item.Bind(chapter.ChapterId, chapter.DisplayTitle, chapter.State,
                        string.Equals(chapter.ChapterId, selectedId, StringComparison.Ordinal), HandleChapterSelected);
                    if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
                }
                else
                {
                    item.Unbind();
                    if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
                }
            }
        }

        private bool BindEntryItems()
        {
            EnsureEntryPool(flattenedEntries.Count);
            for (var index = 0; index < entryItems.Count; index++)
            {
                var item = entryItems[index];
                if (index < flattenedEntries.Count)
                {
                    var entry = flattenedEntries[index];
                    if (!item.Bind(entry.DisplayTitle, entry.State, entry.Depth))
                    {
                        DeactivateEntryItems();
                        LastDiagnostic = "Timeline runtime exposed an unsupported entry state.";
                        return false;
                    }
                    if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
                }
                else
                {
                    item.Clear();
                    if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
                }
            }

            return true;
        }

        private bool TryFlatten(IReadOnlyList<VNTimelineEntryRuntime> roots, out string diagnostic)
        {
            flattenedEntries.Clear();
            foreach (var root in roots)
            {
                if (root == null) continue;
                if (!AppendPreOrder(root, 0, out diagnostic)) return false;
            }

            diagnostic = null;
            return true;
        }

        private bool AppendPreOrder(VNTimelineEntryRuntime entry, int depth, out string diagnostic)
        {
            if (entry.State != VNTimelineEntryState.Discovered && entry.State != VNTimelineEntryState.Completed)
            {
                diagnostic = "Timeline runtime exposed a hidden or unsupported entry state; entry metadata was withheld.";
                return false;
            }

            flattenedEntries.Add(new FlattenedEntry(entry.DisplayTitle, entry.State, Mathf.Max(0, depth)));
            foreach (var child in entry.Children)
            {
                if (child == null) continue;
                if (!AppendPreOrder(child, depth + 1, out diagnostic)) return false;
            }

            diagnostic = null;
            return true;
        }

        private void EnsureChapterPool(int count)
        {
            while (chapterItems.Count < count)
            {
                var item = Instantiate(chapterItemPrefab, chapterContent, false);
                item.gameObject.SetActive(false);
                chapterItems.Add(item);
            }
        }

        private void EnsureEntryPool(int count)
        {
            while (entryItems.Count < count)
            {
                var item = Instantiate(entryItemPrefab, entryContent, false);
                item.gameObject.SetActive(false);
                entryItems.Add(item);
            }
        }

        private void DeactivateChapterItems()
        {
            foreach (var item in chapterItems)
            {
                if (item == null) continue;
                item.Unbind();
                if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
            }
        }

        private void DeactivateEntryItems()
        {
            foreach (var item in entryItems)
            {
                if (item == null) continue;
                item.Clear();
                if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
            }
        }

        private void ResetScrollToTop()
        {
            if (entryScrollRect == null) return;
            Canvas.ForceUpdateCanvases();
            entryScrollRect.verticalNormalizedPosition = 1f;
        }

        private void HandleChapterSelected(string chapterId) => TrySelectChapter(chapterId);

        private void ClearSelectedChapterText()
        {
            if (selectedChapterTitleText != null) selectedChapterTitleText.text = string.Empty;
            if (selectedChapterStateText != null) selectedChapterStateText.text = string.Empty;
        }

        private void SetStateRoots(bool globalEmpty, bool locked, bool empty, bool content)
        {
            SetActive(globalEmptyStateRoot, globalEmpty);
            SetActive(chapterLockedStateRoot, locked);
            SetActive(chapterEmptyStateRoot, empty);
            SetActive(chapterContentStateRoot, content);
        }

        private bool FailAndClear(string diagnostic)
        {
            currentSnapshot = null;
            DeactivateChapterItems();
            DeactivateEntryItems();
            SetStateRoots(globalEmpty: false, locked: false, empty: false, content: false);
            ClearSelectedChapterText();
            LastDiagnostic = diagnostic;
            return false;
        }

        private static VNTimelineChapterRuntime ResolveSelection(
            IReadOnlyList<VNTimelineChapterRuntime> chapters,
            string selectedId)
        {
            if (!string.IsNullOrEmpty(selectedId))
            {
                foreach (var chapter in chapters)
                    if (chapter != null && string.Equals(chapter.ChapterId, selectedId, StringComparison.Ordinal)) return chapter;
            }

            foreach (var chapter in chapters)
            {
                if (chapter == null) continue;
                if (chapter.State == VNTimelineChapterState.Available ||
                    chapter.State == VNTimelineChapterState.InProgress ||
                    chapter.State == VNTimelineChapterState.Completed)
                    return chapter;
            }

            foreach (var chapter in chapters)
                if (chapter != null) return chapter;

            return null;
        }

        private static string FormatChapterState(VNTimelineChapterState state)
        {
            return state switch
            {
                VNTimelineChapterState.Locked => "Locked",
                VNTimelineChapterState.Available => "Available",
                VNTimelineChapterState.InProgress => "In progress",
                VNTimelineChapterState.Completed => "Completed",
                _ => string.Empty,
            };
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null) target.SetActive(active);
        }

        private readonly struct FlattenedEntry
        {
            public readonly string DisplayTitle;
            public readonly VNTimelineEntryState State;
            public readonly int Depth;

            public FlattenedEntry(string displayTitle, VNTimelineEntryState state, int depth)
            {
                DisplayTitle = displayTitle;
                State = state;
                Depth = depth;
            }
        }
    }
}
