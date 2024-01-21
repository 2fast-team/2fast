using Project2FA.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Project2FA.UWP.Views
{
    public sealed partial class InAppPaymentContentDialog : ContentDialog
    {
        public InAppPaymentContentDialogViewModel ViewModel => DataContext as InAppPaymentContentDialogViewModel;
        public InAppPaymentContentDialog()
        {
            this.InitializeComponent();
            //this.Loaded += InAppPaymentContentDialog_Loaded;

        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
