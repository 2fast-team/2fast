using System;
using Prism.Ioc;
using UNOversal.Services.Settings;
using Project2FA.Services.Enums;
using Project2FA.Extensions;

#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Project2FA.UNO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.Services
{
    public class SettingsService
    {
        SettingsHelper _helper;
        private SettingsService()
        {
            _helper = new SettingsHelper(App.Current.Container.Resolve<ISettingsAdapter>());
        }

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static SettingsService Instance { get; } = new SettingsService();

        public bool UseHeaderBackButton
        {
            get => _helper.SafeRead(nameof(UseHeaderBackButton), true);
            set => _helper.TryWrite(nameof(UseHeaderBackButton), value);
        }

        public bool UseRoundCorner
        {
            get => _helper.SafeRead(nameof(UseRoundCorner), false);
            set
            {
                _helper.TryWrite(nameof(UseRoundCorner), value);
                // workaround for switching the resources...
                switch (AppTheme)
                {
                    case Theme.System:
                        if (OriginalAppTheme == ApplicationTheme.Dark)
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
        }



        public int QRCodeScanSeconds
        {
            get => _helper.SafeRead(nameof(QRCodeScanSeconds), 5);
            set => _helper.TryWrite(nameof(QRCodeScanSeconds), value);
        }

        public int AutoLogoutMinutes
        {
            get => _helper.SafeRead(nameof(AutoLogoutMinutes), 10);
            set => _helper.TryWrite(nameof(AutoLogoutMinutes), value);
        }

        public bool UseDarkOrWhiteTitleBar
        {
            get => _helper.SafeRead(nameof(UseDarkOrWhiteTitleBar), false);
            set => _helper.TryWrite(nameof(UseDarkOrWhiteTitleBar), value);
        }

        public bool AppRated
        {
            get => _helper.SafeRead(nameof(AppRated), false);
            set => _helper.TryWrite(nameof(AppRated), value);
        }

        public bool UseExtendedHash
        {
            get => _helper.SafeRead(nameof(UseExtendedHash), false);
            set => _helper.TryWrite(nameof(UseExtendedHash), value);
        }

        public bool UseAutoLogout
        {
            get => _helper.SafeRead(nameof(UseAutoLogout), true);
            set => _helper.TryWrite(nameof(UseAutoLogout), value);
        }

        public bool UseNTPServerCorrection
        {
            get => _helper.SafeRead(nameof(UseNTPServerCorrection), false);
            set => _helper.TryWrite(nameof(UseNTPServerCorrection), value);
        }

        public bool AdvancedPasswordSecurity
        {
            get => _helper.SafeRead(nameof(AdvancedPasswordSecurity), false);
            set => _helper.TryWrite(nameof(AdvancedPasswordSecurity), value);
        }

        public bool UseHiddenTOTP
        {
            get => _helper.SafeRead(nameof(UseHiddenTOTP), false);
            set => _helper.TryWrite(nameof(UseHiddenTOTP), value);
        }

        public bool PrideMonthDesign
        {
            get => _helper.SafeRead(nameof(PrideMonthDesign), false);
            set => _helper.TryWrite(nameof(PrideMonthDesign), value);
        }

        public string NTPServerString
        {
            get => _helper.SafeRead(nameof(UseExtendedHash), "time.windows.com");
            set => _helper.TryWrite(nameof(UseExtendedHash), value);
        }

        public string UnhandledExceptionStr
        {
            get => _helper.SafeRead(nameof(UnhandledExceptionStr), string.Empty);
            set => _helper.TryWrite(nameof(UnhandledExceptionStr), value);
        }

        public bool ActivateWindowsHello
        {
            get => _helper.SafeRead(nameof(ActivateWindowsHello), true);
            set => _helper.TryWrite(nameof(ActivateWindowsHello), value);
        }

        public WindowsHelloPreferEnum PreferWindowsHello
        {
            get => _helper.SafeReadEnum(nameof(PreferWindowsHello), WindowsHelloPreferEnum.None);
            set => _helper.TryWrite(nameof(PreferWindowsHello), value);
        }

        public Theme AppTheme
        {
            get => _helper.SafeReadEnum(nameof(AppTheme), Theme.System);
            set
            {
                _helper.TryWrite(nameof(AppTheme), value);
                switch (value)
                {
                    default:
                    case Theme.System:
                        (Window.Current.Content as FrameworkElement).RequestedTheme = OriginalAppTheme.ToElementTheme();
                        break;
                    case Theme.Dark:
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                        break;
                    case Theme.Light:
                        (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                        break;
                }
            }
        }

        public ApplicationTheme AppStartSetTheme(ApplicationTheme startTheme)
        {
            OriginalAppTheme = startTheme;
            if (AppTheme == Theme.Dark || AppTheme == Theme.Light)
            {
                switch (AppTheme)
                {
                    default:
                    case Theme.System:
                        return startTheme;
                    case Theme.Dark:
                        return ApplicationTheme.Dark;
                    case Theme.Light:
                        return ApplicationTheme.Light;
                }
            }
            else
            {
                return startTheme;
            }
        }

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public ApplicationTheme OriginalAppTheme
        {
            get => _helper.SafeReadEnum(nameof(OriginalAppTheme), ApplicationTheme.Light);
            set => _helper.TryWrite(nameof(OriginalAppTheme), value);
        }

        public void ResetSystemTheme(ApplicationTheme theme)
        {
            OriginalAppTheme = theme;
            switch (theme)
            {
                case ApplicationTheme.Light:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
                    break;
                case ApplicationTheme.Dark:
                    (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
                    break;
                default:
                    break;
            }
        }

        public DateTime LastCheckedSystemTime
        {
            get => _helper.SafeRead(nameof(LastCheckedSystemTime), new DateTime());
            set =>_helper.TryWrite(nameof(LastCheckedSystemTime), value);
        }
    }
}
