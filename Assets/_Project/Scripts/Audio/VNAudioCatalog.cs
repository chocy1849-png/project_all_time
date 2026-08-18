using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAllTime.VN.Audio
{
    [Serializable]
    public sealed class VNBgmCatalogEntry
    {
        [SerializeField] private string id;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 1f;
        [SerializeField] private bool loop = true;

        public string Id => id;
        public AudioClip Clip => clip;
        public float DefaultVolume => defaultVolume;
        public bool Loop => loop;
    }

    [Serializable]
    public sealed class VNSfxCatalogEntry
    {
        [SerializeField] private string id;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 1f;

        public string Id => id;
        public AudioClip Clip => clip;
        public float DefaultVolume => defaultVolume;
    }

    [CreateAssetMenu(menuName = "VN/Audio/Audio Catalog", fileName = "VNAudioCatalog")]
    public sealed class VNAudioCatalog : ScriptableObject
    {
        [SerializeField] private List<VNBgmCatalogEntry> bgm = new();
        [SerializeField] private List<VNSfxCatalogEntry> sfx = new();

        private readonly Dictionary<string, VNBgmCatalogEntry> bgmById = new();
        private readonly Dictionary<string, VNSfxCatalogEntry> sfxById = new();
        private bool indexesBuilt;
        private bool indexesValid;

        public bool TryGetBgm(string id, out VNBgmCatalogEntry entry)
        {
            entry = null;
            BuildIndexes();
            return indexesValid && !string.IsNullOrEmpty(id) && bgmById.TryGetValue(id, out entry);
        }

        public bool TryGetSfx(string id, out VNSfxCatalogEntry entry)
        {
            entry = null;
            BuildIndexes();
            return indexesValid && !string.IsNullOrEmpty(id) && sfxById.TryGetValue(id, out entry);
        }

        private void OnValidate()
        {
            indexesBuilt = false;
            ValidateEntries();
        }

        private void BuildIndexes()
        {
            if (indexesBuilt) return;

            indexesBuilt = true;
            indexesValid = true;
            bgmById.Clear();
            sfxById.Clear();

            AddEntries(bgm, bgmById, "BGM", entry => entry == null ? null : entry.Id, entry => entry != null && entry.Clip != null);
            AddEntries(sfx, sfxById, "SFX", entry => entry == null ? null : entry.Id, entry => entry != null && entry.Clip != null);
        }

        private void AddEntries<T>(IEnumerable<T> entries, Dictionary<string, T> index, string label, Func<T, string> getId, Func<T, bool> isConfigured)
        {
            foreach (var entry in entries)
            {
                var id = getId(entry);
                if (!VNAudioValidation.IsStableId(id) || !isConfigured(entry))
                {
                    Debug.LogError($"VN Audio Catalog has an incomplete or invalid {label} entry '{id}'.", this);
                    indexesValid = false;
                    continue;
                }

                if (!index.TryAdd(id, entry))
                {
                    Debug.LogError($"VN Audio Catalog contains duplicate {label} ID '{id}'.", this);
                    indexesValid = false;
                }
            }
        }

        private void ValidateEntries()
        {
            VNAudioValidation.LogInvalidOrDuplicateEntries(bgm, "BGM", entry => entry == null ? null : entry.Id, this);
            VNAudioValidation.LogInvalidOrDuplicateEntries(sfx, "SFX", entry => entry == null ? null : entry.Id, this);
        }
    }

    internal static class VNAudioValidation
    {
        public static bool IsStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || !char.IsLower(value[0])) return false;
            foreach (var character in value)
                if (!(char.IsLower(character) || char.IsDigit(character) || character == '_')) return false;
            return true;
        }

        public static void LogInvalidOrDuplicateEntries<T>(IEnumerable<T> entries, string label, Func<T, string> getId, UnityEngine.Object context)
        {
            var seen = new HashSet<string>();
            foreach (var entry in entries)
            {
                var id = getId(entry);
                if (!string.IsNullOrEmpty(id) && !IsStableId(id))
                    Debug.LogError($"VN Audio {label} ID '{id}' must use lowercase snake_case.", context);
                else if (!string.IsNullOrEmpty(id) && !seen.Add(id))
                    Debug.LogError($"VN Audio Catalog contains duplicate {label} ID '{id}'.", context);
            }
        }
    }
}
