using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNMetaProgressReadHistoryIntegrationTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;
        private VNDialogueSessionState sessionState;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M8ReadHistoryTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot);
            sessionState = CreateSessionState("M8-04 Session A");
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [Test]
        public void PersistentSeed_IsEffectiveOrdinalUnionAndDoesNotClearSessionOverlay()
        {
            var history = sessionState.ReadHistory;
            PresentAndFullyDisplay("session");
            Assert.That(ConsumeCurrentLine(), Is.True);
            Assert.That(history.ReplacePersistentBaseline(new[] { "b", "A", "a", "b" }), Is.True);
            Assert.That(history.IsRead("session"), Is.True);
            Assert.That(history.IsRead("A"), Is.True);
            Assert.That(history.IsRead("a"), Is.True);
            Assert.That(history.Count, Is.EqualTo(4));
            CollectionAssert.AreEqual(new[] { "A", "a", "b", "session" }, history.Snapshot());

            Assert.That(history.ReplacePersistentBaseline(null), Is.False);
            Assert.That(history.ReplacePersistentBaseline(new string[] { null }), Is.False);
            Assert.That(history.ReplacePersistentBaseline(new[] { "valid", "  " }), Is.False);
            Assert.That(history.IsRead("A"), Is.True, "Rejected replacement must not partially clear the baseline.");
        }

        [Test]
        public void PersistentSeed_AndReadOnlySkipTreatPersistedIdsAsRead()
        {
            SaveReadLine("persisted");
            var service = LoadService();
            using var bridge = new VNPersistentReadHistoryBridge(sessionState, service);
            Assert.That(bridge.Initialize(), Is.True);
            Assert.That(sessionState.ReadHistory.IsRead("persisted"), Is.True);

            var convenience = CreateConvenienceRuntime();
            Present("persisted");
            convenience.SetSkipEnabled(true);
            TickSkip(convenience, 1);
            Assert.That(convenience.IsSkipEnabled, Is.True);

            sessionState.InvalidateTransientPresentation();
            Present("unread");
            convenience.SetSkipEnabled(true);
            TickSkip(convenience, 2);
            Assert.That(convenience.IsSkipEnabled, Is.False);
        }

        [Test]
        public void AuthorizedConsume_PersistsThenPromotesAndClearSessionRetainsDurableRead()
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = LoadService();
            using var bridge = new VNPersistentReadHistoryBridge(sessionState, service);
            Assert.That(bridge.Initialize(), Is.True);

            PresentAndFullyDisplay("line_a");
            Assert.That(writes, Is.EqualTo(0), "Full display alone is not read or persisted.");
            Assert.That(ConsumeCurrentLine(), Is.True);
            Assert.That(writes, Is.EqualTo(1));
            CollectionAssert.AreEqual(new[] { "line_a" }, service.Current.readLineIds);
            Assert.That(sessionState.ReadHistory.IsRead("line_a"), Is.True);

            sessionState.ClearSession();
            Assert.That(sessionState.Backlog.Count, Is.Zero);
            Assert.That(sessionState.ReadHistory.IsRead("line_a"), Is.True);
            Assert.That(sessionState.ReadHistory.Count, Is.EqualTo(1));
        }

        [Test]
        public void AuthorizedSessionRead_RemainsOverlayOnlyUntilTheBridgePersistsIt()
        {
            var service = LoadService();
            PresentAndFullyDisplay("overlay_only");
            Assert.That(ConsumeCurrentLine(), Is.True);

            Assert.That(sessionState.ReadHistory.IsRead("overlay_only"), Is.True);
            Assert.That(service.Current.readLineIds, Is.Empty);
            sessionState.ClearSession();
            Assert.That(sessionState.ReadHistory.IsRead("overlay_only"), Is.False);
        }

        [Test]
        public void PersistenceFailure_LeavesReadSessionOnlyUntilClearSession()
        {
            var service = LoadService();
            Directory.CreateDirectory(repository.CanonicalFilePath);
            using var bridge = new VNPersistentReadHistoryBridge(sessionState, service);
            Assert.That(bridge.Initialize(), Is.True);

            PresentAndFullyDisplay("unsaved");
            Assert.That(ConsumeCurrentLine(), Is.True);
            Assert.That(sessionState.ReadHistory.IsRead("unsaved"), Is.True);
            Assert.That(service.Current.readLineIds, Is.Empty);
            Assert.That(bridge.LastDiagnostic, Is.Not.Empty);

            sessionState.ClearSession();
            Assert.That(sessionState.ReadHistory.IsRead("unsaved"), Is.False);
        }

        [Test]
        public void DuplicateOccurrence_DoesNotEmitAnotherPersistenceWrite()
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = LoadService();
            using var bridge = new VNPersistentReadHistoryBridge(sessionState, service);
            bridge.Initialize();

            PresentAndFullyDisplay("repeat");
            ConsumeCurrentLine();
            sessionState.InvalidateTransientPresentation();
            PresentAndFullyDisplay("repeat");
            ConsumeCurrentLine();

            Assert.That(writes, Is.EqualTo(1));
            CollectionAssert.AreEqual(new[] { "repeat" }, service.Current.readLineIds);
        }

        [Test]
        public void Restart_ReseedsDurableReadWithoutSecondConsume()
        {
            var serviceA = LoadService();
            using (var bridgeA = new VNPersistentReadHistoryBridge(sessionState, serviceA))
            {
                bridgeA.Initialize();
                PresentAndFullyDisplay("restart_line");
                ConsumeCurrentLine();
            }

            var sessionB = CreateSessionState("M8-04 Session B");
            var serviceB = LoadService();
            using var bridgeB = new VNPersistentReadHistoryBridge(sessionB, serviceB);
            Assert.That(bridgeB.Initialize(), Is.True);
            Assert.That(sessionB.ReadHistory.IsRead("restart_line"), Is.True);
        }

        [Test]
        public void OptionsAndInvalidOrFullDisplayOnlyPaths_DoNotPersist()
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = LoadService();
            using var bridge = new VNPersistentReadHistoryBridge(sessionState, service);
            bridge.Initialize();

            PresentAndFullyDisplay("full_only");
            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));
            Assert.That(writes, Is.Zero);
            InvokeBeginOptions();
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.False);
            Assert.That(writes, Is.Zero);

            PresentAndFullyDisplay(" ");
            Assert.That(ConsumeCurrentLine(), Is.False);
            Assert.That(writes, Is.Zero);
            Assert.That(service.Current.readLineIds, Is.Empty);
        }

        [Test]
        public void SameFrameAdvanceRejection_DoesNotPersist()
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = LoadService();
            using var bridge = new VNPersistentReadHistoryBridge(sessionState, service);
            bridge.Initialize();
            CreateConvenienceRuntime();
            var advanceBridge = sessionState.GetComponent<VNLineAdvancerInputBridge>();

            PresentAndFullyDisplay("same_frame");
            SetPrivateField(sessionState, "currentPresentationStartedFrame", Time.frameCount);
            Assert.That(advanceBridge.TryAdvance(VNAdvanceSource.Manual), Is.False);
            Assert.That(writes, Is.Zero);
            Assert.That(service.Current.readLineIds, Is.Empty);
        }

        [Test]
        public void M5TransientInvalidation_DoesNotRollBackPersistentBaselineOrMetaProgress()
        {
            SaveReadLine("durable");
            var service = LoadService();
            using var bridge = new VNPersistentReadHistoryBridge(sessionState, service);
            bridge.Initialize();
            PresentAndFullyDisplay("transient");

            sessionState.InvalidateTransientPresentation();
            Assert.That(sessionState.ReadHistory.IsRead("durable"), Is.True);
            CollectionAssert.AreEqual(new[] { "durable" }, service.Current.readLineIds);
        }

        [Test]
        public void InitializationIsIdempotent_AndDisposalStopsWrites()
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = LoadService();
            var bridge = new VNPersistentReadHistoryBridge(sessionState, service);
            Assert.That(bridge.Initialize(), Is.True);
            Assert.That(bridge.Initialize(), Is.True);
            PresentAndFullyDisplay("once");
            ConsumeCurrentLine();
            Assert.That(writes, Is.EqualTo(1));

            bridge.Dispose();
            bridge.Dispose();
            sessionState.InvalidateTransientPresentation();
            PresentAndFullyDisplay("after_dispose");
            ConsumeCurrentLine();
            Assert.That(writes, Is.EqualTo(1));
            Assert.That(service.Current.readLineIds, Does.Not.Contain("after_dispose"));
        }

        private VNMetaProgressService LoadService()
        {
            var service = new VNMetaProgressService(repository);
            service.Load();
            return service;
        }

        private void SaveReadLine(string lineId)
        {
            var data = VNMetaProgressDefaults.CreateDefault();
            data.readLineIds = new[] { lineId };
            Assert.That(repository.Write(data).Succeeded, Is.True);
        }

        private VNDialogueSessionState CreateSessionState(string name)
        {
            var gameObject = new GameObject(name);
            ownedObjects.Add(gameObject);
            return gameObject.AddComponent<VNDialogueSessionState>();
        }

        private VNConvenienceController CreateConvenienceRuntime()
        {
            var host = sessionState.gameObject;
            var lineAdvancer = host.AddComponent<LineAdvancer>();
            lineAdvancer.enabled = false;
            var gate = host.AddComponent<VNInteractionGate>();
            var advanceBridge = host.AddComponent<VNLineAdvancerInputBridge>();
            var convenience = host.AddComponent<VNConvenienceController>();
            SetPrivateField(gate, "sessionState", sessionState);
            SetPrivateField(advanceBridge, "sessionState", sessionState);
            SetPrivateField(advanceBridge, "interactionGate", gate);
            SetPrivateField(convenience, "sessionState", sessionState);
            SetPrivateField(convenience, "advanceBridge", advanceBridge);
            SetPrivateField(convenience, "interactionGate", gate);
            return convenience;
        }

        private void Present(string lineId)
        {
            var line = new LocalizedLine
            {
                TextID = lineId,
                Text = new MarkupParseResult("Test", new List<MarkupAttribute>()),
            };
            Invoke(sessionState, "BeginLine", line);
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
        }

        private void PresentAndFullyDisplay(string lineId)
        {
            Present(lineId);
            var occurrence = sessionState.CurrentPresentationOccurrence;
            Assert.That((bool)Invoke(sessionState, "TryRecordFullDisplay", occurrence, false), Is.True);
        }

        private bool ConsumeCurrentLine() => sessionState.TryAuthorizeCurrentLineConsume();

        private void InvokeBeginOptions() => Invoke(sessionState, "BeginOptions");

        private void TickSkip(VNConvenienceController controller, int frameCount)
        {
            Invoke(controller, "TickSkip", sessionState.CurrentPresentationOccurrence, 1f, frameCount);
        }

        private static object Invoke(object target, string methodName, params object[] arguments)
        {
            var argumentTypes = new Type[arguments.Length];
            for (var index = 0; index < arguments.Length; index++) argumentTypes[index] = arguments[index].GetType();
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, argumentTypes, null);
            Assert.That(method, Is.Not.Null, methodName);
            return method.Invoke(target, arguments);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }
    }
}
