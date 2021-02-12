using Project2FA.Core.Services;
using Project2FA.UWP.Services;
using Project2FA.UWP.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    /// <summary>
    /// Content Dialog for changing the password of the datafile
    /// </summary>
    public sealed partial class ChangeDatafilePasswordContentDialog : ContentDialog
    {
        public ChangeDatafilePasswordContentDialogViewModel ViewModel;

        /// <summary>
        /// Constructor, initialize view model
        /// </summary>
        public ChangeDatafilePasswordContentDialog()
        {
            this.InitializeComponent();
            ViewModel = new ChangeDatafilePasswordContentDialogViewModel();
        }

        /// <summary>
        /// Constructor, initialize view model
        /// </summary>
        /// <param name="invalidPassword">When true, </param>
        public ChangeDatafilePasswordContentDialog(bool invalidPassword)
        {
            this.InitializeComponent();
            ViewModel = new ChangeDatafilePasswordContentDialogViewModel();
            ViewModel.InvalidPassword = invalidPassword;
        }

        /// <summary>
        /// Action after the "change password" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var useArgonHash = SettingsService.Instance.UseExtendedHash;
            // first check if the given current password matches with the datafile
            if ((await App.Repository.Password.GetAsync()).Hash == CryptoService.CreateStringHash(ViewModel.CurrentPassword, useArgonHash))
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

        private void BTN_ConfirmBadPassword_Click(object sender, RoutedEventArgs e)
        {
            PB_CurrentPassword.Focus(FocusState.Programmatic);
        }
    }
}
