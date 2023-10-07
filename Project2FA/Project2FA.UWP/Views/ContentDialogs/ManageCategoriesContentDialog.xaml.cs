using Project2FA.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class ManageCategoriesContentDialog : ContentDialog
    {
        public ManageCategoriesContentDialogViewModel ViewModel => DataContext as ManageCategoriesContentDialogViewModel;
        public ManageCategoriesContentDialog()
        {
            this.InitializeComponent();
        }
    }
}
