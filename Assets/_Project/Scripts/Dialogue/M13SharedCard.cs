using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Content and availability presentation; selection and persistence belong to the owner.</summary>
    public sealed class M13SharedCard : MonoBehaviour
    {
        [SerializeField] private Image thumbnail;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text metadata;
        [SerializeField] private Selectable selectable;
        public void Configure(Image image, TMP_Text titleText, TMP_Text metadataText, Selectable control)
        { thumbnail = image; title = titleText; metadata = metadataText; selectable = control; }
        public void SetContent(string heading, string details, Sprite image)
        { title.text = heading; metadata.text = details; thumbnail.sprite = image; thumbnail.enabled = image != null; }
        public void SetLocked(bool locked) { selectable.interactable = !locked; }
    }
}
