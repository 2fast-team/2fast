using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.UI.Xaml.Media;
using Project2FA.UnoApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project2FA.Uno.Droid
{

    [global::Android.App.ApplicationAttribute(
        Label = "@string/ApplicationName",
        Icon = "@mipmap/schluessel",
        LargeHeap = true,
        HardwareAccelerated = true,
        Theme = "@style/AppTheme",
        RequestLegacyExternalStorage = true,
        AllowBackup =false
    )]
    public class Application : Microsoft.UI.Xaml.NativeApplication
    {
        public Application(IntPtr javaReference, JniHandleOwnership transfer)
            : base(() => new App(), javaReference, transfer)
        {
            App.InitializeLogging();
        }
    }
}
