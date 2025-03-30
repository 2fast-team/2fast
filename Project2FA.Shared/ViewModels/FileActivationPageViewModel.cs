using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Ioc;
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
using Project2FA.Utils;
using Newtonsoft.Json;
using UNOversal.Services.Serialization;
using UNOversal.Navigation;



#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
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
    public class FileActivationPageViewModel : CredentialViewModelBase, IInitialize
    {
        private ISecretService SecretService { get; }
        private IFileService FileService { get; }
        private ILoggingService LoggingService { get; }
        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        private ISerializationService SerializationService { get; }

        public FileActivationPageViewModel()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            DialogService = App.Current.Container.Resolve<IDialogService>();
            NewtonsoftJSONService = App.Current.Container.Resolve<INewtonsoftJSONService>();
            FileService = App.Current.Container.Resolve<IFileService>();
            LoggingService = App.Current.Container.Resolve<ILoggingService>();
            SerializationService = App.Current.Container.Resolve<ISerializationService>();
            LoginCommand = new AsyncRelayCommand(CheckLoginTask);
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
        public async void LoginWithEnterKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await CheckLoginTask();
            }
        }

        /// <summary>
        /// Checks the password for the login
        /// </summary>
        private async Task CheckLoginTask()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                await CheckNavigationRequest();
            }
        }

        /// <summary>
        /// Check the input
        /// </summary>
        /// <returns>return true if password is valid;
        /// else decryption not working</returns>
        private async Task<bool> CheckNavigationRequest()
        {
            var (succesful, noerror) = await TestPassword(DataService.Instance.ActivatedDatafile);
            if (succesful && noerror)
            {
#if WINDOWS_UWP
                SecretService.Helper.WriteSecret(
                    Constants.ContainerName, 
                    Constants.ActivatedDatafileHashName,
                    SerializationService.Serialize(ProtectData.Protect(Encoding.UTF8.GetBytes(Password))));
                App.ShellPageInstance.SetTitleBarAsDraggable();
#else
                SecretService.Helper.WriteSecret(
                    Constants.ContainerName,
                    Constants.ActivatedDatafileHashName,
                    SerializationService.Serialize(Encoding.UTF8.GetBytes(Password)));
#endif
                App.ShellPageInstance.ViewModel.NavigationIsAllowed = true;
                await App.ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
                Window.Current.Content = App.ShellPageInstance;
                return true;
            }
            else
            {
                Password = string.Empty;
                return false;
            }
        }

        private async Task<(bool succesful,bool noerror)> TestPassword(StorageFile storageFile)
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
                        // file is not valid
                        await ErrorDialogs.CorruptDataFileError();
                        return (false, false);
                    }
                    else
                    {
                        byte[] iv = datafile.IV;
                        if (datafile.Collection.Count > 0)
                        {
                            try
                            {
                                DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                                (Encoding.UTF8.GetBytes(Password), iv, datafileStr, datafile.Version);
                            }
                            catch (Exception exc)
                            {
                                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                                await ShowLoginError();
                                return (false, false);
                            }
                        }
                        else
                        {
                            // file have no accounts added
                            await ErrorDialogs.EmptyDataFileError();
                            return (false, false);
                        }

                        return (true, true);
                    }
                }
                else
                {
                    await Utils.ErrorDialogs.ShowUnauthorizedAccessError();
                    return (false, false);
                }
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);

                return (false, false);
            }
        }

        public void Initialize(INavigationParameters parameters)
        {
#if ANDROID || IOS
            App.ShellPageInstance.ViewModel.TabBarIsVisible = false;
#endif
        }

        public string DatafileName
        {
            get => DataService.Instance.ActivatedDatafile.Name;
        }
    }
}
