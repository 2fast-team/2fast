using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Core.Utils;
using Project2FA.Repository.Models;
using Project2FA.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Serialization;
using UNOversal.Services.Logging;
using UNOversal.Ioc;



#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml.Controls;
#else
using Project2FA.UnoApp;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
    public class EditAccountViewModelBase : ObservableObject
    {
        private TwoFACodeModel _twoFACodeModel, _tempModel;
        private FontIdentifikationCollectionModel _iconNameCollectionModel;
        private bool _isEditBoxVisible;
        private bool _notesExpanded = true;
        private bool _isPrimaryBTNEnabled;

        public ObservableCollection<FontIdentifikationModel> FontIdentifikationCollection { get; } = new ObservableCollection<FontIdentifikationModel>();
        public ObservableCollection<CategoryModel> GlobalTempCategories { get; } = new ObservableCollection<CategoryModel>();
        public ICommand CancelButtonCommand { get; internal set; }
        public ICommand PrimaryButtonCommand { get; internal set; }
        public ICommand DeleteAccountIconCommand { get; internal set; }
        public ICommand EditAccountIconCommand { get; internal set; }

        public ISerializationService SerializationService { get; internal set; }
        public EditAccountViewModelBase()
        {
#if WINDOWS_UWP
            // note expander is collapsed if a pro version is active
            NotesExpanded = !IsProVersion;
#endif
        }

        /// <summary>
        /// Search the account list by the search term
        /// </summary>
        /// <param name="senderText">Input search text</param>
        /// <returns></returns>
        public async Task<bool> SearchAccountFonts(string senderText)
        {
            if (string.IsNullOrEmpty(senderText) == false && senderText.Length >= 2 && senderText != Strings.Resources.AccountCodePageSearchNotFound)
            {
                var tempList = DataService.Instance.FontIconCollection.Where(x => x.Name.Contains(senderText, System.StringComparison.OrdinalIgnoreCase)).ToList();
                FontIdentifikationCollection.AddRange(tempList, true);
                try
                {
                    if (FontIdentifikationCollection.Count == 0)
                    {
                        FontIdentifikationCollection.Add(new FontIdentifikationModel { Name = Strings.Resources.AccountCodePageSearchNotFound });
                        return true;
                    }
                    return true;
                }
                catch (Exception exc)
                {
                    await App.Current.Container.Resolve<ILoggingService>().LogException(exc, SettingsService.Instance.LoggingSetting);
#if WINDOWS_UWP
                    UWP.TrackingManager.TrackExceptionCatched(nameof(SearchAccountFonts), exc);
#endif
                    FontIdentifikationCollection.Clear();
                    return false;
                }

            }
            else
            {
                FontIdentifikationCollection.Clear();
                return false;
            }
        }

        public TwoFACodeModel Model
        {
            get => _twoFACodeModel;
            set
            {
                if (SetProperty(ref _twoFACodeModel, value))
                {
                    if (DataService.Instance.GlobalCategories != null && DataService.Instance.GlobalCategories.Count > 0)
                    {
                        GlobalTempCategories.Clear();
                        for (int i = 0; i < DataService.Instance.GlobalCategories.Count; i++)
                        {
                            GlobalTempCategories.Add((CategoryModel)DataService.Instance.GlobalCategories[i].Clone());
                        }
                    }
                }
            }
        }

        public TwoFACodeModel TempModel
        {
            get => _tempModel;
            set
            {
                if(SetProperty(ref _tempModel, value))
                {
                    //OnPropertyChanged(nameof(Issuer));
                    OnPropertyChanged(nameof(Label));
                    OnPropertyChanged(nameof(Notes));
                    OnPropertyChanged(nameof(Issuer));
                    //OnPropertyChanged(nameof(Issuer));
                }
            }

        }

        /// <summary>
        /// Checks if the inputs are correct and enables / disables the submit button
        /// </summary>
        private void CheckInputs()
        {
            IsPrimaryBTNEnable = !string.IsNullOrWhiteSpace(Label) && !string.IsNullOrWhiteSpace(Issuer);
        }

        public FontIdentifikationCollectionModel IconNameCollectionModel
        {
            get => _iconNameCollectionModel;
            private set => _iconNameCollectionModel = value;
        }
        public string Issuer
        {
            get => TempModel.Issuer;
            set
            {
                TempModel.Issuer = value;
                OnPropertyChanged(nameof(Issuer));
                CheckInputs();
            }
        }
        public string Label
        {
            get => TempModel.Label;
            set
            {
                TempModel.Label = value;
                OnPropertyChanged(nameof(Label));
                CheckInputs();
            }
        }
        public string Notes
        {
            get
            {
                if (TempModel.Notes is null)
                {
                    return string.Empty;
                }
                return TempModel.Notes;
            }
            set
            {
                TempModel.Notes = value;
                OnPropertyChanged(nameof(Notes));
                CheckInputs();
            }
        }

        
        public string AccountIconName
        {
            get => TempModel.AccountIconName;
            set
            {
                TempModel.AccountIconName = value;
                OnPropertyChanged(nameof(TempModel));
                OnPropertyChanged(nameof(AccountIconName));
                CheckInputs();
            }
        }
        public bool IsPrimaryBTNEnable
        {
            get => _isPrimaryBTNEnabled;
            set => SetProperty(ref _isPrimaryBTNEnabled, value);
        }

        public bool IsEditBoxVisible
        {
            get => _isEditBoxVisible;
            set => SetProperty(ref _isEditBoxVisible, value);
        }
        public bool NotesExpanded 
        { 
            get => _notesExpanded;
            set => SetProperty(ref _notesExpanded, value); 
        }

        public bool NoCategoriesExists
        {
            get
            {
                return DataService.Instance.GlobalCategories.Count == 0;
            }
        }

        public bool CategoriesExists
        {
            get
            {
                return DataService.Instance.GlobalCategories.Count > 0;
            }
        }

#if WINDOWS_UWP
        public bool IsProVersion
        {
            get => SettingsService.Instance.IsProVersion;
        }
#endif
    }
}
