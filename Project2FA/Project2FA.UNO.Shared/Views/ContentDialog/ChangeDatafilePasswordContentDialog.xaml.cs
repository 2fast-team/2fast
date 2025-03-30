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

    public sealed partial class ChangeDatafilePasswordContentDialog : ContentDialog
    {
        public ChangeDatafilePasswordContentDialogViewModel ViewModel => DataContext as ChangeDatafilePasswordContentDialogViewModel;
        public ChangeDatafilePasswordContentDialog()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
