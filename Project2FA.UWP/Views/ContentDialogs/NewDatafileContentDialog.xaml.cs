using Project2FA.UWP.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class NewDatafileContentDialog : ContentDialog
    {
        public NewDatafileContentDialogViewModel ViewModel;
        public NewDatafileContentDialog()
        {
            this.InitializeComponent();
            ViewModel = new NewDatafileContentDialogViewModel();
            Loaded += NewDatafileContentDialog_Loaded;
            MainPivot.Items.Remove(FolderPivotItem);
            MainPivot.Items.Remove(WebDAVPivotItem);
            MainPivot.Items.Remove(WebDAVFolderPivotItem);
        }

        private void NewDatafileContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.WebDAVViewer = WebDAVView;
        }

        private async void BTN_LocalPath_Click(object sender, RoutedEventArgs e)
        {
            if (!MainPivot.Items.Contains(FolderPivotItem))
            {
                MainPivot.Items.Add(FolderPivotItem);
            }
            if (MainPivot.Items.Contains(WebDAVPivotItem))
            {
                MainPivot.Items.Remove(WebDAVPivotItem);
            }
            bool result = await ViewModel.SetLocalPath();
            if (!result)
            {
                if (MainPivot.Items.Contains(FolderPivotItem))
                {
                    MainPivot.Items.Remove(FolderPivotItem);
                }
            }
        }

        private void BTN_WebDAV_Click(object sender, RoutedEventArgs e)
        {
            if (MainPivot.Items.Contains(FolderPivotItem))
            {
                MainPivot.Items.Remove(FolderPivotItem);
            }
            if (!MainPivot.Items.Contains(WebDAVPivotItem))
            {
                MainPivot.Items.Add(WebDAVPivotItem);
            }
            ViewModel.SelectedIndex = 1;
            //WebDAVFrame.Navigate(typeof(WebDAVLoginPage));
        }
    }
}
