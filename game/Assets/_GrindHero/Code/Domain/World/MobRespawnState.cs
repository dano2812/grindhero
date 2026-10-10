namespace GrindHero.Domain.World
{
    public readonly struct MobRespawnState
    {
        public MobRespawnState(
            long nextRespawnUnixSeconds,
            int respawnsToday,
            long utcDayKey,
            bool isAlive = false)
        {
            NextRespawnUnixSeconds = nextRespawnUnixSeconds;
            RespawnsToday = respawnsToday;
            UtcDayKey = utcDayKey;
            IsAlive = isAlive;
        }

        public long NextRespawnUnixSeconds { get; }
        public int RespawnsToday { get; }
        public long UtcDayKey { get; }
        public bool IsAlive { get; }
    }
}
