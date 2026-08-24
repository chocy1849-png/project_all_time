using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>Read-only, pooled CanvasGroup view of the session Backlog.</summary>
    [DisallowMultipleComponent]
    public sealed class VNBacklogModal : MonoBehaviour
    {
        [SerializeField] private CanvasGroup modalCanvasGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform content;
        [SerializeField] private VNBacklogItem itemPrefab;
        [SerializeField] private TMP_Text emptyStateText;
        [SerializeField] private VNDialogueSessionState sessionState;

        private readonly List<VNBacklogItem> itemPool = new();
        private bool isOpen;

        public bool IsOpen => isOpen;
        public int PooledItemCount => itemPool.Count;
        public event System.Action CloseRequested;

        private void Awake()
        {
            if (modalCanvasGroup != null) SetVisible(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public bool TryOpen()
        {
            if (!HasRequiredReferences()) return false;
            Bind(sessionState.Backlog.Entries);
            SetVisible(true);
            isOpen = true;
            ScrollToLatest();
            return true;
        }

        public bool Close()
        {
            if (!isOpen) return true;
            if (modalCanvasGroup == null) return false;
            SetVisible(false);
            isOpen = false;
            return true;
        }

        private void Bind(IReadOnlyList<VNBacklogEntry> entries)
        {
            var count = entries?.Count ?? 0;
            EnsurePool(count);
            for (var index = 0; index < itemPool.Count; index++)
            {
                var active = index < count;
                itemPool[index].gameObject.SetActive(active);
                if (active) itemPool[index].Bind(entries[index]);
            }

            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(count == 0);
            }
        }

        private void EnsurePool(int required)
        {
            while (itemPool.Count < required)
            {
                var item = Instantiate(itemPrefab, content);
                item.gameObject.SetActive(false);
                itemPool.Add(item);
            }
        }

        private void ScrollToLatest()
        {
            if (scrollRect == null) return;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private bool HasRequiredReferences()
        {
            if (modalCanvasGroup != null && content != null && itemPrefab != null && sessionState != null) return true;
            Debug.LogError($"{nameof(VNBacklogModal)} requires CanvasGroup, Content, Item Prefab, and session state references.", this);
            return false;
        }

        private void SetVisible(bool visible)
        {
            modalCanvasGroup.alpha = visible ? 1f : 0f;
            modalCanvasGroup.interactable = visible;
            modalCanvasGroup.blocksRaycasts = visible;
        }

        private void HandleCloseClicked() => CloseRequested?.Invoke();
    }
}
