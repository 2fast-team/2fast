using Prism.Commands;
using Prism.Mvvm;
using Project2FA.Repository.Models;
using Project2FA.UWP.Services;
using System.Windows.Input;
using System.Linq;
using Prism.Services.Dialogs;
using System.IO;
using Windows.Storage;
using System.Threading.Tasks;
using System;
using Windows.Storage.Streams;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Project2FA.UWP.Helpers;
using Template10.Services.Serialization;

namespace Project2FA.UWP.ViewModels
{
    public class EditAccountContentDialogViewModel : BindableBase, IDialogInitializeAsync
    {
        private TwoFACodeModel _twoFACodeModel;
        private string _tempIssuer, _tempLabel, _tempAccountIconName, _tempAccountSVGIcon;
        private IconNameCollectionModel _iconNameCollectionModel;

        public ICommand CancelButtonCommand { get; }
        public ICommand PrimaryButtonCommand { get; }

        private ISerializationService SerializationService { get; }

        public EditAccountContentDialogViewModel(ISerializationService serializationService)
        {
            SerializationService = serializationService;
            PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                TwoFACodeModel.AccountSVGIcon = AccountSVGIcon;
                await DataService.Instance.WriteLocalDatafile();
            });
            CancelButtonCommand = new DelegateCommand(() =>
            {
                TwoFACodeModel.Issuer = _tempIssuer;
                TwoFACodeModel.Label = _tempLabel;
                TwoFACodeModel.AccountIconName = _tempAccountIconName;
            });
        }

        public string Issuer
        {
            get => TwoFACodeModel.Issuer;
            set
            {
                TwoFACodeModel.Issuer = value;
            }
        }
        public string Label
        {
            get => TwoFACodeModel.Label;
            set
            {
                TwoFACodeModel.Label = value;
            }
        }

        public string AccountIconName
        {
            get => TwoFACodeModel.AccountIconName;
            set
            {
                TwoFACodeModel.AccountIconName = value;
                RaisePropertyChanged(nameof(AccountIconName));
            }
        }

        public string AccountSVGIcon
        {
            get => _tempAccountSVGIcon;
            set
            {
                SetProperty(ref _tempAccountSVGIcon, value);
            }
        }

        public TwoFACodeModel TwoFACodeModel
        {
            get => _twoFACodeModel;
            set
            {
                SetProperty(ref _twoFACodeModel, value);
                _tempIssuer = TwoFACodeModel.Issuer;
                _tempLabel = TwoFACodeModel.Label;
                _tempAccountIconName = TwoFACodeModel.AccountIconName;
            }
        }

        public IconNameCollectionModel IconNameCollectionModel
        {
            get => _iconNameCollectionModel;
            private set => _iconNameCollectionModel = value;
        }

        private async Task LoadIconNameCollection()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/JSONs/IconNameCollection.json"));
            IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();
            using (StreamReader r = new StreamReader(randomStream.AsStreamForRead()))
            {
                IconNameCollectionModel = SerializationService.Deserialize<IconNameCollectionModel>(await r.ReadToEndAsync());
            }
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<TwoFACodeModel>("model", out var model))
            {
                TwoFACodeModel = model;
                AccountIconName = TwoFACodeModel.AccountIconName;
                string iconStr = await SVGColorHelper.ManipulateSVGColor(TwoFACodeModel, AccountIconName, TwoFACodeModel.IsFavourite);
                if (!string.IsNullOrWhiteSpace(iconStr))
                {
                    AccountSVGIcon = iconStr;
                }
                await LoadIconNameCollection();
            }
        }
    }
}
