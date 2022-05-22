using Windows.UI.Xaml.Controls;
namespace Project2FA.UWP.Views
{
    public sealed partial class BlankPage : Page
    {
        public BlankPage()
        {
            this.InitializeComponent();
            this.Loaded += BlankPage_Loaded;
        }

        private void BlankPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.ShellPageInstance.ShellViewInternal.Header = string.Empty;
            App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
        }
    }
}
