using System;

namespace GrindHero.Domain.Progression
{
    public static class ProgressionRules
    {
        public static int AddPoints(int currentPoints, int pointsToAdd)
        {
            if (currentPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentPoints));
            }

            if (pointsToAdd < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pointsToAdd));
            }

            return checked(currentPoints + pointsToAdd);
        }

        public static int CalculateMaximumHealth(
            int baseMaximumHealth,
            int healthPoints,
            int healthPerPoint)
        {
            if (baseMaximumHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseMaximumHealth));
            }

            if (healthPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(healthPoints));
            }

            if (healthPerPoint < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(healthPerPoint));
            }

            return checked(baseMaximumHealth + (healthPoints * healthPerPoint));
        }

        public static double CalculateAxeDamage(
            double baseAxeDamage,
            double weaponDamage,
            int axeAttackPoints,
            double damagePerPoint)
        {
            if (double.IsNaN(baseAxeDamage) ||
                double.IsInfinity(baseAxeDamage) ||
                baseAxeDamage < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAxeDamage));
            }

            if (double.IsNaN(weaponDamage) ||
                double.IsInfinity(weaponDamage) ||
                weaponDamage < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(weaponDamage));
            }

            if (axeAttackPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(axeAttackPoints));
            }

            if (double.IsNaN(damagePerPoint) ||
                double.IsInfinity(damagePerPoint) ||
                damagePerPoint < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(damagePerPoint));
            }

            return baseAxeDamage + weaponDamage + (axeAttackPoints * damagePerPoint);
        }
    }
}
