# Combat Training Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the Quest 3-first Blood and Glory combat training vertical slice defined in `docs/superpowers/specs/2026-06-05-combat-training-vertical-slice-design.md`.

**Architecture:** Implement deterministic combat rules in `Assets/BloodAndGlory/Combat/Core`, then add Unity runtime adapters for weapons, hurtboxes, enemy AI, debug UI, and training-scene content. Keep combat decisions in core services and make Unity scenes/prefabs provide data, sensors, and presentation.

**Tech Stack:** Unity 6000.4.9f1, C#, Unity Test Framework 1.6.0, XR Interaction Toolkit 3.4.1, AI Navigation 2.0.12, URP 17.4.0, Git LFS 3.7.1.

---

## Verification Commands

Use these commands throughout the plan:

```bash
/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/ewanlynch/Documents/GitHub/BloodAndGlory -runTests -testPlatform EditMode -testResults /Users/ewanlynch/Documents/GitHub/BloodAndGlory/TestResults-EditMode.xml -quit
```

```bash
/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/ewanlynch/Documents/GitHub/BloodAndGlory -runTests -testPlatform PlayMode -testResults /Users/ewanlynch/Documents/GitHub/BloodAndGlory/TestResults-PlayMode.xml -quit
```

Expected successful output includes `Exiting batchmode successfully now!` and a test result XML with no failed tests.

## File Structure

Create these folders and files during the plan:

```text
Assets/BloodAndGlory/Combat/
  Core/
    BloodAndGlory.Combat.Core.asmdef
    AttackDefinitionData.cs
    BlockContext.cs
    CombatEvent.cs
    CombatEventType.cs
    CombatResolver.cs
    CombatState.cs
    DamageResult.cs
    DeathMode.cs
    EnemyCombatState.cs
    EnemyDecisionService.cs
    EnemyProfileData.cs
    HealthState.cs
    HitProposal.cs
    HurtboxRegion.cs
    RegionDamageTable.cs
    WeaponProfileData.cs
  Runtime/
    BloodAndGlory.Combat.Runtime.asmdef
    Authoring/
      CombatantAuthoring.cs
      HurtboxAuthoring.cs
      WeaponProfileAsset.cs
      EnemyProfileAsset.cs
      AttackDefinitionAsset.cs
    Enemy/
      EnemyCombatController.cs
    Weapons/
      WeaponMarkerSet.cs
      WeaponSweepDriver.cs
    Debug/
      CombatDebugOverlay.cs
  Editor/
    BloodAndGlory.Combat.Editor.asmdef
    CombatContentBuilder.cs
    CombatPrefabValidator.cs
Assets/BloodAndGlory/Combat/Tests.EditMode/
  BloodAndGlory.Combat.Tests.EditMode.asmdef
  CombatResolverTests.cs
  EnemyDecisionServiceTests.cs
  CombatPrefabValidatorTests.cs
Assets/BloodAndGlory/Combat/Tests.PlayMode/
  BloodAndGlory.Combat.Tests.PlayMode.asmdef
  WeaponSweepDriverPlayModeTests.cs
  CombatTrainingSceneSmokeTests.cs
Assets/BloodAndGlory/CombatContent/
  Scenes/CombatTrainingScene.unity
  Prefabs/Weapons/Broadsword_Combat.prefab
  Prefabs/Enemies/PeasantBrown_Combat.prefab
  Profiles/BroadswordProfile.asset
  Profiles/PeasantWeakProfile.asset
  Attacks/Peasant_Broadsword_Attack_01.asset
  Materials/Training/TrainingWhite.mat
  Debug/
```

The implementation can create Unity assets through the editor script `CombatContentBuilder` so serialized YAML does not have to be hand-written.

### Task 1: Assembly And Folder Scaffold

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Core/BloodAndGlory.Combat.Core.asmdef`
- Create: `Assets/BloodAndGlory/Combat/Runtime/BloodAndGlory.Combat.Runtime.asmdef`
- Create: `Assets/BloodAndGlory/Combat/Editor/BloodAndGlory.Combat.Editor.asmdef`
- Create: `Assets/BloodAndGlory/Combat/Tests.EditMode/BloodAndGlory.Combat.Tests.EditMode.asmdef`
- Create: `Assets/BloodAndGlory/Combat/Tests.PlayMode/BloodAndGlory.Combat.Tests.PlayMode.asmdef`

- [ ] **Step 1: Create folders**

Run:

```bash
mkdir -p Assets/BloodAndGlory/Combat/Core Assets/BloodAndGlory/Combat/Runtime/Authoring Assets/BloodAndGlory/Combat/Runtime/Enemy Assets/BloodAndGlory/Combat/Runtime/Weapons Assets/BloodAndGlory/Combat/Runtime/Debug Assets/BloodAndGlory/Combat/Editor Assets/BloodAndGlory/Combat/Tests.EditMode Assets/BloodAndGlory/Combat/Tests.PlayMode Assets/BloodAndGlory/CombatContent/Scenes Assets/BloodAndGlory/CombatContent/Prefabs/Weapons Assets/BloodAndGlory/CombatContent/Prefabs/Enemies Assets/BloodAndGlory/CombatContent/Profiles Assets/BloodAndGlory/CombatContent/Attacks Assets/BloodAndGlory/CombatContent/Materials/Training Assets/BloodAndGlory/CombatContent/Debug
```

Expected: command exits `0`.

- [ ] **Step 2: Create core asmdef**

Create `Assets/BloodAndGlory/Combat/Core/BloodAndGlory.Combat.Core.asmdef`:

```json
{
  "name": "BloodAndGlory.Combat.Core",
  "rootNamespace": "BloodAndGlory.Combat.Core",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": true
}
```

- [ ] **Step 3: Create runtime asmdef**

Create `Assets/BloodAndGlory/Combat/Runtime/BloodAndGlory.Combat.Runtime.asmdef`:

```json
{
  "name": "BloodAndGlory.Combat.Runtime",
  "rootNamespace": "BloodAndGlory.Combat.Runtime",
  "references": [
    "BloodAndGlory.Combat.Core",
    "Unity.AI.Navigation",
    "Unity.InputSystem",
    "Unity.XR.Interaction.Toolkit"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

- [ ] **Step 4: Create editor asmdef**

Create `Assets/BloodAndGlory/Combat/Editor/BloodAndGlory.Combat.Editor.asmdef`:

```json
{
  "name": "BloodAndGlory.Combat.Editor",
  "rootNamespace": "BloodAndGlory.Combat.Editor",
  "references": [
    "BloodAndGlory.Combat.Core",
    "BloodAndGlory.Combat.Runtime",
    "Unity.AI.Navigation"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

- [ ] **Step 5: Create Edit Mode test asmdef**

Create `Assets/BloodAndGlory/Combat/Tests.EditMode/BloodAndGlory.Combat.Tests.EditMode.asmdef`:

```json
{
  "name": "BloodAndGlory.Combat.Tests.EditMode",
  "rootNamespace": "BloodAndGlory.Combat.Tests.EditMode",
  "references": [
    "BloodAndGlory.Combat.Core",
    "BloodAndGlory.Combat.Runtime",
    "BloodAndGlory.Combat.Editor",
    "UnityEngine.TestRunner",
    "UnityEditor.TestRunner"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": [
    "nunit.framework.dll"
  ],
  "autoReferenced": false,
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"
  ],
  "versionDefines": [],
  "noEngineReferences": false
}
```

- [ ] **Step 6: Create Play Mode test asmdef**

Create `Assets/BloodAndGlory/Combat/Tests.PlayMode/BloodAndGlory.Combat.Tests.PlayMode.asmdef`:

```json
{
  "name": "BloodAndGlory.Combat.Tests.PlayMode",
  "rootNamespace": "BloodAndGlory.Combat.Tests.PlayMode",
  "references": [
    "BloodAndGlory.Combat.Core",
    "BloodAndGlory.Combat.Runtime",
    "UnityEngine.TestRunner"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": [
    "nunit.framework.dll"
  ],
  "autoReferenced": false,
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"
  ],
  "versionDefines": [],
  "noEngineReferences": false
}
```

- [ ] **Step 7: Run Edit Mode compile check**

Run the Edit Mode verification command.

Expected: Unity may report `0 tests`, but compilation succeeds.

- [ ] **Step 8: Commit**

```bash
git add Assets/BloodAndGlory/Combat Assets/BloodAndGlory/CombatContent
git commit -m "feat: scaffold combat assemblies"
```

### Task 2: Core Combat Data Model

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Core/HurtboxRegion.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/CombatEventType.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/DeathMode.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/HealthState.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/RegionDamageTable.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/WeaponProfileData.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/HitProposal.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/BlockContext.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/CombatEvent.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/DamageResult.cs`

