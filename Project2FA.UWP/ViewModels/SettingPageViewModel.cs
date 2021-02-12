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
using System.Collections.ObjectModel;
using Project2FA.UWP.Utils;
using Template10.Utilities;
using System.Linq;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Collections;

namespace Project2FA.UWP.ViewModels
{
    /// <summary>
    /// Navigation part from the settings page
    /// </summary>
    public class SettingPageViewModel : BindableBase, IInitialize
    {
        public SettingsPartViewModel SettingsPartViewModel { get; }
        public AboutPartViewModel AboutPartViewModel { get; }
        public DatafilePartViewModel DatafilePartViewModel { get; }

        private int _selectedItem;
        public SettingPageViewModel(IDialogService dialogService, IMarketplaceService marketplaceService)
        {
            SettingsPartViewModel = new SettingsPartViewModel(dialogService);
            DatafilePartViewModel = new DatafilePartViewModel(dialogService);
            AboutPartViewModel  = new AboutPartViewModel(marketplaceService);
        }

        public void Initialize(INavigationParameters parameters)
        {
            int selectedItem;
            if (parameters.TryGetValue<int>("PivotItem", out selectedItem))
            {
                SelectedItem = selectedItem;
            }
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
        private bool _isWindowsHelloActive;
        public ICommand MakeFactoryResetCommand { get; }

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
        }

        /// <summary>
        /// Make a factory reset of the app
        /// </summary>
        private async void MakeFactoryReset()
        {
            var dialog = new ContentDialog();
            dialog.Title = Resources.SettingsFactoryResetDialogTitle;
            var markdown = new MarkdownTextBlock();
            markdown.Text = Resources.SettingsFactoryResetMessage;
            dialog.Content = markdown;
            dialog.PrimaryButtonText = Resources.No;
            dialog.SecondaryButtonText = Resources.Yes;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            var result = await _dialogService.ShowAsync(dialog);

            switch (result)
            {
                case ContentDialogResult.Secondary:
                    var passwordHash = await App.Repository.Password.GetAsync();
                    //delete password in the secret vault
                    App.Current.Container.Resolve<ISecretService>().Helper.RemoveSecret(passwordHash.Hash);
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

        public bool PreferWindowsHello
        {
            get
            {
                switch (_settings.PreferWindowsHello)
                {
                    case WindowsHelloPreferEnum.None:
                        IsWindowsHelloActive = false;
                        return false;
                    case WindowsHelloPreferEnum.No:
                        IsWindowsHelloActive = true;
                        return false;
                    case WindowsHelloPreferEnum.Prefer:
                        IsWindowsHelloActive = true;
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
            }
        }

        public bool UseHeaderBackButton
        {
            get { return _settings.UseHeaderBackButton; }
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

        public bool IsTitleBarSupported
        {
            get
            {
                return Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationViewTitleBar");
            }
        }

        public bool IsWindowsHelloActive
        {
            get => _isWindowsHelloActive;
            set => SetProperty(ref _isWindowsHelloActive, value);
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
        
        ///// <summary>
        ///// Deletes the current datafile
        ///// </summary>
        //private async void DeleteDatafile()
        //{
        //    // TODO dialog (security prompt) for the reset
        //    var dialog = new ContentDialog();
        //    dialog.Content = Resources.SettingsRemoveDatafileMessage;
        //    dialog.PrimaryButtonText = Resources.No;
        //    dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
        //    dialog.SecondaryButtonText = Resources.Yes;
        //    var result = await _dialogService.ShowAsync(dialog);

        //    switch (result)
        //    {
        //        case ContentDialogResult.Secondary:
        //            // delete datafile from storage and local file DB
        //            // TODO: Implement database deletion
        //            // TODO delete password from secret vault
        //            await App.Repository.Datafile.DeleteAsync();
        //            await App.Repository.Password.DeleteAsync();
        //            DataService.Instance.DeleteLocalDatafile();
        //            break;
        //        case ContentDialogResult.Primary:
        //            break;
        //        case ContentDialogResult.None:
        //            break;
        //        default:
        //            break;
        //    }
        //}

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

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName + " - two factor authenticator supporting TOTP";

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
