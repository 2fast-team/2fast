using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using System.Collections.ObjectModel;
using System.Linq;


#if !WINDOWS_UWP
using Microsoft.UI.Xaml.Data;
using Project2FA.UNO;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class EditAccountPageViewModel : EditAccountViewModelBase, IInitialize
    {
        private INavigationService NavigationService { get; }

        public ICommand NavigateBackCommand { get; }


        public EditAccountPageViewModel(ISerializationService serializationService, INavigationService navigationService)
        {
            SerializationService = serializationService;
            NavigationService = navigationService;
            PrimaryButtonCommand = new RelayCommand(async () =>
            {
                Model.Issuer = Issuer;
                Model.Label = Label;
                Model.AccountIconName = AccountIconName;

                //Model.Notes = Notes;
                //Model.SelectedCategories ??= new ObservableCollection<CategoryModel>();
                //Model.SelectedCategories.AddRange(GlobalTempCategories.Where(x => x.IsSelected == true), true);
                await DataService.Instance.WriteLocalDatafile();
                await NavigateBackCommandTask();
            });
            NavigateBackCommand = new AsyncRelayCommand(NavigateBackCommandTask);
            CancelButtonCommand = new AsyncRelayCommand(NavigateBackCommandTask);
            DeleteAccountIconCommand = new RelayCommand(() =>
            {
                AccountIconName = string.Empty;
            });
            EditAccountIconCommand = new RelayCommand(() =>
            {
                IsEditBoxVisible = !IsEditBoxVisible;
            });

#if __ANDROID__ || __IOS__
            App.ShellPageInstance.ViewModel.TabBarIsVisible = false;
#endif
        }

        private async Task NavigateBackCommandTask()
        {
            await NavigationService.GoBackAsync();
        }

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.TryGetValue<TwoFACodeModel>("model", out var model))
            {
                Model = model;
                TempModel = (TwoFACodeModel)Model.Clone();
                AccountIconName = TempModel.AccountIconName;
            }
        }
    }
}
