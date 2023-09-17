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
        private int _selectedIndex = 0;
        private bool _isMobileSearchActive = false;
        public ICommand AccountCodePageCommand { get; }
        public ICommand SearchPageCommand { get; }
        public ICommand SettingsPageCommand { get; }
#if !WINDOWS_UWP
        private TabBarItem _selectedItem;
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
            //if (App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
            //{
            //    if (uIElement is not SearchPage)
            //    {
            //        await NavigationService.NavigateAsync(nameof(SearchPage));
            //    }
            //}
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
        /// <summary>
        /// 
        /// </summary>
        public bool IsMobile 
        {
            get
            {
#if ANDROID || IOS
                return true && NavigationIsAllowed;
#else
                return false;
#endif
            }
        }

        public int SelectedIndex 
        { 
            get => _selectedIndex;
            set
            {
                SetProperty(ref _selectedIndex, value);
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
