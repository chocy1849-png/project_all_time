using System;
using System.Collections;
using System.Collections.Generic;
using ProjectAllTime.VN.Audio;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

namespace ProjectAllTime.VN.SaveLoad
{
    public enum VNSaveLoadOperationStatus
    {
        Succeeded,
        ConfirmationRequired,
        SaveUnavailableNoCheckpoint,
        SaveUnavailableUnstableSnapshot,
        RepositoryWriteFailed,
        ThumbnailWarning,
        InvalidOrCorruptedLoad,
        UnsupportedSave,
        LoadValidationFailed,
        InvalidRequest,
        Busy,
    }

    public sealed class VNSaveLoadOperationResult
    {
        public VNSaveLoadOperationStatus Status { get; }
        public string Message { get; }
        public bool Succeeded => Status == VNSaveLoadOperationStatus.Succeeded;

        private VNSaveLoadOperationResult(VNSaveLoadOperationStatus status, string message)
        {
            Status = status;
            Message = message;
        }

        public static VNSaveLoadOperationResult Create(VNSaveLoadOperationStatus status, string message) => new(status, message);
    }

    /// <summary>
    /// Scene composition root and application boundary for M5's user-facing
    /// save/load workflow. Repository and full coordinator rules remain in
    /// their dedicated M5 services; this class only sequences UI, capture,
    /// checkpoint autosave, and input suppression around them.
    /// </summary>
    public sealed class VNSaveLoadController : MonoBehaviour
    {
        [Header("M5 Runtime Dependencies")]
        [SerializeField] private VNSaveLoadModal saveLoadModal;
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNCheckpointService checkpointService;
        [SerializeField] private VNPresentationController presentationController;
        [SerializeField] private VNTransitionController transitionController;
        [SerializeField] private VNAudioController audioController;
        [SerializeField] private VNPresentationCatalog presentationCatalog;
        [SerializeField] private VNDialogueSessionState sessionState;
        [SerializeField] private VNInteractionGate interactionGate;
        [SerializeField] private VNInformationOverlay informationOverlay;
        [SerializeField] private VNProloguePanels prologuePanels;

        [Header("M2 Modal Input Gate")]
        [Tooltip("Assign the existing VNInputActions Dialogue/Advance InputActionReference. It is disabled only while the Save/Load modal or a load transaction owns input.")]
        [SerializeField] private InputActionReference dialogueAdvanceAction;

        [Header("Autosave")]
        [SerializeField] private bool autosaveOnSuccessfulCheckpoint = true;

        private readonly VNPlayTimeTracker playTimeTracker = new();
        private readonly VNThumbnailService thumbnailService = new();
        private readonly HashSet<VNSaveSlotKey> thumbnailPlaceholderSlots = new();
        private readonly Dictionary<VNSaveSlotKey, long> thumbnailRevisions = new();
        private VNSaveRepository repository;
        private VNYarnSaveCoordinator saveCoordinator;
        private VNSaveSlotKey? pendingManualOverwrite;
        private readonly VNCheckpointAutosaveGuard autosaveGuard = new();
        private static readonly HashSet<string> ProductionAutosaveLines = new(StringComparer.Ordinal)
        {
            "line:prologue_s001_016", "line:prologue_s002_001", "line:prologue_s003_001",
            "line:prologue_s004_001", "line:prologue_s005_048", "line:prologue_s006_017",
            "line:prologue_s007_001", "line:prologue_s008_017",
        };
        private bool dialogueAdvanceWasEnabled;
        private bool dialogueAdvanceSuppressed;
        private bool loadInProgress;

        public event Action<VNSaveLoadOperationResult> OperationCompleted;
        public event Action<string> StatusChanged;
        /// <summary>Raised exactly while a validated Load transaction owns runtime state.</summary>
        public event Action<bool> LoadStateChanged;

