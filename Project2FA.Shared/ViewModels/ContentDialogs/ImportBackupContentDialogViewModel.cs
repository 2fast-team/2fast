
using CommunityToolkit.Mvvm.Input;
using Project2FA.Core.Utils;
using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Importer;
using Project2FA.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Dialogs;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
    public class ImportBackupContentDialogViewModel : AddAccountViewModelBase, IDialogInitialize
    {
        private IBackupImporterService BackupImporterService { get; }
        private IDialogService DialogService { get; }
        private int _selectedPivotIndex = 0;
        private StorageFile _importStorageFile;
        private bool _isLoading = false;
        private bool _isCheckedInputs = false;
        public ICommand FileImportCommand { get; }
        private string _password = string.Empty;
        private string _lastPivotItemName = string.Empty;
        private bool _passwordGivenChecked = true;
        private int _selectedImportFormatIndex = -1;
        public ICommand ImportAccountCommand { get; }
        public ObservableCollection<BackupServiceEnum> BackupServiceEnumList { get; } = new ObservableCollection<BackupServiceEnum>();


        public ImportBackupContentDialogViewModel(IBackupImporterService backupImporterService, IDialogService dialogService)
        {
            BackupImporterService = backupImporterService;
            DialogService = dialogService;
            FileImportCommand = new AsyncRelayCommand(FileImportCommandTask);
            ImportAccountCommand = new AsyncRelayCommand(ImportAccountCommandTask);
        }

        private async Task ImportAccountCommandTask()
        {
            var accountsToImport = ImportCollection.Where(x => x.IsChecked);
            var result = await DataService.Instance.AddAccountsToCollection(accountsToImport.ToList());
            if (result)
            {
                await DataService.Instance.WriteLocalDatafile();
            }
        }

        public async Task<bool> ConfirmImportTask()
        {
            List<TwoFACodeModel> accountList = null;
            bool successful = false;
            Exception exc = null;

            (accountList, successful, exc) = await BackupImporterService.ImportBackup(ImportStorageFile, Password, (BackupServiceEnum)SelectedImportFormatIndex);

            if (successful)
            {
                ImportCollection.AddRange(accountList, true);
                return true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog(Strings.Resources.ImportBackupPasswordError, Strings.Resources.Error);
                await dialog.ShowAsync();
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

            IsLoading = true;
            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                ImportStorageFile = file;
            }
            IsLoading = false;
            return ImportStorageFile;
        }

        public void Initialize(IDialogParameters parameters)
        {
            this.EntryEnum = Repository.Models.Enums.AccountEntryEnum.Import;
            var importList = Enum.GetValues(typeof(BackupServiceEnum)).Cast<BackupServiceEnum>();
            BackupServiceEnumList.AddRange(importList.Where(x => x != BackupServiceEnum.None));
        }

        public void CheckInputs()
        {
            if (string.IsNullOrEmpty(Password) && PasswordGivenChecked)
            {
                IsCheckedInputs = false;
                return;
            }
            if (ImportStorageFile == null || SelectedImportFormatIndex == -1)
            {
                IsCheckedInputs = false;
                return;
            }
            IsCheckedInputs = true;
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
            set
            {
                if(SetProperty(ref _password, value))
                {
                    CheckInputs();
                }
            }
        }
        public bool IsCheckedInputs
        { 
            get => _isCheckedInputs; 
            set => SetProperty(ref _isCheckedInputs, value);
        }
        public string LastPivotItemName 
        { 
            get => _lastPivotItemName;
            set
            {
                if(SetProperty(ref _lastPivotItemName, value))
                {
                    CheckInputs();
                }
            }
        }
        public bool PasswordGivenChecked 
        { 
            get => _passwordGivenChecked;
            set
            {
                if(SetProperty(ref _passwordGivenChecked, value))
                {
                    CheckInputs();
                }
            }
        }

        public int SelectedImportFormatIndex 
        { 
            get => _selectedImportFormatIndex;
            set
            {
                if(SetProperty(ref _selectedImportFormatIndex, value))
                {
                    CheckInputs();
                }
            }
        }
    }


}
