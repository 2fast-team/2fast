﻿using System.Linq;
#if HAS_WINUI
using Microsoft.Security.Credentials;
#else
using Windows.Security.Credentials;
#endif


namespace Project2FA.Uno.Core.Secrets
{
    public class SecretHelper
    {
        // https://msdn.microsoft.com/en-us/library/windows/apps/windows.security.credentials.passwordvault.aspx

        static PasswordVault _vault;

        static SecretHelper()
        {
            _vault = new PasswordVault();
        }

        public string ReadSecret(string key)
        {
            return ReadSecret(GetType().ToString(), key);
        }

        public string ReadSecret(string container, string key)
        {
            if (_vault.RetrieveAll().Any(x => x.Resource == container && x.UserName == key))
            {
                var credential = _vault.Retrieve(container, key);
                credential.RetrievePassword();
                return credential.Password;
            }
            else
            {
                return string.Empty;
            }
        }

        public void WriteSecret(string key, string secret)
        {
            WriteSecret(GetType().ToString(), key, secret);
        }

        public void WriteSecret(string container, string key, string secret)
        {
            if (_vault.RetrieveAll().Any(x => x.Resource == container && x.UserName == key))
            {
                var credential = _vault.Retrieve(container, key);
                credential.RetrievePassword();
                credential.Password = secret;
                _vault.Add(credential);
            }
            else
            {
                var credential = new PasswordCredential(container, key, secret);
                _vault.Add(credential);
            }
        }

        public void RemoveSecret(string key)
        {
            RemoveSecret(GetType().ToString(), key);
        }

        public void RemoveSecret(string container, string key)
        {
            IsSecretExistsForKey(container, key, true);
        }

        public bool IsSecretExistsForKey(string key)
        {
            return IsSecretExistsForKey(GetType().ToString(), key, false);
        }

        public bool IsSecretExistsForKey(string container, string key)
        {
            return IsSecretExistsForKey(container, key, false);
        }

        private bool IsSecretExistsForKey(string container, string key, bool RemoveIfExists)
        {
            if (_vault.RetrieveAll().Any(x => x.Resource == container && x.UserName == key))
            {
                var credential = _vault.Retrieve(container, key);
                credential.RetrievePassword();

                if (credential.Password.Length > 0)
                {
                    if (RemoveIfExists)
                    {
                        _vault.Remove(credential);
                    }
                    return true;
                }
                else
                {
                    // a blank key shouldn't exist, but who knows ...
                    _vault.Remove(credential);
                }
            }

            return false;
        }
    }
}
