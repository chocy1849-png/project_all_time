using System;
using ProjectAllTime.VN.Records.Archive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Archive.UI
{
    /// <summary>Renders one safe Archive entry row without consulting Archive authorities.</summary>
    [DisallowMultipleComponent]
    public sealed class VNArchiveEntryItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject unlockedIndicator;
        [SerializeField] private GameObject lockedIndicator;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private string lockedTitlePlaceholder = "Locked";

        private Action<string> onSelected;
        private string boundArchiveId;

        private void OnEnable() => RegisterClickListener();

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(HandleButtonClicked);
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (button == null) return Fail("Archive entry Button is missing.", out diagnostic);
            if (titleText == null) return Fail("Archive entry title TMP_Text is missing.", out diagnostic);
            if (unlockedIndicator == null) return Fail("Archive entry unlocked indicator is missing.", out diagnostic);
            if (lockedIndicator == null) return Fail("Archive entry locked indicator is missing.", out diagnostic);
            if (ReferenceEquals(unlockedIndicator, lockedIndicator))
                return Fail("Archive entry locked and unlocked indicators must be different objects.", out diagnostic);
            diagnostic = null;
            return true;
        }

        public bool Bind(
            VNArchiveEntryProjection projection, bool selected, Action<string> selectionCallback, out string diagnostic)
        {
            Unbind();
            if (!TryValidateWiring(out diagnostic)) return false;
            if (projection == null) return Fail("Archive entry projection is missing.", out diagnostic);
            if (string.IsNullOrEmpty(projection.ArchiveId))
                return Fail("Archive entry projection has no Archive ID.", out diagnostic);

            boundArchiveId = projection.ArchiveId;
            onSelected = selectionCallback;
            unlockedIndicator.SetActive(projection.IsUnlocked);
            lockedIndicator.SetActive(!projection.IsUnlocked);
            SetSelected(selected);

            var safeTitle = projection.DisplayTitle;
            if (!projection.IsUnlocked && safeTitle == null)
                safeTitle = string.IsNullOrWhiteSpace(lockedTitlePlaceholder) ? "Locked" : lockedTitlePlaceholder;
            titleText.text = safeTitle ?? string.Empty;
            titleText.gameObject.SetActive(safeTitle != null);
            button.interactable = selectionCallback != null;
            RegisterClickListener();
            diagnostic = null;
            return true;
        }

        public void Unbind()
        {
            onSelected = null;
            boundArchiveId = null;
            if (titleText != null)
            {
                titleText.text = string.Empty;
                titleText.gameObject.SetActive(false);
            }
            if (unlockedIndicator != null) unlockedIndicator.SetActive(false);
            if (lockedIndicator != null) lockedIndicator.SetActive(true);
            SetSelected(false);
            if (button != null) button.interactable = false;
        }

        private void SetSelected(bool selected)
        {
            if (selectedIndicator != null) selectedIndicator.SetActive(selected);
        }

        private void RegisterClickListener()
        {
            if (button == null) return;
            button.onClick.RemoveListener(HandleButtonClicked);
            button.onClick.AddListener(HandleButtonClicked);
        }

        private void HandleButtonClicked()
        {
            if (!string.IsNullOrEmpty(boundArchiveId)) onSelected?.Invoke(boundArchiveId);
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}