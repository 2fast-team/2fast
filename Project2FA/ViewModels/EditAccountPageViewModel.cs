using Prism.Commands;
using Prism.Navigation;
using Project2FA.Repository.Models;
using Project2FA.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Project2FA.ViewModels
{
    public class EditAccountPageViewModel : ViewModelBase, INavigatedAware
    {
        private TwoFACodeModel _twoFACodeModel;
        private bool _saveCommand;
        private string _tempIssuer, _tempLabel;
        INavigationService NavigationService { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand SaveButtonCommand { get; }

        public EditAccountPageViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            SaveButtonCommand = new DelegateCommand(async() =>
            {
                _saveCommand = true;
                await NavigationService.GoBackAsync();
            });
            CancelButtonCommand = new DelegateCommand(async () =>
            {
                await NavigationService.GoBackAsync();
            });
        }

        public TwoFACodeModel TwoFACodeModel
        {
            get => _twoFACodeModel;
            set => SetProperty(ref _twoFACodeModel, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (_saveCommand)
            {
                DataService.Instance.WriteLocalDatafile();
            }
            else
            {
                TwoFACodeModel.Issuer = _tempIssuer;
                TwoFACodeModel.Label = _tempLabel;
            }
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            TwoFACodeModel = parameters.GetValue<TwoFACodeModel>("model");
            _tempIssuer = TwoFACodeModel.Issuer;
            _tempLabel = TwoFACodeModel.Label;
        }
    }
}
