using System;
using System.Collections.Generic;
using ProjectAllTime.VN.Dialogue;
using UnityEngine.InputSystem;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>
    /// Project-owned keyboard rebinding transaction owner. It resolves every
    /// binding by stable Action and Binding GUIDs; indices exist only at API use.
    /// </summary>
    public sealed class VNInputRebindService : IDisposable
    {
        private const string KeyboardPathPrefix = "<Keyboard>/";
        private static readonly TargetDefinition[] Definitions =
        {
            new(VNRebindTarget.Advance, "7c75d042-0409-418a-bf92-a84220ce2099", "10ff2c09-1c83-4da5-aefa-e2673f2cd6ba"),
            new(VNRebindTarget.ToggleAuto, "40c4fd51-8e22-48d7-91dd-90a40c664f55", "cd8b1b2c-2f7e-4350-a613-b1a3a03e5b50"),
            new(VNRebindTarget.SkipHold, "12575a0b-46d0-45af-98a6-4ae535125107", "2078a088-1d2d-4bb1-abbd-7dbcd1f86a45", "e92a13d9-0445-4866-b4ff-8cf1c84d84ca"),
            new(VNRebindTarget.ToggleHide, "b61e0edd-7c5b-4700-8ff0-e0d9e2c35999", "95a6ab90-446d-400f-9629-27666fc1a288"),
            new(VNRebindTarget.QuickSave, "f2bd4b3a-3bbd-4583-82f5-ff08e58e58e1", "84ee8301-4b8e-4a80-aa96-baaab0f7bb89"),
            new(VNRebindTarget.QuickLoad, "373b2241-7417-466f-8ab8-8db0df045fff", "1c5a833f-a797-4802-bf3e-0464906de59f"),
        };

        private static readonly Guid FixedAdvanceMouseBindingId = new("7e3a486e-8b78-41c4-b91c-91bb167f735e");
        private static readonly Guid FixedCancelBindingId = new("6ef85e6d-940b-4612-b1ce-4986893c4e63");
        private readonly VNSettingsService settingsService;
        private readonly InputActionAsset inputActions;
        private readonly VNConvenienceInputRouter router;
        private InputActionRebindingExtensions.RebindingOperation activeOperation;
        private InputAction activeAction;
        private bool activeActionWasEnabled;
        private VNRebindTarget? activeTarget;
        private Action<VNRebindResult, string> activeCompletion;
        private string capturedCandidatePath;
        private VNRebindResult? captureFailure;
        private string captureDiagnostic;
        private bool disposed;

        public VNInputRebindService(VNSettingsService settingsService, InputActionAsset inputActions, VNConvenienceInputRouter router)
        {
            this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.inputActions = inputActions ?? throw new ArgumentNullException(nameof(inputActions));
            this.router = router ?? throw new ArgumentNullException(nameof(router));
        }

        public bool IsRebinding => activeOperation != null;
        public VNRebindTarget? ActiveTarget => activeTarget;

        public bool TryValidateInputContract(out string diagnostic)
        {
            foreach (var definition in Definitions)
            {
                if (!TryResolve(definition.Target, out var resolved, out diagnostic)) return false;
                if (!IsKeyboardOriginal(resolved.Action.bindings[resolved.PrimaryIndex]))
                {
                    diagnostic = $"Target '{definition.Target}' no longer has the required original Keyboard binding.";
                    return false;
                }
                if (resolved.HasCompanion && !IsKeyboardOriginal(resolved.Action.bindings[resolved.CompanionIndex]))
                {
                    diagnostic = "SkipHold no longer has the required original Right Ctrl Keyboard binding.";
                    return false;
                }
            }

            if (!TryFindBinding(FixedAdvanceMouseBindingId, out var mouseAction, out var mouseIndex) || !IsMouseLeftOriginal(mouseAction.bindings[mouseIndex]))
            {
                diagnostic = "The fixed Advance Left Mouse binding is missing or invalid.";
                return false;
            }
            if (!TryFindBinding(FixedCancelBindingId, out var cancelAction, out var cancelIndex) || !IsEscapeOriginal(cancelAction.bindings[cancelIndex]))
            {
                diagnostic = "The fixed Cancel Escape binding is missing or invalid.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        public bool TryApplyCurrentSettings(out string diagnostic)
        {
            if (!TryValidateInputContract(out diagnostic)) return false;
            var previous = CaptureOverrides();
            try
            {
                var json = settingsService.Current.inputBindingOverridesJson;
                inputActions.RemoveAllBindingOverrides();
                if (!string.IsNullOrEmpty(json)) inputActions.LoadBindingOverridesFromJson(json, removeExisting: false);
                if (!TryValidateRuntimeOverrides(out diagnostic))
                {
                    RestoreOverrides(previous);
                    return false;
                }
                diagnostic = null;
                return true;
            }
            catch (Exception)
            {
                RestoreOverrides(previous);
                diagnostic = "Persisted input binding overrides could not be applied.";
                return false;
            }
        }

        public bool TryGetBindingDisplay(VNRebindTarget target, out VNInputBindingDisplay display, out string diagnostic)
        {
            display = null;
            if (!TryResolve(target, out var resolved, out diagnostic)) return false;
            var primary = resolved.Action.bindings[resolved.PrimaryIndex];
            var isDefault = primary.overridePath == null && !HasProcessorOrInteractionOverride(primary);
            var effectivePath = primary.effectivePath ?? string.Empty;
            var displayString = resolved.Action.GetBindingDisplayString(resolved.PrimaryIndex);
            if (target == VNRebindTarget.SkipHold && isDefault && IsDefaultBinding(resolved.Action.bindings[resolved.CompanionIndex]))
            {
                effectivePath = string.Concat(effectivePath, " | ", resolved.Action.bindings[resolved.CompanionIndex].effectivePath ?? string.Empty);
                displayString = string.Concat(displayString, " / ", resolved.Action.GetBindingDisplayString(resolved.CompanionIndex));
            }
            display = new VNInputBindingDisplay(target, effectivePath, displayString, isDefault);
            diagnostic = null;
            return true;
        }

        public bool BeginRebind(VNRebindTarget target, Action<VNRebindResult, string> completion, out string diagnostic)
        {
            if (disposed || IsRebinding)
            {
                diagnostic = disposed ? "Input rebinding service is disposed." : "Another input rebind is already active.";
                return false;
            }
            if (!TryValidateInputContract(out diagnostic) || !TryResolve(target, out var resolved, out diagnostic)) return false;

            activeTarget = target;
            activeAction = resolved.Action;
            activeActionWasEnabled = activeAction.enabled;
            activeCompletion = completion;
            capturedCandidatePath = null;
            captureFailure = null;
            captureDiagnostic = null;
            router.BeginRebindCaptureSuspension();
            try
            {
                if (activeActionWasEnabled) activeAction.Disable();
                activeOperation = activeAction.PerformInteractiveRebinding(resolved.PrimaryIndex)
                    .WithControlsHavingToMatchPath("<Keyboard>")
                    .WithCancelingThrough("<Keyboard>/escape")
                    .WithMatchingEventsBeingSuppressed(true)
                    .WithActionEventNotificationsBeingSuppressed(true)
                    .OnMatchWaitForAnother(0f)
                    .OnApplyBinding((_, path) => StageCandidate(path))
                    .OnCancel(_ => FinishActive(VNRebindResult.Canceled, "Input rebind was canceled."))
                    .OnComplete(_ => CompleteCapturedRebind());
                activeOperation.Start();
                diagnostic = null;
                return true;
            }
            catch (Exception)
            {
                FinishActive(VNRebindResult.ContractInvalid, "Input rebind could not be started.");
                diagnostic = "Input rebind could not be started.";
                return false;
            }
        }

        public void CancelActiveRebind()
        {
            if (activeOperation == null) return;
            try { activeOperation.Cancel(); }
            catch (Exception) { FinishActive(VNRebindResult.Canceled, "Input rebind was canceled."); }
        }

        public bool TryResetBinding(VNRebindTarget target, out string diagnostic)
        {
            if (IsRebinding) { diagnostic = "Cannot reset bindings while a rebind is active."; return false; }
            if (!TryValidateInputContract(out diagnostic) || !TryResolve(target, out var resolved, out diagnostic)) return false;
            var previous = CaptureOverrides();
            resolved.Action.RemoveBindingOverride(resolved.PrimaryIndex);
            if (resolved.HasCompanion) resolved.Action.RemoveBindingOverride(resolved.CompanionIndex);
            if (!TryValidateRuntimeOverrides(out diagnostic) || !TryPersistCurrentOverrides(out diagnostic))
            {
                RestoreOverrides(previous);
                return false;
            }
            return true;
        }

        public bool TryResetAllBindings(out string diagnostic)
        {
            if (IsRebinding) { diagnostic = "Cannot reset bindings while a rebind is active."; return false; }
            if (!TryValidateInputContract(out diagnostic)) return false;
            var previous = CaptureOverrides();
            inputActions.RemoveAllBindingOverrides();
            if (!TryValidateRuntimeOverrides(out diagnostic)) { RestoreOverrides(previous); return false; }
            var replacement = settingsService.Current;
            replacement.inputBindingOverridesJson = string.Empty;
            if (!settingsService.TrySave(replacement, out diagnostic)) { RestoreOverrides(previous); return false; }
            return true;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            CancelActiveRebind();
            if (activeOperation != null) FinishActive(VNRebindResult.Canceled, "Input rebind service was disposed.");
        }

        private void StageCandidate(string path)
        {
            capturedCandidatePath = path;
            if (!IsKeyboardPath(path))
            {
                captureFailure = VNRebindResult.ContractInvalid;
                captureDiagnostic = "Only Keyboard controls may be rebound.";
                return;
            }
            if (string.Equals(path, "<Keyboard>/escape", StringComparison.OrdinalIgnoreCase))
            {
                captureFailure = VNRebindResult.Canceled;
                captureDiagnostic = "Escape is reserved for canceling input rebinding.";
                return;
            }
            if (IsDuplicatePath(activeTarget.Value, path))
            {
                captureFailure = VNRebindResult.Duplicate;
                captureDiagnostic = $"Keyboard control '{path}' is already assigned to another M7 shortcut.";
            }
        }

        private void CompleteCapturedRebind()
        {
            if (captureFailure.HasValue) { FinishActive(captureFailure.Value, captureDiagnostic); return; }
            var diagnostic = string.Empty;
            if (string.IsNullOrEmpty(capturedCandidatePath) || !TryResolve(activeTarget.Value, out var resolved, out diagnostic))
            {
                FinishActive(VNRebindResult.ContractInvalid, string.IsNullOrEmpty(diagnostic) ? "No valid Keyboard control was captured." : diagnostic);
                return;
            }
            var previous = CaptureOverrides();
            resolved.Action.ApplyBindingOverride(resolved.PrimaryIndex, capturedCandidatePath);
            if (resolved.HasCompanion) resolved.Action.ApplyBindingOverride(resolved.CompanionIndex, string.Empty);
            if (!TryValidateRuntimeOverrides(out diagnostic))
            {
                RestoreOverrides(previous);
                FinishActive(VNRebindResult.ContractInvalid, diagnostic);
                return;
            }
            if (!TryPersistCurrentOverrides(out diagnostic))
            {
                RestoreOverrides(previous);
                FinishActive(VNRebindResult.PersistenceFailed, diagnostic);
                return;
            }
            FinishActive(VNRebindResult.Succeeded, null);
        }

        private bool TryPersistCurrentOverrides(out string diagnostic)
        {
            var replacement = settingsService.Current;
            replacement.inputBindingOverridesJson = HasAnyOverrides() ? inputActions.SaveBindingOverridesAsJson() : string.Empty;
            return settingsService.TrySave(replacement, out diagnostic);
        }

        private bool TryValidateRuntimeOverrides(out string diagnostic)
        {
            if (!TryValidateInputContract(out diagnostic)) return false;
            if (!TryFindBinding(FixedAdvanceMouseBindingId, out var mouseAction, out var mouseIndex) || !IsDefaultBinding(mouseAction.bindings[mouseIndex]))
            {
                diagnostic = "Persisted overrides may not modify fixed Advance Left Mouse.";
                return false;
            }
            if (!TryFindBinding(FixedCancelBindingId, out var cancelAction, out var cancelIndex) || !IsDefaultBinding(cancelAction.bindings[cancelIndex]))
            {
                diagnostic = "Persisted overrides may not modify fixed Cancel Escape.";
                return false;
            }

            var occupied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var definition in Definitions)
            {
                if (!TryResolve(definition.Target, out var resolved, out diagnostic)) return false;
                var primary = resolved.Action.bindings[resolved.PrimaryIndex];
                if (HasProcessorOrInteractionOverride(primary))
                {
                    diagnostic = $"Target '{definition.Target}' may not override processors or interactions.";
                    return false;
                }
                if (!IsKeyboardPath(primary.effectivePath))
                {
                    diagnostic = $"Target '{definition.Target}' has a non-Keyboard effective binding.";
                    return false;
                }
                if (!occupied.Add(primary.effectivePath))
                {
                    diagnostic = $"Duplicate Keyboard binding '{primary.effectivePath}' is not allowed.";
                    return false;
                }
                if (resolved.HasCompanion)
                {
                    var companion = resolved.Action.bindings[resolved.CompanionIndex];
                    if (HasProcessorOrInteractionOverride(companion))
                    {
                        diagnostic = "SkipHold companion may not override processors or interactions.";
                        return false;
                    }
                    var defaultState = primary.overridePath == null && companion.overridePath == null;
                    var customState = !string.IsNullOrEmpty(primary.overridePath) && companion.overridePath == string.Empty;
                    if (!defaultState && !customState)
                    {
                        diagnostic = "SkipHold overrides must be default dual-Ctrl or custom primary with disabled Right Ctrl companion.";
                        return false;
                    }
                    if (defaultState && (!IsKeyboardPath(companion.effectivePath) || !occupied.Add(companion.effectivePath)))
                    {
                        diagnostic = "SkipHold Right Ctrl companion is invalid or duplicates another shortcut.";
                        return false;
                    }
                }
            }
            diagnostic = null;
            return true;
        }

        private bool IsDuplicatePath(VNRebindTarget candidateTarget, string path)
        {
            foreach (var definition in Definitions)
            {
                if (definition.Target == candidateTarget) continue;
                if (!TryResolve(definition.Target, out var resolved, out _)) return true;
                if (string.Equals(resolved.Action.bindings[resolved.PrimaryIndex].effectivePath, path, StringComparison.OrdinalIgnoreCase)) return true;
                if (resolved.HasCompanion && string.Equals(resolved.Action.bindings[resolved.CompanionIndex].effectivePath, path, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private bool TryResolve(VNRebindTarget target, out ResolvedTarget resolved, out string diagnostic)
        {
            resolved = default;
            var definition = Array.Find(Definitions, item => item.Target == target);
            if (definition == null) { diagnostic = "Unknown input rebind target."; return false; }
            var action = inputActions.FindAction(definition.ActionId);
            if (action == null) { diagnostic = $"Expected Action ID '{definition.ActionId}' is missing for '{target}'."; return false; }
            if (!TryFindBindingIndex(action, definition.BindingId, out var primaryIndex))
            {
                diagnostic = $"Expected Binding ID '{definition.BindingId}' is missing for '{target}'.";
                return false;
            }
            var companionIndex = -1;
            if (definition.CompanionBindingId != Guid.Empty && !TryFindBindingIndex(action, definition.CompanionBindingId, out companionIndex))
            {
                diagnostic = $"Expected SkipHold companion Binding ID '{definition.CompanionBindingId}' is missing.";
                return false;
            }
            resolved = new ResolvedTarget(action, primaryIndex, companionIndex);
            diagnostic = null;
            return true;
        }

        private bool TryFindBinding(Guid bindingId, out InputAction action, out int bindingIndex)
        {
            foreach (var map in inputActions.actionMaps)
            foreach (var candidate in map.actions)
            if (TryFindBindingIndex(candidate, bindingId, out bindingIndex)) { action = candidate; return true; }
            action = null;
            bindingIndex = -1;
            return false;
        }

        private static bool TryFindBindingIndex(InputAction action, Guid bindingId, out int bindingIndex)
        {
            for (var index = 0; index < action.bindings.Count; index++)
                if (action.bindings[index].id == bindingId) { bindingIndex = index; return true; }
            bindingIndex = -1;
            return false;
        }

        private string CaptureOverrides() => HasAnyOverrides() ? inputActions.SaveBindingOverridesAsJson() : string.Empty;

        private void RestoreOverrides(string json)
        {
            inputActions.RemoveAllBindingOverrides();
            if (!string.IsNullOrEmpty(json)) inputActions.LoadBindingOverridesFromJson(json, removeExisting: false);
        }

        private bool HasAnyOverrides()
        {
            foreach (var map in inputActions.actionMaps)
            foreach (var binding in map.bindings)
                if (binding.overridePath != null || binding.overrideProcessors != null || binding.overrideInteractions != null)
                    return true;
            return false;
        }

        private void FinishActive(VNRebindResult result, string diagnostic)
        {
            var operation = activeOperation;
            var action = activeAction;
            var wasEnabled = activeActionWasEnabled;
            var completion = activeCompletion;
            activeOperation = null;
            activeAction = null;
            activeTarget = null;
            activeCompletion = null;
            capturedCandidatePath = null;
            captureFailure = null;
            captureDiagnostic = null;
            activeActionWasEnabled = false;
            try { operation?.Dispose(); }
            finally
            {
                if (wasEnabled && action != null && !action.enabled) action.Enable();
                router.EndRebindCaptureSuspension();
            }
            completion?.Invoke(result, diagnostic);
        }

        private static bool IsKeyboardOriginal(InputBinding binding) => IsKeyboardPath(binding.path);
        private static bool IsDefaultBinding(InputBinding binding) => binding.overridePath == null && !HasProcessorOrInteractionOverride(binding);
        private static bool HasProcessorOrInteractionOverride(InputBinding binding) => binding.overrideProcessors != null || binding.overrideInteractions != null;
        private static bool IsMouseLeftOriginal(InputBinding binding) => string.Equals(binding.path, "<Mouse>/leftButton", StringComparison.OrdinalIgnoreCase);
        private static bool IsEscapeOriginal(InputBinding binding) => string.Equals(binding.path, "<Keyboard>/escape", StringComparison.OrdinalIgnoreCase);
        private static bool IsKeyboardPath(string path) => !string.IsNullOrEmpty(path) && path.StartsWith(KeyboardPathPrefix, StringComparison.OrdinalIgnoreCase);

        private sealed class TargetDefinition
        {
            public VNRebindTarget Target { get; }
            public Guid ActionId { get; }
            public Guid BindingId { get; }
            public Guid CompanionBindingId { get; }
            public TargetDefinition(VNRebindTarget target, string actionId, string bindingId, string companionBindingId = null)
            {
                Target = target;
                ActionId = new Guid(actionId);
                BindingId = new Guid(bindingId);
                CompanionBindingId = companionBindingId == null ? Guid.Empty : new Guid(companionBindingId);
            }
        }

        private readonly struct ResolvedTarget
        {
            public InputAction Action { get; }
            public int PrimaryIndex { get; }
            public int CompanionIndex { get; }
            public bool HasCompanion => CompanionIndex >= 0;
            public ResolvedTarget(InputAction action, int primaryIndex, int companionIndex)
            {
                Action = action;
                PrimaryIndex = primaryIndex;
                CompanionIndex = companionIndex;
            }
        }
    }
}
