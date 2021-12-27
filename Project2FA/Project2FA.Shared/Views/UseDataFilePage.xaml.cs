using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
#if HAS_WINUI
using Microsoft.UI.Xaml.Controls;
#else
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
#endif

namespace Project2FA.Views
{
    public sealed partial class UseDataFilePage : Page
    {
        public UseDataFilePage()
        {
            this.InitializeComponent();
        }
    }
}
