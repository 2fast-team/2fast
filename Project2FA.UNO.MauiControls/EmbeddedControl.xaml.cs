using Microsoft.Maui.Controls;
using System;
using ZXing.Net.Maui;

namespace Project2FA.UNO.MauiControls;

public partial class EmbeddedControl : ContentView
{
    EmbeddedControlViewModel ViewModel { get; } = new EmbeddedControlViewModel();
    public EmbeddedControl()
    {
        InitializeComponent();
        cameraBarcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.TwoDimensional,
            AutoRotate = true,
            Multiple = true
        };
    }

    private void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        foreach (var barcode in e.Results)
        {
            ViewModel.Parameter = barcode.Value;
        }
    }
}
