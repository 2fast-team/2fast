using Microsoft.UI.Xaml.Controls;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Project2FA.UWP.Views
{
    public sealed partial class FileActivationPage : Page
    {
        readonly FileActivationPageViewModel ViewModel = new FileActivationPageViewModel();
        public FileActivationPage()
        {
            this.InitializeComponent();
            this.Loaded += FileActivationPage_Loaded;
        }

        private void FileActivationPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);
            if (SettingsService.Instance.UseRoundCorner)
            {
                App.Current.Resources["ControlCornerRadius"] = new CornerRadius(4, 4, 4, 4);
                App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(8, 8, 8, 8);
            }
            else
            {
                App.Current.Resources["ControlCornerRadius"] = new CornerRadius(0);
                App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(0);
            }
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (SettingsService.Instance.OriginalAppTheme == ApplicationTheme.Dark)
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case Theme.Dark:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    break;
                case Theme.Light:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    break;
                default:
                    break;
            }
            if (System.Diagnostics.Debugger.IsAttached || SettingsService.Instance.PrideMonthDesign)
            {
                PageStaticBackgroundBorder.Visibility = Visibility.Visible;
                PageImageBackgroundBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void LoginKeydownCheckEnterSubmit(object sender, KeyRoutedEventArgs e)
        {
            ViewModel.LoginWithEnterKeyDown(e);
        }

        private void HLBTN_NotAvailableInfo(object sender, RoutedEventArgs e)
        {
            TeachingTip teachingTip = new TeachingTip
            {
                Target = sender as FrameworkElement,
                MaxWidth = 400,
                Title = Strings.Resources.FunctionNotAvailable,
                Subtitle = Strings.Resources.WindowsHelloNotAvailable,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true
            };
            RootGrid.Children.Add(teachingTip);
        }
    }
}
