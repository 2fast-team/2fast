using Prism.Mvvm;
using Project2FA.Core.Services.WebDAV;

namespace Project2FA.UWP.ViewModels
{
    public class WebDAVPresenterPageViewModel : BindableBase
    {
        public WebDAVPresenterPageViewModel()
        {
            
        }

        public async void StartLoading(bool createDatafile)
        {
            await Directory.StartDirectoryListing(createDatafile);
        }
        public DirectoryService Directory
        {
            get => DirectoryService.Instance;
        }
    }
}
