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

        public bool AcceptQuest(string questId)
        {
            ValidateId(questId);
            if (Array.IndexOf(Data.acceptedQuestIds, questId) >= 0)
            {
                return false;
            }

            Data.acceptedQuestIds = Append(Data.acceptedQuestIds, questId);
            return true;
        }

        public bool ClaimQuestReward(string questId, int healthPoints, int attackPoints)
        {
            ValidateId(questId);
            ValidateReward(healthPoints);
            ValidateReward(attackPoints);
            if (!Data.firstBossDefeated ||
                Array.IndexOf(Data.acceptedQuestIds, questId) < 0 ||
                Array.IndexOf(Data.completedQuestIds, questId) >= 0)
            {
                return false;
            }

            var nextHealth = ProgressionRules.AddPoints(Data.healthPoints, healthPoints);
            var nextAttack = ProgressionRules.AddPoints(Data.axeAttackPoints, attackPoints);
            var completed = Append(Data.completedQuestIds, questId);
            Data.healthPoints = nextHealth;
            Data.axeAttackPoints = nextAttack;
            Data.completedQuestIds = completed;
            return true;
        }

        public bool LootTreasure(string treasureId, int healthPoints)
        {
            ValidateId(treasureId);
            ValidateReward(healthPoints);
            if (Array.IndexOf(Data.lootedTreasureIds, treasureId) >= 0)
            {
                return false;
            }

            var nextHealth = ProgressionRules.AddPoints(Data.healthPoints, healthPoints);
            var looted = Append(Data.lootedTreasureIds, treasureId);
            Data.healthPoints = nextHealth;
            Data.lootedTreasureIds = looted;
            return true;
        }

        public bool CompleteSection(string sectionId)
        {
            ValidateId(sectionId);
            if (!Data.firstBossDefeated || Array.IndexOf(Data.completedSectionIds, sectionId) >= 0)
            {
                return false;
            }

            Data.completedSectionIds = Append(Data.completedSectionIds, sectionId);
            return true;
        }

        private static string[] Append(string[] source, string id)
        {
            var updated = new string[source.Length + 1];
            Array.Copy(source, updated, source.Length);
            updated[source.Length] = id;
            return updated;
        }

        private static void ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A stable content ID is required.", nameof(id));
            }
        }

        private static void ValidateReward(int points)
        {
            if (points < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(points));
            }
        }

        public void Save()
        {
            _repository.Save(Data);
        }
    }
}
