using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

#nullable enable

namespace ProjectAllTime.VN.Dialogue
{
    /// <summary>
    /// Resolves the one enabled LinePresenter explicitly owned by a DialogueRunner.
    /// M6 lifecycle and M7 settings application share this authority rule.
    /// </summary>
    public static class VNAuthoritativeLinePresenterResolver
    {
        public static bool TryResolve(DialogueRunner? dialogueRunner, out LinePresenter? linePresenter, out string diagnostic)
        {
            linePresenter = null;
            if (dialogueRunner == null)
            {
                diagnostic = "Cannot resolve the authoritative LinePresenter: LocalizedLine.Source is not a DialogueRunner.";
                return false;
            }

            var candidates = new List<LinePresenter>();
            foreach (var presenter in dialogueRunner.DialoguePresenters)
            {
                if (presenter is LinePresenter line && line.isActiveAndEnabled && !candidates.Contains(line))
                    candidates.Add(line);
            }

            if (candidates.Count != 1)
            {
                diagnostic = $"DialogueRunner {Describe(dialogueRunner)} has {candidates.Count} enabled distinct LinePresenters.";
                return false;
            }

            linePresenter = candidates[0];
            diagnostic = string.Empty;
            return true;
        }

        private static string Describe(Object value) => value == null ? "<null>" : $"{value.name}/{value.GetInstanceID()}";
    }
}
