using System;
using Windows.Storage;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Project2FA.Strings;
using Prism.Ioc;
using Project2FA.Repository.Models;
using System.Linq;
using Windows.Security.Credentials;
using Project2FA.Core.Services.NTP;
using System.Threading.Tasks;
using Project2FA.Core;
using Windows.ApplicationModel.Email;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Navigation;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.Utils;
using CommunityToolkit.Mvvm.Collections;
using Windows.System;


#if WINDOWS_UWP
using Project2FA.UWP.Services;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Project2FA.Services.Marketplace;
using Microsoft.Toolkit.Uwp.UI.Controls;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Project2FA.UNO.Views;
using Project2FA.UNO;
#endif

namespace Project2FA.ViewModels
{
    /// <summary>
    /// Navigation part from the settings page
    /// </summary>
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class SettingPageViewModel : ObservableObject, IInitialize, IConfirmNavigationAsync
    {
        public SettingsPartViewModel SettingsPartViewModel { get; }
        public AboutPartViewModel AboutPartViewModel { get; }
        public DatafilePartViewModel DatafilePartViewModel { get; }
        public INavigationService NavigationService { get; }

        public ICommand RateAppCommand { get; }
        public ICommand SendMailCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand InAppPaymentCommand { get; }
        public ICommand SeeSourceCodeCommand { get; }
        public ICommand ManageSubscriptionsCommand { get; }

        private int _selectedItem;
        public SettingPageViewModel(IDialogService dialogService, ISecretService secretService, INavigationService navigationService)
        {
#if WINDOWS_UWP
            IMarketplaceService marketplaceService = App.Current.Container.Resolve<IMarketplaceService>();
#endif
            SettingsPartViewModel = new SettingsPartViewModel(dialogService);
            DatafilePartViewModel = new DatafilePartViewModel(dialogService, secretService);
#if WINDOWS_UWP
            AboutPartViewModel  = new AboutPartViewModel(marketplaceService);
#else
            AboutPartViewModel = new AboutPartViewModel();
#endif
            RateAppCommand = new RelayCommand(() =>
            {
                AboutPartViewModel.RateApp();
            });

            SeeSourceCodeCommand = new AsyncRelayCommand(async() => 
            {
                Uri uri = new Uri("https://github.com/2fast-team/2fast");
                await Launcher.LaunchUriAsync(uri);
            });

#if WINDOWS_UWP
            InAppPaymentCommand = new AsyncRelayCommand(async() =>
            {
                IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
                var inAppPaymentDialog = new InAppPaymentContentDialog();
                var result = await dialogService.ShowDialogAsync(inAppPaymentDialog, new DialogParameters());
                if (result == ContentDialogResult.Primary)
                {
                    var service = App.Current.Container.Resolve<IPurchaseAddOnService>();
                    var selectedPurchaseItem = inAppPaymentDialog.ViewModel.Items.Where(x => x.IsChecked == true).FirstOrDefault();
                    service.Initialize(selectedPurchaseItem.StoreId);
                    await service.SetupPurchaseAddOnInfoAsync();
                    await service.PromptUserToPurchaseAsync();
                }
            });

            ManageSubscriptionsCommand = new AsyncRelayCommand(async() => 
            {
                Uri uri = new Uri("https://account.microsoft.com/services");
                await Launcher.LaunchUriAsync(uri);
            });
#endif
            NavigationService = navigationService;
            NavigateBackCommand = new AsyncRelayCommand(NavigateBackCommandTask);
            //SendMailCommand = new AsyncRelayCommand(SendMail);
        }

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.TryGetValue<int>("PivotItem", out int selectedItem))
            {
                SelectedItem = selectedItem;
            }

