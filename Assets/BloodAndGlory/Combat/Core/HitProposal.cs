namespace BloodAndGlory.Combat.Core
{
    public readonly struct HitProposal
    {
        public HitProposal(
            int attackerId,
            int defenderId,
            string weaponId,
            HurtboxRegion region,
            float impactVelocity,
            float timeSeconds,
            bool defenderIsPlayer)
        {
            AttackerId = attackerId;
            DefenderId = defenderId;
            WeaponId = weaponId;
            Region = region;
            ImpactVelocity = impactVelocity;
            TimeSeconds = timeSeconds;
            DefenderIsPlayer = defenderIsPlayer;
        }

        public int AttackerId { get; }
        public int DefenderId { get; }
        public string WeaponId { get; }
        public HurtboxRegion Region { get; }
        public float ImpactVelocity { get; }
        public float TimeSeconds { get; }
        public bool DefenderIsPlayer { get; }
    }
}
