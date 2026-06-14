using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    [RequireComponent(typeof(Collider))]
    public sealed class HurtboxAuthoring : MonoBehaviour
    {
        [SerializeField] private CombatantAuthoring owner;
        [SerializeField] private HurtboxRegion region = HurtboxRegion.Torso;

        public CombatantAuthoring Owner => owner;
        public HurtboxRegion Region => region;

        public void ConfigureForTests(CombatantAuthoring owner, HurtboxRegion region)
        {
            this.owner = owner;
            this.region = region;
            GetComponent<Collider>().isTrigger = true;
        }

        private void Reset()
        {
            owner = GetComponentInParent<CombatantAuthoring>();
            var colliderComponent = GetComponent<Collider>();
            colliderComponent.isTrigger = true;
        }

        private void OnValidate()
        {
            if (owner == null)
                owner = GetComponentInParent<CombatantAuthoring>();
        }
    }
}
