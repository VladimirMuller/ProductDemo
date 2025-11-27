using Microsoft.EntityFrameworkCore;

namespace ProductDemo.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        ValueTask<T?> GetAsync(int id);
        Task AddAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(int id);
        Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);
    }

    public class RepositortyBase<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext dbContext;
        protected readonly DbSet<T> dbSet;
        public RepositortyBase(ApplicationDbContext context)
        {
            this.dbContext = context ?? throw new ArgumentNullException(nameof(context));
            this.dbSet = context.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet.ToListAsync(cancellationToken);
        }
        public async ValueTask<T?> GetAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }
        public async Task AddAsync(T item)
        {
            await dbSet.AddAsync(item);
            await dbContext.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            T? item = await GetAsync(id);
            if (item is not null)
            {
                dbSet.Remove(item);
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task UpdateAsync(T item)
        {
            dbSet.Update(item);
            await dbContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(dbSet.Where(predicate));
        }
    }

}
