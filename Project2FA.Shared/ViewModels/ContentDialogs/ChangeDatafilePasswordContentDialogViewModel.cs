using System.Threading.Tasks;
using System.Windows.Input;
using Project2FA.Core;
using Project2FA.Repository.Models;
using System;
using Windows.Storage;
using Project2FA.Core.Services.JSON;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;
using UNOversal.Services.File;
using Project2FA.Services;
using Project2FA.Core.Utils;
using Project2FA.Core.Services.Crypto;
using System.Text;
using UNOversal.Services.Logging;

#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
#else
using Project2FA.UnoApp;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    /// <summary>
    /// View model for the content dialog to change the password of the current datafile
    /// </summary>
    public class ChangeDatafilePasswordContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        private string _currentPassword, _newPassword, _newPasswordRepeat;
        private bool _isPrimaryBTNEnable;
        private bool _showError;
        private bool _invalidPassword = false;
        private bool _passwordChanged;
        private ISecretService SecretService { get; }

        private IFileService FileService { get; }

        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        private ILoggingService LoggingService { get; }
        public ICommand ConfirmErrorCommand { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChangeDatafilePasswordContentDialogViewModel(
            ISecretService secretService, 
            IFileService fileService, 
            INewtonsoftJSONService newtonsoftJSONService,
            ILoggingService loggingService)
        {
            SecretService = secretService;
            FileService = fileService;
            NewtonsoftJSONService = newtonsoftJSONService;
            LoggingService = loggingService;
            ConfirmErrorCommand = new RelayCommand(() =>
            {
                ShowError = false;
                CurrentPassword = string.Empty;
            });
        }

        public void Initialize(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<bool>("isInvalid", out bool isInvalid))
            {
                InvalidPassword = isInvalid;
            }
        }

        /// <summary>
        /// Changing the password in the datafile and local database
        /// </summary>
        public async Task ChangePasswordInFileAndDB()
        {
            DBPasswordHashModel passwordHashDB;
            string hash;
            if (DataService.Instance.ActivatedDatafile != null)
            {
                SecretService.Helper.RemoveSecret(Constants.ContainerName, Constants.ActivatedDatafileHashName);
                byte[] encryptedBytes;

                encryptedBytes = Encoding.UTF8.GetBytes(NewPassword);
#if WINDOWS_UWP
                SecretService.Helper.WriteSecret(
                    Constants.ContainerName,
                    Constants.ActivatedDatafileHashName,
                    NewtonsoftJSONService.Serialize(ProtectData.Protect(encryptedBytes)));
#else
                SecretService.Helper.WriteSecret(
                    Constants.ContainerName,
                    Constants.ActivatedDatafileHashName,
                    NewtonsoftJSONService.Serialize(encryptedBytes));
#endif
            }
            else
            {
                passwordHashDB = await App.Repository.Password.GetAsync();
                //delete password in the secret vault
                SecretService.Helper.RemoveSecret(Constants.ContainerName, passwordHashDB.Hash);
                // delete the hash in DB
                await App.Repository.Password.DeleteAsync();
                //set new hash
                passwordHashDB.Hash = CryptoService.CreateStringHash(NewPassword);
                // update db with new pw hash
                var model = await App.Repository.Password.UpsertAsync(passwordHashDB);
                hash = model.Hash;
                // save new pw in the secret vault
                SecretService.Helper.WriteSecret(Constants.ContainerName, hash, NewPassword);
            }

            //datafile must not changed when password was invalid (written already by other app)
            if (InvalidPassword == false)
            {
                await DataService.Instance.WriteLocalDatafile();
            }
            else
            {
                // reload collection
                await DataService.Instance.ReloadDatafile();
            }
            PasswordChanged = true;
        }

        /// <summary>
        /// Checks if the password is correct or not and displays an error message
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> TestPassword()
        {

            if (DataService.Instance.ActivatedDatafile != null)
            {
                StorageFolder storageFolder = await DataService.Instance.ActivatedDatafile.GetParentAsync();
                return await TestPassword(DataService.Instance.ActivatedDatafile, storageFolder);
            }
            else
            {
                DBDatafileModel dbDatafile = await App.Repository.Datafile.GetAsync();
                StorageFolder folder = dbDatafile.IsWebDAV ?
                                    ApplicationData.Current.LocalFolder :
                                    await StorageFolder.GetFolderFromPathAsync(dbDatafile.Path);
                StorageFile file = await folder.GetFileAsync(dbDatafile.Name);
                return await TestPassword(file, folder);
            }
        }

        public async Task<string> GetCurrentPasswordHash()
        {
            string hash;
            if (DataService.Instance.ActivatedDatafile != null)
            {
#if WINDOWS_UWP
                hash = CryptoService.CreateStringHash(ProtectData.Unprotect(NewtonsoftJSONService.Deserialize<byte[]>(SecretService.Helper.ReadSecret(Constants.ContainerName, Constants.ActivatedDatafileHashName))));
#else
                // TODO encrypted password via ProtectData is not supported
                hash = CryptoService.CreateStringHash(NewtonsoftJSONService.Deserialize<byte[]>(SecretService.Helper.ReadSecret(Constants.ContainerName, Constants.ActivatedDatafileHashName)));
#endif
            }
            else
            {
                hash = (await App.Repository.Password.GetAsync()).Hash;
            }
            return hash;
        }

        /// <summary>
        /// Test the new password, if the current password is invalid, or test the current password, if a new password will be set
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="storageFolder"></param>
        /// <returns></returns>
        private async Task<bool> TestPassword(StorageFile storageFile, StorageFolder storageFolder)
        {
            string datafileStr = await FileService.ReadStringAsync(storageFile.Name, storageFolder);
            //read the iv for AES
            DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
            byte[] iv = datafile.IV;

            try
            {
                // if the current password is invalid, try to load the datafile with the new password
                if (InvalidPassword)
                {
                    DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>(Encoding.UTF8.GetBytes(NewPassword), iv, datafileStr, datafile.Version);
                }
                else
                {
                    // check the current password, if the file can be decrypted
                    DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>(Encoding.UTF8.GetBytes(CurrentPassword), iv, datafileStr, datafile.Version);
                }

                return true;
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                ShowError = true;
                if (InvalidPassword)
                {
                    NewPassword = string.Empty;
                    NewPasswordRepeat = string.Empty;
                }
                else
                {
                    CurrentPassword = string.Empty;
                }
                return false;
            }
        }

        #region GetSetter
        public string CurrentPassword 
        { 
            get => _currentPassword;
            set
            {
                if (SetProperty(ref _currentPassword, value))
                {
                    CheckInputs();
                }
            }
        }
        public string NewPassword 
        { 
            get => _newPassword;
            set
            {
                if (SetProperty(ref _newPassword, value))
                {
                    CheckInputs();
                }
            }
        }
        public string NewPasswordRepeat 
        { 
            get => _newPasswordRepeat;
            set
            {
                if(SetProperty(ref _newPasswordRepeat, value))
                {
                    CheckInputs();
                }
            }
        }

        private void CheckInputs()
        {
            if (InvalidPassword)
            {
                if (NewPassword == NewPasswordRepeat &&
                    (!string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(NewPasswordRepeat)))
                {
                    IsPrimaryBTNEnable = true;
                }
                else
                {
                    IsPrimaryBTNEnable = false;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(CurrentPassword))
                {
                    if (NewPassword == NewPasswordRepeat &&
                        (!string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(NewPasswordRepeat)))
                    {
                        IsPrimaryBTNEnable = true;
                    }
                    else
                    {
                        IsPrimaryBTNEnable = false;
                    }
                }
                else
                {
                    IsPrimaryBTNEnable = false;
                }
            }
        }

        public bool PasswordIsNotInvalid
        {
            get => !InvalidPassword;
        }

        public bool IsPrimaryBTNEnable
        {
            get => _isPrimaryBTNEnable;
            set => SetProperty(ref _isPrimaryBTNEnable, value);
        }

        public bool ShowError
        {
            get => _showError;
            set => SetProperty(ref _showError, value);
        }
        public bool InvalidPassword
        { 
            get => _invalidPassword;
            set
            {
                if(SetProperty(ref _invalidPassword, value))
                {
                    OnPropertyChanged(nameof(PasswordIsNotInvalid));
                }
            }
        }
        public bool PasswordChanged 
        { 
            get => _passwordChanged;
            set => _passwordChanged = value;
        }
        #endregion
    }
}
