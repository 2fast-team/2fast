using Microsoft.Web.WebView2.Core;
using Project2FA.ViewModels;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class WebDAVAuthContentDialog : ContentDialog
    {
        public WebDAVAuthContentDialogViewModel ViewModel => DataContext as WebDAVAuthContentDialogViewModel;

        public WebDAVAuthContentDialog()
        {
            this.InitializeComponent();
            this.Loaded += WebDAVAuthContentDialog_Loaded;
        }

        private async void WebDAVAuthContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Only initialize WebView2 when the embedded (v1) mode is active
            if (LoginWebView.Visibility == Visibility.Visible)
            {
                await InitializeWebView2Async();
            }

            // Watch for mode changes: initialize WebView2 when the user switches to v1
            ViewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(WebDAVAuthContentDialogViewModel.IsFlowV2Inverse)
                    && LoginWebView.Visibility == Visibility.Visible)
                {
                    _ = InitializeWebView2Async();
                }
            };
        }

        private async System.Threading.Tasks.Task InitializeWebView2Async()
        {
            await LoginWebView.EnsureCoreWebView2Async();
            LoginWebView.CoreWebView2.WebResourceRequested -= CoreWebView2_WebResourceRequested;

            // Intercept requests to inject the OCS-APIREQUEST header required by Login Flow v1
            if (ViewModel?.URL != null)
            {
                LoginWebView.CoreWebView2.AddWebResourceRequestedFilter(
                    ViewModel.URL.ToString(), CoreWebView2WebResourceContext.All);
            }
            LoginWebView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
        }

        private void CoreWebView2_WebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
        {
            // Only add the header to document (page) requests, not images/scripts/etc.
            if (args.ResourceContext != CoreWebView2WebResourceContext.Document)
                return;

            args.Request.Headers.SetHeader("OCS-APIREQUEST", "true");
        }

        /// <summary>
        /// Called by the "Open login in browser" HyperlinkButton – launches the Nextcloud
        /// Login Flow v2 URL in the default system browser.
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
}