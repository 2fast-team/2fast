using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Services.JSON
{
    public interface ISerializationService
    {
        object Deserialize(string parameter);
        Task<T> DeserializeAsync<T>(Stream utf8Json);
        T Deserialize<T>(string parameter);
        string Serialize(object parameter);
        bool TrySerialize(object parameter, out string result);
        //Task<bool> TrySerializeAsync(Stream utf8Json, out string result);
        bool TryDeserialize<T>(string parameter, out T result);
        Task<T> TryDeserializeAsync<T>(Stream utf8Json);
    }
}
