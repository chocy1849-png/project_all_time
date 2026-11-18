using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>Presentation-only Controls tabs. Binding strings always come from the existing rebind service.</summary>
    public sealed class VNControlsGuide : MonoBehaviour
    {
        [SerializeField] private VNControlsSettingsView owner;
        [SerializeField] private Button bindingsTab, guideTab;
        [SerializeField] private GameObject bindingsRoot, guideRoot;
        [SerializeField] private TMP_Text[] bindingLabels;
        private void OnEnable() { if(bindingsTab!=null) bindingsTab.onClick.AddListener(ShowBindings); if(guideTab!=null) guideTab.onClick.AddListener(ShowGuide); ShowBindings(); }
        private void OnDisable() { if(bindingsTab!=null) bindingsTab.onClick.RemoveListener(ShowBindings); if(guideTab!=null) guideTab.onClick.RemoveListener(ShowGuide); }
        public void Configure(VNControlsSettingsView view, Button bindings, Button guide, GameObject editable, GameObject readOnly, TMP_Text[] labels)
        { owner=view; bindingsTab=bindings; guideTab=guide; bindingsRoot=editable; guideRoot=readOnly; bindingLabels=labels; }
        public void ShowBindings() { SetMode(false); }
        public void ShowGuide() { if(owner!=null) owner.PrepareForClose(); SetMode(true); RefreshBindings(); }
        private void SetMode(bool guide)
        {
            if(bindingsRoot!=null) bindingsRoot.SetActive(!guide); if(guideRoot!=null) guideRoot.SetActive(guide);
            var a=bindingsTab!=null?bindingsTab.GetComponent<ProjectAllTime.VN.Dialogue.M13SharedControlState>():null;
            var b=guideTab!=null?guideTab.GetComponent<ProjectAllTime.VN.Dialogue.M13SharedControlState>():null;
            if(a!=null) a.SetActive(!guide); if(b!=null) b.SetActive(guide);
        }
        public void RefreshBindings()
        {
            if(owner==null || bindingLabels==null) return;
            for(int i=0;i<bindingLabels.Length;i++) if(bindingLabels[i]!=null) bindingLabels[i].text=owner.GetBindingLabel((VNRebindTarget)i);
        }
    }
}
