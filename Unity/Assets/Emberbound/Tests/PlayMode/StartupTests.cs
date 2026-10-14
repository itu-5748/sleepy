using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Emberbound.Tests
{
    public class StartupTests
    {
        [UnityTest] public IEnumerator GameCreatesTerrainModelsAndCamera()
        {
            var root = new GameObject("Emberbound test"); root.AddComponent<Game>(); yield return null;
            Assert.IsNotNull(root.transform.Find("Static world/Raised stone paths"));
            Assert.IsNotNull(root.transform.Find("Run actors/Knight"));
            var camera = root.transform.Find("Top-down camera").GetComponent<Camera>();
            Assert.IsTrue(camera.orthographic); Assert.Greater(root.GetComponentsInChildren<MeshRenderer>().Length, 10);
            Object.Destroy(root); yield return null;
        }
    }
}
