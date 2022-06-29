using Project2FA.MAUI.ViewModels;

namespace Project2FA.MAUI.Views;

public partial class AccountCodePage : ContentPage
{
	public AccountCodePageViewModel ViewModel => BindingContext as AccountCodePageViewModel;
    public AccountCodePage(AccountCodePageViewModel vm)
	{
		InitializeComponent();
		BindingContext	= vm;
	}
}