using System.Collections;
using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("Joint")]
    public HingeJoint joint;
    public float closedAngle = 0f;
    public float openAngle = 100f;

    [Header("Spring/Damper (base)")]
    public float spring = 90f;
    public float damper = 50f;

    [Header("Motion")]
    [Tooltip("Smooth close speed, degrees per second.")]
    public float closeSpeedDegPerSec = 60f;

    [Header("Stutter Open Settings")]
    public bool stutterOnOpen = true;
    [Tooltip("Degrees advanced per stutter step.")]
    public Vector2 stepDegreesRange = new Vector2(6f, 14f);
    [Tooltip("Pause between steps (seconds).")]
    public Vector2 pauseRange = new Vector2(0.06f, 0.16f);
    [Tooltip("Optional tiny backward slip after a step (degrees).")]
    public float backJitterDegrees = 0.5f;
    [Tooltip("Chance [0..1] to apply back slip on a step.")]
    [Range(0f, 1f)] public float backJitterChance = 0.45f;

    [Header("Damping During Move")]
    [Tooltip("Multiply damper while gate is moving (adds heft)")]
    public float movingDamperMultiplier = 1.25f;

    Coroutine _move;
    float _eps = 0.25f;

    void Awake()
    {
        if (!joint) joint = GetComponent<HingeJoint>();
        ApplySpringDamper(spring, damper, Mathf.Clamp(GetTarget(), -360f, 360f));
    }

    public void Open()
    {
        StartMove(stutterOnOpen ? StutterTo(openAngle) : SmoothTo(openAngle, speedDegPerSec: closeSpeedDegPerSec * 0.6f));
    }

    public void Close()
    {
        StartMove(SmoothTo(closedAngle, speedDegPerSec: closeSpeedDegPerSec));
    }

    // ——— Internals ———

    void StartMove(IEnumerator routine)
    {
        if (_move != null) StopCoroutine(_move);
        _move = StartCoroutine(routine);
    }

    IEnumerator StutterTo(float targetAngle)
    {
        float baseSpring = spring, baseDamper = damper;
        ApplySpringDamper(baseSpring, baseDamper * movingDamperMultiplier, GetTarget());

        float t = GetTarget();
        while (targetAngle - t > _eps)
        {
            // Step forward a random amount
            float step = Random.Range(stepDegreesRange.x, stepDegreesRange.y);
            t = Mathf.Min(targetAngle, t + step);
            SetTarget(t);

            // Optional tiny backward slip to feel mechanical
            if (backJitterDegrees > 0f && Random.value < backJitterChance)
            {
                float slip = Mathf.Min(backJitterDegrees, Mathf.Max(0f, t - closedAngle));
                if (slip > 0f)
                {
                    SetTarget(t - slip);
                    yield return new WaitForSeconds(Random.Range(0.02f, 0.05f));
                    SetTarget(t);
                }
            }

            // Pause between stutters
            yield return new WaitForSeconds(Random.Range(pauseRange.x, pauseRange.y));
        }

        ApplySpringDamper(baseSpring, baseDamper, GetTarget());
        _move = null;
    }

    IEnumerator SmoothTo(float targetAngle, float speedDegPerSec)
    {
        float baseSpring = spring, baseDamper = damper;
        ApplySpringDamper(baseSpring, baseDamper * movingDamperMultiplier, GetTarget());

        float current = GetTarget();
        while (Mathf.Abs(targetAngle - current) > _eps)
        {
            float step = speedDegPerSec * Time.deltaTime * Mathf.Sign(targetAngle - current);
            if (Mathf.Abs(step) > Mathf.Abs(targetAngle - current)) step = targetAngle - current;
            current += step;
            SetTarget(current);
            yield return null; // next frame
        }

        ApplySpringDamper(baseSpring, baseDamper, GetTarget());
        _move = null;
    }

    // ——— Joint helpers ———
    void SetTarget(float angle)
    {
        var js = joint.spring;
        js.targetPosition = angle;
        joint.spring = js; // reassign to apply
    }
    float GetTarget() => joint.spring.targetPosition;

    void ApplySpringDamper(float s, float d, float keepTarget)
    {
        joint.useSpring = true;
        var js = joint.spring;
        js.spring = s;
        js.damper = d;
        js.targetPosition = keepTarget;
        joint.spring = js;
    }
}
