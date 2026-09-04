using BloodAndGlory.Combat.Runtime.Authoring;
using BloodAndGlory.Combat.Runtime.Debug;
using BloodAndGlory.Combat.Runtime.Enemy;
using BloodAndGlory.Combat.Runtime.Training;
using BloodAndGlory.Combat.Runtime.Weapons;
using BloodAndGlory.Combat.Core;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BloodAndGlory.Combat.Editor
{
    public static class CombatContentBuilder
    {
        private const string Root = "Assets/_BloodAndGlory/CombatContent";
        private const string TrainingScenePath = Root + "/Scenes/CombatTrainingScene.unity";
        private const string SourceBroadswordPath = "Assets/_Models/SyntyStudios/PolygonKnights/Prefabs/Weapons/SM_Wep_Broadsword_01.prefab";
        private const string SourcePeasantPath = "Assets/_Models/SyntyStudios/PolygonAdventure/Prefabs/Characters/Character_Peasant_Brown.prefab";
        private const string SourceXrOriginPath = Root + "/XR Combat Rig.prefab";
        private const string SourceXrDeviceSimulatorPath = "Assets/Samples/XR Interaction Toolkit/3.4.1/XR Device Simulator/XR Device Simulator.prefab";
        private const string CombatBroadswordPath = Root + "/Prefabs/Weapons/Broadsword_Combat.prefab";
        private const string CombatPeasantPath = Root + "/Prefabs/Enemies/PeasantBrown_Combat.prefab";
        private const string PeasantCombatControllerPath = Root + "/Animation/PeasantCombat.controller";
        private const string BroadswordProfilePath = Root + "/Profiles/BroadswordProfile.asset";
        private const string PeasantProfilePath = Root + "/Profiles/PeasantWeakProfile.asset";
        private const string PeasantAttackPath = Root + "/Attacks/Peasant_Broadsword_Attack_01.asset";
        private const string PendingSceneWireKey = "BloodAndGlory.Combat.PendingTrainingSceneAssetReferenceWire";
        private const string TrainingFloorName = "Training Floor";
        private const string DirectionalLightName = "Directional Light";
        private const string PlayerSpawnName = "Player Spawn";
        private const string EnemySpawnName = "Enemy Spawn";
        private const string XrCombatRigName = "XR Combat Rig";
        private const string XrDeviceSimulatorName = "XR Device Simulator";
        private const string PlayerBroadswordName = "Player Broadsword";
        private const string PeasantName = "PeasantBrown_Combat";
        private const string CombatDebugOverlayName = "Combat Debug Overlay";
        private const string CombatDebugTextName = "Combat Debug Text";
        private const string CombatTrainingRuntimeName = "Combat Training Runtime";
        private const string PeasantIdleClipPath = "Assets/_Animations/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@CombatIdle1H01.fbx";
        private const string PeasantWalkClipPath = "Assets/_Animations/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx";
        private const string PeasantAttackClipPath = "Assets/_Animations/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_R.fbx";
        private const string PeasantBlockClipPath = "Assets/_Animations/Sword Animation/Assets/Animations/anim_block_idle.FBX";
        private const string PeasantDeathClipPath = "Assets/_Animations/Kevin Iglesias/Human Animations/Animations/Male/Combat/HumanM@Death01.fbx";
        private const string LegacyPeasantIdleClipPath = "Assets/_Animations/DoubleL/One Hand Up/Movement/Idle/Idle/OneHand_Up_Stand_Idle_A_2.fbx";
        private const string LegacyPeasantWalkClipPath = "Assets/_Animations/DoubleL/One Hand Up/Movement/Walk/Base/InPlace/OneHand_Up_Walk_F_InPlace.fbx";
        private const string LegacyPeasantAttackClipPath = "Assets/_Animations/DoubleL/One Hand Up/Attack_A/OneHand_Up_Attack_1_InPlace.fbx";
        private const string LegacyPeasantBlockClipPath = "Assets/_Animations/DoubleL/One Hand Up/Sheild/Idle/OneHand_Up_Shield_Block_Idle.fbx";

        [InitializeOnLoadMethod]
        private static void RegisterPendingSceneWire()
        {
            EditorApplication.update -= RunPendingSceneWire;
            EditorApplication.update += RunPendingSceneWire;
        }

        [MenuItem("Blood And Glory/Combat/Rebuild Training Slice Content")]
        public static void Rebuild()
        {
            EnsureFolders();
            var weaponProfile = CreateOrReplaceAsset<WeaponProfileAsset>(BroadswordProfilePath);
            var enemyProfile = CreateOrReplaceAsset<EnemyProfileAsset>(PeasantProfilePath);
            var attack = CreateOrReplaceAsset<AttackDefinitionAsset>(PeasantAttackPath);
            var material = CreateTrainingMaterial();

            AssetDatabase.SaveAssets();
            var peasantController = CreatePeasantCombatAnimatorController();
            CreateCombatPrefabs(enemyProfile, attack, peasantController);
            CreateTrainingScene(material, enemyProfile, attack, weaponProfile);
            AssetDatabase.SaveAssets();
            QueueSavedTrainingSceneAssetReferenceWire();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "_BloodAndGlory");
            CreateFolder("Assets/_BloodAndGlory", "CombatContent");
            CreateFolder(Root, "Scenes");
            CreateFolder(Root, "Prefabs");
            CreateFolder(Root + "/Prefabs", "Weapons");
            CreateFolder(Root + "/Prefabs", "Enemies");
            CreateFolder(Root, "Animation");
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

        private static AnimatorController CreatePeasantCombatAnimatorController()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PeasantCombatControllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(PeasantCombatControllerPath);

            EnsureAnimatorParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureAnimatorParameter(controller, "CombatState", AnimatorControllerParameterType.Int);
            EnsureAnimatorParameter(controller, "Dead", AnimatorControllerParameterType.Bool);

            var stateMachine = controller.layers[0].stateMachine;

            var idle = GetOrCreateState(stateMachine, "Idle");
            EnsureStateMotion(idle, PeasantIdleClipPath, LegacyPeasantIdleClipPath);
            stateMachine.defaultState = idle;

            var walk = GetOrCreateState(stateMachine, "Approach");
            EnsureStateMotion(walk, PeasantWalkClipPath, LegacyPeasantWalkClipPath);

            var attack = GetOrCreateState(stateMachine, "Attack");
            EnsureStateMotion(attack, PeasantAttackClipPath, LegacyPeasantAttackClipPath);

            var defend = GetOrCreateState(stateMachine, "Defend");
            EnsureStateMotion(defend, PeasantBlockClipPath, LegacyPeasantBlockClipPath);

            var dead = GetOrCreateState(stateMachine, "Dead");
            EnsureStateMotion(dead, PeasantDeathClipPath, LegacyPeasantIdleClipPath);

            ClearTransitions(idle, walk, attack, defend, dead);
            ClearAnyStateTransitions(stateMachine);
            AddTransition(idle, walk, "Speed", AnimatorConditionMode.Greater, 0.05f);
            AddTransition(walk, idle, "Speed", AnimatorConditionMode.Less, 0.05f);
            AddCombatStateTransition(idle, attack, EnemyCombatState.AttackCommit);
            AddCombatStateTransition(walk, attack, EnemyCombatState.AttackCommit);
            AddCombatStateTransition(idle, defend, EnemyCombatState.Block);
            AddCombatStateTransition(walk, defend, EnemyCombatState.Block);
            AddCombatStateTransition(idle, defend, EnemyCombatState.Parry);
            AddCombatStateTransition(walk, defend, EnemyCombatState.Parry);
            AddCombatStateTransition(attack, idle, EnemyCombatState.Recover);
            AddCombatStateTransition(attack, idle, EnemyCombatState.Idle);
            AddCombatStateTransition(defend, idle, EnemyCombatState.Recover);
            AddCombatStateTransition(defend, idle, EnemyCombatState.Idle);

            var deadTransition = stateMachine.AddAnyStateTransition(dead);
            deadTransition.hasExitTime = false;
            deadTransition.duration = 0.1f;
            deadTransition.AddCondition(AnimatorConditionMode.If, 0f, "Dead");

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static void EnsureAnimatorParameter(
            AnimatorController controller,
            string parameterName,
            AnimatorControllerParameterType parameterType)
        {
            var existing = controller.parameters.FirstOrDefault(parameter => parameter.name == parameterName);
            if (existing != null && existing.type == parameterType)
                return;

            if (existing != null)
                controller.RemoveParameter(existing);

            controller.AddParameter(parameterName, parameterType);
        }

        private static AnimatorState GetOrCreateState(AnimatorStateMachine stateMachine, string stateName)
        {
            var existing = stateMachine.states
                .Select(childState => childState.state)
                .FirstOrDefault(state => state.name == stateName);

            return existing != null ? existing : stateMachine.AddState(stateName);
        }

        private static void EnsureStateMotion(
            AnimatorState state,
            string fallbackClipPath,
            params string[] replaceableClipPaths)
        {
            if (state.motion == null || replaceableClipPaths.Contains(AssetDatabase.GetAssetPath(state.motion)))
                state.motion = LoadAnimationClip(fallbackClipPath);
        }

        private static void ClearTransitions(params AnimatorState[] states)
        {
            foreach (var state in states)
            {
                foreach (var transition in state.transitions.ToArray())
                    state.RemoveTransition(transition);
            }
        }

        private static void ClearAnyStateTransitions(AnimatorStateMachine stateMachine)
        {
            foreach (var transition in stateMachine.anyStateTransitions.ToArray())
                stateMachine.RemoveAnyStateTransition(transition);
        }

        private static AnimationClip LoadAnimationClip(string path)
        {
            var clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate => !candidate.name.StartsWith("__preview__"));

            if (clip == null)
                throw new System.IO.FileNotFoundException("Animation clip missing.", path);

            return clip;
        }

        private static void AddTransition(
            AnimatorState from,
            AnimatorState to,
            string parameter,
            AnimatorConditionMode mode,
            float threshold)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.15f;
            transition.AddCondition(mode, threshold, parameter);
        }

        private static void AddCombatStateTransition(AnimatorState from, AnimatorState to, EnemyCombatState combatState)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.1f;
            transition.AddCondition(AnimatorConditionMode.Equals, (int)combatState, "CombatState");
        }

