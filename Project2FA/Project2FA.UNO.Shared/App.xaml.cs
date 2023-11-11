using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Prism;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Prism.Ioc;
using Project2FA.UNO.Views;
using Project2FA.ViewModels;
using Project2FA.Repository.Database;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Project2FA.Core;
using Windows.Storage;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using UNOversal.Services.Network;
using UNOversal.Services.Secrets;
using UNOversal.Services.Settings;
using UNOversal.Services.Serialization;
using Project2FA.Core.Services.JSON;
using UNOversal.Ioc;
using UNOversal.DryIoc;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using WinUIWindow = Microsoft.UI.Xaml.Window;
using WindowActivatedEventArgs = Microsoft.UI.Xaml.WindowActivatedEventArgs;
using UNOversal;
using Project2FA.Services;
using Project2FA.Core.Services.NTP;
using CommunityToolkit.WinUI.Helpers;
using Windows.UI.Core;
using Uno.UI;

namespace Project2FA.UNO
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
        public static ShellPage ShellPageInstance { get; private set; }

        /// <summary>
        /// Pipeline for interacting with database.
        /// </summary>
        public static IProject2FARepository Repository { get; private set; }

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
            //Uno.UI.FeatureConfiguration.Style.ConfigureNativeFrameNavigation();
#endif

            this.InitializeComponent();
            RequestedTheme = SettingsService.Instance.AppStartSetTheme(RequestedTheme);

        }

//#if __IOS__ && DEBUG
//        // Hot Restart support for debug only
//        public override bool FinishedLaunching(UIKit.UIApplication uiApplication, Foundation.NSDictionary launchOptions)
//        {
//            Microsoft.UI.Xaml.Application.Start(_ => new App());
//            return base.FinishedLaunching(uiApplication, launchOptions);
//        }
//#endif

        public override async Task OnStartAsync(IApplicationArgs args)
        {
            if (WinUIWindow.Current.Content == null)
            {
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
                // set DB
                if (Repository is null)
                {
                    string databasePath = ApplicationData.Current.LocalFolder.Path + string.Format(@"\{0}.db", Constants.ContainerName);
                    var dbOptions = new DbContextOptionsBuilder<Project2FAContext>().UseSqlite("Data Source=" + databasePath);
                    Repository = new DBProject2FARepository(dbOptions);
                }

                // LoadIconNames(); //only for development
                // handle startup
                if (args?.Arguments is ILaunchActivatedEventArgs e)
                {
                    // TODO Uno release
                    //SystemInformation.Instance.TrackAppUse(e);
                    // set custom splash screen page
                    //Window.Current.Content = new SplashPage(e.SplashScreen);
                }

                if (args.Arguments is ProtocolActivatedEventArgs fileStartOnLaunch)
                {
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
                    if (await Repository.Password.GetAsync() is not null)
                    {
                        LoginPage loginPage = Container.Resolve<LoginPage>();
                        WinUIWindow.Current.Content = loginPage;
                    }
                    else
                    {
                        await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(WelcomePage));
                        WinUIWindow.Current.Content = ShellPageInstance;
                    }
                }
            }
            if (args.Arguments is ProtocolActivatedEventArgs fileActivated)
            {
#if __ANDROID__
                Android.Net.Uri strLink = Android.Net.Uri.Parse(fileActivated.Uri.ToString());
                DataService.Instance.ActivatedDatafile = StorageFile.GetFromSafUri(strLink);
#endif

                FileActivationPage fileActivationPage = Container.Resolve<FileActivationPage>();
                WinUIWindow.Current.Content = fileActivationPage;
            }
#if DEBUG
            WinUIWindow.Current.EnableHotReload();
