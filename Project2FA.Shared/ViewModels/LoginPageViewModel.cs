using CommunityToolkit.Mvvm.Input;
using UNOversal.Ioc;
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
using Project2FA.Utils;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using UNOversal.Navigation;
using UNOversal.Services.Logging;

#if WINDOWS_UWP && NET10_0_OR_GREATER
using WinRT;
#endif


#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using WinUIWindow = Windows.UI.Xaml.Window;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
using Windows.UI.Xaml;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
using WinUIWindow = Microsoft.UI.Xaml.Window;
#endif

#if IOS
using UIKit;
using LocalAuthentication;
using BiometryService;
#endif

#if ANDROID
using AndroidX.Biometric;
using BiometryService;
using AndroidX.Lifecycle;
using Project2FA.Uno;
using Project2FA.Uno.Droid;
#endif

namespace Project2FA.ViewModels
{
    /// <summary>
    /// View model for the login page
    /// </summary>
#if !WINDOWS_UWP
    [Bindable]
#endif

#if WINDOWS_UWP
    public partial class LoginPageViewModel : CredentialViewModelBase
#else
    public partial class LoginPageViewModel : CredentialViewModelBase, IInitialize
#endif
    {
        private ILoggingService LoggingService { get; }
#if ANDROID || IOS
        private IBiometryService BiometryService { get; }
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
#endif
        /// <summary>
        /// Constructor
        /// </summary>
        public LoginPageViewModel()
        {
            DialogService = App.Current.Container.Resolve<IDialogService>();
            LoggingService = App.Current.Container.Resolve<LoggingService>();
            LoginCommand = new RelayCommand(CheckLogin);
#if WINDOWS_UWP
            WindowsHelloLoginCommand = new RelayCommand(WindowsHelloLoginCommandTask);
#endif

#if IOS
			var laContext = new LAContext
			{
				LocalizedReason = Resources.LoginPageIOSBiometricReason,
				LocalizedFallbackTitle = "FALLBACK",
				LocalizedCancelTitle = Resources.ButtonTextCancel
			};

            BiometryService = new BiometryService.BiometryService(
				"Biometrics_Confirm",
				laContext,
				LAPolicy.DeviceOwnerAuthentication);
#endif

            //Note that not all combinations of authenticator types are supported prior to Android 11 (API 30). Specifically, DEVICE_CREDENTIAL alone is unsupported prior to API 30, and BIOMETRIC_STRONG | DEVICE_CREDENTIAL is unsupported on API 28-29
#if ANDROID
            Func<BiometricPrompt.PromptInfo> promptBuilder;
            if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.Q)
            {
                promptBuilder = () => new BiometricPrompt.PromptInfo.Builder()
                    .SetTitle(Strings.Resources.BiometricLoginTitle)
                    .SetSubtitle(Strings.Resources.BiometricLoginSubtitle)
                    //.SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricWeak | BiometricManager.Authenticators.DeviceCredential) // Fallback on secure pin WARNING cannot Encrypt data with this settings
                    .SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricStrong) // used for Encrypt decrypt feature for device bellow Android 11
                    .SetNegativeButtonText(Strings.Resources.ButtonTextCancel)
                    .Build();
            }
            else
            {
                promptBuilder = () => new BiometricPrompt.PromptInfo.Builder()
                    .SetTitle(Strings.Resources.BiometricLoginTitle)
                    .SetSubtitle(Strings.Resources.BiometricLoginSubtitle)
                    // BiometricManager.Authenticators.DeviceCredential == Fallback on secure pin
                    .SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricStrong)
                    // Do not set NegativeButtonText if BiometricManager.Authenticators.DeviceCredential is allowed with BiometricManager.Authenticators.BiometricStrong
                    .SetNegativeButtonText(Strings.Resources.ButtonTextCancel)
                    .Build();
            }

            BiometryService = new BiometryService.BiometryService(
                MainActivity.Instance,
                promptBuilder
            );
#endif
#if ANDROID || IOS
            App.ShellPageInstance.ViewModel.TabBarIsVisible = false;
            BiometricoLoginCommand = new AsyncRelayCommand(BiometricoLoginCommandTask);
