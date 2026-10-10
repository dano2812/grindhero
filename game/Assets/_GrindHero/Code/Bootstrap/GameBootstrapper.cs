using System;
using System.Collections.Generic;
using System.IO;
using GrindHero.Application.Combat;
using GrindHero.Application.Progression;
using GrindHero.Domain.Combat;
using GrindHero.Domain.Progression;
using GrindHero.Domain.World;
using GrindHero.Infrastructure.Persistence;
using GrindHero.Presentation;
using GrindHero.Presentation.Definitions;
using GrindHero.Presentation.Input;
using GrindHero.Presentation.UI;
using GrindHero.Presentation.World;
using UnityEngine;

namespace GrindHero
{
    [RequireComponent(typeof(CombatWorld))]
    public sealed class GameBootstrapper : MonoBehaviour, ICombatSession
    {
        [SerializeField] private FirstSliceTuning tuning;
        [SerializeField] private HeroActor hero;
        [SerializeField] private Transform heroSpawnPoint;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private DynamicJoystickInput movementInput;
        [SerializeField] private HUDPresenter hud;
        [SerializeField] private ExitGate exitGate;
        [SerializeField] private EnemySpawnPoint[] enemySpawnPoints;
        [SerializeField, Tooltip("Optional relative directory under the temporary cache for isolated integration tests.")]
        private string saveDirectoryOverride;

        private readonly Dictionary<string, EnemySpawnPoint> _spawnPointsById =
            new Dictionary<string, EnemySpawnPoint>(StringComparer.Ordinal);

        private CombatWorld _combatWorld;
        private IPlayerProgressRepository _progressRepository;
        private ProgressionSession _progression;
        private bool _isReady;
        private bool _isPaused;
        private bool _pauseRequested;
        private bool _saveFailureReported;
        private float _nextSaveTime;

        public bool IsReady => _isReady;
        public bool IsPaused => _isPaused;
        public HeroActor Hero => hero;
        public CombatWorld CombatWorld => _combatWorld;
        public ExitGate Gate => exitGate;
        public PlayerProgressData ProgressData => _progression != null ? _progression.Data : null;
        public int AxeAttackPoints => _progression != null ? _progression.Data.axeAttackPoints : 0;

        public float CurrentHealth =>
            _progression != null ? _progression.Data.currentHealth : 0f;

        public float MaximumHealth
        {
            get
            {
                if (_progression == null || tuning == null)
                {
                    return 0f;
                }

                return ProgressionRules.CalculateMaximumHealth(
                    tuning.startingMaximumHealth,
                    _progression.Data.healthPoints,
                    tuning.healthPerPoint);
            }
        }

        public float AxeDamage
        {
            get
            {
                if (tuning == null || tuning.startingWeapon == null)
                {
                    return 0f;
                }

                var points = _progression != null ? _progression.Data.axeAttackPoints : 0;
                return (float)ProgressionRules.CalculateAxeDamage(
                    tuning.baseAxeDamage,
                    tuning.startingWeapon.weaponDamage,
                    points,
                    tuning.axeDamagePerPoint);
            }
        }

        public float HeadAxeArmor => GetArmorValue(tuning != null ? tuning.headArmor : null, DamageType.Axe);
        public float TorsoAxeArmor => GetArmorValue(tuning != null ? tuning.torsoArmor : null, DamageType.Axe);
        public float DodgeChance => tuning != null ? tuning.startingDodgeChance : 0f;
        public float BlockChance => tuning != null ? tuning.startingBlockChance : 0f;
        public float BlockDamageMultiplier => tuning != null ? tuning.blockDamageMultiplier : 0.8f;
        public bool BossDefeated => _progression != null && _progression.Data.firstBossDefeated;

        private void Start()
        {
            try
            {
                StartGame();
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is InvalidDataException ||
                exception is UnauthorizedAccessException ||
                exception is InvalidOperationException ||
                exception is ArgumentException)
            {
                Time.timeScale = 0f;
                if (hud != null)
                {
                    hud.ShowFatalError(
                        $"Unable to start the game safely. Progress was not reset.\n{exception.Message}");
                }

                Debug.LogError($"First-slice startup failed: {exception}", this);
            }
        }

        private void StartGame()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            ValidateSceneSetup();

            _combatWorld = GetComponent<CombatWorld>();
            _progressRepository = string.IsNullOrWhiteSpace(saveDirectoryOverride)
                ? JsonFilePlayerProgressRepository.CreateForCurrentPlatform()
                : new JsonFilePlayerProgressRepository(GetSaveDirectory());
            var loaded = _progressRepository.TryLoad(out var progressData, out var recoveredFromBackup);
            if (recoveredFromBackup)
            {
                Debug.LogWarning("The player save was recovered from its last-known-good backup.", this);
            }

