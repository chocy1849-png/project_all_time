using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Settings;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class M1303SettingsIntegrationTests
{
    string directory; GameObject root, panelObject; InputActionAsset input; VNInputRebindService rebind;
    VNSettingsService service; VNSettingsPanel panel;
    T Ref<T>(UnityEngine.Object target,string name) where T:UnityEngine.Object => (T)new SerializedObject(target).FindProperty(name).objectReferenceValue;
    [SetUp] public void Setup()
    {
        directory=Path.Combine(Path.GetTempPath(),"M1303_"+Guid.NewGuid().ToString("N"));
        service=new VNSettingsService(VNSettingsRepository.CreateForTesting(directory)); service.Load();
        var scene=SceneManager.GetSceneByPath("Assets/_Project/Scenes/VN_Main.unity"); var opened=!scene.IsValid() || !scene.isLoaded;
        if(opened) scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/VN_Main.unity",OpenSceneMode.Additive);
        var source=scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<VNSettingsPanel>(true)).Single();
        panelObject=UnityEngine.Object.Instantiate(source.gameObject); panel=panelObject.GetComponent<VNSettingsPanel>();
        if(opened) EditorSceneManager.CloseScene(scene,true);
        root=new GameObject("Settings isolated test"); var convenience=root.AddComponent<VNConvenienceController>(); var runner=root.AddComponent<DialogueRunner>(); var presenter=root.AddComponent<LinePresenter>(); presenter.Typewriter=new LetterTypewriter(); runner.DialoguePresenters=new DialoguePresenterBase[]{presenter}; var router=root.AddComponent<VNConvenienceInputRouter>();
        input=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/VNInputActions.inputactions"));
        rebind=new VNInputRebindService(service,input,router);
        Assert.IsTrue(panel.Initialize(service,new VNDisplaySettingsController(service,new Display()),new VNTextAutoSettingsController(service,runner,convenience),new VNAudioSettingsController(service,new Mixer()),new VNGameplaySettingsController(service,convenience),rebind));
        // EditMode does not run disabled-view lifecycle as Play Mode does; attach the same handlers explicitly.
        foreach(var field in new[]{"displayView","textView","audioView","gameplayView"})
        {
            var view=Ref<Component>(panel,field);
            view.GetType().GetMethod("RegisterListeners",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(view,new object[]{true});
        }
    }
    [TearDown] public void Cleanup()
    {
        if(rebind!=null) rebind.Dispose(); if(panelObject!=null) UnityEngine.Object.DestroyImmediate(panelObject); if(root!=null) UnityEngine.Object.DestroyImmediate(root); if(input!=null) UnityEngine.Object.DestroyImmediate(input);
        if(Directory.Exists(directory)) Directory.Delete(directory,true);
    }
    static void Register(Component view) => view.GetType().GetMethod("RegisterListeners",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(view,new object[]{true});
    [Test] public void ProductionTextAndAudioSliders_CommitOnlyAtBoundaryAndKeepChannelsIndependent()
    {
        panel.ShowCategory(VNSettingsCategory.Text); var text=Ref<VNTextSettingsView>(panel,"textView"); Register(text); var slider=Ref<Slider>(text,"textSpeedSlider"); var commit=Ref<VNSettingsSliderCommit>(text,"textSpeedCommit");
        var old=service.Current.textSpeedLps; var candidate=old==40?60:40; slider.value=candidate; Assert.AreEqual(old,service.Current.textSpeedLps); commit.Commit(); Assert.AreEqual(candidate,service.Current.textSpeedLps);
        var auto=Ref<Slider>(text,"autoSpeedSlider"); auto.value=.42f; Ref<VNSettingsSliderCommit>(text,"autoSpeedCommit").Commit(); Assert.AreEqual(.42f,service.Current.autoSpeedNormalized,.001f);
        panel.ShowCategory(VNSettingsCategory.Audio); var audio=Ref<VNAudioSettingsView>(panel,"audioView"); Register(audio); var names=new[]{"master","bgm","sfx","voice"}; var values=new[]{.2f,.4f,.6f,.8f};
        for(int i=0;i<names.Length;i++){Ref<Slider>(audio,names[i]+"Slider").value=values[i]; Ref<VNSettingsSliderCommit>(audio,names[i]+"Commit").Commit();}
        var current=service.Current; Assert.AreEqual(values[0],current.masterVolumeNormalized,.001f); Assert.AreEqual(values[1],current.bgmVolumeNormalized,.001f); Assert.AreEqual(values[2],current.sfxVolumeNormalized,.001f); Assert.AreEqual(values[3],current.voiceVolumeNormalized,.001f);
        var reload=new VNSettingsService(VNSettingsRepository.CreateForTesting(directory)); var restored=reload.Load(); Assert.AreEqual(candidate,restored.textSpeedLps); Assert.AreEqual(values[3],restored.voiceVolumeNormalized,.001f);
    }
    [Test] public void ProductionDisplayAndGameplayControls_UseAuthorityAndRefresh()
    {
        panel.ShowCategory(VNSettingsCategory.Display); var display=Ref<VNDisplaySettingsView>(panel,"displayView"); var mode=Ref<TMP_Dropdown>(display,"displayModeDropdown"); var resolution=Ref<TMP_Dropdown>(display,"resolutionDropdown");
        mode.value=1; Assert.AreEqual(VNSettingsDefaults.WindowedDisplayMode,service.Current.displayMode); Assert.IsTrue(resolution.interactable);
        mode.value=0; Assert.AreNotEqual(VNSettingsDefaults.WindowedDisplayMode,service.Current.displayMode); Assert.IsFalse(resolution.interactable);
        panel.ShowCategory(VNSettingsCategory.Gameplay); var gameplay=Ref<VNGameplaySettingsView>(panel,"gameplayView"); var skip=Ref<Toggle>(gameplay,"skipUnreadToggle"); var shake=Ref<Toggle>(gameplay,"screenShakeToggle");
        var newSkip=!service.Current.skipUnread; var newShake=!service.Current.screenShakeEnabled; skip.isOn=newSkip; shake.isOn=newShake; Assert.AreEqual(newSkip,service.Current.skipUnread); Assert.AreEqual(newShake,service.Current.screenShakeEnabled);
        panel.RefreshFromAuthority(); Assert.AreEqual(newSkip,skip.isOn); Assert.AreEqual(newShake,shake.isOn);
    }
    [Test] public void ReadOnlyFutureSettings_DisablesControlsWithoutOverwritingFile()
    {
        var repository=VNSettingsRepository.CreateForTesting(directory); Directory.CreateDirectory(directory);
        const string future="{\"schemaVersion\":999,\"futureField\":\"keep\"}"; File.WriteAllText(repository.CanonicalFilePath,future);
        service.Load(); Assert.IsFalse(service.CanWrite); panel.RefreshFromAuthority();
        foreach(var slider in panel.GetComponentsInChildren<Slider>(true)) Assert.IsFalse(slider.interactable,slider.name);
        foreach(var dropdown in panel.GetComponentsInChildren<TMP_Dropdown>(true)) Assert.IsFalse(dropdown.interactable,dropdown.name);
        var gameplay=Ref<VNGameplaySettingsView>(panel,"gameplayView"); foreach(var toggle in gameplay.GetComponentsInChildren<Toggle>(true)) Assert.IsFalse(toggle.interactable,toggle.name);
        var controls=Ref<VNControlsSettingsView>(panel,"controlsView"); Assert.IsFalse(Ref<Button>(controls,"resetAllButton").interactable);
        foreach(var item in controls.GetComponentsInChildren<VNRebindItem>(true)) { Assert.IsFalse(Ref<Button>(item,"rebindButton").interactable); Assert.IsFalse(Ref<Button>(item,"resetButton").interactable); }
        Assert.AreEqual(future,File.ReadAllText(repository.CanonicalFilePath));
    }
    [Test] public void ProductionRebindRows_CompleteCancelConflictAndResetThroughExistingService()
    {
        panel.ShowCategory(VNSettingsCategory.Controls);
        var view=Ref<VNControlsSettingsView>(panel,"controlsView");
        var register=view.GetType().GetMethod("RegisterListeners",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance); register.Invoke(view,new object[]{true});
        var item=view.GetComponentsInChildren<VNRebindItem>(true).Single(x=>x.Target==VNRebindTarget.Advance);
        item.GetType().GetMethod("OnEnable",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(item,null);
        var button=Ref<Button>(item,"rebindButton"); var reset=Ref<Button>(item,"resetButton");
        var previous=InputSystem.settings.backgroundBehavior; InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        var keyboard=InputSystem.AddDevice<Keyboard>();
        try
        {
            button.onClick.Invoke(); Assert.IsTrue(rebind.IsRebinding);
            InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(Key.K)); InputSystem.Update();
            Assert.IsFalse(rebind.IsRebinding); Assert.IsTrue(rebind.TryGetBindingDisplay(VNRebindTarget.Advance,out var changed,out var diagnostic),diagnostic); Assert.IsTrue(changed.DisplayString.Contains("K"));
            var guide=view.GetComponent<VNControlsGuide>(); guide.RefreshBindings(); var labels=new SerializedObject(guide).FindProperty("bindingLabels"); var guideKey=(TMP_Text)labels.GetArrayElementAtIndex(0).objectReferenceValue; Assert.AreEqual(changed.DisplayString,guideKey.text);
            reset.onClick.Invoke(); Assert.IsTrue(rebind.TryGetBindingDisplay(VNRebindTarget.Advance,out var restored,out diagnostic),diagnostic); Assert.IsTrue(restored.DisplayString.Contains("Space"));
            InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState()); InputSystem.Update();
            button.onClick.Invoke(); InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(Key.A)); InputSystem.Update(); Assert.IsFalse(rebind.IsRebinding);
            Assert.IsTrue(rebind.TryGetBindingDisplay(VNRebindTarget.Advance,out restored,out diagnostic),diagnostic); Assert.IsTrue(restored.DisplayString.Contains("Space"));
            InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState()); InputSystem.Update();
            button.onClick.Invoke(); InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(Key.Escape)); InputSystem.Update(); Assert.IsFalse(rebind.IsRebinding);
            InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState()); InputSystem.Update();
            button.onClick.Invoke(); Assert.IsTrue(rebind.IsRebinding); panel.ShowCategory(VNSettingsCategory.Text); Assert.IsFalse(rebind.IsRebinding);
            panel.ShowCategory(VNSettingsCategory.Controls); button.onClick.Invoke(); Assert.IsTrue(rebind.IsRebinding); panel.PrepareForClose(); Assert.IsFalse(rebind.IsRebinding);
            InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState()); InputSystem.Update();
            button.onClick.Invoke(); InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(Key.K)); InputSystem.Update();
            Assert.IsTrue(rebind.TryGetBindingDisplay(VNRebindTarget.Advance,out changed,out diagnostic),diagnostic); Assert.IsTrue(changed.DisplayString.Contains("K"));
            Ref<Button>(view,"resetAllButton").onClick.Invoke(); Assert.IsTrue(rebind.TryGetBindingDisplay(VNRebindTarget.Advance,out restored,out diagnostic),diagnostic); Assert.IsTrue(restored.DisplayString.Contains("Space"));
        }
        finally { InputSystem.RemoveDevice(keyboard); InputSystem.settings.backgroundBehavior=previous; }
    }
    sealed class Display:IVNDisplayRuntime { public Resolution[] SupportedResolutions=>Array.Empty<Resolution>(); public int NativeWidth=>1920; public int NativeHeight=>1080; public void SetResolution(int w,int h,FullScreenMode mode){} }
    sealed class Mixer:IVNAudioMixerRuntime { public bool TryGetFloat(string name,out float value){value=0;return true;} public bool TrySetFloat(string name,float value)=>true; }
}
