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
        private const string SourceXrOriginPath = Root + "/XR Combat Rig.prefab";
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

            if (existingAttachPoint != null && FindChildRecursive(instance.transform, existingAttachPoint.name) == null)
            {
                var attachPoint = new GameObject(existingAttachPoint.name);
                attachPoint.transform.SetParent(instance.transform, false);
                attachPoint.transform.localPosition = existingAttachPoint.localPosition;
                attachPoint.transform.localRotation = existingAttachPoint.localRotation;
                attachPoint.transform.localScale = existingAttachPoint.localScale;

                var serializedGrab = new SerializedObject(grabInteractable);
                var attachTransform = serializedGrab.FindProperty("m_AttachTransform");
                if (attachTransform.objectReferenceValue == null)
                    attachTransform.objectReferenceValue = attachPoint.transform;
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
                xrOrigin.name = "XR Combat Rig";
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
