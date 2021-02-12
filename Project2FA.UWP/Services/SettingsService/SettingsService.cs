using Project2FA.UWP.Extensions;
using Project2FA.UWP.Services.Enums;
using System;
using Template10.Services.Settings;
using Windows.UI.Xaml;

namespace Project2FA.UWP.Services
{
    public class SettingsService
    {
        SettingsHelper _helper;
        private SettingsService()
        {
            _helper = new SettingsHelper();
        }

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static SettingsService Instance { get; } = new SettingsService();

        public bool UseHeaderBackButton
        {
            get { return _helper.SafeRead(nameof(UseHeaderBackButton), true); }
            set
            {
                _helper.TryWrite<bool>(nameof(UseHeaderBackButton), value);
            }
        }

        public int QRCodeScanSeconds
        {
            get { return _helper.SafeRead<int>(nameof(QRCodeScanSeconds), 5); }
            set
            {
                _helper.TryWrite<int>(nameof(QRCodeScanSeconds), value);
            }
        }

        public bool UseDarkOrWhiteTitleBar
        {
            get { return _helper.SafeRead(nameof(UseDarkOrWhiteTitleBar), false); }
            set
            {
                _helper.TryWrite<bool>(nameof(UseDarkOrWhiteTitleBar), value);
            }
        }

        public bool AppRated
        {
            get { return _helper.SafeRead<bool>(nameof(AppRated), false); }
            set
            {
                _helper.TryWrite<bool>(nameof(AppRated), value);
            }
        }

        public bool UseExtendedHash
        {
            get { return _helper.SafeRead<bool>(nameof(UseExtendedHash), false); }
            set
            {
                _helper.TryWrite<bool>(nameof(UseExtendedHash), value);
            }
        }

        public WindowsHelloPreferEnum PreferWindowsHello
        {
            get { return _helper.SafeReadEnum<WindowsHelloPreferEnum>(nameof(PreferWindowsHello), WindowsHelloPreferEnum.None); }
            set
            {
                _helper.TryWrite<WindowsHelloPreferEnum>(nameof(PreferWindowsHello), value);
            }
        }

        public Theme AppTheme
        {
            get
            {
                return _helper.SafeReadEnum<Theme>(nameof(AppTheme), Theme.System);
            }
            set
            {
                _helper.TryWrite<Theme>(nameof(AppTheme), value);
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

        public ApplicationTheme OriginalAppTheme
        {
            get
            {
                return _helper.SafeReadEnum<ApplicationTheme>(nameof(OriginalAppTheme), ApplicationTheme.Light);
            }
            set
            {
                _helper.TryWrite<ApplicationTheme>(nameof(OriginalAppTheme), value);
            }
        }

        public DateTime LastCheckedSystemTime
        {
            get
            {
                return _helper.SafeReadEnum<DateTime>(nameof(LastCheckedSystemTime), new DateTime());
            }
            set
            {
                _helper.TryWrite<DateTime>(nameof(LastCheckedSystemTime), value);
            }
        }
    }
}
