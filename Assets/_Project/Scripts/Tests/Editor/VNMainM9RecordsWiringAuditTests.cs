using System;
using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records;
using ProjectAllTime.VN.Records.UI;
using ProjectAllTime.VN.Records.Timeline;
using ProjectAllTime.VN.Records.Timeline.UI;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Gallery.UI;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Archive.UI;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Achievements.UI;
using ProjectAllTime.VN.Records.Replay.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class VNMainM9RecordsWiringAuditTests
    {
        internal const string ScenePath = "Assets/_Project/Scenes/VN_Main.unity";
        internal const string CatalogRoot = "Assets/_Project/Settings/Records/Production/";
        private Scene scene;
        private bool opened;

        [SetUp]
        public void SetUp()
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }

        [Test]
        public void Composition_UsesOneRuntimeOwnerAndAllAuthoritativeReferences()
        {
            var bootstrap = Single<VNRecordsRuntimeBootstrap>();
            Assert.That(bootstrap.gameObject, Is.SameAs(Single<VNMetaProgressRuntimeBootstrap>().gameObject));
            Assert.That(bootstrap.gameObject, Is.SameAs(Single<ProjectAllTime.VN.Settings.VNSettingsRuntimeBootstrap>().gameObject));
            Assert.That(Ref<VNMetaProgressRuntimeBootstrap>(bootstrap,"metaProgressBootstrap"), Is.SameAs(Single<VNMetaProgressRuntimeBootstrap>()));
            Assert.That(Ref<VNTimelineView>(bootstrap,"timelineView"), Is.SameAs(Single<VNTimelineView>()));
            Assert.That(Ref<VNCGGalleryView>(bootstrap,"galleryView"), Is.SameAs(Single<VNCGGalleryView>()));
            Assert.That(Ref<VNArchiveView>(bootstrap,"archiveView"), Is.SameAs(Single<VNArchiveView>()));
            Assert.That(Ref<VNAchievementsView>(bootstrap,"achievementsView"), Is.SameAs(Single<VNAchievementsView>()));
            Assert.That(Ref<VNReplayController>(bootstrap,"replayController"), Is.SameAs(Single<VNReplayController>()));
            Assert.That(bootstrap.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            var order = (DefaultExecutionOrder)Attribute.GetCustomAttribute(typeof(VNRecordsRuntimeBootstrap),typeof(DefaultExecutionOrder));
            Assert.That(order.order, Is.EqualTo(-1));
            Assert.That(MonoImporter.GetExecutionOrder(MonoScript.FromMonoBehaviour(bootstrap)), Is.Zero);
        }

        [Test]
        public void Modal_HasFourDistinctInactiveRootsAndRuntimeOwnedControls()
        {
            var modal = Single<VNRecordsModal>();
            Assert.That(modal.gameObject.activeInHierarchy, Is.True);
            Assert.That(modal.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            var group = Ref<CanvasGroup>(modal,"modalCanvasGroup");
            Assert.That(group, Is.SameAs(modal.GetComponent<CanvasGroup>()));
            Assert.That(group.alpha, Is.Zero);
            Assert.That(group.interactable || group.blocksRaycasts, Is.False);
            var roots = new[]{"timeline","gallery","archive","achievements"}.Select(n=>Ref<GameObject>(modal,n+"Root")).ToArray();
            Assert.That(roots.Distinct().Count(), Is.EqualTo(4));
            foreach (var root in roots)
            {
                Assert.That(root.transform.IsChildOf(modal.transform), Is.True);
                Assert.That(root.activeSelf, Is.False);
            }
            var selectedIndicators = new[]
            {
                Ref<GameObject>(modal,"timelineSelectedIndicator"),
                Ref<GameObject>(modal,"gallerySelectedIndicator"),
                Ref<GameObject>(modal,"archiveSelectedIndicator"),
                Ref<GameObject>(modal,"achievementsSelectedIndicator"),
            };
            Assert.That(selectedIndicators.All(indicator => indicator != null), Is.True);
            Assert.That(selectedIndicators.Distinct().Count(), Is.EqualTo(4));
            Assert.That(modal.GetComponentsInChildren<Image>(true).Any(i=>i.name=="Dimmer" && i.raycastTarget && i.transform.parent==modal.transform), Is.True);
            Assert.That(Ref<VNRecordsModal>(Single<VNConvenienceModalController>(),"recordsModal"), Is.SameAs(modal));
            foreach (var button in modal.GetComponentsInChildren<Button>(true)) Empty(button);
        }

        [Test]
        public void QuickControl_RetainsNineDistinctButtonsAndTheRecordsEntryPoint()
        {
            var quick = Single<VNQuickControlBar>();
            var buttons = new[]{"next","hide","backlog","skip","auto","save","load","settings","records"}
                .Select(n=>Ref<Button>(quick,n+"Button")).ToArray();
            Assert.That(buttons.Distinct().Count(), Is.EqualTo(9));
            foreach (var button in buttons) Assert.That(button.transform.IsChildOf(quick.transform), Is.True);
            Empty(buttons.Last());
            Assert.That(buttons.Last().GetComponentInChildren<TMPro.TMP_Text>(true).text, Is.EqualTo("Records"));
        }

        [Test]
        public void DomainViews_ValidateTheirInactiveTemplatesAndScrollContracts()
        {
            var timeline = Single<VNTimelineView>();
            var gallery = Single<VNCGGalleryView>();
            var archive = Single<VNArchiveView>();
            var achievements = Single<VNAchievementsView>();
            Assert.That(timeline.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            Assert.That(gallery.TryValidateWiring(out diagnostic), Is.True, diagnostic);
            Assert.That(archive.TryValidateWiring(out diagnostic), Is.True, diagnostic);
            Assert.That(achievements.TryValidateWiring(out diagnostic), Is.True, diagnostic);
            Template(timeline,"chapterItemPrefab",Ref<Transform>(timeline,"chapterContent"));
            Template(timeline,"entryItemPrefab",Ref<Transform>(timeline,"entryContent"));
            Template(gallery,"itemPrefab",Ref<Transform>(gallery,"itemContent"));
            Template(archive,"categoryItemPrefab",Ref<Transform>(archive,"categoryContent"));
            Template(archive,"entryItemPrefab",Ref<Transform>(archive,"entryContent"));
            Template(achievements,"itemPrefab",Ref<Transform>(achievements,"itemContent"));
            Assert.That(Ref<Button>(Ref<Component>(timeline,"entryItemPrefab"),"replayButton").gameObject.activeSelf, Is.False);
            foreach (var scroll in Single<VNRecordsModal>().GetComponentsInChildren<ScrollRect>(true))
            {
                Assert.That(scroll.horizontal, Is.False);
                Assert.That(scroll.vertical, Is.True);
                Assert.That(scroll.viewport.GetComponent<RectMask2D>(), Is.Not.Null);
                Assert.That(scroll.content.parent, Is.SameAs(scroll.viewport));
                Assert.That(scroll.content.GetComponent<ContentSizeFitter>().verticalFit, Is.EqualTo(ContentSizeFitter.FitMode.PreferredSize));
                Assert.That(scroll.content.GetComponents<LayoutGroup>().Length, Is.EqualTo(1));
            }
            var grid = Ref<Transform>(gallery,"itemContent").GetComponent<GridLayoutGroup>();
            Assert.That(grid.constraint, Is.EqualTo(GridLayoutGroup.Constraint.FixedColumnCount));
            Assert.That(grid.constraintCount, Is.GreaterThan(0));
            Assert.That(Ref<Image>(Ref<Component>(gallery,"itemPrefab"),"thumbnailImage").preserveAspect, Is.True);
        }

        [Test]
        public void Replay_IsAnInactiveOverlayWithEntirelyLocalPresentationAndControls()
        {
            var controller = Single<VNReplayController>();
            var modal = Single<VNRecordsModal>();
            var host = Ref<Transform>(controller,"replaySessionHost");
            var view = Ref<VNReplayView>(controller,"replayViewTemplate");
            Assert.That(controller.TryValidateWiring(out var diagnostic), Is.True, diagnostic);
            Assert.That(view.TryValidateWiring(out diagnostic), Is.True, diagnostic);
            Assert.That(host.IsChildOf(modal.transform), Is.True);
            foreach (var key in new[]{"timeline","gallery","archive","achievements"})
                Assert.That(host.IsChildOf(Ref<GameObject>(modal,key+"Root").transform), Is.False);
            Assert.That(view.transform.IsChildOf(host), Is.True);
            Assert.That(view.gameObject.activeSelf, Is.False);
            Assert.That(Ref<CanvasGroup>(view,"overlayCanvasGroup"), Is.SameAs(view.GetComponent<CanvasGroup>()));
            Assert.That(view.GetComponent<Image>().raycastTarget, Is.True);
            var presentation = view.PresentationController;
            Assert.That(presentation.transform.IsChildOf(view.transform), Is.True);
            var production = All<VNPresentationController>().Single(p=>!p.transform.IsChildOf(modal.transform));
            Assert.That(presentation, Is.Not.SameAs(production));
            Assert.That(Ref<VNPresentationCatalog>(presentation,"catalog"), Is.SameAs(Ref<VNPresentationCatalog>(production,"catalog")));
            foreach (var key in new[]{"backgroundImage","cgImage"})
                Assert.That(Ref<Image>(presentation,key).transform.IsChildOf(view.transform), Is.True);
            var slots = view.GetComponentsInChildren<VNCharacterSlotView>(true);
            Assert.That(slots.Length, Is.EqualTo(5));
            CollectionAssert.AreEquivalent(Enum.GetValues(typeof(VNCharacterSlot)), slots.Select(s=>s.Slot));
            foreach (var slot in slots) Assert.That(slot.IsConfigured, Is.True);
            Assert.That(view.LinePresenter.transform.IsChildOf(view.transform), Is.True);
            Assert.That(view.LinePresenter.autoAdvance, Is.False);
            Assert.That(view.LinePresenter.lineText.transform.IsChildOf(view.transform), Is.True);
            Assert.That(view.LinePresenter.characterNameText.transform.IsChildOf(view.transform), Is.True);
            Assert.That(new SerializedObject(view.LinePresenter).FindProperty("eventHandlers").arraySize, Is.Zero);
            Assert.That(view.GetComponentsInChildren<OptionsPresenter>(true), Is.Empty);
            Assert.That(view.GetComponentsInChildren<VNLineLifecyclePresenter>(true), Is.Empty);
            Assert.That(view.GetComponentsInChildren<AudioSource>(true), Is.Empty);
            Empty(view.NextButton); Empty(view.CloseButton);
        }

        [Test]
        public void ProductionCatalogs_AreTheFourAuthoritativeEmptyAssetsAndValidate()
        {
            var bootstrap = Single<VNRecordsRuntimeBootstrap>();
            var timeline = Catalog<VNTimelineCatalog>(bootstrap,"timelineCatalog");
            var gallery = Catalog<VNCGGalleryCatalog>(bootstrap,"galleryCatalog");
            var archive = Catalog<VNArchiveCatalog>(bootstrap,"archiveCatalog");
            var achievements = Catalog<VNAchievementCatalog>(bootstrap,"achievementCatalog");
            var runner = Single<DialogueRunner>();
            var yarn = Ref<YarnProject>(bootstrap,"yarnProject");
            Assert.That(yarn, Is.SameAs(Ref<YarnProject>(runner,"yarnProject")));
            var production = All<VNPresentationController>().Single(p=>!p.transform.IsChildOf(Single<VNRecordsModal>().transform));
            var presentation = Ref<VNPresentationCatalog>(bootstrap,"presentationCatalog");
            Assert.That(presentation, Is.SameAs(Ref<VNPresentationCatalog>(production,"catalog")));
            Assert.That(timeline.Chapters, Is.Empty); Assert.That(timeline.Entries, Is.Empty);
            Assert.That(gallery.Entries, Is.Empty); Assert.That(archive.Categories, Is.Empty); Assert.That(archive.Entries, Is.Empty);
            Assert.That(achievements.Achievements, Is.Empty);
            Assert.That(VNRecordsCatalogValidator.TryValidate(timeline,gallery,archive,achievements,
                new VNRecordsCatalogValidationContext(yarn,presentation),out var diagnostic), Is.True, diagnostic);
        }

        [Test]
        public void ExistingSceneAuthorities_KeepTheirBaselineAndHaveNoMissingScripts()
        {
            var runner = Single<DialogueRunner>();
            var so = new SerializedObject(runner);
            Assert.That(so.FindProperty("autoStart").boolValue, Is.True);
            Assert.That(so.FindProperty("startNode").stringValue, Is.EqualTo("M2_UI_START"));
            Assert.That(All<EventSystem>().Length, Is.EqualTo(1));
            Assert.That(All<Canvas>().Length, Is.EqualTo(2));
            var canvas = Single<VNRecordsModal>().GetComponentInParent<Canvas>();
            Assert.That(canvas.GetComponent<GraphicRaycaster>(), Is.Not.Null);
            var scaler = canvas.GetComponent<CanvasScaler>();
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920,1080)));
            Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(.5f));
            var modalController = Single<VNConvenienceModalController>();
            Assert.That(Ref<VNBacklogModal>(modalController,"backlogModal"), Is.SameAs(Single<VNBacklogModal>()));
            Assert.That(Ref<VNSettingsModal>(modalController,"settingsModal"), Is.SameAs(Single<VNSettingsModal>()));
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject), Is.Zero,t.name);
        }

        private T[] All<T>() where T:Component => scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<T>(true)).ToArray();
        private T Single<T>() where T:Component { var values=All<T>(); Assert.That(values.Length,Is.EqualTo(1),typeof(T).Name); return values[0]; }
        internal static T Ref<T>(UnityEngine.Object owner,string field) where T:UnityEngine.Object
        { var p=new SerializedObject(owner).FindProperty(field); Assert.That(p,Is.Not.Null,field); var value=p.objectReferenceValue as T; Assert.That(value,Is.Not.Null,field); return value; }
        private static T Catalog<T>(VNRecordsRuntimeBootstrap bootstrap,string field) where T:ScriptableObject
        { var value=Ref<T>(bootstrap,field); Assert.That(AssetDatabase.GetAssetPath(value),Is.EqualTo(CatalogRoot+typeof(T).Name+".asset")); return value; }
        private static void Template(Component owner,string field,Transform content)
        { var template=Ref<Component>(owner,field); Assert.That(template.gameObject.activeSelf,Is.False); Assert.That(template.transform.IsChildOf(content),Is.False); }
        private static void Empty(Button button) { Assert.That(button,Is.Not.Null); Assert.That(button.onClick.GetPersistentEventCount(),Is.Zero,button.name); }
    }
}
