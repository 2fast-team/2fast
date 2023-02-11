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


namespace Project2FA.UNO.Views
{
    public sealed partial class UseDataFilePage : Page
    {
        public UseDataFilePageViewModel ViewModel => DataContext as UseDataFilePageViewModel;
        public UseDataFilePage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
            this.Loaded += UseDataFilePage_Loaded;
        }

        private void UseDataFilePage_Loaded(object sender, RoutedEventArgs e)
        {
            //MainPivot.Items.Remove(FolderPivotItem);
        }

        private async void PB_LocalPassword_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrEmpty(ViewModel.Password))
            {
                await ViewModel.SetAndCheckLocalDatafile();
            }
        }

        private async void BTN_LocalFile_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            //if (!MainPivot.Items.Contains(FolderPivotItem))
            //{
            //    MainPivot.Items.Add(FolderPivotItem);
            //}
            bool result = await ViewModel.UseExistDatafile();
            if (!result)
            {
                //if (MainPivot.Items.Contains(FolderPivotItem))
                //{
                //    MainPivot.Items.Remove(FolderPivotItem);
                //}
            }
            PB_LocalPassword.Focus(FocusState.Programmatic);
        }
    }
}
