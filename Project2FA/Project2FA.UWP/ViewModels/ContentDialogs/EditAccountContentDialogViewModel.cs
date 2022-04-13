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
                await DataService.Instance.WriteLocalDatafile();
            });
            CancelButtonCommand = new DelegateCommand(() =>
            {
                Model.Issuer = TempIssuer;
                Model.Label = TempLabel;
                Model.AccountIconName = TempAccountIconName;
                Model.AccountSVGIcon = TempAccountSVGIcon;
            });
        }

        public string Issuer
        {
            get => Model.Issuer;
            set
            {
                Model.Issuer = value;
            }
        }
        public string Label
        {
            get => Model.Label;
            set
            {
                Model.Label = value;
            }
        }

        public string AccountIconName
        {
            get => Model.AccountIconName;
            set
            {
                Model.AccountIconName = value;
                RaisePropertyChanged(nameof(AccountIconName));
            }
        }

        public TwoFACodeModel Model
        {
            get => _twoFACodeModel;
            set
            {
                SetProperty(ref _twoFACodeModel, value);
                TempIssuer = Model.Issuer;
                TempLabel = Model.Label;
                TempAccountIconName = Model.AccountIconName;
            }
        }

        public IconNameCollectionModel IconNameCollectionModel
        {
            get => _iconNameCollectionModel;
            private set => _iconNameCollectionModel = value;
        }
        public string TempIssuer { get => _tempIssuer; set => _tempIssuer = value; }
        public string TempLabel { get => _tempLabel; set => _tempLabel = value; }
        public string TempAccountIconName { get => _tempAccountIconName; set => _tempAccountIconName = value; }
        public string TempAccountSVGIcon { get => _tempAccountSVGIcon; set => _tempAccountSVGIcon = value; }

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
                Model = model;
                AccountIconName = Model.AccountIconName;
                var iconStr = await SVGColorHelper.ManipulateSVGColor(Model, AccountIconName, Model.IsFavourite);
                await LoadIconNameCollection();
            }
        }
    }
}
