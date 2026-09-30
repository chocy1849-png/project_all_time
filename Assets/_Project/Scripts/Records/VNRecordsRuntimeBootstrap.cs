using System;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Achievements.UI;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Archive.UI;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Gallery.UI;
using ProjectAllTime.VN.Records.Replay.UI;
using ProjectAllTime.VN.Records.Timeline;
using ProjectAllTime.VN.Records.Timeline.UI;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Records
{
    /// <summary>Composes Records from the M8 authority before default-order dialogue starts.</summary>
    [DefaultExecutionOrder(-1)]
    [DisallowMultipleComponent]
    public sealed class VNRecordsRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private VNMetaProgressRuntimeBootstrap metaProgressBootstrap;
        [SerializeField] private YarnProject yarnProject;
        [SerializeField] private VNPresentationCatalog presentationCatalog;
        [SerializeField] private VNTimelineCatalog timelineCatalog;
        [SerializeField] private VNCGGalleryCatalog galleryCatalog;
        [SerializeField] private VNArchiveCatalog archiveCatalog;
        [SerializeField] private VNAchievementCatalog achievementCatalog;
        [SerializeField] private VNTimelineView timelineView;
        [SerializeField] private VNCGGalleryView galleryView;
        [SerializeField] private VNArchiveView archiveView;
        [SerializeField] private VNAchievementsView achievementsView;
        [SerializeField] private VNReplayController replayController;

        private VNTimelineService timelineService;
        private VNTimelineRuntime timelineRuntime;
        private VNCGGalleryService galleryService;
        private VNArchiveService archiveService;
        private VNAchievementService achievementService;
        private VNAchievementEvaluator achievementEvaluator;
        private bool initializationAttempted;

        public bool IsInitialized { get; private set; }
        public string LastDiagnostic { get; private set; }

        private void Start()
        {
            Initialize();
            if (!IsInitialized)
                Debug.LogError($"{nameof(VNRecordsRuntimeBootstrap)}: {LastDiagnostic}", this);
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (metaProgressBootstrap == null) return Fail("MetaProgress bootstrap is required.", out diagnostic);
            if (yarnProject == null) return Fail("Yarn Project is required.", out diagnostic);
            if (presentationCatalog == null) return Fail("Presentation Catalog is required.", out diagnostic);
            if (timelineCatalog == null) return Fail("Timeline Catalog is required.", out diagnostic);
            if (galleryCatalog == null) return Fail("Gallery Catalog is required.", out diagnostic);
            if (archiveCatalog == null) return Fail("Archive Catalog is required.", out diagnostic);
            if (achievementCatalog == null) return Fail("Achievement Catalog is required.", out diagnostic);
            if (timelineView == null) return Fail("Timeline View is required.", out diagnostic);
            if (galleryView == null) return Fail("Gallery View is required.", out diagnostic);
            if (archiveView == null) return Fail("Archive View is required.", out diagnostic);
            if (achievementsView == null) return Fail("Achievements View is required.", out diagnostic);
            if (replayController == null) return Fail("Replay Controller is required.", out diagnostic);
            if (!timelineView.TryValidateWiring(out diagnostic)) return false;
            if (!galleryView.TryValidateWiring(out diagnostic)) return false;
            if (!archiveView.TryValidateWiring(out diagnostic)) return false;
            if (!achievementsView.TryValidateWiring(out diagnostic)) return false;
            if (!replayController.TryValidateWiring(out diagnostic)) return false;
            return TryValidateCatalogs(out diagnostic);
        }

        private bool TryValidateCatalogs(out string diagnostic) =>
            VNRecordsCatalogValidator.TryValidate(timelineCatalog, galleryCatalog, archiveCatalog,
                achievementCatalog, new VNRecordsCatalogValidationContext(yarnProject, presentationCatalog), out diagnostic);

        // Start owns the single attempt; callers cannot initialize from Awake before M8 loads.
        private void Initialize()
        {
            if (initializationAttempted) return;
            initializationAttempted = true;
            if (!TryValidateWiring(out var diagnostic)) { LastDiagnostic = diagnostic; return; }
            if (!metaProgressBootstrap.IsInitialized)
            { LastDiagnostic = "MetaProgress bootstrap must be initialized before Records."; return; }
            var progress = metaProgressBootstrap.MetaProgressService;
            if (progress == null)
            { LastDiagnostic = "Initialized MetaProgress bootstrap must provide its MetaProgressService."; return; }
            if (!TryValidateCatalogs(out diagnostic)) { LastDiagnostic = diagnostic; return; }

            VNAchievementEvaluator candidateEvaluator = null;
            try
            {
                var candidateTimelineService = new VNTimelineService(timelineCatalog, progress);
                var candidateTimelineRuntime = new VNTimelineRuntime(timelineCatalog, candidateTimelineService, progress);
                var candidateGalleryService = new VNCGGalleryService(galleryCatalog, presentationCatalog, progress);
                var candidateArchiveService = new VNArchiveService(archiveCatalog, progress);
                var candidateAchievementService = new VNAchievementService(achievementCatalog, progress);
                candidateEvaluator = new VNAchievementEvaluator(achievementCatalog, galleryCatalog, archiveCatalog, progress);
                if (!candidateEvaluator.Initialize())
                {
                    LastDiagnostic = candidateEvaluator.LastDiagnostic ?? "Achievement evaluator initialization failed.";
                    candidateEvaluator.Dispose();
                    return;
                }

                Require(timelineView.Initialize(candidateTimelineRuntime), "Timeline View", timelineView.LastDiagnostic);
                Require(galleryView.Initialize(candidateGalleryService), "Gallery View", galleryView.LastDiagnostic);
                Require(archiveView.Initialize(candidateArchiveService), "Archive View", archiveView.LastDiagnostic);
                Require(achievementsView.Initialize(candidateAchievementService), "Achievements View", achievementsView.LastDiagnostic);
                Require(replayController.Initialize(candidateTimelineRuntime, timelineCatalog, yarnProject),
                    "Replay Controller", replayController.LastDiagnostic);
                Require(timelineView.InitializeReplay(replayController), "Timeline Replay", timelineView.LastDiagnostic);

                timelineService = candidateTimelineService;
                timelineRuntime = candidateTimelineRuntime;
                galleryService = candidateGalleryService;
                archiveService = candidateArchiveService;
                achievementService = candidateAchievementService;
                achievementEvaluator = candidateEvaluator;
                IsInitialized = true;
                LastDiagnostic = null;
            }
            catch (Exception exception)
            {
                candidateEvaluator?.Dispose();
                ClearRuntimeOwnership();
                LastDiagnostic = "Records initialization failed: " + exception.Message;
            }
        }

        private static void Require(bool success, string owner, string diagnostic)
        {
            if (!success) throw new InvalidOperationException(owner + ": " + (diagnostic ?? "initialization failed."));
        }

        private void OnDestroy()
        {
            achievementEvaluator?.Dispose();
            ClearRuntimeOwnership();
        }

        private void ClearRuntimeOwnership()
        {
            achievementEvaluator = null;
            achievementService = null;
            archiveService = null;
            galleryService = null;
            timelineRuntime = null;
            timelineService = null;
            IsInitialized = false;
        }

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }
}
