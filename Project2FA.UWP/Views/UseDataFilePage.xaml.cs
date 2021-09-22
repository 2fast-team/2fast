using Project2FA.UWP.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Project2FA.UWP.Views
{
    public sealed partial class UseDataFilePage : Page
    {
        public UseDataFilePageViewModel ViewModel => DataContext as UseDataFilePageViewModel;
        public UseDataFilePage()
        {
            this.InitializeComponent();
            this.Loaded += UseDataFilePage_Loaded;
        }

        private void UseDataFilePage_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivot.Items.Remove(FolderPivotItem);
            MainPivot.Items.Remove(WebDAVPivotItem);
            App.ShellPageInstance.ShellViewInternal.Header = string.Empty;
            App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
        }

        private async void PB_LocalPassword_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrEmpty(ViewModel.Password))
            {
                if (MainPivot.Items.Contains(WebDAVPivotItem))
                {
                    await ViewModel.SetAndCheckLocalWebDAVDatafile();
                }
                else
                {
                    await ViewModel.SetAndCheckLocalDatafile();
                }
            }
        }

        private async void BTN_LocalFile_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MainPivot.Items.Remove(WebDAVPivotItem);
            if (!MainPivot.Items.Contains(FolderPivotItem))
            {
                MainPivot.Items.Add(FolderPivotItem);
            }
            bool result = await ViewModel.UseExistDatafile();
            if (!result)
            {
                if (MainPivot.Items.Contains(FolderPivotItem))
                {
                    MainPivot.Items.Remove(FolderPivotItem);
                }
            }
            PB_LocalPassword.Focus(FocusState.Programmatic);
        }

        private void BTN_WebDAV_Click(object sender, RoutedEventArgs e)
        {
            MainPivot.Items.Remove(FolderPivotItem);
            if (!MainPivot.Items.Contains(WebDAVPivotItem))
            {
                MainPivot.Items.Add(WebDAVPivotItem);
            }
            ViewModel.SelectedIndex = 1;
            ViewModel.ChooseWebDAV();
        }
    }
}
