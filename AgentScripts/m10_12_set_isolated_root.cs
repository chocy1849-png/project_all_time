var root = System.IO.Path.GetFullPath(System.IO.Path.Combine(
    System.IO.Path.GetTempPath(), "project_all_time_m10_12_43oxvqoa", "SaveLoad"));
System.IO.Directory.CreateDirectory(System.IO.Path.Combine(root, "Reports"));
System.Environment.SetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT", root, System.EnvironmentVariableTarget.Process);
if (!string.Equals(System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT"), root, System.StringComparison.OrdinalIgnoreCase))
    throw new System.Exception("Could not set process-scoped validation root.");
return root;
