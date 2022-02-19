using Project2FA.UWP.ViewModels;
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
    }
}
