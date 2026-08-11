using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAllTime.VN.Presentation
{
    public enum VNCharacterFacing { Left, Right }

    [Serializable]
    public sealed class VNExpressionDefinition
    {
        [SerializeField] private string expressionId;
        [SerializeField] private Sprite headSprite;

        public string ExpressionId => expressionId;
        public Sprite HeadSprite => headSprite;
    }

    [CreateAssetMenu(menuName = "VN/Presentation/Character Definition", fileName = "VNCharacterDefinition")]
    public sealed class VNCharacterDefinition : ScriptableObject
    {
        [SerializeField] private string characterId;
        [SerializeField] private List<string> speakerAliases = new();
        [SerializeField] private VNCharacterFacing defaultFacing = VNCharacterFacing.Right;
        [SerializeField, Min(0.01f)] private float defaultScale = 1f;
        [SerializeField] private Sprite backHairSprite;
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private string defaultExpressionId;
        [SerializeField] private List<VNExpressionDefinition> expressions = new();

        public string CharacterId => characterId;
        public IReadOnlyList<string> SpeakerAliases => speakerAliases;
        public VNCharacterFacing DefaultFacing => defaultFacing;
        public float DefaultScale => defaultScale;
        public Sprite BackHairSprite => backHairSprite;
        public Sprite BodySprite => bodySprite;
        public string DefaultExpressionId => defaultExpressionId;

        public bool TryGetExpression(string expressionId, out VNExpressionDefinition expression)
        {
            expression = null;
            if (string.IsNullOrWhiteSpace(expressionId)) return false;
            foreach (var candidate in expressions)
            {
                if (candidate != null && candidate.ExpressionId == expressionId)
                {
                    expression = candidate;
                    return true;
                }
            }
            return false;
        }

        internal bool ValidateForRuntime()
        {
            var valid = VNPresentationValidation.IsStableId(characterId)
                && bodySprite != null
                && VNPresentationValidation.IsStableId(defaultExpressionId)
                && VNPresentationValidation.AreUnique(expressions, entry => entry == null ? null : entry.ExpressionId, "expression ID", this)
                && TryGetExpression(defaultExpressionId, out _);
            if (!valid) Debug.LogError($"VN Character Definition '{characterId}' is incomplete or invalid.", this);
            return valid;
        }

        private void OnValidate()
        {
            VNPresentationValidation.LogInvalidId(nameof(characterId), characterId, this);
            VNPresentationValidation.LogDuplicates(speakerAliases, "speaker alias", this);
            VNPresentationValidation.LogInvalidId("default expression ID", defaultExpressionId, this);
            VNPresentationValidation.LogDuplicates(expressions, entry => entry == null ? null : entry.ExpressionId, "expression ID", this);
        }
    }

    internal static class VNPresentationValidation
    {
        public static bool IsStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || !char.IsLower(value[0])) return false;
            foreach (var character in value)
                if (!(char.IsLower(character) || char.IsDigit(character) || character == '_')) return false;
            return true;
        }

        public static void LogInvalidId(string label, string value, UnityEngine.Object context)
        {
            if (!string.IsNullOrEmpty(value) && !IsStableId(value))
                Debug.LogError($"VN Presentation {label} '{value}' must use lowercase snake_case.", context);
        }

        public static void LogDuplicates<T>(IEnumerable<T> values, string label, UnityEngine.Object context)
            => LogDuplicates(values, value => Convert.ToString(value), label, context);

        public static void LogDuplicates<T>(IEnumerable<T> values, Func<T, string> getId, string label, UnityEngine.Object context)
        {
            var seen = new HashSet<string>();
            foreach (var value in values)
            {
                var id = getId(value);
                if (!string.IsNullOrEmpty(id) && !seen.Add(id))
                    Debug.LogError($"VN Presentation contains duplicate {label} '{id}'.", context);
            }
        }

        public static bool AreUnique<T>(IEnumerable<T> values, Func<T, string> getId, string label, UnityEngine.Object context)
        {
            var valid = true;
            var seen = new HashSet<string>();
            foreach (var value in values)
            {
                var id = getId(value);
                if (!IsStableId(id) || !seen.Add(id))
                {
                    Debug.LogError($"VN Presentation contains an invalid or duplicate {label} '{id}'.", context);
                    valid = false;
                }
            }
            return valid;
        }
    }
}