        public VNPlayTimeTracker PlayTimeTracker => playTimeTracker;
        public VNSaveRepository Repository => repository;
        public VNSaveLoadMode CurrentMode => saveLoadModal == null ? VNSaveLoadMode.Save : saveLoadModal.Mode;
        public VNSaveLoadCategory CurrentCategory => saveLoadModal == null ? VNSaveLoadCategory.Manual : saveLoadModal.Category;
        public int CurrentPage => saveLoadModal == null ? 0 : saveLoadModal.Page;
        public bool IsModalOpen => saveLoadModal != null && saveLoadModal.IsOpen;
        public bool IsLoadInProgress => loadInProgress;
        public bool IsOverwriteConfirmationActive => saveLoadModal != null && saveLoadModal.IsOverwriteConfirmationActive;

        private void Awake()
        {
            repository = new VNSaveRepository();
            saveCoordinator = new VNYarnSaveCoordinator(repository, checkpointService, dialogueRunner, playTimeTracker, presentationController, transitionController, audioController, sessionState, interactionGate, informationOverlay, prologuePanels);
            saveLoadModal?.Initialize(this, repository, thumbnailService, presentationCatalog);
        }

        private void OnEnable()
        {
            if (checkpointService != null) checkpointService.CheckpointEntered += HandleCheckpointEntered;
            if (sessionState != null) sessionState.CurrentLineChanged += HandleProductionLineChanged;
        }

        private void OnDisable()
        {
            if (checkpointService != null) checkpointService.CheckpointEntered -= HandleCheckpointEntered;
            if (sessionState != null) sessionState.CurrentLineChanged -= HandleProductionLineChanged;
            saveLoadModal?.EndThumbnailVisualSuppression();
            RestoreDialogueAdvanceInput();
        }

        private void Update()
        {
            // M5 keeps elapsed application play time in one controller-owned
            // runtime service. Modal visibility intentionally does not reset it.
            playTimeTracker.TryAdvance(Time.unscaledDeltaTime);
        }

        public void OpenSave() => Open(VNSaveLoadMode.Save);

        public void OpenLoad() => Open(VNSaveLoadMode.Load);

        public void Close()
        {
            pendingManualOverwrite = null;
            saveLoadModal?.HideOverwriteConfirmation();
            saveLoadModal?.Hide();
            RestoreDialogueAdvanceInput();
        }

        public VNSaveLoadOperationResult SaveManual(int slotIndex)
        {
            var slotKey = new VNSaveSlotKey(VNSaveSlotType.Manual, slotIndex);
            if (!slotKey.IsValid) return Complete(VNSaveLoadOperationStatus.InvalidRequest, "The requested Manual save slot is invalid.");
            if (loadInProgress) return Complete(VNSaveLoadOperationStatus.Busy, "Load is already in progress.");

            var existing = repository.Read(slotKey);
            switch (existing.State)
            {
                case VNSaveSlotState.Empty:
                    return WriteCompleteSlot(slotKey, "Manual save complete.");
                case VNSaveSlotState.Valid:
                    if (!IsCompatibleOverwrite(slotKey, existing.SaveData))
                        return Complete(VNSaveLoadOperationStatus.UnsupportedSave, "This save is incompatible with the current narrative and is preserved.");
                    pendingManualOverwrite = slotKey;
                    saveLoadModal?.ShowOverwriteConfirmation(VNSaveLoadSlotModelBuilder.Build(repository.InspectAllSlots(), VNSaveLoadCategory.Manual, slotIndex / VNSaveLoadSlotModelBuilder.ManualSlotsPerPage)[slotIndex % VNSaveLoadSlotModelBuilder.ManualSlotsPerPage]);
                    return Complete(VNSaveLoadOperationStatus.ConfirmationRequired, "Confirm overwrite of the selected Manual save.");
                case VNSaveSlotState.Unsupported:
                    return Complete(VNSaveLoadOperationStatus.UnsupportedSave, "Unsupported Manual saves are preserved and cannot be overwritten.");
                case VNSaveSlotState.Corrupted:
                    return Complete(VNSaveLoadOperationStatus.InvalidOrCorruptedLoad, "Corrupted Manual saves are preserved and cannot be overwritten.");
                default:
                    return Complete(VNSaveLoadOperationStatus.InvalidRequest, "The selected save slot could not be inspected.");
            }
        }

