using Project2FA.Repository.Models;
using Project2FA.UWP.ViewModels.ContentDialogs;
using WebDAVClient.Types;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Project2FA.UWP.Views.ContentDialogs
{
    public sealed partial class WebViewDatafileContentDialog : ContentDialog
    {
        public WebViewDatafileContentDialogViewModel ViewModel = new WebViewDatafileContentDialogViewModel();

        public WebViewDatafileContentDialog(bool createDatafileCase, Status result)
        {
            this.InitializeComponent();
            ViewModel.LoadProperties(result);
            Title = createDatafileCase ? Strings.Resources.CreateDatafile : Strings.Resources.LoadDatafile;
            ViewModel.StartLoading(createDatafileCase);
        }

        private void WebDAVItemTemplate_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement).DataContext is ResourceInfoModel resourceInfo)
            {
                if (resourceInfo.IsDirectory)
                {
                    ViewModel.Directory.PathStack.Add(new PathInfoModel
                    {
                        ResourceInfo = resourceInfo,
                        PathIndex = ViewModel.Directory.GetPathDepth + 1
                    });
                    ViewModel.Directory.SelectedPathIndex = ViewModel.Directory.GetPathDepth;
                }
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void BreadcrumbBar_ItemClicked(Microsoft.UI.Xaml.Controls.BreadcrumbBar sender, Microsoft.UI.Xaml.Controls.BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Item is PathInfoModel model)
            {
                // set the new index => load and navigate to the new index
                ViewModel.Directory.SelectedPathIndex = model.PathIndex;
            }
        }
    }
}
