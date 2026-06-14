using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class VillagerPeasantWalkingController : MonoBehaviour
{
    [Header("Path Settings")]
    [Tooltip("Empty GameObjects marking the walk loop in order.")]
    public Transform[] waypoints;

    /// <summary>
    /// How quickly the character turns toward its next waypoint (degrees/sec).
    /// Matches your NavMeshAgent.angularSpeed by default.
    /// </summary>
    [Tooltip("Degrees per second to rotate toward the path.")]
    public float turnSpeed = 360f;

    [Tooltip("Pause duration after a loop is complete")]
    public float loopPauseDuration = 10f;

    // Internal
    private NavMeshAgent _agent;
    private Animator _anim;
    private int _currentIndex = 0;
    private bool _isWaiting = false;
    private float _waitEndTime = 0f;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();

        // Let the agent move you, but we'll handle facing manually:
        _agent.updatePosition = true;
        _agent.updateRotation = false;
    }

    void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
            _agent.SetDestination(waypoints[_currentIndex].position);
    }

    void Update()
    {
        // 1) Speed parameter (0…1)
        float speedPct = _agent.velocity.magnitude / _agent.speed;
        _anim.SetFloat("Speed", speedPct);

        // 2) Turn parameter (–1…+1) + apply rotation
        Vector3 desiredDir = (_agent.steeringTarget - transform.position).normalized;
        float turnPct = 0f;
        if (desiredDir.sqrMagnitude > 0.001f)
        {
            // Signed angle between forward and desiredDir
            float angle = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
            turnPct = Mathf.Clamp(angle / 90f, -1f, +1f);

            // Rotate smoothly toward that direction
            float step = turnSpeed * Time.deltaTime;
            float clamp = Mathf.Clamp(angle, -step, +step);
            transform.Rotate(0f, clamp, 0f);
        }
        _anim.SetFloat("Turn", turnPct);

        // 3) Waypoint arrival check
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (_isWaiting)
            {
                if (Time.time >= _waitEndTime)
                {
                    _isWaiting = false;
                    AdvanceToNextWaypoint();
                }
            }
            else
            {
                // We’ve just arrived at a waypoint
                // Check if this is the last in the loop
                if (_currentIndex == waypoints.Length - 1)
                {
                    // Start waiting
                    _isWaiting = true;
                    _waitEndTime = Time.time + loopPauseDuration;
                    // Animator will see Speed ~0 so Idle plays automatically
                }
                else
                {
                    // Immediately go to the next waypoint
                    AdvanceToNextWaypoint();
                }
            }
        }
    }

    private void AdvanceToNextWaypoint()
    {
        _currentIndex = (_currentIndex + 1) % waypoints.Length;
        _agent.SetDestination(waypoints[_currentIndex].position);
    }
}