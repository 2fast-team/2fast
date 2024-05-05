using Microsoft.UI.Xaml.Controls;
using Project2FA.ViewModels;

namespace Project2FA.UNO.Views
{
    public sealed partial class EditAccountPage : Page
    {
        public EditAccountPageViewModel ViewModel => DataContext as EditAccountPageViewModel;
        public EditAccountPage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
