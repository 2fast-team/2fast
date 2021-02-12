using Project2FA.UWP.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class DeleteDatafileContentDialog : ContentDialog
    {
        DeleteDatafileContentDialogViewModel ViewModel = new DeleteDatafileContentDialogViewModel();
        public DeleteDatafileContentDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //prevent the close of this ContentDialog
            args.Cancel = true;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
