using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Core.Services;
using Project2FA.Repository.Models;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using UNOversal.Services.Secrets;
using Project2FA.UNO.Services;
using Project2FA.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Prism.Navigation;
using Prism.Ioc;
using Windows.Storage.Pickers;

namespace Project2FA.UNO.ViewModels
{
    public class DatafileViewModelBase : ObservableObject
    {
        private string _serverAddress;
        private string _username;
        private int _selectedIndex;
        private string _dateFileName;
        private bool _datafileBTNActive;
        private bool _isLoading;
        private bool _showError;

        private StorageFolder _localStorageFolder;

        private StorageFile _localStorageFile;
        private string _password, _passwordRepeat;

        private string _webDAVPassword;
        private bool _webDAVDatafilePropertiesEnabled;
        private bool _webDAVLoginRequiered;
        private bool _webDAVDatafilePropertiesExpanded;
        private bool _isWebDAVCreationButtonEnable;
        private bool _webDAVLoginError;

        public ICommand PrimaryButtonCommand { get; set; }
        public ICommand ChangePathCommand { get; set; }
        public ICommand ConfirmErrorCommand { get; set; }
        public ICommand CheckServerAddressCommand { get; set; }

        //public ICommand ChooseWebDAVCommand { get; set; }

        public ICommand LoginCommand { get; set; }

        public ICommand WebDAVLoginCommand { get; set; }

        private ISecretService SecretService { get; }

        private IFileService FileService { get; }

        private IDialogService DialogService { get; }

#if __ANDROID__
        internal const int RequestCode = 6002;
        public static TaskCompletionSource<Android.Content.Intent?>? _currentFileOpenPickerRequest;
#endif

        //public DatafileViewModelBase(ISecretService secretService, IFileService fileService, IDialogService dialogService)
        //{
        //    SecretService = secretService;
        //    FileService = fileService;
        //    DialogService = dialogService;
        //    ErrorsChanged += Model_ErrorsChanged;
        //}

        public DatafileViewModelBase()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            FileService = App.Current.Container.Resolve<IFileService>();
            DialogService = App.Current.Container.Resolve<IDialogService>();
            //ErrorsChanged += Model_ErrorsChanged;
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public virtual Task CheckInputs()
        {
            return Task.CompletedTask;
        }

//#if __ANDROID__
//        internal static bool TryHandleIntent(Android.Content.Intent intent, Android.App.Result resultCode)
//        {
//            if (_currentFileOpenPickerRequest == null)
//            {
//                return false;
//            }
//            if (resultCode == Android.App.Result.Canceled)
//            {
//                _currentFileOpenPickerRequest.SetResult(null);
//            }
//            else
//            {
//                _currentFileOpenPickerRequest.SetResult(intent);
//            }
//            return true;
//        }
//#endif

        /// <summary>
        /// Creates a local DB with the data from the datafile
        /// </summary>
        /// <param name="isWebDAV"></param>
        public async Task<DBDatafileModel> CreateLocalFileDB(bool isWebDAV)
        {
            string hash = CryptoService.CreateStringHash(Password, false);
            DBPasswordHashModel passwordModel = await App.Repository.Password.UpsertAsync(new DBPasswordHashModel { Hash = hash });
            string tempDataFileName;
            if (!DateFileName.Contains(".2fa"))
            {
                DateFileName += ".2fa";
            }
            tempDataFileName = DateFileName;

#if __IOS__
            Foundation.NSError error;
            Foundation.NSUrl url = Foundation.NSUrl.FromString(LocalStorageFile.Path);
            Foundation.NSData bookmark = url.CreateBookmarkData(Foundation.NSUrlBookmarkCreationOptions.WithSecurityScope, null, null, out error);
            Foundation.NSUserDefaults.StandardUserDefaults["bookmark"] = bookmark;
            Foundation.NSUserDefaults.StandardUserDefaults.Synchronize();
#endif

            var model = new DBDatafileModel
            {
                DBPasswordHashModel = passwordModel,
                IsWebDAV = isWebDAV,
                Path = LocalStorageFolder == null ? LocalStorageFile.Path: LocalStorageFolder.Path,
                Name = tempDataFileName
            };


            await App.Repository.Datafile.UpsertAsync(model);


            // write the password with the hash(key) in the secret vault
            SecretService.Helper.WriteSecret(Constants.ContainerName, hash, Password);

            return model;
        }

        /// <summary>
        /// Check if a .2fa datafile exists.
        /// </summary>
        /// <returns>true if the datafile exists with the name, else false</returns>
        public Task<bool> CheckIfNameExists(string name)
        {
            return FileService.FileExistsAsync(name, LocalStorageFolder);
        }

        #region GetSets

        public string ServerAddress
        {
            get => _serverAddress;
            set => SetProperty(ref _serverAddress, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string WebDAVPassword
        {
            get => _webDAVPassword;
            set => SetProperty(ref _webDAVPassword, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }
        public StorageFile LocalStorageFile
        {
            get => _localStorageFile;
            set => SetProperty(ref _localStorageFile, value);
        }

        public StorageFolder LocalStorageFolder
        {
            get => _localStorageFolder;
            set => SetProperty(ref _localStorageFolder, value);
        }

        public string DateFileName
        {
            get => _dateFileName;
            set
            {
                if (SetProperty(ref _dateFileName, value))
                {
                    CheckInputs();
                }
            }
        }

        public bool DatafileBTNActive
        {
            get => _datafileBTNActive;
            set => SetProperty(ref _datafileBTNActive, value);
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                CheckInputs();
            }
        }

        public string PasswordRepeat
        {
            get => _passwordRepeat;
            set
            {
                SetProperty(ref _passwordRepeat, value);
                CheckInputs();
            }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool ShowError
        {
            get => _showError;
            set => SetProperty(ref _showError, value);
        }

        public bool WebDAVDatafilePropertiesEnabled
        {
            get => _webDAVDatafilePropertiesEnabled;
            set => SetProperty(ref _webDAVDatafilePropertiesEnabled, value);
        }

        public bool WebDAVLoginRequiered
        {
            get => _webDAVLoginRequiered;
            set => SetProperty(ref _webDAVLoginRequiered, value);
        }

        public bool WebDAVDatafilePropertiesExpanded
        {
            get => _webDAVDatafilePropertiesExpanded;
            set => SetProperty(ref _webDAVDatafilePropertiesExpanded, value);
        }

        public bool IsWebDAVCreationButtonEnable
        {
            get => _isWebDAVCreationButtonEnable;
            set => SetProperty(ref _isWebDAVCreationButtonEnable, value);
        }
        public bool WebDAVLoginError
        {
            get => _webDAVLoginError;
            set => SetProperty(ref _webDAVLoginError, value);
        }

        #endregion
    }
}
