﻿using Project2FA.Services;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible;
using System.Collections.Generic;
using System.Linq;
using UNOversal.Ioc;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;
using Project2FA.Services.Enums;
using UNOversal.Navigation;
using Project2FA.Utils;
using UNOversal.Services.Dialogs;
using Project2FA.ViewModels;
using Windows.System;
using Windows.Services.Store;
using Project2FA.UWP.Services;
using Project2FA.Core;
using UNOversal.Services.Network;
using UNOversal.Services.Gesture;
using UNOversal.Services.Logging;
using Windows.UI.Core.Preview;
using UNOversal.Helpers;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;

namespace Project2FA.UWP.Views
{
    public sealed partial class ShellPage : Page
    {
        private readonly SystemNavigationManager _navManager;
        public NavigationView ShellViewInternal { get; private set; }
        public Frame MainFrame { get; }
        public ShellPageViewModel ViewModel { get; } = new ShellPageViewModel();
        private readonly CoreApplicationViewTitleBar _coreTitleBar;

        public ShellPage()
        {
            InitializeComponent();
            _navManager = SystemNavigationManager.GetForCurrentView();
            _settingsNavigationStr = "SettingPage?PivotItem=0";

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += App_CloseRequested;

            // determine and set if the app is started in debug mode
            ViewModel.Title = System.Diagnostics.Debugger.IsAttached ? "[Debug] " + Strings.Resources.ApplicationName : Strings.Resources.ApplicationName;
            _coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            _coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;

            SetTitleBarAsDraggable();

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            _coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            if (WindowDisplayInfo.GetForCurrentView() == WindowDisplayMode.FullScreenTabletMode)
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }

            ShellViewInternal = ShellView;
            ShellView.Content = MainFrame = new Frame();
            GestureService.SetupWindowListeners(Window.Current.CoreWindow);
            ViewModel.NavigationService = NavigationFactory.Create(MainFrame).AttachGestures(Window.Current, Gesture.Back, Gesture.Forward);

            SetupGestures();
            SetupBackButton();

            ViewModel.NavigationService.CanGoBackChanged += (s, args) =>
            {
                //Backbutton setting
                if (SettingsService.Instance.UseHeaderBackButton)
                {
                    _navManager.AppViewBackButtonVisibility = ViewModel.NavigationService.CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                }
                else
                {
                    if (ShellView.IsBackEnabled != ViewModel.NavigationService.CanGoBack())
                    {
                        ShellView.IsBackEnabled = ViewModel.NavigationService.CanGoBack();
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
                SettingsService.Instance.IsScreenCaptureEnabled = true;
            }
            else
            {
                //prevent screenshot capture for the app
                SettingsService.Instance.IsScreenCaptureEnabled = false;
            }
            Loaded += ShellPage_Loaded;
        }

        private async void App_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var deferral = e.GetDeferral();
            if (DataService.Instance.CollectionAccessSemaphore.CurrentCount == 0)
            {
                e.Handled = true;
                var dialogService = App.Current.Container.Resolve<IDialogService>();
                var dialog = new ContentDialog();
                dialog.Title = Strings.Resources.CloseAppDialogTitle;
                dialog.Content = Strings.Resources.CloseAppDialogContent;
                dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                dialog.PrimaryButtonText = Strings.Resources.Confirm;
                await dialogService.ShowDialogAsync(dialog, new DialogParameters());
            }
            deferral.Complete();
        }

