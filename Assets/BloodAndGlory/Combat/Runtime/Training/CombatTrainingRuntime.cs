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

        private void OnEnable()
        {
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

        private void OnPlayerHitProposed(HitProposal hit)
        {
            if (playerSwordProfile == null || combatState == null)
                return;

            var profile = playerSwordProfile.ToData();
            var result = resolver.ResolveHit(combatState, hit, profile, new BlockContext(false, false));
            var suppressedDuplicate = result.Event.Type == CombatEventType.SuppressedDuplicate;

            debugOverlay?.RecordEvent(result.Event, suppressedDuplicate);

            if (!suppressedDuplicate)
                combatState = combatState.WithHealth(result.Health).RecordHit(hit, profile);

            if (result.Event.Type == CombatEventType.Died)
                enemyController?.Kill();
        }
    }
}
