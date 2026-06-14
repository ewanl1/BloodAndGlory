using System.Collections;
using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Enemy;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BloodAndGlory.Combat.Tests.PlayMode
{
    public sealed class PeasantRuntimePlayModeTests
    {
        [UnityTest]
        public IEnumerator Peasant_CombatTrainingScene_ApproachesAndProposesAttack()
        {
            yield return SceneManager.LoadSceneAsync("CombatTrainingScene", LoadSceneMode.Single);
            yield return null;

            var enemy = GameObject.Find("PeasantBrown_Combat");
            Assert.IsNotNull(enemy);

            var controller = enemy.GetComponent<EnemyCombatController>();
            Assert.IsNotNull(controller);

            HitProposal? proposedAttack = null;
            controller.AttackProposed += proposal => proposedAttack = proposal;

            yield return WaitForFiniteDistance(controller);
            var startingDistance = controller.DistanceToTarget;

            yield return WaitUntil(
                () => controller.DistanceToTarget < startingDistance - 0.1f,
                timeoutSeconds: 2.0f,
                $"Expected peasant to approach player. Start={startingDistance:0.00}, Current={controller.DistanceToTarget:0.00}, Movement={controller.MovementMode}, State={controller.State}.");

            yield return WaitUntil(
                () => proposedAttack.HasValue,
                timeoutSeconds: 8.0f,
                $"Expected peasant to emit an attack proposal. Distance={controller.DistanceToTarget:0.00}, Movement={controller.MovementMode}, State={controller.State}.");

            Assert.IsTrue(proposedAttack.Value.DefenderIsPlayer);
            Assert.AreEqual(1, proposedAttack.Value.DefenderId);
            Assert.AreEqual("broadsword", proposedAttack.Value.WeaponId);
            Assert.AreEqual(HurtboxRegion.Torso, proposedAttack.Value.Region);
        }

        private static IEnumerator WaitForFiniteDistance(EnemyCombatController controller)
        {
            var deadline = Time.time + 1.0f;
            while (float.IsInfinity(controller.DistanceToTarget) && Time.time < deadline)
                yield return null;

            Assert.IsFalse(float.IsInfinity(controller.DistanceToTarget), "Enemy never resolved a finite target distance.");
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeoutSeconds, string failureMessage)
        {
            var deadline = Time.time + timeoutSeconds;
            while (!condition() && Time.time < deadline)
                yield return null;

            Assert.IsTrue(condition(), failureMessage);
        }
    }
}
