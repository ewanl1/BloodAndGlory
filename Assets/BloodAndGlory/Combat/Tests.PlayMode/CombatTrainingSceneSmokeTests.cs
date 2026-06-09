using System.Collections;
using System.Reflection;
using BloodAndGlory.Combat.Runtime.Debug;
using BloodAndGlory.Combat.Runtime.Training;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BloodAndGlory.Combat.Tests.PlayMode
{
    public sealed class CombatTrainingSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator CombatTrainingScene_Loads()
        {
            yield return SceneManager.LoadSceneAsync("CombatTrainingScene", LoadSceneMode.Single);
            var scene = SceneManager.GetActiveScene();

            Assert.AreEqual("CombatTrainingScene", scene.name);
            Assert.IsNotNull(GameObject.Find("Training Floor"));
            var overlayObject = GameObject.Find("Combat Debug Overlay");
            Assert.IsNotNull(overlayObject);
            var overlay = overlayObject.GetComponent<CombatDebugOverlay>();
            Assert.IsNotNull(overlay);
            AssertSerializedReference(overlay, "worldText");
            var debugText = GameObject.Find("Combat Debug Text");
            Assert.IsNotNull(debugText);
            Assert.IsNotNull(debugText.GetComponent<TextMesh>());
            var runtimeObject = GameObject.Find("Combat Training Runtime");
            Assert.IsNotNull(runtimeObject);
            var runtime = runtimeObject.GetComponent<CombatTrainingRuntime>();
            Assert.IsNotNull(runtime);
            AssertSerializedReference(runtime, "playerSword");
            AssertSerializedReference(runtime, "playerSwordProfile");
            AssertSerializedReference(runtime, "enemyCombatant");
            AssertSerializedReference(runtime, "enemyController");
            AssertSerializedReference(runtime, "debugOverlay");
            Assert.IsNotNull(GameObject.Find("XR Combat Rig"));

            var sword = GameObject.Find("Player Broadsword");
            Assert.IsNotNull(sword);
            var rigidbody = sword.GetComponent<Rigidbody>();
            Assert.IsNotNull(rigidbody);
            Assert.IsTrue(rigidbody.useGravity);
        }

        private static void AssertSerializedReference(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing serialized field {fieldName}.");
            Assert.IsNotNull(field.GetValue(target), $"{fieldName} is not wired.");
        }
    }
}
