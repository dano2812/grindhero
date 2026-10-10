using System;
using GrindHero.Domain.Combat;
using GrindHero.Domain.Progression;
using GrindHero.Domain.World;
using NUnit.Framework;

namespace GrindHero.Tests.Domain
{
    public sealed class CombatRuleTests
    {
        [Test]
        public void DamageResolver_SubtractsMatchingArmorAndClampsAtZero()
        {
            var result = DamageResolver.Resolve(100d, 5d, 15d, false, false, 0.8d);
            var fullyArmoredResult = DamageResolver.Resolve(10d, 20d, 3d, false, false, 0.8d);

            Assert.That(result.FinalDamage, Is.EqualTo(80d));
            Assert.That(fullyArmoredResult.FinalDamage, Is.Zero);
        }

        [Test]
        public void DamageResolver_AppliesBlockAfterArmor()
        {
            var result = DamageResolver.Resolve(100d, 5d, 15d, false, true, 0.8d);

            Assert.That(result.DamageAfterArmor, Is.EqualTo(80d));
            Assert.That(result.FinalDamage, Is.EqualTo(64d));
            Assert.That(result.Blocked, Is.True);
        }

        [Test]
        public void DamageResolver_DodgeAvoidsDamageAndSkipsBlock()
        {
            var result = DamageResolver.Resolve(100d, 5d, 15d, true, true, 0.8d);

            Assert.That(result.FinalDamage, Is.Zero);
            Assert.That(result.Dodged, Is.True);
            Assert.That(result.Blocked, Is.False);
        }

        [Test]
        public void DamageResolver_RejectsInvalidDamageValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => DamageResolver.Resolve(double.NaN, 0d, 0d, false, false, 0.8d));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => DamageResolver.Resolve(1d, 0d, 0d, false, false, 1.1d));
        }

        [Test]
        public void ProgressionRules_HealthAndAxeAttackScaleWithPermanentPoints()
        {
            var maximumHealth = ProgressionRules.CalculateMaximumHealth(100, 3, 5);
            var axeDamage = ProgressionRules.CalculateAxeDamage(15d, 5d, 2, 1d);

            Assert.That(maximumHealth, Is.EqualTo(115));
            Assert.That(axeDamage, Is.EqualTo(22d));
        }

        [Test]
        public void MobRespawnRules_WaitsForCooldownAndCountsOnlyActualRespawns()
        {
            const long day = 638700000000000000;
            var alive = MobRespawnRules.MarkSpawned(new MobRespawnState(0, 0, day), day);
            var defeated = MobRespawnRules.Schedule(alive, 1000, 120, day);

            var beforeCooldown = MobRespawnRules.TryConsumeRespawn(
                defeated,
                1119,
                3,
                day,
                out var waitingState);
            var cooldownComplete = MobRespawnRules.TryConsumeRespawn(
                defeated,
                1120,
                3,
                day,
                out var respawnedState);

            Assert.That(beforeCooldown, Is.False);
            Assert.That(waitingState.RespawnsToday, Is.Zero);
            Assert.That(cooldownComplete, Is.True);
            Assert.That(respawnedState.RespawnsToday, Is.EqualTo(1));
            Assert.That(respawnedState.IsAlive, Is.True);
        }

        [Test]
        public void MobRespawnRules_DailyCapResetsAtTheNextUtcDay()
        {
            const long firstDay = 638700000000000000;
            const long nextDay = firstDay + TimeSpan.TicksPerDay;
            var state = MobRespawnRules.MarkSpawned(
                new MobRespawnState(0, 0, firstDay),
                firstDay);

            for (var i = 0; i < 3; i++)
            {
                state = MobRespawnRules.Schedule(state, 1000 + i, 0, firstDay);
                Assert.That(
                    MobRespawnRules.TryConsumeRespawn(state, 1000 + i, 3, firstDay, out state),
                    Is.True);
            }

            state = MobRespawnRules.Schedule(state, 2000, 0, firstDay);
            Assert.That(
                MobRespawnRules.TryConsumeRespawn(state, 2000, 3, firstDay, out _),
                Is.False);

            var nextDayRespawn = MobRespawnRules.TryConsumeRespawn(
                state,
                2000,
                3,
                nextDay,
                out var updated);

            Assert.That(nextDayRespawn, Is.True);
            Assert.That(updated.RespawnsToday, Is.EqualTo(1));
            Assert.That(updated.UtcDayKey, Is.EqualTo(nextDay));
        }
    }
}
