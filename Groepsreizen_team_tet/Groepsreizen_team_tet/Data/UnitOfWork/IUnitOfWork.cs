namespace Groepsreizen_team_tet.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        //IRepositories hier plaatsen
        IRepository<Groepsreis> GroepsreisRepository { get; }
        IRepository<Bestemming> BestemmingRepository { get; }
        IRepository<Activiteit> ActiviteitRepository { get; }
        IRepository<Foto> FotoRepository { get; }
        IRepository<Deelnemer> DeelnemerRepository { get; }
        IRepository<Kind> KindRepository { get; }
        IRepository<Models.Monitor> MonitorRepository { get; }
        IRepository<Opleiding> OpleidingRepository { get; }
        IRepository<Onkosten> OnkostenRepository { get; }
        IRepository<Wachtlijst> WachtlijstRepository { get; }

        public Task SaveAsync();
    }
}
