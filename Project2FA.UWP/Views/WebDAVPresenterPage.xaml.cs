using Project2FA.Repository.Models;
using Project2FA.UWP.ViewModels;
using WebDAVClient.Types;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Project2FA.UWP.Views
{
    public sealed partial class WebDAVPresenterPage : Page
    {
        public bool CreateDatafile { get; set; }
        public WebDAVPresenterPageViewModel ViewModel = new WebDAVPresenterPageViewModel();
        public WebDAVPresenterPage()
        {
            this.InitializeComponent();
        }

        private void WebDAVItemTemplate_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement).DataContext is ResourceInfoModel resourceInfo)
            {
                if (resourceInfo.IsDirectory)
                {
                    ViewModel.Directory.PathStack.Add(new PathInfoModel
                    {
                        ResourceInfo = resourceInfo
                    });
                    ViewModel.Directory.SelectedPathIndex = ViewModel.Directory.GetPathDepth;
                }
            }
        }
    }
}
