using Prism.Mvvm;
using System.Threading.Tasks;
using Prism.Ioc;
using System.Windows.Input;
using Template10.Services.Secrets;
using Prism.Commands;
using Project2FA.Core.Services;
using Project2FA.UWP.Services;
using Project2FA.Core;
using Project2FA.Repository.Models;
using System;
using Windows.Storage;
using Template10.Services.File;
using Project2FA.Core.Services.JSON;
using Prism.Services.Dialogs;
using Template10.Utilities;

namespace Project2FA.UWP.ViewModels
{
    /// <summary>
    /// View model for the content dialog to change the password of the current datafile
    /// </summary>
    public class ChangeDatafilePasswordContentDialogViewModel : BindableBase, IDialogInitialize
    {
        private string _currentPassword, _newPassword, _newPasswordRepeat;
        private bool _isPrimaryBTNEnable;
        private bool _showError;
        private bool _invalidPassword = false;
        private bool _passwordChanged;
        private ISecretService SecretService { get; }

        private IFileService FileService { get; }

        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        public ICommand ConfirmErrorCommand { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChangeDatafilePasswordContentDialogViewModel()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            FileService = App.Current.Container.Resolve<IFileService>();
            NewtonsoftJSONService = App.Current.Container.Resolve<INewtonsoftJSONService>();
            ConfirmErrorCommand = new DelegateCommand(() =>
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
                //hash = CryptoService.CreateStringHash(NewPassword);
                SecretService.Helper.WriteSecret(Constants.ContainerName, Constants.ActivatedDatafileHashName, NewPassword);
            }
            else
            {
                passwordHashDB = await App.Repository.Password.GetAsync();
                //delete password in the secret vault
                SecretService.Helper.RemoveSecret(Constants.ContainerName, passwordHashDB.Hash);
                //set new hash
                passwordHashDB.Hash = CryptoService.CreateStringHash(NewPassword, SettingsService.Instance.UseExtendedHash);
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

        /// <summary>
        /// 
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
                    DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>(NewPassword, iv, datafileStr);
                    // load the collection
                    DataService.Instance.Collection.AddRange(deserializeCollection.Collection, true);
                }
                else
                {
                    DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>(CurrentPassword, iv, datafileStr);
                }

                return true;
            }
            catch (Exception)
            {
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
                    (!string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(NewPasswordRepeat)))
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
                if (!string.IsNullOrEmpty(CurrentPassword))
                {
                    if (NewPassword == NewPasswordRepeat &&
                        (!string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(NewPasswordRepeat)))
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
            set => SetProperty(ref _invalidPassword, value);
        }
        public bool PasswordChanged 
        { 
            get => _passwordChanged;
            set => _passwordChanged = value;
        }
        #endregion
    }
}
