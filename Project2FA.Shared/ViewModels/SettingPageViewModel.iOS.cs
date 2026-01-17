#if __IOS__
using BiometryService;
using LocalAuthentication;
using Project2FA.Services.Enums;
using System;
using System.Collections.Generic;
using System.Text;


namespace Project2FA.ViewModels
{
    public partial class SettingsPartViewModel
    {

        private bool _isBiometricLoginSupported;
        private IBiometryService BiometryService { get;  set; }
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public bool IsBiometricLoginSupported
        {
            get => _isBiometricLoginSupported;
            set => SetProperty(ref _isBiometricLoginSupported, value);
        }

        public bool ActivateBiometricLogin
        {
            get => _settings.ActivateBiometricLogin;
            set
            {
                _settings.ActivateBiometricLogin = value;
                OnPropertyChanged(nameof(ActivateBiometricLogin));

            }
        }

        public bool PreferBiometricLogin
        {
            get
            {
                switch (_settings.PreferBiometricLogin)
                {
                    case BiometricPreferEnum.None:
                    case BiometricPreferEnum.No:
                        return false;
                    case BiometricPreferEnum.Prefer:
                        return true;
                    default:
                        return false;
                }
            }
            set
            {
                _settings.PreferBiometricLogin = value ? BiometricPreferEnum.Prefer : BiometricPreferEnum.No;
                OnPropertyChanged(nameof(PreferBiometricLogin));
            }
        }

        private async Task CheckBiometricLoginIsSupported()
        {
            BiometryService = new BiometryService.BiometryService(
                "Biometrics_Confirm",
                null,
                LAPolicy.DeviceOwnerAuthentication);

            var capabilities = await BiometryService.GetCapabilities(_cancellationToken);
            if (capabilities.IsEnabled && capabilities.IsSupported)
            {
                IsBiometricLoginSupported = true;
            }
            else
            {
                ActivateBiometricLogin = false;
            }
        }
    }
}
#endif