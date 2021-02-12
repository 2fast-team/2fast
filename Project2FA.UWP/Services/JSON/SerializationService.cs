using System;
using System.Text.Json;
using Template10.Services.Serialization;

namespace Project2FA.UWP.Services
{
    public class SerializationService : ISerializationService
    {
        public SerializationService()
        {
            Settings = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
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
            if (value == null)
                return default(T);

            if (string.IsNullOrEmpty(value))
                return default(T);

            // Deserialize from json
            return JsonSerializer.Deserialize<T>(value, Settings);
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
    }
}
