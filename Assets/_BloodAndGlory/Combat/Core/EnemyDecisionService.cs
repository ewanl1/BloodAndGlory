namespace BloodAndGlory.Combat.Core
{
    public sealed class EnemyDecisionService
    {
        public EnemyCombatState Decide(
            EnemyCombatState current,
            EnemyProfileData profile,
            float distanceToTarget,
            float timeInState,
            float randomValue)
        {
            return current switch
            {
                EnemyCombatState.Idle => distanceToTarget > profile.PreferredAttackDistance
                    ? EnemyCombatState.Approach
                    : EnemyCombatState.Telegraph,
                EnemyCombatState.Approach => distanceToTarget <= profile.PreferredAttackDistance
                    ? EnemyCombatState.Telegraph
                    : EnemyCombatState.Approach,
                EnemyCombatState.Telegraph => timeInState >= profile.TelegraphSeconds
                    ? EnemyCombatState.AttackCommit
                    : EnemyCombatState.Telegraph,
                EnemyCombatState.AttackCommit => timeInState >= profile.AttackCommitSeconds
                    ? EnemyCombatState.Recover
                    : EnemyCombatState.AttackCommit,
                EnemyCombatState.Recover => timeInState >= profile.RecoverSeconds
                    ? EnemyCombatState.Approach
                    : EnemyCombatState.Recover,
                EnemyCombatState.Block => timeInState >= profile.RecoverSeconds
                    ? EnemyCombatState.Recover
                    : EnemyCombatState.Block,
                EnemyCombatState.Parry => timeInState >= profile.RecoverSeconds
                    ? EnemyCombatState.Recover
                    : EnemyCombatState.Parry,
                EnemyCombatState.Stagger => timeInState >= profile.RecoverSeconds
                    ? EnemyCombatState.Recover
                    : EnemyCombatState.Stagger,
                EnemyCombatState.Dead => EnemyCombatState.Dead,
                _ => EnemyCombatState.Idle
            };
        }

        public EnemyCombatState DecideDefense(EnemyProfileData profile, float randomValue)
        {
            if (randomValue <= profile.ParryChance)
                return EnemyCombatState.Parry;

            if (randomValue <= profile.ParryChance + profile.BlockChance)
                return EnemyCombatState.Block;

            return EnemyCombatState.Recover;
        }
    }
}
