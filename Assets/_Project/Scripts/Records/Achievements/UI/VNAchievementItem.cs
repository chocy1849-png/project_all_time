using System;
using ProjectAllTime.VN.Records.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Achievements.UI
{
    /// <summary>Renders one Achievement projection without consulting its catalog or unlock authority.</summary>
    [DisallowMultipleComponent]
    public sealed class VNAchievementItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject lockedIndicator;
        [SerializeField] private GameObject unlockedIndicator;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject secretLockedIndicator;
        [SerializeField] private string secretLockedPlaceholder = "???";

        private Action<string> onSelected;
        private string boundAchievementId;

        private void OnEnable() => RegisterClickListener();

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(HandleButtonClicked);
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (button == null) return Fail("Achievement item Button is missing.", out diagnostic);
            if (titleText == null) return Fail("Achievement item title TMP_Text is missing.", out diagnostic);
            if (lockedIndicator == null) return Fail("Achievement item locked indicator is missing.", out diagnostic);
            if (unlockedIndicator == null) return Fail("Achievement item unlocked indicator is missing.", out diagnostic);
            if (ReferenceEquals(lockedIndicator, unlockedIndicator))
                return Fail("Achievement item locked and unlocked indicators must be different objects.", out diagnostic);
            if (secretLockedIndicator != null &&
                (ReferenceEquals(secretLockedIndicator, lockedIndicator) ||
                 ReferenceEquals(secretLockedIndicator, unlockedIndicator)))
                return Fail("Achievement item secret indicator must be separate from locked and unlocked indicators.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Bind(
            VNAchievementProjection projection,
            bool selected,
            Action<string> selectionCallback,
            out string diagnostic)
        {
            Unbind();
            if (!TryValidateWiring(out diagnostic)) return false;
            if (projection == null) return Fail("Achievement item projection is missing.", out diagnostic);
            if (string.IsNullOrEmpty(projection.AchievementId))
                return Fail("Achievement projection has no internal ID.", out diagnostic);

            boundAchievementId = projection.AchievementId;
            onSelected = selectionCallback;
            lockedIndicator.SetActive(!projection.IsUnlocked);
            unlockedIndicator.SetActive(projection.IsUnlocked);
            if (selectedIndicator != null) selectedIndicator.SetActive(selected);

            var isSecretLocked = !projection.IsUnlocked && projection.DisplayTitle == null;
            if (secretLockedIndicator != null) secretLockedIndicator.SetActive(isSecretLocked);

            var title = projection.DisplayTitle;
            if (isSecretLocked)
                title = string.IsNullOrWhiteSpace(secretLockedPlaceholder) ? "???" : secretLockedPlaceholder;
            titleText.text = title ?? string.Empty;
            titleText.gameObject.SetActive(title != null);

            var projectedIcon = isSecretLocked ? null : projection.OptionalIcon;
            SetIcon(projectedIcon);
            button.interactable = selectionCallback != null;
            RegisterClickListener();
            diagnostic = null;
            return true;
        }

        public void Unbind()
        {
            onSelected = null;
            boundAchievementId = null;
            if (titleText != null)
            {
                titleText.text = string.Empty;
                titleText.gameObject.SetActive(false);
            }
            SetIcon(null);
            if (lockedIndicator != null) lockedIndicator.SetActive(true);
            if (unlockedIndicator != null) unlockedIndicator.SetActive(false);
            if (selectedIndicator != null) selectedIndicator.SetActive(false);
            if (secretLockedIndicator != null) secretLockedIndicator.SetActive(false);
            if (button != null) button.interactable = false;
        }

        private void SetIcon(Sprite sprite)
        {
            if (iconImage == null) return;
            iconImage.sprite = sprite;
            iconImage.preserveAspect = true;
            iconImage.enabled = sprite != null;
            iconImage.gameObject.SetActive(sprite != null);
        }

        private void RegisterClickListener()
        {
            if (button == null) return;
            button.onClick.RemoveListener(HandleButtonClicked);
            button.onClick.AddListener(HandleButtonClicked);
        }

        private void HandleButtonClicked()
        {
            if (!string.IsNullOrEmpty(boundAchievementId))
                onSelected?.Invoke(boundAchievementId);
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}
