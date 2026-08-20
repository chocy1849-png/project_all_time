using System;
using System.Collections;
using System.Collections.Generic;
using ProjectAllTime.VN.SaveLoad;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Presentation
{
    public enum VNCharacterSlot { FarLeft, Left, Center, Right, FarRight }

    public sealed class VNCharacterRuntimeState
    {
        public string CharacterId { get; internal set; }
        public string ExpressionId { get; internal set; }
        public VNCharacterSlot Slot { get; internal set; }
        public VNCharacterFacing Facing { get; internal set; }
        public float Scale { get; internal set; }
        public bool Visible { get; internal set; }
        public bool SpeakerActive { get; internal set; }
    }

    /// <summary>
    /// Fully validated, logical presentation input for immediate load restore.
    /// It deliberately holds no transient fade, speaker-focus, or hierarchy
    /// state.
    /// </summary>
    public sealed class VNPresentationRestorePlan
    {
        public string BackgroundId { get; }
        public string CGId { get; }
        internal IReadOnlyList<CharacterSaveState> Characters { get; }

        internal VNPresentationRestorePlan(string backgroundId, string cgId, List<CharacterSaveState> characters)
        {
            BackgroundId = backgroundId;
            CGId = cgId;
            Characters = characters;
        }
    }

    public sealed class VNPresentationController : MonoBehaviour
    {
        private static readonly Color ActiveColor = Color.white;
        private static readonly Color InactiveColor = new(0.65f, 0.65f, 0.65f, 1f);

        [SerializeField] private VNPresentationCatalog catalog;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image cgImage;
        [SerializeField] private List<VNCharacterSlotView> characterSlotViews = new();
        [SerializeField, Min(0f)] private float speakerHighlightDuration = 0.20f;

        private readonly Dictionary<VNCharacterSlot, VNCharacterSlotView> viewsBySlot = new();
        private readonly Dictionary<VNCharacterSlot, string> charactersBySlot = new();
        private readonly Dictionary<string, VNCharacterRuntimeState> visibleCharacters = new();
        private Coroutine speakerHighlightCoroutine;

        public IReadOnlyDictionary<string, VNCharacterRuntimeState> VisibleCharacters => visibleCharacters;
        public Image BackgroundImage => backgroundImage;
        public Image CGImage => cgImage;
        public string CurrentBackgroundId { get; private set; }
        public string CurrentCGId { get; private set; }

        private void Awake()
        {
            BuildSlotIndex();
            SetImageVisible(backgroundImage, backgroundImage != null && backgroundImage.sprite != null);
            SetImageVisible(cgImage, cgImage != null && cgImage.sprite != null);
            foreach (var view in viewsBySlot.Values) view.SetVisible(false);
        }

        public bool SetBackground(string backgroundId)
        {
            if (!TryGetBackgroundSprite(backgroundId, out var sprite) || backgroundImage == null) return Fail($"Cannot set unknown background '{backgroundId}'.");
            backgroundImage.sprite = sprite;
            SetImageVisible(backgroundImage, true);
            CurrentBackgroundId = backgroundId;
            return true;
        }

        public bool ClearBackground()
        {
            if (backgroundImage == null) return Fail("VN Presentation Controller has no Background Image reference.");
            backgroundImage.sprite = null;
            SetImageVisible(backgroundImage, false);
            CurrentBackgroundId = null;
            return true;
        }

        public bool SetCG(string cgId)
        {
            if (!TryGetCGSprite(cgId, out var sprite) || cgImage == null) return Fail($"Cannot set unknown CG '{cgId}'.");
            cgImage.sprite = sprite;
            SetImageVisible(cgImage, true);
            CurrentCGId = cgId;
            return true;
        }

        public void ClearCG()
        {
            if (cgImage == null) { Debug.LogError("VN Presentation Controller has no CG Image reference.", this); return; }
            cgImage.sprite = null;
            SetImageVisible(cgImage, false);
            CurrentCGId = null;
        }

        /// <summary>
        /// Captures only the authoritative M3 logical state. Characters are
        /// ordered by slot and then ID so SaveSlotData is deterministic.
        /// </summary>
        public bool TryCaptureStableState(out PresentationState presentationState, out string diagnostic)
        {
            presentationState = new PresentationState
            {
                backgroundId = CurrentBackgroundId ?? string.Empty,
                cgId = CurrentCGId ?? string.Empty,
                characters = new CharacterSaveState[visibleCharacters.Count],
            };

            var states = new List<VNCharacterRuntimeState>(visibleCharacters.Values);
            states.Sort(CompareRuntimeStates);
            for (var index = 0; index < states.Count; index++)
            {
                var state = states[index];
                presentationState.characters[index] = new CharacterSaveState
                {
                    characterId = state.CharacterId,
                    expressionId = state.ExpressionId,
                    slot = ToSerializedSlot(state.Slot),
                    facing = ToSerializedFacing(state.Facing),
                    scale = state.Scale,
                };
            }

            if (!TryPrepareRestore(presentationState, out _, out diagnostic))
            {
                presentationState = null;
                return false;
            }

            diagnostic = null;
            return true;
        }

        /// <summary>
        /// Validates saved logical IDs against the current M3 catalog and all
        /// configured slot views without changing presentation state.
        /// </summary>
        public bool TryPrepareRestore(PresentationState presentationState, out VNPresentationRestorePlan restorePlan, out string diagnostic)
        {
            restorePlan = null;
            if (presentationState == null || presentationState.backgroundId == null || presentationState.cgId == null || presentationState.characters == null)
            {
                diagnostic = "Saved presentation state is incomplete.";
                return false;
            }

            if (backgroundImage == null || cgImage == null)
            {
                diagnostic = "VN Presentation Controller requires Background and CG Image references for restore.";
                return false;
            }

            if (!string.IsNullOrEmpty(presentationState.backgroundId) && !TryGetBackgroundSprite(presentationState.backgroundId, out _))
            {
                diagnostic = $"Saved background '{presentationState.backgroundId}' is not available in the presentation catalog.";
                return false;
            }

            if (!string.IsNullOrEmpty(presentationState.cgId) && !TryGetCGSprite(presentationState.cgId, out _))
            {
                diagnostic = $"Saved CG '{presentationState.cgId}' is not available in the presentation catalog.";
                return false;
            }

            var characterIds = new HashSet<string>(StringComparer.Ordinal);
            var occupiedSlots = new HashSet<VNCharacterSlot>();
            var characters = new List<CharacterSaveState>(presentationState.characters.Length);
            foreach (var savedCharacter in presentationState.characters)
            {
                if (savedCharacter == null || string.IsNullOrWhiteSpace(savedCharacter.characterId) || string.IsNullOrWhiteSpace(savedCharacter.expressionId) ||
                    !TryParseSlot(savedCharacter.slot, out var slot) || !TryParseFacing(savedCharacter.facing, out var facing) || !IsPositiveFinite(savedCharacter.scale))
                {
                    diagnostic = "Saved character presentation state is invalid.";
                    return false;
                }

                if (!characterIds.Add(savedCharacter.characterId) || !occupiedSlots.Add(slot))
                {
                    diagnostic = "Saved visible character IDs and occupied slots must each be unique.";
                    return false;
                }

                if (catalog == null || !catalog.TryGetCharacter(savedCharacter.characterId, out var character) || !character.TryGetExpression(savedCharacter.expressionId, out _) || !HasConfiguredSlotView(slot))
                {
                    diagnostic = $"Saved character '{savedCharacter.characterId}' cannot be restored with its current definition, expression, or slot view.";
                    return false;
                }

                characters.Add(new CharacterSaveState
                {
                    characterId = savedCharacter.characterId,
                    expressionId = savedCharacter.expressionId,
                    slot = ToSerializedSlot(slot),
                    facing = ToSerializedFacing(facing),
                    scale = savedCharacter.scale,
                });
            }

            characters.Sort(CompareSavedCharacters);
            restorePlan = new VNPresentationRestorePlan(presentationState.backgroundId, presentationState.cgId, characters);
            diagnostic = null;
            return true;
        }

        /// <summary>
        /// Reconstructs an already validated stable presentation immediately.
        /// Callers normalize M4 transitions first; this method never replays a
        /// fade or crossfade.
        /// </summary>
        public bool RestorePreparedState(VNPresentationRestorePlan restorePlan, out string diagnostic)
        {
            if (restorePlan == null)
            {
                diagnostic = "A validated presentation restore plan is required.";
                return false;
            }

            ClearAllVisibleCharacters();
            if (string.IsNullOrEmpty(restorePlan.BackgroundId))
            {
                if (!ClearBackground()) { diagnostic = "Saved background could not be cleared."; return false; }
            }
            else if (!SetBackground(restorePlan.BackgroundId))
            {
                diagnostic = "Saved background could not be restored.";
                return false;
            }

            foreach (var savedCharacter in restorePlan.Characters)
            {
                if (!TryParseSlot(savedCharacter.slot, out var slot) || !TryParseFacing(savedCharacter.facing, out var facing) ||
                    !ShowCharacter(savedCharacter.characterId, savedCharacter.expressionId, slot) ||
                    !SetFacing(savedCharacter.characterId, facing) || !SetScale(savedCharacter.characterId, savedCharacter.scale))
                {
                    diagnostic = "Saved character presentation could not be restored.";
                    return false;
                }
            }

            if (string.IsNullOrEmpty(restorePlan.CGId)) ClearCG();
            else if (!SetCG(restorePlan.CGId))
            {
                diagnostic = "Saved CG could not be restored.";
                return false;
            }

            NormalizeStableCharacterVisuals();
            ResetSpeakerFocusToNeutral();
            diagnostic = null;
            return true;
        }

        /// <summary>
        /// Makes CanvasGroup visibility stable without changing M3's RGB
        /// speaker-focus mechanism.
        /// </summary>
        public void NormalizeStableCharacterVisuals()
        {
            foreach (var pair in viewsBySlot)
            {
                pair.Value.SetFadeAlpha(1f);
                pair.Value.SetVisible(charactersBySlot.ContainsKey(pair.Key));
            }
        }

        /// <summary>
        /// Clears persisted speaker state. The next Yarn line remains the sole
        /// authority for speaker focus after a load.
        /// </summary>
        public void ResetSpeakerFocusToNeutral()
        {
            if (speakerHighlightCoroutine != null) StopCoroutine(speakerHighlightCoroutine);
            speakerHighlightCoroutine = null;
            foreach (var state in visibleCharacters.Values)
            {
                state.SpeakerActive = true;
                if (viewsBySlot.TryGetValue(state.Slot, out var view)) view.SetTint(ActiveColor);
            }
        }

        public bool TryGetBackgroundSprite(string backgroundId, out Sprite sprite)
        {
            sprite = null;
            return catalog != null && catalog.TryGetBackground(backgroundId, out sprite);
        }

        public bool TryGetCGSprite(string cgId, out Sprite sprite)
        {
            sprite = null;
            return catalog != null && catalog.TryGetCG(cgId, out sprite);
        }

        public bool TryGetCharacterSlotView(VNCharacterSlot slot, out VNCharacterSlotView view) => TryGetSlotView(slot, out view);

        public bool TryGetVisibleCharacterSlotView(string characterId, out VNCharacterSlotView view)
        {
            view = null;
            return TryGetVisibleCharacter(characterId, out var state) && TryGetSlotView(state.Slot, out view);
        }

        public bool ShowCharacter(string characterId, string expressionId, VNCharacterSlot slot)
        {
            if (!TryResolveCharacter(characterId, expressionId, out var character, out var resolvedExpressionId, out var expression) || !TryGetSlotView(slot, out var view)) return false;
            if (charactersBySlot.TryGetValue(slot, out var occupant) && occupant != characterId) return Fail($"Cannot show '{characterId}' in occupied slot '{slot}' (occupied by '{occupant}').");
            if (visibleCharacters.TryGetValue(characterId, out var existing) && existing.Slot != slot) return Fail($"Character '{characterId}' is already visible in '{existing.Slot}'. Use vn_move instead.");

            var state = existing ?? new VNCharacterRuntimeState { CharacterId = characterId };
            state.ExpressionId = resolvedExpressionId;
            state.Slot = slot;
            state.Facing = existing == null ? character.DefaultFacing : existing.Facing;
            state.Scale = existing == null ? character.DefaultScale : existing.Scale;
            state.Visible = true;
            state.SpeakerActive = true;
            visibleCharacters[characterId] = state;
            charactersBySlot[slot] = characterId;
            view.ApplyCharacter(character, expression, state);
            SetAllVisibleCharactersActive();
            return true;
        }

        public bool SetExpression(string characterId, string expressionId)
        {
            if (!TryGetVisibleCharacter(characterId, out var state) || !TryResolveCharacter(characterId, expressionId, out var character, out var resolvedExpressionId, out var expression)) return false;
            state.ExpressionId = resolvedExpressionId;
            viewsBySlot[state.Slot].ApplyCharacter(character, expression, state);
            return true;
        }

        public bool MoveCharacter(string characterId, VNCharacterSlot destination)
        {
            if (!TryGetVisibleCharacter(characterId, out var state) || !TryGetSlotView(destination, out var destinationView) || !TryResolveCharacter(characterId, state.ExpressionId, out var character, out _, out var expression)) return false;
            if (charactersBySlot.TryGetValue(destination, out var occupant) && occupant != characterId) return Fail($"Cannot move '{characterId}' into occupied slot '{destination}' (occupied by '{occupant}').");
            if (state.Slot == destination) return true;

            var sourceSlot = state.Slot;
            state.Slot = destination;
            destinationView.ApplyCharacter(character, expression, state);
            viewsBySlot[sourceSlot].SetVisible(false);
            charactersBySlot.Remove(sourceSlot);
            charactersBySlot[destination] = characterId;
            return true;
        }

        public bool SetFacing(string characterId, VNCharacterFacing facing)
        {
            if (!TryGetVisibleCharacter(characterId, out var state)) return false;
            state.Facing = facing;
            viewsBySlot[state.Slot].ApplyTransform(state);
            return true;
        }

        public bool SetScale(string characterId, float scale)
        {
            if (scale <= 0f || !TryGetVisibleCharacter(characterId, out var state)) return Fail($"Cannot set invalid scale '{scale}' for '{characterId}'.");
            state.Scale = scale;
            viewsBySlot[state.Slot].ApplyTransform(state);
            return true;
        }

        public bool HideCharacter(string characterId)
        {
            if (!TryGetVisibleCharacter(characterId, out var state)) return false;
            viewsBySlot[state.Slot].SetVisible(false);
            charactersBySlot.Remove(state.Slot);
            visibleCharacters.Remove(characterId);
            SetAllVisibleCharactersActive();
            return true;
        }

        public void FocusSpeaker(string speakerAlias)
        {
            string characterId = null;
            var hasVisibleSpeaker = catalog != null && catalog.TryResolveSpeakerAlias(speakerAlias, out characterId) && visibleCharacters.ContainsKey(characterId);
            foreach (var state in visibleCharacters.Values) state.SpeakerActive = !hasVisibleSpeaker || state.CharacterId == characterId;
            TransitionSpeakerHighlight();
        }

        public static bool TryParseSlot(string value, out VNCharacterSlot slot)
        {
            switch (value)
            {
                case "far_left": slot = VNCharacterSlot.FarLeft; return true;
                case "left": slot = VNCharacterSlot.Left; return true;
                case "center": slot = VNCharacterSlot.Center; return true;
                case "right": slot = VNCharacterSlot.Right; return true;
                case "far_right": slot = VNCharacterSlot.FarRight; return true;
                default: slot = default; return false;
            }
        }

        public static bool TryParseFacing(string value, out VNCharacterFacing facing)
        {
            switch (value)
            {
                case "left": facing = VNCharacterFacing.Left; return true;
                case "right": facing = VNCharacterFacing.Right; return true;
                default: facing = default; return false;
            }
        }

        public static string ToSerializedSlot(VNCharacterSlot slot)
        {
            switch (slot)
            {
                case VNCharacterSlot.FarLeft: return "far_left";
                case VNCharacterSlot.Left: return "left";
                case VNCharacterSlot.Center: return "center";
                case VNCharacterSlot.Right: return "right";
                case VNCharacterSlot.FarRight: return "far_right";
                default: return null;
            }
        }

        public static string ToSerializedFacing(VNCharacterFacing facing) => facing == VNCharacterFacing.Left ? "left" : "right";

        private bool TryResolveCharacter(string characterId, string expressionId, out VNCharacterDefinition character, out string resolvedExpressionId, out VNExpressionDefinition expression)
        {
            character = null;
            resolvedExpressionId = null;
            expression = null;
            if (catalog == null || !catalog.TryGetCharacter(characterId, out character)) return Fail($"Cannot resolve character '{characterId}'.");
            resolvedExpressionId = expressionId == "default" ? character.DefaultExpressionId : expressionId;
            if (!character.TryGetExpression(resolvedExpressionId, out expression)) return Fail($"Cannot resolve expression '{expressionId}' for '{characterId}'.");
            return true;
        }

        private bool TryGetVisibleCharacter(string characterId, out VNCharacterRuntimeState state)
        {
            if (visibleCharacters.TryGetValue(characterId, out state)) return true;
            return Fail($"Character '{characterId}' is not visible.");
        }

        private bool TryGetSlotView(VNCharacterSlot slot, out VNCharacterSlotView view)
        {
            if (viewsBySlot.TryGetValue(slot, out view) && view != null && view.IsConfigured) return true;
            return Fail($"VN Presentation Controller has no configured slot view for '{slot}'.");
        }

        private bool HasConfiguredSlotView(VNCharacterSlot slot) => viewsBySlot.TryGetValue(slot, out var view) && view != null && view.IsConfigured;

        private void ClearAllVisibleCharacters()
        {
            foreach (var view in viewsBySlot.Values)
            {
                view.SetFadeAlpha(1f);
                view.SetVisible(false);
            }
            charactersBySlot.Clear();
            visibleCharacters.Clear();
            if (speakerHighlightCoroutine != null) StopCoroutine(speakerHighlightCoroutine);
            speakerHighlightCoroutine = null;
        }

        private void BuildSlotIndex()
        {
            viewsBySlot.Clear();
            foreach (var view in characterSlotViews)
            {
                if (view == null || !view.IsConfigured || !viewsBySlot.TryAdd(view.Slot, view))
                    Debug.LogError("VN Presentation Controller requires one configured VNCharacterSlotView for each character slot.", this);
            }
        }

        private void SetAllVisibleCharactersActive()
        {
            foreach (var state in visibleCharacters.Values) state.SpeakerActive = true;
            TransitionSpeakerHighlight();
        }

        private void TransitionSpeakerHighlight()
        {
            if (!Application.isPlaying)
            {
                ApplySpeakerHighlightImmediately();
                return;
            }

            if (speakerHighlightCoroutine != null) StopCoroutine(speakerHighlightCoroutine);
            speakerHighlightCoroutine = StartCoroutine(TransitionSpeakerHighlightRoutine());
        }

        private void ApplySpeakerHighlightImmediately()
        {
            foreach (var state in visibleCharacters.Values)
                if (viewsBySlot.TryGetValue(state.Slot, out var view)) view.SetTint(state.SpeakerActive ? ActiveColor : InactiveColor);
        }

        private IEnumerator TransitionSpeakerHighlightRoutine()
        {
            var starts = new Dictionary<VNCharacterSlotView, Color>();
            var targets = new Dictionary<VNCharacterSlotView, Color>();
            foreach (var state in visibleCharacters.Values)
            {
                var view = viewsBySlot[state.Slot];
                starts[view] = view.Tint;
                targets[view] = state.SpeakerActive ? ActiveColor : InactiveColor;
            }

            if (speakerHighlightDuration <= 0f)
            {
                foreach (var pair in targets) pair.Key.SetTint(pair.Value);
                yield break;
            }

            for (var elapsed = 0f; elapsed < speakerHighlightDuration; elapsed += Time.unscaledDeltaTime)
            {
                var progress = elapsed / speakerHighlightDuration;
                foreach (var pair in targets) pair.Key.SetTint(Color.Lerp(starts[pair.Key], pair.Value, progress));
                yield return null;
            }

            foreach (var pair in targets) pair.Key.SetTint(pair.Value);
        }

        private static void SetImageVisible(Image image, bool visible)
        {
            if (image != null) image.enabled = visible;
        }

        private static int CompareRuntimeStates(VNCharacterRuntimeState left, VNCharacterRuntimeState right)
        {
            var slotComparison = left.Slot.CompareTo(right.Slot);
            return slotComparison != 0 ? slotComparison : string.CompareOrdinal(left.CharacterId, right.CharacterId);
        }

        private static int CompareSavedCharacters(CharacterSaveState left, CharacterSaveState right)
        {
            TryParseSlot(left.slot, out var leftSlot);
            TryParseSlot(right.slot, out var rightSlot);
            var slotComparison = leftSlot.CompareTo(rightSlot);
            return slotComparison != 0 ? slotComparison : string.CompareOrdinal(left.characterId, right.characterId);
        }

        private static bool IsPositiveFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;

        private bool Fail(string message)
        {
            Debug.LogError(message, this);
            return false;
        }
    }
}
