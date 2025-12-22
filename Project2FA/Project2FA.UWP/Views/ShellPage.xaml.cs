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
using UNOversal.Ioc;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;
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
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using Windows.UI.Xaml.Media;

#if NET9_0_OR_GREATER
using WinRT;
#endif

namespace Project2FA.UWP.Views
{
    public sealed partial class ShellPage : Page
    {
        private readonly SystemNavigationManager _navManager;
        public NavigationView ShellViewInternal { get; private set; }
        public Frame MainFrame { get; }
        public ShellPageViewModel ViewModel { get; } = new ShellPageViewModel();
        private readonly CoreApplicationViewTitleBar _coreTitleBar;
        private string _settingsNavigationStr;
        private object PreviousItem { get; set; }


#if NET9_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(NavigationViewItem))]
#endif
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

#if NET9_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Style))]
#endif
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

#if NET9_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Style))]
        [DynamicWindowsRuntimeCast(typeof(FrameworkElement))]
#endif
        private async void ShellPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (SystemInformationHelper.Instance.OperatingSystemVersion.Build >= 22000)
            {
                // set background transparency for Windows 11+
                //this.ShellView.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }

            bool changedResources = false;
            // set the corner radius for the controls
            if (!SettingsService.Instance.UseRoundCorner)
            {
                App.Current.Resources["ControlCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["ComboBoxItemCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["ComboBoxItemPillCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["TokenItemCornerRadius"] = new CornerRadius(0);
                changedResources = true;
            }

            if(SettingsService.Instance.UseCompactDesign)
            {
                App.Current.Resources["ControlContentThemeFontSize"] = 14;
                App.Current.Resources["TextControlThemeMinHeight"] = 24;
                App.Current.Resources["TextControlThemePadding"] = new Thickness(2, 2, 6, 1);
                App.Current.Resources["ListViewItemMinHeight"] = 32;
                App.Current.Resources["TreeViewItemMinHeight"] = 24;
                App.Current.Resources["TreeViewItemMultiSelectCheckBoxMinHeight"] = 24;
                App.Current.Resources["TreeViewItemPresenterMargin"] = 0;
                App.Current.Resources["TreeViewItemPresenterPadding"] = 0;
                App.Current.Resources["TimePickerHostPadding"] = new Thickness(0, 1, 0, 2);
                App.Current.Resources["DatePickerHostPadding"] = new Thickness(0, 1, 0, 2);
                App.Current.Resources["DatePickerHostMonthPadding"] = new Thickness(9, 0, 0, 1);
                App.Current.Resources["ComboBoxEditableTextPadding"] = new Thickness(10, 0, 30, 0);
                App.Current.Resources["ComboBoxMinHeight"] = 24;
                App.Current.Resources["ComboBoxPadding"] = new Thickness(12, 1, 0, 3);

                App.Current.Resources["AccountListSpacing"] = 6;

                App.Current.Resources["NavigationViewItemOnLeftMinHeight"] = 32;

                App.Current.Resources["SettingsCardMinHeight"] = 56;
                App.Current.Resources["SettingsCardPadding"] = new Thickness(12, 12, 12, 12);
                App.Current.Resources["SettingsCardHeaderIconMargin"] = new Thickness(2, 0, 14, 0);
                App.Current.Resources["SettingsCardActionIconMargin"] = new Thickness(10, 0, 0, 0);

                App.Current.Resources["ButtonPadding"] = new Thickness(6, 2, 6, 3);

                // Font sizes
                App.Current.Resources["HeaderContentThemeFontSize"] = 20;
                //ItemContentThemeFontSize
                changedResources = true;
            }

            if(changedResources)
            {
                ViewModel.RefreshThemeForResources();
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
                //if (SystemInformationHelper.Instance.PreviousVersionInstalled.Equals(PackageVersionHelper.ToPackageVersion("1.2.7.0")))
                //{
                //    //check after update, if the user already have a subscription
                //    var purchaseService = App.Current.Container.Resolve<IPurchaseAddOnService>();
                //    var networkService = App.Current.Container.Resolve<INetworkService>();
                //    if (await networkService.GetIsInternetAvailableAsync())
                //    {
                //        purchaseService.Initialize(Constants.SupportSubscriptionId);
                //        (bool IsActiveMonthSubscription, StoreLicense infoMonth) = await purchaseService.SetupPurchaseAddOnInfoAsync();

                //        if (IsActiveMonthSubscription)
                //        {
                //            // set new expiration date and last check time
                //            SettingsService.Instance.IsProVersion = true;
                //            SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                //            SettingsService.Instance.NextCheckedInPurchaseAddon = infoMonth.ExpirationDate;
                //        }
                //    }
                //}

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

        /// <summary>
        /// Checks the current status of in-app subscriptions and updates purchase information as needed based on the
        /// application's startup state.
        /// </summary>
        /// <remarks>This method requires an active internet connection to validate and update
        /// subscription information. If no connection is available and <paramref name="isFirstStart"/> is <see
        /// langword="true"/>, add-on status cannot be updated. The method interacts with purchase and network services
        /// to ensure subscription data is current.</remarks>
        /// <param name="isFirstStart">Indicates whether this is the first time the application has started. If <see langword="true"/>, all
        /// available add-ons are checked; otherwise, only relevant subscriptions are validated.</param>
        /// <returns>A task that represents the asynchronous operation of checking and updating in-app subscription status.</returns>
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
                                await PurchaseAddOnInfoAsync(purchaseService, Constants.SupportSubscriptionId, isFirstStart);
                            }

                            if (SettingsService.Instance.PurchasedStoreId == Constants.OneYearSubscriptionId)
                            {
                                purchaseService.Initialize(Constants.OneYearSubscriptionId);
                                await PurchaseAddOnInfoAsync(purchaseService, Constants.OneYearSubscriptionId, isFirstStart);
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
                        await PurchaseAddOnInfoAsync(purchaseService, Constants.SupportSubscriptionId, isFirstStart);

                        purchaseService.Initialize(Constants.OneYearSubscriptionId);
                        await PurchaseAddOnInfoAsync(purchaseService, Constants.OneYearSubscriptionId, isFirstStart);

                        purchaseService.Initialize(Constants.LifeTimeId);
                        await PurchaseAddOnInfoAsync(purchaseService, Constants.LifeTimeId, isFirstStart);
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

        /// <summary>
        /// Initializes and updates purchase add-on information, including license status and related settings, based on
        /// the specified add-on and startup context.
        /// </summary>
        /// <remarks>If the add-on is active, the method updates expiration and check times, and sets the
        /// application to pro version on first start. If the add-on is not active, the pro version status is
        /// disabled.</remarks>
        /// <param name="purchaseAddOnService">The service used to retrieve and set up purchase add-on license information.</param>
        /// <param name="AddonID">The identifier of the add-on whose purchase information is being processed.</param>
        /// <param name="isFirstStart">Indicates whether this is the first application star. If <see langword="true"/>, additional
        /// settings are updated to reflect the new purchase.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task PurchaseAddOnInfoAsync(IPurchaseAddOnService purchaseAddOnService, string AddonID, bool isFirstStart)
        {
            (bool IsActiveAddon, StoreLicense licenseInfo) = await purchaseAddOnService.SetupPurchaseAddOnInfoAsync();

            if (IsActiveAddon)
            {
                // set new expiration date and last check time
                SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                SettingsService.Instance.NextCheckedInPurchaseAddon = licenseInfo.ExpirationDate;
                if (isFirstStart)
                {
                    SettingsService.Instance.IsProVersion = true;
                    SettingsService.Instance.PurchasedStoreId = AddonID;
                }
            }
            else
            {
                SettingsService.Instance.IsProVersion = false;
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

        /// <summary>
        /// Sets the selected item in the shell view and optionally performs navigation to the associated page.
        /// </summary>
        /// <remarks>If navigation fails, the selection may revert to the previous item or be cleared.
        /// Selecting the same item consecutively does not trigger navigation or update the selection. When the settings
        /// item is selected, navigation uses a predefined settings path.</remarks>
        /// <param name="selectedItem">The item to select in the shell view. Can be a navigation item or the settings item. If null, the selection
        /// is cleared.</param>
        /// <param name="withNavigation">Indicates whether navigation to the item's associated page should be performed. If <see langword="true"/>,
        /// navigation is attempted; otherwise, only the selection is updated.</param>
        /// <returns>A task that represents the asynchronous operation of setting the selected item and performing navigation if
        /// requested.</returns>
#if NET9_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(NavigationViewItem))]
#endif
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
