using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    public sealed class CombatantAuthoring : MonoBehaviour
    {
        [SerializeField] private int combatantId = 1;
        [SerializeField] private int maxHitPoints = 100;
        [SerializeField] private bool isPlayer;
        [SerializeField] private DeathMode deathMode = DeathMode.AnimatedDeath;

        public int CombatantId => combatantId;
        public bool IsPlayer => isPlayer;
        public DeathMode DeathMode => deathMode;
        public HealthState InitialHealth => new HealthState(maxHitPoints);

        public void ConfigureForTests(int combatantId, bool isPlayer, int maxHitPoints)
        {
            this.combatantId = Mathf.Max(1, combatantId);
            this.isPlayer = isPlayer;
            this.maxHitPoints = Mathf.Max(1, maxHitPoints);
        }

        private void OnValidate()
        {
            combatantId = Mathf.Max(1, combatantId);
            maxHitPoints = Mathf.Max(1, maxHitPoints);
        }
    }
}
