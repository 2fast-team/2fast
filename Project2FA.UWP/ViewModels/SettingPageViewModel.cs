using Prism.Mvvm;
using System;
using Template10.Services.Dialog;
using Windows.UI.Xaml.Controls;
using Project2FA.UWP.Services.Enums;
using Windows.Storage;
using System.Windows.Input;
using Prism.Commands;
using Windows.ApplicationModel.Core;
using Project2FA.UWP.Strings;
using Project2FA.UWP.Services;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Prism.Ioc;
using Template10.Services.Secrets;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Prism.Navigation;
using Template10.Services.Marketplace;
using Project2FA.Repository.Models;
using Project2FA.UWP.Utils;
using System.Linq;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Collections;
using Windows.Security.Credentials;
using Project2FA.Core.Services.NTP;
using System.Threading.Tasks;

namespace Project2FA.UWP.ViewModels
{
    /// <summary>
    /// Navigation part from the settings page
    /// </summary>
    public class SettingPageViewModel : BindableBase, IInitialize, IConfirmNavigationAsync
    {
        public SettingsPartViewModel SettingsPartViewModel { get; }
        public AboutPartViewModel AboutPartViewModel { get; }
        public DatafilePartViewModel DatafilePartViewModel { get; }
        public ICommand RateAppCommand { get; }

        private int _selectedItem;
        public SettingPageViewModel(IDialogService dialogService, IMarketplaceService marketplaceService)
        {
            SettingsPartViewModel = new SettingsPartViewModel(dialogService);
            DatafilePartViewModel = new DatafilePartViewModel(dialogService);
            AboutPartViewModel  = new AboutPartViewModel(marketplaceService);
            RateAppCommand = new DelegateCommand(() =>
            {
                AboutPartViewModel.RateApp();
            });
        }

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.TryGetValue<int>("PivotItem", out int selectedItem))
            {
                SelectedItem = selectedItem;
            }
        }

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
    public class SettingsPartViewModel : BindableBase
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

            MakeFactoryResetCommand = new DelegateCommand(MakeFactoryReset);
            SaveNTPServerAddressCommand = new DelegateCommand(() =>
            {
                SettingsService.Instance.NTPServerString = _ntpServerStr;
                NtpServerEditValid = false;
            });

            CheckWindowsHelloIsSupported();
        }

        /// <summary>
        /// Make a factory reset of the app
        /// </summary>
        private async void MakeFactoryReset()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.SettingsFactoryResetDialogTitle;
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = Resources.SettingsFactoryResetMessage;
            dialog.Content = markdown;
            dialog.PrimaryButtonText = Resources.No;
            dialog.SecondaryButtonText = Resources.Yes;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            ContentDialogResult result = await _dialogService.ShowAsync(dialog);

            switch (result)
            {
                case ContentDialogResult.Secondary:
                    DBPasswordHashModel passwordHash = await App.Repository.Password.GetAsync();
                    //delete password in the secret vault
                    App.Current.Container.Resolve<ISecretService>().Helper.RemoveSecret(passwordHash.Hash);
                    //TODO remove WebDAV login
                    // reset data and restart app
                    await ApplicationData.Current.ClearAsync();
                    //PrismApplication.Current.
                    await CoreApplication.RequestRestartAsync("Factory reset");
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
        private async void CheckNTPServer(string newAddress)
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

        public bool IsWindowsHelloActive
        {
            get
            {
                switch (_settings.PreferWindowsHello)
                {
                    case WindowsHelloPreferEnum.None:
                    case WindowsHelloPreferEnum.No:
                        return false;
                    case WindowsHelloPreferEnum.Prefer:
                        return true;
                    default:
                        return false;
                }
            }
            set
            {
                if (value)
                {
                    _settings.PreferWindowsHello = WindowsHelloPreferEnum.Prefer;
                }
                else
                {
                    _settings.PreferWindowsHello = WindowsHelloPreferEnum.No;
                }
                RaisePropertyChanged(nameof(IsWindowsHelloActive));
            }
        }

        private async void CheckWindowsHelloIsSupported()
        {
            IsWindowsHelloSupported = await KeyCredentialManager.IsSupportedAsync();
        }

        public bool UseHeaderBackButton
        {
            get => _settings.UseHeaderBackButton;
            set
            {
                if (_settings.UseHeaderBackButton != value)
                {
                    _settings.UseHeaderBackButton = value;
                    RaisePropertyChanged(nameof(UseHeaderBackButton));
                    App.ShellPageInstance.SetupBackButton();
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
                    RaisePropertyChanged(nameof(SetQRCodeScanSeconds));
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
                RaisePropertyChanged(nameof(UseNTPServerCorrection));
            }
        }
    }

    /// <summary>
    /// Datafile tab from the settings page
    /// </summary>
    public class DatafilePartViewModel : BindableBase
    {
        private IDialogService _dialogService { get; }

        public ICommand ChangeDatafilePasswordCommand { get; }
        public ICommand EditDatafileCommand { get; }
        public ICommand DeleteDatafileCommand { get; }

        private string _datafilePath;
        private string _datafileName;
        private bool _notifyPasswordChanged;

        /// <summary>
        /// Datafile part view model constructor
        /// </summary>
        public DatafilePartViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            InitializeDataFileAttributes();

            // open content dialog to change the password
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            ChangeDatafilePasswordCommand = new DelegateCommand(async() => {
                var dialog = new ChangeDatafilePasswordContentDialog();
                if (NotifyPasswordChanged)
                {
                    NotifyPasswordChanged = false;
                }
                var result = await _dialogService.ShowAsync(dialog);
                if (result == ContentDialogResult.Primary)
                {
                    if (dialog.ViewModel.PasswordChanged)
                    {
                        NotifyPasswordChanged = true;
                    }
                }
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            EditDatafileCommand = new DelegateCommand(EditDatafile);
            //DeleteDatafileCommand = new DelegateCommand(DeleteDatafile);
        }

        /// <summary>
        /// Initialize the attributes for the datafile part
        /// </summary>
        /// <returns></returns>
        private async void InitializeDataFileAttributes()
        {
            var dbDatafile = await App.Repository.Datafile.GetAsync();

            DatafileName = dbDatafile.Name;
            DatafilePath = dbDatafile.Path;
        }
        
        /// <summary>
        /// Edit the current datafile
        /// </summary>
        public async void EditDatafile()
        {
            var dialog = new UpdateDatafileContentDialog();
            await _dialogService.ShowAsync(dialog);
        }

        public string DatafilePath { get => _datafilePath; set => SetProperty(ref _datafilePath, value); }
        public string DatafileName { get => _datafileName; set => SetProperty(ref _datafileName, value); }
        public bool NotifyPasswordChanged { get => _notifyPasswordChanged; set => SetProperty(ref _notifyPasswordChanged, value); }
    }

    /// <summary>
    /// About tab from the settings page
    /// </summary>
    public class AboutPartViewModel : BindableBase
    {
        private IMarketplaceService _marketplaceService { get; }
        public AboutPartViewModel(IMarketplaceService marketplaceService)
        {
            _marketplaceService = marketplaceService;
            LoadDependencyList();
        }

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
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        public async void RateApp()
        {
            await _marketplaceService.LaunchAppReviewInStoreAsync();
        }
    }
}