#if UNITY_INCLUDE_TESTS
        public static GameObject GetOrCreateRootSceneObjectForTests(string objectName)
        {
            return GetOrCreateRootSceneObject(objectName, () => new GameObject(objectName));
        }

        public static GameObject GetOrCreateChildSceneObjectForTests(
            Transform parent,
            string objectName,
            out bool created)
        {
            return GetOrCreateChildSceneObject(parent, objectName, out created);
        }
#endif

        private static void CreateCombatPrefabs(
            EnemyProfileAsset enemyProfile,
            AttackDefinitionAsset attack,
            RuntimeAnimatorController peasantController)
        {
            CreateBroadswordPrefab();
            CreatePeasantPrefab(enemyProfile, attack, peasantController);
        }

        private static void CreateBroadswordPrefab()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourceBroadswordPath);
            if (source == null)
                throw new System.IO.FileNotFoundException("Broadsword source prefab missing.", SourceBroadswordPath);

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CombatBroadswordPath);
            var existingAttachPoint = existing == null ? null : FindChildRecursive(existing.transform, "AttachPoint");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = "Broadsword_Combat";

            var rigidbody = instance.GetComponent<Rigidbody>();
            if (rigidbody == null)
                rigidbody = instance.AddComponent<Rigidbody>();

            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.mass = Mathf.Max(1.5f, rigidbody.mass);
            rigidbody.linearDamping = Mathf.Max(0.02f, rigidbody.linearDamping);
            rigidbody.angularDamping = Mathf.Max(0.05f, rigidbody.angularDamping);

            var grabInteractable = instance.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
                grabInteractable = instance.AddComponent<XRGrabInteractable>();

            var instanceAttachPoint = existingAttachPoint == null ? null : FindChildRecursive(instance.transform, existingAttachPoint.name);
            if (existingAttachPoint != null && instanceAttachPoint == null)
            {
                var attachPoint = new GameObject(existingAttachPoint.name);
                attachPoint.transform.SetParent(instance.transform, false);
                attachPoint.transform.localPosition = existingAttachPoint.localPosition;
                attachPoint.transform.localRotation = existingAttachPoint.localRotation;
                attachPoint.transform.localScale = existingAttachPoint.localScale;
                instanceAttachPoint = attachPoint.transform;
            }

            if (instanceAttachPoint != null)
            {
                var serializedGrab = new SerializedObject(grabInteractable);
                serializedGrab.FindProperty("m_AttachTransform").objectReferenceValue = instanceAttachPoint;
                serializedGrab.ApplyModifiedPropertiesWithoutUndo();
            }

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

        private static Transform FindChildRecursive(Transform root, string name)
        {
            if (root.name == name)
                return root;

            foreach (Transform child in root)
            {
                var match = FindChildRecursive(child, name);
                if (match != null)
                    return match;
            }

            return null;
        }

        private static void CreatePeasantPrefab(
            EnemyProfileAsset enemyProfile,
            AttackDefinitionAsset attack,
            RuntimeAnimatorController peasantController)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePeasantPath);
            if (source == null)
                throw new System.IO.FileNotFoundException("Peasant source prefab missing.", SourcePeasantPath);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = "PeasantBrown_Combat";

            var animator = instance.GetComponent<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.runtimeAnimatorController = peasantController;
            }

            LiftRendererBoundsToRoot(instance);

            var combatant = instance.GetComponent<CombatantAuthoring>();
            if (combatant == null)
                combatant = instance.AddComponent<CombatantAuthoring>();
            combatant.ConfigureForTests(10, isPlayer: false, maxHitPoints: 100);

            var agent = instance.GetComponent<NavMeshAgent>();
            if (agent == null)
                agent = instance.AddComponent<NavMeshAgent>();

            ConfigureAgentFromRendererBounds(instance, agent);

            var enemyController = instance.GetComponent<EnemyCombatController>();
            if (enemyController == null)
                enemyController = instance.AddComponent<EnemyCombatController>();

            var serializedEnemy = new SerializedObject(enemyController);
            AssignObjectReference(serializedEnemy, "enemyProfile", enemyProfile);
            AssignObjectReference(serializedEnemy, "attackDefinition", attack);

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

        private static void LiftRendererBoundsToRoot(GameObject root)
        {
            if (!TryCalculateRendererBounds(root, out var bounds) || bounds.min.y >= 0f)
                return;

            var lift = -bounds.min.y;
            foreach (Transform child in root.transform)
                child.localPosition += Vector3.up * lift;
        }

        private static void ConfigureAgentFromRendererBounds(GameObject root, NavMeshAgent agent)
        {
            if (!TryCalculateRendererBounds(root, out var bounds))
                return;

            var height = Mathf.Max(1.8f, bounds.size.y);
            agent.height = height;
            agent.baseOffset = height * 0.5f;
            agent.radius = 0.35f;
        }

        private static bool TryCalculateRendererBounds(GameObject root, out Bounds bounds)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1))
                bounds.Encapsulate(renderer.bounds);

            return true;
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
            var scene = OpenOrCreateTrainingScene();

            var floor = GetOrCreateRootSceneObject(TrainingFloorName, () => GameObject.CreatePrimitive(PrimitiveType.Cube));
            floor.transform.localScale = new Vector3(12f, 0.1f, 12f);
            var floorRenderer = floor.GetComponent<MeshRenderer>();
            if (floorRenderer != null)
                floorRenderer.sharedMaterial = material;
            EnsureComponent<NavMeshSurface>(floor);

            var lightObject = GetOrCreateRootSceneObject(DirectionalLightName, () => new GameObject(DirectionalLightName));
            var light = EnsureComponent<Light>(lightObject);
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var playerSpawn = GetOrCreateRootSceneObject(PlayerSpawnName, () => new GameObject(PlayerSpawnName));
            playerSpawn.transform.position = new Vector3(0f, 0.1f, -3f);
            var enemyTarget = playerSpawn.transform;

            var enemySpawn = GetOrCreateRootSceneObject(EnemySpawnName, () => new GameObject(EnemySpawnName));
            enemySpawn.transform.position = new Vector3(0f, 0.1f, 2f);

            var xrOrigin = GameObject.Find(XrCombatRigName);
            var xrOriginSource = AssetDatabase.LoadAssetAtPath<GameObject>(SourceXrOriginPath);
            if (xrOrigin == null && xrOriginSource != null)
            {
                xrOrigin = (GameObject)PrefabUtility.InstantiatePrefab(xrOriginSource);
                xrOrigin.name = XrCombatRigName;
            }

            if (xrOrigin != null)
            {
                xrOrigin.transform.position = playerSpawn.transform.position;
                enemyTarget = xrOrigin.transform;
            }

            EnsureXrDeviceSimulator();

            DestroySceneObjectIfExists(PlayerBroadswordName);
            WeaponSweepDriver sweep = null;
            var weaponSource = AssetDatabase.LoadAssetAtPath<GameObject>(CombatBroadswordPath);
            if (weaponSource != null)
            {
                var weapon = (GameObject)PrefabUtility.InstantiatePrefab(weaponSource);
                weapon.name = PlayerBroadswordName;
                weapon.transform.position = new Vector3(0.6f, 0.9f, -2.2f);
                sweep = weapon.GetComponent<WeaponSweepDriver>();
                var serializedSweep = new SerializedObject(sweep);
                serializedSweep.FindProperty("attackerId").intValue = 1;
                serializedSweep.FindProperty("weaponId").stringValue = weaponProfile.ToData().Id;
                serializedSweep.ApplyModifiedPropertiesWithoutUndo();
            }

            DestroySceneObjectIfExists(PeasantName);
            GameObject enemy = null;
            EnemyCombatController enemyController = null;
            var enemySource = AssetDatabase.LoadAssetAtPath<GameObject>(CombatPeasantPath);
            if (enemySource != null)
            {
                enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemySource);
                enemy.name = PeasantName;
                enemy.transform.position = enemySpawn.transform.position;
                enemyController = enemy.GetComponent<EnemyCombatController>();
                var serializedEnemy = new SerializedObject(enemyController);
                AssignObjectReference(serializedEnemy, "target", enemyTarget);
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemyController);
            }

            var debug = GetOrCreateRootSceneObject(CombatDebugOverlayName, () => new GameObject(CombatDebugOverlayName));
            var overlay = EnsureComponent<CombatDebugOverlay>(debug);
            var textObject = GetOrCreateChildSceneObject(debug.transform, CombatDebugTextName, out var textCreated);
            if (textCreated)
            {
                textObject.transform.localPosition = new Vector3(-2.2f, 1.8f, 1.6f);
                textObject.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
                textObject.transform.localScale = Vector3.one * 0.08f;
            }

            var textMesh = EnsureComponent<TextMesh>(textObject);
            textMesh.anchor = TextAnchor.UpperLeft;
            textMesh.alignment = TextAlignment.Left;
            textMesh.fontSize = 42;
            textMesh.color = Color.black;

            var serializedOverlay = new SerializedObject(overlay);
            AssignObjectReference(serializedOverlay, "enemy", enemyController);
            AssignObjectReference(serializedOverlay, "worldText", textMesh);

            DestroySceneObjectIfExists(CombatTrainingRuntimeName);
            var runtime = new GameObject(CombatTrainingRuntimeName);
            var trainingRuntime = EnsureComponent<CombatTrainingRuntime>(runtime);
            var serializedRuntime = new SerializedObject(trainingRuntime);
            AssignObjectReference(serializedRuntime, "playerSword", sweep);
            AssignObjectReference(serializedRuntime, "playerSwordProfile", weaponProfile);
            AssignObjectReference(serializedRuntime, "enemyCombatant", enemy == null ? null : enemy.GetComponent<CombatantAuthoring>());
            AssignObjectReference(serializedRuntime, "enemyController", enemyController);
            AssignObjectReference(serializedRuntime, "debugOverlay", overlay);

            EditorSceneManager.SaveScene(scene, TrainingScenePath);
            WireSavedTrainingSceneAssetReferences();
            AddTrainingSceneToBuildSettings();
        }

        private static Scene OpenOrCreateTrainingScene()
        {
            return System.IO.File.Exists(TrainingScenePath)
                ? EditorSceneManager.OpenScene(TrainingScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void EnsureXrDeviceSimulator()
        {
            if (GameObject.Find(XrDeviceSimulatorName) != null)
                return;

            var simulatorSource = AssetDatabase.LoadAssetAtPath<GameObject>(SourceXrDeviceSimulatorPath);
            if (simulatorSource == null)
                return;

            var simulator = (GameObject)PrefabUtility.InstantiatePrefab(simulatorSource);
            simulator.name = XrDeviceSimulatorName;
        }

        private static GameObject GetOrCreateRootSceneObject(string objectName, System.Func<GameObject> create)
        {
            var existing = GameObject.Find(objectName);
            if (existing != null)
                return existing;

            var created = create();
            created.name = objectName;
            return created;
        }

        private static GameObject GetOrCreateChildSceneObject(
            Transform parent,
            string objectName,
            out bool created)
        {
            var existing = parent.Find(objectName);
            if (existing != null)
            {
                created = false;
                return existing.gameObject;
            }

            var child = new GameObject(objectName);
            child.transform.SetParent(parent, false);
            created = true;
            return child;
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void DestroySceneObjectIfExists(string objectName)
        {
            var existing = GameObject.Find(objectName);
            if (existing != null)
                Object.DestroyImmediate(existing);
        }

        private static void WireSavedTrainingSceneAssetReferences()
        {
            AssetDatabase.ImportAsset(BroadswordProfilePath);
            var weaponProfile = AssetDatabase.LoadAssetAtPath<WeaponProfileAsset>(BroadswordProfilePath);
            if (weaponProfile == null)
                throw new System.IO.FileNotFoundException("Broadsword profile asset missing.", BroadswordProfilePath);

            var scene = EditorSceneManager.OpenScene(TrainingScenePath, OpenSceneMode.Single);
            var runtime = Object.FindAnyObjectByType<CombatTrainingRuntime>();
            if (runtime == null)
                throw new System.InvalidOperationException("Combat Training Runtime was not found after saving the training scene.");

            var profileField = typeof(CombatTrainingRuntime).GetField(
                "playerSwordProfile",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (profileField == null)
                throw new System.MissingMemberException(nameof(CombatTrainingRuntime), "playerSwordProfile");

            profileField.SetValue(runtime, weaponProfile);
            EditorUtility.SetDirty(runtime);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, TrainingScenePath);
            AssetDatabase.SaveAssets();
        }

        private static void QueueSavedTrainingSceneAssetReferenceWire()
        {
            EditorPrefs.SetBool(PendingSceneWireKey, true);
            EditorApplication.update -= RunPendingSceneWire;
            EditorApplication.update += RunPendingSceneWire;
        }

        private static void RunPendingSceneWire()
        {
            if (!EditorPrefs.GetBool(PendingSceneWireKey, false))
            {
                EditorApplication.update -= RunPendingSceneWire;
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            EditorApplication.update -= RunPendingSceneWire;
            EditorPrefs.DeleteKey(PendingSceneWireKey);
            WireSavedTrainingSceneAssetReferences();
        }

        private static void AssignObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
                throw new System.MissingMemberException(serializedObject.targetObject.GetType().Name, propertyName);

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(serializedObject.targetObject);
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
