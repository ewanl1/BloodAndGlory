# Combat Training Validation Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the failed manual Quest 3 validation items from the original combat training vertical slice without expanding the scope beyond `docs/superpowers/specs/2026-06-05-combat-training-vertical-slice-design.md`.

**Architecture:** Keep deterministic combat decisions in `Assets/BloodAndGlory/Combat/Core`. Add the missing Unity runtime glue that connects weapon sweep proposals to `CombatResolver`, scene combat state, enemy presentation, and VR-visible debug output. Preserve the user's manual scene fixes: `XR Combat Rig`, sword gravity, and sword attach point.

**Tech Stack:** Unity 6000.4.9f1, C#, Unity Test Framework, XR Interaction Toolkit 3.4.1, AI Navigation 2.0.12, URP 17.4.0, Quest 3 standalone manual validation.

---

## Spec And Plan Alignment

This addendum closes gaps found during Task 12 of `docs/superpowers/plans/2026-06-06-combat-training-vertical-slice-implementation.md`.

Keep these requirements fixed:

- `CombatTrainingScene` remains the controlled training scene.
- Player rig is copied or derived from the working arena rig.
- Broadsword contacts submit fixed-step `HitProposal`s to `CombatResolver`.
- Debug output shows velocity, region, damage, event, enemy state, and block/parry outcome.
- Peasant approaches with NavMesh, attacks in place, and rarely blocks/parries.
- Enemy attacks produce `Blocked` or `WouldHitPlayer`, not real player HP damage.

Keep these items out of scope:

- Quest 2 support.
- Full player body model or real player HP/death.
- Lunge/root-motion attacks.
- Arena art polish.
- Dismemberment.

## Verification Commands

Use the Unity test runner without `-quit`. The Test Framework starts runs on the next editor update and exits the editor itself.

```bash
/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/ewanlynch/Documents/GitHub/BloodAndGlory -runTests -testPlatform EditMode -assemblyNames BloodAndGlory.Combat.Tests.EditMode -testResults /Users/ewanlynch/Documents/GitHub/BloodAndGlory/TestResults-EditMode.xml -logFile /private/tmp/bloodandglory-editmode-validation-fixes.log
```

```bash
/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/ewanlynch/Documents/GitHub/BloodAndGlory -runTests -testPlatform PlayMode -assemblyNames BloodAndGlory.Combat.Tests.PlayMode -testResults /Users/ewanlynch/Documents/GitHub/BloodAndGlory/TestResults-PlayMode.xml -logFile /private/tmp/bloodandglory-playmode-validation-fixes.log
```

Expected: test result XML contains `failed="0"`.

## File Structure

Create or modify these files:

```text
Assets/BloodAndGlory/Combat/Runtime/
  Training/
    CombatTrainingRuntime.cs
  Debug/
    CombatDebugOverlay.cs
  Enemy/
    EnemyCombatController.cs
  Weapons/
    WeaponSweepDriver.cs
Assets/BloodAndGlory/Combat/Editor/
  CombatContentBuilder.cs
  CombatPrefabValidator.cs
Assets/BloodAndGlory/Combat/Tests.EditMode/
  CombatTrainingRuntimeTests.cs
  CombatPrefabValidatorTests.cs
Assets/BloodAndGlory/Combat/Tests.PlayMode/
  CombatTrainingSceneSmokeTests.cs
Assets/BloodAndGlory/CombatContent/
  XR Combat Rig.prefab
  Scenes/CombatTrainingScene.unity
  Prefabs/Weapons/Broadsword_Combat.prefab
  Prefabs/Enemies/PeasantBrown_Combat.prefab
docs/combat/quest3-combat-training-checklist.md
```

## Task 1: Preserve Manual Scene Fixes As First-Class Content

**Files:**
- Modify: `Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs`
- Modify: `Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs`
- Modify: `Assets/BloodAndGlory/Combat/Editor/CombatPrefabValidator.cs`
- Asset: `Assets/BloodAndGlory/CombatContent/XR Combat Rig.prefab`
- Asset: `Assets/BloodAndGlory/CombatContent/Prefabs/Weapons/Broadsword_Combat.prefab`
- Asset: `Assets/BloodAndGlory/CombatContent/Scenes/CombatTrainingScene.unity`

