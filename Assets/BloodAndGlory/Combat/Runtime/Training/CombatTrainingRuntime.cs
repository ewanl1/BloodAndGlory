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
        [SerializeField] private float playerBlockDotThreshold = 0.45f;
        [SerializeField] private float playerBlockDistance = 1.25f;

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

            if (enemyController != null)
                enemyController.AttackProposed += OnEnemyAttackProposed;
        }

        private void OnDisable()
        {
            if (playerSword != null)
                playerSword.HitProposed -= OnPlayerHitProposed;

            if (enemyController != null)
                enemyController.AttackProposed -= OnEnemyAttackProposed;
        }

        private void Update()
        {
            if (enemyController != null)
                debugOverlay?.SetActiveAttack(enemyController.IsAttackActive ? "Peasant Broadsword" : "None");
        }

        public static DamageResult ResolvePlayerHitForTests(
            CombatState state,
            HitProposal hit,
            WeaponProfileData profile,
            BlockContext block)
        {
            return new CombatResolver().ResolveHit(state, hit, profile, block);
        }

        public static DamageResult ResolveEnemyHitForTests(
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

        public DamageResult ResolveEnemyHitForRuntimeTests(HitProposal hit, BlockContext block)
        {
            return ResolveEnemyHit(hit, block);
        }

        private void OnPlayerHitProposed(HitProposal hit)
        {
            ResolvePlayerHit(hit, new BlockContext(false, false));
        }

        private void OnEnemyAttackProposed(HitProposal hit)
        {
            ResolveEnemyHit(hit, GetPlayerBlockContext());
        }

        private DamageResult ResolveEnemyHit(HitProposal hit, BlockContext block)
        {
            if (!hasTestProfile && playerSwordProfile == null)
                return default;

            var profile = hasTestProfile ? testProfile : playerSwordProfile.ToData();
            var result = resolver.ResolveHit(new CombatState(new HealthState(100)), hit, profile, block);
            debugOverlay?.RecordEvent(result.Event, result.Event.Type == CombatEventType.SuppressedDuplicate);
            return result;
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

        private BlockContext GetPlayerBlockContext()
        {
            if (playerSword == null || enemyController == null)
                return new BlockContext(false, false);

            var swordPosition = playerSword.transform.position;
            var enemyPosition = enemyController.transform.position;
            if (Vector3.Distance(swordPosition, enemyPosition) > playerBlockDistance)
                return new BlockContext(false, false);

            var cameraTransform = Camera.main == null ? null : Camera.main.transform;
            var playerPosition = cameraTransform == null ? swordPosition : cameraTransform.position;
            var enemyToPlayer = playerPosition - enemyPosition;
            var enemyToSword = swordPosition - enemyPosition;

            if (enemyToPlayer.sqrMagnitude <= 0.0001f || enemyToSword.sqrMagnitude <= 0.0001f)
                return new BlockContext(false, false);

            var dot = Vector3.Dot(enemyToPlayer.normalized, enemyToSword.normalized);
            return new BlockContext(dot >= playerBlockDotThreshold, false);
        }
    }
}
