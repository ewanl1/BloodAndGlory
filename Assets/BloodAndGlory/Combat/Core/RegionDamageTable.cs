using System;

namespace BloodAndGlory.Combat.Core
{
    public readonly struct RegionDamageTable
    {
        public RegionDamageTable(float head, float torso, float arm, float leg)
        {
            Head = Validate(head, nameof(head));
            Torso = Validate(torso, nameof(torso));
            Arm = Validate(arm, nameof(arm));
            Leg = Validate(leg, nameof(leg));
        }

        public float Head { get; }
        public float Torso { get; }
        public float Arm { get; }
        public float Leg { get; }

        public static RegionDamageTable BroadswordDefaults => new RegionDamageTable(1.5f, 1.0f, 0.75f, 0.75f);

        public float MultiplierFor(HurtboxRegion region)
        {
            return region switch
            {
                HurtboxRegion.Head => Head,
                HurtboxRegion.Torso => Torso,
                HurtboxRegion.Arm => Arm,
                HurtboxRegion.Leg => Leg,
                _ => 1.0f
            };
        }

        private static float Validate(float value, string name)
        {
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(name, "Region multiplier must be greater than zero.");

            return value;
        }
    }
}
