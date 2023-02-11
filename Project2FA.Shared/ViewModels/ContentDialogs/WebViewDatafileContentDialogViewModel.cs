using Prism.Mvvm;
using Prism.Ioc;
using Project2FA.Services.WebDAV;
using System.Threading.Tasks;
using WebDAVClient.Types;
using Project2FA.Core;
using System.Windows.Input;
using Prism.Commands;
using Project2FA.Repository.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;

#if WINDOWS_UWP
using Project2FA.UWP;
#else
using Project2FA.UNO;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class WebViewDatafileContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        private string _webDAVServerBackgroundUrl;
        private bool _isLoading;
        private bool _chooseItemPossible;
        private bool _createDatafileCase;
        private string _title;
        private string _webDAVProductName;
        private WebDAVFileOrFolderModel _selectedItem;
        private string _primaryButtonText;
        public ICommand WebDAVBackCommand { get; }
        public ICommand PrimaryButtonCommand { get; }

        public WebViewDatafileContentDialogViewModel()
        {
            WebDAVBackCommand = new RelayCommand(() =>
            {
                Directory.SelectedPathIndex = Directory.GetPathDepth - 1;
            });
            PrimaryButtonCommand = new RelayCommand(() =>
            {
                if (SelectedItem != null)
                {
                    ChoosenOneDatafile = SelectedItem;
                }
            });

        }

        public void Initialize(IDialogParameters parameters)
        {
            parameters.TryGetValue<bool>("CreateDatafileCase", out _createDatafileCase);
            if (parameters.TryGetValue<Status>("Status", out Status result))
            {
                LoadProperties(result);
            }
            Title = _createDatafileCase ? Strings.Resources.CreateDatafile : Strings.Resources.LoadDatafile;
            PrimaryButtonText = _createDatafileCase ? 
                Strings.Resources.WebViewDatafileContentDialogChooseFolder :
                Strings.Resources.WebViewDatafileContentDialogChooseFile;
            StartLoading(_createDatafileCase);
        }

        private void LoadProperties(Status result)
        {
            WebDAVProductName = result.Productname;
            ISecretService secretService = App.Current.Container.Resolve<ISecretService>();
            WebDAVServerBackgroundUrl = secretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress") + "/index.php/apps/theming/image/background";
        }

        private Task StartLoading(bool createDatafile)
        {
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
                if (_createDatafileCase && value is null && _selectedItem != null)
                {
                    return;
                }
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
                        else if(value.IsDirectory && _createDatafileCase)
                        {
                            ChooseItemPossible = true;
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
        public string Title 
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }
        public string PrimaryButtonText 
        { 
            get => _primaryButtonText;
            set => SetProperty(ref _primaryButtonText, value);
        }
    }
}
