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
using Project2FA.Services;

namespace Project2FA.UNO.Views
{
    public sealed partial class LoginPage : Page
    {
        readonly LoginPageViewModel ViewModel = new LoginPageViewModel();
        public LoginPage(bool isLogout = false)
        {
            this.InitializeComponent();
            ViewModel.IsLogout = isLogout;
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
            this.Loaded += LoginPage_Loaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached || SettingsService.Instance.PrideMonthDesign)
            {
                PageStaticBackgroundBorder.Visibility = Visibility.Visible;
                PageImageBackgroundBorder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