- [ ] **Step 1: Inspect manual asset state**

Run:

```bash
git status --short -- Assets/BloodAndGlory/CombatContent
```

Expected: status includes `XR Combat Rig.prefab`, modified `Broadsword_Combat.prefab`, and modified `CombatTrainingScene.unity`.

- [ ] **Step 2: Change content builder constants**

In `CombatContentBuilder.cs`, replace the default XRI sample rig source with:

```csharp
private const string SourceXrOriginPath = Root + "/XR Combat Rig.prefab";
```

Keep the old sample prefab path out of the builder so regeneration does not reintroduce the broken generic rig.

- [ ] **Step 3: Preserve sword gravity when regenerating**

In `CreateBroadswordPrefab`, set rigidbody fields whether the component already exists or is newly added:

```csharp
var rigidbody = instance.GetComponent<Rigidbody>();
if (rigidbody == null)
    rigidbody = instance.AddComponent<Rigidbody>();

rigidbody.useGravity = true;
rigidbody.isKinematic = false;
rigidbody.mass = Mathf.Max(1.5f, rigidbody.mass);
rigidbody.drag = Mathf.Max(0.02f, rigidbody.drag);
rigidbody.angularDrag = Mathf.Max(0.05f, rigidbody.angularDrag);
```

Do not overwrite the user's attach point if it already exists on the prefab.

- [ ] **Step 4: Add smoke assertions for the fixed content**

Extend `CombatTrainingSceneSmokeTests.CombatTrainingScene_Loads` with:

```csharp
Assert.IsNotNull(GameObject.Find("XR Combat Rig"));

var sword = GameObject.Find("Player Broadsword");
Assert.IsNotNull(sword);
var rigidbody = sword.GetComponent<Rigidbody>();
Assert.IsNotNull(rigidbody);
Assert.IsTrue(rigidbody.useGravity);
```

- [ ] **Step 5: Add validator checks**

Add validator errors for:

```text
CombatTrainingScene must contain XR Combat Rig.
Broadsword_Combat must have a Rigidbody with useGravity enabled.
Broadsword_Combat must have an XRGrabInteractable.
```

- [ ] **Step 6: Run PlayMode smoke test**

Run the PlayMode verification command.

Expected: `CombatTrainingScene_Loads` passes.

- [ ] **Step 7: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs Assets/BloodAndGlory/Combat/Editor/CombatPrefabValidator.cs Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs Assets/BloodAndGlory/CombatContent
git commit -m "fix: preserve combat training vr rig and sword physics"
```

## Task 2: Add Runtime Combat Coordinator

**Files:**
- Create: `Assets/BloodAndGlory/Combat/Runtime/Training/CombatTrainingRuntime.cs`
- Create: `Assets/BloodAndGlory/Combat/Tests.EditMode/CombatTrainingRuntimeTests.cs`
- Modify: `Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs`
- Modify: `Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs`

- [ ] **Step 1: Write coordinator tests**

Create `CombatTrainingRuntimeTests.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Training;
using NUnit.Framework;

namespace BloodAndGlory.Combat.Tests.EditMode
{
    public sealed class CombatTrainingRuntimeTests
    {
        [Test]
        public void ResolvePlayerHit_AppliesDamageAndReportsDebug()
        {
            var state = new CombatState(new HealthState(100));
            var profile = WeaponProfileData.BroadswordDefaults;
            var hit = new HitProposal(1, 10, "broadsword", HurtboxRegion.Head, 8f, 1f, false);

            var result = CombatTrainingRuntime.ResolvePlayerHitForTests(state, hit, profile, BlockContext.None);

            Assert.Greater(result.Event.Damage, 1);
            Assert.AreEqual(CombatEventType.Damaged, result.Event.Type);
            Assert.Less(result.Health.CurrentHitPoints, state.Health.CurrentHitPoints);
        }
    }
}
```

Expected before implementation: FAIL because `CombatTrainingRuntime` does not exist.

- [ ] **Step 2: Add coordinator component**

Create `Assets/BloodAndGlory/Combat/Runtime/Training/CombatTrainingRuntime.cs`:

```csharp
using BloodAndGlory.Combat.Core;
using BloodAndGlory.Combat.Runtime.Authoring;
using BloodAndGlory.Combat.Runtime.Debug;
using BloodAndGlory.Combat.Runtime.Enemy;
using BloodAndGlory.Combat.Runtime.Weapons;
using UnityEngine;

