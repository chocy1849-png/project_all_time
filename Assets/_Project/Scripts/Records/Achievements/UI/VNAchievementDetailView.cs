using ProjectAllTime.VN.Records.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Achievements.UI
{
    /// <summary>Displays only metadata supplied by the Achievement projection.</summary>
    [DisallowMultipleComponent]
    public sealed class VNAchievementDetailView : MonoBehaviour
    {
        [SerializeField] private GameObject detailRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject lockedStateRoot;
        [SerializeField] private GameObject unlockedStateRoot;
        [SerializeField] private GameObject secretLockedStateRoot;

        public bool TryValidateWiring(out string diagnostic)
        {
            if (detailRoot == null) return Fail("Achievement detail root is missing.", out diagnostic);
            if (titleText == null) return Fail("Achievement detail title TMP_Text is missing.", out diagnostic);
            if (descriptionText == null) return Fail("Achievement detail description TMP_Text is missing.", out diagnostic);
            if (lockedStateRoot == null) return Fail("Achievement locked detail state root is missing.", out diagnostic);
            if (unlockedStateRoot == null) return Fail("Achievement unlocked detail state root is missing.", out diagnostic);
            if (ReferenceEquals(detailRoot, lockedStateRoot) ||
                ReferenceEquals(detailRoot, unlockedStateRoot) ||
                ReferenceEquals(lockedStateRoot, unlockedStateRoot))
                return Fail("Achievement detail and locked/unlocked state roots must be different objects.", out diagnostic);
            if (secretLockedStateRoot != null &&
                (ReferenceEquals(detailRoot, secretLockedStateRoot) ||
                 ReferenceEquals(lockedStateRoot, secretLockedStateRoot) ||
                 ReferenceEquals(unlockedStateRoot, secretLockedStateRoot)))
                return Fail("Achievement secret locked state root must be separate from other detail roots.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Bind(VNAchievementProjection projection, out string diagnostic)
        {
            if (!TryValidateWiring(out diagnostic)) return false;
            if (projection == null) return Fail("Achievement detail projection is missing.", out diagnostic);

            ClearMetadata();
            detailRoot.SetActive(true);

            if (projection.IsUnlocked)
            {
                SetProjectedMetadata(projection);
                lockedStateRoot.SetActive(false);
                unlockedStateRoot.SetActive(true);
                if (secretLockedStateRoot != null) secretLockedStateRoot.SetActive(false);
            }
            else if (projection.DisplayTitle != null)
            {
                SetProjectedMetadata(projection);
                lockedStateRoot.SetActive(true);
                unlockedStateRoot.SetActive(false);
                if (secretLockedStateRoot != null) secretLockedStateRoot.SetActive(false);
            }
            else
            {
                lockedStateRoot.SetActive(secretLockedStateRoot == null);
                unlockedStateRoot.SetActive(false);
                if (secretLockedStateRoot != null) secretLockedStateRoot.SetActive(true);
            }

            diagnostic = null;
            return true;
        }

        public void Clear()
        {
            ClearMetadata();
            if (lockedStateRoot != null) lockedStateRoot.SetActive(false);
            if (unlockedStateRoot != null) unlockedStateRoot.SetActive(false);
            if (secretLockedStateRoot != null) secretLockedStateRoot.SetActive(false);
            if (detailRoot != null) detailRoot.SetActive(false);
        }

        private void SetProjectedMetadata(VNAchievementProjection projection)
        {
            titleText.text = projection.DisplayTitle ?? string.Empty;
            descriptionText.text = projection.Description ?? string.Empty;
            SetIcon(projection.OptionalIcon);
        }

        private void ClearMetadata()
        {
            if (titleText != null) titleText.text = string.Empty;
            if (descriptionText != null) descriptionText.text = string.Empty;
            SetIcon(null);
        }

        private void SetIcon(Sprite sprite)
        {
            if (iconImage == null) return;
            iconImage.sprite = sprite;
            iconImage.preserveAspect = true;
            iconImage.enabled = sprite != null;
            iconImage.gameObject.SetActive(sprite != null);
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}
