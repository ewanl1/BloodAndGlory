using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    [CreateAssetMenu(menuName = "Blood And Glory/Combat/Attack Definition", fileName = "AttackDefinition")]
    public sealed class AttackDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id = "peasant_broadsword_attack_01";
        [SerializeField] private AnimationClip animationClip;
        [SerializeField] private float activeStartSeconds = 0.25f;
        [SerializeField] private float activeEndSeconds = 0.55f;
        [SerializeField] private AttackMovementPolicy movementPolicy = AttackMovementPolicy.None;

        public AnimationClip AnimationClip => animationClip;

        public AttackDefinitionData ToData()
        {
            return new AttackDefinitionData(id, activeStartSeconds, activeEndSeconds, movementPolicy);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = "peasant_broadsword_attack_01";
            activeStartSeconds = Mathf.Max(0f, activeStartSeconds);
            activeEndSeconds = Mathf.Max(activeStartSeconds + 0.01f, activeEndSeconds);
            movementPolicy = AttackMovementPolicy.None;
        }
    }
}
