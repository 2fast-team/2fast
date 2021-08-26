using Prism.Mvvm;
using Project2FA.UWP.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Template10.Services.File;
using Template10.Services.Network;
using Template10.Services.Secrets;
using WebDAVClient.Exceptions;
using WebDAVClient.Types;
using Windows.Storage;
using Prism.Ioc;
using Project2FA.Core.Services;
using Project2FA.Repository.Models;
using System.ComponentModel.DataAnnotations;
using Project2FA.UWP.Services;
using Project2FA.Core;

namespace Project2FA.UWP.ViewModels
{
    public class DatafileViewModelBase : BindableBase
    {
        private string _serverAddress;
        private string _username;
        private string _webDAVPassword;
        private int _selectedIndex;
        private string _dateFileName;
        private bool _isPrimaryBTNEnable;
        private bool _isLoading;
        private bool _showError;
        private StorageFolder _localStorageFolder;
        private string _password, _passwordRepeat;
        internal WebDAVFileOrFolderModel _choosenOneWebDAVFile;

        public ICommand PrimaryButtonCommand { get; set; }
        public ICommand ChangePathCommand { get; set; }
        public ICommand ConfirmErrorCommand { get; set; }
        public ICommand CheckServerAddressCommand { get; set; }

        public ICommand ChooseWebDAVCommand { get; set; }

        public ICommand LoginCommand { get; set; }

        public ICommand WebDAVLoginCommand { get; set; }

        private ISecretService SecretService { get; }

        private IFileService _fileService { get; }

        public DatafileViewModelBase()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            _fileService = App.Current.Container.Resolve<IFileService>();
            //ErrorsChanged += Model_ErrorsChanged;
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public virtual void CheckInputs()
        {
        }

        /// <summary>
        /// Creates a local DB with the data from the datafile
        /// </summary>
        /// <param name="isWebDAV"></param>
        public async Task CreateLocalFileDB(bool isWebDAV)
        {
            //TODO WebDAV case
            // local filedata
            string hash = CryptoService.CreateStringHash(Password, false);
            DBPasswordHashModel passwordModel = await App.Repository.Password.UpsertAsync(new DBPasswordHashModel { Hash = hash });
            string tempDataFileName;
            if (!isWebDAV)
            {
                if (!DateFileName.Contains(".2fa"))
                {
                    DateFileName += ".2fa";
                }
                tempDataFileName = DateFileName;
            }
            else
            {
                tempDataFileName= _choosenOneWebDAVFile.Name;
            }


            await App.Repository.Datafile.UpsertAsync(
                new DBDatafileModel
                {
                    DBPasswordHashModel = passwordModel,
                    IsWebDAV = isWebDAV,
                    Path = isWebDAV ? _choosenOneWebDAVFile.Path : LocalStorageFolder.Path,
                    Name = tempDataFileName
                });
            // write the password with the hash(key) in the secret vault
            SecretService.Helper.WriteSecret(Constants.ContainerName, hash, Password);
        }

        /// <summary>
        /// Check if a .2fa datafile exists.
        /// </summary>
        /// <returns>true if the datafile exists with the name, else false</returns>
        public Task<bool> CheckIfNameExists(string name)
        {
            return _fileService.FileExistsAsync(name, LocalStorageFolder);
        }


        //private void Model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        //{
        //    OnPropertyChanged(nameof(Errors)); // Update Errors on every Error change, so I can bind to it.
        //}

        #region WebDAV
        /// <summary>
        /// Checkes the web dav login credentials
        /// </summary>
        public async Task<bool> CheckLoginAsync()
        {
            bool result = await WebDAVClient.Client.CheckUserLogin(ServerAddress, Username, WebDAVPassword);

            if (result)
            {
                SecretService.Helper.WriteSecret(Constants.ContainerName, "WDPassword", WebDAVPassword);
                SecretService.Helper.WriteSecret(Constants.ContainerName, "WDServerAddress", ServerAddress);
                SecretService.Helper.WriteSecret(Constants.ContainerName, "WDUsername", Username);

                //var client = UWP.Services.WebDAV.WebDAVClientService.Instance.GetClient();
                //var directory = new DirectoryService();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the status of the web dav server
        /// </summary>
        public async Task<(bool success, Status result)> CheckServerStatus()
        {
            INetworkService networkService = App.Current.Container.Resolve<INetworkService>();

            if (await networkService.GetIsInternetAvailableAsync())
            {
                Status result = await CheckAndFixServerAddress();
                if (result != null)
                {
                    if (result.Installed == true && result.Maintenance == false)
                    {
                        return (true, result);
                    }
                }
            }
            else
            {
                //TODO Error Message: No internet is available
            }
            return (false, null);
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
                Status response = await WebDAVClient.Client.GetServerStatus(ServerAddress);
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
                Status response = await WebDAVClient.Client.GetServerStatus(ServerAddress, ignoreServerCertificateErrors);
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
                Status response = await WebDAVClient.Client.GetServerStatus(ServerAddress);
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
        //public List<(string name, string message)> Errors
        //{
        //    get
        //    {
        //        var list = new List<(string name, string message)>();
        //        foreach (var item in from ValidationResult e in GetErrors(null) select e)
        //        {
        //            list.Add((item.MemberNames.FirstOrDefault(), item.ErrorMessage));
        //        }
        //        return list;
        //    }
        //}

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
                RaisePropertyChanged(nameof(UseExtendedHash));
            }
        }

        [Required]
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
                SetProperty(ref _password, value);
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

        #endregion
    }
}
