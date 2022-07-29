using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Services.JSON
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
                result = default;
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
                return default;

            if (string.IsNullOrEmpty(value))
                return default;

            // Deserialize from json
            return JsonSerializer.Deserialize<T>(value, Settings);
        }

        public async Task<T> DeserializeAsync<T>(Stream utf8Json)
        {
            if (utf8Json == null)
                return default;

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
                var r = Deserialize<T>(value);
                if (r == null)
                {
                    result = default;
                    return false;
                }
                result = r;
                return true;
            }
            catch
            {
                result = default;
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
                    return default;
                }
                return r;
            }
            catch
            {
                return default;
            }
        }



        //public Task<bool> TrySerializeAsync(Stream stream, object parameter, out string result)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        Serializer.Serialize(memoryStream, person);
        //        var byteArray = memoryStream.ToArray();
        //    }
        //    try
        //    {
        //        var memoryStream = new MemoryStream();
        //        result = Serialize(parameter);
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        result = default(string);
        //        return false;
        //    }
        //}


    }
}
