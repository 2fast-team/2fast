using Microsoft.Toolkit.Uwp.Helpers;
using Project2FA.UWP.Services;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Prism.Services;
using System.Linq;
using Prism.Ioc;
using Template10.Services.Dialog;
using Windows.ApplicationModel.Core;
using Project2FA.UWP.Utils;
using Prism.Navigation;
using System.Threading.Tasks;
using Project2FA.UWP.Services.Enums;

namespace Project2FA.UWP.Views
{
    public sealed partial class ShellPage : Page, INotifyPropertyChanged
    {
        public delegate void ThemeChangedHandler(Theme oldTheme, Theme newTheme);
        private SystemNavigationManager _navManager;
        private bool _navigationIsAllowed = true;
        private string _title;
        public INavigationService NavigationService { get; private set; }
        public NavigationView ShellViewInternal { get; private set; }
        //private ThemeListener _listener;

        public ShellPage()
        {
            InitializeComponent();
            _navManager = SystemNavigationManager.GetForCurrentView();
            _settingsNavigationStr = "SettingPage?PivotItem=0";

            string title = Windows.ApplicationModel.Package.Current.DisplayName;
            // determine and set if the app is started in debug mode
            Title = System.Diagnostics.Debugger.IsAttached ? "[Debug] " + title : title;

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;

            SetTitleBarAsDraggable();

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            if (WindowDisplayInfo.GetForCurrentView() == WindowDisplayMode.FullScreenTabletMode)
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }

            ShellViewInternal = ShellView;
            ShellView.Content = MainFrame = new Frame();
            NavigationService = NavigationFactory.Create(MainFrame).AttachGestures(Window.Current, Gesture.Back, Gesture.Forward, Gesture.Refresh);

            SetupGestures();
            SetupBackButton();

            NavigationService.CanGoBackChanged += (s, args) =>
            {
                //Backbutton setting
                if (SettingsService.Instance.UseHeaderBackButton)
                {
                    _navManager.AppViewBackButtonVisibility = NavigationService.CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                }
                else
                {
                    if (ShellView.IsBackEnabled != NavigationService.CanGoBack())
                    {
                        ShellView.IsBackEnabled = NavigationService.CanGoBack();
                        if (ShellView.IsBackButtonVisible == NavigationViewBackButtonVisible.Collapsed)
                        {
                            ShellView.IsBackButtonVisible = NavigationViewBackButtonVisible.Auto;
                        }
                    }
                }
            };

            MainFrame.Navigated += (s, e) =>
            {
                if (TryFindItem(e.SourcePageType, e.Parameter, out object item))
                {
                    SetSelectedItem(item, false);
                }
            };

            ShellView.ItemInvoked += (sender, args) =>
            {
                SelectedItem = args.IsSettingsInvoked ? ShellView.SettingsItem : Find(args.InvokedItemContainer as NavigationViewItem);
            };

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = true;
            }
            else
            {
                //prevent screenshot capture for the app
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = false;
            }

            //_listener = new ThemeListener();
            //_listener.ThemeChanged += Listener_ThemeChanged;

