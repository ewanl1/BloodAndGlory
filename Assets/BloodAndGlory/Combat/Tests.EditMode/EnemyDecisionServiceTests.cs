using BloodAndGlory.Combat.Core;
using NUnit.Framework;

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
    }
}
