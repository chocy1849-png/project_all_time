var root = System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
var session = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNDialogueSessionState>();
var saves = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.SaveLoad.VNSaveLoadController>();
var progress = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.MetaProgress.VNMetaProgressRuntimeBootstrap>().MetaProgressService;
var modalController = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNConvenienceModalController>();
var runner = UnityEngine.Object.FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
var slot = new ProjectAllTime.VN.SaveLoad.VNSaveSlotKey(ProjectAllTime.VN.SaveLoad.VNSaveSlotType.Manual, 0);
if (!UnityEditor.EditorApplication.isPlaying || session == null || saves == null || progress == null || modalController == null || !runner.IsDialogueRunning)
    throw new System.Exception("A live production story session is required.");
if (!string.Equals(System.IO.Path.GetFullPath(saves.Repository.StorageRoot), System.IO.Path.GetFullPath(System.IO.Path.Combine(root, "SaveData")), System.StringComparison.OrdinalIgnoreCase))
    throw new System.Exception("SaveData root is not isolated.");
if (!modalController.CloseActiveModal() && modalController.IsConvenienceModalOpen)
    throw new System.Exception("Records modal could not release its interaction lease before Load.");
var before = UnityEngine.JsonUtility.ToJson(progress.Current);
var beforeData = progress.Current;
var report = System.IO.Path.Combine(root, "Reports", "m10_12_meta_load_restart.txt");
var baseline = System.IO.Path.Combine(root, "Reports", "m10_12_meta_before_restart.json");
System.IO.File.WriteAllText(report, "BEFORE\treads=" + beforeData.readLineIds.Length + "\n");
var startedAt = UnityEditor.EditorApplication.timeSinceStartup;
var requested = saves.Load(slot);
if (!requested.Succeeded) throw new System.Exception("Earlier manual save was not loadable: " + requested.Message);
UnityEditor.EditorApplication.CallbackFunction check = null;
check = () => {
    try {
        if (!UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.timeSinceStartup - startedAt > 30)
            throw new System.Exception("Earlier save load did not settle.");
        if (saves.IsLoadInProgress || session.CurrentLineId != "line:prologue_s001_016" || !session.IsCurrentLineFullyDisplayed) return;
        var after = UnityEngine.JsonUtility.ToJson(progress.Current);
        if (!string.Equals(before, after, System.StringComparison.Ordinal))
            throw new System.Exception("Durable MetaProgress changed during Load.");
        System.IO.File.WriteAllText(baseline, before);
        System.IO.File.AppendAllText(report, "PASS\tloaded=" + session.CurrentLineId + "\treads=" + progress.Current.readLineIds.Length + "\tunlocksUnchanged=true\tloadLeaseReleased=" + !saves.IsLoadInProgress + "\nDONE\n");
        UnityEditor.EditorApplication.update -= check;
    } catch (System.Exception exception) {
        System.IO.File.AppendAllText(report, "FAIL\t" + System.Uri.EscapeDataString(exception.ToString()) + "\n");
        UnityEditor.EditorApplication.update -= check;
    }
};
UnityEditor.EditorApplication.update += check;
return report;
