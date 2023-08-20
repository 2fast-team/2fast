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
    public class EditAccountContentDialogViewModel : EditAccountViewModelBase, IDialogInitializeAsync
    {
        
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
                TempAccountSVGIcon = null;
                TempAccountIconName = null;
            });
            EditAccountIconCommand = new RelayCommand(() =>
            {
                IsEditBoxVisible = !IsEditBoxVisible;
            });
        }

        private async Task SaveAndCloseDialog()
        {
            Model.Issuer = TempIssuer;
            Model.Label = TempLabel;
            Model.AccountIconName = TempAccountIconName;
            (bool success, string iconStr) = await SVGColorHelper.GetSVGIconWithThemeColor(Model.IsFavourite, TempAccountIconName);
            if (success)
            {
                Model.AccountSVGIcon = iconStr;
            }
            else
            {
                Model.AccountSVGIcon = null;
            }
            Model.Notes = TempNotes;
            Model.SelectedCategories = GlobalTempCategories.Where(x => x.IsSelected == true).ToList();
            for (int i = 0; i < TempAccountCategoryList.Count; i++)
            {
                TempAccountCategoryList[i].IsSelected = false;
            }
            DataService.Instance.GlobalCategories = TempAccountCategoryList;
            await DataService.Instance.WriteLocalDatafile();
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<TwoFACodeModel>("model", out var model))
            {
                Model = model;
                TempAccountIconName = Model.AccountIconName;
                if (!string.IsNullOrWhiteSpace(TempAccountIconName))
                {
                    (bool success, string iconStr) = await SVGColorHelper.GetSVGIconWithThemeColor(Model.IsFavourite, TempAccountIconName, Model.IsFavourite);
                    if (success)
                    {
                        TempAccountSVGIcon = iconStr;
                    }
                }
                else
                {
                    TempIconLabel = TempLabel;
                }

                //Model.SelectedCategories.Add(GlobalTempCategories.FirstOrDefault()); // debug
                // add all categories
                //foreach (var category in GlobalTempCategories)
                //{
                //    TempAccountCategoryList.Add(new TokenItem { Content = category.Name, Icon = new FontIcon { Glyph = category.Glyph }, Tag = category.Guid });
                //}

                TempAccountCategoryList.Clear();
                //filter which are selected
                var selectedList = GlobalTempCategories.Where(x=> Model.SelectedCategories.Where(y => y.Guid == x.Guid).Any());
                for (int i = 0; i < selectedList.Count(); i++)
                {
                    selectedList.ElementAt(i).IsSelected = true;
                    TempAccountCategoryList.Add(selectedList.ElementAt(i));
                }
                await LoadIconNameCollection();
            }
        }
    }
}