#endif

            WinUIWindow.Current.Activate();
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                if (WinUIWindow.Current.Content is ShellPage)
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
            if (e.WindowActivationState == CoreWindowActivationState.CodeActivated)
            {
                if (_focusLostTimer == null)
                {
                    return;
                }
                if (_focusLostTimer.IsEnabled)
                {
                    _focusLostTimer.Stop();
                }
            }
        }


        private async void FocusLostTimer_Tick(object sender, object e)
        {
            if (await Repository.Password.GetAsync() is null)
            {
                _focusLostTimer.Stop();
                return;
            }
            TimeSpan timeDiff = DateTime.Now - _focusLostTime;
            if (SettingsService.Instance.UseAutoLogout)
            {
                if (true )//timeDiff.TotalMinutes >= SettingsService.Instance.AutoLogoutMinutes)
                {
                    _focusLostTimer.Stop();
                    var dialogService = Current.Container.Resolve<IDialogService>();
                    if (await dialogService.IsDialogRunning())
                    {
                        dialogService.CloseDialogs();
                    }
                    await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(BlankPage));
                    if (DataService.Instance.ActivatedDatafile != null)
                    {
                        var fileActivationPage = new FileActivationPage();
                        WinUIWindow.Current.Content = fileActivationPage;
                    }
                    else
                    {
#if ANDROID || IOS
                        var loginPage = new LoginPage(true);
#else
                        var loginPage = new LoginPage(true);
#endif

                        WinUIWindow.Current.Content = loginPage;
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
        public override bool OpenUrl(UIKit.UIApplication app, Foundation.NSUrl url, Foundation.NSDictionary options)
        {
            string urlPath = url.Path;
            if (_activeDatafileUrl != null)
            {
                //release the access
                _activeDatafileUrl.StopAccessingSecurityScopedResource();
            }
            _activeDatafileUrl = url;

            if (Foundation.NSFileManager.DefaultManager.IsReadableFile(urlPath))
            {
                LoadStorageFile(url,urlPath);
                //openedFile = new StorageFile(new );//await StorageFile.GetFileFromPathAsync(urlPath);
                //data = Foundation.NSData.FromFile(urlPath);
            }
            else
            {
                if (url.StartAccessingSecurityScopedResource())
                {
                    //data = Foundation.NSData.FromFile(urlPath);
                    LoadStorageFile(url,urlPath);
                    
                }
            }

            DataService.Instance.OpenDatefileUrl = url;
            return base.OpenUrl(app, url, options);
        }

        private async Task LoadStorageFile(Foundation.NSUrl url, string path)
        {
            DataService.Instance.ActivatedDatafile = await StorageFile.GetFileFromPathAsync(path);
            url.StopAccessingSecurityScopedResource();
        }
#endif

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
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

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IDialogService, DialogService>();
            containerRegistry.RegisterSingleton<IFileService, FileService>();
            containerRegistry.RegisterSingleton<INetworkService, NetworkService>();
            containerRegistry.RegisterSingleton<ISecretService, SecretService>();
            containerRegistry.RegisterSingleton<ISettingsHelper, SettingsHelper>();
            containerRegistry.RegisterSingleton<ISettingsAdapter, LocalSettingsAdapter>();
            containerRegistry.RegisterSingleton<IProject2FARepository, DBProject2FARepository>();
            containerRegistry.RegisterSingleton<ISerializationService, SerializationService>();
            containerRegistry.RegisterSingleton<INewtonsoftJSONService, NewtonsoftJSONService>();
            containerRegistry.RegisterSingleton<INetworkTimeService, NetworkTimeService>();
            containerRegistry.RegisterSingleton<BiometryService.IBiometryService, BiometryService.BiometryService>();

            containerRegistry.RegisterSingleton<ShellPage>();
            containerRegistry.RegisterForNavigation<BlankPage, BlankPageViewModel>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<WelcomePage, WelcomePageViewModel>();
            containerRegistry.RegisterForNavigation<AccountCodePage, AccountCodePageViewModel>();
            containerRegistry.RegisterForNavigation<NewDataFilePage, NewDataFilePageViewModel>();
            containerRegistry.RegisterForNavigation<UseDataFilePage, UseDataFilePageViewModel>();
            containerRegistry.RegisterForNavigation<SettingPage, SettingPageViewModel>();
            //containerRegistry.RegisterForNavigation<AppAboutPage, AppAboutPageViewModel>();
#if __IOS__ || __ANDROID__
            containerRegistry.RegisterForNavigation<EditAccountPage, EditAccountPageViewModel>();
            containerRegistry.RegisterForNavigation<AddAccountPage, AddAccountPageViewModel>();
#else
            containerRegistry.RegisterDialog<EditAccountContentDialog, EditAccountContentDialogViewModel>();
            containerRegistry.RegisterForNavigation<AddAccountContentDialog, AddAccountContentDialogViewModel>();
#endif

            //contentdialogs and view-models
            containerRegistry.RegisterDialog<AddAccountContentDialog, AddAccountContentDialogViewModel>();
            containerRegistry.RegisterDialog<ChangeDatafilePasswordContentDialog, ChangeDatafilePasswordContentDialogViewModel>();
            
            containerRegistry.RegisterDialog<RateAppContentDialog>();
            containerRegistry.RegisterDialog<UpdateDatafileContentDialog, UpdateDatafileContentDialogViewModel>();
            containerRegistry.RegisterDialog<UseDatafileContentDialog, UseDatafileContentDialogViewModel>();
            containerRegistry.RegisterDialog<WebViewDatafileContentDialog, WebViewDatafileContentDialogViewModel>();
            containerRegistry.RegisterDialog<DisplayQRCodeContentDialog, DisplayQRCodeContentDialogViewModel>();
            //containerRegistry.RegisterDialog<TutorialContentDialog, TutorialContentDialogViewModel>();
        }
    }
}
