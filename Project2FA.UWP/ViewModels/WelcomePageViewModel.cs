using Prism.Commands;
using Prism.Mvvm;
using System;
using Project2FA.UWP.Views;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Prism.Navigation;
using Project2FA.UWP.Utils;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using Microsoft.Toolkit.Mvvm.Input;

namespace Project2FA.UWP.ViewModels
{
    public class WelcomePageViewModel : BindableBase
    {
        IDialogService _dialogService { get; }
        INavigationService _navigationService { get; }
        public ICommand NewDatefileCommand { get; }
        public ICommand TutorialCommand { get; }
        public ICommand OpenTutorialCommand { get; }

        public ICommand UseExistDatefileCommand { get; }
        private bool _isTutorialOpen;

        private string _title;

        //private bool _canNavigate;

        public WelcomePageViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            // disable the navigation to other pages
            App.ShellPageInstance.NavigationIsAllowed = false;
            Title = Strings.Resources.WelcomePageTitle;

            NewDatefileCommand = new AsyncRelayCommand(NewDatafile);
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            UseExistDatefileCommand = new DelegateCommand(async () =>
            {
                await _navigationService.NavigateAsync(nameof(UseDataFilePage));
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            TutorialCommand = new DelegateCommand(() =>
            {
                IsTutorialOpen = !IsTutorialOpen;
            });
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            OpenTutorialCommand = new DelegateCommand(async() =>
            {
                var dialog = new ContentDialog();
                dialog.Title = "#test";
                dialog.PrimaryButtonText = "#bla";
                if (IsTutorialOpen)
                {
                    IsTutorialOpen = false;
                }
                await _dialogService.ShowDialogAsync(dialog,new DialogParameters());
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
        }

        private async Task NewDatafile()
        {
            var dialog = new NewDatafileContentDialog();
            var result = await _dialogService.ShowDialogAsync(dialog, new DialogParameters());
            if (result == ContentDialogResult.Primary)
            {
                //_canNavigate = true;
                string navPath = "/" + nameof(AccountCodePage);
                await _navigationService.NavigateAsync(navPath);
            }
        }

        private async Task UseExistDatafile()
        {
            try
            {
                //TODO current workaround: check permission to the file system (broadFileSystemAccess)
                string path = @"C:\Windows\explorer.exe";
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                UseDatafileContentDialog dialog = new UseDatafileContentDialog();
                ContentDialogResult result = await _dialogService.ShowDialogAsync(dialog, new DialogParameters());

                //result is also none, when the datafileDB is correct created
                if (result == ContentDialogResult.None)
                {
                    var datafileDB = await App.Repository.Datafile.GetAsync();
                    if (datafileDB != null)
                    {
                        //_canNavigate = true;
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
        public bool IsTutorialOpen { get => _isTutorialOpen; set => SetProperty(ref _isTutorialOpen, value); }
    }
}
