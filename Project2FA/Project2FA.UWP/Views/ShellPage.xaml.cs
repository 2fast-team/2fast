using Microsoft.Toolkit.Uwp.Helpers;
using Project2FA.Services;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible;
using System.Collections.Generic;
using System.Linq;
using Prism.Ioc;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;
using Project2FA.Services.Enums;
using UNOversal.Navigation;
using Project2FA.Utils;
using UNOversal.Services.Dialogs;
using Project2FA.ViewModels;
using Windows.System;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class ShellPage : Page
    {
        private SystemNavigationManager _navManager;

        public INavigationService NavigationService { get; private set; }
        public NavigationView ShellViewInternal { get; private set; }
        public Frame MainFrame { get; }
        public ShellPageViewModel ViewModel { get; } = new ShellPageViewModel();
        private CoreApplicationViewTitleBar coreTitleBar;

        public ShellPage()
        {
            InitializeComponent();
            _navManager = SystemNavigationManager.GetForCurrentView();
            _settingsNavigationStr = "SettingPage?PivotItem=0";

            string title = Windows.ApplicationModel.Package.Current.DisplayName;
            // determine and set if the app is started in debug mode
            ViewModel.Title = System.Diagnostics.Debugger.IsAttached ? "[Debug] " + title : title;

            coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
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
            GestureService.SetupWindowListeners(Window.Current.CoreWindow);
            NavigationService = NavigationFactory.Create(MainFrame).AttachGestures(Window.Current, Gesture.Back, Gesture.Forward);

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

            MainFrame.Navigated += async(s, e) =>
            {
                if (TryFindItem(e.SourcePageType, e.Parameter, out object item))
                {
                    await SetSelectedItem(item, false);
                }
            };

            ShellView.ItemInvoked += async (sender, args) =>
            {
                if (args.IsSettingsInvoked)
                {
                    await SetSelectedItem(ShellView.SettingsItem);
                }
                else
                {
                    await SetSelectedItem(Find(args.InvokedItemContainer as NavigationViewItem));
                }
            };

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = ViewModel.IsScreenCaptureEnabled = true;
            }
            else
            {
                //prevent screenshot capture for the app
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = ViewModel.IsScreenCaptureEnabled = false;
            }
            Loaded += ShellPage_Loaded;
        }

        private async void ShellPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!SettingsService.Instance.UseRoundCorner)
            {
                App.Current.Resources["ControlCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["ComboBoxItemCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["ComboBoxItemPillCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["TokenItemCornerRadius"] = new CornerRadius(0);
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
                    await dialogService.ShowDialogAsync(dialog, new DialogParameters());
                }
            }

            if (SystemInformation.Instance.IsAppUpdated)
            {
                //if (SystemInformation.Instance.PreviousVersionInstalled.Equals(PackageVersionHelper.ToPackageVersion("1.0.5.0")))
                //{
                //    if (SystemInformation.Instance.OperatingSystemVersion.Build >= 22000)
                //    {
                //        // set the round corner for Windows 11+
                //        SettingsService.Instance.UseRoundCorner = true;
                //    }
                //}

                ContentDialog dialog = new ContentDialog();
                dialog.Title = Strings.Resources.NewAppFeaturesTitle;
                var markdown = new MarkdownTextBlock();
                markdown.Text = Strings.Resources.NewAppFeaturesContent;
                dialog.Content = markdown;
                dialog.PrimaryButtonText = Strings.Resources.Confirm;
                dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                await dialogService.ShowDialogAsync(dialog, new DialogParameters());
            }

            // If this is the first run, activate the ntp server checks
            // else => UseNTPServerCorrection is false
            if (SystemInformation.Instance.IsFirstRun)
            {
                if (SystemInformation.Instance.OperatingSystemVersion.Build >= 22000)
                {
                    // set the round corner for Windows 11+
                    SettingsService.Instance.UseRoundCorner = true;
                }
            }


            // ChangeBackgroundColorSetting(false);

        }

        public void SetTitleBarAsDraggable()
        {
            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);
        }

        //public void ChangeBackgroundColorSetting(bool isMicaStyle)
        //{
        //    if (isMicaStyle)
        //    {
        //        var control = (ContentPresenter)ShellView.FindDescendant("ContentPresenterFrame");
        //        control.Background = (AcrylicBrush)App.Current.Resources["ShellAcrylicWindowBrush"];
        //    }
        //    else
        //    {
        //        var control = (ContentPresenter)ShellView.FindDescendant("ContentPresenterFrame");
        //        //control.Background = (SolidColorBrush)App.Current.Resources["SystemColorBackgroundThemeBrush"];
        //    }
        //}

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
            Uri uri = new Uri("https://github.com/2fast-team/2fast/discussions");
            await Launcher.LaunchUriAsync(uri);
            // https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
            //Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            //await launcher.LaunchAsync();
        }

        private string _settingsNavigationStr;

        private object PreviousItem { get; set; }

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
    }
}