        public VNSaveLoadOperationResult ConfirmManualOverwrite()
        {
            if (!pendingManualOverwrite.HasValue) return Complete(VNSaveLoadOperationStatus.InvalidRequest, "No Manual overwrite is pending.");
            var slotKey = pendingManualOverwrite.Value;
            pendingManualOverwrite = null;
            saveLoadModal?.HideOverwriteConfirmation();
            return WriteCompleteSlot(slotKey, "Manual save overwritten.", true);
        }

        public void CancelManualOverwrite()
        {
            pendingManualOverwrite = null;
            saveLoadModal?.HideOverwriteConfirmation();
            PublishStatus("Manual overwrite cancelled.");
        }

        public VNSaveLoadOperationResult QuickSave()
        {
            if (loadInProgress) return Complete(VNSaveLoadOperationStatus.Busy, "Load is already in progress.");
            return WriteCompleteSlot(new VNSaveSlotKey(VNSaveSlotType.Quick, 0), "Quick Save complete.");
        }

        public VNSaveLoadOperationResult Load(VNSaveSlotKey slotKey)
        {
            if (!slotKey.IsValid) return Complete(VNSaveLoadOperationStatus.InvalidRequest, "The requested load slot is invalid.");
            if (loadInProgress) return Complete(VNSaveLoadOperationStatus.Busy, "Load is already in progress.");

            var validation = saveCoordinator.ValidateLoad(slotKey);
            if (validation.Status != VNYarnLoadValidationStatus.Valid)
            {
                var status = validation.Status == VNYarnLoadValidationStatus.ReadFailed
                    ? VNSaveLoadOperationStatus.InvalidOrCorruptedLoad
                    : VNSaveLoadOperationStatus.LoadValidationFailed;
                return Complete(status, validation.Diagnostic ?? "The selected save could not be prepared for load.");
            }

            // The full M5-04 preflight completed without mutation. The modal is
            // now hidden, but input remains suppressed through the transaction.
            pendingManualOverwrite = null;
            saveLoadModal?.Hide();
            autosaveGuard.ExpectLoadedCheckpoint(validation.Checkpoint.CheckpointId);
            SetLoadInProgress(true);
            StartLoad(validation);
            return Complete(VNSaveLoadOperationStatus.Succeeded, "Load prepared and started.");
        }

        public VNSaveLoadOperationResult QuickLoad() => Load(new VNSaveSlotKey(VNSaveSlotType.Quick, 0));

        public void RefreshSlots()
        {
            if (saveLoadModal == null || !saveLoadModal.IsOpen) return;
            var models = VNSaveLoadSlotModelBuilder.Build(repository.InspectAllSlots(), saveLoadModal.Category, saveLoadModal.Page);
            foreach (var model in models)
                if (thumbnailPlaceholderSlots.Contains(model.SlotKey)) model.ThumbnailFileName = string.Empty;
            saveLoadModal.BindSlots(models);
        }

        public void SetCategory(VNSaveLoadCategory category)
        {
            if (saveLoadModal == null || !saveLoadModal.IsOpen) return;
            saveLoadModal.SetNavigation(category, 0);
            RefreshSlots();
        }

        public void ChangePage(int delta)
        {
            if (saveLoadModal == null || !saveLoadModal.IsOpen) return;
            saveLoadModal.SetNavigation(saveLoadModal.Category, saveLoadModal.Page + delta);
            RefreshSlots();
        }

        internal bool IsSlotInteractive(VNSaveSlotViewModel model)
        {
            if (model == null || saveLoadModal == null) return false;
            var interaction = VNSaveLoadInteractionPolicy.GetInteraction(saveLoadModal.Mode, saveLoadModal.Category, model.State);
            return interaction != VNSaveSlotInteraction.Disabled;
        }

