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
            else presentationController.FocusSpeaker(ShouldFocusSpeaker(line.CharacterName) ? line.CharacterName : string.Empty);
            return YarnTask.CompletedTask;
        }
        public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
        public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;

        /// <summary>Thought/off-screen identity stays in the name label and history, without physical focus.</summary>
        public static bool ShouldFocusSpeaker(string label) => !string.IsNullOrWhiteSpace(label) &&
            !label.EndsWith("·독백", System.StringComparison.Ordinal) &&
            !label.EndsWith("·내부", System.StringComparison.Ordinal) &&
            !label.EndsWith("·화면 밖", System.StringComparison.Ordinal);
    }
}
