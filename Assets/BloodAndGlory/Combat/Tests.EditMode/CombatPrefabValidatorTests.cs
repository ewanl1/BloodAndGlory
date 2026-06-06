using BloodAndGlory.Combat.Editor;
using NUnit.Framework;
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
    }
}
