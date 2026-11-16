using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectAllTime.VN.Dialogue;

public class M13SharedControlTests
{
    [Test]
    public void Tab_SelectionAndFocusRemainIndependent()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/M13/M13_SharedTab.prefab");
        var go = Object.Instantiate(prefab);
        try
        {
            var state = go.GetComponent<M13SharedControlState>();
            var focus = go.transform.Find("Focus").gameObject;
            var active = go.transform.Find("Active").gameObject;
            state.SetActive(true); state.OnSelect(null);
            Assert.IsTrue(focus.activeSelf); Assert.IsTrue(active.activeSelf);
            state.OnDeselect(null); Assert.IsFalse(focus.activeSelf); Assert.IsTrue(active.activeSelf);
            go.GetComponent<Button>().interactable = false; state.OnSelect(null);
            Assert.IsFalse(focus.activeSelf);
            Assert.IsNotNull(go.GetComponent<VNUIThemeBinding>().Theme);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
