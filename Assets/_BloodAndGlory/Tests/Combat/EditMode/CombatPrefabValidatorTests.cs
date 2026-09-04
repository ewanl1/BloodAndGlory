using BloodAndGlory.Combat.Editor;
using BloodAndGlory.Combat.Core;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatPrefabValidatorTests
    {
        private const string CombatPeasantPath = "Assets/_BloodAndGlory/CombatContent/Prefabs/Enemies/PeasantBrown_Combat.prefab";

        [Test]
        public void ValidateCombatantWithoutHurtboxes_Fails()
        {
            var root = new GameObject("combatant");
            root.AddComponent<BloodAndGlory.Combat.Runtime.Authoring.CombatantAuthoring>();

            var result = CombatPrefabValidator.Validate(root);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Message, Does.Contain("hurtbox"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void ValidateCombatPeasantPrefab_RequiresAnimatorController()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatPeasantPath);

            Assert.IsNotNull(prefab);

            var result = CombatPrefabValidator.Validate(prefab);
            var animator = prefab.GetComponent<Animator>();

            Assert.IsTrue(result.IsValid, result.Message);
            Assert.IsNotNull(animator);
            Assert.IsNotNull(animator.runtimeAnimatorController);
        }

        [Test]
        public void ValidateCombatPeasantPrefab_RequiresCombatAnimatorParameters()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatPeasantPath);
            Assert.IsNotNull(prefab);

            var result = CombatPrefabValidator.Validate(prefab);
            var controller = prefab.GetComponent<Animator>()?.runtimeAnimatorController as AnimatorController;

            Assert.IsTrue(result.IsValid, result.Message);
            Assert.IsNotNull(controller);
            AssertHasParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            AssertHasParameter(controller, "CombatState", AnimatorControllerParameterType.Int);
            AssertHasParameter(controller, "Dead", AnimatorControllerParameterType.Bool);
        }

        [Test]
        public void ValidateCombatPeasantPrefab_AttackTransitionUsesAttackCommitState()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatPeasantPath);
            Assert.IsNotNull(prefab);

            var controller = prefab.GetComponent<Animator>()?.runtimeAnimatorController as AnimatorController;
            Assert.IsNotNull(controller);

            AssertHasCombatStateTransition(controller, "Idle", "Attack", EnemyCombatState.AttackCommit);
            AssertHasCombatStateTransition(controller, "Approach", "Attack", EnemyCombatState.AttackCommit);
        }

        [Test]
        public void ValidateCombatPeasantPrefab_RequiresGroundedRendererBoundsAndAgentOffset()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatPeasantPath);
            Assert.IsNotNull(prefab);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var result = CombatPrefabValidator.Validate(instance);
                var agent = instance.GetComponent<NavMeshAgent>();
                var bounds = CalculateRendererBounds(instance);

                Assert.IsTrue(result.IsValid, result.Message);
                Assert.IsNotNull(agent);
                Assert.Greater(agent.baseOffset, 0f);
                Assert.GreaterOrEqual(bounds.min.y, -0.001f, "Peasant visual bounds should not spawn below the root/floor.");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void AssertHasParameter(
            AnimatorController controller,
            string parameterName,
            AnimatorControllerParameterType parameterType)
        {
            var parameter = controller.parameters.FirstOrDefault(candidate => candidate.name == parameterName);

            Assert.IsNotNull(parameter, $"Missing animator parameter {parameterName}.");
            Assert.AreEqual(parameterType, parameter.type, $"Animator parameter {parameterName} has wrong type.");
        }

        private static void AssertHasCombatStateTransition(
            AnimatorController controller,
            string sourceStateName,
            string destinationStateName,
            EnemyCombatState combatState)
        {
            var sourceState = controller.layers[0].stateMachine.states
                .Select(child => child.state)
                .FirstOrDefault(state => state.name == sourceStateName);
            Assert.IsNotNull(sourceState, $"Missing animator state {sourceStateName}.");

            var expectedThreshold = (float)(int)combatState;
            var hasTransition = sourceState.transitions.Any(transition =>
                transition.destinationState != null &&
                transition.destinationState.name == destinationStateName &&
                transition.conditions.Any(condition =>
                    condition.parameter == "CombatState" &&
                    condition.mode == AnimatorConditionMode.Equals &&
                    Mathf.Approximately(condition.threshold, expectedThreshold)));

            Assert.IsTrue(
                hasTransition,
                $"{sourceStateName} must transition to {destinationStateName} when CombatState equals {combatState} ({expectedThreshold}).");
        }

        private static Bounds CalculateRendererBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            Assert.IsNotEmpty(renderers, "Peasant prefab must have renderers.");

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1))
                bounds.Encapsulate(renderer.bounds);

            return bounds;
        }
    }
}
