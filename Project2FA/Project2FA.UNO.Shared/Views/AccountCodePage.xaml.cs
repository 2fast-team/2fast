using Prism.Ioc;
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
using Prism;
using Windows.UI;
using Project2FA.Repository.Models;
using UNOversal.Services.Dialogs;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using CommunityToolkit.Mvvm.Input;

namespace Project2FA.UNO.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountCodePage : Page
    {
        public AccountCodePageViewModel ViewModel => DataContext as AccountCodePageViewModel;
        public AccountCodePage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
        }



        private void CreateTeachingTip(FrameworkElement element)
        {
            //TeachingTip teachingTip = new TeachingTip
            //{
            //    Target = element,
            //    Content = Strings.Resources.AccountCodePageCopyCodeTeachingTip,

            //    BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
            //    IsOpen = true,
            //};
            //MainGrid.Children.Add(teachingTip);
        }

        /// <summary>
        /// Copies the current generated TOTP of the entry into the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BTN_CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                //if (await Copy2FACodeToClipboard(model))
                //{
                //    CreateTeachingTip(sender as FrameworkElement);
                //}
            }
        }

        private async void BTN_EditItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                await ViewModel.EditAccountFromCollection(model);
            }
        }

        private async void BTN_AddAccountManual_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.AddAccountManual();
        }

        private async void BTN_AddAccountCamera_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.AddAccountWithCamera();
        }

        private async void BTN_ShareItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                await ViewModel.ExportQRCode(model);
            }
        }

        private async void BTN_DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                await ViewModel.DeleteAccountFromCollection(model);   
            }
        }
    }
}
