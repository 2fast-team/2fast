using Project2FA.MAUI.ViewModels;
using Project2FA.Repository.Models;

namespace Project2FA.MAUI.Views;

public partial class AccountCodePage : ContentPage
{
	public AccountCodePageViewModel ViewModel => BindingContext as AccountCodePageViewModel;
    public AccountCodePage(AccountCodePageViewModel vm)
	{
		InitializeComponent();
		BindingContext	= vm;
		this.NavigatingFrom += AccountCodePage_NavigatingFrom;
        this.NavigatedTo += AccountCodePage_NavigatedTo;
	}

    private void AccountCodePage_NavigatedTo(object sender, NavigatedToEventArgs e)
    {
        ViewModel.StartTOTPLogic(true);
    }

    private void AccountCodePage_NavigatingFrom(object sender, NavigatingFromEventArgs e)
	{
        //detach the events
        if (ViewModel.DispatcherTOTPTimer.IsRunning)
        {
            ViewModel.DispatcherTOTPTimer.Stop();
            //ViewModel.DispatcherTOTPTimer.Tick -= ViewModel.TOTPTimer;
        }
        if (ViewModel.DispatcherTimerDeletedModel.IsRunning)
        {
            ViewModel.DispatcherTimerDeletedModel.Stop();
        }
        ViewModel.TwoFADataService.TOTPEventStopwatch.Reset();
    }

	private async void BTN_CopyToClipboard_Clicked(object sender, EventArgs e)
	{
  //      if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
		//{

		//}
            await Clipboard.Default.SetTextAsync("This text was highlighted in the UI.");
    }
}