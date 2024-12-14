using Project2FA.Extensions;
using Project2FA.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class SettingPage : Page
    {
        public SettingPageViewModel ViewModel => DataContext as SettingPageViewModel;
        public SettingPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingPage_Loaded;
        }

        private void SettingPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.ShellPageInstance.ShellViewInternal.Header = string.Empty;
            App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
        }

        private void SettingsExpander_Expanded(object sender, System.EventArgs e)
        {
            SV_SettingsOptions.ScrollToElement(sender as FrameworkElement);
        }
    }
}
