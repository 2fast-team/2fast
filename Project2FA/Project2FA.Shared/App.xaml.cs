using DryIoc;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using Project2FA.Views;
using System;
using System.Reflection;
using Prism.Regions;
using Prism.Modularity;
using Project2FA.ViewModels;
using Microsoft.EntityFrameworkCore;
using Project2FA.Repository.Database;
using Project2FA.Core;
using System.Threading.Tasks;
using Windows.Storage;

namespace Project2FA
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : PrismApplication
    {
        private static Window _window;

#if HAS_UNO_WINUI || NETCOREAPP
        public static XamlRoot MainXamlRoot { get; private set; }
#endif
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
            InitializeLogging();

            this.InitializeComponent();

#if !HAS_UNO_WINUI && !NETCOREAPP
           //this.Suspending += OnSuspending;
#endif
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
//        protected override void OnLaunched(LaunchActivatedEventArgs args)
//        {
//#if __IOS__
//            // requires Xamarin Test Cloud Agent
//            Xamarin.Calabash.Start();
//#endif

//            base.OnLaunched(args);
//        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }


        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        //protected override void OnSuspending(SuspendingEventArgs e)
        //{
        //    var deferral = e.SuspendingOperation.GetDeferral();
        //    //TODO: Save application state and stop any background activity
        //    deferral.Complete();
        //}

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
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

            //global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
        }

        //protected override void ConfigureViewModelLocator()
        //{
        //    base.ConfigureViewModelLocator();
        //    ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
        //    {
        //        string viewName = viewType.FullName;
        //        if (viewName == null)
        //        {
        //            return null;
        //        }

        //        if (viewName.EndsWith("View"))
        //        {
        //            viewName = viewName.Substring(0, viewName.Length - 4);
        //        }

        //        if (viewName.EndsWith("Control"))
        //        {
        //            viewName = viewName.Substring(0, viewName.Length - 7);
        //        }

        //        viewName = viewName.Replace(".Views.", ".ViewModels.");
        //        viewName = viewName.Replace(".Controls.", ".ControlViewModels.");
        //        string viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
        //        string viewModelName = $"{viewName}ViewModel, {viewAssemblyName}";
        //        return Type.GetType(viewModelName);
        //    });

        //}

        protected override void OnInitialized()
        {
            base.OnInitialized();
            StartApp();
        }

        protected async Task StartApp()
        {
            var regionManager = Container.Resolve<IRegionManager>();
            // set DB
            if (Repository is null)
            {
                string databasePath = ApplicationData.Current.LocalFolder.Path + string.Format(@"\{0}.db", Constants.ContainerName);
                var dbOptions = new DbContextOptionsBuilder<Project2FAContext>().UseSqlite("Data Source=" + databasePath);
                Repository = new DBProject2FARepository(dbOptions);
            }

            if (!(await Repository.Password.GetAsync() is null))
            {
                //LoginPage loginPage = Container.Resolve<LoginPage>();
                //Window.Current.Content = loginPage;
            }
            else
            {
                regionManager.RequestNavigate("ContentRegion", nameof(WelcomePage));
                Window.Current.Content = ShellPageInstance;
            }
        }

        protected override UIElement CreateShell()
        {
            ShellPageInstance = Container.Resolve<ShellPage>();
#if NET5_0 && WINDOWS
            _window = new Window();
            _window.Activate();
            _window.Content = ShellPageInstance;
#endif

#if HAS_UNO_WINUI || NETCOREAPP
            ShellPageInstance.Loaded += (s, e) => {
                MainXamlRoot = ShellPageInstance.XamlRoot;
            };
#endif

            return ShellPageInstance;
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            //moduleCatalog.AddModule<ModuleA.ModuleAModule>(InitializationMode.OnDemand);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
           containerRegistry.RegisterSingleton<IProject2FARepository, DBProject2FARepository>();
            containerRegistry.RegisterForNavigation<WelcomePage, WelcomePageViewModel>();
            containerRegistry.RegisterForNavigation<UseDataFilePage, UseDataFilePageViewModel>();
            containerRegistry.RegisterForNavigation<NewDataFilePage, NewDataFilePageViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            containerRegistry.RegisterForNavigation<AccountCodePage, AccountCodePageViewModel>();
        }
    }
}
