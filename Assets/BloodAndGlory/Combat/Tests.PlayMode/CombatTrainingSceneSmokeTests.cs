using System.Collections;
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
            Assert.IsNotNull(GameObject.Find("Combat Debug Overlay"));
            Assert.IsNotNull(GameObject.Find("XR Combat Rig"));

            var sword = GameObject.Find("Player Broadsword");
            Assert.IsNotNull(sword);
            var rigidbody = sword.GetComponent<Rigidbody>();
            Assert.IsNotNull(rigidbody);
            Assert.IsTrue(rigidbody.useGravity);
        }
    }
}
