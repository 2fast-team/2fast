using Microsoft.EntityFrameworkCore;
using Project2FA.Core;
using Project2FA.Core.Services.JSON;
using Project2FA.Core.Services.NTP;
using Project2FA.Helpers;
using Project2FA.Repository.Database;
using Project2FA.Services;
using Project2FA.Services.Marketplace;
using Project2FA.UWP.Services.Compression;
using Project2FA.Services.Parser;
using Project2FA.Services.Web;
using Project2FA.Utils;
using Project2FA.UWP.Services;
using Project2FA.UWP.Views;
using Project2FA.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using UNOversal;
using UNOversal.DryIoc;
using UNOversal.Ioc;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using UNOversal.Services.Gesture;
using UNOversal.Services.Logging;
using UNOversal.Services.Network;
using UNOversal.Services.Secrets;
using UNOversal.Services.Serialization;
using UNOversal.Services.Settings;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Project2FA.Services.Importer;
using UNOversal.Helpers;

namespace Project2FA.UWP
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
            TrackingManager.TrackUnobservedTaskException("UnobservedTaskException", e);
            SettingsService.Instance.UnhandledExceptionStr += e.Exception.ToString() + e.Exception.InnerException + e.Exception.StackTrace;
            // let the app crash...
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            TrackingManager.TrackUnhandledException(nameof(App_UnhandledException), e);
            SettingsService.Instance.UnhandledExceptionStr += e.Exception.ToString() + e.Exception.InnerException + e.Exception.StackTrace + e.Message;
            // let the app crash...
        }

        protected override UIElement CreateShell()
        {
            ShellPageInstance = Container.Resolve<ShellPage>();
            return ShellPageInstance;
        }

        public override void RegisterTypes(IContainerRegistry container)
        {
            // custom services
            container.RegisterSingleton<ICompressionService, CompressionService>();
            container.RegisterSingleton<IDialogService, DialogService>();
            container.RegisterSingleton<IFileService, FileService>();
            container.RegisterSingleton<IMarketplaceService, MarketplaceService>();
            container.RegisterSingleton<INetworkService, NetworkService>();
            container.RegisterSingleton<ISecretService, SecretService>();
            container.RegisterSingleton<ISettingsHelper, SettingsHelper>();
            container.RegisterSingleton<IWebApiService, WebApiService>();
            container.RegisterSingleton<IGestureService, GestureService>();
            container.RegisterSingleton<IProject2FARepository, DBProject2FARepository>();
            container.RegisterSingleton<ISerializationService, SerializationService>(); //for internal uwp services
            container.RegisterSingleton<INewtonsoftJSONService, NewtonsoftJSONService>(); //netstandard for general access
            container.RegisterSingleton<ISettingsAdapter, LocalSettingsAdapter>();
            container.RegisterSingleton<IProject2FAParser, Project2FAParser>();
            container.RegisterSingleton<INetworkTimeService, NetworkTimeService>();
            container.RegisterSingleton<IPurchaseAddOnService, PurchaseAddOnService>();
            container.RegisterSingleton<ILoggingService, LoggingService>();
            container.RegisterSingleton<IBackupImporterService, BackupImporterService>();
            container.RegisterSingleton<IAegisBackupService, AegisBackupService>();
            // pages and view-models
            container.RegisterSingleton<ShellPage, ShellPage>();
            container.RegisterSingleton<LoginPage, LoginPage>();
            container.RegisterForNavigation<AccountCodePage, AccountCodePageViewModel>();
            container.RegisterForNavigation<WelcomePage, WelcomePageViewModel>();
            container.RegisterForNavigation<SettingPage, SettingPageViewModel>();
            container.RegisterForNavigation<BlankPage, BlankPageViewModel>();
            container.RegisterForNavigation<UseDataFilePage, UseDataFilePageViewModel>();
            container.RegisterForNavigation<NewDataFilePage, NewDataFilePageViewModel>();
            container.RegisterForNavigation<TutorialPage, TutorialPageViewModel>();
            //contentdialogs and view-models
            container.RegisterDialog<AddAccountContentDialog, AddAccountContentDialogViewModel>();
            container.RegisterDialog<ImportAccountContentDialog, ImportAccountContentDialogViewModel>();
            container.RegisterDialog<ChangeDatafilePasswordContentDialog, ChangeDatafilePasswordContentDialogViewModel>();
            container.RegisterDialog<EditAccountContentDialog, EditAccountContentDialogViewModel>();
            container.RegisterDialog<RateAppContentDialog>();
            container.RegisterDialog<UpdateDatafileContentDialog, UpdateDatafileContentDialogViewModel>();
            container.RegisterDialog<UseDatafileContentDialog, UseDatafileContentDialogViewModel>();
            container.RegisterDialog<WebViewDatafileContentDialog, WebViewDatafileContentDialogViewModel>();
            container.RegisterDialog<DisplayQRCodeContentDialog, DisplayQRCodeContentDialogViewModel>();
            container.RegisterDialog<ManageCategoriesContentDialog, ManageCategoriesContentDialogViewModel>();
            container.RegisterDialog<InAppPaymentContentDialog, InAppPaymentContentDialogViewModel>();
            container.RegisterDialog<WebDAVAuthContentDialog, WebDAVAuthContentDialogViewModel>();
        }

        public override async Task OnStartAsync(IApplicationArgs args)
        {
            if (Window.Current.Content == null)
            {
                ThemeHelper.Initialize();
                // Hide default title bar
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                if (!coreTitleBar.ExtendViewIntoTitleBar)
                {
                    coreTitleBar.ExtendViewIntoTitleBar = true;
                }
                Window.Current.Activated -= Current_Activated;
                Window.Current.Activated += Current_Activated;
                // set DB
                if (Repository is null)
                {
                    string databasePath = ApplicationData.Current.LocalFolder.Path + string.Format(@"\{0}.db", Constants.ContainerName);
                    var dbOptions = new DbContextOptionsBuilder<Project2FAContext>().UseSqlite("Data Source=" + databasePath);
                    Repository = new DBProject2FARepository(dbOptions);
                }

                //await LoadIconNames(); // for development only
                // handle startup
                if (args?.Arguments is ILaunchActivatedEventArgs e)
                {
                    SystemInformationHelper.Instance.TrackAppUse(e as LaunchActivatedEventArgs);
                    // set custom splash screen page
                    //Window.Current.Content = new SplashPage(e.SplashScreen);
                }

                if (args.Arguments is ProtocolActivatedEventArgs protoActivated)
                {
                    string content = protoActivated.Uri.ToString();
                    var parser = App.Current.Container.Resolve<IProject2FAParser>();
                    var cmdlist = parser.ParseCmdStr(content);
                    for (int i = 0; i < cmdlist.Count; i++)
                    {
                        if (cmdlist[i].Key == "isScreenCaptureEnabled")
                        {
                            if (bool.TryParse(cmdlist[i].Value, out bool value))
                            {
                                ShellPageInstance.ViewModel.IsScreenCaptureEnabled = value;
                            }
                            break;
                        }
                        //if (cmdlist[i].Key == "startLogFileCmd")
                        //{
                        //    try
                        //    {
                        //        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Constants.LogName);
                        //        // Launch the URI and pass in the recommended app
                        //        var success = await Launcher.LaunchFileAsync(file);
                        //    }
                        //    catch (Exception)
                        //    {
                        //        await ErrorDialogs.ShowLogNotFound();
                        //    }
                        //    break;
                        //}
                        //if (SettingsService.Instance.IsProVersion)
                        //{
                        //    if (cmdlist[i].Key == "addAccount")
                        //    {
                        //        var accountParams = parser.ParseQRCodeStr(cmdlist[i].Value);
                        //        if (accountParams.Count > 0 && accountParams[0].Value == "totp")
                        //        {
                        //            var dialog = new AddAccountContentDialog();
                        //            var dialogParams = new DialogParameters();
                        //            dialogParams.Add("account", accountParams);
                        //            await this.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, dialogParams);
                        //        }
                        //    }
                        //    break;
                        //}
                    }
                }

                if (args.Arguments is FileActivatedEventArgs fileActivated)
                {
                    var file = fileActivated.Files.FirstOrDefault();
                    DataService.Instance.ActivatedDatafile = (StorageFile)file;
                    var dialogService = Current.Container.Resolve<IDialogService>();
                    if (await dialogService.IsDialogRunning())
                    {
                        dialogService.CloseDialogs();
                    }
                    await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(BlankPage));
                    FileActivationPage fileActivationPage = Container.Resolve<FileActivationPage>();
                    Window.Current.Content = fileActivationPage;
                }
                else
                {
                    if (!(await Repository.Password.GetAsync() is null))
                    {
                        LoginPage loginPage = Container.Resolve<LoginPage>();
                        Window.Current.Content = loginPage;
                    }
                    else
                    {
                        await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(TutorialPage));
                        Window.Current.Content = ShellPageInstance;
                    }
                }
            }
            else
            {
                if (args.Arguments is FileActivatedEventArgs fileActivated)
                {
                    var file = fileActivated.Files.FirstOrDefault();
                    DataService.Instance.ActivatedDatafile = (StorageFile)file;
                    FileActivationPage fileActivationPage = Container.Resolve<FileActivationPage>();
                    Window.Current.Content = fileActivationPage;
                }

                // else invalid request
            }

            if (args.Arguments is ProtocolActivatedEventArgs protoLaunchActivated)
            {
                string content = protoLaunchActivated.Uri.ToString();
                var parser = App.Current.Container.Resolve<IProject2FAParser>();
                var cmdlist = parser.ParseCmdStr(HttpUtility.UrlDecode(content));
                for (int i = 0; i < cmdlist.Count; i++)
                {
                    if (cmdlist[i].Key == "startLogFileCmd")
                    {
                        try
                        {
                            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Constants.LogName);
                            // Launch the URI and pass in the recommended app
                            var success = await Launcher.LaunchFileAsync(file);
                        }
                        catch (Exception)
                        {
                            await ErrorDialogs.ShowLogNotFound();
                        }
                    }
                    if (Window.Current.Content is ShellPage shellpage && shellpage.MainFrame.Content is AccountCodePage)
                    {
                        if (SettingsService.Instance.IsProVersion)
                        {
                            if (cmdlist[i].Key == "addAccount")
                            {
                                var accountParams = parser.ParseQRCodeStr(cmdlist[i].Value);
                                if (accountParams.Count > 0 && accountParams[0].Value == "totp")
                                {
                                    var dialog = new AddAccountContentDialog();
                                    var dialogParams = new DialogParameters();
                                    dialogParams.Add("account", accountParams);
                                    await this.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, dialogParams);
                                }
                            }
                            break;
                        }
                    }
                }
            }

            Window.Current.Activate();
        }

        #region AutoLogout
        /// <summary>
        /// Detects if the focus is lost for the app and start the timer for auto logout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                if (timeDiff.TotalMinutes >= SettingsService.Instance.AutoLogoutMinutes)
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
                        Window.Current.Content = fileActivationPage;
                    }
                    else
                    {
                        var loginPage = new LoginPage(true);
                        Window.Current.Content = loginPage;
                    }
                }
            }
        }
        #endregion
    }
}
