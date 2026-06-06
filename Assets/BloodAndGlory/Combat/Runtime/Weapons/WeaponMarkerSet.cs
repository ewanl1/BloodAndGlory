using System;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Weapons
{
    public sealed class WeaponMarkerSet : MonoBehaviour
    {
        [SerializeField] private Transform[] markers = Array.Empty<Transform>();

        public int Count => markers.Length;

        public Transform GetMarker(int index)
        {
            return markers[index];
        }

        public void ConfigureForTests(Transform[] testMarkers)
        {
            markers = testMarkers ?? Array.Empty<Transform>();
        }
    }
}
