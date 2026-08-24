using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.SaveLoad;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNLineLifecycleTypewriterIntegrationTests
    {
        private readonly List<Object> ownedObjects = new();
        private VNDialogueSessionState sessionState;
        private VNLineLifecyclePresenter lifecycle;
        private VNLineLifecycleMarkupHandler handler;
        private LinePresenter linePresenter;
        private TextMeshProUGUI lineText;
        private VNInteractionGate gate;
        private VNLineAdvancerInputBridge bridge;
        private VNConvenienceController convenience;

        [SetUp]
        public void SetUp()
        {
            var root = new GameObject("M6 Visual Lifecycle Integration");
            ownedObjects.Add(root);
            sessionState = root.AddComponent<VNDialogueSessionState>();
            linePresenter = root.AddComponent<LinePresenter>();
            lifecycle = root.AddComponent<VNLineLifecyclePresenter>();
            handler = root.AddComponent<VNLineLifecycleMarkupHandler>();
            root.AddComponent<LineAdvancer>().enabled = false;
            gate = root.AddComponent<VNInteractionGate>();
            bridge = root.AddComponent<VNLineAdvancerInputBridge>();
            convenience = root.AddComponent<VNConvenienceController>();
            var textObject = new GameObject("M6 Line Text");
            ownedObjects.Add(textObject);
            lineText = textObject.AddComponent<TextMeshProUGUI>();
            linePresenter.lineText = lineText;
            linePresenter.characterNameText = lineText;

            SetPrivateField(lifecycle, "sessionState", sessionState);
            SetPrivateField(lifecycle, "linePresenter", linePresenter);
            SetPrivateField(handler, "lifecyclePresenter", lifecycle);
            SetPrivateField(gate, "sessionState", sessionState);
            SetPrivateField(bridge, "sessionState", sessionState);
            SetPrivateField(bridge, "interactionGate", gate);
            SetPrivateField(convenience, "sessionState", sessionState);
            SetPrivateField(convenience, "advanceBridge", bridge);
            SetPrivateField(convenience, "interactionGate", gate);
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
        }

        [Test]
        public void IncompleteMatchingVisualText_IsNotFullDisplayed_UntilVisibleCountReachesCharacterCount()
        {
            Present("line:visual-incomplete", "Visible state.");
            SetVisual("Visible state.", 1);
            ObserveVisual();
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.False);

            SetVisual("Visible state.", CharacterCount("Visible state."));
            ObserveVisual();
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.True);
            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));
        }

        [Test]
        public void MatchingVisualText_AtOrAboveCharacterCount_RecordsOncePerOccurrence()
        {
            Present("line:visual-full", "Fully visible.");
            SetVisual("Fully visible.", CharacterCount("Fully visible.") + 2);
            ObserveVisual();
            ObserveVisual();

            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.True);
            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));
        }

        [Test]
        public void StaleOrSameFrameVisualState_CannotAuthorizeNewOccurrence()
        {
            Present("line:old", "Old visible.");
            SetVisual("Old visible.", CharacterCount("Old visible."));
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
            ObserveVisual();
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.True);

            Present("line:new", "New visible.");
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
            ObserveVisual();
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.False, "Old TMP text must not complete the new occurrence.");

            SetVisual("New visible.", CharacterCount("New visible."));
            SetPrivateField(sessionState, "currentPresentationStartedFrame", Time.frameCount);
            ObserveVisual();
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.False, "BeginLine frame must never authorize full display.");

            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
            ObserveVisual();
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.True);
        }

        [Test]
        public void NextContentCancellation_DoesNotCreateVisualFullDisplayBacklog()
        {
            using var next = new CancellationTokenSource();
            Present("line:visual-cancel", "Cancelled.", new LineCancellationToken
            {
                NextContentToken = next.Token,
                HurryUpToken = CancellationToken.None,
            });
            next.Cancel();
            SetVisual("Cancelled.", CharacterCount("Cancelled."));
            ObserveVisual();

            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.False);
            Assert.That(sessionState.Backlog.Count, Is.Zero);
        }

        [Test]
        public void VisualFullDisplay_DrivesAutoAndReadOnlySkipForExactYarnTextId()
        {
            var forwarded = new List<VNAdvanceSource>();
            bridge.AdvanceForwarded += forwarded.Add;
            PresentAndVisuallyComplete("line:m6_repeat_01", "First occurrence.");
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
            convenience.SetAutoEnabled(true);
            Tick(0f, 1);
            Tick(0f, 2);
            Tick(5f, 3);

            Assert.That(forwarded, Does.Contain(VNAdvanceSource.Auto));
            Assert.That(sessionState.ReadHistory.IsRead("line:m6_repeat_01"), Is.True);

            PresentAndVisuallyComplete("line:m6_repeat_01", "Second occurrence.");
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
            convenience.SetSkipEnabled(true);
            Tick(6f, 4);
            Tick(6.1f, 5);
            Assert.That(forwarded, Does.Contain(VNAdvanceSource.Skip));

            PresentAndVisuallyComplete("line:m6_unread_after_repeat_01", "Unread occurrence.");
            Tick(7f, 6);
            Tick(7.1f, 7);
            Assert.That(convenience.IsSkipEnabled, Is.False);
        }

        [Test]
        public void MarkupHandler_IsAdvisoryOnly_AndLoadBarrierVisualStateStillPasses()
        {
            Present("line:handler-advisory", "Visual authority.");
            handler.OnLineDisplayBegin(default, null);
            handler.OnLineDisplayComplete();
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.False);

            var runnerObject = new GameObject("M6 Load Barrier Runner");
            ownedObjects.Add(runnerObject);
            var runner = runnerObject.AddComponent<DialogueRunner>();
            var presenterObject = new GameObject("M6 Load Barrier Presenter");
            ownedObjects.Add(presenterObject);
            var presenter = presenterObject.AddComponent<LinePresenter>();
            presenter.canvasGroup = presenterObject.AddComponent<CanvasGroup>();
            presenter.lineText = presenterObject.AddComponent<TextMeshProUGUI>();
            presenter.canvasGroup.alpha = 0f;
            presenter.lineText.maxVisibleCharacters = 0;
            runner.DialoguePresenters = new DialoguePresenterBase[] { presenter };

            Assert.That(VNLinePresenterLoadBarrier.TryResolveLinePresenter(runner, out _, out var diagnostic), Is.True, diagnostic);
            Assert.That(VNLinePresenterLoadBarrier.IsQuiescent(presenter, out diagnostic), Is.True, diagnostic);
        }

        private void PresentAndVisuallyComplete(string lineId, string text)
        {
            Present(lineId, text);
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
            SetVisual(text, CharacterCount(text));
            ObserveVisual();
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.True);
        }

        private void Present(string lineId, string text, LineCancellationToken? token = null)
        {
            lifecycle.RunLineAsync(CreateLine(lineId, text), token ?? new LineCancellationToken
            {
                NextContentToken = CancellationToken.None,
                HurryUpToken = CancellationToken.None,
            });
        }

        private void SetVisual(string text, int maxVisibleCharacters)
        {
            lineText.text = text;
            lineText.maxVisibleCharacters = maxVisibleCharacters;
        }

        private int CharacterCount(string text) => lineText.GetTextInfo(text).characterCount;

        private void ObserveVisual()
        {
            var method = typeof(VNLineLifecyclePresenter).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(lifecycle, null);
        }

        private void Tick(float time, int frame)
        {
            var method = typeof(VNConvenienceController).GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(convenience, new object[] { time, frame });
        }

        private static LocalizedLine CreateLine(string lineId, string text) => new()
        {
            TextID = lineId,
            Text = new MarkupParseResult(text, new List<MarkupAttribute>()),
        };

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }
    }
}