            if (parameters.TryGetValue<bool>("OpenInAppPayments", out bool openInAppPayments))
            {
                if (openInAppPayments)
                {
                    InAppPaymentCommand.Execute(null);
                }
            }
        }

        private async Task NavigateBackCommandTask()
        {
            await NavigationService.GoBackAsync();
        }

        //private async Task SendMail()
        //{
        //    EmailMessage emailMessage = new EmailMessage();
        //    emailMessage.To.Add(new EmailRecipient("app-2fast@outlook.com"));
        //    emailMessage.Subject = "Support 2fast";

        //    await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        //}

        public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            return !await dialogService.IsDialogRunning();
        }

        public int SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }
        
    }

    /// <summary>
    /// Main content part of the settings page
    /// </summary>

#if !WINDOWS_UWP
    [Bindable]
#endif
    public class SettingsPartViewModel : ObservableObject
    {
        private SettingsService _settings;
        private IDialogService _dialogService { get; }
        private bool _isWindowsHelloSupported;
        private bool _manualNTPServerConfiurationChecked;
        private bool _progressIsIndeterminate;
        private string _ntpServerStr;
        private bool _ntpServerEditValid;
        private bool _ntpServerEditException;

        public ICommand MakeFactoryResetCommand { get; }
        public ICommand SaveNTPServerAddressCommand { get; }

        /// <summary>
        /// View Model constructor
        /// </summary>
        /// <param name="resourceService"></param>
        /// <param name="dialogService"></param>
        public SettingsPartViewModel(IDialogService dialogService)
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                _settings = SettingsService.Instance;
            _dialogService = dialogService;

            MakeFactoryResetCommand = new RelayCommand(MakeFactoryReset);
            SaveNTPServerAddressCommand = new RelayCommand(() =>
            {
                SettingsService.Instance.NTPServerString = _ntpServerStr;
                NtpServerEditValid = false;
            });

#if WINDOWS_UWP
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CheckWindowsHelloIsSupported();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#else
            App.ShellPageInstance.ViewModel.TabBarIsVisible = false;
#endif
        }

        /// <summary>
        /// Make a factory reset of the app
        /// </summary>
        private async void MakeFactoryReset()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.SettingsFactoryResetDialogTitle;
#if WINDOWS_UWP
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = Resources.SettingsFactoryResetMessage;
            dialog.Content = markdown;
#else
            TextBlock txt = new TextBlock();
            txt.Text = Resources.SettingsFactoryResetMessage;
            dialog.Content = txt;
#endif
            dialog.PrimaryButtonText = Resources.No;
            dialog.SecondaryButtonText = Resources.Yes;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            ContentDialogResult result = await _dialogService.ShowDialogAsync(dialog, new DialogParameters());

            switch (result)
            {
                case ContentDialogResult.Secondary:
                    DBPasswordHashModel passwordHash = await App.Repository.Password.GetAsync();
                    var secretHelper = App.Current.Container.Resolve<ISecretService>();
                    //delete password in the secret vault
                    secretHelper.Helper.RemoveSecret(passwordHash.Hash);
                    //remove WebDAV login
                    secretHelper.Helper.RemoveSecret("WDServerAddress");
                    secretHelper.Helper.RemoveSecret("WDUsername");
                    secretHelper.Helper.RemoveSecret("WDPassword");
                    secretHelper.Helper.RemoveSecret(Constants.ActivatedDatafileHashName);

                    // reset data and restart app
#if WINDOWS_UWP
                    await ApplicationData.Current.ClearAsync();
                    await CoreApplication.RequestRestartAsync("Factory reset");
#endif
#if __IOS__
                    App.Current.Exit();
#endif

#if ANDROID
                    App.Current.Exit();
#endif
                    break;
                case ContentDialogResult.Primary:
                    break;
                case ContentDialogResult.None:
                    break;
                default:
                    break;
            }
        }

