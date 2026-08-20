using System;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Presentation;
using Yarn.Unity;

namespace ProjectAllTime.VN.SaveLoad
{
    public enum VNYarnLoadValidationStatus
    {
        Valid,
        ReadFailed,
        InvalidCheckpoint,
        InvalidVariables,
        InvalidPresentation,
        InvalidAudio,
        InvalidPlayTime,
        InvalidDependencies,
    }

    /// <summary>
    /// Complete immutable-by-convention restore input prepared before any load
    /// mutation. Controller plans contain catalog-resolved stable logical state,
    /// never M3/M4 transient animation or source-role data.
    /// </summary>
    public sealed class VNYarnLoadValidationResult
    {
        public VNYarnLoadValidationStatus Status { get; }
        public SaveSlotData SaveData { get; }
        public VNCheckpointContext Checkpoint { get; }
        public string Diagnostic { get; }
        internal VNYarnVariableRestorePlan RestorePlan { get; }
        internal VNPresentationRestorePlan PresentationRestorePlan { get; }
        internal VNAudioRestorePlan AudioRestorePlan { get; }
        internal float PlayedSeconds { get; }

        private VNYarnLoadValidationResult(
            VNYarnLoadValidationStatus status,
            SaveSlotData saveData,
            VNCheckpointContext checkpoint,
            VNYarnVariableRestorePlan restorePlan,
            VNPresentationRestorePlan presentationRestorePlan,
            VNAudioRestorePlan audioRestorePlan,
            float playedSeconds,
            string diagnostic)
        {
            Status = status;
            SaveData = saveData;
            Checkpoint = checkpoint;
            RestorePlan = restorePlan;
            PresentationRestorePlan = presentationRestorePlan;
            AudioRestorePlan = audioRestorePlan;
            PlayedSeconds = playedSeconds;
            Diagnostic = diagnostic;
        }

        public static VNYarnLoadValidationResult Valid(
            SaveSlotData saveData,
            VNCheckpointContext checkpoint,
            VNYarnVariableRestorePlan restorePlan,
            VNPresentationRestorePlan presentationRestorePlan,
            VNAudioRestorePlan audioRestorePlan,
            float playedSeconds)
            => new(VNYarnLoadValidationStatus.Valid, saveData, checkpoint, restorePlan, presentationRestorePlan, audioRestorePlan, playedSeconds, null);

        public static VNYarnLoadValidationResult Failure(VNYarnLoadValidationStatus status, string diagnostic)
            => new(status, null, default, null, null, null, 0f, diagnostic);
    }

    public sealed class VNYarnLoadExecutionResult
    {
        public bool Succeeded { get; }
        public bool CrossedStopBoundary { get; }
        public string Diagnostic { get; }

        private VNYarnLoadExecutionResult(bool succeeded, bool crossedStopBoundary, string diagnostic)
        {
            Succeeded = succeeded;
            CrossedStopBoundary = crossedStopBoundary;
            Diagnostic = diagnostic;
        }

        public static VNYarnLoadExecutionResult Success(bool crossedStopBoundary) => new(true, crossedStopBoundary, null);
        public static VNYarnLoadExecutionResult Failure(bool crossedStopBoundary, string diagnostic) => new(false, crossedStopBoundary, diagnostic);
    }

    /// <summary>
    /// Full M5 logical-save coordinator. It composes state from the M3/M4
    /// controller authorities and keeps complete restore-plan validation ahead
    /// of the one-way DialogueRunner stop boundary.
    /// </summary>
    public sealed class VNYarnSaveCoordinator
    {
        private readonly VNSaveRepository repository;
        private readonly VNCheckpointService checkpointService;
        private readonly DialogueRunner dialogueRunner;
        private readonly VNPlayTimeTracker playTimeTracker;
        private readonly VNPresentationController presentationController;
        private readonly VNTransitionController transitionController;
        private readonly VNAudioController audioController;

