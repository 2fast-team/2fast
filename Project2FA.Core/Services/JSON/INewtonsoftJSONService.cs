namespace Project2FA.Core.Services.JSON
{
    public interface INewtonsoftJSONService
    {
        object Deserialize(string parameter);
        T DeserializeDecrypt<T>(string key, byte[] initVector, string value, int encryptionVersion);

        T DeserializeDecrypt<T>(byte[] keyArray, byte[] initVector, string value, int encryptionVersion);
        T Deserialize<T>(string parameter);
        string Serialize(object parameter);
        string SerializeEncrypt(string key, byte[] initVector, object parameter, int encryptionVersion);
        string SerializeEncrypt(byte[] keyArray, byte[] initVector, object parameter, int encryptionVersion);
        bool TrySerialize(object parameter, out string result);
        bool TryDeserialize<T>(string parameter, out T result);
    }
}
