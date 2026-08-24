using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Dialogue;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNConvenienceRuntimeTests
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();
        private VNDialogueSessionState sessionState;
        private VNLineLifecyclePresenter lifecycle;
        private VNLineLifecycleMarkupHandler markupHandler;
        private LinePresenter linePresenter;
        private TextMeshProUGUI lineText;
        private VNInteractionGate gate;
        private VNLineAdvancerInputBridge bridge;
        private VNConvenienceController convenience;
        private int forwardedCount;
        private VNAdvanceSource lastSource;

        [SetUp]
        public void SetUp()
        {
            forwardedCount = 0;
            lastSource = default;
            var gameObject = new GameObject("M6-03 Convenience Runtime Test");
            ownedObjects.Add(gameObject);
            var lineAdvancer = gameObject.AddComponent<LineAdvancer>();
            // This test exercises the project bridge directly; prevent Yarn's
            // current InputActions mode from observing the test runner's input.
            lineAdvancer.enabled = false;
            sessionState = gameObject.AddComponent<VNDialogueSessionState>();
            lifecycle = gameObject.AddComponent<VNLineLifecyclePresenter>();
            markupHandler = gameObject.AddComponent<VNLineLifecycleMarkupHandler>();
            linePresenter = gameObject.AddComponent<LinePresenter>();
            var textObject = new GameObject("M6 Visual Text");
            ownedObjects.Add(textObject);
            lineText = textObject.AddComponent<TextMeshProUGUI>();
            linePresenter.lineText = lineText;
            linePresenter.characterNameText = lineText;
            gate = gameObject.AddComponent<VNInteractionGate>();
            bridge = gameObject.AddComponent<VNLineAdvancerInputBridge>();
            convenience = gameObject.AddComponent<VNConvenienceController>();

            SetPrivateField(lifecycle, "sessionState", sessionState);
            SetPrivateField(lifecycle, "linePresenter", linePresenter);
            SetPrivateField(markupHandler, "lifecyclePresenter", lifecycle);
            SetPrivateField(gate, "sessionState", sessionState);
            SetPrivateField(bridge, "sessionState", sessionState);
            SetPrivateField(bridge, "interactionGate", gate);
            SetPrivateField(convenience, "sessionState", sessionState);
            SetPrivateField(convenience, "advanceBridge", bridge);
            SetPrivateField(convenience, "interactionGate", gate);
            bridge.AdvanceForwarded += source =>
            {
                forwardedCount++;
                lastSource = source;
            };
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
        }

        [Test]
        public void Bridge_RejectsBlockedAndOptionsInput_AndDoesNotFabricateReadHistory()
        {
            Present("bridge", "A bridge line.");
            gate.SetUiHidden(true);
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Manual), Is.False);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(sessionState.ReadHistory.IsRead("bridge"), Is.False);

            gate.SetUiHidden(false);
            lifecycle.RunOptionsAsync(Array.Empty<DialogueOption>(), new LineCancellationToken
            {
                NextContentToken = System.Threading.CancellationToken.None,
                HurryUpToken = System.Threading.CancellationToken.None,
            });
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Manual), Is.False);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(sessionState.ReadHistory.IsRead("bridge"), Is.False);
        }

        [Test]
        public void Bridge_HurriesBeforeFullDisplay_AndAuthorizesReadOnlyOnFullConsume()
        {
            Present("bridge-read", "Read only after normal consume.");

            Assert.That(bridge.TryAdvance(VNAdvanceSource.Manual), Is.True);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(lastSource, Is.EqualTo(VNAdvanceSource.Manual));
            Assert.That(sessionState.ReadHistory.IsRead("bridge-read"), Is.False);

            CompleteDisplay();
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Manual), Is.True);
            Assert.That(forwardedCount, Is.EqualTo(2));
            Assert.That(sessionState.ReadHistory.IsRead("bridge-read"), Is.True);
        }

        [Test]
        public void Bridge_RejectsThePresentationFrame_BeforeHurryOrReadAuthorization()
        {
            Present("same-frame", "Same frame.");
            SetPrivateField(sessionState, "currentPresentationStartedFrame", Time.frameCount);

            Assert.That(bridge.TryAdvance(VNAdvanceSource.Manual), Is.False);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.False);
            Assert.That(sessionState.ReadHistory.IsRead("same-frame"), Is.False);

            CompleteDisplay();
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Manual), Is.False);
            Assert.That(sessionState.ReadHistory.IsRead("same-frame"), Is.False);

            var method = typeof(VNLineAdvancerInputBridge).GetMethod("TryAdvance", BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(VNAdvanceSource), typeof(int) }, null);
            Assert.That(method, Is.Not.Null);
            Assert.That((bool)method.Invoke(bridge, new object[] { VNAdvanceSource.Manual, Time.frameCount + 1 }), Is.True);
            Assert.That(sessionState.ReadHistory.IsRead("same-frame"), Is.True);
        }

        [Test]
        public void Bridge_RejectsInactiveAndBlankIdFullPresentationWithoutRead()
        {
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Manual), Is.False);
            Present(" ", "No stable identifier.");
            CompleteDisplay();
            Assert.That(bridge.TryAdvance(VNAdvanceSource.Manual), Is.False);
            Assert.That(sessionState.ReadHistory.Count, Is.Zero);
        }

        [Test]
        public void Auto_UsesBoundedTextDelay_AndOnlyStartsAfterFullDisplay()
        {
            Assert.That(convenience.GetAutoDelaySeconds(string.Empty), Is.EqualTo(0.8f));
            Assert.That(convenience.GetAutoDelaySeconds(new string('x', 1000)), Is.EqualTo(4f));

            Present("auto", "Short");
            convenience.SetAutoEnabled(true);
            Tick(0f, 1); // observe occurrence
            Tick(10f, 2); // not fully displayed: no timer/request
            Assert.That(forwardedCount, Is.Zero);

            CompleteDisplay();
            Tick(10f, 3); // arm at 10.8
            Tick(10.79f, 4);
            Assert.That(forwardedCount, Is.Zero);
            Tick(10.8f, 5);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(lastSource, Is.EqualTo(VNAdvanceSource.Auto));
        }

        [Test]
        public void Auto_SuspendsForHiddenUiWithoutDisabling_AndOldOccurrenceCannotAdvanceNewLine()
        {
            Present("old", "Old.");
            CompleteDisplay();
            convenience.SetAutoEnabled(true);
            Tick(0f, 1);
            Tick(0f, 2);
            gate.SetUiHidden(true);
            Tick(10f, 3);
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(convenience.IsAutoEnabled, Is.True);

            gate.SetUiHidden(false);
            Present("new", "New.");
            CompleteDisplay();
            Tick(10f, 4); // occurrence reset, no inherited deadline
            Assert.That(forwardedCount, Is.Zero);
            Tick(10f, 5);
            Assert.That(forwardedCount, Is.Zero);
        }

        [Test]
        public void SkipRead_StopsAtUnreadAndBlankLines_ButSkipAllMayHurryThem()
        {
            Present("unread", "Unread.");
            convenience.SetSkipEnabled(true);
            Tick(0f, 1);
            Tick(0f, 2);
            Assert.That(convenience.IsSkipEnabled, Is.False);
            Assert.That(forwardedCount, Is.Zero);

            markupHandler.OnLineWillDismiss();
            Present(" ", "Blank id.");
            convenience.SetSkipEnabled(true);
            Tick(1f, 3);
            Tick(1f, 4);
            Assert.That(convenience.IsSkipEnabled, Is.False);
            Assert.That(forwardedCount, Is.Zero);

            convenience.SetSkipPolicy(VNSkipPolicy.All);
            convenience.SetSkipEnabled(true);
            Tick(2f, 5);
            Tick(2f, 6);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(lastSource, Is.EqualTo(VNAdvanceSource.Skip));
        }

        [Test]
        public void SkipRead_HurriesReadLineThenConsumesOnlyAfterVerifiedFullDisplay_WithThrottle()
        {
            MarkRead("repeat", "Prior occurrence.");
            Present("repeat", "Current occurrence.");
            convenience.SetSkipEnabled(true);
            Tick(0f, 1);
            Tick(0f, 2);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(sessionState.IsCurrentLineFullyDisplayed, Is.False);

            Tick(0.01f, 3);
            Assert.That(forwardedCount, Is.EqualTo(1), "Skip throttle prevents a second request before the interval.");
            CompleteDisplay();
            Tick(0.05f, 4);
            Assert.That(forwardedCount, Is.EqualTo(2));
            Tick(0.10f, 5);
            Assert.That(forwardedCount, Is.EqualTo(2), "A full occurrence is consumed at most once.");
        }

        [Test]
        public void Skip_DoesNotConsumeThePresentationFrame_AndCanAdvanceOnALaterFrame()
        {
            Present("skip-frame", "Skip frame.");
            CompleteDisplay();
            convenience.SetSkipPolicy(VNSkipPolicy.All);
            convenience.SetSkipEnabled(true);
            SetPrivateField(sessionState, "currentPresentationStartedFrame", Time.frameCount);

            Tick(0f, Time.frameCount); // observes the occurrence
            Tick(1f, Time.frameCount); // requests Skip, but bridge rejects the presentation frame
            Assert.That(forwardedCount, Is.Zero);
            Assert.That(GetPrivateField<long>(convenience, "skipConsumedOccurrence"), Is.EqualTo(long.MinValue));

            Tick(2f, Time.frameCount + 1);
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(lastSource, Is.EqualTo(VNAdvanceSource.Skip));
        }

        [Test]
        public void AutoAndSkip_AreMutuallyExclusive()
        {
            convenience.SetAutoEnabled(true);
            Assert.That(convenience.IsAutoEnabled, Is.True);
            convenience.SetSkipEnabled(true);
            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(convenience.IsSkipEnabled, Is.True);
            convenience.SetAutoEnabled(true);
            Assert.That(convenience.IsAutoEnabled, Is.True);
            Assert.That(convenience.IsSkipEnabled, Is.False);
        }

        [Test]
        public void OptionalVoice_UnvoicedAndDialogueCompletionAreLogicallyComplete()
        {
            var voiceObject = new GameObject("M6-03 Voice State Test");
            ownedObjects.Add(voiceObject);
            var presenter = voiceObject.AddComponent<VNOptionalVoicePresenter>();
            presenter.RunLineAsync(CreateLine("unvoiced", "Unvoiced."), default);
            Assert.That(presenter.CurrentVoiceLineId, Is.EqualTo("unvoiced"));
            Assert.That(presenter.CurrentLineHasVoice, Is.False);
            Assert.That(presenter.IsCurrentVoiceComplete, Is.True);
            Assert.That(presenter.IsCurrentVoicePendingOrPlaying, Is.False);

            presenter.OnDialogueCompleteAsync();
            Assert.That(presenter.CurrentVoiceLineId, Is.Empty);
            Assert.That(presenter.CurrentLineHasVoice, Is.False);
            Assert.That(presenter.IsCurrentVoiceComplete, Is.True);
        }

        [Test]
        public void OptionalVoice_StaleCompletionCannotCompleteANewerVoiceOccurrence()
        {
            var voiceObject = new GameObject("M6-03 Voice Generation Test");
            ownedObjects.Add(voiceObject);
            var presenter = voiceObject.AddComponent<VNOptionalVoicePresenter>();
            var begin = GetPrivateMethod(typeof(VNOptionalVoicePresenter), "BeginVoicePresentation");
            var setPending = GetPrivateMethod(typeof(VNOptionalVoicePresenter), "SetVoicePending");
            var complete = GetPrivateMethod(typeof(VNOptionalVoicePresenter), "CompleteVoicePresentation");

            var first = (long)begin.Invoke(presenter, new object[] { CreateLine("repeated", "First.") });
            setPending.Invoke(presenter, new object[] { first });
            var second = (long)begin.Invoke(presenter, new object[] { CreateLine("repeated", "Second.") });
            setPending.Invoke(presenter, new object[] { second });
            complete.Invoke(presenter, new object[] { first });

            Assert.That(presenter.CurrentVoiceLineId, Is.EqualTo("repeated"));
            Assert.That(presenter.CurrentLineHasVoice, Is.True);
            Assert.That(presenter.IsCurrentVoiceComplete, Is.False);

            complete.Invoke(presenter, new object[] { second });
            Assert.That(presenter.IsCurrentVoiceComplete, Is.True);
        }

        private void MarkRead(string lineId, string text)
        {
            Present(lineId, text);
            CompleteDisplay();
            Assert.That(sessionState.TryAuthorizeCurrentLineConsume(), Is.True);
            markupHandler.OnLineWillDismiss();
        }

        private void Present(string lineId, string text)
        {
            lifecycle.RunLineAsync(CreateLine(lineId, text), default);
            SetPrivateField(sessionState, "currentPresentationStartedFrame", -1);
        }

        private void CompleteDisplay()
        {
            lineText.text = sessionState.CurrentText;
            lineText.maxVisibleCharacters = lineText.GetTextInfo(lineText.text).characterCount;
            InvokePrivateNoArguments(lifecycle, "LateUpdate");
        }

        private void Tick(float time, int frame)
        {
            var method = typeof(VNConvenienceController).GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(convenience, new object[] { time, frame });
        }

        private static LocalizedLine CreateLine(string lineId, string text)
        {
            return new LocalizedLine
            {
                TextID = lineId,
                Text = new MarkupParseResult(text, new List<MarkupAttribute>()),
            };
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            return (T)field.GetValue(target);
        }

        private static MethodInfo GetPrivateMethod(Type type, string methodName)
        {
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            return method;
        }

        private static void InvokePrivateNoArguments(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }
    }
}
