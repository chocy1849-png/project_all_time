using System;
using ProjectAllTime.VN.Records.Timeline;
using ProjectAllTime.VN.Records.UI;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

namespace ProjectAllTime.VN.Records.Replay.UI
{
    /// <summary>Authoritative eligibility, session, and Records-modal lifecycle owner for Replay.</summary>
    [DisallowMultipleComponent]
    public sealed class VNReplayController : MonoBehaviour
    {
        [SerializeField] private VNRecordsModal recordsModal;
        [SerializeField] private Transform replaySessionHost;
        [SerializeField] private VNReplayView replayViewTemplate;

        private VNTimelineRuntime timelineRuntime;
        private VNTimelineCatalog timelineCatalog;
        private YarnProject yarnProject;
        private VNReplaySession activeSession;
        private VNReplayView activeView;
        private UnityAction activeCompletionListener;
        private VNRecordsModal subscribedRecordsModal;
        private bool initialized;
        private bool cleanupInProgress;

        public string LastDiagnostic { get; private set; }
        public bool HasActiveSession => activeSession != null && activeSession.Lifecycle != VNReplaySessionLifecycle.Disposed;
        public event Action ReplayEnded;

        private void OnEnable()
        {
            HideTemplate();
            SubscribeToRecordsModal();
        }

        private void OnDisable()
        {
            UnsubscribeFromRecordsModal();
            if (activeSession != null) CleanupReplayAsync(activeSession, cancel: true).Forget();
        }

        public bool Initialize(VNTimelineRuntime runtime, VNTimelineCatalog catalog, YarnProject project)
        {
            if (runtime == null) return Fail("A VNTimelineRuntime is required.");
            if (catalog == null) return Fail("A VNTimelineCatalog is required.");
            if (project == null) return Fail("A YarnProject is required.");

            if (initialized)
            {
                if (!ReferenceEquals(timelineRuntime, runtime) ||
                    !ReferenceEquals(timelineCatalog, catalog) ||
                    !ReferenceEquals(yarnProject, project))
                    return Fail("VNReplayController is already initialized with different dependencies.");

                LastDiagnostic = null;
                SubscribeToRecordsModal();
                HideTemplate();
                return true;
            }

            timelineRuntime = runtime;
            timelineCatalog = catalog;
            yarnProject = project;
            initialized = true;
            LastDiagnostic = null;
            SubscribeToRecordsModal();
            HideTemplate();
            return true;
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (recordsModal == null) return Fail("Replay controller requires a VNRecordsModal.", out diagnostic);
            if (replaySessionHost == null) return Fail("Replay controller requires a Replay session host.", out diagnostic);
            if (!replaySessionHost.IsChildOf(recordsModal.transform))
                return Fail("Replay session host must be inside the Records modal hierarchy.", out diagnostic);
            if (replayViewTemplate == null) return Fail("Replay controller requires an inactive VNReplayView template.", out diagnostic);
            if (replayViewTemplate.transform == replaySessionHost || replaySessionHost.IsChildOf(replayViewTemplate.transform))
                return Fail("Replay view template cannot contain the Replay session host.", out diagnostic);
            if (!replayViewTemplate.TryValidateWiring(out var viewDiagnostic))
                return Fail("Replay view template: " + viewDiagnostic, out diagnostic);

            diagnostic = null;
            return true;
        }

        /// <summary>Checks current visible Timeline state and safe single-node content without persistent writes.</summary>
        public bool CanReplay(string entryId)
        {
            if (!initialized) return Fail("Replay controller has not been initialized.");
            if (string.IsNullOrWhiteSpace(entryId)) return Fail("Timeline entry ID is empty.");
            if (cleanupInProgress || IsSessionBlockingNewReplay()) return Fail("Replay is already active.");

            VNTimelineSnapshot snapshot;
            try
            {
                snapshot = timelineRuntime.BuildSnapshot();
            }
            catch (Exception exception)
            {
                return Fail("Timeline state could not be checked: " + exception.Message);
            }

            if (snapshot == null || !snapshot.TryGetEntry(entryId, out var runtimeEntry))
                return Fail("Timeline entry is not visible.");
            if (runtimeEntry.State != VNTimelineEntryState.Completed)
                return Fail("Timeline entry is not completed.");
            if (!timelineCatalog.TryGetEntry(entryId, out var definition) || definition == null)
                return Fail("Timeline entry definition is unavailable.");
            if (string.IsNullOrWhiteSpace(definition.ReplayNode))
                return Fail("Timeline entry has no Replay node.");

            var policy = new VNReplayContentPolicy(new[] { definition.ReplayNode });
            var validation = VNReplayContentValidator.Validate(yarnProject, definition.ReplayNode, policy);
            if (!validation.IsValid)
                return Fail("Replay content is not safe for Replay v1.");

            LastDiagnostic = null;
            return true;
        }

