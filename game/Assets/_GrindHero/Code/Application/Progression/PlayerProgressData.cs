using System;

namespace GrindHero.Application.Progression
{
    [Serializable]
    public sealed class PlayerProgressData
    {
        public int schemaVersion = 2;
        public int healthPoints;
        public int axeAttackPoints;
        public float currentHealth;
        public float heroPositionX;
        public float heroPositionY;
        public float heroPositionZ;
        public bool firstBossDefeated;
        public string[] acceptedQuestIds = Array.Empty<string>();
        public string[] completedQuestIds = Array.Empty<string>();
        public string[] lootedTreasureIds = Array.Empty<string>();
        public string[] completedSectionIds = Array.Empty<string>();
        public EnemyRespawnData[] enemyRespawns = Array.Empty<EnemyRespawnData>();
    }

    [Serializable]
    public sealed class EnemyRespawnData
    {
        public string spawnId;
        public long nextRespawnUnixSeconds;
        public int respawnsToday;
        public long utcDayKey;
        public bool isAlive;
    }
}
