using System;
using System.IO;
using GrindHero;
using GrindHero.Domain.Combat;
using GrindHero.Presentation;
using GrindHero.Presentation.Definitions;
using GrindHero.Presentation.Input;
using GrindHero.Presentation.UI;
using GrindHero.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GrindHero.EditorTools
{
    public static class FirstSliceSceneBuilder
    {
        private const string ContentRoot = "Assets/_GrindHero";
        private const string DataRoot = ContentRoot + "/Data";
        private const string MaterialRoot = ContentRoot + "/Art/Materials";
        private const string SceneRoot = ContentRoot + "/Scenes";
        private const string PlayModeTestRoot = ContentRoot + "/Tests/PlayMode";
        private const string MainScenePath = SceneRoot + "/FirstSlice.unity";
        private const string TestScenePath = PlayModeTestRoot + "/FirstSliceTestScene.unity";

        [MenuItem("GrindHero/Build First Slice")]
        public static void BuildFromMenu()
        {
            BuildProject();
        }

        public static void BuildFromCommandLine()
        {
            BuildProject();
        }

        private static void BuildProject()
        {
            EnsureFolder("Assets", ContentRoot);
            EnsureFolder(ContentRoot, DataRoot);
            EnsureFolder(ContentRoot, ContentRoot + "/Art");
            EnsureFolder(ContentRoot + "/Art", MaterialRoot);
            EnsureFolder(ContentRoot, SceneRoot);
            EnsureFolder(ContentRoot, ContentRoot + "/Tests");
            EnsureFolder(ContentRoot + "/Tests", PlayModeTestRoot);
            AssetDatabase.Refresh();

            var materials = CreateMaterials();
            var content = CreateContent();

            BuildScene(MainScenePath, content, materials, string.Empty);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            content = CreateContent();
            BuildScene(
                TestScenePath,
                content,
                materials,
                "GrindHero-PlayModeTests");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true),
                new EditorBuildSettingsScene(TestScenePath, false)
            };
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Built the first-slice scene and test scene at {MainScenePath}.");
        }

        private static FirstSliceContent CreateContent()
        {
            var weapon = CreateAsset(
                DataRoot + "/BasicAxe.asset",
                () => ScriptableObject.CreateInstance<WeaponDefinition>(),
                asset =>
                {
                    asset.stableId = "weapon.axe.basic";
                    asset.damageType = DamageType.Axe;
                    asset.weaponDamage = 5f;
                    asset.attackCycleSeconds = 1f;
                    asset.windupSeconds = 0.25f;
                    asset.attackRange = 2.5f;
                });

            var headArmor = CreateAsset(
                DataRoot + "/HeadAxeArmor.asset",
                () => ScriptableObject.CreateInstance<ArmorDefinition>(),
                asset =>
                {
                    asset.stableId = "armor.head.axe.graybox";
                    asset.slot = ArmorSlot.Head;
                    asset.protectsAgainst = DamageType.Axe;
                    asset.defense = 2f;
                    asset.grayboxColor = new Color(0.74f, 0.64f, 0.32f);
                });

            var torsoArmor = CreateAsset(
                DataRoot + "/TorsoAxeArmor.asset",
                () => ScriptableObject.CreateInstance<ArmorDefinition>(),
                asset =>
                {
                    asset.stableId = "armor.torso.axe.graybox";
                    asset.slot = ArmorSlot.Torso;
                    asset.protectsAgainst = DamageType.Axe;
                    asset.defense = 3f;
                    asset.grayboxColor = new Color(0.56f, 0.6f, 0.68f);
                });

            var basicHealth = CreateEnemyDefinition(
                "BasicHealth",
                "enemy.basic.health",
                "Basic — HP",
                EnemyArchetype.BasicHealth,
                80f,
                11f,
                0.5f,
                2.1f,
                8f,
                3.2f,
                120,
                RewardStat.Health,
                new Color(0.72f, 0.24f, 0.19f));

            var basicAttack = CreateEnemyDefinition(
                "BasicAttack",
                "enemy.basic.axe-attack",
                "Basic — Axe Attack",
                EnemyArchetype.BasicAxeAttack,
                80f,
                13f,
                0.5f,
                2.1f,
                8f,
                3.2f,
                120,
                RewardStat.AxeAttack,
                new Color(0.76f, 0.43f, 0.16f));

            var advanced = CreateEnemyDefinition(
                "Advanced",
                "enemy.advanced.graybox",
                "Advanced",
                EnemyArchetype.Advanced,
                180f,
                18f,
                0.65f,
                2.25f,
                9f,
                3.4f,
                300,
                RewardStat.None,
                new Color(0.49f, 0.22f, 0.64f));

            var boss = CreateEnemyDefinition(
                "Boss",
                "enemy.boss.first-section",
                "Section Boss",
                EnemyArchetype.Boss,
                450f,
                22f,
                0.6f,
                2.5f,
                12f,
                3.6f,
                0,
                RewardStat.None,
                new Color(0.68f, 0.12f, 0.18f));
            boss.canRespawn = false;
            boss.maximumRespawnsPerDay = 0;
            boss.returnAndHeal = false;
            EditorUtility.SetDirty(boss);

            var tuning = CreateAsset(
                DataRoot + "/FirstSliceTuning.asset",
                () => ScriptableObject.CreateInstance<FirstSliceTuning>(),
                asset =>
                {
                    asset.startingMaximumHealth = 100;
                    asset.healthPerPoint = 5;
                    asset.baseAxeDamage = 15f;
                    asset.axeDamagePerPoint = 1f;
                    asset.movementSpeed = 4.5f;
                    asset.startingDodgeChance = 0f;
                    asset.startingBlockChance = 0f;
                    asset.blockDamageMultiplier = 0.8f;
                    asset.respawnHealthFraction = 0.5f;
                    asset.startingWeapon = weapon;
                    asset.headArmor = headArmor;
                    asset.torsoArmor = torsoArmor;
                    asset.maximumMobRespawnsPerDay = 3;
                    asset.saveIntervalSeconds = 5f;
                });

            return new FirstSliceContent(tuning, basicHealth, basicAttack, advanced, boss);
        }

        private static EnemyDefinition CreateEnemyDefinition(
            string assetName,
            string stableId,
            string displayName,
            EnemyArchetype archetype,
            float maximumHealth,
            float attackDamage,
            float windupSeconds,
            float attackRange,
            float aggroRange,
            float movementSpeed,
            int respawnCooldownSeconds,
            RewardStat rewardStat,
            Color color)
        {
            var definition = CreateAsset(
                DataRoot + "/" + assetName + ".asset",
                () => ScriptableObject.CreateInstance<EnemyDefinition>(),
                asset =>
                {
                    asset.stableId = stableId;
                    asset.displayName = displayName;
                    asset.archetype = archetype;
                    asset.attackDamageType = DamageType.Axe;
                    asset.maximumHealth = maximumHealth;
                    asset.attackDamage = attackDamage;
                    asset.attackCycleSeconds = 1f;
                    asset.windupSeconds = windupSeconds;
                    asset.attackRange = attackRange;
                    asset.aggroRange = aggroRange;
                    asset.movementSpeed = movementSpeed;
                    asset.chaseTimeoutSeconds = 5f;
                    asset.returnHealFractionPerSecond = 0.1f;
                    asset.returnAndHeal = true;
                    asset.canRespawn = true;
                    asset.respawnCooldownSeconds = respawnCooldownSeconds;
                    asset.maximumRespawnsPerDay = 3;
                    asset.rewardStat = rewardStat;
                    asset.rewardPoints = rewardStat == RewardStat.None ? 0 : 1;
                    asset.grayboxColor = color;
                });

            return definition;
        }

        private static FirstSliceMaterials CreateMaterials()
        {
            return new FirstSliceMaterials(
                CreateMaterial(
                    MaterialRoot + "/Ground.mat",
                    new Color(0.18f, 0.3f, 0.24f)),
                CreateMaterial(
                    MaterialRoot + "/Path.mat",
                    new Color(0.48f, 0.4f, 0.29f)),
                CreateMaterial(
                    MaterialRoot + "/Boundary.mat",
                    new Color(0.28f, 0.33f, 0.3f)),
                CreateMaterial(
                    MaterialRoot + "/Ruins.mat",
                    new Color(0.4f, 0.44f, 0.43f)));
        }

        private static void BuildScene(
            string scenePath,
            FirstSliceContent content,
            FirstSliceMaterials materials,
            string saveDirectoryOverride)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.5f, 0.55f);
            RenderSettings.fog = false;

            CreateDirectionalLight();
            CreateArena(materials);

            var spawn = new GameObject("Hero Spawn");
            spawn.transform.position = new Vector3(-13f, 0f, 0f);

            var heroObject = new GameObject("Hero");
            heroObject.transform.position = spawn.transform.position;
            var characterController = heroObject.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.38f;
            characterController.center = new Vector3(0f, 1f, 0f);
            heroObject.AddComponent<HeroActor>();

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 14f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.backgroundColor = new Color(0.23f, 0.33f, 0.39f);
            cameraObject.AddComponent<AudioListener>();
            var cameraFollow = cameraObject.AddComponent<CameraFollow>();

            var inputObject = new GameObject("Touch Input");
            var movementInput = inputObject.AddComponent<DynamicJoystickInput>();

            var hudObject = new GameObject("HUD");
            var hud = hudObject.AddComponent<HUDPresenter>();

            var gateObject = new GameObject("Section Exit");
            gateObject.transform.position = new Vector3(15.5f, 0f, 0f);
            gateObject.AddComponent<ExitGate>();

            var spawnPoints = new[]
            {
                CreateSpawnPoint("basic-hp-west", content.BasicHealth, new Vector3(-6f, 0f, 2f)),
                CreateSpawnPoint("basic-attack-center", content.BasicAttack, new Vector3(-1f, 0f, -3f)),
                CreateSpawnPoint("advanced-east", content.Advanced, new Vector3(5f, 0f, 3f)),
                CreateSpawnPoint("first-boss", content.Boss, new Vector3(10f, 0f, 0f))
            };

            var bootstrapObject = new GameObject("Game Bootstrapper");
            var combatWorld = bootstrapObject.AddComponent<CombatWorld>();
            var bootstrapper = bootstrapObject.AddComponent<GameBootstrapper>();
            ConfigureBootstrapper(
                bootstrapper,
                content.Tuning,
                heroObject.GetComponent<HeroActor>(),
                spawn.transform,
                cameraFollow,
                movementInput,
                hud,
                gateObject.GetComponent<ExitGate>(),
                spawnPoints,
                saveDirectoryOverride);

            MarkSceneObjectsStatic(spawn, heroObject, gateObject);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Could not save Unity scene: {scenePath}");
            }
        }

        private static void CreateArena(FirstSliceMaterials materials)
        {
            CreateBox(
                "Arena floor",
                Vector3.zero,
                new Vector3(36f, 0.5f, 30f),
                materials.Ground);
            CreateBox(
                "Combat path",
                new Vector3(0f, 0.02f, 0f),
                new Vector3(29f, 0.05f, 4f),
                materials.Path);

            CreateBox(
                "North boundary",
                new Vector3(0f, 1.5f, 15f),
                new Vector3(37f, 3f, 0.6f),
                materials.Boundary);
            CreateBox(
                "South boundary",
                new Vector3(0f, 1.5f, -15f),
                new Vector3(37f, 3f, 0.6f),
                materials.Boundary);
            CreateBox(
                "West boundary",
                new Vector3(-18f, 1.5f, 0f),
                new Vector3(0.6f, 3f, 30f),
                materials.Boundary);
            CreateBox(
                "East boundary",
                new Vector3(18f, 1.5f, 0f),
                new Vector3(0.6f, 3f, 30f),
                materials.Boundary);

            CreateRuin(0, new Vector3(-10f, 0f, 10f), materials.Ruins);
            CreateRuin(1, new Vector3(1f, 0f, 11f), materials.Ruins);
            CreateRuin(2, new Vector3(8f, 0f, -10f), materials.Ruins);
            CreateRuin(3, new Vector3(-5f, 0f, -10f), materials.Ruins);
        }

        private static void CreateRuin(int index, Vector3 position, Material material)
        {
            var ruin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ruin.name = $"Low ruin {index + 1}";
            ruin.transform.position = position + new Vector3(0f, 0.55f, 0f);
            ruin.transform.localScale = new Vector3(1.7f, 1.1f, 1.3f);
            ruin.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateBox(
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateDirectionalLight()
        {
            var lightObject = new GameObject("Directional Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(48f, -34f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.91f, 0.76f);
            light.shadows = LightShadows.Soft;
        }

        private static EnemySpawnPoint CreateSpawnPoint(
            string spawnId,
            EnemyDefinition definition,
            Vector3 position)
        {
            var spawnObject = new GameObject($"Spawn - {definition.displayName}");
            spawnObject.transform.position = position;
            var spawnPoint = spawnObject.AddComponent<EnemySpawnPoint>();
            var serialized = new SerializedObject(spawnPoint);
            serialized.FindProperty("spawnId").stringValue = spawnId;
            serialized.FindProperty("definition").objectReferenceValue = definition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return spawnPoint;
        }

        private static void ConfigureBootstrapper(
            GameBootstrapper bootstrapper,
            FirstSliceTuning tuning,
            HeroActor hero,
            Transform heroSpawnPoint,
            CameraFollow cameraFollow,
            DynamicJoystickInput movementInput,
            HUDPresenter hud,
            ExitGate exitGate,
            EnemySpawnPoint[] enemySpawnPoints,
            string saveDirectoryOverride)
        {
            var serialized = new SerializedObject(bootstrapper);
            serialized.FindProperty("tuning").objectReferenceValue = tuning;
            serialized.FindProperty("hero").objectReferenceValue = hero;
            serialized.FindProperty("heroSpawnPoint").objectReferenceValue = heroSpawnPoint;
            serialized.FindProperty("cameraFollow").objectReferenceValue = cameraFollow;
            serialized.FindProperty("movementInput").objectReferenceValue = movementInput;
            serialized.FindProperty("hud").objectReferenceValue = hud;
            serialized.FindProperty("exitGate").objectReferenceValue = exitGate;
            serialized.FindProperty("saveDirectoryOverride").stringValue = saveDirectoryOverride;

            var spawnPoints = serialized.FindProperty("enemySpawnPoints");
            spawnPoints.arraySize = enemySpawnPoints.Length;
            for (var i = 0; i < enemySpawnPoints.Length; i++)
            {
                spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue = enemySpawnPoints[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material CreateMaterial(string assetPath, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    throw new InvalidOperationException("The URP/Lit shader was not available while building the graybox.");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static T CreateAsset<T>(
            string assetPath,
            Func<T> create,
            Action<T> configure)
            where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                asset = create();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            configure(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void EnsureFolder(string parent, string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static void MarkSceneObjectsStatic(
            GameObject spawn,
            GameObject hero,
            GameObject gate)
        {
            spawn.isStatic = true;
            hero.isStatic = false;
            gate.isStatic = false;
        }

        private sealed class FirstSliceContent
        {
            public FirstSliceContent(
                FirstSliceTuning tuning,
                EnemyDefinition basicHealth,
                EnemyDefinition basicAttack,
                EnemyDefinition advanced,
                EnemyDefinition boss)
            {
                Tuning = tuning;
                BasicHealth = basicHealth;
                BasicAttack = basicAttack;
                Advanced = advanced;
                Boss = boss;
            }

            public FirstSliceTuning Tuning { get; }
            public EnemyDefinition BasicHealth { get; }
            public EnemyDefinition BasicAttack { get; }
            public EnemyDefinition Advanced { get; }
            public EnemyDefinition Boss { get; }
        }

        private sealed class FirstSliceMaterials
        {
            public FirstSliceMaterials(
                Material ground,
                Material path,
                Material boundary,
                Material ruins)
            {
                Ground = ground;
                Path = path;
                Boundary = boundary;
                Ruins = ruins;
            }

            public Material Ground { get; }
            public Material Path { get; }
            public Material Boundary { get; }
            public Material Ruins { get; }
        }
    }
}
