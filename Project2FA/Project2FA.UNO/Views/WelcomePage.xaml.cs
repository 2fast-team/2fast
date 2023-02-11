using Microsoft.UI.Xaml.Controls;
using Project2FA.ViewModels;
using Microsoft.UI.Xaml;

namespace Project2FA.UNO.Views
{

    public sealed partial class WelcomePage : Page
    {
        public WelcomePageViewModel ViewModel => DataContext as WelcomePageViewModel;
        public WelcomePage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
            PageStaticBackgroundBorder.Visibility = Visibility.Visible;
        }
    }
}
