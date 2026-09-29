using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Achievements.UI;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNAchievementsViewTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private readonly List<VNAchievementEvaluator> evaluators = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private int writes;

        [SetUp]
        public void SetUp()
        {
            writes = 0;
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M912AchievementsUiTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var evaluator in evaluators) evaluator?.Dispose();
            evaluators.Clear();
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                if (ownedObjects[index] != null) UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void EnableBeforeInjectionIsSafe_AndActiveInitializationRefreshesImmediately()
        {
            var rig = CreateRig(false);
            Invoke(rig.View, "OnEnable");
            Assert.That(rig.View.LastDiagnostic, Is.Null);

            rig.Root.SetActive(true);
            Invoke(rig.View, "OnEnable");
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_lifecycle", "Lifecycle", 0));
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(1));
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_lifecycle"));
            Assert.That(rig.ContentState.activeSelf, Is.True);
        }

        [Test]
        public void InitializeBeforeActivationRefreshesOnEnable()
        {
            var rig = CreateRig(false);
            var catalog = CreateAchievementCatalog(Achievement("achievement_late", "Late", 0));
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            Assert.That(ActiveItems(rig), Is.Empty);

            rig.Root.SetActive(true);
            Invoke(rig.View, "OnEnable");
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(1));
            Assert.That(rig.DetailRoot.activeSelf, Is.True);
        }

        [Test]
        public void InitializeIsIdempotentForSameServiceAndRejectsNullOrDifferentService()
        {
            var rig = CreateRig();
            var service = CreateService(CreateAchievementCatalog(Achievement("achievement_one", "One", 0)));
            Assert.That(rig.View.Initialize(null), Is.False);
            Assert.That(rig.View.LastDiagnostic, Does.Contain("VNAchievementService"));
            Assert.That(rig.View.Initialize(service), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.Initialize(service), Is.True, rig.View.LastDiagnostic);

            var other = CreateService(CreateAchievementCatalog());
            Assert.That(rig.View.Initialize(other), Is.False);
            Assert.That(rig.View.LastDiagnostic, Does.Contain("different service"));
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(1));
        }

        [Test]
        public void EmptyCatalogShowsEmptyStateClearsDetailAndDoesNotWrite()
        {
            var rig = CreateRig();
            var writeBaseline = writes;
            Assert.That(rig.View.Initialize(CreateService(CreateAchievementCatalog())), Is.True, rig.View.LastDiagnostic);

            Assert.That(rig.EmptyState.activeSelf, Is.True);
            Assert.That(rig.ContentState.activeSelf, Is.False);
            Assert.That(ActiveItems(rig), Is.Empty);
            Assert.That(rig.DetailRoot.activeSelf, Is.False);
            Assert.That(rig.View.SelectedAchievementId, Is.Null);
            Assert.That(rig.View.LastDiagnostic, Is.Null);
            Assert.That(writes, Is.EqualTo(writeBaseline));
        }

        [Test]
        public void InitialSelectionPrefersFirstUnlockedThenVisibleLockedThenFirstSecret()
        {
            SeedAchievement("achievement_c_unlocked");
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_a_secret", "Secret A", 0, hidden: true),
                Achievement("achievement_b_visible", "Visible B", 1),
                Achievement("achievement_c_unlocked", "Unlocked C", 2, hidden: true));
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_c_unlocked"));
            Assert.That(rig.DetailUnlocked.activeSelf, Is.True);

            var visibleRig = CreateRig();
            var visibleCatalog = CreateAchievementCatalog(
                Achievement("achievement_a_secret_only", "Secret A", 0, hidden: true),
                Achievement("achievement_b_visible_only", "Visible B", 1));
            Assert.That(visibleRig.View.Initialize(CreateService(visibleCatalog)), Is.True);
            Assert.That(visibleRig.View.SelectedAchievementId, Is.EqualTo("achievement_b_visible_only"));
            Assert.That(visibleRig.DetailLocked.activeSelf, Is.True);
            Assert.That(visibleRig.Title.text, Is.EqualTo("Visible B"));

            var secretRig = CreateRig();
            var secretCatalog = CreateAchievementCatalog(
                Achievement("achievement_a_secret_first", "Secret A", 0, hidden: true),
                Achievement("achievement_b_secret_second", "Secret B", 1, hidden: true));
            Assert.That(secretRig.View.Initialize(CreateService(secretCatalog)), Is.True);
            Assert.That(secretRig.View.SelectedAchievementId, Is.EqualTo("achievement_a_secret_first"));
            Assert.That(secretRig.DetailSecret.activeSelf, Is.True);
            Assert.That(secretRig.Title.text, Is.Empty);
        }

        [Test]
        public void VisibleLockedProjectionRendersItsTitleDescriptionAndAllowedIcon()
        {
            var icon = CreateSprite();
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_visible", "Visible achievement", 0, description: "Visible description",
                    hidden: false, icon: icon));
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);

            var row = ActiveItems(rig).Single();
            Assert.That(GetField<GameObject>(row, "lockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(row, "unlockedIndicator").activeSelf, Is.False);
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Is.EqualTo("Visible achievement"));
            Assert.That(GetField<Image>(row, "iconImage").sprite, Is.SameAs(icon));
            Assert.That(GetField<Image>(row, "iconImage").enabled, Is.True);
            Assert.That(GetField<GameObject>(row, "secretLockedIndicator").activeSelf, Is.False);
            Assert.That(GetField<Button>(row, "button").interactable, Is.True);

            GetField<Button>(row, "button").onClick.Invoke();
            Assert.That(rig.DetailLocked.activeSelf, Is.True);
            Assert.That(rig.DetailUnlocked.activeSelf, Is.False);
            Assert.That(rig.DetailSecret.activeSelf, Is.False);
            Assert.That(rig.Title.text, Is.EqualTo("Visible achievement"));
            Assert.That(rig.Description.text, Is.EqualTo("Visible description"));
            Assert.That(rig.DetailIcon.sprite, Is.SameAs(icon));
            Assert.That(rig.DetailIcon.enabled, Is.True);
        }

        [Test]
        public void SecretLockedProjectionUsesGenericPlaceholderAndNoMetadata()
        {
            var icon = CreateSprite();
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_internal_secret", "AUTHORED SECRET TITLE", 0,
                    description: "AUTHORED SECRET DESCRIPTION", hidden: true, icon: icon));
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);

            var row = ActiveItems(rig).Single();
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Is.EqualTo("???"));
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Does.Not.Contain("achievement_internal_secret"));
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Does.Not.Contain("AUTHORED"));
            Assert.That(GetField<GameObject>(row, "lockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(row, "unlockedIndicator").activeSelf, Is.False);
            Assert.That(GetField<GameObject>(row, "secretLockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<Image>(row, "iconImage").sprite, Is.Null);
            Assert.That(GetField<Image>(row, "iconImage").enabled, Is.False);

            GetField<Button>(row, "button").onClick.Invoke();
            Assert.That(rig.DetailSecret.activeSelf, Is.True);
            Assert.That(rig.DetailLocked.activeSelf, Is.False);
            Assert.That(rig.Title.text, Is.Empty);
            Assert.That(rig.Description.text, Is.Empty);
            Assert.That(rig.DetailIcon.sprite, Is.Null);
            Assert.That(rig.DetailIcon.enabled, Is.False);
            Assert.That(VisibleText(rig), Does.Not.Contain("AUTHORED SECRET"));
            Assert.That(VisibleText(rig), Does.Not.Contain("achievement_internal_secret"));
        }
        [Test]
        public void UnlockingHiddenManualAchievementRefreshesTitleDescriptionAndIcon()
        {
            var icon = CreateSprite();
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_hidden_manual", "Hidden Manual", 0, description: "Revealed detail",
                    hidden: true, type: VNAchievementConditionType.Manual, icon: icon));
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            Assert.That(GetField<TMP_Text>(ActiveItems(rig).Single(), "titleText").text, Is.EqualTo("???"));
            Assert.That(rig.DetailSecret.activeSelf, Is.True);
            Assert.That(rig.Title.text, Is.Empty);
            Assert.That(rig.Description.text, Is.Empty);
            Assert.That(rig.DetailIcon.sprite, Is.Null);

            var writesBeforeUnlock = writes;
            SeedAchievement("achievement_hidden_manual");
            var writesAfterExternalUnlock = writes;
            Assert.That(writesAfterExternalUnlock, Is.GreaterThan(writesBeforeUnlock));
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);

            var row = ActiveItems(rig).Single();
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Is.EqualTo("Hidden Manual"));
            Assert.That(GetField<GameObject>(row, "lockedIndicator").activeSelf, Is.False);
            Assert.That(GetField<GameObject>(row, "unlockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(row, "secretLockedIndicator").activeSelf, Is.False);
            Assert.That(GetField<Image>(row, "iconImage").sprite, Is.SameAs(icon));
            Assert.That(rig.DetailUnlocked.activeSelf, Is.True);
            Assert.That(rig.DetailSecret.activeSelf, Is.False);
            Assert.That(rig.Title.text, Is.EqualTo("Hidden Manual"));
            Assert.That(rig.Description.text, Is.EqualTo("Revealed detail"));
            Assert.That(rig.DetailIcon.sprite, Is.SameAs(icon));
            Assert.That(writes, Is.EqualTo(writesAfterExternalUnlock), "Refreshing UI must not write progress.");
        }

        [Test]
        public void ManualAchievementIntegrationRemainsExternallyUnlockedAndUiOnlyObservesIt()
        {
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_manual_external", "Manual Award", 0,
                    type: VNAchievementConditionType.Manual));
            var evaluator = CreateEvaluator(catalog);
            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);
            Assert.That(metaProgress.IsAchievementUnlocked("achievement_manual_external"), Is.False);

            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            Assert.That(rig.DetailLocked.activeSelf, Is.True);
            Assert.That(rig.Title.text, Is.EqualTo("Manual Award"));

            SeedAchievement("achievement_manual_external");
            var writeBaseline = writes;
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(metaProgress.IsAchievementUnlocked("achievement_manual_external"), Is.True);
            Assert.That(rig.DetailUnlocked.activeSelf, Is.True);
            Assert.That(rig.View.LastDiagnostic, Is.Null);
            Assert.That(writes, Is.EqualTo(writeBaseline));
        }

        [Test]
        public void DerivedAchievementFlowsThroughEvaluatorPersistenceProjectionAndView()
        {
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_derived_cg", "CG Collector", 0,
                    description: "Persisted derived unlock", hidden: false,
                    type: VNAchievementConditionType.CGCountAtLeast, threshold: 1));
            var evaluator = CreateEvaluator(catalog, "cg_evaluator_input");
            Assert.That(evaluator.Initialize(), Is.True, evaluator.LastDiagnostic);

            var service = CreateService(catalog);
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(service), Is.True, rig.View.LastDiagnostic);
            Assert.That(metaProgress.IsAchievementUnlocked("achievement_derived_cg"), Is.False);
            Assert.That(rig.DetailLocked.activeSelf, Is.True);
            Assert.That(rig.Title.text, Is.EqualTo("CG Collector"));

            SeedCG("cg_evaluator_input");
            Assert.That(metaProgress.IsAchievementUnlocked("achievement_derived_cg"), Is.True,
                "Evaluator's progress subscription should persist the derived unlock.");
            var writeBaseline = writes;
            var bytesBeforeUiRefresh = ReadPersistedBytes();
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);

            var row = ActiveItems(rig).Single();
            Assert.That(GetField<GameObject>(row, "unlockedIndicator").activeSelf, Is.True);
            Assert.That(rig.DetailUnlocked.activeSelf, Is.True);
            Assert.That(rig.Description.text, Is.EqualTo("Persisted derived unlock"));
            Assert.That(writes, Is.EqualTo(writeBaseline));
            CollectionAssert.AreEqual(bytesBeforeUiRefresh, ReadPersistedBytes());
        }

        [Test]
        public void RefreshPreservesSelectedIdAndFallsBackInServiceOrderWhenItDisappears()
        {
            var first = Achievement("achievement_a", "Z title", 0);
            var second = Achievement("achievement_z", "A title", 0);
            var catalog = CreateAchievementCatalog(second, first);
            var service = CreateService(catalog);
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(service), Is.True, rig.View.LastDiagnostic);

            CollectionAssert.AreEqual(new[] { "Z title", "A title" },
                ActiveItems(rig).Select(item => GetField<TMP_Text>(item, "titleText").text));
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_a"));

            Assert.That(rig.View.TrySelectAchievement("achievement_z"), Is.True);
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_z"));

            SetList(catalog, "achievements", new[] { second });
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_z"));
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(1));

            SetList(catalog, "achievements", new[] { first });
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_a"));
            Assert.That(GetField<TMP_Text>(ActiveItems(rig).Single(), "titleText").text, Is.EqualTo("Z title"));
        }

        [Test]
        public void PoolReusesTenFourAndSevenRows()
        {
            var definitions = Enumerable.Range(0, 10)
                .Select(index => Achievement($"achievement_{index:00}", $"Award {index}", index))
                .ToArray();
            var catalog = CreateAchievementCatalog(definitions);
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.PoolCapacity, Is.GreaterThanOrEqualTo(10));
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(10));

            SetList(catalog, "achievements", definitions.Take(4));
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(4));
            Assert.That(rig.View.PoolCapacity, Is.GreaterThanOrEqualTo(10));

            SetList(catalog, "achievements", definitions.Take(7));
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(7));
            Assert.That(rig.View.PoolCapacity, Is.GreaterThanOrEqualTo(10));
        }

        [Test]
        public void RebindingUnlockedPoolRowAsSecretClearsIconTitleDetailAndOldCallback()
        {
            var icon = CreateSprite();
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_old_unlocked", "Formerly unlocked", 0,
                    description: "Old secret text", icon: icon));
            SeedAchievement("achievement_old_unlocked");
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            var row = ActiveItems(rig).Single();
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Is.EqualTo("Formerly unlocked"));
            Assert.That(GetField<Image>(row, "iconImage").sprite, Is.SameAs(icon));

            SetList(catalog, "achievements", new[]
            {
                Achievement("achievement_new_secret", "Secret authored title", 0,
                    description: "Secret authored description", hidden: true, icon: icon)
            });
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);

            row = ActiveItems(rig).Single();
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Is.EqualTo("???"));
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Does.Not.Contain("Formerly unlocked"));
            Assert.That(GetField<GameObject>(row, "lockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(row, "unlockedIndicator").activeSelf, Is.False);
            Assert.That(GetField<GameObject>(row, "secretLockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<Image>(row, "iconImage").sprite, Is.Null);
            Assert.That(GetField<Image>(row, "iconImage").enabled, Is.False);
            Assert.That(rig.DetailSecret.activeSelf, Is.True);
            Assert.That(rig.Title.text, Is.Empty);
            Assert.That(rig.Description.text, Is.Empty);
            Assert.That(rig.DetailIcon.sprite, Is.Null);
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_new_secret"));

            GetField<Button>(row, "button").onClick.Invoke();
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_new_secret"));
            Assert.That(VisibleText(rig), Does.Not.Contain("Secret authored"));
        }
        [Test]
        public void RepeatedItemBindingAndRefreshKeepOneSelectionCallback()
        {
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_callback", "Callback", 0));
            var service = CreateService(catalog);
            var projection = service.GetAchievements().Single();
            var rig = CreateRig();
            var item = rig.ItemPrefab;
            var callbackCount = 0;
            var replacementCallbackCount = 0;

            for (var index = 0; index < 10; index++)
                Assert.That(item.Bind(projection, false, _ => callbackCount++, out var diagnostic), Is.True, diagnostic);
            GetField<Button>(item, "button").onClick.Invoke();
            Assert.That(callbackCount, Is.EqualTo(1));

            Assert.That(item.Bind(projection, false, _ => replacementCallbackCount++, out var replacementDiagnostic),
                Is.True, replacementDiagnostic);
            GetField<Button>(item, "button").onClick.Invoke();
            Assert.That(callbackCount, Is.EqualTo(1), "A replaced selection callback must no longer be invoked.");
            Assert.That(replacementCallbackCount, Is.EqualTo(1));

            Assert.That(rig.View.Initialize(service), Is.True);
            for (var index = 0; index < 10; index++)
                Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            GetField<Button>(ActiveItems(rig).Single(), "button").onClick.Invoke();
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_callback"));
        }

        [Test]
        public void ViewWiringValidatesScrollContentStateRootsAndOptionalErrorUi()
        {
            var rig = CreateRig();
            SetField(rig.View, "errorStateRoot", null);
            SetField(rig.View, "errorText", null);
            Assert.That(rig.View.TryValidateWiring(out var validDiagnostic), Is.True, validDiagnostic);

            SetField(rig.View, "itemContent", null);
            Assert.That(rig.View.TryValidateWiring(out var missingContent), Is.False);
            Assert.That(missingContent, Does.Contain("item content"));

            var badRig = CreateRig();
            var wrongContent = CreateRectChild(badRig.Root.transform, "Wrong Content");
            var wrongScroll = CreateRectChild(badRig.Root.transform, "Wrong Scroll").AddComponent<ScrollRect>();
            wrongScroll.content = (RectTransform)wrongContent.transform;
            SetField(badRig.View, "achievementScrollRect", wrongScroll);
            Assert.That(badRig.View.TryValidateWiring(out var scrollDiagnostic), Is.False);
            Assert.That(scrollDiagnostic, Does.Contain("must reference the item content"));
        }

        [Test]
        public void StateRootsAndDetailWiringMustBeDistinct()
        {
            var rig = CreateRig();
            SetField(rig.View, "contentStateRoot", rig.EmptyState);
            Assert.That(rig.View.TryValidateWiring(out var stateDiagnostic), Is.False);
            Assert.That(stateDiagnostic, Does.Contain("state roots must be different"));

            SetField(rig.View, "contentStateRoot", rig.ContentState);
            SetField(rig.Detail, "descriptionText", null);
            Assert.That(rig.View.TryValidateWiring(out var detailDiagnostic), Is.False);
            Assert.That(detailDiagnostic, Does.Contain("description TMP_Text"));
        }

        [Test]
        public void ServiceExceptionClearsOldRowsAndDetailAndUsesErrorState()
        {
            var catalog = CreateAchievementCatalog(Achievement("achievement_before_error", "Before error", 0));
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(1));
            Assert.That(rig.DetailRoot.activeSelf, Is.True);

            SetList(catalog, "achievements", new VNAchievementDefinition[] { null });
            Assert.That(rig.View.Refresh(), Is.False);
            Assert.That(rig.View.LastDiagnostic, Is.Not.Null.And.Not.Empty);
            Assert.That(ActiveItems(rig), Is.Empty);
            Assert.That(rig.DetailRoot.activeSelf, Is.False);
            Assert.That(rig.EmptyState.activeSelf, Is.False);
            Assert.That(rig.ContentState.activeSelf, Is.False);
            Assert.That(rig.ErrorState.activeSelf, Is.True);
            Assert.That(rig.ErrorText.text, Is.EqualTo(rig.View.LastDiagnostic));
        }

        [Test]
        public void ActivationRefreshesPersistedUnlockWithoutUiWritesOrDuplicateListeners()
        {
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_on_enable", "Fresh Award", 0, description: "Fresh Description"));
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(GetField<GameObject>(ActiveItems(rig).Single(), "lockedIndicator").activeSelf, Is.True);

            rig.Root.SetActive(false);
            Invoke(rig.View, "OnDisable");
            SeedAchievement("achievement_on_enable");
            var writesAfterSeed = writes;
            var bytesAfterSeed = ReadPersistedBytes();

            rig.Root.SetActive(true);
            Invoke(rig.View, "OnEnable");
            var row = ActiveItems(rig).Single();
            Assert.That(GetField<GameObject>(row, "unlockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Is.EqualTo("Fresh Award"));
            Assert.That(rig.DetailUnlocked.activeSelf, Is.True);
            Assert.That(rig.Description.text, Is.EqualTo("Fresh Description"));
            Assert.That(writes, Is.EqualTo(writesAfterSeed));
            CollectionAssert.AreEqual(bytesAfterSeed, ReadPersistedBytes());

            for (var index = 0; index < 10; index++)
                Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            GetField<Button>(ActiveItems(rig).Single(), "button").onClick.Invoke();
            Assert.That(rig.View.SelectedAchievementId, Is.EqualTo("achievement_on_enable"));
            Assert.That(writes, Is.EqualTo(writesAfterSeed));
        }

        [Test]
        public void ViewOperationsAreReadOnlyAndStoredBytesRemainUnchanged()
        {
            SeedAchievement("achievement_already_unlocked");
            var icon = CreateSprite();
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_already_unlocked", "Unlocked", 0, description: "Description", icon: icon),
                Achievement("achievement_visible_locked", "Visible locked", 1));
            var rig = CreateRig();
            var writeBaseline = writes;
            var bytesBaseline = ReadPersistedBytes();

            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            Assert.That(rig.View.TrySelectAchievement("achievement_visible_locked"), Is.True);
            Assert.That(rig.View.TrySelectAchievement("achievement_already_unlocked"), Is.True);
            Assert.That(rig.View.Refresh(), Is.True);
            rig.Root.SetActive(false);
            Invoke(rig.View, "OnDisable");
            rig.Root.SetActive(true);
            Invoke(rig.View, "OnEnable");
            Assert.That(rig.View.Refresh(), Is.True);

            Assert.That(writes, Is.EqualTo(writeBaseline));
            CollectionAssert.AreEqual(bytesBaseline, ReadPersistedBytes());
        }
        [Test]
        public void RefreshResetsAchievementScrollToTop()
        {
            var rig = CreateRig();
            var viewport = rig.ScrollRect.viewport;
            var content = rig.ScrollRect.content;
            viewport.sizeDelta = new Vector2(200f, 100f);
            content.sizeDelta = new Vector2(200f, 400f);
            var catalog = CreateAchievementCatalog(
                Achievement("achievement_scroll_a", "Scroll A", 0),
                Achievement("achievement_scroll_b", "Scroll B", 1));
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);

            rig.ScrollRect.verticalNormalizedPosition = .35f;
            Assert.That(rig.ScrollRect.verticalNormalizedPosition, Is.LessThan(1f));
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.ScrollRect.verticalNormalizedPosition, Is.EqualTo(1f).Within(.001f));
        }

        [Test]
        public void ServiceExceptionUsesEmptyFallbackWhenErrorUiIsNotConfigured()
        {
            var catalog = CreateAchievementCatalog(Achievement("achievement_error_fallback", "Before error", 0));
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            SetField(rig.View, "errorStateRoot", null);
            SetField(rig.View, "errorText", null);
            SetList(catalog, "achievements", new VNAchievementDefinition[] { null });

            Assert.That(rig.View.Refresh(), Is.False);
            Assert.That(rig.View.LastDiagnostic, Is.Not.Null.And.Not.Empty);
            Assert.That(ActiveItems(rig), Is.Empty);
            Assert.That(rig.DetailRoot.activeSelf, Is.False);
            Assert.That(rig.EmptyState.activeSelf, Is.True);
            Assert.That(rig.ContentState.activeSelf, Is.False);
        }
        private VNAchievementCatalog CreateAchievementCatalog(params VNAchievementDefinition[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNAchievementCatalog>());
            SetList(catalog, "achievements", definitions);
            return catalog;
        }

        private static VNAchievementDefinition Achievement(
            string id,
            string title,
            int sortOrder,
            string description = "Test description",
            bool hidden = false,
            VNAchievementConditionType type = VNAchievementConditionType.Manual,
            int threshold = 0,
            Sprite icon = null)
        {
            return new VNAchievementDefinition(id, title, description, sortOrder, hidden, type,
                conditionTargetId: null, threshold: threshold, prerequisiteAchievementIds: null, optionalIcon: icon);
        }

        private VNAchievementService CreateService(VNAchievementCatalog catalog) =>
            new(catalog, metaProgress);

        private VNAchievementEvaluator CreateEvaluator(
            VNAchievementCatalog catalog, params string[] cgIds)
        {
            var evaluator = new VNAchievementEvaluator(
                catalog, CreateGalleryCatalog(cgIds), CreateArchiveCatalog(), metaProgress);
            evaluators.Add(evaluator);
            return evaluator;
        }

        private VNCGGalleryCatalog CreateGalleryCatalog(params string[] cgIds)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNCGGalleryCatalog>());
            SetList(catalog, "entries", cgIds.Select((id, index) =>
                new VNCGGalleryEntryDefinition(id, "Test CG " + index, index)));
            return catalog;
        }

        private VNArchiveCatalog CreateArchiveCatalog()
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNArchiveCatalog>());
            SetList(catalog, "categories", new[] { new VNArchiveCategoryDefinition("category_test", "Test") });
            SetList(catalog, "entries", Array.Empty<VNArchiveEntryDefinition>());
            return catalog;
        }

        private void SeedAchievement(string id)
        {
            Assert.That(metaProgress.TryUnlockAchievement(id), Is.True, "Could not seed Achievement " + id);
        }

        private void SeedCG(string id)
        {
            Assert.That(metaProgress.TryUnlockCG(id), Is.True, "Could not seed CG " + id);
        }

        private byte[] ReadPersistedBytes() =>
            File.Exists(repository.CanonicalFilePath) ? File.ReadAllBytes(repository.CanonicalFilePath) : null;

        private Sprite CreateSprite()
        {
            var texture = Own(new Texture2D(2, 2));
            return Own(Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f)));
        }

        private ViewRig CreateRig(bool activate = true)
        {
            var root = CreateOwnedRect("Achievements View Root");
            root.SetActive(false);
            var view = root.AddComponent<VNAchievementsView>();

            var emptyState = CreateRectChild(root.transform, "Empty State");
            var contentState = CreateRectChild(root.transform, "Content State");
            var scrollObject = CreateRectChild(contentState.transform, "Achievement ScrollRect");
            var scrollRect = scrollObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            var viewport = CreateRectChild(scrollObject.transform, "Viewport");
            var itemContent = CreateRectChild(viewport.transform, "Item Content");
            scrollRect.viewport = (RectTransform)viewport.transform;
            scrollRect.content = (RectTransform)itemContent.transform;
            var errorState = CreateRectChild(root.transform, "Error State");
            var errorText = CreateText(CreateRectChild(errorState.transform, "Error Text"));

            var itemPrefabObject = CreateOwnedRect("Achievement Item Prefab");
            itemPrefabObject.SetActive(false);
            var button = CreateButton(CreateRectChild(itemPrefabObject.transform, "Button"));
            var titleText = CreateText(CreateRectChild(itemPrefabObject.transform, "Title"));
            var lockedIndicator = CreateRectChild(itemPrefabObject.transform, "Locked Indicator");
            var unlockedIndicator = CreateRectChild(itemPrefabObject.transform, "Unlocked Indicator");
            var iconImage = CreateImage(CreateRectChild(itemPrefabObject.transform, "Icon"));
            var selectedIndicator = CreateRectChild(itemPrefabObject.transform, "Selected Indicator");
            var secretLockedIndicator = CreateRectChild(itemPrefabObject.transform, "Secret Locked Indicator");
            var itemPrefab = itemPrefabObject.AddComponent<VNAchievementItem>();
            SetField(itemPrefab, "button", button);
            SetField(itemPrefab, "titleText", titleText);
            SetField(itemPrefab, "lockedIndicator", lockedIndicator);
            SetField(itemPrefab, "unlockedIndicator", unlockedIndicator);
            SetField(itemPrefab, "iconImage", iconImage);
            SetField(itemPrefab, "selectedIndicator", selectedIndicator);
            SetField(itemPrefab, "secretLockedIndicator", secretLockedIndicator);

            var detailHost = CreateRectChild(contentState.transform, "Achievement Detail Host");
            var detail = detailHost.AddComponent<VNAchievementDetailView>();
            var detailRoot = CreateRectChild(detailHost.transform, "Detail Root");
            var detailTitle = CreateText(CreateRectChild(detailRoot.transform, "Detail Title"));
            var detailDescription = CreateText(CreateRectChild(detailRoot.transform, "Detail Description"));
            var detailIcon = CreateImage(CreateRectChild(detailRoot.transform, "Detail Icon"));
            var lockedState = CreateRectChild(detailRoot.transform, "Locked State");
            CreateText(CreateRectChild(lockedState.transform, "Locked Label")).text = "Locked";
            var unlockedState = CreateRectChild(detailRoot.transform, "Unlocked State");
            CreateText(CreateRectChild(unlockedState.transform, "Unlocked Label")).text = "Unlocked";
            var secretState = CreateRectChild(detailRoot.transform, "Secret Locked State");
            CreateText(CreateRectChild(secretState.transform, "Secret Label")).text = "???";
            SetField(detail, "detailRoot", detailRoot);
            SetField(detail, "titleText", detailTitle);
            SetField(detail, "descriptionText", detailDescription);
            SetField(detail, "iconImage", detailIcon);
            SetField(detail, "lockedStateRoot", lockedState);
            SetField(detail, "unlockedStateRoot", unlockedState);
            SetField(detail, "secretLockedStateRoot", secretState);

            SetField(view, "achievementScrollRect", scrollRect);
            SetField(view, "itemContent", itemContent.transform);
            SetField(view, "itemPrefab", itemPrefab);
            SetField(view, "detailView", detail);
            SetField(view, "emptyStateRoot", emptyState);
            SetField(view, "contentStateRoot", contentState);
            SetField(view, "errorStateRoot", errorState);
            SetField(view, "errorText", errorText);

            detailRoot.SetActive(false);
            lockedState.SetActive(false);
            unlockedState.SetActive(false);
            secretState.SetActive(false);
            emptyState.SetActive(false);
            contentState.SetActive(false);
            errorState.SetActive(false);

            if (activate)
            {
                root.SetActive(true);
                Invoke(view, "OnEnable");
            }

            return new ViewRig(root, view, itemContent.transform, scrollRect, emptyState, contentState,
                errorState, errorText, itemPrefab, detail, detailRoot, lockedState, unlockedState, secretState,
                detailTitle, detailDescription, detailIcon);
        }
        private GameObject CreateOwnedRect(string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            ownedObjects.Add(gameObject);
            return gameObject;
        }

        private GameObject CreateRectChild(Transform parent, string name)
        {
            var child = CreateOwnedRect(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private TMP_Text CreateText(GameObject gameObject)
        {
            var text = gameObject.AddComponent<TextMeshProUGUI>();
            text.text = string.Empty;
            return text;
        }

        private Button CreateButton(GameObject gameObject) => gameObject.AddComponent<Button>();

        private Image CreateImage(GameObject gameObject) => gameObject.AddComponent<Image>();

        private T Own<T>(T unityObject) where T : UnityEngine.Object
        {
            ownedObjects.Add(unityObject);
            return unityObject;
        }

        private static VNAchievementItem[] ActiveItems(ViewRig rig) =>
            rig.ItemContent.GetComponentsInChildren<VNAchievementItem>(true)
                .Where(item => item.gameObject.activeSelf)
                .ToArray();

        private static string VisibleText(ViewRig rig) => string.Join("\n",
            rig.Root.GetComponentsInChildren<TMP_Text>(true)
                .Where(text => text.gameObject.activeInHierarchy)
                .Select(text => text.text));

        private static void SetList<T>(UnityEngine.Object target, string fieldName, IEnumerable<T> values)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing field " + fieldName + " on " + target.GetType().Name);
            field.SetValue(target, values.ToList());
            var onValidate = target.GetType().GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic);
            onValidate?.Invoke(target, null);
        }

        private static T GetField<T>(object target, string fieldName) where T : class
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing field " + fieldName + " on " + target.GetType().Name);
            return field.GetValue(target) as T;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing field " + fieldName + " on " + target.GetType().Name);
            field.SetValue(target, value);
        }

        private static void Invoke(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Missing method " + methodName + " on " + target.GetType().Name);
            method.Invoke(target, null);
        }

        private sealed class ViewRig
        {
            public readonly GameObject Root;
            public readonly VNAchievementsView View;
            public readonly Transform ItemContent;
            public readonly ScrollRect ScrollRect;
            public readonly GameObject EmptyState;
            public readonly GameObject ContentState;
            public readonly GameObject ErrorState;
            public readonly TMP_Text ErrorText;
            public readonly VNAchievementItem ItemPrefab;
            public readonly VNAchievementDetailView Detail;
            public readonly GameObject DetailRoot;
            public readonly GameObject DetailLocked;
            public readonly GameObject DetailUnlocked;
            public readonly GameObject DetailSecret;
            public readonly TMP_Text Title;
            public readonly TMP_Text Description;
            public readonly Image DetailIcon;

            public ViewRig(
                GameObject root,
                VNAchievementsView view,
                Transform itemContent,
                ScrollRect scrollRect,
                GameObject emptyState,
                GameObject contentState,
                GameObject errorState,
                TMP_Text errorText,
                VNAchievementItem itemPrefab,
                VNAchievementDetailView detail,
                GameObject detailRoot,
                GameObject detailLocked,
                GameObject detailUnlocked,
                GameObject detailSecret,
                TMP_Text title,
                TMP_Text description,
                Image detailIcon)
            {
                Root = root;
                View = view;
                ItemContent = itemContent;
                ScrollRect = scrollRect;
                EmptyState = emptyState;
                ContentState = contentState;
                ErrorState = errorState;
                ErrorText = errorText;
                ItemPrefab = itemPrefab;
                Detail = detail;
                DetailRoot = detailRoot;
                DetailLocked = detailLocked;
                DetailUnlocked = detailUnlocked;
                DetailSecret = detailSecret;
                Title = title;
                Description = description;
                DetailIcon = detailIcon;
            }
        }
    }
}
