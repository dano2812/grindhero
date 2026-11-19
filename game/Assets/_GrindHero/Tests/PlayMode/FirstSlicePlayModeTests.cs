using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GrindHero;
using GrindHero.Application.Combat;
using GrindHero.Domain.Combat;
using GrindHero.Domain.World;
using GrindHero.Presentation;
using GrindHero.Presentation.Definitions;
using GrindHero.Presentation.Input;
using GrindHero.Presentation.World;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GrindHero.Tests.PlayMode
{
    public sealed class FirstSlicePlayModeTests : IPrebuildSetup, IPostBuildCleanup
    {
        private const string TestSaveDirectoryName = "GrindHero-PlayModeTests";
        private const string TestScenePath =
            "Assets/_GrindHero/Tests/PlayMode/FirstSliceTestScene.unity";
        private readonly List<EnemyDefinition> _testDefinitions =
            new List<EnemyDefinition>();

        public void Setup()
        {
#if UNITY_EDITOR
            SetTestSceneEnabled(true);
#endif
        }

        public void Cleanup()
        {
#if UNITY_EDITOR
            SetTestSceneEnabled(false);
#endif
        }

        [UnitySetUp]
        public IEnumerator LoadIsolatedFirstSliceScene()
        {
            DeleteTestSaveDirectory();
            Screen.SetResolution(1440, 810, false);
            var loadOperation = SceneManager.LoadSceneAsync(TestScenePath, LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null, $"Could not load generated test scene at {TestScenePath}.");
            yield return loadOperation;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator UnloadSceneAndDeleteIsolatedSave()
        {
            var testScene = SceneManager.GetSceneByPath(TestScenePath);
            if (testScene.IsValid() && testScene.isLoaded)
            {
                if (SceneManager.sceneCount == 1)
                    SceneManager.CreateScene("Empty test harness");
                var unloadOperation = SceneManager.UnloadSceneAsync(testScene);
                if (unloadOperation != null)
                {
                    yield return unloadOperation;
                }
            }

            foreach (var definition in _testDefinitions)
            {
                if (definition != null)
                {
                    UnityEngine.Object.Destroy(definition);
                }
            }

            _testDefinitions.Clear();
            DeleteTestSaveDirectory();
        }

        [UnityTest]
        public IEnumerator GeneratedTestScene_BootsAndSpawnsTheFirstSlice()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null, "The test scene must contain a game bootstrapper.");
            Assert.That(bootstrapper.IsReady, Is.True, "The game bootstrapper must finish initialization.");
            Assert.That(bootstrapper.Hero, Is.Not.Null);
            Assert.That(FindTestComponent<DynamicJoystickInput>(), Is.Not.Null);
            Assert.That(bootstrapper.CombatWorld.AliveEnemyCount, Is.EqualTo(4));
            Assert.That(bootstrapper.CurrentHealth, Is.EqualTo(100f));
            Assert.That(bootstrapper.Gate.IsUnlocked, Is.False);
            Assert.That(FindTestComponent<Canvas>().transform.Find("Dynamic joystick handle").gameObject.activeSelf,
                Is.False, "The idle joystick must not cover the action.");

            EnemyActor boss = null;
            var activeEnemies = bootstrapper.CombatWorld.ActiveEnemies;
            for (var enemyIndex = 0; enemyIndex < activeEnemies.Count; enemyIndex++)
            {
                var enemy = activeEnemies[enemyIndex];
                if (enemy != null && enemy.Definition != null && enemy.Definition.IsBoss)
                {
                    boss = enemy;
                    break;
                }
            }

            Assert.That(boss, Is.Not.Null, "The test scene must spawn its boss.");
            Assert.That(boss.Definition.returnAndHeal, Is.True);
            Assert.That(
                boss.Definition.returnHealFractionPerSecond,
                Is.EqualTo(0.1f),
                "The boss should use the same 10% maximum-health return-healing rate.");
            Assert.That(boss.Definition.chaseTimeoutSeconds, Is.EqualTo(5f));

            if (bootstrapper.IsPaused)
            {
                bootstrapper.TogglePause();
            }

            bootstrapper.TogglePause();
            Assert.That(bootstrapper.IsPaused, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            bootstrapper.TogglePause();
            Assert.That(bootstrapper.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator HeroHealing_ContinuesDuringCombatButStopsWhenPausedOrDead()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            DisableSceneEnemies();
            if (bootstrapper.IsPaused) bootstrapper.TogglePause();
            var target = bootstrapper.CombatWorld.ActiveEnemies[0];
            var targetView = target.GetComponentInChildren<StylizedActorView>();
            TeleportHero(bootstrapper, target.transform.position + Vector3.back * 2f);
            yield return null;
            yield return null;
            Assert.That(bootstrapper.Hero.CurrentTarget, Is.EqualTo(target));
            Assert.That(targetView.targetRing.gameObject.activeSelf, Is.True);
            bootstrapper.ReceiveHeroDamage(45f, DamageType.Axe);
            var startHealth = bootstrapper.CurrentHealth;
            var startTime = Time.time;
            yield return new WaitForSeconds(0.5f);
            Assert.That(bootstrapper.CurrentHealth - startHealth,
                Is.EqualTo((Time.time - startTime) * 2f).Within(0.15f));
            Assert.That(target.CurrentHealth, Is.LessThan(target.MaximumHealth),
                "The hero should keep attacking while it heals.");
            bootstrapper.TogglePause();
            var pausedHealth = bootstrapper.CurrentHealth;
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(bootstrapper.CurrentHealth, Is.EqualTo(pausedHealth));
            bootstrapper.TogglePause();
            Assert.That(bootstrapper.ReceiveHeroDamage(10000f, DamageType.Axe), Is.True);
            Assert.That(targetView.targetRing.gameObject.activeSelf, Is.False,
                "Dying must clear the old target highlight.");
            yield return new WaitForSeconds(0.2f);
            Assert.That(bootstrapper.CurrentHealth, Is.Zero);
        }

        [UnityTest]
        public IEnumerator FirstSection_QuestTreasureBossAndExitFormACompleteReplaySafeLoop()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            var adventure = FindTestComponent<SectionAdventure>();
            Assert.That(adventure, Is.Not.Null);
            DisableSceneEnemies();
            if (bootstrapper.IsPaused) bootstrapper.TogglePause();
            bootstrapper.Hero.enabled = false;
            TeleportHero(bootstrapper, adventure.Warden.position + Vector3.back);
            yield return null;
            yield return null;
            Assert.That(bootstrapper.IsQuestAccepted(adventure.Definition.questId), Is.True);
            TeleportHero(bootstrapper, adventure.Chest.position + Vector3.back);
            yield return null;
            yield return null;
            Assert.That(adventure.IsChestOpen, Is.True);
            Assert.That(bootstrapper.ProgressData.healthPoints, Is.EqualTo(3));
            Assert.That(bootstrapper.LootTreasure(adventure.Definition.treasureId, 3), Is.False);
            EnemyActor boss = null;
            foreach (var actor in bootstrapper.CombatWorld.ActiveEnemies)
                if (actor.Definition.IsBoss) boss = actor;
            Assert.That(boss, Is.Not.Null);
            boss.TakeDamage(10000f);
            yield return new WaitForSeconds(2.1f);
            Assert.That(bootstrapper.Gate.IsUnlocked, Is.True);
            TeleportHero(bootstrapper, adventure.Warden.position + Vector3.back);
            yield return null;
            yield return null;
            Assert.That(bootstrapper.IsQuestCompleted(adventure.Definition.questId), Is.True);
            Assert.That(bootstrapper.ProgressData.healthPoints, Is.EqualTo(5));
            Assert.That(bootstrapper.ProgressData.axeAttackPoints, Is.EqualTo(2));
            TeleportHero(bootstrapper, bootstrapper.Gate.transform.position +
                bootstrapper.Gate.transform.forward);
            yield return null;
            yield return null;
            Assert.That(bootstrapper.IsSectionCompleted(adventure.Definition.sectionId), Is.True);
            Assert.That(bootstrapper.CompleteSection(adventure.Definition.sectionId), Is.False);
            Assert.That(bootstrapper.ClaimQuestReward(adventure.Definition.questId, 2, 2), Is.False);
            var sectionId = adventure.Definition.sectionId;
            var questId = adventure.Definition.questId;
            var treasureId = adventure.Definition.treasureId;
            yield return SceneManager.LoadSceneAsync(TestScenePath, LoadSceneMode.Single);
            yield return null;
            bootstrapper = FindTestComponent<GameBootstrapper>();
            adventure = FindTestComponent<SectionAdventure>();
            Assert.That(bootstrapper.IsQuestCompleted(questId), Is.True);
            Assert.That(bootstrapper.IsSectionCompleted(sectionId), Is.True);
            Assert.That(bootstrapper.IsTreasureLooted(treasureId), Is.True);
            Assert.That(adventure.IsChestOpen, Is.True);
            Assert.That(bootstrapper.Gate.IsUnlocked, Is.True);
            Assert.That(bootstrapper.CombatWorld.AliveEnemyCount, Is.EqualTo(3));
            Assert.That(bootstrapper.ProgressData.healthPoints, Is.EqualTo(5));
            Assert.That(bootstrapper.ProgressData.axeAttackPoints, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator FirstSection_RendersOriginalArtAndCapturesPreviewViews()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            var adventure = FindTestComponent<SectionAdventure>();
            Assert.That(bootstrapper.Hero.GetComponentInChildren<StylizedActorView>(), Is.Not.Null);
            Assert.That(GameObject.Find("Meadow island"), Is.Not.Null);
            DisableSceneEnemies();
            if (bootstrapper.IsPaused) bootstrapper.TogglePause();
            bootstrapper.Hero.enabled = false;
            yield return new WaitForSeconds(0.3f);
            CapturePreview("mosswatch-opening");
            TeleportHero(bootstrapper, adventure.Warden.position + Vector3.back);
            yield return new WaitForSeconds(0.6f);
            CapturePreview("mosswatch-story");
            TeleportHero(bootstrapper, new Vector3(-5f, 0f, -5.3f));
            yield return new WaitForSeconds(0.6f);
            CapturePreview("mosswatch-grove");
            TeleportHero(bootstrapper, new Vector3(6f, 0f, -1f));
            yield return new WaitForSeconds(0.6f);
            CapturePreview("mosswatch-guardian");
            Assert.That(adventure.Definition.displayName, Is.Not.Empty);
        }

        private static void TeleportHero(GameBootstrapper bootstrapper, Vector3 position)
        {
            var controller = bootstrapper.Hero.GetComponent<CharacterController>();
            controller.enabled = false;
            bootstrapper.Hero.transform.position = position;
            controller.enabled = true;
        }

        private static void CapturePreview(string name)
        {
#if UNITY_EDITOR
            var camera = FindTestComponent<CameraFollow>().GetComponent<Camera>();
            var canvas = FindTestComponent<GrindHero.Presentation.UI.HUDPresenter>().GetComponent<Canvas>();
            var previousMode = canvas.renderMode;
            var previousCamera = canvas.worldCamera;
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var target = RenderTexture.GetTemporary(1440, 810, 24);
            var texture = new Texture2D(1440, 810, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0f, 0f, 1440f, 810f), 0, 0);
                texture.Apply();
                var folder = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", ".artifacts"));
                Directory.CreateDirectory(folder);
                File.WriteAllBytes(Path.Combine(folder, name + ".png"), texture.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = previousMode;
                canvas.worldCamera = previousCamera;
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.Destroy(texture);
            }
#endif
        }

        [UnityTest]
        public IEnumerator EnemyFocusRange_OnlyChasesInsideTwiceAttackRange()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null);
            if (bootstrapper.IsPaused)
            {
                bootstrapper.TogglePause();
            }

            DisableSceneEnemies();
            bootstrapper.Hero.enabled = false;

            var enemy = CreateTestEnemy(
                bootstrapper,
                new TestCombatSession(false),
                false,
                out var spawnTransform);
            var focusRange = enemy.Definition.FocusRange;
            Assert.That(
                focusRange,
                Is.EqualTo(enemy.Definition.attackRange * 2f));

            var heroTransform = bootstrapper.Hero.transform;
            var outOfRangePosition =
                heroTransform.position + Vector3.right * (focusRange + 0.5f);
            enemy.transform.position = outOfRangePosition;
            spawnTransform.position = outOfRangePosition;

            yield return null;
            yield return null;

            Assert.That(
                Vector3.Distance(enemy.transform.position, outOfRangePosition),
                Is.EqualTo(0f).Within(0.001f),
                "An idle enemy should not move toward the hero outside its focus range.");

            var characterController = bootstrapper.Hero.GetComponent<CharacterController>();
            characterController.enabled = false;
            var insideFocusDistance =
                (focusRange + enemy.Definition.attackRange) * 0.5f;
            heroTransform.position =
                enemy.transform.position - Vector3.right * insideFocusDistance;
            characterController.enabled = true;

            var initialEnemyX = enemy.transform.position.x;
            var chaseDeadline = Time.time + 1f;
            while (enemy.transform.position.x >= initialEnemyX && Time.time < chaseDeadline)
            {
                yield return null;
            }

            Assert.That(
                enemy.transform.position.x,
                Is.LessThan(initialEnemyX),
                "An idle enemy should chase the hero after entering its focus range.");
        }

        [UnityTest]
        public IEnumerator LivingActors_ShowProjectedHealthBannersWithCurrentValues()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null);
            if (bootstrapper.IsPaused)
            {
                bootstrapper.TogglePause();
            }

            DisableSceneEnemies();
            bootstrapper.Hero.enabled = false;
            var enemy = CreateTestEnemy(
                bootstrapper,
                new TestCombatSession(false),
                false,
                out _);
            var enemyId = enemy.GetEntityId();
            yield return null;

            var heroBanner = GameObject.Find("Hero health banner");
            var enemyBanner = GameObject.Find($"Mob health banner {enemyId}");
            Assert.That(heroBanner, Is.Not.Null);
            Assert.That(enemyBanner, Is.Not.Null);
            Assert.That(heroBanner.transform.parent.name, Is.EqualTo("Safe area"));
            Assert.That(enemyBanner.transform.parent.name, Is.EqualTo("Safe area"));

            var heroHealthText = heroBanner.transform
                .Find("Health values")
                .GetComponent<UnityEngine.UI.Text>();
            var enemyHealthText = enemyBanner.transform
                .Find("Health values")
                .GetComponent<UnityEngine.UI.Text>();
            var heroHealthFill = heroBanner.transform
                .Find("Health bar track/Health bar fill")
                .GetComponent<UnityEngine.UI.Image>();
            var enemyHealthFill = enemyBanner.transform
                .Find("Health bar track/Health bar fill")
                .GetComponent<UnityEngine.UI.Image>();
            Assert.That(heroHealthText.text, Does.Contain("Hero"));
            Assert.That(enemyHealthText.text, Does.Contain(enemy.DisplayName));
            Assert.That(heroHealthFill.fillAmount, Is.EqualTo(1f).Within(0.001f));
            Assert.That(enemyHealthFill.fillAmount, Is.EqualTo(1f).Within(0.001f));

            var camera = FindTestComponent<CameraFollow>().GetComponent<Camera>();
            var safeArea = GameObject.Find("Safe area").GetComponent<RectTransform>();
            var enemyScreenPosition = camera.WorldToScreenPoint(
                enemy.transform.position + Vector3.up * 2.75f);
            Assert.That(
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    safeArea,
                    new Vector2(enemyScreenPosition.x, enemyScreenPosition.y),
                    null,
                    out var expectedEnemyPosition),
                Is.True);
            Assert.That(
                Vector2.Distance(
                    enemyBanner.GetComponent<RectTransform>().anchoredPosition,
                    expectedEnemyPosition),
                Is.LessThan(0.1f),
                "The mob health banner should follow the actor's projected position.");

            var previousHeroText = heroHealthText.text;
            Assert.That(bootstrapper.ReceiveHeroDamage(25f, DamageType.Axe), Is.False);
            enemy.TakeDamage(20f);
            yield return null;

            Assert.That(heroHealthText.text, Is.Not.EqualTo(previousHeroText));
            Assert.That(heroHealthFill.fillAmount, Is.EqualTo(
                bootstrapper.CurrentHealth / bootstrapper.MaximumHealth).Within(0.001f));
            Assert.That(enemyHealthText.text, Does.Contain("60 / 80 HP"));
            Assert.That(enemyHealthFill.fillAmount, Is.EqualTo(0.75f).Within(0.001f));

            enemy.TakeDamage(10000f);
            yield return null;
            yield return null;
            Assert.That(
                GameObject.Find($"Mob health banner {enemyId}"),
                Is.Null,
                "A defeated mob's health banner should be removed.");
        }

        [UnityTest]
        public IEnumerator ReturningMobs_HealAtTenPercentPerSecondUntilFullHealth()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null);
            if (bootstrapper.IsPaused)
            {
                bootstrapper.TogglePause();
            }

            DisableSceneEnemies();
            bootstrapper.Hero.enabled = false;
            var healingEnemy = CreateTestEnemy(
                bootstrapper,
                new TestCombatSession(false),
                true,
                out _);
            var cappedEnemy = CreateTestEnemy(
                bootstrapper,
                new TestCombatSession(false),
                true,
                out _);
            var healToFullEnemy = CreateTestEnemy(
                bootstrapper,
                new TestCombatSession(false),
                true,
                out var healToFullSpawn);
            healingEnemy.Definition.chaseTimeoutSeconds = 0.05f;
            cappedEnemy.Definition.chaseTimeoutSeconds = 0.05f;
            healToFullEnemy.Definition.chaseTimeoutSeconds = 0.05f;
            healingEnemy.Definition.movementSpeed = 0.1f;
            cappedEnemy.Definition.movementSpeed = 0.1f;
            healToFullEnemy.Definition.movementSpeed = 10f;
            Assert.That(
                healingEnemy.Definition.returnHealFractionPerSecond,
                Is.EqualTo(0.1f),
                "Mobs should heal at exactly 10% of their maximum HP per second.");

            var heroController = bootstrapper.Hero.GetComponent<CharacterController>();
            heroController.enabled = false;
            bootstrapper.Hero.transform.position += Vector3.right * 20f;
            heroController.enabled = true;
            healingEnemy.TakeDamage(40f);
            cappedEnemy.TakeDamage(0.1f);
            healToFullEnemy.TakeDamage(10f);
            yield return new WaitForSeconds(0.3f);

            var healthBeforeMeasurement = healingEnemy.CurrentHealth;
            var measurementStartedAt = Time.time;
            yield return new WaitForSeconds(0.5f);
            var elapsed = Time.time - measurementStartedAt;
            var expectedHealing = healingEnemy.MaximumHealth *
                healingEnemy.Definition.returnHealFractionPerSecond *
                elapsed;
            Assert.That(
                healingEnemy.CurrentHealth - healthBeforeMeasurement,
                Is.EqualTo(expectedHealing).Within(0.25f),
                "A returning mob should regenerate 10% of maximum HP per second.");
            Assert.That(
                cappedEnemy.CurrentHealth,
                Is.EqualTo(cappedEnemy.MaximumHealth).Within(0.001f),
                "Healing should stop at the mob's maximum HP.");

            var returnDeadline = Time.time + 1.5f;
            while (Vector3.Distance(
                       healToFullEnemy.transform.position,
                       healToFullSpawn.position) > 0.08f &&
                   Time.time < returnDeadline)
            {
                yield return null;
            }

            Assert.That(
                Vector3.Distance(healToFullEnemy.transform.position, healToFullSpawn.position),
                Is.LessThanOrEqualTo(0.08f),
                "The injured mob should return to its spawn before completing recovery.");
            Assert.That(
                healToFullEnemy.CurrentHealth,
                Is.LessThan(healToFullEnemy.MaximumHealth),
                "The test mob should still be injured when it reaches its spawn.");
            yield return new WaitForSeconds(1.5f);
            Assert.That(
                healToFullEnemy.CurrentHealth,
                Is.EqualTo(healToFullEnemy.MaximumHealth).Within(0.001f),
                "A mob should continue healing while idle at its spawn until it is fully restored.");
        }

        [UnityTest]
        public IEnumerator RespawnableMobDefeat_ShowsSpawnPointMarkerAndRemainingRespawns()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null);
            if (bootstrapper.IsPaused)
            {
                bootstrapper.TogglePause();
            }

            var spawnPoint = FindTestRespawnableSpawnPoint();
            Assert.That(spawnPoint, Is.Not.Null);
            Assert.That(spawnPoint.ActiveActor, Is.Not.Null);
            Assert.That(spawnPoint.MaximumRespawnsPerDay, Is.GreaterThan(0));

            var spawnId = spawnPoint.SpawnId;
            spawnPoint.ActiveActor.TakeDamage(10000f);
            Assert.That(spawnPoint.ShouldShowRespawnStatus, Is.True);
            yield return null;

            var row = GameObject.Find($"Respawn status {spawnId}");
            Assert.That(row, Is.Not.Null, "A defeated respawnable mob should have a spawn-point marker.");
            Assert.That(row.transform.parent.name, Is.EqualTo("Safe area"));
            Assert.That(GameObject.Find("Mob respawn panel"), Is.Null);
            Assert.That(row.transform.Find($"Respawn cooldown {spawnId}"), Is.Not.Null);
            var pips = row.transform.Find($"Daily respawn indicators {spawnId}");
            Assert.That(pips, Is.Not.Null);
            Assert.That(pips.childCount, Is.EqualTo(spawnPoint.MaximumRespawnsPerDay));
            Assert.That(
                row.transform.Find("Respawn cooldown track/Respawn cooldown progress"),
                Is.Not.Null);

            var cameraFollow = FindTestComponent<CameraFollow>();
            Assert.That(cameraFollow, Is.Not.Null, "The test scene must contain its world camera.");
            var camera = cameraFollow.GetComponent<Camera>();
            var safeArea = GameObject.Find("Safe area").GetComponent<RectTransform>();
            var screenPosition = camera.WorldToScreenPoint(
                spawnPoint.transform.position + Vector3.up * 2.2f);
            Assert.That(
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    safeArea,
                    new Vector2(screenPosition.x, screenPosition.y),
                    null,
                    out var expectedLocalPosition),
                Is.True);
            Assert.That(
                Vector2.Distance(
                    row.GetComponent<RectTransform>().anchoredPosition,
                    expectedLocalPosition),
                Is.LessThan(0.1f),
                "The marker should track the spawn point's projected screen position.");

            var remainingRespawns = Mathf.Clamp(
                spawnPoint.MaximumRespawnsPerDay - spawnPoint.RespawnsToday,
                0,
                spawnPoint.MaximumRespawnsPerDay);
            for (var pipIndex = 0; pipIndex < pips.childCount; pipIndex++)
            {
                var expectedColor = pipIndex < remainingRespawns
                    ? new Color(0.98f, 0.72f, 0.24f, 1f)
                    : new Color(0.16f, 0.2f, 0.26f, 1f);
                Assert.That(
                    pips.GetChild(pipIndex).GetComponent<UnityEngine.UI.Image>().color,
                    Is.EqualTo(expectedColor));
            }

            var nextRespawnUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
            var savedRespawn = Array.Find(
                bootstrapper.ProgressData.enemyRespawns,
                entry => entry != null && entry.spawnId == spawnId);
            Assert.That(savedRespawn, Is.Not.Null);
            savedRespawn.nextRespawnUnixSeconds = nextRespawnUnixSeconds;
            savedRespawn.respawnsToday = spawnPoint.MaximumRespawnsPerDay;
            savedRespawn.utcDayKey = DateTime.UtcNow.Date.Ticks;
            bootstrapper.TryRespawnEnemy(spawnId);
            yield return null;

            Assert.That(
                spawnPoint.RespawnsToday,
                Is.EqualTo(spawnPoint.MaximumRespawnsPerDay));
            for (var pipIndex = 0; pipIndex < pips.childCount; pipIndex++)
            {
                Assert.That(
                    pips.GetChild(pipIndex).GetComponent<UnityEngine.UI.Image>().color,
                    Is.EqualTo(new Color(0.16f, 0.2f, 0.26f, 1f)),
                    "Consumed daily respawns should appear as muted pips.");
            }

            savedRespawn.utcDayKey = DateTime.UtcNow.Date.Ticks - TimeSpan.TicksPerDay;
            bootstrapper.TryRespawnEnemy(spawnId);
            yield return null;

            Assert.That(spawnPoint.RespawnsToday, Is.Zero);
            for (var pipIndex = 0; pipIndex < pips.childCount; pipIndex++)
            {
                Assert.That(
                    pips.GetChild(pipIndex).GetComponent<UnityEngine.UI.Image>().color,
                    Is.EqualTo(new Color(0.98f, 0.72f, 0.24f, 1f)),
                    "All pips should show the reset daily allowance after the UTC day changes.");
            }
        }

        [UnityTest]
        public IEnumerator LethalDamage_RespawnsAfterThreeSecondsAtHalfHealth()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null, "The test scene must contain a game bootstrapper.");
            if (bootstrapper.IsPaused)
            {
                bootstrapper.TogglePause();
            }

            DisableSceneEnemies();
            var spawnPosition = bootstrapper.Hero.transform.position;
            var characterController = bootstrapper.Hero.GetComponent<CharacterController>();
            characterController.enabled = false;
            bootstrapper.Hero.transform.position = spawnPosition + Vector3.right * 2f;
            characterController.enabled = true;
            var deathPosition = bootstrapper.Hero.transform.position;

            Assert.That(bootstrapper.ReceiveHeroDamage(100f, DamageType.Axe), Is.False);
            Assert.That(bootstrapper.CurrentHealth, Is.EqualTo(5f).Within(0.01f));
            Assert.That(bootstrapper.ReceiveHeroDamage(10f, DamageType.Axe), Is.True);
            Assert.That(bootstrapper.IsHeroRespawning, Is.True);
            Assert.That(bootstrapper.CurrentHealth, Is.Zero);
            Assert.That(bootstrapper.Hero.IsRespawning, Is.True);
            Assert.That(bootstrapper.Hero.transform.position, Is.EqualTo(deathPosition));

            yield return new WaitForSeconds(2.8f);
            Assert.That(bootstrapper.IsHeroRespawning, Is.True);
            Assert.That(bootstrapper.CurrentHealth, Is.Zero);

            var respawnDeadline = Time.time + 0.5f;
            while (bootstrapper.IsHeroRespawning && Time.time < respawnDeadline)
                yield return null;
            Assert.That(bootstrapper.IsHeroRespawning, Is.False);
            Assert.That(bootstrapper.Hero.IsRespawning, Is.False);
            Assert.That(bootstrapper.CurrentHealth, Is.InRange(50f, 50f + Time.deltaTime * 2f + 0.01f));
            Assert.That(bootstrapper.Hero.transform.position, Is.EqualTo(spawnPosition));
        }

        [UnityTest]
        public IEnumerator LethalEnemy_RetreatsAndDoesNotAttackUntilThreeSecondLockExpires()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null, "The test scene must contain a game bootstrapper.");
            if (bootstrapper.IsPaused)
            {
                bootstrapper.TogglePause();
            }

            DisableSceneEnemies();
            bootstrapper.Hero.enabled = false;

            var session = new TestCombatSession();
            var enemy = CreateTestEnemy(bootstrapper, session, false, out var spawnTransform);
            var otherSession = new TestCombatSession(false)
            {
                IsHeroRespawning = true
            };
            CreateTestEnemy(bootstrapper, otherSession, true, out _);
            var distanceAtSpawn = Vector3.Distance(enemy.transform.position, spawnTransform.position);
            var deathDeadline = Time.time + 1f;
            while (session.ReceivedDamageCount == 0 && Time.time < deathDeadline)
            {
                yield return null;
            }

            Assert.That(session.ReceivedDamageCount, Is.EqualTo(1));
            Assert.That(session.IsHeroRespawning, Is.True);
            yield return new WaitForSeconds(0.2f);
            Assert.That(
                Vector3.Distance(enemy.transform.position, spawnTransform.position),
                Is.LessThan(distanceAtSpawn));

            yield return new WaitForSeconds(2.7f);
            Assert.That(session.ReceivedDamageCount, Is.EqualTo(1));
            Assert.That(otherSession.ReceivedDamageCount, Is.Zero);

            session.IsHeroRespawning = false;
            yield return new WaitForSeconds(0.2f);
            Assert.That(session.ReceivedDamageCount, Is.GreaterThan(1));
            Assert.That(otherSession.ReceivedDamageCount, Is.Zero);

            otherSession.IsHeroRespawning = false;
            yield return new WaitForSeconds(0.2f);
            Assert.That(otherSession.ReceivedDamageCount, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator HeroHit_CancelsLethalEnemyRetreatLock()
        {
            var bootstrapper = FindTestComponent<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null);
            if (bootstrapper.IsPaused)
            {
                bootstrapper.TogglePause();
            }

            DisableSceneEnemies();
            bootstrapper.Hero.enabled = false;

            var session = new TestCombatSession();
            var enemy = CreateTestEnemy(bootstrapper, session, true, out _);
            var deathDeadline = Time.time + 1f;
            while (session.ReceivedDamageCount == 0 && Time.time < deathDeadline)
            {
                yield return null;
            }

            Assert.That(session.ReceivedDamageCount, Is.EqualTo(1));
            var lethalHitTime = Time.time;
            session.IsHeroRespawning = false;
            enemy.TakeDamage(1f);

            var followUpAttackDeadline = Time.time + 1f;
            while (session.ReceivedDamageCount == 1 && Time.time < followUpAttackDeadline)
            {
                yield return null;
            }

            Assert.That(session.ReceivedDamageCount, Is.GreaterThan(1));
            Assert.That(Time.time - lethalHitTime, Is.LessThan(1f));
        }

        private static void DisableSceneEnemies()
        {
            var testScene = SceneManager.GetSceneByPath(TestScenePath);
            if (!testScene.IsValid() || !testScene.isLoaded)
            {
                return;
            }

            foreach (var rootObject in testScene.GetRootGameObjects())
            {
                foreach (var enemy in rootObject.GetComponentsInChildren<EnemyActor>(true))
                {
                    enemy.enabled = false;
                }
            }
        }

        private static T FindTestComponent<T>() where T : Component
        {
            var testScene = SceneManager.GetSceneByPath(TestScenePath);
            if (!testScene.IsValid() || !testScene.isLoaded)
            {
                return null;
            }

            var rootObjects = testScene.GetRootGameObjects();
            foreach (var rootObject in rootObjects)
            {
                var component = rootObject.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static EnemySpawnPoint FindTestRespawnableSpawnPoint()
        {
            var testScene = SceneManager.GetSceneByPath(TestScenePath);
            if (!testScene.IsValid() || !testScene.isLoaded)
            {
                return null;
            }

            foreach (var rootObject in testScene.GetRootGameObjects())
            {
                foreach (var spawnPoint in rootObject.GetComponentsInChildren<EnemySpawnPoint>(true))
                {
                    if (spawnPoint.Definition != null && spawnPoint.Definition.canRespawn)
                    {
                        return spawnPoint;
                    }
                }
            }

            return null;
        }

        private EnemyActor CreateTestEnemy(
            GameBootstrapper bootstrapper,
            TestCombatSession session,
            bool returnAndHeal,
            out Transform spawnTransform)
        {
            var heroPosition = bootstrapper.Hero.transform.position;
            var spawnObject = new GameObject("Test enemy spawn");
            spawnObject.transform.SetParent(bootstrapper.transform, false);
            spawnObject.transform.position = heroPosition + new Vector3(0f, 0f, 1.5f);
            spawnTransform = spawnObject.transform;

            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _testDefinitions.Add(definition);
            definition.stableId = "test.enemy.killer";
            definition.displayName = "Test Killer";
            definition.archetype = EnemyArchetype.Boss;
            definition.attackDamageType = DamageType.Axe;
            definition.maximumHealth = 80f;
            definition.attackDamage = 10f;
            definition.attackCycleSeconds = 0.25f;
            definition.windupSeconds = 0.05f;
            definition.attackRange = 2f;
            definition.movementSpeed = 3f;
            definition.chaseTimeoutSeconds = 5f;
            definition.returnHealFractionPerSecond = 0.1f;
            definition.returnAndHeal = returnAndHeal;
            definition.canRespawn = false;
            definition.respawnCooldownSeconds = 0;
            definition.maximumRespawnsPerDay = 0;
            definition.grayboxColor = Color.red;

            var spawnPoint = spawnObject.AddComponent<EnemySpawnPoint>();
            spawnPoint.Configure(session, bootstrapper.CombatWorld, bootstrapper.Hero.transform, 0);
            var enemyObject = new GameObject("Test Killer");
            enemyObject.transform.SetParent(bootstrapper.transform, false);
            enemyObject.transform.position = heroPosition + new Vector3(1.5f, 0f, 0f);
            var enemy = enemyObject.AddComponent<EnemyActor>();
            enemy.Initialize(
                definition,
                bootstrapper.Hero.transform,
                spawnTransform,
                session,
                bootstrapper.CombatWorld,
                spawnPoint);
            return enemy;
        }

        private static void DeleteTestSaveDirectory()
        {
            var testSaveDirectory = Path.Combine(
                UnityEngine.Application.temporaryCachePath,
                TestSaveDirectoryName);
            if (Directory.Exists(testSaveDirectory))
            {
                Directory.Delete(testSaveDirectory, true);
            }
        }

#if UNITY_EDITOR
        private static void SetTestSceneEnabled(bool enabled)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (var i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == TestScenePath)
                {
                    scenes[i] = new EditorBuildSettingsScene(TestScenePath, enabled);
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            }

            if (enabled)
            {
                scenes.Add(new EditorBuildSettingsScene(TestScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
#endif

        private sealed class TestCombatSession : ICombatSession
        {
            private readonly bool _killOnFirstHit;

            public TestCombatSession(bool killOnFirstHit = true)
            {
                _killOnFirstHit = killOnFirstHit;
            }

            public float CurrentHealth => 100f;
            public float MaximumHealth => 100f;
            public float AxeDamage => 10f;
            public int AxeAttackPoints => 0;
            public float HeadAxeArmor => 0f;
            public float TorsoAxeArmor => 0f;
            public float DodgeChance => 0f;
            public float BlockChance => 0f;
            public float BlockDamageMultiplier => 0.8f;
            public bool IsHeroRespawning { get; set; }
            public float EnemyPostKillRetreatSeconds => 3f;
            public bool BossDefeated => false;
            public int ReceivedDamageCount { get; private set; }

            public bool ReceiveHeroDamage(float rawDamage, DamageType damageType)
            {
                ReceivedDamageCount++;
                if (_killOnFirstHit && ReceivedDamageCount == 1)
                {
                    IsHeroRespawning = true;
                    return true;
                }

                return false;
            }

            public void RecordHeroPosition(float x, float y, float z)
            {
            }

            public void HandleEnemyDefeated(EnemyDefeatRecord defeat)
            {
            }

            public void TryRespawnEnemy(string spawnId)
            {
            }
        }
    }
}
