using System;
using System.Collections.Generic;

namespace BloodAndGlory.Combat.Core
{
    public sealed class CombatState
    {
        private readonly Dictionary<HitKey, float> _lastHitTimes;

        public CombatState(HealthState health)
            : this(health, new Dictionary<HitKey, float>())
        {
        }

        private CombatState(HealthState health, Dictionary<HitKey, float> lastHitTimes)
        {
            Health = health;
            _lastHitTimes = lastHitTimes;
        }

        public HealthState Health { get; }

        public bool IsDuplicate(HitProposal hit, WeaponProfileData profile)
        {
            var key = HitKey.From(hit);
            return _lastHitTimes.TryGetValue(key, out var lastTime)
                && hit.TimeSeconds - lastTime < profile.DuplicateHitWindowSeconds;
        }

        public CombatState WithHealth(HealthState health)
        {
            return new CombatState(health, new Dictionary<HitKey, float>(_lastHitTimes));
        }

        public CombatState RecordHit(HitProposal hit, WeaponProfileData profile)
        {
            var copy = new Dictionary<HitKey, float>(_lastHitTimes)
            {
                [HitKey.From(hit)] = hit.TimeSeconds
            };
            return new CombatState(Health, copy);
        }

        private readonly struct HitKey : IEquatable<HitKey>
        {
            private HitKey(int attackerId, int defenderId, string weaponId, HurtboxRegion region)
            {
                AttackerId = attackerId;
                DefenderId = defenderId;
                WeaponId = weaponId;
                Region = region;
            }

            private int AttackerId { get; }
            private int DefenderId { get; }
            private string WeaponId { get; }
            private HurtboxRegion Region { get; }

            public static HitKey From(HitProposal hit)
            {
                return new HitKey(hit.AttackerId, hit.DefenderId, hit.WeaponId, hit.Region);
            }

            public bool Equals(HitKey other)
            {
                return AttackerId == other.AttackerId
                    && DefenderId == other.DefenderId
                    && WeaponId == other.WeaponId
                    && Region == other.Region;
            }

            public override bool Equals(object obj)
            {
                return obj is HitKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(AttackerId, DefenderId, WeaponId, Region);
            }
        }
    }
}
