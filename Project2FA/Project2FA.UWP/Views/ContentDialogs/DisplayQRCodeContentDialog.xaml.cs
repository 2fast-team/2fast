using Project2FA.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class DisplayQRCodeContentDialog : ContentDialog
    {
        public DisplayQRCodeContentDialogViewModel ViewModel => DataContext as DisplayQRCodeContentDialogViewModel;
        public DisplayQRCodeContentDialog()
        {
            this.InitializeComponent();
        }
    }
}
