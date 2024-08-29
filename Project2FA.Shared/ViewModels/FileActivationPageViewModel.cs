using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using System.Threading.Tasks;
using System.Windows.Input;
using Project2FA.Core.Services;
using Project2FA.Strings;
using System;
using Project2FA.Core;
using Project2FA.Repository.Models;
using Project2FA.Core.Services.JSON;
using Windows.Storage;
using System.Text;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;
using UNOversal.Services.File;
using Project2FA.Services;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.Core.Messenger;
using Project2FA.Core.Services.Crypto;
using UNOversal.Services.Logging;



#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class FileActivationPageViewModel : CredentialViewModelBase
    {
        private string _password;
        private ISecretService SecretService { get; }
        private IFileService FileService { get; }
        private ILoggingService LoggingService { get; }
        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        private string _applicationTitle;
        private bool _isScreenCaptureEnabled;

        public FileActivationPageViewModel()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            DialogService = App.Current.Container.Resolve<IDialogService>();
            NewtonsoftJSONService = App.Current.Container.Resolve<INewtonsoftJSONService>();
            FileService = App.Current.Container.Resolve<IFileService>();
            LoggingService = App.Current.Container.Resolve<ILoggingService>();
            LoginCommand = new RelayCommand(CheckLogin);
            App.ShellPageInstance.ViewModel.NavigationIsAllowed = false;
            var title = Windows.ApplicationModel.Package.Current.DisplayName;
            ApplicationTitle = System.Diagnostics.Debugger.IsAttached ? "[Debug] " + title : title;
            //register the messenger calls
            Messenger.Register<FileActivationPageViewModel, IsScreenCaptureEnabledChangedMessage>(this, (r, m) => r.IsScreenCaptureEnabled = m.Value);
        }

        /// <summary>
        /// Make a login with hitting 'Enter' key possible
        /// </summary>
        /// <param name="e"></param>
        public void LoginWithEnterKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                CheckLogin();
            }
        }

        /// <summary>
        /// Checks the password for the login
        /// </summary>
        private async void CheckLogin()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                if (!await CheckNavigationRequest())
                {
                    Password = string.Empty;
                    await ShowLoginError();
                }
            }
        }

        /// <summary>
        /// Check the input
        /// </summary>
        /// <returns>return true if password is valid;
        /// else decryption not working</returns>
        private async Task<bool> CheckNavigationRequest()
        {
            if (await TestPassword(DataService.Instance.ActivatedDatafile))
            {
                byte[] encryptedBytes;

                encryptedBytes = Encoding.UTF8.GetBytes(Password);

#if WINDOWS_UWP
                SecretService.Helper.WriteSecret(
                    Constants.ContainerName, 
                    Constants.ActivatedDatafileHashName,
                    NewtonsoftJSONService.Serialize(ProtectData.Protect(encryptedBytes)));
                App.ShellPageInstance.SetTitleBarAsDraggable();
#else
                SecretService.Helper.WriteSecret(
                    Constants.ContainerName,
                    Constants.ActivatedDatafileHashName,
                    NewtonsoftJSONService.Serialize(encryptedBytes));
#endif
                App.ShellPageInstance.ViewModel.NavigationIsAllowed = true;
                await App.ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
                Window.Current.Content = App.ShellPageInstance;
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> TestPassword(StorageFile storageFile)
        {
            try
            {
                if (storageFile != null)
                {
                    string datafileStr = string.Empty;
                    // create new thread for buggy Android, else NetworkOnMainThreadException 
#if ANDROID
                    await Task.Run(async () =>
                    {
                        datafileStr = await FileIO.ReadTextAsync(storageFile);
                    });
#else
                    datafileStr = await FileIO.ReadTextAsync(storageFile);
#endif
                    //read the iv for AES
                    DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                    if (datafile.IV == null)
                    {
                        //TODO add error dialog for corrupt datafile
                        return false;
                    }
                    else
                    {
                        byte[] iv = datafile.IV;
                        DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                            (Password, iv, datafileStr, datafile.Version);
                        return true;
                    }
                }
                else
                {
                    await Utils.ErrorDialogs.ShowUnauthorizedAccessError();
                    return false;
                }
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                //Error = exc.Message;
                //ShowError = true;
                //Password = string.Empty;

                return false;
            }
        }

        /// <summary>
        /// Shows a wrong password error to the user
        /// </summary>
        private Task ShowLoginError()
        {
            var dialog = new ContentDialog();
            dialog.Title = Resources.Error;
            dialog.Content = Resources.LoginPagePasswordMismatch;
            dialog.PrimaryButtonText = Resources.Confirm;
            return DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public string DatafileName
        {
            get => DataService.Instance.ActivatedDatafile.Name;
        }
    }
}
