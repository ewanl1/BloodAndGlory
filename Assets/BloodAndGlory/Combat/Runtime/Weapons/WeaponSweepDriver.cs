using System;
using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Weapons
{
    [RequireComponent(typeof(WeaponMarkerSet))]
    public sealed class WeaponSweepDriver : MonoBehaviour
    {
        [SerializeField] private int attackerId = 1;
        [SerializeField] private string weaponId = "broadsword";
        [SerializeField] private float sweepRadius = 0.08f;
        [SerializeField] private LayerMask hurtboxLayers = ~0;

        private WeaponMarkerSet markerSet;
        private Vector3[] previousPositions = Array.Empty<Vector3>();
        private bool hasPreviousPositions;

        public event Action<HitProposal> HitProposed;

        private void Awake()
        {
            markerSet = GetComponent<WeaponMarkerSet>();
            previousPositions = new Vector3[markerSet.Count];
        }

        private void FixedUpdate()
        {
            EnsurePreviousBuffer();

            if (!hasPreviousPositions)
            {
                CapturePreviousPositions();
                hasPreviousPositions = true;
                return;
            }

            for (var i = 0; i < markerSet.Count; i++)
            {
                var marker = markerSet.GetMarker(i);
                var current = marker.position;
                var previous = previousPositions[i];
                var delta = current - previous;
                var distance = delta.magnitude;

                if (distance > 0.0001f)
                    Sweep(previous, delta.normalized, distance, distance / Time.fixedDeltaTime);

                previousPositions[i] = current;
            }
        }

        public void ConfigureForTests(int attackerId, string weaponId, WeaponMarkerSet markers)
        {
            this.attackerId = attackerId;
            this.weaponId = weaponId;
            markerSet = markers;
            previousPositions = new Vector3[markerSet.Count];
        }

        private void Sweep(Vector3 origin, Vector3 direction, float distance, float velocity)
        {
            var hits = Physics.SphereCastAll(origin, sweepRadius, direction, distance, hurtboxLayers, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                var hurtbox = hit.collider.GetComponentInParent<HurtboxAuthoring>();
                if (hurtbox == null || hurtbox.Owner == null)
                    continue;

                HitProposed?.Invoke(new HitProposal(
                    attackerId,
                    hurtbox.Owner.CombatantId,
                    weaponId,
                    hurtbox.Region,
                    velocity,
                    Time.time,
                    hurtbox.Owner.IsPlayer));
            }
        }

        private void EnsurePreviousBuffer()
        {
            if (markerSet == null)
                markerSet = GetComponent<WeaponMarkerSet>();

            if (previousPositions.Length != markerSet.Count)
                previousPositions = new Vector3[markerSet.Count];
        }

        private void CapturePreviousPositions()
        {
            for (var i = 0; i < markerSet.Count; i++)
                previousPositions[i] = markerSet.GetMarker(i).position;
        }
    }
}
