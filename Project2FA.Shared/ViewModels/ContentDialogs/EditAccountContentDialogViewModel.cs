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
    public partial class EditAccountContentDialogViewModel : EditAccountViewModelBase, IDialogInitialize
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
                AccountIconName = string.Empty;
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




    }
}
