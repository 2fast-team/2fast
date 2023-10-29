#if WINDOWS_UWP

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using UNOversal.Services.Dialogs;
using Project2FA.Models;
using System.Collections.ObjectModel;

namespace Project2FA.ViewModels
{
    public class InAppPaymentContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        private bool _primaryButtonCanClick;
        private int _selectedIndex = -1;
        public ObservableCollection<InAppPaymentSubscriptionModel> Items { get; } = new ObservableCollection<InAppPaymentSubscriptionModel>();

        public bool PrimaryButtonCanClick 
        {
            get => _primaryButtonCanClick; 
            set => SetProperty(ref _primaryButtonCanClick, value);
        }
        public int SelectedIndex 
        { 
            get => _selectedIndex; 
            set
            {
                if (SetProperty(ref _selectedIndex, value))
                {
                    if (value != -1)
                    {
                        PrimaryButtonCanClick = true;
                    }
                    else
                    {
                        PrimaryButtonCanClick = false;
                    }
                }
            }
                
        }

        public void Initialize(IDialogParameters parameters)
        {
            Items.Add(new InAppPaymentSubscriptionModel { Description = "Test", Url = "ms-appx:///Assets/FileLogo.png" });
        }
    }
}
#endif