#endif



            var title = Strings.Resources.ApplicationName;
            ApplicationTitle = System.Diagnostics.Debugger.IsAttached ? "[Debug] " + title : title;
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
                else if (WindowsHelloIsUsable && settings.PreferWindowsHello == Services.Enums.BiometricPreferEnum.Prefer)
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
        #if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Style))]
        #endif
        private async void WindowsHelloLoginCommandTask()
        {
            try
            {
                UserConsentVerificationResult consentResult = await UserConsentVerifier.RequestVerificationAsync(Resources.WindowsHelloLoginMessage);
                if (consentResult == UserConsentVerificationResult.Verified)
                {
                    var secretService = App.Current.Container.Resolve<ISecretService>();

                    if (!await CheckNavigationRequest(secretService.Helper.ReadSecret(Constants.ContainerName, SettingsService.Instance.DataFilePasswordHash)))
                    {
                        await ShowLoginError();
                    }
                }
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                TrackingManager.TrackExceptionCatched(nameof(WindowsHelloLoginCommandTask), exc);
                var dialog = new ContentDialog
                {
                    Title = Resources.ErrorHandle
                };
                var textBlock = new TextBlock();
                textBlock.Text = Resources.LoginPageWindowsHelloError;
                dialog.Content = textBlock;
                dialog.Style = App.Current.Resources[Constants.ContentDialogStyleName] as Style;
                dialog.PrimaryButtonText = Resources.Confirm;
                dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;

                await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
            }
        }
#endif


#if ANDROID || IOS
        private async Task BiometricoLoginCommandTask()
        {
            try
            {
                IsLoading = true;
                await BiometryService.ScanBiometry(_cancellationToken);
                // Authentication Passed
                var secretService = App.Current.Container.Resolve<ISecretService>();
                if (!await CheckNavigationRequest(secretService.Helper.ReadSecret(Constants.ContainerName, SettingsService.Instance.DataFilePasswordHash)))
                {
                    await ShowLoginError();
                    IsLoading = false;
                }
            }
            catch (BiometryException biometryException)
            {
                IsLoading = false;
                await LoggingService.LogException(biometryException, SettingsService.Instance.LoggingSetting);
            }
            catch (Exception exc)
            {
                IsLoading = false;
                await LoggingService.LogException(exc,SettingsService.Instance.LoggingSetting);
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
                bool isFingerprintReader = capabilities.BiometryType == BiometryType.Fingerprint ? true : false;

                if (capabilities.IsSupported && capabilities.IsEnabled)
                {
                    BiometricIsUsable = SettingsService.Instance.ActivateWindowsHello;
                    var settings = SettingsService.Instance;
                    if (settings.PreferBiometricLogin == Services.Enums.BiometricPreferEnum.None)
                    {
                        var dialog = new ContentDialog();
                        var markdown = new TextBlock
                        {
                            Text = isFingerprintReader ? Resources.BiometricFingerPreferMessage : Resources.BiometricFacePreferMessage,
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.WrapWholeWords
                        };
                        dialog.XamlRoot = App.ShellPageInstance.XamlRoot;
                        dialog.Content = markdown;
                        dialog.PrimaryButtonText = Resources.Yes;
                        dialog.SecondaryButtonText = Resources.No;
                        await Task.Delay(1500);
                        var result = await DialogService.ShowDialogAsync(dialog, new DialogParameters());
                        switch (result)
                        {
                            case ContentDialogResult.None:
                                break;
                            case ContentDialogResult.Primary:
                                settings.PreferBiometricLogin = Services.Enums.BiometricPreferEnum.Prefer;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                BiometricoLoginCommandTask();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            BiometricoLoginCommandTask();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc,SettingsService.Instance.LoggingSetting);
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
            string pwdhash = CryptoService.CreateStringHash(password);
            if (!string.IsNullOrWhiteSpace(SettingsService.Instance.DataFilePasswordHash) && SettingsService.Instance.DataFilePasswordHash == pwdhash)
            {
                //var navigationParameters = new NavigationParameters();
                //navigationParameters.Add("pwd", password);
#if WINDOWS_UWP
                App.ShellPageInstance.SetTitleBarAsDraggable();
                WinUIWindow.Current.Content = App.ShellPageInstance;
#endif

                await App.ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
                WinUIWindow.Current.Activate();

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Initialize(INavigationParameters parameters)
        {
            IsLoading = false;
            if (parameters.TryGetValue<bool>("isLogout", out var isLogout))
            {
                IsLogout = isLogout;
            }
#if ANDROID || IOS
            CheckCapabilityBiometricLogin();
#endif

        }
    }
}
