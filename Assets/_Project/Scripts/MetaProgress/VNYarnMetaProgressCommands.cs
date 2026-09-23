using System;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.MetaProgress
{
    /// <summary>
    /// Scene-independent owner for the five authored MetaProgress command
    /// handlers. A later runtime bootstrap creates and disposes this object.
    /// </summary>
    public sealed class VNYarnMetaProgressCommands : IDisposable
    {
        private static readonly string[] CommandNames =
        {
            "vn_unlock_cg",
            "vn_unlock_chapter",
            "vn_unlock_archive",
            "vn_unlock_achievement",
            "vn_complete_ending",
        };

        private readonly DialogueRunner dialogueRunner;
        private readonly VNMetaProgressService metaProgressService;
        private bool handlersRegistered;
        private bool disposed;

        public bool IsRegistered => handlersRegistered;

        public VNYarnMetaProgressCommands(DialogueRunner dialogueRunner, VNMetaProgressService metaProgressService)
        {
            this.dialogueRunner = dialogueRunner ?? throw new ArgumentNullException(nameof(dialogueRunner));
            this.metaProgressService = metaProgressService ?? throw new ArgumentNullException(nameof(metaProgressService));
        }

        public void Register()
        {
            if (disposed || handlersRegistered) return;

            dialogueRunner.AddCommandHandler<string>("vn_unlock_cg", UnlockCG);
            dialogueRunner.AddCommandHandler<string>("vn_unlock_chapter", UnlockChapter);
            dialogueRunner.AddCommandHandler<string>("vn_unlock_archive", UnlockArchive);
            dialogueRunner.AddCommandHandler<string>("vn_unlock_achievement", UnlockAchievement);
            dialogueRunner.AddCommandHandler<string>("vn_complete_ending", CompleteEnding);
            handlersRegistered = true;
        }

        public void Dispose()
        {
            if (disposed) return;
            if (handlersRegistered)
            {
                foreach (var commandName in CommandNames) dialogueRunner.RemoveCommandHandler(commandName);
                handlersRegistered = false;
            }

            disposed = true;
        }

        private void UnlockCG(string id) => TryMutate("vn_unlock_cg", "CG", id, metaProgressService.TryUnlockCG);
        private void UnlockChapter(string id) => TryMutate("vn_unlock_chapter", "chapter", id, metaProgressService.TryUnlockChapter);
        private void UnlockArchive(string id) => TryMutate("vn_unlock_archive", "archive entry", id, metaProgressService.TryUnlockArchiveEntry);
        private void UnlockAchievement(string id) => TryMutate("vn_unlock_achievement", "achievement", id, metaProgressService.TryUnlockAchievement);
        private void CompleteEnding(string id) => TryMutate("vn_complete_ending", "ending", id, metaProgressService.TryCompleteEnding);

        private void TryMutate(string commandName, string category, string id, Func<string, bool> mutation)
        {
            if (mutation(id)) return;
            Debug.LogError($"MetaProgress {category} command '{commandName}' rejected ID '{id ?? "<null>"}': " +
                           (metaProgressService.LastDiagnostic ?? "unknown persistence failure"));
        }
    }
}
