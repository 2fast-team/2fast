using Project2FA.Repository.Models;
using System.Threading.Tasks;

namespace Project2FA.Repository.Database
{
    public interface IPasswordHashRepository
    {
        /// <summary>
        /// Returns the password hash model. 
        /// </summary>
        Task<DBPasswordHashModel> GetAsync();

        /// <summary>
        /// Adds a new password hash. If the password hash does not exist, updates the 
        /// existing password hash otherwise.
        /// </summary>
        Task<DBPasswordHashModel> UpsertAsync(DBPasswordHashModel pwhash);

        /// <summary>
        /// Deletes a password hash.
        /// </summary>
        Task DeleteAsync();
    }
}
