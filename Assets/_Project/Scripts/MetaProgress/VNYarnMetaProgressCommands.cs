using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly HashSet<string> ownedCommandNames = new(StringComparer.Ordinal);
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

            try
            {
                // Yarn 3.2.7 reports a duplicate handler with Debug.LogError
                // rather than throwing. Detect collisions before any M8 handler
                // is added so registration is all-or-none.
                foreach (var commandName in CommandNames)
                {
                    if (IsCommandRegistered(commandName))
                        throw new InvalidOperationException("Yarn command '" + commandName + "' is already registered.");
                }

                Register("vn_unlock_cg", UnlockCG);
                Register("vn_unlock_chapter", UnlockChapter);
                Register("vn_unlock_archive", UnlockArchive);
                Register("vn_unlock_achievement", UnlockAchievement);
                Register("vn_complete_ending", CompleteEnding);
                handlersRegistered = true;
            }
            catch
            {
                UnregisterOwnedHandlers();
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            UnregisterOwnedHandlers();

            disposed = true;
        }

        private void Register(string commandName, Action<string> handler)
        {
            dialogueRunner.AddCommandHandler(commandName, handler);
            if (!IsCommandRegistered(commandName))
                throw new InvalidOperationException("Yarn command '" + commandName + "' could not be registered.");
            ownedCommandNames.Add(commandName);
        }

        private void UnregisterOwnedHandlers()
        {
            foreach (var commandName in ownedCommandNames) dialogueRunner.RemoveCommandHandler(commandName);
            ownedCommandNames.Clear();
            handlersRegistered = false;
        }

        private bool IsCommandRegistered(string commandName)
        {
            var dispatcherProperty = typeof(DialogueRunner).GetProperty("CommandDispatcher", BindingFlags.Instance | BindingFlags.NonPublic);
            if (dispatcherProperty == null) throw new InvalidOperationException("Yarn DialogueRunner command dispatcher is unavailable.");
            var dispatcher = dispatcherProperty.GetValue(dialogueRunner);
            var commandsProperty = dispatcher?.GetType().GetProperty("Commands", BindingFlags.Instance | BindingFlags.Public);
            if (commandsProperty?.GetValue(dispatcher) is not IEnumerable commands)
                throw new InvalidOperationException("Yarn DialogueRunner command registry is unavailable.");
            return commands.Cast<ICommand>().Any(command => string.Equals(command.Name, commandName, StringComparison.Ordinal));
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