            Loaded += ShellPage_Loaded;
        }

        //private void Listener_ThemeChanged(ThemeListener sender)
        //{
        //    ApplicationTheme theme = sender.CurrentTheme;
        //    switch (theme)
        //    {
        //        case ApplicationTheme.Light:
        //            SettingsService.Instance.AppTheme = Theme.Light;
        //            break;
        //        case ApplicationTheme.Dark:
        //            SettingsService.Instance.AppTheme = Theme.Dark;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        private async void ShellPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SettingsService.Instance.UnhandledExceptionStr))
            {
                await CheckUnhandledExceptionLastSession();
            }
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            //Rate information for the user
            if (SystemInformation.Instance.LaunchCount == 5 || SystemInformation.Instance.LaunchCount == 15)
            {
                if (!SettingsService.Instance.AppRated && (MainFrame.Content as FrameworkElement).GetType() != typeof(WelcomePage))
                {
                    var dialog = new RateAppContentDialog();
                    await dialogService.ShowAsync(dialog);
                }
            }

            // TODO add check for 1.0.5 to 1.0.6
            if (SystemInformation.Instance.IsAppUpdated)
            {
                if (SystemInformation.Instance.PreviousVersionInstalled.Equals(PackageVersionHelper.ToPackageVersion("1.0.5.0")))
                {
                    if (SystemInformation.Instance.OperatingSystemVersion.Build >= 22000)
                    {
                        // set the round corner for Windows 11+
                        SettingsService.Instance.UseRoundCorner = true;
                    }
                }

                ContentDialog dialog = new ContentDialog();
                dialog.Title = Strings.Resources.NewAppFeaturesTitle;
                dialog.Content = Strings.Resources.NewAppFeaturesContent;
                dialog.PrimaryButtonText = Strings.Resources.Confirm;
                dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                await dialogService.ShowAsync(dialog);
            }

            // If this is the first run, activate the ntp server checks
            // else => UseNTPServerCorrection is false
            if (SystemInformation.Instance.IsFirstRun)
            {
                SettingsService.Instance.UseNTPServerCorrection = true;
                if (SystemInformation.Instance.OperatingSystemVersion.Build >= 22000)
                {
                    // set the round corner for Windows 11+
                    SettingsService.Instance.UseRoundCorner = true;
                }
            }

            if (SettingsService.Instance.UseRoundCorner)
            {
                App.Current.Resources["ControlCornerRadius"] = new CornerRadius(4, 4, 4, 4);
                App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(8, 8, 8, 8);
            }
            else
            {
                App.Current.Resources["ControlCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(0);
            }
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (SettingsService.Instance.OriginalAppTheme == ApplicationTheme.Dark)
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case Theme.Dark:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    break;
                case Theme.Light:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    break;
                default:
                    break;
            }
        }

        public void SetTitleBarAsDraggable()
        {
            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            const int smallLeftIndent = 12; // largeLeftIndent = 24;

            AppTitle.TranslationTransition = new Vector3Transition();
            AppTitle.Translation = new System.Numerics.Vector3(smallLeftIndent + (float)coreTitleBar.SystemOverlayLeftInset, 0, 0);
            //LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            //RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset); 
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetupBackButton()
        {
            SettingsService settings = SettingsService.Instance;
            if (settings.UseHeaderBackButton)
            {
                ShellView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
                _navManager.AppViewBackButtonVisibility = NavigationService.CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
            else
            {
                _navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                ShellView.IsBackButtonVisible = NavigationViewBackButtonVisible.Auto;
                ShellView.IsBackEnabled = NavigationService.CanGoBack();
            }
        }

        private void SetupGestures()
        {
            _navManager.BackRequested += NavManager_BackRequested;
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            ShellView.BackRequested += async (s, e) => await NavigationService.GoBackAsync();
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
        }

        private Task CheckUnhandledExceptionLastSession()
        {
            return ErrorDialogs.ShowUnexpectedError(SettingsService.Instance.UnhandledExceptionStr);
        }

        private async void NavManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (NavigationService.CanGoBack())
            {
                e.Handled = true;
                await NavigationService.GoBackAsync();
            }
        }

        private async void FeedbackItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
            Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private string _settingsNavigationStr;

        private object PreviousItem { get; set; }

        private object SelectedItem
        {
            set => SetSelectedItem(value);
        }

        private async Task SetSelectedItem(object selectedItem, bool withNavigation = true)
        {
            if (selectedItem == null)
            {
                ShellView.SelectedItem = null;
            }
            else if (selectedItem == PreviousItem)
            {
                // already set
            }
            else if (selectedItem == ShellView.SettingsItem)
            {
                if (withNavigation)
                {
                    if ((await NavigationService.NavigateAsync(_settingsNavigationStr)).Success)
                    {
                        PreviousItem = selectedItem;
                        ShellView.SelectedItem = selectedItem;
                    }
                    else
                    {
                        ShellView.SelectedItem = null;
                    }
                }
                else
                {
                    PreviousItem = selectedItem;
                    ShellView.SelectedItem = selectedItem;
                }
            }
            else if (selectedItem is NavigationViewItem item)
            {
                if (item.Tag is string path)
                {
                    if (!withNavigation)
                    {
                        PreviousItem = item;
                        ShellView.SelectedItem = item;
                    }
                    else if ((await NavigationService.NavigateAsync(path)).Success)
                    {
                        PreviousItem = selectedItem;
                        ShellView.SelectedItem = selectedItem;
                    }
                    else
                    {
                        ShellView.SelectedItem = PreviousItem;
                    }
                }
            }
        }

        private bool TryFindItem(Type type, object parameter, out object item)
        {
            // is page registered?

            if (!PageNavigationRegistry.TryGetRegistration(type, out PageNavigationInfo info))
            {
                item = null;
                return false;
            }

            // search settings

            if (NavigationQueue.TryParse(_settingsNavigationStr, null, out NavigationQueue settings))
            {
                if (type == settings.Last().View && (string)parameter == settings.Last().QueryString)
                {
                    item = ShellView.SettingsItem;
                    return true;
                }
                else
                {
                    // not settings
                }
            }

            // filter menu items
            IEnumerable<(NavigationViewItem Item, string Path)> menuItems = ShellView.MenuItems
                .OfType<NavigationViewItem>()
                .Select(x => (
                    Item: x,
                    Path: x.Tag as string
                ))
                .Where(x => !string.IsNullOrEmpty(x.Path));

            // search filtered items

            foreach ((NavigationViewItem Item, string Path) in menuItems)
            {
                if (NavigationQueue.TryParse(Path, null, out NavigationQueue menuQueue)
                    && Equals(menuQueue.Last().View, type) && menuQueue.Last().QueryString == (string)parameter)
                {
                    item = Item;
                    return true;
                }
            }

            // filter footer menu items
            IEnumerable<(NavigationViewItem Item, string Path)> footerMenuItems = ShellView.FooterMenuItems
                .OfType<NavigationViewItem>()
                .Select(x => (
                    Item: x,
                    Path: x.Tag as string
                ))
                .Where(x => !string.IsNullOrEmpty(x.Path));

            // search filtered items

            foreach ((NavigationViewItem Item, string Path) in footerMenuItems)
            {
                if (NavigationQueue.TryParse(Path, null, out NavigationQueue menuQueue)
                    && Equals(menuQueue.Last().View, type) && menuQueue.Last().QueryString == (string)parameter)
                {
                    item = Item;
                    return true;
                }
            }

            // not found

            item = null;
            return false;
        }

        private NavigationViewItem Find(NavigationViewItem item)
        {
            NavigationViewItem menuItem = ShellView.MenuItems.OfType<NavigationViewItem>().SingleOrDefault(x => x.Equals(item) && x.Tag != null);
            if (menuItem is null)
            {
                menuItem = ShellView.FooterMenuItems.OfType<NavigationViewItem>().SingleOrDefault(x => x.Equals(item) && x.Tag != null);
            }
            return menuItem;
        }

        #region NotifyPropertyChanged
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        private bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        #endregion
        /// <summary>
        /// Allow or disable the NavigationView items
        /// </summary>
        public bool NavigationIsAllowed
        {
            get => _navigationIsAllowed;
            set => SetProperty(ref _navigationIsAllowed, value);
        }
        public Frame MainFrame { get; }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
    }
}
