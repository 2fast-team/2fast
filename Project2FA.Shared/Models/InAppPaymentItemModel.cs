#if WINDOWS_UWP

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.Core.Messenger;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.Repository.Models
{
    public partial class InAppPaymentItemModel : ObservableRecipient
    {
        private string _storeId;
        public string StoreId 
        {
            get => _storeId; 
            set => _storeId = value; 
        }
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

        private string _uidCheckBox;

        public string UidCheckBox 
        { 
            get => _uidCheckBox;
            set => SetProperty(ref _uidCheckBox, value);
        }

        private bool _isEnabled;
        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set => SetProperty(ref _isEnabled, value);
        }

        private bool _isChecked;
        public bool IsChecked
        { 
            get => _isChecked; 
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    Messenger.Send(new InAppItemChangedMessage(this));
                }
                
            }
        }

    }
}
#endif