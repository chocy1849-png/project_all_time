using System;
using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Unity;

#nullable enable

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Passive Yarn presenter that owns per-occurrence session lifecycle and
    /// observes the active DialogueRunner LinePresenter for full-display
    /// authority.
    /// </summary>
    public sealed class VNLineLifecyclePresenter : DialoguePresenterBase
    {
        [SerializeField] private VNDialogueSessionState? sessionState;
        [SerializeField] private LinePresenter? linePresenter;

        private long currentLineOccurrence;
        private LineCancellationToken currentLineToken;
        private bool hasCurrentLineToken;
        private LinePresenter? authoritativeLinePresenter;
        private string expectedDisplayText = string.Empty;
        private int matchingTextObservedFrame = -1;
        private bool visualStateLogged;
        private bool wiringLogged;
        private bool serializedMismatchLogged;

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
            matchingTextObservedFrame = -1;
            visualStateLogged = false;
            authoritativeLinePresenter = ResolveAuthoritativeLinePresenter(line.Source as DialogueRunner);
            expectedDisplayText = authoritativeLinePresenter == null
                ? string.Empty
                : GetExpectedDisplayText(line, authoritativeLinePresenter);
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

        /// <summary>Marks the active occurrence fully displayed from the active LinePresenter callback.</summary>
        internal void HandleMarkupDisplayComplete()
        {
            VNConvenienceDiagnostics.Log(
                $"[M6-LIFECYCLE] callback complete: lineId={sessionState?.CurrentLineId}, occurrence={currentLineOccurrence}, " +
                $"presenter={Describe(authoritativeLinePresenter)}");
            RecordFullDisplay("callback");
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
                currentLineToken.IsNextContentRequested || authoritativeLinePresenter?.lineText == null ||
                Time.frameCount <= sessionState.CurrentPresentationStartedFrame)
                return;

            var textView = authoritativeLinePresenter.lineText;
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
            RecordFullDisplay("visual-fallback");
        }

        private void RecordFullDisplay(string source)
        {
            if (!hasCurrentLineToken || sessionState == null || authoritativeLinePresenter == null ||
                !sessionState.IsLineActive || currentLineOccurrence != sessionState.CurrentPresentationOccurrence ||
                currentLineToken.IsNextContentRequested)
                return;

            if (!sessionState.TryRecordFullDisplay(currentLineOccurrence, currentLineToken.IsNextContentRequested)) return;

            if (source == "callback")
            {
                VNConvenienceDiagnostics.Log(
                    $"[M6-LIFECYCLE] full display source=callback: lineId={sessionState.CurrentLineId}, " +
                    $"occurrence={currentLineOccurrence}, presenter={Describe(authoritativeLinePresenter)}");
            }
            else
            {
                VNConvenienceDiagnostics.Log(
                    $"[M6-LIFECYCLE] visual fallback: lineId={sessionState.CurrentLineId}, occurrence={currentLineOccurrence}");
            }
        }

        private LinePresenter? ResolveAuthoritativeLinePresenter(DialogueRunner? runner)
        {
            if (!VNAuthoritativeLinePresenterResolver.TryResolve(runner, out var authoritative, out var diagnostic))
            {
                Debug.LogError(
                    $"[M6-WIRING] {diagnostic} M6 will not authorize full display.",
                    this);
                return null;
            }

            if (!wiringLogged)
            {
                wiringLogged = true;
                VNConvenienceDiagnostics.Log(
                    $"[M6-WIRING] runner={Describe(runner)} authoritativePresenter={Describe(authoritative)} " +
                    $"serializedPresenter={Describe(linePresenter)} same={ReferenceEquals(linePresenter, authoritative)} " +
                    $"lineText={Describe(authoritative!.lineText)}");
            }
            if (linePresenter != null && !ReferenceEquals(linePresenter, authoritative) && !serializedMismatchLogged)
            {
                serializedMismatchLogged = true;
                Debug.LogError(
                    $"[M6-WIRING] Serialized LinePresenter {Describe(linePresenter)} differs from authoritative {Describe(authoritative)}; using authoritative presenter.",
                    this);
            }

            return authoritative;
        }

        private static string Describe(UnityEngine.Object? value)
        {
            return value == null ? "<null>" : $"{value.name}/{value.GetInstanceID()}";
        }

        private static string GetExpectedDisplayText(LocalizedLine line, LinePresenter presenter)
        {
            if (presenter.characterNameText == null && presenter.showCharacterNameInLine)
                return line?.Text.Text ?? string.Empty;
            return line?.TextWithoutCharacterName.Text ?? string.Empty;
        }

        private void ClearTrackedLine()
        {
            hasCurrentLineToken = false;
            currentLineOccurrence = 0;
            currentLineToken = default;
            authoritativeLinePresenter = null;
            expectedDisplayText = string.Empty;
            matchingTextObservedFrame = -1;
            visualStateLogged = false;
        }
    }
}
