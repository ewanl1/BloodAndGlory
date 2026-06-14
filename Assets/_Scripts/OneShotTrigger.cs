using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class OneShotTrigger : MonoBehaviour
{
    [Tooltip("Tag on your XR rig root / Rigidbody / collider.")]
    public string playerTag = "Player";

    [Tooltip("Disable this trigger's collider after it fires once.")]
    public bool disableColliderOnUse = true;

    [Tooltip("Optional cool-down guard in seconds before this can fire (ignored if 'once').")]
    public float cooldown = 0f;

    [Header("Events")]
    public UnityEvent onFirstEnter;

    bool _used;
    float _lastFireTime = -999f;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Stabilizes CC interactions (separates this from moving bodies)
        var rb = GetComponent<Rigidbody>();
        if (!rb) { rb = gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false; }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_used) return;
        if (!IsPlayer(other)) return;
        if (cooldown > 0f && (Time.time - _lastFireTime) < cooldown) return;

        _lastFireTime = Time.time;
        _used = true;

        onFirstEnter?.Invoke();

        if (disableColliderOnUse)
            GetComponent<Collider>().enabled = false;
        // else Destroy(this); // if you prefer removing the script instead
    }

    bool IsPlayer(Collider c)
    {
        if (c.CompareTag(playerTag)) return true;
        if (c.attachedRigidbody && c.attachedRigidbody.CompareTag(playerTag)) return true;
        return c.transform.root && c.transform.root.CompareTag(playerTag);
    }
}
