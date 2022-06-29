using Project2FA.Core.Services;
using Project2FA.MAUI.Services;
using Project2FA.MAUI.ViewModels;

namespace Project2FA.MAUI.Views;

public partial class LoginPage : ContentPage
{
	public LoginPageViewModel ViewModel => BindingContext as LoginPageViewModel;
    public LoginPage(LoginPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}


}