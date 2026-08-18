using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Presentation
{
    public sealed class VNCharacterSlotView : MonoBehaviour
    {
        [SerializeField] private VNCharacterSlot slot;
        [SerializeField] private RectTransform visualRoot;
        [SerializeField] private Image backHairImage;
        [SerializeField] private Image bodyImage;
        [SerializeField] private Image headImage;
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        public VNCharacterSlot Slot => slot;
        public Color Tint => bodyImage.color;
        public CanvasGroup FadeCanvasGroup => fadeCanvasGroup;
        public bool HasFadeCanvasGroup => fadeCanvasGroup != null;

        public bool IsConfigured => visualRoot != null && backHairImage != null && bodyImage != null && headImage != null;

        public void SetVisible(bool visible)
        {
            if (visible && fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;
            if (visualRoot != null) visualRoot.gameObject.SetActive(visible);
        }

        public void SetFadeAlpha(float alpha)
        {
            if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        public void ApplyCharacter(VNCharacterDefinition character, VNExpressionDefinition expression, VNCharacterRuntimeState state)
        {
            SetLayer(backHairImage, character.BackHairSprite);
            SetLayer(bodyImage, character.BodySprite);
            SetLayer(headImage, expression.HeadSprite);
            ApplyTransform(state);
            SetTint(state.SpeakerActive ? Color.white : new Color(0.65f, 0.65f, 0.65f, 1f));
            SetVisible(true);
        }

        public void ApplyTransform(VNCharacterRuntimeState state)
        {
            var scaleX = state.Facing == VNCharacterFacing.Left ? -state.Scale : state.Scale;
            visualRoot.localScale = new Vector3(scaleX, state.Scale, 1f);
        }

        public void SetTint(Color color)
        {
            backHairImage.color = color;
            bodyImage.color = color;
            headImage.color = color;
        }

        private static void SetLayer(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
        }
    }
}
