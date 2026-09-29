using System;
using System.Collections.Generic;
using ProjectAllTime.VN.Records.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Records.Achievements.UI
{
    /// <summary>Renders the persisted Achievement projections supplied by VNAchievementService.</summary>
    [DisallowMultipleComponent]
    public sealed class VNAchievementsView : MonoBehaviour
    {
        [SerializeField] private ScrollRect achievementScrollRect;
        [SerializeField] private Transform itemContent;
        [SerializeField] private VNAchievementItem itemPrefab;
        [SerializeField] private VNAchievementDetailView detailView;
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private GameObject contentStateRoot;
        [SerializeField] private GameObject errorStateRoot;
        [SerializeField] private TMP_Text errorText;

        private readonly List<VNAchievementItem> items = new();
        private VNAchievementService achievementService;
        private IReadOnlyList<VNAchievementProjection> currentAchievements;
        private string selectedAchievementId;
        private bool initialized;

        public string SelectedAchievementId => selectedAchievementId;
        public string LastDiagnostic { get; private set; }
        public int PoolCapacity => items.Count;

        private void Awake()
        {
            SetStateRoots(false, false, false, null);
            if (detailView != null) detailView.Clear();
        }

        private void OnEnable()
        {
            if (initialized) Refresh();
        }

        private void OnDisable()
        {
            if (detailView != null) detailView.Clear();
        }

        public bool Initialize(VNAchievementService service)
        {
            if (service == null)
            {
                LastDiagnostic = "A VNAchievementService is required.";
                return false;
            }

            if (initialized)
            {
                if (!ReferenceEquals(achievementService, service))
                {
                    LastDiagnostic = "VNAchievementsView is already initialized with a different service.";
                    return false;
                }

                LastDiagnostic = null;
                return !isActiveAndEnabled || Refresh();
            }

            achievementService = service;
            initialized = true;
            LastDiagnostic = null;
            return !isActiveAndEnabled || Refresh();
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (achievementScrollRect == null)
                return Fail("Achievement ScrollRect is missing.", out diagnostic);
            if (itemContent == null)
                return Fail("Achievement item content Transform is missing.", out diagnostic);
            if (itemContent is not RectTransform contentRect)
                return Fail("Achievement item content must have a RectTransform.", out diagnostic);
            if (achievementScrollRect.content != contentRect)
                return Fail("Achievement ScrollRect content must reference the item content RectTransform.", out diagnostic);
            if (itemPrefab == null)
                return Fail("Achievement item prefab is missing.", out diagnostic);
            if (!itemPrefab.TryValidateWiring(out var itemDiagnostic))
                return Fail("Achievement item prefab: " + itemDiagnostic, out diagnostic);
            if (detailView == null)
                return Fail("Achievement detail view is missing.", out diagnostic);
            if (!detailView.TryValidateWiring(out var detailDiagnostic))
                return Fail("Achievement detail view: " + detailDiagnostic, out diagnostic);
            if (emptyStateRoot == null)
                return Fail("Achievement empty state root is missing.", out diagnostic);
            if (contentStateRoot == null)
                return Fail("Achievement content state root is missing.", out diagnostic);
            if (ReferenceEquals(emptyStateRoot, contentStateRoot))
                return Fail("Achievement empty and content state roots must be different objects.", out diagnostic);
            if (errorStateRoot != null &&
                (ReferenceEquals(errorStateRoot, emptyStateRoot) ||
                 ReferenceEquals(errorStateRoot, contentStateRoot)))
                return Fail("Achievement error state root must be separate from empty and content roots.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Refresh()
        {
            if (!initialized || achievementService == null)
                return FailAndClear("Achievement service has not been initialized.");
            if (!TryValidateWiring(out var wiringDiagnostic))
                return FailAndClear(wiringDiagnostic);

            IReadOnlyList<VNAchievementProjection> achievements;
            try
            {
                achievements = achievementService.GetAchievements();
            }
            catch (Exception exception)
            {
                return FailAndClear("Achievement projections could not be queried: " + exception.Message);
            }

            if (achievements == null)
                return FailAndClear("Achievement service returned no projection list.");
            currentAchievements = achievements;
            if (achievements.Count == 0)
            {
                selectedAchievementId = null;
                DeactivateAllItems();
                detailView.Clear();
                SetStateRoots(true, false, false, null);
                ResetScrollToTop();
                LastDiagnostic = null;
                return true;
            }

            var selected = FindAchievement(achievements, selectedAchievementId) ?? ChooseInitialSelection(achievements);
            if (selected == null || string.IsNullOrEmpty(selected.AchievementId))
                return FailAndClear("Achievement service returned no selectable projection.");
            selectedAchievementId = selected.AchievementId;

            if (!BindItems(achievements, selectedAchievementId))
                return FailAndClear(LastDiagnostic ?? "Achievement rows could not be rendered.");
            if (!detailView.Bind(selected, out var detailDiagnostic))
                return FailAndClear("Achievement detail could not be rendered: " + detailDiagnostic);

            SetStateRoots(false, true, false, null);
            ResetScrollToTop();
            LastDiagnostic = null;
            return true;
        }

        public bool TrySelectAchievement(string achievementId)
        {
            if (!initialized || currentAchievements == null || string.IsNullOrWhiteSpace(achievementId))
                return false;
            if (!TryValidateWiring(out var wiringDiagnostic))
                return FailAndClear(wiringDiagnostic);
            var selected = FindAchievement(currentAchievements, achievementId);
            if (selected == null) return false;

            selectedAchievementId = selected.AchievementId;
            if (!BindItems(currentAchievements, selectedAchievementId))
                return FailAndClear(LastDiagnostic ?? "Achievement rows could not be rendered.");
            if (!detailView.Bind(selected, out var detailDiagnostic))
                return FailAndClear("Achievement detail could not be rendered: " + detailDiagnostic);

            SetStateRoots(false, true, false, null);
            LastDiagnostic = null;
            return true;
        }

        private bool BindItems(IReadOnlyList<VNAchievementProjection> achievements, string selectedId)
        {
            if (!EnsurePool(achievements.Count))
                return Fail(LastDiagnostic ?? "Achievement row pool could not be created.");

            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];
                if (index >= achievements.Count)
                {
                    item.Unbind();
                    if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
                    continue;
                }

                var projection = achievements[index];
                if (projection == null)
                    return Fail("Achievement service returned a null projection.");
                if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
                if (!item.Bind(projection,
                        string.Equals(projection.AchievementId, selectedId, StringComparison.Ordinal),
                        HandleAchievementSelected,
                        out var itemDiagnostic))
                    return Fail("Achievement row could not be bound: " + itemDiagnostic);
            }
            return true;
        }

        private bool EnsurePool(int requiredCount)
        {
            try
            {
                while (items.Count < requiredCount)
                {
                    var item = Instantiate(itemPrefab, itemContent, false);
                    if (item == null) return false;
                    item.gameObject.SetActive(false);
                    items.Add(item);
                }
                return true;
            }
            catch (Exception exception)
            {
                LastDiagnostic = "Achievement row pool failed: " + exception.Message;
                return false;
            }
        }
        private VNAchievementProjection ChooseInitialSelection(IReadOnlyList<VNAchievementProjection> achievements)
        {
            for (var index = 0; index < achievements.Count; index++)
                if (achievements[index] != null && achievements[index].IsUnlocked)
                    return achievements[index];

            for (var index = 0; index < achievements.Count; index++)
                if (achievements[index] != null && achievements[index].DisplayTitle != null)
                    return achievements[index];

            return achievements.Count > 0 ? achievements[0] : null;
        }

        private void HandleAchievementSelected(string id) => TrySelectAchievement(id);

        private bool FailAndClear(string diagnostic)
        {
            DeactivateAllItems();
            currentAchievements = null;
            if (detailView != null) detailView.Clear();
            SetStateRoots(errorStateRoot == null, false, errorStateRoot != null, diagnostic);
            ResetScrollToTop();
            LastDiagnostic = diagnostic;
            return false;
        }

        private void DeactivateAllItems()
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                item.Unbind();
                if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
            }
        }

        private void ResetScrollToTop()
        {
            if (itemContent is not RectTransform contentRect ||
                achievementScrollRect == null ||
                achievementScrollRect.content != contentRect) return;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            achievementScrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
        }

        private void SetStateRoots(bool empty, bool content, bool error, string diagnostic)
        {
            if (emptyStateRoot != null) emptyStateRoot.SetActive(empty);
            if (contentStateRoot != null) contentStateRoot.SetActive(content);
            if (errorStateRoot != null) errorStateRoot.SetActive(error);
            if (errorText != null) errorText.text = diagnostic ?? string.Empty;
        }

        private static VNAchievementProjection FindAchievement(
            IReadOnlyList<VNAchievementProjection> achievements, string id)
        {
            if (achievements == null || string.IsNullOrEmpty(id)) return null;
            for (var index = 0; index < achievements.Count; index++)
            {
                var achievement = achievements[index];
                if (achievement != null &&
                    string.Equals(achievement.AchievementId, id, StringComparison.Ordinal))
                    return achievement;
            }
            return null;
        }

        private bool Fail(string message)
        {
            LastDiagnostic = message;
            return false;
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}
