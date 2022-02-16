﻿namespace Project2FA.Uno.Core.Secrets
{
    public class SecretService : SecretServiceBase, ISecretService
    {
        public string ConnectionString
        {
            get => Helper.ReadSecret(nameof(ConnectionString));
            set => Helper.WriteSecret(nameof(ConnectionString), value);
        }
    }
}
