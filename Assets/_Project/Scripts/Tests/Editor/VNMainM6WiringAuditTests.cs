using System.Collections.Generic;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNMainM6WiringAuditTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/VN_Main.unity";
        private const string DialoguePanelPrefabPath = "Assets/_Project/Prefabs/UI/VNDialoguePanel.prefab";

        [Test]
        public void VNMain_UsesOneAuthoritativePresenterAndFinalM6Baseline()
        {
            using var scope = OpenScene();
            var runner = GetSingleComponent<DialogueRunner>(scope.Scene);
            var lifecycle = GetSingleComponent<VNLineLifecyclePresenter>(scope.Scene);
            var handler = GetSingleComponent<VNLineLifecycleMarkupHandler>(scope.Scene);
            var lineAdvancer = GetSingleComponent<LineAdvancer>(scope.Scene);
            var authoritative = GetSingleEnabledRunnerLinePresenter(runner);
            var serializedLifecyclePresenter = new SerializedObject(lifecycle)
                .FindProperty("linePresenter").objectReferenceValue as LinePresenter;
            var serializedAdvancerPresenter = new SerializedObject(lineAdvancer)
                .FindProperty("presenter").objectReferenceValue as LinePresenter;
            var oldDefault = FindOldDefaultLinePresenter(scope.Scene, authoritative);

            Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(authoritative.gameObject), Is.EqualTo(DialoguePanelPrefabPath));
            Assert.That(serializedLifecyclePresenter, Is.SameAs(authoritative));
            Assert.That(serializedAdvancerPresenter, Is.SameAs(authoritative));
            Assert.That(ContainsHandler(authoritative, handler), Is.True);
            Assert.That(CountHandlers(authoritative, handler), Is.EqualTo(1));
            Assert.That(oldDefault, Is.Not.Null);
            Assert.That(ContainsHandler(oldDefault, handler), Is.False);
            Assert.That(new SerializedObject(handler).FindProperty("enableM6TechnicalDiagnostics").boolValue, Is.False);
            Assert.That(new SerializedObject(runner).FindProperty("startNode").stringValue, Is.EqualTo("M2_UI_START"));
            Assert.That(lineAdvancer.GetComponent<VNLineAdvancerInputBridge>(), Is.Not.Null);
            Assert.That(new SerializedObject(lineAdvancer).FindProperty("inputMode").intValue, Is.EqualTo(4));
        }

        private static LinePresenter GetSingleEnabledRunnerLinePresenter(DialogueRunner runner)
        {
            var candidates = new List<LinePresenter>();
            foreach (var presenter in runner.DialoguePresenters)
            {
                if (presenter is LinePresenter line && line.isActiveAndEnabled && !candidates.Contains(line))
                    candidates.Add(line);
            }

            Assert.That(candidates, Has.Count.EqualTo(1), "DialogueRunner must have exactly one enabled distinct LinePresenter.");
            return candidates[0];
        }

        private static LinePresenter FindOldDefaultLinePresenter(Scene scene, LinePresenter authoritative)
        {
            foreach (var line in GetComponentsInScene<LinePresenter>(scene))
            {
                if (line != authoritative) return line;
            }
            return null;
        }

        private static bool ContainsHandler(LinePresenter presenter, VNLineLifecycleMarkupHandler handler)
        {
            return CountHandlers(presenter, handler) > 0;
        }

        private static int CountHandlers(LinePresenter presenter, VNLineLifecycleMarkupHandler handler)
        {
            if (presenter == null) return 0;
            var eventHandlers = new SerializedObject(presenter).FindProperty("eventHandlers");
            if (eventHandlers == null || !eventHandlers.isArray) return 0;

            var count = 0;
            for (var index = 0; index < eventHandlers.arraySize; index++)
            {
                if (eventHandlers.GetArrayElementAtIndex(index).objectReferenceValue == handler) count++;
            }
            return count;
        }

        private static T GetSingleComponent<T>(Scene scene) where T : Component
        {
            var matches = GetComponentsInScene<T>(scene);
            Assert.That(matches, Has.Count.EqualTo(1), $"Expected exactly one {typeof(T).Name} in {ScenePath}.");
            return matches[0];
        }

        private static List<T> GetComponentsInScene<T>(Scene scene) where T : Component
        {
            var matches = new List<T>();
            foreach (var root in scene.GetRootGameObjects())
                matches.AddRange(root.GetComponentsInChildren<T>(true));
            return matches;
        }

        private sealed class SceneScope : System.IDisposable
        {
            public Scene Scene { get; }
            private readonly bool openedForAudit;

            public SceneScope(Scene scene, bool openedForAudit)
            {
                Scene = scene;
                this.openedForAudit = openedForAudit;
            }

            public void Dispose()
            {
                if (openedForAudit) EditorSceneManager.CloseScene(Scene, true);
            }
        }

        private static SceneScope OpenScene()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (scene.IsValid() && scene.isLoaded) return new SceneScope(scene, false);
            return new SceneScope(EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive), true);
        }
    }
}
