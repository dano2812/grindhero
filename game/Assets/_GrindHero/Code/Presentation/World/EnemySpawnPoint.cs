using System;
using GrindHero.Application.Combat;
using GrindHero.Domain.Combat;
using GrindHero.Domain.World;
using GrindHero.Presentation;
using GrindHero.Presentation.Definitions;
using UnityEngine;

namespace GrindHero.Presentation.World
{
    public sealed class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnId;
        [SerializeField] private EnemyDefinition definition;

        private ICombatSession _combatSession;
        private CombatWorld _combatWorld;
        private Transform _hero;
        private EnemyActor _activeActor;
        private long _nextRespawnUnixSeconds;
        private int _respawnsToday;
        private long _utcDayKey;
        private int _maximumRespawnsPerDay;
        private float _nextRespawnCheckTime;

        public string SpawnId => spawnId;
        public EnemyDefinition Definition => definition;
        public EnemyActor ActiveActor => _activeActor;
        public int RespawnsToday =>
            _utcDayKey == DateTime.UtcNow.Date.Ticks ? _respawnsToday : 0;
        public int MaximumRespawnsPerDay => _maximumRespawnsPerDay;
        public bool ShouldShowRespawnStatus =>
            _activeActor == null &&
            _nextRespawnUnixSeconds > 0 &&
            definition != null &&
            definition.canRespawn &&
            _maximumRespawnsPerDay > 0;
        public float RespawnSecondsRemaining
        {
            get
            {
                if (_nextRespawnUnixSeconds <= 0)
                {
                    return 0f;
                }

                var currentUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
                return Mathf.Max(0f, (float)(_nextRespawnUnixSeconds - currentUnixSeconds));
            }
        }
        public float RespawnCooldownProgress
        {
            get
            {
                if (definition == null || definition.respawnCooldownSeconds <= 0)
                {
                    return 1f;
                }

                return Mathf.Clamp01(
                    1f - RespawnSecondsRemaining / definition.respawnCooldownSeconds);
            }
        }

        public void Configure(
            ICombatSession combatSession,
            CombatWorld combatWorld,
            Transform hero,
            int maximumRespawnsPerDay)
        {
            if (maximumRespawnsPerDay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumRespawnsPerDay));
            }

            _combatSession = combatSession;
            _combatWorld = combatWorld;
            _hero = hero;
            _maximumRespawnsPerDay = maximumRespawnsPerDay;
        }

        public void SetRespawnDeadline(long unixSeconds)
        {
            if (_nextRespawnUnixSeconds == unixSeconds)
            {
                return;
            }

            _nextRespawnUnixSeconds = unixSeconds;
            _nextRespawnCheckTime = 0f;
        }

        public void SetRespawnState(MobRespawnState state)
        {
            SetRespawnDeadline(state.NextRespawnUnixSeconds);
            _respawnsToday = state.RespawnsToday;
            _utcDayKey = state.UtcDayKey;
        }

        public void SpawnEnemy()
        {
            if (_activeActor != null || definition == null)
            {
                return;
            }

            _nextRespawnUnixSeconds = 0;
            _nextRespawnCheckTime = 0f;
            var enemyObject = new GameObject(definition.displayName);
            enemyObject.transform.SetPositionAndRotation(transform.position, transform.rotation);

            var bodyCollider = enemyObject.AddComponent<CapsuleCollider>();
            bodyCollider.center = new Vector3(0f, 1f, 0f);
            bodyCollider.radius = 0.42f;
            bodyCollider.height = 2f;
            bodyCollider.isTrigger = true;

            _activeActor = enemyObject.AddComponent<EnemyActor>();
            _activeActor.Initialize(
                definition,
                _hero,
                transform,
                _combatSession,
                _combatWorld,
                this);
        }

        public void HandleEnemyDefeated(EnemyActor actor)
        {
            if (actor == null || actor != _activeActor)
            {
                return;
            }

            _activeActor = null;
            var defeat = new EnemyDefeatRecord(
                spawnId,
                definition.archetype,
                definition.rewardStat,
                definition.rewardPoints,
                definition.canRespawn,
                definition.maximumRespawnsPerDay,
                definition.respawnCooldownSeconds);
            _combatSession.HandleEnemyDefeated(defeat);
        }

        private void Update()
        {
            if (_activeActor != null ||
                _combatSession == null ||
                _nextRespawnUnixSeconds <= 0 ||
                Time.unscaledTime < _nextRespawnCheckTime)
            {
                return;
            }

            _nextRespawnCheckTime = Time.unscaledTime + 1f;
            _combatSession.TryRespawnEnemy(spawnId);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = definition != null ? definition.grayboxColor : Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.65f);
        }
    }
}
