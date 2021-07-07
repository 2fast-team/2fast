using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Helpers;
using Prism;
using Prism.Ioc;
using Prism.Unity;
using Project2FA.Core;
using Project2FA.Core.Services.JSON;
using Project2FA.Core.Services.NTP;
using Project2FA.Core.Services.Parser;
using Project2FA.Repository.Database;
using Project2FA.UWP.Services;
using Project2FA.UWP.ViewModels;
using Project2FA.UWP.Views;
using System;
using System.Threading.Tasks;
using Template10.Services.Dialog;
using Template10.Services.Serialization;
using Template10.Services.Settings;
using Template10.Utilities;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Project2FA.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : PrismApplication
    {
        private DateTime _focusLostTime;
        private DispatcherTimer _focusLostTimer;
        /// <summary>
        /// Creates the access of the static instance of the ShellPage
        /// </summary>
        public static ShellPage ShellPageInstance { get; } = new ShellPage();

        /// <summary>
        /// Pipeline for interacting with database.
        /// </summary>
        public static IProject2FARepository Repository { get; private set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            RequestedTheme = SettingsService.Instance.AppStartSetTheme(RequestedTheme);
            UnhandledException += App_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            TrackingManager.TrackException(e.Exception);
            SettingsService.Instance.UnhandledExceptionStr += e.Exception.Message + "\n" + e.Exception.StackTrace + "\n"
            + e.Exception.InnerException;
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            TrackingManager.TrackException(e.Exception);
            SettingsService.Instance.UnhandledExceptionStr += e.Exception.Message + "\n" + e.Exception.StackTrace + "\n"
            + e.Exception.InnerException;
        }

        public override void RegisterTypes(IContainerRegistry container)
        {
            // standard template 10 services
            container.RegisterTemplate10Services();
            // custom services
            container.RegisterSingleton<IProject2FARepository, DBProject2FARepository>();
            container.RegisterSingleton<ISerializationService, SerializationService>(); //for internal uwp services
            container.RegisterSingleton<INewtonsoftJSONService, NewtonsoftJSONService>(); //netstandard for general access
            container.RegisterSingleton<ISettingsAdapter, LocalSettingsAdapter>();
            container.RegisterSingleton<IProject2FAParser, Project2FAParser>();
            container.RegisterSingleton<INetworkTimeService, NetworkTimeService>();
            // pages and view-models
            container.RegisterSingleton<ShellPage, ShellPage>();
            container.RegisterSingleton<LoginPage, LoginPage>();
            container.RegisterView<AccountCodePage, AccountCodePageViewModel>();
            container.RegisterView<WelcomePage, WelcomePageViewModel>();
            container.RegisterView<SettingPage, SettingPageViewModel>();
            container.RegisterView<BlankPage, BlankPageViewModel>();
        }

        public override async Task OnStartAsync(IStartArgs args)
        {
            if (Window.Current.Content == null)
            {
                //if (System.Diagnostics.Debugger.IsAttached)
                //{
                //    ApplicationLanguages.PrimaryLanguageOverride = "en";
                //}
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                if (!coreTitleBar.ExtendViewIntoTitleBar)
                {
                    coreTitleBar.ExtendViewIntoTitleBar = true;
                }
                Window.Current.Activated += Current_Activated;
                bool loginRequiered = false;
                // set DB
                if (Repository is null)
                {
                    string databasePath = ApplicationData.Current.LocalFolder.Path + string.Format(@"\{0}.db", Constants.ContainerName);
                    var dbOptions = new DbContextOptionsBuilder<Project2FAContext>().UseSqlite("Data Source=" + databasePath);
                    Repository = new DBProject2FARepository(dbOptions);
                }

                // built initial navigation path
                string navigationPath = string.Empty;

                // handle startup
                if (args?.Arguments is ILaunchActivatedEventArgs e)
                {
                    SystemInformation.Instance.TrackAppUse(e as LaunchActivatedEventArgs);
                    // set custom splash screen page
                    //Window.Current.Content = new SplashPage(e.SplashScreen);
                }
                if (await Repository.Password.GetAsync() is null)
                {
                    navigationPath = "/WelcomePage";
                }
                else
                {
                    loginRequiered = true;
                }

                if (loginRequiered)
                {
                    LoginPage loginPage = Container.Resolve<LoginPage>();
                    Window.Current.Content = loginPage;
                }
                else
                {
                    await ShellPageInstance.NavigationService.NavigateAsync(navigationPath);
                    Window.Current.Content = ShellPageInstance;
                }
            }
            Window.Current.Activate();
        }

        #region AutoLogout
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                if (Window.Current.Content is ShellPage)
                {
                    if (_focusLostTimer == null)
                    {
                        _focusLostTimer = new DispatcherTimer();
                        _focusLostTimer.Interval = new TimeSpan(0, 1, 0); //every minute
                        _focusLostTimer.Tick += FocusLostTimer_Tick;
                    }
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
            var timeDiff = DateTime.Now - _focusLostTime;
            if (timeDiff.TotalMinutes >= 10)
            {
                _focusLostTimer.Stop();
                bool isLogout = true;
                var dialogService = Current.Container.Resolve<IDialogService>();
                if (await dialogService.IsDialogRunning())
                {
                    dialogService.CancelDialogs();
                }
                await ShellPageInstance.NavigationService.NavigateAsync("/BlankPage");
                var loginPage = new LoginPage(isLogout);
                Window.Current.Content = loginPage;
            }
        }
        #endregion
    }
}