        /// <summary>
        /// Retained for pre-M5-04 callers. Full composition requires the M3/M4
        /// dependencies supplied by the overload below and will report that
        /// missing wiring rather than returning a partial save.
        /// </summary>
        public VNYarnSaveCoordinator(VNSaveRepository repository, VNCheckpointService checkpointService, DialogueRunner dialogueRunner, VNPlayTimeTracker playTimeTracker)
            : this(repository, checkpointService, dialogueRunner, playTimeTracker, null, null, null) { }

        public VNYarnSaveCoordinator(
            VNSaveRepository repository,
            VNCheckpointService checkpointService,
            DialogueRunner dialogueRunner,
            VNPlayTimeTracker playTimeTracker,
            VNPresentationController presentationController,
            VNTransitionController transitionController,
            VNAudioController audioController)
        {
            this.repository = repository;
            this.checkpointService = checkpointService;
            this.dialogueRunner = dialogueRunner;
            this.playTimeTracker = playTimeTracker;
            this.presentationController = presentationController;
            this.transitionController = transitionController;
            this.audioController = audioController;
        }

        /// <summary>
        /// Builds one complete, production schema-v1 DTO only. It deliberately
        /// does not write a slot, letting application code sequence thumbnail
        /// invalidation and UI refresh around the authoritative JSON write.
        /// </summary>
        public bool TryComposeCompleteSave(VNSaveSlotKey slotKey, out SaveSlotData saveData, out string diagnostic)
        {
            saveData = null;
            if (!slotKey.IsValid)
            {
                diagnostic = "The requested save slot key is invalid.";
                return false;
            }

            if (!TryGetCoreDependencies(out diagnostic)) return false;
            if (!checkpointService.TryGetCurrentCheckpoint(out var checkpoint))
            {
                diagnostic = "No validated checkpoint is currently active.";
                return false;
            }

            if (!TryGetFullSnapshotDependencies(out diagnostic)) return false;
            if (transitionController.IsTransitionActive)
            {
                diagnostic = "Save composition is temporarily unavailable while a presentation transition is active.";
                return false;
            }

            if (audioController.IsBgmTransitionActive)
            {
                diagnostic = "Save composition is temporarily unavailable while a BGM transition is active.";
                return false;
            }

            if (!VNYarnVariableSnapshot.TryCapture(dialogueRunner.VariableStorage, out var yarnVariables, out diagnostic) ||
                !presentationController.TryCaptureStableState(out var presentationState, out diagnostic) ||
                !audioController.TryCaptureStableState(out var audioState, out diagnostic))
                return false;

            if (!slotKey.TryGetCanonicalThumbnailFileName(out var thumbnailFileName))
            {
                diagnostic = "The requested save slot cannot produce a canonical thumbnail filename.";
                return false;
            }

            saveData = new SaveSlotData
            {
                schemaVersion = VNSaveSerializer.CurrentSchemaVersion,
                slotType = slotKey.ToSerializedSlotType(),
                slotIndex = slotKey.SlotIndex,
                checkpointId = checkpoint.CheckpointId,
                resumeNode = checkpoint.ResumeNode,
                yarnVariables = yarnVariables,
                presentationState = presentationState,
                audioState = audioState,
                chapterId = checkpoint.ChapterId,
                sceneTitle = checkpoint.SceneTitle,
                playedSeconds = playTimeTracker.PlayedSeconds,
                savedAtUtcIso8601 = VNSaveSerializer.CreateUtcTimestamp(),
                thumbnailFileName = thumbnailFileName,
            };

            if (!VNSaveSerializer.TryValidate(saveData, slotKey, out diagnostic))
            {
                saveData = null;
                return false;
            }

            diagnostic = null;
            return true;
        }

        /// <summary>
        /// Compatibility alias for the M5-03/M5-04 tests and callers. The
        /// implementation has always been a real complete snapshot as of
        /// M5-04; new production callers should use TryComposeCompleteSave.
        /// </summary>
        public bool TryComposeTechnicalSave(VNSaveSlotKey slotKey, out SaveSlotData saveData, out string diagnostic)
            => TryComposeCompleteSave(slotKey, out saveData, out diagnostic);