        /// <summary>Revalidates entry eligibility, then creates one isolated Replay session beneath ReplayHost.</summary>
        public bool TryStartReplay(string entryId)
        {
            if (!DisposePreviousTerminalSession()) return false;
            if (!CanReplay(entryId)) return false;
            if (!TryValidateWiring(out var wiringDiagnostic)) return Fail(wiringDiagnostic);
            if (!recordsModal.IsOpen || recordsModal.ActiveTab != VNRecordsTab.Timeline)
                return Fail("Replay can start only from the open Timeline tab.");

            if (!timelineCatalog.TryGetEntry(entryId, out var definition) || definition == null || string.IsNullOrWhiteSpace(definition.ReplayNode))
                return Fail("Timeline entry has no Replay node.");

            var policy = new VNReplayContentPolicy(new[] { definition.ReplayNode });
            VNReplaySession session = null;
            VNReplayView view = null;
            try
            {
                session = VNReplaySession.Create(
                    yarnProject,
                    policy,
                    parent =>
                    {
                        view = CreateReplayView(parent);
                        return view.PresentationController;
                    },
                    _ => view == null ? null : view.LinePresenter,
                    replaySessionHost);
            }
            catch (Exception exception)
            {
                return Fail("Replay view or session could not be created: " + exception.Message);
            }

            activeSession = session;
            activeView = view;
            if (!view.Bind(session.Runner, out var viewDiagnostic))
            {
                LastDiagnostic = viewDiagnostic;
                CleanupReplayAsync(session, cancel: false).Forget();
                return false;
            }

            view.CloseRequested += HandleCloseRequested;
            session.Runner.onDialogueComplete ??= new UnityEvent();
            activeCompletionListener = () => HandleDialogueCompleted(session);
            session.Runner.onDialogueComplete.AddListener(activeCompletionListener);

            if (!session.TryStart(definition.ReplayNode, out var startDiagnostic))
            {
                LastDiagnostic = startDiagnostic ?? "Replay could not start.";
                CleanupReplayAsync(session, cancel: false).Forget();
                return false;
            }

            LastDiagnostic = null;
            return true;
        }

        public async YarnTask CancelActiveReplayAsync()
        {
            var session = activeSession;
            if (session == null || cleanupInProgress) return;
            await CleanupReplayAsync(session, cancel: true);
        }

        private VNReplayView CreateReplayView(Transform sessionRoot)
        {
            var clone = Instantiate(replayViewTemplate.gameObject, sessionRoot, false);
            clone.name = "VN Replay View";
            var view = clone.GetComponent<VNReplayView>();
            if (view == null) throw new InvalidOperationException("Replay view template root is missing VNReplayView.");
            if (!view.TryValidateWiring(out var diagnostic)) throw new InvalidOperationException(diagnostic);
            view.LinePresenter.autoAdvance = false;
            clone.SetActive(true);
            return view;
        }

        private void HandleDialogueCompleted(VNReplaySession session)
        {
            if (!ReferenceEquals(activeSession, session) || cleanupInProgress) return;

            var lifecycle = session.Lifecycle;
            if (lifecycle == VNReplaySessionLifecycle.Cancelling) return;
            if (lifecycle != VNReplaySessionLifecycle.Completed)
                LastDiagnostic = session.Diagnostic ?? "Replay ended before normal completion.";

            CleanupReplayAsync(session, cancel: false).Forget();
        }

        private void HandleCloseRequested() => CancelActiveReplayAsync().Forget();

        private void HandleRecordsClosed() => CancelActiveReplayAsync().Forget();

        private void HandleActiveTabChanged(VNRecordsTab tab)
        {
            if (tab != VNRecordsTab.Timeline) CancelActiveReplayAsync().Forget();
        }

        private async YarnTask CleanupReplayAsync(VNReplaySession session, bool cancel)
        {
            if (session == null || !ReferenceEquals(activeSession, session) || cleanupInProgress) return;
            cleanupInProgress = true;
            var view = activeView;
            if (view != null)
            {
                view.CloseRequested -= HandleCloseRequested;
                view.Unbind();
            }
            RemoveCompletionListener(session);

            try
            {
                if (cancel) await session.CancelAsync();
                await session.DisposeAsync();
            }
            catch (Exception exception)
            {
                LastDiagnostic = "Replay cleanup failed: " + exception.Message;
                try { await session.DisposeAsync(); }
                catch (Exception disposeException) { LastDiagnostic += " " + disposeException.Message; }
            }
            finally
            {
                if (ReferenceEquals(activeSession, session))
                {
                    activeSession = null;
                    activeView = null;
                }
                cleanupInProgress = false;
                ReplayEnded?.Invoke();
            }
        }

        private void RemoveCompletionListener(VNReplaySession session)
        {
            if (session?.Runner?.onDialogueComplete != null && activeCompletionListener != null)
                session.Runner.onDialogueComplete.RemoveListener(activeCompletionListener);
            activeCompletionListener = null;
        }

        private bool DisposePreviousTerminalSession()
        {
            if (activeSession == null) return true;
            if (cleanupInProgress || IsSessionBlockingNewReplay())
                return Fail("Replay is already active.");

            var previous = activeSession;
            CleanupReplayAsync(previous, cancel: false).Forget();
            if (activeSession != null) return Fail("Previous Replay is still cleaning up.");
            return true;
        }

        private bool IsSessionBlockingNewReplay()
        {
            if (activeSession == null) return false;
            var lifecycle = activeSession.Lifecycle;
            return lifecycle == VNReplaySessionLifecycle.Running ||
                   lifecycle == VNReplaySessionLifecycle.Cancelling;
        }

        private bool Fail(string diagnostic)
        {
            LastDiagnostic = diagnostic;
            return false;
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }

        private void HideTemplate()
        {
            if (replayViewTemplate != null && replayViewTemplate.gameObject.activeSelf)
                replayViewTemplate.gameObject.SetActive(false);
        }

        private void SubscribeToRecordsModal()
        {
            if (!isActiveAndEnabled || recordsModal == null || ReferenceEquals(subscribedRecordsModal, recordsModal)) return;
            UnsubscribeFromRecordsModal();
            subscribedRecordsModal = recordsModal;
            subscribedRecordsModal.Closed += HandleRecordsClosed;
            subscribedRecordsModal.ActiveTabChanged += HandleActiveTabChanged;
        }

        private void UnsubscribeFromRecordsModal()
        {
            if (subscribedRecordsModal == null) return;
            subscribedRecordsModal.Closed -= HandleRecordsClosed;
            subscribedRecordsModal.ActiveTabChanged -= HandleActiveTabChanged;
            subscribedRecordsModal = null;
        }
    }
}
