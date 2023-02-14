using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.Core.Messenger;
using System;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
    public class ShellPageViewModel : ObservableRecipient
    {
        private bool _navigationIsAllowed = true;
        private string _title;
        private bool _isScreenCaptureEnabled = false;
        public bool NavigationIsAllowed
        {
            get => _navigationIsAllowed;
            set => SetProperty(ref _navigationIsAllowed, value);
        }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
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
    }


}