        private async void ShellPage_Loaded(object sender, RoutedEventArgs e)
        {
            // set the corner radius for the controls
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

            // open error dialog for last session
            if (!string.IsNullOrEmpty(SettingsService.Instance.UnhandledExceptionStr))
            {
                // always write the last exception string in log
                await App.Current.Container.Resolve<ILoggingService>().Log(SettingsService.Instance.UnhandledExceptionStr, LoggingPreferEnum.Full);
                await CheckUnhandledExceptionLastSession();
            }
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            //Rate information for the user
            if (SystemInformationHelper.Instance.LaunchCount == 5 || 
                SystemInformationHelper.Instance.LaunchCount == 15 || 
                SystemInformationHelper.Instance.LaunchCount == 45)
            {
                if (!SettingsService.Instance.AppRated && (MainFrame.Content as FrameworkElement).GetType() != typeof(WelcomePage))
                {
                    var dialog = new RateAppContentDialog();
                    await dialogService.ShowDialogAsync(dialog, new DialogParameters());
                }
            }

            if (SystemInformationHelper.Instance.IsAppUpdated && !DataService.Instance.NewAppUpdateDialogDisplayed)
            {
                //if (SystemInformationHelper.Instance.PreviousVersionInstalled.Equals(PackageVersionHelper.ToPackageVersion("1.0.5.0")))
                //{
                //    if (SystemInformationHelper.Instance.OperatingSystemVersion.Build >= 22000)
                //    {
                //        // set the round corner for Windows 11+
                //        SettingsService.Instance.UseRoundCorner = true;
                //    }
                //}

                if (SystemInformationHelper.Instance.PreviousVersionInstalled.Equals(PackageVersionHelper.ToPackageVersion("1.2.7.0")))
                {
                    //check after update, if the user already have a subscription
                    var purchaseService = App.Current.Container.Resolve<IPurchaseAddOnService>();
                    var networkService = App.Current.Container.Resolve<INetworkService>();
                    if (await networkService.GetIsInternetAvailableAsync())
                    {
                        purchaseService.Initialize(Constants.SupportSubscriptionId);
                        (bool IsActiveMonthSubscription, StoreLicense infoMonth) = await purchaseService.SetupPurchaseAddOnInfoAsync();

                        if (IsActiveMonthSubscription)
                        {
                            // set new expiration date and last check time
                            SettingsService.Instance.IsProVersion = true;
                            SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                            SettingsService.Instance.NextCheckedInPurchaseAddon = infoMonth.ExpirationDate;
                        }
                    }
                }

                DataService.Instance.NewAppUpdateDialogDisplayed = true;
                ContentDialog dialog = new ContentDialog();
                string clickedLink = string.Empty; // save the clicked link
                dialog.Title = Strings.Resources.NewAppFeaturesTitle;
                var markdown = new MarkdownTextBlock();
                markdown.Text = Strings.Resources.NewAppFeaturesContent;
                markdown.OnLinkClicked += Markdown_LinkClicked;
                dialog.Content = markdown;
                dialog.PrimaryButtonText = Strings.Resources.Confirm;
                dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                await dialogService.ShowDialogAsync(dialog, new DialogParameters());
                // check for in app navigation parameters
                if (!string.IsNullOrWhiteSpace(clickedLink))
                {
                    if (!clickedLink.StartsWith("http"))
                    {
                        await ViewModel.NavigationService.NavigateAsync(clickedLink);
                    }
                }

                // if a link is clicked, the dialog will be close for in app navigation parameters
                async void Markdown_LinkClicked(object sender, LinkClickedEventArgs e)
                {
                    clickedLink = e.Uri.ToString();
                    if(clickedLink.StartsWith("http"))
                    {
                        if (Uri.TryCreate(e.Uri.ToString(), UriKind.Absolute, out Uri link))
                        {
                            await Launcher.LaunchUriAsync(link);
                        }
                    }
                    else
                    {
                        dialogService.CloseDialogs();
                    }
                }
            }

            if (SystemInformationHelper.Instance.IsFirstRun)
            {
                if (SystemInformationHelper.Instance.OperatingSystemVersion.Build >= 22000)
                {
                    // set the round corner for Windows 11+
                    SettingsService.Instance.UseRoundCorner = true;
                }
                // not run on UI thread
                await CheckInAppSubscriptionStatus(true).ConfigureAwait(false);
            }
            else
            {
                // not run on UI thread
                await CheckInAppSubscriptionStatus(false).ConfigureAwait(false);
            }


            // ChangeBackgroundColorSetting(false);

        }

