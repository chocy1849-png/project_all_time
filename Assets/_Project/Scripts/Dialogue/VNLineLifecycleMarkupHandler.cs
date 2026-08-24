using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

#nullable enable

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Serialized Yarn Typewriter Event Handler for the authoritative active
    /// LinePresenter. Its completion callback is M6's primary full-display
    /// signal; the presenter retains TMP observation as a defensive watchdog.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VNLineLifecycleMarkupHandler : ActionMarkupHandler
    {
        [SerializeField] private VNLineLifecyclePresenter? lifecyclePresenter;
        [SerializeField] private bool enableM6TechnicalDiagnostics;

        private void Awake()
        {
            lifecyclePresenter ??= GetComponent<VNLineLifecyclePresenter>();
            VNConvenienceDiagnostics.SetEnabled(enableM6TechnicalDiagnostics);
        }

        public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text) { }

        public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
        {
            lifecyclePresenter?.HandleMarkupDisplayBegin();
        }

        public override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
        {
            return YarnTask.CompletedTask;
        }

        public override void OnLineDisplayComplete()
        {
            lifecyclePresenter?.HandleMarkupDisplayComplete();
        }

        public override void OnLineWillDismiss()
        {
            lifecyclePresenter?.HandleMarkupLineWillDismiss();
        }
    }
}
