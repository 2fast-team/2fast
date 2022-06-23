using System;
using System.Security.Cryptography;

namespace Project2FA.Core
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
                    new RNGCryptoServiceProvider().GetBytes(s_aditionalEntropy);
                }
                return s_aditionalEntropy;
            }
        }
        public static byte[] Protect(byte[] data)
        {
            //try
            //{
            //    // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
            //    // only by the same current user.
            //    return ProtectedData.Protect(data, AditionalEntropy, DataProtectionScope.CurrentUser);
            //}
            //catch (CryptographicException e)
            //{
            //    Console.WriteLine("Data was not encrypted. An error occurred.");
            //    Console.WriteLine(e.ToString());
            //    return null;
            //}

            return ProtectedData.Protect(data, AditionalEntropy, DataProtectionScope.CurrentUser);
        }

        public static byte[] Unprotect(byte[] data)
        {
            //try
            //{
            //    //Decrypt the data using DataProtectionScope.CurrentUser.
            //    return ProtectedData.Unprotect(data, AditionalEntropy, DataProtectionScope.CurrentUser);
            //}
            //catch (CryptographicException e)
            //{
            //    Console.WriteLine("Data was not decrypted. An error occurred.");
            //    Console.WriteLine(e.ToString());
            //    return null;
            //}
            return ProtectedData.Unprotect(data, AditionalEntropy, DataProtectionScope.CurrentUser);
        }
    }
}
