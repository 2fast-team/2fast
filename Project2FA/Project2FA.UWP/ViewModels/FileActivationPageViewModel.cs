using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Project2FA.Core.Services;
using Project2FA.UWP.Services;
using Project2FA.UWP.Strings;
using System;
using Project2FA.Core;
using Template10.Services.Secrets;
using Template10.Services.File;
using Project2FA.Repository.Models;
using Project2FA.Core.Services.JSON;
using Windows.Storage;
using System.Text;
using Project2FA.UWP.Views;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Project2FA.UWP.ViewModels
{
    public class FileActivationPageViewModel : ObservableObject
    {
        private string _password;
        public ICommand LoginCommand { get; }
        private IDialogService DialogService { get; }
        private ISecretService SecretService { get; }
        private IFileService FileService { get; }
        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        private string _applicationTitle;
        public FileActivationPageViewModel()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            DialogService = App.Current.Container.Resolve<IDialogService>();
            NewtonsoftJSONService = App.Current.Container.Resolve<INewtonsoftJSONService>();
            FileService = App.Current.Container.Resolve<IFileService>();
            LoginCommand = new RelayCommand(CheckLogin);
            App.ShellPageInstance.NavigationIsAllowed = false;
            var title = Windows.ApplicationModel.Package.Current.DisplayName;
            ApplicationTitle = System.Diagnostics.Debugger.IsAttached ? "[Debug] " + title : title;
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
        /// <param name="password"></param>
        /// <returns>return true if password is valid;
        /// else decryption not working</returns>
        private async Task<bool> CheckNavigationRequest()
        {
            if (await TestPassword(DataService.Instance.ActivatedDatafile))
            {
                IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(Password, BinaryStringEncoding.Utf8);
                CryptographicBuffer.CopyToByteArray(buffer, out var encryptedBytes);

                SecretService.Helper.WriteSecret(
                    Constants.ContainerName, 
                    Constants.ActivatedDatafileHashName,
                    NewtonsoftJSONService.Serialize(ProtectData.Protect(encryptedBytes)));
                App.ShellPageInstance.SetTitleBarAsDraggable();
                App.ShellPageInstance.NavigationIsAllowed = true;
                await App.ShellPageInstance.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
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
                string datafileStr = await FileService.ReadStringAsync(storageFile.Name, await storageFile.GetParentAsync());
                //read the iv for AES
                DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                if (datafile.IV == null)
                {
                    return false;
                }
                else
                {
                    byte[] iv = datafile.IV;
                    DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                        (Password, iv, datafileStr);
                    return true;
                }

            }
            catch (Exception exc)
            {
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
            dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
            dialog.Title = Resources.Error;
            dialog.Content = Resources.LoginPagePasswordMismatch;
            dialog.PrimaryButtonText = Resources.Confirm;
            return DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public string DatafileName
        {
            get => DataService.Instance.ActivatedDatafile.Name;
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ApplicationTitle
        {
            get => _applicationTitle;
            set => SetProperty(ref _applicationTitle, value);
        }
    }
}
