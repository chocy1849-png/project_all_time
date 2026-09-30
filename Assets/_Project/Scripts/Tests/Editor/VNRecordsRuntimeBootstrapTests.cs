using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;
using Object = UnityEngine.Object;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class VNRecordsRuntimeBootstrapTests
    {
        private readonly List<Object> owned = new();
        private Scene scene;
        private bool opened;
        private string temporaryRoot;
        private int writes;
        private VNMetaProgressRepository repository;
        private VNMetaProgressRuntimeBootstrap meta;
        private VNRecordsRuntimeBootstrap bootstrap;
        private VNRecordsModal modal;
        private VNAchievementCatalog achievements;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(),"M9RecordsBootstrap-"+Guid.NewGuid().ToString("N"));
            writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot,()=>writes++);
            scene = SceneManager.GetSceneByPath(VNMainM9RecordsWiringAuditTests.ScenePath);
            opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(VNMainM9RecordsWiringAuditTests.ScenePath,OpenSceneMode.Additive);
            var source = scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<VNRecordsRuntimeBootstrap>(true)).Single();
            var canvas = Own(new GameObject("M9 test canvas",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster)));
            // Clone the authored UI graph, preserving only its internal references. No production Start runs in EditMode.
            var sourceModal = scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<VNRecordsModal>(true)).Single();
            modal = Object.Instantiate(sourceModal,canvas.transform,false);
            var host = Own(new GameObject("M9 test runtime"));
            host.AddComponent<VNDialogueSessionState>();
            meta = host.AddComponent<VNMetaProgressRuntimeBootstrap>();
            var runner = Own(new GameObject("M9 test runner")).AddComponent<DialogueRunner>();
            Set(meta,"dialogueRunner",runner);
            Set(meta,"repositoryFactory",(Func<VNMetaProgressRepository>)(()=>repository));
            bootstrap = host.AddComponent<VNRecordsRuntimeBootstrap>();
            Set(bootstrap,"metaProgressBootstrap",meta);
            Set(bootstrap,"yarnProject",Ref<YarnProject>(source,"yarnProject"));
            Set(bootstrap,"presentationCatalog",Ref<VNPresentationCatalog>(source,"presentationCatalog"));
            Set(bootstrap,"timelineCatalog",Own(ScriptableObject.CreateInstance<VNTimelineCatalog>()));
            Set(bootstrap,"galleryCatalog",Own(ScriptableObject.CreateInstance<VNCGGalleryCatalog>()));
            Set(bootstrap,"archiveCatalog",Own(ScriptableObject.CreateInstance<VNArchiveCatalog>()));
            achievements = Own(ScriptableObject.CreateInstance<VNAchievementCatalog>());
            Set(bootstrap,"achievementCatalog",achievements);
            Set(bootstrap,"timelineView",modal.GetComponentInChildren<VNTimelineView>(true));
            Set(bootstrap,"galleryView",modal.GetComponentInChildren<VNCGGalleryView>(true));
            Set(bootstrap,"archiveView",modal.GetComponentInChildren<VNArchiveView>(true));
            Set(bootstrap,"achievementsView",modal.GetComponentInChildren<VNAchievementsView>(true));
            Set(bootstrap,"replayController",modal.GetComponent<VNReplayController>());
            Assert.That(bootstrap.TryValidateWiring(out var diagnostic),Is.True,diagnostic);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var item in owned.AsEnumerable().Reverse()) if(item!=null) Object.DestroyImmediate(item);
            owned.Clear();
            if(opened) EditorSceneManager.CloseScene(scene,true);
            if(Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot,true);
        }

        [TestCase("metaProgressBootstrap")]
        [TestCase("yarnProject")]
        [TestCase("presentationCatalog")]
        [TestCase("timelineCatalog")]
        [TestCase("galleryCatalog")]
        [TestCase("archiveCatalog")]
        [TestCase("achievementCatalog")]
        [TestCase("timelineView")]
        [TestCase("galleryView")]
        [TestCase("archiveView")]
        [TestCase("achievementsView")]
        [TestCase("replayController")]
        public void NullDependency_FailsBeforeCompositionOrPersistence(string field)
        {
            Set(bootstrap,field,null);
            Assert.That(bootstrap.TryValidateWiring(out var diagnostic),Is.False);
            Assert.That(diagnostic,Is.Not.Empty);
            Invoke(bootstrap,"Initialize");
            Assert.That(bootstrap.IsInitialized,Is.False);
            Assert.That(bootstrap.LastDiagnostic,Is.EqualTo(diagnostic));
            AssertNoOwners();
            Assert.That(writes,Is.Zero);
        }

        [Test]
        public void MetaProgress_MustAlreadyBeInitializedAndMustExposeItsSingleAuthority()
        {
            Invoke(bootstrap,"Initialize");
            Assert.That(bootstrap.LastDiagnostic,Does.Contain("must be initialized"));
            Assert.That(meta.IsInitialized,Is.False);
            AssertNoOwners();
            Assert.That(writes,Is.Zero);
        }

        [Test]
        public void MissingMetaProgressService_IsRejectedWithoutCreatingAnotherAuthority()
        {
            Set(meta,"<IsInitialized>k__BackingField",true);
            Invoke(bootstrap,"Initialize");
            Assert.That(bootstrap.LastDiagnostic,Does.Contain("must provide its MetaProgressService"));
            AssertNoOwners();
            Assert.That(writes,Is.Zero);
        }

        [Test]
        public void InvalidView_IsRejectedBeforeEvaluatorSubscription()
        {
            InitializeMeta();
            Set(Ref<VNTimelineView>(bootstrap,"timelineView"),"entryContent",null);
            Assert.That(bootstrap.TryValidateWiring(out var diagnostic),Is.False);
            Invoke(bootstrap,"Initialize");
            Assert.That(bootstrap.LastDiagnostic,Is.EqualTo(diagnostic));
            AssertNoOwners();
            Assert.That(writes,Is.Zero);
        }

        [Test]
        public void InvalidCatalog_PropagatesTheExistingValidatorDiagnostic()
        {
            InitializeMeta();
            var timeline = Ref<VNTimelineCatalog>(bootstrap,"timelineCatalog");
            Set(timeline,"chapters",new List<VNTimelineChapterDefinition>{new VNTimelineChapterDefinition("INVALID","Test")});
            Assert.That(bootstrap.TryValidateWiring(out var diagnostic),Is.False);
            Invoke(bootstrap,"Initialize");
            Assert.That(bootstrap.LastDiagnostic,Is.EqualTo(diagnostic));
            Assert.That(diagnostic,Does.Contain("lowercase snake_case"));
            AssertNoOwners();
            Assert.That(writes,Is.Zero);
        }

        [Test]
        public void ValidComposition_UsesM8ServiceAndInitializesEvaluationBeforeFirstViewProjection()
        {
            InitializeMeta();
            SetAchievement();
            Assert.That(meta.MetaProgressService.TryCompleteEnding("test_ending"),Is.True);
            var view = Ref<VNAchievementsView>(bootstrap,"achievementsView");
            view.gameObject.SetActive(true);
            Invoke(bootstrap,"Initialize");
            Assert.That(bootstrap.IsInitialized,Is.True,bootstrap.LastDiagnostic);
            Assert.That(bootstrap.LastDiagnostic,Is.Null);
            var owners = OwnerFields.Select(n=>Get(bootstrap,n)).ToArray();
            Assert.That(owners,Has.None.Null);
            var progress = meta.MetaProgressService;
            Assert.That(progress.IsAchievementUnlocked("test_award"),Is.True);
            foreach(var owner in owners)
            {
                var progressField=owner.GetType().GetField("metaProgress",BindingFlags.Instance|BindingFlags.NonPublic);
                Assert.That(progressField,Is.Not.Null,owner.GetType().Name);
                Assert.That(progressField.GetValue(owner),Is.SameAs(progress));
            }
            Assert.That(Get(Ref<VNTimelineView>(bootstrap,"timelineView"),"runtime"),Is.SameAs(Get(bootstrap,"timelineRuntime")));
            Assert.That(Get(view,"achievementService"),Is.SameAs(Get(bootstrap,"achievementService")));
            var projections=(IReadOnlyList<VNAchievementProjection>)Get(view,"currentAchievements");
            Assert.That(projections.Single().IsUnlocked,Is.True);
            Assert.That(Get(Ref<VNReplayController>(bootstrap,"replayController"),"timelineRuntime"),Is.SameAs(Get(bootstrap,"timelineRuntime")));
            Assert.That(Get(Ref<VNTimelineView>(bootstrap,"timelineView"),"replayController"),Is.SameAs(Ref<VNReplayController>(bootstrap,"replayController")));
            Assert.That(writes,Is.EqualTo(2));
            Assert.That(typeof(VNRecordsRuntimeBootstrap).GetFields(BindingFlags.Instance|BindingFlags.NonPublic)
                .Any(f=>f.FieldType==typeof(VNMetaProgressService)||f.FieldType==typeof(VNMetaProgressRepository)),Is.False);
        }

        [Test]
        public void SameBootstrap_InitializesServicesAndEvaluatorExactlyOnce()
        {
            InitializeMeta();
            Invoke(bootstrap,"Initialize");
            var owners=OwnerFields.Select(n=>Get(bootstrap,n)).ToArray();
            var evaluator=Get(bootstrap,"achievementEvaluator");
            Assert.That(Get(evaluator,"evaluationEntryCountForTests"),Is.EqualTo(1));
            Invoke(bootstrap,"Initialize");
            CollectionAssert.AreEqual(owners,OwnerFields.Select(n=>Get(bootstrap,n)).ToArray());
            Assert.That(Get(evaluator,"evaluationEntryCountForTests"),Is.EqualTo(1));
            Assert.That(writes,Is.Zero);
        }

        [Test]
        public void FailedAttempt_DoesNotRetryAfterWiringIsRepaired()
        {
            var yarn=Ref<YarnProject>(bootstrap,"yarnProject");
            Set(bootstrap,"yarnProject",null);
            Invoke(bootstrap,"Initialize");
            var diagnostic=bootstrap.LastDiagnostic;
            Set(bootstrap,"yarnProject",yarn);
            InitializeMeta();
            Invoke(bootstrap,"Initialize");
            Assert.That(bootstrap.IsInitialized,Is.False);
            Assert.That(bootstrap.LastDiagnostic,Is.EqualTo(diagnostic));
            AssertNoOwners();
        }

        [Test]
        public void EvaluatorFailure_PreservesDiagnosticAndDoesNotInitializeViews()
        {
            InitializeMeta();
            SetAchievement();
            Assert.That(meta.MetaProgressService.TryCompleteEnding("test_ending"),Is.True);
            const string future="{\"schemaVersion\":2,\"futureField\":\"preserve\"}";
            File.WriteAllText(repository.CanonicalFilePath,future);
            Invoke(bootstrap,"Initialize");
            Assert.That(bootstrap.IsInitialized,Is.False);
            Assert.That(bootstrap.LastDiagnostic,Does.Contain("Could not persist derived Achievement"));
            Assert.That(Get(Ref<VNAchievementsView>(bootstrap,"achievementsView"),"initialized"),Is.False);
            AssertNoOwners();
            Assert.That(File.ReadAllText(repository.CanonicalFilePath),Is.EqualTo(future));
            var listeners=(Delegate)Get(meta.MetaProgressService,"ProgressChanged");
            Assert.That(listeners==null || listeners.GetInvocationList().All(d=>d.Target is not VNAchievementEvaluator),Is.True);
        }

        [Test]
        public void Destroy_DisposesOnlyTheOwnedEvaluatorAndRetainsM8AndSceneUi()
        {
            InitializeMeta();
            SetAchievement();
            Invoke(bootstrap,"Initialize");
            var evaluator=(VNAchievementEvaluator)Get(bootstrap,"achievementEvaluator");
            var progress=meta.MetaProgressService;
            Invoke(bootstrap,"OnDestroy");
            Invoke(bootstrap,"OnDestroy");
            Assert.That(evaluator.IsInitialized,Is.False);
            Assert.That(bootstrap.IsInitialized,Is.False);
            AssertNoOwners();
            Assert.That(meta.MetaProgressService,Is.SameAs(progress));
            Assert.That(meta.IsInitialized,Is.True);
            Assert.That(modal,Is.Not.Null);
            Assert.That(progress.TryCompleteEnding("test_ending"),Is.True);
            Assert.That(progress.IsAchievementUnlocked("test_award"),Is.False);
        }

        private static readonly string[] OwnerFields={"timelineService","timelineRuntime","galleryService","archiveService","achievementService","achievementEvaluator"};
        private void AssertNoOwners() { foreach(var name in OwnerFields) Assert.That(Get(bootstrap,name),Is.Null,name); }
        private void InitializeMeta() { Invoke(meta,"Initialize"); Assert.That(meta.IsInitialized,Is.True,meta.LastDiagnostic); }
        private void SetAchievement() => Set(achievements,"achievements",new List<VNAchievementDefinition>{
            new VNAchievementDefinition("test_award","Test award","Temporary fixture",0,false,VNAchievementConditionType.EndingCompleted,"test_ending")});
        private T Own<T>(T value) where T:Object { owned.Add(value); return value; }
        private static T Ref<T>(Object owner,string field) where T:Object => VNMainM9RecordsWiringAuditTests.Ref<T>(owner,field);
        private static object Get(object target,string field) { var info=target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic); Assert.That(info,Is.Not.Null,field); return info.GetValue(target); }
        private static void Set(object target,string field,object value)
        {
            var info=target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(info,Is.Not.Null,field);
            info.SetValue(target,value);
            // Match Unity's authored-asset edit lifecycle so cached catalog views are refreshed.
            if(target is ScriptableObject)
                target.GetType().GetMethod("OnValidate",BindingFlags.Instance|BindingFlags.NonPublic)?.Invoke(target,null);
        }
        private static void Invoke(object target,string method) => target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);
    }
}
