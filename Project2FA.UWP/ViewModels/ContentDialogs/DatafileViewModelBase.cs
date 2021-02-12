using Prism.Mvvm;
using Project2FA.UWP.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Template10.Services.File;
using Template10.Services.Network;
using Template10.Services.Secrets;
using WebDAVClient.Exceptions;
using WebDAVClient.Types;
using Windows.Storage;
using Xamarin.Essentials;
using Prism.Ioc;
using Project2FA.Core.Services;
using Project2FA.Repository.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Project2FA.UWP.Services;
using System.ComponentModel;

namespace Project2FA.UWP.ViewModels
{
    public class DatafileViewModelBase : ObservableValidator
    {
        private string _serverAddress;
        private string _username;
        private string _webDAVPassword;
        private string _webDAVServerBackgroundUrl;
        private int _selectedIndex;
        private int _flipViewSelectedIndex;
        private string _dateFileName;
        private bool _isPrimaryBTNEnable;
        private bool _isLoading;
        private bool _showError;
        private StorageFolder _localStorageFolder;
        private string _password, _passwordRepeat;
        private WebDAVPresenterPage _webDAVViewer;

        public ICommand PrimaryButtonCommand { get; set; }
        public ICommand ChangePathCommand { get; set; }
        public ICommand ConfirmErrorCommand { get; set; }
        public ICommand CheckServerAddressCommand { get; set; }

        public ICommand UseWebDAVCommand { get; set; }

        public ICommand LoginCommand { get; set; }

        public ICommand WebDAVSaveSettingsCommand { get; set; }

        private ISecretService _secretService { get; }
        private IFileService _fileService { get; }

