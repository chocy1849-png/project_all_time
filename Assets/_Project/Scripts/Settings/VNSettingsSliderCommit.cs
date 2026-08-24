using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Settings
{
    /// <summary>Turns many slider preview changes into one explicit user commit.</summary>
    [DisallowMultipleComponent]
    public sealed class VNSettingsSliderCommit : MonoBehaviour, IPointerUpHandler, IEndDragHandler, ISubmitHandler, IDeselectHandler
    {
        [SerializeField] private Slider slider;
        public event Action<float> CommitRequested;

        public void Initialize(Slider source)
        {
            if (slider != null && slider != source) throw new InvalidOperationException("Slider commit seam is already initialized for another Slider.");
            slider = source;
        }

        public void Commit()
        {
            if (slider != null) CommitRequested?.Invoke(slider.value);
        }

        public void OnPointerUp(PointerEventData eventData) => Commit();
        public void OnEndDrag(PointerEventData eventData) => Commit();
        public void OnSubmit(BaseEventData eventData) => Commit();
        public void OnDeselect(BaseEventData eventData) => Commit();
    }
}
