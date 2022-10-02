using Project2FA.MAUI.ViewModels;

namespace Project2FA.MAUI.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPageViewModel ViewModel => BindingContext as SettingsPageViewModel;
    public SettingsPage(SettingsPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}