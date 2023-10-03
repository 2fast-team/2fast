using Project2FA.UWP.Controls;
using Project2FA.ViewModels;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Project2FA.UWP.Views
{
    public sealed partial class NewDataFilePage : Page
    {
        public NewDataFilePageViewModel ViewModel => DataContext as NewDataFilePageViewModel;
        public NewDataFilePage()
        {
            this.InitializeComponent();
            this.Loaded += NewDataFilePage_Loaded;
        }

        private void NewDataFilePage_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivot.Items.Remove(FolderPivotItem);
            MainPivot.Items.Remove(WebDAVPivotItem);
            App.ShellPageInstance.ShellViewInternal.Header = string.Empty;
            App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
        }

        private void HLBTN_PasswordInfo(object sender, RoutedEventArgs e)
        {
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = sender as FrameworkElement,
                Title = Strings.Resources.NewDatafilePasswordInfoTitle,
                Subtitle = Strings.Resources.NewDatafilePasswordInfo,
                AutoCloseInterval = 8000,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true,
            };
            RootGrid.Children.Add(teachingTip);
        }

        private async void BTN_LocalPath_Click(object sender, RoutedEventArgs e)
        {
            if (!MainPivot.Items.Contains(FolderPivotItem))
            {
                MainPivot.Items.Add(FolderPivotItem);
            }
            ViewModel.SelectWebDAV = false;
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
            MainPivot.Items.Remove(FolderPivotItem);
            if (!MainPivot.Items.Contains(WebDAVPivotItem))
            {
                MainPivot.Items.Add(WebDAVPivotItem);
            }
            ViewModel.SelectedIndex = 1;
            ViewModel.SelectWebDAV = true;
            ViewModel.ChooseWebDAV();
        }

        private void UseDatafileContentDialogWDLogin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PB_LocalPassword_KeyDown(object sender, KeyRoutedEventArgs e)
        {

        }

        private void HLBTN_WDPasswordInfo(object sender, RoutedEventArgs e)
        {
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = sender as FrameworkElement,
                Subtitle = Strings.Resources.WebDAVAppPasswordInfo,
                AutoCloseInterval = 8000,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true,
            };
            RootGrid.Children.Add(teachingTip);
        }
    }
}
