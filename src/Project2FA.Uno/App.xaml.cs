using Microsoft.Extensions.Logging;
using Project2FA.Core.Services.NTP;
using Project2FA.Services;
using Project2FA.Services.Importer;
using Project2FA.Services.Parser;
#if RELEASE
using Project2FA.Uno;
#endif
using Project2FA.Uno.Views;
using Project2FA.ViewModels;
#if DEBUG
using Uno.UI.HotDesign;
#endif
using UNOversal;
using UNOversal.DryIoc;
using UNOversal.Helpers;
using UNOversal.Ioc;
using UNOversal.Navigation;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using UNOversal.Services.Gesture;
using UNOversal.Services.Logging;
using UNOversal.Services.Network;
using UNOversal.Services.Secrets;
using UNOversal.Services.Serialization;
using UNOversal.Services.Settings;
using Windows.ApplicationModel.Activation;
using WinUIWindow = Microsoft.UI.Xaml.Window;

namespace Project2FA.UnoApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : UNOversalApplication
    {
        private DateTime _focusLostTime;
        private DispatcherTimer _focusLostTimer;
        /// <summary>
        /// Creates the access of the static instance of the ShellPage
        /// </summary>
        internal static ShellPage ShellPageInstance { get; private set; }

        /// <summary>
        /// Gets the main window of the app.
        /// </summary>
        protected WinUIWindow? MainWindow { get; private set; }

#if __IOS__
        Foundation.NSUrl _activeDatafileUrl;
#endif
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeLogging();

#if __IOS__ || __ANDROID__
            //SetStyles();
            //FeatureConfiguration.Style.ConfigureNativeFrameNavigation();
#endif

            this.InitializeComponent();
            RequestedTheme = SettingsService.Instance.AppStartSetTheme(RequestedTheme);

        }

        public override async Task OnStartAsync(IApplicationArgs args)
        {
            MainWindow = WinUIWindow.Current;
#if DEBUG
            //WinUIWindow.Current.EnableHotReload(); // obsolete
            MainWindow.UseStudio();
#endif
            MainWindow.SetWindowIcon();

#if __ANDROID__ || __IOS__
            //FeatureConfiguration.ListViewBase.AnimateScrollIntoView = false;
#endif

            if (MainWindow.Content == null)
            {
                MainWindow.Content = ShellPageInstance;
                //ThemeHelper.Initialize();
                // Hide default title bar
                // not implemented
                //CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                //if (!coreTitleBar.ExtendViewIntoTitleBar)
                //{
                //    coreTitleBar.ExtendViewIntoTitleBar = true;
                //}

                WinUIWindow.Current.Activated -= Current_Activated;
                WinUIWindow.Current.Activated += Current_Activated;

                // LoadIconNames(); //only for development
                // handle startup
                if (args?.Arguments is ILaunchActivatedEventArgs e)
                {
                    SystemInformationHelper.Instance.TrackAppUse(e);
                }

                if (args.Arguments is ProtocolActivatedEventArgs fileStartOnLaunch)
                {
                    // TODO not implemented in Android/iOS
                    //var file = fileActivated.Files.FirstOrDefault();
                    //DataService.Instance.ActivatedDatafile = (StorageFile)file;
                    //var dialogService = Current.Container.Resolve<IDialogService>();
                    //if (await dialogService.IsDialogRunning())
                    //{
                    //    dialogService.CloseDialogs();
                    //}
                    //await ShellPageInstance.NavigationService.NavigateAsync("/" + nameof(BlankPage));
                    //FileActivationPage fileActivationPage = Container.Resolve<FileActivationPage>();
                    //WinUIWindow.Current.Content = fileActivationPage;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(SettingsService.Instance.DataFilePasswordHash))
                    {
                        await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(LoginPage));
                    }
                    else
                    {
                        await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(TutorialPage));
                    }
                }
            }
            if (args.Arguments is ProtocolActivatedEventArgs fileActivated)
            {
#if __ANDROID__
                Android.Net.Uri strLink = Android.Net.Uri.Parse(fileActivated.Uri.ToString());
                DataService.Instance.ActivatedDatafile = StorageFile.GetFromSafUri(strLink);
#endif
                await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(FileActivationPage));
            }

            WinUIWindow.Current.Activate();
        }

        private void SetStyles()
        {
#if ANDROID
            //var style = new Style(typeof(NativePivotPresenter))
            //{
            //    Setters =
            //    {
            //        new Setter<NativePivotPresenter>("Template", pb => pb
            //            .Template = new ControlTemplate(() =>
            //                new Grid
            //                {
            //                    RowDefinitions =
            //                    {
            //                        new RowDefinition(){ Height = GridLength.Auto},
            //                        new RowDefinition(){ Height = new GridLength(1, GridUnitType.Star)},
            //                    },

            //                    Children =
            //                    {
            //                        // Header
            //                        new Border
            //                        {
            //                            Child = new Uno.UI.Controls.SlidingTabLayout(ContextHelper.Current)
            //                            {
            //                                LayoutParameters = new Android.Views.ViewGroup.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent, Android.Views.ViewGroup.LayoutParams.WrapContent),
            //                            },
            //                            BorderThickness = new Thickness(0,0,0,1),
            //                        }
            //                        .Apply(b => b.SetBinding("Background", new Binding { Path = "Background", RelativeSource = RelativeSource.TemplatedParent }))
            //                        .Apply(b => b.SetBinding("BorderBrush", new Binding { Path = "BorderBrush", RelativeSource = RelativeSource.TemplatedParent })),

            //                        // Content
            //                        new ExtendedViewPager(ContextHelper.Current)
            //                        {
            //                            OffscreenPageLimit = 1,
            //                            PageMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, ContextHelper.Current.Resources.DisplayMetrics),
            //                            SwipeEnabled = true,
            //                        }
            //                        .Apply(v => Grid.SetRow(v, 1))
            //                    }
            //            })
            //        )
            //    }
            //};

            //Style.RegisterDefaultStyleForType(typeof(NativePivotPresenter), style);
#endif
        }

        /// <summary>
        /// Detects if the focus is lost for the app and start the timer for auto logout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            // reference https://github.com/unoplatform/uno/discussions/17625#discussioncomment-10114134
