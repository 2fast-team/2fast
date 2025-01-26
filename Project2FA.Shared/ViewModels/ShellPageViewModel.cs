using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.Core.Messenger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Navigation;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Project2FA.Services;

#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml.Controls;
#else
using Uno.Toolkit.UI;
using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class ShellPageViewModel : ObservableRecipient
    {
        private bool _navigationIsAllowed = true;
        private string _title;
        private bool _isScreenCaptureEnabled;
        public ICommand AccountCodePageCommand { get; }
        public ICommand SearchPageCommand { get; }
        public ICommand SettingsPageCommand { get; }
#if !WINDOWS_UWP
        private TabBarItem _selectedItem;
        private int _selectedIndex = 0;
        private bool _isMobileSearchActive = false;
#endif

        public ShellPageViewModel()
        {
#if ANDROID || IOS
            AccountCodePageCommand = new AsyncRelayCommand(AccountCodePageCommandTask);
            SearchPageCommand = new AsyncRelayCommand(SearchPageCommandTask);
            SettingsPageCommand = new AsyncRelayCommand(SettingsPageCommandTask);
#endif
        }

#if !WINDOWS_UWP
        private async Task SettingsPageCommandTask()
        {
            if(App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
            {
                if (uIElement is not SettingPage)
                {
                    await NavigationService.NavigateAsync(nameof(SettingPage));
                }
            }  
        }

        private async Task SearchPageCommandTask()
        {
            if (App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
            {
                if (uIElement is SettingPage)
                {
                    await NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
                }
            }
        }

        private async Task AccountCodePageCommandTask()
        {
            if (App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
            {
                if (uIElement is not AccountCodePage)
                {
                    await NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
                }
            }
        }
#endif

        public INavigationService NavigationService { get; internal set; }
        public bool NavigationIsAllowed
        {
            get => _navigationIsAllowed;
            set
            {
                if(SetProperty(ref _navigationIsAllowed, value))
                {
#if ANDROID || IOS
                    OnPropertyChanged(nameof(IsMobile));
#endif
                }
            }
        }
        public string Title 
        {
            get => _title; 
            set => SetProperty(ref _title, value); 
        }
        
        public bool IsScreenCaptureEnabled
        {
            get => _isScreenCaptureEnabled;
            set
            {
                if (SetProperty(ref _isScreenCaptureEnabled, value))
                {
                    Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = value;
                    Messenger.Send(new IsScreenCaptureEnabledChangedMessage(value));
                }
            }
        }

#if !WINDOWS_UWP

        private bool _tabBarIsVisible;
        /// <summary>
        /// The TabBar should be only visible at mobile devices
        /// </summary>
        public bool IsMobile 
        {
            get
            {
#if ANDROID || IOS
                return true;
#else
                return false;
#endif
            }
        }
        public bool TabBarIsVisible
        {
            get => IsMobile & _tabBarIsVisible;
            set=> SetProperty(ref _tabBarIsVisible, value);
        }


        public int SelectedIndex 
        { 
            get => _selectedIndex;
            set
            {
                SetProperty(ref _selectedIndex, value);
                //only for mobile devices, check if account filters are set

                if (value == 0)
                {
                    if (DataService.Instance.ACVCollection.Filter != null)
                    {
                        DataService.Instance.ACVCollection.Filter = null;
                    }
                }

                if (value == 1)
                {
                    IsMobileSearchActive = true;
                }
                else
                {
                    IsMobileSearchActive = false;
                }
            }
        }

        public TabBarItem SelectedItem 
        { 
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }
        public bool IsMobileSearchActive 
        { 
            get => _isMobileSearchActive; 
            set => SetProperty(ref _isMobileSearchActive, value);
        }
#endif
    }
}
