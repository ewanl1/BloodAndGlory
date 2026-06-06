using System;

namespace BloodAndGlory.Combat.Core
{
    public sealed class CombatResolver
    {
        public DamageResult ResolveHit(
            CombatState state,
            HitProposal hit,
            WeaponProfileData weapon,
            BlockContext block)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (block.IsParryWindowActive)
                return NoDamage(state.Health, CombatEventType.Parried, hit);

            if (block.IsBlocking)
                return NoDamage(state.Health, CombatEventType.Blocked, hit);

            if (hit.DefenderIsPlayer)
                return NoDamage(state.Health, CombatEventType.WouldHitPlayer, hit);

            if (state.IsDuplicate(hit, weapon))
                return NoDamage(state.Health, CombatEventType.SuppressedDuplicate, hit);

            var damage = CalculateDamage(hit, weapon);
            var health = state.Health.ApplyDamage(damage);
            var eventType = health.IsAlive ? CombatEventType.Damaged : CombatEventType.Died;
            var combatEvent = new CombatEvent(eventType, hit.AttackerId, hit.DefenderId, hit.WeaponId, hit.Region, damage, hit.ImpactVelocity, hit.TimeSeconds);
            var fullReaction = hit.ImpactVelocity >= weapon.MinimumFullReactionVelocity;
            return new DamageResult(health, combatEvent, fullReaction);
        }

        private static DamageResult NoDamage(HealthState health, CombatEventType eventType, HitProposal hit)
        {
            var combatEvent = new CombatEvent(eventType, hit.AttackerId, hit.DefenderId, hit.WeaponId, hit.Region, 0, hit.ImpactVelocity, hit.TimeSeconds);
            return new DamageResult(health, combatEvent, fullReaction: false);
        }

        private static int CalculateDamage(HitProposal hit, WeaponProfileData weapon)
        {
            var normalizedVelocity = Math.Clamp(hit.ImpactVelocity / weapon.MaximumDamageVelocity, 0f, 1f);
            var baseDamage = weapon.MinimumDamage + (weapon.MaximumDamage - weapon.MinimumDamage) * normalizedVelocity;
            var regionDamage = baseDamage * weapon.RegionDamage.MultiplierFor(hit.Region);
            return Math.Max(weapon.MinimumDamage, (int)MathF.Round(regionDamage));
        }
    }
}
