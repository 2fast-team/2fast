using Prism.Commands;
using Prism.Mvvm;
using Project2FA.Core.Services;
using Project2FA.UWP.Services;
using System;
using System.Windows.Input;
using Template10.Services.Dialog;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
using Windows.UI.Xaml.Controls;
using Prism.Ioc;
using Windows.UI.Xaml.Input;
using Project2FA.UWP.Strings;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Template10.Services.Secrets;

namespace Project2FA.UWP.ViewModels
{
    /// <summary>
    /// View model for the login page
    /// </summary>
    public class LoginPageViewModel : BindableBase
    {
        private bool _windowsHelloIsUsable, _isLogout;
        private string _password;
        public ICommand LoginCommand { get; }
        public ICommand WindowsHelloLoginCommand { get; }
        private IDialogService _dialogService { get; }
        private string _applicationTitle;

        /// <summary>
        /// Constructor
        /// </summary>
        public LoginPageViewModel()
        {
            _dialogService = App.Current.Container.Resolve<IDialogService>();
            LoginCommand = new DelegateCommand(CheckLogin);
            WindowsHelloLoginCommand = new DelegateCommand(WindowsHelloLogin);
            ApplicationTitle = Windows.ApplicationModel.Package.Current.DisplayName;
        }

        /// <summary>
        /// Checks and starts Windows Hello login, if possible and desired
        /// </summary>
        public async void CheckCapabilityWindowsHello()
        {
            if (await KeyCredentialManager.IsSupportedAsync())
            {
                WindowsHelloIsUsable = true;
                var settings = SettingsService.Instance;
                if (settings.PreferWindowsHello == Services.Enums.WindowsHelloPreferEnum.None)
                {
                    var dialog = new ContentDialog();
                    var markdown = new MarkdownTextBlock
                    {
                        Text = Resources.WindowsHelloPreferMessage
                    };
                    dialog.Content = markdown;
                    dialog.PrimaryButtonText = Resources.Yes;
                    dialog.SecondaryButtonText = Resources.No;
                    var result = await _dialogService.ShowAsync(dialog);
                    switch (result)
                    {
                        case ContentDialogResult.None:
                            break;
                        case ContentDialogResult.Primary:
                            settings.PreferWindowsHello = Services.Enums.WindowsHelloPreferEnum.Prefer;
                            WindowsHelloLogin();
                            break;
                        case ContentDialogResult.Secondary:
                            settings.PreferWindowsHello = Services.Enums.WindowsHelloPreferEnum.No;
                            break;
                        default:
                            break;
                    }
                }
                else if(settings.PreferWindowsHello == Services.Enums.WindowsHelloPreferEnum.Prefer)
                {
                    if (!IsLogout)
                    {
                        WindowsHelloLogin();
                    }
                }

            }
        }

        /// <summary>
        /// Verify login with Windows Hello
        /// </summary>
        private async void WindowsHelloLogin()
        {
            UserConsentVerificationResult consentResult = await UserConsentVerifier.RequestVerificationAsync(Resources.WindowsHelloLoginMessage);
            if (consentResult.Equals(UserConsentVerificationResult.Verified))
            {
                var dbHash = await App.Repository.Password.GetAsync();
                var secretService = App.Current.Container.Resolve<ISecretService>();
                if (!await CheckNavigationRequest(secretService.Helper.ReadSecret(Constants.ContainerName,dbHash.Hash)))
                {
                    ShowLoginError();
                }
            }
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
                if (!await CheckNavigationRequest(Password))
                {
                    ShowLoginError();
                    Password = string.Empty;
                }
            }
        }

        /// <summary>
        /// Check if the input have the same hash as the saved password
        /// and set the Windows content to the ShellPage if true
        /// </summary>
        /// <param name="password"></param>
        /// <returns>return true if password hash is valid;
        /// else when the hash is not equal to the saved password</returns>
        private async Task<bool> CheckNavigationRequest(string password)
        {
            var dbHash = await App.Repository.Password.GetAsync();
            if (SettingsService.Instance.UseExtendedHash)
            {
                //byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                //if (Argon2.Verify(dbHash.Hash, passwordBytes, Environment.ProcessorCount))
                //{
                //    App.ShellPageInstance.SetTitleBarAsDraggable();
                //    await App.ShellPageInstance.NavigationService.NavigateAsync("/AccountCodePage");
                //    Window.Current.Content = App.ShellPageInstance;
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
                return false;
            }
            else
            {
                string pwdhash = CryptoService.CreateStringHash(password);
                if (dbHash.Hash == pwdhash)
                {
                    App.ShellPageInstance.SetTitleBarAsDraggable();
                    await App.ShellPageInstance.NavigationService.NavigateAsync("/AccountCodePage");
                    Window.Current.Content = App.ShellPageInstance;
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        /// <summary>
        /// Shows a wrong password error to the user
        /// </summary>
        private async void ShowLoginError()
        {
            var dialog = new ContentDialog();
            dialog.Title = Resources.Error;
            dialog.Content = Resources.LoginPagePasswordMismatch;
            dialog.PrimaryButtonText = Resources.Confirm;
            await _dialogService.ShowAsync(dialog);
        }

        public bool WindowsHelloIsUsable
        {
            get => _windowsHelloIsUsable;
            set => SetProperty(ref _windowsHelloIsUsable, value);
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public bool IsLogout 
        { 
            get => _isLogout;
            set => SetProperty(ref _isLogout, value);
        }
        public string ApplicationTitle 
        { 
            get => _applicationTitle;
            set => SetProperty(ref _applicationTitle, value);
        }
    }
}