- [ ] **Step 1: Write data model compile test**

Create `Assets/BloodAndGlory/Combat/Tests.EditMode/CombatResolverTests.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using NUnit.Framework;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatResolverTests
    {
        [Test]
        public void HealthState_ClampsDamageAndMarksDead()
        {
            var health = new HealthState(10);

            health = health.ApplyDamage(4);
            Assert.AreEqual(6, health.CurrentHitPoints);
            Assert.IsTrue(health.IsAlive);

            health = health.ApplyDamage(20);
            Assert.AreEqual(0, health.CurrentHitPoints);
            Assert.IsFalse(health.IsAlive);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run the Edit Mode verification command.

Expected: FAIL because `HealthState` does not exist.

- [ ] **Step 3: Add enums**

Create `Assets/BloodAndGlory/Combat/Core/HurtboxRegion.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public enum HurtboxRegion
    {
        Head = 0,
        Torso = 1,
        Arm = 2,
        Leg = 3
    }
}
```

Create `Assets/BloodAndGlory/Combat/Core/CombatEventType.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public enum CombatEventType
    {
        Damaged = 0,
        Blocked = 1,
        Parried = 2,
        WouldHitPlayer = 3,
        Died = 4,
        SuppressedDuplicate = 5
    }
}
```

Create `Assets/BloodAndGlory/Combat/Core/DeathMode.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public enum DeathMode
    {
        AnimatedDeath = 0,
        RagdollDeath = 1
    }
}
```

- [ ] **Step 4: Add health state**

Create `Assets/BloodAndGlory/Combat/Core/HealthState.cs`:

```csharp
using System;

namespace BloodAndGlory.Combat.Core
{
    public readonly struct HealthState
    {
        public HealthState(int maxHitPoints)
            : this(maxHitPoints, maxHitPoints)
        {
        }

        private HealthState(int maxHitPoints, int currentHitPoints)
        {
            if (maxHitPoints <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHitPoints), "Max HP must be greater than zero.");

            MaxHitPoints = maxHitPoints;
            CurrentHitPoints = Math.Clamp(currentHitPoints, 0, maxHitPoints);
        }

        public int MaxHitPoints { get; }
        public int CurrentHitPoints { get; }
        public bool IsAlive => CurrentHitPoints > 0;

        public HealthState ApplyDamage(int damage)
        {
            if (damage <= 0)
                return this;

            return new HealthState(MaxHitPoints, CurrentHitPoints - damage);
        }
    }
}
```

- [ ] **Step 5: Add region damage table**

Create `Assets/BloodAndGlory/Combat/Core/RegionDamageTable.cs`:

```csharp
using System;

namespace BloodAndGlory.Combat.Core
{
    public readonly struct RegionDamageTable
    {
        public RegionDamageTable(float head, float torso, float arm, float leg)
        {
            Head = Validate(head, nameof(head));
            Torso = Validate(torso, nameof(torso));
            Arm = Validate(arm, nameof(arm));
            Leg = Validate(leg, nameof(leg));
        }

        public float Head { get; }
        public float Torso { get; }
        public float Arm { get; }
        public float Leg { get; }

        public static RegionDamageTable BroadswordDefaults => new RegionDamageTable(1.5f, 1.0f, 0.75f, 0.75f);

        public float MultiplierFor(HurtboxRegion region)
        {
            return region switch
            {
                HurtboxRegion.Head => Head,
                HurtboxRegion.Torso => Torso,
                HurtboxRegion.Arm => Arm,
                HurtboxRegion.Leg => Leg,
                _ => 1.0f
            };
        }

        private static float Validate(float value, string name)
        {
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(name, "Region multiplier must be greater than zero.");

            return value;
        }
    }
}
```

- [ ] **Step 6: Add weapon, hit, block, and event data**

Create `Assets/BloodAndGlory/Combat/Core/WeaponProfileData.cs`:

```csharp
using System;

namespace BloodAndGlory.Combat.Core
{
    public readonly struct WeaponProfileData
    {
        public WeaponProfileData(
            string id,
            int minimumDamage,
            int maximumDamage,
            float minimumFullReactionVelocity,
            float maximumDamageVelocity,
            float duplicateHitWindowSeconds,
            RegionDamageTable regionDamage)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Weapon id is required.", nameof(id));
            if (minimumDamage < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumDamage));
            if (maximumDamage < minimumDamage)
                throw new ArgumentOutOfRangeException(nameof(maximumDamage));
            if (minimumFullReactionVelocity < 0f)
                throw new ArgumentOutOfRangeException(nameof(minimumFullReactionVelocity));
            if (maximumDamageVelocity <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maximumDamageVelocity));
            if (duplicateHitWindowSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duplicateHitWindowSeconds));

            Id = id;
            MinimumDamage = minimumDamage;
            MaximumDamage = maximumDamage;
            MinimumFullReactionVelocity = minimumFullReactionVelocity;
            MaximumDamageVelocity = maximumDamageVelocity;
            DuplicateHitWindowSeconds = duplicateHitWindowSeconds;
            RegionDamage = regionDamage;
        }

        public string Id { get; }
        public int MinimumDamage { get; }
        public int MaximumDamage { get; }
        public float MinimumFullReactionVelocity { get; }
        public float MaximumDamageVelocity { get; }
        public float DuplicateHitWindowSeconds { get; }
        public RegionDamageTable RegionDamage { get; }

        public static WeaponProfileData BroadswordDefaults => new WeaponProfileData(
            "broadsword",
            minimumDamage: 1,
            maximumDamage: 18,
            minimumFullReactionVelocity: 1.75f,
            maximumDamageVelocity: 8.0f,
            duplicateHitWindowSeconds: 0.35f,
            regionDamage: RegionDamageTable.BroadswordDefaults);
    }
}
```

Create `Assets/BloodAndGlory/Combat/Core/HitProposal.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public readonly struct HitProposal
    {
        public HitProposal(
            int attackerId,
            int defenderId,
            string weaponId,
            HurtboxRegion region,
            float impactVelocity,
            float timeSeconds,
            bool defenderIsPlayer)
        {
            AttackerId = attackerId;
            DefenderId = defenderId;
            WeaponId = weaponId;
            Region = region;
            ImpactVelocity = impactVelocity;
            TimeSeconds = timeSeconds;
            DefenderIsPlayer = defenderIsPlayer;
        }

        public int AttackerId { get; }
        public int DefenderId { get; }
        public string WeaponId { get; }
        public HurtboxRegion Region { get; }
        public float ImpactVelocity { get; }
        public float TimeSeconds { get; }
        public bool DefenderIsPlayer { get; }
    }
}
```

Create `Assets/BloodAndGlory/Combat/Core/BlockContext.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public readonly struct BlockContext
    {
        public BlockContext(bool isBlocking, bool isParryWindowActive)
        {
            IsBlocking = isBlocking;
            IsParryWindowActive = isParryWindowActive;
        }

        public bool IsBlocking { get; }
        public bool IsParryWindowActive { get; }
    }
}
```

Create `Assets/BloodAndGlory/Combat/Core/CombatEvent.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public readonly struct CombatEvent
    {
        public CombatEvent(
            CombatEventType type,
            int attackerId,
            int defenderId,
            string weaponId,
            HurtboxRegion region,
            int damage,
            float impactVelocity,
            float timeSeconds)
        {
            Type = type;
            AttackerId = attackerId;
            DefenderId = defenderId;
            WeaponId = weaponId;
            Region = region;
            Damage = damage;
            ImpactVelocity = impactVelocity;
            TimeSeconds = timeSeconds;
        }

        public CombatEventType Type { get; }
        public int AttackerId { get; }
        public int DefenderId { get; }
        public string WeaponId { get; }
        public HurtboxRegion Region { get; }
        public int Damage { get; }
        public float ImpactVelocity { get; }
        public float TimeSeconds { get; }
    }
}
```

Create `Assets/BloodAndGlory/Combat/Core/DamageResult.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public readonly struct DamageResult
    {
        public DamageResult(HealthState health, CombatEvent combatEvent, bool fullReaction)
        {
            Health = health;
            Event = combatEvent;
            FullReaction = fullReaction;
        }

        public HealthState Health { get; }
        public CombatEvent Event { get; }
        public bool FullReaction { get; }
    }
}
```

- [ ] **Step 7: Run test to verify it passes**

Run the Edit Mode verification command.

Expected: PASS for `HealthState_ClampsDamageAndMarksDead`.

- [ ] **Step 8: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Core Assets/BloodAndGlory/Combat/Tests.EditMode
git commit -m "feat: add combat core data model"
```

