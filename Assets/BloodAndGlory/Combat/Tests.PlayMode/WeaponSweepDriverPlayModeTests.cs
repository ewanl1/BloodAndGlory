using System.Collections;
using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using BloodAndGlory.Combat.Runtime.Weapons;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BloodAndGlory.Combat.Tests.PlayMode
{
    public sealed class WeaponSweepDriverPlayModeTests
    {
        [UnityTest]
        public IEnumerator SweepAcrossHurtbox_EmitsHitProposal()
        {
            var weapon = new GameObject("weapon");
            var markerRoot = new GameObject("markers");
            markerRoot.transform.SetParent(weapon.transform);
            var guard = CreateMarker(markerRoot.transform, "guard", new Vector3(-0.5f, 0f, 0f));
            var tip = CreateMarker(markerRoot.transform, "tip", new Vector3(0.5f, 0f, 0f));

            var markers = weapon.AddComponent<WeaponMarkerSet>();
            markers.ConfigureForTests(new[] { guard, tip });
            var driver = weapon.AddComponent<WeaponSweepDriver>();
            driver.ConfigureForTests(attackerId: 1, weaponId: "broadsword", markers);

            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "target";
            target.transform.position = Vector3.zero;
            target.transform.localScale = Vector3.one * 0.25f;
            var combatant = target.AddComponent<CombatantAuthoring>();
            combatant.ConfigureForTests(2, isPlayer: false, maxHitPoints: 100);
            var hurtbox = target.AddComponent<HurtboxAuthoring>();
            hurtbox.ConfigureForTests(combatant, HurtboxRegion.Torso);

            HitProposal? lastProposal = null;
            driver.HitProposed += proposal => lastProposal = proposal;

            weapon.transform.position = new Vector3(-1f, 0f, 0f);
            yield return new WaitForFixedUpdate();
            weapon.transform.position = new Vector3(1f, 0f, 0f);
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(lastProposal.HasValue);
            Assert.AreEqual(1, lastProposal.Value.AttackerId);
            Assert.AreEqual(combatant.CombatantId, lastProposal.Value.DefenderId);
            Assert.AreEqual(hurtbox.Region, lastProposal.Value.Region);

            Object.Destroy(weapon);
            Object.Destroy(markerRoot);
            Object.Destroy(target);
        }

        private static Transform CreateMarker(Transform parent, string name, Vector3 localPosition)
        {
            var marker = new GameObject(name).transform;
            marker.SetParent(parent);
            marker.localPosition = localPosition;
            return marker;
        }
    }
}
