using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.Tests.Editor
{
    /// <summary>TECHNICAL / NON-CANON white rectangle beside an ordinary exit.</summary>
    public sealed class VNLocalizedEffectTests
    {
        private GameObject root;
        private VNTransitionController transition;
        private CanvasGroup effect;
        private Image normalExit;
        private RectTransform dialogue;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Technical entrance", typeof(RectTransform));
            transition = root.AddComponent<VNTransitionController>();
            var door = new GameObject("Localized door", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            door.transform.SetParent(root.transform, false);
            effect = door.GetComponent<CanvasGroup>(); effect.alpha = 0;
            ((RectTransform)door.transform).sizeDelta = new Vector2(100, 200);
            var exit = new GameObject("Normal exit", typeof(RectTransform), typeof(Image)); exit.transform.SetParent(root.transform, false);
            normalExit = exit.GetComponent<Image>();
            var ui = new GameObject("Dialogue UI", typeof(RectTransform)); ui.transform.SetParent(root.transform, false);
            dialogue = (RectTransform)ui.transform; dialogue.anchoredPosition = new Vector2(30, -50);
            typeof(VNTransitionController).GetField("localizedEffectCanvasGroup", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(transition, effect);
        }

        [TearDown] public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void LocalDoor_Lifecycle_KeepsOrdinaryExitAndUiStable_AndBlocksSaveUntilHidden()
        {
            var position = dialogue.anchoredPosition;
            Drain(transition.FadeLocalizedEffect(true, 0));
            Assert.That(effect.alpha, Is.EqualTo(1));
            Assert.That(transition.IsTransitionActive, Is.True, "An unpersisted visible effect cannot enter a stable snapshot.");
            Assert.That(normalExit.enabled, Is.True);
            Assert.That(dialogue.anchoredPosition, Is.EqualTo(position));
            Assert.That(((RectTransform)effect.transform).rect.width, Is.EqualTo(100));
            Drain(transition.FadeLocalizedEffect(false, 0));
            Assert.That(effect.alpha, Is.Zero);
            Assert.That(transition.IsTransitionActive, Is.False);
            Assert.That(normalExit.enabled, Is.True);
        }

        [Test]
        public void Load_ClearsEffect_AndStaleExternalEnumeratorCannotResurrectItOrEndANewOperation()
        {
            var appearing = transition.FadeLocalizedEffect(true, 100);
            Assert.That(appearing.MoveNext(), Is.True);
            Assert.That(transition.IsTransitionActive, Is.True);
            transition.NormalizeForLoad();
            Assert.That(effect.alpha, Is.Zero);
            var newer = transition.FadeLocalizedEffect(true, 100);
            Assert.That(newer.MoveNext(), Is.True);
            Assert.That(appearing.MoveNext(), Is.False);
            Assert.That(transition.IsTransitionActive, Is.True);
            transition.NormalizeForLoad();
            Assert.That(newer.MoveNext(), Is.False);
            Assert.That(effect.alpha, Is.Zero);
            Assert.That(transition.IsTransitionActive, Is.False);
        }

        private static void Drain(IEnumerator routine) { while (routine.MoveNext()) if (routine.Current is IEnumerator nested) Drain(nested); }
    }
}