### Task 3: Combat Resolver Rules

**Files:**
- Modify: `Assets/BloodAndGlory/Combat/Tests.EditMode/CombatResolverTests.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/CombatState.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/CombatResolver.cs`

- [ ] **Step 1: Replace resolver tests**

Replace `Assets/BloodAndGlory/Combat/Tests.EditMode/CombatResolverTests.cs` with:

```csharp
using BloodAndGlory.Combat.Core;
using NUnit.Framework;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatResolverTests
    {
        [Test]
        public void HealthState_ClampsDamageAndMarksDead()
        {
            var health = new HealthState(10);

            health = health.ApplyDamage(4);
            Assert.AreEqual(6, health.CurrentHitPoints);
            Assert.IsTrue(health.IsAlive);

            health = health.ApplyDamage(20);
            Assert.AreEqual(0, health.CurrentHitPoints);
            Assert.IsFalse(health.IsAlive);
        }

        [Test]
        public void ResolveHit_AppliesVelocityScaledRegionDamage()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 2, profile.Id, HurtboxRegion.Head, 4.0f, 10.0f, defenderIsPlayer: false);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.Damaged, result.Event.Type);
            Assert.Greater(result.Event.Damage, profile.MinimumDamage);
            Assert.LessOrEqual(result.Event.Damage, profile.MaximumDamage * 2);
            Assert.AreEqual(100 - result.Event.Damage, result.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveHit_ProducesMinimumChipDamageForLowVelocity()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 0.05f, 10.0f, defenderIsPlayer: false);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.Damaged, result.Event.Type);
            Assert.AreEqual(1, result.Event.Damage);
            Assert.IsFalse(result.FullReaction);
        }

        [Test]
        public void ResolveHit_SuppressesDuplicateWithinWindow()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var first = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 6.0f, 1.0f, defenderIsPlayer: false);
            var duplicate = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 6.0f, 1.1f, defenderIsPlayer: false);

            var firstResult = resolver.ResolveHit(state, first, profile, new BlockContext(false, false));
            state = state.WithHealth(firstResult.Health).RecordHit(first, profile);

            var duplicateResult = resolver.ResolveHit(state, duplicate, profile, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.SuppressedDuplicate, duplicateResult.Event.Type);
            Assert.AreEqual(firstResult.Health.CurrentHitPoints, duplicateResult.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveHit_ParryBeatsBlockAndPreventsDamage()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 6.0f, 2.0f, defenderIsPlayer: false);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(isBlocking: true, isParryWindowActive: true));

            Assert.AreEqual(CombatEventType.Parried, result.Event.Type);
            Assert.AreEqual(0, result.Event.Damage);
            Assert.AreEqual(100, result.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveHit_BlockPreventsDamage()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 2, profile.Id, HurtboxRegion.Torso, 6.0f, 2.0f, defenderIsPlayer: false);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(isBlocking: true, isParryWindowActive: false));

            Assert.AreEqual(CombatEventType.Blocked, result.Event.Type);
            Assert.AreEqual(0, result.Event.Damage);
            Assert.AreEqual(100, result.Health.CurrentHitPoints);
        }

        [Test]
        public void ResolveHit_PlayerDefenderProducesWouldHitPlayer()
        {
            var resolver = new CombatResolver();
            var profile = WeaponProfileData.BroadswordDefaults;
            var state = new CombatState(new HealthState(100));
            var hit = new HitProposal(1, 99, profile.Id, HurtboxRegion.Torso, 6.0f, 2.0f, defenderIsPlayer: true);

            var result = resolver.ResolveHit(state, hit, profile, new BlockContext(false, false));

            Assert.AreEqual(CombatEventType.WouldHitPlayer, result.Event.Type);
            Assert.AreEqual(0, result.Event.Damage);
            Assert.AreEqual(100, result.Health.CurrentHitPoints);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run the Edit Mode verification command.

Expected: FAIL because `CombatResolver` and `CombatState` do not exist.

- [ ] **Step 3: Add combat state**

Create `Assets/BloodAndGlory/Combat/Core/CombatState.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace BloodAndGlory.Combat.Core
{
    public sealed class CombatState
    {
        private readonly Dictionary<HitKey, float> _lastHitTimes;

        public CombatState(HealthState health)
            : this(health, new Dictionary<HitKey, float>())
        {
        }

        private CombatState(HealthState health, Dictionary<HitKey, float> lastHitTimes)
        {
            Health = health;
            _lastHitTimes = lastHitTimes;
        }

        public HealthState Health { get; }

        public bool IsDuplicate(HitProposal hit, WeaponProfileData profile)
        {
            var key = HitKey.From(hit);
            return _lastHitTimes.TryGetValue(key, out var lastTime)
                && hit.TimeSeconds - lastTime < profile.DuplicateHitWindowSeconds;
        }

        public CombatState WithHealth(HealthState health)
        {
            return new CombatState(health, new Dictionary<HitKey, float>(_lastHitTimes));
        }

        public CombatState RecordHit(HitProposal hit, WeaponProfileData profile)
        {
            var copy = new Dictionary<HitKey, float>(_lastHitTimes)
            {
                [HitKey.From(hit)] = hit.TimeSeconds
            };
            return new CombatState(Health, copy);
        }

        private readonly struct HitKey : IEquatable<HitKey>
        {
            private HitKey(int attackerId, int defenderId, string weaponId, HurtboxRegion region)
            {
                AttackerId = attackerId;
                DefenderId = defenderId;
                WeaponId = weaponId;
                Region = region;
            }

            private int AttackerId { get; }
            private int DefenderId { get; }
            private string WeaponId { get; }
            private HurtboxRegion Region { get; }

            public static HitKey From(HitProposal hit)
            {
                return new HitKey(hit.AttackerId, hit.DefenderId, hit.WeaponId, hit.Region);
            }

            public bool Equals(HitKey other)
            {
                return AttackerId == other.AttackerId
                    && DefenderId == other.DefenderId
                    && WeaponId == other.WeaponId
                    && Region == other.Region;
            }

            public override bool Equals(object obj)
            {
                return obj is HitKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(AttackerId, DefenderId, WeaponId, Region);
            }
        }
    }
}
```

- [ ] **Step 4: Add resolver**

Create `Assets/BloodAndGlory/Combat/Core/CombatResolver.cs`:

```csharp
using System;

namespace BloodAndGlory.Combat.Core
{
    public sealed class CombatResolver
    {
        public DamageResult ResolveHit(
            CombatState state,
            HitProposal hit,
            WeaponProfileData weapon,
            BlockContext block)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (block.IsParryWindowActive)
                return NoDamage(state.Health, CombatEventType.Parried, hit);

            if (block.IsBlocking)
                return NoDamage(state.Health, CombatEventType.Blocked, hit);

