using Project2FA.Repository.Models;
using System.Windows.Input;
using System.IO;
using Windows.Storage;
using System.Threading.Tasks;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Serialization;
using Project2FA.Helpers;
using Project2FA.Services;
using System.Collections.ObjectModel;
using Project2FA.Core.Utils;
using System.Linq;

#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class EditAccountContentDialogViewModel : EditAccountViewModelBase, IDialogInitialize
    {
        public ObservableCollection<FontIdentifikationModel> FontIdentifikationCollection { get; } = new ObservableCollection<FontIdentifikationModel>();
        public EditAccountContentDialogViewModel(ISerializationService serializationService)
        {
            SerializationService = serializationService;
            PrimaryButtonCommand = new AsyncRelayCommand(SaveAndCloseDialog);
            CancelButtonCommand = new RelayCommand(() =>
            {
                //Model.Issuer = TempIssuer;
                //Model.Label = TempLabel;
                //Model.AccountIconName = TempAccountIconName;
                //Model.AccountSVGIcon = TempAccountSVGIcon;
            });
            DeleteAccountIconCommand = new RelayCommand(() =>
            {
                AccountIconName = null;
            });
            EditAccountIconCommand = new RelayCommand(() =>
            {
                IsEditBoxVisible = !IsEditBoxVisible;
            });
        }

        private async Task SaveAndCloseDialog()
        {
            Model.Issuer = Issuer;
            Model.Label = Label;
            Model.AccountIconName = AccountIconName;

            Model.Notes = Notes;
            Model.SelectedCategories ??= new ObservableCollection<CategoryModel>();
            Model.SelectedCategories.AddRange(GlobalTempCategories.Where(x => x.IsSelected == true), true);
            await DataService.Instance.WriteLocalDatafile();
        }

        public void Initialize(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<TwoFACodeModel>("model", out var model))
            {
                Model = model;
                TempModel = (TwoFACodeModel)Model.Clone();
                AccountIconName = Model.AccountIconName;
            }
        }

        /// <summary>
        /// Search the account list by the search term
        /// </summary>
        /// <param name="senderText">Input search text</param>
        /// <returns></returns>
        public Task<bool> SearchAccountFonts(string senderText)
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
                        return Task.FromResult(true);
                    }
                    return Task.FromResult(true);
                }
                catch (Exception exc)
                {
#if WINDOWS_UWP
                    TrackingManager.TrackExceptionCatched(nameof(SearchAccountFonts), exc);
#endif
                    FontIdentifikationCollection.Clear();
                    return Task.FromResult(false);
                }

            }
            else
            {
                FontIdentifikationCollection.Clear();
                return Task.FromResult(false);
            }
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
    }
}
