using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Training;
using NUnit.Framework;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatTrainingRuntimeTests
    {
        [Test]
        public void ResolvePlayerHit_AppliesDamageAndReportsDebug()
        {
            var state = new CombatState(new HealthState(100));
            var profile = WeaponProfileData.BroadswordDefaults;
            var hit = new HitProposal(1, 10, "broadsword", HurtboxRegion.Head, 8f, 1f, false);

            var result = CombatTrainingRuntime.ResolvePlayerHitForTests(state, hit, profile, new BlockContext(false, false));

            Assert.Greater(result.Event.Damage, 1);
            Assert.AreEqual(CombatEventType.Damaged, result.Event.Type);
            Assert.Less(result.Health.CurrentHitPoints, state.Health.CurrentHitPoints);
        }
    }
}
