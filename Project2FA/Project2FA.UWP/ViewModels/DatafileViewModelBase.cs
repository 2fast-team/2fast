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
using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Commands;
using Project2FA.UWP.Strings;
using CommunityToolkit.Mvvm.Messaging;

namespace Project2FA.UWP.ViewModels
{
    public class DatafileViewModelBase : ObservableRecipient
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
        private bool _webDAVCredentialsEntered;

        public ICommand PrimaryButtonCommand { get; set; }
        public ICommand ChangePathCommand { get; set; }
        public ICommand ConfirmErrorCommand { get; set; }
        public ICommand CheckServerAddressCommand { get; set; }

        public ICommand LoginCommand { get; set; }

        public ICommand WebDAVLoginCommand { get; set; }

        private ISecretService SecretService { get; }

        private IFileService FileService { get; }

        private IDialogService DialogService { get; }

        public DatafileViewModelBase(ISecretService secretService, IFileService fileService, IDialogService dialogService)
        {
            SecretService = secretService;
            FileService = fileService;
            DialogService = dialogService;
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

            var model = new DBDatafileModel
            {
                DBPasswordHashModel = passwordModel,
                IsWebDAV = isWebDAV,
                Path = isWebDAV ? _choosenOneWebDAVFile.Path : LocalStorageFolder.Path,
                Name = tempDataFileName
            };


            await App.Repository.Datafile.UpsertAsync(model);
            if (isWebDAV)
            {
                // initial file upload
                await DataService.Instance.UploadDatafileWithWebDAV(ApplicationData.Current.LocalFolder, model, true);
            }
            
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
                    TrackingManager.TrackException(nameof(CheckServerStatus),ex);
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
                        await ShowServerAddressNotFoundError();
                        return null;
                    }
                    else
                    {
                        return response;
                    }
                }
                catch
                {
                    await ShowServerAddressNotFoundError();
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
                    await ShowServerCredentialsError();
                }
            }
            else
            {
                if (result != null)
                {
                    if (result.Maintenance)
                    {
                        await ShowServerMaintenanceError();
                    }
                    if (!result.Installed)
                    {
                        await ShowServerNotInstalledError();
                    }
                }
                else
                {
                    await ShowServerAddressNotFoundError();
                }
                //Messenger.Send(new UsernameChangedMessage(Username));
            }
            IsLoading = false;
        }

        /// <summary>
        /// Shows an error message that the sever address was not found
        /// </summary>
        /// <returns></returns>
        public Task ShowServerAddressNotFoundError()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.DatafileViewModelWebDAVServerNotFound;
            dialog.Content = Resources.DatafileViewModelWebDAVServerNotFoundDesc;
            dialog.PrimaryButtonText = Resources.Confirm;
            return DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public Task ShowServerCredentialsError()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.DatafileViewModelWebDAVCredentialsError;
            dialog.Content = Resources.DatafileViewModelWebDAVCredentialsErrorDesc;
            dialog.PrimaryButtonText = Resources.Confirm;
            return DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public Task ShowServerMaintenanceError()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.DatafileViewModelWebDAVMaintenanceError;
            dialog.Content = Resources.DatafileViewModelWebDAVMaintenanceErrorDesc;
            dialog.PrimaryButtonText = Resources.Confirm;
            return DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public Task ShowServerNotInstalledError()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.DatafileViewModelWebDAVNotInstalledError;
            dialog.Content = Resources.DatafileViewModelWebDAVNotInstalledErrorDesc;
            dialog.PrimaryButtonText = Resources.Confirm;
            return DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        #endregion

        private void CheckWebDAVInputs()
        {
            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_webDAVPassword) && !string.IsNullOrEmpty(ServerAddress))
            {
                WebDAVCredentialsEntered = true;
            }
            else
            {
                WebDAVCredentialsEntered = false;
            }
        }
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
            set
            {
                if (SetProperty(ref _serverAddress, value))
                {
                    CheckWebDAVInputs();
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if(SetProperty(ref _username, value))
                {
                    CheckWebDAVInputs();
                }
            }
        }

        public string WebDAVPassword
        {
            get => _webDAVPassword;
            set
            {
                if (SetProperty(ref _webDAVPassword, value))
                {
                    CheckWebDAVInputs();
                }
            }
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
        public bool WebDAVCredentialsEntered 
        { 
            get => _webDAVCredentialsEntered;
            set => SetProperty(ref _webDAVCredentialsEntered, value);
        }

        #endregion
    }
}
