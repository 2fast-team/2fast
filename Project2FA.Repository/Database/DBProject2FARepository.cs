using Microsoft.EntityFrameworkCore;

namespace Project2FA.Repository.Database
{
    public class DBProject2FARepository : IProject2FARepository
    {
        private readonly DbContextOptions<Project2FAContext> _dbOptions;

        public DBProject2FARepository(DbContextOptionsBuilder<Project2FAContext>
            dbOptionsBuilder)
        {
            _dbOptions = dbOptionsBuilder.Options;
            using (var db = new Project2FAContext(_dbOptions))
            {
                db.Database.EnsureCreated();
            }
        }

        public IDatafileRepository Datafile => new DBDatafileRepository(
            new Project2FAContext(_dbOptions));

        public IPasswordHashRepository Password => new DBPasswordHashRepository(
            new Project2FAContext(_dbOptions));
    }
}
