using System.Threading;
using System;
using TMPro;
using UnityEngine;
using Yarn.Unity;

#nullable enable

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Passive Yarn presenter that owns per-occurrence session lifecycle and
    /// observes the actual LinePresenter TMP view for full-display authority.
    /// </summary>
    public sealed class VNLineLifecyclePresenter : DialoguePresenterBase
    {
        [SerializeField] private VNDialogueSessionState? sessionState;
        [SerializeField] private LinePresenter? linePresenter;

        private long currentLineOccurrence;
        private LineCancellationToken currentLineToken;
        private bool hasCurrentLineToken;
        private string expectedDisplayText = string.Empty;
        private int matchingTextObservedFrame = -1;
        private bool visualStateLogged;

        private void LateUpdate()
        {
            ObserveVisualFullDisplay();
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            ClearTrackedLine();
            sessionState?.InvalidateTransientPresentation();
            return YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            ClearTrackedLine();
            sessionState?.InvalidateTransientPresentation();
            return YarnTask.CompletedTask;
        }

        public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            if (sessionState == null) return YarnTask.CompletedTask;

            currentLineOccurrence = sessionState.BeginLine(line);
            currentLineToken = token;
            hasCurrentLineToken = true;
            expectedDisplayText = GetExpectedDisplayText(line);
            matchingTextObservedFrame = -1;
            visualStateLogged = false;
            VNConvenienceDiagnostics.Log($"[M6-LIFECYCLE] line Begin: lineId={sessionState.CurrentLineId}, occurrence={currentLineOccurrence}");
            return YarnTask.CompletedTask;
        }

        public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken cancellationToken)
        {
            ClearTrackedLine();
            if (sessionState == null) return DialogueRunner.NoOptionSelected;
            var optionOccurrence = sessionState.BeginOptions();
            return ObserveOptionsAsync(optionOccurrence, cancellationToken);
        }

        /// <summary>Advisory-only legacy handler seam retained for M6-08B wiring.</summary>
        internal void HandleMarkupDisplayBegin()
        {
            VNConvenienceDiagnostics.Log("[M6-LIFECYCLE] markup callback: display begin (advisory)");
        }

        /// <summary>Advisory-only legacy handler seam retained for M6-08B wiring.</summary>
        internal void HandleMarkupDisplayComplete()
        {
            VNConvenienceDiagnostics.Log("[M6-LIFECYCLE] markup callback: display complete (advisory)");
        }

        /// <summary>Advisory-only legacy handler seam retained for M6-08B wiring.</summary>
        internal void HandleMarkupLineWillDismiss()
        {
            VNConvenienceDiagnostics.Log("[M6-LIFECYCLE] markup callback: line dismiss (advisory)");
        }

        private async YarnTask<DialogueOption?> ObserveOptionsAsync(long optionOccurrence, LineCancellationToken cancellationToken)
        {
            using var registration = cancellationToken.NextContentToken.Register(
                () => sessionState?.EndOptions(optionOccurrence));
            await YarnTask.WaitUntilCanceled(cancellationToken.NextContentToken).SuppressCancellationThrow();
            sessionState?.EndOptions(optionOccurrence);
            return await DialogueRunner.NoOptionSelected;
        }

        private void ObserveVisualFullDisplay()
        {
            if (!hasCurrentLineToken || sessionState == null || !sessionState.IsLineActive ||
                sessionState.IsCurrentLineFullyDisplayed || currentLineOccurrence != sessionState.CurrentPresentationOccurrence ||
                currentLineToken.IsNextContentRequested || linePresenter?.lineText == null ||
                Time.frameCount <= sessionState.CurrentPresentationStartedFrame)
                return;

            var textView = linePresenter.lineText;
            var displayedText = textView.text ?? string.Empty;
            if (!string.Equals(displayedText, expectedDisplayText, StringComparison.Ordinal)) return;

            var visibleCharacterCount = textView.GetTextInfo(displayedText).characterCount;
            if (!visualStateLogged)
            {
                visualStateLogged = true;
                VNConvenienceDiagnostics.Log(
                    $"[M6-LIFECYCLE] visual state: lineId={sessionState.CurrentLineId}, occurrence={currentLineOccurrence}, " +
                    $"active={sessionState.IsLineActive}, full={sessionState.IsCurrentLineFullyDisplayed}, text={displayedText}, " +
                    $"visible={textView.maxVisibleCharacters}, characters={visibleCharacterCount}, nextRequested={currentLineToken.IsNextContentRequested}, " +
                    $"backlog={sessionState.Backlog.Count}, read={sessionState.ReadHistory.IsRead(sessionState.CurrentLineId)}");
            }

            // Empty text has no visible-character transition to distinguish a
            // newly prepared line from stale empty UI, so require a later
            // matching frame before accepting it.
            if (visibleCharacterCount == 0)
            {
                if (matchingTextObservedFrame < 0)
                {
                    matchingTextObservedFrame = Time.frameCount;
                    return;
                }
                if (Time.frameCount <= matchingTextObservedFrame) return;
            }

            if (textView.maxVisibleCharacters < visibleCharacterCount) return;
            if (sessionState.TryRecordFullDisplay(currentLineOccurrence, false))
                VNConvenienceDiagnostics.Log($"[M6-LIFECYCLE] visual display Complete: lineId={sessionState.CurrentLineId}, occurrence={currentLineOccurrence}");
        }

        private string GetExpectedDisplayText(LocalizedLine line)
        {
            if (linePresenter?.characterNameText == null && linePresenter?.showCharacterNameInLine == true)
                return line?.Text.Text ?? string.Empty;
            return line?.TextWithoutCharacterName.Text ?? string.Empty;
        }

        private void ClearTrackedLine()
        {
            hasCurrentLineToken = false;
            currentLineOccurrence = 0;
            currentLineToken = default;
            expectedDisplayText = string.Empty;
            matchingTextObservedFrame = -1;
            visualStateLogged = false;
        }
    }
}