#if WINDOWS
    // For WinUI, use WindowActivationState
    if (e.WindowActivationState == WindowActivationState.CodeActivated ||
        e.WindowActivationState == WindowActivationState.PointerActivated)
#else
            // You can check it on other platforms with Windows.UI.Core.CoreWindowActivationState
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.CodeActivated ||
                e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.PointerActivated)
#endif
            {
                // Focused
                if (_focusLostTimer == null)
                {
                    return;
                }
                if (_focusLostTimer.IsEnabled)
                {
                    _focusLostTimer.Stop();
                }
            }
            else
            {
                // Not focused
                if (MainWindow.Content is ShellPage)
                {
                    if (_focusLostTimer == null)
                    {
                        _focusLostTimer = new DispatcherTimer();
                        _focusLostTimer.Interval = new TimeSpan(0, 1, 0); //every minute
                    }
                    _focusLostTimer.Tick -= FocusLostTimer_Tick;
                    _focusLostTimer.Tick += FocusLostTimer_Tick;
                    _focusLostTimer.Start();
                    _focusLostTime = DateTime.Now;
                }
            }
        }


        private async void FocusLostTimer_Tick(object sender, object e)
        {
            if (string.IsNullOrWhiteSpace(SettingsService.Instance.DataFilePasswordHash))
            {
                _focusLostTimer.Stop();
                return;
            }
            TimeSpan timeDiff = DateTime.Now - _focusLostTime;
            if (SettingsService.Instance.UseAutoLogout)
            {
                if (timeDiff.TotalMinutes >= SettingsService.Instance.AutoLogoutMinutes)
                {
                    _focusLostTimer.Stop();
                    var dialogService = Current.Container.Resolve<IDialogService>();
                    if (await dialogService.IsDialogRunning())
                    {
                        dialogService.CloseDialogs();
                    }
                    if (ShellPageInstance.MainFrame.DispatcherQueue.HasThreadAccess)
                    {
                        //await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(BlankPage));
                        if (DataService.Instance.ActivatedDatafile != null)
                        {
                            await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(FileActivationPage));
                        }
                        else
                        {
                            var navigationParameters = new NavigationParameters();
                            navigationParameters.Add("isLogout", true);
                            await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(LoginPage), navigationParameters);
                        }
                    }
                }
            }
        }

        protected override UIElement CreateShell()
        {
            ShellPageInstance = new ShellPage();
            return ShellPageInstance;
        }

#if __IOS__
        //public override bool OpenUrl(UIKit.UIApplication app, Foundation.NSUrl url, Foundation.NSDictionary options)
        //{
        //    string urlPath = url.Path;
        //    if (_activeDatafileUrl != null)
        //    {
        //        //release the access
        //        _activeDatafileUrl.StopAccessingSecurityScopedResource();
        //    }
        //    _activeDatafileUrl = url;

        //    if (Foundation.NSFileManager.DefaultManager.IsReadableFile(urlPath))
        //    {
        //        LoadStorageFile(url,urlPath);
        //        //openedFile = new StorageFile(new );//await StorageFile.GetFileFromPathAsync(urlPath);
        //        //data = Foundation.NSData.FromFile(urlPath);
        //    }
        //    else
        //    {
        //        if (url.StartAccessingSecurityScopedResource())
        //        {
        //            //data = Foundation.NSData.FromFile(urlPath);
        //            LoadStorageFile(url,urlPath);
                    
        //        }
        //    }

        //    DataService.Instance.OpenDatefileUrl = url;
        //    return base.OpenUrl(app, url, options);
        //}

        private async Task LoadStorageFile(Foundation.NSUrl url, string path)
        {
            DataService.Instance.ActivatedDatafile = await StorageFile.GetFileFromPathAsync(path);
            url.StopAccessingSecurityScopedResource();
        }
