using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNDialogueSessionStateTests
    {
        private readonly List<Object> ownedObjects = new();
        private VNDialogueSessionState sessionState;
        private VNLineLifecyclePresenter lifecyclePresenter;
        private VNLineLifecycleMarkupHandler markupHandler;
        private LinePresenter linePresenter;
        private TextMeshProUGUI lineText;

        [SetUp]
        public void SetUp()
        {
            var gameObject = new GameObject("M6-02 Dialogue Session Test");
            ownedObjects.Add(gameObject);
            sessionState = gameObject.AddComponent<VNDialogueSessionState>();
            lifecyclePresenter = gameObject.AddComponent<VNLineLifecyclePresenter>();
            markupHandler = gameObject.AddComponent<VNLineLifecycleMarkupHandler>();
            linePresenter = gameObject.AddComponent<LinePresenter>();
            var textObject = new GameObject("M6 Visual Text");
            ownedObjects.Add(textObject);
            lineText = textObject.AddComponent<TextMeshProUGUI>();
            linePresenter.lineText = lineText;
            linePresenter.characterNameText = lineText;
            SetPrivateField(lifecyclePresenter, "sessionState", sessionState);
            SetPrivateField(lifecyclePresenter, "linePresenter", linePresenter);
            SetPrivateField(markupHandler, "lifecyclePresenter", lifecyclePresenter);
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
        }

        [Test]
        public void LocalizedLine_MapsToBacklogFields_AndDetectsNarration()
        {
            Present(CreateLine("line-speaker", "Eve", "Welcome."));
            CompleteDisplay();

            var spoken = sessionState.Backlog.Entries[0];
            Assert.That(spoken.LineId, Is.EqualTo("line-speaker"));
            Assert.That(spoken.SpeakerName, Is.EqualTo("Eve"));
            Assert.That(spoken.Text, Is.EqualTo("Welcome."));
            Assert.That(spoken.IsNarration, Is.False);

            Dismiss();
            Present(CreateLine("line-narration", null, "The room is quiet."));
            CompleteDisplay();

            var narration = sessionState.Backlog.Entries[1];
            Assert.That(narration.SpeakerName, Is.Empty);
            Assert.That(narration.Text, Is.EqualTo("The room is quiet."));
            Assert.That(narration.IsNarration, Is.True);
        }

        [Test]
        public void BlankSpeakerName_IsNarration()
        {
            Present(CreateLine("line-blank-speaker", "   ", "A distant bell rings."));
            CompleteDisplay();

            Assert.That(sessionState.Backlog.Entries[0].IsNarration, Is.True);
        }

        [Test]
        public void FullDisplay_AppendsOncePerOccurrence_ButLaterOccurrencesRemainDistinct()
        {
            Present(CreateLine("repeated", "Eve", "First occurrence."));
            CompleteDisplay();
            CompleteDisplay();
            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));

            Dismiss();
            Present(CreateLine("repeated", "Eve", "Second occurrence."));
            CompleteDisplay();

            Assert.That(sessionState.Backlog.Count, Is.EqualTo(2));
            Assert.That(sessionState.Backlog.Entries[1].Text, Is.EqualTo("Second occurrence."));
        }

        [Test]
        public void HurryCancellation_AllowsFullDisplay_ButNextContentCancellationDoesNot()
        {
            using var hurrySource = new CancellationTokenSource();
            Present(CreateLine("hurry", "Eve", "Hurry is visible."), new LineCancellationToken
            {
                NextContentToken = CancellationToken.None,
                HurryUpToken = hurrySource.Token,
            });
            hurrySource.Cancel();
            CompleteDisplay();
            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));

            Dismiss();
            using var nextContentSource = new CancellationTokenSource();
            Present(CreateLine("cancelled", "Eve", "Never validly displayed."), new LineCancellationToken
            {
                NextContentToken = nextContentSource.Token,
                HurryUpToken = CancellationToken.None,
            });
            nextContentSource.Cancel();
            CompleteDisplay();

            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.False);
        }

        [Test]
        public void FullDisplayAlone_DoesNotMarkRead_AndAuthorizedConsumeDoes()
        {
            Present(CreateLine("readable", "Eve", "Read this."));

            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.False);
            CompleteDisplay();
            Assert.That(sessionState.ReadHistory.IsRead("readable"), Is.False);

            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.True);
            Assert.That(sessionState.ReadHistory.IsRead("readable"), Is.True);
            Assert.That(sessionState.ReadHistory.Count, Is.EqualTo(1));
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.True);
            Assert.That(sessionState.ReadHistory.Count, Is.EqualTo(1));
        }

        [Test]
        public void MissingLineId_IsBackloggedButNeverRead()
        {
            Present(CreateLine("  ", "Eve", "No stable ID."));
            CompleteDisplay();

            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));
            Assert.That(sessionState.Backlog.Entries[0].LineId, Is.EqualTo("  "));
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.False);
            Assert.That(sessionState.ReadHistory.Count, Is.Zero);
        }

        [Test]
        public void TransientInvalidation_RejectsStaleDisplayAndConsume()
        {
            Present(CreateLine("stale", "Eve", "This is interrupted."));
            sessionState.InvalidateTransientPresentation();
            lineText.text = "This is interrupted.";
            lineText.maxVisibleCharacters = lineText.GetTextInfo(lineText.text).characterCount;
            InvokePrivateNoArguments(lifecyclePresenter, "LateUpdate");

            Assert.That(sessionState.Backlog.Count, Is.Zero);
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.False);
            Assert.That(sessionState.IsLineActive, Is.False);
        }

        [UnityTest]
        public IEnumerator OptionsActive_FollowsCancellationLifetime_AndDoesNotKeepLineState()
        {
            Present(CreateLine("before-options", "Eve", "Choose."));
            CompleteDisplay();
            using var cancellationSource = new CancellationTokenSource();

            _ = lifecyclePresenter.RunOptionsAsync(System.Array.Empty<DialogueOption>(), new LineCancellationToken
            {
                NextContentToken = cancellationSource.Token,
                HurryUpToken = CancellationToken.None,
            });

            Assert.That(sessionState.OptionsActive, Is.True);
            Assert.That(sessionState.IsLineActive, Is.False);
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.False);

            cancellationSource.Cancel();
            yield return null;

            Assert.That(sessionState.OptionsActive, Is.False);
        }

        [Test]
        public void DialogueLifecycle_PreservesSessionData_AndExplicitClearRemovesIt()
        {
            Present(CreateLine("persist-session", "Eve", "Keep this."));
            CompleteDisplay();
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.True);

            lifecyclePresenter.OnDialogueCompleteAsync();
            lifecyclePresenter.OnDialogueStartedAsync();

            Assert.That(sessionState.Backlog.Count, Is.EqualTo(1));
            Assert.That(sessionState.ReadHistory.IsRead("persist-session"), Is.True);

            sessionState.ClearSession();
            Assert.That(sessionState.Backlog.Count, Is.Zero);
            Assert.That(sessionState.ReadHistory.Count, Is.Zero);
        }

        private void Present(LocalizedLine line, LineCancellationToken? token = null)
        {
            lifecyclePresenter.RunLineAsync(line, token ?? new LineCancellationToken
            {
                NextContentToken = CancellationToken.None,
                HurryUpToken = CancellationToken.None,
            });
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
        }

        private void CompleteDisplay()
        {
            lineText.text = sessionState.CurrentText;
            lineText.maxVisibleCharacters = lineText.GetTextInfo(lineText.text).characterCount;
            InvokePrivateNoArguments(lifecyclePresenter, "LateUpdate");
        }

        private void Dismiss() { }

        private static LocalizedLine CreateLine(string lineId, string speakerName, string text)
        {
            var attributes = new List<MarkupAttribute>();
            if (speakerName != null)
            {
                var constructor = typeof(MarkupAttribute).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(int),
                        typeof(int),
                        typeof(int),
                        typeof(string),
                        typeof(IEnumerable<MarkupProperty>),
                    },
                    null);
                Assert.That(constructor, Is.Not.Null, "Yarn 3.2.7 MarkupAttribute constructor is required for this localized-line test scaffold.");
                var propertyConstructor = typeof(MarkupProperty).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(string), typeof(string) },
                    null);
                Assert.That(propertyConstructor, Is.Not.Null, "Yarn 3.2.7 MarkupProperty constructor is required for this localized-line test scaffold.");
                attributes.Add((MarkupAttribute)constructor.Invoke(new object[]
                {
                    0,
                    0,
                    0,
                    "character",
                    new[] { (MarkupProperty)propertyConstructor.Invoke(new object[] { "name", speakerName }) },
                }));
            }

            return new LocalizedLine
            {
                TextID = lineId,
                Text = new MarkupParseResult(text, attributes),
            };
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private static void InvokePrivateNoArguments(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }
    }
}
