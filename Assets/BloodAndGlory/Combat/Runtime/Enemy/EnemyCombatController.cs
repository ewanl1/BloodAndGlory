using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using UnityEngine;
using UnityEngine.AI;

namespace BloodAndGlory.Combat.Runtime.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(CombatantAuthoring))]
    public sealed class EnemyCombatController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private EnemyProfileAsset enemyProfile;
        [SerializeField] private AttackDefinitionAsset attackDefinition;

        private readonly EnemyDecisionService decisionService = new EnemyDecisionService();
        private NavMeshAgent agent;
        private Animator animator;
        private CombatantAuthoring combatant;
        private EnemyCombatState state = EnemyCombatState.Idle;
        private float stateEnteredAt;
        private bool attackProposalSent;

        public EnemyCombatState State => state;
        public bool IsAttackActive => state == EnemyCombatState.AttackCommit;
        public float TimeInState => Time.time - stateEnteredAt;
        public event System.Action<HitProposal> AttackProposed;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            combatant = GetComponent<CombatantAuthoring>();
            agent.updateRotation = true;
        }

        private void Update()
        {
            if (state == EnemyCombatState.Dead)
            {
                ApplyDeadState();
                return;
            }

            if (target == null || enemyProfile == null)
                return;

            var profile = enemyProfile.ToData();
            var distance = Vector3.Distance(transform.position, target.position);
            var next = decisionService.Decide(state, profile, distance, Time.time - stateEnteredAt, Random.value);
            if (next != state)
                EnterState(next);

            ApplyState(profile);
        }

        public void ConfigureForTests(Transform target, EnemyProfileAsset profile)
        {
            this.target = target;
            enemyProfile = profile;
        }

        public HitProposal CreateAttackProposalForTests(float timeSeconds)
        {
            return CreateAttackProposal(timeSeconds);
        }

        public void Kill()
        {
            EnterState(EnemyCombatState.Dead);
            ApplyDeadState();
        }

        private void EnterState(EnemyCombatState next)
        {
            state = next;
            stateEnteredAt = Time.time;
            attackProposalSent = state != EnemyCombatState.AttackCommit;
            if (animator != null)
                animator.SetInteger("CombatState", (int)state);
        }

        private void ApplyState(EnemyProfileData profile)
        {
            switch (state)
            {
                case EnemyCombatState.Approach:
                    agent.isStopped = false;
                    agent.stoppingDistance = profile.PreferredAttackDistance;
                    agent.SetDestination(target.position);
                    animator.SetFloat("Speed", agent.velocity.magnitude);
                    break;
                case EnemyCombatState.Telegraph:
                case EnemyCombatState.Recover:
                case EnemyCombatState.Block:
                case EnemyCombatState.Parry:
                case EnemyCombatState.Stagger:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    break;
                case EnemyCombatState.AttackCommit:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    TryProposeAttack();
                    break;
                case EnemyCombatState.Dead:
                    ApplyDeadState();
                    break;
                default:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    break;
            }
        }

        private void TryProposeAttack()
        {
            if (attackProposalSent)
                return;

            if (attackDefinition != null)
            {
                var attack = attackDefinition.ToData();
                if (!attack.IsActive(Time.time - stateEnteredAt))
                    return;
            }

            attackProposalSent = true;
            AttackProposed?.Invoke(CreateAttackProposal(Time.time));
        }

        private HitProposal CreateAttackProposal(float timeSeconds)
        {
            var authoring = combatant == null ? GetComponent<CombatantAuthoring>() : combatant;
            var attackerId = authoring == null ? 10 : authoring.CombatantId;
            return new HitProposal(
                attackerId,
                1,
                "broadsword",
                HurtboxRegion.Torso,
                4f,
                timeSeconds,
                true);
        }

        private void ApplyDeadState()
        {
            if (agent != null)
                agent.isStopped = true;

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("Dead", true);
            }

            enabled = false;
        }
    }
}
