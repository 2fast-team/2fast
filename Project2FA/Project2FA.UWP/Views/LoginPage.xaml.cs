using Project2FA.UWP.Services;
using Project2FA.UWP.Services.Enums;
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
            if (SettingsService.Instance.UseRoundCorner)
            {
                App.Current.Resources["ControlCornerRadius"] = new CornerRadius(4, 4, 4, 4);
                App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(8, 8, 8, 8);
            }
            else
            {
                App.Current.Resources["ControlCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(0);
            }
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (SettingsService.Instance.OriginalAppTheme == ApplicationTheme.Dark)
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case Theme.Dark:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    break;
                case Theme.Light:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    break;
                default:
                    break;
            }
            if (System.Diagnostics.Debugger.IsAttached || SettingsService.Instance.PrideMonthDesign)
            {
                PageStaticBackgroundBorder.Visibility = Visibility.Visible;
                PageImageBackgroundBorder.Visibility = Visibility.Collapsed;
            }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ViewModel.CheckCapabilityWindowsHello();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void LoginKeydownCheckEnterSubmit(object sender, KeyRoutedEventArgs e)
        {
            ViewModel.LoginWithEnterKeyDown(e);
        }
    }
}
