using Microsoft.EntityFrameworkCore;
using Prism;
using Prism.Ioc;
using Prism.Navigation;
using Project2FA.Core.Services.JSON;
using Project2FA.Core.Services.Parser;
using Project2FA.Repository.Database;
using Project2FA.Services.File;
using Project2FA.ViewModels;
using Project2FA.Views;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Project2FA
{
    public partial class App
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        /// <summary>
        /// Pipeline for interacting with database.
        /// </summary>
        public static IProject2FARepository Repository { get; private set; }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            bool loginRequiered = false;
            string navigationPath = string.Empty;
            //set DB
            if (Repository == null)
            {
                string databaseName = "Project2FA.db";
                string databasePath;
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        SQLitePCL.Batteries_V2.Init();
                        databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", databaseName);
                        break;
                    case Device.Android:
                        databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName);
                        break;
                    default:
                        throw new NotImplementedException("Platform not supported");
                }
                var dbOptions = new DbContextOptionsBuilder<Project2FAContext>().UseSqlite("Data Source=" + databasePath);
                Repository = new DBProject2FARepository(dbOptions);
            }

            if (await Repository.Password.GetAsync() is null)
            {
                navigationPath = "NavigationPage/WelcomePage";
            }
            else
            {
                loginRequiered = true;
            }

            if (loginRequiered)
            {
                //var loginPage = Container.Resolve<LoginPage>();
                navigationPath = "NavigationPage/LoginPage";
                //Window.Current.Content = loginPage;
            }

            await NavigationService.NavigateAsync(navigationPath);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //custom services
            containerRegistry.RegisterSingleton<IProject2FARepository, DBProject2FARepository>();
            containerRegistry.RegisterSingleton<INewtonsoftJSONService, NewtonsoftJSONService>();
            containerRegistry.RegisterSingleton<IProject2FAParser, Project2FAParser>();
            containerRegistry.RegisterSingleton<IFileService, FileService>();


            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<AccountCodePage, AccountCodePageViewModel>();
            containerRegistry.RegisterForNavigation<WelcomePage, WelcomePageViewModel>();
            containerRegistry.RegisterForNavigation<SettingPage, SettingPageViewModel>();
            containerRegistry.RegisterForNavigation<EditAccountPage, EditAccountPageViewModel>();
            containerRegistry.RegisterForNavigation<AddAccountTabbedPage, AddAccountTabbedPageViewModel>();
            containerRegistry.RegisterForNavigation<UseDatafilePage, UseDatafilePageViewModel>();

        }
    }
}
