﻿
using CommunityToolkit.Mvvm.Input;
using Project2FA.Core.Utils;
using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Importer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private bool _isCheckedInputs = true;
        public ICommand FileImportCommand { get; }
        private string _password = string.Empty;
        private string _lastPivotItemName = string.Empty;
        private bool _passwordGivenChecked = true;
        public ICommand ImportAccountCommand { get; }

        public ObservableCollection<TwoFACodeModel> ImportCollection { get; } = new ObservableCollection<TwoFACodeModel>();

        public ImportAccountContentDialogViewModel(IBackupImporterService backupImporterService)
        {
            BackupImporterService = backupImporterService;
            FileImportCommand = new AsyncRelayCommand(FileImportCommandTask);
            ImportAccountCommand = new RelayCommand(ImportAccountsToCollection);
        }

        private void ImportAccountsToCollection()
        {
            var accountsToImport = ImportCollection.Where(x => x.IsChecked);
            DataService.Instance.Collection.AddRange(accountsToImport);
        }

        public async Task<bool> ConfirmImportTask()
        {
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/aegis.encrypted.json"));
            (List<TwoFACodeModel> accountList, bool successful)  = await BackupImporterService.ImportAegisBackup(ImportStorageFile, Password);
            if (successful)
            {
                ImportCollection.AddRange(accountList, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<StorageFile> FileImportCommandTask()
        {
            FileOpenPicker filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            filePicker.FileTypeFilter.Add(".json");

            // TODO : Set file types for Android and iOS
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
            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                ImportStorageFile = file;
            }
            IsLoading = false;
            return ImportStorageFile;
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
        public string Password 
        { 
            get => _password; 
            set => SetProperty(ref _password, value);
        }
        public bool IsCheckedInputs
        { 
            get => _isCheckedInputs; 
            set => SetProperty(ref _isCheckedInputs, value);
        }
        public string LastPivotItemName 
        { 
            get => _lastPivotItemName; 
            set => SetProperty(ref _lastPivotItemName, value);
        }
        public bool PasswordGivenChecked 
        { 
            get => _passwordGivenChecked; 
            set => SetProperty(ref _passwordGivenChecked, value);
        }
    }


}
