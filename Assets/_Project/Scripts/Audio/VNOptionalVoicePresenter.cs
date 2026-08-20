using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Audio
{
    /// <summary>
    /// Makes Yarn voice-over optional per localized line while retaining the
    /// Yarn Spinner VoiceOverPresenter playback implementation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VNOptionalVoicePresenter : DialoguePresenterBase
    {
        [SerializeField] private VoiceOverPresenter voiceOverPresenter;

        private long voiceOccurrence;
        private string currentVoiceLineId = string.Empty;
        private bool currentLineHasVoice;
        private bool isCurrentVoiceComplete = true;

        public string CurrentVoiceLineId => currentVoiceLineId;
        public bool CurrentLineHasVoice => currentLineHasVoice;
        public bool IsCurrentVoiceComplete => isCurrentVoiceComplete;
        public bool IsCurrentVoicePendingOrPlaying => currentLineHasVoice && !isCurrentVoiceComplete;

        public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            var occurrence = BeginVoicePresentation(line);
            if (line.Asset == null) return YarnTask.CompletedTask;

            if (!HasUsableVoiceAsset(line.Asset))
            {
                Debug.LogError(
                    $"{nameof(VNOptionalVoicePresenter)} cannot play voice for line {line.TextID}: " +
                    $"the associated asset is {line.Asset.GetType().Name}, not an {nameof(AudioClip)}.",
                    this);
                return YarnTask.CompletedTask;
            }

            if (voiceOverPresenter == null)
            {
                Debug.LogError(
                    $"{nameof(VNOptionalVoicePresenter)} requires a {nameof(VoiceOverPresenter)} reference " +
                    $"to play voice for line {line.TextID}.",
                    this);
                return YarnTask.CompletedTask;
            }

            SetVoicePending(occurrence);
            return ObserveVoiceCompletionAsync(line, token, occurrence);
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            ResetVoicePresentation();
            return voiceOverPresenter != null
                ? voiceOverPresenter.OnDialogueStartedAsync()
                : YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            ResetVoicePresentation();
            return voiceOverPresenter != null
                ? voiceOverPresenter.OnDialogueCompleteAsync()
                : YarnTask.CompletedTask;
        }

        private long BeginVoicePresentation(LocalizedLine line)
        {
            voiceOccurrence++;
            currentVoiceLineId = line?.TextID ?? string.Empty;
            currentLineHasVoice = false;
            isCurrentVoiceComplete = true;
            return voiceOccurrence;
        }

        private async YarnTask ObserveVoiceCompletionAsync(LocalizedLine line, LineCancellationToken token, long occurrence)
        {
            await voiceOverPresenter.RunLineAsync(line, token);
            CompleteVoicePresentation(occurrence);
        }

        private void SetVoicePending(long occurrence)
        {
            if (occurrence != voiceOccurrence) return;
            currentLineHasVoice = true;
            isCurrentVoiceComplete = false;
        }

        private void CompleteVoicePresentation(long occurrence)
        {
            if (occurrence == voiceOccurrence) isCurrentVoiceComplete = true;
        }

        private void ResetVoicePresentation()
        {
            voiceOccurrence++;
            currentVoiceLineId = string.Empty;
            currentLineHasVoice = false;
            isCurrentVoiceComplete = true;
        }

        private static bool HasUsableVoiceAsset(Object asset)
        {
            return asset is AudioClip ||
                   asset is IAssetProvider provider && provider.TryGetAsset(out AudioClip _);
        }
    }
}
