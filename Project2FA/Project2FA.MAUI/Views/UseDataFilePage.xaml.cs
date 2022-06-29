using Project2FA.MAUI.ViewModels;

namespace Project2FA.MAUI.Views;

public partial class UseDataFilePage : ContentPage
{
	public UseDataFilePageViewModel ViewModel => BindingContext as UseDataFilePageViewModel;
    public UseDataFilePage(UseDataFilePageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}