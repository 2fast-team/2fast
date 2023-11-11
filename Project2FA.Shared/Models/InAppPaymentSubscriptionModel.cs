#if WINDOWS_UWP

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.Models
{
    public class InAppPaymentSubscriptionModel : ObservableObject
    {
        private string _url;

        public string Url 
        { 
            get => _url; 
            set => SetProperty(ref _url, value);
        }

        private string _description;
        public string Description 
        { 
            get => _description; 
            set => SetProperty(ref _description, value);
        }

        private bool _isEnabled;
        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set => SetProperty(ref _isEnabled, value);
        }


    }
}
#endif