        /// <summary>Composes and durably writes one complete slot snapshot.</summary>
        public VNSaveOperationResult TryWriteCompleteSave(VNSaveSlotKey slotKey, out SaveSlotData saveData)
        {
            saveData = null;
            if (!TryComposeCompleteSave(slotKey, out saveData, out var diagnostic))
                return VNSaveOperationResult.Failure(diagnostic);

            var writeResult = repository.Write(slotKey, saveData);
            if (!writeResult.Succeeded) saveData = null;
            return writeResult;
        }

        /// <summary>
        /// Unwired backend-only autosave capability. It uses the identical full
        /// composition path as all future save UI and never creates a partial
        /// autosave format or subscribes to checkpoint events itself.
        /// </summary>
        public VNSaveOperationResult TryWriteCompleteAutoSave()
            => TryWriteCompleteAutoSave(out _, out _);

        /// <summary>
        /// Backend autosave capability with the physical key and exact DTO
        /// exposed to the application layer for thumbnail follow-up work.
        /// </summary>
        public VNSaveOperationResult TryWriteCompleteAutoSave(out VNSaveSlotKey writtenKey, out SaveSlotData saveData)
        {
            writtenKey = default;
            saveData = null;
            if (!TryGetCoreDependencies(out var dependencyDiagnostic)) return VNSaveOperationResult.Failure(dependencyDiagnostic);
            var allocation = repository.AllocateNextAutoSlot();
            if (allocation.Status != VNAutoSlotAllocationStatus.Allocated || !allocation.SlotKey.HasValue)
                return VNSaveOperationResult.Failure(allocation.Diagnostic);

            var writeResult = TryWriteCompleteSave(allocation.SlotKey.Value, out saveData);
            if (writeResult.Succeeded)
            {
                writtenKey = allocation.SlotKey.Value;
                // The backend-only path has no capture coroutine, so it leaves
                // a placeholder rather than knowingly carrying a prior image
                // for newly authoritative JSON. The UI controller performs the
                // later end-of-frame capture.
                new VNThumbnailService().TryRemoveJpgSidecar(repository, writtenKey);
            }
            return writeResult;
        }

        /// <summary>
        /// Builds a complete presentation/audio/Yarn/playtime restore plan with
        /// no dialogue, variable, context, or M3/M4 runtime mutation.
        /// </summary>
        public VNYarnLoadValidationResult ValidateLoad(VNSaveSlotKey slotKey)
        {
            if (!TryGetCoreDependencies(out var dependencyDiagnostic))
                return VNYarnLoadValidationResult.Failure(VNYarnLoadValidationStatus.InvalidDependencies, dependencyDiagnostic);

            var readResult = repository.Read(slotKey);
            if (readResult.State != VNSaveSlotState.Valid)
            {
                var diagnostic = readResult.Diagnostic ?? $"Save slot is {readResult.State}.";
                return VNYarnLoadValidationResult.Failure(VNYarnLoadValidationStatus.ReadFailed, diagnostic);
            }

            if (!checkpointService.TryValidateSavedCheckpoint(readResult.SaveData, dialogueRunner, out var checkpoint, out var checkpointDiagnostic))
                return VNYarnLoadValidationResult.Failure(VNYarnLoadValidationStatus.InvalidCheckpoint, checkpointDiagnostic);

            if (!VNYarnVariableSnapshot.TryPrepareRestore(readResult.SaveData.yarnVariables, out var yarnRestorePlan, out var variablesDiagnostic))
                return VNYarnLoadValidationResult.Failure(VNYarnLoadValidationStatus.InvalidVariables, variablesDiagnostic);

            if (!TryGetFullSnapshotDependencies(out dependencyDiagnostic))
                return VNYarnLoadValidationResult.Failure(VNYarnLoadValidationStatus.InvalidDependencies, dependencyDiagnostic);

            if (!presentationController.TryPrepareRestore(readResult.SaveData.presentationState, out var presentationRestorePlan, out var presentationDiagnostic))
                return VNYarnLoadValidationResult.Failure(VNYarnLoadValidationStatus.InvalidPresentation, presentationDiagnostic);

            if (!audioController.TryPrepareRestore(readResult.SaveData.audioState, out var audioRestorePlan, out var audioDiagnostic))
                return VNYarnLoadValidationResult.Failure(VNYarnLoadValidationStatus.InvalidAudio, audioDiagnostic);

            if (!VNPlayTimeTracker.IsValidPlayedSeconds(readResult.SaveData.playedSeconds))
                return VNYarnLoadValidationResult.Failure(VNYarnLoadValidationStatus.InvalidPlayTime, "Saved play time is invalid.");

            return VNYarnLoadValidationResult.Valid(readResult.SaveData, checkpoint, yarnRestorePlan, presentationRestorePlan, audioRestorePlan, readResult.SaveData.playedSeconds);
        }

