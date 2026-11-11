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

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M12TitleIntegrationTests
    {
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
