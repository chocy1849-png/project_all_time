using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Presentation
{
    public sealed class VNSpeakerFocusPresenter : DialoguePresenterBase
    {
        [SerializeField] private VNPresentationController presentationController;
        public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            if (presentationController == null) Debug.LogError("VNSpeakerFocusPresenter requires a Presentation Controller reference.", this);
            else presentationController.FocusSpeaker(line.CharacterName);
            return YarnTask.CompletedTask;
        }
        public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
        public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;
    }
}
