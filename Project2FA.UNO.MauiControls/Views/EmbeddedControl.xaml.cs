using Microsoft.Maui.Controls;
using Project2FA.UNO.MauiControls.ViewModel;
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
        ViewModel.CameraReader = cameraBarcodeReaderView;
    }

    private void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        foreach (var barcode in e.Results)
        {
            ViewModel.Parameter = barcode.Value;
        }
    }
}
