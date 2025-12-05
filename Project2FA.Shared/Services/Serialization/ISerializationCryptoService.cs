namespace Project2FA.Services
{
    public interface ISerializationCryptoService
    {
        T DeserializeDecrypt<T>(byte[] keyArray, byte[] initVector, string value, int encryptionVersion);
        string SerializeEncrypt(byte[] keyArray, byte[] initVector, object parameter, int encryptionVersion);
    }
}