namespace BloodAndGlory.Combat.Runtime.Training
{
    public sealed class CombatTrainingRuntime : MonoBehaviour
    {
        [SerializeField] private WeaponSweepDriver playerSword;
        [SerializeField] private WeaponProfileAsset playerSwordProfile;
        [SerializeField] private CombatantAuthoring enemyCombatant;
        [SerializeField] private EnemyCombatController enemyController;
        [SerializeField] private CombatDebugOverlay debugOverlay;

        private readonly CombatResolver resolver = new CombatResolver();
        private CombatState combatState;

        private void OnEnable()
        {
            if (enemyCombatant != null)
                combatState = new CombatState(enemyCombatant.InitialHealth);

            if (playerSword != null)
                playerSword.HitProposed += OnPlayerHitProposed;
        }

        private void OnDisable()
        {
            if (playerSword != null)
                playerSword.HitProposed -= OnPlayerHitProposed;
        }

        public static DamageResult ResolvePlayerHitForTests(
            CombatState state,
            HitProposal hit,
            WeaponProfileData profile,
            BlockContext block)
        {
            return new CombatResolver().ResolveHit(state, hit, profile, block);
        }

        private void OnPlayerHitProposed(HitProposal hit)
        {
            if (playerSwordProfile == null)
                return;

            var profile = playerSwordProfile.ToData();
            var result = resolver.ResolveHit(combatState, hit, profile, BlockContext.None);
            debugOverlay?.RecordEvent(result.Event, result.Event.Type == CombatEventType.SuppressedDuplicate);

            if (result.Event.Type != CombatEventType.SuppressedDuplicate)
                combatState = combatState.WithHealth(result.Health).RecordHit(hit, profile);

            if (result.Event.Type == CombatEventType.Died)
                enemyController?.Kill();
        }
    }
}
```

- [ ] **Step 3: Add scene object in content builder**

In `CreateTrainingScene`, after sword, enemy, and overlay are created, add:

```csharp
var runtime = new GameObject("Combat Training Runtime");
var trainingRuntime = runtime.AddComponent<CombatTrainingRuntime>();
var serializedRuntime = new SerializedObject(trainingRuntime);
serializedRuntime.FindProperty("playerSword").objectReferenceValue = sweep;
serializedRuntime.FindProperty("playerSwordProfile").objectReferenceValue = weaponProfile;
serializedRuntime.FindProperty("enemyCombatant").objectReferenceValue = enemy.GetComponent<CombatantAuthoring>();
serializedRuntime.FindProperty("enemyController").objectReferenceValue = enemy.GetComponent<EnemyCombatController>();
serializedRuntime.FindProperty("debugOverlay").objectReferenceValue = overlay;
serializedRuntime.ApplyModifiedPropertiesWithoutUndo();
```

Add `using BloodAndGlory.Combat.Runtime.Training;`.

- [ ] **Step 4: Extend smoke test**

Add:

```csharp
Assert.IsNotNull(GameObject.Find("Combat Training Runtime"));
```

- [ ] **Step 5: Run EditMode and PlayMode tests**

Run both verification commands.

Expected: all tests pass.

- [ ] **Step 6: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Runtime/Training Assets/BloodAndGlory/Combat/Tests.EditMode/CombatTrainingRuntimeTests.cs Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs Assets/BloodAndGlory/CombatContent
git commit -m "feat: connect combat training hit resolution"
```

## Task 3: Replace Placeholder Overlay With VR-Visible Debug Panel

