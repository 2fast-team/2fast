using Project2FA.ViewModels;

namespace Project2FA.Uno.Views
{
    public sealed partial class SettingPage : Page
    {
        public SettingPageViewModel ViewModel => DataContext as SettingPageViewModel;
        public SettingPage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
