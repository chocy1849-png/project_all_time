using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Markup;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    [TestFixture]
    public sealed class VNConvenienceInputRouterTests
    {
        private readonly List<Object> ownedObjects = new();
        private VNDialogueSessionState sessionState;
        private VNLineLifecyclePresenter lifecycle;
        private VNInteractionGate gate;
        private VNLineAdvancerInputBridge bridge;
        private VNConvenienceController convenience;
        private VNConvenienceInputRouter router;
        private int forwardedCount;

        [SetUp]
        public void SetUp()
        {
            forwardedCount = 0;
            var root = new GameObject("M6-05.5 Input Router Test");
            ownedObjects.Add(root);
            root.AddComponent<LineAdvancer>().enabled = false;
            sessionState = root.AddComponent<VNDialogueSessionState>();
            lifecycle = root.AddComponent<VNLineLifecyclePresenter>();
            gate = root.AddComponent<VNInteractionGate>();
            bridge = root.AddComponent<VNLineAdvancerInputBridge>();
            convenience = root.AddComponent<VNConvenienceController>();
            router = root.AddComponent<VNConvenienceInputRouter>();
            SetPrivateField(lifecycle, "sessionState", sessionState);
            SetPrivateField(gate, "sessionState", sessionState);
            SetPrivateField(bridge, "sessionState", sessionState);
            SetPrivateField(bridge, "interactionGate", gate);
            SetPrivateField(convenience, "sessionState", sessionState);
            SetPrivateField(convenience, "advanceBridge", bridge);
            SetPrivateField(convenience, "interactionGate", gate);
            SetPrivateField(router, "convenienceController", convenience);
            SetPrivateField(router, "interactionGate", gate);
            InvokePrivateNoArguments(router, "OnEnable");
            bridge.AdvanceForwarded += _ => forwardedCount++;
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = ownedObjects.Count - 1; index >= 0; index--)
                Object.DestroyImmediate(ownedObjects[index]);
            ownedObjects.Clear();
        }

        [Test]
        public void AdvanceRouting_MouseOverUiIsSuppressed_ButSpaceRoutesEvenWhenPointerIsOverUi()
        {
            Present("advance", "Advance.");
            Assert.That(RouteAdvance(true, true), Is.False);
            Assert.That(forwardedCount, Is.Zero);

            Assert.That(RouteAdvance(false, true), Is.True, "Keyboard Space must not depend on cursor position.");
            Assert.That(forwardedCount, Is.EqualTo(1));
            Assert.That(RouteAdvance(true, false), Is.True);
            Assert.That(forwardedCount, Is.EqualTo(2));
        }

        [Test]
        public void CtrlHold_FromManualEnablesSkipTemporarily_AndPersistentSkipSurvivesRelease()
        {
            InvokeRouter("BeginSkipHold");
            Assert.That(convenience.IsSkipEnabled, Is.True);
            Assert.That(convenience.IsAutoEnabled, Is.False);
            InvokeRouter("EndSkipHold");
            Assert.That(convenience.IsSkipEnabled, Is.False);

            convenience.SetSkipEnabled(true);
            InvokeRouter("BeginSkipHold");
            InvokeRouter("EndSkipHold");
            Assert.That(convenience.IsSkipEnabled, Is.True);
        }

        [Test]
        public void CtrlHold_FromAutoRestoresAuto_UnlessLoadInvalidatesTheHold()
        {
            convenience.SetAutoEnabled(true);
            InvokeRouter("BeginSkipHold");
            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(convenience.IsSkipEnabled, Is.True);
            InvokeRouter("EndSkipHold");
            Assert.That(convenience.IsAutoEnabled, Is.True);
            Assert.That(convenience.IsSkipEnabled, Is.False);

            convenience.SetAutoEnabled(true);
            InvokeRouter("BeginSkipHold");
            InvokePrivate(convenience, "HandleLoadStateChanged", true);
            InvokeRouter("EndSkipHold");
            Assert.That(convenience.IsAutoEnabled, Is.False);
            Assert.That(convenience.IsSkipEnabled, Is.False);
        }

        [Test]
        public void Router_EnablesAndReleasesOnlyTheActionItEnabled()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            ownedObjects.Add(asset);
            var action = asset.AddActionMap("Dialogue").AddAction("advance", InputActionType.Button);
            var reference = InputActionReference.Create(action);
            ownedObjects.Add(reference);
            SetPrivateField(router, "advanceAction", reference);
            InvokePrivateNoArguments(router, "OnDisable");
            InvokePrivateNoArguments(router, "OnEnable");
            Assert.That(action.enabled, Is.True);
            InvokePrivateNoArguments(router, "OnDisable");
            Assert.That(action.enabled, Is.False);
        }

        private bool RouteAdvance(bool leftMouse, bool pointerOverUi)
        {
            var method = typeof(VNConvenienceInputRouter).GetMethod("RouteAdvance", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(router, new object[] { leftMouse, pointerOverUi });
        }

        private void InvokeRouter(string methodName) => InvokePrivateNoArguments(router, methodName);

        private void Present(string lineId, string text)
        {
            lifecycle.RunLineAsync(new LocalizedLine
            {
                TextID = lineId,
                Text = new MarkupParseResult(text, new List<MarkupAttribute>()),
            }, new LineCancellationToken
            {
                NextContentToken = System.Threading.CancellationToken.None,
                HurryUpToken = System.Threading.CancellationToken.None,
            });
            lifecycle.OnLineDisplayBegin(default, null);
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
