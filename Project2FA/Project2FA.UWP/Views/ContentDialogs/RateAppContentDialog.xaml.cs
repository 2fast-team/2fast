using Template10.Services.Marketplace;
using Windows.UI.Xaml;
using Prism.Ioc;
using Windows.UI.Xaml.Controls;
using Project2FA.UWP.Services;

namespace Project2FA.UWP.Views
{
    public sealed partial class RateAppContentDialog : ContentDialog
    {
        public RateAppContentDialog()
        {
            this.InitializeComponent();
            //if (SystemInformation.Instance.LaunchCount >= 15)
            //{
            //    BTN_RateAppLater.Visibility = Visibility.Collapsed;
            //}
        }

        private async void BTN_RateAppYes_Click(object sender, RoutedEventArgs e)
        {
            var setting = SettingsService.Instance;
            var service = App.Current.Container.Resolve<IMarketplaceService>();
            setting.AppRated = true;
            await service.LaunchAppReviewInStoreAsync();
            Hide();
        }

        private void BTN_RateAppNo_Click(object sender, RoutedEventArgs e)
        {
            var setting = SettingsService.Instance;
            setting.AppRated = true;
            Hide();
        }

        private void BTN_RateAppLater_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
