using Prism.Commands;
using Prism.Mvvm;
using Project2FA.Repository.Models;
using Project2FA.UWP.Services;
using System.Windows.Input;
using Prism.Services.Dialogs;
using System.IO;
using Windows.Storage;
using System.Threading.Tasks;
using System;
using Windows.Storage.Streams;
using Project2FA.UWP.Helpers;
using Template10.Services.Serialization;

namespace Project2FA.UWP.ViewModels
{
    public class EditAccountContentDialogViewModel : BindableBase, IDialogInitializeAsync
    {
        private TwoFACodeModel _twoFACodeModel;
        private string _tempIssuer, _tempLabel, _tempAccountIconName, _tempAccountSVGIcon, _tempNotes;
        private IconNameCollectionModel _iconNameCollectionModel;

        public ICommand CancelButtonCommand { get; }
        public ICommand PrimaryButtonCommand { get; }
        public ICommand DeleteAccountIconCommand { get; }
        public ICommand CancelAccountIconCommand { get; }

        private ISerializationService SerializationService { get; }

        public EditAccountContentDialogViewModel(ISerializationService serializationService)
        {
            SerializationService = serializationService;
            PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                Model.Issuer = TempIssuer;
                Model.Label = TempLabel;
                Model.AccountIconName = TempAccountIconName;
                (bool success, string iconStr) = await SVGColorHelper.GetSVGIconWithThemeColor(Model.IsFavourite, TempAccountIconName);
                if (success)
                {
                    Model.AccountSVGIcon = iconStr;
                }
                Model.Notes = TempNotes;
                await DataService.Instance.WriteLocalDatafile();
            });
            CancelButtonCommand = new DelegateCommand(() =>
            {
                //Model.Issuer = TempIssuer;
                //Model.Label = TempLabel;
                //Model.AccountIconName = TempAccountIconName;
                //Model.AccountSVGIcon = TempAccountSVGIcon;
            });
            DeleteAccountIconCommand = new DelegateCommand(() =>
            {
                Model.AccountSVGIcon = null;
                Model.AccountIconName = null;
            });
        }

        public TwoFACodeModel Model
        {
            get => _twoFACodeModel;
            set
            {
                if(SetProperty(ref _twoFACodeModel, value))
                {
                    TempIssuer = Model.Issuer;
                    TempLabel = Model.Label;
                    TempAccountIconName = Model.AccountIconName;
                    TempAccountSVGIcon = Model.AccountSVGIcon;
                    if(!string.IsNullOrWhiteSpace(value.Notes))
                    {
                        TempNotes = Model.Notes;
                    }
                }
            }
        }

        public IconNameCollectionModel IconNameCollectionModel
        {
            get => _iconNameCollectionModel;
            private set => _iconNameCollectionModel = value;
        }
        public string TempIssuer 
        { 
            get => _tempIssuer; 
            set => SetProperty(ref _tempIssuer, value);
        }
        public string TempLabel 
        { 
            get => _tempLabel; 
            set => SetProperty(ref _tempLabel, value);
        }
        public string TempNotes
        {
            get => _tempNotes;
            set => SetProperty(ref _tempNotes, value);
        }
        public string TempAccountIconName
        { 
            get => _tempAccountIconName;
            set => SetProperty(ref _tempAccountIconName, value);
        }
        public string TempAccountSVGIcon 
        { 
            get => _tempAccountSVGIcon;
            set => SetProperty(ref _tempAccountSVGIcon, value); 
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

        public async Task LoadIconSVG()
        {
            (bool success, string iconStr) = await SVGColorHelper.GetSVGIconWithThemeColor(Model.IsFavourite, TempAccountIconName, Model.IsFavourite);
            if (success)
            {
                TempAccountSVGIcon = iconStr;
            }
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<TwoFACodeModel>("model", out var model))
            {
                Model = model;
                TempAccountIconName = Model.AccountIconName;
                (bool success, string iconStr) = await SVGColorHelper.GetSVGIconWithThemeColor(Model.IsFavourite, TempAccountIconName, Model.IsFavourite);
                if (success)
                {
                    TempAccountSVGIcon = iconStr;
                }
                await LoadIconNameCollection();
            }
        }
    }
}
