using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OpenCloseGate : MonoBehaviour
{
    public enum ZoneType { Open, Close }
    [Header("Zone")]
    public ZoneType zone = ZoneType.Open;
    public string playerTag = "Player";

    [Header("Gate / Joint")]
    public HingeJoint gateJoint;
    public float closedAngle = 0f, openAngle = 100f;

    [Header("OPEN tuning")]
    [Tooltip("Spring stiffness when opening (lower = heavier/slower)")]
    public float openSpring = 90f;
    [Tooltip("Spring damping when opening (higher = less bounce, slower)")]
    public float openDamper = 50f;

    [Header("CLOSE tuning")]
    [Tooltip("Spring stiffness when closing (make this higher to snap shut)")]
    public float closeSpring = 160f;
    [Tooltip("Spring damping when closing")]
    public float closeDamper = 60f;

    [Header("Optional: 'Slam shut' burst")]
    public bool slamOnClose = true;
    [Tooltip("Degrees/sec for the brief motor push toward closed.")]
    public float slamVelocityDegPerSec = 280f;
    [Tooltip("Max motor force for the slam.")]
    public float slamForce = 1200f;
    [Tooltip("How long the motor push lasts (sec).")]
    public float slamDuration = 0.20f;

    Coroutine _slamCo;

    void Start()
    {
        if (!gateJoint)
        {
            Debug.LogWarning($"{name}: No HingeJoint assigned.");
            return;
        }
        gateJoint.useSpring = true;     // we primarily drive with spring/damper
        gateJoint.useMotor = false;    // motor only used briefly for slam
        ApplySpringDamper(openSpring, openDamper, GetTarget()); // initialize
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || !gateJoint) return;

        if (zone == ZoneType.Open)
        {
            if (_slamCo != null) 
            { 
                StopCoroutine(_slamCo); 
                _slamCo = null; 
                gateJoint.useMotor = false; 
            }

            ApplySpringDamper(openSpring, openDamper, openAngle);
        }
        else // ZoneType.Close
        {
            ApplySpringDamper(closeSpring, closeDamper, closedAngle);
            if (slamOnClose)
            {
                if (_slamCo != null) StopCoroutine(_slamCo);
                _slamCo = StartCoroutine(SlamBurstToward(closedAngle));
            }
        }
    }

    // --- helpers ---
    bool IsPlayer(Collider c) =>
        c.CompareTag(playerTag) ||
        (c.attachedRigidbody && c.attachedRigidbody.CompareTag(playerTag)) ||
        (c.transform.root && c.transform.root.CompareTag(playerTag));

    void ApplySpringDamper(float spring, float damper, float target)
    {
        var js = gateJoint.spring;
        js.spring = spring;
        js.damper = damper;
        js.targetPosition = target;
        gateJoint.spring = js; // reassign applies it
    }

    float GetTarget() => gateJoint.spring.targetPosition;

    IEnumerator SlamBurstToward(float targetAngle)
    {
        // Briefly enable the motor to add a “kick” toward the target, then turn it off.
        var m = gateJoint.motor;
        m.force = slamForce;

        // Determine sign to drive toward 'targetAngle'
        float dir = Mathf.Sign(Mathf.DeltaAngle(gateJoint.angle, targetAngle));
        m.targetVelocity = dir * Mathf.Abs(slamVelocityDegPerSec);

        gateJoint.motor = m;
        gateJoint.useMotor = true;
        yield return new WaitForSeconds(slamDuration);
        gateJoint.useMotor = false;
        _slamCo = null;
    }
}
