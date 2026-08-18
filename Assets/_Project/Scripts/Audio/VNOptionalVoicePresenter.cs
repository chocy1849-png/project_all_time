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

        public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
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

            return voiceOverPresenter.RunLineAsync(line, token);
        }

        public override YarnTask OnDialogueStartedAsync() =>
            voiceOverPresenter != null
                ? voiceOverPresenter.OnDialogueStartedAsync()
                : YarnTask.CompletedTask;

        public override YarnTask OnDialogueCompleteAsync() =>
            voiceOverPresenter != null
                ? voiceOverPresenter.OnDialogueCompleteAsync()
                : YarnTask.CompletedTask;

        private static bool HasUsableVoiceAsset(Object asset)
        {
            return asset is AudioClip ||
                   asset is IAssetProvider provider && provider.TryGetAsset(out AudioClip _);
        }
    }
}