**Files:**
- Modify: `Assets/BloodAndGlory/Combat/Runtime/Debug/CombatDebugOverlay.cs`
- Modify: `Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs`
- Modify: `Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs`

- [ ] **Step 1: Keep `OnGUI` as editor fallback and add world text**

Modify `CombatDebugOverlay` to cache a `TextMesh` child:

```csharp
[SerializeField] private TextMesh worldText;

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
```

Change `OnGUI` labels to read from `BuildText()` lines so editor and VR output stay consistent.

- [ ] **Step 2: Create world text in content builder**

In `CreateTrainingScene`, after creating `Combat Debug Overlay`, add a child:

```csharp
var textObject = new GameObject("Combat Debug Text");
textObject.transform.SetParent(debug.transform, false);
textObject.transform.localPosition = new Vector3(-2.2f, 1.8f, 1.6f);
textObject.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
textObject.transform.localScale = Vector3.one * 0.08f;

var textMesh = textObject.AddComponent<TextMesh>();
textMesh.anchor = TextAnchor.UpperLeft;
textMesh.alignment = TextAlignment.Left;
textMesh.fontSize = 42;
textMesh.color = Color.black;

serializedOverlay.FindProperty("worldText").objectReferenceValue = textMesh;
serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
```

- [ ] **Step 3: Extend smoke test**

Add:

```csharp
Assert.IsNotNull(GameObject.Find("Combat Debug Text"));
```

- [ ] **Step 4: Run PlayMode tests**

Run the PlayMode verification command.

Expected: smoke test passes.

- [ ] **Step 5: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Runtime/Debug/CombatDebugOverlay.cs Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs Assets/BloodAndGlory/Combat/Tests.PlayMode/CombatTrainingSceneSmokeTests.cs Assets/BloodAndGlory/CombatContent
git commit -m "feat: add vr combat debug panel"
```

## Task 4: Put Peasant Into A Valid Animated Idle

**Files:**
- Modify: `Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs`
- Modify: `Assets/BloodAndGlory/Combat/Editor/CombatPrefabValidator.cs`
- Modify: `Assets/BloodAndGlory/Combat/Tests.EditMode/CombatPrefabValidatorTests.cs`
- Asset: `Assets/BloodAndGlory/CombatContent/Prefabs/Enemies/PeasantBrown_Combat.prefab`

- [ ] **Step 1: Add validator test for peasant animator**

Extend `CombatPrefabValidatorTests` with a test that fails if `PeasantBrown_Combat.prefab` has no `Animator.runtimeAnimatorController`.

Expected before implementation: FAIL if the generated combat peasant has no controller.

- [ ] **Step 2: Assign a conservative existing controller**

In `CombatContentBuilder`, add:

```csharp
private const string SourcePeasantAnimatorControllerPath = "Assets/SyntyStudios/PolygonAdventure/Models/Characters/Character.controller";
```

In `CreatePeasantPrefab`, after instantiating the source:

```csharp
var animator = instance.GetComponent<Animator>();
if (animator != null && animator.runtimeAnimatorController == null)
{
    var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SourcePeasantAnimatorControllerPath);
    if (controller != null)
        animator.runtimeAnimatorController = controller;
}
```

If this controller is visually unsuitable in headset, replace it in a later task with a combat-specific controller using the imported one-hand assets. The immediate requirement is to leave T-pose and enter a valid animated pose.

- [ ] **Step 3: Run EditMode tests**

Run the EditMode verification command.

Expected: validator tests pass.

- [ ] **Step 4: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Editor/CombatContentBuilder.cs Assets/BloodAndGlory/Combat/Editor/CombatPrefabValidator.cs Assets/BloodAndGlory/Combat/Tests.EditMode/CombatPrefabValidatorTests.cs Assets/BloodAndGlory/CombatContent/Prefabs/Enemies/PeasantBrown_Combat.prefab
git commit -m "fix: give peasant combat prefab an idle controller"
```

## Task 5: Add First Weak Peasant Attack Loop

