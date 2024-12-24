
using Project2FA.Services.Importer;
using System;
using System.Threading.Tasks;
using UNOversal.Services.Dialogs;
using Windows.Storage;

namespace Project2FA.ViewModels
{
    public class ImportAccountContentDialogViewModel : AddAccountViewModelBase, IDialogInitializeAsync
    {
    
        private IBackupImporterService BackupImporterService { get; }
        public ImportAccountContentDialogViewModel(IBackupImporterService backupImporterService)
        {
            BackupImporterService = backupImporterService;
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/aegis.encrypted.json"));
            await BackupImporterService.ImportAegisBackup(file, "test");
        }
    }


}
