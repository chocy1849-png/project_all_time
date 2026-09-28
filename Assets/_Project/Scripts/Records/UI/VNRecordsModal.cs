using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.UI
{
    /// <summary>Shared visibility and navigation shell for the Records sections.</summary>
    [DisallowMultipleComponent]
    public sealed class VNRecordsModal : MonoBehaviour
    {
        [SerializeField] private CanvasGroup modalCanvasGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button timelineTabButton;
        [SerializeField] private Button galleryTabButton;
        [SerializeField] private Button archiveTabButton;
        [SerializeField] private Button achievementsTabButton;
        [SerializeField] private GameObject timelineRoot;
        [SerializeField] private GameObject galleryRoot;
        [SerializeField] private GameObject archiveRoot;
        [SerializeField] private GameObject achievementsRoot;
        [SerializeField] private GameObject timelineSelectedIndicator;
        [SerializeField] private GameObject gallerySelectedIndicator;
        [SerializeField] private GameObject archiveSelectedIndicator;
        [SerializeField] private GameObject achievementsSelectedIndicator;

        private bool isOpen;

        public bool IsOpen => isOpen;
        public VNRecordsTab ActiveTab { get; private set; } = VNRecordsTab.Timeline;

        public event Action CloseRequested;
        public event Action Opened;
        public event Action Closed;
        public event Action<VNRecordsTab> ActiveTabChanged;

        private void Awake()
        {
            SetVisible(false);
            SetAllRootsInactive();
            SetIndicators(VNRecordsTab.Timeline);
        }

        private void OnEnable()
        {
            closeButton?.onClick.AddListener(HandleCloseClicked);
            timelineTabButton?.onClick.AddListener(HandleTimelineTabClicked);
            galleryTabButton?.onClick.AddListener(HandleGalleryTabClicked);
            archiveTabButton?.onClick.AddListener(HandleArchiveTabClicked);
            achievementsTabButton?.onClick.AddListener(HandleAchievementsTabClicked);
        }

        private void OnDisable()
        {
            closeButton?.onClick.RemoveListener(HandleCloseClicked);
            timelineTabButton?.onClick.RemoveListener(HandleTimelineTabClicked);
            galleryTabButton?.onClick.RemoveListener(HandleGalleryTabClicked);
            archiveTabButton?.onClick.RemoveListener(HandleArchiveTabClicked);
            achievementsTabButton?.onClick.RemoveListener(HandleAchievementsTabClicked);
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (modalCanvasGroup == null) return Fail("CanvasGroup reference is missing.", out diagnostic);
            if (closeButton == null) return Fail("Close button reference is missing.", out diagnostic);
            if (timelineTabButton == null) return Fail("Timeline tab button reference is missing.", out diagnostic);
            if (galleryTabButton == null) return Fail("Gallery tab button reference is missing.", out diagnostic);
            if (archiveTabButton == null) return Fail("Archive tab button reference is missing.", out diagnostic);
            if (achievementsTabButton == null) return Fail("Achievements tab button reference is missing.", out diagnostic);
            if (timelineRoot == null) return Fail("Timeline root reference is missing.", out diagnostic);
            if (galleryRoot == null) return Fail("Gallery root reference is missing.", out diagnostic);
            if (archiveRoot == null) return Fail("Archive root reference is missing.", out diagnostic);
            if (achievementsRoot == null) return Fail("Achievements root reference is missing.", out diagnostic);
            if (HasDuplicateRoots()) return Fail("Each Records tab must have a distinct root GameObject.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool TryOpen()
        {
            if (!TryValidateWiring(out _)) return false;
            if (isOpen) return true;

            SetVisible(true);
            SetAllRootsInactive();
            SetRootActive(GetRoot(VNRecordsTab.Timeline), true);
            ActiveTab = VNRecordsTab.Timeline;
            SetIndicators(ActiveTab);
            isOpen = true;
            Opened?.Invoke();
            return true;
        }

        public bool Close()
        {
            if (!isOpen)
            {
                SetVisible(false);
                SetAllRootsInactive();
                return true;
            }
            if (modalCanvasGroup == null) return false;

            SetVisible(false);
            SetAllRootsInactive();
            isOpen = false;
            Closed?.Invoke();
            return true;
        }

        public bool TrySelectTab(VNRecordsTab tab)
        {
            if (!isOpen || !Enum.IsDefined(typeof(VNRecordsTab), tab)) return false;
            if (ActiveTab == tab) return true;

            ActiveTab = tab;
            ApplyTabState(tab);
            ActiveTabChanged?.Invoke(tab);
            return true;
        }

        private void ApplyTabState(VNRecordsTab tab)
        {
            SetAllRootsInactive();
            SetRootActive(GetRoot(tab), true);
            SetIndicators(tab);
        }

        private GameObject GetRoot(VNRecordsTab tab)
        {
            return tab switch
            {
                VNRecordsTab.Timeline => timelineRoot,
                VNRecordsTab.Gallery => galleryRoot,
                VNRecordsTab.Archive => archiveRoot,
                VNRecordsTab.Achievements => achievementsRoot,
                _ => null,
            };
        }

        private void SetIndicators(VNRecordsTab tab)
        {
            SetRootActive(timelineSelectedIndicator, tab == VNRecordsTab.Timeline);
            SetRootActive(gallerySelectedIndicator, tab == VNRecordsTab.Gallery);
            SetRootActive(archiveSelectedIndicator, tab == VNRecordsTab.Archive);
            SetRootActive(achievementsSelectedIndicator, tab == VNRecordsTab.Achievements);
        }

        private void SetVisible(bool visible)
        {
            if (modalCanvasGroup == null) return;
            modalCanvasGroup.alpha = visible ? 1f : 0f;
            modalCanvasGroup.interactable = visible;
            modalCanvasGroup.blocksRaycasts = visible;
        }

        private void SetAllRootsInactive()
        {
            SetRootActive(timelineRoot, false);
            SetRootActive(galleryRoot, false);
            SetRootActive(archiveRoot, false);
            SetRootActive(achievementsRoot, false);
        }

        private bool HasDuplicateRoots()
        {
            var roots = new[] { timelineRoot, galleryRoot, archiveRoot, achievementsRoot };
            for (var i = 0; i < roots.Length; i++)
            {
                for (var j = i + 1; j < roots.Length; j++)
                {
                    if (roots[i] == roots[j]) return true;
                }
            }

            return false;
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }

        private static void SetRootActive(GameObject root, bool active)
        {
            if (root != null) root.SetActive(active);
        }

        private void HandleCloseClicked() => CloseRequested?.Invoke();
        private void HandleTimelineTabClicked() => TrySelectTab(VNRecordsTab.Timeline);
        private void HandleGalleryTabClicked() => TrySelectTab(VNRecordsTab.Gallery);
        private void HandleArchiveTabClicked() => TrySelectTab(VNRecordsTab.Archive);
        private void HandleAchievementsTabClicked() => TrySelectTab(VNRecordsTab.Achievements);
    }
}