        internal void HandleSlotSelected(VNSaveSlotViewModel model)
        {
            if (model == null || saveLoadModal == null) return;
            if (saveLoadModal.Mode == VNSaveLoadMode.Load)
            {
                Load(model.SlotKey);
                return;
            }

            switch (saveLoadModal.Category)
            {
                case VNSaveLoadCategory.Manual:
                    SaveManual(model.SlotKey.SlotIndex);
                    break;
                case VNSaveLoadCategory.Quick:
                    QuickSave();
                    break;
                default:
                    Complete(VNSaveLoadOperationStatus.InvalidRequest, "Auto saves are visible for inspection but are not manually writable.");
                    break;
            }
        }

        /// <summary>
        /// Public early pointer-down hook for an external Save/Load opener. It
        /// lets an opener suppress the same left-click before its Button.onClick
        /// reaches OpenSave/OpenLoad.
        /// </summary>
        public void BeginModalInputSuppression() => SuppressDialogueAdvanceInput();

        private void Open(VNSaveLoadMode mode)
        {
            if (saveLoadModal == null)
            {
                Complete(VNSaveLoadOperationStatus.InvalidRequest, "VNSaveLoadController requires a Save Load Modal reference.");
                return;
            }

            SuppressDialogueAdvanceInput();
            saveLoadModal.Show(mode, VNSaveLoadCategory.Manual, 0);
            RefreshSlots();
        }

        private VNSaveLoadOperationResult WriteCompleteSlot(VNSaveSlotKey slotKey, string successMessage, bool confirmedManualOverwrite = false)
        {
            var existing = repository.Read(slotKey);
            if (existing.State == VNSaveSlotState.Corrupted || existing.State == VNSaveSlotState.Unsupported ||
                (existing.State == VNSaveSlotState.Valid && !IsCompatibleOverwrite(slotKey, existing.SaveData)))
                return Complete(VNSaveLoadOperationStatus.UnsupportedSave, "The existing save is corrupted or incompatible and is preserved.");
            var result = saveCoordinator.TryWriteCompleteSave(slotKey, confirmedManualOverwrite, out var savedData);
            if (!result.Succeeded)
            {
                var status = ClassifySaveFailure(result.Diagnostic);
                return Complete(status, result.Diagnostic ?? "Save could not be written.");
            }

            // JSON is authoritative. Remove a prior canonical JPG before the
            // asynchronous replacement so an old image is never knowingly used
            // to represent this new save.
            thumbnailPlaceholderSlots.Add(slotKey);
            if (!thumbnailService.TryRemoveJpgSidecar(repository, slotKey))
                ReportThumbnailWarning(successMessage + " Thumbnail refresh may retain an old sidecar until replacement.");
            else
                PublishStatus(successMessage + " Thumbnail is refreshing.");

            QueueThumbnailCapture(slotKey, savedData);
            RefreshSlots();
            var completed = VNSaveLoadOperationResult.Create(VNSaveLoadOperationStatus.Succeeded, successMessage);
            OperationCompleted?.Invoke(completed);
            return completed;
        }

        private async void StartLoad(VNYarnLoadValidationResult validation)
        {
            try
            {
                var execution = await saveCoordinator.ExecuteValidatedLoad(validation);
                if (!execution.Succeeded)
                    Complete(VNSaveLoadOperationStatus.LoadValidationFailed, execution.Diagnostic ?? "Load execution failed after runtime mutation began.");
                else
                    PublishStatus("Load complete.");
            }
            catch (Exception)
            {
                Complete(VNSaveLoadOperationStatus.LoadValidationFailed, "Load execution failed unexpectedly.");
            }
            finally
            {
                // If the resumed node contained no matching checkpoint command,
                // do not let a later unrelated checkpoint be consumed.
                autosaveGuard.Clear();
                SetLoadInProgress(false);
                RestoreDialogueAdvanceInput();
            }
        }

        private void QueueThumbnailCapture(VNSaveSlotKey slotKey, SaveSlotData savedData)
        {
            thumbnailRevisions.TryGetValue(slotKey, out var revision);
            thumbnailRevisions[slotKey] = ++revision;
            var lineId = savedData != null && savedData.schemaVersion == VNSaveSerializer.ExactLineSchemaVersion
                ? savedData.resumeLineId : null;
            var occurrence = sessionState == null ? 0 : sessionState.CurrentPresentationOccurrence;
            StartCoroutine(CaptureThumbnailAfterSave(slotKey, revision, lineId, occurrence));
        }

