var session = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNDialogueSessionState>();
var bridge = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNLineAdvancerInputBridge>();
var profile = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNProfileBrowser>();
var panels = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNProloguePanels>();
var saves = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.SaveLoad.VNSaveLoadController>();
var isolatedRoot = System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Live Play Mode required.");
if (string.IsNullOrWhiteSpace(isolatedRoot) || !string.Equals(System.IO.Path.GetFullPath(saves.Repository.StorageRoot), System.IO.Path.GetFullPath(System.IO.Path.Combine(isolatedRoot, "SaveData")), System.StringComparison.OrdinalIgnoreCase)) throw new System.Exception("Unexpected SaveData root.");
if (session.CurrentLineId != "line:prologue_s007_008") throw new System.Exception("Expected approved S007 exact-line start, got " + session.CurrentLineId);
if (!UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.Replay.UI.VNReplayController>(UnityEngine.FindObjectsInactive.Include).HasActiveSession) { } else throw new System.Exception("Orphan Replay session before S008 progression.");
var outputPath = System.IO.Path.Combine(isolatedRoot, "end_title.txt");
System.IO.File.WriteAllText(outputPath, "START " + session.CurrentLineId + "\n");
int advances = 0, profilesInspected = 0; bool sawS008 = false, sawProfileBrowser = false; string priorLine = session.CurrentLineId;
double started = UnityEditor.EditorApplication.timeSinceStartup, lastAction = 0;
UnityEditor.EditorApplication.CallbackFunction tick = null;
tick = () => { try {
    if (!UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.timeSinceStartup - started > 240) throw new System.Exception("S008/end-title progression timed out at " + session.CurrentLineId + " advances=" + advances);
    if (!string.Equals(session.CurrentLineId, priorLine, System.StringComparison.Ordinal)) { priorLine = session.CurrentLineId; System.IO.File.AppendAllText(outputPath, "LINE " + priorLine + "\n"); if (priorLine.StartsWith("line:prologue_s008_", System.StringComparison.Ordinal)) sawS008 = true; }
    if (profile.IsBrowsing) {
        sawProfileBrowser = true;
        if (UnityEditor.EditorApplication.timeSinceStartup - lastAction < 0.3) return;
        lastAction = UnityEditor.EditorApplication.timeSinceStartup;
        if (profile.IsOverview) {
            if (profile.AllReviewed) { if (!profile.TryContinue()) throw new System.Exception("Profile browser refused Continue after all three profiles were reviewed."); System.IO.File.AppendAllText(outputPath, "PROFILE_CONTINUED\n"); }
            else { for (int k = 0; k < 3; k++) if (!profile.HasReviewed(k)) { if (!profile.Inspect(k)) throw new System.Exception("Profile Inspect failed at " + k); profilesInspected++; System.IO.File.AppendAllText(outputPath, "PROFILE_INSPECT " + k + " pending=" + profile.IsReviewPending + "\n"); break; } }
        } else if (!profile.IsReviewPending) { if (!profile.Back()) throw new System.Exception("Profile Back failed."); }
        else if (session.IsLineActive && session.IsCurrentLineFullyDisplayed && bridge.TryAdvance(ProjectAllTime.VN.Dialogue.VNAdvanceSource.Manual)) { advances++; System.IO.File.AppendAllText(outputPath, "PROFILE_REVIEW_ADVANCE " + session.CurrentLineId + "\n"); }
        return;
    }
    var title = (UnityEngine.CanvasGroup)new UnityEditor.SerializedObject(panels).FindProperty("endTitle").objectReferenceValue;
    var diagram = (UnityEngine.CanvasGroup)new UnityEditor.SerializedObject(panels).FindProperty("diagram").objectReferenceValue;
    if (title.alpha > 0.99f) {
        if (title.interactable || title.blocksRaycasts || diagram.alpha > 0.01f) throw new System.Exception("End title transient state is invalid.");
        if (!sawS008 || !sawProfileBrowser || profilesInspected != 3) throw new System.Exception("Flow missed S008/profile path: S008=" + sawS008 + " browser=" + sawProfileBrowser + " inspected=" + profilesInspected);
        System.IO.File.AppendAllText(outputPath, "PASS end-title line=" + session.CurrentLineId + " advances=" + advances + " inspected=" + profilesInspected + " diagramHidden=" + (diagram.alpha < 0.01f) + "\nDONE\n"); UnityEditor.EditorApplication.update -= tick; return;
    }
    if (UnityEditor.EditorApplication.timeSinceStartup - lastAction < 0.3) return;
    lastAction = UnityEditor.EditorApplication.timeSinceStartup;
    if (session.IsLineActive && session.IsCurrentLineFullyDisplayed && bridge.TryAdvance(ProjectAllTime.VN.Dialogue.VNAdvanceSource.Manual)) advances++;
} catch (System.Exception e) { System.IO.File.AppendAllText(outputPath, "FAIL " + e + "\n"); UnityEditor.EditorApplication.update -= tick; } };
UnityEditor.EditorApplication.update += tick;
return outputPath;


