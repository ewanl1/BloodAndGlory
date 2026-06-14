using BloodAndGlory.Combat.Runtime.Authoring;
using BloodAndGlory.Combat.Runtime.Debug;
using BloodAndGlory.Combat.Runtime.Enemy;
using BloodAndGlory.Combat.Runtime.Training;
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
        private const string SourcePeasantAnimatorControllerPath = "Assets/SyntyStudios/PolygonAdventure/Models/Characters/Character.controller";
        private const string SourceXrOriginPath = Root + "/XR Combat Rig.prefab";
        private const string CombatBroadswordPath = Root + "/Prefabs/Weapons/Broadsword_Combat.prefab";
        private const string CombatPeasantPath = Root + "/Prefabs/Enemies/PeasantBrown_Combat.prefab";
        private const string BroadswordProfilePath = Root + "/Profiles/BroadswordProfile.asset";
        private const string PeasantProfilePath = Root + "/Profiles/PeasantWeakProfile.asset";
        private const string PeasantAttackPath = Root + "/Attacks/Peasant_Broadsword_Attack_01.asset";
        private const string PendingSceneWireKey = "BloodAndGlory.Combat.PendingTrainingSceneAssetReferenceWire";

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
            CreateCombatPrefabs(enemyProfile, attack);
            CreateTrainingScene(material, enemyProfile, attack, weaponProfile);
            AssetDatabase.SaveAssets();
            QueueSavedTrainingSceneAssetReferenceWire();
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

        private static void CreateCombatPrefabs(EnemyProfileAsset enemyProfile, AttackDefinitionAsset attack)
        {
            CreateBroadswordPrefab();
            CreatePeasantPrefab(enemyProfile, attack);
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

        private static void CreatePeasantPrefab(EnemyProfileAsset enemyProfile, AttackDefinitionAsset attack)
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
                if (animator.runtimeAnimatorController == null)
                {
                    var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SourcePeasantAnimatorControllerPath);
                    if (controller != null)
                        animator.runtimeAnimatorController = controller;
                }
            }

            var combatant = instance.GetComponent<CombatantAuthoring>();
            if (combatant == null)
                combatant = instance.AddComponent<CombatantAuthoring>();
            combatant.ConfigureForTests(10, isPlayer: false, maxHitPoints: 100);

            if (instance.GetComponent<NavMeshAgent>() == null)
                instance.AddComponent<NavMeshAgent>();

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
            var enemyTarget = playerSpawn.transform;

            var enemySpawn = new GameObject("Enemy Spawn");
            enemySpawn.transform.position = new Vector3(0f, 0.1f, 2f);

            var xrOriginSource = AssetDatabase.LoadAssetAtPath<GameObject>(SourceXrOriginPath);
            if (xrOriginSource != null)
            {
                var xrOrigin = (GameObject)PrefabUtility.InstantiatePrefab(xrOriginSource);
                xrOrigin.name = "XR Combat Rig";
                xrOrigin.transform.position = playerSpawn.transform.position;
                enemyTarget = xrOrigin.transform;
            }

            WeaponSweepDriver sweep = null;
            var weaponSource = AssetDatabase.LoadAssetAtPath<GameObject>(CombatBroadswordPath);
            if (weaponSource != null)
            {
                var weapon = (GameObject)PrefabUtility.InstantiatePrefab(weaponSource);
                weapon.name = "Player Broadsword";
                weapon.transform.position = new Vector3(0.6f, 0.9f, -2.2f);
                sweep = weapon.GetComponent<WeaponSweepDriver>();
                var serializedSweep = new SerializedObject(sweep);
                serializedSweep.FindProperty("attackerId").intValue = 1;
                serializedSweep.FindProperty("weaponId").stringValue = weaponProfile.ToData().Id;
                serializedSweep.ApplyModifiedPropertiesWithoutUndo();
            }

            GameObject enemy = null;
            EnemyCombatController enemyController = null;
            var enemySource = AssetDatabase.LoadAssetAtPath<GameObject>(CombatPeasantPath);
            if (enemySource != null)
            {
                enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemySource);
                enemy.name = "PeasantBrown_Combat";
                enemy.transform.position = enemySpawn.transform.position;
                enemyController = enemy.GetComponent<EnemyCombatController>();
                var serializedEnemy = new SerializedObject(enemyController);
                AssignObjectReference(serializedEnemy, "target", enemyTarget);
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemyController);
            }

            var debug = new GameObject("Combat Debug Overlay");
            var overlay = debug.AddComponent<CombatDebugOverlay>();
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

            var serializedOverlay = new SerializedObject(overlay);
            AssignObjectReference(serializedOverlay, "enemy", enemyController);
            AssignObjectReference(serializedOverlay, "worldText", textMesh);

            var runtime = new GameObject("Combat Training Runtime");
            var trainingRuntime = runtime.AddComponent<CombatTrainingRuntime>();
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
