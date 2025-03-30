using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Project2FA.ViewModels;
using Project2FA.Services;


namespace Project2FA.Uno.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginPageViewModel ViewModel => DataContext as LoginPageViewModel;
        public LoginPage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
            this.Loaded += LoginPage_Loaded;
        }

        private async void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached || SettingsService.Instance.PrideMonthDesign)
            {
                PageStaticBackgroundBorder.Visibility = Visibility.Visible;
                PageImageBackgroundBorder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
