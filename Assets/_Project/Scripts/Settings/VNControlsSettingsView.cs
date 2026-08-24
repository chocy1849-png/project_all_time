using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    [DisallowMultipleComponent]
    public sealed class VNControlsSettingsView : MonoBehaviour
    {
        [SerializeField] private VNRebindItem[] rebindItems;
        [SerializeField] private Button resetAllButton;
        private readonly Dictionary<VNRebindTarget, VNRebindItem> items = new();
        private VNSettingsService settingsService;
        private VNInputRebindService rebindService;
        private Action<string> report;
        private bool initialized;
        private bool listenersRegistered;

        public bool IsListening => rebindService != null && rebindService.IsRebinding;

        private void OnEnable() => RegisterListeners(true);
        private void OnDisable()
        {
            RegisterListeners(false);
            PrepareForClose();
        }

        public bool Initialize(VNSettingsService service, VNInputRebindService serviceRebind, Action<string> diagnostic)
        {
            settingsService = service ?? throw new ArgumentNullException(nameof(service));
            rebindService = serviceRebind ?? throw new ArgumentNullException(nameof(serviceRebind));
            report = diagnostic;
            items.Clear();
            foreach (var item in rebindItems ?? Array.Empty<VNRebindItem>())
            {
                if (item == null || !items.TryAdd(item.Target, item))
                {
                    report?.Invoke("Controls UI contains a missing or duplicate rebind target.");
                    initialized = false;
                    return false;
                }
            }
            foreach (VNRebindTarget target in Enum.GetValues(typeof(VNRebindTarget)))
                if (!items.ContainsKey(target)) { report?.Invoke("Controls UI is missing a required rebind target."); initialized = false; return false; }
            initialized = true;
            RegisterListeners(true);
            Refresh(service.Current, service.CanWrite);
            return true;
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            diagnostic = null;
            if (resetAllButton == null || rebindItems == null || rebindItems.Length != Enum.GetValues(typeof(VNRebindTarget)).Length) { diagnostic = "Controls UI requires Reset All and exactly six rebind items."; return false; }
            var seen = new HashSet<VNRebindTarget>();
            foreach (var item in rebindItems)
                if (item == null || !item.TryValidateWiring(out diagnostic) || !seen.Add(item.Target)) { diagnostic ??= "Controls UI contains a missing or duplicate target."; return false; }
            diagnostic = null; return true;
        }

        public void Refresh(VNSettingsData settings, bool canWrite)
        {
            if (!initialized) return;
            var listeningTarget = rebindService.ActiveTarget;
            foreach (var pair in items)
            {
                if (!rebindService.TryGetBindingDisplay(pair.Key, out var display, out var diagnostic))
                {
                    report?.Invoke(diagnostic);
                    display = null;
                }
                pair.Value.Refresh(display, canWrite && !IsListening, listeningTarget == pair.Key);
            }
            if (resetAllButton != null) resetAllButton.interactable = canWrite && !IsListening;
        }

        public void PrepareForClose()
        {
            if (rebindService != null && rebindService.IsRebinding) rebindService.CancelActiveRebind();
        }

        private void HandleRebind(VNRebindTarget target)
        {
            if (!initialized || !settingsService.CanWrite || IsListening) return;
            if (!rebindService.BeginRebind(target, HandleRebindCompletion, out var diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }
        private void HandleReset(VNRebindTarget target)
        {
            if (!initialized || !settingsService.CanWrite || IsListening) return;
            var success = rebindService.TryResetBinding(target, out var diagnostic);
            if (!success || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }
        private void HandleResetAll()
        {
            if (!initialized || !settingsService.CanWrite || IsListening) return;
            var success = rebindService.TryResetAllBindings(out var diagnostic);
            if (!success || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic);
            Refresh(settingsService.Current, settingsService.CanWrite);
        }
        private void HandleRebindCompletion(VNRebindResult result, string diagnostic)
        {
            if (result != VNRebindResult.Succeeded || !string.IsNullOrEmpty(diagnostic)) report?.Invoke(diagnostic ?? result.ToString());
            Refresh(settingsService.Current, settingsService.CanWrite);
        }
        private void RegisterListeners(bool add)
        {
            if (listenersRegistered == add) return;
            listenersRegistered = add;
            if (resetAllButton != null) { if (add) resetAllButton.onClick.AddListener(HandleResetAll); else resetAllButton.onClick.RemoveListener(HandleResetAll); }
            foreach (var item in rebindItems ?? Array.Empty<VNRebindItem>())
            {
                if (item == null) continue;
                if (add) { item.RebindRequested += HandleRebind; item.ResetRequested += HandleReset; }
                else { item.RebindRequested -= HandleRebind; item.ResetRequested -= HandleReset; }
            }
        }
    }
}
