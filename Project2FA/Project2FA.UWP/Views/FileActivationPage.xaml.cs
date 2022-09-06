using Project2FA.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class FileActivationPage : Page
    {
        public FileActivationPageViewModel ViewModel => DataContext as FileActivationPageViewModel;
        public FileActivationPage()
        {
            this.InitializeComponent();
        }

        private void LoginKeydownCheckEnterSubmit(object sender, KeyRoutedEventArgs e)
        {

        }
    }
}
