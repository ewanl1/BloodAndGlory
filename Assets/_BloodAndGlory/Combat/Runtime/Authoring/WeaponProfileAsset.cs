using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    [CreateAssetMenu(menuName = "Blood And Glory/Combat/Weapon Profile", fileName = "WeaponProfile")]
    public sealed class WeaponProfileAsset : ScriptableObject
    {
        [SerializeField] private string id = "broadsword";
        [SerializeField] private int minimumDamage = 1;
        [SerializeField] private int maximumDamage = 18;
        [SerializeField] private float minimumFullReactionVelocity = 1.75f;
        [SerializeField] private float maximumDamageVelocity = 8.0f;
        [SerializeField] private float duplicateHitWindowSeconds = 0.35f;
        [SerializeField] private float headMultiplier = 1.5f;
        [SerializeField] private float torsoMultiplier = 1.0f;
        [SerializeField] private float armMultiplier = 0.75f;
        [SerializeField] private float legMultiplier = 0.75f;

        public WeaponProfileData ToData()
        {
            return new WeaponProfileData(
                id,
                minimumDamage,
                maximumDamage,
                minimumFullReactionVelocity,
                maximumDamageVelocity,
                duplicateHitWindowSeconds,
                new RegionDamageTable(headMultiplier, torsoMultiplier, armMultiplier, legMultiplier));
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = "broadsword";
            minimumDamage = Mathf.Max(0, minimumDamage);
            maximumDamage = Mathf.Max(minimumDamage, maximumDamage);
            minimumFullReactionVelocity = Mathf.Max(0f, minimumFullReactionVelocity);
            maximumDamageVelocity = Mathf.Max(0.01f, maximumDamageVelocity);
            duplicateHitWindowSeconds = Mathf.Max(0.01f, duplicateHitWindowSeconds);
            headMultiplier = Mathf.Max(0.01f, headMultiplier);
            torsoMultiplier = Mathf.Max(0.01f, torsoMultiplier);
            armMultiplier = Mathf.Max(0.01f, armMultiplier);
            legMultiplier = Mathf.Max(0.01f, legMultiplier);
        }
    }
}
