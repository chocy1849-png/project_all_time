using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Unity;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.SaveLoad;

namespace ProjectAllTime.VN.Records.Replay
{
    public enum VNReplaySessionLifecycle
    {
        Created,
        Running,
        Completed,
        Rejected,
        Failed,
        Cancelling,
        Cancelled,
        Disposed,
    }

    /// <summary>
    /// Owns a secondary Yarn runner, fresh variable storage, presenter, visual commands,
    /// and presentation hierarchy for one disposable Replay session.
    /// </summary>
    public sealed class VNReplaySession
    {
        private static readonly HashSet<Type> ForbiddenRuntimeComponents = new()
        {
            typeof(VNDialogueSessionState),
            typeof(VNLineLifecyclePresenter),
            typeof(VNLineLifecycleMarkupHandler),
            typeof(VNPersistentReadHistoryBridge),
            typeof(VNYarnCheckpointCommands),
            typeof(VNCheckpointService),
            typeof(VNYarnMetaProgressCommands),
            typeof(VNYarnAudioCommands),
            typeof(VNAudioController),
            typeof(VNYarnTransitionCommands),
            typeof(VNTransitionController),
            typeof(VNSaveLoadController),
        };

        private readonly YarnProject project;
        private readonly VNReplayContentPolicy contentPolicy;
        private readonly GameObject root;
        private readonly VNReplayPresentationCommands presentationCommands;
        private readonly CommandHandler originalCommandHandler;
        private VNReplaySessionLifecycle lifecycle = VNReplaySessionLifecycle.Created;
        private string diagnostic;

        public DialogueRunner Runner { get; }
        public InMemoryVariableStorage VariableStorage { get; }
        public DialoguePresenterBase Presenter { get; }
        public VNPresentationController PresentationController { get; }
        public VNReplaySessionLifecycle Lifecycle
        {
            get
            {
                if (lifecycle == VNReplaySessionLifecycle.Running &&
                    Runner != null && !Runner.IsDialogueRunning && Runner.DialogueTask.IsCompletedSuccessfully())
                    lifecycle = VNReplaySessionLifecycle.Completed;
                return lifecycle;
            }
        }
        public string Diagnostic => diagnostic;
        public bool IsDialogueRunning => Runner != null && Runner.IsDialogueRunning;
        public bool RuntimeHandlersRegistered => presentationCommands != null && presentationCommands.RegisteredRunner == Runner;

        public static VNReplaySession Create(
            YarnProject project,
            VNReplayContentPolicy contentPolicy,
            Func<Transform, VNPresentationController> presentationFactory,
            Func<Transform, DialoguePresenterBase> presenterFactory)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (contentPolicy == null) throw new ArgumentNullException(nameof(contentPolicy));
            if (presentationFactory == null) throw new ArgumentNullException(nameof(presentationFactory));
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var root = new GameObject("VN Replay Session");
            root.SetActive(false);
            try
            {
                var storage = root.AddComponent<InMemoryVariableStorage>();
                var runner = root.AddComponent<DialogueRunner>();
                runner.autoStart = false;
                runner.VariableStorage = storage;
                runner.SetProject(project);
                runner.VariableStorage.Program = project.Program;

                var presentation = presentationFactory(root.transform);
                var presenter = presenterFactory(root.transform);
                if (presentation == null || !presentation.transform.IsChildOf(root.transform))
                    throw new InvalidOperationException("Replay presentation factory must return a controller owned by the Replay hierarchy.");
                if (presenter == null || !presenter.transform.IsChildOf(root.transform))
                    throw new InvalidOperationException("Replay presenter factory must return a presenter owned by the Replay hierarchy.");

                EnsureReplayHierarchyHasNoProductionCapabilities(root);

                runner.DialoguePresenters = new[] { presenter };
                // Accessing this property creates Yarn Spinner's runner-owned fallback provider before Play starts.
                _ = runner.LineProvider;

                var commandOwnerObject = new GameObject("Replay Visual Command Owner");
                commandOwnerObject.transform.SetParent(root.transform, false);
                commandOwnerObject.SetActive(false);
                var commandOwner = commandOwnerObject.AddComponent<VNReplayPresentationCommands>();
                commandOwner.Bind(runner, presentation);

                // Yarn Spinner 3.2.7 returns early for unknown commands without completing VM content.
                // Intercept commands before dispatch so a bypassed static validation fails and stops cleanly.
                var originalHandler = runner.Dialogue.CommandHandler;
                if (originalHandler == null)
                    throw new InvalidOperationException("Replay runner has no Yarn command handler to guard.");

                var session = new VNReplaySession(
                    project,
                    contentPolicy,
                    root,
                    runner,
                    storage,
                    presenter,
                    presentation,
                    commandOwner,
                    originalHandler);
                runner.Dialogue.CommandHandler = session.GuardedCommandHandler;
                root.SetActive(true);
                commandOwnerObject.SetActive(true);
                return session;
            }
            catch
            {
                UnityEngine.Object.Destroy(root);
                throw;
            }
        }

