using System;

namespace BloodAndGlory.Combat.Core
{
    public readonly struct EnemyProfileData
    {
        public EnemyProfileData(
            float preferredAttackDistance,
            float telegraphSeconds,
            float attackCommitSeconds,
            float recoverSeconds,
            float blockChance,
            float parryChance)
        {
            if (preferredAttackDistance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(preferredAttackDistance));
            if (telegraphSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(telegraphSeconds));
            if (attackCommitSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(attackCommitSeconds));
            if (recoverSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(recoverSeconds));

            PreferredAttackDistance = preferredAttackDistance;
            TelegraphSeconds = telegraphSeconds;
            AttackCommitSeconds = attackCommitSeconds;
            RecoverSeconds = recoverSeconds;
            BlockChance = Math.Clamp(blockChance, 0f, 1f);
            ParryChance = Math.Clamp(parryChance, 0f, 1f);
        }

        public float PreferredAttackDistance { get; }
        public float TelegraphSeconds { get; }
        public float AttackCommitSeconds { get; }
        public float RecoverSeconds { get; }
        public float BlockChance { get; }
        public float ParryChance { get; }

        public static EnemyProfileData WeakPeasantDefaults => new EnemyProfileData(
            preferredAttackDistance: 1.6f,
            telegraphSeconds: 0.65f,
            attackCommitSeconds: 0.75f,
            recoverSeconds: 0.9f,
            blockChance: 0.08f,
            parryChance: 0.03f);
    }
}
