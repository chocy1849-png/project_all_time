using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

#nullable enable

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Legacy serialized Yarn Typewriter Event Handler retained only for
    /// diagnostics. M6 full-display state is observed from LinePresenter TMP
    /// state by VNLineLifecyclePresenter and does not depend on this component.
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