            if (hit.DefenderIsPlayer)
                return NoDamage(state.Health, CombatEventType.WouldHitPlayer, hit);

            if (state.IsDuplicate(hit, weapon))
                return NoDamage(state.Health, CombatEventType.SuppressedDuplicate, hit);

            var damage = CalculateDamage(hit, weapon);
            var health = state.Health.ApplyDamage(damage);
            var eventType = health.IsAlive ? CombatEventType.Damaged : CombatEventType.Died;
            var combatEvent = new CombatEvent(eventType, hit.AttackerId, hit.DefenderId, hit.WeaponId, hit.Region, damage, hit.ImpactVelocity, hit.TimeSeconds);
            var fullReaction = hit.ImpactVelocity >= weapon.MinimumFullReactionVelocity;
            return new DamageResult(health, combatEvent, fullReaction);
        }

        private static DamageResult NoDamage(HealthState health, CombatEventType eventType, HitProposal hit)
        {
            var combatEvent = new CombatEvent(eventType, hit.AttackerId, hit.DefenderId, hit.WeaponId, hit.Region, 0, hit.ImpactVelocity, hit.TimeSeconds);
            return new DamageResult(health, combatEvent, fullReaction: false);
        }

        private static int CalculateDamage(HitProposal hit, WeaponProfileData weapon)
        {
            var normalizedVelocity = Math.Clamp(hit.ImpactVelocity / weapon.MaximumDamageVelocity, 0f, 1f);
            var baseDamage = weapon.MinimumDamage + (weapon.MaximumDamage - weapon.MinimumDamage) * normalizedVelocity;
            var regionDamage = baseDamage * weapon.RegionDamage.MultiplierFor(hit.Region);
            return Math.Max(weapon.MinimumDamage, (int)MathF.Round(regionDamage));
        }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run the Edit Mode verification command.

Expected: PASS for all `CombatResolverTests`.

- [ ] **Step 6: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Core Assets/BloodAndGlory/Combat/Tests.EditMode/CombatResolverTests.cs
git commit -m "feat: resolve core combat hits"
```

### Task 4: Enemy Decision Core

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Core/EnemyCombatState.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/EnemyProfileData.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/AttackDefinitionData.cs`
- Create: `Assets/BloodAndGlory/Combat/Core/EnemyDecisionService.cs`
- Create: `Assets/BloodAndGlory/Combat/Tests.EditMode/EnemyDecisionServiceTests.cs`

- [ ] **Step 1: Write enemy FSM tests**

Create `Assets/BloodAndGlory/Combat/Tests.EditMode/EnemyDecisionServiceTests.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using NUnit.Framework;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class EnemyDecisionServiceTests
    {
        [Test]
        public void Decide_ApproachesWhenOutOfRange()
        {
            var service = new EnemyDecisionService();
            var profile = EnemyProfileData.WeakPeasantDefaults;

            var next = service.Decide(EnemyCombatState.Idle, profile, distanceToTarget: 5.0f, timeInState: 0.2f, randomValue: 0.5f);

            Assert.AreEqual(EnemyCombatState.Approach, next);
        }

        [Test]
        public void Decide_TelegraphsWhenInsidePreferredRange()
        {
            var service = new EnemyDecisionService();
            var profile = EnemyProfileData.WeakPeasantDefaults;

            var next = service.Decide(EnemyCombatState.Approach, profile, distanceToTarget: 1.4f, timeInState: 0.2f, randomValue: 0.5f);

            Assert.AreEqual(EnemyCombatState.Telegraph, next);
        }

        [Test]
        public void Decide_WeakPeasantRarelyBlocks()
        {
            var service = new EnemyDecisionService();
            var profile = EnemyProfileData.WeakPeasantDefaults;

            var doesNotBlock = service.DecideDefense(profile, randomValue: 0.9f);
            var doesBlock = service.DecideDefense(profile, randomValue: 0.01f);

            Assert.AreEqual(EnemyCombatState.Recover, doesNotBlock);
            Assert.AreEqual(EnemyCombatState.Block, doesBlock);
        }

        [Test]
        public void Decide_TransitionsAttackCommitToRecoverAfterActiveWindow()
        {
            var service = new EnemyDecisionService();
            var profile = EnemyProfileData.WeakPeasantDefaults;

            var next = service.Decide(EnemyCombatState.AttackCommit, profile, distanceToTarget: 1.2f, timeInState: 0.8f, randomValue: 0.5f);

            Assert.AreEqual(EnemyCombatState.Recover, next);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run the Edit Mode verification command.

Expected: FAIL because enemy decision types do not exist.

- [ ] **Step 3: Add enemy core types**

Create `Assets/BloodAndGlory/Combat/Core/EnemyCombatState.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public enum EnemyCombatState
    {
        Idle = 0,
        Approach = 1,
        Telegraph = 2,
        AttackCommit = 3,
        Recover = 4,
        Block = 5,
        Parry = 6,
        Stagger = 7,
        Dead = 8
    }
}
```

Create `Assets/BloodAndGlory/Combat/Core/EnemyProfileData.cs`:

```csharp
using System;

namespace BloodAndGlory.Combat.Core
{
    public readonly struct EnemyProfileData
    {
        public EnemyProfileData(
            float preferredAttackDistance,
            float telegraphSeconds,
            float attackCommitSeconds,
            float recoverSeconds,
            float blockChance,
            float parryChance)
        {
            if (preferredAttackDistance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(preferredAttackDistance));
            if (telegraphSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(telegraphSeconds));
            if (attackCommitSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(attackCommitSeconds));
            if (recoverSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(recoverSeconds));

            PreferredAttackDistance = preferredAttackDistance;
            TelegraphSeconds = telegraphSeconds;
            AttackCommitSeconds = attackCommitSeconds;
            RecoverSeconds = recoverSeconds;
            BlockChance = Math.Clamp(blockChance, 0f, 1f);
            ParryChance = Math.Clamp(parryChance, 0f, 1f);
        }

        public float PreferredAttackDistance { get; }
        public float TelegraphSeconds { get; }
        public float AttackCommitSeconds { get; }
        public float RecoverSeconds { get; }
        public float BlockChance { get; }
        public float ParryChance { get; }

        public static EnemyProfileData WeakPeasantDefaults => new EnemyProfileData(
            preferredAttackDistance: 1.6f,
            telegraphSeconds: 0.65f,
            attackCommitSeconds: 0.75f,
            recoverSeconds: 0.9f,
            blockChance: 0.08f,
            parryChance: 0.03f);
    }
}
```

Create `Assets/BloodAndGlory/Combat/Core/AttackDefinitionData.cs`:

```csharp
using System;

namespace BloodAndGlory.Combat.Core
{
    public enum AttackMovementPolicy
    {
        None = 0,
        ScriptedStep = 1,
        RootMotion = 2
    }

    public readonly struct AttackDefinitionData
    {
        public AttackDefinitionData(string id, float activeStartSeconds, float activeEndSeconds, AttackMovementPolicy movementPolicy)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Attack id is required.", nameof(id));
            if (activeStartSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(activeStartSeconds));
            if (activeEndSeconds <= activeStartSeconds)
                throw new ArgumentOutOfRangeException(nameof(activeEndSeconds));

            Id = id;
            ActiveStartSeconds = activeStartSeconds;
            ActiveEndSeconds = activeEndSeconds;
            MovementPolicy = movementPolicy;
        }

        public string Id { get; }
        public float ActiveStartSeconds { get; }
        public float ActiveEndSeconds { get; }
        public AttackMovementPolicy MovementPolicy { get; }

