using Prism.Commands;
using Prism.Mvvm;
using Project2FA.UWP.Views;
using System.Windows.Input;
using Prism.Navigation;
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

        public WelcomePageViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            // disable the navigation to other pages
            App.ShellPageInstance.NavigationIsAllowed = false;
            Title = Strings.Resources.WelcomePageTitle;

            NewDatefileCommand = new AsyncRelayCommand(NewDatafile);

            UseExistDatefileCommand = new AsyncRelayCommand(UseExistDatafile);

            TutorialCommand = new DelegateCommand(() =>
            {
                IsTutorialOpen = !IsTutorialOpen;
            });
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            OpenTutorialCommand = new DelegateCommand(async() =>
            {
                var dialog = new TutorialContentDialog();
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
            await _navigationService.NavigateAsync(nameof(NewDataFilePage));
        }

        private async Task UseExistDatafile()
        {
            await _navigationService.NavigateAsync(nameof(UseDataFilePage));
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public bool IsTutorialOpen { get => _isTutorialOpen; set => SetProperty(ref _isTutorialOpen, value); }
    }
}
