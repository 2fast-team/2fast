using Microsoft.EntityFrameworkCore;
using Project2FA.Repository.Models;

namespace Project2FA.Repository.Database
{
    public class Project2FAContext : DbContext
    {
        public DbSet<DBPasswordHashModel> Password { get; set; }

        public DbSet<DBDatafileModel> Datafile { get; set; }

        /// <summary>
        /// Creates a new Project2FA DbContext.
        /// </summary>
        public Project2FAContext(DbContextOptions<Project2FAContext> options) : base(options)
        {

        }
    }
}
