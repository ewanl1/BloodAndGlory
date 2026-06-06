using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    [CreateAssetMenu(menuName = "Blood And Glory/Combat/Enemy Profile", fileName = "EnemyProfile")]
    public sealed class EnemyProfileAsset : ScriptableObject
    {
        [SerializeField] private float preferredAttackDistance = 1.6f;
        [SerializeField] private float telegraphSeconds = 0.65f;
        [SerializeField] private float attackCommitSeconds = 0.75f;
        [SerializeField] private float recoverSeconds = 0.9f;
        [SerializeField] private float blockChance = 0.08f;
        [SerializeField] private float parryChance = 0.03f;

        public EnemyProfileData ToData()
        {
            return new EnemyProfileData(
                preferredAttackDistance,
                telegraphSeconds,
                attackCommitSeconds,
                recoverSeconds,
                blockChance,
                parryChance);
        }

        private void OnValidate()
        {
            preferredAttackDistance = Mathf.Max(0.1f, preferredAttackDistance);
            telegraphSeconds = Mathf.Max(0.01f, telegraphSeconds);
            attackCommitSeconds = Mathf.Max(0.01f, attackCommitSeconds);
            recoverSeconds = Mathf.Max(0.01f, recoverSeconds);
            blockChance = Mathf.Clamp01(blockChance);
            parryChance = Mathf.Clamp01(parryChance);
        }
    }
}
