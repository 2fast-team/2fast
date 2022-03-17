namespace Project2FA.Uno.Core.Secrets
{
    public interface ISecretService
    {
        string ConnectionString { get; set; }

        SecretHelper Helper { get; }
    }
}