            _progression = new ProgressionSession(progressData, _progressRepository);
            if (_progression.Data.enemyRespawns == null)
            {
                _progression.Data.enemyRespawns = Array.Empty<EnemyRespawnData>();
            }

            if (loaded)
            {
                hero.transform.position = new Vector3(
                    progressData.heroPositionX,
                    progressData.heroPositionY,
                    progressData.heroPositionZ);
                progressData.currentHealth = Mathf.Min(progressData.currentHealth, MaximumHealth);
            }
            else
            {
                progressData.currentHealth = MaximumHealth;
                StoreHeroPosition();
            }

            hero.Initialize(tuning, _combatWorld, movementInput, this, heroSpawnPoint);
            cameraFollow.Initialize(hero.transform);
            hud.Initialize(this, _combatWorld, hero, movementInput, TogglePause);
            exitGate.Initialize(this, hud, BossDefeated);
            if (recoveredFromBackup)
            {
                hud.ShowMessage(
                    "Recovered the last saved progress backup.",
                    new Color(1f, 0.82f, 0.28f));
            }

            ConfigureSpawnPoints();
            _isReady = true;
            InitializeSpawnPoints();

            if (progressData.currentHealth <= 0f)
            {
                RespawnHeroAfterDefeat();
            }

            _nextSaveTime = Time.unscaledTime + tuning.saveIntervalSeconds;
            if (_pauseRequested)
            {
                SetPaused(true);
            }
        }

        public void TogglePause()
        {
            SetPaused(!_isPaused);
        }

        private void Update()
        {
            if (!_isReady)
            {
                return;
            }

            if (!_isPaused && Time.unscaledTime >= _nextSaveTime)
            {
                SaveProgress();
                _nextSaveTime = Time.unscaledTime + tuning.saveIntervalSeconds;
            }
        }

        public void ReceiveHeroDamage(float rawDamage, DamageType damageType)
        {
            if (!_isReady || rawDamage <= 0f)
            {
                return;
            }

            var dodged = UnityEngine.Random.value < DodgeChance;
            var blocked = !dodged && UnityEngine.Random.value < BlockChance;
            var result = DamageResolver.Resolve(
                rawDamage,
                GetArmorValue(tuning.headArmor, damageType),
                GetArmorValue(tuning.torsoArmor, damageType),
                dodged,
                blocked,
                BlockDamageMultiplier);

            if (result.Dodged)
            {
                hud.ShowMessage("DODGE", new Color(0.3f, 0.85f, 1f));
            }
            else if (result.Blocked)
            {
                hud.ShowMessage("BLOCK", new Color(0.98f, 0.82f, 0.28f));
            }

            _progression.Data.currentHealth = Mathf.Max(
                0f,
                _progression.Data.currentHealth - (float)result.FinalDamage);
            if (_progression.Data.currentHealth <= 0f)
            {
                RespawnHeroAfterDefeat();
            }
        }

        public void RecordHeroPosition(float x, float y, float z)
        {
            if (_progression == null)
            {
                return;
            }

            _progression.Data.heroPositionX = x;
            _progression.Data.heroPositionY = y;
            _progression.Data.heroPositionZ = z;
        }

        public void HandleEnemyDefeated(EnemyDefeatRecord defeat)
        {
            if (!_isReady || string.IsNullOrWhiteSpace(defeat.SpawnId))
            {
                return;
            }

            var reward = new StatReward(defeat.RewardStat, defeat.RewardPoints);
            if (_progression.GrantReward(reward))
            {
                var statName = reward.Stat == RewardStat.Health ? "Health" : "Axe Attack";
                hud.ShowMessage($"+{reward.Points} {statName} point", new Color(0.5f, 1f, 0.6f));
            }

            if (defeat.Archetype == EnemyArchetype.Boss &&
                _progression.CompleteFirstBoss())
            {
                exitGate.SetUnlocked(true);
                hud.ShowMessage("BOSS DEFEATED — EXIT UNLOCKED", new Color(0.5f, 1f, 0.6f));
            }

            var state = GetRespawnState(defeat.SpawnId);
            var today = GetUtcDayKey();
            if (defeat.CanRespawn)
            {
                state = MobRespawnRules.Schedule(
                    state,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    defeat.RespawnCooldownSeconds,
                    today);
            }
            else
            {
                state = MobRespawnRules.MarkDefeated(state, today);
            }

            StoreRespawnState(defeat.SpawnId, state);
            if (_spawnPointsById.TryGetValue(defeat.SpawnId, out var spawnPoint))
            {
                spawnPoint.SetRespawnDeadline(state.NextRespawnUnixSeconds);
            }

            SaveProgress();
        }

