using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Project2FA.MAUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.ViewModels
{
    public partial class WelcomePageViewModel : ObservableObject
    {
        public WelcomePageViewModel()
        {

        }

        [RelayCommand]
        async Task LoadExistDatafile()
        {
            await Shell.Current.GoToAsync(nameof(UseDataFilePage));
        }
    }
}
