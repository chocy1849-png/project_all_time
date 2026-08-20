using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

#nullable enable

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Passive Yarn presenter that captures localized line data and observes the
    /// existing LinePresenter typewriter. It never advances dialogue or selects
    /// options.
    /// </summary>
    public sealed class VNLineLifecyclePresenter : DialoguePresenterBase, IActionMarkupHandler
    {
        [SerializeField] private VNDialogueSessionState? sessionState;
        [SerializeField] private LinePresenter? linePresenter;

        private long currentLineOccurrence;
        private long displayOccurrence;
        private LineCancellationToken currentLineToken;
        private bool hasCurrentLineToken;
        private bool handlerRegistered;

        private void Awake()
        {
            TryRegisterTypewriterHandler();
        }

        private void OnEnable()
        {
            TryRegisterTypewriterHandler();
        }

        private void Start()
        {
            TryRegisterTypewriterHandler();
        }

        private void OnDisable()
        {
            UnregisterTypewriterHandler();
        }

        private void OnDestroy()
        {
            UnregisterTypewriterHandler();
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            ClearTrackedLine();
            sessionState?.InvalidateTransientPresentation();
            TryRegisterTypewriterHandler();
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

            TryRegisterTypewriterHandler();
            currentLineOccurrence = sessionState.BeginLine(line);
            currentLineToken = token;
            hasCurrentLineToken = true;
            return YarnTask.CompletedTask;
        }

        public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken cancellationToken)
        {
            ClearTrackedLine();
            if (sessionState == null) return DialogueRunner.NoOptionSelected;
            var optionOccurrence = sessionState.BeginOptions();
            return ObserveOptionsAsync(optionOccurrence, cancellationToken);
        }

        public void OnPrepareForLine(MarkupParseResult line, TMP_Text text) { }

        public void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
        {
            // Yarn invokes this synchronously from the current LinePresenter
            // typewriter. Preserve the occurrence it belongs to so an old
            // callback cannot act after transient state has been invalidated.
            displayOccurrence = hasCurrentLineToken ? currentLineOccurrence : 0;
        }

        public YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
        {
            return YarnTask.CompletedTask;
        }

        public void OnLineDisplayComplete()
        {
            if (!hasCurrentLineToken || sessionState == null || displayOccurrence == 0) return;
            sessionState.TryRecordFullDisplay(displayOccurrence, currentLineToken.IsNextContentRequested);
        }

        public void OnLineWillDismiss()
        {
            if (sessionState != null && hasCurrentLineToken)
                sessionState.EndLine(currentLineOccurrence);
            ClearTrackedLine();
        }

        private async YarnTask<DialogueOption?> ObserveOptionsAsync(long optionOccurrence, LineCancellationToken cancellationToken)
        {
            using var registration = cancellationToken.NextContentToken.Register(
                () => sessionState?.EndOptions(optionOccurrence));
            await YarnTask.WaitUntilCanceled(cancellationToken.NextContentToken).SuppressCancellationThrow();
            sessionState?.EndOptions(optionOccurrence);
            return await DialogueRunner.NoOptionSelected;
        }

        private void TryRegisterTypewriterHandler()
        {
            if (handlerRegistered || linePresenter == null || linePresenter.Typewriter == null) return;
            var handlers = linePresenter.Typewriter.ActionMarkupHandlers;
            if (!handlers.Contains(this)) handlers.Add(this);
            handlerRegistered = true;
        }

        private void UnregisterTypewriterHandler()
        {
            if (!handlerRegistered) return;
            if (linePresenter?.Typewriter != null)
                linePresenter.Typewriter.ActionMarkupHandlers.Remove(this);
            handlerRegistered = false;
        }

        private void ClearTrackedLine()
        {
            hasCurrentLineToken = false;
            currentLineOccurrence = 0;
            displayOccurrence = 0;
            currentLineToken = default;
        }
    }
}
