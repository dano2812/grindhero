using System.Collections;
using System.Collections.Generic;
using System.IO;
using GrindHero;
using GrindHero.Presentation.Input;
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
            var loadOperation = SceneManager.LoadSceneAsync(TestScenePath, LoadSceneMode.Additive);
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
                var unloadOperation = SceneManager.UnloadSceneAsync(testScene);
                if (unloadOperation != null)
                {
                    yield return unloadOperation;
                }
            }

            DeleteTestSaveDirectory();
        }

        [UnityTest]
        public IEnumerator GeneratedTestScene_BootsAndSpawnsTheFirstSlice()
        {
            var bootstrapper = Object.FindAnyObjectByType<GameBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null, "The test scene must contain a game bootstrapper.");
            Assert.That(bootstrapper.IsReady, Is.True, "The game bootstrapper must finish initialization.");
            Assert.That(bootstrapper.Hero, Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<DynamicJoystickInput>(), Is.Not.Null);
            Assert.That(bootstrapper.CombatWorld.AliveEnemyCount, Is.EqualTo(4));
            Assert.That(bootstrapper.CurrentHealth, Is.EqualTo(100f));
            Assert.That(bootstrapper.Gate.IsUnlocked, Is.False);

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
    }
}
