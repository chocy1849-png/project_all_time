using System;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Project-owned, non-persistent session state for the line currently
    /// presented by Yarn plus the Backlog and read history accumulated during
    /// this application session.
    /// </summary>
    public sealed class VNDialogueSessionState : MonoBehaviour
    {
        private readonly VNBacklogService backlog = new();
        private readonly VNReadHistoryService readHistory = new();

        private long currentOccurrence;
        private int currentPresentationStartedFrame = -1;
        private bool isLineActive;
        private bool isCurrentLineFullyDisplayed;
        private bool optionsActive;
        private string currentLineId = string.Empty;
        private string currentSpeakerName = string.Empty;
        private string currentText = string.Empty;

        public VNBacklogService Backlog => backlog;
        public VNReadHistoryService ReadHistory => readHistory;

        public bool IsLineActive => isLineActive;
        /// <summary>
        /// Monotonically changing identity for the current transient presentation.
        /// Automation uses this rather than a stable Yarn line ID, which can recur.
        /// </summary>
        public long CurrentPresentationOccurrence => currentOccurrence;
        /// <summary>Frame in which the current line occurrence began.</summary>
        public int CurrentPresentationStartedFrame => currentPresentationStartedFrame;
        public string CurrentLineId => currentLineId;
        public string CurrentSpeakerName => currentSpeakerName;
        public string CurrentText => currentText;
        public bool IsCurrentLineFullyDisplayed => isCurrentLineFullyDisplayed;
        public bool OptionsActive => optionsActive;

        public event Action CurrentLineChanged;
        public event Action<bool> CurrentLineFullDisplayChanged;
        public event Action<bool> OptionsActiveChanged;
        public event Action<string> ReadStateChanged;

        /// <summary>
        /// Marks the current stable line ID as read only after a future input
        /// bridge has accepted a real consume request for a fully displayed line.
        /// </summary>
        public bool TryAuthorizeCurrentLineConsume()
        {
            var fullyDisplayed = isCurrentLineFullyDisplayed;
            if (!isLineActive || !fullyDisplayed || optionsActive ||
                string.IsNullOrWhiteSpace(currentLineId))
                return false;

            var wasAdded = readHistory.RecordAuthorizedConsume(currentLineId);
            if (wasAdded) ReadStateChanged?.Invoke(currentLineId);
            VNConvenienceDiagnostics.Log(
                $"[M6-READ] consume result: lineId={currentLineId}, occurrence={currentOccurrence}, " +
                $"fullyDisplayed={fullyDisplayed}, recorded={wasAdded}, readHistoryContains={readHistory.IsRead(currentLineId)}");
            return true;
        }

        /// <summary>
        /// Clears only per-presentation state. Future Load/Stop orchestration may
        /// call this before it mutates the DialogueRunner; session data remains.
        /// </summary>
        public void InvalidateTransientPresentation()
        {
            currentOccurrence++;
            SetOptionsActive(false);
            ClearCurrentLine();
        }

        /// <summary>Explicit application-session reset for future New Game flow.</summary>
        public void ClearSession()
        {
            InvalidateTransientPresentation();
            backlog.ClearSession();
            readHistory.ClearSession();
        }

        internal long BeginLine(LocalizedLine line)
        {
            currentOccurrence++;
            currentPresentationStartedFrame = Time.frameCount;
            SetOptionsActive(false);

            isLineActive = true;
            isCurrentLineFullyDisplayed = false;
            currentLineId = line?.TextID ?? string.Empty;
            currentSpeakerName = line?.CharacterName ?? string.Empty;
            currentText = line?.TextWithoutCharacterName.Text ?? string.Empty;
            CurrentLineChanged?.Invoke();
            CurrentLineFullDisplayChanged?.Invoke(false);
            return currentOccurrence;
        }

        internal bool TryRecordFullDisplay(long occurrence, bool nextContentWasRequested)
        {
            if (occurrence != currentOccurrence || !isLineActive ||
                isCurrentLineFullyDisplayed || nextContentWasRequested)
                return false;

            isCurrentLineFullyDisplayed = true;
            backlog.Append(new VNBacklogEntry(currentLineId, currentSpeakerName, currentText));
            CurrentLineFullDisplayChanged?.Invoke(true);
            return true;
        }

        internal void EndLine(long occurrence)
        {
            if (occurrence != currentOccurrence) return;
            ClearCurrentLine();
        }

        internal long BeginOptions()
        {
            currentOccurrence++;
            ClearCurrentLine();
            SetOptionsActive(true);
            return currentOccurrence;
        }

        internal void EndOptions(long occurrence)
        {
            if (occurrence != currentOccurrence) return;
            SetOptionsActive(false);
        }

        private void ClearCurrentLine()
        {
            var hadLine = isLineActive || isCurrentLineFullyDisplayed ||
                          currentLineId.Length > 0 || currentSpeakerName.Length > 0 || currentText.Length > 0;
            var wasFullyDisplayed = isCurrentLineFullyDisplayed;
            isLineActive = false;
            isCurrentLineFullyDisplayed = false;
            currentLineId = string.Empty;
            currentSpeakerName = string.Empty;
            currentText = string.Empty;
            if (hadLine) CurrentLineChanged?.Invoke();
            if (wasFullyDisplayed) CurrentLineFullDisplayChanged?.Invoke(false);
        }

        private void SetOptionsActive(bool value)
        {
            if (optionsActive == value) return;
            optionsActive = value;
            OptionsActiveChanged?.Invoke(value);
        }
    }
}
