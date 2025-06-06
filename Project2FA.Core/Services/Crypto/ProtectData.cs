
using System;
using System.Security.Cryptography;

namespace Project2FA.Core.Services.Crypto
{
    public static class ProtectData
    {
        private static byte[] s_aditionalEntropy;
        private static byte[] AditionalEntropy
        {
            get
            {
                if (s_aditionalEntropy is null)
                {
                    s_aditionalEntropy = new byte[5];
                    RandomNumberGenerator.Create().GetBytes(s_aditionalEntropy);
                }
                return s_aditionalEntropy;
            }
        }
        public static byte[] Protect(byte[] data)
        {
            return ProtectedData.Protect(data, AditionalEntropy, DataProtectionScope.CurrentUser);
        }

        public static byte[] Unprotect(byte[] data)
        {
            return ProtectedData.Unprotect(data, AditionalEntropy, DataProtectionScope.CurrentUser);
        }
    }
}

