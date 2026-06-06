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
    }
}