**Files:**
- Modify: `Assets/BloodAndGlory/Combat/Runtime/Enemy/EnemyCombatController.cs`
- Modify: `Assets/BloodAndGlory/Combat/Runtime/Training/CombatTrainingRuntime.cs`
- Modify: `Assets/BloodAndGlory/Combat/Runtime/Debug/CombatDebugOverlay.cs`
- Modify: `Assets/BloodAndGlory/Combat/Tests.EditMode/EnemyDecisionServiceTests.cs`
- Modify: `Assets/BloodAndGlory/Combat/Tests.EditMode/CombatTrainingRuntimeTests.cs`

- [ ] **Step 1: Add tests for enemy attack resolution**

Add a test that resolves an enemy attack proposal against a player defender:

```csharp
[Test]
public void ResolveEnemyHit_PlayerDefenderProducesWouldHitPlayer()
{
    var state = new CombatState();
    state.RegisterCombatant(1, HealthState.Alive(100));
    var profile = WeaponProfileData.DefaultBroadsword;
    var hit = new HitProposal(10, 1, "broadsword", HurtboxRegion.Torso, 4f, 1f, true);

    var result = CombatTrainingRuntime.ResolveEnemyHitForTests(state, hit, profile, BlockContext.None);

    Assert.AreEqual(CombatEventType.WouldHitPlayer, result.Event.Type);
    Assert.AreEqual(0, result.DamageApplied);
}
```

Add a second test for the required first-slice block outcome:

```csharp
[Test]
public void ResolveEnemyHit_PlayerBlockProducesBlocked()
{
    var state = new CombatState();
    state.RegisterCombatant(1, HealthState.Alive(100));
    var profile = WeaponProfileData.DefaultBroadsword;
    var hit = new HitProposal(10, 1, "broadsword", HurtboxRegion.Torso, 4f, 1f, true);

    var result = CombatTrainingRuntime.ResolveEnemyHitForTests(
        state,
        hit,
        profile,
        new BlockContext(isBlocking: true, isParryWindowActive: false));

    Assert.AreEqual(CombatEventType.Blocked, result.Event.Type);
    Assert.AreEqual(0, result.DamageApplied);
}
```

Expected before implementation: FAIL because `ResolveEnemyHitForTests` does not exist.

- [ ] **Step 2: Add enemy attack event helper**

In `CombatTrainingRuntime`, add:

```csharp
public static DamageResult ResolveEnemyHitForTests(
    CombatState state,
    HitProposal hit,
    WeaponProfileData profile,
    BlockContext block)
{
    return new CombatResolver().ResolveHit(state, hit, profile, block);
}
```

- [ ] **Step 3: Expose enemy attack window state**

In `EnemyCombatController`, add:

```csharp
public bool IsAttackActive => state == EnemyCombatState.AttackCommit;
public float TimeInState => Time.time - stateEnteredAt;
```

Keep attacks in-place: `agent.isStopped = true` during `Telegraph`, `AttackCommit`, and `Recover`.

- [ ] **Step 4: Emit one enemy attack proposal per attack commit**

Add an event that fires once during each `AttackCommit` state:

```csharp
public event Action<HitProposal> AttackProposed;
```

When entering `AttackCommit`, reset a private `attackProposalSent` flag. During `AttackCommit`, if the flag is false, emit a `HitProposal` using:

```text
AttackerId: enemy combatant id
DefenderId: player placeholder id 1
WeaponId: broadsword
Region: Torso
Velocity: default weak attack velocity from attack/weapon tuning
ContactTime: Time.time
DefenderIsPlayer: true
```

This is intentionally not full enemy weapon collision. It is the first testable player-damage-deferral loop required by the spec: the peasant attacks in place, the runtime resolves the event, and the overlay shows whether the attack would have hit or was blocked.

- [ ] **Step 5: Add minimal player block context**

In `CombatTrainingRuntime`, add serialized references for the player's broadsword transform or `WeaponSweepDriver`, plus configurable block thresholds:

```text
playerBlockDotThreshold: 0.45
playerBlockDistance: 1.25
```

