namespace GrindHero.Domain.Combat
{
    public readonly struct DamageResult
    {
        public DamageResult(
            double rawDamage,
            double matchingArmor,
            double damageAfterArmor,
            double finalDamage,
            bool dodged,
            bool blocked)
        {
            RawDamage = rawDamage;
            MatchingArmor = matchingArmor;
            DamageAfterArmor = damageAfterArmor;
            FinalDamage = finalDamage;
            Dodged = dodged;
            Blocked = blocked;
        }

        public double RawDamage { get; }
        public double MatchingArmor { get; }
        public double DamageAfterArmor { get; }
        public double FinalDamage { get; }
        public bool Dodged { get; }
        public bool Blocked { get; }
    }
}
