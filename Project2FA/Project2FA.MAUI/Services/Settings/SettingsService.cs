using Project2FA.MAUI.Services.Settings.Enums;

namespace Project2FA.MAUI.Services
{

    public class SettingsService
    {
        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static SettingsService Instance { get; } = new SettingsService();

        public bool AppRated
        {
            get => Preferences.Get(nameof(AppRated), false);
            set => Preferences.Set(nameof(AppRated), value);
        }

        public bool UseNTPServerCorrection
        {
            get => Preferences.Get(nameof(UseNTPServerCorrection), false);
            set => Preferences.Set(nameof(UseNTPServerCorrection), value);
        }

        public bool UseHiddenTOTP
        {
            get => Preferences.Get(nameof(UseHiddenTOTP), true);
            set => Preferences.Set(nameof(UseHiddenTOTP), value);
        }

        public bool PrideMonthDesign
        {
            get => Preferences.Get(nameof(PrideMonthDesign), false);
            set => Preferences.Set(nameof(PrideMonthDesign), value);
        }

        public string NTPServerString
        {
            get => Preferences.Get(nameof(NTPServerString), "time.windows.com");
            set => Preferences.Set(nameof(NTPServerString), value);
        }

        public string UnhandledExceptionStr
        {
            get => Preferences.Get(nameof(UnhandledExceptionStr), string.Empty);
            set => Preferences.Set(nameof(UnhandledExceptionStr), value);
        }

        public BiometricLoginPrefer PreferWindowsHello
        {
            get => (BiometricLoginPrefer)Preferences.Get(nameof(PreferWindowsHello), 0);
            set => Preferences.Set(nameof(PreferWindowsHello), Convert.ToInt32(value));
        }

        public Theme ApplyTheme
        {
            get => (Theme)Preferences.Get(nameof(ApplyTheme), 0);
            set
            {
                Preferences.Set(nameof(ApplyTheme), Convert.ToInt32(value));
                switch (value)
                {
                    default:
                    case Theme.System:
                        Application.Current.UserAppTheme = Application.Current.RequestedTheme;
                        break;
                    case Theme.Dark:
                        Application.Current.UserAppTheme = AppTheme.Dark;
                        break;
                    case Theme.Light:
                        Application.Current.UserAppTheme = AppTheme.Light;
                        break;
                }
            }
        }

        public AppTheme AppStartSetTheme(AppTheme startTheme)
        {
            if (ApplyTheme == Theme.Dark || ApplyTheme == Theme.Light)
            {
                return ApplyTheme switch
                {
                    Theme.Dark => AppTheme.Dark,
                    Theme.Light => AppTheme.Light,
                    _ => startTheme,
                };
            }
            else
            {
                return startTheme;
            }
        }

        public void ResetSystemTheme(AppTheme theme)
        {
            switch (theme)
            {
                case AppTheme.Light:
                    Application.Current.UserAppTheme = AppTheme.Light;
                    break;
                case AppTheme.Dark:
                    Application.Current.UserAppTheme = AppTheme.Dark;
                    break;
                default:
                    Application.Current.UserAppTheme = Application.Current.RequestedTheme;
                    break;
            }
        }

        public DateTime LastCheckedSystemTime
        {
            get => Preferences.Get(nameof(LastCheckedSystemTime), new DateTime());
            set => Preferences.Set(nameof(LastCheckedSystemTime), value);
        }
    }
}
