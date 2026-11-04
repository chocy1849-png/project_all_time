using System.Collections;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Presentation
{
    /// <summary>Scene-owned passive facts bridge, registered only on the production runner.</summary>
    [DisallowMultipleComponent]
    public sealed class VNYarnInformationCommands : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNInformationOverlay overlay;
        [SerializeField] private VNSaveLoadController saveLoad;
        private bool registered;
        private long generation;

        private void OnEnable()
        {
            if (registered) return;
            if (dialogueRunner == null || overlay == null || saveLoad == null)
            {
                Debug.LogError("VNYarnInformationCommands requires production runner, overlay and SaveLoad references.", this);
                return;
            }
            dialogueRunner.AddCommandHandler<string, string, string, string>("vn_info_show", TrackedShow);
            dialogueRunner.AddCommandHandler("vn_info_hide", Hide);
            saveLoad.LoadStateChanged += Loading;
            registered = true;
        }

        private void OnDisable()
        {
            generation++;
            if (registered)
            {
                if (dialogueRunner != null)
                {
                    dialogueRunner.RemoveCommandHandler("vn_info_show");
                    dialogueRunner.RemoveCommandHandler("vn_info_hide");
                }
                if (saveLoad != null) saveLoad.LoadStateChanged -= Loading;
                registered = false;
            }
            overlay?.Hide();
        }

        private void Loading(bool active) { if (active) generation++; }

        private IEnumerator TrackedShow(string title, string first, string second, string third)
            => VNAsyncCommandRegistry.Track(dialogueRunner, Show(title, first, second, third));

        private IEnumerator Show(string title, string first, string second, string third)
        {
            if (overlay == null || saveLoad == null || string.IsNullOrWhiteSpace(first))
            {
                Debug.LogError("vn_info_show requires an overlay, SaveLoad and a nonempty first fact.", this);
                yield break;
            }
            var current = generation;
            while (saveLoad.IsLoadInProgress)
            {
                if (!isActiveAndEnabled || current != generation || dialogueRunner == null || !dialogueRunner.IsDialogueRunning) yield break;
                yield return null;
            }
            if (!isActiveAndEnabled || current != generation) yield break;
            var facts = first;
            if (!string.IsNullOrEmpty(second)) facts += "\n" + second;
            if (!string.IsNullOrEmpty(third)) facts += "\n" + third;
            if (!overlay.Show(title, facts)) Debug.LogError("vn_info_show could not show its configured passive overlay.", this);
        }

        private void Hide() => overlay?.Hide();
    }
}
