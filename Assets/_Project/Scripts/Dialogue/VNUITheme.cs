using UnityEngine;

namespace ProjectAllTime.VN.Dialogue
{
    [CreateAssetMenu(menuName = "All Time/UI Theme")]
    public sealed class VNUITheme : ScriptableObject
    {
        public Color panel = Color.white;
        public Color text = new Color32(247, 244, 239, 255);
        public Color speaker = new Color32(229, 202, 162, 255);
        public Color accent = new Color32(129, 187, 183, 255);
        public Color normal = new Color32(219, 226, 226, 255);
        public Color hover = Color.white;
        public Color pressed = new Color32(129, 187, 183, 255);
        public Color disabled = new Color32(145, 154, 160, 180);
        public float dialogueSize = 32;
        public float speakerSize = 28;
        public float iconSize = 27;
        public float spacing = 6;
    }
}