        private static void EnsureReplayHierarchyHasNoProductionCapabilities(GameObject replayRoot)
        {
            foreach (var component in replayRoot.GetComponentsInChildren<Component>(true))
            {
                if (component == null || !ForbiddenRuntimeComponents.Contains(component.GetType())) continue;
                throw new InvalidOperationException(
                    $"Replay hierarchy cannot own production capability '{component.GetType().Name}'.");
            }
        }

        private VNReplaySession(
            YarnProject project,
            VNReplayContentPolicy contentPolicy,
            GameObject root,
            DialogueRunner runner,
            InMemoryVariableStorage variableStorage,
            DialoguePresenterBase presenter,
            VNPresentationController presentationController,
            VNReplayPresentationCommands presentationCommands,
            CommandHandler originalCommandHandler)
        {
            this.project = project;
            this.contentPolicy = contentPolicy;
            this.root = root;
            Runner = runner;
            VariableStorage = variableStorage;
            Presenter = presenter;
            PresentationController = presentationController;
            this.presentationCommands = presentationCommands;
            this.originalCommandHandler = originalCommandHandler;
        }

        public bool TryStart(string rootNode, out string failureDiagnostic)
        {
            failureDiagnostic = null;
            if (lifecycle != VNReplaySessionLifecycle.Created)
            {
                failureDiagnostic = "Replay session has already been started or disposed.";
                diagnostic = failureDiagnostic;
                return false;
            }

            var result = VNReplayContentValidator.Validate(project, rootNode, contentPolicy);
            if (!result.IsValid)
            {
                diagnostic = result.Diagnostic;
                lifecycle = VNReplaySessionLifecycle.Rejected;
                failureDiagnostic = diagnostic;
                return false;
            }

            lifecycle = VNReplaySessionLifecycle.Running;
            try
            {
                Runner.StartDialogue(rootNode).Forget();
                return true;
            }
            catch (Exception exception)
            {
                diagnostic = "Replay could not start: " + exception.Message;
                lifecycle = VNReplaySessionLifecycle.Failed;
                failureDiagnostic = diagnostic;
                return false;
            }
        }

        public async YarnTask CancelAsync()
        {
            if (Lifecycle != VNReplaySessionLifecycle.Running || !Runner.IsDialogueRunning) return;
            lifecycle = VNReplaySessionLifecycle.Cancelling;
            await Runner.Stop();
            ClearReplayVisualState();
            lifecycle = VNReplaySessionLifecycle.Cancelled;
        }

        public async YarnTask DisposeAsync()
        {
            if (lifecycle == VNReplaySessionLifecycle.Disposed) return;
            if (Runner != null && Runner.IsDialogueRunning)
            {
                lifecycle = VNReplaySessionLifecycle.Cancelling;
                await Runner.Stop();
            }
            ClearReplayVisualState();
            if (presentationCommands != null) presentationCommands.enabled = false;
            if (root != null) UnityEngine.Object.Destroy(root);
            lifecycle = VNReplaySessionLifecycle.Disposed;
        }

        /// <summary>Test and diagnostics seam: starts no content; exposes the reason a bypassed command was stopped.</summary>
        private void GuardedCommandHandler(Command command)
        {
            if (VNReplayContentPolicy.IsVisualCommandAllowed(command.Text))
            {
                originalCommandHandler(command);
                return;
            }

            var commandText = string.IsNullOrWhiteSpace(command.Text) ? "<empty>" : command.Text;
            diagnostic = $"Replay stopped before dispatching forbidden command '{commandText}'.";
            if (lifecycle != VNReplaySessionLifecycle.Disposed)
                lifecycle = VNReplaySessionLifecycle.Failed;

            if (Runner != null && Runner.IsDialogueRunning)
                StopAfterRuntimeFailureAsync().Forget();
        }

        private async YarnTask StopAfterRuntimeFailureAsync()
        {
            if (Runner != null && Runner.IsDialogueRunning)
                await Runner.Stop();
            ClearReplayVisualState();
        }

        private void ClearReplayVisualState()
        {
            if (PresentationController == null) return;
            PresentationController.ClearBackground();
            PresentationController.ClearCG();
            var visibleCharacterIds = new System.Collections.Generic.List<string>(PresentationController.VisibleCharacters.Keys);
            foreach (var characterId in visibleCharacterIds)
                PresentationController.HideCharacter(characterId);
        }
    }
}