        public bool IsActive(float timeInAttack)
        {
            return timeInAttack >= ActiveStartSeconds && timeInAttack <= ActiveEndSeconds;
        }
    }
}
```

- [ ] **Step 4: Add decision service**

Create `Assets/BloodAndGlory/Combat/Core/EnemyDecisionService.cs`:

```csharp
namespace BloodAndGlory.Combat.Core
{
    public sealed class EnemyDecisionService
    {
        public EnemyCombatState Decide(
            EnemyCombatState current,
            EnemyProfileData profile,
            float distanceToTarget,
            float timeInState,
            float randomValue)
        {
            return current switch
            {
                EnemyCombatState.Idle => distanceToTarget > profile.PreferredAttackDistance
                    ? EnemyCombatState.Approach
                    : EnemyCombatState.Telegraph,
                EnemyCombatState.Approach => distanceToTarget <= profile.PreferredAttackDistance
                    ? EnemyCombatState.Telegraph
                    : EnemyCombatState.Approach,
                EnemyCombatState.Telegraph => timeInState >= profile.TelegraphSeconds
                    ? EnemyCombatState.AttackCommit
                    : EnemyCombatState.Telegraph,
                EnemyCombatState.AttackCommit => timeInState >= profile.AttackCommitSeconds
                    ? EnemyCombatState.Recover
                    : EnemyCombatState.AttackCommit,
                EnemyCombatState.Recover => timeInState >= profile.RecoverSeconds
                    ? EnemyCombatState.Approach
                    : EnemyCombatState.Recover,
                EnemyCombatState.Block => timeInState >= profile.RecoverSeconds
                    ? EnemyCombatState.Recover
                    : EnemyCombatState.Block,
                EnemyCombatState.Parry => timeInState >= profile.RecoverSeconds
                    ? EnemyCombatState.Recover
                    : EnemyCombatState.Parry,
                EnemyCombatState.Stagger => timeInState >= profile.RecoverSeconds
                    ? EnemyCombatState.Recover
                    : EnemyCombatState.Stagger,
                EnemyCombatState.Dead => EnemyCombatState.Dead,
                _ => EnemyCombatState.Idle
            };
        }

