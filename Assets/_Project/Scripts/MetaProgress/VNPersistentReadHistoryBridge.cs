using System;
using ProjectAllTime.VN.Dialogue;

namespace ProjectAllTime.VN.MetaProgress
{
    /// <summary>
    /// Connects the existing M6 authorized-consume event to MetaProgress. The
    /// caller owns MetaProgress Load and supplies an already-loaded service.
    /// </summary>
    public sealed class VNPersistentReadHistoryBridge : IDisposable
    {
        private readonly VNDialogueSessionState sessionState;
        private readonly VNMetaProgressService metaProgressService;
        private bool initialized;
        private bool disposed;
        private string lastDiagnostic;

        public VNPersistentReadHistoryBridge(VNDialogueSessionState sessionState, VNMetaProgressService metaProgressService)
        {
            this.sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
            this.metaProgressService = metaProgressService ?? throw new ArgumentNullException(nameof(metaProgressService));
        }

        public bool IsInitialized => initialized;
        public string LastDiagnostic => lastDiagnostic;

        /// <summary>Seeds the durable baseline and subscribes exactly once.</summary>
        public bool Initialize()
        {
            if (disposed)
            {
                lastDiagnostic = "Persistent read-history bridge has been disposed.";
                return false;
            }

            if (!sessionState.ReadHistory.ReplacePersistentBaseline(metaProgressService.Current.readLineIds))
            {
                lastDiagnostic = "MetaProgress supplied an invalid read-history baseline.";
                return false;
            }

            if (!initialized)
            {
                sessionState.ReadStateChanged += HandleReadStateChanged;
                initialized = true;
            }

            lastDiagnostic = null;
            return true;
        }

        public void Dispose()
        {
            if (disposed) return;
            if (initialized) sessionState.ReadStateChanged -= HandleReadStateChanged;
            initialized = false;
            disposed = true;
        }

        private void HandleReadStateChanged(string lineId)
        {
            if (!metaProgressService.TryRecordReadLine(lineId))
            {
                lastDiagnostic = metaProgressService.LastDiagnostic ?? "MetaProgress did not durably persist the read line.";
                return;
            }

            if (!sessionState.ReadHistory.PromoteToPersistent(lineId))
            {
                lastDiagnostic = "MetaProgress persisted a read line, but the session baseline rejected it.";
                return;
            }

            lastDiagnostic = null;
        }
    }
}
