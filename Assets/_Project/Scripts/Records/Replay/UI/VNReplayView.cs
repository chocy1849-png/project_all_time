using System;
using ProjectAllTime.VN.Presentation;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

namespace ProjectAllTime.VN.Records.Replay.UI
{
    /// <summary>Replay-owned overlay and controls. It never owns or destroys the Replay session.</summary>
    [DisallowMultipleComponent]
    public sealed class VNReplayView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlayCanvasGroup;
        [SerializeField] private VNPresentationController presentationController;
        [SerializeField] private LinePresenter linePresenter;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;

        private DialogueRunner replayRunner;

        public event Action CloseRequested;

        public VNPresentationController PresentationController => presentationController;
        public LinePresenter LinePresenter => linePresenter;
        public Button NextButton => nextButton;
        public Button CloseButton => closeButton;
        public bool IsBlockingInput => overlayCanvasGroup != null && overlayCanvasGroup.blocksRaycasts && overlayCanvasGroup.interactable;

        private void OnEnable() => Unbind();
        private void OnDisable() => Unbind();
        private void OnDestroy() => Unbind();

        public bool TryValidateWiring(out string diagnostic)
        {
            if (overlayCanvasGroup == null) return Fail("Replay view requires an overlay CanvasGroup.", out diagnostic);
            if (overlayCanvasGroup.transform != transform)
                return Fail("Replay overlay CanvasGroup must be on the Replay view root.", out diagnostic);
            var raycastBlocker = overlayCanvasGroup.GetComponent<Graphic>();
            if (raycastBlocker == null || !raycastBlocker.raycastTarget)
                return Fail("Replay view root requires a raycast-target Graphic to block the Records UI.", out diagnostic);
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null || canvas.GetComponent<GraphicRaycaster>() == null)
                return Fail("Replay view must share a parent Canvas with a GraphicRaycaster.", out diagnostic);
            if (presentationController == null) return Fail("Replay view requires a VNPresentationController.", out diagnostic);
            if (!IsOwnedByView(presentationController.transform))
                return Fail("Replay presentation controller must belong to the Replay view hierarchy.", out diagnostic);
            if (linePresenter == null) return Fail("Replay view requires a Yarn LinePresenter.", out diagnostic);
            if (!IsOwnedByView(linePresenter.transform))
                return Fail("Replay LinePresenter must belong to the Replay view hierarchy.", out diagnostic);
            if (nextButton == null) return Fail("Replay view requires a Next button.", out diagnostic);
            if (!IsOwnedByView(nextButton.transform)) return Fail("Replay Next button must belong to the Replay view hierarchy.", out diagnostic);
            if (closeButton == null) return Fail("Replay view requires a Close button.", out diagnostic);
            if (!IsOwnedByView(closeButton.transform)) return Fail("Replay Close button must belong to the Replay view hierarchy.", out diagnostic);

            diagnostic = null;
            return true;
        }

        public bool Bind(DialogueRunner runner, out string diagnostic)
        {
            if (!TryValidateWiring(out diagnostic)) return false;
            if (runner == null) return Fail("Replay view requires its Replay DialogueRunner.", out diagnostic);

            Unbind();
            replayRunner = runner;
            linePresenter.autoAdvance = false;
            nextButton.onClick.AddListener(HandleNextClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
            SetOverlayVisible(true);
            return true;
        }

        public void Unbind()
        {
            if (nextButton != null) nextButton.onClick.RemoveListener(HandleNextClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(HandleCloseClicked);
            replayRunner = null;
            SetOverlayVisible(false);
        }

        private void HandleNextClicked()
        {
            if (replayRunner != null && replayRunner.IsDialogueRunning)
                replayRunner.RequestNextLine();
        }

        private void HandleCloseClicked() => CloseRequested?.Invoke();

        private void SetOverlayVisible(bool visible)
        {
            if (overlayCanvasGroup == null) return;
            overlayCanvasGroup.alpha = visible ? 1f : 0f;
            overlayCanvasGroup.interactable = visible;
            overlayCanvasGroup.blocksRaycasts = visible;
        }

        private bool IsOwnedByView(Transform target) =>
            target != null && (target == transform || target.IsChildOf(transform));

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}
