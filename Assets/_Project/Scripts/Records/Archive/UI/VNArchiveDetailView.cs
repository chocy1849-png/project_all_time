using ProjectAllTime.VN.Records.Archive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Archive.UI
{
    /// <summary>Displays only projected unlocked Archive content or a generic locked state.</summary>
    [DisallowMultipleComponent]
    public sealed class VNArchiveDetailView : MonoBehaviour
    {
        [SerializeField] private GameObject detailRoot;
        [SerializeField] private GameObject lockedStateRoot;
        [SerializeField] private GameObject unlockedContentRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image optionalImage;

        public bool TryValidateWiring(out string diagnostic)
        {
            if (detailRoot == null) return Fail("Archive detail root is missing.", out diagnostic);
            if (lockedStateRoot == null) return Fail("Archive locked detail state root is missing.", out diagnostic);
            if (unlockedContentRoot == null) return Fail("Archive unlocked detail content root is missing.", out diagnostic);
            if (ReferenceEquals(detailRoot, lockedStateRoot) ||
                ReferenceEquals(detailRoot, unlockedContentRoot) ||
                ReferenceEquals(lockedStateRoot, unlockedContentRoot))
                return Fail("Archive detail root and state roots must be different objects.", out diagnostic);
            if (titleText == null) return Fail("Archive detail title TMP_Text is missing.", out diagnostic);
            if (summaryText == null) return Fail("Archive detail summary TMP_Text is missing.", out diagnostic);
            if (bodyText == null) return Fail("Archive detail body TMP_Text is missing.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Bind(VNArchiveEntryProjection projection, out string diagnostic)
        {
            if (!TryValidateWiring(out diagnostic)) return false;
            if (projection == null) return Fail("Archive detail projection is missing.", out diagnostic);

            ClearUnlockedContent();
            detailRoot.SetActive(true);
            if (!projection.IsUnlocked)
            {
                lockedStateRoot.SetActive(true);
                unlockedContentRoot.SetActive(false);
                diagnostic = null;
                return true;
            }

            titleText.text = projection.DisplayTitle ?? string.Empty;
            summaryText.text = projection.Summary ?? string.Empty;
            bodyText.text = projection.Body ?? string.Empty;
            if (optionalImage != null)
            {
                optionalImage.sprite = projection.OptionalImage;
                optionalImage.preserveAspect = true;
                optionalImage.enabled = projection.OptionalImage != null;
                optionalImage.gameObject.SetActive(projection.OptionalImage != null);
            }

            lockedStateRoot.SetActive(false);
            unlockedContentRoot.SetActive(true);
            diagnostic = null;
            return true;
        }

        public void Clear()
        {
            ClearUnlockedContent();
            if (lockedStateRoot != null) lockedStateRoot.SetActive(false);
            if (unlockedContentRoot != null) unlockedContentRoot.SetActive(false);
            if (detailRoot != null) detailRoot.SetActive(false);
        }

        private void ClearUnlockedContent()
        {
            if (titleText != null) titleText.text = string.Empty;
            if (summaryText != null) summaryText.text = string.Empty;
            if (bodyText != null) bodyText.text = string.Empty;
            if (optionalImage != null)
            {
                optionalImage.sprite = null;
                optionalImage.enabled = false;
                optionalImage.preserveAspect = true;
                optionalImage.gameObject.SetActive(false);
            }
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}