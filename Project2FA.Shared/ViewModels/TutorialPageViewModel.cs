using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Repository.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using UNOversal.Navigation;
#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
#else
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class TutorialPageViewModel : ObservableObject, IInitialize
    {
        private int _selectedIndex = 0;
        private int _maxIndex = 5;
        private bool _isTooltipOpen;
        private bool _isManualOpened;
        public ICommand BackButtonCommand;
        public ICommand FordwardButtonCommand;
        public ICommand SkipButtonCommand;
        public ICommand FinishButtonCommand;
        public INavigationService NavigationService { get; private set; }
        public ObservableCollection<TwoFACodeModel> Collection { get; } = new ObservableCollection<TwoFACodeModel>();


        public TutorialPageViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            Collection.Add(new TwoFACodeModel { Issuer = "2fast@test.com", Label = "2fast", Seconds= 30, SecretByteArray = new byte[8] });
            BackButtonCommand = new RelayCommand(() => 
            {
                SelectedIndex--;
            });
            FordwardButtonCommand = new RelayCommand(() => 
            {
                SelectedIndex++;
            });
            SkipButtonCommand = new AsyncRelayCommand(SkipButtonCommandTask);
            FinishButtonCommand = new AsyncRelayCommand(SkipButtonCommandTask);
        }
        
        private async Task SkipButtonCommandTask()
        {
            if (IsManualOpened)
            {
                await NavigationService.GoBackAsync();
            }
            else
            {
                await NavigationService.NavigateAsync(nameof(WelcomePage));
            }
        }

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.TryGetValue<bool>("ManualView", out bool isManualOpened))
            {
                IsManualOpened = isManualOpened;
            }
            else

            {
                // disable the navigation to other pages
                App.ShellPageInstance.ViewModel.NavigationIsAllowed = false;
            }
           
        }

        public int SelectedIndex 
        { 
            get => _selectedIndex;
            set
            {
                if (SetProperty(ref _selectedIndex, value))
                {
                    OnPropertyChanged(nameof(BackButtonVisible));
                    OnPropertyChanged(nameof(ForwardButtonVisible));
                    OnPropertyChanged(nameof(SkipButtonVisible));
                    OnPropertyChanged(nameof(FinishButtonVisible));
                }
            }
        }
        
        public bool BackButtonVisible
        {
            get => SelectedIndex > 0;
        }

        public bool ForwardButtonVisible
        {
            get => SelectedIndex < _maxIndex;
        }
        public bool SkipButtonVisible
        {
            get => SelectedIndex < _maxIndex;
        }
        public bool FinishButtonVisible
        {
            get => SelectedIndex == _maxIndex;
        }
        public bool IsManualOpened 
        {
            get => _isManualOpened; 
            set => SetProperty(ref _isManualOpened, value);
        }
    }
}