        private async Task CheckInAppSubscriptionStatus(bool isFirstStart)
        {
            var purchaseService = App.Current.Container.Resolve<IPurchaseAddOnService>();
            var networkService = App.Current.Container.Resolve<INetworkService>();
            if (await networkService.GetIsInternetAvailableAsync())
            {
                if (SettingsService.Instance.IsProVersion)
                {
                    if (SettingsService.Instance.PurchasedStoreId == Constants.OneYearSubscriptionId ||
                        SettingsService.Instance.PurchasedStoreId == Constants.SupportSubscriptionId)
                    {
                        if (SettingsService.Instance.NextCheckedInPurchaseAddon != new DateTimeOffset() && SettingsService.Instance.NextCheckedInPurchaseAddon < DateTimeOffset.Now)
                        {
                            if (SettingsService.Instance.PurchasedStoreId == Constants.SupportSubscriptionId)
                            {
                                purchaseService.Initialize(Constants.SupportSubscriptionId);
                                (bool IsActiveMonthSubscription, StoreLicense infoMonth) = await purchaseService.SetupPurchaseAddOnInfoAsync();

                                if (IsActiveMonthSubscription)
                                {
                                    // set new expiration date and last check time
                                    SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                                    SettingsService.Instance.NextCheckedInPurchaseAddon = infoMonth.ExpirationDate;
                                }
                                else
                                {
                                    SettingsService.Instance.IsProVersion = false;
                                }
                            }

                            if (SettingsService.Instance.PurchasedStoreId == Constants.OneYearSubscriptionId)
                            {
                                purchaseService.Initialize(Constants.OneYearSubscriptionId);
                                (bool IsActiveYearSubscription, StoreLicense infoYear) = await purchaseService.SetupPurchaseAddOnInfoAsync();
                                if (IsActiveYearSubscription)
                                {
                                    // set new expiration date and last check time
                                    SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                                    SettingsService.Instance.NextCheckedInPurchaseAddon = infoYear.ExpirationDate;
                                }
                                else
                                {
                                    SettingsService.Instance.IsProVersion = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // only check all addons, if this is the first start
                    if (isFirstStart)
                    {
                        purchaseService.Initialize(Constants.SupportSubscriptionId);
                        (bool IsActiveMonthSubscription, StoreLicense infoMonth) = await purchaseService.SetupPurchaseAddOnInfoAsync();

                        if (IsActiveMonthSubscription)
                        {
                            // set new expiration date and last check time
                            SettingsService.Instance.IsProVersion = true;
                            SettingsService.Instance.PurchasedStoreId = Constants.SupportSubscriptionId;
                            SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                            SettingsService.Instance.NextCheckedInPurchaseAddon = infoMonth.ExpirationDate;
                        }

                        purchaseService.Initialize(Constants.OneYearSubscriptionId);
                        (bool IsActiveYearSubscription, StoreLicense infoYear) = await purchaseService.SetupPurchaseAddOnInfoAsync();
                        if (IsActiveYearSubscription)
                        {
                            // set new expiration date and last check time
                            SettingsService.Instance.IsProVersion = true;
                            SettingsService.Instance.PurchasedStoreId = Constants.OneYearSubscriptionId;
                            SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                            SettingsService.Instance.NextCheckedInPurchaseAddon = infoYear.ExpirationDate;
                        }

                        purchaseService.Initialize(Constants.LifeTimeId);
                        (bool IsActiveLifeTimeBuy, StoreLicense infoLifeTime) = await purchaseService.SetupPurchaseAddOnInfoAsync();
                        if (IsActiveLifeTimeBuy)
                        {
                            // set new expiration date and last check time
                            SettingsService.Instance.IsProVersion = true;
                            SettingsService.Instance.PurchasedStoreId = Constants.LifeTimeId;
                            SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                            SettingsService.Instance.NextCheckedInPurchaseAddon = infoLifeTime.ExpirationDate;
                        }
                    }
                }
            }
            else
            {
                if (isFirstStart)
                {
                    // TODO give feedback that addons cannot be set, if no internet connection is available
                }

            }
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
                _navManager.AppViewBackButtonVisibility = ViewModel.NavigationService.CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
            else
            {
                _navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                ShellView.IsBackButtonVisible = NavigationViewBackButtonVisible.Auto;
                ShellView.IsBackEnabled = ViewModel.NavigationService.CanGoBack();
            }
        }

        private void SetupGestures()
        {
            _navManager.BackRequested += NavManager_BackRequested;
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            ShellView.BackRequested += async (s, e) => await ViewModel.NavigationService.GoBackAsync();
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
        }

        private Task CheckUnhandledExceptionLastSession()
        {
            return ErrorDialogs.ShowUnexpectedError(SettingsService.Instance.UnhandledExceptionStr);
        }

        private async void NavManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (ViewModel.NavigationService.CanGoBack())
            {
                e.Handled = true;
                await ViewModel.NavigationService.GoBackAsync();
            }
        }

        private async void FeedbackItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                Uri uri = new Uri("https://github.com/2fast-team/2fast/discussions");
                await Launcher.LaunchUriAsync(uri);
            }
            catch (Exception exc)
            {
                await App.Current.Container.Resolve<ILoggingService>().LogException(exc, SettingsService.Instance.LoggingSetting);
            }

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
                    if ((await ViewModel.NavigationService.NavigateAsync(_settingsNavigationStr)).Success)
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
                    else if ((await ViewModel.NavigationService.NavigateAsync(path)).Success)
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
