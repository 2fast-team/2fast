using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Navigation;
using Project2FA.UNO.Views;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Navigation;

namespace Project2FA.UNO.ViewModels
{
    public class WelcomePageViewModel : ObservableObject
    {
        public ICommand NavigateToCommand { get; }
        public AsyncRelayCommand NewDatefileCommand { get; } 
        public AsyncRelayCommand UseExistDatefileCommand { get; }
        public INavigationService NavigationService { get; }
        public WelcomePageViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            //NavigateToCommand = new RelayCommand( async() =>
            //{
            //    await NavigationService.NavigateAsync(nameof(AccountCodePage));
            //    //await Task.Delay(1000);
            //});
            NewDatefileCommand = new AsyncRelayCommand(NewDatefileCommandTask);
            UseExistDatefileCommand = new AsyncRelayCommand(UseExistDatefileCommandTask);
        }

        private async Task NewDatefileCommandTask()
        {
            await NavigationService.NavigateAsync(nameof(NewDataFilePage));
        }

        private async Task UseExistDatefileCommandTask()
        {
            //try
            //{
            //    //var baseModel = App.Current.Container.Resolve(typeof(DatafileViewModelBase));
            //    var viewModel = App.Current.Container.Resolve(typeof(UseDataFilePageViewModel));
            //}
            //catch (System.Exception exc)
            //{

                
            //}
            await NavigationService.NavigateAsync(nameof(UseDataFilePage));
        }
    }
}
