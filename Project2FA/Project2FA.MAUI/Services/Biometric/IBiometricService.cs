using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Services.Biometric
{
    public interface IBiometricService
    {
        Task<bool> LoginWithBiometrics();
    }
}