#endif

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        public static void InitializeLogging()
        {
#if DEBUG
            // Logging is disabled by default for release builds, as it incurs a significant
            // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
            // is a concern for your application, keep this disabled. If you're running on web or 
            // desktop targets, you can use url or command line parameters to enable it.
            //
            // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
                builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
                builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
                builder.AddDebug();
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // RemoteControl and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
            global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
#if __IOS__ || __ANDROID__
            global::Uno.UI.FeatureConfiguration.Style.ConfigureNativeFrameNavigation();
#endif
        }

        public override void RegisterTypes(IContainerRegistry container)
        {
            // custom services
            container.RegisterSingleton<IDialogService, DialogService>();
            container.RegisterSingleton<IFileService, FileService>();
            container.RegisterSingleton<INetworkService, NetworkService>();
            container.RegisterSingleton<ISecretService, SecretService>();
            container.RegisterSingleton<ISettingsHelper, SettingsHelper>();
            container.RegisterSingleton<IGestureService, GestureService>();
            container.RegisterSingleton<ISerializationService, SerializationService>();
            container.RegisterSingleton<ISerializationCryptoService, SerializationCryptoService>();
            container.RegisterSingleton<ISettingsAdapter, LocalSettingsAdapter>();
            container.RegisterSingleton<IProject2FAParser, Project2FAParser>();
            container.RegisterSingleton<INetworkTimeService, NetworkTimeService>();
            //container.RegisterSingleton<IPurchaseAddOnService, PurchaseAddOnService>();
            container.RegisterSingleton<ILoggingService, LoggingService>();
            container.RegisterSingleton<IBackupImporterService, BackupImporterService>();
            container.RegisterSingleton<IAegisBackupImportService, AegisBackupImportService>();
            container.RegisterSingleton<IAndOTPBackupImportService, AndOTPBackupImportService>();
            container.RegisterSingleton<ITwoFASBackupImportService, TwoFASBackupImportService>();
            container.RegisterSingleton<BiometryService.IBiometryService, BiometryService.BiometryService>();


            container.RegisterSingleton<ShellPage>();
            container.RegisterForNavigation<BlankPage, BlankPageViewModel>();
            container.RegisterForNavigation<TutorialPage, TutorialPageViewModel>();
            container.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            container.RegisterForNavigation<FileActivationPage, FileActivationPageViewModel>();
            container.RegisterForNavigation<WelcomePage, WelcomePageViewModel>();
            container.RegisterForNavigation<AccountCodePage, AccountCodePageViewModel>();
            container.RegisterForNavigation<NewDataFilePage, NewDataFilePageViewModel>();
            container.RegisterForNavigation<UseDataFilePage, UseDataFilePageViewModel>();
            container.RegisterForNavigation<SettingPage, SettingPageViewModel>();

            //containerRegistry.RegisterForNavigation<AppAboutPage, AppAboutPageViewModel>();
            // fullscreen pages instead of dialogs
#if __IOS__ || __ANDROID__
            container.RegisterForNavigation<EditAccountPage, EditAccountPageViewModel>();
            container.RegisterForNavigation<AddAccountPage, AddAccountPageViewModel>();
            container.RegisterForNavigation<CameraPage, CameraPageViewModel>();
#else
            containerRegistry.RegisterDialog<EditAccountContentDialog, EditAccountContentDialogViewModel>();
            containerRegistry.RegisterForNavigation<AddAccountContentDialog, AddAccountContentDialogViewModel>();
#endif

            //contentdialogs and view-models
            container.RegisterDialog<AddAccountContentDialog, AddAccountContentDialogViewModel>();
            container.RegisterDialog<ChangeDatafilePasswordContentDialog, ChangeDatafilePasswordContentDialogViewModel>();

            container.RegisterDialog<RateAppContentDialog>();
            container.RegisterDialog<UpdateDatafileContentDialog, UpdateDatafileContentDialogViewModel>();
            container.RegisterDialog<UseDatafileContentDialog, UseDatafileContentDialogViewModel>();
            container.RegisterDialog<WebViewDatafileContentDialog, WebViewDatafileContentDialogViewModel>();
            container.RegisterDialog<DisplayQRCodeContentDialog, DisplayQRCodeContentDialogViewModel>();
            //containerRegistry.RegisterDialog<TutorialContentDialog, TutorialContentDialogViewModel>();
        }
    }
}
