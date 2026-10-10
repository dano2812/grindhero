namespace GrindHero.Domain.Combat
{
    public enum DamageType
    {
        Axe
    }

    public enum EnemyArchetype
    {
        BasicHealth,
        BasicAxeAttack,
        Advanced,
        Boss
    }

    public enum RewardStat
    {
        None,
        Health,
        AxeAttack
    }

    public readonly struct StatReward
    {
        public StatReward(RewardStat stat, int points)
        {
            if (points < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(points));
            }

            Stat = stat;
            Points = points;
        }

        public RewardStat Stat { get; }
        public int Points { get; }
    }
}
