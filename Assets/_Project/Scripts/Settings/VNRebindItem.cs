using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    [DisallowMultipleComponent]
    public sealed class VNRebindItem : MonoBehaviour
    {
        [SerializeField] private VNRebindTarget target;
        [SerializeField] private TMP_Text actionNameText;
        [SerializeField] private TMP_Text bindingText;
        [SerializeField] private Button rebindButton;
        [SerializeField] private Button resetButton;

        public VNRebindTarget Target => target;
        public event Action<VNRebindTarget> RebindRequested;
        public event Action<VNRebindTarget> ResetRequested;

        private void OnEnable()
        {
            if (rebindButton != null) rebindButton.onClick.AddListener(HandleRebind);
            if (resetButton != null) resetButton.onClick.AddListener(HandleReset);
        }
        private void OnDisable()
        {
            if (rebindButton != null) rebindButton.onClick.RemoveListener(HandleRebind);
            if (resetButton != null) resetButton.onClick.RemoveListener(HandleReset);
        }
        public void Refresh(VNInputBindingDisplay display, bool canMutate, bool listening)
        {
            if (actionNameText != null) actionNameText.text = target.ToString();
            if (bindingText != null) bindingText.text = listening ? "Press a key… (Esc to cancel)" : display?.DisplayString ?? "Unavailable";
            if (rebindButton != null) rebindButton.interactable = canMutate && !listening;
            if (resetButton != null) resetButton.interactable = canMutate && !listening;
        }
        private void HandleRebind() => RebindRequested?.Invoke(target);
        private void HandleReset() => ResetRequested?.Invoke(target);
    }
}
