using Microsoft.EntityFrameworkCore;
using Project2FA.Repository.Models;
using System.Threading.Tasks;

namespace Project2FA.Repository.Database
{
    public class DBPasswordHashRepository : IPasswordHashRepository
    {
        private readonly Project2FAContext _db;

        public DBPasswordHashRepository(Project2FAContext db)
        {
            _db = db;
        }
        public async Task DeleteAsync()
        {
            _db.Password.Remove(await _db.Password.FirstOrDefaultAsync());
        }

        public async Task<DBPasswordHashModel> GetAsync()
        {
            return await _db.Password.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<DBPasswordHashModel> UpsertAsync(DBPasswordHashModel pwHash)
        {
            var existing = await _db.Password.FirstOrDefaultAsync();
            if (null == existing)
            {
                _db.ChangeTracker.TrackGraph(pwHash, node =>
                    node.Entry.State = !node.Entry.IsKeySet ? EntityState.Added : EntityState.Unchanged);
                _db.Password.Add(pwHash);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(pwHash);
            }
            await _db.SaveChangesAsync();
            return pwHash;
        }
    }
}
