using Microsoft.EntityFrameworkCore;
using Project2FA.Core;
using Project2FA.MAUI.Services;
using Project2FA.MAUI.Views;
using Project2FA.Repository.Database;
using System.ComponentModel;

namespace Project2FA.MAUI;

public partial class App : Application
{
    /// <summary>
    /// Creates the access of the static instance of the ShellPage
    /// </summary>
    public static AppShell AppShellInstance { get; private set; } = new AppShell();
    /// <summary>
    /// Pipeline for interacting with database.
    /// </summary>
    public static IProject2FARepository Repository { get; private set; }
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
	{
		InitializeComponent();
        MainPage = AppShellInstance = new AppShell();
        UserAppTheme = SettingsService.Instance.AppStartSetTheme(RequestedTheme);
    }

    protected override void OnStart()
    {
        OnStartAsync();
    }

    protected async Task OnStartAsync()
    {
        //if (MainPage == null)
        //{

        //}
        // set DB
        if (Repository is null)
        {
            //FileSystem.Current.AppDataDirectory + 
            string databasePath = Path.Combine(FileSystem.AppDataDirectory, string.Format("{0}.db", Constants.ContainerName)); //string.Format(@"/{0}.db", Constants.ContainerName);
            var dbOptions = new DbContextOptionsBuilder<Project2FAContext>().UseSqlite("Data Source=" + databasePath);
            Repository = new DBProject2FARepository(dbOptions);
        }


        if (await Repository.Password.GetAsync() is not null)
        {
            //MainPage = new LoginPage(new ViewModels.LoginPageViewModel());
            await Shell.Current.GoToAsync("//" + nameof(LoginPage));
        }
        else
        {
            //MainPage = new AppShell();
            await Shell.Current.GoToAsync("//" + nameof(WelcomePage));
        }

        //remove the navigation stack
        //var existingPages = Shell.Current.Navigation.NavigationStack.ToList();
        //foreach (var page in existingPages)
        //{
        //    Shell.Current.Navigation.RemovePage(page);
        //}
    }
}
