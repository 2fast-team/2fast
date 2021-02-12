using Project2FA.UWP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Project2FA.UWP.Views
{
    public sealed partial class LoginPage : Page
    {
        readonly LoginPageViewModel ViewModel = new LoginPageViewModel();
        readonly bool _isLogout;
        public LoginPage(bool isLogout = false)
        {
            this.InitializeComponent();
            _isLogout = isLogout;
            this.Loaded += LoginPage_Loaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);
            ViewModel.IsLogout = _isLogout;
        }

        private void LoginKeydownCheckEnterSubmit(object sender, KeyRoutedEventArgs e)
        {
            ViewModel.LoginWithEnterKeyDown(e);
        }
    }
}
