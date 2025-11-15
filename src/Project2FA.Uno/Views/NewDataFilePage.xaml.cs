using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project2FA.ViewModels;

namespace Project2FA.Uno.Views
{
    public sealed partial class NewDataFilePage : Page
    {
        public NewDataFilePageViewModel ViewModel => DataContext as NewDataFilePageViewModel;
        public NewDataFilePage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
        }

        private void HLBTN_PasswordInfo(object sender, RoutedEventArgs e)
        {
            //AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            //{
            //    Target = sender as FrameworkElement,
            //    Title = Strings.Resources.NewDatafilePasswordInfoTitle,
            //    Subtitle = Strings.Resources.NewDatafilePasswordInfo,
            //    AutoCloseInterval = 8000,
            //    IsLightDismissEnabled = true,
            //    BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
            //    IsOpen = true,
            //};
            //RootGrid.Children.Add(teachingTip);
        }

        private async void BTN_LocalPath_Click(object sender, RoutedEventArgs e)
        {
            //if (!MainPivot.Items.Contains(FolderPivotItem))
            //{
            //    MainPivot.Items.Add(FolderPivotItem);
            //}
            //ViewModel.SelectWebDAV = false;
            //bool result = await ViewModel.SetLocalPath();
            //if (!result)
            //{
            //    if (MainPivot.Items.Contains(FolderPivotItem))
            //    {
            //        MainPivot.Items.Remove(FolderPivotItem);
            //    }
            //}
        }

        private void BTN_WebDAV_Click(object sender, RoutedEventArgs e)
        {
            //MainPivot.Items.Remove(FolderPivotItem);
            //if (!MainPivot.Items.Contains(WebDAVPivotItem))
            //{
            //    MainPivot.Items.Add(WebDAVPivotItem);
            //}
            //ViewModel.SelectedIndex = 1;
            //ViewModel.SelectWebDAV = true;
            //ViewModel.ChooseWebDAV();
        }

        private void HLBTN_WDPasswordInfo(object sender, RoutedEventArgs e)
        {
            //AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            //{
            //    Target = sender as FrameworkElement,
            //    Subtitle = Strings.Resources.WebDAVAppPasswordInfo,
            //    AutoCloseInterval = 8000,
            //    IsLightDismissEnabled = true,
            //    BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
            //    IsOpen = true,
            //};
            //RootGrid.Children.Add(teachingTip);
        }
    }
}