        public EnemyCombatState DecideDefense(EnemyProfileData profile, float randomValue)
        {
            if (randomValue <= profile.ParryChance)
                return EnemyCombatState.Parry;

            if (randomValue <= profile.ParryChance + profile.BlockChance)
                return EnemyCombatState.Block;

            return EnemyCombatState.Recover;
        }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run the Edit Mode verification command.

Expected: PASS for `EnemyDecisionServiceTests` and `CombatResolverTests`.

- [ ] **Step 6: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Core Assets/BloodAndGlory/Combat/Tests.EditMode/EnemyDecisionServiceTests.cs
git commit -m "feat: add weak peasant combat decisions"
```

### Task 5: Runtime Authoring Assets And Components

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Runtime/Authoring/WeaponProfileAsset.cs`
- Create: `Assets/BloodAndGlory/Combat/Runtime/Authoring/EnemyProfileAsset.cs`
- Create: `Assets/BloodAndGlory/Combat/Runtime/Authoring/AttackDefinitionAsset.cs`
- Create: `Assets/BloodAndGlory/Combat/Runtime/Authoring/CombatantAuthoring.cs`
- Create: `Assets/BloodAndGlory/Combat/Runtime/Authoring/HurtboxAuthoring.cs`

- [ ] **Step 1: Add weapon profile asset**

Create `Assets/BloodAndGlory/Combat/Runtime/Authoring/WeaponProfileAsset.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    [CreateAssetMenu(menuName = "Blood And Glory/Combat/Weapon Profile", fileName = "WeaponProfile")]
    public sealed class WeaponProfileAsset : ScriptableObject
    {
        [SerializeField] private string id = "broadsword";
        [SerializeField] private int minimumDamage = 1;
        [SerializeField] private int maximumDamage = 18;
        [SerializeField] private float minimumFullReactionVelocity = 1.75f;
        [SerializeField] private float maximumDamageVelocity = 8.0f;
        [SerializeField] private float duplicateHitWindowSeconds = 0.35f;
        [SerializeField] private float headMultiplier = 1.5f;
        [SerializeField] private float torsoMultiplier = 1.0f;
        [SerializeField] private float armMultiplier = 0.75f;
        [SerializeField] private float legMultiplier = 0.75f;

        public WeaponProfileData ToData()
        {
            return new WeaponProfileData(
                id,
                minimumDamage,
                maximumDamage,
                minimumFullReactionVelocity,
                maximumDamageVelocity,
                duplicateHitWindowSeconds,
                new RegionDamageTable(headMultiplier, torsoMultiplier, armMultiplier, legMultiplier));
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = "broadsword";
            minimumDamage = Mathf.Max(0, minimumDamage);
            maximumDamage = Mathf.Max(minimumDamage, maximumDamage);
            minimumFullReactionVelocity = Mathf.Max(0f, minimumFullReactionVelocity);
            maximumDamageVelocity = Mathf.Max(0.01f, maximumDamageVelocity);
            duplicateHitWindowSeconds = Mathf.Max(0.01f, duplicateHitWindowSeconds);
            headMultiplier = Mathf.Max(0.01f, headMultiplier);
            torsoMultiplier = Mathf.Max(0.01f, torsoMultiplier);
            armMultiplier = Mathf.Max(0.01f, armMultiplier);
            legMultiplier = Mathf.Max(0.01f, legMultiplier);
        }
    }
}
```

- [ ] **Step 2: Add enemy and attack assets**

Create `Assets/BloodAndGlory/Combat/Runtime/Authoring/EnemyProfileAsset.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    [CreateAssetMenu(menuName = "Blood And Glory/Combat/Enemy Profile", fileName = "EnemyProfile")]
    public sealed class EnemyProfileAsset : ScriptableObject
    {
        [SerializeField] private float preferredAttackDistance = 1.6f;
        [SerializeField] private float telegraphSeconds = 0.65f;
        [SerializeField] private float attackCommitSeconds = 0.75f;
        [SerializeField] private float recoverSeconds = 0.9f;
        [SerializeField] private float blockChance = 0.08f;
        [SerializeField] private float parryChance = 0.03f;

        public EnemyProfileData ToData()
        {
            return new EnemyProfileData(
                preferredAttackDistance,
                telegraphSeconds,
                attackCommitSeconds,
                recoverSeconds,
                blockChance,
                parryChance);
        }

        private void OnValidate()
        {
            preferredAttackDistance = Mathf.Max(0.1f, preferredAttackDistance);
            telegraphSeconds = Mathf.Max(0.01f, telegraphSeconds);
            attackCommitSeconds = Mathf.Max(0.01f, attackCommitSeconds);
            recoverSeconds = Mathf.Max(0.01f, recoverSeconds);
            blockChance = Mathf.Clamp01(blockChance);
            parryChance = Mathf.Clamp01(parryChance);
        }
    }
}
```

Create `Assets/BloodAndGlory/Combat/Runtime/Authoring/AttackDefinitionAsset.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    [CreateAssetMenu(menuName = "Blood And Glory/Combat/Attack Definition", fileName = "AttackDefinition")]
    public sealed class AttackDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id = "peasant_broadsword_attack_01";
        [SerializeField] private AnimationClip animationClip;
        [SerializeField] private float activeStartSeconds = 0.25f;
        [SerializeField] private float activeEndSeconds = 0.55f;
        [SerializeField] private AttackMovementPolicy movementPolicy = AttackMovementPolicy.None;

        public AnimationClip AnimationClip => animationClip;

        public AttackDefinitionData ToData()
        {
            return new AttackDefinitionData(id, activeStartSeconds, activeEndSeconds, movementPolicy);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = "peasant_broadsword_attack_01";
            activeStartSeconds = Mathf.Max(0f, activeStartSeconds);
            activeEndSeconds = Mathf.Max(activeStartSeconds + 0.01f, activeEndSeconds);
            movementPolicy = AttackMovementPolicy.None;
        }
    }
}
```

- [ ] **Step 3: Add authoring components**

Create `Assets/BloodAndGlory/Combat/Runtime/Authoring/CombatantAuthoring.cs`:

```csharp
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
```

Create `Assets/BloodAndGlory/Combat/Runtime/Authoring/HurtboxAuthoring.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Authoring
{
    [RequireComponent(typeof(Collider))]
    public sealed class HurtboxAuthoring : MonoBehaviour
    {
        [SerializeField] private CombatantAuthoring owner;
        [SerializeField] private HurtboxRegion region = HurtboxRegion.Torso;

        public CombatantAuthoring Owner => owner;
        public HurtboxRegion Region => region;

        public void ConfigureForTests(CombatantAuthoring owner, HurtboxRegion region)
        {
            this.owner = owner;
            this.region = region;
            GetComponent<Collider>().isTrigger = true;
        }

        private void Reset()
        {
            owner = GetComponentInParent<CombatantAuthoring>();
            var colliderComponent = GetComponent<Collider>();
            colliderComponent.isTrigger = true;
        }

        private void OnValidate()
        {
            if (owner == null)
                owner = GetComponentInParent<CombatantAuthoring>();
        }
    }
}
```

- [ ] **Step 4: Run Edit Mode compile check**

Run the Edit Mode verification command.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Runtime/Authoring
git commit -m "feat: add combat authoring assets"
```

### Task 6: Weapon Sweep Runtime And Play Mode Fixture

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Runtime/Weapons/WeaponMarkerSet.cs`
- Create: `Assets/BloodAndGlory/Combat/Runtime/Weapons/WeaponSweepDriver.cs`
- Create: `Assets/BloodAndGlory/Combat/Tests.PlayMode/WeaponSweepDriverPlayModeTests.cs`

- [ ] **Step 1: Write Play Mode sweep test**

Create `Assets/BloodAndGlory/Combat/Tests.PlayMode/WeaponSweepDriverPlayModeTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run Play Mode test to verify it fails**

Run the Play Mode verification command.

Expected: FAIL because `WeaponMarkerSet` and `WeaponSweepDriver` do not exist.

- [ ] **Step 3: Add marker set**

Create `Assets/BloodAndGlory/Combat/Runtime/Weapons/WeaponMarkerSet.cs`:

```csharp
using System;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Weapons
{
    public sealed class WeaponMarkerSet : MonoBehaviour
    {
        [SerializeField] private Transform[] markers = Array.Empty<Transform>();

        public int Count => markers.Length;

        public Transform GetMarker(int index)
        {
            return markers[index];
        }

        public void ConfigureForTests(Transform[] testMarkers)
        {
            markers = testMarkers ?? Array.Empty<Transform>();
        }
    }
}
```

- [ ] **Step 4: Add sweep driver**

Create `Assets/BloodAndGlory/Combat/Runtime/Weapons/WeaponSweepDriver.cs`:

```csharp
using System;
using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Weapons
{
    [RequireComponent(typeof(WeaponMarkerSet))]
    public sealed class WeaponSweepDriver : MonoBehaviour
    {
        [SerializeField] private int attackerId = 1;
        [SerializeField] private string weaponId = "broadsword";
        [SerializeField] private float sweepRadius = 0.08f;
        [SerializeField] private LayerMask hurtboxLayers = ~0;

        private WeaponMarkerSet markerSet;
        private Vector3[] previousPositions = Array.Empty<Vector3>();
        private bool hasPreviousPositions;

        public event Action<HitProposal> HitProposed;

        private void Awake()
        {
            markerSet = GetComponent<WeaponMarkerSet>();
            previousPositions = new Vector3[markerSet.Count];
        }

        private void FixedUpdate()
        {
            EnsurePreviousBuffer();

            if (!hasPreviousPositions)
            {
                CapturePreviousPositions();
                hasPreviousPositions = true;
                return;
            }

            for (var i = 0; i < markerSet.Count; i++)
            {
                var marker = markerSet.GetMarker(i);
                var current = marker.position;
                var previous = previousPositions[i];
                var delta = current - previous;
                var distance = delta.magnitude;

                if (distance > 0.0001f)
                    Sweep(previous, delta.normalized, distance, distance / Time.fixedDeltaTime);

                previousPositions[i] = current;
            }
        }

        public void ConfigureForTests(int attackerId, string weaponId, WeaponMarkerSet markers)
        {
            this.attackerId = attackerId;
            this.weaponId = weaponId;
            markerSet = markers;
            previousPositions = new Vector3[markerSet.Count];
        }

        private void Sweep(Vector3 origin, Vector3 direction, float distance, float velocity)
        {
            var hits = Physics.SphereCastAll(origin, sweepRadius, direction, distance, hurtboxLayers, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                var hurtbox = hit.collider.GetComponentInParent<HurtboxAuthoring>();
                if (hurtbox == null || hurtbox.Owner == null)
                    continue;

                HitProposed?.Invoke(new HitProposal(
                    attackerId,
                    hurtbox.Owner.CombatantId,
                    weaponId,
                    hurtbox.Region,
                    velocity,
                    Time.time,
                    hurtbox.Owner.IsPlayer));
            }
        }

        private void EnsurePreviousBuffer()
        {
            if (markerSet == null)
                markerSet = GetComponent<WeaponMarkerSet>();

            if (previousPositions.Length != markerSet.Count)
                previousPositions = new Vector3[markerSet.Count];
        }

        private void CapturePreviousPositions()
        {
            for (var i = 0; i < markerSet.Count; i++)
                previousPositions[i] = markerSet.GetMarker(i).position;
        }
    }
}
```

- [ ] **Step 5: Run Play Mode test to verify it passes**

Run the Play Mode verification command.

Expected: PASS for `SweepAcrossHurtbox_EmitsHitProposal`.

- [ ] **Step 6: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Runtime/Weapons Assets/BloodAndGlory/Combat/Tests.PlayMode/WeaponSweepDriverPlayModeTests.cs
git commit -m "feat: add weapon sweep proposals"
```

### Task 7: Enemy Runtime Controller

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Runtime/Enemy/EnemyCombatController.cs`

- [ ] **Step 1: Add enemy controller**

Create `Assets/BloodAndGlory/Combat/Runtime/Enemy/EnemyCombatController.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using UnityEngine;
using UnityEngine.AI;

namespace BloodAndGlory.Combat.Runtime.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(CombatantAuthoring))]
    public sealed class EnemyCombatController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private EnemyProfileAsset enemyProfile;
        [SerializeField] private AttackDefinitionAsset attackDefinition;

        private readonly EnemyDecisionService decisionService = new EnemyDecisionService();
        private NavMeshAgent agent;
        private Animator animator;
        private EnemyCombatState state = EnemyCombatState.Idle;
        private float stateEnteredAt;

        public EnemyCombatState State => state;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            agent.updateRotation = true;
        }

        private void Update()
        {
            if (target == null || enemyProfile == null)
                return;

            var profile = enemyProfile.ToData();
            var distance = Vector3.Distance(transform.position, target.position);
            var next = decisionService.Decide(state, profile, distance, Time.time - stateEnteredAt, Random.value);
            if (next != state)
                EnterState(next);

            ApplyState(profile);
        }

        public void ConfigureForTests(Transform target, EnemyProfileAsset profile)
        {
            this.target = target;
            enemyProfile = profile;
        }

        public void Kill()
        {
            EnterState(EnemyCombatState.Dead);
        }

        private void EnterState(EnemyCombatState next)
        {
            state = next;
            stateEnteredAt = Time.time;
            animator.SetInteger("CombatState", (int)state);
        }

        private void ApplyState(EnemyProfileData profile)
        {
            switch (state)
            {
                case EnemyCombatState.Approach:
                    agent.isStopped = false;
                    agent.stoppingDistance = profile.PreferredAttackDistance;
                    agent.SetDestination(target.position);
                    animator.SetFloat("Speed", agent.velocity.magnitude);
                    break;
                case EnemyCombatState.Telegraph:
                case EnemyCombatState.AttackCommit:
                case EnemyCombatState.Recover:
                case EnemyCombatState.Block:
                case EnemyCombatState.Parry:
                case EnemyCombatState.Stagger:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    break;
                case EnemyCombatState.Dead:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    animator.SetBool("Dead", true);
                    enabled = false;
                    break;
                default:
                    agent.isStopped = true;
                    animator.SetFloat("Speed", 0f);
                    break;
            }
        }
    }
}
```

- [ ] **Step 2: Run Edit Mode compile check**

Run the Edit Mode verification command.

Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Runtime/Enemy/EnemyCombatController.cs
git commit -m "feat: add enemy combat controller"
```

