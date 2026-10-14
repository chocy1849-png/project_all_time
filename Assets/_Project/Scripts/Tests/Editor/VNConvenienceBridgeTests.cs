using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.SaveLoad;
using ProjectAllTime.VN.Presentation;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNConvenienceBridgeTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private VNDialogueSessionState sessionState;
        private VNLineLifecyclePresenter lifecycle;
        private VNLineLifecycleMarkupHandler markupHandler;
        private LinePresenter linePresenter;
        private TextMeshProUGUI lineText;
        private DialogueRunner dialogueRunner;
        private VNInteractionGate gate;
        private VNLineAdvancerInputBridge bridge;
        private VNConvenienceController convenience;
        private VNUIVisibilityController visibility;
        private CanvasGroup dialogueLayer;
        private CanvasGroup quickControlLayer;
        private int forwardedCount;

        [SetUp]
        public void SetUp()
        {
            forwardedCount = 0;
            var root = new GameObject("M6-04 Convenience Bridge Test");
            ownedObjects.Add(root);
            var lineAdvancer = root.AddComponent<LineAdvancer>();
            lineAdvancer.enabled = false;
            sessionState = root.AddComponent<VNDialogueSessionState>();
            lifecycle = root.AddComponent<VNLineLifecyclePresenter>();
            markupHandler = root.AddComponent<VNLineLifecycleMarkupHandler>();
            linePresenter = root.AddComponent<LinePresenter>();
            dialogueRunner = root.AddComponent<DialogueRunner>();
            var textObject = new GameObject("M6 Visual Text");
            ownedObjects.Add(textObject);
            textObject.AddComponent<Canvas>();
            lineText = textObject.AddComponent<TextMeshProUGUI>();
            lineText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            linePresenter.lineText = lineText;
            linePresenter.characterNameText = lineText;
            dialogueRunner.DialoguePresenters = new DialoguePresenterBase[] { linePresenter };
            gate = root.AddComponent<VNInteractionGate>();
            bridge = root.AddComponent<VNLineAdvancerInputBridge>();
            visibility = root.AddComponent<VNUIVisibilityController>();
            convenience = root.AddComponent<VNConvenienceController>();

            dialogueLayer = CreateCanvasGroup("Dialogue Layer", 0.65f, false, true);
            quickControlLayer = CreateCanvasGroup("QuickControl Layer", 0.9f, true, false);
            SetPrivateField(lifecycle, "sessionState", sessionState);
            SetPrivateField(lifecycle, "linePresenter", linePresenter);
            SetPrivateField(markupHandler, "lifecyclePresenter", lifecycle);
            SetPrivateField(gate, "sessionState", sessionState);
            SetPrivateField(bridge, "sessionState", sessionState);
            SetPrivateField(bridge, "interactionGate", gate);
            SetPrivateField(visibility, "dialogueLayer", dialogueLayer);
            SetPrivateField(visibility, "quickControlLayer", quickControlLayer);
            SetPrivateField(visibility, "interactionGate", gate);
            SetPrivateField(convenience, "sessionState", sessionState);
            SetPrivateField(convenience, "advanceBridge", bridge);
            SetPrivateField(convenience, "interactionGate", gate);
            SetPrivateField(convenience, "uiVisibilityController", visibility);
            bridge.AdvanceForwarded += _ => forwardedCount++;
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
        }

        [Test]
        public void ManualNext_UsesSharedBridge_WhenVisible_AndRejectsBlockedOrOptions()
        {
            Present("manual", "Manual bridge line.");
            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(sessionState.ReadHistory.IsRead("manual"), Is.False);

            gate.SetUiHidden(true);
            Assert.That(convenience.HandleManualAdvance(), Is.False);
            Assert.That(forwardedCount, Is.EqualTo(1));
            gate.SetUiHidden(false);

            lifecycle.RunOptionsAsync(Array.Empty<DialogueOption>(), NewLineToken());
            Assert.That(convenience.HandleManualAdvance(), Is.False);
            Assert.That(forwardedCount, Is.EqualTo(1));
        }

        [Test]
        public void Hide_Show_CapturesCanvasGroups_AndDoesNotDeactivateLayers()
        {
            Present("hide", "Hide while typewriting is allowed.");
            Assert.That(visibility.TryHideUi(), Is.True);
            AssertHidden(dialogueLayer);
            AssertHidden(quickControlLayer);
            Assert.That(dialogueLayer.gameObject.activeSelf, Is.True);
            Assert.That(quickControlLayer.gameObject.activeSelf, Is.True);
            Assert.That(gate.IsUiHidden, Is.True);
            Assert.That(visibility.TryHideUi(), Is.True, "Hide is idempotent.");

            Assert.That(visibility.ShowUi(), Is.True);
            Assert.That(dialogueLayer.alpha, Is.EqualTo(0.65f));
            Assert.That(dialogueLayer.interactable, Is.False);
            Assert.That(dialogueLayer.blocksRaycasts, Is.True);
            Assert.That(quickControlLayer.alpha, Is.EqualTo(0.9f));
            Assert.That(quickControlLayer.interactable, Is.True);
            Assert.That(quickControlLayer.blocksRaycasts, Is.False);
            Assert.That(gate.IsUiHidden, Is.False);
            Assert.That(visibility.ShowUi(), Is.True, "Show is idempotent.");
        }

        [Test]
        public void Hide_IsRejectedForMissingGroup_Options_AndInactivePresentation()
        {
            Assert.That(visibility.TryHideUi(), Is.False, "A command-only/no-line interval cannot hide UI.");
            Present("options", "Choose.");
            lifecycle.RunOptionsAsync(Array.Empty<DialogueOption>(), NewLineToken());
            Assert.That(visibility.TryHideUi(), Is.False);

            lifecycle.OnDialogueStartedAsync();
            Present("missing", "Missing reference is safe.");
            SetPrivateField(visibility, "quickControlLayer", null);
            LogAssert.Expect(LogType.Error, new Regex("VNUIVisibilityController requires"));
            Assert.That(visibility.TryHideUi(), Is.False);
            Assert.That(dialogueLayer.alpha, Is.EqualTo(0.65f), "No partial hide is applied.");
            Assert.That(gate.IsUiHidden, Is.False);
        }

        [Test]
        public void HiddenManualAdvance_RestoresOnly_ThenALaterAdvanceMayConsumeRead()
        {
            Present("hidden-read", "A completed line.");
            CompleteDisplay();
            Assert.That(convenience.TryHideUi(), Is.True);

            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(visibility.IsUiHidden, Is.False);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(sessionState.ReadHistory.IsRead("hidden-read"), Is.False);

            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(sessionState.ReadHistory.IsRead("hidden-read"), Is.True);
        }

        [Test]
        public void Hide_SuspendsAutoAndSkipWithoutDisablingTheirLogicalState()
        {
            Present("auto", "Automation remains selected.");
            convenience.SetAutoEnabled(true);
            Assert.That(convenience.TryHideUi(), Is.True);
            Assert.That(convenience.IsAutoEnabled, Is.True);
            Assert.That(convenience.IsSkipEnabled, Is.False);
            Assert.That(gate.CanRunAutomation, Is.False);
            Assert.That(convenience.ShowUi(), Is.True);
            Assert.That(convenience.IsAutoEnabled, Is.True);

            convenience.SetSkipEnabled(true);
            Assert.That(convenience.TryHideUi(), Is.True);
            Assert.That(convenience.IsSkipEnabled, Is.True);
            Assert.That(gate.CanRunAutomation, Is.False);
        }

        [Test]
        public void SaveLoadActions_AreGatedButRemainAvailableDuringOptions_AndPointerSeamDelegates()
        {
            var saveLoad = CreateSaveLoadController();
            var statuses = new List<string>();
            saveLoad.StatusChanged += statuses.Add;
            SetPrivateField(gate, "saveLoadController", saveLoad);
            SetPrivateField(convenience, "saveLoadController", saveLoad);

            lifecycle.RunOptionsAsync(Array.Empty<DialogueOption>(), NewLineToken());
            Assert.That(convenience.OpenSave(), Is.True);
            Assert.That(convenience.OpenLoad(), Is.True);
            Assert.That(convenience.BeginSaveLoadOpenerInputSuppression(), Is.True);
            Assert.That(statuses.Count, Is.GreaterThanOrEqualTo(3));

            gate.SetUiHidden(true);
            Assert.That(convenience.OpenSave(), Is.False);
            Assert.That(convenience.QuickSave().Status, Is.EqualTo(VNSaveLoadOperationStatus.Busy));
            Assert.That(convenience.QuickLoad().Status, Is.EqualTo(VNSaveLoadOperationStatus.Busy));
        }

        [Test]
        public void FailedQuickLoad_DoesNotNormalizeModes_ButAuthoritativeLoadStartDoes()
        {
            var saveLoad = CreateSaveLoadController();
            SetPrivateField(gate, "saveLoadController", saveLoad);
            SetPrivateField(convenience, "saveLoadController", saveLoad);
            MarkRead("session", "Keep session services.");
            Present("transient", "Invalidate only this line.");
            convenience.SetAutoEnabled(true);
            Assert.That(convenience.QuickLoad().Succeeded, Is.False);
            Assert.That(convenience.IsAutoEnabled, Is.True);

            Assert.That(convenience.TryHideUi(), Is.True);
            var requested = 0;
            convenience.SafeManualStateRequested += () => requested++;
            InvokePrivate(convenience, "HandleLoadStateChanged", true);

            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(convenience.IsSkipEnabled, Is.False);
            Assert.That(visibility.IsUiHidden, Is.False);
            Assert.That(sessionState.IsLineActive, Is.False);
            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));
            Assert.That(sessionState.ReadHistory.IsRead("session"), Is.True);
            Assert.That(requested, Is.EqualTo(1));

            InvokePrivate(convenience, "HandleLoadStateChanged", false);
            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(convenience.IsSkipEnabled, Is.False);
        }

        [TestCase(false, VNSkipPolicy.ReadOnly)]
        [TestCase(false, VNSkipPolicy.All)]
        [TestCase(true, VNSkipPolicy.ReadOnly)]
        public void AuthoredHold_BlocksAutomationWithoutChangingModes_ThenResumesOnNextOccurrence(bool auto, VNSkipPolicy policy)
        {
            Present("held", "An authored manual beat.");
            sessionState.RequireManualAdvance();
            convenience.SetSkipPolicy(policy);
            if (auto) convenience.SetAutoEnabled(true); else convenience.SetSkipEnabled(true);
            Tick(0, 100); Tick(20, 101);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(auto ? convenience.IsAutoEnabled : convenience.IsSkipEnabled, Is.True);
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Auto), Is.False);
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Skip), Is.False);
            Assert.That(convenience.HandleManualAdvance(), Is.True, "Manual hurry remains available.");
            Assert.That(sessionState.IsManualAdvanceRequired, Is.True, "Hurry does not consume the hold.");
            CompleteDisplay();
            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(sessionState.IsManualAdvanceRequired, Is.False);
            Assert.That(sessionState.ReadHistory.IsRead("held"), Is.True);
            Present("held", "Same stable TextID, new occurrence."); CompleteDisplay();
            Tick(21, 102); Tick(22, 103); Tick(40, 104);
            Assert.That(forwardedCount, Is.EqualTo(3), "Exactly one automated consume follows the two manual requests.");
        }

        [Test]
        public void AuthoredHold_HiddenRestoreAndModalCannotConsume_LoadClearsAndReentryRearms()
        {
            Present("held-hidden", "Hold."); CompleteDisplay(); sessionState.RequireManualAdvance();
            Assert.That(visibility.TryHideUi(), Is.True);
            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(sessionState.IsManualAdvanceRequired, Is.True);
            gate.SetConvenienceModalActive(true);
            Assert.That(convenience.HandleManualAdvance(), Is.False);
            gate.SetConvenienceModalActive(false);
            Assert.That(sessionState.IsManualAdvanceRequired, Is.True);
            InvokePrivate(convenience, "HandleLoadStateChanged", true);
            Assert.That(sessionState.IsManualAdvanceRequired, Is.False);
            Present("held-hidden", "Hold."); sessionState.RequireManualAdvance(); CompleteDisplay();
            Assert.That(convenience.HandleManualAdvance(), Is.True);
            Assert.That(sessionState.IsManualAdvanceRequired, Is.False);
        }

        private void Tick(float time, int frame) => typeof(VNConvenienceController)
            .GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(convenience, new object[] { time, frame });

        [Test]
        public void InformationOverlay_HideModalAndLoadReentry_LeaveDialogueHistoryUntouched()
        {
            var save = CreateSaveLoadController();
            var panel = new GameObject("Technical facts"); ownedObjects.Add(panel); panel.SetActive(false);
            var group = panel.AddComponent<CanvasGroup>();
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)); ownedObjects.Add(title);
            var body = new GameObject("Facts", typeof(RectTransform), typeof(TextMeshProUGUI)); ownedObjects.Add(body);
            var overlay = panel.AddComponent<VNInformationOverlay>();
            SetPrivateField(overlay, "root", group); SetPrivateField(overlay, "titleText", title.GetComponent<TMP_Text>());
            SetPrivateField(overlay, "bodyText", body.GetComponent<TMP_Text>());
            SetPrivateField(overlay, "visibility", visibility); SetPrivateField(overlay, "saveLoad", save);
            panel.SetActive(true);
            InvokePrivateNoArguments(overlay, "OnEnable");
            Present("under-facts", "Underlying line.");
            Assert.That(overlay.Show("Technical", "Exact authored facts."), Is.True);
            Assert.That(body.GetComponent<TMP_Text>().text, Is.EqualTo("Exact authored facts."));
            Assert.That(group.blocksRaycasts, Is.False);
            Assert.That(gate.CanAdvanceStory, Is.True);
            visibility.TryHideUi(); Assert.That(group.alpha, Is.Zero);
            visibility.ShowUi(); Assert.That(group.alpha, Is.EqualTo(1));
            gate.SetConvenienceModalActive(true); Assert.That(overlay.IsShown, Is.True);
            gate.SetConvenienceModalActive(false);
            overlay.Show("Update", "Updated exact facts.");
            Assert.That(body.GetComponent<TMP_Text>().text, Is.EqualTo("Updated exact facts."));
            Assert.That(sessionState.Backlog.Count, Is.Zero); Assert.That(sessionState.ReadHistory.Count, Is.Zero);
            Assert.That(forwardedCount, Is.Zero);
            typeof(VNSaveLoadController).GetMethod("SetLoadInProgress", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(save, new object[] { true });
            Assert.That(overlay.IsShown, Is.False); Assert.That(group.alpha, Is.Zero);
            Assert.That(overlay.Show("Stale", "Must reject while loading."), Is.False);
            typeof(VNSaveLoadController).GetMethod("SetLoadInProgress", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(save, new object[] { false });
            Assert.That(overlay.Show("Reentry", "Reconstructed authored facts."), Is.True);
            Assert.That(body.GetComponent<TMP_Text>().text, Is.EqualTo("Reconstructed authored facts."));
            overlay.Hide(); Assert.That(group.alpha, Is.Zero);
            Assert.That(sessionState.Backlog.Count, Is.Zero); Assert.That(sessionState.ReadHistory.Count, Is.Zero);
        }

        [TestCase(2, 0, 1)]
        [TestCase(1, 2, 0)]
        public void ProfileBrowse_ArbitraryOrder_OneShotThoughts_SharedManualLine_AndSingleNeutralContinuation(int first, int second, int third)
        {
            var browser = CreateProfileBrowser(out _);
            var reviews = new List<int>(); var completed = 0;
            browser.ReviewRequested += reviews.Add; browser.Completed += () => completed++;
            Present("under-browser", "Must not leak."); CompleteDisplay();
            Assert.That(browser.BeginBrowse(Profiles()), Is.True);
            Assert.That(browser.IsOverview, Is.True);
            for (var index = 0; index < 3; index++) Assert.That(browser.HasReviewed(index), Is.False);
            Assert.That(browser.TryContinue(), Is.False);
            Assert.That(convenience.HandleManualAdvance(), Is.False);
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Auto), Is.False);
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Skip), Is.False);
            Assert.That(convenience.ToggleAuto(), Is.False); Assert.That(convenience.ToggleSkip(), Is.False);
            Assert.That(visibility.TryHideUi(), Is.False); Assert.That(gate.CanUseSaveLoad, Is.False);
            foreach (var index in new[] { first, second, third })
            {
                Assert.That(browser.Inspect(index), Is.True);
                if (index != 2)
                {
                    Assert.That(browser.IsReviewPending, Is.True);
                    Assert.That(convenience.HandleManualAdvance(), Is.False, "Pending review cannot consume the previous underlying occurrence.");
                    Assert.That(browser.Back(), Is.False, "Do not abandon an ordinary first-review thought mid-line.");
                    Present("technical-profile-" + index, "Technical first-review thought."); CompleteDisplay();
                    Assert.That(bridge.TryAdvance(VNAdvanceSource.Auto), Is.False);
                    Assert.That(convenience.HandleManualAdvance(), Is.True, "Owned ordinary LinePresenter reading is manual only.");
                    Assert.That(convenience.HandleManualAdvance(), Is.False, "The review window closes on accepted consume.");
                    Assert.That(browser.CompleteReview(index), Is.True);
                    Assert.That(browser.CompleteReview(index), Is.False);
                }
                else Assert.That(browser.IsReviewPending, Is.False, "Hana has no immediate review thought.");
                Assert.That(browser.HasReviewed(index), Is.True);
                Assert.That(convenience.HandleCancel(), Is.True, "Esc belongs to browser Back.");
                Assert.That(browser.IsOverview, Is.True);
                Assert.That(browser.Inspect(index), Is.True); Assert.That(browser.IsReviewPending, Is.False);
                Assert.That(browser.Back(), Is.True);
            }
            Assert.That(reviews.Count, Is.EqualTo(2));
            Assert.That(forwardedCount, Is.EqualTo(2));
            Assert.That(browser.TryContinue(), Is.True); Assert.That(browser.TryContinue(), Is.False);
            Assert.That(completed, Is.EqualTo(1)); Assert.That(gate.IsStoryInteractionActive, Is.False);
            Assert.That(gate.CanUseSaveLoad, Is.True);
        }

        [Test]
        public void ProfileBrowse_LoadAndDisableClearTransientState_AndCannotStealAnotherOwner()
        {
            var browser = CreateProfileBrowser(out var save);
            Assert.That(browser.BeginBrowse(Profiles()), Is.True);
            var other = CreateProfileBrowser(out _);
            Assert.That(other.BeginBrowse(Profiles()), Is.False);
            other.CancelBrowse(); Assert.That(gate.IsStoryInteractionActive, Is.True);
            browser.Inspect(2);
            typeof(VNSaveLoadController).GetMethod("SetLoadInProgress", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(save, new object[] { true });
            Assert.That(browser.IsBrowsing, Is.False); Assert.That(browser.HasReviewed(2), Is.False);
            Assert.That(gate.IsStoryInteractionActive, Is.False);
            Assert.That(browser.BeginBrowse(Profiles()), Is.False);
            typeof(VNSaveLoadController).GetMethod("SetLoadInProgress", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(save, new object[] { false });
            Assert.That(browser.BeginBrowse(Profiles()), Is.True);
            InvokePrivateNoArguments(browser, "OnDisable");
            Assert.That(browser.IsBrowsing, Is.False); Assert.That(gate.IsStoryInteractionActive, Is.False);
        }

        private static VNProfileContent[] Profiles() => new[]
        {
            new VNProfileContent("선주", "Technical first facts", true),
            new VNProfileContent("해진", "Technical second facts", true),
            new VNProfileContent("하나", "Technical third facts", false),
        };

        private VNProfileBrowser CreateProfileBrowser(out VNSaveLoadController save)
        {
            save = CreateSaveLoadController();
            var panel = new GameObject("Technical profiles"); ownedObjects.Add(panel); panel.SetActive(false);
            var browser = panel.AddComponent<VNProfileBrowser>(); var group = panel.AddComponent<CanvasGroup>();
            GameObject Child(string name, params Type[] types)
            {
                var child = new GameObject(name, types); child.transform.SetParent(panel.transform, false); return child;
            }
            var buttons = new UnityEngine.UI.Button[3]; var labels = new TMP_Text[3]; var reviewed = new TMP_Text[3];
            for (var i = 0; i < 3; i++)
            {
                buttons[i] = Child("Equal card", typeof(RectTransform), typeof(UnityEngine.UI.Button)).GetComponent<UnityEngine.UI.Button>();
                labels[i] = Child("Name", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
                reviewed[i] = Child("Reviewed", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            }
            SetPrivateField(browser, "root", group); SetPrivateField(browser, "overview", Child("Overview"));
            SetPrivateField(browser, "detail", Child("Detail")); SetPrivateField(browser, "profileButtons", buttons);
            SetPrivateField(browser, "profileNames", labels); SetPrivateField(browser, "reviewedLabels", reviewed);
            SetPrivateField(browser, "detailName", Child("Detail name", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>());
            SetPrivateField(browser, "detailFacts", Child("Detail facts", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>());
            SetPrivateField(browser, "backButton", Child("Back", typeof(RectTransform), typeof(UnityEngine.UI.Button)).GetComponent<UnityEngine.UI.Button>());
            SetPrivateField(browser, "continueButton", Child("Continue", typeof(RectTransform), typeof(UnityEngine.UI.Button)).GetComponent<UnityEngine.UI.Button>());
            SetPrivateField(browser, "interactionGate", gate); SetPrivateField(browser, "saveLoad", save);
            panel.SetActive(true); InvokePrivateNoArguments(browser, "OnEnable");
            return browser;
        }

        private VNSaveLoadController CreateSaveLoadController()
        {
            var gameObject = new GameObject("M6-04 SaveLoad Test");
            ownedObjects.Add(gameObject);
            var controller = gameObject.AddComponent<VNSaveLoadController>();
            InvokePrivateNoArguments(controller, "Awake");
            return controller;
        }

        private CanvasGroup CreateCanvasGroup(string name, float alpha, bool interactable, bool blocksRaycasts)
        {
            var gameObject = new GameObject(name);
            ownedObjects.Add(gameObject);
            var group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = alpha;
            group.interactable = interactable;
            group.blocksRaycasts = blocksRaycasts;
            return group;
        }

        private void MarkRead(string lineId, string text)
        {
            Present(lineId, text);
            CompleteDisplay();
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.True);
        }

        private void Present(string lineId, string text)
        {
            var line = CreateLine(lineId, text);
            line.Source = dialogueRunner;
            lifecycle.RunLineAsync(line, NewLineToken());
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
        }

        private void CompleteDisplay()
        {
            markupHandler.OnLineDisplayComplete();
        }

        private static LineCancellationToken NewLineToken() => new()
        {
            NextContentToken = System.Threading.CancellationToken.None,
            HurryUpToken = System.Threading.CancellationToken.None,
        };

        private static LocalizedLine CreateLine(string lineId, string text) => new()
        {
            TextID = lineId,
            Text = new MarkupParseResult(text, new List<MarkupAttribute>()),
        };

        private static void AssertHidden(CanvasGroup group)
        {
            Assert.That(group.alpha, Is.Zero);
            Assert.That(group.interactable, Is.False);
            Assert.That(group.blocksRaycasts, Is.False);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName, bool value)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, new object[] { value });
        }

        private static void InvokePrivateNoArguments(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }
    }
}
