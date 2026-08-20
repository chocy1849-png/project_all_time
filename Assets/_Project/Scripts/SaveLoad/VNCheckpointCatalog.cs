using System;
using System.Collections.Generic;
using Yarn.Unity;
using UnityEngine;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Project-authored checkpoint authority. A catalog is valid only when all
    /// of its definitions are complete, unique, and resolve to supplied Yarn
    /// Project nodes.
    /// </summary>
    [CreateAssetMenu(menuName = "VN/Save Load/Checkpoint Catalog", fileName = "VNCheckpointCatalog")]
    public sealed class VNCheckpointCatalog : ScriptableObject
    {
        [SerializeField] private List<VNCheckpointDefinition> checkpointDefinitions = new();

        public bool TryResolve(string checkpointId, out VNCheckpointContext context, out string diagnostic)
        {
            context = default;
            if (!TryBuildIndex(out var definitionsById, out diagnostic)) return false;

            if (string.IsNullOrWhiteSpace(checkpointId) || !definitionsById.TryGetValue(checkpointId, out context))
            {
                diagnostic = $"Checkpoint ID '{checkpointId}' is not defined by the checkpoint catalog.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        /// <summary>
        /// Validates every checkpoint definition against the supplied Yarn
        /// Project; a single stale re-entry node invalidates the catalog.
        /// </summary>
        public bool TryValidate(YarnProject yarnProject, out string diagnostic)
        {
            if (!TryBuildIndex(out var definitionsById, out diagnostic)) return false;
            if (!TryGetNodeNames(yarnProject, out var nodeNames, out diagnostic)) return false;

            foreach (var pair in definitionsById)
            {
                if (!nodeNames.Contains(pair.Value.ResumeNode))
                {
                    diagnostic = $"Checkpoint '{pair.Key}' references missing Yarn node '{pair.Value.ResumeNode}'.";
                    return false;
                }
            }

            diagnostic = null;
            return true;
        }

        public bool TryValidateContextResumeNode(VNCheckpointContext context, YarnProject yarnProject, out string diagnostic)
        {
            if (!IsDefinitionValid(context, out diagnostic)) return false;
            if (!TryGetNodeNames(yarnProject, out var nodeNames, out diagnostic)) return false;
            if (!nodeNames.Contains(context.ResumeNode))
            {
                diagnostic = $"Checkpoint '{context.CheckpointId}' references missing Yarn node '{context.ResumeNode}'.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        private bool TryBuildIndex(out Dictionary<string, VNCheckpointContext> definitionsById, out string diagnostic)
        {
            definitionsById = new Dictionary<string, VNCheckpointContext>(StringComparer.Ordinal);
            if (checkpointDefinitions == null)
            {
                diagnostic = "Checkpoint catalog definitions are missing.";
                return false;
            }

            foreach (var definition in checkpointDefinitions)
            {
                if (definition == null)
                {
                    diagnostic = "Checkpoint catalog contains an empty definition.";
                    return false;
                }

                var context = definition.ToContext();
                if (!IsDefinitionValid(context, out diagnostic)) return false;
                if (!definitionsById.TryAdd(context.CheckpointId, context))
                {
                    diagnostic = $"Checkpoint catalog contains duplicate checkpoint ID '{context.CheckpointId}'.";
                    return false;
                }
            }

            diagnostic = null;
            return true;
        }

        private static bool IsDefinitionValid(VNCheckpointContext context, out string diagnostic)
        {
            if (!VNCheckpointValidation.IsStableId(context.CheckpointId))
            {
                diagnostic = "Checkpoint IDs must use lowercase snake_case.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(context.ResumeNode))
            {
                diagnostic = $"Checkpoint '{context.CheckpointId}' requires an exact resume node.";
                return false;
            }

            if (!VNCheckpointValidation.IsStableId(context.ChapterId))
            {
                diagnostic = $"Checkpoint '{context.CheckpointId}' requires a lowercase snake_case chapter ID.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(context.SceneTitle))
            {
                diagnostic = $"Checkpoint '{context.CheckpointId}' requires a scene title.";
                return false;
            }

            diagnostic = null;
            return true;
        }

        private static bool TryGetNodeNames(YarnProject yarnProject, out HashSet<string> nodeNames, out string diagnostic)
        {
            nodeNames = null;
            if (yarnProject == null)
            {
                diagnostic = "A Yarn Project is required to validate checkpoint resume nodes.";
                return false;
            }

            try
            {
                nodeNames = new HashSet<string>(yarnProject.NodeNames ?? Array.Empty<string>(), StringComparer.Ordinal);
                diagnostic = null;
                return true;
            }
            catch (Exception)
            {
                diagnostic = "The assigned Yarn Project could not provide its node list.";
                return false;
            }
        }
    }

    internal static class VNCheckpointValidation
    {
        public static bool IsStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || value[0] < 'a' || value[0] > 'z') return false;
            foreach (var character in value)
            {
                var isLowerLetter = character >= 'a' && character <= 'z';
                var isDigit = character >= '0' && character <= '9';
                if (!isLowerLetter && !isDigit && character != '_') return false;
            }

            return true;
        }
    }
}
