using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Gallery.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNCGGalleryViewTests
    {
        private const string PresentationCatalogPath = "Assets/_Project/Settings/Presentation/M3_PresentationCatalog.asset";

        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgress;
        private VNPresentationCatalog currentPresentationCatalog;
        private Sprite currentM3Sprite;
        private int writes;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M910GalleryTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            metaProgress = new VNMetaProgressService(repository);
            metaProgress.Load();

            currentPresentationCatalog = AssetDatabase.LoadAssetAtPath<VNPresentationCatalog>(PresentationCatalogPath);
            Assert.That(currentPresentationCatalog, Is.Not.Null);
            Assert.That(currentPresentationCatalog.TryGetCG("m3_cg", out currentM3Sprite), Is.True);
            Assert.That(currentM3Sprite, Is.Not.Null);
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
        public void EnableBeforeInjection_IsSafeAndInitializeRefreshesImmediatelyWhenActive()
        {
            var rig = CreateRig(activate: false);
            EnableRoot(rig);
            Assert.That(rig.View.LastDiagnostic, Is.Null);

            var service = CreateService(CreateGalleryCatalog(
                new VNCGGalleryEntryDefinition("cg_lifecycle", "Lifecycle", 0)),
                CreatePresentationCatalog("cg_lifecycle"));
            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(1));
            Assert.That(rig.EmptyState.activeSelf, Is.False);
            Assert.That(rig.ContentState.activeSelf, Is.True);
        }

        [Test]
        public void Initialize_IsIdempotentForSameServiceAndRejectsNullOrDifferentService()
        {
            var rig = CreateRig();
            var catalog = CreateGalleryCatalog(new VNCGGalleryEntryDefinition("cg_init", "Init", 0));
            var service = CreateService(catalog, CreatePresentationCatalog("cg_init"));

            Assert.That(Initialize(rig, null), Is.False);
            Assert.That(rig.View.LastDiagnostic, Does.Contain("VNCGGalleryService"));
            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);

            var otherService = CreateService(CreateGalleryCatalog(), CreatePresentationCatalog());
            Assert.That(Initialize(rig, otherService), Is.False);
            Assert.That(rig.View.LastDiagnostic, Does.Contain("different service"));
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(1));
        }

        [Test]
        public void EmptyGallery_ShowsGlobalEmptyStateAndNoCells()
        {
            var rig = CreateRig();
            var service = CreateService(CreateGalleryCatalog(), CreatePresentationCatalog());

            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            Assert.That(rig.EmptyState.activeSelf, Is.True);
            Assert.That(rig.ContentState.activeSelf, Is.False);
            Assert.That(rig.ErrorState.activeSelf, Is.False);
            Assert.That(ActiveItems(rig), Is.Empty);
            Assert.That(rig.ViewerGroup.alpha, Is.Zero);
            Assert.That(rig.View.LastDiagnostic, Is.Null);
        }

        [Test]
        public void LockedCells_ShowOnlyProjectedSafeTitlesAndClearArtwork()
        {
            var rig = CreateRig();
            var service = CreateService(
                CreateGalleryCatalog(
                    new VNCGGalleryEntryDefinition("cg_secret", "Secret title", 0, false),
                    new VNCGGalleryEntryDefinition("cg_known", "Known title", 1, true)),
                CreatePresentationCatalog("cg_secret", "cg_known"));

            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            var items = ActiveItems(rig);
            Assert.That(items, Has.Length.EqualTo(2));

            var hiddenTitleItem = items[0];
            Assert.That(GetField<GameObject>(hiddenTitleItem, "lockedRoot").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(hiddenTitleItem, "unlockedRoot").activeSelf, Is.False);
            Assert.That(GetField<Image>(hiddenTitleItem, "thumbnailImage").sprite, Is.Null);
            Assert.That(GetField<Button>(hiddenTitleItem, "button").interactable, Is.False);
            Assert.That(GetField<TMP_Text>(hiddenTitleItem, "titleText").text, Is.Empty);
            Assert.That(GetField<TMP_Text>(hiddenTitleItem, "titleText").gameObject.activeSelf, Is.False);

            var visibleTitleItem = items[1];
            var safeTitle = GetField<TMP_Text>(visibleTitleItem, "titleText");
            Assert.That(GetField<GameObject>(visibleTitleItem, "lockedRoot").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(visibleTitleItem, "unlockedRoot").activeSelf, Is.False);
            Assert.That(GetField<Image>(visibleTitleItem, "thumbnailImage").sprite, Is.Null);
            Assert.That(safeTitle.text, Is.EqualTo("Known title"));
            Assert.That(safeTitle.gameObject.activeSelf, Is.True);
            Assert.That(GetField<Button>(visibleTitleItem, "button").interactable, Is.False);
            Assert.That(items.Select(item => GetField<TMP_Text>(item, "titleText").text),
                Does.Not.Contain("cg_secret").And.Not.Contain("cg_known"));

            GetField<Button>(visibleTitleItem, "button").onClick.Invoke();
            Assert.That(rig.ViewerGroup.alpha, Is.Zero);
            Assert.That(rig.ViewerImage.sprite, Is.Null);
        }

        [Test]
        public void UnlockedCell_UsesProjectionSpriteAndOpensFullscreenViewer()
        {
            SeedCG("cg_open");
            var rig = CreateRig();
            var service = CreateService(
                CreateGalleryCatalog(new VNCGGalleryEntryDefinition("cg_open", "Unlocked CG", 0)),
                CreatePresentationCatalog("cg_open"));

            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            var item = ActiveItems(rig).Single();
            var thumbnail = GetField<Image>(item, "thumbnailImage");
            Assert.That(GetField<GameObject>(item, "unlockedRoot").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(item, "lockedRoot").activeSelf, Is.False);
            Assert.That(thumbnail.sprite, Is.SameAs(currentM3Sprite));
            Assert.That(thumbnail.enabled, Is.True);
            Assert.That(thumbnail.preserveAspect, Is.True);
            Assert.That(GetField<TMP_Text>(item, "titleText").text, Is.EqualTo("Unlocked CG"));
            Assert.That(GetField<Button>(item, "button").interactable, Is.True);

            GetField<Button>(item, "button").onClick.Invoke();
            Assert.That(rig.ViewerGroup.alpha, Is.EqualTo(1f));
            Assert.That(rig.ViewerGroup.interactable, Is.True);
            Assert.That(rig.ViewerGroup.blocksRaycasts, Is.True);
            Assert.That(rig.ViewerImage.sprite, Is.SameAs(currentM3Sprite));
            Assert.That(rig.ViewerImage.preserveAspect, Is.True);
            Assert.That(rig.ViewerTitle.text, Is.EqualTo("Unlocked CG"));

            rig.CloseButton.onClick.Invoke();
            AssertViewerClosed(rig);
        }

        [Test]
        public void Pool_ReusesCellsForEightThreeAndSixEntries()
        {
            var definitions = Enumerable.Range(0, 8)
                .Select(index => new VNCGGalleryEntryDefinition($"cg_pool_{index:00}", $"Title {index}", index))
                .ToArray();
            var catalog = CreateGalleryCatalog(definitions);
            var rig = CreateRig();
            var service = CreateService(catalog, CreatePresentationCatalog(definitions.Select(entry => entry.CgId).ToArray()));

            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            var initialItems = AllItems(rig);
            Assert.That(initialItems, Has.Length.EqualTo(8));
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(8));

            SetList(catalog, "entries", definitions.Take(3));
            Assert.That(Refresh(rig), Is.True, rig.View.LastDiagnostic);
            Assert.That(AllItems(rig), Has.Length.EqualTo(8));
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(3));

            SetList(catalog, "entries", definitions.Take(6));
            Assert.That(Refresh(rig), Is.True, rig.View.LastDiagnostic);
            Assert.That(AllItems(rig), Has.Length.EqualTo(8));
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(6));
            CollectionAssert.AreEqual(initialItems, AllItems(rig), "The existing cells should be reused after shrinking and growing.");
        }

        [Test]
        public void RebindingAnUnlockedCellAsLocked_ClearsSpriteTitleAndClickAction()
        {
            SeedCG("cg_unlocked_a");
            var catalog = CreateGalleryCatalog(new VNCGGalleryEntryDefinition("cg_unlocked_a", "Artwork A", 0));
            var rig = CreateRig();
            var service = CreateService(catalog, CreatePresentationCatalog("cg_unlocked_a", "cg_locked_b"));

            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            var item = ActiveItems(rig).Single();
            Assert.That(GetField<Image>(item, "thumbnailImage").sprite, Is.SameAs(currentM3Sprite));
            GetField<Button>(item, "button").onClick.Invoke();
            Assert.That(rig.ViewerGroup.alpha, Is.EqualTo(1f));

            SetList(catalog, "entries", new[]
            {
                new VNCGGalleryEntryDefinition("cg_locked_b", "Safe title B", 0, true),
            });
            Assert.That(Refresh(rig), Is.True, rig.View.LastDiagnostic);

            item = ActiveItems(rig).Single();
            Assert.That(GetField<GameObject>(item, "lockedRoot").activeSelf, Is.True);
            Assert.That(GetField<GameObject>(item, "unlockedRoot").activeSelf, Is.False);
            Assert.That(GetField<Image>(item, "thumbnailImage").sprite, Is.Null);
            Assert.That(GetField<TMP_Text>(item, "titleText").text, Is.EqualTo("Safe title B"));
            Assert.That(GetField<Button>(item, "button").interactable, Is.False);
            AssertViewerClosed(rig);

            GetField<Button>(item, "button").onClick.Invoke();
            AssertViewerClosed(rig);
        }

        [Test]
        public void RepeatedBinding_DoesNotAccumulateButtonCallbacks()
        {
            SeedCG("cg_callback");
            var rig = CreateRig();
            var service = CreateService(
                CreateGalleryCatalog(new VNCGGalleryEntryDefinition("cg_callback", "Callback", 0)),
                CreatePresentationCatalog("cg_callback"));
            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);

            var item = ActiveItems(rig).Single();
            var projection = GetProjection(service, "cg_callback");
            var selectedCount = 0;
            for (var index = 0; index < 5; index++)
                Assert.That(item.Bind(projection, (_, _) => selectedCount++, out var diagnostic), Is.True, diagnostic);

            GetField<Button>(item, "button").onClick.Invoke();
            Assert.That(selectedCount, Is.EqualTo(1));
        }

        [Test]
        public void Viewer_RejectsNullAndClearsPreviousImageAndTitle()
        {
            var rig = CreateRig();
            Assert.That(rig.Viewer.TryOpen(currentM3Sprite, "Current CG"), Is.True);
            Assert.That(rig.Viewer.TryOpen(null, "Must not remain"), Is.False);
            AssertViewerClosed(rig);

            Assert.That(rig.Viewer.TryOpen(currentM3Sprite, "Current CG"), Is.True);
            DisableRoot(rig);
            AssertViewerClosed(rig);
            EnableRoot(rig);
            AssertViewerClosed(rig);
        }

        [Test]
        public void GalleryActivation_RefreshesUnlocksAndRemainsReadOnly()
        {
            var rig = CreateRig();
            var service = CreateService(
                CreateGalleryCatalog(new VNCGGalleryEntryDefinition("m3_cg", "M3 CG", 0)),
                currentPresentationCatalog);
            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            var item = ActiveItems(rig).Single();
            Assert.That(GetField<Button>(item, "button").interactable, Is.False);

            SeedCG("m3_cg");
            SeedCG("unknown_old_cg");
            var writeBaseline = writes;
            var metaBytesBefore = File.ReadAllBytes(repository.CanonicalFilePath);

            DisableRoot(rig);
            EnableRoot(rig);
            item = ActiveItems(rig).Single();
            Assert.That(GetField<Button>(item, "button").interactable, Is.True);
            Assert.That(GetField<Image>(item, "thumbnailImage").sprite, Is.SameAs(currentM3Sprite));
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(1), "Unknown stale MetaProgress IDs must not create Gallery slots.");

            GetField<Button>(item, "button").onClick.Invoke();
            rig.CloseButton.onClick.Invoke();
            Refresh(rig);
            DisableRoot(rig);
            EnableRoot(rig);

            Assert.That(writes, Is.EqualTo(writeBaseline));
            CollectionAssert.AreEqual(metaBytesBefore, File.ReadAllBytes(repository.CanonicalFilePath));
            Assert.That(rig.View.LastDiagnostic, Is.Null);
        }

        [Test]
        public void ProjectionFailure_ClearsPreviousCellsAndViewerAndShowsDiagnostic()
        {
            SeedCG("m3_cg");
            var rig = CreateRig();
            var service = CreateService(
                CreateGalleryCatalog(
                    new VNCGGalleryEntryDefinition("m3_cg", "M3 CG", 0),
                    new VNCGGalleryEntryDefinition("cg_missing_sprite", "Missing", 1)),
                currentPresentationCatalog);
            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            Assert.That(ActiveItems(rig), Has.Length.EqualTo(2));

            GetField<Button>(ActiveItems(rig)[0], "button").onClick.Invoke();
            Assert.That(rig.ViewerGroup.alpha, Is.EqualTo(1f));
            SeedCG("cg_missing_sprite");
            DisableRoot(rig);
            EnableRoot(rig);

            Assert.That(Refresh(rig), Is.False);
            Assert.That(rig.View.LastDiagnostic, Does.Contain("Unlocked Gallery CG 'cg_missing_sprite'"));
            Assert.That(ActiveItems(rig), Is.Empty);
            Assert.That(rig.ContentState.activeSelf, Is.False);
            Assert.That(rig.EmptyState.activeSelf, Is.False);
            Assert.That(rig.ErrorState.activeSelf, Is.True);
            Assert.That(rig.ErrorText.text, Is.EqualTo(rig.View.LastDiagnostic));
            AssertViewerClosed(rig);
        }

        [Test]
        public void BindingOrder_MatchesServiceProjectionOrderWithoutAdditionalSorting()
        {
            var rig = CreateRig();
            var service = CreateService(
                CreateGalleryCatalog(
                    new VNCGGalleryEntryDefinition("cg_z", "Z", 1, true),
                    new VNCGGalleryEntryDefinition("cg_m", "M", 0, true),
                    new VNCGGalleryEntryDefinition("cg_a", "A", 0, true)),
                CreatePresentationCatalog());

            Assert.That(Initialize(rig, service), Is.True, rig.View.LastDiagnostic);
            CollectionAssert.AreEqual(new[] { "A", "M", "Z" },
                ActiveItems(rig).Select(item => GetField<TMP_Text>(item, "titleText").text));
        }

        [Test]
        public void MissingScrollContentWiring_FailsDeterministicallyAndClearsContent()
        {
            var rig = CreateRig();
            rig.ScrollRect.content = null;
            var service = CreateService(CreateGalleryCatalog(), CreatePresentationCatalog());

            Assert.That(Initialize(rig, service), Is.False);
            var firstDiagnostic = rig.View.LastDiagnostic;
            Assert.That(firstDiagnostic, Does.Contain("must reference the item content"));
            Assert.That(Refresh(rig), Is.False);
            Assert.That(rig.View.LastDiagnostic, Is.EqualTo(firstDiagnostic));
            Assert.That(rig.ErrorState.activeSelf, Is.True);
            Assert.That(rig.ContentState.activeSelf, Is.False);
        }

        [Test]
        public void UiComponents_HaveNoDirectCatalogMetaProgressOrCgIdFieldDependency()
        {
            var viewFields = typeof(VNCGGalleryView).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var itemFields = typeof(VNCGGalleryItem).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var forbiddenTypes = new[]
            {
                typeof(VNCGGalleryCatalog),
                typeof(VNPresentationCatalog),
                typeof(VNMetaProgressService),
            };

            Assert.That(viewFields.Any(field => forbiddenTypes.Contains(field.FieldType)), Is.False);
            Assert.That(itemFields.Any(field => forbiddenTypes.Contains(field.FieldType)), Is.False);
            Assert.That(itemFields.Any(field => field.Name.IndexOf("cgid", StringComparison.OrdinalIgnoreCase) >= 0), Is.False);
        }

        private static bool Initialize(ViewRig rig, VNCGGalleryService service)
        {
            var result = rig.View.Initialize(service);
            BindActiveItemListeners(rig);
            return result;
        }

        private static bool Refresh(ViewRig rig)
        {
            var result = rig.View.Refresh();
            BindActiveItemListeners(rig);
            return result;
        }

        private static void DisableRoot(ViewRig rig)
        {
            rig.Root.SetActive(false);
            InvokePrivateNoArguments(rig.View, "OnDisable");
            InvokePrivateNoArguments(rig.Viewer, "OnDisable");
            foreach (var item in AllItems(rig)) InvokePrivateNoArguments(item, "OnDisable");
        }

        private static void EnableRoot(ViewRig rig)
        {
            rig.Root.SetActive(true);
            InvokePrivateNoArguments(rig.Viewer, "OnEnable");
            InvokePrivateNoArguments(rig.View, "OnEnable");
            BindActiveItemListeners(rig);
        }

        private static void BindActiveItemListeners(ViewRig rig)
        {
            foreach (var item in ActiveItems(rig)) InvokePrivateNoArguments(item, "OnEnable");
        }

        private static void InvokePrivateNoArguments(object instance, string methodName)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "Missing lifecycle method " + methodName);
            method.Invoke(instance, null);
        }
        private ViewRig CreateRig(bool activate = true)
        {
            var root = CreateOwnedRect("Gallery Root");
            root.SetActive(false);
            var view = root.AddComponent<VNCGGalleryView>();

            var scrollObject = CreateRectChild(root.transform, "Grid Scroll");
            var scrollRect = scrollObject.AddComponent<ScrollRect>();
            var viewport = CreateRectChild(scrollObject.transform, "Viewport");
            scrollRect.viewport = (RectTransform)viewport.transform;

            var emptyState = CreateRectChild(root.transform, "Empty State");
            var contentState = CreateRectChild(root.transform, "Content State");
            var itemContentObject = CreateRectChild(contentState.transform, "Content");
            var itemContent = (RectTransform)itemContentObject.transform;
            scrollRect.content = itemContent;
            var errorState = CreateRectChild(root.transform, "Error State");
            var errorText = CreateText(CreateRectChild(errorState.transform, "Error Text"));

            var viewerObject = CreateRectChild(root.transform, "Fullscreen Viewer");
            var viewerGroup = viewerObject.AddComponent<CanvasGroup>();
            var fullImage = CreateImage(CreateRectChild(viewerObject.transform, "Full Image"));
            var viewerTitle = CreateText(CreateRectChild(viewerObject.transform, "Title"));
            var closeButton = CreateButton(CreateRectChild(viewerObject.transform, "Close Button"));
            var viewer = viewerObject.AddComponent<VNCGGalleryViewer>();
            SetField(viewer, "viewerCanvasGroup", viewerGroup);
            SetField(viewer, "fullImage", fullImage);
            SetField(viewer, "closeButton", closeButton);
            SetField(viewer, "titleText", viewerTitle);

            var itemPrefabObject = CreateOwnedRect("Gallery Item Prefab");
            itemPrefabObject.SetActive(false);
            var itemButton = CreateButton(CreateRectChild(itemPrefabObject.transform, "Button"));
            var unlockedRoot = CreateRectChild(itemPrefabObject.transform, "Unlocked");
            var thumbnail = CreateImage(CreateRectChild(unlockedRoot.transform, "Thumbnail"));
            var lockedRoot = CreateRectChild(itemPrefabObject.transform, "Locked");
            var itemTitle = CreateText(CreateRectChild(itemPrefabObject.transform, "Item Title"));
            var item = itemPrefabObject.AddComponent<VNCGGalleryItem>();
            SetField(item, "button", itemButton);
            SetField(item, "thumbnailImage", thumbnail);
            SetField(item, "unlockedRoot", unlockedRoot);
            SetField(item, "lockedRoot", lockedRoot);
            SetField(item, "titleText", itemTitle);

            SetField(view, "galleryScrollRect", scrollRect);
            SetField(view, "itemContent", itemContent);
            SetField(view, "itemPrefab", item);
            SetField(view, "emptyStateRoot", emptyState);
            SetField(view, "contentStateRoot", contentState);
            SetField(view, "fullscreenViewer", viewer);
            SetField(view, "errorStateRoot", errorState);
            SetField(view, "errorText", errorText);

            if (activate)
            {
                root.SetActive(true);
                InvokePrivateNoArguments(viewer, "OnEnable");
            }
            return new ViewRig(root, view, itemContent, scrollRect, emptyState, contentState, errorState,
                viewer, viewerGroup, fullImage, viewerTitle, closeButton, errorText);
        }

        private VNCGGalleryCatalog CreateGalleryCatalog(params VNCGGalleryEntryDefinition[] definitions)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNCGGalleryCatalog>());
            SetList(catalog, "entries", definitions);
            return catalog;
        }

        private VNPresentationCatalog CreatePresentationCatalog(params string[] cgIds)
        {
            var catalog = Own(ScriptableObject.CreateInstance<VNPresentationCatalog>());
            var entries = new List<VNSpriteCatalogEntry>();
            foreach (var id in cgIds)
            {
                var entry = new VNSpriteCatalogEntry();
                SetField(entry, "id", id);
                SetField(entry, "sprite", currentM3Sprite);
                entries.Add(entry);
            }
            SetList(catalog, "cgs", entries);
            return catalog;
        }

        private VNCGGalleryService CreateService(VNCGGalleryCatalog catalog, VNPresentationCatalog presentationCatalog)
        {
            return new VNCGGalleryService(catalog, presentationCatalog, metaProgress);
        }

        private void SeedCG(string id)
        {
            Assert.That(metaProgress.TryUnlockCG(id), Is.True, "Could not seed CG " + id);
        }

        private static VNCGGalleryEntryProjection GetProjection(VNCGGalleryService service, string cgId)
        {
            Assert.That(service.TryGetGalleryEntries(out var entries, out var diagnostic), Is.True, diagnostic);
            return entries.Single(entry => entry.CgId == cgId);
        }

        private static VNCGGalleryItem[] AllItems(ViewRig rig)
        {
            return rig.Content.GetComponentsInChildren<VNCGGalleryItem>(true)
                .OrderBy(item => item.transform.GetSiblingIndex())
                .ToArray();
        }

        private static VNCGGalleryItem[] ActiveItems(ViewRig rig)
        {
            return AllItems(rig).Where(item => item.gameObject.activeSelf).ToArray();
        }

        private static void AssertViewerClosed(ViewRig rig)
        {
            Assert.That(rig.ViewerGroup.alpha, Is.Zero);
            Assert.That(rig.ViewerGroup.interactable, Is.False);
            Assert.That(rig.ViewerGroup.blocksRaycasts, Is.False);
            Assert.That(rig.ViewerImage.sprite, Is.Null);
            Assert.That(rig.ViewerTitle.text, Is.Empty);
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

        private static Image CreateImage(GameObject gameObject)
        {
            return gameObject.AddComponent<Image>();
        }

        private static Button CreateButton(GameObject gameObject)
        {
            var graphic = gameObject.AddComponent<Image>();
            var button = gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            return button;
        }

        private static TMP_Text CreateText(GameObject gameObject)
        {
            return gameObject.AddComponent<TextMeshProUGUI>();
        }

        private T Own<T>(T value) where T : UnityEngine.Object
        {
            ownedObjects.Add(value);
            return value;
        }

        private static void SetList<T>(object instance, string fieldName, IEnumerable<T> values)
        {
            SetField(instance, fieldName, new List<T>(values));
            var onValidate = instance.GetType().GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidate?.Invoke(instance, null);
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

        private sealed class ViewRig
        {
            public readonly GameObject Root;
            public readonly VNCGGalleryView View;
            public readonly Transform Content;
            public readonly ScrollRect ScrollRect;
            public readonly GameObject EmptyState;
            public readonly GameObject ContentState;
            public readonly GameObject ErrorState;
            public readonly VNCGGalleryViewer Viewer;
            public readonly CanvasGroup ViewerGroup;
            public readonly Image ViewerImage;
            public readonly TMP_Text ViewerTitle;
            public readonly Button CloseButton;
            public readonly TMP_Text ErrorText;

            public ViewRig(GameObject root, VNCGGalleryView view, Transform content, ScrollRect scrollRect,
                GameObject emptyState, GameObject contentState, GameObject errorState, VNCGGalleryViewer viewer,
                CanvasGroup viewerGroup, Image viewerImage, TMP_Text viewerTitle, Button closeButton, TMP_Text errorText)
            {
                Root = root;
                View = view;
                Content = content;
                ScrollRect = scrollRect;
                EmptyState = emptyState;
                ContentState = contentState;
                ErrorState = errorState;
                Viewer = viewer;
                ViewerGroup = viewerGroup;
                ViewerImage = viewerImage;
                ViewerTitle = viewerTitle;
                CloseButton = closeButton;
                ErrorText = errorText;
            }
        }
    }
}
