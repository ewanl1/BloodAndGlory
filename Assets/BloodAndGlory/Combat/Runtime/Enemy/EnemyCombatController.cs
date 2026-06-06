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
        private EnemyCombatState state = EnemyCombatState.Idle;
        private float stateEnteredAt;

        public EnemyCombatState State => state;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            agent.updateRotation = true;
        }

        private void Update()
        {
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

        public void Kill()
        {
            EnterState(EnemyCombatState.Dead);
        }

        private void EnterState(EnemyCombatState next)
        {
            state = next;
            stateEnteredAt = Time.time;
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
                case EnemyCombatState.AttackCommit:
                case EnemyCombatState.Recover:
                case EnemyCombatState.Block:
                case EnemyCombatState.Parry:
                case EnemyCombatState.Stagger:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    break;
                case EnemyCombatState.Dead:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    animator.SetBool("Dead", true);
                    enabled = false;
                    break;
                default:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    break;
            }
        }
    }
}
