using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using System;
using System.Threading.Tasks;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;
using Project2FA.Core;
using Project2FA.Services;
using Project2FA.Strings;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.Core.Messenger;
using Project2FA.Core.Services.Crypto;
using System.Threading;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using WinUIWindow = Windows.UI.Xaml.Window;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Data;
using Project2FA.UNO;
using Project2FA.UNO.Views;
using WinUIWindow = Microsoft.UI.Xaml.Window;
using CommunityToolkit.WinUI.UI.Controls;
#endif

#if __IOS__ || MACCATALYST
using UIKit;
using LocalAuthentication;
using BiometryService;
#endif

#if __ANDROID__
using Project2FA.UNO;
using AndroidX.Biometric;
using BiometryService;
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
#if ANDROID || IOS || MACCATALYST
        private IBiometryService BiometryService { get; }
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
#endif
        /// <summary>
        /// Constructor
        /// </summary>
        public LoginPageViewModel()
        {
            DialogService = App.Current.Container.Resolve<IDialogService>();
            LoginCommand = new RelayCommand(CheckLogin);
#if WINDOWS_UWP
            WindowsHelloLoginCommand = new RelayCommand(WindowsHelloLoginCommandTask);
#else
#if __IOS__
			var laContext = new LAContext
			{
				LocalizedReason = "REASON THAT APP WANTS TO USE BIOMETRY :)",
				LocalizedFallbackTitle = "FALLBACK",
				LocalizedCancelTitle = "CANCEL"
			};

            BiometryService = new BiometryService.BiometryService(
				"Biometrics_Confirm",
				laContext,
				LAPolicy.DeviceOwnerAuthentication);
#endif

            //Note that not all combinations of authenticator types are supported prior to Android 11 (API 30). Specifically, DEVICE_CREDENTIAL alone is unsupported prior to API 30, and BIOMETRIC_STRONG | DEVICE_CREDENTIAL is unsupported on API 28-29
#if __ANDROID__
            Func<BiometricPrompt.PromptInfo> promptBuilder;
            if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.Q)
            {
                promptBuilder = () => new BiometricPrompt.PromptInfo.Builder()
                    .SetTitle("Biometrics SignIn")
                    .SetSubtitle("Biometrics Confirm")
                    //.SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricWeak | BiometricManager.Authenticators.DeviceCredential) // Fallback on secure pin WARNING cannot Encrypt data with this settings
                    .SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricStrong) // used for Encrypt decrypt feature for device bellow Android 11
                    .SetNegativeButtonText("Cancel")
                    .Build();
            }
            else
            {
                promptBuilder = () => new BiometricPrompt.PromptInfo.Builder()
                    .SetTitle("Biometrics SignIn")
                    .SetSubtitle("Biometrics Confirm")
                    // BiometricManager.Authenticators.DeviceCredential == Fallback on secure pin
                    .SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricStrong)
                    // Do not set NegativeButtonText if BiometricManager.Authenticators.DeviceCredential is allowed with BiometricManager.Authenticators.BiometricStrong
                    .SetNegativeButtonText("Cancel")
                    .Build();
            }

            BiometryService = new BiometryService.BiometryService(
                MainActivity.Instance,
                promptBuilder
            );
#endif
            BiometricoLoginCommand = new AsyncRelayCommand(BiometricoLoginCommandTask);
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
                if (settings.PreferWindowsHello == Services.Enums.BiometricPreferEnum.None)
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
                            settings.PreferWindowsHello = Services.Enums.BiometricPreferEnum.Prefer;
                            WindowsHelloLoginCommandTask();
                            break;
                        case ContentDialogResult.Secondary:
                            settings.PreferWindowsHello = Services.Enums.BiometricPreferEnum.No;
                            break;
                        default:
                            break;
                    }
                }
                else if (settings.PreferWindowsHello == Services.Enums.BiometricPreferEnum.Prefer)
                {
                    if (!IsLogout)
                    {
                        WindowsHelloLoginCommandTask();
                    }
                }
            }
        }


        /// <summary>
        /// Verify login with Windows Hello
        /// </summary>
        private async void WindowsHelloLoginCommandTask()
        {
            UserConsentVerificationResult consentResult = await UserConsentVerifier.RequestVerificationAsync(Resources.WindowsHelloLoginMessage);
            if (consentResult == UserConsentVerificationResult.Verified)
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


#if ANDROID || IOS


        private async Task BiometricoLoginCommandTask()
        {
            try
            {
                await BiometryService.ScanBiometry(_cancellationToken);
                // Authentication Passed
                var dbHash = await App.Repository.Password.GetAsync();
                var secretService = App.Current.Container.Resolve<ISecretService>();
                if (!await CheckNavigationRequest(secretService.Helper.ReadSecret(Constants.ContainerName, dbHash.Hash)))
                {
                    await ShowLoginError();
                }

            }
            catch (BiometryException biometryException)
            {
                // TxtAuthenticationStatus.Text = ParseBiometryException(biometryException);
            }
            catch (Exception exc)
            {

            }
           

        }
        /// <summary>
        /// Checks and starts biometric login, if possible and desired
        /// </summary>
        public async Task CheckCapabilityBiometricLogin()
        {
            try
            {
                // TODO change from Windows Hello!
                var capabilities = await BiometryService.GetCapabilities(_cancellationToken);
                if (capabilities.IsSupported && capabilities.IsEnabled)
                {
                    BiometricIsUsable = SettingsService.Instance.ActivateWindowsHello;
                    var settings = SettingsService.Instance;
                    if (settings.PreferBiometricLogin == Services.Enums.BiometricPreferEnum.None)
                    {
                        var dialog = new ContentDialog();
                        var markdown = new TextBlock
                        {
                            Text = Resources.WindowsHelloPreferMessage,
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.WrapWholeWords
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
                                settings.PreferBiometricLogin = Services.Enums.BiometricPreferEnum.Prefer;
                                BiometricoLoginCommandTask();
                                break;
                            case ContentDialogResult.Secondary:
                                settings.PreferBiometricLogin = Services.Enums.BiometricPreferEnum.No;
                                break;
                            default:
                                break;
                        }
                    }
                    else if (settings.PreferBiometricLogin == Services.Enums.BiometricPreferEnum.Prefer)
                    {
                        if (!IsLogout)
                        {
                            BiometricoLoginCommandTask();
                        }
                    }
                }
            }
            catch (Exception exc)
            {

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
