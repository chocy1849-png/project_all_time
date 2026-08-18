using System.Collections;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Audio
{
    public sealed class VNYarnAudioCommands : MonoBehaviour
    {
        private static readonly string[] CommandNames = { "bgm_play", "bgm_crossfade", "bgm_pause", "bgm_resume", "bgm_stop", "sfx_play" };

        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNAudioController audioController;
        private bool handlersRegistered;

        private void OnEnable()
        {
            UnregisterHandlers();
            RegisterHandlers();
        }

        private void OnDisable() => UnregisterHandlers();

        private void RegisterHandlers()
        {
            if (dialogueRunner == null || audioController == null)
            {
                Debug.LogError("VNYarnAudioCommands requires Dialogue Runner and Audio Controller references.", this);
                return;
            }

            dialogueRunner.AddCommandHandler<string>("bgm_play", PlayBgm);
            dialogueRunner.AddCommandHandler<string, float>("bgm_crossfade", CrossfadeBgm);
            dialogueRunner.AddCommandHandler<float>("bgm_pause", PauseBgm);
            dialogueRunner.AddCommandHandler<float>("bgm_resume", ResumeBgm);
            dialogueRunner.AddCommandHandler<float>("bgm_stop", StopBgm);
            dialogueRunner.AddCommandHandler<string>("sfx_play", PlaySfx);
            handlersRegistered = true;
        }

        private void UnregisterHandlers()
        {
            if (!handlersRegistered || dialogueRunner == null) return;
            foreach (var commandName in CommandNames) dialogueRunner.RemoveCommandHandler(commandName);
            handlersRegistered = false;
        }

        private IEnumerator CrossfadeBgm(string bgmId, float duration) => audioController.CrossfadeBgm(bgmId, duration);
        private IEnumerator PauseBgm(float duration) => audioController.PauseBgm(duration);
        private IEnumerator ResumeBgm(float duration) => audioController.ResumeBgm(duration);
        private IEnumerator StopBgm(float duration) => audioController.FadeStopBgm(duration);
        private void PlayBgm(string bgmId) => audioController.PlayBgm(bgmId);
        private void PlaySfx(string sfxId) => audioController.PlaySfx(sfxId);
    }
}
