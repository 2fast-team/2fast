using Prism.Commands;
using Prism.Mvvm;
using System;
using Project2FA.UWP.Views;
using System.Windows.Input;
using Template10.Services.Dialog;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Prism.Navigation;
using Project2FA.UWP.Utils;

namespace Project2FA.UWP.ViewModels
{
    public class WelcomePageViewModel : BindableBase, IConfirmNavigation
    {
        IDialogService _dialogService { get; }
        INavigationService _navigationService { get; }
        public ICommand NewDatefileCommand { get; }

        public ICommand UseExistDatefileCommand { get; }

        private string _title;

        private bool _canNavigate;

        public WelcomePageViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            App.ShellPageInstance.NavigationIsAllowed = false;
            Title = "#Willkommen";

            NewDatefileCommand = new DelegateCommand(NewDatafile);
            UseExistDatefileCommand = new DelegateCommand(UseExistDatafile);
        }
        private async void NewDatafile()
        {
            var dialog = new NewDatafileContentDialog();
            var result = await _dialogService.ShowAsync(dialog);
            if (result == ContentDialogResult.Primary)
            {
                _canNavigate = true;
                string navPath = "/" + nameof(AccountCodePage);
                await _navigationService.NavigateAsync(navPath);
            }
        }
        private async void UseExistDatafile()
        {
            try
            {
                //TODO current workaround: check permission to the file system (broadFileSystemAccess)
                string path = @"C:\Windows\explorer.exe";
                var file = await StorageFile.GetFileFromPathAsync(path);

                var dialog = new UseDatafileContentDialog();
                var result = await _dialogService.ShowAsync(dialog);

                //result is also none, when the datafileDB is correct created
                if (result == ContentDialogResult.None)
                {
                    var datafileDB = await App.Repository.Datafile.GetAsync();
                    if (datafileDB != null)
                    {
                        _canNavigate = true;
                        await _navigationService.NavigateAsync("/" + nameof(AccountCodePage));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ErrorDialogs.UnauthorizedAccessDialog();
            }
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }

        public bool CanNavigate(INavigationParameters parameters)
        {
            if (_canNavigate)
            {
                App.ShellPageInstance.NavigationIsAllowed = true;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
