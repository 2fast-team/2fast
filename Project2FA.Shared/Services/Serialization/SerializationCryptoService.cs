using Project2FA.Core.Services.Crypto;
using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Project2FA.Services
{
    public class SerializationCryptoService : ISerializationCryptoService
    {
        /// <summary>
        /// JSON serializer settings.
        /// </summary>
        public JsonSerializerOptions Settings { get; }
        public SerializationCryptoService()
        {
            Settings = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = SerializationContext.Default
            };
        }

        public T DeserializeDecrypt<T>(byte[] keyArray, byte[] initVector, string value, int encryptionVersion)
        {
            CryptoService.Initialize(keyArray, initVector);

            JsonTypeInfo<T>? typeInfo = Settings.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>;
            if (typeInfo == null)
                throw new InvalidOperationException($"Type {typeof(T)} not supported by source generator.");

            if (value == null)
                return default(T);

            if (string.IsNullOrEmpty(value))
                return default(T);

            try
            {
                return JsonSerializer.Deserialize<T>(value, typeInfo);
            }
            finally
            {
                CryptoService.Clear();
            }
        }

        public string SerializeEncrypt(byte[] keyArray, byte[] initVector, object parameter, int encryptionVersion)
        {
            CryptoService.Initialize(keyArray, initVector);

            // Use JsonTypeInfo for AOT and trimming safety
            var type = parameter.GetType();
            var typeInfo = Settings.GetTypeInfo(type);
            if (typeInfo == null)
                throw new InvalidOperationException($"Type {type} not supported by source generator.");

            try
            {
                return JsonSerializer.Serialize(parameter, typeInfo);
            }
            finally
            {
                CryptoService.Clear();
            }
        }
    }
}
