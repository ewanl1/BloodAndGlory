# Agent Notes

This is a Unity project. Prefer working through the Unity MCP/editor when it is available; it gives better signal than shell-only inspection for scene, prefab, console, and Play Mode behavior.

## Unity Workflow

- Use Unity MCP to check editor state, read console errors, run menu items, inspect scenes, and run Play Mode validation.
- Avoid batchmode as the first choice while the editor/MCP is connected. Use batchmode mainly for CI-style clean runs or when the editor is unavailable.
- After script edits, refresh Unity and confirm there are no compile errors before deeper validation.
- Play Mode tests may need Standalone XR initialization disabled in local editor runs if no desktop XR runtime is available. Restore the XR setting after the run.

## Combat Training Slice

- The combat training slice is driven by the spec and plans under `docs/superpowers/`.
- Combat tests live under:
  - `Assets/_BloodAndGlory/Tests/Combat/EditMode`
  - `Assets/_BloodAndGlory/Tests/Combat/PlayMode`
- Use `Blood And Glory/Combat/Rebuild Training Slice Content` to regenerate combat training content.
- Treat these generated assets carefully:
  - `Assets/_BloodAndGlory/CombatContent/Scenes/CombatTrainingScene.unity`
  - `Assets/_BloodAndGlory/CombatContent/Prefabs/Weapons/Broadsword_Combat.prefab`
  - `Assets/_BloodAndGlory/CombatContent/Prefabs/Enemies/PeasantBrown_Combat.prefab`
- If manual fixes are needed in generated content, prefer preserving them in `CombatContentBuilder` so rebuilds do not regress them.

## Asset Layout

- Top-level folders beginning with `_` are the organized project/vendor asset areas. Prefer searching these first.
- Non-underscore top-level folders are Unity-created or Unity-managed areas; avoid editing them unless the task specifically involves Unity settings, packages, samples, or templates.
- Project-owned code and generated combat content are under `Assets/_BloodAndGlory`.
- Reusable animation assets are under `Assets/_Animations`. For combat animation work, start with `Assets/_Animations/DoubleL/Demo/Anim`, which includes one-hand idle, walk, run, block, hit, and attack clips.
- Synty model/prefab assets are under `Assets/_Models/SyntyStudios`.

## Known Local Validation Notes

- The current training scene can run without a baked NavMesh; peasant movement should fall back to transform-based movement. NavMesh warnings are expected until this is addressed.
- Animator warnings for missing `CombatState` and `Speed` parameters are currently non-fatal, but should be treated as cleanup work.
- Verify combat Play Mode behavior with the `BloodAndGlory.Combat.Tests.PlayMode` assembly. The NUnit XML result is written by Unity under the project's application support test-results path.

## Repo Hygiene

- The repo contains many imported assets and can have large unrelated Unity churn. Keep changes scoped to relevant `_` folders, especially `Assets/_BloodAndGlory`, unless intentionally editing project settings or vendor/sample assets.
- Do not revert unrelated dirty files. Inspect focused diffs for files you touch.
- Avoid committing `.DS_Store`, transient `Temp/` files, local test result XML, or unrelated material/settings churn.
