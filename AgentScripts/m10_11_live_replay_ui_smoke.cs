var rc = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.Replay.UI.VNReplayController>();
var modal = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.UI.VNRecordsModal>();
var timeline = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.Timeline.UI.VNTimelineView>();
var sessionState = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNDialogueSessionState>();
var runner = UnityEngine.Object.FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
var progress = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.MetaProgress.VNMetaProgressRuntimeBootstrap>().MetaProgressService;
var presentation = UnityEngine.Object.FindObjectsByType<ProjectAllTime.VN.Presentation.VNPresentationController>(UnityEngine.FindObjectsSortMode.None).Single(p => p.name == "PresentationRuntime");
var audio = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Audio.VNAudioController>();
var saves = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.SaveLoad.VNSaveLoadController>();
var isolatedRoot = System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
if (string.IsNullOrWhiteSpace(isolatedRoot) || !System.IO.Directory.Exists(System.IO.Path.Combine(isolatedRoot, "SaveData"))) throw new System.Exception("Isolated SaveData root is required.");
if (!modal.IsOpen || modal.ActiveTab != ProjectAllTime.VN.Records.UI.VNRecordsTab.Timeline) throw new System.Exception("Open Timeline tab is required.");
var row = timeline.GetComponentsInChildren<ProjectAllTime.VN.Records.Timeline.UI.VNTimelineEntryItem>(true).FirstOrDefault(item => { var button = new UnityEditor.SerializedObject(item).FindProperty("replayButton").objectReferenceValue as UnityEngine.UI.Button; return item.gameObject.activeInHierarchy && button != null && button.interactable; });
if (row == null) throw new System.Exception("No completed production Timeline Replay button is available.");
var rowButton = (UnityEngine.UI.Button)new UnityEditor.SerializedObject(row).FindProperty("replayButton").objectReferenceValue;
var outputPath = System.IO.Path.Combine(isolatedRoot, "replay_ui_smoke.txt");
var key = new ProjectAllTime.VN.SaveLoad.VNSaveSlotKey(ProjectAllTime.VN.SaveLoad.VNSaveSlotType.Manual, 2);
if (!saves.Repository.TryGetSlotPath(key, out var savePath) || !System.IO.File.Exists(savePath)) throw new System.Exception("Isolated Manual slot 2 is required for Replay isolation snapshot.");
string Snapshot() { presentation.TryCaptureStableState(out var p, out _); audio.TryCaptureStableState(out var a, out _); ProjectAllTime.VN.SaveLoad.VNYarnVariableSnapshot.TryCapture(runner.VariableStorage, out var v, out _); using var sha = System.Security.Cryptography.SHA256.Create(); var hash = System.Convert.ToBase64String(sha.ComputeHash(System.IO.File.ReadAllBytes(savePath))); return sessionState.CurrentLineId + "|" + sessionState.Backlog.Count + "|" + UnityEngine.JsonUtility.ToJson(progress.Current) + "|" + UnityEngine.JsonUtility.ToJson(p) + "|" + a.bgmId + "|" + UnityEngine.JsonUtility.ToJson(v) + "|" + saves.IsLoadInProgress + "|" + hash; }
var original = Snapshot();
int phase = 0, clicks = 0;
double started = UnityEditor.EditorApplication.timeSinceStartup, last = 0;
System.IO.File.WriteAllText(outputPath, "START " + row.name + "\n");
UnityEditor.EditorApplication.CallbackFunction tick = null;
tick = () => { try {
    if (!UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.timeSinceStartup - started > 75) throw new System.Exception("Replay UI smoke timed out in phase " + phase);
    if (phase == 0) { rowButton.onClick.Invoke(); if (!rc.HasActiveSession) throw new System.Exception("Timeline Replay button did not start Replay: " + rc.LastDiagnostic); phase = 1; last = UnityEditor.EditorApplication.timeSinceStartup; System.IO.File.AppendAllText(outputPath, "STARTED_FROM_TIMELINE_BUTTON\n"); return; }
    if (phase == 1) { var view = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.Replay.UI.VNReplayView>(); if (view == null || UnityEditor.EditorApplication.timeSinceStartup - last < 0.4) return; view.CloseButton.onClick.Invoke(); phase = 2; return; }
    if (phase == 2) { if (rc.HasActiveSession) return; if (Snapshot() != original) throw new System.Exception("Production state changed after Replay cancellation."); System.IO.File.AppendAllText(outputPath, "PASS cancellation preserved production state\n"); phase = 3; last = UnityEditor.EditorApplication.timeSinceStartup; return; }
    if (phase == 3) { rowButton.onClick.Invoke(); if (!rc.HasActiveSession) throw new System.Exception("Timeline Replay button could not restart after cancellation: " + rc.LastDiagnostic); phase = 4; clicks = 0; last = UnityEditor.EditorApplication.timeSinceStartup; return; }
    if (phase == 4) { if (!rc.HasActiveSession) { phase = 5; return; } var view = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.Replay.UI.VNReplayView>(); if (view == null || UnityEditor.EditorApplication.timeSinceStartup - last < 0.5) return; last = UnityEditor.EditorApplication.timeSinceStartup; view.NextButton.onClick.Invoke(); clicks++; if (clicks > 8) throw new System.Exception("Replay required too many Next actions."); return; }
    if (phase == 5) { if (rc.HasActiveSession) return; if (Snapshot() != original) throw new System.Exception("Production state changed after Replay completion."); System.IO.File.AppendAllText(outputPath, "PASS normal completion preserved production state; Next clicks=" + clicks + "\nDONE\n"); UnityEditor.EditorApplication.update -= tick; return; }
} catch (System.Exception e) { System.IO.File.AppendAllText(outputPath, "FAIL " + e + "\n"); UnityEditor.EditorApplication.update -= tick; } };
UnityEditor.EditorApplication.update += tick;
return outputPath;
