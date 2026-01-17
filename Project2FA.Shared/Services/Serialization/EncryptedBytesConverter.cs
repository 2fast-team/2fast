using OtpNet;
using Project2FA.Core.Services.Crypto;
using Project2FA.Helpers;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Project2FA.Services.Serialization
{
    public sealed class EncryptedBytesConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var encrypted = reader.GetString();
            if (CryptoService.IsInitialized())
            {
                if (encrypted is null) return Array.Empty<byte>();
                var decrypted = CryptoService.Decrypt(Convert.FromBase64String(encrypted));
                return decrypted;
            }
            else
            {
                if (encrypted is null) return Array.Empty<byte>();
                return Convert.FromBase64String(encrypted);
            }

        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            var encrypted = CryptoService.Encrypt(value);
            writer.WriteStringValue(encrypted);
        }
    }
}
