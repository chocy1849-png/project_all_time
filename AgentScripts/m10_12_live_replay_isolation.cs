var rc=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.Replay.UI.VNReplayController>();
var s=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNDialogueSessionState>();
var r=UnityEngine.Object.FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
var progress=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.MetaProgress.VNMetaProgressRuntimeBootstrap>().MetaProgressService;
var present=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNPresentationController>();
var audio=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Audio.VNAudioController>();
var saves=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.SaveLoad.VNSaveLoadController>();
var isolatedRoot=System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
if(string.IsNullOrWhiteSpace(isolatedRoot) || !string.Equals(System.IO.Path.GetFullPath(saves.Repository.StorageRoot),System.IO.Path.GetFullPath(System.IO.Path.Combine(isolatedRoot,"SaveData")),System.StringComparison.OrdinalIgnoreCase)) throw new System.Exception("An initialized isolated SaveData root is required.");
var path=System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT"),"m10_12_replay_isolation.txt");
string Snapshot() { present.TryCaptureStableState(out var p,out _); audio.TryCaptureStableState(out var a,out _); ProjectAllTime.VN.SaveLoad.VNYarnVariableSnapshot.TryCapture(r.VariableStorage,out var v,out _); using var sha=System.Security.Cryptography.SHA256.Create(); var key=new ProjectAllTime.VN.SaveLoad.VNSaveSlotKey(ProjectAllTime.VN.SaveLoad.VNSaveSlotType.Manual,2); saves.Repository.TryGetSlotPath(key,out var savePath); var hash=System.Convert.ToBase64String(sha.ComputeHash(System.IO.File.ReadAllBytes(savePath))); return s.CurrentLineId+"|"+s.Backlog.Count+"|"+UnityEngine.JsonUtility.ToJson(progress.Current)+"|"+UnityEngine.JsonUtility.ToJson(p)+"|"+a.bgmId+"|"+UnityEngine.JsonUtility.ToJson(v)+"|"+saves.IsLoadInProgress+"|"+hash; }
var modalController=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNConvenienceModalController>();
if(modalController==null || !modalController.TryOpenRecords()) throw new System.Exception("Could not open the production Records modal through its shared modal controller.");
var recordsModal=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.UI.VNRecordsModal>();
if(!recordsModal.IsOpen || recordsModal.ActiveTab!=ProjectAllTime.VN.Records.UI.VNRecordsTab.Timeline) throw new System.Exception("Records did not open on the Timeline tab.");
var original=Snapshot(); var cases=new[]{"prologue_s001","prologue_s001","prologue_s002","prologue_s002"};
int i=0,phase=0; double started=UnityEditor.EditorApplication.timeSinceStartup, last=0; int clicks=0;
System.IO.File.WriteAllText(path,"START\n");
UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=> { try {
  if (!UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.timeSinceStartup-started>75) throw new System.Exception("timeout " + i);
  if(i>=cases.Length) { System.IO.File.AppendAllText(path,"DONE\n"); UnityEditor.EditorApplication.update-=tick; return; }
  if(phase==0) { if(!rc.TryStartReplay(cases[i])) throw new System.Exception("start: "+rc.LastDiagnostic); phase=1; clicks=0; last=UnityEditor.EditorApplication.timeSinceStartup; return; }
  if(phase==1) { var v=UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.Replay.UI.VNReplayView>(); if(!rc.HasActiveSession){phase=2; return;} if(v==null || UnityEditor.EditorApplication.timeSinceStartup-last<0.4) return; last=UnityEditor.EditorApplication.timeSinceStartup; if(i%2==1) {v.CloseButton.onClick.Invoke(); phase=2;} else {v.NextButton.onClick.Invoke(); clicks++;} return; }
  if(rc.HasActiveSession)return;
  if(Snapshot()!=original) throw new System.Exception("production state changed during "+cases[i]);
  System.IO.File.AppendAllText(path,"PASS "+cases[i]+(i%2==0?" completed":" cancelled")+" clicks="+clicks+"\n"); i++; phase=0;
} catch(System.Exception e) {System.IO.File.AppendAllText(path,"FAIL "+e+"\n"); UnityEditor.EditorApplication.update-=tick;} };
UnityEditor.EditorApplication.update+=tick;
return path;
