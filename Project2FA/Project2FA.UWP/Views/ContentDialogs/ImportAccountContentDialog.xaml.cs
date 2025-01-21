using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UNOversal.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Project2FA.UWP.Views
{
    public sealed partial class ImportAccountContentDialog : ContentDialog
    {
        public ImportAccountContentDialogViewModel ViewModel => DataContext as ImportAccountContentDialogViewModel;
        public ImportAccountContentDialog()
        {
            this.InitializeComponent();
            this.Loaded += ImportAccountContentDialog_Loaded;
        }

        private void ImportAccountContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (RequestedTheme != SettingsService.Instance.OriginalAppTheme.ToElementTheme())
                    {
                        RequestedTheme = SettingsService.Instance.OriginalAppTheme.ToElementTheme();
                    }
                    break;
                case Theme.Dark:
                    if (RequestedTheme != ElementTheme.Dark)
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    break;
                case Theme.Light:
                    if (RequestedTheme != ElementTheme.Light)
                    {
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                default:
                    break;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void HLBTN_QRCodeInfo(object sender, RoutedEventArgs e)
        {

        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TwoFACodeModel model)
            {
                if (model.IsEnabled)
                {
                    model.IsChecked = !model.IsChecked;
                }
            }
        }
    }
}
