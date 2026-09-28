using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Yarn.Unity;

namespace ProjectAllTime.VN.Records.Replay
{
    public sealed class VNReplayContentValidationResult
    {
        private readonly ReadOnlyCollection<string> reachableNodes;

        public bool IsValid { get; }
        public string Diagnostic { get; }
        public IReadOnlyList<string> ReachableNodes => reachableNodes;

        internal VNReplayContentValidationResult(bool isValid, string diagnostic, IEnumerable<string> reachableNodes)
        {
            IsValid = isValid;
            Diagnostic = diagnostic;
            this.reachableNodes = Array.AsReadOnly(new List<string>(reachableNodes ?? Array.Empty<string>()).ToArray());
        }
    }

    /// <summary>Fail-closed validation of the compiled node closure available to a Replay session.</summary>
    public static class VNReplayContentValidator
    {
        public static VNReplayContentValidationResult Validate(
            YarnProject project,
            string rootNode,
            VNReplayContentPolicy policy)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal);
            if (project == null) return Invalid("Replay has no Yarn Project.", visited);
            if (project.Program == null) return Invalid("Replay Yarn Project has no compiled Program.", visited);
            if (policy == null) return Invalid("Replay has no content policy.", visited);
            if (string.IsNullOrWhiteSpace(rootNode)) return Invalid("Replay root node is empty.", visited);
            if (!policy.IsNodeApproved(rootNode))
                return Invalid($"Replay root '{rootNode}' is not explicitly approved.", visited);

            var pending = new Stack<string>();
            pending.Push(rootNode);

            while (pending.Count > 0)
            {
                var nodeName = pending.Pop();
                if (!visited.Add(nodeName)) continue;
                if (!policy.IsNodeApproved(nodeName))
                    return Invalid($"Reachable node '{nodeName}' is not explicitly approved.", visited);
                if (!project.Program.Nodes.TryGetValue(nodeName, out var node))
                    return Invalid($"Approved Replay node '{nodeName}' is missing from the compiled Program.", visited);

                foreach (var instruction in node.Instructions)
                {
                    var instructionType = instruction.InstructionTypeCase.ToString();
                    if (instructionType == "AddOption" || instructionType == "ShowOptions")
                        return Invalid($"Replay node '{nodeName}' contains interactive options.", visited);
                }

                for (var instructionIndex = 0; instructionIndex < node.Instructions.Count; instructionIndex++)
                {
                    var instruction = node.Instructions[instructionIndex];
                    switch (instruction.InstructionTypeCase.ToString())
                    {
                        case "RunLine":
                        case "PushString":
                        case "PushFloat":
                        case "PushBool":
                        case "PushVariable":
                        case "StoreVariable":
                        case "Pop":
                        case "Stop":
                        case "Return":
                            break;

                        case "RunCommand":
                            var commandText = instruction.RunCommand.CommandText;
                            if (!VNReplayContentPolicy.IsVisualCommandAllowed(commandText))
                                return Invalid($"Replay node '{nodeName}' contains forbidden command '{commandText}'.", visited);
                            break;

                        case "RunNode":
                            var targetNode = instruction.RunNode.NodeName;
                            if (!policy.IsNodeApproved(targetNode))
                                return Invalid($"Replay node '{nodeName}' reaches unapproved node '{targetNode}'.", visited);
                            pending.Push(targetNode);
                            break;

                        case "JumpTo":
                            var destination = instruction.JumpTo.Destination;
                            if (destination <= instructionIndex || destination > node.Instructions.Count)
                                return Invalid($"Replay node '{nodeName}' contains a backward or invalid control-flow jump.", visited);
                            break;

                        case "AddOption":
                        case "ShowOptions":
                            return Invalid($"Replay node '{nodeName}' contains interactive options.", visited);

                        default:
                            return Invalid(
                                $"Replay node '{nodeName}' contains unsupported instruction '{instruction.InstructionTypeCase}'.",
                                visited);
                    }
                }
            }

            return new VNReplayContentValidationResult(true, null, visited);
        }

        private static VNReplayContentValidationResult Invalid(string diagnostic, IEnumerable<string> reachableNodes)
        {
            return new VNReplayContentValidationResult(false, diagnostic, reachableNodes);
        }
    }
}
