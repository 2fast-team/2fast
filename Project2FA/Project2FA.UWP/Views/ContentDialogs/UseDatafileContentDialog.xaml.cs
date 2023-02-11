using Project2FA.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class UseDatafileContentDialog : ContentDialog
    {
        public UseDatafileContentDialogViewModel ViewModel => DataContext as UseDatafileContentDialogViewModel; 

        /// <summary>
        /// Constructor
        /// </summary>
        public UseDatafileContentDialog()
        {
            this.InitializeComponent();
            MainPivot.Items.Remove(FolderPivotItem);
            //MainPivot.Items.Remove(WebDAVPivotItem);
            //MainPivot.Items.Remove(WebDAVFolderPivotItem);
        }
        
        /// <summary>
        /// Handle click on local file button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BTN_LocalFile_Click(object sender, RoutedEventArgs e)
        {
            if (!MainPivot.Items.Contains(FolderPivotItem))
            {
                MainPivot.Items.Add(FolderPivotItem);
            }
            bool result = await ViewModel.SetLocalFile();
            if (!result)
            {
                if (MainPivot.Items.Contains(FolderPivotItem))
                {
                    MainPivot.Items.Remove(FolderPivotItem);
                }
            }
            PB_LocalPassword.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Handle click on submit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //prevent the close of this ContentDialog
            args.Cancel = true;
            //first check if the given password matches with the datafile
            if (await ViewModel.TestPassword())
            {
                await ViewModel.CreateLocalFileDB();
                args.Cancel = false;
                Hide();
            }
        }

        private async void PB_LocalPassword_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrEmpty(ViewModel.Password))
            {
                if (await ViewModel.TestPassword())
                {
                    await ViewModel.CreateLocalFileDB();
                    Hide();
                }
            }
        }
    }
}
