using System;

namespace GrindHero.Application.Progression
{
    [Serializable]
    public sealed class PlayerProgressData
    {
        public int schemaVersion = 1;
        public int healthPoints;
        public int axeAttackPoints;
        public float currentHealth;
        public float heroPositionX;
        public float heroPositionY;
        public float heroPositionZ;
        public bool firstBossDefeated;
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
