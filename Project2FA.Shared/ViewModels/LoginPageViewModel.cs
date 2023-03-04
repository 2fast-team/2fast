using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using Project2FA.Core.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
using Project2FA.Core;
using Project2FA.Services;
using Project2FA.Strings;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.Core.Messenger;
using Project2FA.Core.Services.Crypto;


#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using WinUIWindow = Windows.UI.Xaml.Window;
using Microsoft.Toolkit.Uwp.UI.Controls;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Data;
using Project2FA.UNO;
using Project2FA.UNO.Views;
using WinUIWindow = Microsoft.UI.Xaml.Window;
#endif

namespace Project2FA.ViewModels
{
    /// <summary>
    /// View model for the login page
    /// </summary>
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class LoginPageViewModel : CredentialViewModelBase
    {


        /// <summary>
        /// Constructor
        /// </summary>
        public LoginPageViewModel()
        {
            DialogService = App.Current.Container.Resolve<IDialogService>();
            LoginCommand = new RelayCommand(CheckLogin);
#if WINDOWS_UWP
            WindowsHelloLoginCommand = new RelayCommand(WindowsHelloLogin);
#endif
            var title = Windows.ApplicationModel.Package.Current.DisplayName;
            ApplicationTitle = System.Diagnostics.Debugger.IsAttached ? "[Debug] " + title : title;
            //register the messenger calls
            Messenger.Register<LoginPageViewModel, IsScreenCaptureEnabledChangedMessage>(this, (r, m) => r.IsScreenCaptureEnabled = m.Value);
        }

#if WINDOWS_UWP
        /// <summary>
        /// Checks and starts Windows Hello login, if possible and desired
        /// </summary>
        public async Task CheckCapabilityWindowsHello()
        {
            if (await KeyCredentialManager.IsSupportedAsync())
            {
                WindowsHelloIsUsable = SettingsService.Instance.ActivateWindowsHello;
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
                    var result = await DialogService.ShowDialogAsync(dialog, new DialogParameters());
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
                else if (settings.PreferWindowsHello == Services.Enums.WindowsHelloPreferEnum.Prefer)
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
                //TODO check if this is a problem
                if (!await CheckNavigationRequest(secretService.Helper.ReadSecret(Constants.ContainerName, dbHash.Hash)))
                {
                    await ShowLoginError();
                }
            }
        }
#endif

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
                    Password = string.Empty;
                    await ShowLoginError();
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

            string pwdhash = CryptoService.CreateStringHash(password);
            if (dbHash.Hash == pwdhash)
            {
#if WINDOWS_UWP
                App.ShellPageInstance.SetTitleBarAsDraggable();
#endif
                await App.ShellPageInstance.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
                WinUIWindow.Current.Content = App.ShellPageInstance;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
