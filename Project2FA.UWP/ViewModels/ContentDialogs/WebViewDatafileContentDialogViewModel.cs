using Prism.Mvvm;
using Prism.Ioc;
using Project2FA.UWP.Services.WebDAV;
using System.Threading.Tasks;
using Template10.Services.Secrets;
using WebDAVClient.Types;
using Project2FA.Core;
using System.Windows.Input;
using Prism.Commands;
using Project2FA.Repository.Models;

namespace Project2FA.UWP.ViewModels.ContentDialogs
{
    public class WebViewDatafileContentDialogViewModel : BindableBase
    {
        private string _webDAVServerBackgroundUrl;
        private bool _createDatafile;
        private bool _isLoading;
        private bool _chooseItemPossible;
        private string _webDAVProductName;
        private WebDAVFileOrFolderModel _selectedItem;
        public ICommand WebDAVBackCommand { get; }
        public ICommand PrimaryButtonCommand { get; }

        public WebViewDatafileContentDialogViewModel()
        {
            WebDAVBackCommand = new DelegateCommand(() =>
            {
                Directory.SelectedPathIndex = Directory.GetPathDepth - 1;
            });
            PrimaryButtonCommand = new DelegateCommand(() =>
            {
                if (SelectedItem != null)
                {
                    ChoosenOneDatafile = SelectedItem;
                }
            });
        }

        public void LoadProperties(Status result)
        {
            WebDAVProductName = result.Productname;
            ISecretService secretService = App.Current.Container.Resolve<ISecretService>();
            WebDAVServerBackgroundUrl = secretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress") + "/index.php/apps/theming/image/background";
        }

        public Task StartLoading(bool createDatafile)
        {
            _createDatafile = createDatafile;
            return Directory.StartDirectoryListing(createDatafile);
        }

        public WebDAVDirectoryService Directory => WebDAVDirectoryService.Instance;

        public string WebDAVServerBackgroundUrl
        {
            get => _webDAVServerBackgroundUrl;
            set => SetProperty(ref _webDAVServerBackgroundUrl, value);
        }
        public string WebDAVProductName
        {
            get => _webDAVProductName;
            set => SetProperty(ref _webDAVProductName, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public WebDAVFileOrFolderModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    bool notvalid = false;
                    if (value != null)
                    {
                        if (value.ContentType != null)
                        {
                            if (value.Name.Contains("2fa"))
                            {
                                ChooseItemPossible = true;
                            }
                        }
                        else
                        {
                            notvalid = true;
                        }
                    }
                    else
                    {
                        notvalid = true;
                    }
                    if (notvalid)
                    {
                        ChooseItemPossible = false;
                    }
                }
            }
        }

        public bool ChooseItemPossible 
        {
            get => _chooseItemPossible;
            set => SetProperty(ref _chooseItemPossible, value);
        }
        public WebDAVFileOrFolderModel ChoosenOneDatafile { get; private set; }
    }
}
