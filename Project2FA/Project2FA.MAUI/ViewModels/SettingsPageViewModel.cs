using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.MAUI.Services;
using Project2FA.MAUI.Services.Settings.Enums;
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

        #region Theme
        public Theme Theme
        {
            get => _settings.ApplyTheme;
            set
            {
                if (_settings.ApplyTheme.Equals(value))
                {
                    return;
                }

                _settings.ApplyTheme = value;
                DataService.Instance.ReloadAccountIconSVGs();
            }
        }

        public bool ThemeAsLight
        {
            get => Theme.Equals(Theme.Light);
            set
            {
                if (value)
                {
                    Theme = Theme.Light;
                }
            }
        }

        public bool ThemeAsDark
        {
            get => Theme.Equals(Theme.Dark);
            set
            {
                if (value)
                {
                    Theme = Theme.Dark;
                }
            }
        }

        public bool ThemeAsSystem
        {
            get => Theme.Equals(Theme.System);
            set
            {
                if (value)
                {
                    Theme = Theme.System;
                }
            }
        }

        public int ThemeIndex
        {
            get => _settings.ApplyTheme switch
            {
                Theme.Dark => 1,
                Theme.Light => 0,
                Theme.System => 2,
                _ => 2,
            };

            set
            {
                switch (value)
                {
                    case 0: ThemeAsLight = true; break;
                    case 1: ThemeAsDark = true; break;
                    case 2: ThemeAsSystem = true; break;
                }
            }
        }
        #endregion

    }
}
