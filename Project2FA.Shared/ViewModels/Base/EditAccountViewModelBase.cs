using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Helpers;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Project2FA.ViewModels
{
    public class EditAccountViewModelBase : ObservableObject
    {
        private TwoFACodeModel _twoFACodeModel;
        private string _tempIssuer, _tempLabel, _tempAccountIconName,
            _tempAccountSVGIcon, _tempNotes, _tempIconLabel;
        private IconNameCollectionModel _iconNameCollectionModel;
        private bool _isEditBoxVisible;

        public ICommand CancelButtonCommand { get; internal set; }
        public ICommand PrimaryButtonCommand { get; internal set; }
        public ICommand DeleteAccountIconCommand { get; internal set; }
        public ICommand EditAccountIconCommand { get; internal set; }

        public ISerializationService SerializationService { get; internal set; }
        public EditAccountViewModelBase()
        {

        }
        public TwoFACodeModel Model
        {
            get => _twoFACodeModel;
            set
            {
                if (SetProperty(ref _twoFACodeModel, value))
                {
                    TempIssuer = Model.Issuer;
                    TempLabel = Model.Label;
                    TempAccountIconName = Model.AccountIconName;
                    TempAccountSVGIcon = Model.AccountSVGIcon;
                    if (!string.IsNullOrWhiteSpace(value.Notes))
                    {
                        TempNotes = Model.Notes;
                    }
                    else
                    {
                        TempNotes = string.Empty;
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
            set
            {
                if (SetProperty(ref _tempAccountIconName, value))
                {
                    if (value != null)
                    {
                        TempIconLabel = string.Empty;
                    }
                    else
                    {
                        TempIconLabel = TempLabel;
                    }
                }
            }
        }
        public string TempAccountSVGIcon
        {
            get => _tempAccountSVGIcon;
            set => SetProperty(ref _tempAccountSVGIcon, value);
        }
        public string TempIconLabel
        {
            get => _tempIconLabel;
            set => SetProperty(ref _tempIconLabel, value);
        }
        public bool IsEditBoxVisible
        {
            get => _isEditBoxVisible;
            set => SetProperty(ref _isEditBoxVisible, value);
        }

        public async Task LoadIconNameCollection()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/JSONs/IconNameCollection.json"));
                IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();
                using (StreamReader r = new StreamReader(randomStream.AsStreamForRead()))
                {
                    IconNameCollectionModel = SerializationService.Deserialize<IconNameCollectionModel>(await r.ReadToEndAsync());
                }
            }
            catch (Exception exc)
            {
                //TOOD add exception dialog
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
    }
}
