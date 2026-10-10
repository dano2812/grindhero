using System;
using GrindHero.Domain.Combat;
using GrindHero.Domain.Progression;

namespace GrindHero.Application.Progression
{
    public sealed class ProgressionSession
    {
        private readonly IPlayerProgressRepository _repository;

        public ProgressionSession(
            PlayerProgressData data,
            IPlayerProgressRepository repository)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public PlayerProgressData Data { get; }

        public bool GrantReward(StatReward reward)
        {
            if (reward.Points == 0 || reward.Stat == RewardStat.None)
            {
                return false;
            }

            switch (reward.Stat)
            {
                case RewardStat.Health:
                    Data.healthPoints = ProgressionRules.AddPoints(
                        Data.healthPoints,
                        reward.Points);
                    return true;
                case RewardStat.AxeAttack:
                    Data.axeAttackPoints = ProgressionRules.AddPoints(
                        Data.axeAttackPoints,
                        reward.Points);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reward));
            }
        }

        public bool CompleteFirstBoss()
        {
            if (Data.firstBossDefeated)
            {
                return false;
            }

            Data.firstBossDefeated = true;
            return true;
        }

        public void Save()
        {
            _repository.Save(Data);
        }
    }
}
