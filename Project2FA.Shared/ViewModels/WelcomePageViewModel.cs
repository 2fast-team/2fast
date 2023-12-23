using System.Windows.Input;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Dialogs;
using UNOversal.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
#else
using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class WelcomePageViewModel : ObservableObject
    {
        IDialogService _dialogService { get; }
        INavigationService _navigationService { get; }
        public ICommand NewDatefileCommand { get; }
        public ICommand TutorialCommand { get; }

        public ICommand UseExistDatefileCommand { get; }
        private bool _isTutorialOpen;

        public WelcomePageViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            // disable the navigation to other pages
            App.ShellPageInstance.ViewModel.NavigationIsAllowed = false;

            NewDatefileCommand = new AsyncRelayCommand(NewDatafileCommandTask);

            UseExistDatefileCommand = new AsyncRelayCommand(UseExistDatafileCommandTask);
        }

        private async Task NewDatafileCommandTask()
        {
            await _navigationService.NavigateAsync(nameof(NewDataFilePage));
        }

        private async Task UseExistDatafileCommandTask()
        {
            await _navigationService.NavigateAsync(nameof(UseDataFilePage));
        }
    }
}