        /// <summary>
        /// Executes only a fully prepared load. Failures before this method
        /// preserve runtime state; once active dialogue is stopped, no rollback
        /// is attempted.
        /// </summary>
        public async YarnTask<VNYarnLoadExecutionResult> ExecuteValidatedLoad(VNYarnLoadValidationResult validation)
        {
            if (validation == null || validation.Status != VNYarnLoadValidationStatus.Valid || validation.RestorePlan == null ||
                validation.PresentationRestorePlan == null || validation.AudioRestorePlan == null)
                return VNYarnLoadExecutionResult.Failure(false, "A successful complete pre-mutation load validation result is required.");
            if (!TryGetCoreDependencies(out var dependencyDiagnostic) || !TryGetFullSnapshotDependencies(out dependencyDiagnostic))
                return VNYarnLoadExecutionResult.Failure(false, dependencyDiagnostic);

            var crossedStopBoundary = false;
            try
            {
                if (dialogueRunner.IsDialogueRunning)
                {
                    crossedStopBoundary = true;
                    await dialogueRunner.Stop();
                }

                transitionController.NormalizeForLoad();
                audioController.NormalizeTransientForLoad();

                if (!VNYarnVariableSnapshot.TryRestorePrepared(dialogueRunner.VariableStorage, validation.RestorePlan, out var variableDiagnostic))
                    return VNYarnLoadExecutionResult.Failure(crossedStopBoundary, variableDiagnostic);
                if (!presentationController.RestorePreparedState(validation.PresentationRestorePlan, out var presentationDiagnostic))
                    return VNYarnLoadExecutionResult.Failure(crossedStopBoundary, presentationDiagnostic);
                transitionController.FinalizeStableStateAfterLoad();
                if (!audioController.RestorePreparedState(validation.AudioRestorePlan, out var audioDiagnostic))
                    return VNYarnLoadExecutionResult.Failure(crossedStopBoundary, audioDiagnostic);
                if (!playTimeTracker.TrySetPlayedSeconds(validation.PlayedSeconds))
                    return VNYarnLoadExecutionResult.Failure(crossedStopBoundary, "Validated play time could not be restored.");

                // This is after the destructive boundary, but before the
                // re-entry node can issue any subsequent checkpoint command.
                checkpointService.AdoptValidatedContext(validation.Checkpoint);
                await dialogueRunner.StartDialogue(validation.Checkpoint.ResumeNode);
                return VNYarnLoadExecutionResult.Success(crossedStopBoundary);
            }
            catch (Exception)
            {
                return VNYarnLoadExecutionResult.Failure(crossedStopBoundary, "Load resume failed after runtime execution began; no rollback is attempted.");
            }
        }

        private bool TryGetCoreDependencies(out string diagnostic)
        {
            if (repository == null || checkpointService == null || dialogueRunner == null || playTimeTracker == null)
            {
                diagnostic = "VNYarnSaveCoordinator requires repository, checkpoint service, Dialogue Runner, and play-time tracker dependencies.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        private bool TryGetFullSnapshotDependencies(out string diagnostic)
        {
            if (presentationController == null || transitionController == null || audioController == null)
            {
                diagnostic = "Full M5 save/load requires Presentation Controller, Transition Controller, and Audio Controller references.";
                return false;
            }

            diagnostic = null;
            return true;
        }
    }
}
