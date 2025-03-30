using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Project2FA.ViewModels;

namespace Project2FA.Uno.Views
{

    public sealed partial class FileActivationPage : Page
    {
        public FileActivationPageViewModel ViewModel => DataContext as FileActivationPageViewModel;
        public FileActivationPage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
        }

        private void LoginKeydownCheckEnterSubmit(object sender, KeyRoutedEventArgs e)
        {
            ViewModel.LoginWithEnterKeyDown(e);
        }
    }
}
