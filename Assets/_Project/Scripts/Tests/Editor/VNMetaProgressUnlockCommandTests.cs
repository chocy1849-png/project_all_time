using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using UnityEngine;
using UnityEngine.TestTools;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNMetaProgressUnlockCommandTests
    {
        private static readonly string[] CommandNames =
        {
            "vn_unlock_cg", "vn_unlock_chapter", "vn_unlock_archive", "vn_unlock_achievement", "vn_complete_ending",
        };

        private readonly List<UnityEngine.Object> ownedObjects = new();
        private string temporaryRoot;
        private VNMetaProgressRepository repository;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M8UnlockCommandTests_" + Guid.NewGuid().ToString("N"));
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, true);
        }

        [TestCase("cg")]
        [TestCase("chapter")]
        [TestCase("archive")]
        [TestCase("achievement")]
        [TestCase("ending")]
        public void ServiceQueries_RespectPersistenceFirstOrdinalSetSemantics(string category)
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = LoadService(repository);
            var operation = GetOperation(service, category);

            Assert.That(operation.Mutate("id_a"), Is.True);
            Assert.That(operation.Query("id_a"), Is.True);
            Assert.That(operation.Mutate("id_a"), Is.True);
            Assert.That(writes, Is.EqualTo(1), "Duplicate mutation must not write again.");
            Assert.That(operation.Mutate("ID_A"), Is.True);
            Assert.That(operation.Query("ID_A"), Is.True);
            Assert.That(writes, Is.EqualTo(2), "Ordinal identity keeps case variants distinct.");

            foreach (var invalidId in new[] { null, string.Empty, "   " })
            {
                Assert.That(operation.Mutate(invalidId), Is.False);
                Assert.That(operation.Query(invalidId), Is.False);
            }
            Assert.That(writes, Is.EqualTo(2));

            var failedRoot = Path.Combine(temporaryRoot, "failure_" + category);
            var failedRepository = VNMetaProgressRepository.CreateForTesting(failedRoot);
            var failedService = LoadService(failedRepository);
            Directory.CreateDirectory(failedRepository.CanonicalFilePath);
            var failedOperation = GetOperation(failedService, category);
            Assert.That(failedOperation.Mutate("will_fail"), Is.False);
            Assert.That(failedOperation.Query("will_fail"), Is.False);
        }

        [Test]
        public void Commands_RegisterExactNamesDispatchToCorrectCollectionsAndDoNotTouchReadHistory()
        {
            var writes = 0;
            repository = VNMetaProgressRepository.CreateForTesting(temporaryRoot, () => writes++);
            var service = LoadService(repository);
            var runner = CreateRunner();
            using var commands = new VNYarnMetaProgressCommands(runner, service);

            commands.Register();
            commands.Register();
            Assert.That(commands.IsRegistered, Is.True);
            var names = RegisteredCommandNames(runner);
            CollectionAssert.IsSubsetOf(CommandNames, names);
            Assert.That(names, Does.Not.Contain("vn_unlock_ending"));
            Assert.That(names, Does.Not.Contain("vn_mark_read"));

            Dispatch(runner, "vn_unlock_cg cg_a");
            Dispatch(runner, "vn_unlock_chapter chapter_a");
            Dispatch(runner, "vn_unlock_archive archive_a");
            Dispatch(runner, "vn_unlock_achievement achievement_a");
            Dispatch(runner, "vn_complete_ending ending_a");
            Assert.That(service.IsCGUnlocked("cg_a"), Is.True);
            Assert.That(service.IsChapterUnlocked("chapter_a"), Is.True);
            Assert.That(service.IsArchiveEntryUnlocked("archive_a"), Is.True);
            Assert.That(service.IsAchievementUnlocked("achievement_a"), Is.True);
            Assert.That(service.IsEndingCompleted("ending_a"), Is.True);
            Assert.That(service.Current.readLineIds, Is.Empty);
            Assert.That(writes, Is.EqualTo(5));

            Dispatch(runner, "vn_unlock_cg cg_a");
            Assert.That(writes, Is.EqualTo(5));
        }

        [Test]
        public void Commands_InvalidAndWriteFailureLogDiagnosticsWithoutFalseUnlocks()
        {
            var service = LoadService(repository);
            var runner = CreateRunner();
            using var commands = new VNYarnMetaProgressCommands(runner, service);
            commands.Register();

            LogAssert.Expect(LogType.Error, new Regex("MetaProgress CG command 'vn_unlock_cg' rejected ID"));
            Dispatch(runner, "vn_unlock_cg \"   \"");
            Assert.That(service.IsCGUnlocked("   "), Is.False);

            var failureRoot = Path.Combine(temporaryRoot, "write_failure");
            var failureRepository = VNMetaProgressRepository.CreateForTesting(failureRoot);
            var failureService = LoadService(failureRepository);
            Directory.CreateDirectory(failureRepository.CanonicalFilePath);
            var failureRunner = CreateRunner();
            using var failureCommands = new VNYarnMetaProgressCommands(failureRunner, failureService);
            failureCommands.Register();
            LogAssert.Expect(LogType.Error, new Regex("MetaProgress achievement command 'vn_unlock_achievement' rejected ID 'achievement_fail'"));
            Dispatch(failureRunner, "vn_unlock_achievement achievement_fail");
            Assert.That(failureService.IsAchievementUnlocked("achievement_fail"), Is.False);
        }

        [Test]
        public void Dispose_RemovesOnlyOwnedHandlersAndIsIdempotent()
        {
            var service = LoadService(repository);
            var runner = CreateRunner();
            runner.AddCommandHandler<string>("unrelated_command", _ => { });
            var commands = new VNYarnMetaProgressCommands(runner, service);
            commands.Register();

            commands.Dispose();
            commands.Dispose();
            Assert.That(commands.IsRegistered, Is.False);
            var names = RegisteredCommandNames(runner);
            foreach (var commandName in CommandNames) Assert.That(names, Does.Not.Contain(commandName));
            Assert.That(names, Does.Contain("unrelated_command"));
        }

        [Test]
        public void ReadAndUnlockAuthoritiesRemainIndependent()
        {
            var service = LoadService(repository);
            Assert.That(service.TryRecordReadLine("line_read"), Is.True);
            Assert.That(service.IsLineRead("line_read"), Is.True);
            Assert.That(service.TryUnlockCG("cg_only"), Is.True);
            Assert.That(service.TryUnlockChapter("chapter_only"), Is.True);
            Assert.That(service.TryUnlockArchiveEntry("archive_only"), Is.True);
            Assert.That(service.TryUnlockAchievement("achievement_only"), Is.True);
            Assert.That(service.TryCompleteEnding("ending_only"), Is.True);
            CollectionAssert.AreEqual(new[] { "line_read" }, service.Current.readLineIds);
            Assert.That(service.IsCGUnlocked("line_read"), Is.False);
            Assert.That(service.IsEndingCompleted("line_read"), Is.False);
        }

        private VNMetaProgressService LoadService(VNMetaProgressRepository targetRepository)
        {
            var service = new VNMetaProgressService(targetRepository);
            service.Load();
            return service;
        }

        private DialogueRunner CreateRunner()
        {
            var gameObject = new GameObject("M8-05 Yarn Command Runner");
            ownedObjects.Add(gameObject);
            return gameObject.AddComponent<DialogueRunner>();
        }

        private static string[] RegisteredCommandNames(DialogueRunner runner)
        {
            var dispatcher = GetDispatcher(runner);
            var commandsProperty = dispatcher.GetType().GetProperty("Commands", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(commandsProperty, Is.Not.Null);
            var registered = (System.Collections.IEnumerable)commandsProperty.GetValue(dispatcher);
            return registered.Cast<ICommand>().Select(command => command.Name).ToArray();
        }

        private static void Dispatch(DialogueRunner runner, string command)
        {
            var dispatcher = GetDispatcher(runner);
            var dispatch = dispatcher.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name.EndsWith(".DispatchCommand", StringComparison.Ordinal) &&
                                           method.GetParameters().Length == 2);
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

        private static Operation GetOperation(VNMetaProgressService service, string category)
        {
            switch (category)
            {
                case "cg": return new Operation(service.TryUnlockCG, service.IsCGUnlocked);
                case "chapter": return new Operation(service.TryUnlockChapter, service.IsChapterUnlocked);
                case "archive": return new Operation(service.TryUnlockArchiveEntry, service.IsArchiveEntryUnlocked);
                case "achievement": return new Operation(service.TryUnlockAchievement, service.IsAchievementUnlocked);
                case "ending": return new Operation(service.TryCompleteEnding, service.IsEndingCompleted);
                default: throw new ArgumentOutOfRangeException(nameof(category));
            }
        }

        private sealed class Operation
        {
            public readonly Func<string, bool> Mutate;
            public readonly Func<string, bool> Query;

            public Operation(Func<string, bool> mutate, Func<string, bool> query)
            {
                Mutate = mutate;
                Query = query;
            }
        }
    }
}
