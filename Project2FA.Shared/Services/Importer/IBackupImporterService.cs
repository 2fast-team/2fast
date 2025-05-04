using Project2FA.Repository.Models;
using Project2FA.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Project2FA.Services.Importer
{
    public interface IBackupImporterService
    {
        Task<(List<TwoFACodeModel> accountList, bool successful, Exception exc)> ImportBackup(StorageFile storageFile, string password, BackupServiceEnum backupServiceEnum);
    }
}
