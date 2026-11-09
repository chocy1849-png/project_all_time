using System;
using System.Collections.Generic;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Timeline;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Records
{
    public enum VNRecordsUnlockKind { Chapter, CG, Archive }

    [Serializable]
    public sealed class VNRecordsUnlockRule
    {
        [SerializeField] private string readLineId;
        [SerializeField] private VNRecordsUnlockKind kind;
        [SerializeField] private string targetId;
        [SerializeField] private string sourceScene;
        [SerializeField, TextArea] private string justification;
        public string ReadLineId => readLineId;
        public VNRecordsUnlockKind Kind => kind;
        public string TargetId => targetId;
        public string SourceScene => sourceScene;
        public string Justification => justification;
    }

    [CreateAssetMenu(menuName = "VN/Records/Unlock Mapping")]
    public sealed class VNRecordsUnlockMapping : ScriptableObject
    {
        [SerializeField] private List<VNRecordsUnlockRule> rules = new();
        public IReadOnlyList<VNRecordsUnlockRule> Rules => rules;
    }

    /// <summary>Projects committed read history into M8 unlocks; never observes pending presentation.</summary>
    public sealed class VNRecordsUnlockReconciler : IDisposable
    {
        private readonly VNRecordsUnlockMapping mapping;
        private readonly VNMetaProgressService progress;
        private bool initialized, reconciling;
        public string LastDiagnostic { get; private set; }

        public VNRecordsUnlockReconciler(VNRecordsUnlockMapping mapping, VNMetaProgressService progress)
        {
            this.mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public bool Initialize(YarnProject yarn, VNTimelineCatalog timeline, VNCGGalleryCatalog gallery, VNArchiveCatalog archive)
        {
            if (initialized) return string.IsNullOrEmpty(LastDiagnostic);
            if (yarn == null || timeline == null || gallery == null || archive == null || mapping.Rules == null)
                return Fail("Unlock mapping dependencies are missing.");
            var lines = new HashSet<string>(yarn.GetLineIDsForNodes(yarn.NodeNames), StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var rule in mapping.Rules)
            {
                if (rule == null || !lines.Contains(rule.ReadLineId) || !seen.Add(rule.ReadLineId + "|" + rule.Kind + "|" + rule.TargetId))
                    return Fail("Unlock mapping has an invalid or duplicate rule.");
                var valid = rule.Kind switch
                {
                    VNRecordsUnlockKind.Chapter => timeline.TryGetChapter(rule.TargetId, out _),
                    VNRecordsUnlockKind.CG => gallery.TryGetCG(rule.TargetId, out _),
                    VNRecordsUnlockKind.Archive => archive.TryGetEntry(rule.TargetId, out _),
                    _ => false
                };
                if (!valid) return Fail("Unlock mapping references an unknown target: " + rule.TargetId);
            }
            progress.ProgressChanged += OnProgressChanged;
            initialized = true;
            return Reconcile();
        }

        public bool Reconcile()
        {
            if (!initialized) return Fail("Unlock mapping is not initialized.");
            if (reconciling) return true;
            reconciling = true;
            try
            {
                foreach (var rule in mapping.Rules)
                {
                    if (!progress.IsLineRead(rule.ReadLineId)) continue;
                    var success = rule.Kind switch
                    {
                        VNRecordsUnlockKind.Chapter => progress.TryUnlockChapter(rule.TargetId),
                        VNRecordsUnlockKind.CG => progress.TryUnlockCG(rule.TargetId),
                        VNRecordsUnlockKind.Archive => progress.TryUnlockArchiveEntry(rule.TargetId),
                        _ => false
                    };
                    if (!success) return Fail(progress.LastDiagnostic ?? "Could not persist a Records unlock.");
                }
                LastDiagnostic = null;
                return true;
            }
            finally { reconciling = false; }
        }

        private void OnProgressChanged(VNMetaProgressChange change)
        {
            if (change.Kind == VNMetaProgressChangeKind.ReadLine && !Reconcile())
                Debug.LogError("Records unlock reconciliation failed: " + LastDiagnostic);
        }

        public void Dispose()
        {
            if (initialized) progress.ProgressChanged -= OnProgressChanged;
            initialized = false;
        }

        private bool Fail(string diagnostic) { LastDiagnostic = diagnostic; return false; }
    }
}
