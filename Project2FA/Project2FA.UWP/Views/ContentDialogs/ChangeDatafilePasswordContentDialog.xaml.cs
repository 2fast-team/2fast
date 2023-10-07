using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Project2FA.ViewModels;
using Project2FA.Core.Services.Crypto;

namespace Project2FA.UWP.Views
{
    /// <summary>
    /// Content Dialog for changing the password of the datafile
    /// </summary>
    public sealed partial class ChangeDatafilePasswordContentDialog : ContentDialog
    {
        public ChangeDatafilePasswordContentDialogViewModel ViewModel => DataContext as ChangeDatafilePasswordContentDialogViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public ChangeDatafilePasswordContentDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Action after the "change password" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string hash = await ViewModel.GetCurrentPasswordHash();
            // first check if the given current password matches with the datafile
            if (hash == CryptoService.CreateStringHash(ViewModel.CurrentPassword))
            {
                if (await ViewModel.TestPassword())
                {
                    await ViewModel.ChangePasswordInFileAndDB();
                }
                else
                {
                    // prevent the close of this ContentDialog
                    args.Cancel = true;
                    ViewModel.ShowError = true;
                }
            }
            else
            {
                // prevent the close of this ContentDialog
                args.Cancel = true;
                ViewModel.ShowError = true;
            }
        }

        private void BTN_ConfirmBadPassword_Click(object sender, RoutedEventArgs e)
        {
            PB_CurrentPassword.Focus(FocusState.Programmatic);
        }
    }
}
