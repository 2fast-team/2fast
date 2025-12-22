using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project2FA.Services;
using Project2FA.UnoApp;
using Project2FA.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UNOversal.Ioc;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Project2FA.Uno.Views;


public sealed partial class RateAppContentDialog : ContentDialog
{
    public RateAppContentDialog()
    {
        this.InitializeComponent();
        // Refresh x:Bind when the DataContext changes.
        DataContextChanged += (s, e) => Bindings.Update();
    }

    private async void BTN_RateAppYes_Click(object sender, RoutedEventArgs e)
    {
        // TODO for Android and iOS release
#if __ANDROID__ || __IOS__
        //await Windows.Services.Store.StoreContext.GetDefault().RequestRateAndReviewAppAsync();
#endif
        Hide();
    }

    private void BTN_RateAppNo_Click(object sender, RoutedEventArgs e)
    {
        var setting = SettingsService.Instance;
        setting.AppRated = true;
        Hide();
    }

    private void BTN_RateAppLater_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
