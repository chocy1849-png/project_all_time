using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.SaveLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class M1304SaveLoadRedesignTests
    {
        private const string CardPath = "Assets/_Project/Prefabs/SaveLoad/VNSaveSlotItem.prefab";

        [Test]
        public void SlotPrefab_ContainsIndependentSelectedAndFocusVisuals()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPath);
            Assert.That(prefab, Is.Not.Null);
            var instance = Object.Instantiate(prefab);
            try
            {
                var state = instance.GetComponent<M13SharedControlState>();
                Assert.That(instance.GetComponent<VNSaveSlotItem>(), Is.Not.Null);
                Assert.That(instance.GetComponent<Button>(), Is.Not.Null);
                Assert.That(state, Is.Not.Null);
                var focus = instance.transform.Find("M13_Focus").gameObject;
                var selected = instance.transform.Find("M13_Active").gameObject;
                var surface = instance.transform.Find("M13_SelectedSurface").gameObject;
                instance.GetComponent<VNSaveSlotItem>().SetSelected(true);
                var slotLabel = instance.transform.Find("SlotLabel").GetComponent<TMPro.TMP_Text>();
                Assert.That((Color32)slotLabel.color, Is.EqualTo(new Color32(240, 218, 187, 255)));
                state.OnSelect(null);
                Assert.That(focus.activeSelf && selected.activeSelf && surface.activeSelf, Is.True);
                state.OnDeselect(null);
                Assert.That(focus.activeSelf, Is.False);
                Assert.That(selected.activeSelf && surface.activeSelf, Is.True);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void SlotThumbnail_UsesAspectPreservingFitter()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPath);
            var image = prefab.transform.Find("ThumbnailArea/Thumbnail");
            Assert.That(image, Is.Not.Null);
            Assert.That(image.GetComponent<AspectRatioFitter>().aspectMode, Is.EqualTo(AspectRatioFitter.AspectMode.FitInParent));
        }

        [TestCase(VNSaveLoadCategory.Manual, 2)]
        [TestCase(VNSaveLoadCategory.Auto, 1)]
        [TestCase(VNSaveLoadCategory.Quick, 1)]
        public void PageCounts_FollowPhysicalSlotAllocation(VNSaveLoadCategory category, int expected)
        {
            Assert.That(VNSaveLoadSlotModelBuilder.GetPageCount(category), Is.EqualTo(expected));
        }
    }
}
