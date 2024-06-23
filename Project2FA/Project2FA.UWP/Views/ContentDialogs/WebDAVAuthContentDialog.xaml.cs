using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Project2FA.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WebUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            await LoginWebView.EnsureCoreWebView2Async();
            // Add a filter to select all resource types under http://www.example.com
            LoginWebView.CoreWebView2.AddWebResourceRequestedFilter(
                  ViewModel.Source.ToString(), CoreWebView2WebResourceContext.All);
            LoginWebView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
        }


        private void CoreWebView2_WebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
        {
            CoreWebView2WebResourceContext resourceContext = args.ResourceContext;
            // Only intercept the document resources
            if (resourceContext != CoreWebView2WebResourceContext.Document)
            {
                return;
            }
            CoreWebView2HttpRequestHeaders requestHeaders = args.Request.Headers;
            requestHeaders.SetHeader("OCS-APIREQUEST", "true");
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
