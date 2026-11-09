using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M10ExactLineFixtureTests
    {
        private const string ProjectPath = "Assets/_Project/Tests/ExactLineFixture/ExactLineFixture.yarnproject";

        [UnityTest]
        public IEnumerator ResumeEntry_PresentsSameLineAndRunsSuccessorWithoutRepeatingPreLineEffects()
        {
            var project = AssetDatabase.LoadAssetAtPath<YarnProject>(ProjectPath);
            Assert.That(project, Is.Not.Null);
            Assert.That(project.Program, Is.Not.Null);
            var root = new GameObject("Exact-line fixture");
            root.SetActive(false);
            var storage = root.AddComponent<InMemoryVariableStorage>();
            var runner = root.AddComponent<DialogueRunner>();
            var presenter = root.AddComponent<ExactLineFixturePresenter>();
            runner.autoStart = false;
            runner.VariableStorage = storage;
            runner.SetProject(project);
            storage.Program = project.Program;
            runner.DialoguePresenters = new DialoguePresenterBase[] { presenter };
            var sfxCount = 0;
            runner.AddCommandHandler("exact_fixture_sfx", () => sfxCount++);
            root.SetActive(true);
            try
            {
                runner.StartDialogue("EXACT_FIXTURE_START").Forget();
                yield return WaitFor(() => presenter.LineIds.Count == 1, "initial line");
                Assert.That(presenter.LineIds[0], Is.EqualTo("line:exact_fixture_a"));
                Assert.That(storage.TryGetValue<float>("$exact_before", out var before), Is.True);
                Assert.That(before, Is.EqualTo(1));
                Assert.That(sfxCount, Is.EqualTo(1));

                // Model restoration of the saved variables, then start the real
                // runner at the line node. No previously executed command runs.
                var stop = runner.Stop();
                yield return WaitFor(() => stop.IsCompletedSuccessfully(), "runner stop and presenter quiescence");
                yield return WaitFor(() => presenter.ActiveLines == 0, "old presenter dismissal");
                yield return null;
                storage.SetValue("$exact_before", before);
                storage.SetValue("$exact_after", 0f);
                runner.StartDialogue("EXACT_FIXTURE_LINE_A").Forget();
                yield return WaitFor(() => presenter.LineIds.Count >= 2, "resumed line");
                Assert.That(presenter.LineIds.Count, Is.EqualTo(2), string.Join(", ", presenter.LineIds));
                Assert.That(presenter.LineIds[1], Is.EqualTo("line:exact_fixture_a"));
                Assert.That(sfxCount, Is.EqualTo(1));
                Assert.That(storage.TryGetValue<float>("$exact_before", out var restoredBefore), Is.True);
                Assert.That(restoredBefore, Is.EqualTo(1));
                Assert.That(storage.TryGetValue<float>("$exact_after", out var priorAfter), Is.True);
                Assert.That(priorAfter, Is.Zero);

                runner.RequestNextLine();
                yield return WaitFor(() => presenter.LineIds.Count == 3, "successor line");
                Assert.That(presenter.LineIds[2], Is.EqualTo("line:exact_fixture_b"));
                Assert.That(storage.TryGetValue<float>("$exact_after", out var after), Is.True);
                Assert.That(after, Is.EqualTo(1));
                Assert.That(sfxCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator TrackedNestedCommand_UnwindsAfterRunnerStopsWithoutExecutingLaterEffects()
        {
            var project = AssetDatabase.LoadAssetAtPath<YarnProject>(ProjectPath);
            var root = new GameObject("Exact-line cancellation fixture");
            root.SetActive(false);
            var storage = root.AddComponent<InMemoryVariableStorage>();
            var runner = root.AddComponent<DialogueRunner>();
            var presenter = root.AddComponent<ExactLineFixturePresenter>();
            runner.autoStart = false;
            runner.VariableStorage = storage;
            runner.SetProject(project);
            storage.Program = project.Program;
            runner.DialoguePresenters = new DialoguePresenterBase[] { presenter };
            root.SetActive(true);
            var sideEffects = 0;
            var disposed = false;
            IEnumerator Child()
            {
                try { yield return null; sideEffects++; }
                finally { disposed = true; }
            }
            IEnumerator Parent() { yield return Child(); }
            try
            {
                runner.StartDialogue("EXACT_FIXTURE_LINE_A").Forget();
                yield return WaitFor(() => presenter.LineIds.Count == 1, "fixture line");
                var tracked = VNAsyncCommandRegistry.Track(runner, Parent());
                Assert.That(tracked.MoveNext(), Is.True);
                Assert.That(VNAsyncCommandRegistry.IsIdle(runner), Is.False);
                var stop = runner.Stop();
                yield return WaitFor(() => stop.IsCompletedSuccessfully(), "runner stop");
                Assert.That(tracked.MoveNext(), Is.False);
                Assert.That(disposed, Is.True);
                Assert.That(sideEffects, Is.Zero);
                Assert.That(VNAsyncCommandRegistry.IsIdle(runner), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static IEnumerator WaitFor(Func<bool> condition, string stage)
        {
            var timeout = System.Diagnostics.Stopwatch.StartNew();
            while (timeout.Elapsed < TimeSpan.FromSeconds(5))
            {
                if (condition()) yield break;
                yield return null;
            }
            Assert.Fail("DialogueRunner did not reach fixture stage: " + stage);
        }
    }

    public sealed class ExactLineFixturePresenter : DialoguePresenterBase
    {
        public readonly List<string> LineIds = new();
        public int ActiveLines { get; private set; }

        public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
        public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;

        public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            ActiveLines++;
            LineIds.Add(line.TextID);
            var completion = new YarnTaskCompletionSource();
            using var registration = token.NextContentToken.Register(() => completion.TrySetResult());
            try
            {
                await completion.Task;
                // A real presenter dismisses asynchronously. Yield so the
                // runner can finish cancelling dialogue before it resumes.
                await System.Threading.Tasks.Task.Yield();
            }
            finally { ActiveLines--; }
        }
    }
}
