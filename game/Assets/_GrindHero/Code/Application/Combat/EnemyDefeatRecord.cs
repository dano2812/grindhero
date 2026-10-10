using GrindHero.Domain.Combat;

namespace GrindHero.Application.Combat
{
    public readonly struct EnemyDefeatRecord
    {
        public EnemyDefeatRecord(
            string spawnId,
            EnemyArchetype archetype,
            RewardStat rewardStat,
            int rewardPoints,
            bool canRespawn,
            int maxRespawnsPerDay,
            int respawnCooldownSeconds)
        {
            SpawnId = spawnId;
            Archetype = archetype;
            RewardStat = rewardStat;
            RewardPoints = rewardPoints;
            CanRespawn = canRespawn;
            MaxRespawnsPerDay = maxRespawnsPerDay;
            RespawnCooldownSeconds = respawnCooldownSeconds;
        }

        public string SpawnId { get; }
        public EnemyArchetype Archetype { get; }
        public RewardStat RewardStat { get; }
        public int RewardPoints { get; }
        public bool CanRespawn { get; }
        public int MaxRespawnsPerDay { get; }
        public int RespawnCooldownSeconds { get; }
    }
}
