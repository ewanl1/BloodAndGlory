using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Training;
using NUnit.Framework;
using UnityEngine;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatTrainingRuntimeTests
    {
        private GameObject runtimeObject;

        [TearDown]
        public void TearDown()
        {
            if (runtimeObject != null)
                UnityEngine.Object.DestroyImmediate(runtimeObject);
        }

        [Test]
        public void ResolvePlayerHitForTests_AppliesDamage()
        {
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 10, "broadsword", HurtboxRegion.Head, 8f, 1f, false);

            var result = CombatTrainingRuntime.ResolvePlayerHitForTests(
                state,
                hit,
                WeaponProfileData.BroadswordDefaults,
                new BlockContext(false, false));

            Assert.Greater(result.Event.Damage, 1);
            Assert.AreEqual(CombatEventType.Damaged, result.Event.Type);
            Assert.Less(result.Health.CurrentHitPoints, state.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveEnemyHitForTests_PlayerDefenderProducesWouldHitPlayer()
        {
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(10, 1, "broadsword", HurtboxRegion.Torso, 4f, 1f, true);

            var result = CombatTrainingRuntime.ResolveEnemyHitForTests(
                state,
                hit,
                WeaponProfileData.BroadswordDefaults,
                new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.WouldHitPlayer, result.Event.Type);
            Assert.AreEqual(0, result.Event.Damage);
        }

        [Test]
        public void ResolveEnemyHitForTests_PlayerBlockProducesBlocked()
        {
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(10, 1, "broadsword", HurtboxRegion.Torso, 4f, 1f, true);

            var result = CombatTrainingRuntime.ResolveEnemyHitForTests(
                state,
                hit,
                WeaponProfileData.BroadswordDefaults,
                new BlockContext(isBlocking: true, isParryWindowActive: false));

            Assert.AreEqual(CombatEventType.Blocked, result.Event.Type);
            Assert.AreEqual(0, result.Event.Damage);
        }

        [Test]
        public void ResolveEnemyHitForRuntimeTests_RecordsPlayerDamageDeferralEvents()
        {
            var runtime = CreateRuntime(100);
            var hit = new HitProposal(10, 1, "broadsword", HurtboxRegion.Torso, 4f, 1f, true);

            var wouldHit = runtime.ResolveEnemyHitForRuntimeTests(hit, new BlockContext(false, false));
            var blocked = runtime.ResolveEnemyHitForRuntimeTests(hit, new BlockContext(isBlocking: true, isParryWindowActive: false));

            Assert.AreEqual(CombatEventType.WouldHitPlayer, wouldHit.Event.Type);
            Assert.AreEqual(CombatEventType.Blocked, blocked.Event.Type);
            Assert.AreEqual(0, wouldHit.Event.Damage);
            Assert.AreEqual(0, blocked.Event.Damage);
        }

        [Test]
        public void ResolvePlayerHitForRuntimeTests_AcceptedHitUpdatesEnemyHealth()
        {
            var runtime = CreateRuntime(100);
            var hit = new HitProposal(1, 10, "broadsword", HurtboxRegion.Head, 8f, 1f, false);

            var result = runtime.ResolvePlayerHitForRuntimeTests(hit, new BlockContext(false, false));

            Assert.Greater(result.Event.Damage, 1);
            Assert.AreEqual(CombatEventType.Damaged, result.Event.Type);
            Assert.Less(runtime.CurrentHealthForRuntimeTests.CurrentHitPoints, 100);
            Assert.AreEqual(result.Health.CurrentHitPoints, runtime.CurrentHealthForRuntimeTests.CurrentHitPoints);
        }

        [Test]
        public void ResolvePlayerHitForRuntimeTests_DuplicateHitDoesNotApplyDamageTwice()
        {
            var runtime = CreateRuntime(100);
            var first = new HitProposal(1, 10, "broadsword", HurtboxRegion.Head, 8f, 1f, false);
            var duplicate = new HitProposal(1, 10, "broadsword", HurtboxRegion.Head, 8f, 1.1f, false);

            var firstResult = runtime.ResolvePlayerHitForRuntimeTests(first, new BlockContext(false, false));
            var healthAfterFirst = runtime.CurrentHealthForRuntimeTests.CurrentHitPoints;
            var duplicateResult = runtime.ResolvePlayerHitForRuntimeTests(duplicate, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.Damaged, firstResult.Event.Type);
            Assert.AreEqual(CombatEventType.SuppressedDuplicate, duplicateResult.Event.Type);
            Assert.AreEqual(healthAfterFirst, runtime.CurrentHealthForRuntimeTests.CurrentHitPoints);
        }

        [Test]
        public void ResolvePlayerHitForRuntimeTests_ZeroDamageResultDoesNotPolluteDuplicateState()
        {
            var runtime = CreateRuntime(100);
            var blocked = new HitProposal(1, 10, "broadsword", HurtboxRegion.Head, 8f, 1f, false);
            var unblocked = new HitProposal(1, 10, "broadsword", HurtboxRegion.Head, 8f, 1.1f, false);

            var blockedResult = runtime.ResolvePlayerHitForRuntimeTests(blocked, new BlockContext(isBlocking: true, isParryWindowActive: false));
            var unblockedResult = runtime.ResolvePlayerHitForRuntimeTests(unblocked, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.Blocked, blockedResult.Event.Type);
            Assert.AreEqual(0, blockedResult.Event.Damage);
            Assert.AreEqual(CombatEventType.Damaged, unblockedResult.Event.Type);
            Assert.Greater(unblockedResult.Event.Damage, 0);
        }

        private CombatTrainingRuntime CreateRuntime(int health)
        {
            runtimeObject = new GameObject("Runtime Test");
            var runtime = runtimeObject.AddComponent<CombatTrainingRuntime>();
            runtime.ConfigureForRuntimeTests(new HealthState(health), WeaponProfileData.BroadswordDefaults);
            return runtime;
        }
    }
}
