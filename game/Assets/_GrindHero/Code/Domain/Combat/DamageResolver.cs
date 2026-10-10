using System;

namespace GrindHero.Domain.Combat
{
    public static class DamageResolver
    {
        public static DamageResult Resolve(
            double rawDamage,
            double headArmor,
            double torsoArmor,
            bool dodged,
            bool blocked,
            double blockMultiplier)
        {
            ValidateNonNegativeFinite(rawDamage, nameof(rawDamage));
            ValidateNonNegativeFinite(headArmor, nameof(headArmor));
            ValidateNonNegativeFinite(torsoArmor, nameof(torsoArmor));

            if (double.IsNaN(blockMultiplier) ||
                double.IsInfinity(blockMultiplier) ||
                blockMultiplier < 0d ||
                blockMultiplier > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(blockMultiplier));
            }

            var matchingArmor = headArmor + torsoArmor;
            if (dodged)
            {
                return new DamageResult(rawDamage, matchingArmor, 0d, 0d, true, false);
            }

            var damageAfterArmor = Math.Max(0d, rawDamage - matchingArmor);
            var finalDamage = blocked
                ? damageAfterArmor * blockMultiplier
                : damageAfterArmor;

            return new DamageResult(
                rawDamage,
                matchingArmor,
                damageAfterArmor,
                finalDamage,
                false,
                blocked);
        }

        private static void ValidateNonNegativeFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
