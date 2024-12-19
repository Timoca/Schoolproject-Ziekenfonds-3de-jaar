namespace Groepsreizen_team_tet.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GroepsreizenContext _context;
        private readonly ILoggerFactory _loggerFactory;



        public UnitOfWork(GroepsreizenContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _loggerFactory = loggerFactory;

            GroepsreisRepository = new Repository<Groepsreis>(_context, _loggerFactory.CreateLogger<Repository<Groepsreis>>());
            BestemmingRepository = new Repository<Bestemming>(_context, _loggerFactory.CreateLogger<Repository<Bestemming>>());
            ActiviteitRepository = new Repository<Activiteit>(_context, _loggerFactory.CreateLogger<Repository<Activiteit>>());
            FotoRepository = new Repository<Foto>(_context, _loggerFactory.CreateLogger<Repository<Foto>>());
            DeelnemerRepository = new Repository<Deelnemer>(_context, _loggerFactory.CreateLogger<Repository<Deelnemer>>());
            KindRepository = new Repository<Kind>(_context, _loggerFactory.CreateLogger<Repository<Kind>>());
            MonitorRepository = new Repository<Models.Monitor>(_context, _loggerFactory.CreateLogger<Repository<Models.Monitor>>());
            OpleidingRepository = new Repository<Opleiding>(_context, _loggerFactory.CreateLogger<Repository<Opleiding>>());
            OnkostenRepository = new Repository<Onkosten>(_context, _loggerFactory.CreateLogger<Repository<Onkosten>>());
            WachtlijstRepository = new Repository<Wachtlijst>(_context, _loggerFactory.CreateLogger<Repository<Wachtlijst>>());
        }


        //IRepositories hier plaatsen
        public IRepository<Groepsreis> GroepsreisRepository { get; }
        public IRepository<Bestemming> BestemmingRepository { get; }
        public IRepository<Activiteit> ActiviteitRepository { get; }
        public IRepository<Foto> FotoRepository { get; }
        public IRepository<Deelnemer> DeelnemerRepository { get; }
        public IRepository<Kind> KindRepository { get; }
        public IRepository<Models.Monitor> MonitorRepository { get; }
        public IRepository<Opleiding> OpleidingRepository { get; }
        public IRepository<Onkosten> OnkostenRepository { get; }
        public IRepository<Wachtlijst> WachtlijstRepository { get; }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
