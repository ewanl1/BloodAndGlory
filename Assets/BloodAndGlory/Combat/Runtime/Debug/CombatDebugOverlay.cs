using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Enemy;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Debug
{
    public sealed class CombatDebugOverlay : MonoBehaviour
    {
        [SerializeField] private EnemyCombatController enemy;
        [SerializeField] private bool visible = true;
        [SerializeField] private TextMesh worldText;

        private CombatEvent? lastEvent;
        private float lastVelocity;
        private string duplicateStatus = "None";

        public void RecordEvent(CombatEvent combatEvent, bool duplicateSuppressed)
        {
            lastEvent = combatEvent;
            lastVelocity = combatEvent.ImpactVelocity;
            duplicateStatus = duplicateSuppressed ? "Suppressed" : "Accepted";
        }

        private string BuildText()
        {
            var eventName = lastEvent.HasValue ? lastEvent.Value.Type.ToString() : "None";
            var region = lastEvent.HasValue ? lastEvent.Value.Region.ToString() : "None";
            var damage = lastEvent.HasValue ? lastEvent.Value.Damage.ToString() : "0";

            return
                "Blood and Glory Combat Debug\n" +
                $"Enemy State: {(enemy == null ? "None" : enemy.State.ToString())}\n" +
                $"Last Velocity: {lastVelocity:0.00}\n" +
                $"Duplicate: {duplicateStatus}\n" +
                $"Event: {eventName}\n" +
                $"Region: {region}\n" +
                $"Damage: {damage}";
        }

        private void LateUpdate()
        {
            if (worldText != null)
                worldText.text = BuildText();
        }

        private void OnGUI()
        {
            if (!visible)
                return;

            GUILayout.BeginArea(new Rect(16, 16, 360, 240), GUI.skin.box);
            foreach (var line in BuildText().Split('\n'))
                GUILayout.Label(line);

            GUILayout.EndArea();
        }
    }
}
