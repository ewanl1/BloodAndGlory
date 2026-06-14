using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using UnityEngine;
using UnityEngine.AI;

namespace BloodAndGlory.Combat.Runtime.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(CombatantAuthoring))]
    public sealed class EnemyCombatController : MonoBehaviour
    {
        private const string PlayerSpawnTargetName = "Player Spawn";

        [SerializeField] private Transform target;
        [SerializeField] private EnemyProfileAsset enemyProfile;
        [SerializeField] private AttackDefinitionAsset attackDefinition;
        [SerializeField] private float fallbackApproachSpeed = 1.2f;
        [SerializeField] private float fallbackTurnDegreesPerSecond = 540f;

        private readonly EnemyDecisionService decisionService = new EnemyDecisionService();
        private NavMeshAgent agent;
        private Animator animator;
        private CombatantAuthoring combatant;
        private EnemyCombatState state = EnemyCombatState.Idle;
        private float stateEnteredAt;
        private bool attackProposalSent;
        private string movementMode = "None";

        public EnemyCombatState State => state;
        public bool IsAttackActive => state == EnemyCombatState.AttackCommit;
        public float DistanceToTarget { get; private set; } = float.PositiveInfinity;
        public string MovementMode => movementMode;
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

            var currentTarget = ResolveTarget();
            if (currentTarget == null || enemyProfile == null)
            {
                DistanceToTarget = float.PositiveInfinity;
                movementMode = "None";
                return;
            }

            var profile = enemyProfile.ToData();
            DistanceToTarget = HorizontalDistance(transform.position, currentTarget.position);
            var next = decisionService.Decide(state, profile, DistanceToTarget, Time.time - stateEnteredAt, Random.value);
            if (next != state)
                EnterState(next);

            ApplyState(profile, currentTarget);
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

        public static float HorizontalDistanceForTests(Vector3 a, Vector3 b)
        {
            return HorizontalDistance(a, b);
        }

        public static Vector3 CalculateFallbackStepForTests(
            Vector3 currentPosition,
            Vector3 targetPosition,
            float speed,
            float deltaTime,
            float stopDistance)
        {
            return CalculateFallbackStep(currentPosition, targetPosition, speed, deltaTime, stopDistance);
        }

        public static bool ShouldPreferRuntimeCameraTargetForTests(Transform assignedTarget, Camera runtimeCamera)
        {
            return ShouldPreferRuntimeCameraTarget(assignedTarget, runtimeCamera);
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

        private void ApplyState(EnemyProfileData profile, Transform currentTarget)
        {
            switch (state)
            {
                case EnemyCombatState.Approach:
                    if (CanUseNavMeshAgent())
                    {
                        movementMode = "NavMesh";
                        agent.isStopped = false;
                        agent.stoppingDistance = profile.PreferredAttackDistance;
                        agent.SetDestination(currentTarget.position);
                        animator.SetFloat("Speed", agent.velocity.magnitude);
                    }
                    else
                    {
                        movementMode = "Fallback";
                        MoveWithoutNavMesh(currentTarget.position, profile.PreferredAttackDistance);
                    }

                    break;
                case EnemyCombatState.Telegraph:
                case EnemyCombatState.Recover:
                case EnemyCombatState.Block:
                case EnemyCombatState.Parry:
                case EnemyCombatState.Stagger:
                    movementMode = "None";
                    StopAgentIfUsable();
                    FaceTarget(currentTarget.position);
                    animator.SetFloat("Speed", 0f);
                    break;
                case EnemyCombatState.AttackCommit:
                    movementMode = "None";
                    StopAgentIfUsable();
                    FaceTarget(currentTarget.position);
                    animator.SetFloat("Speed", 0f);
                    TryProposeAttack();
                    break;
                case EnemyCombatState.Dead:
                    ApplyDeadState();
                    break;
                default:
                    movementMode = "None";
                    StopAgentIfUsable();
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

        private Transform ResolveTarget()
        {
            var runtimeCamera = ResolveRuntimeCamera();
            if (ShouldPreferRuntimeCameraTarget(target, runtimeCamera))
                return runtimeCamera.transform;

            return target;
        }

        private static Camera ResolveRuntimeCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
                return mainCamera;

            return Object.FindFirstObjectByType<Camera>();
        }

        private static bool ShouldPreferRuntimeCameraTarget(Transform assignedTarget, Camera runtimeCamera)
        {
            if (runtimeCamera == null)
                return false;

            return assignedTarget == null || assignedTarget.name == PlayerSpawnTargetName;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private static Vector3 CalculateFallbackStep(
            Vector3 currentPosition,
            Vector3 targetPosition,
            float speed,
            float deltaTime,
            float stopDistance)
        {
            var flatTarget = new Vector3(targetPosition.x, currentPosition.y, targetPosition.z);
            if (Vector3.Distance(currentPosition, flatTarget) <= stopDistance)
                return currentPosition;

            return Vector3.MoveTowards(currentPosition, flatTarget, speed * deltaTime);
        }

        private bool CanUseNavMeshAgent()
        {
            return agent != null && agent.enabled && agent.isOnNavMesh;
        }

        private void StopAgentIfUsable()
        {
            if (CanUseNavMeshAgent())
                agent.isStopped = true;
        }

        private void MoveWithoutNavMesh(Vector3 targetPosition, float stopDistance)
        {
            var nextPosition = CalculateFallbackStep(transform.position, targetPosition, fallbackApproachSpeed, Time.deltaTime, stopDistance);
            var moved = nextPosition != transform.position;
            transform.position = nextPosition;
            FaceTarget(targetPosition);
            animator.SetFloat("Speed", moved ? fallbackApproachSpeed : 0f);
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            var direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                fallbackTurnDegreesPerSecond * Time.deltaTime);
        }

        private void ApplyDeadState()
        {
            movementMode = "None";
            StopAgentIfUsable();

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("Dead", true);
            }

            enabled = false;
        }
    }
}
