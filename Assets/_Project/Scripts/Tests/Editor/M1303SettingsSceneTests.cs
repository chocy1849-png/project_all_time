using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Settings;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class M1303SettingsSceneTests
{
    const string ScenePath="Assets/_Project/Scenes/VN_Main.unity";
    Scene scene; bool opened; VNSettingsPanel panel;
    [SetUp] public void SetUp()
    {
        scene=SceneManager.GetSceneByPath(ScenePath); opened=!scene.IsValid() || !scene.isLoaded;
        if(opened) scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        panel=scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<VNSettingsPanel>(true)).Single();
    }
    [TearDown] public void TearDown() { if(opened) EditorSceneManager.CloseScene(scene,true); }
    T Ref<T>(Object owner,string name) where T:Object => (T)new SerializedObject(owner).FindProperty(name).objectReferenceValue;
    [Test] public void ProductionScene_ExistingViewsAndDropdownTemplatesAreWired()
    {
        Assert.IsTrue(panel.TryValidateWiring(out var diagnostic),diagnostic);
        var dropdowns=panel.GetComponentsInChildren<TMP_Dropdown>(true); Assert.AreEqual(2,dropdowns.Length);
        foreach(var dropdown in dropdowns) { Assert.NotNull(dropdown.template); Assert.NotNull(dropdown.itemText); Assert.NotNull(dropdown.template.GetComponentInChildren<Toggle>(true)); }
        foreach(var text in panel.GetComponentsInChildren<TMP_Text>(true)) Assert.AreEqual("M13_Shared_Korean_SDF",text.font.name,text.name);
    }
    [Test] public void CategorySelection_OnlySelectedContentAndActiveIndicatorRemainVisible()
    {
        var names=new[]{"display","text","audio","gameplay","controls"};
        for(int selected=0;selected<names.Length;selected++)
        {
            panel.ShowCategory((VNSettingsCategory)selected);
            for(int index=0;index<names.Length;index++)
            {
                Assert.AreEqual(index==selected,Ref<GameObject>(panel,names[index]+"Content").activeSelf);
                var button=Ref<Button>(panel,names[index]+"CategoryButton"); var state=button.GetComponent<M13SharedControlState>(); Assert.NotNull(state);
                var active=button.transform.Find("M13_Active"); Assert.NotNull(active); Assert.AreEqual(index==selected,active.gameObject.activeSelf);
            }
        }
    }
    [Test] public void SliderCommitSeams_StayOnAllSixExistingSliderObjects()
    {
        var sliders=panel.GetComponentsInChildren<Slider>(true); Assert.AreEqual(6,sliders.Length);
        foreach(var slider in sliders) {var commit=slider.GetComponent<VNSettingsSliderCommit>(); Assert.NotNull(commit,slider.name); Assert.IsTrue(commit.TryValidateWiring(out var diagnostic),diagnostic); Assert.NotNull(slider.fillRect); Assert.NotNull(slider.handleRect); var visual=slider.transform.Find("M13_Slider_Visual"); Assert.NotNull(visual); Assert.IsTrue(PrefabUtility.IsAnyPrefabInstanceRoot(visual.gameObject));}
    }
    [Test] public void AllControls_KeepReferencesAndNoMissingScripts()
    {
        Assert.AreEqual(6,panel.GetComponentsInChildren<VNRebindItem>(true).Length);
        var gameplay=panel.GetComponentInChildren<VNGameplaySettingsView>(true);
        foreach(var toggle in gameplay.GetComponentsInChildren<Toggle>(true)) { var visual=toggle.transform.Find("M13_Toggle_Visual"); Assert.NotNull(visual); Assert.IsTrue(PrefabUtility.IsAnyPrefabInstanceRoot(visual.gameObject)); }
        foreach(var item in panel.GetComponentsInChildren<VNRebindItem>(true)) Assert.IsTrue(item.TryValidateWiring(out var diagnostic),diagnostic);
        foreach(var t in panel.GetComponentsInChildren<Transform>(true)) Assert.AreEqual(0,GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject),t.name);
        foreach(var image in panel.GetComponentsInChildren<Image>(true)) if(image.type==Image.Type.Sliced) Assert.NotNull(image.sprite,image.name);
    }
}
