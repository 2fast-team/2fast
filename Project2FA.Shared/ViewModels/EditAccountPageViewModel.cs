using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Common;
using Project2FA.Helpers;
using Project2FA.Repository.Models;
using Project2FA.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Navigation;
using UNOversal.Services.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
#if !WINDOWS_UWP
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class EditAccountPageViewModel : EditAccountViewModelBase, IInitializeAsync
    {
        private INavigationService NavigationService { get; }


        public EditAccountPageViewModel(ISerializationService serializationService, INavigationService navigationService)
        {
            SerializationService = serializationService;
            NavigationService = navigationService;
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
                await NavigateBack();
            });
            CancelButtonCommand = new AsyncRelayCommand(NavigateBack);
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

        private async Task NavigateBack()
        {
            await NavigationService.GoBackAsync();
        }

        public async Task InitializeAsync(INavigationParameters parameters)
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
