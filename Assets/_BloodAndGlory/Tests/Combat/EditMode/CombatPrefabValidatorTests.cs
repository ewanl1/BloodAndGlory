using BloodAndGlory.Combat.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatPrefabValidatorTests
    {
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
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/BloodAndGlory/CombatContent/Prefabs/Enemies/PeasantBrown_Combat.prefab");

            Assert.IsNotNull(prefab);

            var result = CombatPrefabValidator.Validate(prefab);
            var animator = prefab.GetComponent<Animator>();

            Assert.IsTrue(result.IsValid, result.Message);
            Assert.IsNotNull(animator);
            Assert.IsNotNull(animator.runtimeAnimatorController);
        }
    }
}
