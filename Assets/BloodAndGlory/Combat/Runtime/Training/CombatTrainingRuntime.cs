using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using BloodAndGlory.Combat.Runtime.Debug;
using BloodAndGlory.Combat.Runtime.Enemy;
using BloodAndGlory.Combat.Runtime.Weapons;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Training
{
    public sealed class CombatTrainingRuntime : MonoBehaviour
    {
        [SerializeField] private WeaponSweepDriver playerSword;
        [SerializeField] private WeaponProfileAsset playerSwordProfile;
        [SerializeField] private CombatantAuthoring enemyCombatant;
        [SerializeField] private EnemyCombatController enemyController;
        [SerializeField] private CombatDebugOverlay debugOverlay;

        private readonly CombatResolver resolver = new CombatResolver();
        private CombatState combatState;
        private WeaponProfileData testProfile;
        private bool hasTestProfile;
        private bool enemyKilled;

        private void OnEnable()
        {
            combatState = null;
            enemyKilled = false;

            if (enemyCombatant != null)
                combatState = new CombatState(enemyCombatant.InitialHealth);

            if (playerSword != null)
                playerSword.HitProposed += OnPlayerHitProposed;
        }

        private void OnDisable()
        {
            if (playerSword != null)
                playerSword.HitProposed -= OnPlayerHitProposed;
        }

        public static DamageResult ResolvePlayerHitForTests(
            CombatState state,
            HitProposal hit,
            WeaponProfileData profile,
            BlockContext block)
        {
            return new CombatResolver().ResolveHit(state, hit, profile, block);
        }

        public void ConfigureForRuntimeTests(HealthState initialHealth, WeaponProfileData profile)
        {
            combatState = new CombatState(initialHealth);
            testProfile = profile;
            hasTestProfile = true;
            enemyKilled = false;
        }

        public HealthState CurrentHealthForRuntimeTests => combatState.Health;

        public DamageResult ResolvePlayerHitForRuntimeTests(HitProposal hit, BlockContext block)
        {
            return ResolvePlayerHit(hit, block);
        }

        private void OnPlayerHitProposed(HitProposal hit)
        {
            ResolvePlayerHit(hit, new BlockContext(false, false));
        }

        private DamageResult ResolvePlayerHit(HitProposal hit, BlockContext block)
        {
            if (combatState == null || (!hasTestProfile && playerSwordProfile == null))
                return default;

            var profile = hasTestProfile ? testProfile : playerSwordProfile.ToData();
            var result = resolver.ResolveHit(combatState, hit, profile, block);
            var suppressedDuplicate = result.Event.Type == CombatEventType.SuppressedDuplicate;

            debugOverlay?.RecordEvent(result.Event, suppressedDuplicate);

            if (result.Event.Damage > 0)
                combatState = combatState.WithHealth(result.Health).RecordHit(hit, profile);

            if (result.Event.Type == CombatEventType.Died && !enemyKilled)
            {
                enemyKilled = true;
                enemyController?.Kill();
            }

            return result;
        }
    }
}
