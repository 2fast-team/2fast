using Microsoft.UI.Xaml.Controls;
using Project2FA.ViewModels;
using Microsoft.UI.Xaml;
using Project2FA.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Project2FA.Uno.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WelcomePage : Page
    {
        public WelcomePageViewModel ViewModel => DataContext as WelcomePageViewModel;
        public WelcomePage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
            this.Loaded += WelcomePage_Loaded;
        }

        private void WelcomePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached || SettingsService.Instance.PrideMonthDesign)
            {
                PageStaticBackgroundBorder.Visibility = Visibility.Visible;
                PageImageBackgroundBorder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