        private IEnumerator CaptureThumbnailAfterSave(VNSaveSlotKey slotKey, long revision, string lineId, long occurrence)
        {
            var modalWasSuppressed = saveLoadModal != null && saveLoadModal.BeginThumbnailVisualSuppression();
            byte[] jpgBytes = null;
            string captureDiagnostic = null;
            yield return thumbnailService.CaptureCurrentGameViewJpg((bytes, diagnostic) =>
            {
                jpgBytes = bytes;
                captureDiagnostic = diagnostic;
            });
            if (modalWasSuppressed) saveLoadModal.EndThumbnailVisualSuppression();

            if (!thumbnailRevisions.TryGetValue(slotKey, out var currentRevision) || currentRevision != revision)
                yield break; // A newer JSON save owns this slot and its preview.
            if (!string.IsNullOrEmpty(lineId) && (loadInProgress || sessionState == null || !sessionState.IsLineActive ||
                sessionState.IsCurrentLineConsumed || sessionState.CurrentPresentationOccurrence != occurrence || sessionState.CurrentLineId != lineId))
            {
                ReportThumbnailWarning("Save succeeded, but its thumbnail was skipped because the displayed line changed.");
                RefreshSlots();
                yield break;
            }

            if (jpgBytes == null || jpgBytes.Length == 0)
            {
                ReportThumbnailWarning("Save succeeded, but thumbnail capture failed: " + (captureDiagnostic ?? "unknown capture error"));
                RefreshSlots();
                yield break;
            }

            var writeResult = thumbnailService.WriteJpgSidecar(repository, slotKey, jpgBytes);
            if (!writeResult.Succeeded)
                ReportThumbnailWarning("Save succeeded, but thumbnail could not be written: " + writeResult.Diagnostic);
            else
            {
                thumbnailPlaceholderSlots.Remove(slotKey);
                PublishStatus("Save complete.");
            }
            RefreshSlots();
        }

        private void HandleCheckpointEntered(VNCheckpointContext checkpoint)
        {
            if (autosaveGuard.ConsumeIfExpected(checkpoint.CheckpointId))
            {
                // Exactly one matching entry caused by the resumed save node is
                // consumed. Any later normal checkpoint entry autosaves again.
                return;
            }

            if (!autosaveOnSuccessfulCheckpoint) return;
            var allocation = repository.AllocateNextAutoSlot(IsCompatibleOverwrite);
            if (allocation.Status != VNAutoSlotAllocationStatus.Allocated || !allocation.SlotKey.HasValue)
            {
                PublishStatus(allocation.Diagnostic ?? "Autosave could not allocate a safe slot.");
                return;
            }

            var writeResult = saveCoordinator.TryWriteLegacyCheckpointAutoSave(allocation.SlotKey.Value, out var save);
            if (!writeResult.Succeeded)
            {
                PublishStatus("Autosave was skipped: " + (writeResult.Diagnostic ?? "complete snapshot was unavailable"));
                return;
            }

            thumbnailPlaceholderSlots.Add(allocation.SlotKey.Value);
            thumbnailService.TryRemoveJpgSidecar(repository, allocation.SlotKey.Value);
            QueueThumbnailCapture(allocation.SlotKey.Value, save);
            RefreshSlots();
        }

        private void HandleProductionLineChanged()
        {
            if (!autosaveOnSuccessfulCheckpoint || loadInProgress || sessionState == null ||
                !sessionState.IsLineActive || !ProductionAutosaveLines.Contains(sessionState.CurrentLineId)) return;
            StartCoroutine(AutosaveProductionLine(sessionState.CurrentLineId, sessionState.CurrentPresentationOccurrence));
        }

