var session = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNDialogueSessionState>();
var bridge = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNLineAdvancerInputBridge>();
var saves = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.SaveLoad.VNSaveLoadController>();
var isolatedRoot = System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
if (string.IsNullOrWhiteSpace(isolatedRoot) || !string.Equals(System.IO.Path.GetFullPath(saves.Repository.StorageRoot), System.IO.Path.GetFullPath(System.IO.Path.Combine(isolatedRoot, "SaveData")), System.StringComparison.OrdinalIgnoreCase)) throw new System.Exception("An initialized isolated SaveData root is required.");
var runner = UnityEngine.Object.FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
var progress = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.MetaProgress.VNMetaProgressRuntimeBootstrap>().MetaProgressService;
var profile = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNProfileBrowser>();
var overlay = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNInformationOverlay>();
var presentation = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNPresentationController>();
var audio = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Audio.VNAudioController>();
var targets = new[] { "line:prologue_s001_016", "line:prologue_s002_009", "line:prologue_s005_048", "line:prologue_s006_017", "line:prologue_s007_008", "line:prologue_s008_017" };
var successors = new[] { "line:prologue_s001_017", "line:prologue_s002_010", "line:prologue_s005_049", "line:prologue_s006_018", "line:prologue_s007_009", "line:prologue_s008_018" };
var path = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT"), "matrix.txt");
System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
System.IO.File.WriteAllText(path, "START\n");
int index = 0, phase = 0, readsBeforeLoad = 0; string snapshot = "", variables = "", bgm = ""; bool expectedOverlay = false; double phaseTime = UnityEditor.EditorApplication.timeSinceStartup;
var deadline = phaseTime + 240;
UnityEditor.EditorApplication.CallbackFunction step = null;
step = () => {
 try {
    if (!UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.timeSinceStartup > deadline) throw new System.Exception("timed out at " + index + "/" + phase + " line=" + session.CurrentLineId);
    if (index == targets.Length) { System.IO.File.AppendAllText(path, "DONE\n"); UnityEditor.EditorApplication.update -= step; return; }
    if (profile.IsBrowsing) {
        if (profile.IsOverview) { if (profile.AllReviewed) profile.TryContinue(); else { for (int k=0; k<3; k++) if (!profile.HasReviewed(k)) { profile.Inspect(k); break; } } }
        else if (!profile.IsReviewPending) profile.Back();
        else bridge.TryAdvance(ProjectAllTime.VN.Dialogue.VNAdvanceSource.Manual);
        return;
    }
    if (phase == 0) {
        if (session.CurrentLineId != targets[index] || !session.IsCurrentLineFullyDisplayed) { bridge.TryAdvance(ProjectAllTime.VN.Dialogue.VNAdvanceSource.Manual); return; }
        var result = saves.SaveManual(index);
        if (!result.Succeeded) { if (UnityEditor.EditorApplication.timeSinceStartup - phaseTime > 30) throw new System.Exception("save " + targets[index] + ": " + result.Message); return; }
        var data = saves.Repository.Read(new ProjectAllTime.VN.SaveLoad.VNSaveSlotKey(ProjectAllTime.VN.SaveLoad.VNSaveSlotType.Manual, index)).SaveData;
        snapshot = UnityEngine.JsonUtility.ToJson(data.presentationState); variables = UnityEngine.JsonUtility.ToJson(data.yarnVariables); bgm=data.audioState.bgmId; expectedOverlay=overlay.IsShown;
        System.IO.File.AppendAllText(path, "SAVE " + targets[index] + " bgm=" + bgm + " overlay=" + expectedOverlay + " presentation=" + snapshot + "\n");
        phase=1; phaseTime=UnityEditor.EditorApplication.timeSinceStartup; bridge.TryAdvance(ProjectAllTime.VN.Dialogue.VNAdvanceSource.Manual); return;
    }
    if (phase == 1) {
        if (session.CurrentLineId == targets[index] || !session.IsLineActive) { bridge.TryAdvance(ProjectAllTime.VN.Dialogue.VNAdvanceSource.Manual); return; }
        readsBeforeLoad=progress.Current.readLineIds.Length;
        var load=saves.Load(new ProjectAllTime.VN.SaveLoad.VNSaveSlotKey(ProjectAllTime.VN.SaveLoad.VNSaveSlotType.Manual,index));
        if (!load.Succeeded) throw new System.Exception("load failed: " + load.Message);
        phase=2; phaseTime=UnityEditor.EditorApplication.timeSinceStartup; return;
    }
    if (phase == 2) {
        if (saves.IsLoadInProgress || session.CurrentLineId != targets[index] || !session.IsCurrentLineFullyDisplayed) { if(UnityEditor.EditorApplication.timeSinceStartup-phaseTime > 20) throw new System.Exception("restore timeout " + targets[index] + " actual="+session.CurrentLineId); return; }
        if (!presentation.TryCaptureStableState(out var p, out var pd)) throw new System.Exception(pd);
        if (!ProjectAllTime.VN.SaveLoad.VNYarnVariableSnapshot.TryCapture(runner.VariableStorage, out var v, out var vd)) throw new System.Exception(vd);
        if (!audio.TryCaptureStableState(out var a, out var ad)) throw new System.Exception(ad);
        var sameP = snapshot == UnityEngine.JsonUtility.ToJson(p); var sameV = variables == UnityEngine.JsonUtility.ToJson(v);
        if (!sameP || !sameV || bgm != a.bgmId || expectedOverlay != overlay.IsShown || readsBeforeLoad != progress.Current.readLineIds.Length) throw new System.Exception("mismatch " + targets[index] + " presentation="+sameP+" variables="+sameV+" bgm="+a.bgmId+" overlay="+overlay.IsShown+" reads="+progress.Current.readLineIds.Length+" expected="+readsBeforeLoad);
        if(index==1 && p.cgId!="cg_beach_photo_child") throw new System.Exception("S002 CG mismatch");
        if(index==3 && !System.Linq.Enumerable.Any(p.characters, x=>x.characterId=="rope")) throw new System.Exception("S006 cyber Rope missing");
        if(index==4 && !overlay.IsShown) throw new System.Exception("S007 overlay missing");
        if(index==5 && profile.IsBrowsing) throw new System.Exception("S008 profile reopened");
        phase=3; phaseTime=UnityEditor.EditorApplication.timeSinceStartup; bridge.TryAdvance(ProjectAllTime.VN.Dialogue.VNAdvanceSource.Manual); return;
    }
    if (phase == 3) {
        if(session.CurrentLineId == targets[index] || !session.IsLineActive) { if(UnityEditor.EditorApplication.timeSinceStartup-phaseTime>20) throw new System.Exception("successor timeout " + targets[index]); return; }
        if(session.CurrentLineId != successors[index]) throw new System.Exception("wrong successor " + session.CurrentLineId + " expected " + successors[index]);
        System.IO.File.AppendAllText(path, "PASS " + targets[index] + " -> " + session.CurrentLineId + "\n"); index++; phase=0; phaseTime=UnityEditor.EditorApplication.timeSinceStartup; return;
    }
 } catch(System.Exception e) { System.IO.File.AppendAllText(path,"FAIL " + e + "\n"); UnityEditor.EditorApplication.update -= step; }
};
UnityEditor.EditorApplication.update += step;
return path;
