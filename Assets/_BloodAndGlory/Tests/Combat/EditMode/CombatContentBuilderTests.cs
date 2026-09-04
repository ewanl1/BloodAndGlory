using BloodAndGlory.Combat.Editor;
using NUnit.Framework;
using UnityEngine;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatContentBuilderTests
    {
        private GameObject root;

        [TearDown]
        public void TearDown()
        {
            if (root != null)
                Object.DestroyImmediate(root);
        }

        [Test]
        public void GetOrCreateRootSceneObjectForTests_ReusesExistingObject()
        {
            root = new GameObject("Existing Scene Object");

            var result = CombatContentBuilder.GetOrCreateRootSceneObjectForTests(root.name);

            Assert.AreSame(root, result);
        }

        [Test]
        public void GetOrCreateChildSceneObjectForTests_PreservesExistingTransform()
        {
            root = new GameObject("Parent");
            var child = new GameObject("Combat Debug Text");
            child.transform.SetParent(root.transform, false);
            child.transform.localPosition = new Vector3(1.2f, 3.4f, 5.6f);
            child.transform.localRotation = Quaternion.Euler(10f, 20f, 30f);
            child.transform.localScale = new Vector3(0.2f, 0.3f, 0.4f);

            var result = CombatContentBuilder.GetOrCreateChildSceneObjectForTests(root.transform, child.name, out var created);

            Assert.IsFalse(created);
            Assert.AreSame(child, result);
            Assert.AreEqual(new Vector3(1.2f, 3.4f, 5.6f), result.transform.localPosition);
            Assert.AreEqual(Quaternion.Euler(10f, 20f, 30f), result.transform.localRotation);
            Assert.AreEqual(new Vector3(0.2f, 0.3f, 0.4f), result.transform.localScale);
        }
    }
}
