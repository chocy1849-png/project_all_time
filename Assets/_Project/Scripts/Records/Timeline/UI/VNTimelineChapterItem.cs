using System;
using ProjectAllTime.VN.Records.Timeline;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Timeline.UI
{
    /// <summary>One pooled, selectable chapter row in the Timeline tab.</summary>
    [DisallowMultipleComponent]
    public sealed class VNTimelineChapterItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject lockedIndicator;
        [SerializeField] private GameObject completedIndicator;

        private Action<string> selectionCallback;
        private string chapterId;
        private bool clickListenerRegistered;

        private void OnEnable() => RegisterClickListener();
        private void OnDisable() => UnregisterClickListener();

        public bool TryValidateWiring(out string diagnostic)
        {
            if (button == null)
            {
                diagnostic = "Timeline chapter item requires a Button.";
                return false;
            }
            if (titleText == null)
            {
                diagnostic = "Timeline chapter item requires a title TMP_Text.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        public void Bind(string id, string displayTitle, VNTimelineChapterState state, bool selected, Action<string> onSelected)
        {
            chapterId = id;
            selectionCallback = onSelected;
            if (titleText != null) titleText.text = displayTitle ?? string.Empty;
            if (stateText != null) stateText.text = FormatState(state);
            if (button != null) button.interactable = true;
            SetActive(selectedIndicator, selected);
            SetActive(lockedIndicator, state == VNTimelineChapterState.Locked);
            SetActive(completedIndicator, state == VNTimelineChapterState.Completed);
            RegisterClickListener();
        }

        public void Unbind()
        {
            chapterId = null;
            selectionCallback = null;
            if (button != null) button.interactable = false;
            if (titleText != null) titleText.text = string.Empty;
            if (stateText != null) stateText.text = string.Empty;
            SetActive(selectedIndicator, false);
            SetActive(lockedIndicator, false);
            SetActive(completedIndicator, false);
        }

        private void RegisterClickListener()
        {
            if (button == null || clickListenerRegistered) return;
            button.onClick.AddListener(HandleClicked);
            clickListenerRegistered = true;
        }

        private void UnregisterClickListener()
        {
            if (button == null || !clickListenerRegistered) return;
            button.onClick.RemoveListener(HandleClicked);
            clickListenerRegistered = false;
        }

        private void HandleClicked()
        {
            if (!string.IsNullOrEmpty(chapterId)) selectionCallback?.Invoke(chapterId);
        }

        private static string FormatState(VNTimelineChapterState state)
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

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null) target.SetActive(active);
        }
    }
}
