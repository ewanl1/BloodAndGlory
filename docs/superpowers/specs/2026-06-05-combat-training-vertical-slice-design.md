# Blood and Glory Combat Training Vertical Slice TDD

## Summary

This document defines the first project-specific combat technical design for **Blood and Glory**, a Unity VR gladiator combat game. It uses the existing combat research report in `docs/combat/research/combat-deep-research-report.md` as the architectural spine, but narrows the scope to a playable, testable Quest 3 combat training slice.

The recommended approach is a **vertical slice with a core architecture spine**. The first slice should be small enough to implement and tune in VR, while still establishing stable concepts for future weapons, enemies, player body damage, dismemberment, ragdoll death, and richer AI.

## Targets And Scope

The primary target is **Quest 3 standalone**. PC VR should remain compatible with the same architecture, but PC VR is not the performance budget. Quest 2 is out of scope for this design.

The first combat slice is built in a new training scene, not the current colosseum scene. The training scene should be plain and controlled, similar in spirit to a fighting-game training stage: neutral floor, simple white or gray blockout geometry, clear boundaries, predictable lighting, known spawn points, and minimal scenery.

The slice includes:

- A new `CombatTrainingScene`.
- The XRI player rig.
- The existing grabbable broadsword as the first player weapon.
- One `Character_Peasant_Brown` enemy variant.
- Enemy broadsword attacks with no shield.
- NavMesh approach movement and in-place attack animations.
- Velocity-scaled damage.
- Total HP with regional hit metadata and multipliers.
- Player and enemy blocking.
- Enemy parry/block capability used rarely because this is a weak first opponent.
- A combat debug overlay.
- Automated tests for deterministic combat rules and integration smoke tests where practical.

The slice does not include:

- Quest 2 support.
- Player HP, player death, or a full player body hurtbox rig.
- Dismemberment.
- Multi-enemy combat.
- Heavy weapons.
- Lunge attacks as required behavior.
- Shield enemies.
- Ragdoll death as a completion gate.

These are future extension points, not first-slice obligations.

## Scene Design

Create the first combat scene at:

```text
Assets/BloodAndGlory/CombatContent/Scenes/CombatTrainingScene.unity
```

The scene is a combat lab. It should minimize environmental constraints so combat behavior is easy to observe, reproduce, and test. It should include:

- One XRI player rig copied or derived from the current `BasicScene` setup.
- One player broadsword pickup or equipped broadsword.
- One `PeasantBrown_Combat` enemy prefab variant.
- A simple NavMesh on the training floor.
- Debug spawn/reset points.
- A visible or editor-friendly combat debug overlay.
- Training-stage blockout materials and boundary markers.

`BasicScene` remains useful as an integration target and as a reference for the current rig, weapon pickup, colosseum, gates, and arena staging. It is not the first combat tuning scene.

## Code Architecture

Combat code should live under a project-owned namespace and folder boundary:

```text
Assets/BloodAndGlory/Combat/
  Core/
  Runtime/
  Editor/
  Tests.EditMode/
  Tests.PlayMode/
```

The core should be plain C# wherever possible. Unity-specific runtime components should adapt XR input, physics queries, animation, haptics, audio, VFX, scene references, and debug UI into the core rules. No important combat rule should depend on a specific scene.

Key core concepts:

- `WeaponProfile`: weapon tuning, velocity damage curve, minimum chip damage, maximum damage, handling profile, block/parry settings, duplicate-hit timing.
- `HitProposal`: raw contact information from a weapon sweep before combat rules are resolved.
- `CombatResolver`: the authoritative rule engine for damage, block/parry, duplicate-hit suppression, health, death, and emitted events.
- `HealthState`: total HP and alive/dead state.
- `HurtboxRegion`: head, torso, arms, and legs, each with a region multiplier.
- `AttackDefinition`: enemy attack timing, telegraph, active windows, recovery, damage profile, and movement policy.
- `EnemyProfile`: perception range, preferred distance, approach speed, attack choices, defensive chance, cooldowns, and weak-opponent difficulty tuning.
- `CombatEvent`: resolved outcomes consumed by animation, VFX, audio, haptics, debug UI, tests, and future telemetry.

## Weapon Handling

The first weapon is the existing broadsword, treated as a medium one-handed weapon. It should be converted into a combat-ready prefab variant rather than modifying the source asset directly.

Weapon handling should be profile-driven. The architecture must support different handling profiles later, such as fast daggers and heavy two-handed weapons, but the first slice only needs a broadsword profile.

The broadsword should feel responsive enough for VR comfort while still discouraging wrist-flick exploits. Its profile should define:

- Follow responsiveness.
- Velocity sampling method.
- Minimum chip damage.
- Damage curve by swing velocity.
- Maximum damage.
- Block/parry configuration.
- Duplicate-hit suppression rules.
- Haptic intensity mapping.

