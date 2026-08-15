using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Project2FA.ViewModels;
using System;
using Windows.System;

namespace Project2FA.Uno.Views;

public sealed partial class WebDAVAuthContentDialog : ContentDialog
{
    public WebDAVAuthContentDialogViewModel ViewModel => DataContext as WebDAVAuthContentDialogViewModel;

    public WebDAVAuthContentDialog()
    {
        this.InitializeComponent();
        // Refresh x:Bind when the DataContext changes.
        DataContextChanged += (s, e) => Bindings.Update();
    }

    /// <summary>
    /// Opens the Nextcloud Login Flow v2 URL in the default system browser.
    /// On Linux/Desktop this uses xdg-open via Uno's Launcher implementation.
    /// </summary>
    private async void OpenBrowserButton_Click(object sender, RoutedEventArgs e)
    {
        var url = ViewModel?.LoginUrl;
        if (!string.IsNullOrEmpty(url))
        {
            await Launcher.LaunchUriAsync(new Uri(url));
        }
    }

    private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        ViewModel?.CancelPolling();
    }
}
