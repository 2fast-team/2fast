using Newtonsoft.Json;
using Newtonsoft.Json.Encryption;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Project2FA.Core.Services.JSON
{
    public class NewtonsoftJSONService : INewtonsoftJSONService, IDisposable
    {
        EncryptionFactory _encryptionFactory;
        public NewtonsoftJSONService()
        {
            _encryptionFactory = new EncryptionFactory();
            Settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
        }

        /// <summary>
        /// JSON serializer settings.
        /// </summary>
        public JsonSerializerSettings Settings { get; }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        public string Serialize(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(value.ToString()))
            {
                return string.Empty;
            }

            // Serialize to json
            return JsonConvert.SerializeObject(value);
        }

        public bool TrySerialize(object parameter, out string result)
        {
            try
            {
                result = Serialize(parameter);
                return true;
            }
            catch (Exception)
            {
                result = default(string);
                return false;
            }
        }

        public string SerializeEncrypt(string key, byte[] initVectorArray, object value)
        {
            var byteArrayKey = CryptoService.CreateByteArrayKey(key);
            var serializer = new JsonSerializer
            {
                ContractResolver = _encryptionFactory.GetContractResolver()
            };

            string serialized;

            // per serialize session
            Aes algorithm = Aes.Create();
            algorithm.Key = byteArrayKey;
            algorithm.IV = initVectorArray;

            using (_encryptionFactory.GetEncryptSession(algorithm))
            {
                StringBuilder builder = new StringBuilder();
                using (StringWriter writer = new StringWriter(builder))
                {
                    serializer.Serialize(writer, value);
                }
                return serialized = builder.ToString();
            }
        }


        /// <summary>
        /// Deserializes the value.
        /// </summary>
        public object Deserialize(string value)
        {
            if (value == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            // Deserialize from json
            return JsonConvert.DeserializeObject(value);
        }

        /// <summary>
        /// Deserializes the value.
        /// </summary>
        public T Deserialize<T>(string value)
        {
            if (value == null)
            {
                return default(T);
            }

            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }

            // Deserialize from json
            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Attempts to deserialize the value by absorbing the InvalidCastException that may occur.
        /// </summary>
        /// <returns>
        /// True if deserialization succeeded with non-null result, otherwise False.
        /// </returns>
        /// <remarks>
        /// On success (or return True) deserialized result is copied to the 'out' variable.
        /// On fail (or return False) default(T) is copied to the 'out' variable.
        /// </remarks>
        public bool TryDeserialize<T>(string value, out T result)
        {
            try
            {
                T r = Deserialize<T>(value);
                if (r == null)
                {
                    result = default(T);
                    return false;
                }
                result = r;
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public T DeserializeDecrypt<T>(string key, byte[] initVector, string value)
        {
            var byteArrayKey = CryptoService.CreateByteArrayKey(key);
            

            var serializer = new JsonSerializer
            {
                ContractResolver = _encryptionFactory.GetContractResolver()
            };

            Aes algorithm = Aes.Create();
            algorithm.Key = byteArrayKey;
            algorithm.IV = initVector;

            using (_encryptionFactory.GetDecryptSession(algorithm))
            using (var stringReader = new StringReader(value))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _encryptionFactory?.Dispose();
            }
        }
    }
}