### Task 8: Debug Overlay

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Runtime/Debug/CombatDebugOverlay.cs`

- [ ] **Step 1: Add overlay component**

Create `Assets/BloodAndGlory/Combat/Runtime/Debug/CombatDebugOverlay.cs`:

```csharp
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
```

- [ ] **Step 2: Run Edit Mode compile check**

Run the Edit Mode verification command.

Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Runtime/Debug/CombatDebugOverlay.cs
git commit -m "feat: add combat debug overlay"
```

### Task 9: Editor Content Builder

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs`
- Create assets through Unity menu or script execution:
  - `Assets/BloodAndGlory/CombatContent/Profiles/BroadswordProfile.asset`
  - `Assets/BloodAndGlory/CombatContent/Profiles/PeasantWeakProfile.asset`
  - `Assets/BloodAndGlory/CombatContent/Attacks/Peasant_Broadsword_Attack_01.asset`
  - `Assets/BloodAndGlory/CombatContent/Materials/Training/TrainingWhite.mat`
  - `Assets/BloodAndGlory/CombatContent/Scenes/CombatTrainingScene.unity`

- [ ] **Step 1: Add content builder**

Create `Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs`:

```csharp
using BloodAndGlory.Combat.Runtime.Authoring;
using BloodAndGlory.Combat.Runtime.Debug;
using BloodAndGlory.Combat.Runtime.Enemy;
using BloodAndGlory.Combat.Runtime.Weapons;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BloodAndGlory.Combat.Editor
{
    public static class CombatContentBuilder
    {
        private const string Root = "Assets/BloodAndGlory/CombatContent";
        private const string TrainingScenePath = Root + "/Scenes/CombatTrainingScene.unity";
        private const string SourceBroadswordPath = "Assets/SyntyStudios/PolygonKnights/Prefabs/Weapons/SM_Wep_Broadsword_01.prefab";
        private const string SourcePeasantPath = "Assets/SyntyStudios/PolygonAdventure/Prefabs/Characters/Character_Peasant_Brown.prefab";
        private const string SourceXrOriginPath = "Assets/Samples/XR Interaction Toolkit/3.4.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        private const string CombatBroadswordPath = Root + "/Prefabs/Weapons/Broadsword_Combat.prefab";
        private const string CombatPeasantPath = Root + "/Prefabs/Enemies/PeasantBrown_Combat.prefab";

        [MenuItem("Blood And Glory/Combat/Rebuild Training Slice Content")]
        public static void Rebuild()
        {
            EnsureFolders();
            var weaponProfile = CreateOrReplaceAsset<WeaponProfileAsset>(Root + "/Profiles/BroadswordProfile.asset");
            var enemyProfile = CreateOrReplaceAsset<EnemyProfileAsset>(Root + "/Profiles/PeasantWeakProfile.asset");
            var attack = CreateOrReplaceAsset<AttackDefinitionAsset>(Root + "/Attacks/Peasant_Broadsword_Attack_01.asset");
            var material = CreateTrainingMaterial();

            CreateCombatPrefabs();
            CreateTrainingScene(material, enemyProfile, attack, weaponProfile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "BloodAndGlory");
            CreateFolder("Assets/BloodAndGlory", "CombatContent");
            CreateFolder(Root, "Scenes");
            CreateFolder(Root, "Prefabs");
            CreateFolder(Root + "/Prefabs", "Weapons");
            CreateFolder(Root + "/Prefabs", "Enemies");
            CreateFolder(Root, "Profiles");
            CreateFolder(Root, "Attacks");
            CreateFolder(Root, "Materials");
            CreateFolder(Root + "/Materials", "Training");
            CreateFolder(Root, "Debug");
        }

        private static void CreateFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static T CreateOrReplaceAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material CreateTrainingMaterial()
        {
            const string path = Root + "/Materials/Training/TrainingWhite.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = Color.white;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void CreateCombatPrefabs()
        {
            CreateBroadswordPrefab();
            CreatePeasantPrefab();
        }

        private static void CreateBroadswordPrefab()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourceBroadswordPath);
            if (source == null)
                throw new System.IO.FileNotFoundException("Broadsword source prefab missing.", SourceBroadswordPath);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = "Broadsword_Combat";

            if (instance.GetComponent<Rigidbody>() == null)
            {
                var rigidbody = instance.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = false;
            }

            if (instance.GetComponent<XRGrabInteractable>() == null)
                instance.AddComponent<XRGrabInteractable>();

            var markerSet = instance.GetComponent<WeaponMarkerSet>();
            if (markerSet == null)
                markerSet = instance.AddComponent<WeaponMarkerSet>();

            if (instance.GetComponent<WeaponSweepDriver>() == null)
                instance.AddComponent<WeaponSweepDriver>();

            var markerRoot = new GameObject("Combat Markers");
            markerRoot.transform.SetParent(instance.transform, false);
            var guard = CreateMarker(markerRoot.transform, "Guard", new Vector3(0f, 0f, -0.25f));
            var mid = CreateMarker(markerRoot.transform, "Mid Blade", new Vector3(0f, 0f, 0.15f));
            var upper = CreateMarker(markerRoot.transform, "Upper Blade", new Vector3(0f, 0f, 0.45f));
            var tip = CreateMarker(markerRoot.transform, "Tip", new Vector3(0f, 0f, 0.75f));
            markerSet.ConfigureForTests(new[] { guard, mid, upper, tip });

            AssetDatabase.DeleteAsset(CombatBroadswordPath);
            PrefabUtility.SaveAsPrefabAsset(instance, CombatBroadswordPath);
            Object.DestroyImmediate(instance);
        }

        private static void CreatePeasantPrefab()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePeasantPath);
            if (source == null)
                throw new System.IO.FileNotFoundException("Peasant source prefab missing.", SourcePeasantPath);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = "PeasantBrown_Combat";

            var combatant = instance.GetComponent<CombatantAuthoring>();
            if (combatant == null)
                combatant = instance.AddComponent<CombatantAuthoring>();
            combatant.ConfigureForTests(10, isPlayer: false, maxHitPoints: 100);

            if (instance.GetComponent<NavMeshAgent>() == null)
                instance.AddComponent<NavMeshAgent>();

            if (instance.GetComponent<EnemyCombatController>() == null)
                instance.AddComponent<EnemyCombatController>();

            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso Hurtbox";
            torso.transform.SetParent(instance.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            torso.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
            var renderer = torso.GetComponent<MeshRenderer>();
            Object.DestroyImmediate(renderer);
            var hurtbox = torso.AddComponent<HurtboxAuthoring>();
            hurtbox.ConfigureForTests(combatant, BloodAndGlory.Combat.Core.HurtboxRegion.Torso);

            AssetDatabase.DeleteAsset(CombatPeasantPath);
            PrefabUtility.SaveAsPrefabAsset(instance, CombatPeasantPath);
            Object.DestroyImmediate(instance);
        }

        private static Transform CreateMarker(Transform parent, string name, Vector3 localPosition)
        {
            var marker = new GameObject(name).transform;
            marker.SetParent(parent, false);
            marker.localPosition = localPosition;
            return marker;
        }

        private static void CreateTrainingScene(
            Material material,
            EnemyProfileAsset enemyProfile,
            AttackDefinitionAsset attack,
            WeaponProfileAsset weaponProfile)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Training Floor";
            floor.transform.localScale = new Vector3(12f, 0.1f, 12f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = material;
            floor.AddComponent<NavMeshSurface>();

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var playerSpawn = new GameObject("Player Spawn");
            playerSpawn.transform.position = new Vector3(0f, 0.1f, -3f);

            var enemySpawn = new GameObject("Enemy Spawn");
            enemySpawn.transform.position = new Vector3(0f, 0.1f, 2f);

            var xrOriginSource = AssetDatabase.LoadAssetAtPath<GameObject>(SourceXrOriginPath);
            if (xrOriginSource != null)
            {
                var xrOrigin = (GameObject)PrefabUtility.InstantiatePrefab(xrOriginSource);
                xrOrigin.name = "XR Origin (Combat Training)";
                xrOrigin.transform.position = playerSpawn.transform.position;
            }

            var weaponSource = AssetDatabase.LoadAssetAtPath<GameObject>(CombatBroadswordPath);
            if (weaponSource != null)
            {
                var weapon = (GameObject)PrefabUtility.InstantiatePrefab(weaponSource);
                weapon.name = "Player Broadsword";
                weapon.transform.position = new Vector3(0.6f, 0.9f, -2.2f);
                var sweep = weapon.GetComponent<WeaponSweepDriver>();
                var serializedSweep = new SerializedObject(sweep);
                serializedSweep.FindProperty("attackerId").intValue = 1;
                serializedSweep.FindProperty("weaponId").stringValue = weaponProfile.ToData().Id;
                serializedSweep.ApplyModifiedPropertiesWithoutUndo();
            }

            EnemyCombatController enemyController = null;
            var enemySource = AssetDatabase.LoadAssetAtPath<GameObject>(CombatPeasantPath);
            if (enemySource != null)
            {
                var enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemySource);
                enemy.name = "PeasantBrown_Combat";
                enemy.transform.position = enemySpawn.transform.position;
                enemyController = enemy.GetComponent<EnemyCombatController>();
                var serializedEnemy = new SerializedObject(enemyController);
                serializedEnemy.FindProperty("target").objectReferenceValue = playerSpawn.transform;
                serializedEnemy.FindProperty("enemyProfile").objectReferenceValue = enemyProfile;
                serializedEnemy.FindProperty("attackDefinition").objectReferenceValue = attack;
                serializedEnemy.ApplyModifiedPropertiesWithoutUndo();
            }

            var debug = new GameObject("Combat Debug Overlay");
            var overlay = debug.AddComponent<CombatDebugOverlay>();
            if (enemyController != null)
            {
                var serializedOverlay = new SerializedObject(overlay);
                serializedOverlay.FindProperty("enemy").objectReferenceValue = enemyController;
                serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.SaveScene(scene, TrainingScenePath);
            AddTrainingSceneToBuildSettings();
        }

        private static void AddTrainingSceneToBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(TrainingScenePath, enabled: true)
            };
            EditorBuildSettings.scenes = scenes;
        }
    }
}
```

- [ ] **Step 2: Run Edit Mode compile check**

Run the Edit Mode verification command.

Expected: PASS.

- [ ] **Step 3: Build content through Unity batchmode**

Run:

```bash
/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/ewanlynch/Documents/GitHub/BloodAndGlory -executeMethod BloodAndGlory.Combat.Editor.CombatContentBuilder.Rebuild -quit
```

Expected: the content assets and `CombatTrainingScene.unity` exist under `Assets/BloodAndGlory/CombatContent`, and `ProjectSettings/EditorBuildSettings.asset` contains the enabled training scene.

- [ ] **Step 4: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs Assets/BloodAndGlory/CombatContent ProjectSettings/EditorBuildSettings.asset
git commit -m "feat: build combat training content"
```

