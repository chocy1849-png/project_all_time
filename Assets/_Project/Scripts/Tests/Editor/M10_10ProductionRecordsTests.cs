using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Replay;
using ProjectAllTime.VN.Records.Timeline;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M10_10ProductionRecordsTests
    {
        private const string BasePath = "Assets/_Project/Settings/Records/Production/";
        private static T Asset<T>(string path) where T : UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(path);

        [Test]
        public void ProductionAssets_ValidateAndReplayUsesIsolatedProject()
        {
            var timeline = Asset<VNTimelineCatalog>(BasePath + "VNTimelineCatalog.asset");
            var gallery = Asset<VNCGGalleryCatalog>(BasePath + "VNCGGalleryCatalog.asset");
            var archive = Asset<VNArchiveCatalog>(BasePath + "VNArchiveCatalog.asset");
            var achievements = Asset<VNAchievementCatalog>(BasePath + "VNAchievementCatalog.asset");
            var presentation = Asset<VNPresentationCatalog>("Assets/_Project/Settings/Presentation/Production/M10_ProductionPresentationCatalog.asset");
            var production = Asset<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
            var replay = Asset<YarnProject>("Assets/_Project/RecordsReplay/Yarn/PrologueReplay.yarnproject");
            Assert.That(timeline.Entries.Count, Is.EqualTo(8));
            Assert.That(gallery.Entries.Count, Is.EqualTo(6));
            Assert.That(archive.Entries.Count, Is.EqualTo(5));
            Assert.That(achievements.Achievements.Count, Is.EqualTo(4));
            Assert.That(VNRecordsCatalogValidator.TryValidate(timeline, gallery, archive, achievements,
                new VNRecordsCatalogValidationContext(production, presentation, replayYarnProject: replay), out var diagnostic), Is.True, diagnostic);
            Assert.That(VNProductionLineResume.TryResolve("line:prologue_s001_001", production, out _, out var graph, out diagnostic), Is.True, diagnostic);
            Assert.That(graph, Is.EqualTo("cd43ce5bca1f245f359e90f71297e64232261c5c4b4ee6163790fa1cd3bef0b9"));
            foreach (var entry in timeline.Entries.Where(e => !string.IsNullOrEmpty(e.ReplayNode)))
            {
                Assert.That(production.NodeNames, Does.Not.Contain(entry.ReplayNode));
                var result = VNReplayContentValidator.Validate(replay, entry.ReplayNode, new VNReplayContentPolicy(new[] { entry.ReplayNode }));
                Assert.That(result.IsValid, Is.True, result.Diagnostic);
                Assert.That(replay.Program.Nodes[entry.ReplayNode].Instructions.Any(i => i.StoreVariable != null), Is.False);
            }
            Assert.That(timeline.Entries.Count(e => !string.IsNullOrEmpty(e.ReplayNode)), Is.EqualTo(2));
        }

        [Test]
        public void ProductionMapping_ReconcilesOnlyEarnedReadsAndPersists()
        {
            var root = Path.Combine(Path.GetTempPath(), "ProjectAllTime_M1010_" + Guid.NewGuid().ToString("N"));
            try
            {
                var progress = new VNMetaProgressService(VNMetaProgressRepository.CreateForTesting(root));
                progress.Load();
                var mapping = Asset<VNRecordsUnlockMapping>(BasePath + "VNProductionRecordsUnlockMapping.asset");
                var yarn = Asset<YarnProject>("Assets/_Project/Yarn/GameNarrative.yarnproject");
                var timeline = Asset<VNTimelineCatalog>(BasePath + "VNTimelineCatalog.asset");
                var gallery = Asset<VNCGGalleryCatalog>(BasePath + "VNCGGalleryCatalog.asset");
                var archive = Asset<VNArchiveCatalog>(BasePath + "VNArchiveCatalog.asset");
                Assert.That(mapping.Rules.Count, Is.EqualTo(12));
                Assert.That(progress.TryRecordReadLine("line:prologue_s001_001"), Is.True);
                using var reconciler = new VNRecordsUnlockReconciler(mapping, progress);
                Assert.That(reconciler.Initialize(yarn, timeline, gallery, archive), Is.True, reconciler.LastDiagnostic);
                Assert.That(progress.IsChapterUnlocked("prologue_part_01"), Is.True);
                Assert.That(progress.Current.unlockedCGs, Is.Empty);
                Assert.That(progress.Current.unlockedArchiveEntries, Is.Empty);
                Assert.That(progress.TryRecordReadLine("line:prologue_s001_034"), Is.True);
                Assert.That(progress.IsCGUnlocked("cg_worn_child_shoe"), Is.True);
                Assert.That(progress.IsCGUnlocked("cg_beach_photo_child"), Is.False);
                Assert.That(reconciler.Reconcile(), Is.True);
                foreach (var rule in mapping.Rules)
                {
                    Assert.That(rule.SourceScene, Is.Not.Empty);
                    Assert.That(rule.Justification, Is.Not.Empty);
                    Assert.That(progress.TryRecordReadLine(rule.ReadLineId), Is.True, rule.ReadLineId);
                }
                Assert.That(progress.Current.unlockedCGs.Length, Is.EqualTo(6));
                Assert.That(progress.Current.unlockedArchiveEntries.Length, Is.EqualTo(5));
                var loaded = new VNMetaProgressService(VNMetaProgressRepository.CreateForTesting(root));
                loaded.Load();
                Assert.That(loaded.IsCGUnlocked("cg_worn_child_shoe"), Is.True);
                Assert.That(loaded.IsChapterUnlocked("prologue_part_01"), Is.True);
                Assert.That(loaded.Current.unlockedCGs.Length, Is.EqualTo(6));
                Assert.That(loaded.Current.unlockedArchiveEntries.Length, Is.EqualTo(5));
                Assert.That(loaded.Current.completedEndings, Is.Empty);
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        }
    }
}
