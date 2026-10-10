using GrindHero.Domain.Combat;
using UnityEngine;

namespace GrindHero.Presentation.Definitions
{
    [CreateAssetMenu(menuName = "GrindHero/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
        public EnemyArchetype archetype;
        public DamageType attackDamageType = DamageType.Axe;
        [Min(1f)] public float maximumHealth = 80f;
        [Min(0f)] public float attackDamage = 10f;
        [Min(0.1f)] public float attackCycleSeconds = 1f;
        [Min(0f)] public float windupSeconds = 0.5f;
        [Min(0.1f)] public float attackRange = 1.8f;
        [Min(0.1f)] public float aggroRange = 6f;
        [Min(0f)] public float movementSpeed = 3.2f;
        [Min(0f)] public float chaseTimeoutSeconds = 5f;
        [Range(0f, 1f)] public float returnHealFractionPerSecond = 0.1f;
        [Tooltip("Prototype behavior for unresolved boss leash rules; turn off to make bosses chase indefinitely.")]
        public bool returnAndHeal = true;
        public bool canRespawn = true;
        [Min(0)] public int respawnCooldownSeconds = 120;
        [Min(0)] public int maximumRespawnsPerDay = 3;
        public RewardStat rewardStat;
        [Min(0)] public int rewardPoints;
        public Color grayboxColor = Color.red;

        public bool IsBoss => archetype == EnemyArchetype.Boss;

        public string ValidateDefinition()
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                return "Enemy stable ID is required.";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                return $"Enemy '{stableId}' needs a display name.";
            }

            if (!IsFinite(maximumHealth) || maximumHealth <= 0f)
            {
                return $"Enemy '{stableId}' needs finite positive health.";
            }

            if (!IsFinite(attackDamage) || attackDamage < 0f)
            {
                return $"Enemy '{stableId}' needs finite non-negative attack damage.";
            }

            if (!IsFinite(attackCycleSeconds) || attackCycleSeconds <= 0f)
            {
                return $"Enemy '{stableId}' needs a finite positive attack cycle.";
            }

            if (!IsFinite(windupSeconds) ||
                windupSeconds < 0f ||
                windupSeconds > attackCycleSeconds)
            {
                return $"Enemy '{stableId}' needs a wind-up between zero and its attack cycle.";
            }

            if (!IsFinite(attackRange) ||
                !IsFinite(aggroRange) ||
                !IsFinite(movementSpeed) ||
                attackRange <= 0f ||
                aggroRange < attackRange ||
                movementSpeed < 0f)
            {
                return $"Enemy '{stableId}' needs valid attack, aggro, and movement ranges.";
            }

            if (!IsFinite(chaseTimeoutSeconds) ||
                !IsFinite(returnHealFractionPerSecond) ||
                chaseTimeoutSeconds < 0f ||
                returnHealFractionPerSecond < 0f ||
                returnHealFractionPerSecond > 1f ||
                respawnCooldownSeconds < 0 ||
                maximumRespawnsPerDay < 0 ||
                rewardPoints < 0)
            {
                return $"Enemy '{stableId}' contains negative or out-of-range tuning.";
            }

            return null;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
