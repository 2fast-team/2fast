using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using UNOversal.Services.Serialization;

namespace Project2FA.Services
{
    public class SerializationService : ISerializationService
    {
        public SerializationService()
        {
            Settings = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = SerializationContext.Default
            };
        }

        /// <summary>
        /// JSON serializer settings.
        /// </summary>
        public JsonSerializerOptions Settings { get; }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        public string Serialize(object value)
        {
            if (value == null)
                return null;

            if (string.IsNullOrEmpty(value.ToString()))
                return string.Empty;

            // Serialize to json
            return JsonSerializer.Serialize(value, Settings);
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


        /// <summary>
        /// Deserializes the value.
        /// </summary>
        public object Deserialize(string value)
        {
            if (value == null)
                return null;

            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Deserialize from json
            return JsonSerializer.Deserialize<object>(value, Settings);
        }

        /// <summary>
        /// Deserializes the value.
        /// </summary>
        public T Deserialize<T>(string value)
        {
            JsonTypeInfo<T>? typeInfo = Settings.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>;
            if (typeInfo == null)
                throw new InvalidOperationException($"Type {typeof(T)} not supported by source generator.");

            if (value == null)
                return default(T);

            if (string.IsNullOrEmpty(value))
                return default(T);
            
            // Deserialize from json
            return JsonSerializer.Deserialize<T>(value, typeInfo);
        }

        public async Task<T> DeserializeAsync<T>(Stream utf8Json)
        {
            if (utf8Json == null)
                return default(T);

            //if (string.IsNullOrEmpty(utf8Json))
            //    return default(Task<T>)

            // Deserialize from utf8Json
            return await JsonSerializer.DeserializeAsync<T>(utf8Json, Settings);
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
                var r = this.Deserialize<T>(value);
                if (r == null)
                {
                    result = default(T);
                    return false;
                }
                result = (T)r;
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public async Task<T> TryDeserializeAsync<T>(Stream stream)
        {
            try
            {
                var r = await DeserializeAsync<T>(stream);
                if (r == null)
                {
                    return default(T);
                }
                return r;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
