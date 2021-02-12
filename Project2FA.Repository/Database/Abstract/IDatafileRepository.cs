using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.Repository.Database
{
    public interface IDatafileRepository
    {
        /// <summary>
        /// Returns the datafile model. 
        /// </summary>
        Task<DBDatafileModel> GetAsync();

        /// <summary>
        /// Adds a new datafile. If the datafile does not exist, updates the 
        /// existing datafile otherwise.
        /// </summary>
        Task<DBDatafileModel> UpsertAsync(DBDatafileModel dataFile);

        /// <summary>
        /// Deletes a datafile.
        /// </summary>
        Task DeleteAsync();
    }
}