## Hit Detection And Damage

Authoritative player weapon hits should come from fixed-step weapon sweeps, not from `OnCollisionEnter` or `OnTriggerEnter` as the sole source of truth.

The runtime should track several broadsword markers, such as guard, mid-blade, upper blade, and tip. Each fixed step compares previous and current marker positions, performs sphere or capsule sweep queries against combat hurtbox layers, and submits `HitProposal`s to the `CombatResolver`.

Damage is velocity-scaled:

- A valid blade contact can produce a minimum chip damage of `1` in the initial broadsword tuning.
- Higher swing velocity increases damage through the broadsword `WeaponProfile` curve.
- Damage is capped by the weapon profile.
- Region multipliers apply after base damage.
- Low-velocity contact produces chip/contact feedback without a full hit reaction unless later tuning deliberately changes that behavior.

The enemy has one authoritative total HP pool. Regional damage should still be recorded as metadata for hit feedback, future tuning, and future dismemberment support. Dismemberment does not exist in the first slice.

The resolver must enforce duplicate-hit suppression by attacker, target, weapon, and attack/contact window. Resting the blade inside a hurtbox, sawing, or repeated physics-frame overlap should not tick damage continuously.

## Health And Regions

Use a total HP model for alive/dead state. Region information is part of each hit and may maintain accumulated per-region damage for future systems, but region totals do not decide death in the first slice.

Initial regions:

- `Head`
- `Torso`
- `Arm`
- `Leg`

Each region has a multiplier. Exact values are tuning data and should live in profiles or authoring assets, not hard-coded in scene components.

## Blocking And Parry

Blocking and parrying are symmetric combat rules. Both player and enemy can block with a broadsword, and both can produce `Blocked` or `Parried` outcomes. The difference is input and decision-making:

- The player blocks through weapon pose, lane, or motion.
- The peasant blocks/parries through weak AI states.

A block succeeds when an incoming active weapon sweep intersects the defender's valid block lane or block volume during an active block state.

A parry is a stricter block with tighter timing and a stronger outcome, such as attacker stagger, interrupted attack, or extended recovery. For the first slice, blocking and semantic parry outcomes are required. If available animations do not retarget cleanly, runtime presentation may temporarily reuse a block or hit reaction, but the `Parried` outcome must still exist in core rules, enemy AI decisions, debug output, and tests.

Player blocking is required even though player HP is deferred. Enemy attacks should resolve into `Blocked` or `WouldHitPlayer` debug events rather than real player damage. This lets the team tune block reliability, haptics, sparks, sounds, and enemy recovery before building the player body hurtbox rig.

The first peasant can block/parry, but rarely. Defensive chance, timing accuracy, and recovery should be parameters in `EnemyProfile`.

## Enemy Movement And AI

The first enemy is:

```text
Assets/SyntyStudios/PolygonAdventure/Prefabs/Characters/Character_Peasant_Brown.prefab
```

Create a combat-ready prefab variant under:

```text
Assets/BloodAndGlory/CombatContent/Prefabs/Enemies/PeasantBrown_Combat.prefab
```

The peasant wields the same broadsword class as the player and has no shield.

Enemy behavior is a code-side FSM. The Animator presents the behavior but does not own combat truth.

Required first-slice states:

- `Idle`
- `Approach`
- `Telegraph`
- `AttackCommit`
- `Recover`
- `Block`
- `Parry`
- `Stagger`
- `Dead`

Optional future states:

- `StrafeOrReposition`
- `Retreat`
- `Taunt`
- `Feint`

Movement ownership is strict:

- `NavMeshAgent` owns approach and repositioning.
- The agent stops during in-place attack commits.
- Root motion is disabled for the first peasant.
- Animator parameters mirror movement and combat state for presentation.
- The first peasant uses in-place attacks only.

Future attack definitions may opt into movement policies such as `None`, `ScriptedStep`, or `RootMotion`. The first broadsword peasant uses `None`. Lunge attacks are intentionally deferred but supported by the `AttackDefinition` movement-policy design.

The peasant AI should be weak and readable. It should approach into range, choose from one or two broadsword attacks, telegraph clearly, recover generously, and only occasionally block or parry.

## Death

The first slice should support enemy death as a resolved combat outcome. Ragdoll death is desirable, but it is not required for the first completion gate because `Character_Peasant_Brown` does not currently include a ragdoll setup.

The initial required death behavior is:

- Enemy HP reaches zero.
- Combat input and AI stop.
- A death event is emitted.
- The enemy plays a death animation or enters a non-combat dead state.
- The debug overlay records the final hit and death event.