        public void TryRespawnEnemy(string spawnId)
        {
            if (string.IsNullOrWhiteSpace(spawnId) ||
                !_spawnPointsById.TryGetValue(spawnId, out var spawnPoint))
            {
                return;
            }

            if (!_isReady || spawnPoint == null || spawnPoint.Definition == null)
            {
                return;
            }

            if (spawnPoint.Definition.IsBoss && BossDefeated)
            {
                spawnPoint.SetRespawnDeadline(0);
                return;
            }

            var state = GetRespawnState(spawnPoint.SpawnId);
            var updated = MobRespawnRules.TryConsumeRespawn(
                state,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Mathf.Min(
                    tuning.maximumMobRespawnsPerDay,
                    spawnPoint.Definition.maximumRespawnsPerDay),
                GetUtcDayKey(),
                out var nextState);

            if (!AreEqual(state, nextState))
            {
                StoreRespawnState(spawnPoint.SpawnId, nextState);
                SaveProgress();
            }

            spawnPoint.SetRespawnDeadline(nextState.NextRespawnUnixSeconds);
            if (updated)
            {
                spawnPoint.SpawnEnemy();
            }
        }

        private void ConfigureSpawnPoints()
        {
            if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            {
                throw new InvalidOperationException("The first-slice scene must define at least one enemy spawn point.");
            }

            _spawnPointsById.Clear();
            foreach (var spawnPoint in enemySpawnPoints)
            {
                if (spawnPoint == null ||
                    string.IsNullOrWhiteSpace(spawnPoint.SpawnId) ||
                    spawnPoint.Definition == null)
                {
                    throw new InvalidOperationException("Every enemy spawn point needs a stable ID and definition.");
                }

                var definitionError = spawnPoint.Definition.ValidateDefinition();
                if (definitionError != null)
                {
                    throw new InvalidOperationException(definitionError);
                }

                if (_spawnPointsById.ContainsKey(spawnPoint.SpawnId))
                {
                    throw new InvalidOperationException($"Duplicate enemy spawn ID '{spawnPoint.SpawnId}'.");
                }

                _spawnPointsById.Add(spawnPoint.SpawnId, spawnPoint);
                spawnPoint.Configure(this, _combatWorld, hero.transform);
            }
        }

        private void InitializeSpawnPoints()
        {
            var today = GetUtcDayKey();
            foreach (var spawnPoint in enemySpawnPoints)
            {
                if (spawnPoint.Definition.IsBoss && BossDefeated)
                {
                    continue;
                }

                if (!TryFindRespawnData(spawnPoint.SpawnId, out var savedData))
                {
                    var initialState = MobRespawnRules.MarkSpawned(
                        new MobRespawnState(0, 0, today),
                        today);
                    StoreRespawnState(spawnPoint.SpawnId, initialState);
                    spawnPoint.SpawnEnemy();
                    SaveProgress();
                    continue;
                }

                var state = ToRespawnState(savedData);
                if (state.IsAlive)
                {
                    var currentDayState = MobRespawnRules.MarkSpawned(state, today);
                    if (!AreEqual(state, currentDayState))
                    {
                        StoreRespawnState(spawnPoint.SpawnId, currentDayState);
                        SaveProgress();
                    }

                    spawnPoint.SpawnEnemy();
                    continue;
                }

                spawnPoint.SetRespawnDeadline(state.NextRespawnUnixSeconds);
                if (state.NextRespawnUnixSeconds > 0)
                {
                    TryRespawnEnemy(spawnPoint.SpawnId);
                }
            }
        }

        private MobRespawnState GetRespawnState(string spawnId)
        {
            return TryFindRespawnData(spawnId, out var data)
                ? ToRespawnState(data)
                : new MobRespawnState(0, 0, GetUtcDayKey());
        }

        private bool TryFindRespawnData(string spawnId, out EnemyRespawnData data)
        {
            foreach (var candidate in _progression.Data.enemyRespawns)
            {
                if (candidate != null &&
                    string.Equals(candidate.spawnId, spawnId, StringComparison.Ordinal))
                {
                    data = candidate;
                    return true;
                }
            }

            data = null;
            return false;
        }

