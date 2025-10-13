using Microsoft.EntityFrameworkCore;

namespace TKMelo.Persistance.Repositories
{
    public class Repository<T> where T : class
    {
        protected readonly Data.TKMeloDbContext _db;
        protected DbSet<T> Set => _db.Set<T>();

        public Repository(Data.TKMeloDbContext db) => _db = db;

        public Task<T?> GetAsync(Guid id, CancellationToken ct = default) => Set.FindAsync([id], ct).AsTask();
        public async Task<List<T>> ListAsync(CancellationToken ct = default) => await Set.AsNoTracking().ToListAsync(ct);
        public async Task AddAsync(T entity, CancellationToken ct = default) => await Set.AddAsync(entity, ct);
        public void Update(T entity) => Set.Update(entity);
        public void Remove(T entity) => Set.Remove(entity);
        public Task<int> SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
