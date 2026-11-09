var root = System.Environment.GetEnvironmentVariable("PROJECT_ALL_TIME_TEST_ROOT");
if (string.IsNullOrWhiteSpace(root)) throw new System.Exception("PROJECT_ALL_TIME_TEST_ROOT must point at the isolated validation root.");
var runner = UnityEngine.Object.FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
var session = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNDialogueSessionState>();
var bridge = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Dialogue.VNLineAdvancerInputBridge>();
var saves = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.SaveLoad.VNSaveLoadController>();
var bootstrap = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.MetaProgress.VNMetaProgressRuntimeBootstrap>();
var profile = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNProfileBrowser>();
var panels = UnityEngine.Object.FindFirstObjectByType<ProjectAllTime.VN.Presentation.VNProloguePanels>();
if (!UnityEditor.EditorApplication.isPlaying || runner == null || session == null || bridge == null || saves == null || bootstrap == null || profile == null || panels == null)
    throw new System.Exception("The live VN_Main production runtime is incomplete.");
var expectedRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, "SaveData"));
if (!string.Equals(System.IO.Path.GetFullPath(saves.Repository.StorageRoot), expectedRoot, System.StringComparison.OrdinalIgnoreCase))
    throw new System.Exception("Save repository is not isolated: " + saves.Repository.StorageRoot);