        public DatafileViewModelBase()
        {
            _secretService = App.Current.Container.Resolve<ISecretService>();
            _fileService = App.Current.Container.Resolve<IFileService>();
            ErrorsChanged += Model_ErrorsChanged;
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public virtual void CheckInputs()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isWebDAV"></param>
        public async Task CreateLocalFileDB(bool isWebDAV)
        {
            //TODO WebDAV case
            // local filedata
            var useArgonHash = SettingsService.Instance.UseExtendedHash;
            var hash = CryptoService.CreateStringHash(Password, useArgonHash);
            var passwordModel = await App.Repository.Password.UpsertAsync(new DBPasswordHashModel { Hash = hash });
            if (!DateFileName.Contains(".2fa"))
            {
                DateFileName += ".2fa";
            }
            await App.Repository.Datafile.UpsertAsync(
                new DBDatafileModel
                {
                    DBPasswordHashModel = passwordModel,
                    IsWebDAV = isWebDAV,
                    Path = isWebDAV ? string.Empty : LocalStorageFolder.Path,
                    Name = DateFileName
                }); ;
            // write the password with the hash(key) in the secret vault
            _secretService.Helper.WriteSecret(Constants.ContainerName, hash, Password);
        }

        /// <summary>
        /// Check if a .2fa datafile exists.
        /// </summary>
        /// <returns>true if the datafile exists with the name, else false</returns>
        public async Task<bool> CheckIfNameExists(string name)
        {
            return await _fileService.FileExistsAsync(name, LocalStorageFolder);
        }


        private void Model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Errors)); // Update Errors on every Error change, so I can bind to it.
        }

        #region WebDAV
        /// <summary>
        /// Checkes the web dav login credentials
        /// </summary>
        public async void CheckLoginAsync()
        {
            var result = await WebDAVClient.Client.CheckUserLogin(ServerAddress, Username, WebDAVPassword);

            if (result)
            {
                await SecureStorage.SetAsync("WDPassword", WebDAVPassword);
                await SecureStorage.SetAsync("WDServerAddress", ServerAddress);
                await SecureStorage.SetAsync("WDUsername", Username);
                //var client = UWP.Services.WebDAV.WebDAVClientService.Instance.GetClient();
                //var directory = new DirectoryService();
                FlipViewSelectedIndex = 2;
                bool createDatafile = true;
                WebDAVViewer.ViewModel.StartLoading(createDatafile);
            }
        }

        /// <summary>
        /// Checks the status of the web dav server
        /// </summary>
        public async void CheckServerStatus()
        {
            var networkService = App.Current.Container.Resolve<INetworkService>();

            if (await networkService.GetIsInternetAvailableAsync())
            {
                var result = await CheckAndFixServerAddress();
                if (result != null)
                {
                    if (result.Installed == true && result.Maintenance == false)
                    {
                        FlipViewSelectedIndex = 1;
                        WebDAVServerBackgroundUrl = ServerAddress + "/index.php/apps/theming/image/background";
                    }
                }
            }
            else
            {
                //TODO Error Message: No internet is available
            }
        }

        /// <summary>
        /// Checks the web dav server address and improves it if necessary
        /// </summary>
        /// <returns></returns>
        public async Task<Status> CheckAndFixServerAddress()
        {

            bool ignoreServerCertificateErrors = false;
            if (!ServerAddress.StartsWith("http"))
            {
                ServerAddress = string.Format("https://{0}", ServerAddress);
            }

            try
            {
                var response = await WebDAVClient.Client.GetServerStatus(ServerAddress);
                if (response == null)
                {
                    ServerAddress = ServerAddress.Replace("https:", "http:");
                }
                else
                {
                    return response;
                }
            }
            catch (ResponseError e)
            {
                if (e.Message.Equals("The certificate authority is invalid or incorrect"))
                {
                    //TODO Error Message: The certificate authority is invalid or incorrect
                }
                return null;
            }
            //catch
            //{
            //    await ShowServerAddressNotFoundMessage();
            //    return false;
            //}

            if (ignoreServerCertificateErrors)
            {
                var response = await WebDAVClient.Client.GetServerStatus(ServerAddress, true);
                if (response == null)
                {
                    ServerAddress = ServerAddress.Replace("https:", "http:");
                }
                else
                {
                    return response;
                }
            }

            try
            {
                var response = await WebDAVClient.Client.GetServerStatus(ServerAddress);
                if (response == null)
                {
                    await ShowServerAddressNotFoundMessage();
                    return null;
                }
                else
                {
                    return response;
                }
            }
            catch
            {
                await ShowServerAddressNotFoundMessage();
                return null;
            }
        }

        /// <summary>
        /// Shows an error message that the sever address was not found
        /// </summary>
        /// <returns></returns>
        public Task ShowServerAddressNotFoundMessage()
        {
            throw new NotImplementedException();
        }

        #endregion


        #region GetSets
        public List<(string name, string message)> Errors
        {
            get
            {
                var list = new List<(string name, string message)>();
                foreach (var item in from ValidationResult e in GetErrors(null) select e)
                {
                    list.Add((item.MemberNames.FirstOrDefault(), item.ErrorMessage));
                }
                return list;
            }
        }

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

        public StorageFolder LocalStorageFolder
        {
            get => _localStorageFolder;
            set => SetProperty(ref _localStorageFolder, value);
        }

        public bool UseExtendedHash
        {
            get => SettingsService.Instance.UseExtendedHash;
            set
            {
                SettingsService.Instance.UseExtendedHash = value;
                OnPropertyChanged(nameof(UseExtendedHash));
            }
        }

        [Required]
        public string DateFileName
        {
            get => _dateFileName;
            set
            {
                if (SetProperty(ref _dateFileName, value, true))
                {
                    CheckInputs();
                }
            }
        }

        public bool IsPrimaryBTNEnable
        {
            get => _isPrimaryBTNEnable;
            set => SetProperty(ref _isPrimaryBTNEnable, value);
        }

        [Required]
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value, true);
                CheckInputs();
            }
        }

        //private List<(string name, string message)> _errors;
        //public List<(string name, string message)> Errors
        //{
        //    get
        //    {
        //        if (_errors == null)
        //        {
        //            _errors = new List<(string name, string message)>();
        //        }
        //        foreach (var item in from ValidationResult e in GetErrors(null) select e)
        //        {
        //            _errors.Add((item.MemberNames.FirstOrDefault(), item.ErrorMessage));
        //        }
        //        return _errors;
        //    }
        //}

        [Required]
        public string PasswordRepeat
        {
            get => _passwordRepeat;
            set
            {
                SetProperty(ref _passwordRepeat, value, true);
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
        public int FlipViewSelectedIndex
        {
            get => _flipViewSelectedIndex;
            set => SetProperty(ref _flipViewSelectedIndex, value);
        }
        public string WebDAVServerBackgroundUrl
        {
            get => _webDAVServerBackgroundUrl;
            set => SetProperty(ref _webDAVServerBackgroundUrl, value);
        }
        public WebDAVPresenterPage WebDAVViewer { get => _webDAVViewer; set => _webDAVViewer = value; }

        #endregion
    }
}
