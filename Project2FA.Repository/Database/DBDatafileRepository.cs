using Microsoft.EntityFrameworkCore;
using Project2FA.Repository.Models;
using System.Threading.Tasks;

namespace Project2FA.Repository.Database
{
    public class DBDatafileRepository : IDatafileRepository
    {
        private readonly Project2FAContext _db;

        public DBDatafileRepository(Project2FAContext db)
        {
            _db = db;
        }
        public async Task DeleteAsync()
        {
            await _db.Datafile.FirstOrDefaultAsync();
        }

        public async Task<DBDatafileModel> GetAsync()
        {
            return await _db.Datafile.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<DBDatafileModel> UpsertAsync(DBDatafileModel dataFile)
        {
            var existing = await _db.Datafile.FirstOrDefaultAsync();
            if (null == existing)
            {
                _db.ChangeTracker.TrackGraph(dataFile, node =>
                    node.Entry.State = !node.Entry.IsKeySet ? EntityState.Added : EntityState.Unchanged);
                _db.Datafile.Add(dataFile);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(dataFile);
            }
            await _db.SaveChangesAsync();

            return dataFile;
        }
    }
}
