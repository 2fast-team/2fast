using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project2FA.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Project2FA.Views
{
    public sealed partial class UseDataFilePage : Page
    {
        public UseDataFilePageViewModel ViewModel => DataContext as UseDataFilePageViewModel;
        public UseDataFilePage()
        {
            this.InitializeComponent();
        }
    }
}
