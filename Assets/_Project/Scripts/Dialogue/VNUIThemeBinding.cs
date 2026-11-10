using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Applies a small shared visual palette without owning UI actions.</summary>
    [ExecuteAlways]
    public sealed class VNUIThemeBinding : MonoBehaviour
    {
        [SerializeField] private VNUITheme theme;
        [SerializeField] private Image panel;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text speaker;
        [SerializeField] private TMP_Text dialogue;
        [SerializeField] private TMP_Text tooltip;
        [SerializeField] private Button button;
        public VNUITheme Theme => theme;
        private void OnEnable() => Apply();
        private void OnValidate() => Apply();
        public void Apply()
        {
            if (theme == null) return;
            if (panel != null) panel.color = theme.panel;
            if (icon != null) icon.color = theme.text;
            if (speaker != null) { speaker.color = theme.speaker; speaker.fontSize = theme.speakerSize; }
            if (dialogue != null) { dialogue.color = theme.text; dialogue.fontSize = theme.dialogueSize; }
            if (tooltip != null) tooltip.color = theme.text;
            if (button != null)
            {
                var colors = button.colors; colors.normalColor = theme.normal; colors.highlightedColor = theme.hover;
                colors.selectedColor = theme.hover; colors.pressedColor = theme.pressed; colors.disabledColor = theme.disabled;
                button.colors = colors;
            }
        }
    }
}
