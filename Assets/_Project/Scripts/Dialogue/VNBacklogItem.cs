using TMPro;
using UnityEngine;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Reusable visual binding for one session Backlog entry.</summary>
    public sealed class VNBacklogItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private GameObject speakerContainer;

        public void Bind(VNBacklogEntry entry)
        {
            var narration = entry == null || entry.IsNarration;
            if (speakerNameText != null) speakerNameText.text = narration ? string.Empty : entry.SpeakerName;
            if (dialogueText != null) dialogueText.text = entry?.Text ?? string.Empty;
            if (speakerContainer != null) speakerContainer.SetActive(!narration);
        }
    }
}
