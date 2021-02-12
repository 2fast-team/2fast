using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.ViewModels
{
    class WelcomePageViewModel : ViewModelBase, INavigatedAware
    {
        private INavigationService _navigationService { get; }
        public WelcomePageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private async void CheckDatabaseExists()
        {
            var datafile = await App.Repository.Datafile.GetAsync();
            if (datafile != null)
            {
                //use absolute uri path to reset the navigation stack!
                await _navigationService.NavigateAsync(new Uri("/NavigationPage/AccountCodePage", UriKind.Absolute));
            }
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            //throw new NotImplementedException();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            CheckDatabaseExists();
        }
    }
}
