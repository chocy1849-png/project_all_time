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
        public bool TryValidateWiring(out string diagnostic)
        {
            if (bindingText == null || rebindButton == null || resetButton == null) { diagnostic = $"Rebind item '{target}' requires binding text and both buttons."; return false; }
            diagnostic = null; return true;
        }
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
            if (actionNameText != null) actionNameText.text = DisplayName(target);
            if (bindingText != null)
            {
                bindingText.text = listening ? "키 입력 대기… (Esc 취소)" : display?.DisplayString ?? "사용 불가";
                bindingText.color = listening ? new Color32(129, 213, 202, 255) : new Color32(247, 244, 239, 255);
            }
            if (rebindButton != null) rebindButton.interactable = canMutate && !listening;
            if (resetButton != null) resetButton.interactable = canMutate && !listening;
        }
        private static string DisplayName(VNRebindTarget value)
        {
            switch (value.ToString())
            {
                case "Advance": return "진행";
                case "ToggleAuto": return "자동 진행";
                case "SkipHold": return "건너뛰기";
                case "ToggleHide": return "대화창 숨기기";
                case "QuickSave": return "빠른 저장";
                case "QuickLoad": return "빠른 불러오기";
                default: return value.ToString();
            }
        }
        private void HandleRebind() => RebindRequested?.Invoke(target);
        private void HandleReset() => ResetRequested?.Invoke(target);
    }
}
