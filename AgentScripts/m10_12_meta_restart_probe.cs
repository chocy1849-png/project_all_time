var root = System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
var bootstrap = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.MetaProgress.VNMetaProgressRuntimeBootstrap>();
var saves = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.SaveLoad.VNSaveLoadController>();
if (!UnityEditor.EditorApplication.isPlaying || bootstrap == null || !bootstrap.IsInitialized || saves == null)
    throw new System.Exception("The restarted production runtime has not initialized.");
var metaRepository = (ProjectAllTime.VN.MetaProgress.VNMetaProgressRepository)typeof(ProjectAllTime.VN.MetaProgress.VNMetaProgressRuntimeBootstrap)
    .GetField("repository", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(bootstrap);
var expectedPath = System.IO.Path.Combine(root, "Reports", "m10_12_meta_before_restart.json");
var expected = System.IO.File.ReadAllText(expectedPath);
var actual = UnityEngine.JsonUtility.ToJson(bootstrap.MetaProgressService.Current);
var saveSlot = new ProjectAllTime.VN.SaveLoad.VNSaveSlotKey(ProjectAllTime.VN.SaveLoad.VNSaveSlotType.Manual, 0);
var slotRead = saves.Repository.Read(saveSlot);
if (!string.Equals(System.IO.Path.GetFullPath(metaRepository.StorageRoot), System.IO.Path.GetFullPath(System.IO.Path.Combine(root, "MetaProgress")), System.StringComparison.OrdinalIgnoreCase))
    throw new System.Exception("Restarted MetaProgress repository is not isolated.");
if (!string.Equals(actual, expected, System.StringComparison.Ordinal)) throw new System.Exception("MetaProgress did not survive the Play Mode restart unchanged.");
if (slotRead.State != ProjectAllTime.VN.SaveLoad.VNSaveSlotState.Valid) throw new System.Exception("Earlier real Manual save did not remain valid after restart: " + slotRead.State);
var report = System.IO.Path.Combine(root, "Reports", "m10_12_meta_load_restart.txt");
System.IO.File.AppendAllText(report, "PASS_RESTART\treads=" + bootstrap.MetaProgressService.Current.readLineIds.Length + "\tmetaExact=true\tmanualSlot0=" + slotRead.State + "\nDONE_RESTART\n");
return report;
