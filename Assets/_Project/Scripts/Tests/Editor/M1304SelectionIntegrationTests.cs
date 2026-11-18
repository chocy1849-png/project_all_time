using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M1304SelectionIntegrationTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private string rootPath;
        private VNSaveRepository repository;
        private GameObject modalObject, controllerObject;
        private VNSaveLoadModal modal;
        private VNSaveLoadController controller;
        private Scene scene;
        private bool openedScene;
        private static object Call(object target, string method, params object[] args) => target.GetType().GetMethod(method, Private).Invoke(target, args);
        private static void Set(object target, string name, object value) => target.GetType().GetField(name, Private).SetValue(target, value);
        private static object Get(object target, string name) => target.GetType().GetField(name, Private).GetValue(target);

        [SetUp] public void SetUp()
        {
            rootPath = Path.Combine(Path.GetTempPath(), "M1304_Selection_" + Guid.NewGuid().ToString("N"));
            repository = VNSaveRepository.CreateForTesting(rootPath);
            const string scenePath = "Assets/_Project/Scenes/VN_Main.unity";
            scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.isLoaded) { scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive); openedScene = true; }
            var source = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<VNSaveLoadModal>(true)).Single();
            modalObject = UnityEngine.Object.Instantiate(source.gameObject);
            modal = modalObject.GetComponent<VNSaveLoadModal>();
            controllerObject = new GameObject("M1304 isolated controller");
            controller = controllerObject.AddComponent<VNSaveLoadController>();
            Set(controller, "saveLoadModal", modal);
            Set(controller, "repository", repository);
            Call(modal, "Initialize", controller, repository, new VNThumbnailService(), null);
            controller.OpenSave();
        }
        [TearDown] public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(controllerObject);
            UnityEngine.Object.DestroyImmediate(modalObject);
            if (openedScene) EditorSceneManager.CloseScene(scene, true);
            if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true);
        }
        private VNSaveSlotViewModel First(VNSaveLoadCategory category, int page = 0) => VNSaveLoadSlotModelBuilder.Build(repository.InspectAllSlots(), category, page)[0];
        private void Select(VNSaveSlotViewModel model) => Call(modal, "SelectSlot", model);
        [Test] public void SelectEmptyManual_DoesNotWrite_AndPageChangeClearsSelection()
        {
            var model = First(VNSaveLoadCategory.Manual);
            Select(model);
            Assert.That(repository.Read(model.SlotKey).State, Is.EqualTo(VNSaveSlotState.Empty));
            Assert.That(Get(modal, "selectedKey"), Is.EqualTo(model.SlotKey));
            controller.ChangePage(1);
            Assert.That(Get(modal, "selectedKey"), Is.Null);
            Call(controller, "ExecuteSelected", model.SlotKey);
            Assert.That(repository.Read(model.SlotKey).State, Is.EqualTo(VNSaveSlotState.Empty));
        }
        [Test] public void SelectAutoInSaveMode_CannotExecuteOrCreateSlot()
        {
            controller.SetCategory(VNSaveLoadCategory.Auto);
            var model = First(VNSaveLoadCategory.Auto);
            Select(model);
            Call(controller, "ExecuteSelected", model.SlotKey);
            Assert.That(repository.Read(model.SlotKey).State, Is.EqualTo(VNSaveSlotState.Empty));
        }
        [Test] public void TitleLoad_CannotSwitchToSave()
        {
            controller.Close();
            var gate = controllerObject.AddComponent<ProjectAllTime.VN.Dialogue.VNInteractionGate>();
            gate.SetTitleFlowBlocksGameplay(true); Set(controller, "interactionGate", gate);
            controller.OpenLoad(); Call(modal, "SwitchMode", VNSaveLoadMode.Save);
            Assert.That(modal.Mode, Is.EqualTo(VNSaveLoadMode.Load));
        }
        [Test] public void GameplayLoad_CanSwitchToSaveWhenGateAllowsIt()
        {
            controller.Close();
            var gate = controllerObject.AddComponent<ProjectAllTime.VN.Dialogue.VNInteractionGate>();
            Set(controller, "interactionGate", gate);
            controller.OpenLoad(); Call(modal, "SwitchMode", VNSaveLoadMode.Save);
            Assert.That(modal.Mode, Is.EqualTo(VNSaveLoadMode.Save));
        }
        [Test] public void Confirmation_DisablesParentAndRestoresItOnCancel()
        {
            var group = (CanvasGroup)Get(modal, "mainContentGroup");
            Call(modal, "ShowOverwriteConfirmation", First(VNSaveLoadCategory.Manual));
            Assert.That(modal.IsOverwriteConfirmationActive, Is.True);
            Assert.That(group.interactable, Is.False);
            Assert.That(group.blocksRaycasts, Is.False);
            controller.SetCategory(VNSaveLoadCategory.Auto); controller.ChangePage(1);
            Assert.That(modal.Category, Is.EqualTo(VNSaveLoadCategory.Manual));
            Assert.That(modal.Page, Is.Zero);
            Assert.That(modal.IsOverwriteConfirmationActive, Is.True);
            controller.CancelManualOverwrite();
            Assert.That(modal.IsOverwriteConfirmationActive, Is.False);
            Assert.That(group.interactable && group.blocksRaycasts, Is.True);
        }
        [Test] public void ThumbnailSuppression_RestoresModalVisibilityAndInput()
        {
            var group = (CanvasGroup)Get(modal, "modalCanvasGroup");
            Assert.That((bool)Call(modal, "BeginThumbnailVisualSuppression"), Is.True);
            Assert.That(group.alpha, Is.Zero);
            Assert.That(group.interactable, Is.False);
            Assert.That(group.blocksRaycasts, Is.True);
            controller.ChangePage(1);
            Assert.That(modal.Page, Is.Zero);
            Call(modal, "EndThumbnailVisualSuppression");
            Assert.That(group.alpha, Is.EqualTo(1f));
            Assert.That(group.interactable && group.blocksRaycasts, Is.True);
        }
        [Test] public void CorruptedAndUnsupportedManualSlots_AreNotOverwritten()
        {
            var key = new VNSaveSlotKey(VNSaveSlotType.Manual, 0);
            Assert.That(repository.TryGetSlotPath(key, out var path), Is.True);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            foreach (var json in new[] { "{ broken", "{\"schemaVersion\":999}" })
            {
                File.WriteAllText(path, json);
                controller.RefreshSlots(); Select(First(VNSaveLoadCategory.Manual));
                Call(controller, "ExecuteSelected", key);
                Assert.That(File.ReadAllText(path), Is.EqualTo(json));
            }
        }
    }
}
