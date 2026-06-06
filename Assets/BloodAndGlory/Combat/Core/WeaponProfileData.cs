using System;

namespace BloodAndGlory.Combat.Core
{
    public readonly struct WeaponProfileData
    {
        public WeaponProfileData(
            string id,
            int minimumDamage,
            int maximumDamage,
            float minimumFullReactionVelocity,
            float maximumDamageVelocity,
            float duplicateHitWindowSeconds,
            RegionDamageTable regionDamage)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Weapon id is required.", nameof(id));
            if (minimumDamage < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumDamage));
            if (maximumDamage < minimumDamage)
                throw new ArgumentOutOfRangeException(nameof(maximumDamage));
            if (minimumFullReactionVelocity < 0f)
                throw new ArgumentOutOfRangeException(nameof(minimumFullReactionVelocity));
            if (maximumDamageVelocity <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maximumDamageVelocity));
            if (duplicateHitWindowSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duplicateHitWindowSeconds));

            Id = id;
            MinimumDamage = minimumDamage;
            MaximumDamage = maximumDamage;
            MinimumFullReactionVelocity = minimumFullReactionVelocity;
            MaximumDamageVelocity = maximumDamageVelocity;
            DuplicateHitWindowSeconds = duplicateHitWindowSeconds;
            RegionDamage = regionDamage;
        }

        public string Id { get; }
        public int MinimumDamage { get; }
        public int MaximumDamage { get; }
        public float MinimumFullReactionVelocity { get; }
        public float MaximumDamageVelocity { get; }
        public float DuplicateHitWindowSeconds { get; }
        public RegionDamageTable RegionDamage { get; }

        public static WeaponProfileData BroadswordDefaults => new WeaponProfileData(
            "broadsword",
            minimumDamage: 1,
            maximumDamage: 18,
            minimumFullReactionVelocity: 1.75f,
            maximumDamageVelocity: 8.0f,
            duplicateHitWindowSeconds: 0.35f,
            regionDamage: RegionDamageTable.BroadswordDefaults);
    }
}
