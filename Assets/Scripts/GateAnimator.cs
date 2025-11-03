using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GateAnimator : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Gate pivot to rotate (usually this component's transform).")]
    public Transform gateRoot;
    [Tooltip("Kinematic rigidbody on the gate pivot. CharacterController can't push kinematic bodies.")]
    public Rigidbody gateRb;

    [Header("Events")]
    public UnityEvent onOpen;
    public UnityEvent onClose;

    [Header("Angles (degrees)")]
    [Tooltip("Local-axis angle representing 'closed'. The gate starts closed at play.")]
    public float closedAngle = 0f;
    [Tooltip("Local-axis angle to rotate to when opened.")]
    public float openAngle = 100f;
    [Tooltip("Which local axis to rotate around.")]
    public Vector3 localAxis = Vector3.up;

    [Header("Timing")]
    [Tooltip("Seconds to fully open.")]
    public float openDuration = 1.5f;
    [Tooltip("Seconds to fully close (shorter to feel like a slam).")]
    public float closeDuration = 0.8f;

    [Header("Curves (0..1 time -> 0..1 progress)")]
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    // Slammy close: slow at start, fast finish
    public AnimationCurve closeCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0.1f),
        new Keyframe(0.6f, 0.35f, 1.5f, 1.5f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    Quaternion _closedRot, _openRot;
    Coroutine _anim;
    bool _isOpen;

    void Reset()
    {
        gateRoot = transform;
        gateRb = GetComponent<Rigidbody>();
        if (!gateRb) gateRb = gameObject.AddComponent<Rigidbody>();
        gateRb.isKinematic = true;
        gateRb.useGravity = false;
    }

    void Awake()
    {
        if (!gateRoot) gateRoot = transform;
        if (!gateRb) gateRb = GetComponent<Rigidbody>();
        if (!gateRb)
        {
            Debug.LogWarning("GateAnimator: No Rigidbody found. Adding kinematic RB is recommended for moving colliders.");
        }

        // Assume current rotation == closed; compute open from it.
        _closedRot = gateRoot.rotation;
        _openRot = _closedRot * Quaternion.AngleAxis(openAngle - closedAngle, gateRoot.TransformDirection(localAxis.normalized));
        _isOpen = false;

        if (gateRb) { gateRb.isKinematic = true; gateRb.useGravity = false; }
    }

    public void OpenOnce()
    {
        if (_isOpen) return;
        _isOpen = true;
        onOpen?.Invoke();
        StartAnim(_openRot, openDuration, openCurve);
    }

    public void CloseOnce()
    {
        if (!_isOpen) return;
        _isOpen = false;
        onClose?.Invoke();
        StartAnim(_closedRot, closeDuration, closeCurve);
    }

    void StartAnim(Quaternion target, float duration, AnimationCurve curve)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(RotateTo(target, duration, curve));
    }

    IEnumerator RotateTo(Quaternion target, float duration, AnimationCurve curve)
    {
        // Always drive with physics step; kinematic bodies won't get shoved by players.
        Quaternion start = gateRoot.rotation;
        float t = 0f;
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float a = curve.Evaluate(Mathf.Clamp01(t / duration));
            Quaternion q = Quaternion.Slerp(start, target, a);

            if (gateRb && gateRb.isKinematic) gateRb.MoveRotation(q);
            else gateRoot.rotation = q;

            yield return new WaitForFixedUpdate();
        }

        if (gateRb && gateRb.isKinematic) gateRb.MoveRotation(target);
        else gateRoot.rotation = target;

        _anim = null;
    }
}
