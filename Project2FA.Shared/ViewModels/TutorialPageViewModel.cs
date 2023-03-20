using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
#else
using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
    public class TutorialPageViewModel : ObservableObject
    {
        private bool _isTooltipOpen;
        public ObservableCollection<TwoFACodeModel> Collection { get; } = new ObservableCollection<TwoFACodeModel>();
        public TutorialPageViewModel()
        {
            // disable the navigation to other pages
            App.ShellPageInstance.ViewModel.NavigationIsAllowed = false;
            Collection.Add(new TwoFACodeModel { Issuer = "2fast@test.com", Label = "2fast", Seconds= 30, SecretByteArray = new byte[8] });
        }
    }
}
