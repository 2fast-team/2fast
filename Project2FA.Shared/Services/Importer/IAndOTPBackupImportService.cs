using Project2FA.Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project2FA.Services.Importer
{
    public interface IAndOTPBackupImportService
    {
        public Task<(List<TwoFACodeModel> accountList, bool successful)> ImportBackup(string content, byte[] bytePassword);
    }
}
