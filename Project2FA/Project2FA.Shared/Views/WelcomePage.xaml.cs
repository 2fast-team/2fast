using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Project2FA.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Project2FA.Views
{
    public sealed partial class WelcomePage : Page
    {
        public WelcomePageViewModel ViewModel => DataContext as WelcomePageViewModel;
        public WelcomePage()
        {
            this.InitializeComponent();
        }
    }
}
