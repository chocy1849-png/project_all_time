using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>Turns many slider preview changes into one explicit user commit.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Slider))]
    public sealed class VNSettingsSliderCommit : MonoBehaviour, IPointerUpHandler, IEndDragHandler, ISubmitHandler, IDeselectHandler
    {
        [SerializeField] private Slider slider;
        private float authoritativeValue;
        private bool hasAuthoritativeValue;
        public event Action<float> CommitRequested;

        private void Awake()
        {
            var localSlider = GetComponent<Slider>();
            if (slider == null) slider = localSlider;
        }

        public void Initialize(Slider source)
        {
            if (source == null || source != GetComponent<Slider>()) throw new InvalidOperationException("Slider commit seam must be on the same GameObject as its Slider.");
            if (slider != null && slider != source) throw new InvalidOperationException("Slider commit seam is already initialized for another Slider.");
            slider = source;
        }

        public bool TryValidateWiring(out string diagnostic)
        {
            if (slider == null || slider != GetComponent<Slider>()) { diagnostic = "Slider commit seam must reference its same-GameObject Slider."; return false; }
            diagnostic = null;
            return true;
        }

        public void SyncAuthoritativeValue(float value) { authoritativeValue = value; hasAuthoritativeValue = true; }

        public void Commit()
        {
            if (slider != null && (!hasAuthoritativeValue || !Mathf.Approximately(slider.value, authoritativeValue))) CommitRequested?.Invoke(slider.value);
        }

        public void OnPointerUp(PointerEventData eventData) => Commit();
        public void OnEndDrag(PointerEventData eventData) => Commit();
        public void OnSubmit(BaseEventData eventData) => Commit();
        public void OnDeselect(BaseEventData eventData) => Commit();
    }
}
