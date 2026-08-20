using System;
using UnityEngine;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Unity-authored mapping from a stable save checkpoint ID to a dedicated
    /// Yarn re-entry node. Definitions are owned by VNCheckpointCatalog.
    /// </summary>
    [Serializable]
    public sealed class VNCheckpointDefinition
    {
        [SerializeField] private string checkpointId;
        [SerializeField] private string resumeNode;
        [SerializeField] private string chapterId;
        [SerializeField] private string sceneTitle;

        public string CheckpointId => checkpointId;
        public string ResumeNode => resumeNode;
        public string ChapterId => chapterId;
        public string SceneTitle => sceneTitle;

        internal VNCheckpointContext ToContext() => new(checkpointId, resumeNode, chapterId, sceneTitle);
    }

    /// <summary>
    /// Immutable runtime copy of a validated checkpoint definition. It never
    /// exposes the mutable ScriptableObject-backed definition to consumers.
    /// </summary>
    public readonly struct VNCheckpointContext
    {
        public string CheckpointId { get; }
        public string ResumeNode { get; }
        public string ChapterId { get; }
        public string SceneTitle { get; }

        internal VNCheckpointContext(string checkpointId, string resumeNode, string chapterId, string sceneTitle)
        {
            CheckpointId = checkpointId;
            ResumeNode = resumeNode;
            ChapterId = chapterId;
            SceneTitle = sceneTitle;
        }
    }
}
