using System;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.SaveLoad;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Presentation
{
    public sealed class VNProfileContent
    {
        public string Name { get; }
        public string Facts { get; }
        public bool RequiresFirstReviewLine { get; }
        public VNProfileContent(string name, string facts, bool requiresFirstReviewLine)
        {
            Name = name ?? string.Empty; Facts = facts ?? string.Empty;
            RequiresFirstReviewLine = requiresFirstReviewLine;
        }
    }

    /// <summary>Three equal information cards. No route choice or durable state; story supplies ordinary review lines.</summary>
    [DisallowMultipleComponent]
    public sealed class VNProfileBrowser : MonoBehaviour
    {
        [SerializeField] private CanvasGroup root;
        [SerializeField] private GameObject overview;
        [SerializeField] private GameObject detail;
        [SerializeField] private Button[] profileButtons = new Button[3];
        [SerializeField] private TMP_Text[] profileNames = new TMP_Text[3];
        [SerializeField] private TMP_Text[] reviewedLabels = new TMP_Text[3];
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailFacts;
        [SerializeField] private Button backButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private VNInteractionGate interactionGate;
        [SerializeField] private VNSaveLoadController saveLoad;
        [SerializeField] private string reviewedText = "확인함";

        private readonly bool[] reviewed = new bool[3];
        private readonly UnityAction[] inspectActions = new UnityAction[3];
        private VNProfileContent[] content;
        public bool IsBrowsing { get; private set; }
        public int CurrentProfile { get; private set; } = -1;
        public bool IsOverview => IsBrowsing && CurrentProfile < 0;
        public bool IsReviewPending { get; private set; }
        public bool AllReviewed => reviewed[0] && reviewed[1] && reviewed[2];
        public bool HasReviewed(int index) => index >= 0 && index < 3 && reviewed[index];
        public event Action<int> ReviewRequested;
        public event Action Completed;

        public bool BeginBrowse(VNProfileContent[] profiles)
        {
            if (!isActiveAndEnabled || IsBrowsing || profiles == null || profiles.Length != 3 ||
                Array.Exists(profiles, item => item == null) || !HasReferences() || saveLoad.IsLoadInProgress ||
                !interactionGate.TryAcquireStoryInteraction(this, HandleCancel)) return false;
            content = (VNProfileContent[])profiles.Clone();
            Array.Clear(reviewed, 0, reviewed.Length);
            IsBrowsing = true; CurrentProfile = -1; IsReviewPending = false;
            Render(); return true;
        }

        public bool Inspect(int index)
        {
            if (!IsOverview || index < 0 || index >= 3) return false;
            CurrentProfile = index;
            IsReviewPending = !reviewed[index] && content[index].RequiresFirstReviewLine;
            if (!IsReviewPending) reviewed[index] = true;
            interactionGate.SetStoryReadingEnabled(this, IsReviewPending);
            Render();
            if (IsReviewPending) ReviewRequested?.Invoke(index);
            return true;
        }

        /// <summary>Story calls only after the first ordinary review line finishes through the shared presenter.</summary>
        public bool CompleteReview(int index)
        {
            if (!IsBrowsing || !IsReviewPending || CurrentProfile != index || saveLoad.IsLoadInProgress) return false;
            reviewed[index] = true; IsReviewPending = false;
            interactionGate.SetStoryReadingEnabled(this, false);
            Render(); return true;
        }

        public bool Back()
        {
            if (!IsBrowsing || IsReviewPending || CurrentProfile < 0) return false;
            CurrentProfile = -1; Render(); return true;
        }

        public bool TryContinue()
        {
            if (!IsOverview || !AllReviewed || IsReviewPending) return false;
            CancelBrowse();
            Completed?.Invoke(); return true;
        }

        public void CancelBrowse()
        {
            interactionGate?.ReleaseStoryInteraction(this);
            IsBrowsing = false; CurrentProfile = -1; IsReviewPending = false; content = null;
            Array.Clear(reviewed, 0, reviewed.Length); Render();
        }

        private bool HandleCancel()
        {
            if (!IsBrowsing) return false;
            Back(); return true; // Overview/pending thought consumes Esc without closing or advancing.
        }

        private void OnEnable()
        {
            if (saveLoad != null) saveLoad.LoadStateChanged += HandleLoad;
            for (var i = 0; i < Math.Min(3, profileButtons.Length); i++)
            {
                var index = i;
                inspectActions[i] = () => Inspect(index);
                if (profileButtons[i] != null) profileButtons[i].onClick.AddListener(inspectActions[i]);
            }
            if (backButton != null) backButton.onClick.AddListener(HandleBack);
            if (continueButton != null) continueButton.onClick.AddListener(HandleContinue);
            CancelBrowse();
        }

        private void OnDisable()
        {
            if (saveLoad != null) saveLoad.LoadStateChanged -= HandleLoad;
            for (var i = 0; i < Math.Min(3, profileButtons.Length); i++)
                if (profileButtons[i] != null && inspectActions[i] != null) profileButtons[i].onClick.RemoveListener(inspectActions[i]);
            if (backButton != null) backButton.onClick.RemoveListener(HandleBack);
            if (continueButton != null) continueButton.onClick.RemoveListener(HandleContinue);
            CancelBrowse();
        }

        private void HandleBack() => Back();
        private void HandleContinue() => TryContinue();
        private void HandleLoad(bool loading) { if (loading) CancelBrowse(); }

        private bool HasReferences() => root != null && overview != null && detail != null && detailName != null &&
            detailFacts != null && backButton != null && continueButton != null && interactionGate != null && saveLoad != null &&
            profileButtons.Length == 3 && profileNames.Length == 3 && reviewedLabels.Length == 3 &&
            !Array.Exists(profileButtons, item => item == null) && !Array.Exists(profileNames, item => item == null) &&
            !Array.Exists(reviewedLabels, item => item == null);

        private void Render()
        {
            if (root != null) { root.alpha = IsBrowsing ? 1f : 0f; root.interactable = IsBrowsing; root.blocksRaycasts = IsBrowsing; }
            if (overview != null) overview.SetActive(IsOverview);
            if (detail != null) detail.SetActive(IsBrowsing && !IsOverview);
            for (var i = 0; i < 3; i++)
            {
                if (i < profileButtons.Length && profileButtons[i] != null) profileButtons[i].interactable = IsOverview;
                if (i < profileNames.Length && profileNames[i] != null) profileNames[i].text = content == null ? string.Empty : content[i].Name;
                if (i < reviewedLabels.Length && reviewedLabels[i] != null) reviewedLabels[i].text = reviewed[i] ? reviewedText : string.Empty;
            }
            if (detailName != null) detailName.text = IsBrowsing && !IsOverview ? content[CurrentProfile].Name : string.Empty;
            if (detailFacts != null) detailFacts.text = IsBrowsing && !IsOverview ? content[CurrentProfile].Facts : string.Empty;
            if (backButton != null) backButton.interactable = IsBrowsing && !IsOverview && !IsReviewPending;
            if (continueButton != null) continueButton.interactable = IsOverview && AllReviewed;
        }
    }
}
