using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Settings;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNMetaProgressRuntimeBootstrapTests
    {
        private static readonly string[] CommandNames =
        {
            "vn_unlock_cg", "vn_unlock_chapter", "vn_unlock_archive", "vn_unlock_achievement", "vn_complete_ending",
        };

        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M8RuntimeBootstrapTests_" + Guid.NewGuid().ToString("N"));
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
        public void ExecutionOrder_LeavesSettingsAtMinusOneAndMetaProgressAtMinusTwo()
        {
            Assert.That(GetExecutionOrder(typeof(VNMetaProgressRuntimeBootstrap)), Is.EqualTo(-2));
            Assert.That(GetExecutionOrder(typeof(VNSettingsRuntimeBootstrap)), Is.EqualTo(-1));
        }

        [Test]
        public void MissingWiring_FailsBeforeRepositoryCreation()
        {
            var repositoryCreations = 0;
            var missingRunner = CreateBootstrap(withSessionState: true, withRunner: false);
            SetRepositoryFactory(missingRunner, () =>
            {
                repositoryCreations++;
                return VNMetaProgressRepository.CreateForTesting(temporaryRoot);
            });
            InvokeInitialize(missingRunner);
            Assert.That(missingRunner.IsInitialized, Is.False);
            Assert.That(missingRunner.LastDiagnostic, Does.Contain("Dialogue Runner"));
            Assert.That(repositoryCreations, Is.Zero);

            var missingSession = CreateBootstrap(withSessionState: false, withRunner: true);
            SetRepositoryFactory(missingSession, () =>
            {
                repositoryCreations++;
                return VNMetaProgressRepository.CreateForTesting(temporaryRoot);
            });
            InvokeInitialize(missingSession);
            Assert.That(missingSession.IsInitialized, Is.False);
            Assert.That(missingSession.LastDiagnostic, Does.Contain("VNDialogueSessionState"));
            Assert.That(repositoryCreations, Is.Zero);
        }

        [Test]
        public void SuccessfulComposition_LoadsSeedsSubscribesAndRegistersCommandsBeforeUse()
        {
            var repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot);
            var data = VNMetaProgressDefaults.CreateDefault();
            data.readLineIds = new[] { "already_read" };
            Assert.That(repository.Write(data).Succeeded, Is.True);
            var bootstrap = CreateBootstrap(withSessionState: true, withRunner: true);
            SetRepositoryFactory(bootstrap, () => repository);
            var sessionState = bootstrap.GetComponent<VNDialogueSessionState>();
            var runner = GetRunner(bootstrap);

            InvokeInitialize(bootstrap);
            Assert.That(bootstrap.IsInitialized, Is.True, bootstrap.LastDiagnostic);
            Assert.That(bootstrap.MetaProgressService, Is.Not.Null);
            Assert.That(sessionState.ReadHistory.IsRead("already_read"), Is.True);
            CollectionAssert.IsSubsetOf(CommandNames, RegisteredCommandNames(runner));

            Dispatch(runner, "vn_unlock_cg cg_bootstrap");
            Assert.That(bootstrap.MetaProgressService.IsCGUnlocked("cg_bootstrap"), Is.True);
            PresentAndConsume(sessionState, "new_read");
            Assert.That(bootstrap.MetaProgressService.IsLineRead("new_read"), Is.True);
            Assert.That(sessionState.ReadHistory.IsRead("new_read"), Is.True);
        }

        [Test]
        public void Initialization_IsStableAndMissingFileDoesNotCreateStorage()
        {
            var repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot);
            var bootstrap = CreateBootstrap(withSessionState: true, withRunner: true);
            SetRepositoryFactory(bootstrap, () => repository);
            InvokeInitialize(bootstrap);
            var service = bootstrap.MetaProgressService;
            var bridge = GetPrivateField(bootstrap, "readHistoryBridge");
            var commands = GetPrivateField(bootstrap, "yarnCommands");
            InvokeInitialize(bootstrap);

            Assert.That(bootstrap.IsInitialized, Is.True);
            Assert.That(bootstrap.MetaProgressService, Is.SameAs(service));
            Assert.That(GetPrivateField(bootstrap, "readHistoryBridge"), Is.SameAs(bridge));
            Assert.That(GetPrivateField(bootstrap, "yarnCommands"), Is.SameAs(commands));
            Assert.That(Directory.Exists(temporaryRoot), Is.False);
            Assert.That(File.Exists(repository.CanonicalFilePath), Is.False);
        }

        [Test]
        public void FutureSchemaAndRecoverableCorruption_ComposeSafelyWithDiagnostics()
        {
            var futureRoot = Path.Combine(temporaryRoot, "future");
            var futureRepository = VNMetaProgressRepository.CreateForTesting(futureRoot);
            WriteRaw(futureRepository, "{\"schemaVersion\":2,\"future\":\"preserve\"}");
            var futureBootstrap = CreateBootstrap(withSessionState: true, withRunner: true);
            SetRepositoryFactory(futureBootstrap, () => futureRepository);
            InvokeInitialize(futureBootstrap);
            Assert.That(futureBootstrap.IsInitialized, Is.True);
            Assert.That(futureBootstrap.MetaProgressService.IsWriteProtected, Is.True);
            Assert.That(futureBootstrap.LastDiagnostic, Is.Not.Empty);
            Assert.That(futureBootstrap.MetaProgressService.TryUnlockCG("blocked"), Is.False);

            var corruptRoot = Path.Combine(temporaryRoot, "corrupt");
            var corruptRepository = VNMetaProgressRepository.CreateForTesting(corruptRoot);
            WriteRaw(corruptRepository, "{ malformed");
            var corruptBootstrap = CreateBootstrap(withSessionState: true, withRunner: true);
            SetRepositoryFactory(corruptBootstrap, () => corruptRepository);
            InvokeInitialize(corruptBootstrap);
            Assert.That(corruptBootstrap.IsInitialized, Is.True);
            Assert.That(corruptBootstrap.MetaProgressService.CanWrite, Is.True);
            Assert.That(corruptBootstrap.LastDiagnostic, Is.Not.Empty);
            Assert.That(Directory.GetFiles(corruptRoot, "meta_progress.json.*.corrupt"), Has.Length.EqualTo(1));
        }

        [Test]
        public void CommandCollision_RollsBackBridgeAndLeavesNoPartialM8Commands()
        {
            var repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot);
            var bootstrap = CreateBootstrap(withSessionState: true, withRunner: true);
            var runner = GetRunner(bootstrap);
            runner.AddCommandHandler<string>("vn_unlock_cg", _ => { });
            SetRepositoryFactory(bootstrap, () => repository);

            InvokeInitialize(bootstrap);
            Assert.That(bootstrap.IsInitialized, Is.False);
            Assert.That(bootstrap.MetaProgressService, Is.Null);
            var names = RegisteredCommandNames(runner);
            Assert.That(names, Does.Contain("vn_unlock_cg"));
            foreach (var commandName in CommandNames.Skip(1)) Assert.That(names, Does.Not.Contain(commandName));

            var sessionState = bootstrap.GetComponent<VNDialogueSessionState>();
            PresentAndConsume(sessionState, "unbridged");
            Assert.That(sessionState.ReadHistory.IsRead("unbridged"), Is.True);
            Assert.That(repository.Read().State, Is.EqualTo(VNMetaProgressStorageState.Missing));
        }

        [Test]
        public void Destruction_RemovesOwnedHandlersAndUnsubscribesBridge_AndIsSafeBeforeComposition()
        {
            var repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot);
            var bootstrap = CreateBootstrap(withSessionState: true, withRunner: true);
            SetRepositoryFactory(bootstrap, () => repository);
            var runner = GetRunner(bootstrap);
            var sessionState = bootstrap.GetComponent<VNDialogueSessionState>();
            InvokeInitialize(bootstrap);
            var service = bootstrap.MetaProgressService;

            InvokeOnDestroy(bootstrap);
            InvokeOnDestroy(bootstrap);
            Assert.That(bootstrap.IsInitialized, Is.False);
            foreach (var commandName in CommandNames) Assert.That(RegisteredCommandNames(runner), Does.Not.Contain(commandName));
            PresentAndConsume(sessionState, "after_destroy");
            Assert.That(service.IsLineRead("after_destroy"), Is.False);

            var uninitialized = CreateBootstrap(withSessionState: false, withRunner: false);
            Assert.DoesNotThrow(() => InvokeOnDestroy(uninitialized));
            Assert.DoesNotThrow(() => InvokeOnDestroy(uninitialized));
        }

        private VNMetaProgressRuntimeBootstrap CreateBootstrap(bool withSessionState, bool withRunner)
        {
            var host = new GameObject("M8-06 MetaProgress Bootstrap");
            ownedObjects.Add(host);
            if (withSessionState) host.AddComponent<VNDialogueSessionState>();
            var bootstrap = host.AddComponent<VNMetaProgressRuntimeBootstrap>();
            if (withRunner)
            {
                var runnerObject = new GameObject("M8-06 Dialogue Runner");
                ownedObjects.Add(runnerObject);
                SetPrivateField(bootstrap, "dialogueRunner", runnerObject.AddComponent<DialogueRunner>());
            }

            return bootstrap;
        }

        private static int GetExecutionOrder(Type type)
        {
            var attribute = (DefaultExecutionOrder)Attribute.GetCustomAttribute(type, typeof(DefaultExecutionOrder));
            Assert.That(attribute, Is.Not.Null);
            return attribute.order;
        }

        private static void SetRepositoryFactory(VNMetaProgressRuntimeBootstrap bootstrap, Func<VNMetaProgressRepository> factory)
        {
            SetPrivateField(bootstrap, "repositoryFactory", factory);
        }

        private static DialogueRunner GetRunner(VNMetaProgressRuntimeBootstrap bootstrap)
        {
            return (DialogueRunner)GetPrivateField(bootstrap, "dialogueRunner");
        }

        private static void InvokeInitialize(VNMetaProgressRuntimeBootstrap bootstrap) => InvokePrivate(bootstrap, "Initialize");
        private static void InvokeOnDestroy(VNMetaProgressRuntimeBootstrap bootstrap) => InvokePrivate(bootstrap, "OnDestroy");

        private static void PresentAndConsume(VNDialogueSessionState sessionState, string lineId)
        {
            var line = new LocalizedLine
            {
                TextID = lineId,
                Text = new MarkupParseResult("Test", new List<MarkupAttribute>()),
            };
            var begin = typeof(VNDialogueSessionState).GetMethod("BeginLine", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(begin, Is.Not.Null);
            begin.Invoke(sessionState, new object[] { line });
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
            var fullDisplay = typeof(VNDialogueSessionState).GetMethod("TryRecordFullDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(fullDisplay, Is.Not.Null);
            Assert.That((bool)fullDisplay.Invoke(sessionState, new object[] { sessionState.CurrentPresentationOccurrence, false }), Is.True);
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.True);
        }

        private static string[] RegisteredCommandNames(DialogueRunner runner)
        {
            var dispatcher = GetDispatcher(runner);
            var commandsProperty = dispatcher.GetType().GetProperty("Commands", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(commandsProperty, Is.Not.Null);
            var registered = (IEnumerable)commandsProperty.GetValue(dispatcher);
            return registered.Cast<ICommand>().Select(command => command.Name).ToArray();
        }

        private static void Dispatch(DialogueRunner runner, string command)
        {
            var dispatcher = GetDispatcher(runner);
            var dispatch = dispatcher.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name.EndsWith(".DispatchCommand", StringComparison.Ordinal) && method.GetParameters().Length == 2);
            Assert.That(dispatch, Is.Not.Null);
            dispatch.Invoke(dispatcher, new object[] { command, runner });
        }

        private static object GetDispatcher(DialogueRunner runner)
        {
            var property = typeof(DialogueRunner).GetProperty("CommandDispatcher", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null);
            var dispatcher = property.GetValue(runner);
            Assert.That(dispatcher, Is.Not.Null);
            return dispatcher;
        }

        private static void WriteRaw(VNMetaProgressRepository repository, string contents)
        {
            Directory.CreateDirectory(repository.StorageRoot);
            File.WriteAllText(repository.CanonicalFilePath, contents);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            return field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }
    }
}
