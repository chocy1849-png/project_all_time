using System;
using ProjectAllTime.VN.Records.Archive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Archive.UI
{
    /// <summary>Renders one projected Archive category without consulting Archive authorities.</summary>
    [DisallowMultipleComponent]
    public sealed class VNArchiveCategoryItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text completionCountText;
        [SerializeField] private GameObject selectedIndicator;

        private Action<string> onSelected;
        private string boundCategoryId;

        private void OnEnable() => RegisterClickListener();

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(HandleButtonClicked);
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (button == null) return Fail("Archive category Button is missing.", out diagnostic);
            if (titleText == null) return Fail("Archive category title TMP_Text is missing.", out diagnostic);
            diagnostic = null;
            return true;
        }

        public bool Bind(
            VNArchiveCategoryProjection projection, bool selected, Action<string> selectionCallback, out string diagnostic)
        {
            Unbind();
            if (!TryValidateWiring(out diagnostic)) return false;
            if (projection == null) return Fail("Archive category projection is missing.", out diagnostic);
            if (string.IsNullOrEmpty(projection.CategoryId))
                return Fail("Archive category projection has no category ID.", out diagnostic);
            if (projection.HasCompletionCount && (!projection.UnlockedCount.HasValue || !projection.TotalCount.HasValue))
                return Fail("Archive category completion count is enabled but a projected count is missing.", out diagnostic);

            boundCategoryId = projection.CategoryId;
            onSelected = selectionCallback;
            titleText.text = projection.DisplayTitle ?? string.Empty;
            SetSelected(selected);
            SetCompletionCount(projection);
            button.interactable = selectionCallback != null;
            RegisterClickListener();
            diagnostic = null;
            return true;
        }

        public void Unbind()
        {
            onSelected = null;
            boundCategoryId = null;
            if (titleText != null) titleText.text = string.Empty;
            if (completionCountText != null)
            {
                completionCountText.text = string.Empty;
                completionCountText.gameObject.SetActive(false);
            }
            SetSelected(false);
            if (button != null) button.interactable = false;
        }

        private void SetCompletionCount(VNArchiveCategoryProjection projection)
        {
            if (completionCountText == null) return;
            if (!projection.HasCompletionCount)
            {
                completionCountText.text = string.Empty;
                completionCountText.gameObject.SetActive(false);
                return;
            }

            completionCountText.text = projection.UnlockedCount.Value + " / " + projection.TotalCount.Value;
            completionCountText.gameObject.SetActive(true);
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
            if (!string.IsNullOrEmpty(boundCategoryId)) onSelected?.Invoke(boundCategoryId);
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}