using System;
using System.Collections;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Presentation
{
    /// <summary>Yields only until a browser event, then lets the same runner present the authored thought.</summary>
    [DisallowMultipleComponent]
    public sealed class VNYarnProfileCommands : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private VNProfileBrowser browser;
        private int result = -1;
        private bool registered;

        private static VNProfileContent[] CreateProfiles() => new[]
        {
            new VNProfileContent("선주", "사진 동호회 회원.\n처음 보는 사람과도 잘 어울림.\n즉흥적인 제안이 많음.\n주요 과제: 동네 사진 행사 준비.", true),
            new VNProfileContent("해진", "소규모 공연 준비생.\n감정 표현이 솔직함.\n의견을 바로 주고받는 관계를 선호함.\n주요 과제: 공연 장소 확보와 준비.", true),
            new VNProfileContent("하나", "오월극장 영사 담당 직원.\n차분하고 실무적인 성향.\n빈말보다 구체적인 도움을 신뢰함.\n주요 과제: 폐업 전 마지막 상영 준비.", false),
        };

        private void OnEnable()
        {
            if (dialogueRunner == null || browser == null)
            {
                Debug.LogError("VNYarnProfileCommands requires the production runner and profile browser.", this);
                return;
            }
            dialogueRunner.AddCommandHandler("vn_profile_begin", Begin);
            dialogueRunner.AddCommandHandler("vn_profile_wait", TrackedWaitForEvent);
            dialogueRunner.AddCommandHandler<float>("vn_profile_review_complete", CompleteReview);
            dialogueRunner.AddCommandHandler("vn_profile_silent_complete", CompleteSilentReview);
            dialogueRunner.AddFunction("vn_profile_result", (Func<float>)(() => result));
            browser.ReviewRequested += OnReviewRequested;
            browser.SilentReviewRequested += OnSilentReviewRequested;
            browser.Completed += OnCompleted;
            registered = true;
        }

        private void OnDisable()
        {
            if (!registered) return;
            browser.ReviewRequested -= OnReviewRequested;
            browser.SilentReviewRequested -= OnSilentReviewRequested;
            browser.Completed -= OnCompleted;
            browser.CancelBrowse();
            dialogueRunner.RemoveCommandHandler("vn_profile_begin");
            dialogueRunner.RemoveCommandHandler("vn_profile_wait");
            dialogueRunner.RemoveCommandHandler("vn_profile_review_complete");
            dialogueRunner.RemoveCommandHandler("vn_profile_silent_complete");
            dialogueRunner.RemoveFunction("vn_profile_result");
            registered = false;
        }

        private void Update()
        {
            if (registered && browser.IsBrowsing && !dialogueRunner.IsDialogueRunning)
                browser.CancelBrowse();
        }

        private void Begin()
        {
            result = -1;
            if (!browser.BeginBrowse(CreateProfiles()))
                Debug.LogError("Could not begin the profile browser.", this);
        }

        private IEnumerator TrackedWaitForEvent() => VNAsyncCommandRegistry.Track(dialogueRunner, WaitForEvent());

        private IEnumerator WaitForEvent()
        {
            while (isActiveAndEnabled && dialogueRunner.IsDialogueRunning && browser.IsBrowsing && result < 0)
                yield return null;
        }

        private void CompleteReview(float index)
        {
            if (index != (int)index || !browser.CompleteReview((int)index))
                Debug.LogError("Profile review completion did not match the pending thought.", this);
            else result = -1;
        }

        private void CompleteSilentReview()
        {
            if (result == 2 && browser.IsBrowsing && browser.CurrentProfile == 2 && browser.HasReviewed(2)) result = -1;
            else Debug.LogError("Silent profile completion did not match Hana's review.", this);
        }

        private void OnReviewRequested(int index) => result = index;
        private void OnSilentReviewRequested(int index) { if (index == 2) result = 2; }
        private void OnCompleted() => result = 3;
    }
}
