using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

#nullable enable

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Waits for Yarn's established LinePresenter to finish the old line's
    /// actual visual dismissal before a load starts its resume node.
    /// </summary>
    public sealed class VNLinePresenterLoadBarrier
    {
        private const int MaximumWaitFrames = 240;
        private const float DismissedAlphaEpsilon = 0.0001f;

        public async YarnTask<VNLinePresenterLoadBarrierResult> WaitForQuiescence(DialogueRunner dialogueRunner)
        {
            if (!TryResolveLinePresenter(dialogueRunner, out var presenter, out var diagnostic))
                return VNLinePresenterLoadBarrierResult.Failure(diagnostic);

            for (var frame = 0; frame <= MaximumWaitFrames; frame++)
            {
                if (IsQuiescent(presenter, out diagnostic))
                    return VNLinePresenterLoadBarrierResult.Success();

                if (frame == MaximumWaitFrames)
                    return VNLinePresenterLoadBarrierResult.Failure("Timed out waiting for the previous LinePresenter visual dismissal: " + diagnostic);

                await YarnTask.Yield();
            }

            return VNLinePresenterLoadBarrierResult.Failure("LinePresenter quiescence wait ended unexpectedly.");
        }

        public static bool TryResolveLinePresenter(DialogueRunner? dialogueRunner, out LinePresenter? linePresenter, out string diagnostic)
        {
            linePresenter = null;
            if (dialogueRunner == null)
            {
                diagnostic = "Load requires a DialogueRunner to resolve its established LinePresenter.";
                return false;
            }

            var candidates = new List<LinePresenter>();
            foreach (var presenter in dialogueRunner.DialoguePresenters)
            {
                if (presenter is LinePresenter line && line.isActiveAndEnabled)
                    candidates.Add(line);
            }

            if (candidates.Count != 1)
            {
                diagnostic = $"Load requires exactly one enabled LinePresenter in DialogueRunner.DialoguePresenters; found {candidates.Count}.";
                return false;
            }

            linePresenter = candidates[0];
            diagnostic = string.Empty;
            return true;
        }

        public static bool IsQuiescent(LinePresenter? linePresenter, out string diagnostic)
        {
            if (linePresenter == null)
            {
                diagnostic = "The resolved LinePresenter is missing.";
                return false;
            }
            if (linePresenter.canvasGroup == null)
            {
                diagnostic = "The resolved LinePresenter has no CanvasGroup.";
                return false;
            }
            if (linePresenter.lineText == null)
            {
                diagnostic = "The resolved LinePresenter has no line text.";
                return false;
            }

            // Yarn 3.2.7 LinePresenter invokes Typewriter.ContentDidDismiss()
            // after its visual fade. LetterTypewriter implements that callback
            // by resetting maxVisibleCharacters to zero. Canvas alpha alone is
            // insufficient because OnDialogueCompleteAsync also forces alpha.
            if (linePresenter.canvasGroup.alpha > DismissedAlphaEpsilon)
            {
                diagnostic = "LinePresenter CanvasGroup is still visible.";
                return false;
            }
            if (linePresenter.lineText.maxVisibleCharacters != 0)
            {
                diagnostic = "LinePresenter typewriter content is still visible.";
                return false;
            }

            diagnostic = string.Empty;
            return true;
        }
    }

    public sealed class VNLinePresenterLoadBarrierResult
    {
        public bool Succeeded { get; }
        public string Diagnostic { get; }

        private VNLinePresenterLoadBarrierResult(bool succeeded, string diagnostic)
        {
            Succeeded = succeeded;
            Diagnostic = diagnostic;
        }

        public static VNLinePresenterLoadBarrierResult Success() => new(true, string.Empty);
        public static VNLinePresenterLoadBarrierResult Failure(string diagnostic) => new(false, diagnostic);
    }
}
