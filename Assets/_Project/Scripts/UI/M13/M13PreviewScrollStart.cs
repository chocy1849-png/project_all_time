using UnityEngine;
using UnityEngine.UI;

// Initializes the isolated preview list after layout has been resolved.
[RequireComponent(typeof(ScrollRect))]
public sealed class M13PreviewScrollStart : MonoBehaviour
{
    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
    }
}
