using Project2FA.MAUI.ViewModels;

namespace Project2FA.MAUI.Views;

public partial class FileActivationPage : ContentPage
{
	public FileActivationPageViewModel ViewModel => BindingContext as FileActivationPageViewModel;
    public FileActivationPage(FileActivationPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}