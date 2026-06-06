using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Enemy;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Debug
{
    public sealed class CombatDebugOverlay : MonoBehaviour
    {
        [SerializeField] private EnemyCombatController enemy;
        [SerializeField] private bool visible = true;

        private CombatEvent? lastEvent;
        private float lastVelocity;
        private string duplicateStatus = "None";

        public void RecordEvent(CombatEvent combatEvent, bool duplicateSuppressed)
        {
            lastEvent = combatEvent;
            lastVelocity = combatEvent.ImpactVelocity;
            duplicateStatus = duplicateSuppressed ? "Suppressed" : "Accepted";
        }

        private void OnGUI()
        {
            if (!visible)
                return;

            GUILayout.BeginArea(new Rect(16, 16, 360, 240), GUI.skin.box);
            GUILayout.Label("Blood and Glory Combat Debug");
            GUILayout.Label($"Enemy State: {(enemy == null ? "None" : enemy.State.ToString())}");
            GUILayout.Label($"Last Velocity: {lastVelocity:0.00}");
            GUILayout.Label($"Duplicate: {duplicateStatus}");

            if (lastEvent.HasValue)
            {
                var combatEvent = lastEvent.Value;
                GUILayout.Label($"Event: {combatEvent.Type}");
                GUILayout.Label($"Region: {combatEvent.Region}");
                GUILayout.Label($"Damage: {combatEvent.Damage}");
            }
            else
            {
                GUILayout.Label("Event: None");
                GUILayout.Label("Region: None");
                GUILayout.Label("Damage: 0");
            }

            GUILayout.EndArea();
        }
    }
}
