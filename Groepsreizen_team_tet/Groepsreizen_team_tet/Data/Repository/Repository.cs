using Microsoft.EntityFrameworkCore.Query;

namespace Groepsreizen_team_tet.Data.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly GroepsreizenContext _context;
        private readonly ILogger<Repository<TEntity>> _logger;

        public Repository(GroepsreizenContext context, ILogger<Repository<TEntity>> logger)
        {
            _context = context;
            _logger = logger;

        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }



        // Nieuwe methode voor het ophalen van een entiteit inclusief gerelateerde data
        public async Task<TEntity?> GetByIdWithIncludeAsync(int id, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }


        // Nieuwe methode voor het ophalen van alle entiteiten inclusief gerelateerde data
        public async Task<IEnumerable<TEntity>> GetAllWithIncludeAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{typeof(TEntity).Name} toegevoegd: {entity}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fout bij toevoegen van {typeof(TEntity).Name}: {ex.Message}");
                throw;
            }
        }


        public IQueryable<TEntity> Search()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        public void Create(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

      

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }
    }
}
