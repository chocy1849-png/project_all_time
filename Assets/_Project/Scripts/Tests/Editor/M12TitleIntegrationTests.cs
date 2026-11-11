using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Title;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Settings;
using Object = UnityEngine.Object;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M12TitleIntegrationTests
    {
        private const string RecoveryRootVariable = "PROJECT_ALL_TIME_M12_RECOVERY_ROOT";
        private static bool recoveryFinished;
        private static Exception recoveryFailure;
        private static bool oldRunInBackground;

        [UnityTest]
        public IEnumerator M12_EmptyScene_PlayModeLifecycleCompletes()
        {
            Debug.Log("[M12-LIFECYCLE] opening empty scene");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            Debug.Log("[M12-LIFECYCLE] entered play: " + Application.isPlaying);
            Assert.That(Application.isPlaying, Is.True);
            yield return new ExitPlayMode();
            Debug.Log("[M12-LIFECYCLE] exited play: " + Application.isPlaying);
            Assert.That(Application.isPlaying, Is.False);
        }

        [TearDown]
        public void CleanRecoveryRoot()
        {
            var root = Environment.GetEnvironmentVariable(RecoveryRootVariable);
            if (string.IsNullOrEmpty(root)) return;
            Application.runInBackground = oldRunInBackground;
            Environment.SetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT", null);
            Environment.SetEnvironmentVariable(RecoveryRootVariable, null);
            if (root.StartsWith(Path.Combine(Path.GetTempPath(), "M12_recovery_"), StringComparison.OrdinalIgnoreCase) && Directory.Exists(root))
                Directory.Delete(root, true);
        }
        [Test]
        public void Settings_DefaultRepositoryUsesIsolatedEditorRoot()
        {
            var old = Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
            var root = Path.Combine(Path.GetTempPath(), "M12_settings_" + Guid.NewGuid().ToString("N"));
            try
            {
                Environment.SetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT", root);
                var isolated = new VNSettingsRepository();
                Assert.That(isolated.StorageRoot, Is.EqualTo(Path.Combine(root, "Settings")));
                Assert.That(isolated.StorageRoot, Is.Not.EqualTo(VNSettingsRepository.ProductionStorageRoot));
            }
            finally { Environment.SetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT", old); }
        }

        [UnityTest]
        public IEnumerator ProductionTitle_RecoversAfterAcceptedLoadFails()
        {
            Assert.That(Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT"), Is.Null.Or.Empty, "Recovery test requires an unused isolation override.");
            var recoveryRoot = Path.Combine(Path.GetTempPath(), "M12_recovery_" + Guid.NewGuid().ToString("N"));
            Environment.SetEnvironmentVariable(RecoveryRootVariable, recoveryRoot);
            Environment.SetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT", recoveryRoot);
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/VN_Main.unity", OpenSceneMode.Single);
            yield return new EnterPlayMode();
            oldRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            recoveryFinished = false;
            recoveryFailure = null;
            var host = new GameObject("M12 persistent test host").AddComponent<VNInteractionGate>();
            Object.DontDestroyOnLoad(host.gameObject);
            host.StartCoroutine(RunRecovery());
            yield return WaitUntil(() => recoveryFinished, "recovery coroutine completion");
            Object.Destroy(host.gameObject);
            if (recoveryFailure != null) Assert.Fail(recoveryFailure.ToString());
            yield return new ExitPlayMode();
        }

        private static IEnumerator RunRecovery()
        {
            var stack = new System.Collections.Generic.Stack<IEnumerator>();
            stack.Push(RecoveryRuntime());
            while (stack.Count > 0)
            {
                object current = null;
                bool moved;
                try { moved = stack.Peek().MoveNext(); if (moved) current = stack.Peek().Current; }
                catch (Exception error) { recoveryFailure = error; break; }
                if (!moved) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
                if (current is IEnumerator nested) { stack.Push(nested); continue; }
                yield return current;
            }
            recoveryFinished = true;
        }

        private static IEnumerator RecoveryRuntime()
        {
            // Start registers the production button callbacks after Awake.
            yield return null;
            var flow = Object.FindFirstObjectByType<VNTitleFlowController>();
            var runner = Object.FindFirstObjectByType<DialogueRunner>();
            var session = Object.FindFirstObjectByType<VNDialogueSessionState>();
            var save = Object.FindFirstObjectByType<VNSaveLoadController>();
            Assert.That(flow, Is.Not.Null, "Production Title controller must exist after scene startup.");
            Assert.That(runner.IsDialogueRunning, Is.False, "Title must not automatically start dialogue.");
            Assert.That(((GameObject)typeof(VNTitleFlowController).GetField("titleLayer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(flow)).activeInHierarchy, Is.True);
            Button FieldButton(string name) => (Button)typeof(VNTitleFlowController).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(flow);
            yield return WaitUntil(() => FieldButton("newGame").interactable, "title initialization");
            FieldButton("newGame").onClick.Invoke();
            yield return WaitUntil(() => session.IsLineActive && session.CurrentLineId == "line:prologue_s001_001", "first production line");
            VNSaveLoadOperationResult saved = null;
            yield return WaitUntil(() => { saved = save.QuickSave(); return saved.Succeeded; }, "stable quick save");
            SceneManager.LoadScene(flow.gameObject.scene.buildIndex);
            yield return null;
            yield return null;
            flow = Object.FindFirstObjectByType<VNTitleFlowController>();
            save = Object.FindFirstObjectByType<VNSaveLoadController>();
            yield return WaitUntil(() => FieldButton("continueGame").interactable, "continue enabled");
            var presentation = Object.FindFirstObjectByType<VNPresentationController>();
            var backgroundField = typeof(VNPresentationController).GetField("backgroundImage", BindingFlags.Instance | BindingFlags.NonPublic);
            var background = backgroundField.GetValue(presentation);
            VNSaveLoadOperationResult outcome = null;
            var starts = 0;
            save.LoadStateChanged += active => { if (active) { starts++; backgroundField.SetValue(presentation, null); } };
            save.LoadExecutionCompleted += result => outcome = result;
            LogAssert.Expect(LogType.Error, "VN Presentation Controller has no Background Image reference.");
            FieldButton("continueGame").onClick.Invoke();
            yield return WaitUntil(() => outcome != null, "load completion");
            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(starts, Is.EqualTo(1));
            if (presentation != null) backgroundField.SetValue(presentation, background);
            yield return WaitUntil(() => flow == null, "recovery scene reload");
            yield return null;
            flow = Object.FindFirstObjectByType<VNTitleFlowController>();
            runner = Object.FindFirstObjectByType<DialogueRunner>();
            save = Object.FindFirstObjectByType<VNSaveLoadController>();
            yield return WaitUntil(() => FieldButton("newGame").interactable, "recovered title");
            var status = (TMP_Text)typeof(VNTitleFlowController).GetField("statusText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(flow);
            Assert.That(status.text, Does.Contain("실패"));
            Assert.That(save.IsLoadInProgress, Is.False);
            Assert.That(save.IsModalOpen, Is.False);
            Assert.That(runner.IsDialogueRunning, Is.False);
            Assert.That(Object.FindObjectsByType<VNSaveLoadController>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<VNTitleFlowController>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            var titleGroup = ((GameObject)typeof(VNTitleFlowController).GetField("titleLayer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(flow)).GetComponent<CanvasGroup>();
            Assert.That(titleGroup.interactable && titleGroup.blocksRaycasts, Is.True, "Recovered title must own UI input.");
            FieldButton("newGame").onClick.Invoke();
            session = Object.FindFirstObjectByType<VNDialogueSessionState>();
            yield return WaitUntil(() => session.IsLineActive && session.CurrentLineId == "line:prologue_s001_001", "new game after recovery");
        }

        private static IEnumerator WaitUntil(Func<bool> condition, string stage)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (timer.Elapsed < TimeSpan.FromSeconds(30)) { if (condition()) yield break; yield return null; }
            Assert.Fail("Timed out waiting for " + stage);
        }
        [Test]
        public void ProductionScene_UsesTitleFirstAndExistingRuntime()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/VN_Main.unity", OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var runners = roots.SelectMany(x => x.GetComponentsInChildren<DialogueRunner>(true)).ToArray();
                Assert.That(runners.Length, Is.EqualTo(1));
                var runner = new SerializedObject(runners[0]);
                Assert.That(runner.FindProperty("autoStart").boolValue, Is.False);
                Assert.That(runner.FindProperty("startNode").stringValue, Is.EqualTo("M2_UI_START"));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<EventSystem>(true)).Count(), Is.EqualTo(1));
                var flow = roots.SelectMany(x => x.GetComponentsInChildren<VNTitleFlowController>(true)).Single();
                var so = new SerializedObject(flow);
                foreach (var field in new[] { "runner", "gate", "convenience", "router", "modals", "saves", "settings", "meta", "records", "titleLayer", "gameMenu", "confirmation", "newGame", "continueGame", "loadGame", "recordsButton", "settingsButton", "quitButton" })
                    Assert.That(so.FindProperty(field).objectReferenceValue, Is.Not.Null, field);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        [Test]
        public void Prefab_BackgroundIsIndependentAndReplaceable()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M12/VNTitleScreen.prefab");
            Assert.That(prefab, Is.Not.Null);
            var background = prefab.transform.Find("TitleLayer/Background/BackgroundImage");
            Assert.That(background.GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(background.GetComponent<AspectRatioFitter>().aspectMode, Is.EqualTo(AspectRatioFitter.AspectMode.EnvelopeParent));
            Assert.That(prefab.transform.Find("TitleLayer/DarkOverlay").GetComponent<Image>(), Is.Not.Null);
            Assert.That(prefab.transform.Find("TitleLayer/KoreanTitle"), Is.Not.Null);
            foreach (var name in new[] { "NewGameButton", "ContinueButton", "LoadGameButton", "RecordsButton", "SettingsButton", "QuitButton" })
                Assert.That(prefab.transform.Find("TitleLayer/MainMenu/" + name).GetComponent<Button>(), Is.Not.Null, name);
            var titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M12_Title_Korean_SDF.asset");
            Assert.That(titleFont, Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<TMP_Text>(true).All(text => text.font == titleFont), Is.True);
        }

        [Test]
        public void KoreanFont_CoversTitleAndAllM12MenuLabels()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansKR/M12_Title_Korean_SDF.asset");
            Assert.That(font, Is.Not.Null);
            Assert.That(font.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static));
            Assert.That(font.HasCharacters("우리가 만났던 모든 시대"), Is.True);
            Assert.That(font.HasCharacters("새 게임 이어하기 불러오기 기록 설정 종료 계속하기 타이틀로 확인 취소"), Is.True);
            Assert.That(font.HasCharacters("타이틀로 돌아가시겠습니까? 저장하지 않은 진행은 사라집니다."), Is.True);
        }

        [Test]
        public void MenuNavigation_IsExplicitAndDoesNotReachHiddenDialogueControls()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M12/VNTitleScreen.prefab");
            var gameMenu = prefab.transform.Find("InGameMenu").gameObject;
            Assert.That(gameMenu.GetComponent<CanvasGroup>(), Is.Not.Null);
            var resume = prefab.transform.Find("InGameMenu/Actions/ResumeButton").GetComponent<Button>();
            var title = prefab.transform.Find("InGameMenu/Actions/TitleButton").GetComponent<Button>();
            var quit = prefab.transform.Find("InGameMenu/Actions/QuitButton").GetComponent<Button>();
            Assert.That(resume.navigation.mode, Is.EqualTo(Navigation.Mode.Explicit));
            Assert.That(resume.navigation.selectOnDown, Is.SameAs(title));
            Assert.That(title.navigation.selectOnUp, Is.SameAs(resume));
            Assert.That(title.navigation.selectOnDown, Is.SameAs(quit));
            Assert.That(quit.navigation.selectOnUp, Is.SameAs(title));
            Assert.That(quit.navigation.selectOnDown, Is.Null);

            var confirm = prefab.transform.Find("ConfirmationLayer/Dialog/ConfirmButton").GetComponent<Button>();
            var cancel = prefab.transform.Find("ConfirmationLayer/Dialog/CancelButton").GetComponent<Button>();
            Assert.That(prefab.transform.Find("ConfirmationLayer").GetComponent<CanvasGroup>(), Is.Not.Null);
            Assert.That(confirm.navigation.mode, Is.EqualTo(Navigation.Mode.Explicit));
            Assert.That(confirm.navigation.selectOnRight, Is.SameAs(cancel));
            Assert.That(cancel.navigation.selectOnLeft, Is.SameAs(confirm));
        }

        [Test]
        public void MenuNavigation_MoveEventsFollowTheExplicitButtonChain()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M12/VNTitleScreen.prefab");
            var eventSystemObject = new GameObject("M12 navigation test EventSystem");
            var instance = Object.Instantiate(prefab);
            try
            {
                var eventSystem = eventSystemObject.AddComponent<EventSystem>();
                var menu = instance.transform.Find("InGameMenu").gameObject;
                menu.SetActive(true);
                var resume = instance.transform.Find("InGameMenu/Actions/ResumeButton").GetComponent<Button>();
                var title = instance.transform.Find("InGameMenu/Actions/TitleButton").GetComponent<Button>();
                var quit = instance.transform.Find("InGameMenu/Actions/QuitButton").GetComponent<Button>();

                eventSystem.SetSelectedGameObject(resume.gameObject);
                MoveDown(eventSystem, resume.gameObject);
                Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(title.gameObject));
                MoveDown(eventSystem, title.gameObject);
                Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(quit.gameObject));
                MoveDown(eventSystem, quit.gameObject);
                Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(quit.gameObject), "The final item must not wrap into hidden gameplay controls.");

                var confirm = instance.transform.Find("ConfirmationLayer/Dialog/ConfirmButton").GetComponent<Button>();
                var cancel = instance.transform.Find("ConfirmationLayer/Dialog/CancelButton").GetComponent<Button>();
                instance.transform.Find("ConfirmationLayer").gameObject.SetActive(true);
                eventSystem.SetSelectedGameObject(confirm.gameObject);
                var right = new AxisEventData(eventSystem) { moveVector = Vector2.right, moveDir = MoveDirection.Right };
                ExecuteEvents.Execute(confirm.gameObject, right, ExecuteEvents.moveHandler);
                Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(cancel.gameObject));
            }
            finally
            {
                Object.DestroyImmediate(instance);
                Object.DestroyImmediate(eventSystemObject);
            }
        }

        private static void MoveDown(EventSystem eventSystem, GameObject selected)
        {
            var move = new AxisEventData(eventSystem) { moveVector = Vector2.down, moveDir = MoveDirection.Down };
            ExecuteEvents.Execute(selected, move, ExecuteEvents.moveHandler);
        }

        [Test]
        public void TitleOwnership_BlocksConvenienceAndSaveLoadWithoutClaimingModal()
        {
            var root = new GameObject("M12 gate test");
            try
            {
                var gate = root.AddComponent<VNInteractionGate>();
                Assert.That(gate.CanChangeConvenienceMode, Is.True);
                gate.SetTitleFlowBlocksGameplay(true);
                Assert.That(gate.CanChangeConvenienceMode, Is.False);
                Assert.That(gate.CanUseSaveLoad, Is.False);
                Assert.That(gate.IsBlockingModalActive, Is.False);
                gate.SetTitleFlowBlocksGameplay(false);
                Assert.That(gate.CanUseSaveLoad, Is.True);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void ContinueSelection_ExcludesInvalidAndIncompatibleAndBreaksTiesDeterministically()
        {
            var manual = new VNSaveSlotKey(VNSaveSlotType.Manual, 2);
            var auto = new VNSaveSlotKey(VNSaveSlotType.Auto, 0);
            var quick = new VNSaveSlotKey(VNSaveSlotType.Quick, 0);
            VNSaveSlotInspection Valid(VNSaveSlotKey key, string timestamp) =>
                new(key, VNSaveReadResult.Valid(new SaveSlotData { savedAtUtcIso8601 = timestamp }));
            var slots = new[] { Valid(quick, "2025-01-02T00:00:00.0000000+00:00"), Valid(auto, "2025-01-02T00:00:00.0000000+00:00"),
                Valid(manual, "2025-01-02T00:00:00.0000000+00:00"), Valid(new VNSaveSlotKey(VNSaveSlotType.Manual, 1), "invalid") };
            Assert.That(VNContinueSelection.TrySelect(slots, key => key != manual, out var chosen), Is.True);
            Assert.That(chosen, Is.EqualTo(auto));
            Assert.That(VNContinueSelection.TrySelect(slots, key => true, out chosen), Is.True);
            Assert.That(chosen, Is.EqualTo(manual));
            Assert.That(VNContinueSelection.TrySelect(slots, key => false, out _), Is.False);
        }

        [Test]
        public void ContinueSelection_SkipsNewerInvalidAndIncompatibleSlots()
        {
            var old = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);
            var newer = new VNSaveSlotKey(VNSaveSlotType.Auto, 0);
            var quick = new VNSaveSlotKey(VNSaveSlotType.Quick, 0);
            var slots = new[] {
                new VNSaveSlotInspection(old, VNSaveReadResult.Valid(new SaveSlotData { savedAtUtcIso8601 = "2024-01-01T00:00:00.0000000+00:00" })),
                new VNSaveSlotInspection(newer, VNSaveReadResult.Valid(new SaveSlotData { savedAtUtcIso8601 = "2025-01-01T00:00:00.0000000+00:00" })),
                new VNSaveSlotInspection(quick, VNSaveReadResult.Corrupted("bad json")),
                new VNSaveSlotInspection(new VNSaveSlotKey(VNSaveSlotType.Manual, 1), VNSaveReadResult.Unsupported("schema"))
            };
            Assert.That(VNContinueSelection.TrySelect(slots, key => key != newer, out var chosen), Is.True);
            Assert.That(chosen, Is.EqualTo(old));
        }
    }
}
