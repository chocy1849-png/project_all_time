using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Thin Input System adapter for M6. It forwards action callbacks to the
    /// project convenience layer and never implements dialogue progression.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VNConvenienceInputRouter : MonoBehaviour
    {
        [SerializeField] private VNConvenienceController convenienceController;
        [SerializeField] private VNInteractionGate interactionGate;
        [SerializeField] private InputActionReference advanceAction;
        [SerializeField] private InputActionReference toggleAutoAction;
        [SerializeField] private InputActionReference skipHoldAction;
        [SerializeField] private InputActionReference toggleHideAction;
        [SerializeField] private InputActionReference quickSaveAction;
        [SerializeField] private InputActionReference quickLoadAction;
        [SerializeField] private InputActionReference cancelAction;

        private readonly List<InputAction> actionsEnabledByRouter = new();
        private bool skipHoldActive;
        private bool skipWasEnabledBeforeHold;
        private bool autoWasEnabledBeforeHold;
        private bool holdWasInvalidatedByLoad;

        private void OnEnable()
        {
            SubscribeActions();
            if (convenienceController != null)
                convenienceController.SafeManualStateRequested += HandleSafeManualStateRequested;
        }

        private void OnDisable()
        {
            if (convenienceController != null)
                convenienceController.SafeManualStateRequested -= HandleSafeManualStateRequested;
            UnsubscribeActions();
            DisableActionsOwnedByRouter();
            ClearSkipHoldBookkeeping();
        }

        private void SubscribeActions()
        {
            SubscribePerformed(advanceAction, HandleAdvancePerformed, preventEnableWhileM5OwnsAdvance: true);
            SubscribePerformed(toggleAutoAction, HandleToggleAutoPerformed);
            SubscribeStarted(skipHoldAction, HandleSkipHoldStarted);
            SubscribeCanceled(skipHoldAction, HandleSkipHoldCanceled);
            EnsureActionEnabled(skipHoldAction);
            SubscribePerformed(toggleHideAction, HandleToggleHidePerformed);
            SubscribePerformed(quickSaveAction, HandleQuickSavePerformed);
            SubscribePerformed(quickLoadAction, HandleQuickLoadPerformed);
            SubscribePerformed(cancelAction, HandleCancelPerformed);
        }

        private void UnsubscribeActions()
        {
            UnsubscribePerformed(advanceAction, HandleAdvancePerformed);
            UnsubscribePerformed(toggleAutoAction, HandleToggleAutoPerformed);
            UnsubscribeStarted(skipHoldAction, HandleSkipHoldStarted);
            UnsubscribeCanceled(skipHoldAction, HandleSkipHoldCanceled);
            UnsubscribePerformed(toggleHideAction, HandleToggleHidePerformed);
            UnsubscribePerformed(quickSaveAction, HandleQuickSavePerformed);
            UnsubscribePerformed(quickLoadAction, HandleQuickLoadPerformed);
            UnsubscribePerformed(cancelAction, HandleCancelPerformed);
        }

        private void HandleAdvancePerformed(InputAction.CallbackContext context)
        {
            RouteAdvance(IsLeftMouseButton(context.control), IsPointerOverUi());
        }

        private void HandleToggleAutoPerformed(InputAction.CallbackContext context) => convenienceController?.ToggleAuto();
        private void HandleToggleHidePerformed(InputAction.CallbackContext context) => convenienceController?.ToggleUiVisibility();
        private void HandleQuickSavePerformed(InputAction.CallbackContext context) => convenienceController?.QuickSave();
        private void HandleQuickLoadPerformed(InputAction.CallbackContext context) => convenienceController?.QuickLoad();
        private void HandleCancelPerformed(InputAction.CallbackContext context) => convenienceController?.HandleCancel();
        private void HandleSkipHoldStarted(InputAction.CallbackContext context) => BeginSkipHold();
        private void HandleSkipHoldCanceled(InputAction.CallbackContext context) => EndSkipHold();

        /// <summary>Separated for deterministic tests; mouse UI suppression never applies to Space.</summary>
        internal bool RouteAdvance(bool triggeredByLeftMouse, bool pointerOverUi)
        {
            if (convenienceController == null || triggeredByLeftMouse && pointerOverUi) return false;
            return convenienceController.HandleManualAdvance();
        }

        /// <summary>Separated for deterministic tests; Ctrl remains a momentary Skip overlay.</summary>
        internal void BeginSkipHold()
        {
            if (convenienceController == null || skipHoldActive) return;
            holdWasInvalidatedByLoad = false;
            skipHoldActive = true;
            skipWasEnabledBeforeHold = convenienceController.IsSkipEnabled;
            autoWasEnabledBeforeHold = convenienceController.IsAutoEnabled;
            if (!skipWasEnabledBeforeHold) convenienceController.SetSkipEnabled(true);
        }

        /// <summary>Restores Auto only when the same hold was not invalidated by Load.</summary>
        internal void EndSkipHold()
        {
            if (!skipHoldActive) return;
            var restoreAuto = autoWasEnabledBeforeHold && !holdWasInvalidatedByLoad;
            var restoreSkip = skipWasEnabledBeforeHold && !holdWasInvalidatedByLoad;
            ClearSkipHoldBookkeeping();
            if (convenienceController == null) return;

            if (!restoreSkip) convenienceController.SetSkipEnabled(false);
            if (restoreAuto) convenienceController.SetAutoEnabled(true);
        }

        private void HandleSafeManualStateRequested()
        {
            // The controller has already set Auto and Skip OFF. Do not let a
            // later Ctrl release resurrect the state captured before Load.
            holdWasInvalidatedByLoad = true;
            skipHoldActive = false;
            skipWasEnabledBeforeHold = false;
            autoWasEnabledBeforeHold = false;
        }

        private void SubscribePerformed(InputActionReference reference, System.Action<InputAction.CallbackContext> callback, bool preventEnableWhileM5OwnsAdvance = false)
        {
            if (reference == null || reference.action == null) return;
            reference.action.performed += callback;
            if (!preventEnableWhileM5OwnsAdvance || !M5OwnsDialogueAdvance()) EnsureActionEnabled(reference);
        }

        private static void UnsubscribePerformed(InputActionReference reference, System.Action<InputAction.CallbackContext> callback)
        {
            if (reference != null && reference.action != null) reference.action.performed -= callback;
        }

        private void SubscribeStarted(InputActionReference reference, System.Action<InputAction.CallbackContext> callback)
        {
            if (reference == null || reference.action == null) return;
            reference.action.started += callback;
        }

        private static void UnsubscribeStarted(InputActionReference reference, System.Action<InputAction.CallbackContext> callback)
        {
            if (reference != null && reference.action != null) reference.action.started -= callback;
        }

        private void SubscribeCanceled(InputActionReference reference, System.Action<InputAction.CallbackContext> callback)
        {
            if (reference == null || reference.action == null) return;
            reference.action.canceled += callback;
        }

        private static void UnsubscribeCanceled(InputActionReference reference, System.Action<InputAction.CallbackContext> callback)
        {
            if (reference != null && reference.action != null) reference.action.canceled -= callback;
        }

        private void EnsureActionEnabled(InputActionReference reference)
        {
            if (reference == null || reference.action == null || reference.action.enabled) return;
            reference.action.Enable();
            if (!actionsEnabledByRouter.Contains(reference.action)) actionsEnabledByRouter.Add(reference.action);
        }

        private void DisableActionsOwnedByRouter()
        {
            foreach (var action in actionsEnabledByRouter)
                if (action != null && action.enabled) action.Disable();
            actionsEnabledByRouter.Clear();
        }

        private bool M5OwnsDialogueAdvance()
        {
            return interactionGate != null && interactionGate.IsM5ModalOrLoadActive;
        }

        private static bool IsLeftMouseButton(InputControl control)
        {
            return control is ButtonControl button && button.device is Mouse mouse && button == mouse.leftButton;
        }

        private static bool IsPointerOverUi()
        {
            var eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject();
        }

        private void ClearSkipHoldBookkeeping()
        {
            skipHoldActive = false;
            skipWasEnabledBeforeHold = false;
            autoWasEnabledBeforeHold = false;
        }
    }
}
