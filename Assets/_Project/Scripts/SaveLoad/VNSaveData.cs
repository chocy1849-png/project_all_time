using System;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Plain, project-owned persistent-save data. This contract intentionally
    /// contains no Unity object references or runtime-system dependencies.
    /// </summary>
    [Serializable]
    public sealed class SaveSlotData
    {
        public int schemaVersion = VNSaveSerializer.CurrentSchemaVersion;
        public string slotType = string.Empty;
        public int slotIndex;
        public string checkpointId = string.Empty;
        public string resumeNode = string.Empty;
        public YarnVariablesData yarnVariables = new();
        public PresentationState presentationState = new();
        public AudioState audioState = new();
        public string chapterId = string.Empty;
        public string sceneTitle = string.Empty;
        public float playedSeconds;
        public string savedAtUtcIso8601 = string.Empty;
        public string thumbnailFileName = string.Empty;
    }

    [Serializable]
    public sealed class YarnVariablesData
    {
        public FloatVariableEntry[] floats = Array.Empty<FloatVariableEntry>();
        public StringVariableEntry[] strings = Array.Empty<StringVariableEntry>();
        public BoolVariableEntry[] bools = Array.Empty<BoolVariableEntry>();
    }

    [Serializable]
    public sealed class FloatVariableEntry
    {
        public string name = string.Empty;
        public float value;
    }

    [Serializable]
    public sealed class StringVariableEntry
    {
        public string name = string.Empty;
        public string value = string.Empty;
    }

    [Serializable]
    public sealed class BoolVariableEntry
    {
        public string name = string.Empty;
        public bool value;
    }

    [Serializable]
    public sealed class PresentationState
    {
        public string backgroundId = string.Empty;
        public string cgId = string.Empty;
        public CharacterSaveState[] characters = Array.Empty<CharacterSaveState>();
    }

    [Serializable]
    public sealed class CharacterSaveState
    {
        public string characterId = string.Empty;
        public string expressionId = string.Empty;
        public string slot = string.Empty;
        public string facing = string.Empty;
        public float scale = 1f;
    }

    [Serializable]
    public sealed class AudioState
    {
        public string bgmId = string.Empty;
        public float playbackSeconds;
    }
}
