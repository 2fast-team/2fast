using Project2FA.MAUI.ViewModels;
using Project2FA.MAUI.Views;

namespace Project2FA.MAUI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(BlankPage), typeof(BlankPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(WelcomePage), typeof(WelcomePage));
        Routing.RegisterRoute(nameof(UseDataFilePage), typeof(UseDataFilePage));
        Routing.RegisterRoute(nameof(AccountCodePage), typeof(AccountCodePage));
    }
}