The architecture should support `AnimatedDeath` and `RagdollDeath` modes so ragdoll can be added later without changing the resolver. Ragdoll setup, joint tuning, and animation-to-physics handoff belong in a later implementation phase.

## Player Damage Deferral

Player damage, player death, and the full player body model are out of scope for the first slice. This avoids inventing a fake player hurtbox rig that will be thrown away later.

The architecture should still be symmetric. The eventual player body should use the same `Combatant`, `HealthState`, and `HurtboxRegion` concepts as enemies.

Until then, enemy attacks resolve to:

- `Blocked` when the player successfully blocks.
- `WouldHitPlayer` when the enemy attack would have hit an unblocked player.

These events should drive debug output, haptics, VFX, and tuning without applying player HP damage.

## Combat Debug Overlay

The `CombatTrainingScene` should include a debug overlay as part of the slice. It can be editor-visible first and VR-visible if cheap enough.

The overlay should show:

- Enemy FSM state.
- Active attack window.
- Current weapon profile.
- Last weapon velocity.
- Last hit region.
- Resolved damage.
- Block or parry outcome.
- Duplicate-hit suppression status.
- Last combat event.

This overlay is required because VR melee tuning otherwise becomes hard to diagnose. It should be treated as development tooling, not player-facing UI.

## Asset Organization

Avoid a broad asset-folder cleanup before the combat slice. Unity asset moves can break references, and the first implementation may reveal which animations and prefabs are actually useful.

Create a project-owned combat content area:

```text
Assets/BloodAndGlory/CombatContent/
  Scenes/
  Prefabs/
    Weapons/
    Enemies/
  Profiles/
  Attacks/
  Materials/
    Training/
  Debug/
```

Existing asset-pack files stay where they are. Create prefab variants, profiles, and curated references under `CombatContent`.

Initial combat content:

- `Scenes/CombatTrainingScene.unity`
- `Prefabs/Weapons/Broadsword_Combat.prefab`
- `Prefabs/Enemies/PeasantBrown_Combat.prefab`
- `Profiles/BroadswordProfile.asset`
- `Profiles/PeasantWeakProfile.asset`
- `Attacks/Peasant_Broadsword_Attack_01.asset`
- Training-stage materials and debug assets.

After the slice stabilizes, asset cleanup can be planned as a separate migration with prefab validation.

## Validation Strategy

Use automated tests wherever the behavior is deterministic, and manual Quest 3 checks where VR feel matters.

Edit Mode tests should cover:

- Damage scaling by velocity.
- Minimum chip damage.
- Damage caps.
- Region multipliers.
- Duplicate-hit suppression.
- Block outcomes.
- Parry outcomes.
- HP and death transitions.
- Attack-window ordering.
- Enemy FSM state transitions that do not require scene simulation.

Play Mode tests should cover:

- Weapon sweep fixtures against hurtboxes.
- Start-overlap and fast-swing hit detection.
- Block lane or block volume detection.
- Enemy approach-to-attack behavior in a minimal scene.
- `CombatTrainingScene` smoke checks.
- Prefab authoring validation where Unity objects are required.

Manual Quest 3 checks should cover:

- Broadsword responsiveness.
- Broadsword weight feel.
- Weak taps versus committed swings.
- Block reliability.
- Parry event readability.
- Enemy telegraph clarity.
- Hit reaction readability.
- Haptic timing.
- VFX and audio timing.
- Comfort and frame stability.

## Completion Criteria

The first slice is complete when:

- `CombatTrainingScene` loads with the XRI player rig, broadsword, one peasant enemy, and debug overlay.
- The player can strike the peasant with velocity-scaled broadsword damage.
- The peasant uses total HP with region multipliers and dies when HP reaches zero.
- Duplicate-hit suppression prevents continuous overlap damage.
- The peasant approaches using NavMesh and attacks in place.
- The player can block enemy attacks.
- The peasant can rarely block/parry player attacks, tuned as a weak opponent.
- Enemy attacks produce `Blocked` or `WouldHitPlayer` events, not real player damage.
- Automated Edit Mode tests cover the core resolver rules.
- Play Mode smoke tests cover the combat training scene or minimal combat fixtures.
- Manual Quest 3 testing confirms the slice is playable, readable, and comfortable enough for further tuning.

## Future Extensions

Future work can build on the slice without changing core architecture:

- Player body hurtboxes, player HP, and player death.
- Ragdoll death for enemies.
- Dismemberment using accumulated region damage.
- Heavy and light weapon profiles.
- Dagger, two-handed sword, polearm, and shield profiles.
- Scripted lunge or root-motion attack movement policies.
- Stronger enemy archetypes.
- Shield enemies.
- Multi-enemy arena combat.
- Colosseum integration in `BasicScene`.
- PC VR fidelity scaling.
