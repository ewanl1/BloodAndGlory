using BloodAndGlory.Combat.Runtime.Authoring;
using UnityEngine;

namespace BloodAndGlory.Combat.Editor
{
    public static class CombatPrefabValidator
    {
        public readonly struct ValidationResult
        {
            public ValidationResult(bool isValid, string message)
            {
                IsValid = isValid;
                Message = message;
            }

            public bool IsValid { get; }
            public string Message { get; }
        }

        public static ValidationResult Validate(GameObject root)
        {
            if (root == null)
                return new ValidationResult(false, "Root GameObject is required.");

            var combatant = root.GetComponentInChildren<CombatantAuthoring>();
            if (combatant == null)
                return new ValidationResult(false, "CombatantAuthoring is required.");

            var hurtboxes = root.GetComponentsInChildren<HurtboxAuthoring>(includeInactive: true);
            if (hurtboxes.Length == 0)
                return new ValidationResult(false, "At least one hurtbox is required.");

            foreach (var hurtbox in hurtboxes)
            {
                if (hurtbox.Owner == null)
                    return new ValidationResult(false, "Every hurtbox must reference an owner.");
            }

            return new ValidationResult(true, "Combat prefab is valid.");
        }
    }
}
