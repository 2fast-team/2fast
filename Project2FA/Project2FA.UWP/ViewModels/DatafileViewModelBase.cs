using Prism.Mvvm;
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
using Project2FA.UWP.Views;
using Prism.Services.Dialogs;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.ViewModels
{
    public class DatafileViewModelBase : BindableBase
    {
        private string _serverAddress;
        private string _username;
        private int _selectedIndex;
        private string _dateFileName;
        private bool _datafileBTNActive;
        private bool _isLoading;
        private bool _showError;
        
        private StorageFolder _localStorageFolder;
        private string _password, _passwordRepeat;
        internal WebDAVFileOrFolderModel _choosenOneWebDAVFile;

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

        public DatafileViewModelBase(ISecretService secretService, IFileService fileService)
        {
            SecretService = secretService;
            FileService = fileService;
            //ErrorsChanged += Model_ErrorsChanged;
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public virtual Task CheckInputs()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates a local DB with the data from the datafile
        /// </summary>
        /// <param name="isWebDAV"></param>
        public async Task CreateLocalFileDB(bool isWebDAV)
        {
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
            return FileService.FileExistsAsync(name, LocalStorageFolder);
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
            IsLoading = true;
            bool result = await WebDAVClient.Client.CheckUserLogin(ServerAddress, Username, WebDAVPassword);

            if (result)
            {
                SecretService.Helper.WriteSecret(Constants.ContainerName, "WDPassword", WebDAVPassword);
                SecretService.Helper.WriteSecret(Constants.ContainerName, "WDServerAddress", ServerAddress);
                SecretService.Helper.WriteSecret(Constants.ContainerName, "WDUsername", Username);
                IsLoading = false;
                return true;
            }
            else
            {
                IsLoading = false;
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
                try
                {
                    Status result = await CheckAndFixServerAddress();
                    if (result != null)
                    {
                        if (result.Installed && result.Maintenance == false)
                        {
                            return (true, result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TrackingManager.TrackException(ex);
                    return (false, null);
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
            if (!string.IsNullOrWhiteSpace(ServerAddress))
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
            else
            {
                return null;
            }
        }

        public void ChooseWebDAV()
        {
            if (SecretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress") != string.Empty)
            {
                ServerAddress = SecretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress");
                WebDAVPassword = SecretService.Helper.ReadSecret(Constants.ContainerName, "WDPassword");
                Username = SecretService.Helper.ReadSecret(Constants.ContainerName, "WDUsername");
                //WebDAVLoginRequiered = false;
                //WebDAVDatafilePropertiesExpanded = true;
            }
        }

        public async Task WebDAVLogin(bool createDatafileCase)
        {
            IsLoading = true;
            (bool success, Status result) = await CheckServerStatus();
            if (success)
            {
                if (ServerAddress != string.Empty && Username != string.Empty && WebDAVPassword != string.Empty)
                {
                    if (await CheckLoginAsync())
                    {
                        WebDAVLoginRequiered = false;
                        WebDAVDatafilePropertiesEnabled = true;
                        WebDAVDatafilePropertiesExpanded = true;

                        
                        var dialog = new WebViewDatafileContentDialog();
                        var param = new DialogParameters();
                        param.Add("CreateDatafileCase", createDatafileCase);
                        param.Add("Status", result);
                        ContentDialogResult dialogresult = await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, param);
                        if (dialogresult == ContentDialogResult.Primary)
                        {
                            ChoosenOneWebDAVFile = dialog.ViewModel.ChoosenOneDatafile;
                        }
                    }
                    else
                    {
                        WebDAVLoginError = true;
                        //TODO error Message
                    }
                }
                else
                {
                    //TODO error Message
                }
            }
            else
            {
                //TODO error Message for server status
            }
            IsLoading = false;
        }

        /// <summary>
        /// Shows an error message that the sever address was not found
        /// </summary>
        /// <returns></returns>
        public Task ShowServerAddressNotFoundMessage()
        {
            // TODO add ContentDialog
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

        public bool DatafileBTNActive
        {
            get => _datafileBTNActive;
            set => SetProperty(ref _datafileBTNActive, value);
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

        public WebDAVFileOrFolderModel ChoosenOneWebDAVFile
        {
            get => _choosenOneWebDAVFile;
            set => SetProperty(ref _choosenOneWebDAVFile, value);
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
