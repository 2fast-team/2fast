using Project2FA.Core.Services.Crypto;
using Project2FA.Helpers;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Project2FA.Services
{
    public sealed class EncryptedStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var encrypted = reader.GetString();
            if (CryptoService.IsInitialized())
            {
                return encrypted is null ? "" : CryptoService.Decrypt(encrypted);
            }
            else
            {
                return encrypted;
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            var encrypted = value is null ? "" : CryptoService.Encrypt(value);
            writer.WriteStringValue(encrypted);
        }
    }
}
