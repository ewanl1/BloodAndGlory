using System;

namespace BloodAndGlory.Combat.Core
{
    public readonly struct HealthState
    {
        public HealthState(int maxHitPoints)
            : this(maxHitPoints, maxHitPoints)
        {
        }

        private HealthState(int maxHitPoints, int currentHitPoints)
        {
            if (maxHitPoints <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHitPoints), "Max HP must be greater than zero.");

            MaxHitPoints = maxHitPoints;
            CurrentHitPoints = Math.Clamp(currentHitPoints, 0, maxHitPoints);
        }

        public int MaxHitPoints { get; }
        public int CurrentHitPoints { get; }
        public bool IsAlive => CurrentHitPoints > 0;

        public HealthState ApplyDamage(int damage)
        {
            if (damage <= 0)
                return this;

            return new HealthState(MaxHitPoints, CurrentHitPoints - damage);
        }
    }
}
