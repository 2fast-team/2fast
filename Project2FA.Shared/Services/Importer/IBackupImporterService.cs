using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Project2FA.Services.Importer
{
    public interface IBackupImporterService
    {
        Task ImportAegisBackup(StorageFile storageFile, string password);
    }
}
