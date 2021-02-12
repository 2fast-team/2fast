using Prism.Navigation;
using Prism.Services;
using Project2FA.Repository.Models;
using Project2FA.Resources;
using Project2FA.Services;
using Project2FA.Views;
using System;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Extensions;

namespace Project2FA.ViewModels
{
    public class AccountCodePageViewModel : ViewModelBase, IConfirmNavigation, IInitialize
    {
        private readonly INavigationService _navigationService;
        private bool _navigation;
        private readonly IPageDialogService _pageDialogService;

        public AccountCodePageViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
        {
            _navigationService = navigationService;
            _pageDialogService = pageDialogService;
            _navigation = false;
        }

        /// <summary>
        /// Creates a timer for every collection entry to show the duration of the generated TOTP code
        /// </summary>
        private bool OnTimedEvent()
        {
            for (int i = 0; i < Collection.Count; i++)
            {
                if (Collection[i].Seconds == 0)
                {
                    Collection[i].Seconds = Collection[i].Period;
                    DataService.Instance.GenerateTOTP(i);
                }
                else
                {
                    Collection[i].Seconds--;
                }
            }
            if (_navigation)
            {
                _navigation = false;
                //stop the timer
                return false;
            }
            else
            {
                //run the timer again and again...
                return true;
            }
        }

        public async void EditAccountFromCollection(TwoFACodeModel model)
        {
            var navigationParams = new NavigationParameters();
            navigationParams.Add("model", model);
            await _navigationService.NavigateAsync(nameof(EditAccountPage), navigationParams);
        }

        public async void DeleteAccountFromCollection(TwoFACodeModel model)
        {
            //var result = await _userDialogs.ConfirmAsync(AppResource.AccountCodePageViewModelDeleteDescription, 
            //    AppResource.AccountCodePageViewModelDeleteTitle, 
            //    AppResource.AccountCodePageViewModelDeleteAccept, 
            //    AppResource.AccountCodePageViewModelDeleteCancel);
            //if (result)
            //{
            //    Collection.Remove(model);
            //}
        }

        public bool CanNavigate(INavigationParameters parameters)
        {
            _navigation = true;
            return true;
        }

        public void Initialize(INavigationParameters parameters)
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), OnTimedEvent);
            if (Collection.Count != 0)
            {
                DataService.Instance.ResetCollection();
            }
        }

        public ObservableCollection<TwoFACodeModel> Collection
        {
            get
            {
                return DataService.Instance.Collection;
            }
        }
    }
}
