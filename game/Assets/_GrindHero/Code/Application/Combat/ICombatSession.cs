using GrindHero.Domain.Combat;

namespace GrindHero.Application.Combat
{
    public interface ICombatSession
    {
        float CurrentHealth { get; }
        float MaximumHealth { get; }
        float AxeDamage { get; }
        int AxeAttackPoints { get; }
        float HeadAxeArmor { get; }
        float TorsoAxeArmor { get; }
        float DodgeChance { get; }
        float BlockChance { get; }
        float BlockDamageMultiplier { get; }
        bool BossDefeated { get; }

        void ReceiveHeroDamage(float rawDamage, DamageType damageType);
        void RecordHeroPosition(float x, float y, float z);
        void HandleEnemyDefeated(EnemyDefeatRecord defeat);
        void TryRespawnEnemy(string spawnId);
    }
}
