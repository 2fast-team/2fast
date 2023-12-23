using Microsoft.Maui.Controls;
using System;

namespace Project2FA.UNO.MauiControls;

public partial class EmbeddedControl : ContentView
{
    EmbeddedControlViewModel ViewModel { get; } = new EmbeddedControlViewModel();
    public EmbeddedControl()
    {
        InitializeComponent();
    }

    private void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        foreach (var barcode in e.Results)
        {
            ViewModel.Parameter = barcode.Value;
        }
    }
}