        private void StoreRespawnState(string spawnId, MobRespawnState state)
        {
            if (TryFindRespawnData(spawnId, out var existing))
            {
                existing.nextRespawnUnixSeconds = state.NextRespawnUnixSeconds;
                existing.respawnsToday = state.RespawnsToday;
                existing.utcDayKey = state.UtcDayKey;
                existing.isAlive = state.IsAlive;
                return;
            }

            var existingEntries = _progression.Data.enemyRespawns;
            var updatedEntries = new EnemyRespawnData[existingEntries.Length + 1];
            Array.Copy(existingEntries, updatedEntries, existingEntries.Length);
            updatedEntries[updatedEntries.Length - 1] = new EnemyRespawnData
            {
                spawnId = spawnId,
                nextRespawnUnixSeconds = state.NextRespawnUnixSeconds,
                respawnsToday = state.RespawnsToday,
                utcDayKey = state.UtcDayKey,
                isAlive = state.IsAlive
            };
            _progression.Data.enemyRespawns = updatedEntries;
        }

        private void RespawnHeroAfterDefeat()
        {
            _progression.Data.currentHealth = MaximumHealth * tuning.respawnHealthFraction;
            hero.RespawnAtSpawn();
            hud.ShowMessage("Defeated — returned to spawn at half health.", Color.white);
            SaveProgress();
        }

        private void SaveProgress()
        {
            if (_progression == null)
            {
                return;
            }

            if (hero != null)
            {
                RecordHeroPosition(
                    hero.transform.position.x,
                    hero.transform.position.y,
                    hero.transform.position.z);
            }

            try
            {
                _progression.Save();
                if (_saveFailureReported)
                {
                    _saveFailureReported = false;
                    hud.ClearSaveError();
                }
            }
            catch (IOException exception)
            {
                ReportSaveFailure(exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                ReportSaveFailure(exception);
            }
            catch (InvalidDataException exception)
            {
                ReportSaveFailure(exception);
            }
        }

        private void ReportSaveFailure(Exception exception)
        {
            if (_saveFailureReported)
            {
                return;
            }

            _saveFailureReported = true;
            const string message = "Could not save progress. It may be lost if the app closes.";
            if (hud != null)
            {
                hud.ShowSaveError(message);
            }

            Debug.LogError($"{message} {exception}", this);
        }

        private void StoreHeroPosition()
        {
            RecordHeroPosition(
                hero.transform.position.x,
                hero.transform.position.y,
                hero.transform.position.z);
        }

        private string GetSaveDirectory()
        {
            return Path.IsPathRooted(saveDirectoryOverride)
                ? saveDirectoryOverride
                : Path.Combine(UnityEngine.Application.temporaryCachePath, saveDirectoryOverride);
        }

        private void ValidateSceneSetup()
        {
            if (tuning == null ||
                hero == null ||
                heroSpawnPoint == null ||
                cameraFollow == null ||
                movementInput == null ||
                hud == null ||
                exitGate == null)
            {
                throw new InvalidOperationException("The first-slice scene is missing a required bootstrap reference.");
            }

            var tuningError = tuning.ValidateDefinition();
            if (tuningError != null)
            {
                throw new InvalidOperationException(tuningError);
            }
        }

        private static float GetArmorValue(ArmorDefinition armor, DamageType damageType)
        {
            return armor != null && armor.protectsAgainst == damageType
                ? armor.defense
                : 0f;
        }

        private static MobRespawnState ToRespawnState(EnemyRespawnData data)
        {
            return new MobRespawnState(
                data.nextRespawnUnixSeconds,
                data.respawnsToday,
                data.utcDayKey,
                data.isAlive);
        }

        private static bool AreEqual(MobRespawnState left, MobRespawnState right)
        {
            return left.NextRespawnUnixSeconds == right.NextRespawnUnixSeconds &&
                   left.RespawnsToday == right.RespawnsToday &&
                   left.UtcDayKey == right.UtcDayKey &&
                   left.IsAlive == right.IsAlive;
        }

        private static long GetUtcDayKey()
        {
            return DateTime.UtcNow.Date.Ticks;
        }

        private void OnApplicationPause(bool paused)
        {
            SetPaused(paused);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetPaused(!hasFocus);
        }

        private void SetPaused(bool paused)
        {
            if (!_isReady)
            {
                _pauseRequested = paused;
                return;
            }

            if (_isPaused == paused)
            {
                return;
            }

            if (paused)
            {
                SaveProgress();
            }

            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            hud.SetPaused(paused);
            if (!paused)
            {
                foreach (var spawnPoint in enemySpawnPoints)
                {
                    if (spawnPoint.ActiveActor == null)
                    {
                        TryRespawnEnemy(spawnPoint.SpawnId);
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            SaveProgress();
        }

        private void OnDestroy()
        {
            if (_isReady)
            {
                SaveProgress();
            }

            Time.timeScale = 1f;
        }
    }
}
