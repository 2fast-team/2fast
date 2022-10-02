using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.MAUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private SettingsService _settings;

        public SettingsPageViewModel()
        {
            _settings = SettingsService.Instance;
        }
        public bool UseHiddenTOTP
        {
            get => _settings.UseHiddenTOTP;
            set
            {
                _settings.UseHiddenTOTP = value;
                OnPropertyChanged(nameof(UseHiddenTOTP));
            }
        }
    }
}