### Task 10: Prefab Validator

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Editor/CombatPrefabValidator.cs`
- Create: `Assets/BloodAndGlory/Combat/Tests.EditMode/CombatPrefabValidatorTests.cs`

- [ ] **Step 1: Write validator tests**

Create `Assets/BloodAndGlory/Combat/Tests.EditMode/CombatPrefabValidatorTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run test to verify it fails**

Run the Edit Mode verification command.

Expected: FAIL because `CombatPrefabValidator` does not exist.

- [ ] **Step 3: Add validator**

Create `Assets/BloodAndGlory/Combat/Editor/CombatPrefabValidator.cs`:

```csharp
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
```

- [ ] **Step 4: Run tests to verify they pass**

Run the Edit Mode verification command.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Editor/CombatPrefabValidator.cs Assets/BloodAndGlory/Combat/Tests.EditMode/CombatPrefabValidatorTests.cs
git commit -m "feat: validate combat prefabs"
```

### Task 11: Training Scene Smoke Test

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs`

- [ ] **Step 1: Write scene smoke test**

Create `Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs`:

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BloodAndGlory.Combat.Tests.PlayMode
{
    public sealed class CombatTrainingSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator CombatTrainingScene_Loads()
        {
            yield return SceneManager.LoadSceneAsync("CombatTrainingScene", LoadSceneMode.Single);
            var scene = SceneManager.GetActiveScene();

            Assert.AreEqual("CombatTrainingScene", scene.name);
            Assert.IsNotNull(GameObject.Find("Training Floor"));
            Assert.IsNotNull(GameObject.Find("Combat Debug Overlay"));
        }
    }
}
```

- [ ] **Step 2: Run Play Mode test**

Run the Play Mode verification command.

Expected: PASS for `CombatTrainingScene_Loads`.

- [ ] **Step 3: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs
git commit -m "test: add combat training scene smoke test"
```

### Task 12: Manual Quest 3 Validation Checklist

**Files:**
- Create: `docs/combat/quest3-combat-training-checklist.md`

- [ ] **Step 1: Add manual checklist**

Create `docs/combat/quest3-combat-training-checklist.md`:

```markdown
# Quest 3 Combat Training Manual Checklist

Run this checklist on Quest 3 standalone after automated Edit Mode and Play Mode tests pass.

## Setup

- Build target is Android.
- Scene is `CombatTrainingScene`.
- Player starts near the broadsword.
- One `PeasantBrown_Combat` enemy starts across the training floor.
- Combat debug overlay is visible in editor mirror or headset.

## Weapon Feel

- Broadsword follows the hand responsively enough for comfortable play.
- Slow contact produces chip/contact feedback.
- Committed swings produce visibly stronger damage.
- Wrist flicking does not dominate the fight.

## Hit And Damage

- Debug overlay shows last velocity, region, damage, and event.
- Head, torso, arm, and leg hits are distinguishable through debug output.
- Holding the blade inside the enemy does not repeatedly tick damage.

## Defense

- Player blocks enemy attacks with the broadsword.
- Blocked enemy attacks produce haptics, debug events, and no player damage.
- Parry events are readable in debug output.
- Peasant blocks or parries rarely.

## Enemy

- Peasant approaches using NavMesh.
- Peasant stops before in-place attacks.
- Telegraph is readable.
- Recovery is generous.
- Death stops combat behavior.

## Comfort And Performance

- Frame stability feels acceptable on Quest 3.
- Haptics are not noisy or constant.
- Debug overlay does not obscure basic play.
- No repeated console errors appear during the session.
```

- [ ] **Step 2: Commit**

```bash
git add docs/combat/quest3-combat-training-checklist.md
git commit -m "docs: add quest 3 combat checklist"
```

### Task 13: Full Verification And Handoff

**Files:**
- No new files.

- [ ] **Step 1: Run Edit Mode suite**

Run:

```bash
/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/ewanlynch/Documents/GitHub/BloodAndGlory -runTests -testPlatform EditMode -testResults /Users/ewanlynch/Documents/GitHub/BloodAndGlory/TestResults-EditMode.xml -quit
```

Expected: PASS.

- [ ] **Step 2: Run Play Mode suite**

Run:

```bash
/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/ewanlynch/Documents/GitHub/BloodAndGlory -runTests -testPlatform PlayMode -testResults /Users/ewanlynch/Documents/GitHub/BloodAndGlory/TestResults-PlayMode.xml -quit
```

Expected: PASS.

- [ ] **Step 3: Inspect git status**

Run:

```bash
git status --short
```

Expected: only intentional Unity-generated metadata or user-existing dirty assets remain. Do not revert unrelated user changes.

- [ ] **Step 4: Summarize remaining manual work**

Write a final implementation summary with:

```text
Implemented:
- Combat core and tests
- Runtime weapon sweep
- Enemy controller
- Debug overlay
- Training content builder
- Scene smoke tests

Automated verification:
- Edit Mode: PASS
- Play Mode: PASS

Manual verification needed:
- Quest 3 checklist in docs/combat/quest3-combat-training-checklist.md
```
