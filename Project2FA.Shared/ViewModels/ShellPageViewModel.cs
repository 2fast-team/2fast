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
using Project2FA.Services.Enums;


#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Uno.Toolkit.UI;
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
    public partial class ShellPageViewModel : ObservableRecipient
    {
        private bool _navigationIsAllowed = true;
        private string _title;
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

        public void RefreshThemeForResources()
        {
            // workaround for switching the resources...
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (SettingsService.Instance.OriginalAppTheme == ApplicationTheme.Dark)
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case Theme.Dark:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    break;
                case Theme.Light:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    break;
                default:
                    break;
            }
        }

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
            get => SettingsService.Instance.IsScreenCaptureEnabled;
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
