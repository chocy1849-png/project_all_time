using ProjectAllTime.VN.Records.Timeline;
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

            diagnostic = null;
            return true;
        }

        public bool Bind(string displayTitle, VNTimelineEntryState state, int depth)
        {
            if (state != VNTimelineEntryState.Discovered && state != VNTimelineEntryState.Completed)
            {
                Clear();
                return false;
            }

            if (titleText != null) titleText.text = displayTitle ?? string.Empty;
            if (stateText != null) stateText.text = state == VNTimelineEntryState.Completed ? "Completed" : "Discovered";
            SetActive(completedIndicator, state == VNTimelineEntryState.Completed);

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
            DisableChildInteraction();
        }

        private void DisableChildInteraction()
        {
            foreach (var selectable in GetComponentsInChildren<Selectable>(true))
                selectable.interactable = false;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null) target.SetActive(active);
        }
    }
}
