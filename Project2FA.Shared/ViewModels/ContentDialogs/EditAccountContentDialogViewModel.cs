using Project2FA.Repository.Models;
using System.Windows.Input;
using System.IO;
using Windows.Storage;
using System.Threading.Tasks;
using System;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Serialization;
using Project2FA.Helpers;
using Project2FA.Services;
#if !WINDOWS_UWP
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
            PrimaryButtonCommand = new RelayCommand(async () =>
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
                await DataService.Instance.WriteLocalDatafile();
            });
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
                await LoadIconNameCollection();
            }
        }
    }
}
