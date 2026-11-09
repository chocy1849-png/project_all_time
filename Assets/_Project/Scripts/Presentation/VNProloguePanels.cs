using ProjectAllTime.VN.SaveLoad;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Presentation
{
    /// <summary>Transient scene-owned medical insert and part title.</summary>
    public sealed class VNProloguePanels : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNSaveLoadController saveLoad;
        [SerializeField] private CanvasGroup diagram;
        [SerializeField] private CanvasGroup endTitle;
        private bool registered;
        public bool CanRestoreDiagram => isActiveAndEnabled && diagram != null && saveLoad != null;
        public bool RestoreDiagramForLoad()
        {
            if (!CanRestoreDiagram || !saveLoad.IsLoadInProgress) return false;
            ShowDiagram();
            return true;
        }

        private void OnEnable()
        {
            if (dialogueRunner == null || saveLoad == null || diagram == null || endTitle == null)
            {
                Debug.LogError("VNProloguePanels requires the runner, SaveLoad, diagram and title.", this);
                return;
            }
            HideAll();
            dialogueRunner.AddCommandHandler("vn_medical_diagram_show", ShowDiagram);
            dialogueRunner.AddCommandHandler("vn_medical_diagram_hide", HideDiagram);
            dialogueRunner.AddCommandHandler("vn_part_end_title", ShowTitle);
            dialogueRunner.onDialogueStart?.AddListener(HideAll);
            dialogueRunner.onDialogueComplete?.AddListener(HideDiagram);
            saveLoad.LoadStateChanged += OnLoad;
            registered = true;
        }
        private void OnDisable()
        {
            if (registered)
            {
                dialogueRunner.RemoveCommandHandler("vn_medical_diagram_show");
                dialogueRunner.RemoveCommandHandler("vn_medical_diagram_hide");
                dialogueRunner.RemoveCommandHandler("vn_part_end_title");
                dialogueRunner.onDialogueStart?.RemoveListener(HideAll);
                dialogueRunner.onDialogueComplete?.RemoveListener(HideDiagram);
                saveLoad.LoadStateChanged -= OnLoad;
                registered = false;
            }
            HideAll();
        }
        private void ShowDiagram() => Set(diagram, true);
        private void HideDiagram() => Set(diagram, false);
        private void ShowTitle() { Set(diagram, false); Set(endTitle, true); }
        private void OnLoad(bool active) { if (active) HideAll(); }
        private void HideAll() { Set(diagram, false); Set(endTitle, false); }
        private static void Set(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1 : 0;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}
