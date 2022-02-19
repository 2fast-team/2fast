using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{

    public sealed partial class SplashPage : Page
    {
        public SplashPage(SplashScreen splashScreen)
        {
            this.InitializeComponent();
            Window.Current.Activate();
        }
    }
}
