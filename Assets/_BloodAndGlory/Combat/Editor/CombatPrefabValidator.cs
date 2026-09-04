using BloodAndGlory.Combat.Runtime.Authoring;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
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

            if (root.name == "PeasantBrown_Combat")
            {
                var animator = root.GetComponent<Animator>();
                if (animator == null || animator.runtimeAnimatorController == null)
                    return new ValidationResult(false, "PeasantBrown_Combat must have an Animator with a runtime controller.");

                var animatorController = animator.runtimeAnimatorController as AnimatorController;
                if (animatorController == null)
                    return new ValidationResult(false, "PeasantBrown_Combat must use an AnimatorController.");

                var speed = animatorController.parameters.FirstOrDefault(parameter => parameter.name == "Speed");
                if (speed == null || speed.type != AnimatorControllerParameterType.Float)
                    return new ValidationResult(false, "PeasantBrown_Combat animator must have a float Speed parameter.");

                var combatState = animatorController.parameters.FirstOrDefault(parameter => parameter.name == "CombatState");
                if (combatState == null || combatState.type != AnimatorControllerParameterType.Int)
                    return new ValidationResult(false, "PeasantBrown_Combat animator must have an int CombatState parameter.");

                var dead = animatorController.parameters.FirstOrDefault(parameter => parameter.name == "Dead");
                if (dead == null || dead.type != AnimatorControllerParameterType.Bool)
                    return new ValidationResult(false, "PeasantBrown_Combat animator must have a bool Dead parameter.");

                var agent = root.GetComponent<NavMeshAgent>();
                if (agent == null || agent.baseOffset <= 0f)
                    return new ValidationResult(false, "PeasantBrown_Combat must have a NavMeshAgent with a positive base offset.");

                var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
                if (renderers.Length == 0)
                    return new ValidationResult(false, "PeasantBrown_Combat must have renderers.");

                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers.Skip(1))
                    bounds.Encapsulate(renderer.bounds);

                if (bounds.min.y < -0.001f)
                    return new ValidationResult(false, "PeasantBrown_Combat renderer bounds must not spawn below the root.");
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