var metaRepository = (ProjectAllTime.VN.MetaProgress.VNMetaProgressRepository)typeof(ProjectAllTime.VN.MetaProgress.VNMetaProgressRuntimeBootstrap)
    .GetField("repository", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(bootstrap);
if (metaRepository == null || !string.Equals(System.IO.Path.GetFullPath(metaRepository.StorageRoot), System.IO.Path.GetFullPath(System.IO.Path.Combine(root, "MetaProgress")), System.StringComparison.OrdinalIgnoreCase))
    throw new System.Exception("MetaProgress repository is not isolated: " + metaRepository?.StorageRoot);
var manifest = UnityEngine.Resources.Load<UnityEngine.TextAsset>("M10_09_LINE_RESUME_MANIFEST");
if (manifest == null) throw new System.Exception("Production resume manifest is missing at runtime.");
var matches = System.Text.RegularExpressions.Regex.Matches(manifest.text, "\\\"runtimeLineId\\\"\\s*:\\s*\\\"(?<id>line:prologue_s[0-9]+_[0-9]+)\\\"");
var expected = new string[matches.Count];
for (int i = 0; i < matches.Count; i++) expected[i] = matches[i].Groups["id"].Value;
if (expected.Length != 379 || System.Linq.Enumerable.Count(System.Linq.Enumerable.Distinct(expected)) != expected.Length)
    throw new System.Exception("Expected 379 unique canonical runtime line IDs; got " + expected.Length);
var report = System.IO.Path.Combine(root, "Reports", "m10_12_continuous_story.tsv");
System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(report));
System.IO.File.WriteAllText(report, "START\tPROLOGUE_S001_START\tmanifestLines=" + expected.Length + "\n");
long lastOccurrence = -1;
long capturedOccurrence = -1;
int captured = 0;
int profileActions = 0;
double startedAt = UnityEditor.EditorApplication.timeSinceStartup;
double lastProfileAction = 0;
double timeoutAt = startedAt + 7200;
bool startRequested = false;
string failure = null;
UnityEditor.EditorApplication.CallbackFunction step = null;
step = () => {
    try {
        if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Play Mode stopped during the continuous story run.");
        if (UnityEditor.EditorApplication.timeSinceStartup > timeoutAt) throw new System.Exception("Continuous story timed out at " + captured + "/" + expected.Length + " line=" + session.CurrentLineId);
        if (!startRequested) {
            session.ClearSession();
            runner.Stop().Forget();
            startRequested = true;
            startedAt = UnityEditor.EditorApplication.timeSinceStartup;
            return;
        }
        if (!runner.IsDialogueRunning) {
            if (captured == expected.Length) {
                var so = new UnityEditor.SerializedObject(panels);
                var title = (UnityEngine.CanvasGroup)so.FindProperty("endTitle").objectReferenceValue;
                var diagram = (UnityEngine.CanvasGroup)so.FindProperty("diagram").objectReferenceValue;
                if (title == null || title.alpha < 0.99f || title.interactable || title.blocksRaycasts || diagram == null || diagram.alpha > 0.01f)
                    throw new System.Exception("All 379 paragraphs were observed, but the terminal title/panel state was invalid.");
                System.IO.File.AppendAllText(report, "PASS\tparagraphs=" + captured + "\tprofileActions=" + profileActions + "\tendTitle=true\tdiagramHidden=true\nDONE\n");
                UnityEditor.EditorApplication.update -= step;
                return;
            }
            if (!startRequested || UnityEditor.EditorApplication.timeSinceStartup - startedAt < 0.25) return;
            runner.StartDialogue("PROLOGUE_S001_START").Forget();
            System.IO.File.AppendAllText(report, "ENTRY\tPROLOGUE_S001_START\n");
            startedAt = UnityEditor.EditorApplication.timeSinceStartup;
            return;
        }
        if (session.OptionsActive) throw new System.Exception("Unexpected route choice/options during the canonical prologue.");
        if (profile.IsBrowsing && UnityEditor.EditorApplication.timeSinceStartup - lastProfileAction >= 0.2) {
            lastProfileAction = UnityEditor.EditorApplication.timeSinceStartup;
            if (profile.IsOverview) {
                if (profile.AllReviewed) {
                    if (!profile.TryContinue()) throw new System.Exception("Profile browser refused Continue after all profiles were reviewed.");
                    System.IO.File.AppendAllText(report, "PROFILE\tcontinue\n");
                    profileActions++;
                } else {
                    for (int i = 0; i < 3; i++) if (!profile.HasReviewed(i)) {
                        if (!profile.Inspect(i)) throw new System.Exception("Profile Inspect failed for " + i);
                        System.IO.File.AppendAllText(report, "PROFILE\tinspect=" + i + "\n");
                        profileActions++;
                        break;
                    }
                }
            } else if (!profile.IsReviewPending) {
                if (!profile.Back()) throw new System.Exception("Profile Back failed.");
                profileActions++;
            }
        }
        if (session.IsLineActive && session.CurrentLineId.StartsWith("line:prologue_s", System.StringComparison.Ordinal)) {
            var occurrence = session.CurrentPresentationOccurrence;
            if (occurrence != lastOccurrence) {
                lastOccurrence = occurrence;
                if (captured >= expected.Length) throw new System.Exception("Unexpected additional prologue paragraph: " + session.CurrentLineId);
                if (!string.Equals(session.CurrentLineId, expected[captured], System.StringComparison.Ordinal))
                    throw new System.Exception("Canonical order mismatch at " + captured + ": actual=" + session.CurrentLineId + " expected=" + expected[captured]);
            }
            if (session.IsCurrentLineFullyDisplayed && occurrence != capturedOccurrence) {
                if (!ProjectAllTime.VN.Dialogue.VNAuthoritativeLinePresenterResolver.TryResolve(runner, out var linePresenter, out var diagnostic))
                    throw new System.Exception("Authoritative LinePresenter unavailable: " + diagnostic);
                var shown = linePresenter.lineText == null ? null : linePresenter.lineText.text;
                var canonicalDisplay = session.CurrentText ?? string.Empty;
                if (linePresenter.characterNameText == null && linePresenter.showCharacterNameInLine && session.CurrentSpeakerName.Length > 0)
                    canonicalDisplay = session.CurrentSpeakerName + ": " + canonicalDisplay;
                if (!string.Equals(shown, canonicalDisplay, System.StringComparison.Ordinal))
                    throw new System.Exception("Displayed text diverged from the active Yarn line at " + session.CurrentLineId + ": displayed=" + shown + " session=" + canonicalDisplay);
                var speaker = System.Uri.EscapeDataString(session.CurrentSpeakerName ?? string.Empty);
                var body = System.Uri.EscapeDataString(session.CurrentText ?? string.Empty);
                System.IO.File.AppendAllText(report, "LINE\t" + session.CurrentLineId + "\t" + speaker + "\t" + body + "\n");
                capturedOccurrence = occurrence;
                captured++;
            }
            bridge.TryAdvance(ProjectAllTime.VN.Dialogue.VNAdvanceSource.Manual);
        }
        if (!runner.IsDialogueRunning && captured < expected.Length) {
            if (profile.IsBrowsing) return;
            throw new System.Exception("Dialogue ended before all paragraphs. Captured " + captured + "/" + expected.Length);
        }
    } catch (System.Exception exception) {
        failure = exception.ToString();
        System.IO.File.AppendAllText(report, "FAIL\t" + System.Uri.EscapeDataString(failure) + "\n");
        UnityEditor.EditorApplication.update -= step;
    }
};
UnityEditor.EditorApplication.update += step;
return report;
