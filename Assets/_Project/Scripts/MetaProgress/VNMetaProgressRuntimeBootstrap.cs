using System;
using System.Collections.Generic;
using ProjectAllTime.VN.Dialogue;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.MetaProgress
{
    /// <summary>
    /// Scene-session composition owner for MetaProgress. It prepares durable
    /// state before the default-order DialogueRunner Start auto-starts Yarn.
    /// </summary>
    [DefaultExecutionOrder(-2)]
    [DisallowMultipleComponent]
    public sealed class VNMetaProgressRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;

        private Func<VNMetaProgressRepository> repositoryFactory = () => new VNMetaProgressRepository();
        private VNMetaProgressRepository repository;
        private VNMetaProgressService metaProgressService;
        private VNPersistentReadHistoryBridge readHistoryBridge;
        private VNYarnMetaProgressCommands yarnCommands;
        private bool initializationAttempted;

        public bool IsInitialized { get; private set; }
        public VNMetaProgressService MetaProgressService => metaProgressService;
        public string LastDiagnostic { get; private set; }

        private void Start()
        {
            Initialize();
            if (!IsInitialized)
                Debug.LogError($"{nameof(VNMetaProgressRuntimeBootstrap)}: {LastDiagnostic}", this);
            else if (!string.IsNullOrEmpty(LastDiagnostic))
                Debug.LogWarning($"{nameof(VNMetaProgressRuntimeBootstrap)}: {LastDiagnostic}", this);
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (dialogueRunner == null)
            {
                diagnostic = "Dialogue Runner is required.";
                return false;
            }

            if (GetComponent<VNDialogueSessionState>() == null)
            {
                diagnostic = "A sibling VNDialogueSessionState is required on the same GameObject.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        private void Initialize()
        {
            if (initializationAttempted) return;
            initializationAttempted = true;

            if (!TryValidateWiring(out var wiringDiagnostic))
            {
                LastDiagnostic = wiringDiagnostic;
                return;
            }

            var diagnostics = new List<string>();
            VNPersistentReadHistoryBridge candidateBridge = null;
            VNYarnMetaProgressCommands candidateCommands = null;
            try
            {
                var candidateRepository = repositoryFactory();
                if (candidateRepository == null) throw new InvalidOperationException("MetaProgress repository factory returned null.");
                var candidateService = new VNMetaProgressService(candidateRepository);
                candidateService.Load();
                if (!string.IsNullOrEmpty(candidateService.LastDiagnostic))
                    diagnostics.Add("MetaProgress load: " + candidateService.LastDiagnostic);

                candidateBridge = new VNPersistentReadHistoryBridge(GetComponent<VNDialogueSessionState>(), candidateService);
                if (!candidateBridge.Initialize())
                    throw new InvalidOperationException(candidateBridge.LastDiagnostic ?? "Persistent read-history bridge initialization failed.");

                candidateCommands = new VNYarnMetaProgressCommands(dialogueRunner, candidateService);
                candidateCommands.Register();

                repository = candidateRepository;
                metaProgressService = candidateService;
                readHistoryBridge = candidateBridge;
                yarnCommands = candidateCommands;
                IsInitialized = true;
            }
            catch (Exception exception)
            {
                candidateCommands?.Dispose();
                candidateBridge?.Dispose();
                ClearRuntimeOwnership();
                diagnostics.Add("MetaProgress initialization failed: " + exception.Message);
            }

            LastDiagnostic = diagnostics.Count == 0 ? null : string.Join("\n", diagnostics);
        }

        private void OnDestroy()
        {
            yarnCommands?.Dispose();
            readHistoryBridge?.Dispose();
            ClearRuntimeOwnership();
        }

        private void ClearRuntimeOwnership()
        {
            yarnCommands = null;
            readHistoryBridge = null;
            metaProgressService = null;
            repository = null;
            IsInitialized = false;
        }
    }
}
