using Project2FA.MAUI.ViewModels;

namespace Project2FA.MAUI.Views;

public partial class WelcomePage : ContentPage
{
    public WelcomePageViewModel ViewModel => BindingContext as WelcomePageViewModel;
    public WelcomePage(WelcomePageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}