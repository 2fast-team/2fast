using Project2FA.Extensions;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class ManageCategoriesContentDialog : ContentDialog
    {
        public ManageCategoriesContentDialogViewModel ViewModel => DataContext as ManageCategoriesContentDialogViewModel;
        public ManageCategoriesContentDialog()
        {
            this.InitializeComponent();
            this.Loaded += ManageCategoriesContentDialog_Loaded;
        }

        private void ManageCategoriesContentDialog_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (RequestedTheme != SettingsService.Instance.OriginalAppTheme.ToElementTheme())
                    {
                        RequestedTheme = SettingsService.Instance.OriginalAppTheme.ToElementTheme();
                    }
                    break;
                case Theme.Dark:
                    if (RequestedTheme != ElementTheme.Dark)
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    break;
                case Theme.Light:
                    if (RequestedTheme != ElementTheme.Light)
                    {
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                default:
                    break;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.DataChanged = true;
        }
    }
}
