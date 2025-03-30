
using CommunityToolkit.Mvvm.Input;
using Project2FA.Services.Importer;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Dialogs;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Project2FA.ViewModels
{
    public class ImportAccountContentDialogViewModel : AddAccountViewModelBase, IDialogInitializeAsync
    {
        private IBackupImporterService BackupImporterService { get; }
        private int _selectedPivotIndex = 0;
        private StorageFile _importStorageFile;
        private bool _isLoading = false;
        public ICommand FileImportCommand { get; }


        public ImportAccountContentDialogViewModel(IBackupImporterService backupImporterService)
        {
            BackupImporterService = backupImporterService;
            FileImportCommand = new AsyncRelayCommand(FileImportCommandTask);
        }

        private async Task FileImportCommandTask()
        {
            FileOpenPicker filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            filePicker.FileTypeFilter.Add(".json");
#if __IOS__
            // mapping the filter type to definied UTType
            //Uno.WinRTFeatureConfiguration.FileTypes.FileTypeToUTTypeMapping.Add(".2fa", "com.jpwtechnology.2fa");
#endif

#if ANDROID
            //var androidFlags = Android.Content.ActivityFlags.GrantReadUriPermission |
            //    Android.Content.ActivityFlags.GrantWriteUriPermission |
            //    Android.Content.ActivityFlags.GrantPersistableUriPermission |
            //    Android.Content.ActivityFlags.GrantPrefixUriPermission;
            //// set the persistent access to the file
            //FilePickerHelper.RegisterOnBeforeStartActivity(filePicker, (intent) =>
            //{
            //    intent.AddFlags(androidFlags);
            //});
#endif

            IsLoading = true;
            ImportStorageFile = await filePicker.PickSingleFileAsync();
            if (ImportStorageFile != null)
            {
                SelectedPivotIndex = 1;
            }
            
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/aegis.encrypted.json"));
            //await BackupImporterService.ImportAegisBackup(file, "test");
        }

        public new int SelectedPivotIndex
        { 
            get => _selectedPivotIndex; 
            set => SetProperty(ref _selectedPivotIndex, value);
        }

        public StorageFile ImportStorageFile
        {
            get => _importStorageFile;
            set => SetProperty(ref _importStorageFile, value);
        }
        public bool IsLoading 
        { 
            get => _isLoading; 
            set => SetProperty(ref _isLoading, value);
        }
    }


}
