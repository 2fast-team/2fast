﻿using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Repository.Models;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using UNOversal.Services.Secrets;
using Project2FA.Core;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using UNOversal.Ioc;
using WebDAVClient.Types;
using WebDAVClient.Exceptions;
using UNOversal.Services.Network;
using Project2FA.Strings;
using Project2FA.Core.Services.Crypto;
using UNOversal.Services.Logging;
using Project2FA.Services;
using System.Net;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml.Controls;
#else
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
    public class DatafileViewModelBase : ObservableObject
    {
        private string _serverAddress;
        private string _username;
        private int _selectedIndex = 0;
        private string _dateFileName;
        private bool _datafileBTNActive;
        private bool _isLoading;
        private bool _showError;

        private StorageFolder _localStorageFolder;

        private StorageFile _localStorageFile;
        private string _password, _passwordRepeat;
        private WebDAVFileOrFolderModel _choosenOneWebDAVFile;

        private string _webDAVPassword;
        private bool _webDAVDatafilePropertiesEnabled;
        private bool _webDAVLoginRequiered;
        private bool _webDAVDatafilePropertiesExpanded;
        private bool _isWebDAVCreationButtonEnable;
        private bool _webDAVLoginError;
        private bool _webDAVCredentialsEntered;

        public ICommand PrimaryButtonCommand { get; internal set; }
        public ICommand ChangePathCommand { get; internal set; }
        public ICommand ConfirmErrorCommand { get; internal set; }
        public ICommand CheckServerAddressCommand { get; internal set; }

        //public ICommand ChooseWebDAVCommand { get; set; }

        public ICommand LoginCommand { get; set; }

        public ICommand WebDAVLoginCommand { get; internal set; }

        private ISecretService SecretService { get; }

        private IFileService FileService { get; }

        private IDialogService DialogService { get; }

        private ILoggingService LoggingService { get; }

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
            LoggingService = App.Current.Container.Resolve<ILoggingService>();
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
        public async Task<bool> CreateDataFileSettings(bool isWebDAV)
        {
            try
            {
                string hash = CryptoService.CreateStringHash(Password);
                
                SettingsService.Instance.DataFilePasswordHash = hash;
                if (!DateFileName.Contains(".2fa"))
                {
                    DateFileName += ".2fa";
                }

#if __IOS__
                //save the file path for persistent access on iOS 
                Foundation.NSError error;
                Foundation.NSUrl url = Foundation.NSUrl.FromFilename(LocalStorageFile.Path);
                Foundation.NSData bookmark = url.CreateBookmarkData(Foundation.NSUrlBookmarkCreationOptions.WithSecurityScope, null, null, out error);
                Foundation.NSUserDefaults.StandardUserDefaults[Constants.ContainerName] = bookmark;
                Foundation.NSUserDefaults.StandardUserDefaults.Synchronize();
#endif
#if WINDOWS_UWP
                SettingsService.Instance.DataFilePath = ChoosenOneWebDAVFile == null ? LocalStorageFolder.Path : ChoosenOneWebDAVFile.Path;
#else
                SettingsService.Instance.DataFilePath = ChoosenOneWebDAVFile == null ? LocalStorageFile.Path : ChoosenOneWebDAVFile.Path;
#endif
                SettingsService.Instance.DataFileName = DateFileName;
                SettingsService.Instance.DataFileWebDAVEnabled = isWebDAV;

                // write the password with the hash(key) in the secret vault
                SecretService.Helper.WriteSecret(Constants.ContainerName, hash, Password);
                return true;
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
#if WINDOWS_UWP
                TrackingManager.TrackExceptionCatched(nameof(CreateDataFileSettings), exc);
#endif
                return false;
            }


        }

        /// <summary>
        /// Check if a .2fa datafile exists.
        /// </summary>
        /// <returns>true if the datafile exists with the name, else false</returns>
        public Task<bool> CheckIfNameExists(string name)
        {
            return FileService.FileExistsAsync(name, LocalStorageFolder);
        }

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
                catch (Exception exc)
                {
                    await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
#if WINDOWS_UWP
                    TrackingManager.TrackException(nameof(CheckServerStatus), exc);
#endif
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
                    (Status response, System.Net.HttpStatusCode statusCode) = await WebDAVClient.Client.GetServerStatus(ServerAddress);
                    if (response == null)
                    {
                        ServerAddress = ServerAddress.Replace("https:", "http:");
                    }
                    else if(statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        (Status newResponse, System.Net.HttpStatusCode newStatusCode) = await WebDAVClient.Client.GetServerStatus(ServerAddress, 
                            networkCredential:new NetworkCredential(Username,Password));
                        if (newResponse != null)
                        {
                            return newResponse;
                        }
                    }
                    else
                    {
                        return response;
                    }
                }
                catch (ResponseError exc)
                {
                    await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                    if (exc.Message.Equals("The certificate authority is invalid or incorrect"))
                    {
                        //TODO Error Message: The certificate authority is invalid or incorrect
                    }
                    return null;
                }

                if (ignoreServerCertificateErrors)
                {
                    (Status response, System.Net.HttpStatusCode statusCode) = await WebDAVClient.Client.GetServerStatus(ServerAddress, 
                        ignoreServerCertificateErrors:ignoreServerCertificateErrors);
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
                    (Status response, System.Net.HttpStatusCode statusCode) = await WebDAVClient.Client.GetServerStatus(ServerAddress);
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
                catch (Exception e)
                {
                    await LoggingService.LogException(e, SettingsService.Instance.LoggingSetting);
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

        private void CheckWebDAVInputs()
        {
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(WebDAVPassword) && !string.IsNullOrEmpty(ServerAddress))
            {
                WebDAVCredentialsEntered = true;
                IsWebDAVCreationButtonEnable = true;
            }
            else
            {
                WebDAVCredentialsEntered = false;
                IsWebDAVCreationButtonEnable = false;
            }
        }

#endregion

#region GetSets

        public string ServerAddress
        {
            get => _serverAddress;
            set 
            { 
                if(SetProperty(ref _serverAddress, value))
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
                if(SetProperty(ref _webDAVPassword, value))
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
                if(SetProperty(ref _password, value))
                {
                    CheckInputs();
                }
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
