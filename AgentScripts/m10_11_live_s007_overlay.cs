var records = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Records.UI.VNRecordsModal>();
if (records.IsOpen) { var close = (UnityEngine.UI.Button)new UnityEditor.SerializedObject(records).FindProperty("closeButton").objectReferenceValue; close.onClick.Invoke(); }
var modalController = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNConvenienceModalController>();
if (modalController.IsConvenienceModalOpen) throw new System.Exception("Records modal did not release ownership before Save/Load.");
var saves = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.SaveLoad.VNSaveLoadController>();
var session = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNDialogueSessionState>();
var overlay = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNInformationOverlay>();
var presentation = UnityEngine.Object.FindObjectsByType<ProjectAllTime.VN.Presentation.VNPresentationController>(UnityEngine.FindObjectsSortMode.None).Single(p => p.name == "PresentationRuntime");
var isolatedRoot = System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
if (string.IsNullOrWhiteSpace(isolatedRoot) || !string.Equals(System.IO.Path.GetFullPath(saves.Repository.StorageRoot), System.IO.Path.GetFullPath(System.IO.Path.Combine(isolatedRoot, "SaveData")), System.StringComparison.OrdinalIgnoreCase)) throw new System.Exception("Unexpected SaveData root.");
var outputPath = System.IO.Path.Combine(isolatedRoot, "s007_overlay.txt");
var key = new ProjectAllTime.VN.SaveLoad.VNSaveSlotKey(ProjectAllTime.VN.SaveLoad.VNSaveSlotType.Manual, 4);
var result = saves.Load(key);
if (!result.Succeeded) throw new System.Exception("Load S007 slot failed: " + result.Message);
double start = UnityEditor.EditorApplication.timeSinceStartup;
UnityEditor.EditorApplication.CallbackFunction tick = null;
tick = () => { try {
  if (!UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.timeSinceStartup - start > 25) throw new System.Exception("S007 restore timed out: " + session.CurrentLineId);
  if (saves.IsLoadInProgress || session.CurrentLineId != "line:prologue_s007_008" || !session.IsCurrentLineFullyDisplayed) return;
  if (!overlay.IsShown) throw new System.Exception("S007 information overlay was not restored.");
  if (!presentation.TryCaptureStableState(out var state, out var diagnostic)) throw new System.Exception(diagnostic);
  if (state.characters == null || !System.Linq.Enumerable.Any(state.characters, c => c.characterId == "dokyung")) throw new System.Exception("Expected S007 presenter is not present after supported exact-line load.");
  System.IO.File.WriteAllText(outputPath, "PASS line=" + session.CurrentLineId + " overlay=" + overlay.IsShown + " characters=" + state.characters.Length + " cg=" + state.cgId + "\n");
  UnityEditor.EditorApplication.update -= tick;
} catch (System.Exception e) { System.IO.File.WriteAllText(outputPath, "FAIL " + e + "\n"); UnityEditor.EditorApplication.update -= tick; } };
UnityEditor.EditorApplication.update += tick;
return outputPath;
