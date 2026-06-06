namespace BloodAndGlory.Combat.Core
{
    public readonly struct CombatEvent
    {
        public CombatEvent(
            CombatEventType type,
            int attackerId,
            int defenderId,
            string weaponId,
            HurtboxRegion region,
            int damage,
            float impactVelocity,
            float timeSeconds)
        {
            Type = type;
            AttackerId = attackerId;
            DefenderId = defenderId;
            WeaponId = weaponId;
            Region = region;
            Damage = damage;
            ImpactVelocity = impactVelocity;
            TimeSeconds = timeSeconds;
        }

        public CombatEventType Type { get; }
        public int AttackerId { get; }
        public int DefenderId { get; }
        public string WeaponId { get; }
        public HurtboxRegion Region { get; }
        public int Damage { get; }
        public float ImpactVelocity { get; }
        public float TimeSeconds { get; }
    }
}
