using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>Reflects the existing Toggle, including silent authority refreshes; owns no setting value.</summary>
    [DisallowMultipleComponent]
    public sealed class VNSettingsSwitchVisual : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image track;
        [SerializeField] private RectTransform thumb;
        [SerializeField] private TMP_Text stateLabel;
        private bool? lastOn, lastInteractable;
        public void Configure(Toggle owner, Image background, RectTransform handle, TMP_Text label)
        { toggle=owner; track=background; thumb=handle; stateLabel=label; Refresh(); }
        private void OnEnable() => Refresh();
        private void LateUpdate() { if(toggle!=null && (lastOn!=toggle.isOn || lastInteractable!=toggle.interactable)) Refresh(); }
        private void Refresh()
        {
            if(toggle==null) return; bool on=toggle.isOn, enabled=toggle.interactable;
            lastOn=on; lastInteractable=enabled;
            if(track!=null) track.color=enabled ? (on ? new Color32(67,111,104,255) : new Color32(27,41,43,255)) : new Color32(48,56,55,180);
            if(thumb!=null) { thumb.anchorMin=thumb.anchorMax=new Vector2(on?1:0,.5f); thumb.anchoredPosition=new Vector2(on?-20:20,0); var image=thumb.GetComponent<Image>(); if(image!=null) image.color=enabled ? (on?new Color32(220,236,229,255):new Color32(135,145,141,255)) : new Color32(100,108,105,180); }
            if(stateLabel!=null) { stateLabel.text=on?"켜짐":"꺼짐"; stateLabel.color=enabled ? new Color32(184,194,192,255) : new Color32(110,120,117,180); }
        }
    }
}
