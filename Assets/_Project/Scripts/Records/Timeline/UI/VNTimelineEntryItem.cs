using ProjectAllTime.VN.Records.Timeline;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Timeline.UI
{
    /// <summary>One pooled, non-interactive row in the hierarchical Timeline list.</summary>
    [DisallowMultipleComponent]
    public sealed class VNTimelineEntryItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private LayoutElement indentationSpacer;
        [SerializeField] private float indentationPerLevel = 24f;
        [SerializeField] private GameObject completedIndicator;
        [SerializeField] private Button replayButton;

        private string entryId;
        private Action<string> replayRequested;
        private bool replayAvailable;

        private void Awake() => DisableChildInteraction();
        private void OnEnable() => DisableChildInteraction();

        public bool TryValidateWiring(out string diagnostic)
        {
            if (titleText == null)
            {
                diagnostic = "Timeline entry item requires a title TMP_Text.";
                return false;
            }
            if (stateText == null)
            {
                diagnostic = "Timeline entry item requires a state TMP_Text.";
                return false;
            }
            if (indentationSpacer == null)
            {
                diagnostic = "Timeline entry item requires an indentation LayoutElement.";
                return false;
            }
            if (replayButton != null && !IsOwnedByItem(replayButton.transform))
            {
                diagnostic = "Timeline Replay button must belong to its entry item.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        public bool Bind(string displayTitle, VNTimelineEntryState state, int depth)
        {
            return Bind(null, displayTitle, state, depth, false, null);
        }

        public bool Bind(
            string entryId,
            string displayTitle,
            VNTimelineEntryState state,
            int depth,
            bool canReplay,
            Action<string> replayRequested)
        {
            if (state != VNTimelineEntryState.Discovered && state != VNTimelineEntryState.Completed)
            {
                Clear();
                return false;
            }

            if (titleText != null) titleText.text = displayTitle ?? string.Empty;
            if (stateText != null) stateText.text = state == VNTimelineEntryState.Completed ? "Completed" : "Discovered";
            SetActive(completedIndicator, state == VNTimelineEntryState.Completed);

            UnbindReplayButton();
            this.entryId = entryId;
            this.replayRequested = replayRequested;
            replayAvailable = state == VNTimelineEntryState.Completed &&
                              !string.IsNullOrWhiteSpace(entryId) &&
                              canReplay && replayRequested != null && replayButton != null;
            if (replayButton != null)
            {
                if (replayAvailable) replayButton.onClick.AddListener(HandleReplayClicked);
                SetActive(replayButton.gameObject, replayAvailable);
            }

            var safeDepth = Mathf.Max(0, depth);
            var width = safeDepth * Mathf.Max(0f, indentationPerLevel);
            if (indentationSpacer != null)
            {
                indentationSpacer.minWidth = width;
                indentationSpacer.preferredWidth = width;
                indentationSpacer.flexibleWidth = 0f;
            }

            DisableChildInteraction();
            return true;
        }

        public void Clear()
        {
            if (titleText != null) titleText.text = string.Empty;
            if (stateText != null) stateText.text = string.Empty;
            if (indentationSpacer != null)
            {
                indentationSpacer.minWidth = 0f;
                indentationSpacer.preferredWidth = 0f;
                indentationSpacer.flexibleWidth = 0f;
            }
            SetActive(completedIndicator, false);
            UnbindReplayButton();
            DisableChildInteraction();
        }

        private void DisableChildInteraction()
        {
            foreach (var selectable in GetComponentsInChildren<Selectable>(true))
                if (selectable != replayButton) selectable.interactable = false;
            if (replayButton != null)
            {
                replayButton.interactable = replayAvailable;
                SetActive(replayButton.gameObject, replayAvailable);
            }
        }

        private void OnDestroy() => UnbindReplayButton();

        private void HandleReplayClicked()
        {
            if (replayAvailable && !string.IsNullOrWhiteSpace(entryId)) replayRequested?.Invoke(entryId);
        }

        private void UnbindReplayButton()
        {
            if (replayButton != null)
            {
                replayButton.onClick.RemoveListener(HandleReplayClicked);
                replayButton.interactable = false;
                if (replayButton.gameObject.activeSelf) replayButton.gameObject.SetActive(false);
            }
            replayAvailable = false;
            entryId = null;
            replayRequested = null;
        }

        private bool IsOwnedByItem(Transform target) =>
            target != null && (target == transform || target.IsChildOf(transform));

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null) target.SetActive(active);
        }
    }
}