Implement a small `GetPlayerBlockContext()` method that returns `BlockContext.Blocking` when the player broadsword is within range and roughly between the enemy and rig camera. Keep this conservative and debug-friendly; it does not need final block-volume precision yet.

- [ ] **Step 6: Wire enemy attack proposals into the resolver**

In `CombatTrainingRuntime.OnEnable`, subscribe to the enemy controller's `AttackProposed` event. Resolve the proposal with `ResolveEnemyHitForTests` using `GetPlayerBlockContext()`, then record the `Blocked` or `WouldHitPlayer` result in the overlay.

In `OnDisable`, unsubscribe.

- [ ] **Step 7: Record active attack state in overlay**

Add a string to `CombatDebugOverlay`:

```csharp
[SerializeField] private string activeAttack = "None";

public void SetActiveAttack(string value)
{
    activeAttack = string.IsNullOrWhiteSpace(value) ? "None" : value;
}
```

Add `Active Attack: {activeAttack}` to `BuildText()`.

- [ ] **Step 8: Wire active attack debug from runtime**

In `CombatTrainingRuntime.Update`, set:

```csharp
if (enemyController != null)
    debugOverlay?.SetActiveAttack(enemyController.IsAttackActive ? "Peasant Broadsword" : "None");
```

This task still defers full enemy weapon collision and final player body hurtboxes. It does not defer the spec outcome: the peasant must enter an in-place attack commit, emit a single resolvable attack proposal, and produce `Blocked` or `WouldHitPlayer` debug events.

- [ ] **Step 9: Run EditMode and PlayMode tests**

Run both verification commands.

Expected: all automated tests pass.

- [ ] **Step 10: Commit**

```bash
git add Assets/BloodAndGlory/Combat/Runtime/Enemy/EnemyCombatController.cs Assets/BloodAndGlory/Combat/Runtime/Training/CombatTrainingRuntime.cs Assets/BloodAndGlory/Combat/Runtime/Debug/CombatDebugOverlay.cs Assets/BloodAndGlory/Combat/Tests.EditMode
git commit -m "feat: resolve weak peasant attack debug loop"
```

## Task 6: Update Manual Checklist With Actual Validation State

**Files:**
- Modify: `docs/combat/quest3-combat-training-checklist.md`

- [ ] **Step 1: Add preflight checks**

Add:

```markdown
## Preflight

- Physics GameObject SDK is set to PhysX.
- `CombatTrainingScene` uses `XR Combat Rig`.
- Hands are visible in headset.
- The broadsword uses gravity when released.
- The peasant is not in T-pose.
- The combat debug panel is visible from the player start area.
```

- [ ] **Step 2: Add known-deferred notes**

Add:

```markdown
## Deferred By Spec

- No Quest 2 validation.
- No player HP or player death.
- No lunge/root-motion attacks.
- No dismemberment.
```

- [ ] **Step 3: Commit**

```bash
git add docs/combat/quest3-combat-training-checklist.md
git commit -m "docs: update combat training validation checklist"
```

## Task 7: Full Verification And Handoff

**Files:**
- No code changes expected unless verification fails.

- [ ] **Step 1: Run EditMode tests**

Run the EditMode verification command.

Expected: XML has `failed="0"`.

- [ ] **Step 2: Run PlayMode tests**

Run the PlayMode verification command.

Expected: XML has `failed="0"`.

- [ ] **Step 3: Run diff check**

Run:

```bash
git diff --check -- Assets/BloodAndGlory docs/combat ProjectSettings/EditorBuildSettings.asset
```

Expected: no output.

- [ ] **Step 4: Summarize manual Quest 3 work**

Report that the remaining required manual validation is to rerun `docs/combat/quest3-combat-training-checklist.md` on headset, focusing on:

- hands and rig parity
- broadsword gravity and grab attach point
- peasant idle pose
- player sword hit resolution
- debug panel values
- weak peasant approach/attack readability

- [ ] **Step 5: Commit any verification-only doc adjustments**

If no files changed, skip this commit.
