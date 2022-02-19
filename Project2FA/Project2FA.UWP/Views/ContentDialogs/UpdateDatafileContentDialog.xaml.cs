using Project2FA.UWP.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class UpdateDatafileContentDialog : ContentDialog
    {
        public UpdateDatafileContentDialogViewModel ViewModel => DataContext as UpdateDatafileContentDialogViewModel;   

        /// <summary>
        /// Constructor
        /// </summary>
        public UpdateDatafileContentDialog()
        {
            this.InitializeComponent();
            SetPivotItem();
        }

        private async void SetPivotItem()
        {
            var dbAppFile = await App.Repository.Datafile.GetAsync();
            if(dbAppFile.IsWebDAV)
            {
                MainPivot.Items.Remove(FolderPivotItem);
            }
            else
            {
                MainPivot.Items.Remove(WebDAVPivotItem);
                MainPivot.Items.Remove(WebDAVFolderPivotItem);
            }
        }

        /// <summary>
        /// Handle click on submit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //first check if the given password matches with the datafile
            if (await ViewModel.TestPassword())
            {
                ViewModel.UpdateLocalFileDB();
                Hide();
            }
            else
            {
                args.Cancel = true;
            }
        }
    }
}
