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

	public sealed partial class AddAccountCameraPage : Page
	{
		public AddAccountCameraPageViewModel ViewModel => DataContext as AddAccountCameraPageViewModel;
        public AddAccountCameraPage()
		{
			this.InitializeComponent();
            this.Loaded += AddAccountCameraPage_Loaded;
		}

        private void AddAccountCameraPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
