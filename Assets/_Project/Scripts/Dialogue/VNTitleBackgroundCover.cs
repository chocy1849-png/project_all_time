using UnityEngine;
using UnityEngine.UI;

namespace ProjectAllTime.VN.Title
{
    /// <summary>Cover-crops the assigned sprite without stretching it.</summary>
    [ExecuteAlways, RequireComponent(typeof(Image), typeof(AspectRatioFitter))]
    public sealed class VNTitleBackgroundCover : MonoBehaviour
    {
        private Sprite observed;
        private void OnEnable() => Refresh();
        private void OnValidate() => Refresh();
        private void Update() { if (GetComponent<Image>().sprite != observed) Refresh(); }
        private void Refresh()
        {
            var image = GetComponent<Image>(); var fitter = GetComponent<AspectRatioFitter>();
            if (image == null || fitter == null) return;
            observed = image.sprite;
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            if (observed != null && observed.rect.height > 0) fitter.aspectRatio = observed.rect.width / observed.rect.height;
        }
    }
}
