using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Archive.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNArchiveViewTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private int writes;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M911ArchiveTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                if (ownedObjects[index] != null) UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void EnableBeforeInjectionIsSafe_AndInitializationRefreshesWhenActive()
        {
            var rig = CreateRig(false);
            Invoke(rig.View, "OnEnable");
            Assert.That(rig.View.LastDiagnostic, Is.Null);

            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_lore", "Lore") },
                new[] { new VNArchiveEntryDefinition("archive_lore", "category_lore", "Lore entry", "Summary", "Body") });
            rig.Root.SetActive(true);
            Invoke(rig.View, "OnEnable");
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveCategoryItems(rig), Has.Length.EqualTo(1));
            Assert.That(ActiveEntryItems(rig), Has.Length.EqualTo(1));
            Assert.That(rig.View.SelectedCategoryId, Is.EqualTo("category_lore"));
        }

        [Test]
        public void InitializeIsIdempotentForSameService_AndRejectsNullOrDifferentService()
        {
            var rig = CreateRig();
            var service = CreateService(CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_one", "One") },
                new[] { new VNArchiveEntryDefinition("archive_one", "category_one", "One", "Summary", "Body") }));
            Assert.That(rig.View.Initialize(null), Is.False);
            Assert.That(rig.View.LastDiagnostic, Does.Contain("VNArchiveService"));
            Assert.That(rig.View.Initialize(service), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.Initialize(service), Is.True, rig.View.LastDiagnostic);

            var other = CreateService(CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_other", "Other") },
                Array.Empty<VNArchiveEntryDefinition>()));
            Assert.That(rig.View.Initialize(other), Is.False);
            Assert.That(rig.View.LastDiagnostic, Does.Contain("different service"));
            Assert.That(ActiveCategoryItems(rig), Has.Length.EqualTo(1));
        }

        [Test]
        public void EmptyArchiveShowsGlobalEmptyStateAndClearsBothPoolsAndDetail()
        {
            var rig = CreateRig();
            var service = CreateService(CreateCatalog(
                Array.Empty<VNArchiveCategoryDefinition>(), Array.Empty<VNArchiveEntryDefinition>()));

            Assert.That(rig.View.Initialize(service), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.GlobalEmpty.activeSelf, Is.True);
            Assert.That(rig.CategoryEmpty.activeSelf, Is.False);
            Assert.That(rig.ContentState.activeSelf, Is.False);
            Assert.That(ActiveCategoryItems(rig), Is.Empty);
            Assert.That(ActiveEntryItems(rig), Is.Empty);
            Assert.That(rig.DetailRoot.activeSelf, Is.False);
            Assert.That(rig.View.LastDiagnostic, Is.Null);
        }

        [Test]
        public void CategoryProjectionOrderAndOptInCountsAreRenderedExactly()
        {
            SeedArchive("archive_lore_1");
            var catalog = CreateCatalog(
                new[]
                {
                    new VNArchiveCategoryDefinition("category_z", "Zeta", 5),
                    new VNArchiveCategoryDefinition("category_lore", "Z Lore", 0, true),
                    new VNArchiveCategoryDefinition("category_no_count", "A No Count", 2, false)
                },
                new[]
                {
                    new VNArchiveEntryDefinition("archive_lore_1", "category_lore", "Z first", "S", "B", 0),
                    new VNArchiveEntryDefinition("archive_lore_2", "category_lore", "A second", "S", "B", 1, true),
                    new VNArchiveEntryDefinition("archive_no_count", "category_no_count", "No count", "S", "B")
                });
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);

            var items = ActiveCategoryItems(rig);
            CollectionAssert.AreEqual(new[] { "Z Lore", "A No Count", "Zeta" },
                items.Select(item => GetField<TMP_Text>(item, "titleText").text));
            Assert.That(GetField<TMP_Text>(items[0], "completionCountText").text, Is.EqualTo("1 / 2"));
            Assert.That(GetField<TMP_Text>(items[0], "completionCountText").gameObject.activeSelf, Is.True);
            Assert.That(GetField<TMP_Text>(items[1], "completionCountText").text, Is.Empty);
            Assert.That(GetField<TMP_Text>(items[1], "completionCountText").gameObject.activeSelf, Is.False);
            Assert.That(items.Select(item => GetField<TMP_Text>(item, "completionCountText").text),
                Does.Not.Contain("50%"));
            CollectionAssert.AreEqual(new[] { "Z first", "A second" },
                ActiveEntryItems(rig).Select(item => GetField<TMP_Text>(item, "titleText").text));
        }

        [Test]
        public void InitialEntrySelectionPrefersFirstUnlockedAndLockedRowsRemainSelectable()
        {
            SeedArchive("archive_open_second");
            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_lore", "Lore") },
                new[]
                {
                    new VNArchiveEntryDefinition("archive_locked_first", "category_lore", "Hidden one", "Secret summary", "Secret body", 0),
                    new VNArchiveEntryDefinition("archive_open_second", "category_lore", "Opened", "Visible summary", "Visible body", 1),
                    new VNArchiveEntryDefinition("archive_open_third", "category_lore", "Opened third", "Summary", "Body", 2)
                });
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_open_second"));
            Assert.That(rig.DetailUnlocked.activeSelf, Is.True);
            Assert.That(rig.Title.text, Is.EqualTo("Opened"));

            Assert.That(rig.View.TrySelectEntry("archive_locked_first"), Is.True);
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_locked_first"));
            Assert.That(rig.DetailLocked.activeSelf, Is.True);
            Assert.That(rig.DetailUnlocked.activeSelf, Is.False);
            Assert.That(rig.Title.text, Is.Empty);
            Assert.That(rig.Summary.text, Is.Empty);
            Assert.That(rig.Body.text, Is.Empty);
            Assert.That(GetField<Button>(ActiveEntryItems(rig)[0], "button").interactable, Is.True);
        }

        [Test]
        public void AllLockedCategoryFallsBackToFirstEntryAndUsesOnlySafeTitleOrGenericLabel()
        {
            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_secret", "Secrets") },
                new[]
                {
                    new VNArchiveEntryDefinition("archive_hidden_id", "category_secret", "Hidden authored title", "Hidden summary", "Hidden body", 0, false),
                    new VNArchiveEntryDefinition("archive_safe_id", "category_secret", "Known safe title", "Hidden summary", "Hidden body", 1, true)
                });
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);

            var rows = ActiveEntryItems(rig);
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_hidden_id"));
            Assert.That(GetField<TMP_Text>(rows[0], "titleText").text, Is.EqualTo("Locked"));
            Assert.That(GetField<TMP_Text>(rows[1], "titleText").text, Is.EqualTo("Known safe title"));
            Assert.That(GetField<GameObject>(rows[0], "lockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(rows[0], "unlockedIndicator").activeSelf, Is.False);
            Assert.That(rows.Select(row => GetField<TMP_Text>(row, "titleText").text),
                Does.Not.Contain("archive_hidden_id").And.Not.Contain("Hidden authored title"));
            Assert.That(rig.DetailLocked.activeSelf, Is.True);

            GetField<Button>(rows[0], "button").onClick.Invoke();
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_hidden_id"));
            Assert.That(rig.DetailLocked.activeSelf, Is.True);
            Assert.That(rig.Summary.text, Is.Empty);
            Assert.That(rig.Body.text, Is.Empty);
        }
        [Test]
        public void UnlockedDetailDisplaysOnlyProjectedFieldsAndOptionalImage()
        {
            SeedArchive("archive_unlocked");
            var sprite = CreateSprite();
            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_lore", "Lore") },
                new[]
                {
                    new VNArchiveEntryDefinition("archive_unlocked", "category_lore", "Unlocked title",
                        "Unlocked summary", "Unlocked body", optionalImage: sprite)
                });
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);

            Assert.That(rig.DetailUnlocked.activeSelf, Is.True);
            Assert.That(rig.DetailLocked.activeSelf, Is.False);
            Assert.That(rig.Title.text, Is.EqualTo("Unlocked title"));
            Assert.That(rig.Summary.text, Is.EqualTo("Unlocked summary"));
            Assert.That(rig.Body.text, Is.EqualTo("Unlocked body"));
            Assert.That(rig.OptionalImage.sprite, Is.SameAs(sprite));
            Assert.That(rig.OptionalImage.preserveAspect, Is.True);
            Assert.That(rig.OptionalImage.enabled, Is.True);
        }

        [Test]
        public void RefreshPreservesValidSelectionsAndFallsBackWithoutCrossCategorySelection()
        {
            SeedArchive("archive_a_open");
            SeedArchive("archive_b_open");
            var categories = new[]
            {
                new VNArchiveCategoryDefinition("category_a", "A", 0),
                new VNArchiveCategoryDefinition("category_b", "B", 1)
            };
            var entries = new[]
            {
                new VNArchiveEntryDefinition("archive_a_locked", "category_a", "Locked", "S", "B", 0),
                new VNArchiveEntryDefinition("archive_a_open", "category_a", "Open A", "S A", "B A", 1),
                new VNArchiveEntryDefinition("archive_b_open", "category_b", "Open B", "S B", "B B", 0),
                new VNArchiveEntryDefinition("archive_b_second", "category_b", "Second B", "S", "B", 1)
            };
            var catalog = CreateCatalog(categories, entries);
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_a_open"));

            Assert.That(rig.View.TrySelectEntry("archive_a_locked"), Is.True);
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_a_locked"));

            Assert.That(rig.View.TrySelectCategory("category_b"), Is.True);
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_b_open"));
            Assert.That(rig.View.TrySelectEntry("archive_b_second"), Is.True);
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedCategoryId, Is.EqualTo("category_b"));
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_b_second"));

            SetList(catalog, "entries", new[]
            {
                new VNArchiveEntryDefinition("archive_b_open", "category_b", "Open B", "S B", "B B", 0)
            });
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_b_open"));

            SetList(catalog, "categories", new[] { categories[0] });
            SetList(catalog, "entries", new[]
            {
                new VNArchiveEntryDefinition("archive_a_locked", "category_a", "Locked", "S", "B", 0)
            });
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.SelectedCategoryId, Is.EqualTo("category_a"));
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_a_locked"));
        }

        [Test]
        public void EmptySelectedCategoryKeepsNavigationAndClearsEntryDetail()
        {
            var rig = CreateRig();
            var catalog = CreateCatalog(
                new[]
                {
                    new VNArchiveCategoryDefinition("category_with_entry", "With Entry", 0),
                    new VNArchiveCategoryDefinition("category_empty", "Empty", 1)
                },
                new[] { new VNArchiveEntryDefinition("archive_one", "category_with_entry", "Entry", "S", "B") });
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.TrySelectCategory("category_empty"), Is.True);

            Assert.That(ActiveCategoryItems(rig), Has.Length.EqualTo(2));
            Assert.That(ActiveEntryItems(rig), Is.Empty);
            Assert.That(rig.GlobalEmpty.activeSelf, Is.False);
            Assert.That(rig.CategoryEmpty.activeSelf, Is.True);
            Assert.That(rig.ContentState.activeSelf, Is.False);
            Assert.That(rig.DetailRoot.activeSelf, Is.False);
            Assert.That(rig.View.SelectedArchiveId, Is.Null);
        }

        [Test]
        public void PoolReusesEightThreeAndSixCategoriesAndEntries()
        {
            var categories = Enumerable.Range(0, 8)
                .Select(index => new VNArchiveCategoryDefinition($"category_{index:00}", $"Category {index}", index))
                .ToArray();
            var entries = Enumerable.Range(0, 8)
                .Select(index => new VNArchiveEntryDefinition($"archive_{index:00}", "category_00", $"Entry {index}", "S", "B", index, true))
                .ToArray();
            var catalog = CreateCatalog(categories, entries);
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.View.CategoryPoolCapacity, Is.GreaterThanOrEqualTo(8));
            Assert.That(rig.View.EntryPoolCapacity, Is.GreaterThanOrEqualTo(8));
            Assert.That(ActiveCategoryItems(rig), Has.Length.EqualTo(8));
            Assert.That(ActiveEntryItems(rig), Has.Length.EqualTo(8));

            SetList(catalog, "categories", categories.Take(3));
            SetList(catalog, "entries", entries.Take(3));
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveCategoryItems(rig), Has.Length.EqualTo(3));
            Assert.That(ActiveEntryItems(rig), Has.Length.EqualTo(3));
            Assert.That(rig.View.CategoryPoolCapacity, Is.GreaterThanOrEqualTo(8));
            Assert.That(rig.View.EntryPoolCapacity, Is.GreaterThanOrEqualTo(8));

            SetList(catalog, "categories", categories.Take(6));
            SetList(catalog, "entries", entries.Take(6));
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveCategoryItems(rig), Has.Length.EqualTo(6));
            Assert.That(ActiveEntryItems(rig), Has.Length.EqualTo(6));
            Assert.That(rig.View.CategoryPoolCapacity, Is.GreaterThanOrEqualTo(8));
            Assert.That(rig.View.EntryPoolCapacity, Is.GreaterThanOrEqualTo(8));
        }

        [Test]
        public void ReboundPooledRowAndLockedDetailDoNotLeakPreviouslyUnlockedContent()
        {
            SeedArchive("archive_previously_open");
            var sprite = CreateSprite();
            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_lore", "Lore") },
                new[]
                {
                    new VNArchiveEntryDefinition("archive_previously_open", "category_lore", "Formerly visible",
                        "Secret summary", "Secret body", 0, optionalImage: sprite)
                });
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True, rig.View.LastDiagnostic);
            var pooledRow = ActiveEntryItems(rig)[0];
            Assert.That(GetField<TMP_Text>(pooledRow, "titleText").text, Is.EqualTo("Formerly visible"));

            SetList(catalog, "entries", new[]
            {
                new VNArchiveEntryDefinition("archive_current_locked", "category_lore", "Hidden title",
                    "Hidden summary", "Hidden body", 0, showTitleWhenLocked: false, optionalImage: sprite)
            });
            Assert.That(rig.View.Refresh(), Is.True, rig.View.LastDiagnostic);
            pooledRow = ActiveEntryItems(rig)[0];
            Assert.That(GetField<TMP_Text>(pooledRow, "titleText").text, Is.EqualTo("Locked"));
            Assert.That(GetField<GameObject>(pooledRow, "lockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(pooledRow, "unlockedIndicator").activeSelf, Is.False);
            Assert.That(GetField<TMP_Text>(pooledRow, "titleText").text,
                Does.Not.Contain("archive_current_locked").And.Not.Contain("Hidden title"));
            Assert.That(rig.DetailLocked.activeSelf, Is.True);
            Assert.That(rig.DetailUnlocked.activeSelf, Is.False);
            Assert.That(rig.Title.text, Is.Empty);
            Assert.That(rig.Summary.text, Is.Empty);
            Assert.That(rig.Body.text, Is.Empty);
            Assert.That(rig.OptionalImage.sprite, Is.Null);
            Assert.That(rig.OptionalImage.enabled, Is.False);
        }
        [Test]
        public void InitializeBeforeActivationRefreshesOnEnable()
        {
            var rig = CreateRig(false);
            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_late", "Late") },
                new[] { new VNArchiveEntryDefinition("archive_late", "category_late", "Late entry", "S", "B") });
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            Assert.That(ActiveCategoryItems(rig), Is.Empty);

            rig.Root.SetActive(true);
            Invoke(rig.View, "OnEnable");
            Assert.That(ActiveCategoryItems(rig), Has.Length.EqualTo(1));
            Assert.That(ActiveEntryItems(rig), Has.Length.EqualTo(1));
        }

        [Test]
        public void ArchiveActivationQueriesFreshProjectionAfterUnlock()
        {
            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_fresh", "Fresh") },
                new[] { new VNArchiveEntryDefinition("archive_fresh", "category_fresh", "Fresh title", "Summary", "Body") });
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            var row = ActiveEntryItems(rig).Single();
            Assert.That(GetField<GameObject>(row, "lockedIndicator").activeSelf, Is.True);
            Assert.That(rig.DetailLocked.activeSelf, Is.True);

            SeedArchive("archive_fresh");
            rig.Root.SetActive(false);
            Invoke(rig.View, "OnDisable");
            Assert.That(rig.DetailRoot.activeSelf, Is.False);
            rig.Root.SetActive(true);
            Invoke(rig.View, "OnEnable");

            row = ActiveEntryItems(rig).Single();
            Assert.That(GetField<GameObject>(row, "unlockedIndicator").activeSelf, Is.True);
            Assert.That(GetField<TMP_Text>(row, "titleText").text, Is.EqualTo("Fresh title"));
            Assert.That(rig.DetailUnlocked.activeSelf, Is.True);
            Assert.That(rig.Body.text, Is.EqualTo("Body"));
        }

        [Test]
        public void RepeatedCategoryAndEntryBindReplaceCallbacksDeterministically()
        {
            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_bind", "Bind") },
                new[] { new VNArchiveEntryDefinition("archive_bind", "category_bind", "Entry", "S", "B") });
            var service = CreateService(catalog);
            var categoryProjection = service.GetCategories().Single();
            var entryProjection = service.GetEntries("category_bind").Single();
            var rig = CreateRig();
            var categoryItem = GetField<VNArchiveCategoryItem>(rig.View, "categoryItemPrefab");
            var entryItem = GetField<VNArchiveEntryItem>(rig.View, "entryItemPrefab");
            var categoryCalls = 0;
            var entryCalls = 0;

            for (var index = 0; index < 10; index++)
            {
                Assert.That(categoryItem.Bind(categoryProjection, false, _ => categoryCalls++, out var categoryDiagnostic),
                    Is.True, categoryDiagnostic);
                Assert.That(entryItem.Bind(entryProjection, false, _ => entryCalls++, out var entryDiagnostic),
                    Is.True, entryDiagnostic);
            }
            GetField<Button>(categoryItem, "button").onClick.Invoke();
            GetField<Button>(entryItem, "button").onClick.Invoke();

            Assert.That(categoryCalls, Is.EqualTo(1));
            Assert.That(entryCalls, Is.EqualTo(1));
        }

        [Test]
        public void ViewWiringValidationFailsDeterministicallyAndOptionalErrorUiIsOptional()
        {
            var rig = CreateRig();
            SetField(rig.View, "errorStateRoot", null);
            SetField(rig.View, "errorText", null);
            Assert.That(rig.View.TryValidateWiring(out var validDiagnostic), Is.True, validDiagnostic);

            SetField(rig.View, "entryItemPrefab", null);
            Assert.That(rig.View.TryValidateWiring(out var missingItemDiagnostic), Is.False);
            Assert.That(missingItemDiagnostic, Does.Contain("entry item prefab"));

            var repairedRig = CreateRig();
            var badScrollObject = CreateOwnedRect("Bad Category Scroll");
            var badScroll = badScrollObject.AddComponent<ScrollRect>();
            SetField(repairedRig.View, "categoryScrollRect", badScroll);
            Assert.That(repairedRig.View.TryValidateWiring(out var scrollDiagnostic), Is.False);
            Assert.That(scrollDiagnostic, Does.Contain("category ScrollRect content"));
        }

        [Test]
        public void StateRootsAndDetailWiringMustBeDistinctAndComplete()
        {
            var rig = CreateRig();
            SetField(rig.View, "categoryEmptyStateRoot", rig.GlobalEmpty);
            Assert.That(rig.View.TryValidateWiring(out var rootDiagnostic), Is.False);
            Assert.That(rootDiagnostic, Does.Contain("state roots must be different"));

            SetField(rig.View, "categoryEmptyStateRoot", rig.CategoryEmpty);
            SetField(rig.Detail, "bodyText", null);
            Assert.That(rig.View.TryValidateWiring(out var detailDiagnostic), Is.False);
            Assert.That(detailDiagnostic, Does.Contain("body TMP_Text"));
        }

        [Test]
        public void RefreshSelectionAndDetailOperationsDoNotWriteMetaProgress()
        {
            SeedArchive("archive_write");
            var catalog = CreateCatalog(
                new[]
                {
                    new VNArchiveCategoryDefinition("category_write", "Write", 0),
                    new VNArchiveCategoryDefinition("category_empty", "Empty", 1)
                },
                new[] { new VNArchiveEntryDefinition("archive_write", "category_write", "Writable", "S", "B") });
            var rig = CreateRig();
            var writeBaseline = writes;
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            Assert.That(rig.View.TrySelectEntry("archive_write"), Is.True);
            Assert.That(rig.View.Refresh(), Is.True);
            Assert.That(rig.View.TrySelectCategory("category_empty"), Is.True);
            Assert.That(rig.View.Refresh(), Is.True);
            Assert.That(writes, Is.EqualTo(writeBaseline));
        }

        [Test]
        public void DisableClearsTransientDetailButRetainsSessionOnlySelection()
        {
            var catalog = CreateCatalog(
                new[] { new VNArchiveCategoryDefinition("category_session", "Session") },
                new[] { new VNArchiveEntryDefinition("archive_session", "category_session", "Title", "Summary", "Body") });
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            Assert.That(rig.View.SelectedCategoryId, Is.EqualTo("category_session"));
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_session"));

            Invoke(rig.View, "OnDisable");
            Assert.That(rig.DetailRoot.activeSelf, Is.False);
            Assert.That(rig.Title.text, Is.Empty);
            Assert.That(rig.Summary.text, Is.Empty);
            Assert.That(rig.Body.text, Is.Empty);
            Assert.That(rig.View.SelectedCategoryId, Is.EqualTo("category_session"));
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_session"));
        }
        [Test]
        public void ClickingCategoryChangesSelectionAndClearsOldCategoryEntrySelection()
        {
            var catalog = CreateCatalog(
                new[]
                {
                    new VNArchiveCategoryDefinition("category_a", "A", 0),
                    new VNArchiveCategoryDefinition("category_b", "B", 1)
                },
                new[]
                {
                    new VNArchiveEntryDefinition("archive_a", "category_a", "A entry", "S", "B", 0),
                    new VNArchiveEntryDefinition("archive_b", "category_b", "B entry", "S", "B", 0)
                });
            var rig = CreateRig();
            Assert.That(rig.View.Initialize(CreateService(catalog)), Is.True);
            GetField<Button>(ActiveCategoryItems(rig)[1], "button").onClick.Invoke();
            Assert.That(rig.View.SelectedCategoryId, Is.EqualTo("category_b"));
            Assert.That(rig.View.SelectedArchiveId, Is.EqualTo("archive_b"));
            Assert.That(rig.DetailLocked.activeSelf, Is.True);
            Assert.That(rig.Title.text, Is.Empty);
            Assert.That(ActiveCategoryItems(rig).Count(item =>
                GetField<GameObject>(item, "selectedIndicator").activeSelf), Is.EqualTo(1));
        }

        private VNArchiveCatalog CreateCatalog(
            IEnumerable<VNArchiveCategoryDefinition> categories,
            IEnumerable<VNArchiveEntryDefinition> entries)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNArchiveCatalog>());
            SetList(catalog, "categories", categories);
            SetList(catalog, "entries", entries);
            return catalog;
        }

        private VNArchiveService CreateService(VNArchiveCatalog catalog) => new(catalog, metaProgress);

        private void SeedArchive(string archiveId)
        {
            Assert.That(metaProgress.TryUnlockArchiveEntry(archiveId), Is.True, "Could not seed Archive entry " + archiveId);
        }

        private Sprite CreateSprite()
        {
            var texture = Own(new Texture2D(2, 2));
            var sprite = Own(Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f)));
            return sprite;
        }

        private ViewRig CreateRig(bool activate = true)
        {
            var root = CreateOwnedRect("Archive View Root");
            root.SetActive(false);
            var view = root.AddComponent<VNArchiveView>();

            var categoryContent = CreateRectChild(root.transform, "Category Content");
            var globalEmpty = CreateRectChild(root.transform, "Global Empty");
            var categoryEmpty = CreateRectChild(root.transform, "Category Empty");
            var contentState = CreateRectChild(root.transform, "Archive Content State");
            var entryContent = CreateRectChild(contentState.transform, "Entry Content");
            var errorState = CreateRectChild(root.transform, "Error State");
            var errorText = CreateText(CreateRectChild(errorState.transform, "Error Text"));

            var categoryPrefabObject = CreateOwnedRect("Archive Category Item Prefab");
            categoryPrefabObject.SetActive(false);
            var categoryButton = CreateButton(CreateRectChild(categoryPrefabObject.transform, "Button"));
            var categoryTitle = CreateText(CreateRectChild(categoryPrefabObject.transform, "Title"));
            var categoryCount = CreateText(CreateRectChild(categoryPrefabObject.transform, "Count"));
            var categorySelected = CreateRectChild(categoryPrefabObject.transform, "Selected");
            var categoryPrefab = categoryPrefabObject.AddComponent<VNArchiveCategoryItem>();
            SetField(categoryPrefab, "button", categoryButton);
            SetField(categoryPrefab, "titleText", categoryTitle);
            SetField(categoryPrefab, "completionCountText", categoryCount);
            SetField(categoryPrefab, "selectedIndicator", categorySelected);

            var entryPrefabObject = CreateOwnedRect("Archive Entry Item Prefab");
            entryPrefabObject.SetActive(false);
            var entryButton = CreateButton(CreateRectChild(entryPrefabObject.transform, "Button"));
            var entryTitle = CreateText(CreateRectChild(entryPrefabObject.transform, "Title"));
            var entryUnlocked = CreateRectChild(entryPrefabObject.transform, "Unlocked");
            var entryLocked = CreateRectChild(entryPrefabObject.transform, "Locked");
            var entrySelected = CreateRectChild(entryPrefabObject.transform, "Selected");
            var entryPrefab = entryPrefabObject.AddComponent<VNArchiveEntryItem>();
            SetField(entryPrefab, "button", entryButton);
            SetField(entryPrefab, "titleText", entryTitle);
            SetField(entryPrefab, "unlockedIndicator", entryUnlocked);
            SetField(entryPrefab, "lockedIndicator", entryLocked);
            SetField(entryPrefab, "selectedIndicator", entrySelected);

            var detailHost = CreateRectChild(contentState.transform, "Detail Host");
            var detail = detailHost.AddComponent<VNArchiveDetailView>();
            var detailRoot = CreateRectChild(detailHost.transform, "Detail Root");
            var detailLocked = CreateRectChild(detailRoot.transform, "Locked Detail");
            CreateText(CreateRectChild(detailLocked.transform, "Locked Label")).text = "Locked";
            var detailUnlocked = CreateRectChild(detailRoot.transform, "Unlocked Detail");
            var title = CreateText(CreateRectChild(detailUnlocked.transform, "Detail Title"));
            var summary = CreateText(CreateRectChild(detailUnlocked.transform, "Detail Summary"));
            var body = CreateText(CreateRectChild(detailUnlocked.transform, "Detail Body"));
            var optionalImage = CreateImage(CreateRectChild(detailUnlocked.transform, "Optional Image"));
            SetField(detail, "detailRoot", detailRoot);
            SetField(detail, "lockedStateRoot", detailLocked);
            SetField(detail, "unlockedContentRoot", detailUnlocked);
            SetField(detail, "titleText", title);
            SetField(detail, "summaryText", summary);
            SetField(detail, "bodyText", body);
            SetField(detail, "optionalImage", optionalImage);

            SetField(view, "categoryContent", categoryContent.transform);
            SetField(view, "categoryItemPrefab", categoryPrefab);
            SetField(view, "entryContent", entryContent.transform);
            SetField(view, "entryItemPrefab", entryPrefab);
            SetField(view, "detailView", detail);
            SetField(view, "globalEmptyStateRoot", globalEmpty);
            SetField(view, "categoryEmptyStateRoot", categoryEmpty);
            SetField(view, "contentStateRoot", contentState);
            SetField(view, "errorStateRoot", errorState);
            SetField(view, "errorText", errorText);

            detailRoot.SetActive(false);
            detailLocked.SetActive(false);
            detailUnlocked.SetActive(false);
            globalEmpty.SetActive(false);
            categoryEmpty.SetActive(false);
            contentState.SetActive(false);
            errorState.SetActive(false);

            if (activate)
            {
                root.SetActive(true);
                Invoke(view, "OnEnable");
            }

            return new ViewRig(root, view, categoryContent.transform, entryContent.transform, globalEmpty,
                categoryEmpty, contentState, detail, detailRoot, detailLocked, detailUnlocked, title, summary, body,
                optionalImage, categoryPrefab, entryPrefab);
        }
        private static VNArchiveCategoryItem[] ActiveCategoryItems(ViewRig rig)
        {
            return rig.CategoryContent.GetComponentsInChildren<VNArchiveCategoryItem>(true)
                .Where(item => item.gameObject.activeSelf).OrderBy(item => item.transform.GetSiblingIndex()).ToArray();
        }

        private static VNArchiveEntryItem[] ActiveEntryItems(ViewRig rig)
        {
            return rig.EntryContent.GetComponentsInChildren<VNArchiveEntryItem>(true)
                .Where(item => item.gameObject.activeSelf).OrderBy(item => item.transform.GetSiblingIndex()).ToArray();
        }

        private static GameObject CreateRectChild(Transform parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child;
        }

        private GameObject CreateOwnedRect(string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            ownedObjects.Add(gameObject);
            return gameObject;
        }

        private static Button CreateButton(GameObject gameObject)
        {
            var graphic = gameObject.AddComponent<Image>();
            var button = gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            return button;
        }

        private static TMP_Text CreateText(GameObject gameObject) => gameObject.AddComponent<TextMeshProUGUI>();
        private static Image CreateImage(GameObject gameObject) => gameObject.AddComponent<Image>();

        private T Own<T>(T value) where T : UnityEngine.Object
        {
            ownedObjects.Add(value);
            return value;
        }

        private static void SetList<T>(object instance, string fieldName, IEnumerable<T> values)
        {
            SetField(instance, fieldName, new List<T>(values));
            instance.GetType().GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(instance, null);
        }

        private static T GetField<T>(object instance, string fieldName) where T : class
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "Missing private field " + fieldName);
            return field.GetValue(instance) as T;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "Missing private field " + fieldName);
            field.SetValue(instance, value);
        }

        private static void Invoke(object instance, string methodName)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "Missing lifecycle method " + methodName);
            method.Invoke(instance, null);
        }

        private sealed class ViewRig
        {
            public readonly GameObject Root;
            public readonly VNArchiveView View;
            public readonly Transform CategoryContent;
            public readonly Transform EntryContent;
            public readonly GameObject GlobalEmpty;
            public readonly GameObject CategoryEmpty;
            public readonly GameObject ContentState;
            public readonly VNArchiveDetailView Detail;
            public readonly GameObject DetailRoot;
            public readonly GameObject DetailLocked;
            public readonly GameObject DetailUnlocked;
            public readonly TMP_Text Title;
            public readonly TMP_Text Summary;
            public readonly TMP_Text Body;
            public readonly Image OptionalImage;
            public readonly VNArchiveCategoryItem CategoryPrefab;
            public readonly VNArchiveEntryItem EntryPrefab;

            public ViewRig(
                GameObject root, VNArchiveView view, Transform categoryContent, Transform entryContent,
                GameObject globalEmpty, GameObject categoryEmpty, GameObject contentState, VNArchiveDetailView detail,
                GameObject detailRoot, GameObject detailLocked, GameObject detailUnlocked, TMP_Text title,
                TMP_Text summary, TMP_Text body, Image optionalImage, VNArchiveCategoryItem categoryPrefab,
                VNArchiveEntryItem entryPrefab)
            {
                Root = root;
                View = view;
                CategoryContent = categoryContent;
                EntryContent = entryContent;
                GlobalEmpty = globalEmpty;
                CategoryEmpty = categoryEmpty;
                ContentState = contentState;
                Detail = detail;
                DetailRoot = detailRoot;
                DetailLocked = detailLocked;
                DetailUnlocked = detailUnlocked;
                Title = title;
                Summary = summary;
                Body = body;
                OptionalImage = optionalImage;
                CategoryPrefab = categoryPrefab;
                EntryPrefab = entryPrefab;
            }
        }
    }
}