using BloodAndGlory.Combat.Core;
using NUnit.Framework;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatResolverTests
    {
        [Test]
        public void HealthState_ClampsDamageAndMarksDead()
        {
            var health = new HealthState(10);

            health = health.ApplyDamage(4);
            Assert.AreEqual(6, health.CurrentHitPoints);
            Assert.IsTrue(health.IsAlive);

            health = health.ApplyDamage(20);
            Assert.AreEqual(0, health.CurrentHitPoints);
            Assert.IsFalse(health.IsAlive);
        }

        [Test]
        public void ResolveHit_AppliesVelocityScaledRegionDamage()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 2, profile.Id, HurtboxRegion.Head, 4.0f, 10.0f, defenderIsPlayer: false);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.Damaged, result.Event.Type);
            Assert.Greater(result.Event.Damage, profile.MinimumDamage);
            Assert.LessOrEqual(result.Event.Damage, profile.MaximumDamage * 2);
            Assert.AreEqual(100 - result.Event.Damage, result.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveHit_ProducesMinimumChipDamageForLowVelocity()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 0.05f, 10.0f, defenderIsPlayer: false);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.Damaged, result.Event.Type);
            Assert.AreEqual(1, result.Event.Damage);
            Assert.IsFalse(result.FullReaction);
        }

        [Test]
        public void ResolveHit_SuppressesDuplicateWithinWindow()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var first = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 6.0f, 1.0f, defenderIsPlayer: false);
            var duplicate = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 6.0f, 1.1f, defenderIsPlayer: false);

            var firstResult = resolver.ResolveHit(state, first, profile, new BlockContext(false, false));
            state = state.WithHealth(firstResult.Health).RecordHit(first, profile);

            var duplicateResult = resolver.ResolveHit(state, duplicate, profile, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.SuppressedDuplicate, duplicateResult.Event.Type);
            Assert.AreEqual(firstResult.Health.CurrentHitPoints, duplicateResult.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveHit_ParryBeatsBlockAndPreventsDamage()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 6.0f, 2.0f, defenderIsPlayer: false);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(isBlocking: true, isParryWindowActive: true));

            Assert.AreEqual(CombatEventType.Parried, result.Event.Type);
            Assert.AreEqual(0, result.Event.Damage);
            Assert.AreEqual(100, result.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveHit_BlockPreventsDamage()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 6.0f, 2.0f, defenderIsPlayer: false);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(isBlocking: true, isParryWindowActive: false));

            Assert.AreEqual(CombatEventType.Blocked, result.Event.Type);
            Assert.AreEqual(0, result.Event.Damage);
            Assert.AreEqual(100, result.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveHit_PlayerDefenderProducesWouldHitPlayer()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 99, profile.Id, HurtboxRegion.Torso, 6.0f, 2.0f, defenderIsPlayer: true);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.WouldHitPlayer, result.Event.Type);
            Assert.AreEqual(0, result.Event.Damage);
            Assert.AreEqual(100, result.Health.CurrentHitPoints);
        }
    }
}
