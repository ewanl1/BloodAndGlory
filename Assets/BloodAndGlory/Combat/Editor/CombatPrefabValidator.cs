using BloodAndGlory.Combat.Runtime.Authoring;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

            if (root.name == "CombatTrainingScene")
            {
                if (root.transform.Find("XR Combat Rig") == null)
                    return new ValidationResult(false, "CombatTrainingScene must contain XR Combat Rig.");

                return new ValidationResult(true, "Combat training scene is valid.");
            }

            if (root.name == "Broadsword_Combat")
            {
                var rigidbody = root.GetComponent<Rigidbody>();
                if (rigidbody == null || !rigidbody.useGravity)
                    return new ValidationResult(false, "Broadsword_Combat must have a Rigidbody with useGravity enabled.");

                if (root.GetComponent<XRGrabInteractable>() == null)
                    return new ValidationResult(false, "Broadsword_Combat must have an XRGrabInteractable.");

                return new ValidationResult(true, "Broadsword_Combat is valid.");
            }

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