        private IEnumerator AutosaveProductionLine(string lineId, long occurrence)
        {
            // Let the line presenter and its already-awaited setup settle. A
            // changed occurrence or load cannot consume this pending request.
            yield return null;
            if (loadInProgress || sessionState == null || !sessionState.IsLineActive ||
                sessionState.CurrentPresentationOccurrence != occurrence || sessionState.CurrentLineId != lineId) yield break;
            var allocation = repository.AllocateNextAutoSlot(IsCompatibleOverwrite);
            if (allocation.Status != VNAutoSlotAllocationStatus.Allocated || !allocation.SlotKey.HasValue)
            {
                PublishStatus(allocation.Diagnostic ?? "Autosave could not allocate a safe slot.");
                yield break;
            }
            var key = allocation.SlotKey.Value;
            var result = saveCoordinator.TryWriteCompleteSave(key, out var save);
            if (!result.Succeeded || save == null || save.schemaVersion != VNSaveSerializer.ExactLineSchemaVersion)
            {
                PublishStatus("Autosave was skipped: " + (result.Diagnostic ?? "exact-line state was unavailable"));
                yield break;
            }
            thumbnailPlaceholderSlots.Add(key);
            if (!thumbnailService.TryRemoveJpgSidecar(repository, key))
                ReportThumbnailWarning("Autosave succeeded, but an old thumbnail could not be removed.");
            QueueThumbnailCapture(key, save);
            RefreshSlots();
        }

        private bool IsCompatibleOverwrite(VNSaveSlotKey key, SaveSlotData data)
        {
            return data != null && saveCoordinator != null &&
                saveCoordinator.ValidateLoad(key).Status == VNYarnLoadValidationStatus.Valid;
        }

        private void SuppressDialogueAdvanceInput()
        {
            if (dialogueAdvanceSuppressed) return;
            if (dialogueAdvanceAction == null || dialogueAdvanceAction.action == null)
            {
                PublishStatus("Save/Load modal input suppression requires the VNInputActions Dialogue/Advance InputActionReference.");
                return;
            }

            dialogueAdvanceWasEnabled = dialogueAdvanceAction.action.enabled;
            dialogueAdvanceAction.action.Disable();
            dialogueAdvanceSuppressed = true;
        }

        private void RestoreDialogueAdvanceInput()
        {
            if (!dialogueAdvanceSuppressed) return;
            if (dialogueAdvanceAction != null && dialogueAdvanceAction.action != null && dialogueAdvanceWasEnabled)
                dialogueAdvanceAction.action.Enable();
            dialogueAdvanceSuppressed = false;
            dialogueAdvanceWasEnabled = false;
        }

        private void SetLoadInProgress(bool value)
        {
            if (loadInProgress == value) return;
            loadInProgress = value;
            LoadStateChanged?.Invoke(value);
        }

        private VNSaveLoadOperationResult Complete(VNSaveLoadOperationStatus status, string message)
        {
            PublishStatus(message);
            var result = VNSaveLoadOperationResult.Create(status, message);
            OperationCompleted?.Invoke(result);
            return result;
        }

        private void ReportThumbnailWarning(string message)
        {
            PublishStatus(message);
            OperationCompleted?.Invoke(VNSaveLoadOperationResult.Create(VNSaveLoadOperationStatus.ThumbnailWarning, message));
        }

        private void PublishStatus(string message)
        {
            saveLoadModal?.SetStatus(message);
            StatusChanged?.Invoke(message);
        }

        private static VNSaveLoadOperationStatus ClassifySaveFailure(string diagnostic)
        {
            if (!string.IsNullOrEmpty(diagnostic) && diagnostic.IndexOf("No validated checkpoint", StringComparison.OrdinalIgnoreCase) >= 0)
                return VNSaveLoadOperationStatus.SaveUnavailableNoCheckpoint;
            if (!string.IsNullOrEmpty(diagnostic) &&
                (diagnostic.IndexOf("transition", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 diagnostic.IndexOf("Saving is unavailable", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 diagnostic.IndexOf("BGM", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 diagnostic.IndexOf("presentation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 diagnostic.IndexOf("audio", StringComparison.OrdinalIgnoreCase) >= 0))
                return VNSaveLoadOperationStatus.SaveUnavailableUnstableSnapshot;
            return VNSaveLoadOperationStatus.RepositoryWriteFailed;
        }
    }
}
