namespace TKMelo.Persistance.UnitOfWork
{
    public class UnitOfWork
    {
        private readonly Data.TKMeloDbContext _db;
        public UnitOfWork(Data.TKMeloDbContext db) => _db = db;
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
