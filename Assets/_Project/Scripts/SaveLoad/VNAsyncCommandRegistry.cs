using System;
using System.Collections;
using System.Collections.Generic;
using Yarn.Unity;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>Tracks project-owned Yarn coroutines until they have unwound after a stop.</summary>
    public static class VNAsyncCommandRegistry
    {
        private static readonly Dictionary<DialogueRunner, int> Active = new();

        public static bool IsIdle(DialogueRunner runner) => runner == null || !Active.TryGetValue(runner, out var count) || count == 0;

        public static IEnumerator Track(DialogueRunner runner, IEnumerator operation)
        {
            if (runner == null || operation == null) yield break;
            Active.TryGetValue(runner, out var count);
            Active[runner] = count + 1;
            var stack = new Stack<IEnumerator>();
            stack.Push(operation);
            try
            {
                while (stack.Count > 0 && runner != null && runner.IsDialogueRunning)
                {
                    var current = stack.Peek();
                    if (!current.MoveNext())
                    {
                        (stack.Pop() as IDisposable)?.Dispose();
                        continue;
                    }
                    if (current.Current is IEnumerator nested) stack.Push(nested);
                    else yield return current.Current;
                }
            }
            finally
            {
                while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
                if (!ReferenceEquals(runner, null) && Active.TryGetValue(runner, out var remaining))
                {
                    if (remaining <= 1) Active.Remove(runner);
                    else Active[runner] = remaining - 1;
                }
            }
        }
    }
}
