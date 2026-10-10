namespace GrindHero.Application.Progression
{
    public interface IPlayerProgressRepository
    {
        bool TryLoad(out PlayerProgressData data, out bool recoveredFromBackup);
        void Save(PlayerProgressData data);
    }
}
