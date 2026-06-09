using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using BloodAndGlory.Combat.Runtime.Enemy;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class EnemyDecisionServiceTests
    {
        [Test]
        public void Decide_ApproachesWhenOutOfRange()
        {
            var service = new EnemyDecisionService();
            var profile = EnemyProfileData.WeakPeasantDefaults;

            var next = service.Decide(EnemyCombatState.Idle, profile, distanceToTarget: 5.0f, timeInState: 0.2f, randomValue: 0.5f);

            Assert.AreEqual(EnemyCombatState.Approach, next);
        }

        [Test]
        public void Decide_TelegraphsWhenInsidePreferredRange()
        {
            var service = new EnemyDecisionService();
            var profile = EnemyProfileData.WeakPeasantDefaults;

            var next = service.Decide(EnemyCombatState.Approach, profile, distanceToTarget: 1.4f, timeInState: 0.2f, randomValue: 0.5f);

            Assert.AreEqual(EnemyCombatState.Telegraph, next);
        }

        [Test]
        public void Decide_WeakPeasantRarelyBlocks()
        {
            var service = new EnemyDecisionService();
            var profile = EnemyProfileData.WeakPeasantDefaults;

            var doesNotBlock = service.DecideDefense(profile, randomValue: 0.9f);
            var doesBlock = service.DecideDefense(profile, randomValue: 0.04f);

            Assert.AreEqual(EnemyCombatState.Recover, doesNotBlock);
            Assert.AreEqual(EnemyCombatState.Block, doesBlock);
        }

        [Test]
        public void Decide_TransitionsAttackCommitToRecoverAfterActiveWindow()
        {
            var service = new EnemyDecisionService();
            var profile = EnemyProfileData.WeakPeasantDefaults;

            var next = service.Decide(EnemyCombatState.AttackCommit, profile, distanceToTarget: 1.2f, timeInState: 0.8f, randomValue: 0.5f);

            Assert.AreEqual(EnemyCombatState.Recover, next);
        }

        [Test]
        public void CreateAttackProposalForTests_TargetsPlayerWithBroadsword()
        {
            var enemy = new GameObject("enemy");
            try
            {
                enemy.AddComponent<CombatantAuthoring>().ConfigureForTests(12, isPlayer: false, maxHitPoints: 100);
                enemy.AddComponent<NavMeshAgent>();
                enemy.AddComponent<Animator>();
                var controller = enemy.AddComponent<EnemyCombatController>();

                var hit = controller.CreateAttackProposalForTests(3f);

                Assert.AreEqual(12, hit.AttackerId);
                Assert.AreEqual(1, hit.DefenderId);
                Assert.AreEqual("broadsword", hit.WeaponId);
                Assert.AreEqual(HurtboxRegion.Torso, hit.Region);
                Assert.AreEqual(4f, hit.ImpactVelocity);
                Assert.AreEqual(3f, hit.TimeSeconds);
                Assert.IsTrue(hit.DefenderIsPlayer);
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }
    }
}
