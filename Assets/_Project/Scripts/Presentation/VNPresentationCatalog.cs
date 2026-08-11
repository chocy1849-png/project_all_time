using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAllTime.VN.Presentation
{
    [Serializable]
    public sealed class VNSpriteCatalogEntry
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite sprite;
        public string Id => id;
        public Sprite Sprite => sprite;
    }

    [CreateAssetMenu(menuName = "VN/Presentation/Presentation Catalog", fileName = "VNPresentationCatalog")]
    public sealed class VNPresentationCatalog : ScriptableObject
    {
        [SerializeField] private List<VNCharacterDefinition> characterDefinitions = new();
        [SerializeField] private List<VNSpriteCatalogEntry> backgrounds = new();
        [SerializeField] private List<VNSpriteCatalogEntry> cgs = new();
        private readonly Dictionary<string, VNCharacterDefinition> charactersById = new();
        private readonly Dictionary<string, string> charactersBySpeakerAlias = new();
        private readonly Dictionary<string, Sprite> backgroundsById = new();
        private readonly Dictionary<string, Sprite> cgsById = new();
        private bool indexesBuilt;
        private bool indexesValid;

        public bool TryGetCharacter(string characterId, out VNCharacterDefinition character)
        {
            character = null;
            BuildIndexes();
            return indexesValid && !string.IsNullOrEmpty(characterId) && charactersById.TryGetValue(characterId, out character);
        }
        public bool TryResolveSpeakerAlias(string speakerAlias, out string characterId)
        {
            characterId = null;
            BuildIndexes();
            return indexesValid && !string.IsNullOrEmpty(speakerAlias) && charactersBySpeakerAlias.TryGetValue(speakerAlias, out characterId);
        }
        public bool TryGetBackground(string backgroundId, out Sprite sprite)
        {
            sprite = null;
            BuildIndexes();
            return indexesValid && !string.IsNullOrEmpty(backgroundId) && backgroundsById.TryGetValue(backgroundId, out sprite) && sprite != null;
        }
        public bool TryGetCG(string cgId, out Sprite sprite)
        {
            sprite = null;
            BuildIndexes();
            return indexesValid && !string.IsNullOrEmpty(cgId) && cgsById.TryGetValue(cgId, out sprite) && sprite != null;
        }

        private void OnValidate() { indexesBuilt = false; ValidateEntries(); }
        private void BuildIndexes()
        {
            if (indexesBuilt) return;
            indexesBuilt = true; indexesValid = true;
            charactersById.Clear(); charactersBySpeakerAlias.Clear(); backgroundsById.Clear(); cgsById.Clear();
            foreach (var character in characterDefinitions)
            {
                if (character == null || !character.ValidateForRuntime() || !Add(charactersById, character.CharacterId, character, "character ID")) { indexesValid = false; continue; }
                foreach (var alias in character.SpeakerAliases) AddAlias(alias, character.CharacterId);
            }
            AddSprites(backgrounds, backgroundsById, "background ID");
            AddSprites(cgs, cgsById, "CG ID");
        }
        private void AddSprites(IEnumerable<VNSpriteCatalogEntry> entries, Dictionary<string, Sprite> index, string label)
        {
            foreach (var entry in entries)
                if (entry == null || entry.Sprite == null || !Add(index, entry.Id, entry.Sprite, label)) continue;
        }
        private bool Add<T>(Dictionary<string, T> index, string id, T value, string label)
        {
            if (!VNPresentationValidation.IsStableId(id)) { Debug.LogError($"VN Presentation {label} '{id}' must use lowercase snake_case.", this); indexesValid = false; return false; }
            if (!index.TryAdd(id, value)) { Debug.LogError($"VN Presentation Catalog contains duplicate {label} '{id}'.", this); indexesValid = false; return false; }
            return true;
        }
        private void AddAlias(string alias, string characterId)
        {
            if (string.IsNullOrWhiteSpace(alias) || !charactersBySpeakerAlias.TryAdd(alias, characterId))
            {
                Debug.LogError($"VN Presentation Catalog contains an empty or duplicate speaker alias '{alias}'.", this);
                indexesValid = false;
            }
        }
        private void ValidateEntries()
        {
            VNPresentationValidation.LogDuplicates(characterDefinitions, character => character == null ? null : character.CharacterId, "character ID", this);
            VNPresentationValidation.LogDuplicates(backgrounds, entry => entry == null ? null : entry.Id, "background ID", this);
            VNPresentationValidation.LogDuplicates(cgs, entry => entry == null ? null : entry.Id, "CG ID", this);
        }
    }
}
