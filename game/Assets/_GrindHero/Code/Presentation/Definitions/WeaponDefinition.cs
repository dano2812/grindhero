using GrindHero.Domain.Combat;
using UnityEngine;

namespace GrindHero.Presentation.Definitions
{
    [CreateAssetMenu(menuName = "GrindHero/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string stableId = "weapon.axe.basic";
        public DamageType damageType = DamageType.Axe;
        [Min(0f)] public float weaponDamage = 5f;
        [Min(0.1f)] public float attackCycleSeconds = 1f;
        [Min(0f)] public float windupSeconds = 0.25f;
        [Min(0.1f)] public float attackRange = 2.5f;

        public string ValidateDefinition()
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                return "Weapon stable ID is required.";
            }

            if (weaponDamage < 0f ||
                float.IsNaN(weaponDamage) ||
                float.IsInfinity(weaponDamage))
            {
                return $"Weapon '{stableId}' needs finite non-negative damage.";
            }

            if (attackCycleSeconds <= 0f ||
                float.IsNaN(attackCycleSeconds) ||
                float.IsInfinity(attackCycleSeconds))
            {
                return $"Weapon '{stableId}' needs a finite positive attack cycle.";
            }

            if (windupSeconds < 0f ||
                windupSeconds > attackCycleSeconds ||
                float.IsNaN(windupSeconds) ||
                float.IsInfinity(windupSeconds))
            {
                return $"Weapon '{stableId}' needs a wind-up between zero and its attack cycle.";
            }

            if (attackRange <= 0f || float.IsNaN(attackRange) || float.IsInfinity(attackRange))
            {
                return $"Weapon '{stableId}' needs a finite positive attack range.";
            }

            return null;
        }
    }
}