#region Theme
        public Theme Theme
        {
            get => _settings.AppTheme;
            set
            {
                if (_settings.AppTheme.Equals(value))
                {
                    return;
                }

                _settings.AppTheme = value;
//#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//                DataService.Instance.ReloadAccountIconSVGs();
//#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public bool ThemeAsLight
        {
            get => Theme.Equals(Theme.Light);
            set
            {
                if (value)
                {
                    Theme = Theme.Light;
                }
            }
        }

        public bool ThemeAsDark
        {
            get => Theme.Equals(Theme.Dark);
            set
            {
                if (value)
                {
                    Theme = Theme.Dark;
                }
            }
        }

        public bool ThemeAsSystem
        {
            get => Theme.Equals(Theme.System);
            set
            {
                if (value)
                {
                    Theme = Theme.System;
                }
            }
        }
#endregion

        public bool IsWindowsHelloSupported
        {
            get => _isWindowsHelloSupported;
            set => SetProperty(ref _isWindowsHelloSupported, value);
        }

        public bool ActivateWindowsHello
        {
            get => _settings.ActivateWindowsHello;
            set
            {
                _settings.ActivateWindowsHello = value;
                OnPropertyChanged(nameof(ActivateWindowsHello));

            }
        }

        public bool UseHiddenTOTP
        {
            get => _settings.UseHiddenTOTP;
            set
            {
                _settings.UseHiddenTOTP = value;
                OnPropertyChanged(nameof(UseHiddenTOTP));
            }
        }

        public bool PrideMonthDesign
        {
            get => _settings.PrideMonthDesign;
            set
            {
                _settings.PrideMonthDesign = value;
                OnPropertyChanged(nameof(PrideMonthDesign));
            }
        }

        public string NTPServerStr
        {
            get => _settings.NTPServerString;
            set
            {
                if (value != _settings.NTPServerString)
                {
                    CheckNTPServer(value);
                }
                else
                {
                    NtpServerEditValid = false;
                    NtpServerEditException = false;
                }
            }
        }
        private async Task CheckNTPServer(string newAddress)
        {
            ProgressIsIndeterminate = true;
            try
            {
                NtpServerEditException = false;
                await App.Current.Container.Resolve<INetworkTimeService>().GetNetworkTimeAsync(newAddress);
            }
            catch (Exception)
            {
                NtpServerEditException = true;
            }
            finally
            {
                if (!NtpServerEditException)
                {
                    NtpServerEditValid = true;
                    _ntpServerStr = newAddress;
                }
                else
                {
                    NtpServerEditValid = false;
                }
                ProgressIsIndeterminate = false;
            }
        }

        public bool ProgressIsIndeterminate
        {
            get => _progressIsIndeterminate;

            set => SetProperty(ref _progressIsIndeterminate, value);
        }

        public bool PreferWindowsHelloLogin
        {
            get
            {
                switch (_settings.PreferWindowsHello)
                {
                    case BiometricPreferEnum.None:
                    case BiometricPreferEnum.No:
                        return false;
                    case BiometricPreferEnum.Prefer:
                        return true;
                    default:
                        return false;
                }
            }
            set
            {
                _settings.PreferWindowsHello = value ? BiometricPreferEnum.Prefer : BiometricPreferEnum.No;
                OnPropertyChanged(nameof(PreferWindowsHelloLogin));
            }
        }


#if WINDOWS_UWP
        private async Task CheckWindowsHelloIsSupported()
        {
            IsWindowsHelloSupported = await KeyCredentialManager.IsSupportedAsync();
            if (!IsWindowsHelloSupported)
            {
                ActivateWindowsHello = false;
            }
        }
#endif

        public bool UseHeaderBackButton
        {
            get => _settings.UseHeaderBackButton;
            set
            {
                if (_settings.UseHeaderBackButton != value)
                {
                    _settings.UseHeaderBackButton = value;
                    OnPropertyChanged(nameof(UseHeaderBackButton));
#if WINDOWS_UWP
                    App.ShellPageInstance.SetupBackButton();
#endif
                }
            }
        }

        public bool UseRoundCorner
        {
            get => _settings.UseRoundCorner;
            set
            {
                if (_settings.UseRoundCorner != value)
                {
                    if (value)
                    {
                        App.Current.Resources["ControlCornerRadius"] = new CornerRadius(4, 4, 4, 4);
                        App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(8, 8, 8, 8);
                        App.Current.Resources["ComboBoxItemCornerRadius"] = new CornerRadius(3);
                        App.Current.Resources["ComboBoxItemPillCornerRadius"] = new CornerRadius(1.5);
                        App.Current.Resources["TokenItemCornerRadius"] = new CornerRadius(16,16,16,16);
                    }
                    else
                    {
                        App.Current.Resources["ControlCornerRadius"] = new CornerRadius(0);
                        App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(0);
                        App.Current.Resources["ComboBoxItemCornerRadius"] = new CornerRadius(0);
                        App.Current.Resources["ComboBoxItemPillCornerRadius"] = new CornerRadius(0);
                        App.Current.Resources["TokenItemCornerRadius"] = new CornerRadius(0);
                    }
                    _settings.UseRoundCorner = value;
                    OnPropertyChanged(nameof(UseRoundCorner));
                }
            }
        }

        public int ThemeIndex
        {
            get => _settings.AppTheme switch
            {
                Theme.Dark => 1,
                Theme.Light => 0,
                Theme.System => 2,
                _ => 2,
            };

            set
            {
                switch (value)
                {
                    case 0: ThemeAsLight = true; break;
                    case 1: ThemeAsDark = true; break;
                    case 2: ThemeAsSystem = true; break;
                }
            }
        }

        public int SetQRCodeScanSeconds
        {
            get => _settings.QRCodeScanSeconds;
            set
            {
                if (_settings.QRCodeScanSeconds != value)
                {
                    _settings.QRCodeScanSeconds = value;
                    OnPropertyChanged(nameof(SetQRCodeScanSeconds));
                }
            }
        }

        public int SetAutoLogoutMinutes
        {
            get => _settings.AutoLogoutMinutes;
            set
            {
                if (_settings.AutoLogoutMinutes != value)
                {
                    _settings.AutoLogoutMinutes = value;
                    OnPropertyChanged(nameof(SetAutoLogoutMinutes));
                }
            }
        }

        public bool ManualNTPServerConfiurationChecked 
        {
            get => _manualNTPServerConfiurationChecked;
            set => SetProperty(ref _manualNTPServerConfiurationChecked, value);
        }
        public bool NtpServerEditValid
        {
            get => _ntpServerEditValid;
            set => SetProperty(ref _ntpServerEditValid, value);
        }
        public bool NtpServerEditException 
        {
            get => _ntpServerEditException;
            set => SetProperty(ref _ntpServerEditException, value);
        }

        public bool UseNTPServerCorrection
        {
            get => _settings.UseNTPServerCorrection;
            set
            {
                _settings.UseNTPServerCorrection = value;
                if (!value)
                {
                    ManualNTPServerConfiurationChecked = false;
                }
                OnPropertyChanged(nameof(UseNTPServerCorrection));
            }
        }

        public bool UseAutoLogout
        {
            get => _settings.UseAutoLogout;
            set
            {
                _settings.UseAutoLogout = value;
                OnPropertyChanged(nameof(UseAutoLogout));
            }
        }
#if WINDOWS_UWP
        public bool IsProVersion 
        {
            get => _settings.IsProVersion;
        }

        public bool UseProFeatures
        {
            get => _settings.UseProFeatures;
            set
            {
                _settings.UseProFeatures = value;
                OnPropertyChanged(nameof(UseProFeatures));
            }
        }
#endif
    }


    /// <summary>
    /// Datafile tab from the settings page
    /// </summary>

#if !WINDOWS_UWP
    [Bindable]
#endif
    public class DatafilePartViewModel : ObservableObject
    {
        private IDialogService _dialogService { get; }

        public ICommand ChangeDatafilePasswordCommand { get; }
        //public ICommand EditDatafileCommand { get; }
        public ICommand DeleteDatafileCommand { get; }
        ISecretService SecretService { get; }

        private string _datafilePath;
        private string _datafileName;
        private bool _notifyPasswordChanged;

        /// <summary>
        /// Datafile part view model constructor
        /// </summary>
        public DatafilePartViewModel(IDialogService dialogService, ISecretService secretService)
        {
            _dialogService = dialogService;
            SecretService = secretService;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InitializeDataFileAttributes();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            // open content dialog to change the password
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            ChangeDatafilePasswordCommand = new RelayCommand(async() => {
                var dialog = new ChangeDatafilePasswordContentDialog();
                var param = new DialogParameters();
                param.Add("isInvalid", false);
                if (NotifyPasswordChanged)
                {
                    NotifyPasswordChanged = false;
                }
                var result = await _dialogService.ShowDialogAsync(dialog, param);
                if (result == ContentDialogResult.Primary)
                {
                    if (dialog.ViewModel.PasswordChanged)
                    {
                        NotifyPasswordChanged = true;
                    }
                }
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            //EditDatafileCommand = new DelegateCommand(EditDatafile);
        }

        /// <summary>
        /// Initialize the attributes for the datafile part
        /// </summary>
        /// <returns></returns>
        private async Task InitializeDataFileAttributes()
        {
            var dbDatafile = await App.Repository.Datafile.GetAsync();
            if (DataService.Instance.ActivatedDatafile != null)
            {
                DatafilePath = DataService.Instance.ActivatedDatafile.Path;
                DatafileName = DataService.Instance.ActivatedDatafile.Name;
            }
            else
            {
                DatafileName = dbDatafile.Name;
                DatafilePath = dbDatafile.IsWebDAV? SecretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress") + dbDatafile.Path : dbDatafile.Path;
            }
        }

        public string DatafilePath { get => _datafilePath; set => SetProperty(ref _datafilePath, value); }
        public string DatafileName { get => _datafileName; set => SetProperty(ref _datafileName, value); }
        public bool NotifyPasswordChanged { get => _notifyPasswordChanged; set => SetProperty(ref _notifyPasswordChanged, value); }
    }

    /// <summary>
    /// About tab from the settings page
    /// </summary>

#if !WINDOWS_UWP
    [Bindable]
#endif
    public class AboutPartViewModel : ObservableObject
    {
#if WINDOWS_UWP
        private IMarketplaceService _marketplaceService { get; }

        public AboutPartViewModel(IMarketplaceService marketplaceService)
        {
            _marketplaceService = marketplaceService;
            LoadDependencyList();
        }
#else
        public AboutPartViewModel()
        {
            LoadDependencyList();
        }
#endif

        private async void LoadDependencyList()
        {
            var depList = await new JSONUtil<DependencyModel>().GetJSONDataAsync(new Uri($"ms-appx:///Assets/JSONs/Dependencies.json"));
            foreach (var item in depList)
            {
                if (item.CategoryUid == "Package")
                {
                    item.Category = Resources.SettingsDependencyGroupPackages;
                }
                else
                {
                    item.Category = Resources.SettingsDependencyGroupAssets;
                }
            }
            var grouped = depList.GroupBy(x => x.Category).OrderBy(g => g.Key);
            var contactsSource = new ObservableGroupedCollection<string, DependencyModel>(grouped);
            DependencyCollectionViewSource.Source = contactsSource;
            DependencyCollectionViewSource.IsSourceGrouped = true;
        }
        public CollectionViewSource DependencyCollectionViewSource { get; } = new CollectionViewSource();
        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var ver = Windows.ApplicationModel.Package.Current.Id.Version;
                return ver.Major.ToString() + "." + ver.Minor.ToString() + "." + ver.Build.ToString() + "." + ver.Revision.ToString();
            }
        }
        public async void GiveFeedback()
        {
#if WINDOWS_UWP
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
#endif
        }

        public Task RateApp()
        {
#if WINDOWS_UWP
            return _marketplaceService.LaunchAppReviewInStoreAsync();
#else
            return Task.CompletedTask;
#endif
        }
    }
}
