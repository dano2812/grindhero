using System;

namespace GrindHero.Domain.World
{
    public static class MobRespawnRules
    {
        public static MobRespawnState Schedule(
            MobRespawnState state,
            long nowUnixSeconds,
            int cooldownSeconds,
            long utcDayKey)
        {
            ValidateDayKey(utcDayKey);
            if (nowUnixSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nowUnixSeconds));
            }

            if (cooldownSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownSeconds));
            }

            state = ResetForDay(state, utcDayKey);
            return new MobRespawnState(
                checked(nowUnixSeconds + cooldownSeconds),
                state.RespawnsToday,
                utcDayKey,
                false);
        }

        public static MobRespawnState MarkSpawned(
            MobRespawnState state,
            long utcDayKey)
        {
            ValidateDayKey(utcDayKey);
            state = ResetForDay(state, utcDayKey);
            return new MobRespawnState(0, state.RespawnsToday, utcDayKey, true);
        }

        public static MobRespawnState MarkDefeated(
            MobRespawnState state,
            long utcDayKey)
        {
            ValidateDayKey(utcDayKey);
            state = ResetForDay(state, utcDayKey);
            return new MobRespawnState(0, state.RespawnsToday, utcDayKey, false);
        }

        public static bool TryConsumeRespawn(
            MobRespawnState state,
            long nowUnixSeconds,
            int maximumRespawnsPerDay,
            long utcDayKey,
            out MobRespawnState updatedState)
        {
            ValidateDayKey(utcDayKey);
            if (nowUnixSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nowUnixSeconds));
            }

            if (maximumRespawnsPerDay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumRespawnsPerDay));
            }

            updatedState = ResetForDay(state, utcDayKey);
            if (updatedState.IsAlive ||
                updatedState.NextRespawnUnixSeconds <= 0 ||
                nowUnixSeconds < updatedState.NextRespawnUnixSeconds ||
                updatedState.RespawnsToday >= maximumRespawnsPerDay)
            {
                return false;
            }

            updatedState = new MobRespawnState(
                0,
                checked(updatedState.RespawnsToday + 1),
                utcDayKey,
                true);
            return true;
        }

        private static MobRespawnState ResetForDay(
            MobRespawnState state,
            long utcDayKey)
        {
            if (state.RespawnsToday < 0 ||
                state.NextRespawnUnixSeconds < 0 ||
                state.UtcDayKey < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(state));
            }

            if (state.UtcDayKey == utcDayKey)
            {
                return state;
            }

            return new MobRespawnState(
                state.NextRespawnUnixSeconds,
                0,
                utcDayKey,
                state.IsAlive);
        }

        private static void ValidateDayKey(long utcDayKey)
        {
            if (utcDayKey <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(utcDayKey));
            }
        }
    }
}
