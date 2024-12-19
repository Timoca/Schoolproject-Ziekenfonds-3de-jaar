namespace Groepsreizen_team_tet.Data.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<TEntity?> GetByIdAsync(int id);

        Task AddAsync(TEntity entity);

        Task<TEntity?> GetByIdWithIncludeAsync(int id, params Expression<Func<TEntity, object>>[] includeProperties);
        //Task<TEntity?> GetByIdWithIncludeAsync(int id, Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null!);


        Task<IEnumerable<TEntity>> GetAllWithIncludeAsync(params Expression<Func<TEntity, object>>[] includeProperties);
        IQueryable<TEntity> Search();

        void Create(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
